using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public
	class MorphDummy						:	IStashMorph
	{
		public 
        bool
		CanMorph(
			Type								type)
		{
			return type == typeof(string);
		}

		public 
        bool
        IsCollationEquivalent
		{
			get { return true; }	// indicates that the collating sequence of the original and morphed values are identical  
		}

		public 
		object 
		Into(
			object								value) 
		{
			return value;
		}

		public 
		object 
		Outof(
			object								value) 
		{
			return value;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity]
	public 
	class UnsupportedDataTypeForMorph		:	KeyDataExplicit
	{
			[Stash(Morpher=typeof(MorphDummy))]
			public
			int									ToMorph;
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
		ExceptionUnsupportedDataTypeForMorph() 
		{
			Common<UnsupportedDataTypeForMorph>(StashError.UnsupportedDataTypeForMorph);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
