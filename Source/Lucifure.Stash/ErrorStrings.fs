// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Lucifure

open System
open Microsoft.FSharp.Core


// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

module StashError =

    // -----------------------------------------------------------------------------------------------------------------

    let internal errNumBaseCompiletime = 1000

    let StashClientCompiletime                          = errNumBaseCompiletime     +   0
    let StashAttributeInImplicitMode                    = errNumBaseCompiletime     +   1
    let InvalidEntitySetName                            = errNumBaseCompiletime     +   2
    let InvalidTablePropertyName                        = errNumBaseCompiletime     +   3
    let DuplicateTablePropertyName                      = errNumBaseCompiletime     +   4
    let DuplicateTypeMemberName                         = errNumBaseCompiletime     +   5
    let StashTimestampAttributeIncorrectType            = errNumBaseCompiletime     +   6
    let StashETagAttributeIncorrectType                 = errNumBaseCompiletime     +   7
    let StashAttributeMultiple                          = errNumBaseCompiletime     +   8
    let StashPoolAttributeOnNonDictionary               = errNumBaseCompiletime     +   9
    let UnsupportedDataTypeForMorph                     = errNumBaseCompiletime     +   10
    let UnableToCreateIMorphInstance                    = errNumBaseCompiletime     +   11
    let DoesNotImplementIMorph                          = errNumBaseCompiletime     +   12
    let KeyNotDefined                                   = errNumBaseCompiletime     +   13
    let MultipleStashPoolAttributes                     = errNumBaseCompiletime     +   14
    let StashCollectionAttributeAppliedToNonCollection  = errNumBaseCompiletime     +   15
    
    // -----------------------------------------------------------------------------------------------------------------

    let internal errNumBaseRuntime = 2000

    let UnexpectedRuntime                               = errNumBaseRuntime         +   0
    let ConstantExpressionObject                        = errNumBaseRuntime         +   1
    let RowKeyTooLarge                                  = errNumBaseRuntime         +   2
    let UnsupportedDataTypeOnWrite                      = errNumBaseRuntime         +   3
    let UnsupportedDataTypeOnRead                       = errNumBaseRuntime         +   4
    let UnsupportedConverter                            = errNumBaseRuntime         +   5
    let IncorrectDataContractMorpherType                = errNumBaseRuntime         +   6
    let TableAlreadyExists                              = errNumBaseRuntime         +   8
    let TableNotFound                                   = errNumBaseRuntime         +   9
    let ReceiveTypeMismatch                             = errNumBaseRuntime         +   10
    let TakeValueOutOfRange                             = errNumBaseRuntime         +   11
    let VectorNoIndex                                   = errNumBaseRuntime         +   12
    let VectorInvalidIndex                              = errNumBaseRuntime         +   13
    let ScalarVectorMismatch                            = errNumBaseRuntime         +   14
    let CannotMergeData                                 = errNumBaseRuntime         +   15
    let MissingMembersInType                            = errNumBaseRuntime         +   16
    let CompareToOperandInvalid                         = errNumBaseRuntime         +   17
    let UnableToParseLinqQuery                          = errNumBaseRuntime         +   18
    let QueryAgainstNull                                = errNumBaseRuntime         +   19
    let QueryTypeMismatch                               = errNumBaseRuntime         +   20
    let ETagNotDefined                                  = errNumBaseRuntime         +   21
    let ETagMatchFailed                                 = errNumBaseRuntime         +   22
    let BatchCommitFailed                               = errNumBaseRuntime         +   23
    let ItemAlreadyExistsInContext                      = errNumBaseRuntime         +   24
    let StashAggregate                                  = errNumBaseRuntime         +   25

    // -----------------------------------------------------------------------------------------------------------------
    // these exceptions are not expected to be thrown and signify that some assumptions made by the product are incorrect

    let internal errNumBaseRuntimeInternal = 3000

    let ExpectedVector                                  =   errNumBaseRuntimeInternal +   1   
    let ExpectedETag                                    =   errNumBaseRuntimeInternal +   2
    let UnsupportedMemberType                           =   errNumBaseRuntimeInternal +   3

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type StashMessage
        (error                              :   int
        ,message                            :   string) = 

    member this.Message
        with get() = message

    member this.Error
        with get() = error

// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type StashException
        (message                            :   StashMessage 
        ,? innerException                    :   exn) =

    inherit ApplicationException(
                message.Message,  
                match innerException with Some x -> x | _ -> null)

    member this.Error with get() = message.Error


// ---------------------------------------------------------------------------------------------------------------------
// ---------------------------------------------------------------------------------------------------------------------

type StashCompiletimeException(messages : StashMessage array) =
    inherit StashException(
                new StashMessage(
                    StashError.StashClientCompiletime,
                    "Stash Compile Time error. Examine the Messages property for a list of all errors."))

    member this.Messages with get() = messages

// ---------------------------------------------------------------------------------------------------------------------

type StashRuntimeException(message : StashMessage, ? innerException : exn) =
    inherit StashException(
                message,  
                match innerException with Some x -> x | _ -> null)

// ---------------------------------------------------------------------------------------------------------------------
 
type StashAggregateException(containedExceptions : exn array) =
    inherit StashException(
                new StashMessage(
                    StashError.StashAggregate,
                    "Stash Aggregate Exception. See contained exceptions for more details."))
                
    
    member this.ContainedExceptions with get() = containedExceptions

// ---------------------------------------------------------------------------------------------------------------------


module internal Msg = 

    let RaiseCompiletime msgs                       = raise (StashCompiletimeException msgs)

    let Raise (msg : StashMessage)                  = raise (StashRuntimeException msg)

    let RaiseInner (msg : StashMessage) (ex : exn)  = raise (StashRuntimeException (msg, ex))

    // -----------------------------------------------------------------------------------------------------------------
    // compile time errors 

    let errStashAttributeInImplicitMode = 
            StashMessage(
                StashError.StashAttributeInImplicitMode,
                "The Stash attribute is not allowed in Implicit Mode. Ensure that the StashEntity attribute is applied \
                    to the type in Hybrid or Explicit mode. Note that the StashEntity attribute is not inherited.")

    let errInvalidEntitySetName name pattern = 
            StashMessage(
                StashError.InvalidEntitySetName,
                sprintf 
                    "Entity Set name '%s' is not valid. It must match the regular expression pattern '%s' \
                        and must be 3 to 63 characters long."
                    name
                    pattern)

    let errInvalidTableProperty name = 
            StashMessage(
                StashError.InvalidTablePropertyName,
                sprintf 
                    "An embedded '_' in the table property name '%s' is not supported, for implicitly declared members. \
                        This is necessary for backwards capability since '_' in used internally by Stash to \ 
                        demarcate oversized string and byte[] values as well as to implement collection indexes. \
                        \nEither change the member name or override using the Name property of the Stash attribute."
                    name)

    let errDuplicateTablePropertyName name = 
            StashMessage(
                StashError.DuplicateTablePropertyName,
                sprintf 
                    "Duplicate table property with name '%s'. Table property names must be unique." 
                    name)

    let errDuplicateTypeMemberName name = 
            StashMessage(
                StashError.DuplicateTypeMemberName,
                sprintf 
                    "Duplicate type member with name '%s'. It is possible that a member in the derived \
                        type has the same name as a private or protected member in the derived type."
                    name)

    let errMultipleStashPoolAttributes = 
            StashMessage(
                StashError.MultipleStashPoolAttributes,
                "Only a single member in a type can be decorated with the StashPool attribute.")
    

    let errStashTimestampAttributeIncorrectType name =
            StashMessage(
                StashError.StashTimestampAttributeIncorrectType,
                sprintf 
                    "The member '%s' annotated with a StashTimestamp attribute must be of type DateTime."
                    name)

    let errStashETagAttributeIncorrectType name =
            StashMessage(
                StashError.StashETagAttributeIncorrectType,
                sprintf 
                    "The member '%s' annotated with a StashETag attribute must be of type String."
                    name)

    let errStashAttributeMultiple name =
            StashMessage(
                StashError.StashAttributeMultiple,
                (sprintf 
                    "More than one Stash attribute was used to annotate the member '%s'." 
                    name))

    let errStashPoolAttributeOnNonDictionary name =
            StashMessage(
                StashError.StashPoolAttributeOnNonDictionary,
                (sprintf 
                    "The StashPool attribute is applied to a member '%s', which does not support the \
                        IDictionary<string, object> interface."
                    name))

    let errUnsupportedDataTypeForMorph typeMorph typeData = 
            StashMessage(
                StashError.UnsupportedDataTypeForMorph,
                sprintf 
                    "The Morph type supplied '%s' does not support morphing type '%s'."
                    (typeMorph.ToString())
                    (typeData.ToString()))

    let errUnableToCreateIMorphInstance ty msg =  
            StashMessage(
                StashError.UnableToCreateIMorphInstance,
                sprintf 
                    "Error on creating an instance of the type %s. It is possible that there is no public default constructor. \
                        Exception Message: %s"  
                    (ty.ToString())    
                    msg)
    
    let errDoesNotImplementIMorph ty =  
            StashMessage(
                StashError.DoesNotImplementIMorph,
                sprintf 
                    "The type specified %s, does not implement the IMorph interface." (ty.ToString()))

    let errKeyNotDefined keyName = 
            StashMessage(
                StashError.KeyNotDefined,
                sprintf 
                    "%s not defined in the type."       
                    keyName)

    let errStashCollectionAttributeAppliedToNonCollection name = 
            StashMessage(
                StashError.StashCollectionAttributeAppliedToNonCollection,
                sprintf 
                    "The StashCollection attribute can only be applied to a stash recognized collection. \
                    That is, it implements the IList interface. Ensure that member '%s' is a valid collection."       
                    name)


    // -----------------------------------------------------------------------------------------------------------------
    // Runtime errors

    let errUnexpectedRuntime = 
            StashMessage(
                StashError.UnexpectedRuntime,
                "Unexpected runtime error. Please examine the InnerException for more information.")

    let errTakeValueOutOfRange value = 
            StashMessage(
                StashError.TakeValueOutOfRange,
                sprintf 
                    "The value '%s' specified in the Take method should be in the range 1 thru 1000."
                    value)

    let errUnsupportedDataTypeOnWrite ty = 
            StashMessage(
                StashError.UnsupportedDataTypeOnWrite,
                sprintf
                    "Unsupported table storage data type '%s', on write. \
                    This may occur if a collection of objects contains a non azure table storage data type." 
                    (ty.ToString()))

    let errUnsupportedDataTypeOnRead edmType = 
            StashMessage(
                StashError.UnsupportedDataTypeOnRead,
                sprintf
                    "Unsupported table storage data type '%s', on read." 
                    edmType)

    let errUnsupportedConverter ty = 
            StashMessage(
                StashError.UnsupportedConverter,
                sprintf
                    "Unsupported convert of type '%s' requested." 
                    (ty.ToString()))

    let errIncorrectDataContractMorpherType ty = 
            StashMessage(
                StashError.IncorrectDataContractMorpherType,
                sprintf 
                    "Data Contract Morpher expects a string object, not an object of type '%s'" 
                    (ty.ToString()))


    let errTableAlreadyExists name = 
            StashMessage(
                StashError.TableAlreadyExists,
                sprintf 
                    "Table '%s' already exists or is being deleted." 
                    name)

    let errTableNotFound name = 
            StashMessage(
                StashError.TableNotFound,
                sprintf 
                    "Table '%s' not found." 
                    name)

    let errReceiveTypeMismatch memberName tyExpected tyReceived = 
            StashMessage(
                StashError.ReceiveTypeMismatch,
                sprintf 
                    "Setting member '%s', expected type '%s' but received type '%s'."
                    memberName
                    (tyExpected.ToString())
                    (tyReceived.ToString()))

    let errVectorNoIndex = 
            StashMessage(
                StashError.VectorNoIndex,
                "List Value specified without index")

    let errVectorInvalidIndex = 
            StashMessage(
                StashError.VectorInvalidIndex,
                "List Value specified without a valid index")

    let errScalarVectorMismatch name =
            StashMessage(
                StashError.ScalarVectorMismatch,
                sprintf 
                    "Table property '%s' is scalar but returned vector."
                    name)

    let errCannotMergeData data = 
            StashMessage(
                StashError.CannotMergeData,
                sprintf 
                    "Cannot merge data of type '%s'. Only string and byte[] supported. Merge data is '%s'."       
                    (data.GetType().ToString())
                    (data.ToString()))


    let errRowKeyTooLarge keyName = 
            StashMessage(
                StashError.RowKeyTooLarge,
                sprintf 
                    "%s is too large."       
                    keyName)

    let errConstantExpressionObject = 
            StashMessage(
                StashError.ConstantExpressionObject,
                "Constant Expression Object not supported.")       

    let errMissingMembersInType properties = 
            StashMessage(
                StashError.MissingMembersInType,
                sprintf 
                    "The following properties in the Azure table row do not have corresponding member in the type definition.\
                    %s. Either define the members, define a StashPool or set the IgnoreMissingProperties to true."
                    properties)       

    let errCompareToOperandInvalid (operand : string) = 
            StashMessage(
                StashError.CompareToOperandInvalid,
                sprintf 
                    "CompareTo applied to value '%s', \
                    Can only evaluate a query where the 'CompareTo' operand is '0'."
                    operand)       
                    
    let errUnableToParseLinqQuery query = 
            StashMessage(
                StashError.UnableToParseLinqQuery,
                sprintf 
                    "Unable to parse LINQ query '%s'."
                    query)       

    let errQueryAgainstNull query = 
            StashMessage(
                StashError.UnableToParseLinqQuery,
                sprintf 
                    "Azure table storage cannot be queried against a null value. '%s'."
                    query)       

    let errQueryTypeMismatch x = 
            StashMessage(
                StashError.QueryTypeMismatch,
                sprintf 
                    "The type of the left hand side and right hand side of each segment of a query expression must match.%s"
                    x)       

    let errETagNotDefined typeName = 
            StashMessage(
                StashError.ETagNotDefined,
                sprintf 
                    "Type '%s' does not contain an ETag member. If ETag matching is required for any update and delete like \
                    request, please define a string member decorated with the StashETag attribute."
                    typeName)       

    let errETagMatchFailed eTag = 
            StashMessage(
                StashError.ETagMatchFailed,
                sprintf 
                    "The ETag supplied '%s' failed to match the ETag on the entity. It is possible that another \
                    process updated the entity before this request."
                    eTag)       

    let errBatchCommitFailed errorCode errorMessage = 
            StashMessage(
                StashError.BatchCommitFailed,
                sprintf 
                    "Batch commit failed with Error Code '%s' and Error Message '%s'."
                    errorCode
                    errorMessage)       

    // -----------------------------------------------------------------------------------------------------------------
    // Runtime errors - Unexpected 

    let errUnsupportedMemberType = 
            StashMessage(
                StashError.UnsupportedMemberType, 
                "Only fields and properties supported.")

    let errExpectedVector ty = 
            StashMessage(
                StashError.ExpectedVector,
                sprintf 
                    "Expected Type '%s' to be a vector."       
                    (ty.ToString()))

    let errExpectedETag = 
            StashMessage(
                StashError.ExpectedETag,
                "Expected an ETag value to be present but not found.")


