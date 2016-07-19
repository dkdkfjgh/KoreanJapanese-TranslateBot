using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


using Translator;

namespace dkdkfjghbot
{
    class Program
    {
        
        static void Main(string[] args)
        {
            try
            {
                dkdkfjghbot BOT = new dkdkfjghbot();


                Task.Run(async () => { await BOT.RunBot(); }).Wait();
           }
           catch(Exception e)
           {
               Console.WriteLine(e.StackTrace);
           }

        }
        


    }
}



