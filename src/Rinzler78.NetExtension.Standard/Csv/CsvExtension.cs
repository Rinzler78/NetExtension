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
    public static Task<IEnumerable<ReturnType>> LoadCsvAsync<ReturnType>(this string fileName, char separator = ';')
    {
        return Task.Run(() => fileName.LoadCsv<ReturnType>(separator));
    }

    public static IEnumerable<ReturnType> LoadCsv<ReturnType>(this string fileName, char separator = ';',
        bool hasHeaderRecord = false)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = separator.ToString(),
                HasHeaderRecord = hasHeaderRecord,
            };

            var sourcePath = fileName;

            using (var reader = new StreamReader(sourcePath))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<ReturnType>().ToArray();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return default;
    }
}