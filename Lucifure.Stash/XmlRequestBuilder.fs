// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.IO
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Misc

// -------------------------------------------------------------------------------------------------------

type internal BatchInfo
        (commandType                        :   CommandType
        ,target                             :   string
        ,etag                               :   string)  =

    member this.CommandType with get() = commandType

    member this.Target with get() = target

    member this.ETag with get() = etag

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

module internal XmlRequestBuilder =

    let nsDefault = "http://www.w3.org/2005/Atom"
    let nsD = "http://schemas.microsoft.com/ado/2007/08/dataservices"
    let nsM = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"

    // -----------------------------------------------------------------------------------------------------------------

    let xmlWriterSettings = 
        let settings = new XmlWriterSettings()
        
        settings.Reset()    
        settings.Indent      <- true
        settings.IndentChars <- "\t"
        
        settings
       
    let flushBuilderXml 
            (contentBuilder                     :   XmlWriter -> unit) = 

        (fun (w : XmlWriter) ->
                        w.Flush()
                        contentBuilder w
                        w.Flush() )
    
    let flushBuilderStream
            (contentBuilder                     :   StreamWriter -> unit) = 

        (fun (w : StreamWriter) ->
                        w.Flush()
                        contentBuilder w
                        w.Flush() )

    let private buildUsingXmlWriter
        (contentBuilder                     :   XmlWriter -> unit) = 

        use stream = new MemoryStream()
        let writer = XmlTextWriter.Create(stream, xmlWriterSettings)
        
        contentBuilder writer        

        stream.Seek(0L, SeekOrigin.Begin) |> ignore
            
        (new StreamReader(stream)).ReadToEnd()

    let private buildUsingStreamWriter
        (contentBuilder                     :   StreamWriter -> unit) = 

        use stream = new MemoryStream()
        let writer = new StreamWriter(stream)
       
        contentBuilder writer        

        stream.Seek(0L, SeekOrigin.Begin) |> ignore
            
        (new StreamReader(stream)).ReadToEnd()

    // -----------------------------------------------------------------------------------------------------------------

    let private buildContentEntityBody 
            (properties                     :   NameValues)
            (writer                         :   XmlWriter) = 

        let addProperties (nameValue:NameValue) =

            writer.WriteStartElement("d",  nameValue.Name, null) 

            let edmString,
                valueString = ObjectConverter.toStr nameValue.Value

            // is not a string
            if edmString.Is then
                writer.WriteAttributeString("m", "type", null, edmString)
            else
                // only do these for string types
                if valueString.HasLeadingOrTrailingSpaces then
                    writer.WriteAttributeString("xmlns", "space", null, "preserve")

            writer.WriteString(valueString)

            writer.WriteEndElement()

        writer.WriteStartElement("m", "properties", null)

        properties 
        |> Seq.iter addProperties

        writer.WriteEndElement()


    let private buildEnvelope
            (contentBuilder                 :   XmlWriter -> unit) 
            (writer                         :   XmlWriter)          = 

        writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"")
        writer.WriteStartElement("entry", nsDefault)
        writer.WriteAttributeString("xmlns", "d", null, nsD)
        writer.WriteAttributeString("xmlns", "m", null, nsM)
        
        if true then    // for code nesting 
            writer.WriteElementString("title", "")

            writer.WriteElementString("updated", DateTime.UtcNow.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss.fffffff'Z'"))

            writer.WriteStartElement ("author")
            writer.WriteElementString("name", "")
            writer.WriteEndElement();

            writer.WriteElementString("id", "")

            if true then  // for code nesting   
                writer.WriteStartElement("content")
                writer.WriteAttributeString("type", "application/xml")
            
                contentBuilder writer

                writer.WriteEndElement()   // content
            
        writer.WriteEndElement()   // entry


    // -----------------------------------------------------------------------------------------------------------------

    let buildEntityBody 
            (nameValues                     :   NameValues) =

        nameValues
        |> (buildContentEntityBody >> flushBuilderXml >> buildEnvelope >> flushBuilderXml >> buildUsingXmlWriter)        

    // -----------------------------------------------------------------------------------------------------------------
    
    let private buildBatchEnvelope
            (batchId                        :   Guid)
            (changeSetId                    :   Guid)
            (contentBuilder                 :   StreamWriter -> unit) 
            (writer                         :   StreamWriter)          = 

        let batchBegin = sprintf "--batch_%s" (batchId.ToString())

        writer.WriteLine(batchBegin)
        writer.WriteLine("Content-Type: multipart/mixed; boundary=changeset_{0}", changeSetId.ToString())
    
        contentBuilder writer

        writer.WriteLine()
        writer.Write(sprintf "--changeset_%s" (changeSetId.ToString()));writer.WriteLine("--")
        writer.Write(batchBegin);writer.WriteLine("--")
    
    let private buildChangeSetEnvelope
            (idx                            :   int)
            (bi                             :   BatchInfo)
            (guid                           :   Guid) 
            (contentBuilder                 :   StreamWriter -> unit)
            (writer                         :   StreamWriter)          = 

        writer.WriteLine()
        writer.WriteLine(sprintf
                            "--changeset_%s"
                            (guid.ToString()))
        writer.WriteLine("Content-Type: application/http")
        writer.WriteLine("Content-Transfer-Encoding: binary")
        writer.WriteLine()
        writer.Write(
                sprintf 
                    "%s %s" 
                    (CommandType'.asVerb bi.CommandType) 
                    bi.Target)
        writer.WriteLine(" HTTP/1.1")
        writer.WriteLine("Content-ID: {0}", (idx + 1))

        if CommandType'.requiresETag bi.CommandType then 
            writer.WriteLine("If-Match: {0}", bi.ETag)

        if CommandType'.hasContent bi.CommandType then
            let content = buildUsingStreamWriter contentBuilder 

            writer.WriteLine("Content-Type: application/atom+xml;type=entry")
            writer.WriteLine("Content-Length: {0}", content.Length)
            writer.WriteLine()
            writer.Write(content)


    let buildEntityBodyBatched 
        (batchId                            :   Guid)
        (opProperties                       :   (BatchInfo * NameValues) seq) = 
        
        let asOpNameValues =
            opProperties 
            |>  Seq.map (fun (bi, nvs) -> (bi, nvs))

        let changeSetId = Guid.NewGuid()

        let changeSetEnvelope idx (bi : BatchInfo) (nvs : NameValues) = 

            let buildEnvelopeDetails (w : StreamWriter) =
                XmlTextWriter.Create(w.BaseStream, xmlWriterSettings)
                |> 
                (nvs |> (buildContentEntityBody >> flushBuilderXml >> buildEnvelope 
                                                >> flushBuilderXml))
            buildChangeSetEnvelope
                idx
                bi
                changeSetId
                (flushBuilderStream buildEnvelopeDetails)

        buildBatchEnvelope 
            batchId
            changeSetId
            (fun writer ->  asOpNameValues
                            |>  Seq.iteri   (fun idx (bi, nvs) -> 
                                                changeSetEnvelope idx bi nvs writer
                                            )    
            )
        |> (flushBuilderStream >> buildUsingStreamWriter)                      

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    let private buildTableContent
        (tableName                          :   string) 
        (writer                             :   XmlWriter) = 

        writer.WriteStartElement("m", "properties", null)

        writer.WriteElementString("TableName", nsD, tableName)

        writer.WriteEndElement()


    let buildCreateTable 
        (tableName                          :   string) = 

        tableName |> (buildTableContent >> flushBuilderXml >> buildEnvelope 
                                        >> flushBuilderXml >> buildUsingXmlWriter)


    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
