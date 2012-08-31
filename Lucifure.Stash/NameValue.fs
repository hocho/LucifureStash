// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection
open System.Collections.Generic

// ---------------------------------------------------------------------------------------------------------------------

type NameValue
        (name                               :   string
        ,value                              :   obj) =

    member this.Name
        with get() = name

    member this.Value
        with get() = value

// ---------------------------------------------------------------------------------------------------------------------

type NameValues = NameValue seq

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
