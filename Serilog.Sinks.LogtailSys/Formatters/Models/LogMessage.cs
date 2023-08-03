using Newtonsoft.Json;

namespace Serilog.Formatters.Models;

internal class LogMessage
{
    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("dt")]
    public string? Dt { get; set; }

    [JsonProperty("level")]
    public string? Level { get; set; }

    [JsonProperty("platform")]
    public string? Platform { get; set; }

    [JsonProperty("osplatform")]
    public string? OsPlatform { get; set; }

    [JsonProperty("priority")]
    public int Priority { get; set; }

    [JsonProperty("msgid")]
    public string? MessageId { get; set; }

    [JsonProperty("syslog")]
    public SysLogMessage? SysLogMessage { get; set; }
}
