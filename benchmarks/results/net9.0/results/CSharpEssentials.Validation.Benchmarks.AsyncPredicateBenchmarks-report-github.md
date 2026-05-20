```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method        | Mean     | Error    | StdDev   | Median   | Gen0   | Allocated |
|-------------- |---------:|---------:|---------:|---------:|-------:|----------:|
| CSE-MustAsync | 753.3 ns | 14.10 ns | 27.50 ns | 739.6 ns | 0.1087 |     912 B |
| FV-MustAsync  | 851.5 ns | 13.04 ns | 11.56 ns | 851.4 ns | 0.1755 |    1472 B |
