using System.Collections.Generic;
using SolarSim.Physics;

namespace SolarSim.Models
{
    public static class InitialSystems
    {
        // Rough circular orbits in the ecliptic (z=0). Units: meters, kg, m/s.
        public static List<Body> SunEarthMars()
        {
            const double M_sun   = 1.98847e30;
            const double M_earth = 5.9722e24;
            const double M_mars  = 6.4171e23;
            const double AU = 1.495978707e11; // meters

            double v_earth = System.Math.Sqrt(NBody.G * M_sun / (1.0 * AU));
            double v_mars  = System.Math.Sqrt(NBody.G * M_sun / (1.524 * AU));

            var sun   = new Body("Sun",   M_sun,   new Vector3d(0, 0, 0),          new Vector3d(0, 0, 0));
            var earth = new Body("Earth", M_earth, new Vector3d(AU, 0, 0),         new Vector3d(0, v_earth, 0));
            var mars  = new Body("Mars",  M_mars,  new Vector3d(1.524 * AU, 0, 0), new Vector3d(0, v_mars, 0));

            return new List<Body>{ sun, earth, mars };
        }

        public static List<Body> SunEarthMarsJupiterSaturn()
        {
            const double M_sun    = 1.98847e30;
            const double M_earth  = 5.9722e24;
            const double M_mars   = 6.4171e23;
            const double M_jupiter= 1.89813e27;
            const double M_saturn = 5.6834e26;

            const double AU = 1.495978707e11; // meters

            double r_earth = 1.0 * AU;
            double r_mars  = 1.524 * AU;
            double r_jup   = 5.204 * AU;
            double r_sat   = 9.582 * AU;

            double v_earth = System.Math.Sqrt(NBody.G * M_sun / r_earth);
            double v_mars  = System.Math.Sqrt(NBody.G * M_sun / r_mars);
            double v_jup   = System.Math.Sqrt(NBody.G * M_sun / r_jup);
            double v_sat   = System.Math.Sqrt(NBody.G * M_sun / r_sat);

            var sun     = new Body("Sun",     M_sun,     new Vector3d(0, 0, 0),             new Vector3d(0, 0, 0));
            var earth   = new Body("Earth",   M_earth,   new Vector3d(r_earth, 0, 0),       new Vector3d(0, v_earth, 0));
            var mars    = new Body("Mars",    M_mars,    new Vector3d(r_mars, 0, 0),        new Vector3d(0, v_mars, 0));
            var jupiter = new Body("Jupiter", M_jupiter, new Vector3d(r_jup, 0, 0),         new Vector3d(0, v_jup, 0));
            var saturn  = new Body("Saturn",  M_saturn,  new Vector3d(r_sat, 0, 0),         new Vector3d(0, v_sat, 0));

            return new List<Body>{ sun, earth, mars, jupiter, saturn };
        }
    }
}
