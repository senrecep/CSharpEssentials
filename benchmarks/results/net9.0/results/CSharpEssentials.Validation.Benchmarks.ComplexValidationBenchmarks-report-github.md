```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method              | Job      | IterationCount | LaunchCount | WarmupCount | Mean       | Error     | StdDev   | Gen0   | Gen1   | Allocated |
|-------------------- |--------- |--------------- |------------ |------------ |-----------:|----------:|---------:|-------:|-------:|----------:|
| CSE-Complex-Valid   | ShortRun | 3              | 1           | 3           |   834.6 ns | 102.60 ns |  5.62 ns | 0.3500 |      - |   2.86 KB |
| FV-Complex-Valid    | ShortRun | 3              | 1           | 3           | 2,294.4 ns |  63.11 ns |  3.46 ns | 0.4387 |      - |    3.6 KB |
| CSE-Complex-Invalid | ShortRun | 3              | 1           | 3           | 1,386.4 ns | 197.14 ns | 10.81 ns | 0.8717 | 0.0095 |   7.13 KB |
| FV-Complex-Invalid  | ShortRun | 3              | 1           | 3           | 8,995.8 ns | 662.73 ns | 36.33 ns | 2.8381 | 0.0763 |   23.2 KB |
| CSE-Complex-Valid   | default  | Default        | Default     | Default     |   827.8 ns |   2.85 ns |  2.52 ns | 0.3500 |      - |   2.86 KB |
| FV-Complex-Valid    | default  | Default        | Default     | Default     | 2,193.4 ns |  16.09 ns | 13.44 ns | 0.4387 |      - |    3.6 KB |
| CSE-Complex-Invalid | default  | Default        | Default     | Default     | 1,343.4 ns |  11.55 ns | 10.24 ns | 0.8717 | 0.0095 |   7.13 KB |
| FV-Complex-Invalid  | default  | Default        | Default     | Default     | 8,763.4 ns |  58.93 ns | 49.21 ns | 2.8381 | 0.0763 |   23.2 KB |
