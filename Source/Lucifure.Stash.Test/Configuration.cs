//If you have the Storage Client installed and want to use it, uncomment the following line
//#define USE_STORAGE_CLIENT
using System;
using System.Configuration;
using System.Diagnostics;

using CodeSuperior.Lucifure;

#if USE_STORAGE_CLIENT
using Microsoft.WindowsAzure;
#endif

namespace Lucifure.Stash.Test 
{
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public 
	enum ConfigurationType
	{
		StashCloud,
		StashEmulator,
		StorageAccountCloud,
		StorageAccountEmulator,
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	public
	static  
	class StashConfiguration
	{
			public 
			static 
			ConfigurationType					ConfigType	{get; private set; }
			
		// static constructor used for setting up the CloudStorageAccount so as to use the 
		// Microsoft storage client infrastructure for credentials
		
		static 
		StashConfiguration()
		{
			// *** Change type here switch between storage emulator and the cloud storage ***
			ConfigType = ConfigurationType.StashEmulator;

#if USE_STORAGE_CLIENT
		    CloudStorageAccount.SetConfigurationSettingPublisher(
		        (configName, configSetter) =>
		                configSetter(
		                    ConfigurationManager.AppSettings[configName]));

#endif
		}

		public 
		static
		bool
		IsConfigurationEmulator
		{
			get 
			{ 
				return ConfigType == ConfigurationType.StashEmulator 
						|| ConfigType == ConfigurationType.StorageAccountEmulator; 
			}
		}

		public 
		static
		bool
		IsConfigurationCloud
		{
			get { return	!IsConfigurationEmulator; }
		}

		public
		static
		StashClientOptions
		GetDefaultOptions()
		{
			return new StashClientOptions 
									{
										UseHttps					= false,
				                        Feedback					= TraceFeedback,
										Expect100Continue			= TernarySwitch.False,
										UseNagleAlgorithm			= TernarySwitch.False,
										//ReturnUpdatedInsertEntity	= true,
									};
		}

		public
		static
		StashClient<T>
		GetClient<T>(
			StashClientOptions					options)
		{
			StashClient<T>
			result = null;

			switch(ConfigType)
			{
				case ConfigurationType.StashCloud:
					
					result =	new StashClient<T>(
									new StorageAccountKey(
											ConfigurationManager.AppSettings["AccountName"],
											ConfigurationManager.AppSettings["key"]),
									options);		
					break;

				case ConfigurationType.StashEmulator:

					result = new StashClient<T>(
											options);		
					break;

#if USE_STORAGE_CLIENT
				case ConfigurationType.StorageAccountCloud:
					
					result = GetStasherUsingCloudStorageAccount<T>(
													"DataConnectionString",
													options);
					break;

				case ConfigurationType.StorageAccountEmulator:
					
					result = GetStasherUsingCloudStorageAccount<T>(
													"DataConnectionStringEmulator",
													options);
					break;
#endif
			}

			return result;
		}

		public
		static
		StashClient<T>
		GetClient<T>()
		{
			return GetClient<T>(GetDefaultOptions());
		}

#if USE_STORAGE_CLIENT
		static
		StashClient<T>
		GetStasherUsingCloudStorageAccount<T>(
			CloudStorageAccount					account, 
			StashClientOptions					options)
		{
			options.UseHttps = account.TableEndpoint.Scheme == "https";

			return new StashClient<T>(
							account.Credentials.AccountName,
							new SignRequest(h => account.Credentials.SignRequestLite(h)),
							options);		
		}

		static
		StashClient<T>
		GetStasherUsingCloudStorageAccount<T>(
			string								settings,
			StashClientOptions					options)
		{
			return GetStasherUsingCloudStorageAccount<T>(
									GetCloudStorageAccount(settings),
									options);		
		}

		// -------------------------------------------------------------------------------------------------------------

		public
		static
		CloudStorageAccount
		GetCloudStorageAccount(
			string								setting)
		{
		    return CloudStorageAccount.FromConfigurationSetting(setting);
		}

		public
		static
		CloudStorageAccount
		GetCloudStorageAccount()
		{
		    return GetCloudStorageAccount("DataConnectionString");
		}

		public
		static
		CloudStorageAccount
		GetCloudStorageAccountEmulator()
		{
		    return GetCloudStorageAccount("DataConnectionStringEmulator");
		}
#endif
		// -------------------------------------------------------------------------------------------------------------

			static
			string								_formatUri = "Uri: {0} - {1}";

		/// <summary>
		/// Lucifure Stash invokes a trace feedback call if supplied. 
		/// This implementation, in debug mode, will output the request and response data for debugging purposes.
		/// </summary>
		public 
		static 
		void 
		TraceFeedback(
			object								obj)
		{
			StashRequestResponse				reqRes;
			
			if ((reqRes = obj as StashRequestResponse) != null)
			{	
				Trace.WriteLine("");

				Trace.WriteLine(
							String.Format(
										_formatUri,
										reqRes.Request.Method,
										reqRes.Request.RequestUri));
				
				if (!String.IsNullOrWhiteSpace(reqRes.RequestBody))
				{
					Trace.WriteLine("Request Body:");
					Trace.WriteLine(reqRes.RequestBody);
				}

				if (!String.IsNullOrWhiteSpace(reqRes.ResponseBody))
				{
					Trace.WriteLine("Response Body:");
					Trace.WriteLine(reqRes.ResponseBody);
				}

				Trace.Flush();
			}
		}

	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
}
