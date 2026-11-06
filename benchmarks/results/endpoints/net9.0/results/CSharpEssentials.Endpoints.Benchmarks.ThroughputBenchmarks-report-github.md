```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method  | Variant              | Mean      | Error     | StdDev    | Gen0   | Exceptions | Allocated |
|-------- |--------------------- |----------:|----------:|----------:|-------:|-----------:|----------:|
| **GetById** | **Plain**                | **10.075 μs** | **0.1991 μs** | **0.2131 μs** | **0.4883** |          **-** |   **9.03 KB** |
| Delete  | Plain                |  7.603 μs | 0.1450 μs | 0.1611 μs | 0.4883 |          - |   7.95 KB |
| **GetById** | **CseGenerated**         | **10.320 μs** | **0.2038 μs** | **0.2265 μs** | **0.5493** |          **-** |   **9.03 KB** |
| Delete  | CseGenerated         |  7.828 μs | 0.1236 μs | 0.1156 μs | 0.4883 |          - |   7.95 KB |
| **GetById** | **CseReflection**        | **10.090 μs** | **0.1964 μs** | **0.2621 μs** | **0.4883** |          **-** |   **9.03 KB** |
| Delete  | CseReflection        |  7.655 μs | 0.1388 μs | 0.1230 μs | 0.4883 |          - |   7.95 KB |
| **GetById** | **FastEndpoints**        | **12.726 μs** | **0.2385 μs** | **0.2651 μs** | **0.6104** |          **-** |  **10.41 KB** |
| Delete  | FastEndpoints        |  9.822 μs | 0.1893 μs | 0.1860 μs | 0.5493 |          - |    9.2 KB |
| **GetById** | **FastE(...)ceGen [22]** | **13.231 μs** | **0.2601 μs** | **0.3812 μs** | **0.6104** |          **-** |  **10.41 KB** |
| Delete  | FastE(...)ceGen [22] |  9.830 μs | 0.1932 μs | 0.2512 μs | 0.5493 |          - |    9.2 KB |
