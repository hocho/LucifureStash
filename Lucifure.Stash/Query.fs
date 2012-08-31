// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic

open CodeSuperior.Common

type internal Query<'a> 
        (provider                           :   QueryProviderBase
        ,expression                         :   Expression
        ,callback                           :   (obj -> unit) Option) =

    internal new 
            (provider                       :   QueryProviderBase) = 

        Query (provider, null, None)

    internal new 
            (provider                       :   QueryProviderBase 
            ,callback                       :   (obj -> unit) Option) = 
        
        Query(provider, null, callback)

    override this.ToString() = 
        provider.GetQueryText<'a>(expression)
        
    interface IQueryable<'a> with
        
        // IQuerable
        member this.ElementType 
            with get() = 
                typeof<'a>

        member this.Expression 
            with get() = 
                if expression <> null 
                    then expression
                    else Expression.Constant(this) :> Expression 

        member this.Provider 
            with get() = 
               upcast provider

        // IEnumerable
        member this.GetEnumerator() : IEnumerator = 
            let enum = (provider.Execute(expression) :?> IEnumerable).GetEnumerator()

            match callback with
            |   Some c  ->  IEnumeratorWrapper (enum, c) :> IEnumerator
            |   None    ->  enum

        // IEnumerable<'a>
        member this.GetEnumerator() : IEnumerator<'a> = 

            let enum = (provider.ExecuteGen<'a>(expression) :?> IEnumerable<'a>).GetEnumerator()

            match callback with
            |   Some c  ->  new IEnumeratorWrapper<'a>(enum, c) :> IEnumerator<'a>
            |   None    ->  enum

and 

    [<AbstractClass>]
    QueryProviderBase
        (callback                           :   (obj -> unit) Option) = 

        abstract GetQueryText<'a> : Expression ->  string
    
        abstract Execute : Expression -> obj
        abstract ExecuteGen<'a> : Expression -> obj

        interface IQueryProvider with

            member this.CreateQuery<'a> 
                (expression                 :   Expression)                 : IQueryable<'a> =
             
                upcast new Query<'a>(this, expression, callback) 

            member this.CreateQuery 
                (expression                 :   Expression)                 : IQueryable = 

                Activator.CreateInstance(
                                typeof<Query<_>>.MakeGenericType(
                                                            TypeSystem.GetElementType expression.Type),
                                [| this :> obj ; expression :> obj ; callback :> obj |])
                    :?> IQueryable
            
            member this.Execute<'a>(expression: Expression) : 'a = 
                this.ExecuteGen<'a> expression :?> 'a
                
            member this.Execute(expression: Expression) : obj = 
                this.Execute expression


         






