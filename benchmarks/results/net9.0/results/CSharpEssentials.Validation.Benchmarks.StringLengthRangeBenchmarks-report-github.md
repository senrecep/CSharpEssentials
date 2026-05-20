```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                  | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------------ |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-LengthRange-Valid   |  83.71 ns | 0.340 ns | 0.265 ns | 0.0545 |      - |     456 B |
| FV-LengthRange-Valid    | 154.75 ns | 0.616 ns | 0.576 ns | 0.0715 |      - |     600 B |
| CSE-LengthRange-Invalid | 146.40 ns | 0.318 ns | 0.248 ns | 0.1090 |      - |     912 B |
| FV-LengthRange-Invalid  | 973.32 ns | 3.163 ns | 2.642 ns | 0.4616 | 0.0019 |    3872 B |
