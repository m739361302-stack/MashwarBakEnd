using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IFileStorage
    {
        
        Task<(string storedPath, string fileName, string? contentType, long sizeBytes)>
            SaveAsync(IFormFile file, string folder, string baseName, CancellationToken ct);

        public string GetPublicUrl(string path);
    
    }
}
