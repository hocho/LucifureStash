// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.Web
open System.Collections.Generic
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Net
open System.Reflection

open CodeSuperior.Common
open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional

// -------------------------------------------------------------------------------------------------------

type internal EntityOp<'a when 'a : equality>
        (op                                 :   CommandType
        ,item                               :   'a) =

    member this.Op with get() = op

    member this.Item with get() = item

// -------------------------------------------------------------------------------------------------------

type EntityDescriptor<'a when 'a : equality>
        (entity                         :   'a
        ,state                          :   EntityState) = 

    member this.State with get() = state

    member this.Entity with get() = entity

// ---------------------------------------------------------------------------------------------------------------------

type internal MappingType = 
        |   MemberMapping                   of   MemberMappingType * StashMode
        |   DictionaryMapping               of   IStashPool
        |   Message                         of   StashMessage
        |   NotAvailable

// ---------------------------------------------------------------------------------------------------------------------

type internal TypeReflector
        (ty                                 :   Type) = 
    
    static let internalMapper = StashAttribute() :> IStashProvider

    static let regexPatternEntitySetName = "^[A-Za-z][A-Za-z0-9]{2,62}$"
    static let regexEntitySetName = new Regex(regexPatternEntitySetName)

    let bindingFlagsDeclared = BindingFlags.Instance 
                            ||| BindingFlags.NonPublic 
                            ||| BindingFlags.Public 

    let bindingFlagsBase = BindingFlags.Instance 
                                ||| BindingFlags.NonPublic 

    //if a table attribute exists, pull out the table name, if specified and mode
    let entitySetName, 
        mode       =   ReflectionHelper.getTableInfo ty

    let validateEntitySetName (name : string) = 
        seq {
            if name.Length < 3  || name.Length > 63 || not (regexEntitySetName.IsMatch name) then
                yield (Msg.errInvalidEntitySetName entitySetName regexPatternEntitySetName)
        }                                     

    let getMappingFromAtb
        (   memberInfo                             :   MemberInfo)  
                                                   :   MappingType = 

        try
            // get a list of our attributes                                            
            let atbs =  memberInfo.GetCustomAttributes(true)
                        |>  Seq.filter (fun atb ->  match atb with
                                                    |   :? IStashProvider
                                                    |   :? IStashPoolProvider   ->  true
                                                    |   _                       ->  false)
                        |> Seq.toList

            match atbs.Length with

            |   0   ->  NotAvailable

            |   1   ->  let atb = atbs.Head
                
                        match atb with 
                        |   :?  StashTimestampAttribute when 
                                    ReflectionHelper.getMemberType memberInfo <> typeof<DateTime> 
                                ->  Message (Msg.errStashTimestampAttributeIncorrectType memberInfo.Name)

                        |   :?  StashETagAttribute when
                                    ReflectionHelper.getMemberType memberInfo <> typeof<String> 
                                ->  Message (Msg.errStashETagAttributeIncorrectType memberInfo.Name)
                
                        |   :? IStashProvider as mapper 
                                ->  let memberMappingType
                                        =   match atb with
                                            |   :?  StashKeyBaseAttribute
                                                    ->  Key (mapper.Create memberInfo)     
                                            |   _   
                                                    ->  Data (mapper.Create memberInfo)
                                    MemberMapping (
                                                memberMappingType, 
                                                StashMode.Explicit)

                        |   :? IStashPoolProvider as mapper 
                                ->  DictionaryMapping (mapper.Create memberInfo)

                        | _     ->  NotAvailable
                        
            |   _   ->  Message (Msg.errStashAttributeMultiple memberInfo.Name)

        with
        |   :?  StashCompiletimeException as ex -> Message (ex.Messages.[0])    // assume exactly one message

    // first get public and private (properties & field) on the declared type and then private properties on the bases
    // because reflections does not return private members on base types
    let getMemberInfoProperties = 
 
        let rec getProperties flags (ty : Type) =             

            seq {          
                // returns a sequence of all properties with both a getter and a setter ...
                yield!
                    ty.GetProperties flags
                    |> Seq.map (
                        fun m ->    
                            if m.CanRead && m.CanWrite then

                                let getter      = m.GetGetMethod(true)
                                let setter      = m.GetSetMethod(true)
                                let isPublic    = getter.IsPublic && setter.IsPublic
                                let isPrimitive = ObjectConverter.isPrimitiveNullable m.PropertyType

                                let mapping = getMappingFromAtb (m :> MemberInfo)

                                match mode with 
                                |   StashMode.Implicit      

                                        ->  match mapping with 
                                            |   MappingType.NotAvailable
                                                    ->  if isPublic && isPrimitive
                                                            then    MemberMapping( 
                                                                        Data (internalMapper.Create (m :> MemberInfo)),
                                                                        StashMode.Implicit)
                                                            else    NotAvailable
                                            |   MappingType.Message s
                                                    ->  mapping
                                            |   _   ->  Message Msg.errStashAttributeInImplicitMode

                                |   StashMode.Hybrid

                                        ->  match mapping with 
                                            |   MappingType.NotAvailable when isPublic && isPrimitive
                                                    ->  MemberMapping( 
                                                            Data (internalMapper.Create (m :> MemberInfo)),
                                                            StashMode.Implicit)
                                            |   _   ->  mapping
                                                    
                                |   StashMode.Explicit

                                        ->  mapping
                                                        
                                |   _   ->  NotAvailable
                            else
                                NotAvailable    
                        )

                if ty.BaseType <> typeof<obj> then
                    yield! getProperties bindingFlagsBase ty.BaseType 
            }

        getProperties bindingFlagsDeclared ty

    let getMemberInfoFields = 
            // returns a sequence of all fields
            let rec getFields flags (ty : Type) = 
                seq {
                    yield!  ty.GetFields flags
                            |> Seq.map (fun m -> getMappingFromAtb (m :> MemberInfo))
                
                    if ty.BaseType <> typeof<obj> then
                        yield! getFields bindingFlagsBase ty.BaseType 
                }

            if mode <> StashMode.Implicit then
                getFields bindingFlagsDeclared ty
            else
                Seq.empty<MappingType>

    let mapper = MemberPropertyMapper()

    let isPropertyExplicit = mapper.IsExplicit
    
    let isPropertySplitable = 
        function
        |   Literal.PKey
        |   Literal.RKey
        |   Literal.Timestamp
        |   Literal.ETag        ->  false
        |   _  as name          ->  isPropertyExplicit name

    let isPropertySplitableTrue (property : string) = true

    let getIStash memberMappingType = 

        match memberMappingType with
        |   Data istash     -> istash
        |   Key istash      -> istash

    // seq of MemberPropertyMapping 
    let dictMappings, errors =   
        
                let validateMemberName = 
                    if mode = StashMode.Implicit then  // implicit mode is not restricted for backward compatibility
                        (fun name -> None)
                     else
                        (fun name ->
                            match isPropertyExplicit name && (name.IndexOf('_') <> -1) with 
                            |   true    ->  Some (Msg.errInvalidTableProperty name)
                            |   false   ->  None
                    )
                
                // used by a fold. State is sent and returned
                let load (errorList, dictMappings) mappingType : (StashMessage list * IStashPool list) = // error * dictionary pool list

                    match mappingType with 
                            |   MemberMapping (memberMappingType, mode)
                                    ->  // add first and then 
                                        let errorList = 
                                            (mapper.Add memberMappingType mode)
                                            |> Seq.fold (fun el msg -> msg :: el) errorList

                                        // validate
                                        let errorList = 
                                            match (validateMemberName (getIStash memberMappingType).TablePropertyName) with
                                            |   Some x  ->  x :: errorList
                                            |   None    ->  errorList

                                        errorList, dictMappings

                            |   DictionaryMapping dm
                                    ->  let errorList = 
                                            if dictMappings.Length > 0 
                                                then Msg.errMultipleStashPoolAttributes :: errorList 
                                                else errorList 
                                                    
                                        errorList, dm::dictMappings
                                                            
                            |   Message m
                                    ->  m::errorList, dictMappings

                            |   _
                                    -> errorList, dictMappings

                // begin by validating the entity set name                                
                let errorList = validateEntitySetName entitySetName |> Seq.toList

                // process all properties and fields and get errors
                let errorList, dictMappings = 
                    Seq.append 
                        getMemberInfoProperties 
                        getMemberInfoFields
                    |> Seq.fold load (errorList, [])

                // make sure that the row and partition key have been defined
                let errorList = 
                    seq {
                        yield! errorList

                        if mapper.HasPartitionKey() |> not then
                            yield Msg.errKeyNotDefined Literal.PKey
                        if mapper.HasRowKey() |> not then
                            yield Msg.errKeyNotDefined Literal.RKey
                    }   
                    |>  Seq.toList

                if errorList.Length > 0 then
                    errorList 
                    |>  Seq.groupBy (fun sm -> sm.Message)    // eliminate duplicates
                    |>  Seq.map (fun (k, l) -> Seq.head l)           
                    |>  Seq.toArray
                    |>  (fun msgs -> Msg.RaiseCompiletime msgs)

                let dictMapping =   if dictMappings.Length = 0
                                        then DictionaryMapper.Null
                                        else dictMappings.[0]

                dictMapping, errorList

    let partitionKeyGetter  =   mapper.GetPartitionKeyGetter()    

    let rowKeyGetter        =   mapper.GetRowKeyGetter()    

    let etagGetter          =   if mapper.HasETag() then
                                    mapper.GetETagGetter()
                                else
                                    fun instance    ->  dictMappings.GetMember instance
                                                        |>  Seq.filter (fun nv -> nv.Name = Literal.ETag)
                                                        |>  Seq.cache
                                                        |>  Seq.firstOrDefault 
                                                        |>  function
                                                            |   nameValue when nameValue <> Unchecked.defaultof<NameValue>
                                                                    ->  (string) nameValue.Value
                                                            |   _                                  
                                                                    ->  Constant.ETagNoMatch
                                                
    let etagSetter          =   if mapper.HasETag() then
                                    mapper.GetETagSetter()
                                else
                                    fun instance value -> dictMappings.SetMember(
                                                                            instance, 
                                                                            NameValue(Literal.ETag, value) |> Seq.singleton)
    
    // these values are not be sent to the service
    static 
        let Unsendable (nv : NameValue) =
            nv.Value <> null                    &&
            nv.Name  <> Literal.Timestamp       &&
            nv.Name  <> Literal.ETag

    // member and dictionary splitting is handled slightly differently
    let memberSplitter (nv : NameValue)  =  
        (if isPropertySplitable nv.Name 
            then    ObjectComposer.splitSuffixed nv
            else    Seq.singleton nv)

    let getNameValues supportLargeObjectsInPool instance = 

        let dictionarySplitter =    
                if supportLargeObjectsInPool 
                    then    ObjectComposer.splitSuffixed
                    else    Seq.singleton

        // build one big list of name values
        Seq.append 
            ((mapper.GettersInvoke instance)
                |>  Seq.filter Unsendable
                |>  Seq.map memberSplitter)
            ((dictMappings.GetMember instance)
                |>  Seq.filter Unsendable
                |>  Seq.map dictionarySplitter)
        |> Seq.concat

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    // Sets the value of all the members

    member this.SetValues 
            (ignoreMissingProperties        :   bool) 
            (supportLargeObjectsInPool      :   bool)
            (nameValues                     :   NameValues) = 
        
        let instance = Activator.CreateInstance(ty)

        let unmapped = 
            nameValues 
            |>  ObjectComposer.mergeSuffixed isPropertySplitable

            |>  Seq.groupBy (fun nameValue -> 
                                mapper.GetPropertyGroupName nameValue.Name
                            )

            |>  Seq.choose  (fun (key, nameValues) -> 
                                            if mapper.SetterInvoke instance nameValues
                                                then None
                                                else Some(nameValues)
                            )
            |>  Seq.toList
        
        if unmapped.Length > 0 then
            if dictMappings <> DictionaryMapper.Null then

                let unmapped = 
                    if supportLargeObjectsInPool 
                        then    ObjectComposer.mergeSuffixed isPropertySplitableTrue (Seq.concat unmapped)
                        else    Seq.concat unmapped 
    
                dictMappings.SetMember(
                                    instance, 
                                    unmapped)                   

            elif not ignoreMissingProperties then             // if no ignoring, raise an exception
              
                // format property names
                let properties =    
                    unmapped
                    |>  Seq.map (fun nv -> ObjectComposer.getPrefix (Seq.head nv).Name)
                    |>  Seq.filter (fun name -> name <> Literal.ETag && name <> Literal.Timestamp)
                    |>  Seq.map (fun name ->  sprintf 
                                                "'%s'" 
                                                name)
                    |>  String.concat ", "       
                                                             
                Msg.Raise (Msg.errMissingMembersInType properties)
                    
        instance

    member this.ContentParse
        (xml                                :   string)
                                            :   NameValues seq = 

        XmlResponseParser.getEntities xml

    member this.ContentBuild
            (supportLargeObjectsInPool      :   bool)
            (instance                       :   obj)
                                            :   string  * string = 

        etagGetter instance, 
        XmlRequestBuilder.buildEntityBody 
                    (getNameValues supportLargeObjectsInPool instance)

    member this.ContentBuildBatch
            (supportLargeObjectsInPool      :   bool)
            (batchId                        :   Guid)
            (list                           :   (string * EntityOp<'a>) seq)
                                            :   string = 

        let opValues = 
            list
            |>  Seq.map (fun (target, eo) ->    let nvs = getNameValues supportLargeObjectsInPool eo.Item |> Seq.cache

                                                new BatchInfo(
                                                        eo.Op,
                                                        target,
                                                        if eo.Op = CommandType.Insert
                                                            then    Constant.ETagNoMatch
                                                            else    etagGetter eo.Item)
                                                , nvs)            

        XmlRequestBuilder.buildEntityBodyBatched
                    batchId
                    opValues

    member this.EntitySetName 
            with get() = entitySetName

    member this.Mode 
            with get() = mode

    member this.Mapper 
            with get() = mapper

    member this.GetPartitionKeyValue
            (inst                           :   obj) = 

        partitionKeyGetter inst

    member this.GetRowKeyValue
            (inst                           :   obj) = 

        rowKeyGetter inst

    // returns the partition key and row key such that it uniquely identifies the entity
    member this.GetEntityId
            (inst                           :   obj) = 

        sprintf "%s?%s" (partitionKeyGetter inst) (rowKeyGetter inst)

    member this.GetETagValue
            (inst                           :   obj) = 

        etagGetter inst

    member this.SetETagValue
            (inst                           :   obj) 
            (value                          :   string) = 

        etagSetter inst value

    member this.ComputeEntityDataSize
            (supportLargeObjectsInPool      :   bool)
            (instance                       :   obj) 
                                            :   int     =
    
        getNameValues supportLargeObjectsInPool instance
        |> Seq.sumBy (fun nv -> ObjectConverter.toStorageSize nv)
        |>  (+) (((Literal.PKey.Length + Literal.RKey.Length) * 2)  + 4)

// -------------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------------

// Used to only reflect over the type once and cache the result
type internal 't Reflector private () = 
    static 
        let get = TypeReflector(typeof<'t>)

    static 
        member Get = 
            try
                get
            with
            |   :? TypeInitializationException as x     ->  raise x.InnerException
            |   _                                       ->  reraise()
                        





