# DeliveryDash.Benchmarks

Performance benchmarking project for DeliveryDash using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Prerequisites

- .NET 9.0 SDK
- Run benchmarks in **Release** mode for accurate results

## Running Benchmarks

### From Command Line

```bash
# Run all benchmarks
dotnet run -c Release

# Run specific benchmark class
dotnet run -c Release -- --filter *FileStorageService*

# Run specific benchmark method
dotnet run -c Release -- --filter *SaveMediumImage*

# List all available benchmarks
dotnet run -c Release -- --list flat

# Quick dry run (test without full iterations)
dotnet run -c Release -- --filter *SaveImage* --job dry
```

### From Visual Studio

1. Set `DeliveryDash.Benchmarks` as startup project
2. Switch to **Release** configuration
3. Run without debugging (Ctrl+F5)
4. Select benchmarks from the interactive menu

## Available Benchmarks

### FileStorageServiceBenchmarks

| Benchmark | Description |
|-----------|-------------|
| `SaveSmallImage` | Save 100x100 JPEG image |
| `SaveMediumImage` | Save 800x600 JPEG image |
| `SaveLargeImage` | Save 2000x1500 JPEG image |
| `ValidateMediumImage` | Validate 800x600 image format |
| `SaveBatchImages` | Save batch of 3 medium images |

## Adding New Benchmarks

1. Create a new class in the appropriate folder under `Services/`, `Handlers/`, etc.
2. Add the required attributes:

```csharp
using BenchmarkDotNet.Attributes;

namespace DeliveryDash.Benchmarks.Services;

[MemoryDiagnoser]                                       // Track memory allocations
[Microsoft.VSDiagnostics.DotNetObjectAllocDiagnoser]    // Detailed allocation tracking (VS integration)
[Microsoft.VSDiagnostics.CPUUsageDiagnoser]             // CPU usage metrics (VS integration)
public class MyServiceBenchmarks
{
    private MyService _service = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize service and test data once
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // Clean up resources
    }

    [Benchmark(Description = "Descriptive_Name")]
    public async Task<Result> MyBenchmark()
    {
        // Code to benchmark
    }
}
```

## Understanding Results

### Key Columns

| Column | Description |
|--------|-------------|
| **Mean** | Average execution time |
| **Error** | Half of 99.9% confidence interval |
| **StdDev** | Standard deviation |
| **Allocated** | Managed memory allocated per operation |

### Memory Analysis

The diagnosers provide detailed allocation tracking:
- `System.Byte[]` - Often from MemoryStream resizing
- `HuffmanTable[]` - ImageSharp decoder internals
- Look for large allocations in hot paths

## Integration with Visual Studio

When running benchmarks through Visual Studio's performance tools, additional metrics are captured:
- GC generation collections
- CPU utilization percentage
- Detailed allocation call stacks

## Tips

1. **Always use Release mode** - Debug builds include extra overhead
2. **Close other applications** - Reduces noise in measurements
3. **Run multiple times** - Verify consistency of results
4. **Compare before/after** - Run benchmarks before and after optimizations
