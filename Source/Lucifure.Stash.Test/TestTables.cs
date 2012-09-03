using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Table
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	class TableToCreate							:	KeyDataImplicit
	{
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	partial 
	class TestTables
	{
		void 
		DeleteRandomTables()
		{
			var
			options = StashConfiguration.GetDefaultOptions();

			var
			client = StashConfiguration.GetClient<TableToCreate>(options);

			var 
			tablesRandomPrefixed = client.ListTables(DataGenerator.RandomTableNamePrefix).ToList(); 

			tablesRandomPrefixed.ForEach(client.DeleteTable);
		}

		[TestMethod]
		public 
		void 
		TestTableActions()
		{
			var
			options = StashConfiguration.GetDefaultOptions();

			var 
			tableName = DataGenerator.GetRandomTableName();

			options.OverrideEntitySetNameIsDynamic	= false;
			options.OverrideEntitySetName			= o => tableName;

			var
			client = StashConfiguration.GetClient<TableToCreate>(options);
		
			// exists?
			Assert.IsFalse(client.DoesTableExist());
						
			// create									
			client.CreateTableIfNotExist();

			Assert.IsTrue(client.DoesTableExist());

			client.CreateTableIfNotExist();	

			bool								isSuccess;

			try 
			{
				client.CreateTable();

				isSuccess = false;
			}
			catch (StashException stashEx)
			{
				Assert.IsTrue(stashEx.Error == StashError.TableAlreadyExists);

				isSuccess = true;
			}
			catch (Exception)
			{
				isSuccess = false;
			}

			Assert.IsTrue(isSuccess);

			// delete
			client.DeleteTable();
			
			// should not error
			client.DeleteTableIfNotExists();
				
			try 
			{
				client.DeleteTable();

				isSuccess = false;
			}
			catch (StashException stashEx)
			{
				Assert.IsTrue(stashEx.Error == StashError.TableNotFound);

				isSuccess = true;
			}
			catch (Exception)
			{
				isSuccess = false;
			}

			Assert.IsTrue(isSuccess);
		}

		[TestMethod]
		public 
		void 
		TestListTable()
		{
			DeleteRandomTables();

			var
			options = StashConfiguration.GetDefaultOptions();

			var
			client = StashConfiguration.GetClient<TableToCreate>(options);

			// get existing tables
			var 
			tablesAllInitial = client.ListTables().ToList();


			// make list of randomly table names
			const int 
			testCount = 7;

			var 
			tableNamesForCreate =	Enumerable
										.Range(0, testCount)
										.Select(i => DataGenerator.GetRandomTableName())
										.ToList();

			// create randomly named tables								
			tableNamesForCreate.ForEach(client.CreateTable);

			// retrieve back only the list of random tables. 
			var 
			tablesRandomPrefixed = client.ListTables(DataGenerator.RandomTableNamePrefix).ToList(); 

			// check we got back the same count 
			Assert.IsTrue(tablesRandomPrefixed.Count == testCount);

			// delete all the tables we created
			tableNamesForCreate.ForEach(client.DeleteTable);

			var 
			tablesAllFinal = client.ListTables().ToList();

			// check that list of all tables is >= random tables
			Assert.IsTrue(tablesAllInitial.Count >= tablesAllFinal.Count);

		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}