namespace Beanpole.Client
{
    using Amazon;
    using Amazon.EC2;
    using Amazon.EC2.Model;
    using log4net;
    using System;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    public static class EC2Environment
    {
        private static ILog Log = LogManager.GetLogger(typeof(EC2Environment));

        public static string GetEnvironmentName()
        {
            EC2Environment.Log.Debug("Starting GetEnvironmentName");

            string instanceName = EC2Environment.GetInstanceId();
            EC2Environment.Log.Debug("InstanceName " + instanceName);

            var region = EC2Environment.GetRegion();
            EC2Environment.Log.Debug("region DisplayName: " + region.DisplayName);
            EC2Environment.Log.Debug("region SystemName: " + region.SystemName);

            var instance = EC2Environment.GetInstance(instanceName, region);
            EC2Environment.Log.Debug("InstanceId: " + instance.InstanceId);

            var enviromentName = EC2Environment.GetBeanstalkEnvironmentName(instance);

            return enviromentName;
        }

        private static string GetInstanceId()
        {
            using (WebClient wc = new WebClient())
            {
                string instanceId = wc.DownloadString("http://169.254.169.254/latest/meta-data/instance-id");
                return instanceId;
            }
        }

        private static RegionEndpoint GetRegion()
        {
            using (WebClient wc = new WebClient())
            {
                string jsonResponse = wc.DownloadString("http://169.254.169.254/latest/dynamic/instance-identity/document");

                Match match = Regex.Match(jsonResponse, @"""region"" : ""(.*)""", RegexOptions.IgnoreCase);
                string region = match.Groups[1].Value;
                EC2Environment.Log.Debug("Raw region: " + region);

                RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(region);
                return regionEndpoint;
            }
        }

        private static Instance GetInstance(string instanceName, RegionEndpoint region)
        {
            DescribeInstancesRequest req = new DescribeInstancesRequest()
            {
                InstanceIds = { instanceName }
            };

            DescribeInstancesResponse response = null;

            using (AmazonEC2Client ec2 = new AmazonEC2Client(region))
            {
                response = ec2.DescribeInstances(req);
            }

            Instance instance = response.Reservations
                .SelectMany(x => x.Instances)
                .Single(x => string.Equals(x.InstanceId, instanceName, StringComparison.OrdinalIgnoreCase));

            return instance;
        }

        private static string GetBeanstalkEnvironmentName(Instance instance)
        {
            return instance.Tags.Single(x => string.Equals(x.Key, "Name", StringComparison.OrdinalIgnoreCase))
                .Value;
        }
    }
}
