```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method              | Job      | IterationCount | LaunchCount | WarmupCount | Mean      | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |--------- |--------------- |------------ |------------ |----------:|----------:|----------:|-------:|----------:|
| CSE-LargeCollection | ShortRun | 3              | 1           | 3           |  7.469 μs | 1.4155 μs | 0.0776 μs | 4.2572 |  34.83 KB |
| FV-LargeCollection  | ShortRun | 3              | 1           | 3           | 22.380 μs | 3.6274 μs | 0.1988 μs | 5.7068 |  46.63 KB |
| CSE-LargeCollection | default  | Default        | Default     | Default     |  7.618 μs | 0.0564 μs | 0.0527 μs | 4.2572 |  34.83 KB |
| FV-LargeCollection  | default  | Default        | Default     | Default     | 21.944 μs | 0.1446 μs | 0.1129 μs | 5.7068 |  46.63 KB |
