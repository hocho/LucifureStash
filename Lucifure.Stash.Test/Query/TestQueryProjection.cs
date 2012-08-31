using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Query
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	class SimpleType
	{
			public	
			string								PKey;

			public	
			string								RKey;
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	class TestQueryProjection
	{
		StashClient<KeyDataExplicit>	
		GetClientWithPopulate(
			Guid								pKey,
			int									count)
		{
			StashClient<KeyDataExplicit>
			client = StashConfiguration.GetClient<KeyDataExplicit>();
			
			client.CreateTableIfNotExist();

			while(count-- > 0)			
				client.Insert(
							new KeyDataExplicit {
											PartitionKey = pKey.ToString(), 
											RowKey = Guid.NewGuid().ToString() });

			return client;
		}

		[TestMethod]
		public 
		void ProjectToAnonymousType()
		{
			const int 
			count = 3;

			StashClient<KeyDataExplicit>
			client = GetClientWithPopulate(Guid.NewGuid(), count);

			var 
			query =	from x in client.CreateQuery()		
				        select new { pkey = x.PartitionKey, rkey = x.RowKey };

			int 
			deleteCount = 0;

			foreach(var x in query)
			{
				client.Delete(x.pkey, x.rkey);
				++deleteCount;
			}	

			Assert.IsTrue(deleteCount >= count);
		}

		[TestMethod]
		public 
		void ProjectToSelf()
		{
			const int 
			count = 3;

			StashClient<KeyDataExplicit>
			client = GetClientWithPopulate(Guid.NewGuid(), count);

			var 
			query =	from x in client.CreateQuery() 
			            select x;

			int 
			deleteCount = 0;

			foreach(var x in query)
			{
				client.Delete(x.PartitionKey, x.RowKey);
				++deleteCount;
			}	

			Assert.IsTrue(deleteCount >= count);
		}

		[TestMethod]
		public 
		void ProjectToSimpleType()
		{
			const int 
			count = 3;

			StashClient<KeyDataExplicit>
			client = GetClientWithPopulate(Guid.NewGuid(), count);

			var 
			query =	from x in client.CreateQuery() 
						select new SimpleType{ PKey = x.PartitionKey, RKey = x.RowKey};

			int 
			deleteCount = 0;

			foreach(var x in query)
			{
				client.Delete(x.PKey, x.RKey);
				++deleteCount;
			}	

			Assert.IsTrue(deleteCount >= count);
		}

	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}