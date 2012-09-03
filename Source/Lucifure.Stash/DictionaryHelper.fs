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
// Reflects over a type and builds tables to use for populating the type

module internal DictionaryHelper = 

    type DictionaryInterfaceType = IDictionary<string, obj>

    let setterAllNull = (fun o nameValues -> ())
    let getterAllNull = (fun o -> Seq.empty<NameValue>)              

    let hasDictionaryInterface (memberType : Type) = 
        memberType.GetInterfaces() 
            |> Seq.exists (fun i -> i.UnderlyingSystemType = typeof<DictionaryInterfaceType>)

    let getSetterGetter
        (memberInfo                             :   MemberInfo) = 

        let typeDictionary, setter, getter 
            =   match memberInfo.MemberType with
                |   MemberTypes.Field       
                    ->  
                        let fieldInfo = memberInfo :?> FieldInfo
                               
                        let x (a, b) = fieldInfo.SetValue (a, b)

                        fieldInfo.FieldType,   
                        (fun instance value -> fieldInfo.SetValue(instance, value)), 
                        fieldInfo.GetValue
                            
                |   MemberTypes.Property    
                    ->  
                        let propInfo = memberInfo :?> PropertyInfo

                        let setMethod   = propInfo.GetSetMethod(true);
                        let getMethod   = propInfo.GetGetMethod(true);
        
                        propInfo.PropertyType,
                        (fun instance value -> setMethod.Invoke(instance, [| value |]) |> ignore),
                        (fun instance -> getMethod.Invoke(instance, [||]))

                |   _
                    ->  Msg.Raise Msg.errUnsupportedMemberType // unexpected

        let getDictionary instance = (getter instance) :?> DictionaryInterfaceType                

        let getExistingOrNewDictionary instance = 
                
            match getDictionary instance with
            |   null    ->  let dict = Activator.CreateInstance(typeDictionary) :?> DictionaryInterfaceType
                            setter instance dict
                            dict
            |   dict    ->  dict
            

        let setMember 
                (instance               :   obj) 
                (nameValues             :   NameValues) = 

            let dict = getExistingOrNewDictionary instance

            nameValues 
            |> Seq.iter (fun nameValue ->   dict.[nameValue.Name] <- nameValue.Value)

        let getMember (instance : obj) = 

            match getDictionary instance with
            |   null        ->  Seq.empty<NameValue> 
            |   dict        ->  dict 
                                :> KeyValuePair<string, obj> seq 
                                |> Seq.map (fun item -> NameValue(
                                                            item.Key, 
                                                            item.Value))

        setMember, getMember

