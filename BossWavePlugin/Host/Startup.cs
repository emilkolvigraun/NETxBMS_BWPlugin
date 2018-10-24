using System.IO;
using System.Reflection;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Owin;
using BossWavePlugin.Host;

[assembly: OwinStartup(typeof(Startup))]
namespace BossWavePlugin.Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            var hubConfig = new HubConfiguration { EnableDetailedErrors = true };
            app.MapSignalR(hubConfig);


            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fileSystem = new PhysicalFileSystem(Path.Combine(folderPath,"www"));
            var options = new FileServerOptions { EnableDirectoryBrowsing = false, FileSystem = fileSystem };
            app.UseFileServer(options);
        }
    }
}
