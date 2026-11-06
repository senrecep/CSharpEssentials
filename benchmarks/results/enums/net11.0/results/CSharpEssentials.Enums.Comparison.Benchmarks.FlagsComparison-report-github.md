```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                      | Categories | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_Format     | Format     | 23.4963 ns | 0.0881 ns | 0.0824 ns |  1.00 |    0.00 | 0.0043 |      72 B |        1.00 |
| Bcl_Format                  | Format     | 38.7625 ns | 0.3123 ns | 0.2921 ns |  1.65 |    0.01 | 0.0057 |      96 B |        1.33 |
| EnumsNet_Format             | Format     | 81.0763 ns | 0.7190 ns | 0.6374 ns |  3.45 |    0.03 | 0.0167 |     280 B |        3.89 |
| NetEscapades_Format         | Format     | 36.4224 ns | 0.2880 ns | 0.2694 ns |  1.55 |    0.01 | 0.0057 |      96 B |        1.33 |
|                             |            |            |           |           |       |         |        |           |             |
| Bcl_HasFlag                 | HasFlag    |  0.1682 ns | 0.0092 ns | 0.0081 ns |  1.00 |    0.07 |      - |         - |          NA |
| EnumsNet_HasFlag            | HasFlag    |  0.5819 ns | 0.0067 ns | 0.0062 ns |  3.47 |    0.16 |      - |         - |          NA |
| NetEscapades_HasFlag        | HasFlag    |  0.2109 ns | 0.0092 ns | 0.0086 ns |  1.26 |    0.08 |      - |         - |          NA |
|                             |            |            |           |           |       |         |        |           |             |
| CSharpEssentials_ToWireName | WireName   | 61.7778 ns | 0.4816 ns | 0.4269 ns |     ? |       ? | 0.0091 |     152 B |           ? |
