using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Strategies.Strategies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDbSettings;

public class StrategyService
{
    private readonly IMongoCollection<MainStrategy> _strategiesCollection;

    public StrategyService(IOptions<StrategyDatabaseSettings> strategiesDatabase)
    {
        var mongoClient = new MongoClient(
            strategiesDatabase.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            strategiesDatabase.Value.DatabaseName);

        _strategiesCollection = mongoDatabase.GetCollection<MainStrategy>(
            strategiesDatabase.Value.CollectionName);
    }
    public StrategyService(StrategyDatabaseSettings sds)
    {
        var mongoClient = new MongoClient(sds.ConnectionString);
        var mongoDataBase = mongoClient.GetDatabase(sds.DatabaseName);
        _strategiesCollection = mongoDataBase.GetCollection<MainStrategy>(sds.CollectionName);
    }
    public async Task<List<MainStrategy>> GetAsync() =>
        await _strategiesCollection.Find(_ => true).ToListAsync();
    public async Task<MainStrategy?> GetAsync(string id) =>
        await _strategiesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    public async Task CreateAsync(MainStrategy newContainer) =>
        await _strategiesCollection.InsertOneAsync(newContainer);
    public async Task UpdateAsync(string id, MainStrategy updatedBook) =>
        await _strategiesCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);
    public async Task RemoveAsync(string id) =>
        await _strategiesCollection.DeleteOneAsync(x => x.Id == id);
}
