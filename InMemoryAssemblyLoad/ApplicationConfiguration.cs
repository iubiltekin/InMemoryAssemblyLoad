using System.Configuration;

namespace InMemoryAssemblyLoad
{
    internal class ApplicationConfiguration
    {
        public string FileServerBaseUrl { get; set; }

        public static ApplicationConfiguration Create()
        {
            var applicationConfiguration = new ApplicationConfiguration();
            if (ConfigurationManager.AppSettings[nameof(FileServerBaseUrl)] != null)
                applicationConfiguration.FileServerBaseUrl = ConfigurationManager.AppSettings[nameof(FileServerBaseUrl)];

            return applicationConfiguration;
        }
    }
}