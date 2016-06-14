namespace Beanpole.Controllers
{
    using Repositories;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class RecentlySeenController : Controller
    {
        private ClientRepository clientRepository = new ClientRepository();

        public async Task<ActionResult> Index()
        {
            return View(await clientRepository.GetAll());
        }
    }
}