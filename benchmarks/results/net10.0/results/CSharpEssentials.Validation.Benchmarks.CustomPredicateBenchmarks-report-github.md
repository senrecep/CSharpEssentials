```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Must-Valid   |  36.93 ns | 0.110 ns | 0.098 ns | 0.0296 |      - |     248 B |
| FV-Must-Valid    |  94.43 ns | 0.188 ns | 0.157 ns | 0.0716 |      - |     600 B |
| CSE-Must-Invalid |  57.25 ns | 0.420 ns | 0.350 ns | 0.0526 |      - |     440 B |
| FV-Must-Invalid  | 180.09 ns | 0.515 ns | 0.430 ns | 0.1204 | 0.0002 |    1008 B |
