// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Common

open System
open System.Linq
open System.Threading
open System.Collections
open System.Collections.Generic

module internal Functional = 

    let DoIf value test f = 
        if test value then
            f value
            
    let IfGet value test f = 
        if test value
            then value
            else f() 
        
// ---------------------------------------------------------------------------------------------------------------------

type IEnumeratorWrapper
        (inner                              :   IEnumerator
        ,callback                           :   obj -> unit) =
 
    interface IEnumerator with

        member this.Current 
            with get() =    

                let o = inner.Current
                callback o
                o

        member this.MoveNext() = 

            inner.MoveNext()

        member this.Reset() = 

            inner.Reset()


// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type IEnumeratorWrapper<'a>
        (inner                              :   IEnumerator<'a>
        ,callback                           :   obj -> unit) =
 
    interface IDisposable with
        
        member this.Dispose() = 
            inner.Dispose()

    interface IEnumerator with

        member this.Current 
            with get() =    

                let o = inner.Current
                callback o
                upcast o 

        member this.MoveNext() = 

            inner.MoveNext()

        member this.Reset() = 

            inner.Reset()
    

    interface IEnumerator<'a> with

        member this.Current 
            with get() =    

                let o = inner.Current
                callback o
                o

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type SafeReaderWriter() = 

    let lock = new ReaderWriterLockSlim()

    member this.Read f = 

        lock.EnterReadLock()

        try
            f()
        finally 
            lock.ExitReadLock()

    member this.Write f = 

        lock.EnterWriteLock()
        
        try
            f()
        finally 
            lock.ExitWriteLock()

    

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type DictionarySafe<'k, 'v>  when 'k : equality () = 
   
    let dict = new Dictionary<'k, 'v>()

    let safe = new SafeReaderWriter()

    // writers

    member this.Put key value = 

        fun () -> dict.[key] <- value

        |> safe.Write

    member this.Remove key = 

        (fun () -> dict.Remove key |> ignore) 

        |> safe.Write

    member this.Clear key = 

        (fun () -> dict.Clear() |> ignore) 

        |> safe.Write

    // readers

    member this.Get key = 

        fun () -> dict.[key]

        |> safe.Read

    member this.SafeGet key = 

        (fun () -> 
        match dict.TryGetValue key with
        |   true, v     ->  v
        |   _           ->  Unchecked.defaultof<'v>)

        |> safe.Read

    member this.ContainsKey key = 

        fun () -> dict.ContainsKey key 

        |> safe.Read

    member this.Keys = 

        fun () -> dict.Keys.ToList()

        |> safe.Read

    member this.Values = 

        fun () -> dict.Values.ToList()

        |> safe.Read

