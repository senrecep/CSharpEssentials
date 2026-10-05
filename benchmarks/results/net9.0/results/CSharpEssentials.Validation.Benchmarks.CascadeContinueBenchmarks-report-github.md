```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method               | Job      | IterationCount | LaunchCount | WarmupCount | Mean       | Error     | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |--------- |--------------- |------------ |------------ |-----------:|----------:|---------:|-------:|-------:|----------:|
| CSE-Cascade-Continue | ShortRun | 3              | 1           | 3           |   210.6 ns |   8.74 ns |  0.48 ns | 0.1299 | 0.0002 |   1.06 KB |
| FV-Cascade-Continue  | ShortRun | 3              | 1           | 3           | 1,729.3 ns | 376.74 ns | 20.65 ns | 0.7229 | 0.0057 |   5.91 KB |
| CSE-Cascade-Continue | default  | Default        | Default     | Default     |   209.7 ns |   1.01 ns |  0.90 ns | 0.1299 | 0.0002 |   1.06 KB |
| FV-Cascade-Continue  | default  | Default        | Default     | Default     | 1,692.2 ns |   6.60 ns |  5.51 ns | 0.7229 | 0.0057 |   5.91 KB |
