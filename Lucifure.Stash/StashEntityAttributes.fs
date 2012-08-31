// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Net
open System.Reflection

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional

// ---------------------------------------------------------------------------------------------------------------------
// Attributes
// ---------------------------------------------------------------------------------------------------------------------

module internal AtbType = 
    [<Literal>]
    let Field = 384         // 128 + 256 (Property + Field) - Hard coded because F# did not consider + or ||| as constant folding

// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// Implicit - all public properties are used. 
/// Hybrid - all public properties and Stash annotated members.
/// Hybrid mode is default.
// Explicit - only annotated members.
/// </summary>
type StashMode = 
    |   Implicit = 1
    |   Hybrid   = 2
    |   Explicit = 3

// ---------------------------------------------------------------------------------------------------------------------

type NamedAttribute() = 
    inherit Attribute()

    let mutable 
        _name                               :   string = ""

    member 
        this.Name 
            with set(name) = 
                _name <- name
            and get()
                = _name

// ---------------------------------------------------------------------------------------------------------------------

[<AttributeUsage(AttributeTargets.Class,  AllowMultiple = false, Inherited = true)>]
type StashEntityAttribute() = 
    inherit NamedAttribute()

    let mutable 
        _mode                               :   StashMode = StashMode.Hybrid

    member this.Mode 
            with set(mode) = 
                _mode <- mode
            and get()
                = _mode

