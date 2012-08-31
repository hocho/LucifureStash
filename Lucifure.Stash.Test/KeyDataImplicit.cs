
namespace Lucifure.Stash.Test 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public
	class KeyDataImplicit					:	IKeyDataHelper
	{
			public 
			string								PartitionKey		{ get; set; }

			public 
			string								RowKey				{ get; set; }

		// IKeyDataHelper
		public 
		string
		GetPartitionKey()
		{
			return PartitionKey;
		}

		public
		string 
		GetRowKey()
		{
			return RowKey;
		}

		public 
		string 
		GetETag()
		{
			return null;
		}

		public
		void
		SetPartitionKey(
			string								key)
		{
			PartitionKey = key;
		}					

		public
		void
		SetRowKey(
			string								key)
		{
			RowKey = key;
		}					

		public 
		void
		SetETag(
			string								etag)
		{
		}

		public
		bool
		HasETag()
		{
			return false;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
