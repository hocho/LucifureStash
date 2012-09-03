using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit, Name="StashTest")]
	public 
	class KeyDataWithList					:	KeyDataExplicit,
												IDataHelper<KeyDataWithList>
	{
	        [StashCollection]
	        public
	        ArrayList							Items {get; set;}

	        [StashCollection]
	        public
	        List<int>							Ints;

	        [StashCollection]
	        public
	        List<object>						Objects;

	        [StashCollection]
	        public
	        double[]							Doubles;

	        [StashCollection]
	        public
	        List<PersonInfo>					People;
		
	        [StashCollection]
	        public
	        ulong[]								ULongs;

	        [StashCollection]
	        public
	        List<Int16Enum>						Int16Enums;
			

		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (KeyDataWithList) obj;
			
			return	
						data.PartitionKey		== PartitionKey
					&&	data.RowKey				== RowKey
					&&	ListEquals(Items, data.Items)
					&&	Helper.IsEqual(Ints, data.Ints)
					&&	Helper.IsEqual(Objects, data.Objects)
					&&  Helper.IsEqual(Doubles, data.Doubles)
					&&  Helper.IsEqual(People, data.People)
					&&  Helper.IsEqual(ULongs, data.ULongs)
					&&  Helper.IsEqual(Int16Enums, data.Int16Enums);
		}		

		static
		bool
		ListEquals(
			ArrayList							lhs,
			ArrayList							rhs)
		{
			int									count;
			bool								result;

			if (result = (lhs == null && rhs == null))
			{
				;
			}
			else if (lhs != null && rhs != null && (count = lhs.Count) == rhs.Count)
			{
				bool							isEqual = true;

				for(int i = 0; i < count; ++i)
					if (!DataGenerator.Compare(lhs[i], rhs[i])) 
					{
						isEqual = false;
						break;
					}
				
				result = isEqual;
			}

			return result;
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
	        Items		= null;
			Ints		= null;

			Objects[1]	= null;

			Doubles		= null;
			People		= null;
			ULongs		= null;
			Int16Enums	= null;
		}

		public
        void
		Populate(
			int									stringSize,
			int									binarySize)
		{
			PartitionKey	=	typeof(KeyDataWithList).Name;
			RowKey			=	Guid.NewGuid().ToString();

			// ---------------------------------------------------------------------------------------------------------
			Items = new ArrayList();

			Items.Add(DataGenerator.Rnd.Next(2) == 1);
			Items.Add(DataGenerator.Rnd.Next(Int32.MaxValue));
			Items.Add((long) (DataGenerator.Rnd.NextDouble() * Int64.MaxValue));
			Items.Add(DataGenerator.Rnd.NextDouble() * Double.MaxValue);
			Items.Add(DateTime.UtcNow);
			Items.Add(Guid.NewGuid());
			Items.Add(DataGenerator.GetStringSizeFixed(stringSize));
			Items.Add(DataGenerator.GetBytesSizeFixed(binarySize));
		
			
			// ---------------------------------------------------------------------------------------------------------
			Ints = new List<int>();

			Ints.Add(100);
			Ints.Add(1000);
			Ints.Add(10000);
		

			// ---------------------------------------------------------------------------------------------------------
			Objects = new List<object>();

			Objects.Add("Object1");
			Objects.Add(420);
			Objects.Add("Object3");

			// ---------------------------------------------------------------------------------------------------------
			Doubles = new double[3];

			Doubles[0] = 100.001;
			Doubles[1] = 1000.0001;
			Doubles[2] = 10000.00001;

			// ---------------------------------------------------------------------------------------------------------
			People = new List<PersonInfo>();
			
			People.Add(PersonInfo.CreateNew());
		
			// ---------------------------------------------------------------------------------------------------------
			Int16Enums = new List<Int16Enum>();

			Int16Enums.Add(Int16Enum.Value3);
			Int16Enums.Add(Int16Enum.Value2);
			Int16Enums.Add(Int16Enum.Value1);

			// ---------------------------------------------------------------------------------------------------------
			ULongs = new ulong[3];

			ULongs[0] = ulong.MinValue;
			ULongs[1] = (ulong) (DataGenerator.Rnd.Next(int.MinValue, int.MaxValue) * 
									DataGenerator.Rnd.Next(int.MinValue, int.MaxValue));
			ULongs[2] = ulong.MaxValue;
		}

		public	
		IQueryable
		GetQuery(
			StashClient<KeyDataWithList>		stashClient,
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
