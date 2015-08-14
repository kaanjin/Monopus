using System.Globalization;

namespace ServiceDeploy.Agent
{
    partial class App : IApp
    {
        public string Url
        {
            get
            {
                var baseUrl = string.Format("http://{0}:{1}/", Default.Host, Default.Port);

                string url = baseUrl;
                if (!string.IsNullOrWhiteSpace(Host))
                    url = url.Replace("*:", Host + ":");
                if (Port > 0)
                    url = url.Replace(":" + Default.Port, ":" + Port.ToString(CultureInfo.InvariantCulture));
                return url;
            }
        }
    }
}
