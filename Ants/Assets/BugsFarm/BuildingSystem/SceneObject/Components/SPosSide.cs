using BugsFarm.AstarGraph;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public struct SPosSide : IPosSide
    {
        public bool LookLeft { get; set; }

        public Vector2 Position { get; set; }

        public SPosSide(IPosSide point)
        {
            if (point.IsNullOrDefault())
            {
                Position = default;
                LookLeft = false;
                return;
            }

            Position = point.Position;
            LookLeft = point.LookLeft;
        }
        public SPosSide(Vector2 position, bool lookLeft = false)
        {
            Position = position;
            LookLeft = lookLeft;
        }

        public bool Equals(SPosSide other)
        {
            return Position.Equals(other.Position) && LookLeft == other.LookLeft;
        }

        public override bool Equals(object obj)
        {
            return obj is SPosSide other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (LookLeft.GetHashCode() * 397) ^ Position.GetHashCode();
            }
        }

        public static bool operator ==(SPosSide a, SPosSide b)
        {
            return a.Position == b.Position && a.LookLeft == b.LookLeft;
        }

        public static bool operator ==(SPosSide a, MB_PosSide b)
        {
            return a.Position == b.Position && a.LookLeft == b.LookLeft;
        }

        public static bool operator !=(SPosSide a, MB_PosSide b)
        {
            return a.Position != b.Position && a.LookLeft != b.LookLeft;
        }

        public static bool operator !=(SPosSide a, SPosSide b)
        {
            return a.Position != b.Position && a.LookLeft != b.LookLeft;
        }

        public static implicit operator SPosSide(MB_PosSide x)
        {
            return new SPosSide {Position = x.Position, LookLeft = x.LookLeft};
        }

        public override string ToString()
        {
            return base.ToString() + $" {Position}";
        }
    }
}