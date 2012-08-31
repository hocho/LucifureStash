using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class StashTimestampAttributeIncorrectType
											:	KeyDataExplicit
	{
			[StashTimestamp]
			public
			string								Timestamp;
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
		ExceptionStashTimestampAttributeIncorrectType() 
		{
			Common<StashTimestampAttributeIncorrectType>(StashError.StashTimestampAttributeIncorrectType);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
