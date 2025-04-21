using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Faylz.Client.Services
{
    public class FileService
    {
        private readonly IJSRuntime _jsRuntime;
        
        public FileService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        public async Task InitializeJsInterop()
        {
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
        }
        
        public async Task DownloadFile(byte[] fileData, string contentType, string fileName)
        {
            await _jsRuntime.InvokeVoidAsync("downloadFile", Convert.ToBase64String(fileData), contentType, fileName);
        }
        
        public async Task<byte[]> ReadFileBytesAsync(IBrowserFile file, long maxFileSize)
        {
            using var stream = file.OpenReadStream(maxFileSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
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
    }
} 