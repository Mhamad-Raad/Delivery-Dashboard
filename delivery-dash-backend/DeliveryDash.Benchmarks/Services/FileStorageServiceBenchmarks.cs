using BenchmarkDotNet.Attributes;
using DeliveryDash.Application.Options;
using DeliveryDash.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DeliveryDash.Benchmarks.Services;

/// <summary>
/// Benchmarks for FileStorageService image processing operations.
/// 
/// Key metrics to watch:
/// - Memory allocations (GC pressure from MemoryStream and ImageSharp)
/// - Time scaling with image dimensions
/// - Double-loading overhead in batch operations
/// - Concurrent throughput under server load
/// - Scalability across multiple cores (AMD Epyc optimization)
/// </summary>      
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class FileStorageServiceBenchmarks
{
    private FileStorageService _service = null!;
    private MemoryStream _smallImageStream = null!;
    private MemoryStream _mediumImageStream = null!;
    private MemoryStream _largeImageStream = null!;
    private MemoryStream _extraLargeImageStream = null!;
    private string _tempFolder = null!;

    [Params(1, 4, 8, 16, 32)]
    public int ConcurrentRequests { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), $"DeliveryDash_Benchmark_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempFolder);

        var options = Options.Create(new FileStorageOptions
        {
            UploadPath = _tempFolder,
            AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"],
            MaxFileSizeInBytes = 10 * 1024 * 1024
        });

        _service = new FileStorageService(options, NullLogger<FileStorageService>.Instance);

        // Create test images of varying sizes - more realistic server workload
        _smallImageStream = CreateTestImageStream(100, 100);
        _mediumImageStream = CreateTestImageStream(800, 600);
        _largeImageStream = CreateTestImageStream(2000, 1500);
        _extraLargeImageStream = CreateTestImageStream(4000, 3000);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _smallImageStream?.Dispose();
        _mediumImageStream?.Dispose();
        _largeImageStream?.Dispose();
        _extraLargeImageStream?.Dispose();

        if (Directory.Exists(_tempFolder))
        {
            try { Directory.Delete(_tempFolder, true); } catch { }
        }
    }

    #region Test Data Helpers

    private static MemoryStream CreateTestImageStream(int width, int height)
    {
        var stream = new MemoryStream();
        using (var image = new Image<Rgba32>(width, height))
        {
            // Fill with gradient to simulate real image data
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new Rgba32(
                        (byte)(x * 255 / width),
                        (byte)(y * 255 / height),
                        128,
                        255);
                }
            }
            image.SaveAsJpeg(stream);
        }
        stream.Position = 0;
        return stream;
    }

    #endregion

    #region Single Image Benchmarks

    [Benchmark(Description = "SaveImage_Small_100x100")]
    public async Task<string> SaveSmallImage()
    {
        _smallImageStream.Position = 0;
        return await _service.SaveImageAsync(_smallImageStream, "test.jpg", "benchtest");
    }

    [Benchmark(Description = "SaveImage_Medium_800x600")]
    public async Task<string> SaveMediumImage()
    {
        _mediumImageStream.Position = 0;
        return await _service.SaveImageAsync(_mediumImageStream, "test.jpg", "benchtest");
    }

    [Benchmark(Description = "SaveImage_Large_2000x1500")]
    public async Task<string> SaveLargeImage()
    {
        _largeImageStream.Position = 0;
        return await _service.SaveImageAsync(_largeImageStream, "test.jpg", "benchtest");
    }

    [Benchmark(Description = "SaveImage_ExtraLarge_4000x3000")]
    public async Task<string> SaveExtraLargeImage()
    {
        _extraLargeImageStream.Position = 0;
        return await _service.SaveImageAsync(_extraLargeImageStream, "test.jpg", "benchtest");
    }

    #endregion

    #region Validation Benchmarks

    [Benchmark(Description = "ValidateImage_Medium")]
    public bool ValidateMediumImage()
    {
        _mediumImageStream.Position = 0;
        return _service.ValidateImage(_mediumImageStream, 10 * 1024 * 1024);
    }

    #endregion

    #region Batch Operation Benchmarks

    [Benchmark(Description = "SaveImages_Batch_3_Medium")]
    public async Task<List<string>> SaveBatchImages()
    {
        // Create fresh copies to simulate real batch upload
        using var copy1 = new MemoryStream(_mediumImageStream.ToArray());
        using var copy2 = new MemoryStream(_mediumImageStream.ToArray());
        using var copy3 = new MemoryStream(_mediumImageStream.ToArray());

        var images = new List<(Stream, string)>
        {
            (copy1, "test1.jpg"),
            (copy2, "test2.jpg"),
            (copy3, "test3.jpg")
        };

        return await _service.SaveImagesAsync(images, "batchtest");
    }

    [Benchmark(Description = "SaveImages_Batch_10_Mixed")]
    public async Task<List<string>> SaveLargeBatchMixedImages()
    {
        var images = new List<(Stream, string)>();

        for (int i = 0; i < 10; i++)
        {
            // Mix of small, medium, and large images
            var sourceStream = i % 3 == 0 ? _smallImageStream :
                             i % 3 == 1 ? _mediumImageStream :
                             _largeImageStream;
            images.Add((new MemoryStream(sourceStream.ToArray()), $"test{i}.jpg"));
        }

        try
        {
            return await _service.SaveImagesAsync(images, "largebatchtest");
        }
        finally
        {
            foreach (var (stream, _) in images)
            {
                stream.Dispose();
            }
        }
    }

    #endregion

    #region High-Concurrency Server Stress Benchmarks

    [Benchmark(Description = "Concurrent_SaveMedium_Stress")]
    public async Task ConcurrentSaveMediumStress()
    {
        var tasks = new Task<string>[ConcurrentRequests];

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            var streamCopy = new MemoryStream(_mediumImageStream.ToArray());
            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    return await _service.SaveImageAsync(streamCopy, $"concurrent_{i}.jpg", "stresstest");
                }
                finally
                {
                    streamCopy.Dispose();
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Concurrent_SaveLarge_Stress")]
    public async Task ConcurrentSaveLargeStress()
    {
        var tasks = new Task<string>[ConcurrentRequests];

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            var streamCopy = new MemoryStream(_largeImageStream.ToArray());
            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    return await _service.SaveImageAsync(streamCopy, $"concurrent_large_{i}.jpg", "stresstest");
                }
                finally
                {
                    streamCopy.Dispose();
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Concurrent_MixedOperations_Stress")]
    public async Task ConcurrentMixedOperationsStress()
    {
        var tasks = new Task[ConcurrentRequests];

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            int index = i;
            tasks[i] = Task.Run(async () =>
            {
                // Simulate mixed workload: validation + save operations
                if (index % 4 == 0)
                {
                    using var streamCopy = new MemoryStream(_smallImageStream.ToArray());
                    streamCopy.Position = 0;
                    _service.ValidateImage(streamCopy, 10 * 1024 * 1024);
                    streamCopy.Position = 0;
                    await _service.SaveImageAsync(streamCopy, $"mixed_{index}.jpg", "mixedtest");
                }
                else if (index % 4 == 1)
                {
                    using var streamCopy = new MemoryStream(_mediumImageStream.ToArray());
                    await _service.SaveImageAsync(streamCopy, $"mixed_{index}.jpg", "mixedtest");
                }
                else if (index % 4 == 2)
                {
                    using var streamCopy = new MemoryStream(_largeImageStream.ToArray());
                    await _service.SaveImageAsync(streamCopy, $"mixed_{index}.jpg", "mixedtest");
                }
                else
                {
                    using var streamCopy = new MemoryStream(_mediumImageStream.ToArray());
                    streamCopy.Position = 0;
                    _service.ValidateImage(streamCopy, 10 * 1024 * 1024);
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "Sustained_Load_50_Requests")]
    public async Task SustainedLoadTest()
    {
        const int totalRequests = 50;
        var tasks = new Task<string>[totalRequests];

        for (int i = 0; i < totalRequests; i++)
        {
            // Realistic size distribution: 60% medium, 30% large, 10% small
            var sourceStream = i % 10 < 6 ? _mediumImageStream :
                             i % 10 < 9 ? _largeImageStream :
                             _smallImageStream;

            var streamCopy = new MemoryStream(sourceStream.ToArray());
            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    return await _service.SaveImageAsync(streamCopy, $"sustained_{i}.jpg", "sustainedtest");
                }
                finally
                {
                    streamCopy.Dispose();
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    #endregion
}