namespace MongoDbSettings;

using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Strategies.BatmanStrategy;

public class ContainerService
{
    private readonly IMongoCollection<BatmanContainer> _containersCollection;

    public ContainerService(IOptions<DatabaseSettings> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _containersCollection = mongoDatabase.GetCollection<BatmanContainer>(
            options.Value.CollectionName);
    }
    public ContainerService(DatabaseSettings settings)
    {
        var mongoClient = new MongoClient(settings.ConnectionString);
        var mongoDataBase = mongoClient.GetDatabase(settings.DatabaseName);
        _containersCollection = mongoDataBase.GetCollection<BatmanContainer>(settings.CollectionName);
    }
    public async Task<List<BatmanContainer>> GetAsync() =>
        await _containersCollection.Find(_ => true).ToListAsync();
    public List<BatmanContainer> Get() =>
        _containersCollection.Find(_ => true).ToList();
    public async Task<BatmanContainer?> GetAsync(string id) =>
        await _containersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    public async Task CreateAsync(BatmanContainer newContainer) =>
        await _containersCollection.InsertOneAsync(newContainer);
    public async Task UpdateAsync(string id, BatmanContainer updatedContainer) =>
        await _containersCollection.ReplaceOneAsync(x => x.Id == id, updatedContainer);
    public async Task RemoveAsync(string id) =>
        await _containersCollection.DeleteOneAsync(x => x.Id == id);
}
