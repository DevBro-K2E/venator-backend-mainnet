namespace IsometricShooterWebApp.Data.Models.Configuration
{
    public class AzureRoomConfigurationOptions
    {
        public AppConfiguratioOptions App { get; set; }

        public ContainerConfigurationOptions Container { get; set; }

        public string SubscriptionId { get; set; }

        public string ResourceGroupName { get; set; }

        public class AppConfiguratioOptions
        {
            public string EnvironmentName { get; set; }
        }

        public class ContainerConfigurationOptions
        {
            public string Image { get; set; }

            public double CPU { get; set; }

            public string RAM { get; set; }

            public ContainerRegistryConfigurationOptions Registry { get; set; }
        }

        public class ContainerRegistryConfigurationOptions
        {
            public string Server { get; set; }

            public string UserName { get; set; }

            public string Password { get; set; }

            public string PasswordSecretRef { get; set; } = "reg-pswd-4ea2f43c-9209";
        }
    }
}
