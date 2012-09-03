// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Text
open System.IO
open System.Runtime.Serialization

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal MorphDataContract
        (ty                                 :   Type) = 

    static let encodingByteToChars = Encoding.GetEncoding(65001)

    let serializer = DataContractSerializer(ty)        

    // morphs into a string
    interface IStashMorph with

        member this.CanMorph
                (t                          :   Type)
                                            :   bool    = true

        member this.IsCollationEquivalent 
                                with get()              = true

        member this.Into
                (value                      :   obj)
                                            :   obj = 

            use stream = new MemoryStream()

            serializer.WriteObject(stream, value)
            
            stream.Seek(0L, SeekOrigin.Begin) |> ignore
                
            let reader = new BinaryReader(stream)
            
            upcast new String(
                        encodingByteToChars.GetChars 
                                        (reader.ReadBytes(int32 stream.Length)))

        member this.Outof
                (value                      :   obj)
                                            :   obj = 

            if value.GetType() <> typeof<string> then
                Msg.Raise (Msg.errIncorrectDataContractMorpherType (value.GetType()))

            use writer = new BinaryWriter(new MemoryStream())
            
            let bytes = Encoding.UTF8.GetBytes(value :?> string)

            writer.Write(
                    bytes, 
                    0,
                    bytes.Length)
                    
            writer.Flush()

            writer.Seek(0, SeekOrigin.Begin) |> ignore
 
            serializer.ReadObject(writer.BaseStream)

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

            