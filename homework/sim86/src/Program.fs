open System
open System.IO

let readFile path =
    File.ReadAllBytes path
    
let clearDW (byte: byte) =
    let step1 = byte &&& ~~~(0b00000001uy <<< 1) // clear D flag
    let step2 = step1 &&& ~~~(0b00000001uy <<< 0) // clear W flag
    step2
    
let decodeInstructions (bytes: byte array) =
    let instruction = bytes[0]
    match clearDW instruction with
    | 0b10001000uy -> "mov"
    | _ -> failwith "unsupported instruction"
    
[<EntryPoint>]
let main args =
    // let value = 0b11111111uy
    // let cleared = clearDW value
    // printfn $"Original: %B{value}"
    // printfn $"Cleared:  %B{cleared}"
    let all = readFile args[0] |> decodeInstructions
    printfn $"Arguments:    %A{args}"
    printfn $"Length:       %A{all.Length}"
    printfn $"Instructions: %A{all}"
    0