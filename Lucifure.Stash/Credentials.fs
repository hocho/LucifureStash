// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Web
open System.IO
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic
open System.Net
open System.Security.Cryptography
open System.Globalization


open CodeSuperior.Common.StringExtensions

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type internal StorageKey 
        (key                                    :   byte[])  =

    let computeMacSha256 (data : string) = 

        let hasher = new HMACSHA256(key)
        
        data 
        |>  Encoding.UTF8.GetBytes
        |>  hasher.ComputeHash
        |>  Convert.ToBase64String

    member internal this.ComputeMacSha256
        (data                                   :   string) = 
        
        computeMacSha256 data 

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type SignRequest = delegate of HttpWebRequest -> unit

// ---------------------------------------------------------------------------------------------------------------------

/// <summary>
/// Stash credentials encapsulating the table storage account name and key
/// </summary>
type StorageAccountKey
        (accountName                            :   string
        ,key                                    :   string) =

    static let headerDate   = "x-ms-date"

    static let emulatorCredentials = 
            StorageAccountKey(
                        "devstoreaccount1",
                        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==") 

    let storageKey = StorageKey (key |> Convert.FromBase64String)

    static member internal GetEmulatorCredentials() = 
            emulatorCredentials

    member this.AccountName with get() = accountName
        
    member this.SignRequestLite
        (request                                :   HttpWebRequest) = 

        let ValueToSign() =
        
            sprintf "%s\n/%s%s" 
                request.Headers.[headerDate] 
                accountName 
                (request.RequestUri.PathAndQuery.LeftOf "?")

        request.Headers.Add(headerDate, DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));

        request.Headers.Add( 
                        "Authorization",        
                        sprintf 
                            "%s %s:%s"
                            "SharedKeyLite"
                            accountName
                            (storageKey.ComputeMacSha256 (ValueToSign())))

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

