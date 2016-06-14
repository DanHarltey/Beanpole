namespace Beanpole.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public struct MachineConfig
    {
        public MachineConfig(string environmentName)
        {
            this.EnvironmentName = environmentName;
        }

        public string EnvironmentName
        {
            get;
            private set;
        }
    }
}