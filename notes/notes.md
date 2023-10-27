# Notes

## 003 - Instructions Per Clock

- Terms:
  - IPC: Instructions per clock
  - ILP: Instruction level parallelism

## 004 - Monday Q&A #1 (2023-02-05)

- Sunk cost: often a tool/programming language used is based on familiarity/invested time or resources and not because it is the "best". Even in new companies/new projects people will often use what they know.

- Parallelism in CPU: assembly instructions get internally split up into multiple micro operations (micro-ops), there are tools that simulate a cpu and can tell you what the bottleneck is

## 005 - Single Instruction, Multiple Data (Simd)

- Terms:
  - SIMD: Single Instruction Multiple Data
  - SSE: Streaming SIMD Extensions (SSE, SSE2, SSE3, SSE4)
  - AVX: Advanced Vector Extensions (AVX, AVX2, AVX-512)

~10x performance increase possible by using AVX2 and loop unrolling for example

```cpp
// ================================================================================
// Listing 5 (not optimized) | 0.85 adds/clock
// ================================================================================
typedef unsigned int u32;
u32 SingleScalar(u32 Count, u32 *Input)
{
	u32 Sum = 0;
	for(u32 Index = 0; Index < Count; ++Index)
	{
		Sum += Input[Index];
	}

	return Sum;
}

// ================================================================================
// Listing 14 (AVX2 intrinsics and loop unrolling) | 11 adds/clock
// ================================================================================
typedef unsigned int u32;
u32 __attribute__((target("avx2"))) QuadAVX(u32 Count, u32 *Input)
{
	__m256i SumA = _mm256_setzero_si256();
	__m256i SumB = _mm256_setzero_si256();
	__m256i SumC = _mm256_setzero_si256();
	__m256i SumD = _mm256_setzero_si256();
	for(u32 Index = 0; Index < Count; Index += 32)
	{
		SumA = _mm256_add_epi32(SumA, _mm256_loadu_si256((__m256i *)&Input[Index]));
		SumB = _mm256_add_epi32(SumB, _mm256_loadu_si256((__m256i *)&Input[Index + 8]));
		SumC = _mm256_add_epi32(SumC, _mm256_loadu_si256((__m256i *)&Input[Index + 16]));
		SumD = _mm256_add_epi32(SumD, _mm256_loadu_si256((__m256i *)&Input[Index + 24]));
	}

	__m256i SumAB = _mm256_add_epi32(SumA, SumB);
	__m256i SumCD = _mm256_add_epi32(SumC, SumD);
	__m256i Sum = _mm256_add_epi32(SumAB, SumCD);

	Sum = _mm256_hadd_epi32(Sum, Sum);
	Sum = _mm256_hadd_epi32(Sum, Sum);
	__m256i SumS = _mm256_permute2x128_si256(Sum, Sum, 1 | (1 << 4));
	Sum = _mm256_add_epi32(Sum, SumS);

	return _mm256_cvtsi256_si32(Sum);
}

// ================================================================================
// Listing 15 (AVX2 intrinsics, loop unrolling, optimized memory access (no adress calculation)) | 13.4 adds/clock
// ================================================================================
typedef unsigned int u32;
u32 __attribute__((target("avx2"))) QuadAVXPtr(u32 Count, u32 *Input)
{
	__m256i SumA = _mm256_setzero_si256();
	__m256i SumB = _mm256_setzero_si256();
	__m256i SumC = _mm256_setzero_si256();
	__m256i SumD = _mm256_setzero_si256();

	Count /= 32;
	while(Count--)
	{
		SumA = _mm256_add_epi32(SumA, _mm256_loadu_si256((__m256i *)&Input[0]));
		SumB = _mm256_add_epi32(SumB, _mm256_loadu_si256((__m256i *)&Input[8]));
		SumC = _mm256_add_epi32(SumC, _mm256_loadu_si256((__m256i *)&Input[16]));
		SumD = _mm256_add_epi32(SumD, _mm256_loadu_si256((__m256i *)&Input[24]));

		Input += 32;
	}

	__m256i SumAB = _mm256_add_epi32(SumA, SumB);
	__m256i SumCD = _mm256_add_epi32(SumC, SumD);
	__m256i Sum = _mm256_add_epi32(SumAB, SumCD);

	Sum = _mm256_hadd_epi32(Sum, Sum);
	Sum = _mm256_hadd_epi32(Sum, Sum);
	__m256i SumS = _mm256_permute2x128_si256(Sum, Sum, 1 | (1 << 4));
	Sum = _mm256_add_epi32(Sum, SumS);

	return _mm256_cvtsi256_si32(Sum);
}
```

## 006 - Caching

CPU has multiple caches (L1, L2, L3) and access to memory (RAM).
If data is small enough to fit in fast cache massive speedup is possible.

## 009 - Python Revisited

How to improve performance?

- Reduce Waste
- Increase IPC/ILP
- Use SIMD
- Use Cache
- Use Multithreading

For Python: use something like Cython to improve performance

```python
# ========================================================================
# LISTING 4 (Python with range) | 0.006 adds/cycle
# ========================================================================

def SingleScalar(Count, Input):
    Sum = 0
    for Index in range(0, Count):
        Sum += Input[Index]
    return Sum

# ========================================================================
# LISTING 16 (Python with for-in) | 0.008 adds/cycle
# ========================================================================

def SingleScalarNoRange(Count, Input):
    Sum = 0
    for Value in Input:
        Sum += Value
    return Sum

# ========================================================================
# LISTING 17 (Python with numpy) | 0.007 adds/cycle
# ========================================================================

import numpy
def NumpySum(Count, Input):
    return numpy.sum(Input)

# ========================================================================
# LISTING 18 (Python with built-in sum) | 0.06 adds/cycle
# ========================================================================

def BuiltinSum(Count, Input):
    return sum(Input)

# ========================================================================
# LISTING 19 (Cython, with (automatically generated) Python wrapper) | 4.31 adds/cycle
# ========================================================================

def CythonSum(unsigned int TotalCount, array.array InputArray):
    cdef unsigned int Count
    cdef unsigned int *Input
    cdef __m256i SumA, SumB, SumC, SumD
    cdef __m256i SumAB, SumCD
    cdef __m256i Sum, SumS

    Input = InputArray.data.as_uints

    SumA = _mm256_setzero_si256()
    SumB = _mm256_setzero_si256()
    SumC = _mm256_setzero_si256()
    SumD = _mm256_setzero_si256()

    Count = TotalCount >> 5
    while Count != 0:
        SumA = _mm256_add_epi32(SumA, _mm256_loadu_si256(<__m256i *>&Input[0]))
        SumB = _mm256_add_epi32(SumB, _mm256_loadu_si256(<__m256i *>&Input[8]))
        SumC = _mm256_add_epi32(SumC, _mm256_loadu_si256(<__m256i *>&Input[16]))
        SumD = _mm256_add_epi32(SumD, _mm256_loadu_si256(<__m256i *>&Input[24]))

        Input += 32
        Count -= 1

    SumAB = _mm256_add_epi32(SumA, SumB)
    SumCD = _mm256_add_epi32(SumC, SumD)
    Sum = _mm256_add_epi32(SumAB, SumCD)

    Sum = _mm256_hadd_epi32(Sum, Sum)
    Sum = _mm256_hadd_epi32(Sum, Sum)
    SumS = _mm256_permute2x128_si256(Sum, Sum, 1 | (1 << 4))
    Sum = _mm256_add_epi32(Sum, SumS)

    return _mm256_cvtsi256_si32(Sum)

# ========================================================================
# LISTING 20 (Cython, without Python wrapper) | 4.38 adds/cycle
# ========================================================================

cdef unsigned int CythonSumC(unsigned int TotalCount, unsigned int *Input):
    cdef unsigned int Count
    cdef __m256i SumA, SumB, SumC, SumD
    cdef __m256i SumAB, SumCD
    cdef __m256i Sum, SumS

    Count = TotalCount >> 5

    SumA = _mm256_setzero_si256()
    SumB = _mm256_setzero_si256()
    SumC = _mm256_setzero_si256()
    SumD = _mm256_setzero_si256()
    while Count != 0:
        SumA = _mm256_add_epi32(SumA, _mm256_loadu_si256(<__m256i *>&Input[0]));
        SumB = _mm256_add_epi32(SumB, _mm256_loadu_si256(<__m256i *>&Input[8]));
        SumC = _mm256_add_epi32(SumC, _mm256_loadu_si256(<__m256i *>&Input[16]));
        SumD = _mm256_add_epi32(SumD, _mm256_loadu_si256(<__m256i *>&Input[24]));

        Input += 32
        Count -= 1

    SumAB = _mm256_add_epi32(SumA, SumB)
    SumCD = _mm256_add_epi32(SumC, SumD)
    Sum = _mm256_add_epi32(SumAB, SumCD)

    Sum = _mm256_hadd_epi32(Sum, Sum)
    Sum = _mm256_hadd_epi32(Sum, Sum)
    SumS = _mm256_permute2x128_si256(Sum, Sum, 1 | (1 << 4))
    Sum = _mm256_add_epi32(Sum, SumS)

    return _mm256_cvtsi256_si32(Sum)
```

The Cython versions vastly outperform the other options.

## 011 - The Haversine Distance Problem

```python
# ========================================================================
# LISTING 21
# ========================================================================

from math import radians, sin, cos, sqrt, asin
import time
import json

JSONFile = open('./data/data_10000000_flex.json')

#
# Read the input
#

StartTime = time.time()
JSONInput = json.load(JSONFile)
MidTime = time.time()

#
# Average the haversines
#

def HaversineOfDegrees(X0, Y0, X1, Y1, R):

  dY = radians(Y1 - Y0)
  dX = radians(X1 - X0)
  Y0 = radians(Y0)
  Y1 = radians(Y1)

  RootTerm = (sin(dY/2)**2) + cos(Y0)*cos(Y1)*(sin(dX/2)**2)
  Result = 2*R*asin(sqrt(RootTerm))

  return Result

EarthRadiuskm = 6371
Sum = 0
Count = 0
for Pair in JSONInput['pairs']:
    Sum += HaversineOfDegrees(Pair['x0'], Pair['y0'], Pair['x1'], Pair['y1'], EarthRadiuskm)
    Count += 1
Average = Sum / Count
EndTime = time.time()

#
# Display the result
#

print("Result: " + str(Average))
print("Input = " + str(MidTime - StartTime) + " seconds")
print("Math = " + str(EndTime - MidTime) + " seconds")
print("Total = " + str(EndTime - StartTime) + " seconds")
print("Throughput = " + str(Count/(EndTime - StartTime)) + " haversines/second")
```

Observation: loading the JSON data is about half of the total time, the other half is the actual haversine computation.

## ?
