using System.Collections;
using System.Collections.Generic;


/*
	https://stackoverflow.com/a/10966684/4830242
*/


public class BiDict< T1, T2 > : IEnumerable< KeyValuePair< T1, T2 >>
{
    public class Indexer< T3, T4 >
    {
        Dictionary< T3, T4 > _dictionary;

        public Indexer( Dictionary< T3, T4 > dictionary )
        {
            _dictionary		= dictionary;
        }

        public T4 this[ T3 index ]
        {
            get => _dictionary[ index ];
            set => _dictionary[ index ] = value;
        }
    }


    public Indexer< T1, T2 > Forward		{ get; }
    public Indexer< T2, T1 > Reverse		{ get; }


    Dictionary< T1, T2 > _forward		= new Dictionary< T1, T2 >();
    Dictionary< T2, T1 > _reverse		= new Dictionary< T2, T1 >();


    public BiDict()
    {
        Forward		= new Indexer< T1, T2 >( _forward );
        Reverse		= new Indexer< T2, T1 >( _reverse );
    }


    public void Add( T1 t1, T2 t2 )
    {
		/*
        _forward.Add( t1, t2 );
        _reverse.Add( t2, t1 );
		*/

		Remove( t1 );
		Remove( t2 );

        _forward[ t1 ]		= t2;
        _reverse[ t2 ]		= t1;
    }


    public void Remove( T1 t1 )
    {
		if (t1 == null || !_forward.ContainsKey( t1 ))
			return;

        _reverse.Remove( _forward[ t1 ] );
        _forward.Remove( t1 );
    }


    public void Remove( T2 t2 )
    {
		if (t2 == null || !_reverse.ContainsKey( t2 ))
			return;

        _forward.Remove( _reverse[ t2 ] );
        _reverse.Remove( t2 );
    }


	// For convenience
    public T2 this[ T1 index ]
    {
	    get => Forward[ index ];
	    set => Add( index, value );
    }

    public T1 this[ T2 index ]
    {
	    get => Reverse[ index ];
	    set => Add( value, index );
    }


	Dictionary< T1, T2 >.Enumerator GetEnum()
	{
		return _forward.GetEnumerator();
	}


	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnum();
	}


	public IEnumerator< KeyValuePair< T1, T2 >> GetEnumerator()
	{
		return GetEnum();
	}


	// For ambiguous cases when T1 == T2
	public bool ForwardContainsKey( T1 t1 )		{ return _forward.ContainsKey( t1 );		}
	public bool ReverseContainsKey( T2 t2 )		{ return _reverse.ContainsKey( t2 );		}

	public bool ContainsKey( T1 t1 )			{ return ForwardContainsKey( t1 );			}
	public bool ContainsKey( T2 t2 )			{ return ReverseContainsKey( t2 );			}

	public bool TryGetValue( T1 t1, out T2 value )			{ return _forward.TryGetValue( t1, out value );			}
	public bool TryGetValue( T2 t2, out T1 value )			{ return _reverse.TryGetValue( t2, out value );			}


	public void Clear()
	{
		_forward.Clear();
		_reverse.Clear();
	}
}

