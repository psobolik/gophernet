using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopherLib.Models
{
    public class GopherMenu : GopherContentBase
    {
        public IEnumerable<GopherEntity> GopherEntities { get; set; }

        public GopherMenu(GopherEntity gopherEntity, byte[] bytes)
            : this(gopherEntity, Encoding.UTF8.GetString(bytes, 0, bytes.Length))
        { }

        public GopherMenu(GopherEntity gopherEntity, string text)
            : this(gopherEntity, text.Split(new string[] { GopherEol }, StringSplitOptions.RemoveEmptyEntries))
        { }

        private GopherMenu(GopherEntity gopherEntity, IEnumerable<string> lines)
        {
            GopherEntity = gopherEntity;
            var gopherEntities = new List<GopherEntity>();

            foreach (var line in lines)
            {
                if (line.Length == 1 && line[0] == GopherEof)
                {
                    break;
                }
                gopherEntities.Add(new GopherEntity(line));
            }
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

}
