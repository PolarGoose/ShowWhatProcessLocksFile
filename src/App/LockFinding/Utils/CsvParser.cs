using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

internal static class CsvParser
{
    public static IEnumerable<string[]> Parse(string csv)
    {
        using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        using var parser = new TextFieldParser(csvStream) { TextFieldType = FieldType.Delimited, Delimiters = new[] { "," } };
        while (!parser.EndOfData)
        {
            yield return parser.ReadFields();
        }
    }
}
