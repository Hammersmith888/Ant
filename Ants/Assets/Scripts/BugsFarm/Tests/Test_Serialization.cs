using System.Collections.Generic;

namespace Tests
{

[System.Serializable]
public class TestData_A
{
	public int Value	=> _value;

	int _value;

	public TestData_A( int value )
	{
		_value		= value;
	}

	public void Set( int value )
	{
		_value		= value;
	}
}


[System.Serializable]
public class TestData_B
{
	public TestData_A	data_1;
	public TestData_A	data_2;
	public TestData_A	data_3;
	public TestData_A	data_4;
}


[System.Serializable]
public class TestData_Parent {}

[System.Serializable]
public class TestData_Child1 : TestData_Parent
{
	public int value;
}

[System.Serializable]
public class TestData_Child2 : TestData_Parent
{
	public string text;
}


[System.Serializable]
public class TestData_C
{
	public HashSet< int >	HashSet		=new HashSet< int >();
}


[System.Serializable]
public class TestData_D1
{
	public TestData_D1()
	{
		Tools.Log();
	}
}


[System.Serializable]
public class TestData_D2
{
	TestData_D1		_x		= new TestData_D1();

	public TestData_D2()
	{
		Tools.Log();
	}
}


//public static class Test_Serialization
//{
//	const string	FileName		= "test.dat";
//	static string	Path			=> SavingSystem.DefaultPath( FileName );


//	static bool IsEqual< T >( T a, T b )
//	{
//		return EqualityComparer< T >.Default.Equals( a, b );
//	}


//	static void TestEqual< T >( T a, T b, bool equal = true )
//	{
//		if (IsEqual( a, b ) == equal)
//			Debug.Log( "Test Passed" );
//		else
//			Debug.Log( $"Test FAILED ({ a } {( equal ? "!=" : "==" )} { b })" );
//	}


//	static void TestNonEqual< T >( T a, T b )
//	{
//		TestEqual( a, b, false );
//	}


//	static T SerDeser< T >( T dataToSave ) where  T : class
//	{
//		SavingSystem.Serialize( Path, dataToSave );

//		T dataLoaded		= SavingSystem.Deserialize< T >( Path );
		
//		File.Delete( Path );		// Clean up

//		return dataLoaded;
//	}


//	static void Test_1_Basic()
//	{
//		TestData_A same				= new TestData_A( 13 );
//		TestData_B dataToSave		= new TestData_B();
//		dataToSave.data_1			= same;
//		dataToSave.data_2			= same;
//		dataToSave.data_3			= new TestData_A( 13 );
//		dataToSave.data_4			= null;
		
//		TestData_B dataLoaded		= SerDeser( dataToSave );
        
//		TestEqual		( dataLoaded.data_1.Value,	dataToSave.data_1.Value	);
//		TestEqual		( dataLoaded.data_3.Value,	dataToSave.data_3.Value	);

//		TestEqual		( dataLoaded.data_1,			dataLoaded.data_2			);
//		TestNonEqual	( dataLoaded.data_1,			dataLoaded.data_3			);
//		TestEqual		( dataLoaded.data_4,			null						);
//	}


//	static void Test_2_Polymorphism()
//	{
//		TestData_Child1 child1		= new TestData_Child1{ value = 1 };
//		TestData_Child2 child2		= new TestData_Child2{ text = "Lalala" };

//		List< TestData_Parent > dataToSave		= new List< TestData_Parent >{ child1, child2 };

//		var dataLoaded				= SerDeser( dataToSave );

//		TestEqual( ((TestData_Child1)dataLoaded[ 0 ]).value,	child1.value	);
//		TestEqual( ((TestData_Child2)dataLoaded[ 1 ]).text,	child2.text	);

//		/*
//		Debug.Log( ((TestData_Child1)dataLoaded[ 0 ]).value );
//		Debug.Log( ((TestData_Child2)dataLoaded[ 1 ]).text );
//		*/
//	}


//	static void Test_3_Ant()
//	{
//		// Ant ant			= Factory.Instance.Spawn_Ant( new GPos( Graph.Grass_2 , .33f ) );
//		Ant ant				= Factory.Instance.Spawn_Ant();

//		Ant dataLoaded		= SerDeser( ant );

//		TestEqual( dataLoaded.gPos.gVert,		ant.gPos.gVert	);
//		TestEqual( dataLoaded.gPos.t,			ant.gPos.t		);

//		// Tools.Log( dataLoaded.gPos.gVert.ToString() );

//		Keeper.Destroy( ant );

//		// Factory.Instance.Create_MB( dataLoaded );
//	}


//	static void Test_4_HashSet()
//	{
//		TestData_C dataToSave		= new TestData_C();
//		dataToSave.HashSet.Add( 13 );
//		dataToSave.HashSet.Add( 17 );

//		TestData_C dataLoaded		= SerDeser( dataToSave );

//		TestEqual( dataLoaded.HashSet.Count, dataToSave.HashSet.Count );

//		// foreach (int value in dataLoaded.HashSet) Debug.Log( value );
//	}


//	static void Test_5_Ctor()
//	{
//		// https://social.msdn.microsoft.com/Forums/vstudio/en-US/48f608e1-749c-4b7c-9c81-612d6f89716d/constructor-not-executed-on-deserializing?forum=wcf

//		TestData_D2 dataToSave		= new TestData_D2();

//		TestData_D2 dataLoaded		= SerDeser( dataToSave );
//	}


//	public static void Test()
//    {
//		Test_1_Basic();
//		Test_2_Polymorphism();
//		Test_3_Ant();
//		Test_4_HashSet();
//		// Test_5_Ctor();
//    }
//}

}

