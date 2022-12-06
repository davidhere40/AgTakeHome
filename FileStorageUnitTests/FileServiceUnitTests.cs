using FileStorageAPI.Data.Contexts;
using FileStorageAPI.Data.Entities;
using FileStorageAPI.Data.Models.Files;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApi.Services;

namespace FileStorageUnitTests
{
    public class FileServiceUnitTests
    {
        [Fact]
        public void CreateTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    string fileName = "TestFile";
                    fileService.Create(fileName, new UploadFileRequest() { Data = new byte[] { 1, 2, 3 } });
                    var files = context.CustomerFiles.ToList();
                    Assert.True(files.Count() == 1);
                    CustomerFile expectedFileResult = new CustomerFile()
                    {
                        Name = fileName,
                        Data = new byte[] { 1, 2, 3 },
                        CreateDate = DateTime.Now,
                        Id = 1,
                        IsLatest = true,
                        Version = 1
                    };
                    AssertEqual(files.First(), expectedFileResult, 2);
                }
            }
        }

        [Fact]
        public void DeleteTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    string fileName = "TestFile";
                    fileService.Create(fileName, new UploadFileRequest() { Data = new byte[] { 1, 2, 3 } });
                    fileService.Delete(fileName);
                    var files = context.CustomerFiles.ToList();
                    Assert.True(files.Count() == 0);
                }
            }
        }

        [Fact]
        public void GetByNameTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    string fileName = "TestFile";
                    fileService.Create(fileName, new UploadFileRequest() { Data = new byte[] { 1, 2, 3 } });
                    var fileResult = fileService.GetByName(fileName);
                    CustomerFile expectedFileResult = new CustomerFile()
                    {
                        Name = fileName,
                        Data = new byte[] { 1, 2, 3 },
                        CreateDate = DateTime.Now,
                        Id = 1,
                        IsLatest = true,
                        Version = 1
                    };
                    AssertEqual(fileResult, expectedFileResult, 2);
                }
            }
        }

        [Fact]
        public void ListFilesTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    var files = fileService.ListFiles().ToList();
                    Assert.Equal(0, files.Count());
                }
            }
        }

        [Fact]
        public void UpdateTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    string fileName = "TestFile";
                    fileService.Create(fileName, new UploadFileRequest() { Data = new byte[] { 1, 2, 3 } });
                    fileService.Update(fileName, new UpdateFileRequest() { Data = new byte[] { 2, 3, 4 } });
                    var files = context.CustomerFiles.ToList();
                    Assert.True(files.Count() == 2);
                    
                    //Check file 1
                    CustomerFile expectedFileResult = new CustomerFile()
                    {
                        Name = fileName,
                        Data = new byte[] { 1, 2, 3 },
                        CreateDate = DateTime.Now,
                        Id = 1,
                        IsLatest = false,
                        Version = 1
                    };
                    AssertEqual(files[0], expectedFileResult, 2);

                    //Check file 2
                    CustomerFile expectedFileResult2 = new CustomerFile()
                    {
                        Name = fileName,
                        Data = new byte[] { 2, 3, 4 },
                        CreateDate = DateTime.Now,
                        Id = 2,
                        IsLatest = true,
                        Version = 2
                    };
                    AssertEqual(files[1], expectedFileResult2, 2);
                }
            }
        }

        private void AssertEqual(CustomerFile file, CustomerFile expectedFileResult, int createDateAllowedOffsetSeconds)
        {
            //Compare the dates first since it's time sensitive :)
            Assert.True(Math.Abs(expectedFileResult.CreateDate.Subtract(file.CreateDate).TotalSeconds) <= createDateAllowedOffsetSeconds);
            Assert.Equal(file.Name, expectedFileResult.Name);
            Assert.Equal(file.Id, expectedFileResult.Id);
            Assert.Equal(file.Version, expectedFileResult.Version);
            Assert.Equal(file.IsLatest, expectedFileResult.IsLatest);
            Assert.Equal(file.Data.Length, expectedFileResult.Data.Length);
            for(int i = 0; i < file.Data.Length; i++)
                Assert.Equal(file.Data[i], expectedFileResult.Data[i]);
        }
    }
}