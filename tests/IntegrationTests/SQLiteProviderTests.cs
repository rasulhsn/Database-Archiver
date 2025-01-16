using Dapper;
using DbArchiver.Provider.SQLite;
using DbArchiver.Provider.SQLite.Config;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace DbArchiver.IntegrationTests;

public class SQLiteProviderTests
{
    private const string ConnectionString = "Data Source=:memory:";

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        connection.Execute(@"
                CREATE TABLE test_table (
                    id INTEGER PRIMARY KEY,
                    name TEXT NOT NULL
                );

                INSERT INTO test_table (id, name) VALUES (1, 'InitialName1');
                INSERT INTO test_table (id, name) VALUES (2, 'InitialName2');
            ");
    }
    
    [Fact]
    public async Task InsertAsync_ShouldInsertData()
    {
        // Arrange
        InitializeDatabase();
        var provider = new SQLiteProvider(new NullLogger<SQLiteProvider>());
        var settings = new TargetSettings
        {
            ConnectionString = ConnectionString,
            Table = "test_table"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "id", 3 }, { "name", "TestName3" } },
            new Dictionary<string, object> { { "id", 4 }, { "name", "TestName4" } }
        };

        // Act
        await provider.InsertAsync(settings, testData);

        // Assert
        using var connection = new SqliteConnection(ConnectionString);
        var result = connection.Query("SELECT * FROM test_table WHERE id IN (3, 4)");
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteData()
    {
        // Arrange
        InitializeDatabase();
        var provider = new SQLiteProvider(new NullLogger<SQLiteProvider>());
        var settings = new SourceSettings
        {
            ConnectionString = ConnectionString,
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
        using var connection = new SqliteConnection(ConnectionString);
        var result = connection.Query("SELECT * FROM test_table WHERE id = 1");
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteScriptAsync_ShouldExecuteScript()
    {
        // Arrange
        InitializeDatabase();
        var provider = new SQLiteProvider(new NullLogger<SQLiteProvider>());
        var settings = new TargetSettings
        {
            ConnectionString = ConnectionString,
            Table = "test_table"
        };

        var script = "INSERT INTO test_table (id, name) VALUES (5, 'ScriptName5')";

        // Act
        await provider.ExecuteScriptAsync(settings, script);

        // Assert
        using var connection = new SqliteConnection(ConnectionString);
        var result = connection.Query("SELECT * FROM test_table WHERE id = 5");
        Assert.Single(result);
    }
    
    [Fact]
    public async Task GetIteratorAsync_ShouldReturnDataInBatches()
    {
        // Arrange
        InitializeDatabase();
        var provider = new SQLiteProvider(new NullLogger<SQLiteProvider>());
        var settings = new SourceSettings
        {
            ConnectionString = ConnectionString,
            Table = "test_table",
            IdColumn = "id"
        };

        var batchSize = 1;

        // Act
        var iterator = await provider.GetIteratorAsync(settings, batchSize);

        // Assert
        Assert.NotNull(iterator);

        var hasNext = await iterator.NextAsync();
        Assert.True(hasNext);
        Assert.Single(iterator.Data);
        Assert.Equal(1, ((dynamic)iterator.Data.First()).id);

        hasNext = await iterator.NextAsync();
        Assert.True(hasNext);
        Assert.Single(iterator.Data);
        Assert.Equal(2, ((dynamic)iterator.Data.First()).id);

        hasNext = await iterator.NextAsync();
        Assert.False(hasNext);
    }
    
}