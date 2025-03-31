using DrugIndication.Domain.Entities;
using DrugIndication.Infrastructure.Data;
using MongoDB.Driver;

namespace DrugIndication.Infrastructure.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UserRepository(MongoDbContext context)
        {
            _collection = context.Database.GetCollection<User>("users");
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _collection.InsertOneAsync(user);
        }
    }
}
