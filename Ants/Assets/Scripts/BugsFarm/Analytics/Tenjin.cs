/*

using System;

namespace BugsFarm
{

public class Tenjin : MB_Singleton< Tenjin >
{
	const string TenjinApiKey		= "9HXLH6VQRGEP4SWYP3QTWKVP7QUWY4DA";


	void Start()
	{
		DontDestroyOnLoad( gameObject );
		Connect();
	}


	void OnApplicationPause( bool pauseStatus )
	{
		// https://github.com/tenjin/tenjin-unity-sdk

		if (!pauseStatus)
			Connect();
	}


	void Connect()		=> GetInstance().Connect();


	static BaseTenjin GetInstance()			=> global::Tenjin.getInstance( TenjinApiKey );


	public void CompletedPurchase( string ProductId, string CurrencyCode, int Quantity, double UnitPrice )
	{
		try
		{
			GetInstance().Transaction( ProductId, CurrencyCode, Quantity, UnitPrice, null, null, null );
		}
		catch (Exception) { /* ignored * / }
	}


	public static void CustomEvent( string eventName )
	{
		try
		{
			GetInstance().SendEvent( eventName );
		}
		catch (Exception) { /* ignored * / }
	}


	public static void CustomEvent( string eventName, int parameter )
	{
		try
		{
			GetInstance().SendEvent( eventName, parameter.ToString() );
		}
		catch (Exception) { /* ignored * / }
	}
}
	
}

*/