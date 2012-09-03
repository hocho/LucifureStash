// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.IO
open System.Reflection
open System.Collections.Generic

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type IStashMorph =
    
    /// <summary>
    /// True if can morph the indicated type
    /// </summary>
    abstract member CanMorph                    :   Type -> bool    

    /// <summary>
    /// Morph from original Type member data to Table property data
    /// </summary>
    abstract member Into                        :   obj -> obj

    /// <summary>
    /// Morph Table property data to original Type member data
    /// </summary>
    abstract member Outof                       :   obj -> obj

    /// <summary>
    /// true if the collating sequence of the original and morphed is identical
    /// </summary>
    abstract member IsCollationEquivalent       :   bool            

type IStashKeyMediate = 

    /// <summary>
    /// True if the string represents the whole key and not just part of the key (composite key)
    /// Used for Query modification.
    /// </summary>
    abstract member IsCompleteKeyValue          :   string -> bool

        
    

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type IStashMorphProvider =
    
    abstract member Create                      :   string * Type -> IStashMorph       // Name * Type

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal MorphNull() = 

    static let instance = MorphNull() :> IStashMorph

    static member Instance with get() = instance

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool    = true

        member this.IsCollationEquivalent 
                                with get()              = true
                
        member this.Into
                (value                      :   obj)
                                            :   obj     = value 
        member this.Outof
                (value                      :   obj)
                                            :   obj     = value

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal MorphProviderNull() = 

    interface IStashMorphProvider with

        member this.Create
                (name                       :   string
                ,ty                         :   Type) = 

            MorphNull.Instance

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal KeyMediateNull() = 

    static let instance = KeyMediateNull() :> IStashKeyMediate

    static member Instance with get() = instance

    // morphs into a string
    interface IStashKeyMediate with

        member this.IsCompleteKeyValue
                (value                      :   string) = true

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
