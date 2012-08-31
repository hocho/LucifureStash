using System;
using System.Linq;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Mode=StashMode.Explicit, Name="StashTest")]
	public
	class AllDataExplicit					:	KeyDataExplicit,
												IDataHelper<AllDataExplicit>
	{
	        [StashTimestamp]
	        public
	        DateTime							Timestamp;	// Not populated or used for comparing, read only

			// Fields + Nullable. 8 + 8 + 6 = 22
	        [Stash]
	        public
	        string								StringField;

	        [Stash]
	        public
	        string								StringFieldNull;

	        [Stash]
	        public
	        int									IntField;

	        [Stash]
			public 
			int ?								IntFieldNull;

	        [Stash]
			public 
			int ?								IntFieldNullNot;

	        [Stash]
	        public
	        Int64								Int64Field;

	        [Stash]
	        public
	        Int64 ?								Int64FieldNull;

	        [Stash]
	        public
	        Int64 ?								Int64FieldNullNot;

	        [Stash]
	        public
	        bool								BoolField;

	        [Stash]
	        public
	        bool ?								BoolFieldNull;

	        [Stash]
	        public
	        bool ?								BoolFieldNullNot;

	        [Stash]
	        public
	        Guid								GuidField;

	        [Stash]
	        public
	        Guid ?								GuidFieldNull;

	        [Stash]
	        public
	        Guid ?								GuidFieldNullNot;

	        [Stash]
	        public
	        DateTime							DateTimeField;

	        [Stash]
	        public
	        DateTime ?							DateTimeFieldNull;

	        [Stash]
	        public
	        DateTime ?							DateTimeFieldNullNot;

	        [Stash]
	        public
	        Double								DoubleField;

	        [Stash]
	        public
	        Double ?							DoubleFieldNull;

	        [Stash]
	        public
	        Double ?							DoubleFieldNullNot;

	        [Stash]
	        public
	        byte[]								BinaryField;

	        [Stash]
	        public
	        byte[]								BinaryFieldNull;

			// intrinsic morphs + Nullable. 7 * 4 = 24
	        [Stash]
	        public
			byte								ByteFieldMin,
												ByteFieldMax;
	        [Stash]
	        public
			byte ?								ByteFieldNull,
												ByteFieldNullNot;

			[Stash]
	        public
			sbyte								SByteFieldMin,
			                                    SByteFieldMax;
			[Stash]
	        public
			sbyte ?								SByteFieldNull,
			                                    SByteFieldNullNot;

			[Stash]
	        public
			Int16								Int16FieldMin,
			                                    Int16FieldMax;
			[Stash]
	        public
			Int16 ?								Int16FieldNull,
			                                    Int16FieldNullNot;

			[Stash]
	        public
			UInt16								UInt16FieldMin,
			                                    UInt16FieldMax;
			[Stash]
	        public
			UInt16 ?							UInt16FieldNull,
			                                    UInt16FieldNullNot;

			[Stash]
	        public
			UInt32								UInt32FieldMin,
			                                    UInt32FieldMax;

			[Stash]
	        public
			UInt32 ?							UInt32FieldNull,
			                                    UInt32FieldNullNot;

			[Stash]
	        public
			UInt64								UInt64FieldMin,
			                                    UInt64FieldMax;

			[Stash]
	        public
			UInt64 ?							UInt64FieldNull,
			                                    UInt64FieldNullNot;

			[Stash]
	        public
			Char								CharFieldMin,
			                                    CharFieldMax;

			[Stash]
	        public
			Char ?								CharFieldNull,
			                                    CharFieldNullNot;

			// Enums - 8
			[Stash]
	        public
			ByteEnum							ByteEnumField;
			
			[Stash]
	        public
			ByteEnum ?							ByteEnumFieldNull,
												ByteEnumFieldNullNot;

			[Stash]
	        public
			SByteEnum							SByteEnumField;

			[Stash]
	        public
			SByteEnum ?							SByteEnumFieldNull,
												SByteEnumFieldNullNot;
												
			[Stash]
	        public
			Int16Enum							Int16EnumField;

			[Stash]
	        public
			Int16Enum ?							Int16EnumFieldNull,
												Int16EnumFieldNullNot;

			[Stash]
	        public
			UInt16Enum							UInt16EnumField;

			[Stash]
	        public
			UInt16Enum ?						UInt16EnumFieldNull,
												UInt16EnumFieldNullNot;

			[Stash]
	        public
			Int32Enum							Int32EnumField;

			[Stash]
	        public
			Int32Enum ?							Int32EnumFieldNull,
												Int32EnumFieldNullNot;

			[Stash]
	        public
			UInt32Enum							UInt32EnumField;

			[Stash]
	        public
			UInt32Enum ?						UInt32EnumFieldNull,
												UInt32EnumFieldNullNot;

			[Stash]
	        public
			Int64Enum							Int64EnumField;

			[Stash]
	        public
			Int64Enum ?							Int64EnumFieldNull,
												Int64EnumFieldNullNot;

			[Stash]
	        public
			UInt64Enum							UInt64EnumField;

			[Stash]
	        public
			UInt64Enum ?						UInt64EnumFieldNull,
												UInt64EnumFieldNullNot;

			// Object 
			[Stash]
	        public
			PersonInfo							PersonInfo;

		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (AllDataExplicit) obj;
			
			return	
					data.PartitionKey				== PartitionKey
				&&	data.RowKey						== RowKey

				// Fields + Nullable. 8 + 8 + 6 = 22
				&&	data.BoolField					== BoolField
				&&	data.BoolFieldNull				== null
					&& data.BoolFieldNull			== BoolFieldNull
				&&	data.BoolFieldNullNot			== BoolFieldNullNot

				&&	data.IntField					== IntField
				&&	data.IntFieldNull				== null
					&&	data.IntFieldNull			== IntFieldNull
				&&	data.IntFieldNullNot			== IntFieldNullNot

				&&	data.Int64Field					== Int64Field
				&&	data.Int64FieldNull				== null
					&&	data.Int64FieldNull			== Int64FieldNull
				&&	data.Int64FieldNullNot			== Int64FieldNullNot

				&&	Helper.IsEqual(data.DoubleField, DoubleField)
				&&	data.DoubleFieldNull			== null
					&& data.DoubleFieldNull			== DoubleFieldNull
				&&	Helper.IsEqual(data.DoubleFieldNullNot, DoubleFieldNullNot) 

				&&	Helper.IsEqual(
								data.DateTimeField, 
								DateTimeField)
				&&	data.DateTimeFieldNull					== null
					&& data.DateTimeFieldNull				== DateTimeFieldNull
				&&	Helper.IsEqual(
								data.DateTimeFieldNullNot,
								DateTimeFieldNullNot)

				&&	data.GuidField					== GuidField
				&&	data.GuidFieldNull				== null
					&& data.GuidFieldNull			== GuidFieldNull
				&&	data.GuidFieldNullNot			== GuidFieldNullNot

				&&	data.StringField				== StringField
				&&	data.StringFieldNull			== StringFieldNull

				&&	Helper.IsEqual(
								data.BinaryField,
								BinaryField)
				&&	BinaryFieldNull == null

				// intrinsic morphs + Nullable. 7 * 4 = 24
				&&	data.ByteFieldMin				== ByteFieldMin
				&&	data.ByteFieldMax				== ByteFieldMax
				&&	data.ByteFieldNull				== ByteFieldNull
				&&	data.ByteFieldNullNot			== ByteFieldNullNot

				&&	data.SByteFieldMin				== SByteFieldMin
				&&	data.SByteFieldMax				== SByteFieldMax
				&&	data.SByteFieldNull				== SByteFieldNull
				&&	data.SByteFieldNullNot			== SByteFieldNullNot

				&&	data.Int16FieldMin				== Int16FieldMin
				&&	data.Int16FieldMax				== Int16FieldMax
				&&	data.Int16FieldNull				== Int16FieldNull
				&&	data.Int16FieldNullNot			== Int16FieldNullNot

				&&	data.UInt16FieldMin				== UInt16FieldMin
				&&	data.UInt16FieldMax				== UInt16FieldMax
				&&	data.UInt16FieldNull			== UInt16FieldNull
				&&	data.UInt16FieldNullNot			== UInt16FieldNullNot

				&&	data.UInt32FieldMin				== UInt32FieldMin
				&&	data.UInt32FieldMax				== UInt32FieldMax
				&&	data.UInt32FieldNull			== UInt32FieldNull
				&&	data.UInt32FieldNullNot			== UInt32FieldNullNot

				&&	data.UInt64FieldMin				== UInt64FieldMin
				&&	data.UInt64FieldMax				== UInt64FieldMax				
				&&	data.UInt64FieldNull			== UInt64FieldNull
				&&	data.UInt64FieldNullNot			== UInt64FieldNullNot

				&&	data.CharFieldMin				== CharFieldMin
				&&  data.CharFieldMax				== CharFieldMax
				&&	data.CharFieldNull				== CharFieldNull
				&&  data.CharFieldNullNot			== CharFieldNullNot

				// Enums - 8
				&&  data.ByteEnumField				== ByteEnumField
				&&  data.ByteEnumFieldNull			== ByteEnumFieldNull
				&&  data.ByteEnumFieldNullNot		== ByteEnumFieldNullNot
				
				&&  data.SByteEnumField				== SByteEnumField
				&&  data.SByteEnumFieldNull			== SByteEnumFieldNull
				&&  data.SByteEnumFieldNullNot		== SByteEnumFieldNullNot

				&&  data.Int16EnumField				== Int16EnumField
				&&  data.Int16EnumFieldNull			== Int16EnumFieldNull
				&&  data.Int16EnumFieldNullNot		== Int16EnumFieldNullNot

				&&  data.UInt16EnumField			== UInt16EnumField
				&&  data.UInt16EnumFieldNull		== UInt16EnumFieldNull
				&&  data.UInt16EnumFieldNullNot		== UInt16EnumFieldNullNot

				&&  data.Int32EnumField				== Int32EnumField
				&&  data.Int32EnumFieldNull			== Int32EnumFieldNull
				&&  data.Int32EnumFieldNullNot		== Int32EnumFieldNullNot

				&&  data.UInt32EnumField			== UInt32EnumField
				&&  data.UInt32EnumFieldNull		== UInt32EnumFieldNull
				&&  data.UInt32EnumFieldNullNot		== UInt32EnumFieldNullNot

				&&  data.Int64EnumField				== Int64EnumField
				&&  data.Int64EnumFieldNull			== Int64EnumFieldNull
				&&  data.Int64EnumFieldNullNot		== Int64EnumFieldNullNot

				&&  data.UInt64EnumField			== UInt64EnumField
				&&  data.UInt64EnumFieldNull		== UInt64EnumFieldNull
				&&  data.UInt64EnumFieldNullNot		== UInt64EnumFieldNullNot
			
				// Object
				&&	data.PersonInfo.Equals(PersonInfo)
			;
		}		

		public 
		override
		int
		GetHashCode()
		{
			return (PartitionKey + RowKey).GetHashCode();
		}

		// IDataHelper
		public
		void 
		UpdateDateTime()
		{
			DateTimeField = DateTime.UtcNow;
		}

		public
		virtual
		void
		UpdateForMerge()
		{
			// NOTE: Only DateTime is modified. The rest are nulled if nullable or remain unchanged.

			StringField				=	null;	
	    
			BoolFieldNullNot		=	null;

			IntFieldNullNot			=	null;	

			Int64FieldNullNot		=	null;	

			DoubleFieldNullNot		=	null;	

			DateTimeField			=	new DateTime(2012, 1, 1);
			DateTimeFieldNullNot	=	null;	

			GuidFieldNullNot		=	null;	

			BinaryField				=	null;	

			// intrinsic morphs + Nullable
			ByteFieldNull			=	null;
			ByteFieldNullNot		=	null;	

			SByteFieldNullNot		=	null;

			Int16FieldNullNot		=	null;

			UInt16FieldNullNot		=	null;

			UInt32FieldNullNot		=	null;

			UInt64FieldNullNot		=	null;	
			
			CharFieldNullNot		=	null;

			// Enums - 8
			ByteEnumFieldNullNot	=	null;	

			SByteEnumFieldNullNot	=	null;

			Int16EnumFieldNullNot	=	null;

			UInt16EnumFieldNullNot	=	null;

			Int32EnumFieldNullNot	=	null;	

			UInt32EnumFieldNullNot	=	null;

			Int64EnumFieldNullNot	=	null;

			UInt64EnumFieldNullNot	=	null;
		
			//Object
			// Create a derived generic object type to validate correct de-serialization
			var employeeInfo = EmployeeInfo<int>.CreateNew();
			employeeInfo.Dummy = 102;

			PersonInfo				=	employeeInfo;
		}

	    public
		virtual
	    void
	    Populate(
	        int									stringSize,
	        int									binarySize)
	    {
			PartitionKey			= typeof(AllDataExplicit).Name;
			RowKey					= Guid.NewGuid().ToString();

			// Fields + Nullable. 8 + 8 + 6 = 22
			StringField				=	DataGenerator.GetStringSizeFixed(stringSize);
			StringFieldNull			=	null;
	    
			BoolField				=	DataGenerator.Rnd.Next(2) == 1;
			BoolFieldNull			=	null;
			BoolFieldNullNot		=	DataGenerator.Rnd.Next(2) == 1;

			IntField				=	DataGenerator.Rnd.Next(Int32.MaxValue);
			IntFieldNull			=	null;
			IntFieldNullNot			=	DataGenerator.Rnd.Next(int.MaxValue);

			Int64Field				=	(Int64) (DataGenerator.Rnd.Next(Int32.MaxValue) * DataGenerator.Rnd.Next(Int32.MaxValue));
			Int64FieldNull			=	null;
			Int64FieldNullNot		=	(Int64) (DataGenerator.Rnd.Next(Int32.MaxValue) * DataGenerator.Rnd.Next(Int32.MaxValue));

			DoubleField				=	DataGenerator.Rnd.NextDouble() * Double.MaxValue;
			DoubleFieldNull			=	null;
			DoubleFieldNullNot		=	DataGenerator.Rnd.NextDouble() * Double.MaxValue;

			DateTimeField			=	DateTime.UtcNow;
			DateTimeFieldNull		=	null;
			DateTimeFieldNullNot	=	DateTime.UtcNow;

			GuidField				=	Guid.NewGuid();
			GuidFieldNull			=	null;
			GuidFieldNullNot		=	Guid.NewGuid();

			BinaryField				=	DataGenerator.GetBytesSizeFixed(binarySize);
			BinaryFieldNull			=	null;

			// intrinsic morphs + Nullable. 7 * 4 = 28
			ByteFieldMin			=	Byte.MinValue;
			ByteFieldMax			=	Byte.MaxValue;
			ByteFieldNull			=	null;
			ByteFieldNullNot		=	(byte) DataGenerator.Rnd.Next(byte.MaxValue);

			SByteFieldMin			=	SByte.MinValue;
			SByteFieldMax			=	SByte.MaxValue;
			SByteFieldNull			=	null;
			SByteFieldNullNot		=	(sbyte) DataGenerator.Rnd.Next(sbyte.MaxValue);

			Int16FieldMin			=	Int16.MinValue;
			Int16FieldMax			=	Int16.MaxValue;			
			Int16FieldNull			=	null;
			Int16FieldNullNot		=	(Int16) DataGenerator.Rnd.Next(Int16.MaxValue);

			UInt16FieldMin			=	UInt16.MinValue;
			UInt16FieldMax			=	UInt16.MaxValue;			
			UInt16FieldNull			=	null;
			UInt16FieldNullNot		=	(UInt16) DataGenerator.Rnd.Next(UInt16.MaxValue);

			UInt32FieldMin			=	UInt32.MinValue;
			UInt32FieldMax			=	UInt32.MaxValue;			
			UInt32FieldNull			=	null;
			UInt32FieldNullNot		=	(UInt32) DataGenerator.Rnd.Next(UInt16.MaxValue);

			UInt64FieldMin			=	UInt64.MinValue;
			UInt64FieldMax			=	UInt64.MaxValue;
			UInt64FieldNull			=	null;
			UInt64FieldNullNot		=	(UInt64) (DataGenerator.Rnd.Next(Int32.MaxValue) * DataGenerator.Rnd.Next(Int32.MaxValue));
			
			CharFieldMin			=	Char.MinValue;
			CharFieldMax			=	Char.MaxValue;
			CharFieldNull			=	null;
			CharFieldNullNot		=	(Char) DataGenerator.Rnd.Next(Char.MaxValue);

			// Enums - 8
			ByteEnumField			=	ByteEnum.Value2;
			ByteEnumFieldNull		=	null;
			ByteEnumFieldNullNot	=	ByteEnum.Value1;

			SByteEnumField			=	SByteEnum.Value2;
			SByteEnumFieldNull		=	null;
			SByteEnumFieldNullNot	=	SByteEnum.Value1;

			Int16EnumField			=	Int16Enum.Value2;
			Int16EnumFieldNull		=	null;
			Int16EnumFieldNullNot	=	Int16Enum.Value1;

			UInt16EnumField			=	UInt16Enum.Value2;
			UInt16EnumFieldNull		=	null;
			UInt16EnumFieldNullNot	=	UInt16Enum.Value1;

			Int32EnumField			=	Int32Enum.Value2;
			Int32EnumFieldNull		=	null;
			Int32EnumFieldNullNot	=	Int32Enum.Value1;

			UInt32EnumField			=	UInt32Enum.Value2;
			UInt32EnumFieldNull		=	null;
			UInt32EnumFieldNullNot	=	UInt32Enum.Value1;

			Int64EnumField			=	Int64Enum.Value2;
			Int64EnumFieldNull		=	null;
			Int64EnumFieldNullNot	=	Int64Enum.Value1;

			UInt64EnumField			=	UInt64Enum.Value2;
			UInt64EnumFieldNull		=	null;
			UInt64EnumFieldNullNot	=	UInt64Enum.Value1;
		
			// Object
			// Create a derived generic object type to validate correct de-serialization
			var employeeInfo = EmployeeInfo<int>.CreateNew();
			employeeInfo.Dummy = 101;

			PersonInfo				=	employeeInfo;

		}

		public	
		IQueryable
		GetQuery(
			StashClient<AllDataExplicit>		stashClient,
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
	// Enum classes
	// -----------------------------------------------------------------------------------------------------------------

	public enum ByteEnum : byte
	{
		Value1,
		Value2,
		Value3,
	}

	public enum SByteEnum : sbyte
	{
		Value1,
		Value2,
		Value3,
	}

	public enum Int16Enum : short
	{
		Value1,
		Value2,
		Value3,
	}

	public enum UInt16Enum : ushort
	{
		Value1,
		Value2,
		Value3,
	}

	public enum Int32Enum : int
	{
		Value1,
		Value2,
		Value3,
	}

	public enum UInt32Enum : uint
	{
		Value1,
		Value2,
		Value3,
	}

	public enum Int64Enum : long
	{
		Value1,
		Value2,
		Value3,
	}

	public enum UInt64Enum : ulong
	{
		Value1,
		Value2,
		Value3,
	}
}
