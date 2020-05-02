using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SortingTask
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName1 = "Data";

            try
            {
                int amountLines = 100000000;
                int amountFiles = 5;
                var timer = DateTime.Now;
                Console.WriteLine(timer + " - время начала");
                Console.WriteLine();

                WorkWithDataFile.CreateFileWithData(fileName1, amountLines, 0, 500, 10);
                Console.WriteLine(Math.Round((DateTime.Now - timer).TotalSeconds, 2) + " сек." + " - время создания файла");
                
                timer = DateTime.Now;
                WorkWithDataFile.MergeManyFiles(fileName1, amountFiles);
                Console.WriteLine(Math.Round((DateTime.Now - timer).TotalSeconds, 2) + " сек." + " - время на разбивку и сортировку разбивки");

                timer = DateTime.Now;
                Console.WriteLine();
                Console.WriteLine(timer + " - время окончания");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
        }
    }
}



