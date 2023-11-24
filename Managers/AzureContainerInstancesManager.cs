using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.Batch;
using Azure.ResourceManager.ContainerInstance;
using Azure.ResourceManager.ContainerService;
using Azure.ResourceManager.ContainerService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using IsometricShooterWebApp.Data.Models.Configuration;
using IsometricShooterWebApp.Data.Models.GameApi;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;

namespace IsometricShooterWebApp.Managers
{
    public class AzureContainerInstancesManager
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<AzureContainerInstancesManager> logger;

        public AzureContainerInstancesManager(IConfiguration configuration, ILogger<AzureContainerInstancesManager> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        private void ReloadConfiguration()
        {
            var section = configuration.GetSection("referee:Azure");

            section.GetReloadToken().RegisterChangeCallback(obj =>
            {
                ReloadConfiguration();
            }, null);

            azureRoomOptions = section.Get<AzureRoomConfigurationOptions>();

#if !DEBUG
            InitializeAzure();
#endif
        }

        private void InitializeAzure()
        {
#if RELEASE
            var creds = new DefaultAzureCredential();
#else
            var creds = new AzureCliCredential();
#endif

            var resourceClient = new ArmClient(creds, azureRoomOptions.SubscriptionId);

            var subs = resourceClient.GetDefaultSubscription();

            azureResourceGroup = subs.GetResourceGroup(azureRoomOptions.ResourceGroupName/*"IsometricShooter-rooms"*/).Value;

            if (azureRoomOptions.App.EnvironmentName == default)
            {
                var envs = azureResourceGroup.GetManagedEnvironments();

                azureManagedEnv = envs.FirstOrDefault();

                if (azureManagedEnv == default)
                {
                    throw new Exception($"Not found any Container Apps Environment in resource group {azureResourceGroup.Data.Name}");
                }
            }
            else
            {
                azureManagedEnv = azureResourceGroup.GetManagedEnvironment(azureRoomOptions.App.EnvironmentName);

                if (azureManagedEnv == default)
                {
                    throw new Exception($"Not found '{azureRoomOptions.App.EnvironmentName}' Container Apps Environment in resource group {azureResourceGroup.Data.Name}");
                }
            }
        }


        private AzureRoomConfigurationOptions azureRoomOptions;

        private ResourceGroupResource azureResourceGroup;

        private ManagedEnvironmentResource azureManagedEnv;


        public void Initialize()
        {
            ReloadConfiguration();
        }

        public async Task Test()
        {
            //var roomId = Guid.NewGuid();

            //if (await CreateContainerAsync(new Data.Models.GameApi.GetRefereeRequestModel()
            //{
            //    RoomName = roomId.ToString(),
            //    Region = "eu",
            //    AppVersion = "1.0"
            //}))
            //    await DestroyContainerAsync(roomId.ToString());
        }

        public async Task<bool> CreateContainerAsync(GetRefereeRequestModel roomInfo)
        {
            var name = CorrectRoomContainerName(roomInfo.RoomName);

            logger.LogInformation($"[{nameof(CreateContainerAsync)}] Try create room with '{new { roomInfo.RoomName, ContainerName = name }}'");

            try
            {
                var apps = azureResourceGroup.GetContainerApps();

                var response = (await apps.CreateOrUpdateAsync(Azure.WaitUntil.Completed, name, new ContainerAppData(azureResourceGroup.Data.Location)
                {
                    Template = CreateAppTemplate(roomInfo),
                    ManagedEnvironmentId = azureManagedEnv.Id,
                    Configuration = GetAppConfiguration()
                })).GetRawResponse();

                if (response.IsError)
                    throw new Exception(response.Content?.ToString());

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"[{nameof(CreateContainerAsync)}] Room {roomInfo.RoomName} return error {ex}");
#if DEBUG
                throw;
#endif
            }

            return false;
        }


        public async Task DestroyContainerAsync(string roomName)
        {
            var name = CorrectRoomContainerName(roomName);

            logger.LogInformation($"[{nameof(DestroyContainerAsync)}] Try destroy room with '{new { roomName, ContainerName = name }}'");

            var apps = azureResourceGroup.GetContainerApps();

            try
            {
                var container = apps.Get(name).Value;

                if (container == default)
                    return;

                var response = (await container.DeleteAsync(Azure.WaitUntil.Started)).GetRawResponse();

                if (response.IsError)
                    throw new Exception();
            }
            catch (Exception ex)
            {
                logger.LogError($"[{nameof(DestroyContainerAsync)}] Room {roomName} return error {ex}");
#if DEBUG
                throw;
#endif
            }
        }

        private ContainerAppTemplate CreateAppTemplate(GetRefereeRequestModel roomInfo)
        {
            ContainerAppTemplate result = new ContainerAppTemplate();

            result.Containers.Add(CreateContainerApp(roomInfo));

            result.Scale = new ContainerAppScale() { MaxReplicas = 1 };

            return result;
        }

        private ContainerAppConfiguration GetAppConfiguration()
        {
            var registryOptions = azureRoomOptions.Container.Registry;

            var conf = new ContainerAppConfiguration();

            conf.Secrets.Add(new AppSecret() { Name = registryOptions.PasswordSecretRef, Value = registryOptions.Password });

            conf.Registries.Add(new RegistryCredentials() { Server = registryOptions.Server, Username = registryOptions.UserName, PasswordSecretRef = registryOptions.PasswordSecretRef });
            //conf.Ingress = new IngressProvider()
            //{
            //    AllowInsecure = true,
            //    Transport = IngressTransportMethod.Auto,
            //    External = true
            //};

            return conf;
        }

        private ContainerAppContainer CreateContainerApp(GetRefereeRequestModel roomInfo)
        {
            var containerOptions = azureRoomOptions.Container;

            var roomId = Guid.Parse(roomInfo.RoomName);

            var containerConfiguration = new ContainerAppContainer();

            containerConfiguration.Env.Add(new EnvironmentVar() { Name = "referee", Value = " " });
            containerConfiguration.Env.Add(new EnvironmentVar() { Name = "region", Value = roomInfo.Region });
            containerConfiguration.Env.Add(new EnvironmentVar() { Name = "app_version", Value = roomInfo.AppVersion });
            containerConfiguration.Env.Add(new EnvironmentVar() { Name = "room_name", Value = roomId.ToString() });

            containerConfiguration.Resources = new ContainerResources() { Cpu = containerOptions.CPU, Memory = containerOptions.RAM };

            containerConfiguration.Image = containerOptions.Image;

            containerConfiguration.Name = $"room-{roomId}";

            return containerConfiguration;
        }

        private string CorrectRoomContainerName(string roomName)
        {
            var name = roomName.Replace("-", "");

            name = "R" + name[1..];

            return name.ToLower();
        }
    }
}
