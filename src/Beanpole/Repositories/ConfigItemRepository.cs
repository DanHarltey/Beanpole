namespace Beanpole.Repositories
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using Beanpole.Models;

    public class ConfigItemRepository
    {
        private IMongoCollection<ConfigItem> itemRepository;

        public ConfigItemRepository()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;

            var mongoClient = new MongoDB.Driver.MongoClient(connectionString);
            IMongoDatabase database = mongoClient.GetDatabase("ConfigAdmin");

            this.itemRepository = database.GetCollection<ConfigItem>("ConfigItems");
        }

        public async Task<IEnumerable<ConfigItem>> GetAll()
        {
            var search = await this.itemRepository.FindAsync(new BsonDocument());
            return await search.ToListAsync();
        }

        public async Task<IEnumerable<ConfigItem>> GetAll(string environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            var filter = Builders<ConfigItem>.Filter.Where(x => x.Environment.ToUpper() == environment.ToUpper());

            var search = await this.itemRepository.FindAsync(filter);
            return await search.ToListAsync();
        }

        internal async Task<ConfigItem> Get(ObjectId id)
        {
            var filter = Builders<ConfigItem>.Filter.Eq(x => x._id, id);

            var search = await this.itemRepository.FindAsync(filter);
            var list = await search.ToListAsync();
            return list.FirstOrDefault();
        }

        internal async Task Add(ConfigItem configItem)
        {
            await this.itemRepository.InsertOneAsync(configItem);
        }

        internal async Task Add(IEnumerable<ConfigItem> configItems)
        {
            await this.itemRepository.InsertManyAsync(configItems);
        }

        internal async Task Update(ConfigItem configItem)
        {
            var filter = Builders<ConfigItem>.Filter.Eq(x => x._id, configItem._id);
            var update = Builders<ConfigItem>.Update.Set(x => x.Value, configItem.Value);

            await this.itemRepository.UpdateOneAsync(filter, update);
        }

        internal async Task Delete(ObjectId id)
        {
            await this.itemRepository.DeleteOneAsync(x => x._id == id);
        }

        internal async Task DeleteAll(string environment)
        {
            await this.itemRepository.DeleteManyAsync(x => x.Environment.ToUpper() == environment.ToUpper());
        }
    }
}
