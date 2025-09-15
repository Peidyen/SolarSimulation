using System.Text.Json;
using System.Text.Json.Serialization;
using SolarSim.Physics;
using SolarSim.Models;

public class RpcRequest
{
    [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; } = "2.0";
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("method")] public string Method { get; set; } = string.Empty;
    [JsonPropertyName("params")] public JsonElement? Params { get; set; }
}

public class RpcResponse<T>
{
    [JsonPropertyName("jsonrpc")] public string JsonRpc { get; set; } = "2.0";
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("result")] public T? Result { get; set; }
    [JsonPropertyName("error")] public object? Error { get; set; }
}

public class BodyDTO
{
    public string name { get; set; } = string.Empty;
    public double mass { get; set; }
    public double[] position { get; set; } = new double[3];
    public double[] velocity { get; set; } = new double[3];
}

public class DiagnosticsDTO
{
    public double kinetic { get; set; }
    public double potential { get; set; }
    public double total { get; set; }
    public double[] L { get; set; } = new double[3];
    public double L_mag { get; set; }
}

public static class Mapper
{
    public static BodyDTO ToDTO(Body b) => new()
    {
        name = b.Name,
        mass = b.Mass,
        position = new[] { b.Position.X, b.Position.Y, b.Position.Z },
        velocity = new[] { b.Velocity.X, b.Velocity.Y, b.Velocity.Z }
    };
}

class MCPServer
{
    private readonly NBody _sim;
    private readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public MCPServer()
    {
        _sim = new NBody(InitialSystems.SunEarthMarsJupiterSaturn());
    }

    public void Run()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            try
            {
                var req = JsonSerializer.Deserialize<RpcRequest>(line, _opts)!;
                string method = req.Method;
                switch (method)
                {
                    case "tools/list":
                        Respond(req.Id, new
                        {
                            tools = new[]
                            {
                                new { name = "list_bodies", description = "List body names" },
                                new { name = "get_state", description = "Get full system state" },
                                new { name = "diagnostics", description = "Energy & angular momentum" },
                                new { name = "step", description = "Advance the simulation", schema = new { dt = "seconds (double)", steps = "int (optional, default 1)" }},
                                new { name = "reset", description = "Reset to SEMJS (Sun/Earth/Mars/Jupiter/Saturn)" },
                                new { name = "load_preset", description = "Load initial system by name (SunEarthMars | SunEarthMarsJupiterSaturn)", schema = new { name = "string" }},
                                new { name = "set_body_state", description = "Set a body's state", schema = new { name = "string", mass = "kg (double, optional)", position = "[x,y,z] meters", velocity = "[vx,vy,vz] m/s" }},
                                new { name = "add_body", description = "Add a new body", schema = new { name = "string", mass = "kg (double)", position = "[x,y,z] meters", velocity = "[vx,vy,vz] m/s" }}
                            }
                        });
                        break;

                    case "tools/call":
                        HandleToolCall(req);
                        break;

                    default:
                        RespondError(req.Id, code: -32601, message: $"Unknown method '{method}'");
                        break;
                }
            }
            catch (Exception ex)
            {
                var res = new RpcResponse<object> { JsonRpc = "2.0", Id = null, Error = new { code = -32603, message = ex.Message } };
                Console.WriteLine(JsonSerializer.Serialize(res, _opts));
            }
        }
    }

    private void HandleToolCall(RpcRequest req)
    {
        if (req.Params is null) { RespondError(req.Id, -32602, "Missing params"); return; }
        using var doc = JsonDocument.Parse(req.Params.Value.GetRawText());
        var root = doc.RootElement;
        string tool = root.GetProperty("name").GetString()!;
        var args = root.TryGetProperty("arguments", out var a) ? a : default;

        switch (tool)
        {
            case "list_bodies":
                Respond(req.Id, new { bodies = _sim.Bodies.ConvertAll(b => b.Name) });
                break;

            case "get_state":
                Respond(req.Id, new
                {
                    time_seconds = _sim.Time,
                    bodies = _sim.Bodies.ConvertAll(Mapper.ToDTO)
                });
                break;

            case "diagnostics":
            {
                var d = SolarSim.Physics.Diagnostics.Compute(_sim);
                Respond(req.Id, new DiagnosticsDTO
                {
                    kinetic = d.KineticEnergy,
                    potential = d.PotentialEnergy,
                    total = d.TotalEnergy,
                    L = new[] { d.AngularMomentum.X, d.AngularMomentum.Y, d.AngularMomentum.Z },
                    L_mag = d.AngularMomentumMagnitude
                });
                break;
            }

            case "step":
                double dt = args.Value.GetProperty("dt").GetDouble();
                int steps = args.Value.TryGetProperty("steps", out var s) ? s.GetInt32() : 1;
                for (int i = 0; i < steps; i++) _sim.Step(dt);
                Respond(req.Id, new { ok = true, time_seconds = _sim.Time });
                break;

            case "reset":
                _sim.ResetTo(InitialSystems.SunEarthMarsJupiterSaturn());
                Respond(req.Id, new { ok = true });
                break;

            case "load_preset":
            {
                string name = args.Value.GetProperty("name").GetString()!;
                switch (name.ToLowerInvariant())
                {
                    case "sunearthmars":
                    case "sem":
                        _sim.ResetTo(InitialSystems.SunEarthMars());
                        break;
                    case "sunearthmarsjupitersaturn":
                    case "semjs":
                        _sim.ResetTo(InitialSystems.SunEarthMarsJupiterSaturn());
                        break;
                    default:
                        RespondError(req.Id, -32005, $"Unknown preset '{name}'");
                        return;
                }
                Respond(req.Id, new { ok = true });
                break;
            }

            case "set_body_state":
            {
                string name = args.Value.GetProperty("name").GetString()!;
                var b = _sim.Bodies.Find(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (b == null) { RespondError(req.Id, -32004, $"Body '{name}' not found"); return; }
                if (args.Value.TryGetProperty("mass", out var m)) b.Mass = m.GetDouble();
                var pos = args.Value.GetProperty("position").EnumerateArray();
                var vel = args.Value.GetProperty("velocity").EnumerateArray();
                double[] p = new double[3]; int i = 0; foreach (var e in pos) p[i++] = e.GetDouble();
                double[] v = new double[3]; i = 0; foreach (var e in vel) v[i++] = e.GetDouble();
                b.Position = new Vector3d(p[0], p[1], p[2]);
                b.Velocity = new Vector3d(v[0], v[1], v[2]);
                Respond(req.Id, new { ok = true });
                break;
            }

            case "add_body":
            {
                string name = args.Value.GetProperty("name").GetString()!;
                double mass = args.Value.GetProperty("mass").GetDouble();
                var pos = args.Value.GetProperty("position").EnumerateArray();
                var vel = args.Value.GetProperty("velocity").EnumerateArray();
                double[] p = new double[3]; int i = 0; foreach (var e in pos) p[i++] = e.GetDouble();
                double[] v = new double[3]; i = 0; foreach (var e in vel) v[i++] = e.GetDouble();
                _sim.Bodies.Add(new Body(name, mass, new Vector3d(p[0], p[1], p[2]), new Vector3d(v[0], v[1], v[2])));
                Respond(req.Id, new { ok = true });
                break;
            }

            default:
                RespondError(req.Id, -32601, $"Unknown tool '{tool}'");
                break;
        }
    }

    private void Respond<T>(string? id, T payload)
    {
        var res = new RpcResponse<T> { JsonRpc = "2.0", Id = id, Result = payload };
        Console.WriteLine(JsonSerializer.Serialize(res, _opts));
    }
    private void RespondError(string? id, int code, string message)
    {
        var res = new RpcResponse<object> { JsonRpc = "2.0", Id = id, Error = new { code, message } };
        Console.WriteLine(JsonSerializer.Serialize(res, _opts));
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.Error.WriteLine("SolarSim MCP server listening on STDIN/STDOUT (JSON-RPC 2.0)");
        new MCPServer().Run();
    }
}
