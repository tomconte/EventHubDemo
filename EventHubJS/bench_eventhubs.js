var https = require('https');
var crypto = require('crypto');
var moment = require('moment');
var BSON = require('buffalo');

// Allow more than the default of 5 open sockets
https.globalAgent.maxSockets = 100;

// Event Hubs parameters
var namespace = 'sample-ns'; // Replace with your Service Bus namespace
var hubname ='test'; // Replace with your Event Hub name

// Shared Access Key parameters
// Replace with the Policy Name and Primary (or Secondary) Key from your Event Hub configuration
var my_key_name = 'send';
var my_key = 'xxxxL53JxxxxspglxxxxlZbAxxxxX1z2xxxxzHqXxxxx';

// Sample payload to send
var payload = { Temperature: 1.0, Humidity: 0.25 };

// Prefix for device names
var deviceprefix = 'device-';

// Create a SAS token
// See http://msdn.microsoft.com/library/azure/dn170477.aspx

function create_sas_token(uri, key_name, key)
{
    // Token expires in one hour
    var expiry = moment().add(1, 'hours').unix();

    var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
    var hmac = crypto.createHmac('sha256', key);
    hmac.update(string_to_sign);
    var signature = hmac.digest('base64');
    var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;

    return token;
}

// Create a bunch of SAS tokens for different emulated devices

var device_tokens = [];

var devicename;
var device_sas;
var deviceid;

for (var i=0; i<100; i++) {
  devicename = deviceprefix + i;
  device_tokens.push(
    create_sas_token(
      'https://' + namespace + '.servicebus.windows.net' + '/' + hubname + '/publishers/' + devicename + '/messages',
      my_key_name,
      my_key));
}

// Set up a timer to measure total execution time

console.time('send requests');

process.on('exit', function() {
  console.timeEnd('send requests');
});

/*
** Send a batch of requests to the Event Hub
*/

function sendRequests(nb) {
  for (var i=0; i<nb; i++) {

    // Pick a random device and its corresponding SAS key
    deviceid = Math.floor((Math.random() * 100));
    devicename = deviceprefix + deviceid;
    device_sas = device_tokens[deviceid];

    // Assemble BSON payload
    payload.Temperature++;
    var b = BSON.serialize(payload);

    // Create the HTTP/S request
    var options = {
      hostname: namespace + '.servicebus.windows.net',
      port: 443,
      path: '/' + hubname + '/publishers/' + devicename + '/messages',
      method: 'POST',
      headers: {
        'Authorization': device_sas,
        'Content-Length': b.length,
        'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
      }
    };

    // Send the request
    var req = https.request(options, function(res) {
      //console.log("statusCode: ", res.statusCode);
      res.on('data', function(d) {
        process.stdout.write(d);
      });
    }).on('error', function(e) {
      console.error(e);
    });

    // Write the payload
    req.write(b);
    req.end();
  }
}

/*
** Launch a few batches of requests
*/

for (var i=0; i<100; i++) {
  sendRequests(100);
}
