namespace Beanpole.Repositories
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;
    using Beanpole.Models;

    public class ClientRepository
    {
        private static bool indexed;

        private IMongoCollection<ClientRequest> clientRequestRepository;

        public ClientRepository()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString;

            var mongoClient = new MongoDB.Driver.MongoClient(connectionString);
            IMongoDatabase database = mongoClient.GetDatabase("ConfigAdmin");

            this.clientRequestRepository = database.GetCollection<ClientRequest>("ClientRequests");

            if (!ClientRepository.indexed)
            {
                var keys = Builders<ClientRequest>.IndexKeys.Ascending(x => x.LastSeen);
                var index = new CreateIndexOptions
                {
                    Background = true,
                    Name = "TTL expiry date",
                    ExpireAfter = TimeSpan.FromMinutes(2)
                };

                this.clientRequestRepository.Indexes.CreateOneAsync(
                    keys,
                    index);

                // does matter if done more than once but to save work, do this just once
                ClientRepository.indexed = true;
            }
        }

        public async Task<IEnumerable<ClientRequest>> GetAll()
        {
            var search = await this.clientRequestRepository.FindAsync(new BsonDocument());
            return await search.ToListAsync();
        }

        public async Task<bool> IsActive(string environmentName)
        {
            var dic = await this.GetActiveEnvironments();

            return dic.ContainsKey(environmentName);
        }

        public async Task<Dictionary<string, int>> GetActiveEnvironments()
        {
            var fields = Builders<ClientRequest>.Projection.Include(x => x.EnvironmentName);

            var environmentNames = await this.clientRequestRepository
                .Find(new BsonDocument())
                .Project(fields)
                .ToListAsync();

            // do not use ToDictionary, can have more than one instance in the environment 
            Dictionary<string, int> environments = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var environmentName in environmentNames)
            {
                environments[(string)environmentName[1]] = 0;
            }

            return environments;
        }

        public async Task Insert(ClientRequest clientRequest)
        {
            var filter = Builders<ClientRequest>.Filter.Eq(x => x.Ip, clientRequest.Ip);

            var update = Builders<ClientRequest>.Update
                .Set(x => x.LastSeen, clientRequest.LastSeen)
                .Set(x => x.EnvironmentName, clientRequest.EnvironmentName);

            await this.clientRequestRepository.UpdateOneAsync(filter, update, new UpdateOptions() { IsUpsert = true });
        }
    }
}