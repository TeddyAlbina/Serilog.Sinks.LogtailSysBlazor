using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Logtail
{
    /// <summary>
    /// Sink that writes events to a remote syslog service using UDP
    /// </summary>
    public class LogtailSink : IBatchedLogEventSink, IDisposable
    {
        private readonly HttpClient httpClient;

        private readonly ILogtailFormatter formatter;

        public LogtailSink(ILogtailFormatter formatter, string token, Uri? ingestionUri)
        {
            this.formatter = formatter;

            this.httpClient = new()
            {
                BaseAddress = ingestionUri is null ? new Uri("https://in.logs.betterstack.com", UriKind.Absolute) : ingestionUri
            };

            this.httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to send to the syslog service</param>
        public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events)
            {
                var message = formatter.FormatMessage(logEvent);

                try
                {
                    await httpClient.PostAsync("/", new StringContent(message, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                }
                catch (SocketException ex)
                {
                    SelfLog.WriteLine($"[{nameof(LogtailSink)}] error while sending log event to syslog {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public Task OnEmptyBatchAsync() => Task.CompletedTask;

        public void Dispose() => this.httpClient.Dispose();
    }
}
