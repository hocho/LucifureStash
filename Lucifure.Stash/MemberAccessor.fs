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

module internal MemberAccessor =

    // returns the Type, setter and getter
    let get (memberInfo : MemberInfo) = 

        match memberInfo.MemberType with
        |   MemberTypes.Field       
            ->  
                let fieldInfo = memberInfo :?> FieldInfo

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

        |   _ as x
            ->  Msg.Raise Msg.errUnsupportedMemberType
                
