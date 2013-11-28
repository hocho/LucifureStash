// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.IO
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common.Misc
open CodeSuperior.Common.StringExtensions

// Parses the response from Azure Table Storage
module internal XmlResponseParser =

    type BatchResult = {
            httpResponse        :   int;
            httpResponseText    :   string;
            etag                :   string;
            errorCode           :   string;
            errorMessage        :   string;            
        }

    let batchResultNull = { httpResponse = -1; httpResponseText = ""; etag = Constant.ETagNoMatch; errorCode = ""; errorMessage = "" }
    
    let private adoNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"    

    let private moveToNextNode 
            (reader                         :   XmlReader)
            (nodeType                       :   XmlNodeType) =
        
        let rec next() = 
            if reader.Read() && reader.NodeType <> nodeType then  
                next()
        next()        

    let private getProperties
            (reader                         :   XmlReader) = 

        let rec readAllProperties() = 
            seq {
                if reader.Read() then
                    
                    match reader.NodeType with
                    |   XmlNodeType.Element     ->

                            let edmType =   match reader.GetAttribute("type", adoNamespace) with
                                            |   null    -> ""
                                            |   _ as x  -> x
                            
                            // if a null attribute is found, it is from the storage emulator so ignore
                            let isNull =   match reader.GetAttribute("null", adoNamespace) with
                                            |   x when x = "true"   -> true
                                            |   _                   -> false

                            if not isNull then
                                yield NameValue(
                                                reader.LocalName, 
                                                ObjectConverter.toObj edmType (reader.ReadString()))      
                                                                

                            yield! readAllProperties()

                    |   XmlNodeType.EndElement when reader.LocalName = "properties" ->
                                ()
                    |   _  -> 
                            yield! readAllProperties()
            }

        readAllProperties()

    // -----------------------------------------------------------------------------------------------------------------
    // gets the etag from the xml response

    let getETagValue 
            (xml                            :   string) = 

        let reader = new XmlTextReader(
                            new StringReader(xml))        

        let readETag() = 
                if reader.MoveToAttribute(Constant.ETagAtbInXml, adoNamespace) 
                then    reader.ReadContentAsString()
                else    null

        let rec read() =
            if reader.Read() then
                if reader.NodeType = XmlNodeType.Element then
                    match reader.LocalName with
                    |   "entry"         ->  readETag()
                    |   _               ->  read()
                else
                    read() 
            else
                null

        read()

    let getEntities
        (xml                                :   string)
                                            :   NameValues seq = 

        let reader = new XmlTextReader(
                            new StringReader(xml))        

        let readETag() = 
                if reader.MoveToAttribute(Constant.ETagAtbInXml, adoNamespace) 
                then    reader.ReadContentAsString()
                else    null

        let rec read nameValues =
            seq { 
                if reader.Read() then
                    if reader.NodeType = XmlNodeType.Element then
                        match reader.LocalName with
                        |   "entry"         ->  match readETag() with
                                                |   null        ->  yield! read nameValues          // ignore null etags
                                                |   _  as etag  ->  yield! read (Seq.singleton 
                                                                                    (NameValue(
                                                                                        Literal.ETag,
                                                                                        etag)))
                        |   "properties"    ->  yield   Seq.append
                                                            nameValues
                                                            (getProperties reader) 
                                                yield! read (Seq.empty<NameValue>)  // start next set
                        |   _               ->  yield! read nameValues
                    else
                        yield! read nameValues                     
                }

        read (Seq.empty<NameValue>)
            |>  Seq.cache

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    // Splits the batch response into sub responses
    let splitBatch
            (text                               :   string) = 
                                        
        let read() = 

            let reader = new StringReader(text)

            let rec read' (body : string list) = 
                seq {            
                    match reader.ReadLine() with
                    |   line when line <> null ->

                        match line with 
                        |   line when line.StartsWith("--changesetresponse_") ->
 
                            // consolidate current bodies
                            yield List.foldBack (fun b s -> s + b + Environment.NewLine) body ""

                            if not (line.EndsWith("--"))
                                then    yield! read' []
                                                
                        |   _   -> 

                            yield! read' (line :: body)

                    |   _   -> 

                            ()
                }
            
            read' [] |> Seq.skip 1      // skip the first section which is not a result


        let getError text batchResult = 
            
            let reader = new XmlTextReader(
                                        new StringReader(text))

            let rec read (batchResult: BatchResult) =
                if reader.Read() then
                    if reader.NodeType = XmlNodeType.Element then
                        match reader.LocalName with
                        |   "code"      ->  
                                
                            { batchResult with errorCode = reader.ReadElementString() }
                                
                        |   "message"   ->      
                        
                            { batchResult with errorMessage = reader.ReadElementString() }

                        |   _           ->  
                            batchResult
                    else 
                        batchResult
                    |>  
                        read
                else
                    batchResult

            read batchResult


        let getETag (line : string) = line.Split([| ": " |], StringSplitOptions.RemoveEmptyEntries).[1]
            
        let getHttpResponse (line : string) = 
                
            let parts = line.Split([|" "|], 3, StringSplitOptions.RemoveEmptyEntries)
                
            let error = -1, ""
                
            if parts.Length = 3 then
                match Int32.TryParse(parts.[1]) with 
                |   true, result    -> result, parts.[2] 
                |   _               -> error
            else
                error

        let extractBatchResult text = 
             
            let reader = new StringReader(text)

            let rec read (batchResult : BatchResult) = 
                match reader.ReadLine() with
                |   line when line <> null ->
                            
                    match line with
                    |   httpResponse    when    line.StartsWith("HTTP/1.1") ->  
                        
                        let num, text = getHttpResponse line
                        read ({ batchResult with httpResponse = num; httpResponseText = text })

                    |   etag            when    line.StartsWith("ETag:")    ->  

                        read ({ batchResult with etag = getETag etag })

                    |   xml             when    line.StartsWith("<?xml")   ->
    
                        match batchResult.httpResponse with 
                        |   201     ->  // results  
                                        batchResult
                        |   204     ->  // no content
                                        batchResult
                        |   _       ->  // error
                                        getError (reader.ReadToEnd()) batchResult
                        |> read 
                                   
                    
                    |   _   ->  

                        read batchResult    

                |   _ -> batchResult

            read batchResultNull

        read ()
        |>  Seq.map extractBatchResult 


