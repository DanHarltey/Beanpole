namespace Beanpole.Client.HubClients
{
    using log4net;
    using Microsoft.AspNet.SignalR.Client;
    using System;
    using System.Configuration;
    using System.Net;
    using System.Threading.Tasks;

    public abstract class HubClient : Disposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(HubClient));

        private readonly MachineConfig machineConfig;
        private readonly IHubProxy hubProxy;
        private readonly HubConnection hubConnection;

        public HubClient(MachineConfig machineConfig)
        {
            this.machineConfig = machineConfig;

            // recomended by MS
            ServicePointManager.DefaultConnectionLimit = 10;

            string webAddress = ConfigurationManager.AppSettings["WebAddress"];

            this.hubConnection = new HubConnection(webAddress);
            this.hubConnection.StateChanged += HubClient.LogStateChanges;
            this.hubConnection.Closed += this.Reconnect;
        }

        protected MachineConfig MachineConfig
        {
            get
            {
                base.ThrowIfDisposed();
                return this.machineConfig;
            }
        }

        protected HubConnection HubConnection
        {
            get
            {
                return this.hubConnection;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.hubConnection != null)
                {
                    this.hubConnection.StateChanged -= HubClient.LogStateChanges;
                    this.hubConnection.Closed -= this.Reconnect;
                    this.hubConnection.Dispose();
                }
            }

            base.Dispose(isDisposing);
        }

        protected abstract Task OnConnection();

        protected void Connect()
        {
            this.hubConnection
                .Start()
                .ContinueWith(this.ConnectContinuation);
        }

        private async void Reconnect()
        {
            HubClient.Log.Info("Disconnected attemping reconnect in 10 seconds");
            await Task.Delay(TimeSpan.FromSeconds(10));
            this.Connect();
        }

        private async Task ConnectContinuation(Task startTask)
        {
            if (startTask.Status != TaskStatus.RanToCompletion)
            {
                HubClient.Log.Error($"Failed to connect task state {startTask.Status}", startTask.Exception);
            }
            else
            {
                HubClient.Log.Info("Connected");
                await this.OnConnection();
            }
        }

        private static void LogStateChanges(StateChange obj)
        {
            HubClient.Log.Debug($"State Changed from {obj.OldState} to {obj.NewState}");
        }
    }
}