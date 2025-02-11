using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace SODERIA_I.Services
{
    public class AzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _connectionString;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString), "La cadena de conexión de Azure Storage no está configurada.");
            }

            _blobServiceClient = new BlobServiceClient(_connectionString);
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        // Método para subir un archivo al Blob Storage
        public async Task UploadFileAsync(string containerName, string blobName, Stream fileStream)
        {
            // Obtén el contenedor (si no existe, lo crea)
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Sube el archivo al contenedor
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
        }

        // Método para descargar un archivo desde el Blob Storage
        public async Task<Stream> DownloadFileAsync(string containerName, string blobName)
        {
            // Obtén el contenedor
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Obtén el archivo (blob) desde el contenedor
            var blobClient = containerClient.GetBlobClient(blobName);

            // Descarga el archivo y devuelve un Stream
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}
