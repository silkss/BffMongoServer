using ContainerStore.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContainerStore.WebApi.Services;

public class ContainersService
{
    private readonly IMongoCollection<Container> _containersCollection;

	public ContainersService(IOptions<ContainerStoreDatabaseSettings> containerStoreDatabase)
	{
		var mongoClient = new MongoClient(
			containerStoreDatabase.Value.ConnectionString);
		var mongoDatabase = mongoClient.GetDatabase(
			containerStoreDatabase.Value.DatabaseName);
		_containersCollection = mongoDatabase.GetCollection<Container>(
			containerStoreDatabase.Value.ContainersCollectionName);
	}
    public async Task<List<Container>> GetAsync() =>
        await _containersCollection.Find(_ => true).ToListAsync();
    public async Task<Container?> GetAsync(string id) =>
		await _containersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
	public async Task CreateAsync(Container newContainer) =>
		await _containersCollection.InsertOneAsync(newContainer);
	public async Task UpdateAsync(string id, Container updatedBook) =>
		await _containersCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);
	public async Task RemoveAsync(string id) =>
		await _containersCollection.DeleteOneAsync(x => x.Id == id);
}
