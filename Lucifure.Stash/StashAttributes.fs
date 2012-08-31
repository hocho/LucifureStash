// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

module internal StashAtbHelper = 

    let createMorpher (morpherType : Type Option) : IStashMorph Option =

        match morpherType with
        |   None        ->  None
        |   Some ty     ->  
            // only accept a type which support the IMorph interface
            if  ReflectionHelper.hasMorphInterface ty then
                try
                    Some (Activator.CreateInstance(ty) :?> IStashMorph)
                with _ as ex ->
                    Msg.RaiseCompiletime ([| Msg.errUnableToCreateIMorphInstance ty (ex.ToString()) |])
            else
                Msg.RaiseCompiletime ([| Msg.errDoesNotImplementIMorph ty |])

    let createKeyMediator (keyMediatorType : Type Option) : IStashKeyMediate Option =

        match keyMediatorType with
        |   None        ->  None
        |   Some ty     ->  
            // only accept a type which support the IMorph interface
            if  ReflectionHelper.hasMorphInterface ty then
                try
                    Some (Activator.CreateInstance(ty) :?> IStashKeyMediate)
                with _ as ex ->
                    Msg.RaiseCompiletime ([| Msg.errUnableToCreateIMorphInstance ty (ex.ToString()) |])
            else
                Msg.RaiseCompiletime ([| Msg.errDoesNotImplementIMorph ty |])



// ---------------------------------------------------------------------------------------------------------------------

[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashKeyBaseAttribute
        (name                               :   string) =
 
    inherit Attribute()

    let mutable
        morpherType                         :   Type Option     = None

    let mutable
        keyMediatorType                     :   Type Option     = None

    interface IStashProvider with

        member this.Create memberInfo = 

            let ty = ReflectionHelper.getMemberType memberInfo

            IntrinsicMapperScalar(
                            memberInfo, 
                            StashAtbHelper.createMorpher morpherType, 
                            StashAtbHelper.createKeyMediator keyMediatorType,
                            name) 
                :> IStash
    
    member 
        this.Morpher
            with set(ty : Type) = 
                morpherType <- Some ty

            and get() = 
                match morpherType with
                |   Some x  ->  x
                |   None    ->  null

    member 
        this.KeyMediator
            with set(ty : Type) = 
                keyMediatorType <- Some ty

            and get() = 
                match keyMediatorType with
                |   Some x  ->  x
                |   None    ->  null

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// A StashPartitionKey attribute is used to indicate that a member the Row Key of the Azure storage row.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashPartitionKeyAttribute() = 

    inherit StashKeyBaseAttribute(Literal.PKey)

// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// A StashRowKey attribute is used to indicate that a member the Row Key of the Azure storage row.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashRowKeyAttribute() = 

    inherit StashKeyBaseAttribute(Literal.RKey)

// ---------------------------------------------------------------------------------------------------------------------
// Timestamp is not allowed any morphing capability.

/// <summary>
/// A StashTimestamp attribute is used to indicate that a member of type, DateTime, is the timestamp value of the Azure storage row.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashTimestampAttribute() = 
    inherit Attribute()

    interface IStashProvider with

        member this.Create memberInfo = 

            IntrinsicMapperScalar(memberInfo, None, None, Literal.Timestamp) :> IStash

    
// ---------------------------------------------------------------------------------------------------------------------
// ETag is not allowed any morphing capability.

/// <summary>
/// A StashETag attribute is used to provide optimistic concurrency for update operations.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashETagAttribute() = 
    inherit Attribute()

    interface IStashProvider with

        member this.Create memberInfo = 

            IntrinsicMapperScalar(memberInfo, None, None, Literal.ETag) :> IStash

    
// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
/// A StashPool attribute is used to indicate that a member should be included for interacting with Azure storage.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashBaseAttribute() =
    
    inherit NamedAttribute()

    let mutable
        morpherType                             :   Type Option = None

    member internal this.MorpherType 
                with get() = morpherType

    member 
        this.Morpher
            with set(ty : Type) = 
                morpherType <- Some ty

            and get() = 
                match morpherType with
                |   Some x  ->  x
                |   None    ->  null

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
/// A Stash attribute is used to indicate that a member should be included for interacting with Azure storage.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashAttribute() =
    
    inherit StashBaseAttribute()

    interface IStashProvider with

        member this.Create memberInfo = 

            let ty = ReflectionHelper.getMemberType memberInfo

            IntrinsicMapperScalar(
                                memberInfo, 
                                StashAtbHelper.createMorpher this.MorpherType, 
                                None,
                                this.Name) 
                :> IStash

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
/// A StashCollection attribute is used to indicate that a member supporting the IList or IList<T<> should 
/// be included for interacting with Azure storage.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashCollectionAttribute() =
    
    inherit StashBaseAttribute()

    interface IStashProvider with

        member this.Create memberInfo = 

            let ty = ReflectionHelper.getMemberType memberInfo

            if VectorHelper.isIList ty |> not
                then    Msg.RaiseCompiletime ([| Msg.errStashCollectionAttributeAppliedToNonCollection memberInfo.Name |])
                
            IntrinsicMapperVector(
                            memberInfo, 
                            StashAtbHelper.createMorpher this.MorpherType, 
                            this.Name) 
                :> IStash

// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// The StashPool attribute can only be applied to a member type which supports the IDictionary&lt;string, object&gt; interface.
/// A StashPool attribute can be used to decorate at the most one member of a type.
/// </summary>
[<AttributeUsage(enum AtbType.Field,  AllowMultiple = false, Inherited = true)>]
type StashPoolAttribute() =
    inherit Attribute()

    interface IStashPoolProvider with

        member this.Create memberInfo = 

            if DictionaryHelper.hasDictionaryInterface (ReflectionHelper.getMemberType memberInfo)
                then    DictionaryMapper(memberInfo) :> IStashPool
                else    Msg.RaiseCompiletime ([| Msg.errStashPoolAttributeOnNonDictionary memberInfo.Name |])
    
            

// ---------------------------------------------------------------------------------------------------------------------
