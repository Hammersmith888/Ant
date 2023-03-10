using UnityEngine;


public static class Kinematics
{
	public static float CalcTime( float distance, float a )
	{
		return Mathf.Sqrt( 2 * distance / a );
	}


	public static float CalcAcc( float distance, float time )
	{
		// d = att/2 ==> a = 2d/t^2

		float acc		= 2 * distance / Mathf.Pow( time, 2 );
		
		return acc;
	}


	public static float CalcDec( float distance, float velocity )
	{
		// d = att/2, v = at ==> t = 2d/v, a = vv/2d

		float deceleration		= Mathf.Pow( velocity, 2 ) / (2 * distance);
		
		return deceleration;
	}


	public static float Accelerate( ref float vel_Cur, float vel_Tgt, float a_abs, float deltaTime )
	{
		float a					= (vel_Tgt > vel_Cur ? 1 : -1 ) * a_abs;

		float time_AccFull		= (vel_Tgt - vel_Cur) / a;
		float time_Acc			= Mathf.Min( time_AccFull, deltaTime );
		float time_Fix			= deltaTime - time_Acc;

		float dist_Acc			= vel_Cur * time_Acc + a * time_Acc * time_Acc / 2;

		vel_Cur					+= a * time_Acc;

		float dist_Fix			= vel_Cur * time_Fix;

		return dist_Acc + dist_Fix;
	}


	public static void Accelerate( Rigidbody2D rb, float vel_Tgt, float a_abs, Vector2 axis )
	{
		float vel_Delta			= vel_Tgt - Vector2.Dot( rb.velocity, axis );
		float a_abs_capped		= Mathf.Min( Mathf.Abs( vel_Delta ) / Time.fixedDeltaTime, a_abs );
		float a					= (vel_Delta > 0 ? 1 : -1) * a_abs_capped;
		Vector2 force			= rb.mass * a * axis;

		rb.AddForce( force );
	}


	/*
		Formula for a_abs_capped looks wrong, but somehow it's working...
	*/
	public static void AcceleratePos_1( Rigidbody2D rb, float tgt_Pos, float a_abs, Vector2 axis )
	{
		float vel_Cur			= Vector2.Dot( rb.velocity, axis );
		float pos_Delta			= tgt_Pos - Vector2.Dot( rb.transform.position, axis );
		float a_abs_capped		= Mathf.Min( a_abs, Mathf.Abs( pos_Delta - vel_Cur * Time.fixedDeltaTime ) / Mathf.Pow( Time.fixedDeltaTime / 2, 2 ) );
		float a					= (pos_Delta > 0 ? 1 : -1) * a_abs_capped;

		if (vel_Cur * pos_Delta > 0)
		{
			float time_Dec			= Mathf.Abs( vel_Cur ) / a_abs_capped + Time.fixedDeltaTime;
			float dist_DecAbs		= a_abs_capped * Mathf.Pow( time_Dec, 2 ) / 2;

			if (dist_DecAbs > Mathf.Abs( pos_Delta ))
				a					*= -1;
		}

		Vector2 force			= rb.mass * a * axis;

		rb.AddForce( force );
	}


	public static void AcceleratePos_2( Rigidbody2D rb, float tgt_Pos, float a_abs, Vector2 axis )
	{
		/*
			d = vt + att/2
			a = 2(d - vt)/tt
		*/

		float vel_Cur			= Vector2.Dot( rb.velocity, axis );
		float pos_Delta			= tgt_Pos - Vector2.Dot( rb.transform.position, axis );

		float a_max_Vel			= - vel_Cur / Time.fixedDeltaTime;
		float a_max_Dist		= 2 * (pos_Delta - vel_Cur * Time.fixedDeltaTime) / Mathf.Pow( Time.fixedDeltaTime, 2 );
		float a_abs_max			= Mathf.Max( Mathf.Abs( a_max_Vel ), Mathf.Abs( a_max_Dist ) );
		float a_abs_capped		= Mathf.Min( a_abs, a_abs_max );

		float a					= (pos_Delta > 0 ? 1 : -1) * a_abs_capped;

		if (vel_Cur * pos_Delta > 0)
		{
			float error				= Time.fixedDeltaTime;
			float time_Dec			= Mathf.Abs( vel_Cur ) / a_abs_capped + error;
			float dist_DecAbs		= a_abs_capped * Mathf.Pow( time_Dec, 2 ) / 2;

			if (dist_DecAbs > Mathf.Abs( pos_Delta ))
				a					*= -1;
		}

		Vector2 force			= rb.mass * a * axis;

		rb.AddForce( force );
	}


	public static void DecelerateTorque( Rigidbody2D rb, float a_abs )
	{
		float velocity			= rb.angularVelocity;
		float a_abs_capped		= Mathf.Min( a_abs, Mathf.Abs( velocity ) / Time.fixedDeltaTime );
		float a					= (velocity > 0 ? -1 : 1) * a_abs_capped;

		rb.AddTorque( rb.inertia * a * Time.fixedDeltaTime, ForceMode2D.Force );
	}


	public static void AccelerateTorque( Rigidbody2D rb, float pos_Tgt, float a_abs )
	{
		float dist				= DecelerationDist( rb, pos_Tgt, a_abs );
		float pos_DecEnd		= rb.rotation + dist * (rb.angularVelocity > 0 ? 1 : -1);
		float pos_Tgt_2			= Mathf.RoundToInt( pos_DecEnd / 360 ) * 360;

		_AccelerateTorque( rb, pos_Tgt_2, a_abs );
	}


	/*
		TODO: Add limits
	*/
	static void _AccelerateTorque( Rigidbody2D rb, float pos_Tgt, float a_abs )
	{
		float velocity			= rb.angularVelocity;
		float pos_Delta			= pos_Tgt - rb.rotation;
		float a					= (pos_Delta > 0 ? 1 : -1) * a_abs;

		if (velocity * pos_Delta > 0)
		{
			float dist			= DecelerationDist( rb, pos_Tgt, a_abs );

			if (dist > Mathf.Abs( pos_Delta ))
				a				*= -1;
		}

		rb.AddTorque( rb.inertia * a * Time.fixedDeltaTime, ForceMode2D.Force );
	}


	static float DecelerationDist( Rigidbody2D rb, float pos_Tgt, float a_abs )
	{
		float time		= Mathf.Abs( rb.angularVelocity ) / a_abs;
		float n			= time / Time.fixedDeltaTime;
		float dist		= a_abs * Mathf.Pow( Time.fixedDeltaTime, 2 ) * n * (n + 1) / 2;
		
		return dist;
	}
}

