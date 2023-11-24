using IsometricShooterWebApp.Data.Models.Configuration;
using IsometricShooterWebApp.Data.Models.GameApi;
using System.Diagnostics;
using System.Text;

namespace IsometricShooterWebApp.Managers
{
    public class RefereeManager
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<RefereeManager> logger;
        private readonly AzureContainerInstancesManager azureContainerInstancesManager;
        private RefereeConfigurationOptions Options;

        public RefereeManager(IConfiguration configuration, ILogger<RefereeManager> logger, AzureContainerInstancesManager azureContainerInstancesManager)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.azureContainerInstancesManager = azureContainerInstancesManager;
            LoadOptions();
        }

        private void LoadOptions()
        {
            var section = configuration.GetSection("referee");

            section.GetReloadToken().RegisterChangeCallback(_ => { LoadOptions(); }, null);

            Options = section.Get<RefereeConfigurationOptions>();
        }

        public async Task<bool> RunReferee(GetRefereeRequestModel query)
        {
            if (configuration.GetValue("LocalDebugReferee", false))
                return true;

            return await azureContainerInstancesManager.CreateContainerAsync(query);
 
            //#if DEBUG
            //            return true;
            //#endif

            try
            {
                //var args = string.Join(" ",
                //    $"-region:\"{query.Region}\"",
                //    $"-app_version:\"{query.AppVersion}\"",
                //    $"-room_name:\"{query.RoomName}\"",
                //    Options.AppendParams);

                ProcessStartInfo si = new ProcessStartInfo(Options.AppPath/*, args*/);

                foreach (var item in Options.AppendParams)
                {
                    si.EnvironmentVariables.Add(item.Key, string.Empty);
                }

                si.EnvironmentVariables.Add("region", query.Region);
                si.EnvironmentVariables.Add("app_version", query.AppVersion);
                si.EnvironmentVariables.Add("room_name", query.RoomName);

                var proc = Process.Start(si);

                await Task.Delay(1500);

                return !proc.HasExited;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }

            return false;
        }
    }
}
