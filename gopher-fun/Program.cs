using System;
using System.Threading.Tasks;
using GopherLib;
using GopherLib.Models;

namespace gopher_fun
{
    class Program
    {
        private const string GopherDirUrl = @"gopher://medialab.freaknet.org/1/";
        private const string GopherSearchUrl = @"gopher://gopher.floodgap.com/7/v2/vs#search term";
        private const string GopherDocumentUrl = @"gopher://gopher.floodgap.com/0/gopher/proxy";

        static void Main(string[] args)
        {
            //ExerciseSearch().Wait();
            ExerciseGet().Wait();
            Console.Error.WriteLine("--- done ----");
            Console.ReadKey();
        }

        static async Task ExerciseGet()
        {
            var gopherEntity = new GopherEntity(GopherDirUrl, null);
            var bytes = await GopherClient.GetGopherEntity(gopherEntity);
            var text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var menu = new GopherMenu(gopherEntity, text);
            Console.Write(menu);
        }

        static async Task ExerciseSearch()
        {
            var gopherEntity = new GopherEntity(GopherSearchUrl, null);
            var bytes = await GopherClient.GetGopherEntity(gopherEntity);
            var text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var menu = new GopherMenu(gopherEntity, text);
            Console.Write(menu);
        }

    }
}
