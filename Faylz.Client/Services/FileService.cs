using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Faylz.Client.Models;

namespace Faylz.Client.Services
{
    public class FileService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _jsInitialized = false;
        
        public FileService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        public async Task InitializeJsInterop()
        {
            if (_jsInitialized) return;
            
            await _jsRuntime.InvokeVoidAsync("eval", @"
                window.downloadFile = function(base64Data, contentType, fileName) {
                    const linkSource = `data:${contentType};base64,${base64Data}`;
                    const downloadLink = document.createElement('a');
                    document.body.appendChild(downloadLink);
                    downloadLink.href = linkSource;
                    downloadLink.download = fileName;
                    downloadLink.click();
                    document.body.removeChild(downloadLink);
                }
            ");
            
            _jsInitialized = true;
        }
        
        public async Task DownloadFile(byte[] fileData, string contentType, string fileName)
        {
            await InitializeJsInterop();
            await _jsRuntime.InvokeVoidAsync("downloadFile", Convert.ToBase64String(fileData), contentType, fileName);
        }
        
        public async Task<byte[]> ReadFileBytesAsync(IBrowserFile file, long maxFileSize)
        {
            try
            {

                using var stream = file.OpenReadStream(maxFileSize);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                throw new Exception($"Failed to read file '{file.Name}': {ex.Message}", ex);
            }
        }
        
        public async Task<byte[]> CreateZipArchiveAsync(Dictionary<string, byte[]> files)
        {
            using var zipMemoryStream = new MemoryStream();
            
            using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.Key);
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(file.Value);
                }
            }
            
            return zipMemoryStream.ToArray();
        }
        
        public async Task<List<UploadedFile>> ReadMultipleFilesAsync(IReadOnlyList<IBrowserFile> files, long maxFileSize)
        {
            var processedFiles = new List<UploadedFile>();
            
            foreach (var file in files)
            {
                try
                {
                    var fileData = await ReadFileBytesAsync(file, maxFileSize);
                    
                    var uploadedFile = new UploadedFile
                    {
                        FileName = file.Name,
                        FileData = fileData,
                        ContentType = file.ContentType,
                        OriginalSize = file.Size
                    };
                    
                    processedFiles.Add(uploadedFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {file.Name}: {ex.Message}");
                    // Continue with other files even if one fails
                }
            }
            
            return processedFiles;
        }
    }
} 