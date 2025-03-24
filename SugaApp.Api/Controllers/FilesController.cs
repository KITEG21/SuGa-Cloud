using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace SugaApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]

public class FilesController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(folderPath, file.FileName);

       await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { filePath });
    }

    [HttpGet("getFiles")]
    public async Task<IActionResult> GetFiles()
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        var files = Directory.GetFiles(folderPath).ToList();

        if (files.Count == 0)
        {
            return NoContent();
        }
        
        return Ok(files);
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        // Validate the file name
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name is required.");
        }

        // Define the folder where uploaded files are stored
        var uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        // Get the full path of the requested file
        var sourceFilePath = Path.Combine(uploadFolderPath, fileName);

        // Check if the file exists
        if (!System.IO.File.Exists(sourceFilePath))
        {
            return NotFound("File not found.");
        }

        // Define the folder where downloaded files will be stored
        var downloadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "DownloadedFiles");

        // Ensure the download folder exists
        if (!Directory.Exists(downloadFolderPath))
        {
            Directory.CreateDirectory(downloadFolderPath);
        }

        // Define the destination file path
        var destinationFilePath = Path.Combine(downloadFolderPath, fileName);

        // Copy the file to the download folder
        await using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
        {
            await using (var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }

        // Return a success response
        return Ok(new { message = "File copied successfully.", filePath = destinationFilePath });
    }
    
    
    [HttpDelete("delete/{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("File name is required.");
        }
        
        var uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        var filePath = Path.Combine(uploadFolderPath, fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }
        await Task.Run(() => System.IO.File.Delete(filePath));
        return Ok("File deleted successfully.");
    }
}

