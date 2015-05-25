using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebGrabber.Launcher
{
    class Program
    {
        static void Main()
        {
            Log.Out = Log.OutHandlers.ConsoleHandler.DtThTyLvMs_Out;

            var getPages = new Rabota_E1_ru.Container();

            getPages.Run();

            Console.ReadLine();
        }
    }
}
