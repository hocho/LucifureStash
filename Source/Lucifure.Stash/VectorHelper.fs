// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open System.Xml
open System.Web
open System.Collections
open System.Collections.Generic
open System.IO
open System.Text
open System.Net
open System.Reflection

open CodeSuperior.Common.StringExtensions
open CodeSuperior.Common.Functional

// ---------------------------------------------------------------------------------------------------------------------

[<AbstractClass>]
type internal IListBase()  = 
   
    abstract member SetValue                :   obj -> int -> unit
    abstract member GetValue                :   int -> obj
    abstract member Add                     :   obj -> unit
    abstract member Count                   :   int with get
    
type internal IListObj(list : obj) = 
    inherit IListBase()

    let list = list :?> IList    

    override this.SetValue item index   =   list.[index] <- item
    override this.GetValue index        =   list.[index]
    override this.Add item              =   list.Add item |> ignore   
    override this.Count with get()      =   list.Count

type internal IListGeneric<'a>(list : obj) = 
    inherit IListBase()
    
    let list = list :?> IList<'a>

    override this.SetValue item index   =   list.[index] <- (item :?> 'a)
    override this.GetValue index        =   upcast list.[index]
    override this.Add item              =   list.Add (if item = null 
                                                        then Unchecked.defaultof<'a> 
                                                        else item :?> 'a) 
                                                |> ignore
    override this.Count with get()      =   list.Count

// ---------------------------------------------------------------------------------------------------------------------
// Reflects over a type and builds tables to use for populating the type

module internal VectorHelper = 

    let setterAllNull = (fun o nameValues -> ())
    let getterAllNull = (fun o -> Seq.empty<NameValue>)              

    let isIList (memberType : Type) = 
                    ReflectionHelper.hasListInterface memberType || ReflectionHelper.hasGenericIListInterface memberType


    let (|IsArray|IsIListGeneric|IsIList|) (ty : Type) = 
        if ty.IsArray 
            then IsArray (ty.GetElementType())
        elif ReflectionHelper.hasGenericIListInterface ty
            then IsIListGeneric (ty.GetGenericArguments().[0])
        elif ReflectionHelper.hasListInterface ty
            then IsIList (typeof<obj>)
        else 
            Msg.Raise (Msg.errExpectedVector ty)


    let getListElementType memberType : Type =
        match memberType with
        |   IsArray x           -> x
        |   IsIListGeneric x    -> x
        |   IsIList x           -> x

    let getSetterGetterImpl
        (memberInfo                             :   MemberInfo)
        (tablePropertyName                      :   string)
        (listWrapper                            :   obj -> IListBase) = 

        let typeList, setter, getter 
            =   match memberInfo.MemberType with
                |   MemberTypes.Field       
                    ->  
                        let fieldInfo = memberInfo :?> FieldInfo
                               
                        //let x (a, b) = fieldInfo.SetValue (a, b)

                        fieldInfo.FieldType,   
                        (fun instance value -> fieldInfo.SetValue(instance, value); value), 
                        fieldInfo.GetValue
                            
                |   MemberTypes.Property    
                    ->  
                        let propInfo = memberInfo :?> PropertyInfo

                        let setMethod   = propInfo.GetSetMethod(true);
                        let getMethod   = propInfo.GetGetMethod(true);
        
                        propInfo.PropertyType,
                        (fun instance value -> setMethod.Invoke(instance, [| value |]) |> ignore; value),
                        (fun instance -> getMethod.Invoke(instance, [||]))

                |   _
                    ->  Msg.Raise Msg.errUnsupportedMemberType // unexpected
        
        let isArray = match typeList with |   IsArray x -> true | _ -> false               

        let getItem instance = getter instance                 

        // Calls the list wrapper to return a wrapper list object, creating one if it does not exist
        let createItem instance (args : obj array) = 
                
            listWrapper
                    (match getItem instance with
                    |   null        ->  setter instance (Activator.CreateInstance(typeList, args))
                    |   _ as item   ->  item)


        let growList (list : IListBase) index = 
            
            let rec grow count =
                if index >= count then
                    list.Add(null) |> ignore
                    grow (count + 1) 

            grow list.Count
            
        // sets all the items of a vector, creates the vector if it does not exist
        let setMember 
            (instance                   :   obj) 
            (nameValues                 :   NameValues)    =

            // sort according to index
            let sorted =  
                nameValues
                |>  Seq.map (fun nameValue -> 
                                    match ObjectComposer.getPrefixSuffix nameValue.Name with
                                    |   _, suffix when suffix.Is    
                                            ->  match Int32.TryParse suffix with
                                                |   true, idx   -> idx, nameValue
                                                |   _           -> Msg.Raise Msg.errVectorInvalidIndex
                                    |   _                    
                                            -> Msg.Raise Msg.errVectorNoIndex
                                                                            
                                )
                |>  Seq.sortBy  (fun (index, nameValue) -> -index)  // sort in descending order for efficiency of list management
                |>  Seq.toList

            let items = createItem 
                            instance 
                            (   if isArray 
                                    then [| fst(sorted.[0]) + 1 |]
                                    else [||])

            sorted |>  Seq.iter (fun (index, nameValue) -> 
                                    growList items index
                                    items.SetValue nameValue.Value index)   

        let getMember 
            (tablePropertyName              :   string)
            (instance                       :   obj) = 

            let list = getItem instance

            if list <> null then
                let items = listWrapper list

                Seq.init 
                        items.Count 
                        (fun index -> 
                                NameValue(
                                        sprintf 
                                            "%s_%03d"
                                            tablePropertyName
                                            index, 
                                        items.GetValue index))
            else
                Seq.empty<NameValue>

        typeList, setMember, getMember tablePropertyName

    let getSetterGetter
        (memberInfo                             :   MemberInfo)
        (tablePropertyName                      :   string) = 
        
        let memberType = ReflectionHelper.getMemberType memberInfo
       
        let listElementType = getListElementType memberType

        // returns a function which creates a wrapper object for manipulating a list
        let wrapper : (obj -> IListBase) = 
            match listElementType with 
            | x when x = typeof<obj>            
                ->  (fun list -> IListObj(list) :> IListBase)
            | _ as x                                
                ->  (fun list -> Activator.CreateInstance(
                                                    typeof<IListGeneric<_>>
                                                        .GetGenericTypeDefinition()
                                                        .MakeGenericType([|x|]),
                                                    [| list |]) :?> IListBase)   

        let typeList, setMember, getMember = 
            getSetterGetterImpl memberInfo tablePropertyName wrapper

        typeList, listElementType, setMember, getMember
