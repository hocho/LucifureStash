using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	interface IKeyDataHelper 
	{
		// kept as method and not property so as not to interfere with implicit mode	
		string
		GetPartitionKey();					

		void
		SetPartitionKey(
			string								key);					

		string 
		GetRowKey();

		void
		SetRowKey(
			string								key);					
	
	
		string 
		GetETag();

		void
		SetETag(
			string								key);					
	
	
		bool
		HasETag();
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	interface IDataHelper<T>				:	IKeyDataHelper
	{	
		void 
		UpdateDateTime();

		void 
		UpdateForMerge();

		void 
		Populate(
			int									stringSize,
			int									binarySize);

		IQueryable
		GetQuery(
			StashClient<T>						stashClient,
			string								partitionKey,
			string								rowKey);
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
