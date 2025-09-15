using System;
using SolarSim.Physics;
using SolarSim.Models;

class Program
{
    static void Main(string[] args)
    {
        var system = new NBody(InitialSystems.SunEarthMarsJupiterSaturn());

        Console.WriteLine("SolarSim — Sun/Earth/Mars/Jupiter/Saturn (rough circular orbits)\n");
        double dt = 60 * 60 * 6; // 6 hours per step
        for (int i = 0; i < 400; i++)
        {
            system.Step(dt);
            if (i % 10 == 0)
            {
                PrintState(system);
                var d = SolarSim.Physics.Diagnostics.Compute(system);
                Console.WriteLine($"  KE={d.KineticEnergy:E3}  PE={d.PotentialEnergy:E3}  E={d.TotalEnergy:E3}  |L|={d.AngularMomentumMagnitude:E3}\n");
            }
        }
    }

    static void PrintState(NBody sim)
    {
        Console.WriteLine($"t = {sim.Time / 86400.0:F2} days");
        foreach (var b in sim.Bodies)
        {
            Console.WriteLine($"  {b.Name,-7} pos={b.Position} vel={b.Velocity}");
        }
        Console.WriteLine();
    }
}
