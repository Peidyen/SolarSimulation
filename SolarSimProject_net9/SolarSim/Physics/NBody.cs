using System.Collections.Generic;

namespace SolarSim.Physics
{
    public class NBody
    {
        public const double G = 6.67430e-11;
        public List<Body> Bodies { get; }
        public double Time { get; private set; }
        public NBody(IEnumerable<Body> bodies){ Bodies = new List<Body>(bodies); Time = 0; }

        private Vector3d AccelerationOn(int i, IReadOnlyList<Body> state)
        {
            var ai = Vector3d.Zero;
            var bi = state[i];
            for (int j = 0; j < state.Count; j++)
            {
                if (i == j) continue;
                var bj = state[j];
                var r = bj.Position - bi.Position;
                var dist = r.Magnitude() + 1e-3;
                var a = r * (G * bj.Mass / (dist*dist*dist));
                ai = ai + a;
            }
            return ai;
        }

        public void Step(double dt)
        {
            int n = Bodies.Count;
            var y0 = new Body[n];
            for (int i=0;i<n;i++) y0[i] = Bodies[i].Clone();

            var k1_v = new Vector3d[n]; var k1_p = new Vector3d[n];
            var k2_v = new Vector3d[n]; var k2_p = new Vector3d[n];
            var k3_v = new Vector3d[n]; var k3_p = new Vector3d[n];
            var k4_v = new Vector3d[n]; var k4_p = new Vector3d[n];

            for (int i=0;i<n;i++){ k1_p[i] = y0[i].Velocity; k1_v[i] = AccelerationOn(i, y0); }

            var yk = new Body[n];
            for (int i=0;i<n;i++)
                yk[i] = new Body(y0[i].Name, y0[i].Mass, y0[i].Position + k1_p[i]*(dt/2.0), y0[i].Velocity + k1_v[i]*(dt/2.0));
            for (int i=0;i<n;i++){ k2_p[i] = yk[i].Velocity; k2_v[i] = AccelerationOn(i, yk); }

            for (int i=0;i<n;i++)
                yk[i] = new Body(y0[i].Name, y0[i].Mass, y0[i].Position + k2_p[i]*(dt/2.0), y0[i].Velocity + k2_v[i]*(dt/2.0));
            for (int i=0;i<n;i++){ k3_p[i] = yk[i].Velocity; k3_v[i] = AccelerationOn(i, yk); }

            for (int i=0;i<n;i++)
                yk[i] = new Body(y0[i].Name, y0[i].Mass, y0[i].Position + k3_p[i]*dt, y0[i].Velocity + k3_v[i]*dt);
            for (int i=0;i<n;i++){ k4_p[i] = yk[i].Velocity; k4_v[i] = AccelerationOn(i, yk); }

            for (int i=0;i<n;i++)
            {
                var pos = y0[i].Position + (k1_p[i] + k2_p[i]*2 + k3_p[i]*2 + k4_p[i]) * (dt/6.0);
                var vel = y0[i].Velocity + (k1_v[i] + k2_v[i]*2 + k3_v[i]*2 + k4_v[i]) * (dt/6.0);
                Bodies[i].Position = pos; Bodies[i].Velocity = vel;
            }
            Time += dt;
        }

        public void ResetTo(IEnumerable<Body> bodies){ Bodies.Clear(); Bodies.AddRange(bodies); Time = 0; }
    }
}
