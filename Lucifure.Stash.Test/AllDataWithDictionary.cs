using System;
using System.Collections.Generic;
using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit, Name="StashTest")]
	public
	class AllDataWithDictionary				:	AllDataExplicit,
												IDataHelper<AllDataWithDictionary>
	{
	        [StashPool]
	        public
	        Dictionary<string, object>			Unmapped {get; set;}

		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (AllDataWithDictionary) obj;

			return base.Equals(obj) && DictionaryEquals(Unmapped, data.Unmapped);
		}		

		static 
		bool
		DictionaryEquals(
			IDictionary<string, object>			lhs,
			IDictionary<string, object>			rhs)
		{
			var 
			keysLhs = lhs.Where(x => x.Key != Literal.ETag).OrderBy(x => x.Key).ToList();

			var
			keysRhs = rhs.Where(x => x.Key != Literal.ETag).OrderBy(x => x.Key).ToList();

			return keysLhs.Count() == keysRhs.Count() 
				&& keysLhs.All(x => x.Value.ToString().Equals(rhs[x.Key].ToString())	// values are the same
					&& x.Value.GetType() == rhs[x.Key].GetType());						// types are the same
		}

		
		public 
		override
		int
		GetHashCode()
		{
			return (PartitionKey + RowKey).GetHashCode();
		}

	    public
		override
	    void
	    Populate(
	        int									stringSize,
	        int									binarySize)
	    {
			base.Populate(stringSize, binarySize);

			Unmapped = new Dictionary<string,object>();

			Unmapped["DictionaryStringField"]		= StringField;
			Unmapped["DictionaryIntField"]			= IntField;
			Unmapped["DictionaryLongField"]			= Int64Field;
			Unmapped["DictionaryBoolField"]			= BoolField;
			Unmapped["DictionaryGuidField"]			= GuidField;
			Unmapped["DictionaryDateTimeField"]		= DateTimeField;
			Unmapped["DictionaryDoubleField"]		= DoubleField;
			Unmapped["DictionaryBinaryField"]		= BinaryField;
	    }

		public
		override
		void
		UpdateForMerge()
		{
			base.UpdateForMerge();

			Unmapped["DictionaryStringField"]		= "Lucifure";
		}

		public	
		IQueryable
		GetQuery(
			StashClient<AllDataWithDictionary>	stashClient,
			string								pKey,
			string								rKey)
		{
			if (pKey.Is() && rKey.Is())
				return stashClient.CreateQuery()
							.Where(t => t.PartitionKey == pKey && t.RowKey == rKey);

			else if (pKey.Is())
				return stashClient.CreateQuery()
							.Where(t => t.PartitionKey == pKey);

			else if (rKey.Is())
				return stashClient.CreateQuery()
							.Where(t => t.RowKey == rKey);

			else 
				return stashClient.CreateQuery();
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
