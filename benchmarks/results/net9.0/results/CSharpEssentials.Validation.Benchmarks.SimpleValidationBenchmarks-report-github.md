```

BenchmarkDotNet v0.14.0, macOS 26.6.2 (25G83) [Darwin 25.6.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]   : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD


```
| Method             | Job      | IterationCount | LaunchCount | WarmupCount | Mean       | Error     | StdDev   | Gen0   | Gen1   | Allocated |
|------------------- |--------- |--------------- |------------ |------------ |-----------:|----------:|---------:|-------:|-------:|----------:|
| CSE-Simple-Valid   | ShortRun | 3              | 1           | 3           |   202.2 ns |  12.99 ns |  0.71 ns | 0.0782 |      - |     656 B |
| FV-Simple-Valid    | ShortRun | 3              | 1           | 3           |   375.9 ns |  31.67 ns |  1.74 ns | 0.0839 |      - |     704 B |
| CSE-Simple-Invalid | ShortRun | 3              | 1           | 3           |   297.4 ns |  81.16 ns |  4.45 ns | 0.1826 |      - |    1528 B |
| FV-Simple-Invalid  | ShortRun | 3              | 1           | 3           | 2,455.6 ns | 206.07 ns | 11.30 ns | 0.9041 | 0.0076 |    7584 B |
| CSE-Simple-Valid   | default  | Default        | Default     | Default     |   206.8 ns |   1.60 ns |  1.49 ns | 0.0782 |      - |     656 B |
| FV-Simple-Valid    | default  | Default        | Default     | Default     |   371.4 ns |   2.56 ns |  2.14 ns | 0.0839 |      - |     704 B |
| CSE-Simple-Invalid | default  | Default        | Default     | Default     |   302.3 ns |   1.15 ns |  0.96 ns | 0.1826 |      - |    1528 B |
| FV-Simple-Invalid  | default  | Default        | Default     | Default     | 2,498.2 ns |  10.77 ns |  9.55 ns | 0.9041 | 0.0076 |    7584 B |
