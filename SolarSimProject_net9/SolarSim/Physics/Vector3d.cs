using System;

namespace SolarSim.Physics
{
    public readonly struct Vector3d
    {
        public readonly double X, Y, Z;
        public Vector3d(double x, double y, double z) { X = x; Y = y; Z = z; }
        public static Vector3d operator +(Vector3d a, Vector3d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3d operator -(Vector3d a, Vector3d b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3d operator *(Vector3d a, double s) => new(a.X * s, a.Y * s, a.Z * s);
        public static Vector3d operator /(Vector3d a, double s) => new(a.X / s, a.Y / s, a.Z / s);
        public double Magnitude() => Math.Sqrt(X*X + Y*Y + Z*Z);
        public Vector3d Normalized(){ var m = Magnitude(); return m==0 ? new Vector3d(0,0,0) : this / m; }
        public override string ToString() => $"({X:E3}, {Y:E3}, {Z:E3})";
        public static readonly Vector3d Zero = new(0,0,0);
        public static Vector3d Cross(in Vector3d a, in Vector3d b)
            => new(a.Y*b.Z - a.Z*b.Y, a.Z*b.X - a.X*b.Z, a.X*b.Y - a.Y*b.X);
        public static double Dot(in Vector3d a, in Vector3d b) => a.X*b.X + a.Y*b.Y + a.Z*b.Z;
    }
}
