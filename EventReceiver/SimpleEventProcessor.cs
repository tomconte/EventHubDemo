using Microsoft.AspNet.SignalR.Client;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EventReceiver
{
    // Need this class to parse the BSON...
    // Ideally we should parse to a dynamic object
    public class TelemetryData
    {
        public int Temperature { get; set; }
        public int Humidity { get; set; }
    }

    public class SimpleEventProcessor : IEventProcessor
    {
        private Stopwatch checkpointStopWatch;
        // SignalR
        HubConnection hubConnection;
        IHubProxy eventHubHubProxy;

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("EventProcessor stopped");

            return Task.FromResult<object>(null);
        }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("EventProcessor starting...");

            // Create SignalR connection
            string appUrl = System.Configuration.ConfigurationManager.AppSettings["ApplicationUrl"];
            hubConnection = new HubConnection(appUrl);
            eventHubHubProxy = hubConnection.CreateHubProxy("EventHubHub");
            hubConnection.Start().Wait(); // Wait for connection

            // Stopwatch for checkpoints
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();

            Trace.TraceInformation("EventProcessor started");

            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            List<Task> tasks = new List<Task>();

            foreach (EventData message in messages)
            {
                string key = message.PartitionKey;

                // Log the message
                Trace.TraceInformation(string.Format("Message received.  Partition: '{0}', Device: '{1}', SN: '{2}'", 
                    context.Lease.PartitionId, key, message.SequenceNumber));

                if (String.IsNullOrEmpty(key)) continue;

                try
                {
                    // Parse the device id out of the device name
                    int did = int.Parse(key.Substring(key.IndexOf('-') + 1));

                    // Deserialize the BSON payload

                    TelemetryData o;

                    using (MemoryStream ms = new MemoryStream(message.GetBytes()))
                    {
                        using (BsonReader reader = new BsonReader(ms))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            o = serializer.Deserialize<TelemetryData>(reader);
                        }
                    }

                    float temperature = o.Temperature;

                    // Send to SignalR
                    tasks.Add(eventHubHubProxy.Invoke("EventUpdate", did, temperature));
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error processing {0} : ", message.SequenceNumber);
                    Trace.TraceError(ex.Message);
                    Trace.TraceError(ex.StackTrace);
                    throw;
                }
            }

            // Wait for all tasks
            Trace.TraceInformation("Waiting for {0} tasks", tasks.Count);
            await Task.WhenAll(tasks);

            // Checkpoint every one minute
            if (checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(1))
            {
                await context.CheckpointAsync();
                lock (this)
                {
                    checkpointStopWatch.Reset();
                }
            }
        }
    }
}
