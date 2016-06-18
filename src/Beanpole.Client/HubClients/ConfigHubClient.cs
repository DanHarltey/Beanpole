namespace Beanpole.Client.HubClients
{
    using log4net;
    using Microsoft.AspNet.SignalR.Client;
    using System;
    using System.Threading.Tasks;

    internal class ConfigHubClient : HubClient
    {
        private static ILog Log = LogManager.GetLogger(typeof(ConfigHubClient));

        private readonly ConfigUpdater configUpdater;
        private readonly IHubProxy configHub;
        private readonly IDisposable eventSubscription;

        public ConfigHubClient(MachineConfig machineConfig, ConfigUpdater configUpdater)
            : base(machineConfig)
        {
            if (configUpdater == null)
            {
                throw new ArgumentNullException(nameof(configUpdater));
            }

            this.configUpdater = configUpdater;

            this.configHub = this.HubConnection.CreateHubProxy("ConfigHub");

            Action<string> configUpdate = this.UpdateConfig;
            this.eventSubscription = this.configHub.On("ConfigUpdate", configUpdate);
            this.Connect();
        }

        protected override async Task OnConnection()
        {
            await this.configHub.Invoke("ReportEnvironmentName", this.MachineConfig.EnvironmentName);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.eventSubscription == null)
                {
                    this.eventSubscription.Dispose();
                }
            }

            base.Dispose(isDisposing);
        }

        private void UpdateConfig(string config)
        {
            ConfigHubClient.Log.Debug("UpdateConfig Received");

            try
            {
                this.configUpdater.UpdateToLastest();
                ConfigHubClient.Log.Info("Successfully Updated Config");
            }
            catch (Exception ex)
            {
                ConfigHubClient.Log.Error(ex);
                throw;
            }
        }
    }
}
