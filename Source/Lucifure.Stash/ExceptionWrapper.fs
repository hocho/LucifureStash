// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Net
open Microsoft.FSharp.Core


module internal Exp = 

    let getStatusCode (response : WebResponse) = 
        match response with
        |   null    ->  enum -1
        |   _       ->  (response :?> HttpWebResponse).StatusCode

    let wrap<'a> (f : unit -> 'a) : 'a =

        try
            f()
        with
        |   :? StashException as stashEx     ->  reraise()
        |   _  as ex                         ->  Msg.RaiseInner Msg.errUnexpectedRuntime  ex
                                                 

    let wrapIgnoreResourceNotFoundException<'a> (f : unit -> 'a) : 'a =

        try
            f()
        with
        |   :? StashException as stashEx     ->  reraise()
        |   :? WebException as WebEx -> 

                match getStatusCode WebEx.Response with 
                |   HttpStatusCode.NotFound
                    -> Unchecked.defaultof<'a>
                |   _   
                    ->  Msg.RaiseInner Msg.errUnexpectedRuntime  WebEx
        |   _  as ex                         
            -> Msg.RaiseInner Msg.errUnexpectedRuntime  ex
                                                 

    // Converts an exception to another exception.
    // Mainly used to wrap HttpWebExceptions to something more user friendly.
    let map ex = 
        ()

    
    let consume (f : unit -> unit ) = 

        try
            f()
        with
        |   _   ->  ()

