# C# ↔ TypeScript Parity Plan

> **Amaç:** `CSharpEssentials` ile `tsentials` arasındaki tutarlılığı korumak için bu dosya,
> C# tarafına eklenmesi gereken her özelliği açıklar. Her madde için TypeScript tarafındaki
> referans dosya ve implementasyon notu verilmiştir.
>
> **Kardeş proje:** `/Users/recepsen/Documents/projects/recep/TypeScriptEssentials`
> **Kardeş plan:** `/Users/recepsen/Documents/projects/recep/TypeScriptEssentials/PARITY-PLAN.md`

---

## Durum Göstergesi

| Sembol | Anlam |
|--------|-------|
| `[ ]` | Henüz yapılmadı |
| `[x]` | Tamamlandı |
| `[-]` | Bilinçli olarak atlandı (dil farkı) |

---

## 1. `CSharpEssentials.These` — Yeni Paket (Öncelik: YÜKSEK) ✅

**Neden:** `These<E, A>` hem hata hem değer taşıyabilen üçüncü bir sonuç tipidir.
`Left(error)`, `Right(value)`, `Both(error, value)` — kısmi başarı senaryolarında kritik.
Batch validation, uyarı sistemi gibi kullanım alanları var.

### TypeScript Referans
```
src/these/index.ts
```

### C# Tip Tasarımı
```csharp
// Yeni proje: CSharpEssentials.These/
public readonly record struct These<E, A>
{
    private readonly bool _isLeft;
    private readonly bool _isRight;
    private readonly E? _leftValue;
    private readonly A? _rightValue;

    private These(bool isLeft, bool isRight, E? left, A? right)
    {
        _isLeft = isLeft;
        _isRight = isRight;
        _leftValue = left;
        _rightValue = right;
    }

    public static These<E, A> Left(E error)   => new(true,  false, error, default);
    public static These<E, A> Right(A value)  => new(false, true,  default, value);
    public static These<E, A> Both(E error, A value) => new(true, true, error, value);

    public bool IsLeft  => _isLeft && !_isRight;
    public bool IsRight => !_isLeft && _isRight;
    public bool IsBoth  => _isLeft && _isRight;
}
```

### Eklenecek Metodlar (TS ile 1:1)

**Factories:**
- [x] `These<E,A>.Left(E)`
- [x] `These<E,A>.Right(A)`
- [x] `These<E,A>.Both(E, A)`
- [x] `These<E,A>.FromResult(Result<A>)` — success → Right, failure → Left(firstError)

**Guards:**
- [x] `IsLeft`, `IsRight`, `IsBoth` properties

**Pipeline:**
- [x] `Map<B>(Func<A,B>)` — sadece Right/Both value'yu dönüştürür
- [x] `MapLeft<F>(Func<E,F>)` — sadece Left/Both error'ı dönüştürür
- [x] `FlatMap<B>(Func<A, These<E,B>>)`
- [x] `Tap(Action<A>)` — Right/Both için side effect
- [x] `TapLeft(Action<E>)` — Left/Both için side effect

**Extraction:**
- [x] `Match<U>(Func<E,U> onLeft, Func<A,U> onRight, Func<E,A,U> onBoth)`
- [x] `GetRight()` / `GetLeft()` — `Maybe<T>` döner

**Conversions:**
- [x] `ToResult()` — Both ve Right → success, Left → failure
- [x] `ToResultLenient()` — Both → success (hata görmezden gelinir), Left → failure

**Collections:**
- [x] `TheseCollectionExtensions.Partition<E,A>(IEnumerable<These<E,A>>)` → `(IReadOnlyList<E> lefts, IReadOnlyList<A> rights, IReadOnlyList<(E,A)> boths)`

### Dosya Yapısı
```
CSharpEssentials.These/
  These.cs
  TheseExtensions.cs
  TheseCollectionExtensions.cs
  CSharpEssentials.These.csproj
```

### TS Referans Detayı
```
src/these/index.ts  ←  tüm implementasyon referansı burada
```

---

## 2. `Result.MatchFirst` / `Result.MatchLast` (Öncelik: ORTA) ✅

**Neden:** TS tarafında `matchFirst` ve `matchLast` var; C# tarafında yalnızca `Match` var
ve tüm error listesini alıyor. İlk ya da son hata ile çalışan varyant eksik.

### TypeScript Referans
```
src/result/result.ts   ← matchFirst, matchLast implementasyonu
```

### Hedef Dosya
```
CSharpEssentials.Results/Modules/ResultT.Match.cs   ← mevcut Match'in yanına ekle
```

### C# İmzalar
```csharp
// ResultT partial class genişletmesi
public TResult MatchFirst<TResult>(
    Func<TValue, TResult> onSuccess,
    Func<Error, TResult> onFirstFailure)
    => IsSuccess ? onSuccess(Value) : onFirstFailure(FirstError);

public TResult MatchLast<TResult>(
    Func<TValue, TResult> onSuccess,
    Func<Error, TResult> onLastFailure)
    => IsSuccess ? onSuccess(Value) : onLastFailure(Errors[^1]);
```

---

## 3. `Result.SwitchFirst` / `Result.SwitchLast` (Öncelik: ORTA) ✅

**Neden:** `MatchFirst`/`MatchLast`'ın void varyantları. TS'de `switchFirst`, `switchLast` mevcut.

### TypeScript Referans
```
src/result/result.ts   ← switchFirst, switchLast
```

### Hedef Dosya
```
CSharpEssentials.Results/Modules/ResultT.Switch.cs   ← mevcut Switch'in yanına ekle
```

### C# İmzalar
```csharp
public void SwitchFirst(Action<TValue> onSuccess, Action<Error> onFirstFailure)
{
    if (IsSuccess) onSuccess(Value);
    else onFirstFailure(FirstError);
}

public void SwitchLast(Action<TValue> onSuccess, Action<Error> onLastFailure)
{
    if (IsSuccess) onSuccess(Value);
    else onLastFailure(Errors[^1]);
}
```

---

## 4. `Result.TapIf` / `Result.TapErrorIf` (Öncelik: ORTA) ✅

**Neden:** TS'de `tapIf` ve `tapErrorIf` var; C#'da `Tap` ve `TapError` var ama
koşullu varyantları yok. `BindIf` var ancak side-effect odaklı versiyonu eksik.

### TypeScript Referans
```
src/result/result.ts   ← tapIf, tapErrorIf
```

### Hedef Dosya
```
CSharpEssentials.Results/Modules/ResultT.Tap.cs      ← TapIf buraya
CSharpEssentials.Results/Modules/ResultT.TapError.cs ← TapErrorIf buraya
```

### C# İmzalar
```csharp
// ResultT partial (sync + async)
public Result<TValue> TapIf(bool condition, Action<TValue> action)
    => IsSuccess && condition ? Tap(action) : this;

public Result<TValue> TapIf(Func<TValue, bool> predicate, Action<TValue> action)
    => IsSuccess && predicate(Value) ? Tap(action) : this;

public Result<TValue> TapErrorIf(bool condition, Action<IReadOnlyList<Error>> action)
    => IsFailure && condition ? TapError(action) : this;

// Async varyantlar:
public Task<Result<TValue>> TapIfAsync(bool condition, Func<TValue, Task> action)
public Task<Result<TValue>> TapErrorIfAsync(bool condition, Func<IReadOnlyList<Error>, Task> action)
```

---

## 5. `Result.CompensateFirst` / `Result.CompensateFirstAsync` (Öncelik: ORTA) ✅

**Neden:** TS'de `compensateFirst` var; sadece `errors[0]` üzerinden recover eder.
C#'da `Compensate` tüm error listesini alır; `CompensateFirst` sadece ilk hatayı verir.

### TypeScript Referans
```
src/result/result.ts   ← compensateFirst, compensateFirstAsync
```

### Hedef Dosya
```
CSharpEssentials.Results/Modules/ResultT.Compensate.cs   ← Compensate'in yanına
```

### C# İmzalar
```csharp
public Result<TValue> CompensateFirst(Func<Error, Result<TValue>> fn)
    => IsSuccess ? this : fn(FirstError);

public Task<Result<TValue>> CompensateFirstAsync(Func<Error, Task<Result<TValue>>> fn)
    => IsSuccess ? Task.FromResult(this) : fn(FirstError);
```

---

## 6. `Maybe.FromTry(Func<T>)` (Öncelik: ORTA) ✅

**Neden:** Exception fırlatan bir ifadeyi `Maybe<T>`'ye dönüştürme. TS'de `Maybe.fromTry`
mevcut; C#'da `Try` yok — exception → None yapan factory eksik.

### TypeScript Referans
```
src/maybe/maybe.ts   ← Maybe.fromTry implementasyonu
```

### Hedef Dosya
```
CSharpEssentials.Maybe/Maybe.cs   ← Factory metodlar buraya (static section)
```

### C# İmza
```csharp
// Yeni static factory method
public static Maybe<T> FromTry<T>(Func<T> factory)
{
    try { return From(factory()); }
    catch { return None; }
}
```

---

## 7. `Maybe.TapNone(Action)` (Öncelik: ORTA) ✅

**Neden:** TS'de `Maybe.tapNone` var — None durumunda side effect için.
C#'da `Execute` / `Tap` yalnızca `HasValue` durumunda çalışıyor.

### TypeScript Referans
```
src/maybe/maybe.ts   ← Maybe.tapNone
```

### Hedef Dosya
```
CSharpEssentials.Maybe/Modules/Maybe.Tap.cs   ← mevcut Tap dosyasına ekle
```

### C# İmzalar
```csharp
public Maybe<T> TapNone(Action action)
{
    if (HasNoValue) action();
    return this;
}

public async Task<Maybe<T>> TapNoneAsync(Func<Task> action)
{
    if (HasNoValue) await action();
    return this;
}
```

---

## 8. `Maybe.GetOrElse(Func<T>)` — Lazy Fallback (Öncelik: ORTA) ✅

**Neden:** TS'de `Maybe.getOrElse(m, fn)` mevcut — None durumunda factory çağırır.
C#'da `GetValueOrDefault(defaultValue)` var ama lazy (factory) versiyonu yok.

### TypeScript Referans
```
src/maybe/maybe.ts   ← Maybe.getOrElse
```

### Hedef Dosya
```
CSharpEssentials.Maybe/Modules/GetValueOrDefault.cs   ← mevcut dosyaya factory overload
```

### C# İmza
```csharp
// Overload: lazy factory yerine sabit değer zaten var
public T GetValueOrElse(Func<T> factory)
    => HasValue ? Value : factory();
```

---

## 9. `Maybe.OrElse(Func<Maybe<T>>)` — Lazy Fallback Chain (Öncelik: ORTA) ✅

**Neden:** TS'de `Maybe.orElse(m, fn)` var — None durumunda lazy Maybe döner.
C#'da `Or(Maybe<T>)` var ama eager (önceden hesaplanmış); lazy varyant yok.

### TypeScript Referans
```
src/maybe/maybe.ts   ← Maybe.orElse
```

### Hedef Dosya
```
CSharpEssentials.Maybe/Modules/Or.cs   ← mevcut Or'un yanına
```

### C# İmza
```csharp
public Maybe<T> OrElse(Func<Maybe<T>> factory)
    => HasValue ? this : factory();

public Task<Maybe<T>> OrElseAsync(Func<Task<Maybe<T>>> factory)
    => HasValue ? Task.FromResult(this) : factory();
```

---

## 10. `RuleEngine.FromPredicate` / `FromPredicateAsync` (Öncelik: ORTA) ✅

**Neden:** TS'de `RuleEngine.fromPredicate(pred, error)` var — predicate'ten direkt kural oluşturur.
C#'da bu factory yok; kullanıcı `IRule` implement etmek zorunda.

### TypeScript Referans
```
src/rules/rule-engine.ts   ← RuleEngine.fromPredicate, fromPredicateAsync
```

### Hedef Dosya
```
CSharpEssentials.Rules/Engines/Func.cs   ← mevcut func-tabanlı kural factory'si
```

### C# İmzalar
```csharp
// RuleEngine static class genişletmesi
public static IRule<T> FromPredicate<T>(
    Func<T, bool> predicate,
    Error error)
    => new DelegateRule<T>(ctx => predicate(ctx)
        ? Result.Success()
        : Result.Failure(error));

public static IRule<T> FromPredicate<T>(
    Func<T, bool> predicate,
    Func<T, Error> errorFactory)
    => new DelegateRule<T>(ctx => predicate(ctx)
        ? Result.Success()
        : Result.Failure(errorFactory(ctx)));

public static IAsyncRule<T> FromPredicateAsync<T>(
    Func<T, Task<bool>> predicate,
    Error error);
```

**`DelegateRule<T>` yardımcı sınıfı:**
```csharp
// CSharpEssentials.Rules/Engines/ altına yeni dosya
internal sealed class DelegateRule<T>(Func<T, Result> fn) : IRule<T>
{
    public Result Evaluate(T context) => fn(context);
}
```

---

## 11. `RuleEngine` Typed Varyantları (Öncelik: ORTA)

**Neden:** TS'de `linearTyped`, `andTyped`, `orTyped` — farklı input/output tipler alan
typed rule combinator'ları mevcut. C# tarafında `RuleEngine<TContext, TResult>` var
ama explicit typed factory yok.

### TypeScript Referans
```
src/rules/rule-engine.ts   ← linearTyped, andTyped, orTyped, TypedRule<T,R>
```

### Hedef Dosya
```
CSharpEssentials.Rules/Engines/RuleEngineTResult.cs   ← mevcut typed engine
```

### C# İmzalar (taslak)
```csharp
// Generic typed combinators
public static IRule<TContext, TResult> LinearTyped<TContext, TResult>(
    params IRule<TContext, TResult>[] rules);

public static IRule<TContext, TResult> AndTyped<TContext, TResult>(
    params IRule<TContext, TResult>[] rules);

public static IRule<TContext, TResult> OrTyped<TContext, TResult>(
    params IRule<TContext, TResult>[] rules);
```

---

## 12. `FakeDateTimeProvider` — Test Double (Öncelik: DÜŞÜK) ✅

**Neden:** TS'de `createFakeDateTimeProvider(fixedTime)` ile `advance(ms)` ve `setTime(date)`
metodları olan test double mevcut. C# test projelerinde büyük kolaylık sağlar.

### TypeScript Referans
```
src/time/date-time-provider.ts   ← createFakeDateTimeProvider implementasyonu
```

### Hedef Dosya
```
CSharpEssentials.Time/   ← mevcut proje içine yeni dosya
  FakeDateTimeProvider.cs
```

### C# İmza
```csharp
public sealed class FakeDateTimeProvider(DateTimeOffset fixedTime) : IDateTimeProvider
{
    private DateTimeOffset _current = fixedTime;

    public DateTimeOffset UtcNow => _current;
    public DateOnly UtcNowDate => DateOnly.FromDateTime(_current.UtcDateTime);
    public TimeOnly UtcNowTime => TimeOnly.FromDateTime(_current.UtcDateTime);

    public void Advance(TimeSpan duration) => _current = _current.Add(duration);
    public void SetTime(DateTimeOffset time) => _current = time;
}
```

---

## 13. `pipe` / `flow` Function Combinators (Öncelik: DÜŞÜK)

**Neden:** TS'de `pipe(value, fn1, fn2, ...)` ve `flow(fn1, fn2, ...)` mevcut.
C# LINQ + extension method'larla benzer ifade gücü var ama explicit pipe/flow
daha FP-uyumlu kod yazımını kolaylaştırır.

### TypeScript Referans
```
src/function/index.ts   ← pipe, flow, identity, constant, flip implementasyonu
```

### C# Değerlendirmesi

C# 13+ `extension members` feature'ı ile şu şekilde implement edilebilir:

```csharp
// CSharpEssentials.Core/Extensions/FunctionExtensions.cs
public static class FunctionExtensions
{
    // pipe: değer alıp dizi fonksiyon uyguluyor
    public static TResult Pipe<T, TResult>(this T value, Func<T, TResult> fn)
        => fn(value);

    public static TResult Pipe<T, T2, TResult>(
        this T value,
        Func<T, T2> fn1,
        Func<T2, TResult> fn2)
        => fn2(fn1(value));

    // ... arity 3-8 overload'lar

    // identity
    public static T Identity<T>(T value) => value;

    // constant
    public static Func<T> Constant<T>(T value) => () => value;
}
```

**Not:** C#'da `pipe` için method chaining (extension method) daha idiomatik.
Bu nedenle `pipe` eklenmesi opsiyonel — kullanım ihtiyacı netleşirse ekle.

---

## Tamamlanan İşlemler

| # | Özellik | Paket | Tarih |
|---|---------|-------|-------|
| 1 | `CSharpEssentials.These` — yeni paket | `CSharpEssentials.These` | 2026-05-31 |
| 2 | `Result.MatchFirst` / `MatchLast` | `CSharpEssentials.Results` | önceki sürüm |
| 3 | `Result.SwitchFirst` / `SwitchLast` | `CSharpEssentials.Results` | önceki sürüm |
| 4 | `Result.TapIf(Func<TValue,bool>)` + async | `CSharpEssentials.Results` | 2026-05-31 |
| 5 | `Result.CompensateFirst` / `CompensateFirstAsync` | `CSharpEssentials.Results` | önceki sürüm |
| 6 | `Maybe.FromTry(Func<T>)` | `CSharpEssentials.Maybe` | 2026-05-31 |
| 7 | `Maybe.TapNone` / `TapNoneAsync` | `CSharpEssentials.Maybe` | 2026-05-31 |
| 8 | `Maybe.GetValueOrElse(Func<T>)` | `CSharpEssentials.Maybe` | 2026-05-31 |
| 9 | `Maybe.OrElse(Func<Maybe<T>>)` / `OrElseAsync` | `CSharpEssentials.Maybe` | 2026-05-31 |
| 10 | `RuleEngine.FromPredicate` / `FromPredicateAsync` | `CSharpEssentials.Rules` | 2026-05-31 |
| 12 | `FakeDateTimeProvider` | `CSharpEssentials.Time` | 2026-05-31 |

---

## Notlar

- Bu dosya hem insan hem AI agent tarafından okunmak üzere yazılmıştır.
- Her madde tamamlandığında `[ ]` → `[x]` olarak güncelle.
- Kardeş proje TS PARITY-PLAN ile çift yönlü senkronize tutulmalıdır.
- Her yeni C# paketi için `.csproj` dosyası oluştur ve ana `CSharpEssentials.sln`'e ekle.
- Test dosyaları `tests/` veya ilgili proje içi test klasörüne eklenmelidir.
