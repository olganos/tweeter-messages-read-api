using Core;
using Core.Entities;

using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IMongoCollection<Tweet> _tweetsCollection;
        private readonly IMongoCollection<Reply> _repliesCollection;
        private readonly IMongoCollection<Like> _likesCollection;
        public MessageRepository(string connectionString, string databaseName,
            string tweetCollectionName, string replyCollectionName, string likeCollectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _tweetsCollection = database.GetCollection<Tweet>(tweetCollectionName);
            _repliesCollection = database.GetCollection<Reply>(replyCollectionName);
            _likesCollection = database.GetCollection<Like>(likeCollectionName);
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
                .Lookup<Tweet, Like, Tweet>(
                    _likesCollection,
                    x => x.Id,
                    x => x.TweetId,
                    p => p.Likes)
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
                .Lookup<Tweet, Like, Tweet>(
                    _likesCollection,
                    x => x.Id,
                    x => x.TweetId,
                    p => p.Likes)
                .SortByDescending(x => x.Created)
                .ToListAsync(cancellationToken);
        }
    }
}
