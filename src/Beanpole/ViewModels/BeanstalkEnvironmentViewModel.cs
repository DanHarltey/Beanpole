namespace Beanpole.ViewModels
{
    using System;

    public class BeanstalkEnvironmentViewModel
    {
        public BeanstalkEnvironmentViewModel()
        {
        }

        public string EnvironmentId { get; set; }

        public string EnvironmentName { get; set; }

        public string EndPoint { get; set; }

        public string Version { get; set; }

        public string Helth { get; set; }

        public bool AllowRestart { get; set; }

        public bool HasBeanpole { get; set; }
    }
}