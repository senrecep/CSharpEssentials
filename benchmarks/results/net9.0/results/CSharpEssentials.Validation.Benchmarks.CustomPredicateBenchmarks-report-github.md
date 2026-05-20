```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean      | Error    | StdDev   | Gen0   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Must-Valid   |  49.09 ns | 0.144 ns | 0.128 ns | 0.0373 |     312 B |
| FV-Must-Valid    | 115.63 ns | 0.229 ns | 0.179 ns | 0.0716 |     600 B |
| CSE-Must-Invalid |  73.27 ns | 0.788 ns | 0.699 ns | 0.0602 |     504 B |
| FV-Must-Invalid  | 228.93 ns | 1.184 ns | 1.049 ns | 0.1204 |    1008 B |
