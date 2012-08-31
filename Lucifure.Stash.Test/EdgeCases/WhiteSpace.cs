using System;
using System.Linq;
using CodeSuperior.Lucifure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucifure.Stash.Test.EdgeCases 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	class TestWhiteSpace 
	{
			const 
			string								_partitionKey = "WhiteSpaces";

		[TestMethod]
		public 
		void 
		WhiteSpaceInKeys() 
		{
			// Development storage does not support retrieving row with white spaces in the keys.
			// (Does write them successfully).
			if (StashConfiguration.IsConfigurationCloud)
				DoWhiteSpaceInKeys();
		}

		static
		void 
		DoWhiteSpaceInKeys() 
		{
			StashClient<WhiteSpacesInData>	
			client = StashConfiguration.GetClient<WhiteSpacesInData>();

			client.CreateTableIfNotExist();

			const string 
			partitionKey = "  " + _partitionKey;			// prefix 
			
			string 
			rowKey = Guid.NewGuid().ToString() + " ";		// suffix
			
			const string stringField = "   ";

			const string stringField2= "  X  ";

			client.Insert(
					new WhiteSpacesInData {
						PartitionKey    = partitionKey,
						RowKey			= rowKey,
						StringField     = stringField,  
						StringField2	= stringField2 } );

			var 
			data = 
				client
			        .CreateQuery()
			        .Where(x => x.PartitionKey == partitionKey && x.RowKey == rowKey)
			        .FirstOrDefault();

			data = 
				client
					.CreateQuery()
					.FirstOrDefault(x => x.PartitionKey == partitionKey && x.RowKey == rowKey);

			Assert.IsTrue(data				!= null);
			Assert.IsTrue(data.PartitionKey == partitionKey);
			Assert.IsTrue(data.RowKey       == rowKey);
			Assert.IsTrue(data.StringField  == "");
			Assert.IsTrue(data.StringField2  == stringField2);

			client.Delete(data);
		}

		public 
		static
		string 
		SpaceWrap(
			string								str)
		{
			return String.Format("  {0}  ", str);	
		}

	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit)]
	public
	class WhiteSpacesInData					:	KeyDataExplicit
	{
	        [Stash]
	        public
	        string								StringField;

	        [Stash]
	        public
	        string								StringField2;
	}
		
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
