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

type DelegateId = delegate of obj -> obj

type internal QueryProvider
        (context                            :   RestRequestBuilder
        ,entitySetName                      :   string
        ,ignoreMissingProperties            :   bool 
        ,supportLargeObjectsInPool          :   bool
        ,requester                          :   (unit -> HttpWebRequest) -> string -> HttpWebResponse * string
        ,callback                           :   (obj -> unit) Option) =  

    inherit QueryProviderBase(callback)

    let queryTable
            (tableName                      :   string)
            (filter                         :   string)
            (continuation                   :   string) = 

        let cmds = 
            seq {
                if filter.Is then
                    yield filter

                if continuation.Is then
                    yield continuation                
            }   
                
        let command =   if Seq.isEmpty cmds 
                            then    tableName                       
                            else    tableName + "()?" + String.concat "&" cmds
                                            
        let request() = context.CreateRequest command CommandType.Get null Constant.ETagNoMatch Guid.Empty

        requester request ""

    override this.GetQueryText<'a>(exp: Expression) = 

        try
        
            let expParser = new ExpressionParser<'a>(exp)

            expParser.Filter

        with
        |   _   as ex   ->  ex.ToString()

    override this.Execute(exp: Expression) =

        this.ExecuteGen<obj>(exp)

    override this.ExecuteGen<'a>(exp: Expression) : obj =

        let expParser = new ExpressionParser<'a>(exp)
        let filter      = expParser.Filter
        
        let rec query top continuation count = 
            seq {
                let response, result = queryTable entitySetName filter continuation
                
                let selectExp = 
                        if expParser.ExpSelect <> null
                            then    Expression.Lambda(expParser.ExpSelect).Compile().DynamicInvoke() :?> Delegate
                            else    new DelegateId (fun y -> y) :> Delegate
                
                let rows = 
                    result
                    |> expParser.TypeReflector.ContentParse
                    |> Seq.map (expParser.TypeReflector.SetValues ignoreMissingProperties supportLargeObjectsInPool 
                                    >> (fun o -> (selectExp.DynamicInvoke(o) :?> 'a)))
                    |> Seq.cache

                yield! rows

                let count = count + rows.Count()

                // Do not process continuation if we have already retrieved the number of required rows.
                // Note: If top is use, azure storage could return less than the top count and a continuation token
                // which if fine so we can continue.
                // However, it may also return a continuation token if more than the top rows are available. In 
                // which case we want to pass.     

                // So use continuation only when top is not specified or if specified when less than the top count has 
                // been received.
                if top = 0 || count < top then

                    match RestRequestBuilder.BuildEntityContinuation response with
                    |   continuation when continuation.Is   ->  yield! query top continuation count
                    |   _                                   ->  ()
            }

        let queryFirstOrDefault() = 
                let response, result = queryTable entitySetName filter ""

                let nameValues =    result
                                    |> expParser.TypeReflector.ContentParse
        
                if Seq.isEmpty nameValues 
                    then    null
                    else    Seq.head nameValues |> expParser.TypeReflector.SetValues ignoreMissingProperties supportLargeObjectsInPool

        if expParser.IsFirstOrDefault 
            then    queryFirstOrDefault()
            else    upcast (query expParser.Take "" 0)

// -------------------------------------------------------------------------------------------------------
// -------------------------------------------------------------------------------------------------------
