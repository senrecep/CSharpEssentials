```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method         | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Collection |  49.22 ns | 0.129 ns | 0.101 ns | 0.0296 |     248 B |
| FV-Collection  | 134.40 ns | 0.556 ns | 0.464 ns | 0.0763 |     640 B |
