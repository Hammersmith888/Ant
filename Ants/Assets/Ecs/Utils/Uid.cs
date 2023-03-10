using System;
using UnityEngine;

namespace Ecs.Utils
{
	[Serializable]
	public struct Uid : IEquatable<Uid>
	{
		public static readonly Uid Empty = new Uid(-1);
        
		[SerializeField]
		private int _value;

		private Uid(int value)
		{
			_value = value;
		}

		public bool Equals(Uid other)
		{
			return _value == other._value;
		}

		public override bool Equals(object obj)
		{
			return obj is Uid uid && Equals(uid);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public static explicit operator Uid(int value)
		{
			return new Uid(value);
		}

		public static explicit operator int(Uid uid)
		{
			return uid._value;
		}

		public static bool operator ==(Uid a, Uid b)
		{
			return a._value == b._value;
		}

		public static bool operator !=(Uid a, Uid b)
		{
			return a._value != b._value;
		}

		public override string ToString()
		{
			return $"Uid #{_value}";
		}
	}
}