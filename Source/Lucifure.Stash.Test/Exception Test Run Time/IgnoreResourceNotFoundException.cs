using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.ExceptionsRT
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	partial 
	class TestForExceptionsRT 
	{
		void
		IgnoreResouceNotFoundExceptionImpl(
			bool								ignoreResouceNotFoundException) 
		{
			StashClient<KeyDataExplicit>		clientWithExtra = null;
			KeyDataExplicit						dataWritten = null;

			try
			{
				clientWithExtra = StashConfiguration.GetClient<KeyDataExplicit>();		
			
				clientWithExtra.CreateTableIfNotExist();

				var
				clientMin = StashConfiguration.GetClient<KeyDataExplicit>(
									new StashClientOptions {
											IgnoreResourceNotFoundException = ignoreResouceNotFoundException,
											OverrideEntitySetName = instance => typeof(KeyDataExplicit).Name,
											OverrideEntitySetNameIsDynamic = false,
										});		
				
				dataWritten = new KeyDataExplicit {
												PartitionKey = "ResourceNotFound",
												RowKey = Guid.NewGuid().ToString() };

				clientWithExtra.Insert(dataWritten);

				var
				dataRead = clientMin.Get(
										dataWritten.PartitionKey, 
										dataWritten.RowKey + "X");

				// we should reach here only if ignoreResouceNotFoundException is true
				Assert.IsTrue(ignoreResouceNotFoundException);
				Assert.IsTrue(dataRead == null);
			}
			catch (StashException stashEx)
			{
				Assert.IsTrue(
					!ignoreResouceNotFoundException 
						&& stashEx.Error == StashError.UnexpectedRuntime);
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
		IgnoreResouceNotFoundException()
		{
			IgnoreResouceNotFoundExceptionImpl(true);	
			IgnoreResouceNotFoundExceptionImpl(false);	
		} 
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
