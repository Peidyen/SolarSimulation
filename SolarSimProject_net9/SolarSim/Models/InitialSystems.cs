using System.Collections.Generic;
using SolarSim.Physics;

namespace SolarSim.Models
{
    public static class InitialSystems
    {
        public static List<Body> SunEarthMars()
        {
            const double M_sun = 1.98847e30, M_earth = 5.9722e24, M_mars = 6.4171e23;
            const double AU = 1.495978707e11;
            double v_e = System.Math.Sqrt(NBody.G * M_sun / (1.0 * AU));
            double v_m = System.Math.Sqrt(NBody.G * M_sun / (1.524 * AU));
            var sun   = new Body("Sun",   M_sun,   new Vector3d(0,0,0),           new Vector3d(0,0,0));
            var earth = new Body("Earth", M_earth, new Vector3d(AU,0,0),          new Vector3d(0, v_e, 0));
            var mars  = new Body("Mars",  M_mars,  new Vector3d(1.524*AU,0,0),    new Vector3d(0, v_m, 0));
            return new List<Body> { sun, earth, mars };
        }

        public static List<Body> SunEarthMarsJupiterSaturn()
        {
            const double M_sun = 1.98847e30, M_earth = 5.9722e24, M_mars = 6.4171e23, M_jup=1.89813e27, M_sat=5.6834e26;
            const double AU = 1.495978707e11;
            double r_e=1.0*AU, r_m=1.524*AU, r_j=5.204*AU, r_s=9.582*AU;
            double v_e=System.Math.Sqrt(NBody.G*M_sun/r_e);
            double v_m=System.Math.Sqrt(NBody.G*M_sun/r_m);
            double v_j=System.Math.Sqrt(NBody.G*M_sun/r_j);
            double v_s=System.Math.Sqrt(NBody.G*M_sun/r_s);
            var sun     = new Body("Sun",     M_sun, new Vector3d(0,0,0),          new Vector3d(0,0,0));
            var earth   = new Body("Earth",   M_earth, new Vector3d(r_e,0,0),      new Vector3d(0, v_e, 0));
            var mars    = new Body("Mars",    M_mars, new Vector3d(r_m,0,0),       new Vector3d(0, v_m, 0));
            var jupiter = new Body("Jupiter", M_jup,  new Vector3d(r_j,0,0),       new Vector3d(0, v_j, 0));
            var saturn  = new Body("Saturn",  M_sat,  new Vector3d(r_s,0,0),       new Vector3d(0, v_s, 0));
            return new List<Body> { sun, earth, mars, jupiter, saturn };
        }
    }
}
