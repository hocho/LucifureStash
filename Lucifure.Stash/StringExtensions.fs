// Copyright (c) Code Superior, Inc. All rights reserved. See License.txt in the project root for license information. 

namespace CodeSuperior.Common

module internal StringExtensions = 

    let BoolToString b =    if b 
                            then "true"
                            else "false"

    type System.String with 

        member this.LeftOf (substring: string) = 
            
            let idx = this.IndexOf substring

            if idx = -1 
                then this
                else this.Substring(0, idx)

        member this.Is  
            with get() = System.String.IsNullOrWhiteSpace(this) |> not

        member this.StartsWithSpace
            with get() = this <> null && this.StartsWith(" ")

        member this.EndsWithSpace
            with get() = this <> null && this.EndsWith(" ")
            
        member this.IsAllSpaces
            with get() = this <> null 
                    && this.Length > 0 
                    && this.ToCharArray() |> Seq.forall ((=) ' ')
                    
        member this.HasLeadingOrTrailingSpaces
            with get() = 
                    this <> null 
                        && (this.StartsWith(" ") || this.EndsWith(" "))
