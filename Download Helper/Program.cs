using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download_Helper
{
    class Program
    {
        static void Main(string[] args)
        {

            string mainstr = @"
            *********************************************************
            *           PSNStuff Download Helper                    *
            *********************************************************
             ";

            if (args.Length == 0)
            {
                Console.WriteLine(mainstr);
                 Console.WriteLine("Arguments where incorectly passed \n\nPress any key to exit.");
                 Console.ReadKey();
            }
            else
            {
                Console.WriteLine(mainstr);
               
                //copy the data
                string fileName = args[0];
                string sourcePath = args[1];
                string targetPath = args[2];

                Console.WriteLine("Replacing '{0}'\n\nsourcepath: '{1}'\n\ntargetpath: '{2}'",fileName,sourcePath,targetPath);

                // Use Path class to manipulate file and directory paths.
                string sourceFile = sourcePath;
                string destFile = targetPath;

                // To copy a file to another location and 
                // overwrite the destination file if it already exists.
                System.IO.File.Copy(sourceFile, destFile, true);

                Console.WriteLine("Done... starting new version of psnstuff");

                // Keep console window open in debug mode.
                //Console.WriteLine("Press any key to exit.");
                // Console.ReadKey();
                if (System.IO.File.Exists("psnstuff.exe"))
                {
                    System.Diagnostics.Process.Start("psnstuff.exe");
                }
            }
        }
    }

}
