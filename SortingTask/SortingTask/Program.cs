using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SortingTask
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int maxLenghtFileSizeInput = 6;
            int maxLenghtFileNameInput = 10;
            int minIndexForDataFile = 0;
            int maxIndexForDataFile = 500;
            int splitFileSizeMB = 50;
            int logicCores = Environment.ProcessorCount;
            string remoteUriDictionaryAddress = "https://github.com/SovietRay/SharpConsole-SortingTask/blob/develop/SortingTask/SortingTask/Dictionary_address.txt";
            string remoteUriDictionaryNames = "https://github.com/SovietRay/SharpConsole-SortingTask/blob/develop/SortingTask/SortingTask/Dictionary_names.txt";
            string remoteUriDictionarySurnames = "https://github.com/SovietRay/SharpConsole-SortingTask/blob/develop/SortingTask/SortingTask/Dictionary_surnames.txt";


            Stopwatch timerCreateFile = new Stopwatch();
            Stopwatch timerSortFile = new Stopwatch();
            Stopwatch timerSortFileByTwo = new Stopwatch();

            Console.OutputEncoding = Encoding.GetEncoding(1251);
            Console.InputEncoding = Encoding.GetEncoding(1251);

            try
            {
                          
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFileAsync(new Uri(remoteUriDictionaryAddress), "Dictionary_address.txt");
                    }
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFileAsync(new Uri(remoteUriDictionaryNames), "Dictionary_names.txt");
                    }
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFileAsync(new Uri(remoteUriDictionarySurnames), "Dictionary_surnames.txt");
                    }
                    Console.WriteLine("Download all Dictionary files");
                    await Task.WhenAll();
                    Console.Clear();
                }

                #region Enter file name and size
                Console.Write("Enter the file name: ");
                string fileName = ReadOnlyLetterAndNumber(maxLenghtFileNameInput);
                Console.Write("Enter the file size in megabytes: ");
                int fileSizeMB = ReadOnlyNumber(maxLenghtFileSizeInput);
                Console.Clear();
                #endregion

                #region Message
                Console.WriteLine("The number of processors " + "on this computer is {0}.", logicCores);
                Console.WriteLine("Is this 64-bit OS - {0}.", Environment.Is64BitOperatingSystem);
                #endregion

                timerCreateFile.Start();
                WorkWithDataFile.CreateFileWithRandomData(fileName, fileSizeMB, minIndexForDataFile, maxIndexForDataFile);
                timerCreateFile.Stop();


                timerSortFile.Start();
                await WorkWithDataFile.DataSortManyToOneAsync(fileName, splitFileSizeMB);
                timerSortFile.Stop();

                timerSortFileByTwo.Start();
                await WorkWithDataFile.DataSortTwoToOneAsync(fileName, splitFileSizeMB);
                timerSortFileByTwo.Stop();

                #region Message
                Console.WriteLine();
                Console.WriteLine($"Time to create the random file: {CheckSWTimer(timerCreateFile)}");
                Console.WriteLine($"Time to sort by merge all to one: {CheckSWTimer(timerSortFile)}");
                Console.WriteLine($"Time to sort by two to one: {CheckSWTimer(timerSortFileByTwo)}");
                #endregion

                ReadForContinue();
            }
            catch (Exception e)
            {
                #region Exception
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred in main code!");
                Console.WriteLine(e);
                Console.ResetColor();
                #endregion
                throw;
            }
        }

        private static string CheckSWTimer(Stopwatch timerSW)
        {
            TimeSpan ts = timerSW.Elapsed;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
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

        private static void ReadForContinue()
        {
            Console.Write("Please, type (y) for continue: ");
            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    break;
                }
            } while (true);
        }
    }
}



