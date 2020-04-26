using System.Collections.Generic;
using System.Text;

public static class CsvFileReader
{
    public static List<List<string>> Parse(string data)
    {
        var lines = data.Split('\n');
        var list = new List<List<string>>();
        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            builder.Clear();
            var comma = false;
            var array = line.ToCharArray();
            var values = new List<string>();
            var length = array.Length;
            var index = 0;
            while (index < length)
            {
                var item = array[index++];
                switch (item)
                {
                    case ',':
                        if (comma)
                        {
                            builder.Append(item);
                        }
                        else
                        {
                            values.Add(builder.ToString());
                            builder.Clear();
                        }
                        break;
                    case '"':
                        if (comma && index < length && array[index] == '"')
                        {
                            builder.Append(item);
                            index++;
                        }
                        else
                        {
                            comma = !comma;
                        }
                        break;
                    default:
                        builder.Append(item);
                        break;
                }
            }
            values.Add(builder.ToString());
            if (values.Count == 0) continue;
            list.Add(values);
        }
        return list;
    }
}