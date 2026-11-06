```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       | 95.03 ns | 0.031 ns | 0.028 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 97.40 ns | 0.182 ns | 0.152 ns |  1.02 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       | 61.59 ns | 0.015 ns | 0.013 ns |  0.65 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 94.62 ns | 0.150 ns | 0.117 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       | 52.35 ns | 0.119 ns | 0.105 ns |  0.55 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 97.93 ns | 0.066 ns | 0.055 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy | 61.67 ns | 0.021 ns | 0.017 ns |  0.63 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 92.74 ns | 0.071 ns | 0.055 ns |  0.95 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy | 66.84 ns | 0.080 ns | 0.074 ns |  0.68 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| Ef_EnumToStringConverter                                    | Write      | 62.95 ns | 0.259 ns | 0.229 ns |  1.00 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      | 53.93 ns | 0.017 ns | 0.015 ns |  0.86 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      | 32.40 ns | 0.028 ns | 0.026 ns |  0.51 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      | 54.70 ns | 0.045 ns | 0.042 ns |  0.87 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      | 22.26 ns | 0.017 ns | 0.016 ns |  0.35 |      - |         - |        0.00 |
