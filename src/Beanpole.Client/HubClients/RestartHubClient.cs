namespace Beanpole.Client.HubClients
{
    using log4net;
    using Microsoft.AspNet.SignalR.Client;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class RestartHubClient : HubClient
    {
        private static ILog Log = LogManager.GetLogger(typeof(RestartHubClient));

        private readonly IHubProxy restartHub;
        private readonly IDisposable eventSubscription;

        public RestartHubClient(MachineConfig machineConfig)
            : base(machineConfig)
        {
            this.restartHub = this.HubConnection.CreateHubProxy("RestartHub");
            this.eventSubscription = this.restartHub.On("RestartIIS", RestartHubClient.RestartIIS);
        }

        protected override async Task OnConnection()
        {
            await this.restartHub.Invoke("ReportEnvironmentName", this.MachineConfig.EnvironmentName);
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

        private static void RestartIIS()
        {
            RestartHubClient.Log.Debug("Reseting IIS");

            try
            {
                Process iisReset = new Process();
                iisReset.StartInfo.FileName = "IISReset";
                iisReset.StartInfo.CreateNoWindow = false;
                iisReset.StartInfo.UseShellExecute = true;
                iisReset.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                iisReset.Start();

                iisReset.WaitForExit();
                if (0 != iisReset.ExitCode)
                {
                    RestartHubClient.Log.Error("IISReset returned a none 0 exitcode : " + iisReset.ExitCode);

                    // I should not throw this but cant be both to create an exception class
                    throw new ApplicationException();
                }

                RestartHubClient.Log.Info("Successfully restarted IIS");
            }
            catch (Exception ex)
            {
                RestartHubClient.Log.Error(ex);
                throw;
            }
        }
    }
}
