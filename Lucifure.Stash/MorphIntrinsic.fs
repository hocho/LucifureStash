// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection
open System.Text
open System.IO
open System.Runtime.Serialization

// ---------------------------------------------------------------------------------------------------------------------
// Contains intrinsic morphs for converting byte,int16 and signed values to Int32 and Int64
// ---------------------------------------------------------------------------------------------------------------------

type internal MorphIntrinsicByte() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<byte>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int32 (value :?> byte)) :> obj 


        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (byte (value :?> int32)) :> obj 

type internal MorphIntrinsicSByte() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<sbyte>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int32 (value :?> sbyte)) :> obj 


        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (sbyte (value :?> int32)) :> obj 


type internal MorphIntrinsicInt16() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<int16>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int32 (value :?> int16)) :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (int16 (value :?> int32)) :> obj 


type internal MorphIntrinsicUInt16() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<uint16>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int32 (value :?> uint16)) :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (uint16 (value :?> int32)) :> obj 

type internal MorphIntrinsicUInt32() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<uint32>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int32 (value :?> uint32)) :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (uint32 (value :?> int32)) :> obj 

// used for enums 
type internal MorphIntrinsicInt32() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<int32>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            value :?> int32 :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            value :?> int32 :> obj 

type internal MorphIntrinsicUInt64() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<uint64>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int64 (value :?> uint64)) :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (uint64 (value :?> int64)) :> obj 

// used for enums 
type internal MorphIntrinsicInt64() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<int64>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            value :?> int64 :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            value :?> int64 :> obj 

type internal MorphIntrinsicChar() = 

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 

            t = typeof<char>

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            (int32 (value :?> char)) :> obj 

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            (char (value :?> int32)) :> obj 

//  wrapper to morph only non null values
type internal MorphIntrinsicNullChecked<'a>
        (inner                              :   IStashMorph) = 

    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool = 
            inner.CanMorph t            

        member this.IsCollationEquivalent 
        
            with get()  = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            if value = null 
                then    null
                else    inner.Into value

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            if value = null 
                then    null
                else    upcast ((inner.Outof value) :?> 'a) // casting for nullable enums only

type IMorphIntrinsicBuilder =
    
    abstract member GetNative                   :   Type -> IStashMorph

    abstract member GetForEnum                  :   Type -> IStashMorph


type internal MorphIntrinsicBuilder<'a>() = 

    let wrap inner = MorphIntrinsicNullChecked<'a> inner :> IStashMorph       

    interface IMorphIntrinsicBuilder with

        member this.GetNative ty : IStashMorph = 

            match ty with 
            |   x when  x = typeof<byte>    ->  wrap (MorphIntrinsicByte()  :> IStashMorph) 
            |   x when  x = typeof<sbyte>   ->  wrap (MorphIntrinsicSByte() :> IStashMorph)   

            |   x when  x = typeof<int16>   ->  wrap (MorphIntrinsicInt16() :> IStashMorph)   
            |   x when  x = typeof<uint16>  ->  wrap (MorphIntrinsicUInt16():> IStashMorph)   

            |   x when  x = typeof<uint32>  ->  wrap (MorphIntrinsicUInt32():> IStashMorph)
            |   x when  x = typeof<uint64>  ->  wrap (MorphIntrinsicUInt64():> IStashMorph)

            |   x when  x = typeof<char>    ->  wrap (MorphIntrinsicChar()  :> IStashMorph)
                                                                        
            |   _                           ->  MorphNull.Instance


        // for enums we need a converter for the table supported types too, so these are in addition
        member this.GetForEnum (ty : Type) : IStashMorph = 

            match ty with 
            |   x when  x = typeof<int32>   ->  wrap (MorphIntrinsicInt32():> IStashMorph)
            |   x when  x = typeof<int64>   ->  wrap (MorphIntrinsicInt64():> IStashMorph)
            |   _                           ->  (this :> IMorphIntrinsicBuilder).GetNative ty



module internal MorphIntrinsic = 
    
    let MorphIntrinsicBuilderObj = 
            MorphIntrinsicBuilder<obj>() :> IMorphIntrinsicBuilder

    let validate (morph : IStashMorph) (ty : Type) = 
        if not (morph.CanMorph ty) then
            Msg.RaiseCompiletime ([| Msg.errUnsupportedDataTypeForMorph (morph.GetType()) ty|])

    let get (ty : Type)= 

        let uType, morph = 
            
            if ty.IsEnum then   
                let uType = Enum.GetUnderlyingType ty
                
                uType, MorphIntrinsicBuilderObj.GetForEnum uType

            elif ReflectionHelper.isNullableType ty then    

                let uType = Nullable.GetUnderlyingType ty
                
                if uType.IsEnum then    // could be nullable enum

                    let enumType = uType
                    let uType = Enum.GetUnderlyingType enumType

                    // use reflections to create the type so that we can cast the enum
                    // note: Casting is only needed for Nullable Enums. 
                    // Regular enums and nullable data types do not need casting                               
                    let instance =  Activator.CreateInstance(
                                        typeof<MorphIntrinsicBuilder<_>>
                                            .GetGenericTypeDefinition()
                                            .MakeGenericType([| enumType |]))
                                    :?> IMorphIntrinsicBuilder

                    uType, instance.GetForEnum(uType)
                    
                else
                    uType, MorphIntrinsicBuilderObj.GetNative uType
                      
            // if obj (like list of object, we cannot morph)  
            elif ty = typeof<obj> || ObjectConverter.isPrimitiveCastable ty then  
                
                ty, MorphIntrinsicBuilderObj.GetNative ty
        
            else 
                ty, MorphDataContract ty  :> IStashMorph 

        validate morph uType
                            
        morph        
        
    // if type is nullable enum, return the enum.
    // if enum returns the primitive type
    let getUnderlyingType (ty : Type) = 

        if ty.IsEnum then 

            Enum.GetUnderlyingType ty

        elif ReflectionHelper.isNullableType ty then 

            Nullable.GetUnderlyingType ty

        else ty

    // primitive underlying type even in is nullable enum.
    let getUnderlyingPrimitiveType (ty : Type) = 

        let ty = getUnderlyingType ty

        if ty.IsEnum
            then    Enum.GetUnderlyingType ty
            else    ty

