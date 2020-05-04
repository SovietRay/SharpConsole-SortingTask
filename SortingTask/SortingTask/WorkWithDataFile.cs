using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static void CreateFileWithData(string fileName, int numberOfLines, int minId, int maxId)
        {
            try
            {
                string path = writePath + @"\" + fileName + ".txt";
                string content;
                string content_additional;
                byte[] bytes;

                var timer = DateTime.Now;
                FileInfo fileInf = new FileInfo(path);
                Random random = new Random();

                List<string> dictionary_names = new List<string>();
                List<string> dictionary_surnames = new List<string>();
                List<string> dictionary_cities = new List<string>();

                Console.WriteLine($"Начало создания файла.");

                CreateCollectionFromFile("Dictionary_names", ref dictionary_names);
                CreateCollectionFromFile("Dictionary_surnames", ref dictionary_surnames);
                CreateCollectionFromFile("Dictionary_cities", ref dictionary_cities);

                using (var sw = new FileStream(
                    writePath + @"\" + fileName + ".txt", 
                    FileMode.Create, FileAccess.Write, FileShare.Write, 4096*4))
                {

                    for (int i = 0; i < numberOfLines; i++)
                    {
                        content_additional = $"{dictionary_names[random.Next(0, dictionary_names.Count)]} " +
                            $"{dictionary_surnames[random.Next(0, dictionary_surnames.Count)]} " +
                            $"from {dictionary_cities[random.Next(0, dictionary_cities.Count)]}";
                        content = random.Next(minId, maxId) + ". " + content_additional;
                        bytes = Encoding.Default.GetBytes(content + "\r\n");
                        sw.Write(bytes, 0, bytes.Length);
                    }
                }

                Console.WriteLine($"Окончание создания файла. " +
                    $"Время на создание ({fileName} ~ {Math.Round(fileInf.Length / 1048576.0, 3)}МБ.): " +
                    $"{Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка при создании файла.");
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
            data = data.OrderBy(order => order._text).ThenBy(order => order._id).ToList();
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
                        content = item._id + ". " + item._text;
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

        public static async Task SortAllSplitFilesAsync(string fileName, int maxFiles)
        {
            var timer = DateTime.Now;

            Console.WriteLine();
            Console.WriteLine("Начало сортировки файлов.");

            List<Task> tasks = new List<Task>();

            for (int i = 1; i < maxFiles; i++)
            {
                //await tasks.Add(Task.Run(() => SortFile(fileName + "_" + i)));
                SortFile(fileName + "_" + i);
            }

            //Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Окончание сортировки всех файлов: " +
                $"{Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
            Console.WriteLine();
        }
        public static void SimpleMerge(ref Data data1, ref Data data2)
        {

        }
        public static void FileSplit(string name, int maxFiles)
        {
            byte[] bytes;
            string line;
            int m = 1;

            var timer = DateTime.Now;

            try
            {
                Console.WriteLine();
                Console.WriteLine($"Начало разбивки файла ({name}) на {maxFiles} частей.");

                using (StreamReader sr = new StreamReader(writePath + @"\" + name + ".txt", Encoding.Default))
                {
                    Dictionary<int, FileStream> streamWriters = new Dictionary<int, FileStream>(maxFiles);
                    for (int i = 1; i <= maxFiles; i++)
                    {
                        var sw = new FileStream(writePath + @"\" + name + "_" + i + ".txt", FileMode.Create, FileAccess.Write, FileShare.Write, 4096 * 4);
                        streamWriters.Add(i, sw);
                    }

                    while ((line = sr.ReadLine()) != null)
                    {
                        var sw = streamWriters[m];
                        bytes = Encoding.Default.GetBytes(line + "\r\n");
                        sw.Write(bytes, 0, bytes.Length);
                        m = (m < maxFiles) ? m + 1 : 1;
                    }

                    for (int i = 1; i <= maxFiles; i++)
                    {
                        streamWriters[i].Dispose();
                    }
                }

                Console.WriteLine($"Окончание разбивки: {Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при разбивке файла.");
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
        }
        public static void CreateSortedListFromFiles(string fileName, int maxFiles, ref List<Data> data)
        {
            for (int i = 1; i <= maxFiles; i++)
            {
                using (StreamReader sr = new StreamReader(writePath + @"\" + fileName + "_" + i + ".txt", Encoding.Default))
                {
                    string line;
                    if ((line = sr.ReadLine()) != null)
                    {
                        char ch = '.';
                        int indexOfChar = line.IndexOf(ch);
                        int number;
                        int.TryParse(line.Substring(0, indexOfChar), out number);
                        string text = line.Substring(indexOfChar + 2);
                        data.Add(new Data() { _id = number, _text = text, _fileName = fileName + "_" + i});
                    }   
                }
            }
            data = data.OrderBy(order => order._text).ThenBy(order => order._id).ToList();
        }
        public static void MergeManyFiles (string fileName, int amountFiles)
        {
            string writePath = Directory.GetCurrentDirectory();
            string line;

            var timer = DateTime.Now;
            List<Data> data = new List<Data>();

            try
            {
                Console.WriteLine("Начало сборки файла.");

                Dictionary<string, StreamReader> streamReaders = new Dictionary<string, StreamReader>(amountFiles);
                for (int i = 1; i <= amountFiles; i++)
                {
                    string srFileName = fileName + "_" + i;
                    StreamReader sr = new StreamReader(writePath + @"\" + srFileName + ".txt", Encoding.Default);
                    streamReaders.Add(srFileName, sr);
                }

                foreach (var item in streamReaders)
                {
                    if ((line = item.Value.ReadLine()) != null)
                    {
                        char ch = '.';
                        int indexOfChar = line.IndexOf(ch);
                        int number;
                        int.TryParse(line.Substring(0, indexOfChar), out number);
                        string text = line.Substring(indexOfChar + 2);
                        data.Add(new Data() { _id = number, _text = text, _fileName = item.Key });
                    }
                }

                StreamWriter sw = new StreamWriter(writePath + @"\" + fileName + "_out.txt", false, Encoding.Default);

                while (data.Count > 0)
                {
                    data.OrderBy(order => order._text).ThenBy(order => order._id);
                    sw.WriteLine(data[0]._id + ". " + data[0]._text);
                    if ((line = streamReaders[data[0]._fileName].ReadLine()) != null)
                    {
                        char ch = '.';
                        int indexOfChar = line.IndexOf(ch);
                        int number;
                        int.TryParse(line.Substring(0, indexOfChar), out number);
                        string text = line.Substring(indexOfChar + 2);
                        data[0]._id = number;
                        data[0]._text = text;
                    }
                    else
                        data.RemoveAt(0);
                }

                foreach (var item in streamReaders)
                {
                    item.Value.Dispose();
                }
                sw.Dispose();

                Console.WriteLine($"Окончание сборки файла: { Math.Round((DateTime.Now - timer).TotalSeconds, 2)} сек.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка при сборке файла.");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }
        public static async Task DataSortAsync(string fileName, int amountFiles)
        {
            FileSplit(fileName, amountFiles);
            await SortAllSplitFilesAsync(fileName, amountFiles);
            MergeManyFiles(fileName, amountFiles);
        }
    }

    class Data
    {
        public int _id { get; set; }
        public string _text { get; set; }
        public string _fileName { get; set; }
    }
}