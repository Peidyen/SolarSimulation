namespace SolarSim.Physics
{
    public class Body
    {
        public string Name { get; set; }
        public double Mass { get; set; }
        public Vector3d Position { get; set; }
        public Vector3d Velocity { get; set; }
        public Body(string name, double mass, Vector3d position, Vector3d velocity)
        { Name = name; Mass = mass; Position = position; Velocity = velocity; }
        public Body Clone() => new(Name, Mass, Position, Velocity);
    }
}
