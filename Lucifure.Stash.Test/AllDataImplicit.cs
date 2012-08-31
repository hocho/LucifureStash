using System;
using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
		
	[StashEntity(Name="StashTest")]
	public
	class AllDataImplicit					:	KeyDataImplicit,
												IDataHelper<AllDataImplicit>
	{
			public
			bool								BoolProperty				{ get; set; }

			public
			bool ?								BoolPropertyNull			{ get; set; }

			public
			bool ?								BoolPropertyNullNot			{ get; set; }

			public 
			int									IntProperty					{ get; set; }

			public 
			int ?								IntPropertyNull				{ get; set; }

			public 
			int ?								IntPropertyNullNot			{ get; set; }

			public 
			Int64								Int64Property				{ get; set; }

			public 
			Int64 ?								Int64PropertyNull			{ get; set; }

			public 
			Int64 ?								Int64PropertyNullNot		{ get; set; }

			public 
			Double								DoubleProperty				{ get; set; }

			public 
			Double ?							DoublePropertyNull			{ get; set; }

			public 
			Double ?							DoublePropertyNullNot		{ get; set; }

			public
			DateTime							DateTimeProperty			{ get; set; }

			public
			DateTime ?							DateTimePropertyNull		{ get; set; }

			public
			DateTime ?							DateTimePropertyNullNot		{ get; set; }

			public
			Guid								GuidProperty				{ get; set; }

			public
			Guid ?								GuidPropertyNull			{ get; set; }

			public
			Guid ?								GuidPropertyNullNot			{ get; set; }

			public
			string								StringProperty				{ get; set; }

			public
			string								StringPropertyNull			{ get; set; }


			public 
			byte[]								BinaryProperty				{ get; set; }
	
			public 
			byte[]								BinaryPropertyNull			{ get; set; }
	
		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (AllDataImplicit) obj;
			
			return	
					data.PartitionKey			== PartitionKey
				&&	data.RowKey					== RowKey

				&&	data.BoolProperty			== BoolProperty
				&&	data.BoolPropertyNull		== null
					&& data.BoolPropertyNull	== BoolPropertyNull
				&&	data.BoolPropertyNullNot	== BoolPropertyNullNot

				&&	data.IntProperty			== IntProperty
				&&	data.IntPropertyNull		== null
					&&	data.IntPropertyNull	== IntPropertyNull
				&&	data.IntPropertyNullNot		== IntPropertyNullNot

				&&	data.Int64Property			== Int64Property
				&&	data.Int64PropertyNull		== null
					&&	data.Int64PropertyNull	== Int64PropertyNull
				&&	data.Int64PropertyNullNot	== Int64PropertyNullNot

				&&	Helper.IsEqual(data.DoubleProperty, DoubleProperty)
				&&	data.DoublePropertyNull		== null
					&&	data.DoublePropertyNull == DoublePropertyNull
				&&	Helper.IsEqual(data.DoublePropertyNullNot, DoublePropertyNullNot) 

				&&	Helper.IsEqual(data.DateTimeProperty, DateTimeProperty)
				&&	data.DateTimePropertyNull				== null
					&& data.DateTimePropertyNull			== DateTimePropertyNull
				&&	Helper.IsEqual(
								data.DateTimePropertyNullNot, 
								DateTimePropertyNullNot)

				&&	data.GuidProperty			== GuidProperty
				&&	data.GuidPropertyNull		== null
					&& data.GuidPropertyNull	== GuidPropertyNull
				&&	data.GuidPropertyNullNot	== GuidPropertyNullNot

				&&	data.StringProperty			== StringProperty
				&&	data.StringPropertyNull		== StringPropertyNull

				&&	Helper.IsEqual(
									data.BinaryProperty,
									BinaryProperty);
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
			DateTimeProperty = DateTime.UtcNow;
		}

		public
		void
		UpdateForMerge()
		{
			BoolPropertyNullNot		=	null;	

			IntPropertyNullNot		=	null;	

			Int64PropertyNullNot	=	null;	

			DoublePropertyNullNot	=	null;	

			DateTimeProperty		=	new DateTime(2012, 1, 1);	
			DateTimePropertyNullNot	=	null;	

			GuidPropertyNullNot		=	null;	

			StringPropertyNull		=	null;

			BinaryProperty			=	null;	
		}

		public
        void
		Populate(
			int									stringSize,
			int									binarySize)
		{
			PartitionKey			=	typeof(AllDataImplicit).Name;
			RowKey					=	Guid.NewGuid().ToString();

			BoolProperty			=	DataGenerator.Rnd.Next(2) == 1;
			BoolPropertyNull		=	null;
			BoolPropertyNullNot		=	DataGenerator.Rnd.Next(2) == 1;

			IntProperty				=	DataGenerator.Rnd.Next(Int32.MaxValue);
			IntPropertyNull			=	null;
			IntPropertyNullNot		=	DataGenerator.Rnd.Next(int.MaxValue);

			Int64Property			=	(Int64) (DataGenerator.Rnd.Next(Int32.MaxValue) * DataGenerator.Rnd.Next(Int32.MaxValue));
			Int64PropertyNull		=	null;
			Int64PropertyNullNot	=	(Int64) (DataGenerator.Rnd.Next(Int32.MaxValue) * DataGenerator.Rnd.Next(Int32.MaxValue));

			DoubleProperty			=	DataGenerator.Rnd.NextDouble() * Double.MaxValue;
			DoublePropertyNull		=	null;
			DoublePropertyNullNot	=	DataGenerator.Rnd.NextDouble() * Double.MaxValue;

			DateTimeProperty		=	DateTime.UtcNow;
			DateTimePropertyNull	=	null;
			DateTimePropertyNullNot	=	DateTime.UtcNow;

			GuidProperty			=	Guid.NewGuid();
			GuidPropertyNull		=	null;
			GuidPropertyNullNot		=	Guid.NewGuid();

			StringProperty			=	DataGenerator.GetStringSizeFixed(stringSize);
			StringPropertyNull		=	null;

			BinaryProperty			=	DataGenerator.GetBytesSizeFixed(binarySize);
		}

		public	
		IQueryable
		GetQuery(
			StashClient<AllDataImplicit>		stashClient,
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
