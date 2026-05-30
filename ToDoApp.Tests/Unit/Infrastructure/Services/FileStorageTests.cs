using FluentAssertions;
using System.Text.Json;
using ToDoApp.Application.Enums;
using ToDoApp.Infrastructure.Services;

namespace ToDoAppTests.Unit.Infrastructure.Services
{
    public class FileStorageTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly FileStorage _fileStorage;

        public FileStorageTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"FileStorageTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            _fileStorage = new FileStorage();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        #region Helper Methods

        private string GetTestFilePath(string fileName) => Path.Combine(_testDirectory, fileName);

        private class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<string> Tags { get; set; } = new();
        }

        #endregion

        #region SaveAsync Tests

        [Fact]
        public async Task SaveAsync_WithJsonFormat_SavesDataCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("test.json");
            var testData = new TestData { Id = 1, Name = "Test", Tags = new() { "tag1", "tag2" } };

            // Act
            await _fileStorage.SaveAsync(filePath, testData, FileFormat.Json);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("\"Id\": 1");
            content.Should().Contain("\"Name\": \"Test\"");
        }

        [Fact]
        public async Task SaveAsync_WithAutoFormat_InfersJsonFromExtension()
        {
            // Arrange
            var filePath = GetTestFilePath("test.json");
            var testData = new TestData { Id = 2, Name = "Auto" };

            // Act
            await _fileStorage.SaveAsync(filePath, testData);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("\"Id\": 2");
        }

        [Fact]
        public async Task SaveAsync_WithXmlFormat_ThrowsNotSupportedException()
        {
            // Arrange
            var filePath = GetTestFilePath("test.xml");
            var testData = new TestData { Id = 1, Name = "Test" };

            // Act
            var act = async () => await _fileStorage.SaveAsync(filePath, testData, FileFormat.Xml);

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*Unsupported format*");
        }

        [Fact]
        public async Task SaveAsync_WithCsvFormat_ThrowsNotSupportedException()
        {
            // Arrange
            var filePath = GetTestFilePath("test.csv");
            var testData = new TestData { Id = 1, Name = "Test" };

            // Act
            var act = async () => await _fileStorage.SaveAsync(filePath, testData, FileFormat.Csv);

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*Unsupported format*");
        }

        [Fact]
        public async Task SaveAsync_WithUnknownExtension_ThrowsNotSupportedException()
        {
            // Arrange
            var filePath = GetTestFilePath("test.unknown");
            var testData = new TestData { Id = 1, Name = "Test" };

            // Act
            var act = async () => await _fileStorage.SaveAsync(filePath, testData);

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*Unknown file extension*");
        }

        [Fact]
        public async Task SaveAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var subdirectory = Path.Combine(_testDirectory, "subdir1", "subdir2");
            var filePath = Path.Combine(subdirectory, "test.json");
            var testData = new TestData { Id = 1, Name = "Test" };

            // Act
            await _fileStorage.SaveAsync(filePath, testData);

            // Assert
            Directory.Exists(subdirectory).Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_WithEmptyObject_SavesSuccessfully()
        {
            // Arrange
            var filePath = GetTestFilePath("empty.json");
            var testData = new TestData();

            // Act
            await _fileStorage.SaveAsync(filePath, testData);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("\"Id\": 0");
        }

        [Fact]
        public async Task SaveAsync_WithList_SavesSuccessfully()
        {
            // Arrange
            var filePath = GetTestFilePath("list.json");
            var testData = new List<TestData>
            {
                new() { Id = 1, Name = "First" },
                new() { Id = 2, Name = "Second" }
            };

            // Act
            await _fileStorage.SaveAsync(filePath, testData);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("First");
            content.Should().Contain("Second");
        }

        [Fact]
        public async Task SaveAsync_OverwritesExistingFile()
        {
            // Arrange
            var filePath = GetTestFilePath("overwrite.json");
            var originalData = new TestData { Id = 1, Name = "Original" };
            var newData = new TestData { Id = 2, Name = "New" };

            // Act
            await _fileStorage.SaveAsync(filePath, originalData);
            await _fileStorage.SaveAsync(filePath, newData);

            // Assert
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().Contain("\"Id\": 2");
            content.Should().Contain("New");
            content.Should().NotContain("Original");
        }

        #endregion

        #region LoadAsync Tests

        [Fact]
        public async Task LoadAsync_WithValidJsonFile_LoadsDataCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("load.json");
            var originalData = new TestData { Id = 1, Name = "Test", Tags = new() { "tag1" } };
            await _fileStorage.SaveAsync(filePath, originalData);

            // Act
            var loadedData = await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            loadedData.Should().NotBeNull();
            loadedData.Id.Should().Be(1);
            loadedData.Name.Should().Be("Test");
            loadedData.Tags.Should().HaveCount(1);
            loadedData.Tags[0].Should().Be("tag1");
        }

        [Fact]
        public async Task LoadAsync_WithAutoFormat_InfersJsonFromExtension()
        {
            // Arrange
            var filePath = GetTestFilePath("auto.json");
            var originalData = new TestData { Id = 5, Name = "Auto" };
            await _fileStorage.SaveAsync(filePath, originalData);

            // Act
            var loadedData = await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            loadedData.Id.Should().Be(5);
        }

        [Fact]
        public async Task LoadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var filePath = GetTestFilePath("nonexistent.json");

            // Act
            var act = async () => await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            await act.Should().ThrowAsync<FileNotFoundException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task LoadAsync_WithInvalidJson_ThrowsInvalidDataException()
        {
            // Arrange
            var filePath = GetTestFilePath("invalid.json");
            await File.WriteAllTextAsync(filePath, "{ invalid json }");

            // Act
            var act = async () => await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*Invalid JSON*");
        }

        [Fact]
        public async Task LoadAsync_WithEmptyFile_ThrowsInvalidDataException()
        {
            // Arrange
            var filePath = GetTestFilePath("empty.json");
            await File.WriteAllTextAsync(filePath, "");

            // Act
            var act = async () => await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            await act.Should().ThrowAsync<InvalidDataException>();
        }

        [Fact]
        public async Task LoadAsync_WithNullJsonContent_ThrowsInvalidOperationException()
        {
            // Arrange
            var filePath = GetTestFilePath("null.json");
            await File.WriteAllTextAsync(filePath, "null");

            // Act
            var act = async () => await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Deserialized JSON was null*");
        }

        [Fact]
        public async Task LoadAsync_WithList_LoadsSuccessfully()
        {
            // Arrange
            var filePath = GetTestFilePath("list-load.json");
            var originalData = new List<TestData>
            {
                new() { Id = 1, Name = "First" },
                new() { Id = 2, Name = "Second" }
            };
            await _fileStorage.SaveAsync(filePath, originalData);

            // Act
            var loadedData = await _fileStorage.LoadAsync<List<TestData>>(filePath);

            // Assert
            loadedData.Should().HaveCount(2);
            loadedData[0].Name.Should().Be("First");
            loadedData[1].Name.Should().Be("Second");
        }

        [Fact]
        public async Task LoadAsync_WithXmlFormat_ThrowsNotSupportedException()
        {
            // Arrange
            var filePath = GetTestFilePath("test.xml");
            await File.WriteAllTextAsync(filePath, "<root></root>");

            // Act
            var act = async () => await _fileStorage.LoadAsync<TestData>(filePath, FileFormat.Xml);

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*Unsupported format*");
        }

        [Fact]
        public async Task LoadAsync_WithCsvFormat_ThrowsNotSupportedException()
        {
            // Arrange
            var filePath = GetTestFilePath("test.csv");
            await File.WriteAllTextAsync(filePath, "header1,header2");

            // Act
            var act = async () => await _fileStorage.LoadAsync<TestData>(filePath, FileFormat.Csv);

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage("*Unsupported format*");
        }

        [Fact]
        public async Task LoadAsync_WithCaseInsensitiveProperties_LoadsCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("case.json");
            var json = "{\"id\": 10, \"name\": \"CaseTest\"}";
            await File.WriteAllTextAsync(filePath, json);

            // Act
            var loadedData = await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            loadedData.Id.Should().Be(10);
            loadedData.Name.Should().Be("CaseTest");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task SaveAndLoad_RoundTrip_PreservesData()
        {
            // Arrange
            var filePath = GetTestFilePath("roundtrip.json");
            var originalData = new TestData
            {
                Id = 42,
                Name = "RoundTrip",
                Tags = new() { "tag1", "tag2", "tag3" }
            };

            // Act
            await _fileStorage.SaveAsync(filePath, originalData);
            var loadedData = await _fileStorage.LoadAsync<TestData>(filePath);

            // Assert
            loadedData.Should().BeEquivalentTo(originalData);
        }

        #endregion
    }
}