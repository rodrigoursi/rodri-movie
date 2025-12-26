using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace rodri_movie_mvc.Service
{
    public class ImagenStorage
    {
        private readonly IWebHostEnvironment _env;
        private static readonly HashSet<string> _allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/png",
            "image/jpeg",
            "image/webp"
        };

        public ImagenStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(string userId, IFormFile file, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0) throw new InvalidOperationException("Archivo vacio.");
            if (file.Length > 2 * 1024 * 1024) throw new InvalidOperationException("Supera el límite de 2 MB.");
            // valida content-type declarado
            if (!_allowed.Contains(file.ContentType)) throw new InvalidOperationException("Formato no permitido.");

            // esto genera excepcion si el archivo es corrupto.
            using var image = await Image.LoadAsync(file.OpenReadStream(), ct);

            // recorta y redimensiona.
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(512, 512),
                Mode = ResizeMode.Crop
            }));

            // elegir extension de salida (recomendacion wemp o jpg)
            string ext = ".webp";
            string folderRel = $"/uploads/avatars/{userId}";
            string folderAbs = Path.Combine(_env.WebRootPath, "uploads", "avatars", userId);

            Directory.CreateDirectory(folderAbs);

            string fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}{ext}";
            string absPath = Path.Combine(folderAbs, fileName);
            string relPath = $"{folderRel}/{fileName}".Replace("\\", "/");
            await image.SaveAsWebpAsync(absPath, ct);

            return relPath;
        }

        public Task DeteleAsync(string? relativePath,  CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;
            string abs = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if(File.Exists(abs)) File.Delete(abs);
            return Task.CompletedTask;
        }
    }
}
