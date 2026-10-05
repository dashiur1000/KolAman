using System.Text.Json;

namespace NotificationGate.Services
{
    public class FileSystemWatche
    {
        private readonly FileSystemWatcher _watcher;
        private readonly KafkaProducerService _kafkaProducer;

        public FileSystemWatche(string directoryPath, KafkaProducerService kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            _watcher = new FileSystemWatcher(directoryPath)
            {
                NotifyFilter = NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.Security
                             | NotifyFilters.Size,
                IncludeSubdirectories = true,
                EnableRaisingEvents = false
            };

            _watcher.Created += OnCreated;
            _watcher.InternalBufferSize = 65536;
        }

        public void Start()
        {
            if (!Directory.Exists(_watcher.Path))
            {
                Directory.CreateDirectory(_watcher.Path);
            }

            _watcher.EnableRaisingEvents = true;
            Console.WriteLine($"Watching directory: {_watcher.Path}");
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                string filePath = e.FullPath;

                if (filePath.EndsWith(".ready", StringComparison.OrdinalIgnoreCase))
                {
                    string basePath = filePath.Substring(0, filePath.Length - ".ready".Length);
                    string dataFilePath = string.Empty;

                    if (File.Exists(basePath + ".json"))
                    {
                        dataFilePath = basePath + ".json";
                    }
                    //for (int i = 0; i < 10 && !File.Exists(dataFilePath); i++)
                    //{
                    //    await Task.Delay(500);
                    //    if (File.Exists(basePath + ".json")) dataFilePath = basePath + ".json";
                    //}

                    if (!string.IsNullOrEmpty(dataFilePath) && File.Exists(dataFilePath))
                    {
                        await ProcessAndSendAlertAsync(dataFilePath);
                        Console.WriteLine($"Processed: {dataFilePath}");
                    }
                    else
                    {
                        Console.WriteLine($"Data file not found for ready file: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }

        private async Task ProcessAndSendAlertAsync(string filePath)
        {
            try
            {
                string content = await File.ReadAllTextAsync(filePath);
                string ext = Path.GetExtension(filePath).ToLower();
                string jsonPayload = string.Empty;

                if (ext == ".json")
                {
                    jsonPayload = content;
                }
                else if (ext == ".txt")
                {
                    var alert = new Models.Alert
                    {
                        alert_id = Guid.NewGuid().ToString(),
                        title = Path.GetFileNameWithoutExtension(filePath),
                        content = content,
                        timestamp = DateTime.UtcNow.ToString("o"),
                        status = "new"
                    };
                    jsonPayload = JsonSerializer.Serialize(alert);
                }

                if (!string.IsNullOrEmpty(jsonPayload))
                {
                    await _kafkaProducer.SendAlertAsync(Guid.NewGuid().ToString(), jsonPayload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processing error for {filePath}: {ex.Message}");
            }
        }
    }
}
