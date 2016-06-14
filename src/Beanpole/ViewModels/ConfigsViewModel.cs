namespace Beanpole.ViewModels
{
    using Models;
    using System.Collections.Generic;

    public class ConfigsViewModel
    {
        public string EnvironmentName
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public IEnumerable<ConfigItem> ConfigItems
        {
            get;
            set;
        }
    }
}