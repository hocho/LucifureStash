// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Common

open System
open System.Text
open System.Reflection

module internal Misc =
        
    let stringMaxLen = (1024 * 64) / 2

    let bytesMaxLen = 1024 * 64

    let ifNotNull x = if x <> null then Some x else None

    let cleanName (name : string) = name

    let bindList<'t> (toBind : 't -> bool) (l : 't list) (item : 't) =                 
        match toBind item with true -> item :: l | _ -> l

    // unsafe 
    let subBytes (bytes:byte array) offset (count:int) = 
        
        let result = Array.CreateInstance(typeof<byte>, count) :?> byte array
        
        Buffer.BlockCopy(bytes, offset, result, 0, count)

        result

    let bytesToHex : Byte[] -> string = 

        let hex = Array.init 256 (fun idx -> idx.ToString("x2"))
            
        let f (bytes : Byte[]) = 
            let sb = StringBuilder(bytes.Length)

            bytes |> Seq.iter (fun b -> sb.Append(hex.[int b]) |> ignore)

            sb.ToString()    
        
        f
    
    let TimeSpanMax (ts1 : TimeSpan) (ts2 : TimeSpan) = 
        
        if ts1 > ts2 
            then ts1
            else ts2
        
    let TimeSpanMin (ts1 : TimeSpan) (ts2 : TimeSpan) = 
        
        if ts1 < ts2 
            then ts1
            else ts2

    let TimeSpanMinMax (tsMin : TimeSpan) (tsMax : TimeSpan) (ts : TimeSpan) =
        
        if ts < tsMin then
            tsMin
        elif ts > tsMax then
            tsMax
        else
            ts
             

module internal Seq = 

    let firstOrDefault<'a> (s : 'a seq) = 
        if Seq.isEmpty s
            then    Unchecked.defaultof<'a>
            else    Seq.head s

type internal Caster =
 
    static member private Cast<'a> (o : obj) : 'a = 
        o :?> 'a

    static member CastTo ty o = 

        typeof<Caster>
            .GetMethod("Cast", BindingFlags.NonPublic ||| BindingFlags.Static)
            .MakeGenericMethod([| ty |]).Invoke(null, [| o |])

