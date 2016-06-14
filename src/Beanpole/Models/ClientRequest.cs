namespace Beanpole.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Net;
    [BsonIgnoreExtraElements]
    public class ClientRequest
    {
        private static readonly string SplunkBaseAddress = ConfigurationManager.AppSettings["SplunkBaseAddress"];

        public ClientRequest(string ip, string environmentName, DateTime lastSeen)
        {
            if (string.IsNullOrEmpty(ip))
            {
                new ArgumentException("Can not be null or emtpy", nameof(ip));
            }

            if (string.IsNullOrEmpty(environmentName))
            {
                new ArgumentException("Can not be null or emtpy", nameof(environmentName));
            }

            this.Ip = ip;
            this.EnvironmentName = environmentName;
            this.LastSeen = lastSeen;
        }

        [BsonId]
        [BsonElement("Ip")]
        public string Ip { get; set; }

        [BsonElement("E")]
        [Display(Name = "Environment")]
        public string EnvironmentName { get; set; }

        [Display(Name = "LastSeen (UTC)", Description = "Description LastSeen (UCT)")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("LS")]
        public DateTime LastSeen { get; set; }

        [Display(Name = "Machine Name")]
        public string MachineName
        {
            get
            {
                const string Unknown = "Unknown";

                string machineName = Unknown;

                string[] charHex = new string[5];
                charHex[0] = "IP-";

                string[] decs = this.Ip.Split('.');

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
        }


        [Display(Name = "Splunk Link")]
        public string SplunkLink
        {
            get
            {
                string splunkLink = null;

                if (ClientRequest.SplunkBaseAddress != null)
                {
                    splunkLink = SplunkBaseAddress + "host=\"" + this.MachineName + "\"";
                }

                return splunkLink;
            }
        }
    }
}