using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using SolarSim.Physics;
using SolarSim.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5173");
var app = builder.Build();

var sim = new NBody(InitialSystems.SunEarthMarsJupiterSaturn());

app.MapGet("/state", () => new
{
    time_seconds = sim.Time,
    bodies = sim.Bodies.ConvertAll(b => new
    {
        name = b.Name,
        mass = b.Mass,
        position = new[] { b.Position.X, b.Position.Y, b.Position.Z },
        velocity = new[] { b.Velocity.X, b.Velocity.Y, b.Velocity.Z }
    })
});

app.MapPost("/step", (double dt, int steps) =>
{
    for (int i = 0; i < steps; i++) sim.Step(dt);
    return Results.Ok(new { ok = true, time_seconds = sim.Time });
});

app.MapPost("/reset", () => { sim.ResetTo(InitialSystems.SunEarthMarsJupiterSaturn()); return Results.Ok(new { ok = true }); });
app.MapGet("/diagnostics", () => {
    var d = Diagnostics.Compute(sim);
    return Results.Ok(new { kinetic = d.KineticEnergy, potential = d.PotentialEnergy, total = d.TotalEnergy, L = new[] { d.AngularMomentum.X, d.AngularMomentum.Y, d.AngularMomentum.Z }, L_mag = d.AngularMomentumMagnitude });
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
