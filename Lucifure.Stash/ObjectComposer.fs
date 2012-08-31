// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.Web
open System.IO
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Misc

module internal ObjectComposer = 

    let splitSuffix = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"

    let private split (o : obj) = 

        let splitString (str:string) = 

            let strLen = str.Length

            let rec splitString' offset = 
                seq {
                    match strLen - offset with
                    |   delta when delta <= stringMaxLen 
                            ->  yield  str.Substring(offset, delta)
                    |   _   
                            ->  yield  str.Substring(offset, stringMaxLen)
                                yield! splitString' (offset + stringMaxLen)
                }

            if strLen <= stringMaxLen
                then Seq.singleton str
                else splitString' 0 

        let splitBytes (bytes:byte array) = 

            let bytesLen = bytes.Length

            let rec splitBytes' offset = 
                seq {
                    match bytesLen - offset with
                    |   delta when delta <= bytesMaxLen 
                            ->  yield  subBytes bytes offset delta
                    |   _   
                            ->  yield  subBytes bytes offset bytesMaxLen
                                yield! splitBytes' (offset + bytesMaxLen)
                }

            if bytesLen <= bytesMaxLen
                then Seq.singleton bytes
                else splitBytes' 0 

        match o with
        |   :?  String      as x        ->  splitString x   :?> obj seq
        |   :?  (Byte[])    as x        ->  splitBytes x    :?> obj seq
        |   _               as x        ->  Seq.singleton x
        
    let internal splitSuffixed (nameValue:NameValue) : NameValues = 
        
        let values =
                split nameValue.Value  
                |>  Seq.toList

        if values.Length = 1 
            then Seq.singleton nameValue
            else values 
                |> Seq.mapi (fun idx value -> 
                                        NameValue(
                                            nameValue.Name + "_" + (splitSuffix.[idx]).ToString(), 
                                            value))        

    let private mergeString 
        (nameValues                         :   NameValues) =
    
        let sb = new StringBuilder()
        
        nameValues
        |>  Seq.iter    (fun nv ->  sb.Append nv.Value |> ignore)

        sb.ToString()

    let private mergeBytes
        (nameValues                         :   NameValues) =

        let count = nameValues 
                        |> Seq.fold (fun count nv -> 
                                            count + (nv.Value :?> byte array).Length) 0
 
        let bytes = Array.CreateInstance(typeof<byte>, count) :?> byte array

        nameValues 
            |> Seq.fold (fun offset b  ->   let subBytes = b.Value :?> byte array 

                                            Buffer.BlockCopy(
                                                subBytes,
                                                0,
                                                bytes,
                                                offset,
                                                subBytes.Length)
                                            
                                            offset + subBytes.Length
                        ) 0
            |>  ignore

        bytes

    let private merge
        (nameValues                         :   NameValues) =

        let nameValues = nameValues |> Seq.cache 
        
        if nameValues.Count() > 1 then
            match (nameValues |> Seq.head).Value with
            |   :?  String          ->  mergeString nameValues  :> obj
            |   :?  (Byte array)    ->  mergeBytes nameValues   :> obj
            |   _  as x             ->  Msg.Raise (Msg.errCannotMergeData x)
        else
            (Seq.head nameValues).Value  

    let internal getPrefixSuffix (name : string) = 
            
        let idx = name.LastIndexOf "_"
        if idx = -1
            then    name, ""
            else    name.Substring(0, idx), name.Substring (idx + 1)
        
    let internal getPrefix (name : string) =
        let prefix, suffix = getPrefixSuffix name
                
        if suffix.Is && splitSuffix.IndexOf suffix <> -1  // if valid split suffix (test for index)
            then    prefix                                  //  use prefix 
            else    name                                    //  else use the full original name
   
    // if a property was split, recompose it 
    let mergeSuffixed
        (toMerge                            :   string -> bool) 
        (nameValues                         :   NameValues) = 
    
        nameValues 
        |>  Seq.groupBy (fun nv -> 
                                let name = getPrefix nv.Name
                                match toMerge name with 
                                |   true    ->  name       // group by prefix 
                                |   _       ->  nv.Name)   // group by full name

        |>  Seq.map (fun (key, nvs) -> 
                        let idx = key.Length + 1
                        NameValue(
                            key,
                            nvs |>  Seq.sortBy   (fun nv -> nv.Name.Substring(idx))
                                |>  merge)  
                    )                      

