namespace Beanpole.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class ConfigItem
    {
        public ConfigItem()
        {
        }

        public ObjectId _id
        {
            get;
            set;
        }

        [Required]
        public string Environment
        {
            get;
            set;
        }

        [Required]
        public string Key
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Key + " " + this.Value;
        }
    }
}