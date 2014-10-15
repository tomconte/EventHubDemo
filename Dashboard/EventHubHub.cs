using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dashboard
{
    public class EventHubHub : Hub
    {
        public void EventUpdate(string device, string data)
        {
            Clients.All.eventUpdate(device, data);
        }
    }
}