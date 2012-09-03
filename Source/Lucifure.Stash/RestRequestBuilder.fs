// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.IO
open System.Text
open System.Globalization
open System.Net
open System.Security.Cryptography

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional
open CodeSuperior.Common.Misc

// ---------------------------------------------------------------------------------------------------------------------

[<Flags>]
type EntityState =
    |   Unchanged               =   1   
    |   Inserted                =   2
    |   Updated                 =   4
    |   Merged                  =   8
    |   InsertedOrUpdated       =   16
    |   InsertedOrMerged        =   32
    |   Deleted                 =   64

// keep in sync with EntityStatus so that it is cast-able
[<Flags>]
type internal CommandType =
    |   Unchanged               =   1
    |   Insert                  =   2
    |   Update                  =   4
    |   Merge                   =   8
    |   InsertOrUpdate          =   16
    |   InsertOrMerge           =   32
    |   Delete                  =   64
    |   Get                     =   128
    |   Batch                   =   256


module internal CommandType' = 
    
    let asVerb =
        function
        |   CommandType.Get             ->  "GET"
        |   CommandType.Batch
        |   CommandType.Insert          ->  "POST"
        |   CommandType.InsertOrUpdate
        |   CommandType.Update          ->  "PUT"
        |   CommandType.InsertOrMerge
        |   CommandType.Merge           ->  "MERGE"
        |   CommandType.Delete          ->  "DELETE"                  
        |   _                           ->  ""

    let requiresETag = 
        function 
        |   CommandType.Update
        |   CommandType.Merge
        |   CommandType.Delete          ->  true
        |   _                           ->  false

    let hasContent =
        function
        |   CommandType.Insert
        |   CommandType.InsertOrUpdate
        |   CommandType.Update
        |   CommandType.InsertOrMerge
        |   CommandType.Merge           ->  true
        |   _                           ->  false

// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// Three way switch to indicate an unchanged state or a Boolean value 
/// </summary>
type TernarySwitch =
    |   Unchanged
    |   True
    |   False

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------
// Creates the request to query table storage
type internal RestRequestBuilder
        (account                            :   string
        ,useHttps                           :   bool
        ,timeout                            :   TimeSpan
        ,expect100Continue                  :   TernarySwitch
        ,useNagleAlgorithm                  :   TernarySwitch
        ,requestSigner                      :   HttpWebRequest -> unit) =

    let format  =
        if  account = "devstoreaccount1" 
            then    "http://127.0.0.1:10002/{0}/{1}"                // dev storage cannot be https
            else    sprintf 
                        "http%s://{0}.table.core.windows.net/{1}"
                        (if useHttps then "s" else "")

        
    let Hash 
            (data                           :   string) = 
            
        MD5
            .Create()
            .ComputeHash (Encoding.UTF8.GetBytes data)
            |> bytesToHex

    let AddContent 
            (batchId                        :   Guid)
            (content                        :   string) 
            (request                        :   WebRequest) =
                
        request.ContentLength   <- int64 (content.Length)   
        
        request.Headers.Add("Content-MD5", Hash content)
        request.Headers.Add("x-ms-version", "2011-08-18")
        request.Headers.Add("DataServiceVersion", "1.0;NetFx")
        request.Headers.Add("MaxDataServiceVersion", "2.0;NetFx")
                                
        request.ContentType <-  if batchId = Guid.Empty 
                                    then    "application/atom+xml"
                                    else    sprintf "multipart/mixed; boundary=batch_%s" (batchId.ToString())

        request.Proxy <-  WebRequest.DefaultWebProxy
        //request.Proxy <-  null

        let stream = request.GetRequestStream()
        
        use writer = new StreamWriter(stream)
       
        writer.Write content

    member this.FormatRequest
            (command                        :   string) = 

        String.Format
                    (format
                    ,account 
                    ,command)

    member this.CreateRequest 
            (command                        :   string) 
            (commandType                    :   CommandType) 
            (content                        :   string) 
            (etag                           :   string)
            (batchId                        :   Guid) = 

        let request =   this.FormatRequest command
                        |> WebRequest.Create 
                        :?> HttpWebRequest
    
        request.Timeout                         <- (int) timeout.TotalMilliseconds    
        request.Method                          <- CommandType'.asVerb commandType

        if expect100Continue <> TernarySwitch.Unchanged then
            request.ServicePoint.Expect100Continue  <-  (expect100Continue = TernarySwitch.True)

        if useNagleAlgorithm <> TernarySwitch.Unchanged then
            request.ServicePoint.UseNagleAlgorithm  <-  (useNagleAlgorithm = TernarySwitch.True)
                    
        match commandType with
        | CommandType.Update
        | CommandType.Delete
        | CommandType.Merge         ->  request.Headers.Add("If-Match", etag)
        | _                         ->  // do not specify the etag in other cases
                                        //  especially InsertOrUpdate, InsertOrMerge
                                        ()  

        requestSigner request    

        if content <> null then
            AddContent batchId content request 
        else
            request.ContentLength   <- 0L

        request

    static member GetResponse
            (request                        :   HttpWebRequest) = 
        
            let response = request.GetResponse()
        
            use sr = new StreamReader(response.GetResponseStream())
            let result = sr.ReadToEnd()

            response :?> HttpWebResponse, result
          
    static member BuildEntityContinuation
            (response                       :   HttpWebResponse) = 

        let nextPartitionKey = response.Headers.["x-ms-continuation-NextPartitionKey"]
        let nextRowKey = response.Headers.["x-ms-continuation-NextRowKey"]

        if nextPartitionKey.Is || nextRowKey.Is then
                    
            seq {   
                if nextPartitionKey.Is then
                    yield sprintf "NextPartitionKey=%s" nextPartitionKey
                if nextRowKey.Is then
                    yield sprintf "NextRowKey=%s" nextRowKey
            }   
            |> String.concat "&"
        else
            ""

    static member BuildTableContinuation
            (response                       :   HttpWebResponse) = 

        match response.Headers.["x-ms-continuation-NextTableName"] with
        |   nextTableName when nextTableName.Is     ->  sprintf "NextTableName=%s" nextTableName
        |   _                                       ->  ""
