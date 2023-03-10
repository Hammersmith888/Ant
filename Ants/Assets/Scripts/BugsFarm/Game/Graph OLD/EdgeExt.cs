using QuickGraph;


public class EdgeExt< T > : Edge< T > where T : class
{
	public EdgeExt( T source, T target )
		: base( source, target )
	{}
	
	public T Other( T v )
	{
		return Source == v ? Target : Source;
	}
}

