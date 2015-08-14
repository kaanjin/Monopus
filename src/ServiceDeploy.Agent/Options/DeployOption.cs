namespace ServiceDeploy.Agent.Options
{
    internal class DeployOption : IDeployOption
    {
        private readonly string _installPath;

        public DeployOption(string installPath)
        {
            _installPath = installPath;
        }

        public string InstallPath
        {
            get { return _installPath; }
        }
    }
}
