using System;
using System.Threading.Tasks.Dataflow;
using LobbyEr;

static class Program
{
    public static Server server;
    static void Main ()
    {
        SQL sql = new SQL();
        server = new();
    }
}
