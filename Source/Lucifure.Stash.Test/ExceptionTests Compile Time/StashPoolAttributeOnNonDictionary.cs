using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class StashPoolAttributeOnNonDictionary
											:	KeyDataExplicit
	{
			[StashPool]
			public
			Dictionary<string, int>				Map;
	}	

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	partial 
	class TestForExceptions 
	{
		[TestMethod]
		public 
		void 
		ExceptionStashPoolAttributeOnNonDictionary() 
		{
			Common<StashPoolAttributeOnNonDictionary>(StashError.StashPoolAttributeOnNonDictionary);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
