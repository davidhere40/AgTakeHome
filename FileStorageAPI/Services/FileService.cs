namespace WebApi.Services;

using FileStorageAPI.Data.Entities;
using FileStorageAPI.Data.Models.Files;
using FileStorageAPI.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Http;

public interface IFileService
{
    IEnumerable<CustomerFile> ListFiles();
    CustomerFile GetByName(string name);
    void Create(string name, UploadFileRequest model);
    void Update(string name, UpdateFileRequest model);
    void Delete(string name);
}

public class FileService : IFileService
{
    private FileStorageAPIDBContext _context;
    
    public FileService(FileStorageAPIDBContext context)
    {
        _context = context;
    }

    public IEnumerable<CustomerFile> ListFiles()
    {
        //Exclude binary data when listing files
        var results = 
            (from s in _context.CustomerFiles
             select new CustomerFile
             {
                 Name = s.Name,
                 Version = s.Version,
                 IsLatest = s.IsLatest,
                 CreateDate = s.CreateDate
             }).ToList();
        return results;
    }
        
    public CustomerFile GetByName(string name)
    {
        var customerFile = 
            (from s in _context.CustomerFiles
             where s.IsLatest == true && s.Name == name
             select s).FirstOrDefault();
        if (customerFile == null)
            throw new KeyNotFoundException("A file with that name was not found");
        return customerFile;
    }

    public void Create(string name, UploadFileRequest model)
    {
        // validate
        if (_context.CustomerFiles.Where(x => x.Name == name).FirstOrDefault() != null)
            throw new ArgumentException("A file with that name already exists");

        // map model to new user object
        var newFile = new CustomerFile()
        {
            Name = name,
            Data = model.Data,
            CreateDate = DateTime.Now,
            Version = 1,
            IsLatest = true,
        };

        // save user
        _context.CustomerFiles.Add(newFile);
        _context.SaveChanges();
    }

    public void Update(string name, UpdateFileRequest model)
    {
        using (var dbContextTransaction = _context.Database.BeginTransaction())
        {
            CustomerFile? customerFile = _context.CustomerFiles.Where(x => x.IsLatest == true && x.Name == name).FirstOrDefault();
            if(customerFile == null)
                throw new ArgumentException("No file with that name was found");

            //Update the current file as not latest
            customerFile.IsLatest = false;

            var newFile = new CustomerFile()
            {
                Name = name,
                Version = customerFile.Version + 1,
                IsLatest = true,
                CreateDate = DateTime.Now,
                Data = model.Data,  
            };
            _context.CustomerFiles.Add(newFile);
            _context.SaveChanges();
            dbContextTransaction.Commit();
        }
    }

    public void Delete(string name)
    {
        int count = _context.CustomerFiles.Where(x => x.Name == name).ExecuteDelete();
        if (count == 0)
            throw new ArgumentException("No file with that name was found");
        _context.SaveChanges();
    }
}