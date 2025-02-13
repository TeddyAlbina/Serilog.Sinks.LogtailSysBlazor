using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Serilog.Events;
using Serilog.Formatters.Models;
using Serilog.Formatting;

namespace Serilog.Sinks.Logtail
{
    /// <inheritdoc />
    /// <summary>
    /// Formats messages that comply with syslog RFC5424 & Logtail
    /// https://tools.ietf.org/html/rfc5424
    /// </summary>
    public partial class LogtailFormatter : LogtailFormatterBase
    {
        [GeneratedRegex("[=\\\"\\]]")]
        private static partial Regex PropertyClean();

        [GeneratedRegex("[\\]\\\\\"]")]
        private static partial Regex PropertyCleanSpaceAndBackslashes();

        /// <summary>
        /// Used in place of data that cannot be obtained or is unavailable
        /// </summary>
        private const string NILVALUE = "-";

        private const string DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffzzz";

        private readonly string applicationName;
        private readonly string messageIdPropertyName;
        private readonly string tokenKey;
        private readonly string token;
        private string? operatingSystemPlatform;

        internal const string DefaultMessageIdPropertyName = "SourceContext";

        /// <summary>
        /// Initialize a new instance of <see cref="LogtailFormatter"/> class allowing you to specify values for
        /// the facility, application name, template formatter, and message Id property name.
        /// </summary>
        /// <param name="facility"><inheritdoc cref="Facility" path="/summary"/></param>
        /// <param name="applicationName">A user supplied value representing the application name that will appear in the syslog event. Must be all printable ASCII characters. Max length 48. Defaults to the current process name.</param>
        /// <param name="templateFormatter"><inheritdoc cref="LogtailFormatterBase.templateFormatter" path="/summary"/></param>
        /// <param name="messageIdPropertyName">Where the Id number of the message will be derived from. Defaults to the "SourceContext" property of the syslog event. Property name and value must be all printable ASCII characters with max length of 32.</param>
        /// <param name="sourceHost"><inheritdoc cref="LogtailFormatterBase.Host" path="/summary"/></param>
        /// <param name="severityMapping"><inheritdoc cref="LogtailFormatterBase" path="/param[@name='severityMapping']"/></param>
        /// <param name="tokenKey">The key of Logtail token, something like logtail@11111 source_token</param>
        /// <param name="token">Your source token from rsys logtail</param>
        /// <param name="processId">The process identifier</param>
        /// <param name="processName">The process name</param>
        public LogtailFormatter(
            string tokenKey,
            string token,
            Facility facility = Facility.Local0,
            string? applicationName = null,
            ITextFormatter? templateFormatter = null,
            string? messageIdPropertyName = DefaultMessageIdPropertyName,
            string? sourceHost = null,
            Func<LogEventLevel, Severity>? severityMapping = null,
            string? processId = null,
            string? processName = null)
            : base(facility, templateFormatter, sourceHost, severityMapping)
        {
            this.applicationName = applicationName ?? ProcessName;

            // Conform to the RFC
            this.applicationName = this.applicationName
                .AsPrintableAscii()
                .WithMaxLength(48);

            // Conform to the RFC
            this.messageIdPropertyName = (messageIdPropertyName ?? DefaultMessageIdPropertyName)
                .AsPrintableAscii()
                .WithMaxLength(32);

            this.tokenKey = tokenKey;
            this.token = token;
        }

        private string GetOsPlatform()
        {
            if (string.IsNullOrEmpty(this.operatingSystemPlatform))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("Browser")))
                {
                    this.operatingSystemPlatform = "browser";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    this.operatingSystemPlatform = "windows";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    this.operatingSystemPlatform = "linux";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                {
                    this.operatingSystemPlatform = "freebsd";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    this.operatingSystemPlatform = "macos";
                }
                else
                {
                    this.operatingSystemPlatform = "unknown";
                }
            }

            return this.operatingSystemPlatform;
        }
 
        public override string FormatMessage(LogEvent logEvent)
        {
            var priority = CalculatePriority(logEvent.Level);
            var messageId = GetMessageId(logEvent);

            var timestamp = logEvent.Timestamp.ToString(DATE_FORMAT);
            var sd = RenderStructuredData(logEvent);
            var msg = RenderMessage(logEvent);

            var level = logEvent.Level == LogEventLevel.Information ? "Info" : logEvent.Level.ToString();

            var logmessage = new LogMessage
            {
                Message = msg,
                Dt = timestamp,
                Level = level,
                Platform = "syslog",
                OsPlatform = this.GetOsPlatform(),
                Priority = priority,
                MessageId = messageId,
                SysLogMessage = new SysLogMessage
                {
                    AppName = applicationName ?? "syslog",
                    Facility = this.facility.ToString(),
                    Host = this.Host,
                    HostName = this.Host,
                    Extras = new Dictionary<string, object?>()
                }
            };
             
            if (logEvent.Exception is not null)
            {
                logmessage.SysLogMessage.Extras.Add("exceptionDetail", logEvent.Exception);
            }

            if (sd is { Count: > 0 })
            {
                logmessage.Properties = new Dictionary<string, string>(sd);
            }

            return JsonConvert.SerializeObject(logmessage);
        }

        /// <summary>
        /// Get the LogEvent's SourceContext in a format suitable for use as the MSGID field of a syslog message
        /// </summary>
        /// <param name="logEvent">The LogEvent to extract the context from</param>
        /// <returns>The processed SourceContext, or NILVALUE '-' if not set</returns>
        private string GetMessageId(LogEvent logEvent)
        {
            var hasMsgId = logEvent.Properties.TryGetValue(messageIdPropertyName, out var propertyValue);

            if (!hasMsgId)
                return NILVALUE;

            var result = propertyValue?
                .ToString()
                .TrimAndUnescapeQuotes();

            // Conform to the RFC's restrictions
            result = result?
                .AsPrintableAscii()
                .WithMaxLength(32);

            return result != null && result.Length >= 1
                ? result
                : NILVALUE;
        }

        private static Dictionary<string, string> RenderStructuredData(LogEvent logEvent)
            => logEvent.Properties.ToDictionary(kvp => RenderPropertyKey(kvp.Key), kvp => RenderPropertyValue(kvp.Value));

        private static string RenderPropertyKey(string propertyKey)
        {
            // Conform to the RFC's restrictions
            var result = propertyKey.AsPrintableAscii();

            // Also remove any '=', ']', and '"", as these are also not permitted in structured data parameter names
            // Unescaped regex pattern: [=\"\]]
            result = PropertyClean().Replace(result, string.Empty);

            return result.WithMaxLength(32);
        }

        /// <summary>
        /// All Serilog property values are quoted, which is unnecessary, as we are going to encase them in
        /// quotes anyway, to conform to the specification for syslog structured data values - so this
        /// removes them and also unescapes any others
        /// </summary>
        private static string RenderPropertyValue(LogEventPropertyValue propertyValue)
        {
            // Trim surrounding quotes, and unescape all others
            var result = propertyValue
                .ToString()
                .TrimAndUnescapeQuotes();

            // Use a backslash to escape backslashes, double quotes and closing square brackets
            return PropertyCleanSpaceAndBackslashes().Replace(result, match => $@"\{match}");
        }
    }
}
