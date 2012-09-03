using System;
using System.Linq;
using System.Text;

namespace Lucifure.Stash.Test
{
    // ---------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------

    static
    class DataGenerator
    {
        static
        public
        Random									Rnd = new Random();

		static 
		public
		readonly
		string									RandomTableNamePrefix = "RndTbl";

        static
        public
        byte[]
        GetBytesSizeRandom(
            int maxSize)
        {
            return GetBytesSizeFixed(Rnd.Next(maxSize));
        }

        static
        public
        byte[]
        GetBytesSizeFixed(
            int size)
        {
            byte[]
            result = new byte[size];

            Rnd.NextBytes(result);

            return result;
        }

        static
        public
        string
        GetStringSizeRandom(
            int maxLen)
        {
            return GetStringSizeFixed(Rnd.Next(maxLen));
        }

        static
        public
        string
        GetStringSizeFixed(
            int len)
        {
            StringBuilder
            sb = new StringBuilder();

            for (int i = 0; i < len; ++i)
                sb.Append((Char)Rnd.Next(32, 128));

            return sb.ToString();
        }

        static
        public
        string
        GetRandomTableName()
        {
            StringBuilder
            sb = new StringBuilder();

			sb.Append(RandomTableNamePrefix);

            for (int i = 0; i < 10; ++i)
                sb.Append((Char)Rnd.Next(97, 97 + 26));

            return sb.ToString();
        }

		static 
		public 
		bool
		Compare(
			object							lhs,
			object							rhs)
		{
			bool							result;
			Type							type;

			if (lhs != null && rhs != null && (type = lhs.GetType()) == rhs.GetType())
			{
				if (type == typeof(double))  				
					result = Helper.IsEqual((double)lhs, (double) rhs);

				else if (type == typeof(DateTime))  				
					result = Helper.IsEqual((DateTime)lhs, (DateTime) rhs);

				else if (type == typeof(byte[]))
					result = Enumerable.SequenceEqual(
													(byte[]) lhs,
													(byte[]) rhs);
				else
					result = lhs.Equals(rhs);
			}
			else 
				throw new ApplicationException("To compare, the data type of both lhs and rhs should be the same.");
		
			return result;
		}
    }

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

}
