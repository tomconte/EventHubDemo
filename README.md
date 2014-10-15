EventHubDemo
============

This Azure Event Hub demo shows messages streaming in real time from an Event Hub to a Web page using SignalR, and a Node.JS client to generate traffic. The Node.JS client uses Publisher URIs and Shared Access Signatures to securely send the data to Event Hubs.

- How to configure

You will first need to go the Azure Management Console to create a Service Bus namespace and an Event Hub.

You must create a Shared Access Key for your Event Hub: in the Configuration tab, in the Shared Access Policies section, give the new policy a name like "Sender" and "Send" permissions. This makes sure that your clients will only be able to send data.

To configure the Node.JS client, in `bench_eventhubs.js`, change the following values:

- `namespace` sould contain the name of your Service Bus namespace.
- `hubname` should contain the name of your Event Hub.
- `my_key_name` should contain the Policy Name that you created.
- `my_key` should contain the Primary (or Secondary) Key from your Event Hub configuration.

In the Visual Studio solution, you will also need to configure the same values in the `app.settings` file for the `EventReceiver` solution, in the `appSettings` section:

- `Microsoft.ServiceBus.ConnectionString` is your Service Bus connection string. You will find it in the Connection Information button of your top-level Service Bus configuration.
- `EventHubName` is the name of your Event Hub.
- `ApplicationUrl` is the URL of the Web application where SignalR will run; if running locally you should use something like "http://127.0.0.1:81/" and if you are deploying to Azure, the URL of your deployment, e.g. "http://test.cloudapp.net/".
- `AzureStorageConnectionString` is a Storage connection string that will be used by the EventHub library to coordinate the different consumer Worker Roles.

- How to use

Before you can run the Node.JS client, you should type `npm install` from the `EventHubJS` directory to install dependencies.

Then you can run `node bench_eventhubs.js` to send a bunch of requests. The script is configured to send 10,000 requests.

Now you can start the EventHubDemo solution from Visual Studio (in the Azure Emulator), or deploy it. It is configured to launch two instances of the Receiver role, and two instances of the Web Dashboard.

Open the Web Dashboard page: you should see all the messages sent to the EventHub stream in real-time to your browser.
