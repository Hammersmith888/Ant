

public class GenUnit
{
	public AntType	type;
	public int		level;

	public CfgUnit	cfgUnit;

	public float	HP;
	public float	Attack;
	public float	DPS;
	public float	Power;


	public GenUnit( AntType type, int level )
	{
		this.type		= type;
		this.level		= level;

		cfgUnit			= Data_Fight.Instance.units[ type ];

		HP				= cfgUnit.CalcHP		( level );
		Attack			= cfgUnit.CalcAttack	( level );
		DPS				= cfgUnit.CalcDPS		( level );
		Power			= cfgUnit.CalcPower		( level );
	}
}

