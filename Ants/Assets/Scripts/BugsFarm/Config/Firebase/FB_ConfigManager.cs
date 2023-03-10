
/*
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;


/*
	"Editor freezes more than usual when hitting play."
	https://github.com/firebase/quickstart-unity/issues/639
* /


public class FB_ConfigManager : MB_Singleton< FB_ConfigManager >
{
	public static bool	ConfigLoadTaskEnded			=>
														Instance._task_Fail ||
														Instance._allTasks_Success
	;

	float				time						=> Time.realtimeSinceStartup;


	FirebaseDatabase	_database;
	bool				_task_Fail;
	bool				_allTasks_Success;
	float				_t_start;


	void OnDestroy()
	{
		// https://github.com/firebase/quickstart-unity/issues/445

		_database		= null;
	}


	public void InitFirebase()
	=>
		StartCoroutine( InitFirebaseCoroutine() );


	public void LoadConfig( string config )
	=>
		StartCoroutine( LoadSaveCoroutine( config, true ) );


	public void SaveConfig( string config )
	=>
		StartCoroutine( LoadSaveCoroutine( config, false ) );


	IEnumerator InitFirebaseCoroutine()
	{
		// Init Firebase
		yield return StartCoroutine( WaitForTask( Init(), "Failed to initialize Firebase" ) );
		if (_task_Fail) yield break;
		Debug.Log( "Firebase initialized" );
	}


	IEnumerator LoadSaveCoroutine( string config, bool toLoad )
	{
		// Auth
		yield return StartCoroutine( WaitForTask( Auth(), "Failed to auth" ) );
		if (_task_Fail) yield break;
		Debug.Log( $"Authorized ({ time - _t_start })" );


		if (toLoad)
		{
			// Load
			yield return StartCoroutine( WaitForTask( Load( config ), "Failed to load" ) );
			if (_task_Fail) yield break;
			Debug.Log( $"Loaded ({ time - _t_start })" );
		}
		else
		{
			// Save
			yield return StartCoroutine( WaitForTask( Save( config ), "Failed to save" ) );
			if (_task_Fail) yield break;
			Debug.Log( $"Saved ({ time - _t_start })" );
		}


		Debug.Log( $"Success!" );

		_allTasks_Success		= true;
	}


	async Task Init()
	{
		// await FirebaseApp.CheckAndFixDependenciesAsync();

		await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread( 
		task =>
		{
			var dependencyStatus		= task.Result;

			if (dependencyStatus == Firebase.DependencyStatus.Available)
			{
				// Create and hold a reference to your FirebaseApp,
				// where app is a Firebase.FirebaseApp property of your application class.
				// Crashlytics will use the DefaultInstance, as well;
				// this ensures that Crashlytics is initialized.

				// (!!!) https://github.com/firebase/quickstart-unity/issues/715#issuecomment-659704934 (!!!)
				// force Firebase singleton to initialize, you just need to call DefaultInstance but don't need the returned app
				Firebase.FirebaseApp app		= Firebase.FirebaseApp.DefaultInstance;

				// Set a flag here for indicating that your project is ready to use Firebase.
			}
			else
			{
				UnityEngine.Debug.LogError( $"Could not resolve all Firebase dependencies: {dependencyStatus}" );
				// Firebase Unity SDK is not safe to use here.
			}
		});


		_database				= FirebaseDatabase.DefaultInstance;
	}


	async Task Auth()
	{
		FirebaseAuth auth		= FirebaseAuth.DefaultInstance;
		
		await auth.SignInWithEmailAndPasswordAsync( "writer@bugsfarmgame.com", "7tbdwk" );
		// await auth.SignInWithEmailAndPasswordAsync( "reader@bugsfarmgame.com", "8fntyw" );
	}


	async Task Load( string config )
	{
		try
		{
			DataSnapshot dataSnapshot		= await _database.GetReference( config ).GetValueAsync();

			FB_Packer.Unpack( dataSnapshot.GetRawJsonValue() );
		}
		catch (Exception e)
		{
			Debug.Log( e );
			throw;
		}
	}


	async Task Save( string config )
	{
		try
		{
			string json			= FB_Packer.Pack();

			Debug.Log( json );

			await _database.GetReference( config ).SetRawJsonValueAsync( json );
		}
		catch (Exception e)
		{
			Debug.Log( e );
			throw;
		}
	}


	IEnumerator WaitForTask( Task task, string errorMsg )
	{
		_t_start		= time;

		yield return new WaitUntil( () => task.IsCompleted );

		if (task.Exception != null)
		{
			_task_Fail		= true;

			Debug.Log( errorMsg );
			Debug.Log( task.Exception.Message );
		}
	}
}

*/