using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Lucifure.Stash.Test.Continuation
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	partial
	class TestContinuation
	{
		void 
		ContinuationTokenFull<T>(
			string								partitionKey,
			int									count)
		where 
			T								:	IDataHelper<T>, new()
		{
			TestGeneric.DeleteMultiple<T>(partitionKey);
						
			TestGeneric.FullMultiple<AllDataImplicit>(count, partitionKey, false);

			WithContinuationTokenRead<AllDataImplicit>(partitionKey, count);
		}

		void 
		WithContinuationTokenRead<T>(
			string								partitionKey,
			int									count)
		where 
			T								:	IDataHelper<T>, new()
		{
			var query = TestGeneric.GetQuery<AllDataImplicit>(partitionKey, null);
		
			int readCount = query.ToList().Count;

			Assert.IsTrue(readCount == count);
		}

		void 
		WithContinuationTokenReadN<T>(
			string								partitionKey,
			int									count)
		where 
			T								:	IDataHelper<T>, new()
		{
			var query = TestGeneric.GetQuery<AllDataImplicit>(partitionKey, null);
		
			int readCount = query.Take(count).ToList().Count;

			Assert.IsTrue(readCount == count);
		}

	
		[TestMethod]
		public 
		void ContinuationTokenFull()
		{
			ContinuationTokenFull<AllDataImplicit>(
												"ContinuationTokenTest", 
												1010);						
		}

		[TestMethod]
		public 
		void ContinuationTokenRead()
		{
			WithContinuationTokenRead<AllDataImplicit>(
												"ContinuationTokenTest", 
												1010);						
		}

		[TestMethod]
		public 
		void ContinuationTokenReadN()
		{
			WithContinuationTokenReadN<AllDataImplicit>(
												"ContinuationTokenTest", 
												1000);							
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}