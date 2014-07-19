// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Reflection
open System.Collections.Generic

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Misc

// ---------------------------------------------------------------------------------------------------------------------

type internal IConverter =
    
    abstract member Edm                     :   string 
        
    abstract member DataType                :   Type

    abstract member ToObject                :   string -> obj

    abstract member ToStr                   :   obj -> string     

// ---------------------------------------------------------------------------------------------------------------------

type internal Converter<'a>
            (edm                            :   string
            ,toObject                       :   string -> obj
            ,toString                       :   obj -> string) = 

    let edm =   if edm.Is 
                    then    "Edm." + edm 
                    else    ""

    interface IConverter with 

        member this.Edm  
            with get() = edm
    
        member this.DataType 
            with get() = typeof<'a>    
        
        member  
            this.ToObject
                (value                      :   string) =
           
            toObject value

        member
            this.ToStr
                (o                          :   obj) = 
            
            toString o

// ---------------------------------------------------------------------------------------------------------------------

module internal Converters = 

    let converterInt32 = 
        new Converter<Int32>
                        ("Int32"
                        ,(fun (s : string) -> upcast Int32.Parse s)
                        ,(fun (o : obj) -> o.ToString()))
    
    let converterInt64 = 
        new Converter<Int64>
                        ("Int64"
                        ,(fun (s : string) -> upcast Int64.Parse s)
                        ,(fun (o : obj) -> o.ToString()))

    let converterDouble = 
        new Converter<Double>
                        ("Double"
                        ,(fun (s : string) -> upcast ObjectConverter.stringToDouble s)
                        ,(fun (o : obj) -> ObjectConverter.doubleToString (o :?> double)))

    let converterGuid = 
        new Converter<Guid>
                        ("Guid"
                        ,(fun (s : string) -> upcast Guid.Parse s)
                        ,(fun (o : obj) -> o.ToString()))

    let converterBoolean = 
        new Converter<Boolean>
                        ("Boolean"
                        ,(fun (s : string) -> upcast Boolean.Parse s)
                        ,(fun (o : obj) -> BoolToString (o :?> Boolean)))

    let converterDateTime = 
        new Converter<DateTime>
                        ("DateTime"
                        ,(fun (s : string) -> upcast ObjectConverter.stringToDateTime s)
                        ,(fun (o : obj) -> ObjectConverter.dateTimeToString (o :?> DateTime)))

    let converterString = 
        new Converter<String>
                        (""
                        ,(fun (s : string) -> upcast s)
                        ,(fun (o : obj) -> o.ToString()))

    let converterBinary = 
        new Converter<Byte[]>
                        ("Binary"
                        ,(fun (s : string) -> upcast Convert.FromBase64String s)
                        ,(fun (o : obj) -> Convert.ToBase64String (o :?> Byte[])))
        
    let mapByType = Dictionary<Type, IConverter>()
    let mapByEdm  = Dictionary<string, IConverter>()

    let mapAdd (x : IConverter) =  
            mapByType.Add(x.DataType, x)
            mapByEdm.Add(x.Edm, x)

    let setup = 
        [|  (converterInt32    :> IConverter) 
            (converterInt64    :> IConverter)
            (converterString   :> IConverter)
            (converterDouble   :> IConverter)
            (converterGuid     :> IConverter)
            (converterDateTime :> IConverter)
            (converterBoolean  :> IConverter)
            (converterBinary   :> IConverter)
        |] 
        |> Array.iter mapAdd
    
    let getConverter (o : obj) : IConverter = 
        match mapByType.TryGetValue (o.GetType()) with
        |   true, c     ->  c
        |   _           ->  Msg.Raise (Msg.errUnsupportedConverter (o.GetType()))

