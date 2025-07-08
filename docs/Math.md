# 🔢 Math Utilities

[![System.Numerics](https://img.shields.io/badge/System-Numerics-blue?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/api/system.numerics)
[![High Precision](https://img.shields.io/badge/High-Precision-green?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger)

Advanced mathematical utilities for high-precision calculations, BigInteger operations, and decimal manipulations with safe conversion methods.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [BigInteger Extensions](#biginteger-extensions)
- [Decimal Operations](#decimal-operations)
- [Safe Conversions](#safe-conversions)
- [Usage Examples](#usage-examples)
- [Advanced Scenarios](#advanced-scenarios)
- [Performance Considerations](#performance-considerations)
- [Best Practices](#best-practices)

## Overview

The Math utilities in Rinzler78.NetExtension provide enhanced mathematical operations for high-precision calculations, safe type conversions, and advanced numerical operations using BigInteger and decimal types.

## Core Classes

### `BigIntegerExt`
Extension methods for BigInteger operations and safe conversions:

```csharp
public static class BigIntegerExt
{
    // Safe conversion methods
    public static ulong ToULong(this BigInteger bigInteger)
    public static double ToDouble(this BigInteger bigInteger)
    public static decimal ToDecimal(this BigInteger bigInteger)
}
```

### `DecimalHelper`
Mathematical operations for decimal types:

```csharp
public static class DecimalHelper
{
    // Power operations
    public static decimal Pow(decimal x, decimal y)
    
    // Logarithmic operations
    public static decimal Log(this decimal value)
}
```

## BigInteger Extensions

### Safe Type Conversions
The BigInteger extensions provide safe conversion methods that handle overflow conditions gracefully:

```csharp
using System.Numerics;
using Rinzler78.NetExtension.Math;

// Safe conversion to ulong
var bigInt = new BigInteger(123456789012345);
ulong result = bigInt.ToULong();

// Handling overflow scenarios
var hugeBigInt = BigInteger.Parse("999999999999999999999999999999999999999");
ulong overflowResult = hugeBigInt.ToULong(); // Returns ulong.MaxValue

// Handling negative numbers
var negativeBigInt = new BigInteger(-123456789);
ulong negativeResult = negativeBigInt.ToULong(); // Returns ulong.MinValue (0)
```

### Conversion Examples
```csharp
// Various BigInteger to primitive type conversions
var bigInteger = new BigInteger(987654321098765);

// Convert to ulong (safe)
ulong ulongValue = bigInteger.ToULong();
Console.WriteLine($"ULong: {ulongValue}");

// Convert to double (safe)
double doubleValue = bigInteger.ToDouble();
Console.WriteLine($"Double: {doubleValue}");

// Convert to decimal (safe)
decimal decimalValue = bigInteger.ToDecimal();
Console.WriteLine($"Decimal: {decimalValue}");
```

### Overflow Handling
```csharp
public static class BigIntegerConversionExamples
{
    public static void DemonstrateOverflowHandling()
    {
        // Test with maximum values
        var maxBigInt = BigInteger.Parse("999999999999999999999999999999999999999");
        var minBigInt = BigInteger.Parse("-999999999999999999999999999999999999999");
        
        // Safe conversions with overflow protection
        var maxULong = maxBigInt.ToULong();    // Returns ulong.MaxValue
        var minULong = minBigInt.ToULong();    // Returns ulong.MinValue (0)
        
        var maxDouble = maxBigInt.ToDouble();   // Returns double.MaxValue
        var minDouble = minBigInt.ToDouble();   // Returns double.MinValue
        
        var maxDecimal = maxBigInt.ToDecimal(); // Returns decimal.MaxValue
        var minDecimal = minBigInt.ToDecimal(); // Returns decimal.MinValue
        
        Console.WriteLine($"Max ULong: {maxULong}");
        Console.WriteLine($"Min ULong: {minULong}");
        Console.WriteLine($"Max Double: {maxDouble}");
        Console.WriteLine($"Min Double: {minDouble}");
        Console.WriteLine($"Max Decimal: {maxDecimal}");
        Console.WriteLine($"Min Decimal: {minDecimal}");
    }
}
```

## Decimal Operations

### Power Operations
```csharp
using Rinzler78.NetExtension.Math;

// Calculate decimal powers
decimal baseValue = 2.5m;
decimal exponent = 3.0m;
decimal result = DecimalHelper.Pow(baseValue, exponent);
Console.WriteLine($"{baseValue}^{exponent} = {result}"); // 2.5^3.0 = 15.625

// Complex power calculations
decimal complexBase = 1.5m;
decimal complexExponent = 2.7m;
decimal complexResult = DecimalHelper.Pow(complexBase, complexExponent);
Console.WriteLine($"{complexBase}^{complexExponent} = {complexResult}");
```

### Logarithmic Operations
```csharp
// Natural logarithm calculations
decimal value = 10.0m;
decimal logResult = value.Log();
Console.WriteLine($"ln({value}) = {logResult}"); // ln(10) ≈ 2.302585

// Logarithm in financial calculations
decimal principal = 1000.0m;
decimal rate = 0.05m; // 5% interest rate
decimal years = 10.0m;

// Compound interest calculation using logarithms
decimal compoundAmount = principal * DecimalHelper.Pow(1 + rate, years);
decimal timeToDouble = DecimalHelper.Pow(2.0m, 1.0m / years).Log() / rate.Log();
```

## Safe Conversions

### Error-Safe Conversion Patterns
```csharp
public static class SafeConversionExtensions
{
    public static (bool Success, ulong Value) TryToULong(this BigInteger bigInteger)
    {
        try
        {
            if (bigInteger >= 0 && bigInteger <= ulong.MaxValue)
            {
                return (true, (ulong)bigInteger);
            }
            return (false, bigInteger < 0 ? ulong.MinValue : ulong.MaxValue);
        }
        catch
        {
            return (false, bigInteger < 0 ? ulong.MinValue : ulong.MaxValue);
        }
    }
    
    public static (bool Success, double Value) TryToDouble(this BigInteger bigInteger)
    {
        try
        {
            double result = (double)bigInteger;
            if (!double.IsInfinity(result) && !double.IsNaN(result))
            {
                return (true, result);
            }
            return (false, bigInteger < 0 ? double.MinValue : double.MaxValue);
        }
        catch
        {
            return (false, bigInteger < 0 ? double.MinValue : double.MaxValue);
        }
    }
    
    public static (bool Success, decimal Value) TryToDecimal(this BigInteger bigInteger)
    {
        try
        {
            if (bigInteger >= (BigInteger)decimal.MinValue && 
                bigInteger <= (BigInteger)decimal.MaxValue)
            {
                return (true, (decimal)bigInteger);
            }
            return (false, bigInteger < 0 ? decimal.MinValue : decimal.MaxValue);
        }
        catch
        {
            return (false, bigInteger < 0 ? decimal.MinValue : decimal.MaxValue);
        }
    }
}

// Usage
var bigInt = new BigInteger(123456789);
var (success, value) = bigInt.TryToULong();
if (success)
{
    Console.WriteLine($"Conversion successful: {value}");
}
else
{
    Console.WriteLine($"Conversion failed, fallback value: {value}");
}
```

## Usage Examples

### Financial Calculations
```csharp
public class FinancialCalculator
{
    public static decimal CalculateCompoundInterest(decimal principal, decimal rate, 
        decimal time, decimal compoundingFrequency = 1)
    {
        // A = P(1 + r/n)^(nt)
        decimal ratePerPeriod = rate / compoundingFrequency;
        decimal exponent = compoundingFrequency * time;
        
        return principal * DecimalHelper.Pow(1 + ratePerPeriod, exponent);
    }
    
    public static decimal CalculateContinuousCompoundInterest(decimal principal, 
        decimal rate, decimal time)
    {
        // A = Pe^(rt)
        decimal exponent = rate * time;
        decimal e = 2.71828182845904523536m; // Approximation of e
        
        return principal * DecimalHelper.Pow(e, exponent);
    }
    
    public static decimal CalculateTimeToDouble(decimal rate)
    {
        // Time = ln(2) / rate
        decimal ln2 = DecimalHelper.Log(2.0m);
        return ln2 / rate;
    }
}

// Usage
decimal principal = 1000.0m;
decimal rate = 0.05m; // 5% annual interest
decimal time = 10.0m; // 10 years

decimal compoundAmount = FinancialCalculator.CalculateCompoundInterest(principal, rate, time);
Console.WriteLine($"Compound interest after {time} years: {compoundAmount:C}");

decimal timeToDouble = FinancialCalculator.CalculateTimeToDouble(rate);
Console.WriteLine($"Time to double at {rate:P} rate: {timeToDouble:F2} years");
```

### Scientific Calculations
```csharp
public class ScientificCalculator
{
    public static decimal CalculateExponentialGrowth(decimal initialValue, decimal growthRate, 
        decimal time)
    {
        // N(t) = N₀ * e^(rt)
        decimal e = 2.71828182845904523536m;
        decimal exponent = growthRate * time;
        
        return initialValue * DecimalHelper.Pow(e, exponent);
    }
    
    public static decimal CalculateHalfLife(decimal lambda)
    {
        // t₁/₂ = ln(2) / λ
        decimal ln2 = DecimalHelper.Log(2.0m);
        return ln2 / lambda;
    }
    
    public static decimal CalculateDecayConstant(decimal halfLife)
    {
        // λ = ln(2) / t₁/₂
        decimal ln2 = DecimalHelper.Log(2.0m);
        return ln2 / halfLife;
    }
}

// Usage
decimal initialPopulation = 1000.0m;
decimal growthRate = 0.03m; // 3% growth rate
decimal time = 5.0m; // 5 time units

decimal finalPopulation = ScientificCalculator.CalculateExponentialGrowth(
    initialPopulation, growthRate, time);
Console.WriteLine($"Population after {time} units: {finalPopulation:F0}");
```

### Blockchain and Cryptocurrency Calculations
```csharp
public class CryptoCalculator
{
    public static decimal ConvertWeiToEther(BigInteger weiAmount)
    {
        // 1 Ether = 10^18 Wei
        var weiPerEther = BigInteger.Pow(10, 18);
        
        // Safe conversion to decimal for precise calculations
        var weiDecimal = weiAmount.ToDecimal();
        var divisor = weiPerEther.ToDecimal();
        
        return weiDecimal / divisor;
    }
    
    public static BigInteger ConvertEtherToWei(decimal etherAmount)
    {
        var weiPerEther = BigInteger.Pow(10, 18);
        var weiPerEtherDecimal = weiPerEther.ToDecimal();
        
        var weiDecimal = etherAmount * weiPerEtherDecimal;
        
        // Convert back to BigInteger
        return new BigInteger(weiDecimal);
    }
    
    public static decimal CalculateGasCost(BigInteger gasUsed, BigInteger gasPrice)
    {
        // Gas cost in Wei
        var gasCostWei = gasUsed * gasPrice;
        
        // Convert to Ether for readability
        return ConvertWeiToEther(gasCostWei);
    }
}

// Usage
var weiAmount = BigInteger.Parse("1500000000000000000"); // 1.5 ETH in Wei
var etherAmount = CryptoCalculator.ConvertWeiToEther(weiAmount);
Console.WriteLine($"Wei amount: {weiAmount}");
Console.WriteLine($"Ether amount: {etherAmount}");

var gasUsed = new BigInteger(21000);
var gasPrice = new BigInteger(20000000000); // 20 Gwei
var gasCost = CryptoCalculator.CalculateGasCost(gasUsed, gasPrice);
Console.WriteLine($"Gas cost: {gasCost} ETH");
```

## Advanced Scenarios

### Precision-Critical Calculations
```csharp
public class PrecisionCalculator
{
    public static decimal CalculateWithMaxPrecision(decimal value1, decimal value2, 
        Func<decimal, decimal, decimal> operation)
    {
        // Ensure maximum precision for critical calculations
        try
        {
            var result = operation(value1, value2);
            
            // Validate result is within decimal range
            if (result < decimal.MinValue || result > decimal.MaxValue)
            {
                throw new OverflowException("Result exceeds decimal precision limits");
            }
            
            return result;
        }
        catch (OverflowException)
        {
            // Handle overflow by using BigInteger arithmetic
            var bigValue1 = new BigInteger(value1);
            var bigValue2 = new BigInteger(value2);
            
            // Perform operation with BigInteger (implementation depends on operation)
            var bigResult = bigValue1 + bigValue2; // Example: addition
            
            return bigResult.ToDecimal();
        }
    }
}
```

### Statistical Calculations
```csharp
public class StatisticalCalculator
{
    public static decimal CalculateStandardDeviation(IEnumerable<decimal> values)
    {
        var valuesList = values.ToList();
        var mean = valuesList.Average();
        
        var sumOfSquaredDifferences = valuesList
            .Select(value => DecimalHelper.Pow(value - mean, 2))
            .Sum();
        
        var variance = sumOfSquaredDifferences / valuesList.Count;
        return DecimalHelper.Pow(variance, 0.5m);
    }
    
    public static decimal CalculateNormalDistribution(decimal x, decimal mean, 
        decimal standardDeviation)
    {
        // Normal distribution formula: (1/σ√(2π)) * e^(-½((x-μ)/σ)²)
        var pi = 3.14159265358979323846m;
        var e = 2.71828182845904523536m;
        
        var coefficient = 1 / (standardDeviation * DecimalHelper.Pow(2 * pi, 0.5m));
        var exponent = -0.5m * DecimalHelper.Pow((x - mean) / standardDeviation, 2);
        
        return coefficient * DecimalHelper.Pow(e, exponent);
    }
}

// Usage
var dataSet = new[] { 1.2m, 2.3m, 3.4m, 4.5m, 5.6m, 6.7m, 7.8m, 8.9m };
var standardDeviation = StatisticalCalculator.CalculateStandardDeviation(dataSet);
Console.WriteLine($"Standard deviation: {standardDeviation:F4}");
```

## Performance Considerations

### Optimization Strategies
```csharp
public static class OptimizedMathOperations
{
    // Cache frequently used values
    private static readonly Dictionary<decimal, decimal> LogCache = new();
    private static readonly Dictionary<(decimal, decimal), decimal> PowCache = new();
    
    public static decimal CachedLog(decimal value)
    {
        if (LogCache.TryGetValue(value, out var cached))
            return cached;
        
        var result = value.Log();
        LogCache[value] = result;
        return result;
    }
    
    public static decimal CachedPow(decimal baseValue, decimal exponent)
    {
        var key = (baseValue, exponent);
        if (PowCache.TryGetValue(key, out var cached))
            return cached;
        
        var result = DecimalHelper.Pow(baseValue, exponent);
        PowCache[key] = result;
        return result;
    }
    
    // Batch operations for better performance
    public static decimal[] BatchPow(decimal[] bases, decimal exponent)
    {
        return bases.Select(b => DecimalHelper.Pow(b, exponent)).ToArray();
    }
}
```

### Memory Management
```csharp
public static class MemoryEfficientMath
{
    public static decimal ProcessLargeDataSet(IEnumerable<BigInteger> values, 
        Func<decimal, decimal> operation)
    {
        decimal result = 0;
        
        foreach (var value in values)
        {
            // Convert only when needed, avoiding large collections
            var decimalValue = value.ToDecimal();
            result += operation(decimalValue);
        }
        
        return result;
    }
}
```

## Best Practices

### 1. Error Handling
```csharp
public static class SafeMathOperations
{
    public static decimal SafePow(decimal baseValue, decimal exponent)
    {
        try
        {
            var result = DecimalHelper.Pow(baseValue, exponent);
            
            if (decimal.IsInfinity(result) || decimal.IsNaN(result))
            {
                throw new ArithmeticException("Power operation resulted in invalid value");
            }
            
            return result;
        }
        catch (OverflowException)
        {
            throw new ArithmeticException("Power operation resulted in overflow");
        }
    }
    
    public static decimal SafeLog(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Logarithm undefined for non-positive values");
        }
        
        return value.Log();
    }
}
```

### 2. Validation
```csharp
public static class ValidationExtensions
{
    public static bool IsValidForConversion(this BigInteger bigInteger, Type targetType)
    {
        return targetType switch
        {
            Type t when t == typeof(ulong) => bigInteger >= 0 && bigInteger <= ulong.MaxValue,
            Type t when t == typeof(decimal) => bigInteger >= (BigInteger)decimal.MinValue && 
                                               bigInteger <= (BigInteger)decimal.MaxValue,
            Type t when t == typeof(double) => true, // double can handle most BigInteger values
            _ => false
        };
    }
}
```

### 3. Performance Monitoring
```csharp
public static class InstrumentedMathOperations
{
    public static decimal MeasuredPow(decimal baseValue, decimal exponent)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = DecimalHelper.Pow(baseValue, exponent);
        stopwatch.Stop();
        
        Console.WriteLine($"Pow operation took: {stopwatch.ElapsedTicks} ticks");
        return result;
    }
}
```

### 4. Thread Safety
```csharp
public static class ThreadSafeMathOperations
{
    private static readonly ConcurrentDictionary<decimal, decimal> ThreadSafeLogCache = new();
    
    public static decimal ThreadSafeLog(decimal value)
    {
        return ThreadSafeLogCache.GetOrAdd(value, v => v.Log());
    }
}
```

---

[← Back to Main Documentation](../README.md)