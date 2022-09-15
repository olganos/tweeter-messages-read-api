using Core;
using Core.Entities;

using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IMongoCollection<Tweet> _tweetsCollection;
        private readonly IMongoCollection<Reply> _repliesCollection;
        public MessageRepository(string connectionString, string databaseName,
            string tweetCollectionName, string replyCollectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _tweetsCollection = database.GetCollection<Tweet>(tweetCollectionName);
            _repliesCollection = database.GetCollection<Reply>(replyCollectionName);
        }

        public async Task<List<Tweet>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _tweetsCollection
                .Aggregate()
                .Lookup<Tweet, Reply, Tweet>(
                    _repliesCollection,
                    x => x.Id,
                    x => x.TweetId,
                    p => p.Replies)
                .SortByDescending(x => x.Created)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Tweet>> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            FilterDefinition<Tweet> filter = Builders<Tweet>.Filter.Eq(p => p.UserName, username);

            return await _tweetsCollection
                .Aggregate()
                .Match(filter)
                .Lookup<Tweet, Reply, Tweet>(
                    _repliesCollection,
                    x => x.Id,
                    x => x.TweetId,
                    p => p.Replies)
                .SortByDescending(x => x.Created)
                .ToListAsync(cancellationToken);
        }
    }
}
