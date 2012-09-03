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

// Used for types which have a one to one representation with the EdmType supported in the Azure table

type internal IntrinsicMapperScalar
        (memberInfo                         :   MemberInfo
        ,morph                              :   IStashMorph option
        ,keyMediate                         :   IStashKeyMediate option
        ,overrideTablePropertyName          :   string) =       // if !Is(), get name from MemberInfo

    let memberName = memberInfo.Name
        
    let tablePropertyName = 
        if overrideTablePropertyName.Is
            then overrideTablePropertyName
            else memberName
        |> cleanName

    let memberType,
        memberSet,
        memberGet = MemberAccessor.get memberInfo

    let memberTypeUnderlying = MorphIntrinsic.getUnderlyingType memberType

    let morph   =   match morph with
                    |   Some m  ->  MorphIntrinsic.validate m memberTypeUnderlying
                                    m
                    |   None    ->  MorphIntrinsic.get memberType

    let keyMediate =    match keyMediate with
                        |   Some m  ->  m
                        |   None    ->  KeyMediateNull.Instance

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
            with get() = keyMediate

        member this.SetMember
            (instance                       :   obj
            ,nameValues                     :   NameValues) = 

            nameValues |> Seq.iteri (fun idx nameValue ->
                                        if idx > 0 then
                                            Msg.Raise (Msg.errScalarVectorMismatch tablePropertyName)

                                        let value = morph.Outof nameValue.Value

                                        // only set it if it evaluates to non-null
                                        if value <> null then

                                            // confirm that the type matches
                                            if ReflectionHelper.isTypeOrSubTypeOf 
                                                                            (value.GetType())
                                                                            memberTypeUnderlying
                                                    |> not then
                                                Msg.Raise (Msg.errReceiveTypeMismatch memberName memberTypeUnderlying (value.GetType()))
        
                                            memberSet instance value
                                    )

        member this.GetMember
            (instance                       : obj)
                                            : NameValues = 

            // Morph into must yield a table storable type
            Seq.singleton (NameValue(    
                                tablePropertyName, 
                                morph.Into(memberGet instance)))
            

                             
// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
