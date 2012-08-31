using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Query
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	class TestGeneric
	{
		void
		QueryOnEachDatatype<T>(
			IQueryable<T>						query,
			int									resultCount)
		{
			List<T>
			items = query.ToList();

			if (items.Count != resultCount)
				Assert.Fail();
		}

		[TestMethod]
		public 
		void AllDataImplicitQueryOnEachDatatype()
		{
			// setup
			AllDataImplicit
			item = TypeFactory<AllDataImplicit>.CreateRandomSmall();
			
			StashClient<AllDataImplicit>
			client = StashConfiguration.GetClient<AllDataImplicit>();

			client.Insert(item);

			IQueryable<AllDataImplicit>
			query = client.CreateQuery();

			// queries bool
			bool
			boolProperty = item.BoolProperty;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolProperty == boolProperty), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolProperty == item.BoolProperty), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.BoolPropertyNull == boolProperty), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.BoolPropertyNull == item.BoolProperty), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolPropertyNullNot == item.BoolPropertyNullNot), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolPropertyNullNot == true), 
			    item.BoolPropertyNullNot == true
					?	1
					:	0);

			// queries int
			int 
			intProperty = item.IntProperty;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntProperty == intProperty), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntProperty == item.IntProperty), 
			    1);
		
			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.IntPropertyNull == intProperty), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.IntPropertyNull == item.IntProperty), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntPropertyNullNot == item.IntPropertyNullNot), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntPropertyNullNot == 420), 
			    item.IntPropertyNullNot == 420
					?	1
					:	0);

			// queries int64
			Int64
			int64Property = item.Int64Property;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64Property == int64Property), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64Property == item.Int64Property), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int64PropertyNull == int64Property), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int64PropertyNull == item.Int64Property), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64PropertyNullNot == item.Int64PropertyNullNot), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64PropertyNullNot == 420), 
			    item.Int64PropertyNullNot == 420
					?	1
					:	0);

			// queries double
			double
			doubleProperty = item.DoubleProperty;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoubleProperty == doubleProperty), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoubleProperty == item.DoubleProperty), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DoublePropertyNull == doubleProperty), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DoublePropertyNull == item.DoubleProperty), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoublePropertyNullNot == item.DoublePropertyNullNot), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoublePropertyNullNot == 420), 
			    item.DoublePropertyNullNot == 420
					?	1
					:	0);

			// queries DateTime
			DateTime
			dateTimeProperty = item.DateTimeProperty;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimeProperty == dateTimeProperty), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimeProperty == item.DateTimeProperty), 
			    1);
		
			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DateTimePropertyNull == dateTimeProperty), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DateTimePropertyNull == item.DateTimeProperty), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimePropertyNullNot == item.DateTimePropertyNullNot), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimePropertyNullNot == new DateTime(2000, 1, 1)), 
			    item.DateTimePropertyNullNot == new DateTime(2000, 1, 1)
					?	1
					:	0);

			// queries Guid
			Guid
			guidProperty = item.GuidProperty;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidProperty == guidProperty), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidProperty == item.GuidProperty), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.GuidPropertyNull == guidProperty), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.GuidPropertyNull == item.GuidProperty), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidPropertyNullNot == item.GuidPropertyNullNot), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidPropertyNullNot == Guid.NewGuid()), 
			    0);	// should never match

			// queries string
			string
			stringProperty = item.StringProperty;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.StringProperty == stringProperty), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.StringProperty == item.StringProperty), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.StringProperty == "ABC"), 
			    item.StringProperty == "ABC"
					?	1
					:	0);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.StringPropertyNull == stringProperty), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.StringPropertyNull == item.StringProperty), 
					0);
			}
						
			// cleanup		
			client.Delete(item);	
		}

		[TestMethod]
		public 
		void AllDataExplicitQueryOnEachDatatype()
		{
			// setup
			AllDataExplicit
			item = TypeFactory<AllDataExplicit>.CreateRandomSmall();
			
			StashClient<AllDataExplicit>
			client = StashConfiguration.GetClient<AllDataExplicit>();

			client.Insert(item);

			IQueryable<AllDataExplicit>
			query = client.CreateQuery();


			// queries bool
			bool 
			boolField = item.BoolField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolField == boolField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolField == item.BoolField), 
			    1);

			// queries bool null

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.BoolFieldNull == boolField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.BoolFieldNull == item.BoolField), 
					0);
			}

			// queries bool null not
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.BoolFieldNullNot == item.BoolFieldNullNot), 
			    1);

			// queries int
			int 
			intField = item.IntField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntField == intField), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntField == item.IntField), 
			    1);

			// queries int null

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.IntFieldNull == intField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.IntFieldNull == item.IntField), 
					0);
			}

			// queries int null not
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.IntFieldNullNot == item.IntFieldNullNot), 
			    1);

			// queries int64
			Int64 
			int64Field = item.Int64Field;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64Field == int64Field), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64Field == item.Int64Field), 
			    1);

			// queries int64 null
			
			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int64FieldNull == int64Field), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int64FieldNull == item.Int64Field), 
					0);
			}

			// queries int64 null not
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64FieldNullNot == item.Int64FieldNullNot), 
			    1);

			// queries double
			double 
			doubleField = item.DoubleField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoubleField == doubleField), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoubleField == item.DoubleField), 
			    1);

			// queries double null
			
			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DoubleFieldNull == doubleField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DoubleFieldNull == item.DoubleField), 
					0);
			}

			// queries double null not
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DoubleFieldNullNot == item.DoubleFieldNullNot), 
			    1);

			// queries DateTime
			DateTime
			dateTimeField = item.DateTimeField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimeField == dateTimeField), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimeField == item.DateTimeField), 
			    1);

			// queries DateTime null

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DateTimeField == dateTimeField), 
					1);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.DateTimeField == item.DateTimeField), 
					1);
			}

			// queries DateTime null not
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.DateTimeFieldNullNot == item.DateTimeFieldNullNot), 
			    1);

			// queries Guid
			Guid
			guidField = item.GuidField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidField == guidField), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidField == item.GuidField), 
			    1);

			// queries Guid null 

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.GuidFieldNull == guidField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.GuidFieldNull == item.GuidField), 
					0);
			}

			// queries Guid null not
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.GuidFieldNullNot == item.GuidFieldNullNot), 
			    1);

			// queries string
			string 
			stringField = item.StringField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.StringField == stringField), 
			    1);
		
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.StringField == item.StringField), 
			    1);

			// queries string null

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.StringFieldNull == stringField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.StringFieldNull == item.StringField), 
					0);
			}
					
			// ---------------------------------------------------------------------------------------------------------
			// Implicit Morph

			// byte
			byte
			byteFieldMin = item.ByteFieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteFieldMin == byteFieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteFieldMin == item.ByteFieldMin), 
			    1);

			byte
			byteFieldMax = item.ByteFieldMax;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteFieldMax == byteFieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteFieldMax == item.ByteFieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.ByteFieldNull == item.ByteFieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteFieldNullNot == item.ByteFieldNullNot), 
			    1);

			// sbyte
			sbyte
			sbyteFieldMin = item.SByteFieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteFieldMin == sbyteFieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteFieldMin == item.SByteFieldMin), 
			    1);

			sbyte
			sbyteFieldMax = item.SByteFieldMax;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteFieldMax == sbyteFieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteFieldMax == item.SByteFieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.SByteFieldNull == item.ByteFieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteFieldNullNot == item.SByteFieldNullNot), 
			    1);

			// int16
			Int16
			int16FieldMin = item.Int16FieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16FieldMin == int16FieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16FieldMin == item.Int16FieldMin), 
			    1);

			Int16
			int16FieldMax = item.Int16FieldMax;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16FieldMax == int16FieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16FieldMax == item.Int16FieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int16FieldNull == item.Int16FieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16FieldNullNot == item.Int16FieldNullNot), 
			    1);

			// uint16
			UInt16
			uint16FieldMin = item.UInt16FieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16FieldMin == uint16FieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16FieldMin == item.UInt16FieldMin), 
			    1);

			UInt16
			uint16FieldMax = item.UInt16FieldMax;		
	
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16FieldMax == uint16FieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16FieldMax == item.UInt16FieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt16FieldNull == item.UInt16FieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16FieldNullNot == item.UInt16FieldNullNot), 
			    1);

			// uint32
			UInt32
			uint32FieldMin = item.UInt32FieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32FieldMin == uint32FieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32FieldMin == item.UInt32FieldMin), 
			    1);

			UInt32
			uint32FieldMax = item.UInt32FieldMax;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32FieldMax == uint32FieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32FieldMax == item.UInt32FieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt32FieldNull == item.UInt32FieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32FieldNullNot == item.UInt32FieldNullNot), 
			    1);

			// uint64
			UInt64
			uint64FieldMin = item.UInt64FieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64FieldMin == uint64FieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64FieldMin == item.UInt64FieldMin), 
			    1);

			UInt64
			uint64FieldMax = item.UInt64FieldMax;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64FieldMax == uint64FieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64FieldMax == item.UInt64FieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt64FieldNull == item.UInt64FieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64FieldNullNot == item.UInt64FieldNullNot), 
			    1);

			// char
			char
			charFieldMin = item.CharFieldMin;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.CharFieldMin == charFieldMin), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.CharFieldMin == item.CharFieldMin), 
			    1);

			char
			charFieldMax = item.CharFieldMax;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.CharFieldMax == charFieldMax), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.CharFieldMax == item.CharFieldMax), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.CharFieldNull == item.CharFieldMax), 
					0);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.CharFieldNullNot == item.CharFieldNullNot), 
			    1);

			// Enum byte
			ByteEnum
			byteEnumField = item.ByteEnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteEnumField == byteEnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteEnumField == item.ByteEnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.ByteEnumFieldNull == byteEnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.ByteEnumFieldNull == item.ByteEnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.ByteEnumFieldNullNot == item.ByteEnumFieldNullNot), 
			    1);

			// Enum SByte
			SByteEnum
			sbyteEnumField = item.SByteEnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteEnumField == sbyteEnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteEnumField == item.SByteEnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.SByteEnumFieldNull == sbyteEnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.SByteEnumFieldNull == item.SByteEnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.SByteEnumFieldNullNot == item.SByteEnumFieldNullNot), 
			    1);

			// Enum Int16
			Int16Enum
			int16EnumField = item.Int16EnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16EnumField == int16EnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16EnumField == item.Int16EnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int16EnumFieldNull == int16EnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int16EnumFieldNull == item.Int16EnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int16EnumFieldNullNot == item.Int16EnumFieldNullNot), 
			    1);

			// Enum UInt16
			UInt16Enum
			uint16EnumField = item.UInt16EnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16EnumField == uint16EnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16EnumField == item.UInt16EnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt16EnumFieldNull == uint16EnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt16EnumFieldNull == item.UInt16EnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt16EnumFieldNullNot == item.UInt16EnumFieldNullNot), 
			    1);

			// Enum Int32
			Int32Enum
			int32EnumField = item.Int32EnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int32EnumField == int32EnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int32EnumField == item.Int32EnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int32EnumFieldNull == int32EnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int32EnumFieldNull == item.Int32EnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int32EnumFieldNullNot == item.Int32EnumFieldNullNot), 
			    1);

			// Enum UInt32
			UInt32Enum
			uint32EnumField = item.UInt32EnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32EnumField == uint32EnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32EnumField == item.UInt32EnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt32EnumFieldNull == uint32EnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt32EnumFieldNull == item.UInt32EnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt32EnumFieldNullNot == item.UInt32EnumFieldNullNot), 
			    1);

			// Enum Int64
			Int64Enum
			int64EnumField = item.Int64EnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64EnumField == int64EnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64EnumField == item.Int64EnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int64EnumFieldNull == int64EnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.Int64EnumFieldNull == item.Int64EnumField), 
					0);
			}

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.Int64EnumFieldNullNot == item.Int64EnumFieldNullNot), 
			    1);

			// Enum UInt64
			UInt64Enum
			uint64EnumField = item.UInt64EnumField;

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64EnumField == uint64EnumField), 
			    1);

			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64EnumField == item.UInt64EnumField), 
			    1);

			// Query against a property not in the row not supported by development storage so will fail
			// http://msdn.microsoft.com/en-us/library/windowsazure/gg433135.aspx
			if (StashConfiguration.IsConfigurationCloud)
			{
				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt64EnumFieldNull == uint64EnumField), 
					0);

				QueryOnEachDatatype(
					query.Where(t => t.RowKey == item.RowKey && t.UInt64EnumFieldNull == item.UInt64EnumField), 
					0);
			}
				
			QueryOnEachDatatype(
			    query.Where(t => t.RowKey == item.RowKey && t.UInt64EnumFieldNullNot == item.UInt64EnumFieldNullNot), 
			    1);

			// cleanup		
			client.Delete(item);	
		}

	}
}
