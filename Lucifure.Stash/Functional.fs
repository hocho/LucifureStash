// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Common

open System
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
