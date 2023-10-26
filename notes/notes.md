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

For python: use something like cython to improve performance

## ?
