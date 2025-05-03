using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GopherLib.Models;

public class GopherMenu : GopherContentBase
{
    public IEnumerable<GopherEntity> GopherEntities { get; set; }

    public GopherMenu(GopherEntity gopherEntity, byte[] bytes)
        : this(gopherEntity, Encoding.UTF8.GetString(bytes, 0, bytes.Length))
    {
    }

    public GopherMenu(GopherEntity gopherEntity, string text)
        : this(gopherEntity, text.Split([GopherEol], StringSplitOptions.RemoveEmptyEntries))
    {
    }

    private GopherMenu(GopherEntity gopherEntity, IEnumerable<string> lines)
    {
        GopherEntity = gopherEntity;
        var gopherEntities = lines.TakeWhile(line => line.Length != 1 || line[0] != GopherEof)
            .Select(line => new GopherEntity(line)).ToList();

        GopherEntities = gopherEntities;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var gopherEntity in GopherEntities)
        {
            sb.Append(gopherEntity);
            sb.Append(GopherEol);
        }

        sb.AppendLine(GopherEof.ToString());
        return sb.ToString();
    }
}