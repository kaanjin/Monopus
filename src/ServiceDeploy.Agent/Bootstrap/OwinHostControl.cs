using System;
using Microsoft.Owin.Hosting;
using Topshelf;
using Topshelf.Logging;

namespace ServiceDeploy.Agent.Bootstrap
{
    class OwinHostControl : ServiceControl
    {
        static readonly LogWriter Log = HostLogger.Get<OwinHostControl>();

        private IDisposable _webApp;

        private readonly IApp _appSettings;

        public OwinHostControl(IApp appSettings)
        {
            _appSettings = appSettings;
        }

        public bool Start(HostControl hostControl)
        {
            try
            {
                _webApp = WebApp.Start(_appSettings.Url, builder => new WebApiStartup(_appSettings).Configuration(builder));
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                _webApp.Dispose();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                return false;
            }

            return true;
        }
    }
}