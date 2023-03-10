using UnityEngine;
using UnityEngine.Purchasing;


namespace BugsFarm
{
	// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
	public class Purchaser : MB_Singleton< Purchaser >, IStoreListener
	{
		public static BiDict<IAPType, string> IAP_IDs		= new BiDict<IAPType, string>
		{
			{ IAPType.Crystals_1, "crystals_10"		},
			{ IAPType.Crystals_2, "crystals_100"	},
			{ IAPType.Crystals_3, "crystals_500"	},
		};

		static string _productIdPrefix		= "com.tsgames.ants.";

		static IStoreController         _storeController;                  // The Unity Purchasing system
		static IExtensionProvider       _storeExtensionProvider;           // The store-specific Purchasing subsystems


        public static bool IsInitialized()
        =>
			// Only say we are initialized if both the Purchasing references are set.
			_storeController			!= null &&
			_storeExtensionProvider		!= null
		;


		void Start()
		{
			DontDestroyOnLoad( gameObject );
			InitializePurchasing();
		}


		void InitializePurchasing() 
		{
			// Create a builder, first passing in a suite of Unity provided stores.
			var builder		= ConfigurationBuilder.Instance( StandardPurchasingModule.Instance() );

			foreach (var pair in IAP_IDs)
			{
				string generalId	= pair.Value;
				string storeId		= _productIdPrefix + pair.Value;
				string appleId		= storeId;
				string googleId		= storeId;

				Add_Consumable( builder, generalId, appleId, googleId );
			}

			// Kick off the remainder of the set-up with an asynchronous call, passing the configuration 
			// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
			UnityPurchasing.Initialize( this, builder );
		}


		void Add_Consumable( ConfigurationBuilder builder, string general_ID, string apple_ID, string google_ID )
		{
			IDs ids		= new IDs();
			
			ids.Add( apple_ID,		AppleAppStore	.Name );
			ids.Add( google_ID,		GooglePlay		.Name );

            builder.AddProduct( general_ID, ProductType.Consumable, ids );
		}


		public static void BuyCrystals( IAPType iapType )		=> Instance.BuyProductID( IAP_IDs[ iapType ] );


		public static string GetLocalPriceString( IAPType iapType )
		=>
			_storeController.products.WithID( IAP_IDs[ iapType ] ).metadata.localizedPriceString;


		void BuyProductID( string productId )
		{
			if (!IsInitialized())
			{
				// Report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initialization.
				Debug.Log( "BuyProductID FAIL. Not initialized." );
				return;
			}

			// Look up the Product reference with the general product identifier and the Purchasing system's products collection.
			Product product		= _storeController.products.WithID( productId );

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log( $"Purchasing product asychronously: '{product.definition.id}'" );
				// Buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
				_storeController.InitiatePurchase( product );
			}
			else
			{
				// Report the product look-up failure situation  
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}


		// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
		// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
		public void RestorePurchases()
		{
			// If Purchasing has not yet been set up ...
			if (!IsInitialized())
			{
				// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
				Debug.Log("RestorePurchases FAIL. Not initialized.");
				return;
			}

			// If we are running on an Apple device ... 
			if (
					Application.platform == RuntimePlatform.IPhonePlayer ||
					Application.platform == RuntimePlatform.OSXPlayer
				)
			{
				// ... begin restoring purchases
				Debug.Log("RestorePurchases started ...");

				// Fetch the Apple store-specific subsystem.
				var apple		= _storeExtensionProvider.GetExtension<IAppleExtensions>();
				// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
				// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
				apple.RestoreTransactions( result => {
					// The first phase of restoration. If no more responses are received on ProcessPurchase then 
					// no purchases are available to be restored.
					Debug.Log( "RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore." );
				});
			}
			// Otherwise ...
			else
			{
				// We are not running on an Apple device. No work is necessary to restore purchases.
				Debug.Log( "RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform );
			}
		}


        //  
        // --- IStoreListener
        //

        public void OnInitialized( IStoreController controller, IExtensionProvider extensions )
        {
			// Purchasing has succeeded initializing. Collect our Purchasing references.
			Debug.Log( "OnInitialized: PASS" );

			// Overall Purchasing system, configured with products for this application.
			_storeController				= controller;

			// Store specific subsystem, for accessing device-specific store features.
			_storeExtensionProvider			= extensions;
        }


		public void OnInitializeFailed( InitializationFailureReason error )
		{
			// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
			Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
		}


		public PurchaseProcessingResult ProcessPurchase( PurchaseEventArgs args )
		{
			string id			= args.purchasedProduct.definition.id;
            
			if (IAP_IDs.TryGetValue( id, out IAPType iapType ))
			{
				CfgIAP cfg      = Data_IAPs.Instance.IAPs[ iapType ];
				int amount      = cfg.IAP.amount;

				GameResources.Crystals		+= amount;

				string productId			= args.purchasedProduct.definition.id;
				string transactionId		= args.purchasedProduct.transactionID;
				decimal localizedPrice		= args.purchasedProduct.metadata.localizedPrice;
				string isoCurrencyCode		= args.purchasedProduct.metadata.isoCurrencyCode;
				string receipt				= args.purchasedProduct.receipt;

				Analytics.RealPayment( productId, transactionId, receipt, localizedPrice, isoCurrencyCode );

				Debug.Log( $"ProcessPurchase: PASS. Product: '{id}'" );
			}
			else
			{
				Debug.Log( $"ProcessPurchase: FAIL. Unrecognized product: '{id}'" );
			}

			// Return a flag indicating whether this product has completely been received, or if the application needs 
			// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
			// saving purchased products to the cloud, and when that save is delayed. 
			return PurchaseProcessingResult.Complete;
		}


		public void OnPurchaseFailed( Product product, PurchaseFailureReason failureReason )
		{
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
			// this reason with the user to guide their troubleshooting actions.
			Debug.Log(
				$"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}"
			);
		}
	}
}

