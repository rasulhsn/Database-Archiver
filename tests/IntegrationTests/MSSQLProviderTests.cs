using Dapper;
using DbArchiver.Provider.MSSQL;
using DbArchiver.Provider.MSSQL.Config;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;

namespace DbArchiver.Provider.Integration.Tests;

public class MSSQLProviderTests
{
    private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=TestDbArchiver;Trusted_Connection=True;";
    private readonly ILogger<MSSQLProvider> _logger;
    
    public MSSQLProviderTests()
    {
        var loggerMock = new Mock<ILogger<MSSQLProvider>>();
        _logger = loggerMock.Object;

        // Ensure test database is created
        EnsureDatabase();
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertData()
    {
        // Arrange
        var provider = new MSSQLProvider(_logger);
        var settings = new TargetSettings
        {
            ConnectionString = _connectionString,
            Schema = "dbo",
            Table = "TestTable",
            IdColumn = "Id"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Name", "TestName1" } },
            new Dictionary<string, object> { { "Id", 2 }, { "Name", "TestName2" } }
        };

        // Act
        await provider.InsertAsync(settings, testData);

        // Assert
        using (var connection = new SqlConnection(_connectionString))
        {
            var result = (await connection.QueryAsync("SELECT * FROM dbo.TestTable")).ToList();
            Assert.Equal(2, result.Count);
        }
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteData()
    {
        // Arrange
        var provider = new MSSQLProvider(_logger);
        var settings = new SourceSettings
        {
            ConnectionString = _connectionString,
            Schema = "dbo",
            Table = "TestTable",
            IdColumn = "Id"
        };

        var testData = new List<object>
        {
            new Dictionary<string, object> { { "Id", 1 } }
        };

        // Act
        await provider.DeleteAsync(settings, testData);

        // Assert
        using (var connection = new SqlConnection(_connectionString))
        {
            var result = (await connection.QueryAsync("SELECT * FROM dbo.TestTable")).ToList();
            Assert.Single(result);
            Assert.Equal(2, result.First()["Id"]);
        }
    }
    
    private void EnsureDatabase()
    {
        using (var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;"))
        {
            connection.Open();

            connection.Execute(@"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TestDbArchiver')
                    BEGIN
                        CREATE DATABASE TestDbArchiver;
                    END");

            connection.ChangeDatabase("TestDbArchiver");

            connection.Execute(@"
                    IF OBJECT_ID('dbo.TestTable', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.TestTable (
                            Id INT PRIMARY KEY,
                            Name NVARCHAR(100) NOT NULL
                        );
                    END");
        }
    }
    
}