using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeSuperior.Lucifure;

namespace Lucifure.Stash.Test.Exceptions
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	[TestClass]
	public 
	partial 
	class TestForExceptions 
	{
		public 
		void
		Common<T>(
			int									error)
		{
			bool								isSuccess;

			try
			{
				StashConfiguration.GetClient<T>();

				isSuccess = false;
			}
			catch (StashCompiletimeException stashEx)
			{
				Assert.IsTrue(stashEx.Error == StashError.StashClientCompiletime);

				Assert.IsTrue(
					stashEx.Messages
					.Where(m => m.Error == error)
					.ToList().Count == 1);

				isSuccess = true;
			}
			catch (Exception)
			{
				isSuccess = false;
			}

			Assert.IsTrue(isSuccess);
		}

		public 
		void
		Common<T>(
			int[]								errors)
		{
			bool								isSuccess;

			try
			{
				StashConfiguration.GetClient<T>();
			
				isSuccess = false;
			}
			catch (StashCompiletimeException stashEx)
			{
				Assert.IsTrue(stashEx.Error == StashError.StashClientCompiletime);

				Assert.IsTrue(
					stashEx.Messages
					.Where(m => errors.Contains(m.Error))
					.ToList().Count == errors.Length);

				isSuccess = true;
			}
			catch (Exception)
			{
				isSuccess = false;
			}

			Assert.IsTrue(isSuccess);
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

}
