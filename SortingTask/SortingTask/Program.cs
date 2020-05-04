using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SortingTask
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string fileName = "Data";
            int amountLines = 20;
            int amountFiles = 5;

            try
            {
                #region Ввод данных
                Console.Write("Введите название файла: ");
                fileName = Console.ReadLine();
                Console.Write("Введите количество строк в файле: ");
                amountLines = int.Parse(Console.ReadLine());
                Console.Clear();
                #endregion

                var timer = DateTime.Now;
                Console.WriteLine(timer + " - время начала выполнения.");
                Console.WriteLine();

                WorkWithDataFile.CreateFileWithData(fileName, amountLines, 0, 500);

                #region Ввод количества файлов
                DateTime readLineTimer = DateTime.Now;
                Console.Write("Введите количество файлов, на которые будет разбит файл: ");
                amountFiles = int.Parse(Console.ReadLine());
                var readLineSec = (DateTime.Now - readLineTimer).TotalSeconds;
                #endregion

                await WorkWithDataFile.DataSortAsync(fileName, amountFiles);

                Console.WriteLine();
                Console.WriteLine($"{DateTime.Now} - время окончания выполнения. Итого прошло без учета времени на ввод данных - {Math.Round((DateTime.Now - timer).TotalSeconds - readLineSec, 2)} сек.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка в Main.");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }
    }
}



