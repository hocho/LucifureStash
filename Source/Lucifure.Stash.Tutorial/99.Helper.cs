using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSuperior.Lucifure.Tutorial
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Before running any test, Modify App.Config appropriately
	/// </summary>
	public
	static  
	class StashHelper
	{
		public
		static 
		bool
		DictionaryEquals(
			IDictionary<string, object>			lhs,
			IDictionary<string, object>			rhs)
		{
			// skip the ETag value because it differs
			var 
			keysLhs = lhs.Where(x => x.Key != Literal.ETag).OrderBy(x => x.Key).ToList();

			var
			keysRhs = rhs.Where(x => x.Key != Literal.ETag).OrderBy(x => x.Key).ToList();

			return keysLhs.Count() == keysRhs.Count() 
				&& keysLhs.All(x => x.Value.ToString().Equals(rhs[x.Key].ToString())	// values are the same
					&& x.Value.GetType() == rhs[x.Key].GetType());						// types are the same
		}
	}


}
