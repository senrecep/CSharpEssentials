```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method           | Job      | IterationCount | LaunchCount | WarmupCount | Mean       | Error       | StdDev    | Gen0   | Gen1   | Allocated |
|----------------- |--------- |--------------- |------------ |------------ |-----------:|------------:|----------:|-------:|-------:|----------:|
| CSE-Wide-Valid   | ShortRun | 3              | 1           | 3           |   450.0 ns |   107.89 ns |   5.91 ns | 0.2170 |      - |    1816 B |
| FV-Wide-Valid    | ShortRun | 3              | 1           | 3           |   864.1 ns |    44.31 ns |   2.43 ns | 0.1173 |      - |     984 B |
| CSE-Wide-Invalid | ShortRun | 3              | 1           | 3           |   724.7 ns |    58.06 ns |   3.18 ns | 0.5569 | 0.0038 |    4664 B |
| FV-Wide-Invalid  | ShortRun | 3              | 1           | 3           | 5,387.1 ns | 3,828.65 ns | 209.86 ns | 1.7548 | 0.0305 |   14720 B |
| CSE-Wide-Valid   | default  | Default        | Default     | Default     |   450.5 ns |     1.12 ns |   0.94 ns | 0.2170 |      - |    1816 B |
| FV-Wide-Valid    | default  | Default        | Default     | Default     |   839.0 ns |     5.20 ns |   4.34 ns | 0.1163 |      - |     984 B |
| CSE-Wide-Invalid | default  | Default        | Default     | Default     |   730.8 ns |     3.48 ns |   3.09 ns | 0.5569 | 0.0038 |    4664 B |
| FV-Wide-Invalid  | default  | Default        | Default     | Default     | 5,160.1 ns |    39.24 ns |  36.70 ns | 1.7548 | 0.0305 |   14720 B |
