namespace WebApi.Controllers;

using FileStorageAPI.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApi.Services;

[ApiController]
[Route("[controller]")]
public class FilesController : ControllerBase
{
    private IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Gets a list of all files, included older versions and deleted files, without the binary data
    /// </summary>
    /// <response code="200">Returns a list of files with the following data: string Name, int Version, bool IsDeleted, DateTime CreateDate</response>
    [HttpGet]
    [Route("")]
    [Route("ListFilesAsync")]
    public async Task<IActionResult> ListFilesAsync()
    {
        var task = _fileService.ListFilesAsync();
        await task;
        var files = task.Result;
        return Ok(files);
    }

    /// <summary>
    /// Gets a file by name, includes the binary data as a byte array
    /// </summary>
    /// <param name="name">Name of the file, including the extension, without the file path</param>
    /// <response code="200">CustomerFile object with the following content: int Id, string Name, byte[] Data, int Version, bool IsDeleted, DateTime CreateDate</response>
    [HttpGet]
    [Route("GetByNameAsync")]
    public async Task<IActionResult> GetByNameAsync([FromQuery] string name)
    {
        var task = _fileService.GetByNameAsync(name);
        await task;
        var customerFile = task.Result;
        
        //Alternative implementation returns a FileStreamResult
        //var memory = new MemoryStream();
        //memory.Write(customerFile.Data, 0, customerFile.Data.Length);
        //memory.Position = 0;
        //File(memory, "application/octet-stream", customerFile.FileName)

        return Ok(customerFile);
    }

    /// <summary>
    /// Create a new file with the given file name and content. 
    /// </summary>
    /// <remarks>
    /// The file name will be the same as the submitted file name, including the extension, without the file path. A file with the same name cannot be uploaded twice
    /// </remarks>
    /// <param name="formFile">The file being uploaded</param>
    [HttpPost]
    public async Task<IActionResult> CreateAsync(IFormFile formFile)
    {
        await _fileService.CreateAsync(formFile);
        return Ok(new { message = "File created" });
    }

    /// <summary>
    /// Upload a new version of an existing file
    /// </summary>
    /// <remarks>Performs a soft delete of the old file. Increments the version number of the new upload</remarks>
    /// <param name="formFile">The file being uploaded</param>
    [HttpPut]
    public async Task<IActionResult> UpdateAsync(IFormFile formFile)
    {
        await _fileService.UpdateAsync(formFile);
        return Ok(new { message = "File updated" });
    }

    /// <summary>
    /// Delete a file by name
    /// </summary>
    /// <remarks>Performs a soft delete</remarks>
    /// <param name="name">Name of the file to delete, including the extension, without the file path</param>
    [HttpDelete]
    public async Task<IActionResult> DeleteAsync([FromQuery] string name)
    {
        await _fileService.DeleteAsync(name);
        return Ok(new { message = "File deleted" });
    }
}