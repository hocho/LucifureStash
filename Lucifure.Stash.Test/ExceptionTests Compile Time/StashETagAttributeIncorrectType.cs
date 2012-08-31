using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class StashETagAttributeIncorrectType	:	KeyDataExplicit
	{
			[StashETag]
			public
			int									ETag;
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
		ExceptionStashETagAttributeIncorrectType() 
		{
			Common<StashETagAttributeIncorrectType>(StashError.StashETagAttributeIncorrectType);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
