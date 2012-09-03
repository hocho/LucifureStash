using System;
using System.Runtime.Serialization;

namespace Lucifure.Stash.Test
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[DataContract]
	[KnownType(typeof(EmployeeInfo<int>))]
	public 
	class PersonInfo
	{
			// explicitly have to opt in to serialize member if used as a base class
			[DataMember]
			public
			string								NameFirst { get; set; }

			[DataMember]
			public 
			string								NameLast { get; set; }

			[DataMember]
			public
			DateTime							Dob { get; set;  }

			[DataMember]
			public 
			bool								IsMarried { get; set; }


		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (PersonInfo) obj;
		
			return		NameFirst	== data.NameFirst
					&&	NameLast	== data.NameLast
					&&	Dob			== data.Dob
					&&	IsMarried	== data.IsMarried;
		}

		public
		override
		int
		GetHashCode()
		{
			return		NameFirst.GetHashCode() 
					^	NameLast.GetHashCode()
					^	Dob.GetHashCode()
					^	IsMarried.GetHashCode();
		}

		public
		virtual 
        void
		Populate()
		{
			NameFirst	= "Lucif";
			NameLast	= "Ure";
			Dob			= new DateTime(1991, 1, 1);
			IsMarried	= false;
		}

		public 
		static 
		PersonInfo
		CreateNew()
		{
			PersonInfo
			result = new PersonInfo();

			result.Populate();

			return result;
		}
	}		 

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
	// For base / derived class serialization test

	[DataContract]
	public 
	class EmployeeInfo<T>					:	PersonInfo
	{
			[DataMember]
			public
			DateTime							DateOfHire { get; set;  }

			[DataMember]
			public
			T									Dummy { get; set;  }


		public
		override
		bool
		Equals(
			Object								obj)
		{
			var data = (EmployeeInfo<T>) obj;
		
			return	base.Equals(obj as PersonInfo) 
							&& DateOfHire == data.DateOfHire
							&& Dummy.Equals(data.Dummy);
		}

		public
		override
		int
		GetHashCode()
		{
			return		base.GetHashCode() 
					^	DateOfHire.GetHashCode();
		}

		public
		override 
        void
		Populate()
		{
			base.Populate();

			var 
			now = DateTime.UtcNow;

			DateOfHire = new DateTime(
									now.Year - 1, 
									now.Month,
									now.Day);
		}

		public 
		static 
		new
		EmployeeInfo<T>
		CreateNew()
		{
			var
			result = new EmployeeInfo<T>();

			result.Populate();

			return result;
		}
	}		 

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
