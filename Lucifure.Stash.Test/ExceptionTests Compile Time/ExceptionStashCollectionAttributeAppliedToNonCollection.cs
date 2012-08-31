using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class StashCollectionAttributeAppliedToNonCollection
											:	KeyDataImplicit
	{
			[StashCollection]
			public
			int									NotACollection { get; set; }
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
		ExceptionStashCollectionAttributeAppliedToNonCollection() 
		{
			Common<StashCollectionAttributeAppliedToNonCollection>(
					StashError.StashCollectionAttributeAppliedToNonCollection);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
