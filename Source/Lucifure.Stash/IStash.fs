// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection
open System.Collections.Generic

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type IStash =
    
    abstract member TablePropertyName           :   string 
        
    abstract member MemberName                  :   string 
    
    abstract member MemberType                  :   Type

    abstract member Morpher                     :   IStashMorph

    abstract member KeyMediator                 :   IStashKeyMediate

    abstract member SetMember                   :   obj * NameValues -> unit   

    abstract member GetMember                   :   obj -> NameValues

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
// All explicit attributes need to support these interfaces

type IStashProvider =

    abstract member Create                      :   MemberInfo -> IStash

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type IStashPool =
    
    abstract member SetMember                   :   obj * NameValues -> unit           

    abstract member GetMember                   :   obj -> NameValues

// ---------------------------------------------------------------------------------------------------------------------
// All explicit attributes need to support these interfaces

type IStashPoolProvider =

    abstract member Create                      :   MemberInfo -> IStashPool

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
