using System;
using Microsoft.Owin.Hosting;

namespace BossWavePlugin.Host
{

    public class WebHost
    {
        protected IDisposable mHost_;
        public bool Start()
        {
            var options = new StartOptions();
            options.Urls.Add("http://localhost:7100/");
            options.AppStartup = "BossWavePlugin.Host.Startup, BossWavePlugin";
            mHost_ = WebApp.Start(options);
            return true;
        }

        public bool Stop()
        {
            if (mHost_ == null)
            {
                return true;
            }
            mHost_.Dispose();
            mHost_ = null;
            return true;
        }
    }
}
