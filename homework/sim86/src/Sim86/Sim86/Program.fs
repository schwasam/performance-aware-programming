module Sim86

open System.IO
open BitSyntax

let readFile path =
    let fileStream = File.OpenRead path
    fileStream.Position <- 0
    let memoryStream = new MemoryStream()
    fileStream.CopyTo memoryStream
    memoryStream.Position <- 0
    memoryStream
    
let readFile2 path =
    let stream = new MemoryStream()
    do bitWriter stream {
        // instruction 1
        do! BitWriter.WriteByte(0b10001000uy)
        do! BitWriter.WriteByte(0b00000000uy)
        
        // instruction 2
        do! BitWriter.WriteByte(0b10001011uy)
        do! BitWriter.WriteByte(0b00000000uy)
    }
    stream.Position <- 0
    stream
    
let printStream (stream: MemoryStream) =
    stream.Position <- 0
    let bytes = stream.ToArray()
    for byte in bytes do
        printfn $"%08B{byte}" // pad: 0, width: 8
    printfn $"Bytes: {stream.Length}"
    
let decodeInstructions (stream: Stream, index: int64) =
    let opcode, d, w, mode, reg, rm =
        bitReader stream {
            // printfn $"Stream Position (before byte 1): %d{stream.Position}"
            // printfn $"Stream Length (before byte 1): %d{stream.Length}"
            
            // byte 1
            stream.Position <- index
            let! opcode = BitReader.ReadByte(6)
            let! d = BitReader.ReadBool() // 1 bit
            let! w = BitReader.ReadBool() // 1 bit
            
            // printfn $"Stream Position (after byte 1): %d{stream.Position}"
            
            // byte 2
            stream.Position <- index + 8L
            let! mode = BitReader.ReadByte(2)
            let! reg = BitReader.ReadByte(3)
            let! rm = BitReader.ReadByte(3)
            
            // printfn $"Stream Position (after byte 2): %d{stream.Position}"
            stream.Position <- index + 16L
            
            return opcode, d, w, mode, reg, rm
        }
    match opcode with
    | 0b100010uy -> $"opcode: %B{opcode}, d: %b{d}, w: %b{w}," //mod: %B{mode}, reg: %B{reg}, rm: %B{rm}"
    | _ -> failwith $"unknown opcode: %B{opcode}"
    
[<EntryPoint>]
let main args =
    if args.Length = 0
    then
        printfn "Usage: Sim86.exe <path>"
    else
        let stream = readFile2 args[0]
        printStream stream
        
        // printfn $"Stream Position (before decoding): %d{stream.Position}"
        // decodeInstructions(stream, 0L) |> printfn "%A"
        // printfn $"Stream Position (after 1 decode(s)): %d{stream.Position}"
        // decodeInstructions(stream, stream.Position) |> printfn "%A"
        // printfn $"Stream Position (after 2 decode(s)): %d{stream.Position}"
    0