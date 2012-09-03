// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection
open System.Collections;

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal DictionaryMapperNull
        (memberInfo                         :   MemberInfo) =

    interface IStashPool with

        member this.SetMember
                (instance                   :   obj
                ,nameValues                 :   NameValues) = 
        
            ()

        member this.GetMember
            (instance                       :   obj)
                                            :   NameValues =    
            Seq.empty<NameValue>

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal DictionaryMapper
        (memberInfo                         :   MemberInfo) =

    static let nullInstance = new DictionaryMapperNull(null) :> IStashPool

    let settersAll, getterAll = DictionaryHelper.getSetterGetter memberInfo

    static member Null  
                with get() = nullInstance

    interface IStashPool with

        member this.SetMember
                    (instance                   :   obj
                    ,nameValues                 :   NameValues) = 
    
            settersAll instance nameValues

        member this.GetMember
                    (instance                   :   obj)
                                                :   NameValues =    
            getterAll instance
            


                                 

    


