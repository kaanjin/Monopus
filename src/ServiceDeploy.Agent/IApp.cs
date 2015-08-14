using System.Collections.Specialized;

namespace ServiceDeploy.Agent
{
    public interface IApp
    {
        string Url { get; }
        string InstallPath { get; }
        StringCollection IpAccessList { get; }
    }
}