using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	partial
	class TestGeneric
	{
		static
		T
		Write<T>(
			StashClient<T>						client,
			DataSize							dataSize)
		where 
			T								:	IDataHelper<T>, new()
		{
			T
			data = TypeFactory<T>.Create(dataSize);

			data = client.Insert(data);

			// etag not present in type or has a valid etag
			Assert.IsTrue(!data.HasETag() || data.GetETag().Is());
		
			return data;
		}

		static
		T
		Write<T>(
			DataSize							dataSize)
		where 
			T								:	IDataHelper<T>, new()
		{
			return Write<T>(StashConfiguration.GetClient<T>(), dataSize);
		}

		IList<T>
		Write<T>(
			int									count)
		where 
			T								:	IDataHelper<T>, new()
		{
			List<T>				
			result = new List<T>();

			var context = StashConfiguration.GetClient<T>();

			while (--count >= 0)
			{
				var
				data = TypeFactory<T>.CreateRandomSmall();
		
				context.Insert(data);
			
				result.Add(data);
			}
					
			return result;
		}

		public
		static
		IQueryable<T>
		GetQuery<T>(
			string								partitionKey,
			string								rowKey)
		where 
			T								:	IDataHelper<T>, new()
		{
			return TypeFactory<T>.GetQuery(
										StashConfiguration.GetClient<T>(),
										partitionKey,
										rowKey);
		}

		static
		T
		Read<T>(
			string								partitionKey,
			string								rowKey)
		where 
			T								:	IDataHelper<T>, new()
		{
			return GetQuery<T>(
							partitionKey,
							rowKey)
					.ToList()[0];
		}

		IQueryable<T>
		ReadAll<T>()
		where 
			T								:	IDataHelper<T>, new()
		{
			var
		    service = StashConfiguration.GetClient<T>();

		    var 
		    q = from t in service.CreateQuery()
		            select t;

		    return q;
		}

		static
		T
		Update<T>(
			T									data)
		where 
			T								:	IDataHelper<T>
		{
			var context = StashConfiguration.GetClient<T>();

			data.UpdateDateTime();
			
			string
			etag = data.GetETag();

			data = context.Update(data);

			// check that if etag is present, it changed.
			Assert.IsTrue(!data.HasETag() || data.GetETag() != etag);	

			return data;
		}

		static
		T
		Merge<T>(
			T									data)
		where 
			T								:	IDataHelper<T>
		{
			var context = StashConfiguration.GetClient<T>();

			data.UpdateForMerge();
			
			string
			etag = data.GetETag();

			data = context.Merge(data);

			// check that if etag is present, it changed.
			Assert.IsTrue(!data.HasETag() || data.GetETag() != etag);	

			return data;
		}

		static
		void
		Delete<T>(
			string								partitionKey,
			string								rowKey)
		{
			var context = StashConfiguration.GetClient<T>();

			context.Delete(partitionKey, rowKey);
		}

		void 
		Full<T>(
			DataSize							dataSize)
		where 
			T								:	IDataHelper<T>, new()
		{
			// write
			T 
			written = Write<T>(dataSize);

			// read
			T
			read = Read<T>(
							written.GetPartitionKey(), 
							written.GetRowKey());

			Assert.IsTrue(read.Equals(written));

			// update
			T
			update = Update(read);

			read = Read<T>(
						update.GetPartitionKey(), 
						update.GetRowKey()); 
			
			Assert.IsTrue(read.Equals(update));

			// merge
			T
			merge = Merge(read);

			read = Read<T>(
						update.GetPartitionKey(), 
						update.GetRowKey()); 
			
			read.UpdateForMerge();

			Assert.IsTrue(read.Equals(merge));

			// delete
			Delete<T>(
				read.GetPartitionKey(), 
				read.GetRowKey());
		}

		static
		void 
		InsertOrUpdate<T>(
			DataSize							dataSize)
		where 
			T								:	IDataHelper<T>, new()
		{
			var client = StashConfiguration.GetClient<T>();
			
			T
			data = TypeFactory<T>.Create(dataSize);

			T
			written = client.InsertOrUpdate(data);

			// read
			T
			read = Read<T>(
							written.GetPartitionKey(), 
							written.GetRowKey());

			Assert.IsTrue(read.Equals(written));

			// update
			written.UpdateDateTime();

			written = client.InsertOrUpdate(written);
			written = client.InsertOrMerge(written);

			read = Read<T>(
						written.GetPartitionKey(), 
						written.GetRowKey()); 
			
			Assert.IsTrue(read.Equals(written));

			// delete
			client.Delete(read);
		}

		public 
		static
		int 
		DeleteMultiple<T>(
			IQueryable<T>						query)
		where 
			T								:	IDataHelper<T>, new()
		{
			var 
			items = query.ToList();

			var service = StashConfiguration.GetClient<T>();

			int i = 0;

		    foreach(T item in items)
		    {
				++i;
				service.Delete(item);
			}

			System.Diagnostics.Trace.WriteLine(
										String.Format(
													"{0} items deleted.",
													i));
			return i;
		}

		public 
		static
		int
		DeleteMultiple<T>(
			string								partitionKey)
		where 
			T								:	IDataHelper<T>, new()
		{
		    return DeleteMultiple<T>(
								GetQuery<T>(
											partitionKey, 
											null));
		}

		public
		static
		void
		InsertItems<T>(
			IEnumerable<T>						items)								
		{
			var 
			context = StashConfiguration.GetClient<T>().GetContext();
		
			foreach(T item in items)
				context.Insert(item);

			context.Commit();
		}

		public
		static
		void 
		FullMultiple<T>(
			int									count,
			string								partitionKey,
			bool								withDelete)
		where 
			T								:	IDataHelper<T>, new()
		{
		    List<T>
		    list = Enumerable
		                .Range(1, count)
		                .Select(i => 
								{
									T item = TypeFactory<T>.CreateRandomSmall();
								
									if (partitionKey.Is()) 
										item.SetPartitionKey(partitionKey);
	
									return item;
								})
		                .ToList();
		
		    InsertItems(list);
		
			if (withDelete)
				DeleteMultiple<T>(partitionKey);
		}

		// set a range of row keys and query on them for delete - used to test query
		static
		int
		FullMultipleRange<T>(
			int									count,
			string								partitionKey,
			IQueryable<T>						deleteQuery)
		where 
			T								:	IDataHelper<T>, new()
		{
			List<T>
			list = Enumerable
			            .Range(1, count)
			            .Select(i => 
			                    {
			                        T item = TypeFactory<T>.CreateRandomSmall();
								
			                        if (partitionKey.Is()) 
			                            item.SetPartitionKey(partitionKey);
									
			                        item.SetRowKey(i + Guid.NewGuid().ToString());
							
			                        return item;
			                    })
			            .ToList();
		
			InsertItems(list);

			return	deleteQuery != null
						?	DeleteMultiple<T>(deleteQuery)
						:	-1;
		}

		static
		void 
		WriteNReadN<T>(
			string								partitionKey,
			int									writeCount,
			int									readCount)
		where 
			T								:	IDataHelper<T>, new()
		{
		    List<T>
		    writeList = Enumerable
		                .Range(1, writeCount)
		                .Select(i => 
								{
									T item = TypeFactory<T>.CreateRandomSmall();
								
									if (partitionKey.Is()) 
										item.SetPartitionKey(partitionKey);
	
									return item;
								})
		                .ToList();
		
			InsertItems(writeList);

			var query = GetQuery<T>(partitionKey, null);

			var queryTake = query.Take(readCount);

			var readList = queryTake.ToList(); 

			Assert.IsTrue(readList.Count == readCount);	

			DeleteMultiple<T>(partitionKey);
		}


		[TestMethod]
		public 
		void TestAllDataImplicit()
		{
			StashConfiguration.GetClient<AllDataImplicit>().CreateTableIfNotExist();

			Full<AllDataImplicit>(DataSize.Small);
		}

		[TestMethod]
		public 
		void TestAllDataExplicit()
		{
			Full<AllDataExplicit>(DataSize.Multiple);
		}

		[TestMethod]
		public 
		void TestAllDataImplicitInsertOrUpdate()
		{
			// Upsert not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				InsertOrUpdate<AllDataImplicit>(DataSize.Small);
		}

		[TestMethod]
		public 
		void TestAllDataExplicitInsertOrUpdate()
		{
			// Upsert not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				InsertOrUpdate<AllDataExplicit>(DataSize.Multiple);
		}

		[TestMethod]
		public 
		void TestAllDataWithDictionary()
		{
			Full<AllDataWithDictionary>(DataSize.Large);
		}

		AllDataWithDictionary
		AllDataWithDictionaryMultiple(
			StashClientOptions					options)
		{
			StashClient<AllDataWithDictionary>
			client = StashConfiguration.GetClient<AllDataWithDictionary>(options);

			AllDataWithDictionary
			entity = TypeFactory<AllDataWithDictionary>.Create(DataSize.Multiple); // DataSize.Multiple

			client.Insert(entity);

			AllDataWithDictionary
			entityRead = client.Get(entity.PartitionKey, entity.RowKey);

			Assert.IsTrue(entityRead.Equals(entity));

			return entityRead;
		}

		[TestMethod]
		public 
		void TestAllDataWithDictionaryMultiple()
		{
			// Development storage does not support large entities > 250K or so
			if (StashConfiguration.IsConfigurationCloud)
				DoTestAllDataWithDictionaryMultiple();
		}

		void DoTestAllDataWithDictionaryMultiple()
		{
			bool								isSuccess;

			try
			{
				StashClientOptions
				options = StashConfiguration.GetDefaultOptions();

				AllDataWithDictionaryMultiple(options);		// use large data must fail
				
				// if control came here we failed
				isSuccess = false;
			}
			catch (Exception)
			{
				isSuccess = true;
			}

			if (!isSuccess)
				Assert.Fail();


			{
				StashClientOptions
				options = StashConfiguration.GetDefaultOptions();

				options.SupportLargeObjectsInPool = true;

				AllDataWithDictionary
				entity = AllDataWithDictionaryMultiple(options);

				// now read without the support large pool options and validate the the entities do NOT match
				//	and the unmapped pool count is different
				options.SupportLargeObjectsInPool = false;
				
				StashClient<AllDataWithDictionary>
				client = StashConfiguration.GetClient<AllDataWithDictionary>(options);

				AllDataWithDictionary
				entityRead = client.Get(entity.PartitionKey, entity.RowKey);

				Assert.IsFalse(entityRead.Equals(entity));

				// now read without the support large pool options and validate the the entities DO match
				//	and the unmapped pool count is different
				options.SupportLargeObjectsInPool = true;
				
				client = StashConfiguration.GetClient<AllDataWithDictionary>(options);

				entityRead = client.Get(entity.PartitionKey, entity.RowKey);

				Assert.IsTrue(entityRead.Equals(entity));

				// delete
				client.Delete(entityRead);			
			}
		}

		[TestMethod]
		public 
		void TestKeyDataWithList()
		{
			Full<KeyDataWithList>(DataSize.Random);
		}

		[TestMethod]
		public 
		void TestKeyDataWithObject()
		{
			Full<KeyDataWithObject>(DataSize.Random);
		}

		[TestMethod]
		public 
		void AllDataImplicitWriteList()
		{
			FullMultiple<AllDataImplicit>(10, "WriteList", true);
		}

		[TestMethod]
		public 
		void AllDataImplicitWriteListRange()
		{
			const
			string PKey = "WriteListRange";

			const int 
			count = 9;		// set to 9 or less, so that are range matches the query

			var 
			query0 = StashConfiguration.GetClient<AllDataImplicit>()
			            .CreateQuery()
			            .Where(i => i.PartitionKey == PKey && i.RowKey.CompareTo("1") >= 0 && i.RowKey.CompareTo("9" + "z") <= 0);	// 1 to 9
	
			var 
			query = query0.Where(i => i.BoolProperty == true || i.BoolProperty == false);

			int 
			deleteCount = 0;

			Assert.IsTrue(									
				(deleteCount = FullMultipleRange<AllDataImplicit>(
										count, 
										PKey, 
										 query)) == count,
				String.Format(
							"Delete Count = {0}. Expected Count = {1}.",
							deleteCount,
							count));
		}

		[TestMethod]
		public 
		void TestAllDataImplicitExplicitDeleteAll()
		{
			DeleteMultiple<AllDataImplicit>("");
			DeleteMultiple<AllDataExplicit>("");
		}

		[TestMethod]
		public 
		void 
		ComputeDataSizeValidateBoundary()
		{
			// development storage had different storage limits for this fails
			if (StashConfiguration.IsConfigurationCloud)
				DoComputeDataSizeValidateBoundary();
		}

		void 
		DoComputeDataSizeValidateBoundary()
		{
			// create instance and get its size
			var client= StashConfiguration.GetClient<AllDataExplicit>();
			
			var 
			item = TypeFactory<AllDataExplicit>.CreateRandomMultiple();

			int 
			size = client.ComputeEntityDataSize(item);

			// character count
			int 
			maxSize = 1024 * 1024;

			int 
			delta = (maxSize - size) / 2;		// 2 = size of char

			// Increase size to max and insert
			item.StringField += DataGenerator.GetStringSizeFixed(delta);

			// re-compute size ... should exceed so trim it back 
			int
			reSize = client.ComputeEntityDataSize(item);

			delta = (reSize - maxSize) / 2;
			
			item.StringField = item.StringField.Substring(0, item.StringField.Length - delta); //  - 8
						 
			// re-compute size ... should now fit maxSize
			reSize = client.ComputeEntityDataSize(item);

			client.Insert(item);

			// delete
			client.Delete(item);
		
			// Increase size by 1 and insert expecting to fail
			item.StringField += "A";

			bool								isSuccess;

			try 
			{
			    client.Insert(item);

				isSuccess = false;
			}
			catch (Exception)
			{
			    isSuccess = true;
			}

			Assert.IsTrue(isSuccess);
		}

		[TestMethod]
		public 
		void AllDataImplicitTakeN()
		{
			WriteNReadN<AllDataImplicit>("readNWriteN", 5, 2);						
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
