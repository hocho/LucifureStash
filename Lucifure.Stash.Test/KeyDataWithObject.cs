using System;
using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit, Name="StashTest")]
	public 
	class KeyDataWithObject					:	KeyDataExplicit,
												IDataHelper<KeyDataWithObject>
	{
	        [Stash]
	        public
	        PersonInfo							PersonInfo {get; set;}


		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (KeyDataWithObject) obj;
			
			return	
						data.PartitionKey		== PartitionKey
					&&	data.RowKey				== RowKey
					&&	(data.PersonInfo == null && PersonInfo == null) 
							|| data.PersonInfo.Equals(PersonInfo);
		}		

		public 
		override
		int
		GetHashCode()
		{
			return (PartitionKey + RowKey).GetHashCode();
		}

		public
		void 
		UpdateDateTime()
		{
		}

		public
		void
		UpdateForMerge()
		{
			PersonInfo = null;
		}

		public
        void
		Populate(
			int									stringSize,
			int									binarySize)
		{
			PartitionKey	=	typeof(KeyDataWithObject).Name;
			RowKey			=	Guid.NewGuid().ToString();
			PersonInfo		=	PersonInfo.CreateNew();
		}

		public	
		IQueryable
		GetQuery(
			StashClient<KeyDataWithObject>		stashClient,
			string								partitionKey,
			string								rowKey)
		{
			if (partitionKey.Is() && RowKey.Is())
				return stashClient.CreateQuery()
							.Where(t => t.PartitionKey == partitionKey && t.RowKey == rowKey);

			else if (partitionKey.Is())
				return stashClient.CreateQuery()
							.Where(t => t.PartitionKey == partitionKey);

			else if (rowKey.Is())
				return stashClient.CreateQuery()
							.Where(t => t.RowKey == rowKey);

			else 
				return stashClient.CreateQuery();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}

