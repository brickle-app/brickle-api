using Azure;
using Azure.Data.Tables;

namespace BricklePlatform.Infrastructure.Services.Base;

public class AzureTableStorageBase<T> where T : class, ITableEntity, new()
{
    private readonly TableClient tableClient;

    public AzureTableStorageBase(string storageConnectionString, string tableName)
    {
        TableServiceClient tableServiceClient = new TableServiceClient(storageConnectionString);
        tableClient = tableServiceClient.GetTableClient(tableName);
    }

    protected virtual async Task<T> SaveEntityAsync<TDto>(TDto dto, Func<TDto, T> entityFactory)
    {
        T entity = entityFactory(dto);
        return await InsertAndRetrieveAsync(entity);
    }

    public async Task InsertAsync(T entity)
    {
        await tableClient.AddEntityAsync(entity);
    }

    public async Task<T> InsertAndRetrieveAsync(T entity)
    {
        await InsertAsync(entity);

        await Task.Delay(150);

        return await GetAsync(entity.PartitionKey, entity.RowKey);
    }

    public async Task<T> UpdateWithRetryAsync(T entity)
    {
        int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await UpdateAsync(entity);
                return await GetAsync(entity.PartitionKey, entity.RowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 412)
            {
                if (attempt == maxRetries)
                {
                    throw new InvalidOperationException($"Fallo después de {maxRetries} reintentos.", ex);
                }

                T currentEntity = await GetAsync(entity.PartitionKey, entity.RowKey);
                entity.ETag = currentEntity.ETag;

                await Task.Delay(100 * attempt);
            }
        }

        throw new InvalidOperationException($"Fallo después de {maxRetries} reintentos.");
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity.ETag == default || string.IsNullOrEmpty(entity.ETag.ToString()))
        {
            throw new InvalidOperationException("ETag no puede estar vacío para operaciones de actualización.");
        }

        await tableClient.UpdateEntityAsync(entity, entity.ETag);
    }

    public async Task<T> GetAsync(string partitionKey, string rowKey)
    {
        Response<T> response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
        return response.Value;
    }

    public async Task<IEnumerable<T>> QueryAllAsync(string partitionKey)
    {
        AsyncPageable<T> queryResults = tableClient.QueryAsync<T>(filter: $"PartitionKey eq '{partitionKey}'");
        List<T> results = new List<T>();

        await foreach (T entity in queryResults)
        {
            results.Add(entity);
        }

        return results;
    }
}