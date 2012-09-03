// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.Web
open System.Collections
open System.Collections.Generic
open System.IO
open System.Text
open System.Net
open System.Reflection

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional
open CodeSuperior.Common.Misc

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

module Literal = 
    [<Literal>]
    let PKey            = "PartitionKey"

    [<Literal>]
    let RKey            = "RowKey"

    [<Literal>]
    let Timestamp       = "Timestamp"

    [<Literal>]
    let ETag            = "*ETag"           // note prefixed with '*' to avoid collision with other property names

module internal Constant =
    [<Literal>]
    let ETagNoMatch     = "*"
    let ETagNo          = ""
    let ETagAtbInXml    = "etag"
    let ETagInHeader    = "ETAG"

// ---------------------------------------------------------------------------------------------------------------------
// Reflects over a type and builds tables to use for populating the type

module internal ReflectionHelper = 

    let hasInterface 
            (interfaceTy                        :   Type)
            (ty                                 :   Type) = 

        ty.GetInterfaces() 
            |> Seq.exists (fun i -> i.UnderlyingSystemType = interfaceTy)
        
    let hasGenericInterface 
            (interfaceTy                        :   Type)
            (ty                                 :   Type) = 

        let isGenericIList (i : Type) =  (i.IsGenericType && (i.GetGenericTypeDefinition() = interfaceTy)) = true

        ty.GetInterfaces() 
            |> Seq.exists isGenericIList

    let hasDictionaryInterface : Type -> bool = 
        hasInterface typeof<IDictionary<string,obj>>

    let hasMorphInterface : Type -> bool = 
        hasInterface typeof<IStashMorph>

    let hasListInterface : (Type -> bool) = hasInterface typeof<IList> 
    
    let hasGenericIListInterface : (Type -> bool) = hasGenericInterface typedefof<_ IList>

    let isNullableType (ty : Type) = 
        
        ty.IsGenericType && (ty.GetGenericTypeDefinition() = typedefof<_ Nullable>) = true

    let getMemberType 
        (memberInfo                         : MemberInfo) = 
            match memberInfo.MemberType with
            |   MemberTypes.Field       -> (memberInfo :?> FieldInfo).FieldType
            |   MemberTypes.Property    -> (memberInfo :?> PropertyInfo).PropertyType
            |   _                       -> Msg.Raise Msg.errUnsupportedMemberType

    let getTableInfo 
            (ty                             :   Type) =   
            
        ty.GetCustomAttributes(false)   // StashEntity cannot be inherited
                    |>  Seq.tryFind (fun atb -> atb.GetType() = typeof<StashEntityAttribute>)
                    |>  function 
                        |   Some atb    ->  let tableAtb = atb :?> StashEntityAttribute
                                    
                                            (if tableAtb.Name.Is 
                                                then tableAtb.Name
                                                else ty.Name), tableAtb.Mode
                        |   None        ->  ty.Name, StashMode.Implicit 

    let isTypeOrSubTypeOf
            (tyIs                             :   Type) 
            (tyOf                             :   Type) =

        let rec isTypeOrSubTypeOf' tyIs = 
            match tyIs with
            |   _ when tyIs = tyOf
                    ->  true
            |   _
                    ->  if tyIs = typeof<obj> 
                            then    false
                            else    isTypeOrSubTypeOf' 
                                        (if tyIs.IsGenericType 
                                            then    tyIs.GetGenericTypeDefinition()
                                            else    tyIs).BaseType
        isTypeOrSubTypeOf' tyIs