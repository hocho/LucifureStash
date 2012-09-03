using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public
	enum DataSize
	{
		Random,
		//Fixed,
		Small,
		Large,
		Multiple,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

    public 
    class TypeFactory<T>
	where
		T									:	IDataHelper<T>, new()
    {
		public 
		static
        T
		Create(
			DataSize							dataSize)
		{
			T									result = default(T);

			switch (dataSize)
			{
				case DataSize.Random:
					result = CreateRandomSize();
					break;
				//case DataSize.Fixed:
				//    result = CreateRandomSizeFixed();
				//    break;
				case DataSize.Small:
					result = CreateRandomSmall();
					break;
				case DataSize.Large:
					result = CreateRandomLarge();
					break;
				case DataSize.Multiple:
					result = CreateRandomMultiple();
					break;
			}		
			return result;
		}

		public 
		static
        T
		CreateRandomSize()
		{
			return CreateRandomSizeFixed(
										DataGenerator.Rnd.Next(32 * 1024), 
										DataGenerator.Rnd.Next(64 * 1024));
		}

		public 
		static
        T
		CreateRandomSmall()
		{
			return CreateRandomSizeFixed(
										120,	 
										240);
		}

		public 
		static
        T
		CreateRandomLarge()
		{
			return CreateRandomSizeFixed(
										32 * 1024, 
										64 * 1024);
		}

		public 
		static
        T
		CreateRandomMultiple()
		{
			return CreateRandomSizeFixed(
										(int) ((32 * 1024) * 2.5), 
										(int) ((64 * 1024)	* 2.5));
		}
        
		public 
		static
        T
		CreateRandomSizeFixed(
			int									stringSize,
			int									binarySize)
		{
			T 
			result = new T();

			result.Populate(stringSize, binarySize);
			
			return result;
        }

		public	
		static
		IQueryable<T>
		GetQuery(
			StashClient<T>						stashClient,
			string								partitionKey,
			string								rowKey)
		{
			return (IQueryable<T>) (new T()).GetQuery(
												stashClient,
												partitionKey,
												rowKey);
		}

    }

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
