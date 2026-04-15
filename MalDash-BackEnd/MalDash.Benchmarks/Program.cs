using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Reflection;

namespace MalDash.Benchmarks;

/// <summary>
/// Entry point for running benchmarks.
/// 
/// Usage:
///   Run all benchmarks:     dotnet run -c Release
///   Run specific benchmark: dotnet run -c Release -- --filter *FileStorage*
///   List all benchmarks:    dotnet run -c Release -- --list flat
///   Quick dry run:          dotnet run -c Release -- --filter *SaveImage* --job dry
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, config);
    }
}