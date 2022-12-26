using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Strategies;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MongoDbSettings;

public class ContainerService
{
    private readonly IMongoCollection<Container> _containersCollection;

    public ContainerService(IOptions<DatabaseSettings> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _containersCollection = mongoDatabase.GetCollection<Container>(
            options.Value.CollectionName);
    }
    public ContainerService(DatabaseSettings settings)
    {
        var mongoClient = new MongoClient(settings.ConnectionString);
        var mongoDataBase = mongoClient.GetDatabase(settings.DatabaseName);
        _containersCollection = mongoDataBase.GetCollection<Container>(settings.CollectionName);
    }
    public async Task<List<Container>> GetAsync() =>
        await _containersCollection.Find(_ => true).ToListAsync();
    public List<Container> Get() =>
        _containersCollection.Find(_ => true).ToList();
    public async Task<Container?> GetAsync(string id) =>
        await _containersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    public async Task CreateAsync(Container newContainer) =>
        await _containersCollection.InsertOneAsync(newContainer);
    public async Task UpdateAsync(string id, Container updatedContainer) =>
        await _containersCollection.ReplaceOneAsync(x => x.Id == id, updatedContainer);
    public async Task RemoveAsync(string id) =>
        await _containersCollection.DeleteOneAsync(x => x.Id == id);
}
