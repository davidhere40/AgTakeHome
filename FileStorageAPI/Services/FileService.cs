namespace WebApi.Services;

using FileStorageAPI.Data.Entities;
using FileStorageAPI.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using FileStorageAPI.Helpers;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

public interface IFileService
{
    Task<IEnumerable<CustomerFile>> ListFilesAsync();
    Task<CustomerFile> GetByNameAsync(string name);
    Task CreateAsync(IFormFile formFile);
    Task UpdateAsync(IFormFile formFile);
    Task DeleteAsync(string name);
}

public class FileService : IFileService
{
    private FileStorageAPIDBContext _context;
    
    public FileService(FileStorageAPIDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerFile>> ListFilesAsync()
    {
        //Exclude binary data when listing files
        var task = 
            (from s in _context.CustomerFiles
             select new CustomerFile
             {
                 FileName = s.FileName,
                 Version = s.Version,
                 IsDeleted = s.IsDeleted,
                 CreateDate = s.CreateDate
             }).ToListAsync();
        await task;
        var listOfFiles = task.Result;
        return listOfFiles;
    }
        
    public async Task<CustomerFile> GetByNameAsync(string name)
    {
        var task = 
            (from s in _context.CustomerFiles
             where s.IsDeleted == false && s.FileName == name
             select s).FirstOrDefaultAsync();
        await task;
        var customerFile = task.Result;
        if (customerFile == null)
            throw new KeyNotFoundException("A file with that name was not found");

        return customerFile;
    }

    public async Task CreateAsync(IFormFile formFile)
    {
        //validate
        if (_context.CustomerFiles.Where(x => x.FileName == formFile.FileName).FirstOrDefault() != null)
            throw new ArgumentException("A file with that name already exists");

        // map model to new user object
        var newFile = new CustomerFile()
        {
            FileName = formFile.FileName,
            Data = Utility.GetFileBytes(formFile),
            CreateDate = DateTime.Now.ToUniversalTime(),
            Version = 1,
            IsDeleted = false,
        };

        // save user
        _context.CustomerFiles.Add(newFile);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(IFormFile formFile)
    {
        using (var dbContextTransaction = _context.Database.BeginTransaction())
        {
            var task = _context.CustomerFiles.Where(x => x.IsDeleted == false && x.FileName == formFile.FileName).FirstOrDefaultAsync();
            await task;
            CustomerFile? customerFile = task.Result;
            if (customerFile == null)
                throw new ArgumentException("No file with that name was found");

            //Update the current file as deleted
            customerFile.IsDeleted = true;

            var newFile = new CustomerFile()
            {
                FileName = formFile.FileName,
                Version = customerFile.Version + 1,
                IsDeleted = false,
                CreateDate = DateTime.Now.ToUniversalTime(),
                Data = Utility.GetFileBytes(formFile),
            };
            _context.CustomerFiles.Add(newFile);
            await _context.SaveChangesAsync();
            await dbContextTransaction.CommitAsync();
        }
    }

    public async Task DeleteAsync(string name)
    {
        var files = _context.CustomerFiles.Where(x => x.FileName == name && x.IsDeleted == false).ToList();
        if (files.Count == 0)
            throw new ArgumentException("No file with that name was found");
        files.ForEach(x => x.IsDeleted = true);
        await _context.SaveChangesAsync();
    }
}