using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TableCSVReader : MonoBehaviour
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };


    public static List<Dictionary<string, object>> Read(TextAsset data)
    {
        return ReadInternal(data, out string[] header, out string[] types);
    }

    public static List<Dictionary<string, object>> Read(TextAsset data, out string[] header, out string[] types)
    {
        return ReadInternal(data, out header, out types);
    }


    public static List<Dictionary<string, object>> ReadInternal(TextAsset data, out string[] header, out string[] types)
    {
        var list = new List<Dictionary<string, object>>();

        header = new string[] { };
        types = new string[] { };

        if (data == null)
        {
            Debug.LogError($"{nameof(TableCSVReader)} : TextAsset is Null");
            return null;
        }

        var lines = Regex.Split(data.text, LINE_SPLIT_RE);

        if (lines.Length <= 1)
            return list;

        header = Regex.Split(lines[0], SPLIT_RE);
        types = Regex.Split(lines[1], SPLIT_RE);

        for (var i = 2; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "")
                continue;

            var entry = new Dictionary<string, object>();

            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                object finalvalue = value;

                if (int.TryParse(value, out int intValue))
                {
                    finalvalue = intValue;
                }
                else if (float.TryParse(value, out float floatValue))
                {
                    finalvalue = floatValue;
                }

                entry[header[j]] = finalvalue;
            }

            list.Add(entry);
        }

        return list;
    }
}
