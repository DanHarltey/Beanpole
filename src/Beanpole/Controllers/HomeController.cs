namespace Beanpole.Controllers
{
    using Amazon;
    using Amazon.EC2;
    using Amazon.EC2.Model;
    using Amazon.ElasticBeanstalk;
    using Amazon.ElasticBeanstalk.Model;
    using Repositories;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ViewModels;

    public class HomeController : Controller
    {
        private static readonly string SplunkBaseAddress = ConfigurationManager.AppSettings["SplunkBaseAddress"];
        private static readonly string[] BeanstalkGroups;
        private static readonly string[] RestartBeanstalkGroups;

        private ClientRepository clientRepository = new ClientRepository();

        static HomeController()
        {
            HomeController.BeanstalkGroups = LoadConfig("BeanstalkGroups");
            HomeController.RestartBeanstalkGroups = LoadConfig("RestartBeanstalkGroups");
        }

        // GET: Beanstalk
        public async Task<ActionResult> Index()
        {
            DescribeEnvironmentsResponse environmentsDescription;
            var activeEnvironmentsTask = this.clientRepository.GetActiveEnvironments();

            using (AmazonElasticBeanstalkClient client = new AmazonElasticBeanstalkClient(RegionEndpoint.EUWest1))
            {
                DescribeEnvironmentsRequest request = new DescribeEnvironmentsRequest();
                environmentsDescription = await client.DescribeEnvironmentsAsync(request);
            }

            var environments = environmentsDescription.Environments;
            environments.RemoveAll(x => x.Status == EnvironmentStatus.Terminated || x.Status == EnvironmentStatus.Terminating);

            var activeEnvironments = await activeEnvironmentsTask;

            BeanstalkGroupViewModel[] viewModels =
                new BeanstalkGroupViewModel[HomeController.BeanstalkGroups.Length + 1];

            for (int i = 0; i < HomeController.BeanstalkGroups.Length; i++)
            {
                viewModels[i] = new BeanstalkGroupViewModel(HomeController.BeanstalkGroups[i]);

                for (int j = environments.Count - 1; j >= 0; j--)
                {
                    if (environments[j].EnvironmentName.StartsWith(HomeController.BeanstalkGroups[i], StringComparison.OrdinalIgnoreCase))
                    {
                        // add something
                        var obj = Convert(environments[j]);
                        obj.HasBeanpole = activeEnvironments.ContainsKey(obj.EnvironmentName);

                        viewModels[i].Environments.Add(obj);
                        environments.RemoveAt(j);
                    }
                }
            }

            var unknownGroup = new BeanstalkGroupViewModel("Unknown");
            for (int i = 0; i < environments.Count; i++)
            {
                unknownGroup.Environments.Add(Convert(environments[i]));
            }
            viewModels[viewModels.Length - 1] = unknownGroup;

            for (int i = 0; i < viewModels.Length; i++)
            {
                viewModels[i].Environments.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.EnvironmentName, y.EnvironmentName));
            }

            return View(viewModels);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestartEnvironment(string environmentName)
        {
            if (string.IsNullOrEmpty(environmentName))
            {
                throw new ArgumentNullException(nameof(environmentName));
            }

            if (!HomeController.RestartBeanstalkGroups.Any(x => environmentName.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnauthorizedAccessException();
            }

            RestartAppServerResponse restartResponse;

            try
            {
                using (AmazonElasticBeanstalkClient client = new AmazonElasticBeanstalkClient(RegionEndpoint.EUWest1))
                {
                    RestartAppServerRequest request = new RestartAppServerRequest
                    {
                        EnvironmentName = environmentName,
                    };

                    restartResponse = await client.RestartAppServerAsync(request);
                }
            }
            catch (AmazonElasticBeanstalkException ex)
            {
                return new HttpStatusCodeResult(ex.StatusCode, ex.Message);
            }

            return this.RedirectToAction("Index", "Restart");
        }

        public async Task<ActionResult> RedirectToSplunk(string environmentName)
        {
            DescribeInstancesResponse response;

            using (AmazonEC2Client client = new AmazonEC2Client(RegionEndpoint.EUWest1))
            {
                var request = new DescribeInstancesRequest
                {
                    Filters =
                    {
                        // for select environment.EnvironmentName
                        new Amazon.EC2.Model.Filter { Name = "tag:Name", Values = { environmentName } },
                    },
                };

                response = await client.DescribeInstancesAsync(request);
            }

            List<string> hosts = new List<string>(response.Reservations.Count);
            foreach (var instance in response.Reservations.SelectMany(x => x.Instances))
            {
                if (null != instance.PrivateIpAddress)
                {
                    hosts.Add("host=\"" + MachineName(instance.PrivateIpAddress) + "\"");
                }
            }

            string searchQuery = string.Join(" OR ", hosts);

            string url = HomeController.SplunkBaseAddress + searchQuery;

            return this.Redirect(url);
        }

        private static string MachineName(string ip)
        {
            const string Unknown = "Unknown";

            string machineName = Unknown;

            string[] charHex = new string[5];
            charHex[0] = "IP-";

            string[] decs = ip.Split('.');

            if (decs.Length == 4)
            {
                try
                {
                    for (int i = 0; i < 4; i++)
                    {
                        byte value = byte.Parse(decs[i]);
                        charHex[i + 1] = value.ToString("X2");
                    }

                    machineName = string.Concat(charHex);
                }
                catch (Exception)
                {
                    // shhh nothing to see here
                }
            }

            return machineName;
        }

        private static BeanstalkEnvironmentViewModel Convert(EnvironmentDescription environmentDescription)
        {
            var viewModel = new BeanstalkEnvironmentViewModel
            {
                EnvironmentId = environmentDescription.EnvironmentId,
                EnvironmentName = environmentDescription.EnvironmentName,
                EndPoint = environmentDescription.CNAME,
                Version = environmentDescription.VersionLabel,
                Helth = environmentDescription.Health.Value,
                AllowRestart = HomeController.RestartBeanstalkGroups.Any(x => environmentDescription.EnvironmentName.StartsWith(x, StringComparison.OrdinalIgnoreCase))
            };

            return viewModel;
        }

        private static string[] LoadConfig(string configKey)
        {
            string[] groups;

            string configValue = ConfigurationManager.AppSettings[configKey];
            if (configValue != null)
            {
                groups = configValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                groups = new string[0];
            }

            return groups;
        }
    }
}