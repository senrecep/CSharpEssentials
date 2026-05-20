```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method       | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Equality |  77.45 ns | 0.562 ns | 0.470 ns | 0.0478 |     400 B |
| FV-Equality  | 149.01 ns | 0.470 ns | 0.417 ns | 0.0753 |     632 B |
