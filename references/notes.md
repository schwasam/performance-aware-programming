# References

## The 8086 Family User's Manual - October 1979

### Machine Instruction Encoding and Decoding (p. 257ff)

- the first six bits contain an OPCODE that identifies the basic instruction type (ADD, XOR etc.)
- the D field specifies the direction:
  - 0 = the REG field identifies the source operand
  - 1 = the REG field in the second byte identifies the destination operand
- the W field distinguishes between byte and word operations:
  - 0 = byte
  - 1 = word
- the MOD field identifies the mode (whether one of the operands is in memory or whether both operands are registers)
- the REG field identifies a register that is one of the instruction operands (and is also used as an extension of the OPCODE for some instructions)
- the R/M field identifies the register/memory (how effective address is calculated)

| Field: | OPCODE | D   | W   | MOD | REG | R/M |
| ------ | ------ | --- | --- | --- | --- | --- |
| Bits:  | 6      | 1   | 1   | 2   | 3   | 3   |
