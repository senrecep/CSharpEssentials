using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Rules Example");
Console.WriteLine("========================================\n");

// ============================================================================
// SIMPLE RULES (using .ToRule() extension)
// ============================================================================
Console.WriteLine("--- Simple Rules (Func.ToRule) ---");

IRule<int> isPositive = ((Func<int, Result>)(x => x > 0 ? Result.Success() : Result.Failure(Error.Validation("Value.Positive", "Value must be positive")))).ToRule();
IRule<int> isLessThan100 = ((Func<int, Result>)(x => x < 100 ? Result.Success() : Result.Failure(Error.Validation("Value.Range", "Value must be less than 100")))).ToRule();

Result CheckValue(int value) => isPositive.Evaluate(value);

CheckValue(50).Switch(
    onSuccess: () => Console.WriteLine("50 is valid"),
    onFailure: errors => Console.WriteLine($"50 failed: {errors[0].Description}")
);

CheckValue(-5).Switch(
    onSuccess: () => Console.WriteLine("-5 is valid"),
    onFailure: errors => Console.WriteLine($"-5 failed: {errors[0].Description}")
);
Console.WriteLine();

// ============================================================================
// RULE ENGINE (ALL RULES MUST PASS)
// ============================================================================
Console.WriteLine("--- Rule Engine (And / All) ---");

RuleEngine.Evaluate(new[] { isPositive, isLessThan100 }.And(), 50).Switch(
    onSuccess: () => Console.WriteLine("50 passes all rules"),
    onFailure: errors => Console.WriteLine($"50 failed: {errors[0].Description}")
);

RuleEngine.Evaluate(new[] { isPositive, isLessThan100 }.And(), 150).Switch(
    onSuccess: () => Console.WriteLine("150 passes all rules"),
    onFailure: errors => Console.WriteLine($"150 failed: {errors.Length} error(s)")
);
Console.WriteLine();

// ============================================================================
// OR RULES (AT LEAST ONE MUST PASS)
// ============================================================================
Console.WriteLine("--- Or Rules ---");

IRule<int> isEven = ((Func<int, Result>)(x => x % 2 == 0 ? Result.Success() : Result.Failure(Error.Validation("Value.Even", "Value must be even")))).ToRule();
IRule<int> isDivisibleBy5 = ((Func<int, Result>)(x => x % 5 == 0 ? Result.Success() : Result.Failure(Error.Validation("Value.Div5", "Value must be divisible by 5")))).ToRule();

RuleEngine.Evaluate(new[] { isEven, isDivisibleBy5 }.Or(), 4).Switch(
    onSuccess: () => Console.WriteLine("4 passes (even)"),
    onFailure: errors => Console.WriteLine($"4 failed")
);

RuleEngine.Evaluate(new[] { isEven, isDivisibleBy5 }.Or(), 15).Switch(
    onSuccess: () => Console.WriteLine("15 passes (divisible by 5)"),
    onFailure: errors => Console.WriteLine($"15 failed")
);

RuleEngine.Evaluate(new[] { isEven, isDivisibleBy5 }.Or(), 7).Switch(
    onSuccess: () => Console.WriteLine("7 passes"),
    onFailure: errors => Console.WriteLine($"7 failed (neither even nor divisible by 5)")
);
Console.WriteLine();

// ============================================================================
// CONDITIONAL RULES (If)
// ============================================================================
Console.WriteLine("--- Conditional Rules (If) ---");

IRule<int> adultRule = ((Func<int, Result>)(x => x >= 18 ? Result.Success() : Result.Failure(Error.Validation("Age.Adult", "Must be adult")))).ToRule();
IRule<int> seniorRule = ((Func<int, Result>)(x => x >= 65 ? Result.Success() : Result.Failure(Error.Validation("Age.Senior", "Must be senior")))).ToRule();
IRule<int> minorRule = ((Func<int, Result>)(x => x < 18 ? Result.Success() : Result.Failure(Error.Validation("Age.Minor", "Must be minor")))).ToRule();

Result conditionalResult = RuleEngine.If(adultRule, seniorRule, minorRule, 70);
conditionalResult.Switch(
    onSuccess: () => Console.WriteLine("70: adult -> senior rule passed"),
    onFailure: errors => Console.WriteLine($"70 failed: {errors[0].Description}")
);

Result conditionalFail = RuleEngine.If(adultRule, seniorRule, minorRule, 10);
conditionalFail.Switch(
    onSuccess: () => Console.WriteLine("10: adult -> minor rule passed"),
    onFailure: errors => Console.WriteLine($"10 failed: {errors[0].Description}")
);
Console.WriteLine();

// ============================================================================
// LINEAR RULES (SEQUENCE)
// ============================================================================
Console.WriteLine("--- Linear Rules ---");

IRule<int> step1 = ((Func<int, Result>)(x => x > 0 ? Result.Success() : Result.Failure(Error.Validation("Step1", "Must be positive")))).ToRule();
IRule<int> step2 = ((Func<int, Result>)(x => x < 1000 ? Result.Success() : Result.Failure(Error.Validation("Step2", "Must be less than 1000")))).ToRule();
IRule<int> step3 = ((Func<int, Result>)(x => x % 2 == 0 ? Result.Success() : Result.Failure(Error.Validation("Step3", "Must be even")))).ToRule();

RuleEngine.Linear(new[] { step1, step2, step3 }, 50).Switch(
    onSuccess: () => Console.WriteLine("Linear: all steps passed"),
    onFailure: errors => Console.WriteLine($"Linear failed: {errors[0].Description}")
);

RuleEngine.Linear(new[] { step1, step2, step3 }, 1500).Switch(
    onSuccess: () => Console.WriteLine("Linear: all steps passed"),
    onFailure: errors => Console.WriteLine($"Linear failed: {errors[0].Description}")
);
Console.WriteLine();

// ============================================================================
// RULES WITH RESULT VALUE (IRule<TContext, TResult>)
// ============================================================================
Console.WriteLine("--- Rules with Result Value ---");

IRule<int, string> gradeRule = ((Func<int, Result<string>>)(score =>
{
    if (score >= 90) return Result.Success("A");
    if (score >= 80) return Result.Success("B");
    if (score >= 70) return Result.Success("C");
    if (score >= 60) return Result.Success("D");
    return Result.Failure<string>(Error.Validation("Grade", "Failed"));
})).ToRule();

gradeRule.Evaluate(85).Switch(
    onSuccess: grade => Console.WriteLine($"Grade: {grade}"),
    onError: errors => Console.WriteLine($"Grade error: {errors[0].Description}")
);

gradeRule.Evaluate(45).Switch(
    onSuccess: grade => Console.WriteLine($"Grade: {grade}"),
    onError: errors => Console.WriteLine($"Grade error: {errors[0].Description}")
);
Console.WriteLine();

// ============================================================================
// ASYNC RULES
// ============================================================================
Console.WriteLine("--- Async Rules ---");

IAsyncRule<string> asyncNotEmpty = ((Func<string, CancellationToken, ValueTask<Result>>)(async (s, ct) =>
{
    await Task.Delay(10, ct);
    return !string.IsNullOrWhiteSpace(s) ? Result.Success() : Result.Failure(Error.Validation("Name.Empty", "Name cannot be empty"));
})).ToRule<string>();

asyncNotEmpty.EvaluateAsync("Alice").Result.Switch(
    onSuccess: () => Console.WriteLine("Async rule: valid name"),
    onFailure: errors => Console.WriteLine($"Async rule failed: {errors[0].Description}")
);
Console.WriteLine();

// ============================================================================
// RULES WITH STRING VALIDATION
// ============================================================================
Console.WriteLine("--- Rules with String Validation ---");

IRule<string> notEmpty = ((Func<string, Result>)(s => !string.IsNullOrWhiteSpace(s) ? Result.Success() : Result.Failure(Error.Validation("Name.Empty", "Name cannot be empty")))).ToRule();
IRule<string> maxLength = ((Func<string, Result>)(s => s.Length <= 50 ? Result.Success() : Result.Failure(Error.Validation("Name.Length", "Name must not exceed 50 characters")))).ToRule();

RuleEngine.Evaluate(new[] { notEmpty, maxLength }.And(), "Alice").Switch(
    onSuccess: () => Console.WriteLine("'Alice' is a valid name"),
    onFailure: errors => Console.WriteLine($"'Alice' failed")
);

RuleEngine.Evaluate(new[] { notEmpty, maxLength }.And(), "").Switch(
    onSuccess: () => Console.WriteLine("'' is a valid name"),
    onFailure: errors => Console.WriteLine($"'' failed: {errors[0].Description}")
);
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
