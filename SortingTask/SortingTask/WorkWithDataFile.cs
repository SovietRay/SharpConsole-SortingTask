using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SortingTask
{
    class WorkWithDataFile
    {
        static string writePath = Directory.GetCurrentDirectory();
        static Random random = new Random();

        public static void CreateFileWithData(string fileName, int sizeInMB, int minId, int maxId)
        {
            try
            {
                string pathToFile = writePath + @"\" + fileName + ".txt";
                int fileStreamBuffSize = 4096 * 4;
                long sizeByte = (long)sizeInMB * 1024 * 1024;

                Stopwatch timerSW = Stopwatch.StartNew();
                List<string> dictionaryNames = new List<string>();
                List<string> dictionarySurnames = new List<string>();
                List<string> dictionaryAddress = new List<string>();

                Console.WriteLine($"The program began creating new unsorted file.");
                Console.WriteLine($"Path: {pathToFile}");
                Console.WriteLine($"...");

                CreateCollectionFromFile("Dictionary_names", ref dictionaryNames);
                CreateCollectionFromFile("Dictionary_surnames", ref dictionarySurnames);
                CreateCollectionFromFile("Dictionary_address", ref dictionaryAddress);

                using (var sw = new FileStream(pathToFile, 
                    FileMode.Create, FileAccess.Write, FileShare.Write, fileStreamBuffSize))
                {
                    var currentFileSize = sw.Length;
                    var bufferSize = 2000 * 16;


                    while (currentFileSize < sizeByte)
                    {
                        byte[] bytes = Encoding.Default.GetBytes(
                            GenerateContent(ref dictionaryNames, ref dictionarySurnames, ref dictionaryAddress,
                            minId, maxId, bufferSize)
                            );
                        sw.Write(bytes, 0, bytes.Length);
                        currentFileSize = sw.Length;
                    }

                }
                FileInfo fileInf = new FileInfo(pathToFile);

                Console.WriteLine($"Done!");
                Console.WriteLine($"Total size: {fileName}.txt ~ {Math.Round(fileInf.Length / 1024f / 1024f, 3)}MB");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while creating new unsorted file!");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }

        private static string GenerateContent(
            ref List<string> dictionaryNames,
            ref List<string> dictionarySurnames,
            ref List<string> dictionaryAddress, 
            int minId, int maxId, int bufferSize)
        {          
            StringBuilder content = new StringBuilder(bufferSize);

            while (content.Length < bufferSize)
            {
                content.Append(random.Next(minId, maxId));
                content.Append(". ");
                content.Append(dictionaryNames[random.Next(0, dictionaryNames.Count)]);
                content.Append(" ");
                content.Append(dictionarySurnames[random.Next(0, dictionarySurnames.Count)]);
                content.Append(", ");
                content.Append(dictionaryAddress[random.Next(0, dictionaryAddress.Count)]);
                content.AppendLine();
            }
            return content.ToString();
        }

        public static async Task DataSortAsync(string fileName, int splitFileSize)
        {
            int amountFiles = FileSplit(fileName, splitFileSize);
            await SortAllSplitFilesAsync(fileName, amountFiles);
            //MergeManyFiles(fileName, amountFiles);
        }

        public static int FileSplit(string fileName, int splitFileSize)
        {

            string pathToFile = writePath + @"\" + fileName + ".txt";
            int fileStreamBuffSize = 4096 * 4;
            long bufferContent = (long)splitFileSize * 1024 * 1024;
            int firstFileNumber = 1;

            Stopwatch timerSW = Stopwatch.StartNew();

            try
            {
                Console.WriteLine($"Start splitting a file {fileName}.txt into parts ~{splitFileSize}MB each.");
                Console.WriteLine($"...");

                using (StreamReader sr = new StreamReader(pathToFile, Encoding.Default))
                {
                    string line;

                    StringBuilder content = new StringBuilder();

                    while ((line = sr.ReadLine()) != null)
                    {           
                        if (content.Length <= bufferContent)
                        {
                            content.AppendLine(line);
                        }
                        else
                        {
                            using (var sw = new FileStream(writePath + @"\" + fileName + "_" + firstFileNumber + ".txt",
                                FileMode.Create, FileAccess.Write, FileShare.Write, fileStreamBuffSize))
                            {
                                byte[] bytes = Encoding.Default.GetBytes(content.AppendLine(line).ToString());
                                sw.Write(bytes, 0, bytes.Length);
                                content.Clear();
                                firstFileNumber ++;
                            }
                        }
                    }

                    if (content!=null)
                    {
                        using (var sw = new FileStream(writePath + @"\" + fileName + "_" + (firstFileNumber - 1) + ".txt",
                            FileMode.Append, FileAccess.Write, FileShare.Write, fileStreamBuffSize))
                        {
                            byte[] bytes = Encoding.Default.GetBytes(content.ToString());
                            sw.Write(bytes, 0, bytes.Length);
                            content.Clear();
                        }
                    }
                }

                Console.WriteLine($"Done!");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                Console.WriteLine();
                Console.ReadLine();
                return (firstFileNumber - 1);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while splitting the file!");
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task SortAllSplitFilesAsync(string fileName, int amountFiles)
        {
            var timer = DateTime.Now;
            List<Task> tasks = new List<Task>();

            Console.WriteLine("Начало сортировки файлов.");

            for (int i = 1; i <= amountFiles; i++)
            {
                string file = $"{fileName}_{i}";
                Console.WriteLine(file);
                //Task sortTask = Task.Run(() => SortFile(file));
                SortFile(file);
                //tasks.Add(sortTask);
            }

            //await Task.WhenAll(tasks.ToArray());
            Console.ReadLine();
            Console.WriteLine($"Окончание сортировки всех файлов: " +
                $"{Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
            Console.WriteLine();
        }

        public static void SortFile(string fileName)
        {
            try
            {
                Console.WriteLine($"Начал сортировку {fileName}");
                var timer = DateTime.Now;
                string path = writePath + @"\" + fileName + ".txt";

                List<Data> data = new List<Data>();
                FileInfo fileInf = new FileInfo(path);

                CreateDataCollectionFromFile(fileName, ref data);
                SimpleDataSort(ref data);
                SaveDataToFile(fileName, ref data);

                if (fileInf.Exists)
                {
                    Console.WriteLine($"Размер {fileName}: {Math.Round(fileInf.Length / 1048576.0, 3)}МБ. Время на сортировку: {Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
                }

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка при сортировке файла.");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }

        public static void CreateDataCollectionFromFile(string fileName, ref List<Data> data)
        {
            try
            {
                data.Clear();
                using (StreamReader sr = new StreamReader(writePath + @"\" + fileName + ".txt", Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        char ch = '.';
                        int indexOfChar = line.IndexOf(ch);
                        int number;
                        int.TryParse(line.Substring(0, indexOfChar), out number);
                        string text = line.Substring(indexOfChar + 2);
                        data.Add(new Data() { _id = number, _text = text });
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка при создании коллекции типа - Data.");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }
        public static void CreateCollectionFromFile(string fileName, ref List<string> data)
        {
            try
            {
                data.Clear();
                using (StreamReader sr = new StreamReader(writePath + @"\" + fileName + ".txt", Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        data.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при создании коллекции.");
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
        }

        public static void SimpleDataSort(ref List<Data> data)
        {
            data.OrderBy(order => order._text).ThenBy(order => order._id);
        }

        public static void SaveDataToFile(string fileName, ref List<Data> data)
        {
            string writePath = Directory.GetCurrentDirectory();
            string content;
            byte[] bytes;

            try
            {
                using (var sw = new FileStream(
                    writePath + @"\" + fileName + ".txt",
                    FileMode.Create, FileAccess.Write, FileShare.Write, 4096 * 4))
                {
                    foreach (var item in data)
                    {
                        content = item._id + ". " + item._text; //Добавить окончание с переходам в стринг, где дата, сохранить сразу все в байты и записывать куском в файл. Линкк создает копию... сохранить все в масси и сразу отсортировать
                        bytes = Encoding.Default.GetBytes(content + "\r\n");
                        sw.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка при сохранении Data в файл.");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }

        //public static void createsortedlistfromfiles(string filename, int maxfiles, ref list<data> data)
        //{
        //    for (int i = 1; i <= maxfiles; i++)
        //    {
        //        using (streamreader sr = new streamreader(writepath + @"\" + filename + "_" + i + ".txt", encoding.default))
        //        {
        //            string line;
        //            if ((line = sr.readline()) != null)
        //            {
        //                char ch = '.';
        //                int indexofchar = line.indexof(ch);
        //                int number;
        //                int.tryparse(line.substring(0, indexofchar), out number);
        //                string text = line.substring(indexofchar + 2);
        //                data.add(new data() { _id = number, _text = text, _filename = filename + "_" + i});
        //            }   
        //        }
        //    }
        //    data = data.orderby(order => order._text).thenby(order => order._id).tolist();
        //}
        //public static void MergeManyFiles (string fileName, int amountFiles)
        //{
        //    string writePath = Directory.GetCurrentDirectory();
        //    string line;

        //    var timer = DateTime.Now;
        //    List<Data> data = new List<Data>();

        //    try
        //    {
        //        Console.WriteLine("Начало сборки файла.");

        //        Dictionary<string, StreamReader> streamReaders = new Dictionary<string, StreamReader>(amountFiles);
        //        for (int i = 1; i <= amountFiles; i++)
        //        {
        //            string srFileName = fileName + "_" + i;
        //            StreamReader sr = new StreamReader(writePath + @"\" + srFileName + ".txt", Encoding.Default);
        //            streamReaders.Add(srFileName, sr);
        //        }

        //        foreach (var item in streamReaders)
        //        {
        //            if ((line = item.Value.ReadLine()) != null)
        //            {
        //                char ch = '.';
        //                int indexOfChar = line.IndexOf(ch);
        //                int number;
        //                int.TryParse(line.Substring(0, indexOfChar), out number);
        //                string text = line.Substring(indexOfChar + 2);
        //                data.Add(new Data() { _id = number, _text = text, _fileName = item.Key });
        //            }
        //        }

        //        StreamWriter sw = new StreamWriter(writePath + @"\" + fileName + "_out.txt", false, Encoding.Default);

        //        while (data.Count > 0)
        //        {
        //            data.OrderBy(order => order._text).ThenBy(order => order._id);
        //            sw.WriteLine(data[0]._id + ". " + data[0]._text);
        //            if ((line = streamReaders[data[0]._fileName].ReadLine()) != null)
        //            {
        //                char ch = '.';
        //                int indexOfChar = line.IndexOf(ch);
        //                int number;
        //                int.TryParse(line.Substring(0, indexOfChar), out number);
        //                string text = line.Substring(indexOfChar + 2);
        //                data[0]._id = number;
        //                data[0]._text = text;
        //            }
        //            else
        //                data.RemoveAt(0);
        //        }

        //        foreach (var item in streamReaders)
        //        {
        //            item.Value.Dispose();
        //        }
        //        sw.Dispose();

        //        Console.WriteLine($"Окончание сборки файла: { Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
        //        Console.WriteLine();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine("Ошибка при сборке файла.");
        //        Console.WriteLine(e);
        //        Console.ResetColor();
        //        throw;
        //    }
        //}



        private static string CheckSWTimer(Stopwatch timerSW)
        {
            TimeSpan ts = timerSW.Elapsed;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }
    }

    class Data
    {
        public int _id { get; set; }
        public string _text { get; set; }
        //public string _fileName { get; set; }
    }
}