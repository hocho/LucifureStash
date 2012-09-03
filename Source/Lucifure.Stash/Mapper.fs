// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.Web
open System.Collections.Generic
open System.IO
open System.Text
open System.Net
open System.Reflection

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal MemberMappingType = 
        |   Data            of IStash
        |   Key             of IStash

// ---------------------------------------------------------------------------------------------------------------------
// Lookup a table property or member to its mapping

type internal ByMap = Dictionary<
                            string,
                            MemberMappingType>

// ---------------------------------------------------------------------------------------------------------------------
// A lookup table of IStash interfaces, can be looked up by type member name or table property name
// ---------------------------------------------------------------------------------------------------------------------

type internal MemberPropertyMapper() = 

    // to lookup property/member mapping by property name
    let mapByProperty   = ByMap() 

    // to lookup property/member mapping by member name
    let mapByMember     = ByMap()

    let mapByPropertyMode = new Dictionary<string, StashMode>() // table property name - member mode

    // adds the key/value to a map, returning false if key already exists
    let add 
            (byMap                          :   ByMap) 
            (key                            :   string)
            (mappingType                    :   MemberMappingType)   = 
        
        match not (byMap.ContainsKey(key)) with
        |   true    ->  byMap.[key] <- mappingType
                        true
        |   _       ->  false

    let getIStash memberMappingType = 

        match memberMappingType with
        |   Data istash     -> istash
        |   Key istash      -> istash

    // primary key and row key must be string representations
    let getKeyValueGetter key = 
        match mapByProperty.TryGetValue key with
        |   true, memberMappingType 
                ->  (fun inst ->   
                    
                        let istash = getIStash memberMappingType 
                                            
                        let nameValues = (istash.GetMember inst) |> Seq.toList

                        match nameValues.Length with
                        |   0   ->  ""
                        |   1   ->  // get the to string representation - 
                                    let keyVal = ObjectConverter.asString  nameValues.Head.Value

                                    if keyVal.Length > 1024 then // 1KB character count limit
                                        Msg.Raise (Msg.errRowKeyTooLarge key)

                                    keyVal             

                        |   _   ->  Msg.Raise (Msg.errRowKeyTooLarge key))

        |   _               ->  Msg.Raise (Msg.errKeyNotDefined key)    
                                // should not be raised since checked at compile time 


    // returns true only if property is explicitly defined. This way we can only determine to split/merge only 
    // explicitly properties such that on dictionary pooling they are not split/merged.
    let isExplicit propertyName = 
        match mapByPropertyMode.TryGetValue propertyName with
        |   true, StashMode.Implicit    ->  false
        |   true, StashMode.Explicit    ->  true    
        |   _                           ->  false    // not defined
    
    // if property is explicit or the prefix it explicitly defined, we return the property name or the prefix.
    // this way grouping can be done for split/merge and collections.
    // if implicit or not defined, we return the property name as it, so we do not do any extra processing.
    let getPropertyGroupName propertyName = 
        match mapByPropertyMode.TryGetValue propertyName with
        |   true, StashMode.Explicit    ->  propertyName
        |   _                           ->  let prefix = ObjectComposer.getPrefixSuffix propertyName |> fst
                                            match mapByPropertyMode.TryGetValue prefix with
                                            |   true, StashMode.Explicit    ->  prefix
                                            |   _                           ->  propertyName

    member this.IsExplicit
            (propertyName                   :    string) = 

        isExplicit propertyName

    member this.GetPropertyGroupName 
            (propertyName                   :   string) = 
            
        getPropertyGroupName propertyName
        
    // adds the property member mapping to both lookup dictionaries
    member this.Add 
            (memberMappingType              :   MemberMappingType)
            (mode                           :   StashMode) =

        let istash = getIStash memberMappingType

        // no need to validate here since validated below
        mapByPropertyMode.[istash.MemberName] <- mode        

        seq {
            match   (add 
                        mapByProperty
                        istash.TablePropertyName
                        memberMappingType) with
            |   true    -> ()
            |   _       -> yield (Msg.errDuplicateTablePropertyName istash.TablePropertyName)
        
            match (add 
                        mapByMember
                        istash.MemberName
                        memberMappingType) with
            |   true    -> ()
            |   false   -> yield (Msg.errDuplicateTypeMemberName istash.MemberName)
        }     

    member this.GetMode 
        (propertyName                       :   string) = 

        match mapByPropertyMode.TryGetValue propertyName with
        |   true, mode  ->  Some mode
        |   _           ->  None

    // invokes the setter of a member, that is set the member of the object, using the member name 
    member this.SetterInvoke 
            (inst                           :   obj)            // object instance 
            (nameValues                     :   NameValues) =

        let name = getPropertyGroupName (Seq.head nameValues).Name

        match mapByProperty.TryGetValue name with
        |   true, memberMappingType
                ->  let istash = getIStash memberMappingType
                    istash.SetMember (inst, nameValues)
                    true
        |   _               
                ->  false

    member this.GettersInvoke
            (instance                       :   obj) 
                                            :   NameValues = 
        mapByMember.Values
        |>  Seq.map (fun mmt        -> getIStash mmt)
        |>  Seq.map (fun istash     -> istash.GetMember instance)
        |>  Seq.concat
        
    member this.MemberNameToPropertyName
            (name                          :    string) = 

        match mapByMember.TryGetValue name with
        |   true, memberMappingType
               ->  (getIStash memberMappingType).TablePropertyName
        |   _           
                ->  ""

    member this.MemberNameToMemberMappingType
            (name                          :    string) = 

        match mapByMember.TryGetValue name with
        |   true, memberMappingType
                ->  Some memberMappingType
        |   _               
                ->  None

    member this.MemberNameToStash
            (name                          :    string) = 

        match this.MemberNameToMemberMappingType name with
        |   Some memberMappingType  
                ->  Some (getIStash memberMappingType)
        |   None
                ->  None

    member this.GetPartitionKeyGetter() = 

        getKeyValueGetter Literal.PKey

    member this.GetRowKeyGetter() = 

        getKeyValueGetter Literal.RKey

    member this.HasETag() = 
        mapByProperty.TryGetValue Literal.ETag |> fst

    member this.GetETagGetter() = 

        match mapByProperty.TryGetValue Literal.ETag with
        |   true, memberMappingType
                ->  fun inst -> string (((getIStash memberMappingType).GetMember inst |> Seq.head).Value)
        |   _               
                ->  fun inst -> Constant.ETagNoMatch 

    member this.GetETagSetter() = 

        match mapByProperty.TryGetValue Literal.ETag with
        |   true, memberMappingType
                ->  fun inst value -> (getIStash memberMappingType).SetMember (inst, Seq.singleton (NameValue("", value)))
        |   _               
                ->  fun inst value -> ()

    member this.HasPartitionKey() =
        let exists, _ = mapByProperty.TryGetValue Literal.PKey
        exists   

    member this.HasRowKey() =
        let exists, _ = mapByProperty.TryGetValue Literal.RKey
        exists   

// ---------------------------------------------------------------------------------------------------------------------
