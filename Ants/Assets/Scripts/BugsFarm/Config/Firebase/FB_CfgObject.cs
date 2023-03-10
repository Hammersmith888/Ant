using System.Collections.Generic;
using System.Linq;
using BugsFarm.Config;

public class FB_CfgObject
{
	public int		price;
	public int		maxCount;

	public List< Dictionary< string, float > >		upgrades;

	public List< float >							stages;


	public void SetUpgrades( ObjType type, int subType, CfgUpgrades cfg )
	{
		upgrades		= new List< Dictionary< string, float > >();

		foreach (UpgradeLevel cfgLevel in cfg.levels)
		{
			upgrades.Add( new Dictionary< string, float >() );

			var fb		= upgrades.Last();
			var key		= ( type, subType );

			if (cfg.showParam1)		fb.Add( FB_Packer.ObjParams[ key ][ 0 ], cfgLevel.param1 );
			if (cfg.showParam2)		fb.Add( FB_Packer.ObjParams[ key ][ 1 ], cfgLevel.param2 );
			if (cfg.showParam3)		fb.Add( FB_Packer.ObjParams[ key ][ 2 ], cfgLevel.param3 );

			fb.Add( FB_Packer.UpgradePrice,		cfgLevel.price		);
			fb.Add( FB_Packer.UpgradeTime,		cfgLevel.minutes	);
		}
	}
}

