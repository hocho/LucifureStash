using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Context
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	partial 
	class TestContext
	{
		List<T>
		ContextPlace<T>(
			StashContext<T>						context,
			int									count,
			string								partitionKey,
			Action<T>							action)
		where 
			T								:	IDataHelper<T>, new()
		{
			return	Enumerable
						.Range(1, count)
						.Select(
							i =>
								{
									var item = TypeFactory<T>.CreateRandomSmall();

									item.SetPartitionKey(partitionKey);

									action(item);
									
									return item;	
								}).ToList();
		}

		List<T>
		ContextInsert<T>(
			StashContext<T>						context,
			int									count,
			string								partitionKey)
		where 
			T								:	IDataHelper<T>, new()
		{
			return ContextPlace<T>(context, count, partitionKey, context.Insert);
		}

		List<T>
		ContextInsertOrUpdate<T>(
		    StashContext<T>						context,
		    int									count,
			string								partitionKey)
		where 
		    T								:	IDataHelper<T>, new()
		{
		    return ContextPlace<T>(context, count, partitionKey, context.InsertOrUpdate);
		}

		List<T>
		ContextInsertOrMerge<T>(
		    StashContext<T>						context,
		    int									count,
			string								partitionKey)
		where 
		    T								:	IDataHelper<T>, new()
		{
		    return ContextPlace<T>(context, count, partitionKey, context.InsertOrMerge);
		}

		void
		ContextFull<T>(
			string								partitionKey,
			IQueryable<T>						query,
			CommitStrategy						commitStrategy)
		where 
			T								:	IDataHelper<T>, new()
		{
			var client= StashConfiguration.GetClient<T>();

			const int 
			insertCount = 5;

			// insert insertCount and commit
			var context = client.GetContext();

			var insertItems = ContextInsert<T>(context, insertCount, partitionKey);
			
			Assert.IsTrue(context.GetTrackedEntities(EntityState.Inserted).Count == insertCount);

			context.Commit(commitStrategy);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Unchanged).Count == insertCount);

			// ---------------------------------------------------------------------------------------------------------

			const int 
			updateCount = 7;

			// (1) Insert or Update New. No etag needed.
			context = client.GetContext();

			var insertItems2 = ContextInsertOrUpdate<T>(context, 1, partitionKey);

			// (2) Insert or Merge New. No etag needed.
			var insertItems3 = ContextInsertOrMerge<T>(context, 1, partitionKey);

			// (3) Insert or Update Old. No etag needed.
			insertItems[0].UpdateDateTime();
			context.InsertOrUpdate(insertItems[0]);

			// (4) Insert or Merge Old. No etag needed.
			insertItems[1].UpdateDateTime();
			context.InsertOrMerge(insertItems[1]);
		
			// (5) Update Old. ETag needed.
			insertItems[2].UpdateDateTime();
			context.Update(insertItems[2]);

			// (6) Merge Old. ETag needed.
			insertItems[3].UpdateDateTime();
			context.Merge(insertItems[3]);
			
			// (7) Delete Old. ETag needed.
			context.Delete(insertItems[4]);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.InsertedOrUpdated).Count == 2);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.InsertedOrMerged).Count == 2);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Updated).Count == 1);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Merged).Count == 1);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Deleted).Count == 1);
			
			Assert.IsTrue(context.GetTrackedEntities(
												EntityState.InsertedOrUpdated
											|	EntityState.InsertedOrMerged
											|	EntityState.Updated
											|	EntityState.Merged
											|	EntityState.Deleted).Count == updateCount);

			Assert.IsTrue(context.GetTrackedEntities().Count == updateCount);

			context.Commit(commitStrategy);

			Assert.IsTrue(context.GetTrackedEntities().Count == updateCount - 1);
			Assert.IsTrue(context.GetTrackedEntities(EntityState.Unchanged).Count == updateCount - 1);

			// query and delete ... we should have to delete 6 since we inserted 5 + 2 and deleted 1
			var 
			deleteItems = query.ToList();

			int deleteCount = deleteItems.Count;

			Assert.IsTrue(deleteItems.Count == 6);
				
			deleteItems
				.ForEach(
					i => context.Delete(i));

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Deleted).Count == deleteCount);

			context.Commit(commitStrategy);
			
			Assert.IsTrue(context.GetTrackedEntities(EntityState.Unchanged).Count == 0);
		}

		void
		ContextFull<T>(
			string								partitionKey,
			IQueryable<T>						query)
		where 
			T								:	IDataHelper<T>, new()
		{
			CommitStrategy[]
			strategies = new [] { CommitStrategy.Serial, CommitStrategy.Parallel, CommitStrategy.Batch };

			strategies =  strategies.Reverse().ToArray();

			strategies
				.ToList()
				.ForEach(
					i =>	ContextFull<T>(
								partitionKey,
								query,
								i));
		}

		[TestMethod]
		public 
		void 
		AllDataImplicitContextFull()
		{
			// Upsert not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				string 
				partitionKey = Guid.NewGuid().ToString();

				ContextFull<AllDataImplicit>(
					partitionKey,
					StashConfiguration
						.GetClient<AllDataImplicit>()
						.CreateQuery()
						.Where(i => i.PartitionKey == partitionKey));
			}
		}

		[TestMethod]
		public 
		void 
		AllDataExplicitContextFull()
		{
			// Upsert not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				string 
				partitionKey = Guid.NewGuid().ToString();

				ContextFull<AllDataExplicit>(
					partitionKey,
					StashConfiguration
						.GetClient<AllDataExplicit>()
						.CreateQuery()
						.Where(i => i.PartitionKey == partitionKey));
			}
		}


		// -------------------------------------------------------------------------------------------------------------

		void
		ContextQuery<T>(
			string								partitionKey,
			StashContext<T>						context,
			IQueryable<T>						query,
			CommitStrategy						commitStrategy)
		where 
			T								:	IDataHelper<T>, new()
		{
			// Insert
			const int 
			insertCount = 5;

			var insertItems = ContextInsert<T>(context, insertCount, partitionKey);
			
			Assert.IsTrue(context.GetTrackedEntities(EntityState.Inserted).Count == insertCount);

			context.Commit(commitStrategy);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Unchanged).Count == insertCount);


			// Detach a few
			var 
			detachedItems = insertItems.Where((item, idx) => idx % 2 == 1).ToList(); 
				
			int detachCount = detachedItems.Count;
		
			detachedItems.ForEach(item => context.Detach(item));

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Unchanged).Count == insertCount - detachCount);
						 
			// query
			var queryItems = query.ToList();
			
			Assert.IsTrue(queryItems.Count == insertCount);

			Assert.IsTrue(context.GetTrackedEntities(EntityState.Unchanged).Count == insertCount);
		
			// delete
			queryItems.ForEach(context.Delete);

			context.Commit(commitStrategy);

			Assert.IsTrue(
					context.GetTrackedEntities().Count == 0);
		}


		[TestMethod]
		public 
		void 
		AllDataImplicitContextQuery()
		{
			string 
			partitionKey = Guid.NewGuid().ToString();

			var
			context = StashConfiguration
									.GetClient<AllDataImplicit>()
									.GetContext();

			ContextQuery<AllDataImplicit>(
				partitionKey,
				context,
				context.CreateQuery()
					.Where(i => i.PartitionKey == partitionKey),
				CommitStrategy.Parallel);
		}

		[TestMethod]
		public 
		void 
		AllDataExplicitContextQuery()
		{
			string 
			partitionKey = Guid.NewGuid().ToString();

			var
			context = StashConfiguration
									.GetClient<AllDataExplicit>()
									.GetContext();

			ContextQuery<AllDataExplicit>(
				partitionKey,
				context,
				context.CreateQuery()
					.Where(i => i.PartitionKey == partitionKey),
				CommitStrategy.Parallel);
		}


		[TestMethod]
		public 
		void 
		AllDataImplicitContextMultipleError()
		{
			string 
			partitionKey = Guid.NewGuid().ToString();

			var
			context = StashConfiguration
									.GetClient<AllDataImplicit>()
									.GetContext();

			const int 
			insertCount = 5;
			
			int 
			exceptionCount = 0;



			Enumerable
				.Range(1, insertCount)
				.ForEach(
					i =>
						{
							var item = TypeFactory<AllDataImplicit>.CreateRandomSmall();

							item.SetPartitionKey(partitionKey);

							// make the string too long in some cases so we can catch exceptions
							if (i % 2 == 1) 
							{
								item.StringProperty = DataGenerator.GetStringSizeFixed(32 * 1024 * 3);
								++exceptionCount;
							}

							context.Insert(item);
						});
			
			bool								isSuccess;

			try 
			{
				context.Commit(CommitStrategy.Parallel);
				
				isSuccess = false;
			}
			catch (StashAggregateException aggEx)
			{
				Assert.IsTrue(aggEx.Error == StashError.StashAggregate);
				Assert.IsTrue(aggEx.ContainedExceptions.Length == exceptionCount);

				isSuccess = true;
			}
			catch (Exception)
			{
				isSuccess = false;
			}

			Assert.IsTrue(isSuccess);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}