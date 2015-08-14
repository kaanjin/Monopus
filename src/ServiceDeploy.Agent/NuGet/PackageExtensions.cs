using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;

namespace ServiceDeploy.Agent.NuGet
{
    public static class PackageExtensions
    {
        public static async Task<string> GetDeployScript(this IPackage package)
        {
            var deployFile = package.GetToolFiles().SingleOrDefault(f => f.EffectivePath == "deploy.ps1");

            if (deployFile != null)
            {
                using (var sr = new StreamReader(deployFile.GetStream()))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            return null;
        }

        public static async Task<string> Install(this IPackage package, string source, string dest)
        {
            IPackageRepository packageRepository = new LocalPackageRepository(source);
            IPackageManager packageManager = new PackageManager(packageRepository, dest);

            var tcs = new TaskCompletionSource<string>();

            var handler = new EventHandler<PackageOperationEventArgs>((sender, args) => tcs.SetResult(args.InstallPath));

            packageManager.PackageInstalled += handler;

            packageManager.InstallPackage(package, true, true);

            packageManager.PackageInstalled -= handler;

            if (await Task.WhenAny(tcs.Task, Task.Delay(5000)) == tcs.Task)
            {
                await tcs.Task;
                return tcs.Task.Result;
            }
            else
            {
                throw new TimeoutException("Package install timeout");
            }    
        }

        public static async Task Uninstall(this IPackage package, string source, string dest)
        {
            IPackageRepository packageRepository = new LocalPackageRepository(source);
            IPackageManager packageManager = new PackageManager(packageRepository, dest);

            var tcs = new TaskCompletionSource<string>();

            var handler = new EventHandler<PackageOperationEventArgs>((sender, args) => tcs.SetResult("Ok"));

            packageManager.PackageUninstalled += handler;

            packageManager.UninstallPackage(package, true, true);

            packageManager.PackageUninstalled -= handler;

            if (await Task.WhenAny(tcs.Task, Task.Delay(5000)) == tcs.Task)
            {
                await tcs.Task;
            }
            else
            {
                throw new TimeoutException("Package uninstall timeout");
            }    
        }
    }
}