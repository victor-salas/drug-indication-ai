using DrugIndication.Domain.Entities;
using DrugIndication.Infrastructure.Data;
using MongoDB.Driver;

namespace DrugIndication.Infrastructure
{
    public class ProgramRepository
    {
        private readonly IMongoCollection<ProgramDto> _collection;

        public ProgramRepository(MongoDbContext context)
        {
            _collection = context.Database.GetCollection<ProgramDto>("programs");
        }

        public async Task CreateAsync(ProgramDto program)
        {
            await _collection.InsertOneAsync(program);
        }

        public async Task<ProgramDto?> GetByIdAsync(int id)
        {
            return await _collection.Find(p => p.ProgramID == id).FirstOrDefaultAsync();
        }

        public async Task<List<ProgramDto>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task UpdateAsync(ProgramDto updatedProgram)
        {
            await _collection.ReplaceOneAsync(p => p.ProgramID == updatedProgram.ProgramID, updatedProgram);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _collection.DeleteOneAsync(p => p.ProgramID == id);
            return result.DeletedCount > 0;
        }
        public async Task<List<ProgramDto>> SearchByNameAsync(string searchTerm)
        {
            var filter = Builders<ProgramDto>.Filter
                .Regex(p => p.ProgramName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));

            return await _collection.Find(filter).ToListAsync();
        }

    }
}
