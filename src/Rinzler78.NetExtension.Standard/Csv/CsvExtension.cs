using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Rinzler78.NetExtension.Csv;

public static class CsvExtension
{
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
    /// <returns>An enumerable of deserialized records, or null if the operation fails.</returns>
    /// <exception cref="ArgumentException">Thrown when fileName is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied.</exception>
    /// <exception cref="CsvHelperException">Thrown when CSV parsing fails.</exception>
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
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"CSV file not found: {ex.Message}");
            return default;
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"CSV directory not found: {ex.Message}");
            return default;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to CSV file: {ex.Message}");
            return default;
        }
        catch (CsvHelperException ex)
        {
            Console.WriteLine($"CSV parsing error: {ex.Message}");
            return default;
        }
    }
}
