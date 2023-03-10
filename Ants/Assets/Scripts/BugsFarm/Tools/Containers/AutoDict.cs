using System.Collections.Generic;
using UnityEngine;


/*
	https://stackoverflow.com/questions/10816803/finding-next-available-key-in-a-dictionary-or-related-collection
*/


public class AutoDict< T >
{
	readonly Dictionary< int, T >	_dict			= new Dictionary< int, T >();

	readonly HashSet< int >			_usedKeys		= new HashSet< int >();
	int								_maxKey			= 0;


	int GetFreeKey()
	{
		int key;

		using (var it = _usedKeys.GetEnumerator())
		{
			if (it.MoveNext())
			{
				key		= it.Current;

				_usedKeys.Remove( key );
			}
			else
			{
				key		= ++ _maxKey;
			}
		}

		return key;
	}


	public int Add( T item )
	{
		int key		= GetFreeKey();

		_dict.Add( key, item );

		return key;
	}


	public void Remove( int key )
	{
		_dict.Remove( key );

		_usedKeys.Add( key );
	}


	public T this[ int key ]
	{
		get => _dict[ key ];
		set => _dict[ key ] = value;
	}


	public static void Test()
	{
		AutoDict< string > ad		= new AutoDict< string >();

		int key_A		= ad.Add( "A" );
		int key_B		= ad.Add( "B" );
		int key_C		= ad.Add( "C" );

		ad.Remove( key_B );

		int key_D		= ad.Add( "D" );
		int key_E		= ad.Add( "E" );

		Debug.Log( $"key_A: {key_A}, value: {ad[ key_A ]}" );
		Debug.Log( $"key_B: {key_B}, value: {ad[ key_B ]}" );
		Debug.Log( $"key_C: {key_C}, value: {ad[ key_C ]}" );
		Debug.Log( $"key_D: {key_D}, value: {ad[ key_D ]}" );
		Debug.Log( $"key_E: {key_E}, value: {ad[ key_E ]}" );
	}
}

