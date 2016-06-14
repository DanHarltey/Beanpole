namespace Beanpole.ViewModels
{
    using Models;
    using MongoDB.Bson;

    public class ConfigItemViewModel : ConfigItem
    {
        public ConfigItemViewModel()
        {
        }

        public ConfigItemViewModel(ConfigItem item)
        {
            this._id = item._id;
            this.Environment = item.Environment;
            this.Key = item.Key;
            this.Value = item.Value;
        }

        public string Id
        {
            get
            {
                return base._id.ToString();
            }
            set
            {
                this._id = ObjectId.Parse(value);
            }
        }

        public bool IsActive
        {
            get;
            set;
        }
    }
}