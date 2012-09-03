using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucifure.Stash.Test 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public
	static  
	class Helper 
	{
		public 
		static 
		void 
		ForEach<T>(
			this								IEnumerable<T> source, 
			Action<T>							action)
		{
			foreach (var item in source)
				action(item);
		}

		// used to combat rounding errors to/from string
		public 
		static 
		bool
		IsEqual(
			double								lhs,
			double								rhs)
		{
			return Math.Abs(lhs - rhs) < (0.00001 * lhs);
		}

		public 
		static 
		bool
		IsEqual(
			double ?							lhs,
			double ?							rhs)
		{
			return		(	lhs != null	
						&&	rhs != null
						&&	Math.Abs(lhs.Value - rhs.Value) < (0.00001 * lhs.Value))
					||
						(	lhs == null
						&&	rhs == null);
		}

		// used to combat granularity of storage emulator
		public 
		static 
		bool
		IsEqual(
			DateTime							lhs,
			DateTime							rhs)
		{
			return Math.Abs(lhs.ToUniversalTime().Ticks - rhs.ToUniversalTime().Ticks) <= 9999;
		}

		public 
		static 
		bool
		IsEqual(
			DateTime ?							lhs,
			DateTime ?							rhs)
		{
			return		(	lhs != null 
						&&	rhs	!= null
						&&	IsEqual(lhs.Value, rhs.Value))
					||
						(	lhs == null 
						&&	rhs == null);
		}

		public 
		static 
		bool
		AreNull<T>(
			T									lhs,
			T									rhs)
		where 
			T								:	class
		{
			return lhs == null && rhs == null;
		}

		public 
		static 
		bool
		AreNotNull<T>(
			T									lhs,
			T									rhs)
		where 
			T								:	class
		{
			return lhs != null && rhs != null;
		}

		public 
		static 
		bool
		IsEqual<T>(
			IEnumerable<T>						lhs,
			IEnumerable<T>						rhs)
		{
			return	AreNull(lhs, rhs)	
					||
					(AreNotNull(lhs, rhs) && Enumerable.SequenceEqual(lhs, rhs));
		}

		public 
		static 
		bool
		Is(
			this string							str)
		{
			return !String.IsNullOrWhiteSpace(str);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
