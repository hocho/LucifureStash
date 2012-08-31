// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

module internal TypeSystem = 

    let makeEnumerableType (t : Type) = (typeof<IEnumerable<_>>).MakeGenericType(t)

    let rec findIEnumerable (seqType: Type) =
        let ty = 
                // if not specified or string
                if seqType = null || seqType = typeof<string> then
                    None

                // if array, return an ienumerable of the array type
                else if seqType.IsArray then
                    Some (makeEnumerableType (seqType.GetElementType()))
    
                // if a generic type, see if it can be assignable from an IEnumerable of the corresponding type
                else if seqType.IsGenericType then
                    seqType.GetGenericArguments() 
                            |> Seq.tryPick (fun arg -> 
                                                let ty = makeEnumerableType(arg)
                                                if ty.IsAssignableFrom(seqType) 
                                                    then Some ty
                                                    else None)  
                else 
                    None

        if ty.IsSome then
            ty       
        else    
            // recursively try with all interfaces of the type       
            let ty = seqType.GetInterfaces() |> Seq.tryFind (fun i -> (findIEnumerable i).IsSome)                    
        
            if ty.IsSome then
                ty
        
            // recursively try will all base classes of the type
            else if seqType.BaseType <> null && seqType.BaseType <> typeof<obj> then
                findIEnumerable seqType.BaseType
            else 
                None


    let GetElementType (seqType: Type) = 
        match findIEnumerable seqType with
        |   None        -> seqType 
        |   Some ienum  -> ienum.GetGenericArguments().[0]

