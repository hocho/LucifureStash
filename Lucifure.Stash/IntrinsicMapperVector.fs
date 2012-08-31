// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection
open System.Collections;

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional
open CodeSuperior.Common.Misc

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
// Used for types which have a one to many representation with the EdmType supported in the Azure table

type internal IntrinsicMapperVector
        (memberInfo                         :   MemberInfo  
        ,morph                              :   IStashMorph option
        ,overrideTablePropertyName          :   string) =       // if !Is(), get name from MemberInfo

    let memberName = memberInfo.Name

    let tablePropertyName = 
        if overrideTablePropertyName.Is
            then overrideTablePropertyName
            else memberName
        |> cleanName

    let memberType, listElementType, memberSet, memberGet = 
            VectorHelper.getSetterGetter memberInfo tablePropertyName

    let listElementTypeUnderlying = MorphIntrinsic.getUnderlyingType listElementType

    // Note: Cannot support morphing for object types - do dynamic morphing for intrinsic cast able types?
    let morph   =   match morph with
                    |   Some m  ->  MorphIntrinsic.validate m listElementTypeUnderlying
                                    m
                    |   None    ->  MorphIntrinsic.get listElementType

    // pre build the function depending on the list element type
    let MorphAndTest : (NameValue -> NameValue) = 
        match listElementType with
        |   x when x = typeof<obj>
            
            ->  // cannot do any validation after morphing back
                fun nv -> NameValue(nv.Name, morph.Outof nv.Value)

        |   _   
            ->  fun nv ->
                                        
                    // morph back and 
                    let value = morph.Outof nv.Value
                        
                    // validate
                    if value <> null && value.GetType() <> listElementTypeUnderlying then
                        Msg.Raise (Msg.errReceiveTypeMismatch memberName listElementTypeUnderlying (value.GetType()))

                    NameValue(nv.Name, value)


    interface IStash with

        member this.TablePropertyName
            with get() = tablePropertyName

        member this.MemberName 
            with get() = memberName
            
        member this.MemberType
            with get() = memberType

        member this.Morpher
            with get() = morph

        member this.KeyMediator
            with get() = Unchecked.defaultof<IStashKeyMediate>

        member this.SetMember
                (instance                           :   obj
                ,nameValues                         :   NameValues) = 

            nameValues 
            |>  Seq.map MorphAndTest
            |>  Seq.filter (fun nv -> nv.Value <> null)
            |>  memberSet instance 

        member this.GetMember
                (instance                           : obj)
                                                    : NameValues = 
        
            memberGet instance 
            |> Seq.map (fun nameValue -> 
                                NameValue(nameValue.Name, morph.Into nameValue.Value))
