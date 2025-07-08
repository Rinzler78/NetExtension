# 📚 Collections & LINQ

[![Collections](https://img.shields.io/badge/Collections-Enhanced-blue?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic)
[![LINQ](https://img.shields.io/badge/LINQ-Extensions-purple?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)

Enhanced collection utilities and LINQ extensions for improved data manipulation and processing capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [Collection Helpers](#collection-helpers)
- [LINQ Extensions](#linq-extensions)
- [Array Extensions](#array-extensions)
- [Usage Examples](#usage-examples)
- [Performance Optimization](#performance-optimization)
- [Best Practices](#best-practices)

## Overview

The Collections utilities in Rinzler78.NetExtension provide enhanced collection manipulation capabilities, extended LINQ operations, and optimized array processing methods for efficient data handling.

## Core Classes

### `CollectionHelper`
Utility class for general collection operations and manipulations.

### `EnumerableExtension`
LINQ extension methods for enhanced enumerable operations.

### `ArrayExtension`
Specialized extension methods for array operations and manipulations.

## Collection Helpers

### General Collection Operations
```csharp
public static class CollectionHelper
{
    // Collection manipulation methods
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
    
    public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Remove(item);
        }
    }
    
    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection)
    {
        return collection == null || collection.Count == 0;
    }
    
    public static bool IsNotNullOrEmpty<T>(this ICollection<T>? collection)
    {
        return !collection.IsNullOrEmpty();
    }
}

// Usage
var list = new List<int> { 1, 2, 3 };
var itemsToAdd = new[] { 4, 5, 6 };
list.AddRange(itemsToAdd);

if (list.IsNotNullOrEmpty())
{
    Console.WriteLine($"List contains {list.Count} items");
}
```

### Collection Synchronization
```csharp
public static class ThreadSafeCollectionHelper
{
    public static void SafeAdd<T>(this ICollection<T> collection, T item, object lockObject)
    {
        lock (lockObject)
        {
            collection.Add(item);
        }
    }
    
    public static bool SafeRemove<T>(this ICollection<T> collection, T item, object lockObject)
    {
        lock (lockObject)
        {
            return collection.Remove(item);
        }
    }
    
    public static T[] SafeToArray<T>(this ICollection<T> collection, object lockObject)
    {
        lock (lockObject)
        {
            return collection.ToArray();
        }
    }
}

// Usage
var list = new List<string>();
var lockObject = new object();

// Thread-safe operations
list.SafeAdd("item1", lockObject);
list.SafeAdd("item2", lockObject);
var snapshot = list.SafeToArray(lockObject);
```

## LINQ Extensions

### Enhanced Enumerable Operations
```csharp
public static class EnumerableExtension
{
    // Batch processing
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        var batch = new List<T>(batchSize);
        
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }
        
        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
    
    // Distinct by property
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, 
        Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
            {
                yield return item;
            }
        }
    }
    
    // Safe enumeration
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        return source.Where(item => item != null)!;
    }
    
    // Conditional operations
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, 
        Func<T, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
    
    // Aggregation extensions
    public static decimal SafeSum<T>(this IEnumerable<T> source, Func<T, decimal> selector)
    {
        return source.Select(selector).DefaultIfEmpty(0).Sum();
    }
    
    public static double SafeAverage<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        var values = source.Select(selector).ToList();
        return values.Count > 0 ? values.Average() : 0;
    }
}

// Usage examples
var numbers = Enumerable.Range(1, 100);

// Batch processing
var batches = numbers.Batch(10);
foreach (var batch in batches)
{
    Console.WriteLine($"Batch: {string.Join(", ", batch)}");
}

// Distinct by property
var people = new[]
{
    new { Name = "John", Age = 30 },
    new { Name = "Jane", Age = 25 },
    new { Name = "John", Age = 35 } // Duplicate name
};

var distinctByName = people.DistinctBy(p => p.Name);

// Conditional filtering
var filtered = numbers.WhereIf(true, x => x % 2 == 0); // Only even numbers
```

### Advanced LINQ Operations
```csharp
public static class AdvancedEnumerableExtensions
{
    // Windowed operations
    public static IEnumerable<TResult> SelectWithPrevious<T, TResult>(
        this IEnumerable<T> source, 
        Func<T?, T, TResult> selector)
    {
        T? previous = default;
        bool isFirst = true;
        
        foreach (var current in source)
        {
            yield return selector(isFirst ? default : previous, current);
            previous = current;
            isFirst = false;
        }
    }
    
    // Sliding window
    public static IEnumerable<T[]> SlidingWindow<T>(this IEnumerable<T> source, int windowSize)
    {
        var window = new T[windowSize];
        int index = 0;
        
        foreach (var item in source)
        {
            window[index % windowSize] = item;
            index++;
            
            if (index >= windowSize)
            {
                yield return window.ToArray();
            }
        }
    }
    
    // Interleaving
    public static IEnumerable<T> Interleave<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        using var enum1 = first.GetEnumerator();
        using var enum2 = second.GetEnumerator();
        
        while (enum1.MoveNext() && enum2.MoveNext())
        {
            yield return enum1.Current;
            yield return enum2.Current;
        }
        
        while (enum1.MoveNext())
            yield return enum1.Current;
            
        while (enum2.MoveNext())
            yield return enum2.Current;
    }
    
    // Partitioning
    public static (IEnumerable<T> True, IEnumerable<T> False) Partition<T>(
        this IEnumerable<T> source, 
        Func<T, bool> predicate)
    {
        var trueItems = new List<T>();
        var falseItems = new List<T>();
        
        foreach (var item in source)
        {
            if (predicate(item))
                trueItems.Add(item);
            else
                falseItems.Add(item);
        }
        
        return (trueItems, falseItems);
    }
}

// Usage
var numbers = Enumerable.Range(1, 10);

// Windowed operations
var withPrevious = numbers.SelectWithPrevious((prev, curr) => 
    new { Previous = prev, Current = curr });

// Sliding window
var windows = numbers.SlidingWindow(3);

// Interleaving
var odds = new[] { 1, 3, 5, 7 };
var evens = new[] { 2, 4, 6, 8 };
var interleaved = odds.Interleave(evens);

// Partitioning
var (evenNumbers, oddNumbers) = numbers.Partition(x => x % 2 == 0);
```

## Array Extensions

### Array Manipulation
```csharp
public static class ArrayExtension
{
    // Safe array operations
    public static T? SafeGet<T>(this T[] array, int index)
    {
        return index >= 0 && index < array.Length ? array[index] : default;
    }
    
    public static bool SafeSet<T>(this T[] array, int index, T value)
    {
        if (index >= 0 && index < array.Length)
        {
            array[index] = value;
            return true;
        }
        return false;
    }
    
    // Array searching
    public static int[] FindAllIndices<T>(this T[] array, Func<T, bool> predicate)
    {
        var indices = new List<int>();
        
        for (int i = 0; i < array.Length; i++)
        {
            if (predicate(array[i]))
            {
                indices.Add(i);
            }
        }
        
        return indices.ToArray();
    }
    
    // Array transformation
    public static T[] Resize<T>(this T[] array, int newSize, T defaultValue = default)
    {
        var newArray = new T[newSize];
        
        for (int i = 0; i < Math.Min(array.Length, newSize); i++)
        {
            newArray[i] = array[i];
        }
        
        for (int i = array.Length; i < newSize; i++)
        {
            newArray[i] = defaultValue;
        }
        
        return newArray;
    }
    
    // Array statistics
    public static (T Min, T Max) GetMinMax<T>(this T[] array) where T : IComparable<T>
    {
        if (array.Length == 0)
            throw new InvalidOperationException("Array is empty");
        
        T min = array[0];
        T max = array[0];
        
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i].CompareTo(min) < 0)
                min = array[i];
            if (array[i].CompareTo(max) > 0)
                max = array[i];
        }
        
        return (min, max);
    }
}

// Usage
var numbers = new[] { 1, 2, 3, 4, 5 };

// Safe operations
var item = numbers.SafeGet(10); // Returns default value instead of exception
bool success = numbers.SafeSet(2, 99); // Returns false for invalid indices

// Finding indices
var evenIndices = numbers.FindAllIndices(x => x % 2 == 0);

// Resizing
var resized = numbers.Resize(10, -1);

// Statistics
var (min, max) = numbers.GetMinMax();
Console.WriteLine($"Min: {min}, Max: {max}");
```

### Performance-Optimized Array Operations
```csharp
public static class HighPerformanceArrayExtensions
{
    // Fast array copying
    public static T[] FastCopy<T>(this T[] source)
    {
        var destination = new T[source.Length];
        Array.Copy(source, destination, source.Length);
        return destination;
    }
    
    // Parallel operations
    public static T[] ParallelSelect<T, TResult>(this T[] source, Func<T, TResult> selector)
        where TResult : T
    {
        var result = new T[source.Length];
        
        Parallel.For(0, source.Length, i =>
        {
            result[i] = selector(source[i]);
        });
        
        return result;
    }
    
    // Memory-efficient operations
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        for (int i = 0; i < array.Length; i++)
        {
            action(array[i]);
        }
    }
    
    public static void ForEachWithIndex<T>(this T[] array, Action<T, int> action)
    {
        for (int i = 0; i < array.Length; i++)
        {
            action(array[i], i);
        }
    }
}

// Usage
var largeArray = Enumerable.Range(1, 1000000).ToArray();

// Fast copying
var copy = largeArray.FastCopy();

// Parallel processing
var doubled = largeArray.ParallelSelect(x => x * 2);

// Memory-efficient iteration
largeArray.ForEach(Console.WriteLine);
largeArray.ForEachWithIndex((value, index) => Console.WriteLine($"[{index}] = {value}"));
```

## Usage Examples

### Data Processing Pipeline
```csharp
public class DataProcessor
{
    public static IEnumerable<ProcessedData> ProcessData(IEnumerable<RawData> rawData)
    {
        return rawData
            .WhereNotNull()
            .DistinctBy(x => x.Id)
            .Batch(100)
            .SelectMany(batch => ProcessBatch(batch))
            .Where(result => result.IsValid);
    }
    
    private static IEnumerable<ProcessedData> ProcessBatch(IEnumerable<RawData> batch)
    {
        return batch.Select(item => new ProcessedData
        {
            Id = item.Id,
            ProcessedValue = item.Value * 2,
            IsValid = item.Value > 0
        });
    }
}

public class RawData
{
    public int Id { get; set; }
    public decimal Value { get; set; }
}

public class ProcessedData
{
    public int Id { get; set; }
    public decimal ProcessedValue { get; set; }
    public bool IsValid { get; set; }
}
```

### Statistical Analysis
```csharp
public class StatisticalAnalyzer
{
    public static StatisticalSummary Analyze(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        
        if (valuesList.IsNullOrEmpty())
        {
            return new StatisticalSummary();
        }
        
        var (min, max) = valuesList.ToArray().GetMinMax();
        var mean = valuesList.SafeAverage(x => x);
        var sum = valuesList.SafeSum(x => (decimal)x);
        
        var variance = valuesList
            .Select(x => Math.Pow(x - mean, 2))
            .SafeAverage(x => x);
        
        var standardDeviation = Math.Sqrt(variance);
        
        return new StatisticalSummary
        {
            Count = valuesList.Count,
            Min = min,
            Max = max,
            Mean = mean,
            Sum = sum,
            Variance = variance,
            StandardDeviation = standardDeviation
        };
    }
}

public class StatisticalSummary
{
    public int Count { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Mean { get; set; }
    public decimal Sum { get; set; }
    public double Variance { get; set; }
    public double StandardDeviation { get; set; }
}
```

### Real-time Data Stream Processing
```csharp
public class StreamProcessor<T>
{
    public static IEnumerable<TResult> ProcessStream<TResult>(
        IEnumerable<T> stream,
        int windowSize,
        Func<IEnumerable<T>, TResult> aggregator)
    {
        return stream
            .SlidingWindow(windowSize)
            .Select(window => aggregator(window));
    }
    
    public static IEnumerable<MovingAverage> CalculateMovingAverages(
        IEnumerable<decimal> values,
        int windowSize)
    {
        return ProcessStream(values, windowSize, window => new MovingAverage
        {
            Value = window.Average(),
            Timestamp = DateTime.Now
        });
    }
}

public class MovingAverage
{
    public decimal Value { get; set; }
    public DateTime Timestamp { get; set; }
}

// Usage
var priceStream = GetPriceStream(); // Some data source
var movingAverages = StreamProcessor<decimal>.CalculateMovingAverages(priceStream, 10);
```

## Performance Optimization

### Memory-Efficient Operations
```csharp
public static class MemoryOptimizedExtensions
{
    // Lazy evaluation
    public static IEnumerable<TResult> LazySelect<T, TResult>(
        this IEnumerable<T> source, 
        Func<T, TResult> selector)
    {
        foreach (var item in source)
        {
            yield return selector(item);
        }
    }
    
    // Streaming operations
    public static IEnumerable<T> StreamingDistinct<T>(this IEnumerable<T> source)
    {
        var seen = new HashSet<T>();
        
        foreach (var item in source)
        {
            if (seen.Add(item))
            {
                yield return item;
            }
        }
    }
    
    // Chunked processing
    public static IEnumerable<TResult> ProcessInChunks<T, TResult>(
        this IEnumerable<T> source,
        int chunkSize,
        Func<IEnumerable<T>, TResult> processor)
    {
        return source
            .Batch(chunkSize)
            .Select(processor);
    }
}
```

### Parallel Processing
```csharp
public static class ParallelExtensions
{
    public static IEnumerable<TResult> ParallelSelect<T, TResult>(
        this IEnumerable<T> source,
        Func<T, TResult> selector,
        int maxDegreeOfParallelism = -1)
    {
        var parallelQuery = source.AsParallel();
        
        if (maxDegreeOfParallelism > 0)
        {
            parallelQuery = parallelQuery.WithDegreeOfParallelism(maxDegreeOfParallelism);
        }
        
        return parallelQuery.Select(selector);
    }
    
    public static void ParallelForEach<T>(
        this IEnumerable<T> source,
        Action<T> action,
        int maxDegreeOfParallelism = -1)
    {
        var parallelOptions = new ParallelOptions();
        
        if (maxDegreeOfParallelism > 0)
        {
            parallelOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }
        
        Parallel.ForEach(source, parallelOptions, action);
    }
}
```

## Best Practices

### 1. Null Safety
```csharp
public static class NullSafeExtensions
{
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }
    
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        return source.Where(x => x.HasValue).Select(x => x!.Value);
    }
}
```

### 2. Performance Monitoring
```csharp
public static class InstrumentedExtensions
{
    public static IEnumerable<T> WithPerformanceLogging<T>(
        this IEnumerable<T> source,
        string operationName)
    {
        var stopwatch = Stopwatch.StartNew();
        int count = 0;
        
        foreach (var item in source)
        {
            count++;
            yield return item;
        }
        
        stopwatch.Stop();
        Console.WriteLine($"{operationName}: processed {count} items in {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

### 3. Error Handling
```csharp
public static class SafeExtensions
{
    public static IEnumerable<T> HandleExceptions<T>(
        this IEnumerable<T> source,
        Action<Exception> errorHandler)
    {
        foreach (var item in source)
        {
            T result;
            try
            {
                result = item;
            }
            catch (Exception ex)
            {
                errorHandler(ex);
                continue;
            }
            
            yield return result;
        }
    }
}
```

---

[← Back to Main Documentation](../README.md)