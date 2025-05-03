using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global

namespace GopherLib.Models;

public class GopherDocument : GopherContentBase
{
    public IEnumerable<string> Lines { get; set; }

    public GopherDocument(GopherEntity gopherEntity, byte[] bytes)
        : this(gopherEntity, Encoding.UTF8.GetString(bytes, 0, bytes.Length))
    { }

    public GopherDocument(GopherEntity gopherEntity, string text)
    {
        GopherEntity = gopherEntity;

        var lineList = new List<string>();
        foreach (var line in text.Split([GopherEol], StringSplitOptions.None))
        {
            if (line.StartsWith(GopherEof))
            {
                if (line.Length == 1)
                {
                    // Single dot ends text
                    break;
                }
                // Store the line without the leading dot
                lineList.Add(line[1..]);
            }
            else
            {
                // Store the entire line
                lineList.Add(line);
            }
        }
        Lines = lineList;
    }

    public override string ToString()
    {
        return string.Join(GopherEol, Lines);
    }
}