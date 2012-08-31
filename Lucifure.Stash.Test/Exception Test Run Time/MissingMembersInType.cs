using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.ExceptionsRT
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit)]
	public 
	class MissingMembersInType				:	KeyDataExplicit
	{
			[Stash]
			public
			int									Int0;

			[Stash]
			public
			int									Int1;

			[Stash]
			public
			int									Int2;
	}	

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public partial class TestForExceptionsRT 
	{
		void
		MissingMembersInTypeImpl(
			bool								ignoreMissingProperties) 
		{
			StashClient<MissingMembersInType>	clientWithExtra = null;
			MissingMembersInType				dataWritten = null;

			try
			{
				clientWithExtra = StashConfiguration.GetClient<MissingMembersInType>();		
			
				clientWithExtra.CreateTableIfNotExist();

				var
				clientMin = StashConfiguration.GetClient<KeyDataExplicit>(
									new StashClientOptions {
											IgnoreMissingProperties = ignoreMissingProperties,
											OverrideEntitySetName = instance => typeof(MissingMembersInType).Name,
											OverrideEntitySetNameIsDynamic = false,
										});		
				
				dataWritten = new MissingMembersInType {
													PartitionKey = "MissingMember",
													RowKey = Guid.NewGuid().ToString(),
													Int0 = 0,
													Int1 = 1,
													Int2 = 2 };

				clientWithExtra.Insert(dataWritten);

				var
				dataRead = clientMin.Get(dataWritten.PartitionKey, dataWritten.RowKey);

				// we should reach here only if ignoring missing properties
				Assert.IsTrue(ignoreMissingProperties);
			}
			catch (StashException stashEx)
			{
				Assert.IsTrue(
					!ignoreMissingProperties 
						&& stashEx.Error == StashError.MissingMembersInType);
			}
			catch (Exception ex)
			{
				Assert.Fail();
			}

			clientWithExtra.Delete(dataWritten);
		}

		[TestMethod]
		public 
		void 
		MissingMembersInType()
		{
			MissingMembersInTypeImpl(true);		// ignoreMissingProperties - no throw exception
			MissingMembersInTypeImpl(false);	// do not ignore - throws exception	
		} 
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
