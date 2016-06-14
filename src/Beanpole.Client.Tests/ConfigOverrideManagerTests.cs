using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Beanpole.Client.Tests
{
    [TestClass]
    public class ConfigOverrideManagerTests
    {
        private const string ConfigFileNoOverride = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <configSections>
  </configSections>

  <JayZ>
    <add name=""99"" trip=""50"" execution=""2"" />
  </JayZ>

  <appSettings>
    <add key=""SomeKey"" value=""TRUE"" />
  </appSettings>

  <connectionStrings>
  </connectionStrings>

  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
    </assemblyBinding>
  </runtime>
</configuration>";

        private const string ConfigFileWithOverride = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <configSections>
  </configSections>
  <JayZ>
    <add name=""99"" trip=""50"" execution=""2"" />
  </JayZ>
  <appSettings file=""config_overrides\web.overrides.config"">
    <add key=""SomeKey"" value=""TRUE"" />
  </appSettings>
  <connectionStrings>
  </connectionStrings>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
    </assemblyBinding>
  </runtime>
</configuration>";

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorThrownsOnNullArg()
        {
            new ConfigOverrideManager(null);
        }

        [TestMethod]
        public void Update()
        {
            CreateWebConfig();
            string filePath = Path.GetFullPath("web.config");
            ConfigOverrideManager configOverrideManager = new ConfigOverrideManager(filePath);
            configOverrideManager.Replace("Something");

            string actual = File.ReadAllText(filePath);
            Assert.AreEqual(ConfigFileWithOverride, actual);
        }

        private static void CreateWebConfig()
        {
            File.WriteAllText("web.config", ConfigFileNoOverride);
        }
    }
}
