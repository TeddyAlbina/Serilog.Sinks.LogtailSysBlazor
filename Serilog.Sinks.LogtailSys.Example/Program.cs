// See https://aka.ms/new-console-template for more information


using System.Text.Json;
using System.Text.Json.Serialization;

using Serilog;
using Serilog.Debugging;

var log = new LoggerConfiguration().WriteTo.Logtail(
        token: "",
        appName: "HI"
    )
    .Enrich.FromLogContext()
    .CreateLogger();
    
SelfLog.Enable(Console.Error);

try
{
    using var m = new MemoryStream();
    var t = await JsonSerializer.DeserializeAsync<Dummy>(m);
}
catch (Exception e)
{
    log.Error(e, "Hello, World! 22");

}



Console.ReadKey();

class Dummy {

    [JsonRequired]
    public string? Name { get; set; }
}