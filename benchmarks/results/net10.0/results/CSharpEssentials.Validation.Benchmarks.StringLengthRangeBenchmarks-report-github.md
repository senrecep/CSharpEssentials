```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                  | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------------ |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-LengthRange-Valid   |  64.46 ns | 0.768 ns | 0.718 ns | 0.0391 |      - |     328 B |
| FV-LengthRange-Valid    | 123.98 ns | 0.444 ns | 0.415 ns | 0.0715 |      - |     600 B |
| CSE-LengthRange-Invalid | 139.86 ns | 1.574 ns | 1.472 ns | 0.1013 |      - |     848 B |
| FV-LengthRange-Invalid  | 814.11 ns | 1.843 ns | 1.439 ns | 0.4549 | 0.0029 |    3808 B |
