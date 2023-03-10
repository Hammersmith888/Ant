using System;
using System.Collections.Generic;


namespace BugsFarm
{
public static class Dev2Dev
{
	public static void TutorialStart()
	=>
		TutorialStep( DevToDev.TutorialState.Start );


	public static void TutorialSkip()
	=>
		TutorialStep( DevToDev.TutorialState.Skipped );


	public static void TutorialComplete()
	=>
		TutorialStep( DevToDev.TutorialState.Finish );


	public static void TutorialStep( int tutorialStep )
	{
		try
		{
			DevToDev.Analytics.Tutorial( tutorialStep );
		}
		catch (Exception) { /* ignored */ }
	}


	public static void InAppPurchase( AntType type, int price, Currency currency )
	{
		string purchaseId		= type.ToString();
		string purchaseType		= PurchaseGroup.Insect.ToString();

		InAppPurchase( purchaseId, purchaseType, price, currency );
	}


	public static void InAppPurchase( APlaceable placeable, int price, Currency currency )
	{
		if (
				!Tutorial.Instance.IsActive &&
				Analytics.Names.TryGetValue( ( placeable.Type, placeable.SubType ), out (PurchaseGroup group, string name) tuple )
			)
		{
			string purchaseId		= tuple.name;
			string purchaseType		= tuple.group.ToString();

			InAppPurchase( purchaseId, purchaseType, price, currency );
		}
	}


	static void InAppPurchase( string purchaseId, string purchaseType, int price, Currency currency )
	{
		if (Tutorial.Instance.IsActive)
			return;

		int purchaseAmount		= 1;

		try
		{
			DevToDev.Analytics.InAppPurchase( purchaseId, purchaseType, purchaseAmount, price, currency.ToString( ));
		}
		catch (Exception) { /* ignored */ }
	}


	public static void RealPayment( string productId, string transactionId, float inAppPrice, string inAppCurrencyISOCode )
	{
		try
		{
			DevToDev.Analytics.RealPayment( transactionId, inAppPrice, productId, inAppCurrencyISOCode );
		}
		catch (Exception) { /* ignored */ }
	}


	public static void CustomEvent(
			string							eventName,
			Dictionary< string, object >	parameters = null
		)
	{
		try
		{
			// (!) DevToDev can throw an exception even during new DevToDev.CustomEventParams() call, if not initialized (!)

			DevToDev.CustomEventParams params_d2d		= null;
		
			if (parameters != null)
			{
				params_d2d			= new DevToDev.CustomEventParams();

				int i = 0;
				foreach (var pair in parameters)
				{
					Object o				= pair.Value;
					Type type				= o.GetType();
					string paramName		= pair.Key;

					// https://stackoverflow.com/questions/43080505/c-sharp-7-0-switch-on-system-type
					switch (true)
					{
						case bool _ when type == typeof(int):			params_d2d.AddParam( paramName, (int)pair.Value );		break;
						case bool _ when type == typeof(string):		params_d2d.AddParam( paramName, (string)pair.Value );	break;

						default: throw new Exception( $"Type { type } not handled." );
					}

					i ++;
				}
			}

			DevToDev.Analytics.CustomEvent	( eventName, params_d2d );
		}
		catch (Exception) { /* ignored */ }
	}


	public static void LevelComplete( int completedLevel )
	{
		int newLevel		= completedLevel + 1;

		DevToDev.Analytics.LevelUp( newLevel );
	}
}
	
}

