```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 22.6283 ns | 0.4573 ns | 0.4277 ns |  1.00 |    0.03 | 0.0043 |      72 B |        1.00 |
| Bcl_Format                  | Format     | 37.5867 ns | 0.2219 ns | 0.2076 ns |  1.66 |    0.03 | 0.0057 |      96 B |        1.33 |
| EnumsNet_Format             | Format     | 91.2175 ns | 0.3180 ns | 0.2975 ns |  4.03 |    0.07 | 0.0167 |     280 B |        3.89 |
| NetEscapades_Format         | Format     | 39.4803 ns | 0.2059 ns | 0.1926 ns |  1.75 |    0.03 | 0.0057 |      96 B |        1.33 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1757 ns | 0.0225 ns | 0.0210 ns |  1.01 |    0.16 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.8052 ns | 0.0124 ns | 0.0110 ns |  4.64 |    0.51 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2472 ns | 0.0311 ns | 0.0291 ns |  1.43 |    0.23 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 74.9783 ns | 0.4645 ns | 0.4345 ns |     ? |       ? | 0.0091 |     152 B |           ? |
