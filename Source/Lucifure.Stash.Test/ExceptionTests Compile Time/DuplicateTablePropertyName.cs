using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class DuplicateTablePropertyName		:	KeyDataExplicit
	{
			[Stash]
			public
			string								Name;

			[Stash(Name="Name")]
			public
			string								Name2;
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
		ExceptionDuplicateTablePropertyName() 
		{
			Common<DuplicateTablePropertyName>(StashError.DuplicateTablePropertyName);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
