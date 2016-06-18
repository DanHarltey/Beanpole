namespace Beanpole.Controllers
{
    using Hubs;
    using Microsoft.AspNet.SignalR;
    using MongoDB.Bson;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml;
    using Beanpole.Models;
    using Beanpole.Repositories;
    using ViewModels;

    public class ConfigController : Controller
    {
        private ConfigItemRepository configItemRepository = new ConfigItemRepository();

        private ClientRepository clientRepository = new ClientRepository();

        // GET: ConfigItems
        public async Task<ActionResult> Index(string environment)
        {
            if (environment == null)
            {
                var configItems = this.configItemRepository.GetAll();
                var activeEnvironments = await this.clientRepository.GetActiveEnvironments();

                var viewModels = (await configItems).Select(x =>
                {
                    var viewModel = new ConfigItemViewModel(x);
                    viewModel.IsActive = activeEnvironments.ContainsKey(x.Environment);
                    return viewModel;
                });

                return View(viewModels);
            }
            else
            {
                Task<IEnumerable<ConfigItem>> configItems =
                    this.configItemRepository.GetAll(environment);

                Task<bool> isActive = this.clientRepository.IsActive(environment);

                var viewModle = new ConfigsViewModel
                {
                    EnvironmentName = environment,
                    ConfigItems = await configItems,
                    IsActive = await isActive,
                };
                return View("Environment", viewModle);
            }
        }

        // GET: ConfigItems/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ConfigItem configItem = await this.configItemRepository.Get(ObjectId.Parse(id));
            if (configItem == null)
            {
                return HttpNotFound();
            }
            return View(configItem);
        }

        // GET: ConfigItems/Create
        public ActionResult Create(string environment)
        {
            ConfigItem configItem = new ConfigItem
            {
                Environment = environment
            };

            return View();
        }

        // POST: ConfigItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Environment,Key,Value")] ConfigItem configItem)
        {
            if (ModelState.IsValid)
            {
                await this.configItemRepository.Add(configItem);

                await this.UpdateEnvironment(configItem.Environment);

                return RedirectToAction("Index", new { Environment = configItem.Environment });
            }

            return View(configItem);
        }

        // GET: ConfigItems/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var configItem = await this.configItemRepository.Get(ObjectId.Parse(id));

            if (configItem == null)
            {
                return HttpNotFound();
            }

            ConfigItemViewModel viewModel = new ConfigItemViewModel(configItem);
            return View(viewModel);
        }

        // POST: ConfigItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Environment,Key,Value")] ConfigItemViewModel configItem)
        {
            if (ModelState.IsValid)
            {
                await this.configItemRepository.Update(configItem);

                await this.UpdateEnvironment(configItem.Environment);

                return RedirectToAction("Index", new { Environment = configItem.Environment });
            }

            return View(configItem);
        }

        // GET: ConfigItems/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfigItem configItem = await this.configItemRepository.Get(ObjectId.Parse(id));
            if (configItem == null)
            {
                return HttpNotFound();
            }
            return View(configItem);
        }

        // POST: ConfigItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // get 
            ConfigItem configItem = await this.configItemRepository.Get(ObjectId.Parse(id));

            // then delete
            await this.configItemRepository.Delete(ObjectId.Parse(id));

            await this.UpdateEnvironment(configItem.Environment);

            return RedirectToAction("Index", new { Environment = configItem.Environment });
        }

        // GET: ConfigItems/Delete/5
        public ActionResult DeleteAll(string environment)
        {
            if (environment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Environment = environment;

            return View(ViewBag);
        }

        // POST: ConfigItems/Delete/5
        [HttpPost, ActionName("DeleteAll")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAllConfirmed(string environment)
        {
            if (environment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // delete all
            await this.configItemRepository.DeleteAll(environment);

            await this.UpdateEnvironment(environment);

            return this.RedirectToAction("Index", new { Environment = environment });
        }

        // GET: Config/Upload?Environment=environment
        public ActionResult Upload(string environment)
        {
            if (environment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Environment = environment;

            return View(ViewBag);
        }

        // POST: Config/Upload?Environment=environment
        [HttpPost, ActionName("Upload")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadConfirmed(string environment, HttpPostedFileBase file)
        {
            if (environment == null || file == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string fileUpload;
            using (TextReader tr = new StreamReader(file.InputStream))
            {
                fileUpload = tr.ReadToEnd();
            }

            List<ConfigItem> configItems = new List<ConfigItem>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(fileUpload);
            XmlElement appSettingsNode = xmlDoc["appSettings"];
            foreach (XmlNode item in appSettingsNode.ChildNodes)
            {
                if (string.Equals(item.Name, "add", StringComparison.OrdinalIgnoreCase))
                {
                    var key = item.Attributes["key"]?.Value;
                    var value = item.Attributes["value"]?.Value;

                    if (key != null && value != null)
                    {
                        configItems.Add(new ConfigItem
                        {
                            Environment = environment,
                            Key = key,
                            Value = value,
                        });
                    }
                }
            }

            await this.configItemRepository.DeleteAll(environment);
            await this.configItemRepository.Add(configItems);

            return RedirectToAction("Index", new { Environment = environment });
        }

        // GET: Config/Download?Environment=environment
        public async Task<ActionResult> Download(string environment)
        {
            if (string.IsNullOrEmpty(environment))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string configOverload = await this.CreateConfigOverload(environment);

            return this.Content(
                configOverload,
                "text/xml",
                Encoding.UTF8);
        }

        public async Task<ActionResult> Xml(string environment)
        {
            if (string.IsNullOrEmpty(environment))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // insert into tracking
            var request = new ClientRequest(Request.UserHostAddress, environment, DateTime.UtcNow);
            Task insertTask = this.clientRepository.Insert(request);

            string configOverload = await this.CreateConfigOverload(environment);
            await insertTask;

            return this.Content(
                configOverload,
                "text/xml",
                Encoding.UTF8);
        }

        private async Task UpdateEnvironment(string environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            string config = await this.CreateConfigOverload(environment);

            IHubContext restartHub = GlobalHost.ConnectionManager.GetHubContext<ConfigHub>();
            await restartHub.Clients.Group(environment).ConfigUpdate(config);
        }

        private async Task<string> CreateConfigOverload(string environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            StringBuilder xmlString = new StringBuilder();
            xmlString.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlString.AppendLine("<appSettings>");

            var configItems = await this.configItemRepository.GetAll(environment);

            foreach (var configItem in configItems)
            {
                xmlString.AppendFormat(
                    "    <add key=\"{0}\" value=\"{1}\" />{2}",
                    configItem.Key,
                    configItem.Value,
                    Environment.NewLine);
            }
            xmlString.AppendLine("</appSettings>");

            return xmlString.ToString();
        }
    }
}
