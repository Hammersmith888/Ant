using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using UnityEngine;


/*
	https://stackoverflow.com/questions/703871/quickgraph-dijkstra-example
*/


public class TEST_TEdge : Edge< string >
{
	public TEST_TEdge( string source, string target )
		: base( source, target )
	{}

	public string Other( string v )
	{
		return Source == v ? Target : Source;
	}
}


public class QuickGraphTest : MonoBehaviour
{
    void Start()
    {
		var graph			= new UndirectedGraph< string, TEST_TEdge >( false );
		var edgeCosts		= new Dictionary< TEST_TEdge, double >();
		var edgeCost		= AlgorithmExtensions.GetIndexer( edgeCosts );

		var a_b1			= new TEST_TEdge( "A", "B1" );
		var b1_c			= new TEST_TEdge( "B1", "C" );
		var a_b2			= new TEST_TEdge( "A", "B2" );
		var b2_c			= new TEST_TEdge( "B2", "C" );

		graph.AddVerticesAndEdge( a_b1 );
		graph.AddVerticesAndEdge( b1_c );
		graph.AddVerticesAndEdge( a_b2 );
		graph.AddVerticesAndEdge( b2_c );

		edgeCosts.Add( a_b1, 2 );
		edgeCosts.Add( b1_c, 2 );
		edgeCosts.Add( a_b2, 1 );
		edgeCosts.Add( b2_c, 1 );



		string source		= "C";
		string target		= "A";

		FindPath( graph, edgeCost, source, target );

		edgeCosts[ a_b2 ]	= 100;

		FindPath( graph, edgeCost, source, target );
    }


	void FindPath(
			UndirectedGraph< string, TEST_TEdge >	graph,
			Func< TEST_TEdge, double >				edgeCost,
			string									source,
			string									target
		)
	{
		var tryGetPaths		= graph.ShortestPathsDijkstra( edgeCost, source );

		if (tryGetPaths( target, out var path ))
		{
			string str		= source;
			string cur		= source;
			foreach( var edge in path )
			{
				string nxt		= edge.Other( cur );
				str				+= " -> " + nxt;
				cur				= nxt;
			}
			Debug.Log( str );
		}
		else
			Debug.Log( "Path not found!" );
	}
}

