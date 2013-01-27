// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Net
open System.Web
open System.IO
open System.Threading
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common
open CodeSuperior.Common.StringExtensions

// -------------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------------

/// <summary>
/// ETag Matching strategy to use for Updates and Deletes. Including all flavors of updates like merge, 
/// InsertOrReplace and InsertOrMerge
/// </summary>
type ETagMatch =
        |   IfPresent       =   1
        |   Unconditional   =   2
        |   Must            =   3

/// <summary>
/// Strategy to apply when committing.
/// Serial      - Process each item one at a time, stopping on the first error if any. This is the default strategy.
/// Parallel    - Process items in parallel in asynchronously fashion, returning a single aggregate exception on
/// one or more errors.
/// Batch       - Process all items in a single batch request.
/// Auto        - Process all items as single requests or in one or more batches depending on the partition key,
/// size of the item and the number of items. (Currently unimplemented).
/// </summary>
//[<Flags>]
type CommitStrategy = 
        |   Serial          =   1
        |   Parallel        =   2
        |   Batch           =   4
        //|   BatchAuto       =   8
   

// -------------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------------

/// <summary>
/// StashClient
/// </summary>
type StashClient<'a when 'a : equality> private
        (options                            :   StashClientOptions
        ,accountName                        :   string
        ,signRequest                        :   HttpWebRequest -> unit) = 

    let copyright                           = "Copyright © 2012 - Code Superior, Inc. All rights reserved."
                                           
    let newInstance                         = Activator.CreateInstance(typeof<'a>)

    let typeReflector                       = Reflector<'a>.Get
    
    let ignoreMissingProperties             = options.IgnoreMissingProperties

    let retryPolicy                         = options.RetryPolicy
    
    let supportLargeObjectsInPool           = options.SupportLargeObjectsInPool

    let getEntitySetName = 
        // if override 
        if options.OverrideEntitySetName <> Unchecked.defaultof<OverrideEntitySetName> then    
            // and dynamic, then set to always invoke the method
            if options.OverrideEntitySetNameIsDynamic then 
                options.OverrideEntitySetName.Invoke
            else          
                // invoke statically now and bind the value forever
                let entitySetName = options.OverrideEntitySetName.Invoke newInstance
                fun (o : obj) -> entitySetName
        else    
            let entitySetName = typeReflector.EntitySetName
            fun (o : obj) -> entitySetName

    let expWrapForGet = if options.IgnoreResourceNotFoundException
                            then Exp.wrapIgnoreResourceNotFoundException
                            else Exp.wrap

    let entitySetNameRaw() = getEntitySetName newInstance

    let context = new RestRequestBuilder(
                                    accountName, 
                                    options.UseHttps, 
                                    Misc.TimeSpanMax options.Timeout (TimeSpan.FromMilliseconds(1.0)),
                                    options.Expect100Continue,
                                    options.UseNagleAlgorithm,
                                    signRequest)

    let isSendFeedback, sendFeedback = 
        if options.Feedback <> Unchecked.defaultof<StashFeedback>
            then    true,   if options.SendFeedbackAsync then
                                fun o -> 
                                    try
                                        let task = 
                                            async {
                                                try
                                                    options.Feedback.Invoke o
                                                with
                                                |   _ -> ()
                                            }

                                        Async.StartAsTask(task) |> ignore
                                    with
                                    |   _ -> () // consume the exception since we do not want any leaks
                            else 
                                options.Feedback.Invoke       

            else    false,  fun _ -> ()

    let getResponseStream (response : WebResponse) = 
        match response with
        |   null    ->  ""
        |   _       ->  (new StreamReader(response.GetResponseStream())).ReadToEnd()

    // actual request to table service
    let makeRequest (request : HttpWebRequest) content = 
        
        let shouldRetry = retryPolicy.Invoke()   

        let rec makeRequest' request content attempt =         
            try
                let response, result = RestRequestBuilder.GetResponse request

                if isSendFeedback then    
                    sendFeedback (StashRequestResponse(request, content, response, result, null))
            
                response, result
            with 
                |   :? WebException as ex  ->   

                    match Retryable.canRetry shouldRetry attempt ex with
                    |   true    ->  makeRequest' request content (attempt + 1)
                    |   false   ->  sendFeedback (StashRequestResponse
                                                                (request
                                                                , content
                                                                , ex.Response :?> HttpWebResponse
                                                                , getResponseStream ex.Response
                                                                , ex))
                                    reraise()

                |   _ as ex                 ->  
                    
                    sendFeedback (StashRequestResponse(request, content, null, "", ex))
                    reraise()                  

        makeRequest' request content 1
    

    // -----------------------------------------------------------------------------------------------------------------
    // Table primitives

    let tableList(command : string) = 

        let rec tableList' (cont : string) = 
            seq {
                let request = context.CreateRequest 
                                                (command + 
                                                    (if cont.Is 
                                                        then (if command.Contains "?" 
                                                                then    "&" 
                                                                else    "?") + cont 
                                                        else ""))
                                                CommandType.Get
                                                null
                                                Constant.ETagNo
                                                Guid.Empty

                let response, result = makeRequest request ""

                let tableName = 
                    result
                    |>  typeReflector.ContentParse
                    |>  Seq.concat
                    |>  Seq.map (fun nv -> nv.Value.ToString())            

                yield! tableName

                match RestRequestBuilder.BuildTableContinuation response with
                |   cont when cont.Is   ->  yield! tableList' cont
                |   _  as cont          ->  ()
            }

        tableList' ""

    let tableCreate (entitySetName : string) =

        let content =   XmlRequestBuilder.buildCreateTable entitySetName
        
        let request = context.CreateRequest 
                                        "Tables"
                                        CommandType.Insert
                                        content
                                        Constant.ETagNo
                                        Guid.Empty
        makeRequest request content


    let tableRequest entitySetName commandType =

        let request = context.CreateRequest 
                                        (sprintf 
                                            "Tables('%s')" 
                                            entitySetName)
                                        commandType
                                        null
                                        Constant.ETagNo
                                        Guid.Empty

        makeRequest request ""
    
    let tableGet (entitySetName : string)  =

        tableRequest entitySetName CommandType.Get

    let tableDelete (entitySetName : string) =

        tableRequest entitySetName CommandType.Delete

    let tableExists (entitySetName : string) = 

        try
            tableGet entitySetName |> ignore
            true
        with
        |   :? WebException as WebEx -> 

                match Exp.getStatusCode WebEx.Response with 
                |   HttpStatusCode.NotFound     ->  false
                |   _                           ->  reraise()   

    // -----------------------------------------------------------------------------------------------------------------
    // Implementation

    let getEntity xml = 

        let items = 
            xml
            |>  typeReflector.ContentParse
            |>  Seq.map (typeReflector.SetValues ignoreMissingProperties supportLargeObjectsInPool)
            |>  Seq.cache   
            // cache else the isEmpty / head below will break it

        (if Seq.isEmpty items
            then null
            else (Seq.head items)) :?> 'a

    
    let insert 
        (instance                           :   'a) = 

        let _, content = typeReflector.ContentBuild supportLargeObjectsInPool instance
        
        let request = context.CreateRequest 
                                        (getEntitySetName instance)
                                        CommandType.Insert
                                        content
                                        Constant.ETagNo
                                        Guid.Empty

        let response, result = makeRequest request content

        typeReflector.SetETagValue instance (XmlResponseParser.getETagValue result)
        instance

    let transformETag etag (eTagMatch : ETagMatch) = 

        match eTagMatch with
        |   ETagMatch.IfPresent         ->  etag
        |   ETagMatch.Unconditional     ->  Constant.ETagNoMatch
        |   ETagMatch.Must              ->  if etag = Constant.ETagNoMatch then
                                                Msg.Raise (Msg.errETagNotDefined typeof<'a>.FullName)
                                            else
                                                etag
        |   _                           ->  Msg.Raise   (StashMessage(
                                                                StashError.UnexpectedRuntime,
                                                                "Unexpected ETagMatch supplied."))

    let makeIdCmd
            (entitySetName                  :   string)
            (pkey                           :   string)
            (rkey                           :   string) = 

        sprintf "%s(PartitionKey='%s', RowKey='%s')" entitySetName pkey rkey   

    let makeIdCmdFromInstance
            (instance                       :   'a) = 

        makeIdCmd
            (getEntitySetName instance) 
            (typeReflector.GetPartitionKeyValue instance)
            (typeReflector.GetRowKeyValue instance)

    let update
        (commandType                        :   CommandType) 
        (eTagMatch                          :   ETagMatch) 
        (instance                           :   'a) = 

        let etag, content = typeReflector.ContentBuild supportLargeObjectsInPool instance
        
        let request = context.CreateRequest 
                                        (makeIdCmdFromInstance instance)
                                        commandType
                                        content
                                        (transformETag etag eTagMatch)
                                        Guid.Empty
        try
            let response, result = makeRequest request content

            typeReflector.SetETagValue instance (response.Headers.[Constant.ETagInHeader])

            instance
        with
        |   :? WebException as WebEx -> 

                match Exp.getStatusCode WebEx.Response with 
                |   HttpStatusCode.PreconditionFailed
                    ->  Msg.RaiseInner
                                (Msg.errETagMatchFailed etag)
                                WebEx
                |   _   
                    ->  reraise()   

    let deleteKeyed
        (partitionKey                       :   string) 
        (rowKey                             :   string)
        (etag                               :   string)
        (eTagMatch                          :   ETagMatch) = 
         
        let etag = transformETag etag eTagMatch

        let request = context.CreateRequest 
                                        (makeIdCmd (entitySetNameRaw()) partitionKey rowKey)
                                        CommandType.Delete 
                                        null
                                        etag
                                        Guid.Empty
        try
            makeRequest request ""
        with
        |   :? WebException as WebEx -> 

                match Exp.getStatusCode WebEx.Response with 
                |   HttpStatusCode.PreconditionFailed
                    ->  Msg.RaiseInner
                                (Msg.errETagMatchFailed etag)
                                WebEx
                |   _   
                    ->  reraise()   

    let delete 
        (eTagMatch                          :   ETagMatch) 
        (instance                           :   'a) = 
        
        deleteKeyed
            (typeReflector.GetPartitionKeyValue instance)
            (typeReflector.GetRowKeyValue instance)
            (typeReflector.GetETagValue instance)
            eTagMatch


    let get 
        (partitionKey                       :   string) 
        (rowKey                             :   string) =
         
        let request = context.CreateRequest 
                                        (makeIdCmd (entitySetNameRaw()) partitionKey rowKey)
                                        CommandType.Get 
                                        null
                                        Constant.ETagNoMatch
                                        Guid.Empty    

        let response, result = makeRequest request ""              // do not pass the content here

        getEntity result

    let createQuery
            (callback                       :   (obj -> unit) Option) = 

        Query<'a>(
            new QueryProvider(
                        context,
                        (entitySetNameRaw()),
                        ignoreMissingProperties,
                        supportLargeObjectsInPool,
                        makeRequest,
                        callback)) 
        :> IQueryable<'a>

    let commitBatch
            (list                           :   seq<EntityOp<'a>>)
            (setUnchanged                   :   'a -> unit)             = 

        let batchId = Guid.NewGuid()

        let buildTarget (entityOp : EntityOp<'a>) = 
            match entityOp.Op with
            |   CommandType.Insert          ->  entitySetNameRaw()
            |   CommandType.InsertOrUpdate
            |   CommandType.InsertOrMerge   
            |   CommandType.Update
            |   CommandType.Merge
            |   CommandType.Delete          ->  makeIdCmdFromInstance entityOp.Item
            |   _                           ->  ""
           

        let content = 
                list
                |>  Seq.map (fun eo ->  buildTarget eo |> context.FormatRequest
                                        ,eo)                                                

                |>  typeReflector.ContentBuildBatch supportLargeObjectsInPool batchId
        
        let request = context.CreateRequest 
                                        "$batch"
                                        CommandType.Batch
                                        content
                                        Constant.ETagNo
                                        batchId

        let response, result = makeRequest request content

        let batchResult = XmlResponseParser.splitBatch result |> Seq.cache 
        
        if Seq.isEmpty batchResult then
            Msg.Raise 
                    (StashMessage(
                        StashError.UnexpectedRuntime,
                        "Batch response did not return a result set."))
        else 
            let top = Seq.head batchResult

            if top.errorCode.Is then
                Msg.Raise (Msg.errBatchCommitFailed 
                                top.errorCode 
                                top.errorMessage)
            else 
                // set etags
                batchResult |>
                (list       |>
                    Seq.iter2   
                        (fun orig res ->
                            typeReflector.SetETagValue (orig.Item ) res.etag ))                                     

                list |> Seq.iter (fun op -> setUnchanged op.Item)

    let makeWorkUnit 
            (eo                             : EntityOp<'a>) = 

        let workUnit =  
            match eo.Op with
            |   CommandType.Insert          ->  insert 
            |   CommandType.InsertOrUpdate  ->  update CommandType.InsertOrUpdate   ETagMatch.IfPresent
            |   CommandType.InsertOrMerge   ->  update CommandType.InsertOrMerge    ETagMatch.IfPresent
            |   CommandType.Update          ->  update CommandType.Update           ETagMatch.IfPresent
            |   CommandType.Merge           ->  update CommandType.Merge            ETagMatch.IfPresent
            |   CommandType.Delete          ->  fun i -> delete ETagMatch.IfPresent i |> ignore; i
            |   _                           ->  fun z -> z
                        
        fun () -> workUnit eo.Item
       
    let makeWorkUnitList
            (list                           :   seq<EntityOp<'a>>) 
            (callbackOnSuccess              :   'a -> unit)             = 
        
        list
        |>  Seq.map     (fun eo -> (makeWorkUnit eo) >> callbackOnSuccess)

    let commitSerial
            (list                           :   seq<EntityOp<'a>>) 
            (callbackOnSuccess              :   'a -> unit)             = 

        makeWorkUnitList list callbackOnSuccess
        |>  Seq.iter    (fun f -> f() )

    let commitParallel
            (list                           :   seq<EntityOp<'a>>) 
            (callbackOnSuccess              :   'a -> unit)             = 
        
        let appendEx ex = 
            ()

        // returns list of exceptions
        let invoke workUnit = 
            async {   
                return(
                    try
                        workUnit()
                        null
                    with
                    |   :? StashException as stashEx    ->  stashEx                                                 
                                                            :> exn
                    |   _  as ex                        ->  StashRuntimeException(Msg.errUnexpectedRuntime,  ex) 
                                                            :> exn
                )
            }

        let exns = 
            (makeWorkUnitList list callbackOnSuccess
                |>  Seq.map invoke
                |>  Async.Parallel
                |>  Async.RunSynchronously
                |>  Seq.filter ((<>) null))
                .ToArray()

        if exns.Length > 0 then
            raise (StashAggregateException exns) 
      

    let commit
            (list                           :   seq<EntityOp<'a>>) 
            (commitStrategy                 :   CommitStrategy) 
            (callbackOnSuccess              :   'a -> unit)             = 

        callbackOnSuccess
        |>  (list 
        |>      match commitStrategy with 
                |   CommitStrategy.Serial       ->  commitSerial 
                |   CommitStrategy.Parallel     ->  commitParallel 
                |   CommitStrategy.Batch        ->  commitBatch 
                |   _  as value                 ->  Msg.Raise 
                                                        (StashMessage(
                                                            StashError.UnexpectedRuntime,
                                                            sprintf 
                                                                "Unexpected runtime error CommitStrategy = '%d'." 
                                                                (int value))))

    // -----------------------------------------------------------------------------------------------------------------

    let listTables (prefix : string) = 
        
        let command =   if prefix.Is then
                            sprintf
                                "Tables()?$filter=TableName ge '%s' and TableName le '%s'"
                                prefix
                                (prefix + String('z', 63 - Math.Min(63, prefix.Length)))
                         else
                            "Tables()"

        tableList command        

    let createTable (entitySetName : string) =

        try
            tableCreate entitySetName
        with
        |   :? WebException as WebEx -> 

                match Exp.getStatusCode WebEx.Response with 
                |   HttpStatusCode.Conflict     
                    ->  Msg.RaiseInner
                                (Msg.errTableAlreadyExists entitySetName)
                                WebEx
                |   _   
                    ->  reraise()   

    let createTableIfNotExist (entitySetName : string) =

        if tableExists entitySetName |> not then
            createTable entitySetName |> ignore

    let deleteTable (entitySetName : string) =

        try
            tableDelete entitySetName
        with
        |   :? WebException as WebEx -> 

                match Exp.getStatusCode WebEx.Response with 
                |   HttpStatusCode.NotFound     
                    ->  Msg.RaiseInner
                                (Msg.errTableNotFound entitySetName)    
                                WebEx
                |   _   
                    ->  reraise()   

    let deleteTableIfNotExist (entitySetName : string) =

        if tableExists entitySetName then
            deleteTable entitySetName |> ignore

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    // Public Constructors

    new (credentials                        :   StorageAccountKey
        ,options                            :   StashClientOptions) =
           
        StashClient(
                options,
                credentials.AccountName,
                credentials.SignRequestLite)

    new (credentials                        :   StorageAccountKey) =
           
        StashClient(credentials, StashClientOptions())

    /// <summary>
    /// No credentials specified defaults to the Storage Emulator
    /// </summary>
    new () =

        StashClient(
            StorageAccountKey.GetEmulatorCredentials(),
            StashClientOptions())

    /// <summary>
    /// No credentials specified defaults to the Storage Emulator
    /// </summary>
    new (options                            :   StashClientOptions) =

        StashClient(
            StorageAccountKey.GetEmulatorCredentials(),
            options)

    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Use this constructor when using the credentials from the Microsoft CloudStorageAccount class. 
    /// </summary>
    new (accountName                        :   string
        ,signRequest                        :   SignRequest
        ,options                            :   StashClientOptions) =
           
        StashClient(
                options,
                accountName,
                signRequest.Invoke)

    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Use this constructor when using the credentials from the Microsoft CloudStorageAccount class. 
    /// </summary>
    new (accountName                        :   string
        ,signRequest                        :   SignRequest) =
           
        StashClient(
                StashClientOptions(),
                accountName,
                signRequest.Invoke)

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------

    member this.GetContext() = 
        
        StashContext this

    member internal this.GetEntityId
            (instance                       :   'a) = 
        
        typeReflector.GetEntityId instance

    member internal this.Commit
            (list                           :   seq<EntityOp<'a>>)
            (commitStrategy                 :   CommitStrategy)
            (setUnchanged                   :   'a -> unit)         = 

        Exp.wrap (fun () -> commit list commitStrategy setUnchanged)        

    // -----------------------------------------------------------------------------------------------------------------

    member this.ComputeEntityDataSize
            (instance                       :   'a) 
                                            :   int     =

        Exp.wrap(fun () -> typeReflector.ComputeEntityDataSize 
                                                    supportLargeObjectsInPool 
                                                    instance)

    // -----------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Inserts an entity into the table.
    /// </summary>
    member this.Insert 
        (instance                           :   'a) = 

        Exp.wrap (fun () -> insert instance)
            
    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Updates an entity. ETag matching is performed depending on the ETagMatch enumeration passed in.
    /// </summary>
    member this.Update
            (instance                       :   'a
            ,eTagMatch                      :   ETagMatch) = 

        Exp.wrap (fun () -> update CommandType.Update eTagMatch instance)
    
    /// <summary>
    /// Updates an entity. ETag matching is enforced if the Type supports it.
    /// </summary>
    member this.Update
            (instance                       :   'a) = 

        this.Update (instance, ETagMatch.IfPresent)

    /// <summary>
    /// Updates an entity. ETag matching is disabled.
    /// </summary>
    member this.UpdateUnconditional
            (instance                       :   'a) = 

        this.Update (instance, ETagMatch.Unconditional)

    /// <summary>
    /// Updates an entity. ETag matching is enforced.
    /// </summary>
    member this.UpdateETagMustMatch
            (instance                        :   'a) = 

        this.Update (instance, ETagMatch.Must)

    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Inserts or Updates an entity.
    /// </summary>
    member this.InsertOrUpdate
            (instance                       :   'a) = 

        Exp.wrap (fun () -> update CommandType.InsertOrUpdate ETagMatch.Unconditional instance)
    
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Merges an entity. Only non-null parameters are merged into the existing entity.
    /// ETag matching is performed depending on the ETagMatch enumeration passed in.
    /// </summary>
    member this.Merge
            (instance                       :   'a
            ,eTagMatch                      :   ETagMatch) = 

        Exp.wrap (fun () -> update CommandType.Merge eTagMatch instance)

    /// <summary>
    /// Merges an entity. Only non-null parameters are merged into the existing entity.
    /// ETag matching is enforced if the Type supports it.
    /// </summary>
    member this.Merge
            (instance                       :   'a) = 

        this.Merge (instance, ETagMatch.IfPresent)

    /// <summary>
    /// Merges an entity. Only non-null parameters are merged into the existing entity.
    /// ETag matching is disabled.
    /// </summary>
    member this.MergeUnconditional
            (instance                       :   'a) = 

        this.Merge (instance, ETagMatch.Unconditional)

    /// <summary>
    /// Merges an entity. Only non-null parameters are merged into the existing entity.
    /// ETag matching is enforced.
    /// </summary>
    member this.MergeETagMustMatch
            (instance                        :   'a) = 

        this.Merge (instance, ETagMatch.Must)

    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Inserts or Merges an entity.
    /// </summary>
    member this.InsertOrMerge
            (instance                       :   'a) = 

        Exp.wrap (fun () -> update CommandType.InsertOrMerge ETagMatch.Unconditional instance)
    
    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Deletes an entity, identifying it with the PartitionKey and RowKey of the entity passed in.
    /// Delete is performed based on the entity and the ETagMatch passed in.
    /// </summary>
    member this.Delete
            (instance                       :   'a
            ,eTagMatch                      :   ETagMatch) = 

        Exp.wrap (fun () -> delete
                                eTagMatch
                                instance
                            |> ignore )
                
    /// <summary>
    /// Deletes an entity, identifying it with the PartitionKey and RowKey of the entity passed in.
    /// ETag matching is enforced if the Type supports it.
    /// </summary>
    member this.Delete
            (instance                       :   'a) = 

        this.Delete (instance, ETagMatch.IfPresent)        

    /// <summary>
    /// Deletes an entity, identifying it with the PartitionKey and RowKey of the entity passed in.
    /// Not ETag matching is performed.
    /// </summary>
    member this.DeleteUnconditional
            (instance                       :   'a) = 

        this.Delete (instance, ETagMatch.Unconditional)        

    /// <summary>
    /// Deletes an entity, identifying it with the PartitionKey and RowKey of the entity passed in.
    /// ETag matching is enforced.
    /// </summary>
    member this.DeleteETagMustMatch
            (instance                       :   'a) = 

        this.Delete (instance, ETagMatch.Must)        

    /// <summary>
    /// Deletes an entity unconditionally, identifying it with the PartitionKey and RowKey passed in.
    /// Not ETag match is performed
    /// </summary>
    member this.Delete
            (partitionKey                   :   string
            ,rowKey                         :   string) =

        Exp.wrap (fun () -> deleteKeyed
                                partitionKey
                                rowKey
                                ""
                                ETagMatch.Unconditional |> ignore)

    // -----------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Retrieves an entity, identifying it with the PartitionKey and RowKey passed in.
    /// </summary>
    member this.Get
        (partitionKey                       :   string) 
        (rowKey                             :   string) =
         
        expWrapForGet (fun () -> get partitionKey rowKey)

    // -----------------------------------------------------------------------------------------------------------------

    member internal this.CreateQuery 
            (callback                       :   (obj -> unit) Option) =

        Exp.wrap (fun () -> createQuery callback)

    /// <summary>
    /// Creates a query to retrieves one or more entities based on the query.
    /// </summary>
    member this.CreateQuery() = 

        this.CreateQuery None

    // -----------------------------------------------------------------------------------------------------------------
    // Tables

    /// <summary>
    /// Returns a list of a all the table names which match the supplied prefix
    /// </summary>
    member this.ListTables
        (prefix                             :   string) =

        Exp.wrap (fun () -> listTables prefix)

    /// <summary>
    /// Returns a list of a all the table names
    /// </summary>
    member this.ListTables() =

        this.ListTables ""


    /// <summary>
    /// Creates a table based on the supplied entity table name.
    /// </summary>
    member this.DoesTableExist
            (tableName                      :   string) =

        Exp.wrap (fun () -> tableExists tableName)

    /// <summary>
    /// Creates a table based on the resolved entity table name.
    /// </summary>
    member this.DoesTableExist() =

        this.DoesTableExist (entitySetNameRaw())

    /// <summary>
    /// Creates a table based on the supplied entity table name.
    /// </summary>
    member this.CreateTable
            (tableName                      :   string) =

        Exp.wrap (fun () -> createTable tableName |> ignore)

    /// <summary>
    /// Creates a table based on the resolved entity table name.
    /// </summary>
    member this.CreateTable() =

        this.CreateTable (entitySetNameRaw())

    /// <summary>
    /// Creates a table based on the supplied entity table name, if it does not already exist.
    /// </summary>
    member this.CreateTableIfNotExist
            (tableName                      :   string) =

        Exp.wrap (fun () -> createTableIfNotExist tableName)

    /// <summary>
    /// Creates a table based on the resolved entity table name, if it does not already exist.
    /// </summary>
    member this.CreateTableIfNotExist() =

        this.CreateTableIfNotExist (entitySetNameRaw())

    /// <summary>
    /// Deletes a table based on the supplied entity table name.
    /// </summary>
    member this.DeleteTable
            (tableName                      :   string) =

        Exp.wrap (fun () -> deleteTable tableName |> ignore)

    /// <summary>
    /// Deletes a table based on the resolved entity table name.
    /// </summary>
    member this.DeleteTable() =

        this.DeleteTable (entitySetNameRaw())

    /// <summary>
    /// Deletes a table based on the supplied entity table name.
    /// </summary>
    member this.DeleteTableIfNotExists
            (tableName                      :   string) =

        Exp.wrap (fun () -> deleteTableIfNotExist tableName)

    /// <summary>
    /// Deletes a table based on the resolved entity table name.
    /// </summary>
    member this.DeleteTableIfNotExists() =

        this.DeleteTableIfNotExists (entitySetNameRaw())

// -------------------------------------------------------------------------------------------------------

and StashContext<'a when 'a : equality> internal
        (client                             :   StashClient<'a>) = 
    
    let items = new Dictionary<string, 'a * EntityState>()

    let getId = client.GetEntityId

    let updateState item state = 
        
        items.[getId item] <- (item, state)
    
    let updateStateOnTransition item = 

        Exp.consume 
            (fun () ->         
                let id = getId item

                if snd items.[id] = EntityState.Deleted 
                    then    items.Remove(id) |> ignore
                    else    items.[id] <- (item, EntityState.Unchanged)
            )
        
    let updateStateOnQuery (item : obj) = 
        
        Exp.consume (fun () ->  updateState (item :?> 'a) EntityState.Unchanged )

    static let allEntityStates = 
        
            EntityState.Unchanged 
        ||| EntityState.Inserted 
        ||| EntityState.Updated 
        ||| EntityState.Merged
        ||| EntityState.InsertedOrUpdated
        ||| EntityState.InsertedOrMerged
        ||| EntityState.Deleted

    let getTrackedEntities 
            (state                          :   EntityState) =

        (items 
            |>  Seq.filter  (fun i -> state.HasFlag (i.Value |> snd))
            |>  Seq.map     (fun i -> EntityDescriptor(fst i.Value, snd i.Value)))
            .ToList()
        // ToList because we want to take a snapshot in time

    /// <summary>
    /// Returns an IEnumerable of entity descriptors being tracked in the context.
    /// Filtered by the EntityState. Multiple EntityStates can be ORed together. 
    /// Entities are tracked by being uniquely identified by the Partition and the Row Key.
    /// </summary>
    member this.GetTrackedEntities
            (state                          :   EntityState) =
        
        Exp.wrap (fun () -> getTrackedEntities state)

    /// <summary>
    /// Returns an IEnumerable of entity descriptors being tracked in the context.
    /// Entities are tracked by being uniquely identified by the Partition and the Row Key.
    /// </summary>
    member this.GetTrackedEntities() =

        Exp.wrap (fun () -> getTrackedEntities allEntityStates)
        

    /// <summary>
    /// Removes an entity being tracked from the context.
    /// </summary>
    member this.Detach
            (item                           :   'a) =

        Exp.wrap (fun () -> items.Remove(getId item))

    /// <summary>
    /// Removes all entity being tracked from the context. Essentially clearing the context.
    /// </summary>
    member this.DetachAll
            (item                           :   'a) =

        Exp.wrap (fun () -> items.Clear())

    /// <summary>
    /// Places an entity into the context, such that a insert is performed on commit.
    /// </summary>
    member this.Insert
            (item                           :   'a) =

        Exp.wrap (fun () -> updateState item EntityState.Inserted)

    /// <summary>
    /// Places an entity into the context, such that a update is performed on commit.
    /// </summary>
    member this.Update
            (item                           :   'a) =

        Exp.wrap (fun () -> updateState item EntityState.Updated)

    /// <summary>
    /// Places an entity into the context, such that an insert or update is performed on commit.
    /// </summary>
    member this.InsertOrUpdate
            (item                           :   'a) =

        Exp.wrap (fun () -> updateState item EntityState.InsertedOrUpdated)

    /// <summary>
    /// Places an entity into the context, such that a merge is performed on commit.
    /// </summary>
    member this.Merge
            (item                           :   'a) =

        Exp.wrap (fun () -> updateState item EntityState.Merged)

    /// <summary>
    /// Places an entity into the context, such that an insert or merge is performed on commit.
    /// </summary>
    member this.InsertOrMerge
            (item                           :   'a) =

        Exp.wrap (fun () -> updateState item EntityState.InsertedOrMerged)

    /// <summary>
    /// Places an entity into the context, such that a delete is performed on commit.
    /// </summary>
    member this.Delete
            (item                           :   'a) =

        Exp.wrap (fun () -> updateState item EntityState.Deleted)

    /// <summary>
    /// Commits all unchanged entities to the Azure table, employing to the desired commit strategy.
    /// </summary>
    member this.Commit
            (commitStrategy                 :   CommitStrategy) =   
     
        let list =  (items 
                        |>  Seq.filter (fun i -> snd i.Value <> EntityState.Unchanged)    
                        |> Seq.map (fun i -> EntityOp(enum (int (snd i.Value)), fst i.Value)))
                        .ToList() 

        client.Commit list commitStrategy updateStateOnTransition

    /// <summary>
    /// Commits all unchanged entities to the Azure table, employing to the default commit strategy. 
    /// The default strategy is Serial.
    /// </summary>
    member this.Commit() = 
        
        this.Commit CommitStrategy.Serial        

    /// <summary>
    /// Returns a query object suitable for querying entities such that they are tracked by the context.
    /// </summary>
    member this.CreateQuery() = 

        client.CreateQuery (Some updateStateOnQuery)

