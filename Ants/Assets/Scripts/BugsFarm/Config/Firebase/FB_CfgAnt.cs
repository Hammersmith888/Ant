using System;
using System.Collections.Generic;
using System.Linq;


[Serializable]
public struct FB_CfgAnt
{
	public int								Price;
	public string							Price_CCY;

	public FB_CfgAntConsumption				Consumption;

	public Dictionary< string, int >		TaskTime;

	public Dictionary< string, float >		Other;


	public FB_CfgAnt( AntType type )
	{
		CfgAnt cfg			= Data_Ants.Instance.GetData( type );

		Price				= cfg.price;
		Price_CCY			= FB_Packer.CCY[ cfg.currency ];
		Consumption			= cfg.consumption;


		
		TaskTime			= new Dictionary< string, int >();
		//foreach (var pair in cfg.TaskTime)
		//	TaskTime.Add( FB_Packer.Tasks[ pair.Key ], pair.Value );

	
		if (cfg.other.Any())
		{
			Other			= new Dictionary< string, float >();
			foreach (var pair in cfg.other)
				Other.Add( FB_Packer.OthAntParams[ pair.Key ], pair.Value );
		}
		else
		{
			Other			= null;
		}
	}
}

