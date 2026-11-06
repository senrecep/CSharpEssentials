```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       |  74.79 ns | 0.017 ns | 0.014 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 142.34 ns | 0.110 ns | 0.103 ns |  1.90 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       |  92.95 ns | 0.023 ns | 0.018 ns |  1.24 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 132.76 ns | 0.085 ns | 0.066 ns |  1.77 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       |  47.90 ns | 0.753 ns | 0.668 ns |  0.64 |      - |         - |          NA |
|                                                             |            |           |          |          |       |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 139.89 ns | 0.127 ns | 0.119 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy |  92.70 ns | 0.047 ns | 0.040 ns |  0.66 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 134.37 ns | 0.129 ns | 0.114 ns |  0.96 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy |  61.93 ns | 0.115 ns | 0.102 ns |  0.44 |      - |         - |          NA |
|                                                             |            |           |          |          |       |        |           |             |
| Ef_EnumToStringConverter                                    | Write      |  47.37 ns | 0.379 ns | 0.316 ns |  1.00 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      |  35.61 ns | 0.019 ns | 0.016 ns |  0.75 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      |  30.87 ns | 0.087 ns | 0.081 ns |  0.65 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      |  54.63 ns | 0.015 ns | 0.013 ns |  1.15 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      |  26.48 ns | 0.008 ns | 0.007 ns |  0.56 |      - |         - |        0.00 |
