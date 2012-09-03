using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;
using System.Globalization;

namespace Lucifure.Stash.Test.ComplexKeys
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Morphs a date time to a date
	/// </summary>
	class MorphDateTimeToDate				:	IStashMorph
	{
			static
			readonly
			string								_format = "yyyyMMdd";						

		public 
		bool  
		CanMorph(
			Type								type)
		{
 			return type == typeof(DateTime);
		}

		public 
        bool
        IsCollationEquivalent
		{
			get { return true; }	// indicates that the collating sequence of the original and morphed values are identical  
		}

		public 
		object  
		Into(
			object								value)
		{
			return ((DateTime) value).ToString(_format);
		}

		public 
		object  
		Outof(
			object								value)
		{
 			return DateTime.ParseExact(
									value.ToString(), 
									_format, 
									CultureInfo.InvariantCulture);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
	/// <summary>
	// Holds the number of seconds into the day + a GUID to make it unique
	/// </summary>
	class SecondsUnique
	{
			public
			const int							KeyLength = 5 + 1 + 36;

			const
			char								_delimiter = ':';

			static
			readonly
			char[]								_delimiters = new [] { _delimiter } ;

			const
			string								_formatHalf = "{0:00000}";

			static
			readonly
			string								_formatFull = _formatHalf + _delimiter + "{1}";


			readonly
			int									_seconds;

			readonly
			Guid								_unique;

		public
		SecondsUnique()
		:
			this(DateTime.Now)
		{
		}

		/// <summary>
		/// Use this to pass in the Guid. If Guid is empty, it does not factor in the string key
		/// </summary>
		public
		SecondsUnique(
			DateTime							dateTime,
			Guid								unique)
		{
			dateTime = dateTime.ToUniversalTime();

			_seconds	= (int) dateTime.Subtract(dateTime.Date).TotalSeconds;

			_unique		= unique;
		}

		public
		SecondsUnique(
			DateTime							dateTime)
		:
			this(dateTime, Guid.NewGuid())
		{
		}

		public
		SecondsUnique(
			string								value)
		{
			value = value ?? "";

			string[]
			parts = value.Split(_delimiters);

			if (parts.Length == 0)
			{
				_seconds = 0;
				_unique = Guid.Empty;
			}
			else 
			{
				if (parts.Length >= 1)
				{
					Int32.TryParse(parts[0], out _seconds);
				}
				if (parts.Length >= 2)
				{
					Guid.TryParse(parts[1], out _unique);
				}			
			}
		}

		public
		override
		string
		ToString()
		{
			return	String.Format(
							_unique != Guid.Empty
								?	_formatFull
								:	_formatHalf,
							_seconds,
							_unique);	
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Morpher associated with MorphSeconds
	/// </summary>
	class MorphSecondsUnique				:	IStashMorph, IStashKeyMediate
	{
		public 
		bool  
		CanMorph(
			Type								type)
		{
 			return type == typeof(SecondsUnique);
		}

		public 
        bool
        IsCollationEquivalent
		{
			get { return true; }	// indicates that the collating sequence of the original and morphed values are identical  
		}

		public 
		object  
		Into(
			object								value)
		{
			return ((SecondsUnique) value).ToString();
		}

		public 
		object  
		Outof(
			object								value)
		{
 			return new SecondsUnique(value.ToString());
		}

		// IStashKeyMediate
		public 
		bool 
		IsCompleteKeyValue(
			string								value) 
		{
			return value.Length == SecondsUnique.KeyLength;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[StashEntity(Name="WebLogger", Mode=StashMode.Explicit)]
	class Log
	{
		[StashPartitionKey(Morpher=typeof(MorphDateTimeToDate))]
		public 
		DateTime								Date;

		[StashRowKey(Morpher=typeof(MorphSecondsUnique), KeyMediator=typeof(MorphSecondsUnique))]
		public 
		SecondsUnique							Seconds;
	
		[Stash]
		public 
		DateTime								Moment;

		public
		Log()
		{
		}

		public
		Log(
			DateTime							timestamp)
		{
			Date	= timestamp;
			Seconds = new SecondsUnique(timestamp);
			Moment  = timestamp;
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	class TestComplexKeys
	{
		[TestMethod]
		public 
		void 
		ComplexKeySingle()
		{
			var 
			client= StashConfiguration.GetClient<Log>();

			client.CreateTableIfNotExist();
	
			DateTime
			dt = DateTime.Now;

			// Insert
			Log
			log = new Log(dt);

			client.Insert(log);

			var
			logs = client
						.CreateQuery()
						.Where(x => x.Date == dt && x.Seconds == log.Seconds)
						.ToList();

			Assert.IsTrue(logs.Count == 1);
		}

		[TestMethod]
		public 
		void 
		ComplexKeyRange()
		{
			var 
			client= StashConfiguration.GetClient<Log>();

			client.CreateTableIfNotExist();
	
			DateTime
			dt = DateTime.Now;

			// Insert
			const int 
			count = 3;

			for(int i = 0; i < count; ++i)
				client.Insert(new Log(dt.AddSeconds(0)));
			for(int i = 0; i < count; ++i)
				client.Insert(new Log(dt.AddSeconds(1)));
			for(int i = 0; i < count; ++i)
				client.Insert(new Log(dt.AddSeconds(2)));

			SecondsUnique
			secondsUnique = new SecondsUnique(dt.AddSeconds(1), Guid.Empty);

			var
			logs = client
						.CreateQuery()
						.Where(x => x.Date == dt && x.Seconds == secondsUnique)
						.ToList();

			Assert.IsTrue(logs.Count == count);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}