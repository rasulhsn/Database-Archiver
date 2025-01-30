using DbArchiver.Core.Config;
using DbArchiver.Provider.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace DbArchiver.Core.Unit.Tests
{
    [TestFixture]
    public class DatabaseArchiverTests
    {
        private Mock<IDatabaseProviderSource> _mockSource;
        private Mock<IDatabaseProviderTarget> _mockTarget;
        private Mock<ILogger<DatabaseArchiver>> _mockLogger;
        private Mock<IDatabaseProviderIterator> _mockIterator;
        private TransferSettings _transferSettings;
        private DatabaseArchiver _archiver;

        [SetUp]
        public void SetUp()
        {
            _mockSource = new Mock<IDatabaseProviderSource>();
            _mockTarget = new Mock<IDatabaseProviderTarget>();
            _mockLogger = new Mock<ILogger<DatabaseArchiver>>();
            _mockIterator = new Mock<IDatabaseProviderIterator>();

            _transferSettings = new TransferSettings
            {
                Source = new SourceProviderSettings { Host = "localhost", TransferQuantity = 10, DeleteAfterArchived = true, Settings = new SourceSettings() },
                Target = new TargetProviderSettings { Host = "localhost", PreScript = "PreScript", Settings = new TargetSettings() }
            };

            _archiver = new DatabaseArchiver(_transferSettings, _mockSource.Object, _mockTarget.Object, _mockLogger.Object);
        }

        [Test]
        public async Task ArchiveAsync_ExecutesPreScript_WhenPreScriptExists()
        {
            // Arrange
            _mockSource.Setup(s => s.GetIteratorAsync(_transferSettings.Source.Settings, It.IsAny<int>()))
                .ReturnsAsync(_mockIterator.Object);
            _mockIterator.Setup(i => i.NextAsync()).ReturnsAsync(false);

            // Act
            await _archiver.ArchiveAsync(CancellationToken.None);

            // Assert
            _mockTarget.Verify(t => t.ExecuteScriptAsync(_transferSettings.Target.Settings, _transferSettings.Target.PreScript), Times.Once);
        }

        [Test]
        public async Task ArchiveAsync_ArchivesData_WhenIteratorHasData()
        {
            // Arrange
            var sampleData = new List<object> { new object() };
            _mockIterator.Setup(i => i.NextAsync()).ReturnsAsync(true);
            _mockIterator.Setup(i => i.Data).Returns(sampleData);
            _mockSource.Setup(s => s.GetIteratorAsync(_transferSettings.Source.Settings, It.IsAny<int>()))
                .ReturnsAsync(_mockIterator.Object);
            _mockIterator.SetupSequence(i => i.NextAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Act
            await _archiver.ArchiveAsync(CancellationToken.None);

            // Assert
            _mockTarget.Verify(t => t.InsertAsync(_transferSettings.Target.Settings, sampleData), Times.Once);
        }

        [Test]
        public async Task ArchiveAsync_DeletesData_WhenDeleteAfterArchivedIsTrue()
        {
            // Arrange
            var sampleData = new List<object> { new object() };
            _mockIterator.Setup(i => i.NextAsync()).ReturnsAsync(true);
            _mockIterator.Setup(i => i.Data).Returns(sampleData);
            _mockSource.Setup(s => s.GetIteratorAsync(_transferSettings.Source.Settings, It.IsAny<int>()))
                .ReturnsAsync(_mockIterator.Object);
            _mockIterator.SetupSequence(i => i.NextAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            // Act
            await _archiver.ArchiveAsync(CancellationToken.None);

            // Assert
            _mockSource.Verify(s => s.DeleteAsync(_transferSettings.Source.Settings, sampleData), Times.Once);
        }

        [Test]
        public async Task ArchiveAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var occurException = new Exception("Test Exception");
            _mockSource.Setup(s => s.GetIteratorAsync(_transferSettings.Source.Settings, It.IsAny<int>()))
                .ThrowsAsync(occurException);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _archiver.ArchiveAsync(CancellationToken.None));

            _mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(occurException.Message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task ArchiveAsync_DisposesIterator_WhenCompleted()
        {
            // Arrange
            _mockSource.Setup(s => s.GetIteratorAsync(_transferSettings.Source.Settings, It.IsAny<int>()))
                .ReturnsAsync(_mockIterator.Object);
            _mockIterator.Setup(i => i.NextAsync()).ReturnsAsync(false);

            // Act
            await _archiver.ArchiveAsync(CancellationToken.None);

            // Assert
            _mockIterator.Verify(i => i.Dispose(), Times.Once);
        }
    }
}