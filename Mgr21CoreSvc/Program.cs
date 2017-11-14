using System.ServiceProcess;

namespace Mgr21CoreSvc
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Mgr21CoreService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
