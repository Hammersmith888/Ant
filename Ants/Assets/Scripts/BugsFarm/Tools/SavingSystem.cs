using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


/*
	https://gamedev.stackexchange.com/questions/80638/binary-serialization-of-c-for-gameobject-in-unity
*/


public static class SavingSystem
{
	public static void Serialize< T >( string path, T data )
	{
		CreateFolderIfNotExists( path );
		var file = File.Create(path);
		if (file.CanWrite)
        {
			BinaryFormatter bf = new BinaryFormatter();

			bf.Serialize(file, data);
		}

		// https://answers.unity.com/questions/1035204/serializing-to-disk-sometimes-fails-on-android-but.html
		file.Close();
		file.Dispose();
	}


	public static T Deserialize< T >( string path ) where T : class
	{
		if (!File.Exists( path ))
			return null;

		using (FileStream file			= File.Open(path, FileMode.Open))
		{
			BinaryFormatter bf			= new BinaryFormatter();
			T data						= (T)bf.Deserialize( file );

			return data;
		}
	}


	public static void SaveText( string path, string text )
	{
		CreateFolderIfNotExists( path );

		using (StreamWriter streamWriter			= File.CreateText( path ))
		{
			streamWriter.Write( text );
		}
	}

        
	public static string LoadText( string path )
	{
		if (!File.Exists( path ))
			return "";

		using (StreamReader streamReader			= File.OpenText( path ))
		{
			return streamReader.ReadToEnd();
		}
	}


	static void CreateFolderIfNotExists( string path )
	{
		string folder		= Path.GetDirectoryName( path );

		if (!Directory.Exists( folder ))
			Directory.CreateDirectory( folder );
	}


	public static string DefaultPath( string fileName )
	=>
		Path.Combine( Application.persistentDataPath, fileName );
}

