using System;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;


namespace BugsFarm
{

public class Facebook : MonoBehaviour
{
	static readonly List<(string, Dictionary<string, object>)> buffer_FB		= new List<(string, Dictionary<string, object>)>();

	
	void Start()
	{
		DontDestroyOnLoad( gameObject );
		FB.Init( FbInitCallBack );
	}


	void OnApplicationPause( bool pauseStatus )
	{
		// https://developers.facebook.com/docs/app-events/unity/

		if (!pauseStatus)
		{
			if (FB.IsInitialized)
				FB.ActivateApp();
		}
	}


	void FbInitCallBack()
	{
		if (FB.IsInitialized)
		{
			FB.ActivateApp();

			foreach (var tuple in buffer_FB)
				CustomEvent( tuple.Item1, tuple.Item2 );

			buffer_FB.Clear();
		}
		else
			Debug.Log( "Failed to initialize Facebook SDK!" );
	}


	public static void TutorialComplete()
	=>
		CustomEvent( AppEventName.CompletedTutorial );


	public static void RealPayment( string productId, decimal price, string inAppCurrencyISOCode )
	{
		var parameters			= new Dictionary<string, object>
		{
			{ "Tarif", productId },
		};

		try
		{
			FB.LogPurchase( price, inAppCurrencyISOCode, parameters );
		}
		catch (Exception) { /* ignored */ }
	}


	public static void CustomEvent(
			string							eventName,
			Dictionary< string, object >	parameters = null
		)
	{
		if (FB.IsInitialized)
			try
			{
				FB.LogAppEvent( eventName, parameters: parameters );
			}
			catch (Exception) { /* ignored */ }
		else
			buffer_FB.Add( ( eventName, parameters ) );
	}
}

}

