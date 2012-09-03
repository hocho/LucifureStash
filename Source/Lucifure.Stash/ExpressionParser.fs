// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.IO
open System.Web
open System.Text
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.ObjectModel
open System.Collections.Generic
open System.Reflection

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common

// ---------------------------------------------------------------------------------------------------------------------

type internal ExpNode =
        | Unknown

        | ConstantObj           of obj
        | ConstantObjMorphed    of obj * IStash

        | Property          of string
        | Field             of string
        | PrimaryKey
        | RowKey
        | Binary            of ExpNode * ExpNode
        | And               of ExpNode * ExpNode
        | Or                of ExpNode * ExpNode
        | Eq                of ExpNode * ExpNode
        | Ne                of ExpNode * ExpNode
        | Lt                of ExpNode * ExpNode
        | Le                of ExpNode * ExpNode
        | Gt                of ExpNode * ExpNode
        | Ge                of ExpNode * ExpNode
        | Not               of ExpNode
        | CompareTo         of ExpNode * ExpNode
    
// ---------------------------------------------------------------------------------------------------------------------

type internal   ExpStatement = {   
                        EntityType              :   Type;           // the type of the entity mapped to the table
                                                                    // (not what is the result of the query which could
                                                                    // be another type or anonymous.
                        ExpNodeTree             :   ExpNode;
                        ExpSelect               :   Expression; 
                        Take                    :   int Option;
                        IsFirstOrDefault        :   bool;
                    }

// ---------------------------------------------------------------------------------------------------------------------

module internal Op = 

    [<Literal>]
    let And   = "and"

    [<Literal>]
    let Or    = "or"

    [<Literal>]
    let Eq    = "eq"

    [<Literal>]
    let Ne    = "ne"

    [<Literal>]
    let Lt    = "lt"

    [<Literal>]
    let Le    = "le"

    [<Literal>]
    let Gt    = "gt"

    [<Literal>]
    let Ge    = "ge"

    let reverse = 
        function
        |   Lt          ->  Ge
        |   Le          ->  Gt
        |   Gt          ->  Le
        |   Ge          ->  Lt
        |   _  as op    ->  op

// ---------------------------------------------------------------------------------------------------------------------

type internal ExpressionParser<'a>
        (exp                                :   Expression) =


    static let fmtSpaces = " {0} "

    let sb = new StringBuilder()

    let sbAppendWithSpaces (s: string) = sb.AppendFormat(fmtSpaces, s) |> ignore

    let sbAppendWithSpacesIf (s: string) = if s.Is then sbAppendWithSpaces s

    let createConstantObj value exp =
    
        if value = null then
            Msg.Raise (Msg.errQueryAgainstNull (exp.ToString()))

        ExpNode.ConstantObj value

    let quotedStringValue (str : String) = 

        if str.Is && str.StartsWith("'") && str.EndsWith("'") 
            then    str.Substring(0, str.Length - 2)
            else    str

    let rec visitGeneric (exp: Expression) = 
    
        let value = Expression.Lambda(exp).Compile().DynamicInvoke()

        createConstantObj value exp

    and visitUnary  (exp: UnaryExpression) = 

        match exp.NodeType with
        | ExpressionType.Not                    -> sbAppendWithSpaces "not"
                                                   ExpNode.Not(ExpNode.Unknown)
        | _                                     -> visit exp.Operand


    and visitBinary (exp: BinaryExpression) = 
        
            (match exp.NodeType with                 
            | ExpressionType.AndAlso                -> sbAppendWithSpacesIf "and"
                                                       ExpNode.And                
            | ExpressionType.OrElse                 -> sbAppendWithSpacesIf "or"
                                                       ExpNode.Or
            | ExpressionType.Equal                  -> sbAppendWithSpacesIf "eq"
                                                       ExpNode.Eq
            | ExpressionType.NotEqual               -> sbAppendWithSpacesIf "ne"
                                                       ExpNode.Ne
            | ExpressionType.LessThan               -> sbAppendWithSpacesIf "lt"
                                                       ExpNode.Lt
            | ExpressionType.LessThanOrEqual        -> sbAppendWithSpacesIf "le"
                                                       ExpNode.Le
            | ExpressionType.GreaterThan            -> sbAppendWithSpacesIf "gt"
                                                       ExpNode.Gt
            | ExpressionType.GreaterThanOrEqual     -> sbAppendWithSpacesIf "ge"
                                                       ExpNode.Ge
            | _                                     -> sbAppendWithSpacesIf ""
                                                       ExpNode.Binary) 
                (visit exp.Left, visit exp.Right)


    and visitCall (exp: MethodCallExpression) = 
        
        sbAppendWithSpaces exp.Method.Name

        if exp.Method.Name = "CompareTo" then
            ExpNode.CompareTo 
                ((visit exp.Object)
                ,(visit exp.Arguments.[0]))
        else
            let value = Expression.Lambda(exp).Compile().DynamicInvoke()

            createConstantObj value exp

    and visitMemberAccess (exp: MemberExpression) = 
        
        match exp.Expression.NodeType with
        |   ExpressionType.Constant ->

                let mbr = exp.Member

                let expValue = (exp.Expression :?> ConstantExpression).Value

                let value =  if mbr.MemberType = MemberTypes.Field 
                                then (mbr :?> FieldInfo).GetValue(expValue)
                                else (mbr :?> PropertyInfo).GetGetMethod(true).Invoke(expValue, [||])

                createConstantObj value exp

        |   ExpressionType.MemberAccess ->

                let value = Expression.Lambda(exp).Compile().DynamicInvoke()

                createConstantObj value exp


        |   ExpressionType.Parameter ->

                visit exp.Expression |> ignore

                sbAppendWithSpaces exp.Member.Name

                // match on name because the types are private
                match exp.GetType().Name with
                |   "PropertyExpression"        ->  ExpNode.Property exp.Member.Name
                |   "FieldExpression"           ->  ExpNode.Field exp.Member.Name
                |   _                           ->  ExpNode.Unknown


        |   _ -> ExpNode.Unknown    


    and visitParameter (exp: ParameterExpression) = 
        
        sbAppendWithSpaces exp.Name

        ExpNode.Unknown

    and visitConstant (exp: ConstantExpression) = 
        
        createConstantObj (exp.Value) exp


    and visit (exp: Expression) : ExpNode = 

        if exp <> null then
            match exp.NodeType with
                |   ExpressionType.Quote                ->  visit (exp :?> UnaryExpression).Operand
                |   ExpressionType.Negate     
                |   ExpressionType.NegateChecked
                |   ExpressionType.Not
                |   ExpressionType.Convert
                |   ExpressionType.ConvertChecked
                |   ExpressionType.ArrayLength
                |   ExpressionType.TypeAs               ->  visitUnary (exp :?> UnaryExpression)
                |   ExpressionType.Add
                |   ExpressionType.AddChecked
                |   ExpressionType.Subtract
                |   ExpressionType.SubtractChecked
                |   ExpressionType.Multiply
                |   ExpressionType.MultiplyChecked
                |   ExpressionType.Divide
                |   ExpressionType.Modulo
                |   ExpressionType.And
                |   ExpressionType.AndAlso
                |   ExpressionType.Or
                |   ExpressionType.OrElse
                |   ExpressionType.LessThan
                |   ExpressionType.LessThanOrEqual
                |   ExpressionType.GreaterThan
                |   ExpressionType.GreaterThanOrEqual
                |   ExpressionType.Equal
                |   ExpressionType.NotEqual
                |   ExpressionType.Coalesce
                |   ExpressionType.ArrayIndex
                |   ExpressionType.RightShift
                |   ExpressionType.LeftShift
                |   ExpressionType.ExclusiveOr          ->  visitBinary (exp :?> BinaryExpression)
                |   ExpressionType.TypeIs               ->  visitGeneric(exp :?> TypeBinaryExpression)
                |   ExpressionType.Conditional          ->  visitGeneric(exp :?> ConditionalExpression)
                |   ExpressionType.Constant             ->  visitConstant(exp :?> ConstantExpression)
                |   ExpressionType.Parameter            ->  visitParameter(exp :?> ParameterExpression)
                |   ExpressionType.MemberAccess         ->  visitMemberAccess(exp :?> MemberExpression)
                |   ExpressionType.Call                 ->  visitCall(exp :?> MethodCallExpression)
                |   ExpressionType.Lambda               ->  visit (exp :?> LambdaExpression).Body
                |   ExpressionType.New                  ->  visitGeneric(exp :?> NewExpression)
                |   ExpressionType.NewArrayInit
                |   ExpressionType.NewArrayBounds       ->  visitGeneric(exp :?> NewArrayExpression)
                |   ExpressionType.Invoke               ->  visitGeneric(exp :?> InvocationExpression)
                |   ExpressionType.MemberInit           ->  visitGeneric(exp :?> MemberInitExpression)
                |   ExpressionType.ListInit             ->  visitGeneric(exp :?> ListInitExpression)
                |   _                                   ->  ExpNode.Unknown
        else
            ExpNode.Unknown

    // -----------------------------------------------------------------------------------------------------------------

    let getQueryElementType (exp  : ConstantExpression) = (exp.Value :?> IQueryable<_>).ElementType

    let expStmt = 

        // recursively build up a structure with indicates the high level expression components        
        let rec evalQuery (expStmt : ExpStatement) (exp : Expression) = 
            match exp.NodeType with
            |   ExpressionType.Constant ->
                    { expStmt with 
                            EntityType = (getQueryElementType (exp :?> ConstantExpression)) }

            |   ExpressionType.Quote ->
                    
                    doSubExpression expStmt exp

            |   ExpressionType.Call ->     
                    let methodCallExp = (exp :?> MethodCallExpression)
                    match methodCallExp.Method.Name with
                    |   "Take"              ->  
                        let expStmt = evalQuery expStmt methodCallExp.Arguments.[0] 
                        { expStmt with Take =   
                                        match (visit methodCallExp.Arguments.[1]) with
                                        |   ExpNode.ConstantObj top when top.GetType() = typeof<int> ->  

                                                let top = top :?> int

                                                if top > 0 && top <= 1000 
                                                    then    Some top
                                                    else    Msg.Raise (Msg.errTakeValueOutOfRange (top.ToString()))

                                        |   _  as error -> 
                                            Msg.Raise (Msg.errTakeValueOutOfRange (error.ToString())) } 
                                            // should not really get here
                                                
                    |   "FirstOrDefault"    ->  

                        methodCallExp.Arguments 
                        |> Seq.fold 
                                evalQuery { expStmt with Take = Some 1; IsFirstOrDefault = true; }


                    |   "Where" ->  

                        doWhere expStmt (methodCallExp.Arguments)

                    |   "Select"    ->  

                        // evaluate the constant node, to get the Element Type
                        // store the next node for casting back 
                        let expStmt'  = evalQuery expStmt (methodCallExp.Arguments.[0]) 
                        
                        // use the operand to eliminate indirect
                        { expStmt' with 
                            ExpSelect = (methodCallExp.Arguments.[1] :?> UnaryExpression).Operand }

                    |   _   ->  

                        expStmt

            |   _   ->   
                    expStmt   
        and 
            doSubExpression expStmt (exp : Expression) = 

            let arg2 = visit exp

            if expStmt.ExpNodeTree = ExpNode.Unknown then 
                { expStmt with ExpNodeTree = arg2 }
            else
                { expStmt with ExpNodeTree = ExpNode.And (expStmt.ExpNodeTree, arg2) } 

        and 
            doWhere expStmt (arguments : ReadOnlyCollection<Expression>) = 

                doSubExpression
                    (evalQuery expStmt arguments.[0]) 
                    arguments.[1]


        let expStmt =   {
                            EntityType          = typeof<'a>;           // default to the query type which is the 
                                                                        // same as the entity type unless modified in
                                                                        // the 'select'
                            ExpNodeTree         = ExpNode.Unknown;
                            ExpSelect           = null; 
                            Take                = None; 
                            IsFirstOrDefault    = false;
                        }

        if exp <> null 
            then    evalQuery expStmt exp 
            else    expStmt

    // -----------------------------------------------------------------------------------------------------------------

    static let reflectorGeneric = (typeof<Reflector<_>>)
                                            .GetGenericTypeDefinition()

    // create a reflector for the actual entity type (mapped to azure table storage).
    let typeReflector =   
                (reflectorGeneric
                    .MakeGenericType(expStmt.EntityType)
                    .GetMember("Get", BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Static).[0]
                    :?> PropertyInfo).GetValue(null, [||])
                                                :?> TypeReflector

    // increases a string value by incrementing the last character
    // if the last character is the max value, increments the next to last recursively
    // example, if char set is from A to Z, 
    //      ABC becomes ABD.
    //      ZZZ becomes AAAA
    let strInc (value : string) = 
        
        let sb = new StringBuilder()

        let rec strInc' (chars : char list) (inc : bool) = 
            
            match chars with 
            |   []          ->  match inc with
                                |   true    ->  sb.Append '\u0000' |> ignore
                                |   false   ->  ()
            |   ch :: tl    ->  match inc with
                                |   true    ->  let newChar = char ((uint16 ch) + 1us)
                                                sb.Append (newChar.ToString()) |> ignore
                                                strInc' tl (newChar = '\u0000')
                                |   false   ->  sb.Append ch |> ignore
                                                strInc' tl false
            
        // cast .NET List to F# list
        strInc' (value.ToCharArray().Reverse() |> List.ofSeq) true

        new String(sb.ToString().Reverse().ToArray())
    
    let strQuotedInc (value : string) = 
    
        "'" + strInc (value.Substring(1, value.Length - 2)) + "'"

    let doUnderlyingTypeMatch ty1 ty2 =
        MorphIntrinsic.getUnderlyingPrimitiveType ty1 = MorphIntrinsic.getUnderlyingPrimitiveType ty2

    let constantObjToMorphedString (o : obj) (istash : IStash) = 

        // if type on the lhs and rhs do not match, try converting the rhs
        let o = 
            try
                if doUnderlyingTypeMatch istash.MemberType (o.GetType()) |> not 
                    then    
                            try 
                                Convert.ChangeType(
                                                o, 
                                                (MorphIntrinsic.getUnderlyingPrimitiveType istash.MemberType))
                                |>  istash.Morpher.Into
                            with
                            |   _   -> o                           
                    else
                            o
                            |>  istash.Morpher.Into       

                // no underlying match implies mismatch type which may be resolved by implicit casting
                // current strategy is to attempt a conversion. If successful invoke the lhs morph on the rhs
                // such that the query comparison is performed on the how the lhs is morphed to storage.
            with
            // it is unexpected to raise this error but just in case
            |   _   as  ex  -> Msg.RaiseInner (Msg.errQueryTypeMismatch "") ex

        match o.GetType() with
        |   x when x = typeof<bool>     ->  BoolToString (o :?> bool)
        |   x when x = typeof<int>      ->  (o :?> int).ToString()
        |   x when x = typeof<int64>    ->  (o :?> int64).ToString()
        |   x when x = typeof<double>   ->  (o :?> double).ToString()
        |   x when x = typeof<DateTime> ->  sprintf "datetime'%s'"
                                                (ObjectConverter.dateTimeToString (o :?> DateTime))
        |   x when x = typeof<Guid>     ->  sprintf "guid'%s'"
                                                ((o :?> Guid).ToString())
        |   x when x = typeof<string>   ->  sprintf
                                                "'%s'"
                                                ((o :?> string).Replace("'", "''")) 
        |   _                           ->  o.ToString()
        

    let constantObjToString (o : obj) = 

        let ty = MorphIntrinsic.getUnderlyingPrimitiveType (o.GetType())

        match ty with
        |   x when x = typeof<bool>     ->  BoolToString (o :?> bool)
        |   x when x = typeof<int>      ->  (o :?> int).ToString()
        |   x when x = typeof<int64>    ->  (o :?> int64).ToString()
        |   x when x = typeof<double>   ->  (o :?> double).ToString()
        |   x when x = typeof<DateTime> ->  sprintf "datetime'%s'"
                                                (ObjectConverter.dateTimeToString (o :?> DateTime))
        |   x when x = typeof<Guid>     ->  sprintf "guid'%s'"
                                                ((o :?> Guid).ToString())
        |   x when x = typeof<string>   ->  sprintf
                                                "'%s'"
                                                ((o :?> string).Replace("'", "''")) 
                
        |   x when x = typeof<byte>     ->  ((ObjectConverter.toTableType(o :?> byte)   |> snd).ToString()) 
        |   x when x = typeof<sbyte>    ->  ((ObjectConverter.toTableType(o :?> sbyte)  |> snd).ToString())
        |   x when x = typeof<int16>    ->  ((ObjectConverter.toTableType(o :?> int16)  |> snd).ToString())
        |   x when x = typeof<uint16>   ->  ((ObjectConverter.toTableType(o :?> uint16) |> snd).ToString())
        |   x when x = typeof<uint32>   ->  ((ObjectConverter.toTableType(o :?> uint32) |> snd).ToString())
        |   x when x = typeof<uint64>   ->  ((ObjectConverter.toTableType(o :?> uint64) |> snd).ToString())
        |   x when x = typeof<char>     ->  ((ObjectConverter.toTableType(o :?> char)   |> snd).ToString())

        |   _                           ->  o.ToString()
                                            // to do ... throw exception
            

    let expTreeToString (expStmt :  ExpStatement) = 

        let sb = new StringBuilder()

        let concat (x: string) = sb.Append x |> ignore

        let rec build node = 
            match node with
                |   ExpNode.Property name       ->  concat (typeReflector.Mapper.MemberNameToPropertyName name)   
                |   ExpNode.Field name          ->  concat (typeReflector.Mapper.MemberNameToPropertyName name)     

                |   ExpNode.ConstantObj o       ->  concat (constantObjToString o)
                |   ExpNode.ConstantObjMorphed 
                        (o, istash)             ->  concat (constantObjToMorphedString o istash)

                |   ExpNode.And (l, r)          ->  binary Op.And    l r
                |   ExpNode.Or  (l, r)          ->  binary Op.Or     l r

                |   ExpNode.Eq  (l, r)          ->  compare Op.Eq    l r
                |   ExpNode.Ne  (l, r)          ->  compare Op.Ne    l r
                |   ExpNode.Lt  (l, r)          ->  compare Op.Lt    l r
                |   ExpNode.Le  (l, r)          ->  compare Op.Le    l r
                |   ExpNode.Gt  (l, r)          ->  compare Op.Gt    l r
                |   ExpNode.Ge  (l, r)          ->  compare Op.Ge    l r

                |   _                           ->  ()

        and binary op l r =

            sb.Append "("   |>  ignore
            build l

            // if lhs is a field or property and rhs is a constant object
            match (l,r) with
            |   (ExpNode.Field name, ExpNode.ConstantObj o)
            |   (ExpNode.Property name, ExpNode.ConstantObj o)  ->

                match typeReflector.Mapper.MemberNameToMemberMappingType name with
                // compare to a key
                |   Some (Key istash)
                        ->  
                            // if the operation is an == and the length is less than the key length then generate
                            // a range test.
                            let rValue = constantObjToMorphedString o istash
                            let isRange =   (op = Op.Eq 
                                            &&  (istash.KeyMediator.IsCompleteKeyValue 
                                                                        (quotedStringValue rValue) |> not)) 

                            if isRange then
                                sb.AppendFormat(" {0} ", Op.Ge)  |> ignore
                                concat rValue

                                sb.AppendFormat(" {0} ", Op.And) |> ignore
                                
                                build l
                                sb.AppendFormat(" {0} ", Op.Lt)  |> ignore
                                concat (strQuotedInc rValue)
                            else        
                                // reverse the op if reverse collation
                                let op =    if istash.Morpher.IsCollationEquivalent
                                                then    op
                                                else    Op.reverse op
                                                                   
                                sb.AppendFormat(" {0} ", op)  |> ignore
                                concat rValue
 
                // compare to a non key
                |   Some (Data istash) 
                        ->  
                            //just output the operation and rhs
                            sb.AppendFormat(" {0} ", op)  |> ignore
                            concat (constantObjToMorphedString o istash)                                            

                                
                |   None    
                        ->
                            sb.AppendFormat(" {0} ", op)  |> ignore
                            build r // if property is not found, we should throw an exception?
    
            |   _   
                    ->
                        sb.AppendFormat(" {0} ", op)  |> ignore
                        build r

            sb.Append ")"   |>  ignore

        and compare op l r =
                
            // check if tree looks like this
            //                              op
            //  CompareTo Member Constant       Constant 0 
            match l with
            |   ExpNode.CompareTo (cl, cr)    
                    ->  // validate that r is a 0 constant
                        match r with
                        |   ExpNode.ConstantObj i when i.GetType() = typeof<int>     
                                ->  let i = i :?> int
                                    if i = 0 then
                                        build cl
                                        sb.AppendFormat(" {0} ", op)  |> ignore
                                        build cr
                                    else
                                        Msg.Raise (Msg.errCompareToOperandInvalid (i.ToString())) 
                        |   _
                                ->  Msg.Raise (Msg.errUnableToParseLinqQuery (expStmt.ExpNodeTree.ToString()))
                                                        
            |   _   
                    ->  binary op l r

        build expStmt.ExpNodeTree

        let filter = sb.ToString()

        let cmds = 
            seq {
                if filter.Is then
                    yield "$filter=" + HttpUtility.UrlEncode filter        

                match expStmt.Take with 
                    |   Some i  ->  yield "$top=" + i.ToString()
                    |   _       ->  ()
            }

        String.concat "&" cmds

    // -----------------------------------------------------------------------------------------------------------------

    let filter = expTreeToString expStmt

    // -----------------------------------------------------------------------------------------------------------------

    member this.EntityType 
        with get() = expStmt.EntityType

    member this.ExpSelect
        with get() = expStmt.ExpSelect

    member this.Command 
        with get() = sb.ToString()

    member this.Filter 
        with get() = filter

    member this.Take
        with get() =    match expStmt.Take with Some take -> take | _ -> 0

    member this.IsFirstOrDefault 
        with get() = expStmt.IsFirstOrDefault

    member this.CreateResultInstance() = 

        Activator.CreateInstance(
                            typeof<List<_>>
                                    .GetGenericTypeDefinition()
                                    .MakeGenericType([|expStmt.EntityType|]))   
            :?> IList

    member this.TypeReflector
        with get() = typeReflector
