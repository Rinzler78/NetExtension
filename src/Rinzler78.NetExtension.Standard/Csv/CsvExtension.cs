using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Rinzler78.NetExtension.Csv;

/// <summary>
/// Provides extension methods for loading and parsing CSV files.
/// </summary>
public static class CsvExtension
{
    /// <summary>
    /// Asynchronously loads CSV data from a file and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="ReturnType">The type to deserialize CSV records to.</typeparam>
    /// <param name="fileName">The path to the CSV file.</param>
    /// <param name="separator">The CSV field separator character.</param>
    /// <returns>A task containing an enumerable of deserialized records, or null on failure.</returns>
    public static Task<IEnumerable<ReturnType>?> LoadCsvAsync<ReturnType>(this string fileName, char separator = ';')
    {
        return Task.Run(() => fileName.LoadCsv<ReturnType>(separator));
    }

    /// <summary>
    /// Loads CSV data from a file and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="ReturnType">The type to deserialize CSV records to.</typeparam>
    /// <param name="fileName">The path to the CSV file.</param>
    /// <param name="separator">The CSV field separator character.</param>
    /// <param name="hasHeaderRecord">Whether the CSV file has a header record.</param>
    /// <returns>An enumerable of deserialized records, or null if the file is missing, inaccessible, or malformed.</returns>
    /// <exception cref="ArgumentException">Thrown when fileName is null or empty.</exception>
    public static IEnumerable<ReturnType>? LoadCsv<ReturnType>(this string fileName, char separator = ';',
        bool hasHeaderRecord = false)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        if (Directory.Exists(fileName))
            return default;

        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = separator.ToString(),
                HasHeaderRecord = hasHeaderRecord
            };

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<ReturnType>().ToArray();
            }
        }
        catch (FileNotFoundException)
        {
            return default;
        }
        catch (DirectoryNotFoundException)
        {
            return default;
        }
        catch (UnauthorizedAccessException)
        {
            return default;
        }
        catch (CsvHelperException)
        {
            return default;
        }
    }
}
