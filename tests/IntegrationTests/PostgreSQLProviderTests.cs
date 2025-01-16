using Dapper;
using DbArchiver.Provider.PostgreSQL;
using DbArchiver.Provider.PostgreSQL.Config;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace DbArchiver.IntegrationTests;

public class PostgreSQLProviderTests
{
    private readonly string _connectionString = "Host=localhost;Port=5432;Database=TestDbArchiver;Username=postgres;Password=postgres";
    
    public PostgreSQLProviderTests()
    {
        EnsureDatabase();
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertData()
    {
        // Arrange
        var provider = new PostgreSQLProvider(new NullLogger<PostgreSQLProvider>());
        var settings = new TargetSettings
        {
            ConnectionString = _connectionString,
            Schema = "public",
            Table = "test_table",
            IdColumn = "id"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "id", 1 }, { "name", "TestName1" } },
            new Dictionary<string, object> { { "id", 2 }, { "name", "TestName2" } }
        };

        // Act
        await provider.InsertAsync(settings, testData);

        // Assert
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync("SELECT * FROM public.test_table");
            Assert.Equal(2, result.Count());
        }
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteData()
    {
        // Arrange
        var provider = new PostgreSQLProvider(new NullLogger<PostgreSQLProvider>());
        var settings = new SourceSettings
        {
            ConnectionString = _connectionString,
            Schema = "public",
            Table = "test_table",
            IdColumn = "id"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "id", 1 } }
        };

        // Act
        await provider.DeleteAsync(settings, testData);

        // Assert
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync("SELECT * FROM public.test_table");
            Assert.Single(result);
            Assert.Equal(2, result.First().id);
        }
    }

    [Fact]
    public async Task ExecuteScriptAsync_ShouldExecuteScript()
    {
        // Arrange
        var provider = new PostgreSQLProvider(new NullLogger<PostgreSQLProvider>());
        var settings = new TargetSettings
        {
            ConnectionString = _connectionString,
            Schema = "public",
            Table = "test_table",
            IdColumn = "id"
        };

        var script = "INSERT INTO public.test_table (id, name) VALUES (3, 'TestName3')";

        // Act
        await provider.ExecuteScriptAsync(settings, script);

        // Assert
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            var result = await connection.QueryAsync("SELECT * FROM public.test_table WHERE id = 3");
            Assert.Single(result);
            Assert.Equal("TestName3", result.First().name);
        }
    }

    [Fact]
    public async Task GetIteratorAsync_ShouldReturnDataInBatches()
    {
        // Arrange
        var provider = new PostgreSQLProvider(new NullLogger<PostgreSQLProvider>());
        var settings = new SourceSettings
        {
            ConnectionString = _connectionString,
            Schema = "public",
            Table = "test_table",
            IdColumn = "id"
        };

        var transferQuantity = 1;

        // Act
        var iterator = await provider.GetIteratorAsync(settings, transferQuantity);

        // Assert
        Assert.NotNull(iterator);

        var hasNext = await iterator.NextAsync();
        Assert.True(hasNext);
        Assert.Single(iterator.Data);

        hasNext = await iterator.NextAsync();
        Assert.True(hasNext);
        Assert.Single(iterator.Data);

        hasNext = await iterator.NextAsync();
        Assert.False(hasNext);
    }

    private void EnsureDatabase()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS public.test_table (
                        id INT PRIMARY KEY,
                        name VARCHAR(100) NOT NULL
                    );

                    DELETE FROM public.test_table;

                    INSERT INTO public.test_table (id, name)
                    VALUES
                        (1, 'InitialName1'),
                        (2, 'InitialName2');");
        }
    }
}