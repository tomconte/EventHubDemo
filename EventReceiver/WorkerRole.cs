using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;

namespace EventReceiver
{
    public class WorkerRole : RoleEntryPoint
    {
        EventProcessorHost host;

        public override void Run()
        {
            string eventHubConnectionString = System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            string blobConnectionString = System.Configuration.ConfigurationManager.AppSettings["AzureStorageConnectionString"];
            string workerName = RoleEnvironment.CurrentRoleInstance.Role.Name;
            string eventHubName = System.Configuration.ConfigurationManager.AppSettings["EventHubName"];

            // Event Hub client
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);

            // Default Consumer Group
            EventHubConsumerGroup defaultConsumerGroup = eventHubClient.GetDefaultConsumerGroup();

            // Create the EventProcessorHost
            host = new EventProcessorHost(workerName, eventHubName, defaultConsumerGroup.GroupName, eventHubConnectionString, blobConnectionString);

            // Register our event processor
            host.RegisterEventProcessorAsync<SimpleEventProcessor>();

            // Wait forever
            while (true)
            {
                Task.Delay(60 * 1000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public override void OnStop()
        {
            host.UnregisterEventProcessorAsync().Wait();

            base.OnStop();
        }

    }
}
