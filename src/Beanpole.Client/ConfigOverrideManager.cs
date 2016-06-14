namespace Beanpole.Client
{
    using System;
    using System.IO;
    using System.Xml;

    public class ConfigOverrideManager
    {
        private static readonly string OverrideFile = "config_overrides\\web.overrides.config";
        private readonly string configPath;

        public ConfigOverrideManager(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentException(nameof(configPath));
            }

            this.configPath = configPath;
        }

        public string ConfigPath
        {
            get
            {
                return this.configPath;
            }
        }

        public void Replace(string fileContents)
        {
            // to try stop updating file at same time
            lock (this.configPath)
            {
                this.CreateUpdateOverride(fileContents);

                this.AddOverrideToConfig();
            }
        }

        private void CreateUpdateOverride(string fileContents)
        {
            string webPath = Path.GetDirectoryName(this.configPath);
            string overridePath = Path.Combine(webPath, ConfigOverrideManager.OverrideFile);

            // create dirs
            string overrideFolder = Path.GetDirectoryName(overridePath);
            Directory.CreateDirectory(overrideFolder);

            File.WriteAllText(overridePath, fileContents);
        }

        private void AddOverrideToConfig()
        {
            const string OverrideAttribute = "file";
            XmlDocument xml = new XmlDocument();
            xml.Load(this.configPath);

            XmlElement rootNode = xml["configuration"];
            XmlElement appSettingsNode = rootNode["appSettings"];

            XmlAttribute attribute = appSettingsNode.Attributes[OverrideAttribute];

            if (attribute == null)
            {
                appSettingsNode.SetAttribute(OverrideAttribute, ConfigOverrideManager.OverrideFile);
                xml.Save(this.configPath);
            }
            else if (string.Equals(attribute.Value, ConfigOverrideManager.OverrideFile, StringComparison.OrdinalIgnoreCase))
            {
                attribute.Value = ConfigOverrideManager.OverrideFile;
                xml.Save(this.configPath);
            }
        }
    }
}
