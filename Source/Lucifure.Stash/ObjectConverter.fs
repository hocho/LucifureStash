// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.Web
open System.IO
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Misc

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

// No need to escape anything since the XmlTextWriter will do the needful.
module internal ObjectConverter = 
    
    let stringToDateTime value = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind)

    let dateTimeToString value = XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind)        

    let stringToDouble value = XmlConvert.ToDouble(value)

    let doubleToString (value : float) = XmlConvert.ToString(value)        


    let isPrimitive = 
            function
            |   x when  x = typeof<String>
                ||      x = typeof<Int32>
                ||      x = typeof<Int64>
                ||      x = typeof<Double>
                ||      x = typeof<Guid>
                ||      x = typeof<DateTime>
                ||      x = typeof<Boolean>
                ||      x = typeof<Byte[]>          
                                                    ->  true
            | _                                     ->  false

    let isPrimitiveNullable = 
            function
            |   x when  x = typeof<Nullable<Int32>>
                ||      x = typeof<Nullable<Int64>>
                ||      x = typeof<Nullable<Double>>
                ||      x = typeof<Nullable<Guid>>
                ||      x = typeof<Nullable<DateTime>>
                ||      x = typeof<Nullable<Boolean>>
                                                        ->  true
            | _  as     x                               ->  isPrimitive x
        
    let isPrimitiveCastable =                 
            function 
            |   x   when    x = typeof<byte>
                ||          x = typeof<Nullable<byte>>
                ||          x = typeof<sbyte>
                ||          x = typeof<Nullable<sbyte>>
                ||          x = typeof<char>
                ||          x = typeof<Nullable<char>>
                ||          x = typeof<Int16>
                ||          x = typeof<Nullable<Int16>>
                ||          x = typeof<UInt16>         
                ||          x = typeof<Nullable<UInt16>>         
                ||          x = typeof<UInt32>         
                ||          x = typeof<Nullable<UInt32>>         
                ||          x = typeof<UInt64>      
                ||          x = typeof<Nullable<UInt64>>      
                                                    ->  true
            |   _  as x                             ->  if x.IsEnum 
                                                            then true
                                                            else isPrimitiveNullable x
             

    // converts from the atom string representation to an object
    let toObj edmType value = 

        match edmType with
        |   x when x = ""                   ->  value                               :> obj
        |   x when x = "Edm.String"         ->  value                               :> obj
        |   x when x = "Edm.Int32"          ->  Int32.Parse value                   :> obj
        |   x when x = "Edm.Int64"          ->  Int64.Parse value                   :> obj    
        |   x when x = "Edm.Double"         ->  stringToDouble value                :> obj
        |   x when x = "Edm.Guid"           ->  Guid.Parse value                    :> obj
        |   x when x = "Edm.DateTime"       ->  stringToDateTime value              :> obj
        |   x when x = "Edm.Boolean"        ->  Boolean.Parse value                 :> obj
        |   x when x = "Edm.Binary"         ->  Convert.FromBase64String value      :> obj
        |   _  as x                
                ->  Msg.Raise (Msg.errUnsupportedDataTypeOnRead x)

    // convert from object to atom string representation
    let toStr (o : obj) = 
        match o with
        |   :?  String      as x    ->  ""              , x
        |   :?  Int32       as x    ->  "Edm.Int32"     , x.ToString()
        |   :?  Int64       as x    ->  "Edm.Int64"     , x.ToString()
        |   :?  Double      as x    ->  "Edm.Double"    , doubleToString x
        |   :?  Guid        as x    ->  "Edm.Guid"      , x.ToString()
        |   :?  DateTime    as x    ->  "Edm.DateTime"  , dateTimeToString x
        |   :?  Boolean     as x    ->  "Edm.Boolean"   , BoolToString x
        |   :?  (Byte[])    as x    ->  "Edm.Binary"    , Convert.ToBase64String x
        |   _                  
                ->  Msg.Raise (Msg.errUnsupportedDataTypeOnWrite (o.GetType()))

    // returns the size (number of bytes) of an object
    let toStorageSize (nameValue : NameValue) = 

        // overhead + Size of Property 
        8 + ((nameValue.Name.Length) * 2) + 
        
        match nameValue.Value with
        |   :?  String      as x    ->  (x.Length * 2) + 4  // + 4 for string length
        |   :?  Int32       as x    ->  4
        |   :?  Int64       as x    ->  8
        |   :?  Double      as x    ->  8
        |   :?  Guid        as x    ->  16
        |   :?  DateTime    as x    ->  8
        |   :?  Boolean     as x    ->  1
        |   :?  (Byte[])    as x    ->  x.Length + 4        // + 4 for array length
        |   _                  
                ->  Msg.Raise
                        (StashMessage(
                            StashError.UnexpectedRuntime,
                            sprintf 
                                "The toSize method received an unexpected data type %s" 
                                (nameValue.Value.GetType().ToString())))


    // convert from object to atom string representation
    let asString (o : obj) = match (toStr o) with _,str -> str

    let fromInt32 (value :  int32) (memberTy : Type) : obj =
        match memberTy with
        |   x when x = typeof<byte>     ->  upcast byte value 
        |   x when x = typeof<sbyte>    ->  upcast sbyte value
        |   x when x = typeof<char>     ->  upcast char value
        |   x when x = typeof<int16>    ->  upcast int16 value
        |   x when x = typeof<uint16>   ->  upcast uint16 value
        |   x when x = typeof<uint32>   ->  upcast uint32 value
        |   _                           ->  upcast value

    let fromInt64 (value :  int64) (memberTy : Type) : obj =
        match memberTy with
        |   x when x = typeof<uint64>   ->  upcast int value

        |   x when x = typeof<byte>     ->  upcast byte value 
        |   x when x = typeof<sbyte>    ->  upcast sbyte value
        |   x when x = typeof<char>     ->  upcast char value
        |   x when x = typeof<int16>    ->  upcast int16 value
        |   x when x = typeof<uint16>   ->  upcast uint16 value
        |   x when x = typeof<uint32>   ->  upcast uint32 value
        |   _                           ->  upcast value

    let inline toMemberType (o : obj) (memberTy : Type) : obj = 

        if o.GetType() = memberTy then o      
        else
            match o with
            |   :?  int32 as x      ->  fromInt32 x memberTy
            |   :?  int64 as x      ->  fromInt64 x memberTy
            |   _                   ->  o

    // converts an object into a form suitable for persisting to the storage table
    let toTableType (o : obj) : (bool * obj)= 
        match o with
        |   :?  byte    as x            ->  true, upcast (int32 x)
        |   :?  sbyte   as x            ->  true, upcast (int32 x)
        |   :?  char    as x            ->  true, upcast (int32 x)
        |   :?  int16   as x            ->  true, upcast (int32 x)
        |   :?  uint16  as x            ->  true, upcast (int32 x)
        |   :?  int32   as x            ->  true, upcast x              // for enum matching
        |   :?  uint32  as x            ->  true, upcast (int32 x)
        |   :?  int64   as x            ->  true, upcast x              // for enum matching
        |   :?  uint64  as x            ->  true, upcast (int64 x)
        |   _                           ->  false, o
