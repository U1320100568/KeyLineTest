using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


[assembly:OwinStartup(typeof(KeyLineTest.Services.Startup))]
namespace KeyLineTest.Services
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }

    public class FileUploaderHub : Hub
    {
        static IHubContext HubContext =
            GlobalHost.ConnectionManager.GetHubContext<FileUploaderHub>();

        public  static void UpdateProcess(string connId,string name, float percentage, string message = null)
        {
            HubContext.Clients.Client(connId).updateProgress(name, percentage);
        }
    }
}