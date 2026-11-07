```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       | 79.94 ns | 0.028 ns | 0.023 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 70.06 ns | 0.026 ns | 0.023 ns |  0.88 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       | 56.49 ns | 0.268 ns | 0.251 ns |  0.71 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 54.09 ns | 0.021 ns | 0.020 ns |  0.68 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       | 46.17 ns | 0.025 ns | 0.022 ns |  0.58 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 72.30 ns | 0.028 ns | 0.026 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy | 57.45 ns | 0.080 ns | 0.071 ns |  0.79 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 60.39 ns | 0.055 ns | 0.043 ns |  0.84 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy | 63.47 ns | 0.499 ns | 0.466 ns |  0.88 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| Ef_EnumToStringConverter                                    | Write      | 47.04 ns | 0.170 ns | 0.159 ns |  1.00 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      | 36.02 ns | 0.067 ns | 0.063 ns |  0.77 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      | 30.79 ns | 0.026 ns | 0.025 ns |  0.65 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      | 55.61 ns | 0.031 ns | 0.027 ns |  1.18 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      | 26.50 ns | 0.009 ns | 0.007 ns |  0.56 |      - |         - |        0.00 |
