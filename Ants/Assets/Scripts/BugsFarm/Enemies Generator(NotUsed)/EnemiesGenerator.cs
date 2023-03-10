using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class EnemiesGenerator : MonoBehaviour
{
	readonly List< AntType > enemies		= new List< AntType >
	{
		AntType.PotatoBug,
		AntType.Worm,
		AntType.Cockroach,
	};


	readonly Dictionary< AntType, string > type2code		= new Dictionary<AntType, string>()
	{
		[ AntType.Worm ]			= "Червяк",
		[ AntType.PotatoBug ]		= "Жук",
		[ AntType.Cockroach ]		= "Таро",
	};


	void Start()
	{
		var squadMin			= new List< GenUnit >
		{
			new GenUnit( AntType.Worm,  1 ),
		};
		var squadMax			= new List< GenUnit >
		{
			new GenUnit( AntType.Cockroach,  12 ),
			new GenUnit( AntType.Cockroach,  12 ),
			new GenUnit( AntType.Cockroach,  12 ),
		};
		float C					= 6;

		float p0				= CalcPower( squadMin );
		float p1				= CalcPower( squadMax );

		string text				= string.Empty;

		for (int i = 1; i <= 100; i ++)
		{
			float power			= CfgUnit.CalcExpCurve( i, new Vector2( 1, 100 ), new Vector2( p0, p1 ), C );
			var squad			= BestRandomSquad( power );

			PrintSquad( squad, ref text );
			// Debug.Log( Mathf.RoundToInt( power ) );
		}


		SavingSystem.SaveText( "G:/enemies.csv", text );


		/*
		var squad				= BestRandomSquad( 1000 );
		PrintSquad( squad );
		*/
	}


	List< GenUnit > BestRandomSquad( float totalPower )
	{
		List< GenUnit > best	= null;
		float bestPower			= 0;

		for (int i = 0; i < 50; i ++)
		{
			var squad			= RandomSquad( totalPower );
			float power			= CalcPower( squad );

			if (
					best == null ||
					Mathf.Abs( power - totalPower ) < Mathf.Abs( bestPower - totalPower )
				)
			{
				best			= squad;
				bestPower		= power;
			}
		}

		return best;
	}


	List< GenUnit > RandomSquad( float totalPower )
	{
		var squad		= new List< GenUnit >();
		int n			= Random.Range( 1, 6 + 1 );
		float mul		= n * (n + 1) * .5f;							// Arithmetic Progression, so totalPower == mul * power
		float power		= totalPower / mul;

		for (int i = 0; i < n; i ++)
		{
			AntType type		= Tools.RandomItem( enemies );
			int level			= GetInversePower( type, power ) + Random.Range( -2, +2 + 1 );

			if (level < 1 || level > Constants.MaxUnitLevel)
				continue;

			squad.Add( new GenUnit( type, level ) );
		}

		return squad;
	}


	void Get1TypeSquad( AntType type, int count, float totalPower )
	{
		var squad		= new List< GenUnit >();
		float mul		= count * (count + 1) * .5f;					// Arithmetic Progression, so totalPower == mul * power
		float power		= totalPower / mul;
		int level		= GetInversePower( type, power );

		squad.AddRange( Enumerable.Repeat( new GenUnit( type, level ), count ) );

		// PrintSquad( squad );
	}


	int GetInversePower( AntType type, float power )
	{
		CfgUnit cfgUnit		= Data_Fight.Instance.units[ type ];
		int l0				= 1;
		int l1				= Constants.MaxUnitLevel;
		float p0			= cfgUnit.CalcPower( l0 );
		float p1			= cfgUnit.CalcPower( l1 );
		float C				= FindPowerC( type );

		float dividend		= Mathf.Exp( C ) * (p0 - power) - p1 + power;
		float divisor		= p0 - p1;
		float t				= Mathf.Log( dividend / divisor ) / C;

		int level			= Mathf.RoundToInt( l0 + (l1 - l0) * t );

		return level;
	}


	float FindPowerC( AntType type )
	{
		CfgUnit cfgUnit		= Data_Fight.Instance.units[ type ];

		float tMid			= .25f;
		int lMin			= 1;
		int lMax			= Constants.MaxUnitLevel;
		int lMid			= Mathf.RoundToInt( lMin*(1-tMid) + lMax*(tMid) );

		float pMin			= cfgUnit.CalcPower( lMin );
		float pMax			= cfgUnit.CalcPower( lMax );
		float pMid			= cfgUnit.CalcPower( lMid );
		float C				= (cfgUnit.HP_C + cfgUnit.Attack_C) / 2;
		float step			= (Mathf.Max( cfgUnit.HP_C, cfgUnit.Attack_C ) - C) * 4;
		float p				= 0;

		for (int i = 0; i < 30; i ++)
		{
			p				= CfgUnit.CalcExpCurve( lMid, new Vector2( 1, Constants.MaxUnitLevel ), new Vector2( pMin, pMax ), C );
			int dir			= (int)Mathf.Sign( pMid - p );
			C				-= dir * step;							// lower C means higher p
			step			/= 2;
		}

		// Debug.Log( $"{ type }: { C }" );
		
		return C;
	}


	void PrintSquad( List< GenUnit > squad, ref string text )
	{
		float power			= CalcPower( squad );
		string str			= $"Power: { power :N0} - ";

		for (int i = 0; i < 8 - squad.Count; i ++)
			text			+= ",";

		foreach (GenUnit unit in squad)
		{
			// string s		= $"{ type2code[ unit.type ] } { unit.level },";
			string s		= $"{ unit.type } { unit.level },";
			str				+= s;
			text			+= s;
		}

		text				+= "\n";

		Debug.Log( str );
	}


	float CalcPower( List< GenUnit > squad )
	{
		float accDPS		= 0;
		float accPower		= 0;

		for (int i = squad.Count - 1; i >= 0; i --)
		{
			GenUnit unit	= squad[ i ];
			accDPS			+= unit.DPS;
			accPower		+= unit.HP * accDPS;
		}

		return accPower;
	}
}

