using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit)]
	public
	class KeyDataExplicit					:	IKeyDataHelper
	{
	        [StashPartitionKey]
			public 
			string								PartitionKey		{ get; set; }

	        [StashRowKey]
			public 
			string								RowKey				{ get; set; }

			[StashETag]
			private				
			string								_eTagInternal;
			

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
			return _eTagInternal;
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
			_eTagInternal = etag;
		}

		public
		bool
		HasETag()
		{
			return true;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
