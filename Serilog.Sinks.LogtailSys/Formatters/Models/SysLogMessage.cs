using System.Collections.Generic;

using Newtonsoft.Json;

namespace Serilog.Formatters.Models;

internal class SysLogMessage
{
    [JsonProperty("appname")]
    public string? AppName { get; set; }

    [JsonProperty("facility")]
    public string? Facility { get; set; }

    [JsonProperty("host")]
    public string? Host { get; set; }

    [JsonProperty("hostname")]
    public string? HostName { get; set; }

    [JsonProperty("logtail@11993")]
    public Dictionary<string, object?>? Extras { get; set; }
}
