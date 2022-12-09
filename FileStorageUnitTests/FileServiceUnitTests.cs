using FileStorageAPI.Data.Contexts;
using FileStorageAPI.Data.Entities;
using FileStorageAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebApi.Services;

namespace FileStorageUnitTests
{
    public class FileServiceUnitTests
    {
        [Fact]
        public async Task CreateTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);

                    using (MemoryStream testFileMemoryStream = new MemoryStream())
                    {
                        IFormFile testFile = CreateTestFile(testFileMemoryStream, "TestFile", "123");
                        await fileService.CreateAsync(testFile);
                        var files = context.CustomerFiles.ToList();
                        Assert.Single(files);
                        CustomerFile expectedFileResult = new CustomerFile()
                        {
                            FileName = testFile.FileName,
                            Data = Utility.GetFileBytes(testFile),
                            CreateDate = DateTime.Now.ToUniversalTime(),
                            Id = 1,
                            IsDeleted = false,
                            Version = 1
                        };
                        AssertEqual(files.First(), expectedFileResult, 2);
                    }
                }
            }
        }

        [Fact]
        public async Task DeleteTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    using (MemoryStream testFileMemoryStream = new MemoryStream())
                    {
                        IFormFile testFile = CreateTestFile(testFileMemoryStream, "TestFile", "123");
                        await fileService.CreateAsync(testFile);
                        await fileService.DeleteAsync(testFile.FileName);
                        var undeletedFiles = context.CustomerFiles.Where(x => x.IsDeleted == false).ToList();
                        Assert.Empty(undeletedFiles);//Should not find any undeleted files
                        var deletedFiles = context.CustomerFiles.Where(x => x.IsDeleted == true).ToList();
                        Assert.Single(deletedFiles);//Should find one deleted file
                    }
                }
            }
        }

        [Fact]
        public async Task GetByNameTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    using (MemoryStream testFileMemoryStream = new MemoryStream())
                    {
                        IFormFile testFile = CreateTestFile(testFileMemoryStream, "TestFile", "123");
                        await fileService.CreateAsync(testFile);
                        var fileResult = fileService.GetByNameAsync(testFile.FileName).Result;
                        CustomerFile expectedFileResult = new CustomerFile()
                        {
                            FileName = testFile.FileName,
                            Data = Utility.GetFileBytes(testFile),
                            CreateDate = DateTime.Now.ToUniversalTime(),
                            Id = 1,
                            IsDeleted = false,
                            Version = 1
                        };
                        AssertEqual(fileResult, expectedFileResult, 2);
                    }
                }
            }
        }

        [Fact]
        public async Task ListFilesTest()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();
                var options = new DbContextOptionsBuilder<FileStorageAPIDBContext>().UseSqlite(connection).Options;
                using (var context = new FileStorageAPIDBContext(options))
                {
                    context.Database.EnsureCreated();
                    var fileService = new FileService(context);
                    var task = fileService.ListFilesAsync();
                    await task;
                    var files = task.Result.ToList();
                    Assert.Empty(files);
                }
            }
        }

        [Fact]
        public async Task UpdateTest()
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
                    using (MemoryStream testFile1MemoryStream = new MemoryStream())
                    {
                        using (MemoryStream testFile2MemoryStream = new MemoryStream())
                        {
                            IFormFile testFile1 = CreateTestFile(testFile1MemoryStream, fileName, "123");
                            await fileService.CreateAsync(testFile1);
                            IFormFile testFile2 = CreateTestFile(testFile2MemoryStream, fileName, "234");
                            await fileService.UpdateAsync(testFile2);
                            var files = context.CustomerFiles.ToList();
                            Assert.True(files.Count() == 2);

                            //Check file 1
                            CustomerFile expectedFileResult = new CustomerFile()
                            {
                                FileName = fileName,
                                Data = Utility.GetFileBytes(testFile1),
                                CreateDate = DateTime.Now.ToUniversalTime(),
                                Id = 1,
                                IsDeleted = true,
                                Version = 1
                            };
                            AssertEqual(files[0], expectedFileResult, 2);

                            //Check file 2
                            CustomerFile expectedFileResult2 = new CustomerFile()
                            {
                                FileName = fileName,
                                Data = Utility.GetFileBytes(testFile2),
                                CreateDate = DateTime.Now.ToUniversalTime(),
                                Id = 2,
                                IsDeleted = false,
                                Version = 2
                            };
                            AssertEqual(files[1], expectedFileResult2, 2);
                        }
                    }
                }
            }
        }

        private void AssertEqual(CustomerFile file, CustomerFile expectedFileResult, int createDateAllowedOffsetSeconds)
        {
            Assert.True(Math.Abs(expectedFileResult.CreateDate.Subtract(file.CreateDate).TotalSeconds) <= createDateAllowedOffsetSeconds);
            Assert.Equal(file.FileName, expectedFileResult.FileName);
            Assert.Equal(file.Id, expectedFileResult.Id);
            Assert.Equal(file.Version, expectedFileResult.Version);
            Assert.Equal(file.IsDeleted, expectedFileResult.IsDeleted);
            for (int i = 0; i < file.Data.Length; i++)
                Assert.Equal(file.Data[i], expectedFileResult.Data[i]);
        }

        private IFormFile CreateTestFile(MemoryStream ms, string name, string textData)
        {
            TextWriter tw = new StreamWriter(ms);//Don't dispose here. Memory stream disposed after test completes.
            tw.Write(textData);
            tw.Flush();
            ms.Position = 0;
            var file = new FormFile(ms, 0, textData.Length, name, name);
            return file;
        }
    }
}