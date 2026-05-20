```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method               | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Nullable-Valid   |  85.38 ns | 0.223 ns | 0.174 ns | 0.0564 |      - |     472 B |
| FV-Nullable-Valid    | 226.69 ns | 0.947 ns | 0.791 ns | 0.0715 |      - |     600 B |
| CSE-Nullable-Invalid | 143.37 ns | 1.391 ns | 1.162 ns | 0.1175 |      - |     984 B |
| FV-Nullable-Invalid  | 981.24 ns | 5.758 ns | 4.808 ns | 0.3548 | 0.0019 |    2976 B |
