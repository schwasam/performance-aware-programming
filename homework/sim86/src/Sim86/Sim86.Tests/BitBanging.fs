module Sim86.Tests

open System.IO
open BitSyntax
open FsUnit
open NUnit.Framework

[<TestFixture>]
type ``BitBanging tests`` () =
    [<Test>]
    member test.``bitReader should be able to read a MOV instruction`` () =
        let stream = new MemoryStream()

        do bitWriter stream {
            // instruction (2 bytes)
            do! BitWriter.WriteByte(0b10001000uy)
            do! BitWriter.WriteByte(0b00000000uy)
        }

        stream.Position <- 0L

        let opcode, d, w, mode, reg, rm =
            bitReader stream {
                // byte 1
                let! opcode = BitReader.ReadByte(6)
                let! d = BitReader.ReadBool() // 1 bit
                let! w = BitReader.ReadBool() // 1 bit
            
                // byte 2
                let! mode = BitReader.ReadByte(2)
                let! reg = BitReader.ReadByte(3)
                let! rm = BitReader.ReadByte(3)
            
                return opcode, d, w, mode, reg, rm
            }

        stream.Length |> should equal 2
        stream.Position |> should equal 2

        opcode |> should equal 0b100010uy
        d |> should equal false
        w |> should equal false

        mode |> should equal 0b00uy
        reg |> should equal 0b000uy
        rm |> should equal 0b000uy

    [<Test>]
    member test.``bitReader should be able to read another MOV instruction`` () =
        let stream = new MemoryStream()

        do bitWriter stream {
            // instruction (2 bytes)
            do! BitWriter.WriteByte(0b10001011uy)
            do! BitWriter.WriteByte(0b11000111uy)
        }

        stream.Position <- 0L

        let opcode, d, w, mode, reg, rm =
            bitReader stream {
                // byte 1
                let! opcode = BitReader.ReadByte(6)
                let! d = BitReader.ReadBool() // 1 bit
                let! w = BitReader.ReadBool() // 1 bit
            
                // byte 2
                let! mode = BitReader.ReadByte(2)
                let! reg = BitReader.ReadByte(3)
                let! rm = BitReader.ReadByte(3)
            
                return opcode, d, w, mode, reg, rm
            }

        stream.Length |> should equal 2
        stream.Position |> should equal 2

        opcode |> should equal 0b100010uy
        d |> should equal true
        w |> should equal true

        mode |> should equal 0b11uy
        reg |> should equal 0b000uy
        rm |> should equal 0b111uy
