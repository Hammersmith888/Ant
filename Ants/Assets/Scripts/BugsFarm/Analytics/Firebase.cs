
/*
using Firebase.Analytics;


namespace BugsFarm
{

public static class Firebase
{
	public static void TutorialComplete()
	=>
		CustomEvent( FirebaseAnalytics.EventTutorialComplete );


	public static void RealPayment( string productId, string transactionId, double price, string inAppCurrencyISOCode )
	{
		try
		{
			FirebaseAnalytics.LogEvent(
				FirebaseAnalytics.EventPurchase,
				new Parameter[]
				{
					new Parameter( FirebaseAnalytics.ParameterTransactionId, transactionId ),
					new Parameter( FirebaseAnalytics.ParameterCurrency, inAppCurrencyISOCode ),
					new Parameter( FirebaseAnalytics.ParameterValue, price ),

					// (!!!) https://github.com/firebase/quickstart-unity/issues/713
					// new Parameter( FirebaseAnalytics.ParameterItems, ???? ),

					new Parameter( FirebaseAnalytics.ParameterItemName, productId ),
				}
			);
		}
		catch (Exception) { /* ignored * / }
	}


	public static void CustomEvent(
			string							eventName,
			Dictionary< string, object >	parameters = null
		)
	{
		global::Firebase.Analytics.Parameter[] params_FA	= null;
		
		if (parameters != null)
		{
			params_FA			= new Parameter[ parameters.Count ];

			int i = 0;
			foreach (var pair in parameters)
			{
				Object o				= pair.Value;
				Type type				= o.GetType();
				string paramName		= pair.Key;

				// https://stackoverflow.com/questions/43080505/c-sharp-7-0-switch-on-system-type
				switch (true)
				{
					case bool _ when type == typeof(int):
					{
						int value			= (int)pair.Value;
						params_FA[ i ]		= new Parameter( paramName, value );
						break;
					}

					case bool _ when type == typeof(string):
					{
						string value		= (string)pair.Value;
						params_FA[ i ]		= new Parameter( paramName, value );
						break;
					}

					default: throw new Exception( $"Type { type } not handled." );
				}

				i ++;
			}
		}

		try
		{
			FirebaseAnalytics.LogEvent( eventName, params_FA );
		}
		catch (Exception) { /* ignored * / }
	}
}
	
}

*/