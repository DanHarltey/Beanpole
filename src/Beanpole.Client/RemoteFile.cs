namespace Beanpole.Client
{
    using log4net;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml;

    public class RemoteFile
    {
        private static ILog Log = LogManager.GetLogger(typeof(RemoteFile));

        private readonly string webAddress;
        private byte[] file;

        public RemoteFile(MachineConfig machineConfig)
        {
            this.webAddress =
                ConfigurationManager.AppSettings["XmlEndPointAddress"]
                + machineConfig.EnvironmentName;
        }

        public bool Update()
        {
            bool hasUpdated = false;
            byte[] newFile = this.GetFile();

            if (this.file == null
                || !this.file.SequenceEqual(newFile))
            {
                this.file = newFile;
                hasUpdated = true;
            }

            return hasUpdated;
        }

        public string Content
        {
            get
            {
                RemoteFile.Log.Debug("Reading file from memory");

                if (this.file == null)
                {
                    this.file = this.GetFile();
                }

                string content = Encoding.UTF8.GetString(this.file);

                return content;
            }
        }

        private byte[] GetFile()
        {
            RemoteFile.Log.Debug("Reading remote file");

            using (WebClient client = new WebClient())
            {
                return client.DownloadData(webAddress);
            }
        }
    }
}
