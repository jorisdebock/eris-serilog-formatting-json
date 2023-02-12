using Serilog.Events;
using Serilog.Formatting.Json;

using System;
using System.IO;

namespace Eris.Serilog.Formatting.Json;

public sealed class JsonValueFormatter2 : JsonValueFormatter
{
    public JsonValueFormatter2()
        : base(null)
    {
    }

    // overwrite for camelCase: true
    protected override bool VisitStructureValue(TextWriter state, StructureValue structure)
    {
        state.Write('{');

        var delim = "";

        for (var i = 0; i < structure.Properties.Count; i++)
        {
            state.Write(delim);
            delim = ",";
            var prop = structure.Properties[i];
            WriteQuotedJsonString(prop.Name, state, camelCase: true);
            state.Write(':');
            Visit(state, prop.Value);
        }

        state.Write('}');
        return false;
    }

    // overwrite for camelCase: true
    protected override bool VisitDictionaryValue(TextWriter state, DictionaryValue dictionary)
    {
        state.Write('{');
        var delim = "";
        foreach (var element in dictionary.Elements)
        {
            state.Write(delim);
            delim = ",";
            WriteQuotedJsonString((element.Key.Value ?? "null").ToString()!, state, camelCase: true);
            state.Write(':');
            Visit(state, element.Value);
        }
        state.Write('}');
        return false;
    }

    // overwrite for camelCase: true & span
    public new void WriteQuotedJsonString(string str, TextWriter output)
    {
        WriteQuotedJsonString(str, output, camelCase: false);
    }

    // overwrite for camelCase: true & span
    public void WriteQuotedJsonString(string str, TextWriter output, bool camelCase)
    {
        output.Write('"');

        int num = 0;
        bool flag = false;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (camelCase && i == 0)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    output.Write((char)(c + 32));

                    flag = true;
                    num = i + 1;
                    continue;
                }
            }
            if (c >= ' ' && c != '\\' && c != '"')
            {
                continue;
            }

            flag = true;
            output.Write(str.AsSpan(num, i - num));
            num = i + 1;
            switch (c)
            {
                case '"':
                    output.Write("\\\"");
                    continue;
                case '\\':
                    output.Write("\\\\");
                    continue;
                case '\n':
                    output.Write("\\n");
                    continue;
                case '\r':
                    output.Write("\\r");
                    continue;
                case '\f':
                    output.Write("\\f");
                    continue;
                case '\t':
                    output.Write("\\t");
                    continue;
            }

            output.Write("\\u");
            output.Write(((byte)c).ToString("X4"));
        }

        if (flag)
        {
            if (num != str.Length)
            {
                output.Write(str.AsSpan(num));
            }
        }
        else
        {
            output.Write(str);
        }

        output.Write('"');
    }
}