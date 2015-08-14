using System.Net;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Owin;
using ServiceDeploy.Agent.Options;
using Topshelf.Logging;

namespace ServiceDeploy.Agent.Bootstrap
{
    class WebApiStartup
    {
        static readonly LogWriter Log = HostLogger.Get<WebApiStartup>();

        private readonly IApp _appSettings;

        public WebApiStartup(IApp appSettings)
        {
            _appSettings = appSettings;
        }

        public void Configuration(IAppBuilder app)
        {
            var listener = (HttpListener) app.Properties["System.Net.HttpListener"];

            listener.AuthenticationSchemeSelectorDelegate = r =>
            {
                Log.InfoFormat("Request from {0}", r.RemoteEndPoint);

                if (r.RemoteEndPoint != null && (_appSettings.IpAccessList.Contains(r.RemoteEndPoint.Address.ToString())))
                {
                    Log.InfoFormat("Ip listed, applying {0} authentication scheme", AuthenticationSchemes.Anonymous);
                    return AuthenticationSchemes.Anonymous;
                }

                Log.InfoFormat("Ip unknown, applying {0} authentication scheme", AuthenticationSchemes.IntegratedWindowsAuthentication);
                return AuthenticationSchemes.IntegratedWindowsAuthentication;
            };

            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).InstancePerRequest();
            builder.RegisterInstance(new DeployOption(_appSettings.InstallPath)).As<IDeployOption>();

;            var container = builder.Build();

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            var resolver = new AutofacWebApiDependencyResolver(container);
            config.DependencyResolver = resolver;

            app.UseAutofacMiddleware(container);
            app.UseWebApi(config);
        }
    }
}
