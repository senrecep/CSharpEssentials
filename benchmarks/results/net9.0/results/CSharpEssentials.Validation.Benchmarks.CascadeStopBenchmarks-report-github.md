```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method           | Job      | IterationCount | LaunchCount | WarmupCount | Mean     | Error    | StdDev  | Gen0   | Gen1   | Allocated |
|----------------- |--------- |--------------- |------------ |------------ |---------:|---------:|--------:|-------:|-------:|----------:|
| CSE-Cascade-Stop | ShortRun | 3              | 1           | 3           | 108.9 ns |  8.84 ns | 0.48 ns | 0.0842 | 0.0001 |     704 B |
| FV-Cascade-Stop  | ShortRun | 3              | 1           | 3           | 845.7 ns | 93.76 ns | 5.14 ns | 0.4091 | 0.0019 |    3424 B |
| CSE-Cascade-Stop | default  | Default        | Default     | Default     | 110.4 ns |  0.31 ns | 0.28 ns | 0.0842 | 0.0001 |     704 B |
| FV-Cascade-Stop  | default  | Default        | Default     | Default     | 818.8 ns |  3.43 ns | 2.86 ns | 0.4091 | 0.0019 |    3424 B |
