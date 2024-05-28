using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using EDIAPI.Models;
using Azure.Storage;
using EDIAPI.Services;
using System.Collections.Generic;

namespace EDIAPI.Models
{
    public class FileRequest
    {
        public string FileName { get; set; }
    }
}

namespace EDIAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]



    public class FileUploadController : ControllerBase
    {
        private const string AccountKey = "X05+r0E5+s8wnQdi/wQl3FCBs6Gb7nplNVtsk/hbSD/OdzmORbXOrchP4wQaLAFWRhfzFRzsGVgy+ASta9CVoA==";
        private const string AccountName = "apiedi";
        private const string ContainerName = "demo";

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadEdiFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseMessage("No file uploaded or file is empty"));
            }

            if (!file.FileName.EndsWith(".edi", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ResponseMessage("Invalid file type. Only EDI files are allowed."));
            }

            var blobServiceClient = new BlobServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/"), new StorageSharedKeyCredential(AccountName, AccountKey));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var blobName = $"{fileName}.xml";

            try
            {
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    await Core.ProcessEdiFile(memoryStream, blobName, blobContainerClient);
                    Console.WriteLine("File EDI đã được xử lý và lưu thành XML vào Blob Storage thành công.");
                }

                return Ok(new ResponseMessage("File uploaded and processed successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseMessage($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet]
        [Route("files")]
        public async Task<IActionResult> GetFilesFromBlobStorage()
        {
            var blobServiceClient = new BlobServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/"), new StorageSharedKeyCredential(AccountName, AccountKey));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            var blobItems = blobContainerClient.GetBlobs();
            var fileList = new List<string>();

            foreach (var blobItem in blobItems)
            {
                fileList.Add(blobItem.Name);
            }

            return Ok(fileList);
        }

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            var blobServiceClient = new BlobServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/"), new StorageSharedKeyCredential(AccountName, AccountKey));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            var blobClient = blobContainerClient.GetBlobClient(fileName);
            if (!await blobClient.ExistsAsync())
            {
                return NotFound($"File '{fileName}' not found.");
            }

            var blobDownloadInfo = await blobClient.DownloadAsync();

            return File(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType, fileName);
        }

        // [HttpPost]
        // [Route("get-file-content")]
        // public async Task<IActionResult> GetFileContent([FromBody] FileRequest fileRequest)
        // {
        //     if (fileRequest == null || string.IsNullOrEmpty(fileRequest.FileName))
        //     {
        //         return BadRequest("File name is required in the request body.");
        //     }

        //     var blobServiceClient = new BlobServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/"), new StorageSharedKeyCredential(AccountName, AccountKey));
        //     var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        //     var blobClient = blobContainerClient.GetBlobClient(fileRequest.FileName);
        //     if (!await blobClient.ExistsAsync())
        //     {
        //         return NotFound($"File '{fileRequest.FileName}' not found.");
        //     }

        //     var blobDownloadInfo = await blobClient.DownloadAsync();

        //     using (var streamReader = new StreamReader(blobDownloadInfo.Value.Content))
        //     {
        //         var fileContent = await streamReader.ReadToEndAsync();
        //         return Content(fileContent, "application/json");
        //     }
        // }


        [HttpGet]
        [Route("get-all-file-contents")]
        public async Task<IActionResult> GetAllFileContents(int limit, int page)
        {
            var blobServiceClient = new BlobServiceClient(new Uri($"https://{AccountName}.blob.core.windows.net/"), new StorageSharedKeyCredential(AccountName, AccountKey));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            var blobItems = blobContainerClient.GetBlobs();
            var totalFiles = blobItems.Count();
            var totalPages = (int)Math.Ceiling((double)totalFiles / limit);

            if (page < 1 || page > totalPages)
            {
                return BadRequest($"Page number must be between 1 and {totalPages}");
            }

            var skip = (page - 1) * limit;
            var pagedBlobItems = blobItems.Skip(skip).Take(limit);

            var fileContents = new List<object>();

            foreach (var blobItem in pagedBlobItems)
            {
                var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                var blobDownloadInfo = await blobClient.DownloadAsync();

                using (var streamReader = new StreamReader(blobDownloadInfo.Value.Content))
                {
                    var fileContent = await streamReader.ReadToEndAsync();

                    // Parse the file content as JSON
                    var jsonContent = Newtonsoft.Json.JsonConvert.DeserializeObject(fileContent);

                    fileContents.Add(new
                    {
                        FileName = blobItem.Name,
                        Content = jsonContent
                    });
                }
            }

            return Ok(fileContents);
        }


    }
}
