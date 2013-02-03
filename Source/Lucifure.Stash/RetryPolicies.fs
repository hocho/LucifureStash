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

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type ShouldRetryResponse
        (retry                              :   bool
        ,delay                              :   TimeSpan) = 

    new() = 
        ShouldRetryResponse(false, TimeSpan.Zero)    

    new(delay                              :   TimeSpan) = 
        ShouldRetryResponse(true, delay)    

    member this.Retry 
        with get() = retry
        
    member this.Delay
        with get() = delay

// ---------------------------------------------------------------------------------------------------------------------

type ShouldRetry    =   delegate of int * Exception -> ShouldRetryResponse

type RetryPolicy    =   delegate of unit -> ShouldRetry

// ---------------------------------------------------------------------------------------------------------------------

module Retryable = 

    let DefaultMinBackoff                   = TimeSpan.FromSeconds 3.0
    
    let DefaultMaxBackoff                   = TimeSpan.FromSeconds 90.0

    let DefaultDeltaBackoff                 = TimeSpan.FromSeconds 30.0
    
    let DefaultAttempts                     = 3

    let internal delayInRange = Misc.TimeSpanMinMax DefaultMinBackoff DefaultMaxBackoff // restrict sleep time

    let internal isStatusCodeRetryable (statusCode : HttpStatusCode) = 
        match statusCode with
        |  x when (int x >= 400 && int x < 500)     -> false
        |  HttpStatusCode.NotImplemented                
        |  HttpStatusCode.HttpVersionNotSupported   ->  false
        |   _                                       ->  true

    let internal isExceptionRetryable (ex : Exception) = 
        
        match ex with
        |   :?  WebException as ex  ->  match ex.Response with
                                        |   :? HttpWebResponse  as webRes 
                                                ->  isStatusCodeRetryable webRes.StatusCode
                                        |   _   
                                                ->  false
        |   _                       ->  false


    let internal canRetry 
        (shouldRetry                        :   ShouldRetry) 
        (attempt                            :   int)
        (ex                                 :   Exception) =  

        match isExceptionRetryable ex with
        |   true    ->  let response = shouldRetry.Invoke(attempt, ex)
                        match (response.Retry, response.Delay) with
                        |   true, timespan  ->  Thread.Sleep (delayInRange timespan)  
                                                true
                        |   _               ->  false

        |   _       ->  false

// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// Contains retry policies which can be used as is or varied using the available parameters.
/// </summary>
type RetryPolicies() = 

    /// <summary>
    /// No retry is performed.
    /// </summary>
    static member GetNoRetry()               : RetryPolicy = 

        RetryPolicy
                (fun () -> ShouldRetry
                                    (fun (i : int) (ex : Exception) -> ShouldRetryResponse()))
            

    /// <summary>
    /// A fixed interval policy which applies the same wait period across retries.
    /// (Comparable with the Microsoft Storage Table Client implementation)
    /// </summary>
    static member 
        GetFixedInterval
            (maxAttempts                    :   int)    
            (interval                       :   TimeSpan) 
                                            :   RetryPolicy = 

        RetryPolicy
                (fun () -> ShouldRetry
                                (fun (attempt : int) (ex : Exception) -> 
                                    if attempt < maxAttempts 
                                        then    ShouldRetryResponse(interval)
                                        else    ShouldRetryResponse()))

    /// <summary>
    /// An retry policy which exponentially increases the waiting period with a random range delta, to avoid 
    /// multi-instance collisions within time ranges. (Comparable with the Microsoft Storage Table Client implementation)
    /// </summary>
    static member 
        GetExponential
            (maxAttempts                    :   int
            ,minBackoff                     :   TimeSpan 
            ,maxBackoff                     :   TimeSpan
            ,deltaBackoff                   :   TimeSpan)
                                            :   RetryPolicy = 
        
        let rnd = new Random()
        
        RetryPolicy
                (fun () -> ShouldRetry
                                (fun (attempt : int) (ex : Exception)    ->
                                    if attempt < maxAttempts then

                                        let increment = (Math.Pow(2.0, (double) attempt) - 1.0) * 
                                                            (double) (rnd.Next(
                                                                        int (deltaBackoff.TotalMilliseconds * 0.8), 
                                                                        int (deltaBackoff.TotalMilliseconds * 1.2)))
                                        
                                        let timeToSleepMsec = Math.Min(
                                                                    minBackoff.TotalMilliseconds + increment, 
                                                                    maxBackoff.TotalMilliseconds)

                                        ShouldRetryResponse(TimeSpan.FromMilliseconds(timeToSleepMsec))
                                    else
                                        ShouldRetryResponse()   ))


    /// <summary>
    /// An retry policy which exponentially increases the waiting period with a random range delta, to avoid 
    /// multi-instance collisions within time ranges. (Comparable with the Microsoft Storage Table Client implementation)
    /// </summary>
    static member 
        GetExponential
            (maxAttempts                    :   int
            ,deltaBackoff                   :   TimeSpan)
                                            :   RetryPolicy = 

        RetryPolicies.GetExponential (maxAttempts, Retryable.DefaultMinBackoff, Retryable.DefaultMaxBackoff, deltaBackoff)


    static member 
        internal GetExponentialDefault() = 
        
        RetryPolicies.GetExponential (Retryable.DefaultAttempts, Retryable.DefaultDeltaBackoff)
            
        