using System.Text.Json;

namespace NotificationGate.Services
{
    public class FileSystemWatch
    {
        private readonly FileSystemWatcher _watcher;
        private readonly KafkaProducerService _kafkaProducer;

        public FileSystemWatch(string directoryPath, KafkaProducerService kafkaProducer)
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
                EnableRaisingEvents = true,
            };
            _watcher.Created += OnCreated;
            _watcher.InternalBufferSize = 65536;
            Console.WriteLine($"Watching directory: {_watcher.Path}");
            Console.ReadLine();
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
                if (!string.IsNullOrEmpty(content))
                {
                    await _kafkaProducer.SendAlertAsync(Guid.NewGuid().ToString(), content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processing error for {filePath}: {ex.Message}");
            }
        }
    }
}
