using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class ExceptionsExplicitMultiple
	{
			[StashPartitionKey]
			public
			int[]								PartitionKey { get; set; }

			[StashRowKey]
			public
			int[]								RowKey { get; set; }

			[Stash]
			public
			string								Name_Full {get; set; }

			[Stash]
			public
			string								Name;

			[Stash(Name="Name")]
			public
			string								Name2;


			[StashPool]
			public
			Dictionary<string, object>			Dict1;

			[StashPool]
			public
			Dictionary<string, object>			Dict2;

			[StashTimestamp]
			public
			string								Timestamp;

			[StashETag]
			public
			int									ETag;

			[StashPool]
			public
			Dictionary<string, int>				Map;

			[Stash(Morpher=typeof(MorphDummy))]
			public
			int									ToMorph;

			[Stash(Morpher=typeof(MorphPrivate))]
			public
			int									ToMorph2;

			[Stash(Morpher=typeof(MorphImposter))]
			public
			int									ToMorph3;

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
		ExceptionsExplicitMultiple() 
		{
			Common<ExceptionsExplicitMultiple>(
					new int[] { 
						StashError.InvalidTablePropertyName,
						StashError.DuplicateTablePropertyName,
						StashError.MultipleStashPoolAttributes,
						StashError.StashTimestampAttributeIncorrectType,
						StashError.StashETagAttributeIncorrectType,
						StashError.StashPoolAttributeOnNonDictionary,
						StashError.UnsupportedDataTypeForMorph,
						StashError.UnableToCreateIMorphInstance,
						StashError.DoesNotImplementIMorph,
						StashError.StashCollectionAttributeAppliedToNonCollection,
					});
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
