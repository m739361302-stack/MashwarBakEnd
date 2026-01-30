using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    using Application.Interfaces;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;

    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;

        public LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor http)
        {
            _env = env;
            _http = http;
        }

        public async Task<(string storedPath, string fileName, string? contentType, long sizeBytes)>
            SaveAsync(IFormFile file, string folder, string baseName, CancellationToken ct)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Empty file");

            var root = Path.Combine(_env.ContentRootPath, "uploads", folder);
            Directory.CreateDirectory(root);

            var ext = Path.GetExtension(file.FileName);
            var safeFileName = $"{baseName}{ext}";
            var fullPath = Path.Combine(root, safeFileName);

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream, ct);

            var storedPath = Path.Combine("uploads", folder, safeFileName).Replace("\\", "/");
            return (storedPath, safeFileName, file.ContentType, file.Length);
        }

        public string GetPublicUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            var cleanPath = path.Replace("\\", "/").TrimStart('/');

            // لو رابط جاهز
            if (Uri.TryCreate(cleanPath, UriKind.Absolute, out var abs))
                return abs.ToString();

            var req = _http.HttpContext?.Request;

            // إذا تم استدعاؤها خارج Request (مثلاً background job) رجع مسار نسبي
            if (req == null)
                return "/" + cleanPath;

            var baseUrl = $"{req.Scheme}://{req.Host}";
            return $"{baseUrl}/{cleanPath}";
        }
    }
    //public class LocalFileStorage : IFileStorage
    //{
    //    private readonly IWebHostEnvironment _env;
    //    public LocalFileStorage(IWebHostEnvironment env) => _env = env;

    //    public async Task<(string storedPath, string fileName, string? contentType, long sizeBytes)>
    //        SaveAsync(IFormFile file, string folder, string baseName, CancellationToken ct)
    //    {
    //        if (file == null || file.Length == 0) throw new ArgumentException("Empty file");

    //        var root = Path.Combine(_env.ContentRootPath, "uploads", folder);
    //        Directory.CreateDirectory(root);

    //        var ext = Path.GetExtension(file.FileName);
    //        var safeFileName = $"{baseName}{ext}";
    //        var fullPath = Path.Combine(root, safeFileName);

    //        await using var stream = File.Create(fullPath);
    //        await file.CopyToAsync(stream, ct);

    //        // نخزن path نسبي عشان ما يرتبط بالمسار المحلي
    //        var storedPath = Path.Combine("uploads", folder, safeFileName).Replace("\\", "/");

    //        return (storedPath, safeFileName, file.ContentType, file.Length);
    //    }

    //    public string GetPublicUrl(string path)
    //    {
    //        if (string.IsNullOrWhiteSpace(path)) return "";

    //        var cleanPath = path.Replace("\\", "/").TrimStart('/');

    //        // لو جاك رابط جاهز لا تعيد تركيبه
    //        if (Uri.TryCreate(cleanPath, UriKind.Absolute, out var abs))
    //            return abs.ToString();

    //        // إن لم يتم ضبط BaseUrl، رجّع مسار نسبي
    //        if (string.IsNullOrWhiteSpace(_publicBaseUrl))
    //            return "/" + cleanPath;

    //        return $"{_publicBaseUrl.TrimEnd('/')}/{cleanPath}";
    //    }
    //}

}
