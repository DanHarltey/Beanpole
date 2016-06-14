namespace Beanpole.Controllers
{
    using Microsoft.AspNet.SignalR;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Beanpole.Hubs;

    public class RestartController : Controller
    {
        // GET: Restart
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restart(string environmentName)
        {
            IHubContext restartHub = GlobalHost.ConnectionManager.GetHubContext<RestartHub>();
            await restartHub.Clients.Group(environmentName).RestartIIS();

            return this.RedirectToAction("Index");
        }
    }
}