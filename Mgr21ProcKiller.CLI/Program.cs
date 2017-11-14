using System;

namespace Mgr21ProcKiller.CLI
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Handler().Run();
        }
    }
}
