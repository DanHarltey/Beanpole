namespace Beanpole.Hubs
{
    using Microsoft.AspNet.SignalR;
    using System;
    using System.Threading.Tasks;

    public class RestartHub : Hub
    {
        public async Task ReportEnvironmentName(string environmentName)
        {
            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentException("Can not be null or empty", nameof(environmentName));
            }

            await this.Groups.Add(Context.ConnectionId, environmentName);
        }
    }
}