namespace WebApi.Controllers;

using FileStorageAPI.Data.Models.Files;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public IActionResult ListFiles()
    {
        var files = _fileService.ListFiles();
        return Ok(files);
    }

    [HttpGet("{name}")]
    public IActionResult GetByName(string name)
    {
        var file = _fileService.GetByName(name);
        return Ok(file);
    }

    [HttpPost("{name}")]
    public IActionResult Create(string name, UploadFileRequest model)
    {
        _fileService.Create(name, model);
        return Ok(new { message = "User created" });
    }

    [HttpPut("{name}")]
    public IActionResult Update(string name, UpdateFileRequest model)
    {
        _fileService.Update(name, model);
        return Ok(new { message = "User updated" });
    }

    [HttpDelete("{name}")]
    public IActionResult Delete(string name)
    {
        _fileService.Delete(name);
        return Ok(new { message = "User deleted" });
    }
}