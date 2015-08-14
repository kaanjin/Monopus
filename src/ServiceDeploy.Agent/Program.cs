using System;
using System.Configuration;
using System.Threading;
using ServiceDeploy.Agent.Bootstrap;
using Topshelf;

namespace ServiceDeploy.Agent
{
    internal class Program
    {
        private static readonly Semaphore Semaphore = new Semaphore(1, 1, "ServiceDeploy");

        private static int Main()
        {
            if (!Semaphore.WaitOne(0))
                return -1;

            var exitCode = (int) HostFactory.Run(hc =>
            {
                hc.UseAssemblyInfoForServiceInfo();

                hc.Service(() => new OwinHostControl(App.Default));

                hc.UseLog4Net(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath);

                hc.StartAutomaticallyDelayed();
                hc.RunAsLocalSystem();
            });

            if (exitCode != 0)
                Console.ReadKey();

            return exitCode;
        }
    }
}
