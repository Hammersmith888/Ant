using System;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct Vector3S
    {
        public float X
        {
            get => _x;
            set => _x = value;
        }

        public float Y
        {
            get => _y;
            set => _y = value;
        }

        public float Z
        {
            get => _z;
            set => _z = value;
        }

        [SerializeField] private float _x;
        [SerializeField] private float _y;
        [SerializeField] private float _z;

        public Vector3S(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3S point && point == this;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{X}. {Y}. {Z}]";
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public void Set(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }

        public void Set(Vector2 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = 0;
        }

        public static bool operator ==(Vector3S a, Vector3S b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(Vector3S a, Vector3S b)
        {
            return a.X != b.X && a.Y != b.Y && a.Z != b.Z;
        }
        
        public static bool operator ==(Vector3S a, Vector3 b)
        {
            return a.X == b.x && a.Y == b.y && a.Z == b.z;
        }
        public static bool operator !=(Vector3S a, Vector3 b)
        {
            return a.X != b.x && a.Y != b.y && a.Z != b.z;
        }

        public static implicit operator Vector2(Vector3S x)
        {
            return new Vector2(x.X, x.Y);
        }

        public static implicit operator Vector3S(Vector2 x)
        {
            return new Vector3S(x.x, x.y, 0);
        }

        public static implicit operator Vector3(Vector3S x)
        {
            return new Vector3(x.X, x.Y, x.Z);
        }

        public static implicit operator Vector3S(Vector3 x)
        {
            return new Vector3S(x.x, x.y, x.z);
        }
    }
}