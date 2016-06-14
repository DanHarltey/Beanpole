namespace Beanpole.ViewModels
{
    using System.Collections.Generic;

    public class BeanstalkGroupViewModel
    {
        private readonly string groupName;
        private readonly List<BeanstalkEnvironmentViewModel> environments;

        public BeanstalkGroupViewModel(string groupName)
        {
            this.groupName = groupName;
            this.environments = new List<BeanstalkEnvironmentViewModel>();
        }

        public string GroupName
        {
            get
            {
                return this.groupName;
            }
        }

        public List<BeanstalkEnvironmentViewModel> Environments
        {
            get
            {
                return this.environments;
            }
        }
    }
}