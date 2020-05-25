using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
            int fileSizeMB = 100;
            int maxLenghtFileSizeInput = 6;
            int maxLenghtFileNameInput = 10;
            int minIndexForDataFile = 0;
            int maxIndexForDataFile = 500;
            int splitFileSizeMB = 100;

            Console.OutputEncoding = Encoding.GetEncoding(1251);
            Console.InputEncoding = Encoding.GetEncoding(1251);

            try
            {
                //кодировка
                //пробелы между методами
                //ввод данных, проверка размера файла до достижения определенного размера
                //разбивка на несколько файлов исходя из размера
                //проверка доступной памяти и количество процессоров для их загрузки, убрать имя файла из сортировки
                //своя сортировка, отказ от линкю, через массив и свой сомпаунд?
                //запуск программы как 64
                //работа со строками через стрингер stringbuilder c#
                //заменить таймер
                //

                #region Ввод данных
                Console.Write("Enter the file name: ");
                fileName = ReadOnlyLetterAndNumber(maxLenghtFileNameInput);
                Console.Write("Enter the file size in megabytes: ");
                fileSizeMB = ReadOnlyNumber(maxLenghtFileSizeInput);
                Console.Clear();
                #endregion

                var timerSW = Stopwatch.StartNew();
               
                var timer = DateTime.Now;
                Console.WriteLine(timer + " - program start time.");
                Console.WriteLine();
                
                WorkWithDataFile.CreateFileWithData(fileName, fileSizeMB, minIndexForDataFile, maxIndexForDataFile);
                await WorkWithDataFile.DataSortAsync(fileName, splitFileSizeMB);

                Console.WriteLine();
                //Console.WriteLine($"{DateTime.Now} - время окончания выполнения. Итого прошло без учета времени на ввод данных - {Math.Round((DateTime.Now - timer).TotalSeconds - readLineSec, 2)} сек.");
                CheckSWTimer(timerSW);
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

        private static void CheckSWTimer(Stopwatch timerSW)
        {
            TimeSpan ts = timerSW.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }

        private static int ReadOnlyNumber(int maxLength)
        {
            StringBuilder input = new StringBuilder();

            Console.SetCursorPosition(Console.CursorLeft + maxLength, Console.CursorTop);
            Console.Write("MB");
            Console.SetCursorPosition(Console.CursorLeft - maxLength - 2, Console.CursorTop);

            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (char.IsNumber(keyInfo.KeyChar) && (input.Length <= maxLength - 1))
                {
                    input.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                    
                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (keyInfo.Key == ConsoleKey.Backspace && input.Length!=0)
                {
                    input = input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b");
                }
            } while (true);
            return int.Parse(input.ToString());
        }

        private static string ReadOnlyLetterAndNumber(int maxLength)
        {
            StringBuilder input = new StringBuilder();

            Console.SetCursorPosition(Console.CursorLeft + maxLength, Console.CursorTop);
            Console.Write(".txt");
            Console.SetCursorPosition(Console.CursorLeft - maxLength - 4, Console.CursorTop);

            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (char.IsLetterOrDigit(keyInfo.KeyChar) && (input.Length <= maxLength - 1))
                {
                    input.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);

                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (keyInfo.Key == ConsoleKey.Backspace && input.Length != 0)
                {
                    input = input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b");
                } 
            } while (true);
            return input.ToString();
        }
    }
}



