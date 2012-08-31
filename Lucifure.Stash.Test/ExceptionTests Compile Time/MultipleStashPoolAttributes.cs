using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class MultipleStashPoolAttributes		:	KeyDataExplicit
	{
			[StashPool]
			public
			Dictionary<string, object>			Dict1;

			[StashPool]
			public
			Dictionary<string, object>			Dict2;
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
		ExceptionMultipleStashPoolAttributes() 
		{
			Common<MultipleStashPoolAttributes>(StashError.MultipleStashPoolAttributes);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
