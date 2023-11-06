open System
open System.IO

let readFile path =
    File.ReadAllBytes path
    
let clearDW (byte: byte) =
    let cleared = byte &&& 0b11111100uy // clear D and W flag
    cleared
    
let decodeInstructions (bytes: byte array) =
    let byte1 = bytes[0]
    let byte2 = bytes[1]
    let opcode = clearDW byte1
    match opcode with
    | 0b10001000uy -> "mov"
    | _ -> failwith "unsupported instruction"
    
[<EntryPoint>]
let main args =
    if args.Length < 1
    then
        printfn "Usage: sim86.exe <path>"
    else
        let all = readFile args[0] |> decodeInstructions
        printfn $"Arguments:    %A{args}"
        printfn $"Instructions: %A{all}"
    0