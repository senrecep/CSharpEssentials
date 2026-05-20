```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                  | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------------ |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-LengthRange-Valid   |  67.12 ns | 0.211 ns | 0.176 ns | 0.0391 |      - |     328 B |
| FV-LengthRange-Valid    | 115.55 ns | 0.279 ns | 0.247 ns | 0.0716 |      - |     600 B |
| CSE-LengthRange-Invalid | 145.09 ns | 0.553 ns | 0.432 ns | 0.1013 |      - |     848 B |
| FV-LengthRange-Invalid  | 800.15 ns | 9.501 ns | 8.887 ns | 0.4549 | 0.0029 |    3808 B |
