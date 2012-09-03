using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	class ExceptionsImplicitMultiple_0		:	KeyDataImplicit
	{
			[Stash]
			public
			string								Name				{ get; set; }
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
		ExceptionsImplicitMultiple() 
		{
			Common<ExceptionsImplicitMultiple_0>(
					new int[] { 
						StashError.StashAttributeInImplicitMode,
						StashError.InvalidEntitySetName,
					});
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
