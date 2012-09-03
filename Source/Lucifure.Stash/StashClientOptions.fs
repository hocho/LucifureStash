// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Net
open System.Web
open System.IO
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common.StringExtensions

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// This type is returned if Feedback is setup on StashClientOptions.
/// </summary>
type StashRequestResponse
        (request                            :   HttpWebRequest
        ,requestBody                        :   string
        ,response                           :   HttpWebResponse
        ,responseBody                       :   string
        ,ex                                 :   Exception) = 

    member this.Request         with get()  = request

    member this.RequestBody     with get()  = requestBody

    member this.Respone         with get()  = response

    member this.ResponseBody    with get()  = responseBody

    member this.Exception       with get()    = ex
         

// -------------------------------------------------------------------------------------------------------

type StashFeedback              =   delegate of obj -> unit

type OverrideEntitySetName      =   delegate of obj -> string 

// ---------------------------------------------------------------------------------------------------------------------

type StashClientOptions
        () = 

        
    let mutable feedback                        =   Unchecked.defaultof<StashFeedback>

    let mutable sendFeedbackAsync               =   false

    let mutable overrideEntitySetName           =   Unchecked.defaultof<OverrideEntitySetName>

    let mutable overrideEntityNameSetIsDynamic  =   true

    let mutable useHttps                        =   false

    let mutable ignoreMissingProperties         =   true

    let mutable retryPolicy : RetryPolicy       =   RetryPolicies.GetExponentialDefault()   

    let mutable timeout : TimeSpan              =   TimeSpan.FromSeconds(30.0);

    let mutable expect100Continue               =   TernarySwitch.False;

    let mutable useNagleAlgorithm               =   TernarySwitch.False;

    let mutable supportLargeObjectsInPool       =   false

    /// <summary>
    /// Setup such that a callback on every StashClient invocation returns an object with feedback related information
    /// which could be useful for troubleshooting.
    /// Feedback objects includes StashRequestResponse.
    /// </summary>
    member this.Feedback 
        with get() = 
            feedback

        and set value = 
            feedback <- value

    /// <summary>
    /// Set to true to send feedback asynchronously on another thread. Although this ensures that you request is 
    /// processed without waiting for the feedback to complete, it is possible that during debugging the feedback 
    /// is not processed in time to be helpfully for debugging. 
    /// This value defaults to false to facilitate real-time debugging.
    /// </summary>
    member this.SendFeedbackAsync
        with get() = 
            sendFeedbackAsync

        and set value  = 
            sendFeedbackAsync <- value


    /// <summary>
    /// Use in conjunction with OverrideEntitySetNameIsDynamic.
    /// Setup if you want to override the entity set name. 
    /// If OverrideEntitySetNameIsDynamic this callback will be invoked at every StashClient invocation such that a 
    /// determination can be made as to which azure table to perform the operation on.
    /// This feature can be used to persist data one of many azure tables depending on the data itself. 
    /// Defaults to unset. 
    /// </summary>
    member this.OverrideEntitySetName
        with get() = 
            overrideEntitySetName
        
        and set value = 
            overrideEntitySetName <- value

    /// <summary>
    /// Use in conjunction with OverrideEntitySetName.
    /// Set to true to call OverrideEntitySetName at every StashClient invocation.
    /// Defaults to true. 
    /// </summary>
    member this.OverrideEntitySetNameIsDynamic
        with get() = 
            overrideEntityNameSetIsDynamic
        
        and set value = 
            overrideEntityNameSetIsDynamic <- value

    /// <summary>
    /// Set to true to use Https. Defaults to false and Http.
    /// </summary>
    member this.UseHttps
        with get() = 
            useHttps

        and set value =
            useHttps <- value
    
    /// <summary>
    /// Set to true you do not want an exception to be thrown when the table has a property with is not defined 
    //  in the type. The default value for this property is 'True'.
    /// </summary>
    member this.IgnoreMissingProperties
        with get() = 
            ignoreMissingProperties

        and set value =
            ignoreMissingProperties <- value

    /// <summary>
    /// Defaults to RetryPolicies.GetExponentialDefault() retry policy.
    /// Set this property to apply an alternate retry policy.
    /// </summary>
    member this.RetryPolicy
        with get() = 
            retryPolicy

        and set value =
            retryPolicy <- value

    /// <summary>
    /// Defaults to 30 seconds, which is the timeout on the storage server.
    /// </summary>
    member this.Timeout
        with get() = 
            timeout

        and set value =
            timeout <- value

    /// <summary>
    /// Use TernarySwitch.True to enable Expect100Continue. Default is TernarySwitch.False.
    /// TernarySwitch.Unchanged implies the value is used as set in the ServicePointManager or configuration.
    /// </summary>
    member this.Expect100Continue
        with get() = 
            expect100Continue 
        
        and set value =
            expect100Continue <- value

    /// <summary>
    /// Use TernarySwitch.True to enable UseNagleAlgorithm. Default is TernarySwitch.False.
    /// TernarySwitch.Unchanged implies the value is used as set in the ServicePointManager or configuration.
    /// </summary>
    member this.UseNagleAlgorithm
        with get() = 
            useNagleAlgorithm
        
        and set value =
            useNagleAlgorithm <- value

    /// <summary>
    /// Set to true to enable split/merge support for large strings and byte arrays. Default is false.
    /// </summary>
    member this.SupportLargeObjectsInPool
        with get() =        
            supportLargeObjectsInPool

        and set value = 
            supportLargeObjectsInPool <- value

// TODO Add Estimate Entity Size Delta
