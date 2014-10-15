using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Dashboard.Startup))]
namespace Dashboard
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}