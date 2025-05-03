namespace GopherLib.Models;

public class GopherContentBase
{
    protected const string GopherEol = "\r\n";
    protected const char GopherEof = '.';

    public GopherEntity GopherEntity { get; set; }
}