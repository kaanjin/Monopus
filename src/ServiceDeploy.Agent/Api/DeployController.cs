using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NuGet;
using ServiceDeploy.Agent.NuGet;
using ServiceDeploy.Agent.Options;
using ServiceDeploy.Agent.PowerShell;
using Topshelf.Logging;

namespace ServiceDeploy.Agent.Api
{
    [RoutePrefix("api")]
    public class DeployController : ApiController
    {
        static readonly LogWriter Log = HostLogger.Get<DeployController>();

        private readonly string _sourcePath = Path.Combine(Path.GetTempPath(), "ServiceDeployAgent");

        private readonly IDeployOption _deployOption;

        public DeployController(IDeployOption deployOption)
        {
            _deployOption = deployOption;
        }

        [HttpPost, Route("deploy")]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();

            await Request.Content.ReadAsMultipartAsync(provider);

            string output = null;

            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');

                if (!Directory.Exists(_sourcePath))
                    Directory.CreateDirectory(_sourcePath);

                var path = Path.Combine(_sourcePath, filename);

                using (var stream = File.Create(path))
                {
                    var s = await file.ReadAsByteArrayAsync();
                    await stream.WriteAsync(s, 0, s.Length);
                }

                if (!PackageHelper.IsPackageFile(path))
                {
                    var ex = new Exception("File is not a package!");
                    Log.Error(ex);
                    return InternalServerError(ex);
                }

                var pkg = new OptimizedZipPackage(path);

                try
                {
                    await pkg.Uninstall(_sourcePath, _deployOption.InstallPath);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Error(ex);

                    if (!ex.Message.StartsWith("Unable to find package"))
                        throw;
                }
                catch (TimeoutException ex)
                {
                    Log.Error(ex);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return InternalServerError(ex);
                }

                string installPath;

                try
                {
                    installPath = await pkg.Install(_sourcePath, _deployOption.InstallPath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return InternalServerError(ex);
                }

                string deployScript;

                if ((deployScript = await pkg.GetDeployScript()) != null)
                {
                    var args = new ListDictionary
                    {
                        {"installPath", installPath},
                        {"toolsPath", Path.Combine(installPath, "tools")},
                        {"package", pkg},
                        {"project", null}
                    };

                    if (!Script.Execute(deployScript, args, ref output))
                    {
                        var ex = new Exception(output);
                        Log.Error(ex);
                        return InternalServerError(ex);
                    }
                }
            }

            Log.InfoFormat("Deploy success: {0}", output);

            return Ok(output);
        }
    }
}
