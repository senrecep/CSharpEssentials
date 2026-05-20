```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method       | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Equality |  73.00 ns | 0.591 ns | 0.524 ns | 0.0478 |     400 B |
| FV-Equality  | 158.69 ns | 0.573 ns | 0.508 ns | 0.0753 |     632 B |
