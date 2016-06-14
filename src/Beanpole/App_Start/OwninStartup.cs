using Microsoft.Owin;

[assembly: OwinStartup(typeof(Beanpole.App_Start.OwninStartup))]

namespace Beanpole.App_Start
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Owin;

    public class OwninStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            app.MapSignalR();
        }
    }
}
