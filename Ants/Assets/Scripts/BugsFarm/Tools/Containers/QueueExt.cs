using System;
using System.Collections;
using System.Collections.Generic;


/*
	https://stackoverflow.com/questions/1594375/is-there-a-better-way-to-implement-a-remove-method-for-a-queue
	https://stackoverflow.com/a/1594416/4830242
*/


[Serializable]
public class QueueExt< T > : IEnumerable<T>
{
	public int Count			=> _list.Count;


	LinkedList< T > _list		= new LinkedList< T >();


	public void Enqueue( T item )
	{
		_list.AddLast( item );
	}


	public T Dequeue()
	{
		T item		= _list.First.Value;

		_list.RemoveFirst();

		return item;
	}


	public T Peek()
	{
		return _list.First.Value;
	}


	public bool Remove( T item )
	{
		return _list.Remove( item );
	}


	public void Clear()
	{
		_list.Clear();
	}


	public IEnumerator GetEnumerator()
	{
		return _list.GetEnumerator();
	}


	IEnumerator< T > IEnumerable< T >.GetEnumerator()
	{
		return (IEnumerator< T>)GetEnumerator();
	}
}

