// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open Microsoft.FSharp.Core


module internal Exp = 

    let wrap<'a> (f : unit -> 'a) : 'a =

        try
            f()
        with
        |   :? StashException as stashEx     ->  reraise()
        |   _  as ex                         ->  Msg.RaiseInner Msg.errUnexpectedRuntime  ex
                                                 //reraise()

    // Converts an exception to another exception.
    // Mainly used to wrap HttpWebExceptions to something more user friendly.
    let map ex = 
        ()

    
    let consume (f : unit -> unit ) = 

        try
            f()
        with
        |   _   ->  ()

