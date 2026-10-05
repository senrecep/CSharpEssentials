```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method   | Job      | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev     | Gen0   | Gen1   | Allocated |
|--------- |--------- |--------------- |------------ |------------ |-------------:|------------:|-----------:|-------:|-------:|----------:|
| CSE-Ctor | ShortRun | 3              | 1           | 3           |     2.537 ns |   0.8897 ns |  0.0488 ns | 0.0029 |      - |      24 B |
| FV-Ctor  | ShortRun | 3              | 1           | 3           | 1,979.259 ns | 169.5594 ns |  9.2941 ns | 1.1444 | 0.0153 |    9624 B |
| CSE-Ctor | default  | Default        | Default     | Default     |     2.467 ns |   0.0178 ns |  0.0158 ns | 0.0029 |      - |      24 B |
| FV-Ctor  | default  | Default        | Default     | Default     | 1,995.479 ns |  27.0149 ns | 25.2698 ns | 1.1444 | 0.0153 |    9624 B |
