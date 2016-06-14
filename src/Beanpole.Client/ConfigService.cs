namespace Beanpole.Client
{
    using HubClients;
    using System.Configuration;
    using System.ServiceProcess;

    public partial class ConfigService : ServiceBase
    {
        private ConfigUpdater configUpdater;
        private ConfigHubClient configHubClient;
        private RestartHubClient restartService;

        public ConfigService()
        {
            this.InitializeComponent();
        }

        public void Start()
        {
#if DEBUG
            string environmentName = "awesome!";
#else
            string environmentName = EC2Environment.GetEnvironmentName();
#endif
            MachineConfig machineConfig = new MachineConfig(environmentName);
            this.restartService = new RestartHubClient(machineConfig);

            string configFile = ConfigurationManager.AppSettings["File"];
            ConfigOverrideManager configOverrideManager = new ConfigOverrideManager(configFile);

            this.configUpdater = new ConfigUpdater(machineConfig, configOverrideManager);
            this.configHubClient = new ConfigHubClient(machineConfig, this.configUpdater);
        }

        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        protected override void OnStop()
        {
            if (this.configUpdater != null)
            {
                this.configUpdater.Dispose();
                this.configUpdater = null;
            }

            if (this.restartService != null)
            {
                this.restartService.Dispose();
                this.restartService = null;
            }
        }
    }
}
