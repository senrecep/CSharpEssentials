```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method  | Variant              | Mean      | Error     | StdDev    | Gen0   | Exceptions | Gen1   | Allocated |
|-------- |--------------------- |----------:|----------:|----------:|-------:|-----------:|-------:|----------:|
| **GetById** | **Plain**                |  **9.532 μs** | **0.1803 μs** | **0.1852 μs** | **0.4883** |          **-** |      **-** |   **9.11 KB** |
| Delete  | Plain                |  7.584 μs | 0.1332 μs | 0.1246 μs | 0.4883 |          - |      - |   7.96 KB |
| **GetById** | **CseGenerated**         |  **9.482 μs** | **0.0799 μs** | **0.0667 μs** | **0.5493** |          **-** |      **-** |   **9.11 KB** |
| Delete  | CseGenerated         |  7.381 μs | 0.1368 μs | 0.1280 μs | 0.4883 |          - |      - |   7.96 KB |
| **GetById** | **CseReflection**        |  **9.688 μs** | **0.1425 μs** | **0.1332 μs** | **0.5493** |          **-** |      **-** |   **9.11 KB** |
| Delete  | CseReflection        |  7.352 μs | 0.0505 μs | 0.0448 μs | 0.4883 |          - |      - |   7.96 KB |
| **GetById** | **FastEndpoints**        | **12.195 μs** | **0.2423 μs** | **0.3064 μs** | **0.6104** |          **-** | **0.6104** |  **10.48 KB** |
| Delete  | FastEndpoints        |  9.085 μs | 0.1092 μs | 0.0968 μs | 0.5493 |          - |      - |   9.22 KB |
| **GetById** | **FastE(...)ceGen [22]** | **12.029 μs** | **0.2153 μs** | **0.3156 μs** | **0.6104** |          **-** |      **-** |  **10.48 KB** |
| Delete  | FastE(...)ceGen [22] |  9.119 μs | 0.0824 μs | 0.0688 μs | 0.5493 |          - |      - |   9.22 KB |
