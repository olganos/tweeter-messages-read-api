using Core;
using Core.Entities;

using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _userCollection;
        public UserRepository(string connectionString, string databaseName, string userCollectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _userCollection = database.GetCollection<User>(userCollectionName);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _userCollection.Find(_ => true).ToListAsync(cancellationToken);
        }

        public async Task<List<User>> SearchByPartialUsernameAsync(string partialUsername, CancellationToken cancellationToken)
        {
            return await _userCollection
                .Find(x => x.UserName.Contains(partialUsername, StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);
        }
    }
}
