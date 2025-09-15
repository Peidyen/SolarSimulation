using System;

namespace SolarSim.Physics
{
    public readonly struct DiagnosticsResult
    {
        public double KineticEnergy { get; }
        public double PotentialEnergy { get; }
        public double TotalEnergy => KineticEnergy + PotentialEnergy;
        public Vector3d AngularMomentum { get; }
        public double AngularMomentumMagnitude => AngularMomentum.Magnitude();
        public DiagnosticsResult(double ke, double pe, Vector3d L)
        { KineticEnergy = ke; PotentialEnergy = pe; AngularMomentum = L; }
    }

    public static class Diagnostics
    {
        public static DiagnosticsResult Compute(NBody sim)
        {
            double KE = 0.0;
            double PE = 0.0;
            Vector3d L = Vector3d.Zero;
            var bodies = sim.Bodies;
            int n = bodies.Count;

            // KE and angular momentum
            for (int i = 0; i < n; i++)
            {
                var b = bodies[i];
                var v = b.Velocity;
                KE += 0.5 * b.Mass * (v.X * v.X + v.Y * v.Y + v.Z * v.Z);
                L = L + Vector3d.Cross(b.Position, b.Velocity) * b.Mass;
            }

            // PE (pairwise)
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    var bi = bodies[i]; var bj = bodies[j];
                    var r = (bj.Position - bi.Position).Magnitude() + 1e-3;
                    PE += -NBody.G * bi.Mass * bj.Mass / r;
                }

            return new DiagnosticsResult(KE, PE, L);
        }
    }
}
