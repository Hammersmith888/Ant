using System;
using FullSerializer;


/*
	https://github.com/jacobdufault/fullserializer 
*/


public static class StringSerializationAPI
{
	static readonly fsSerializer _serializer			= new fsSerializer();


	static void RemoveRedundant( fsData data )
	{
		foreach (var pair in data.AsDictionary[ "buildings" ].AsDictionary)
			pair.Value.AsDictionary.Remove( "price" );

		foreach (var pair in data.AsDictionary[ "decorations" ].AsDictionary)
			pair.Value.AsDictionary.Remove( "maxCount" );
	}


	public static string Serialize( Type type, object value )
	{
		// serialize the data
		fsData data;
		_serializer.TrySerialize( type, value, out data ).AssertSuccessWithoutWarnings();


		RemoveRedundant( data );


		// emit the data via JSON
		return fsJsonPrinter.CompressedJson( data );
	}


	public static object Deserialize( Type type, string serializedState )
	{
		// step 1: parse the JSON data
		fsData data					= fsJsonParser.Parse( serializedState );

		// step 2: deserialize the data
		object deserialized			= null;
		_serializer.TryDeserialize( data, type, ref deserialized ).AssertSuccessWithoutWarnings();

		return deserialized;
	}
}

