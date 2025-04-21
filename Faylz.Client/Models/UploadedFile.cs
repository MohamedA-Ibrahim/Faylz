using Microsoft.AspNetCore.Components.Forms;

namespace Faylz.Client.Models
{
    public class UploadedFile
    {
        public string FileName { get; set; } = "";
        public IBrowserFile? File { get; set; }
        public bool IsProcessed { get; set; } = false;
        public byte[]? ProcessedData { get; set; }
        public long OriginalSize { get; set; }
        public long ProcessedSize { get; set; }
    }
} 