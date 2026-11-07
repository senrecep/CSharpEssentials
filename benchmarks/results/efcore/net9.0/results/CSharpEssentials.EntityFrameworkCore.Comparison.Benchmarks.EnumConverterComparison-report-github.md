```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       | 90.62 ns | 0.028 ns | 0.025 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 78.92 ns | 0.032 ns | 0.030 ns |  0.87 |    0.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       | 58.16 ns | 0.073 ns | 0.069 ns |  0.64 |    0.00 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 73.57 ns | 0.120 ns | 0.100 ns |  0.81 |    0.00 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       | 53.13 ns | 0.127 ns | 0.118 ns |  0.59 |    0.00 |      - |         - |          NA |
|                                                             |            |          |          |          |       |         |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 84.65 ns | 0.079 ns | 0.074 ns |  1.00 |    0.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy | 58.12 ns | 0.088 ns | 0.082 ns |  0.69 |    0.00 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 77.11 ns | 0.039 ns | 0.037 ns |  0.91 |    0.00 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy | 68.16 ns | 0.246 ns | 0.230 ns |  0.81 |    0.00 |      - |         - |          NA |
|                                                             |            |          |          |          |       |         |        |           |             |
| Ef_EnumToStringConverter                                    | Write      | 59.14 ns | 0.790 ns | 0.739 ns |  1.00 |    0.02 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      | 53.00 ns | 0.030 ns | 0.028 ns |  0.90 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      | 31.28 ns | 0.009 ns | 0.007 ns |  0.53 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      | 48.32 ns | 0.021 ns | 0.019 ns |  0.82 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      | 22.45 ns | 0.007 ns | 0.006 ns |  0.38 |    0.00 |      - |         - |        0.00 |
