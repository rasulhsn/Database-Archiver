using Dapper;
using DbArchiver.Provider.MySQL;
using DbArchiver.Provider.MySQL.Config;
using Microsoft.Extensions.Logging;
using Moq;
using MySql.Data.MySqlClient;

namespace DbArchiver.IntegrationTests;

public class MySQLProviderTests
{
    private readonly Mock<ILogger<MySQLProvider>> _loggerMock;
    private readonly MySQLProvider _provider;
    private readonly string _connectionString = "Server=localhost;Database=TestDb;Uid=root;Pwd=yourpassword;";
    private readonly string _tableName = "TestTable";
    
    public MySQLProviderTests()
    {
        _loggerMock = new Mock<ILogger<MySQLProvider>>();
        _provider = new MySQLProvider(_loggerMock.Object);
    }

    public async Task InitializeAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var createTableScript = $@"
            CREATE TABLE IF NOT EXISTS `{_tableName}` (
                `Id` INT PRIMARY KEY AUTO_INCREMENT,
                `Name` VARCHAR(255),
                `Value` VARCHAR(255)
            );";
        await connection.ExecuteAsync(createTableScript);
    }
    
    public async Task DisposeAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var dropTableScript = $@"DROP TABLE IF EXISTS `{_tableName}`;";
        await connection.ExecuteAsync(dropTableScript);
    }
    
    [Fact]
    public async Task InsertAsync_ShouldInsertDataCorrectly()
    {
        // Arrange
        var targetSettings = new TargetSettings
        {
            ConnectionString = _connectionString,
            Schema = "TestDb",
            Table = _tableName,
            IdColumn = "Id"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Name", "Test1" }, { "Value", "Value1" } },
            new Dictionary<string, object> { { "Id", 2 }, { "Name", "Test2" }, { "Value", "Value2" } }
        };

        // Act
        await _provider.InsertAsync(targetSettings, testData);

        // Assert
        using var connection = new MySqlConnection(_connectionString);
        var insertedData = (await connection.QueryAsync($"SELECT * FROM `{_tableName}`")).ToList();

        Assert.Equal(2, insertedData.Count);
        Assert.Contains(insertedData, row => row.Name == "Test1" && row.Value == "Value1");
        Assert.Contains(insertedData, row => row.Name == "Test2" && row.Value == "Value2");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteDataCorrectly()
    {
        // Arrange
        var sourceSettings = new SourceSettings
        {
            ConnectionString = _connectionString,
            Schema = "TestDb",
            Table = _tableName,
            IdColumn = "Id"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Name", "Test1" }, { "Value", "Value1" } },
            new Dictionary<string, object> { { "Id", 2 }, { "Name", "Test2" }, { "Value", "Value2" } }
        };

        using var connection = new MySqlConnection(_connectionString);

        // Insert data for testing deletion
        await connection.ExecuteAsync($"INSERT INTO `{_tableName}` (`Id`, `Name`, `Value`) VALUES (@Id, @Name, @Value);", testData);

        // Assert initial data count
        var initialData = (await connection.QueryAsync($"SELECT * FROM `{_tableName}`")).ToList();
        Assert.Equal(2, initialData.Count);

        // Act
        await _provider.DeleteAsync(sourceSettings, testData);

        // Assert
        var remainingData = (await connection.QueryAsync($"SELECT * FROM `{_tableName}`")).ToList();
        Assert.Empty(remainingData);
    }
    
    [Fact]
    public async Task ExecuteScriptAsync_ShouldExecuteCorrectly()
    {
        // Arrange
        var targetSettings = new TargetSettings
        {
            ConnectionString = _connectionString,
            Schema = "TestDb",
            Table = _tableName
        };

        var insertScript = $@"
            INSERT INTO `{_tableName}` (`Name`, `Value`)
            VALUES ('ScriptTest', 'ScriptValue');";

        // Act
        await _provider.ExecuteScriptAsync(targetSettings, insertScript);

        // Assert
        using var connection = new MySqlConnection(_connectionString);
        var data = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM `{_tableName}` WHERE `Name` = 'ScriptTest'");
        Assert.NotNull(data);
        Assert.Equal("ScriptValue", data.Value);
    }

    [Fact]
    public async Task GetIteratorAsync_ShouldReturnBatchedData()
    {
        // Arrange
        var sourceSettings = new SourceSettings
        {
            ConnectionString = _connectionString,
            Schema = "TestDb",
            Table = _tableName,
            IdColumn = "Id"
        };

        var testData = Enumerable.Range(1, 10).Select(i => new
        {
            Id = i,
            Name = $"Name{i}",
            Value = $"Value{i}"
        });

        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync($"INSERT INTO `{_tableName}` (`Id`, `Name`, `Value`) VALUES (@Id, @Name, @Value);", testData);

        // Act
        var iterator = await _provider.GetIteratorAsync(sourceSettings, 5);

        // Assert
        var batchCount = 0;
        while (await iterator.NextAsync())
        {
            batchCount++;
            Assert.Equal(5, iterator.Data.Count());
        }
        Assert.Equal(2, batchCount);

        iterator.Dispose();
    }
    
}