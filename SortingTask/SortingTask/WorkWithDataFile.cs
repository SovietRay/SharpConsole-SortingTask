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
        public static void CreateFileWithData(string fileName, int numberOfLines, int minId, int maxId, int numberOfWordsInLine)
        {
            try
            {
                //string writePath = Directory.GetCurrentDirectory();
                Random random = new Random();

                List<string> dictionary_names = new List<string>();
                List<string> dictionary_surnames = new List<string>();
                List<string> dictionary_cities = new List<string>();

                CreateCollectionFromFile("Dictionary_names", ref dictionary_names);
                CreateCollectionFromFile("Dictionary_surnames", ref dictionary_surnames);
                CreateCollectionFromFile("Dictionary_cities", ref dictionary_cities);

                //using (StreamWriter sw = new StreamWriter(writePath + @"\" + fileName + ".txt", false, Encoding.Default))
                using (var sw = new FileStream(
                    writePath + @"\" + fileName + ".txt", FileMode.Create, FileAccess.Write, FileShare.Write, 4096*4))
                {
                    string content;
                    string content_additional;
                    byte[] bytes;
                    for (int i = 0; i < numberOfLines; i++)
                    {
                        content_additional = $"{dictionary_names[random.Next(0, dictionary_names.Count)]} {dictionary_surnames[random.Next(0, dictionary_surnames.Count)]} from {dictionary_cities[random.Next(0, dictionary_cities.Count)]}";
                        content = random.Next(minId, maxId) + ". " + content_additional;
                        bytes = Encoding.Default.GetBytes(content + "\r\n");
                        sw.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
                throw;
            }
        }

        public static void CreateDataCollectionFromFile(string fileName, ref List<Data> data)
        {
            try
            {
                //string writePath = Directory.GetCurrentDirectory();

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
                Console.WriteLine(e);
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
                Console.WriteLine(e);
                throw;
            }
        }

        public static void SimpleSort(ref List<Data> data)
        {
            data = data.OrderBy(order => order._text).ThenBy(order => order._id).ToList();
        }

        public static void SaveDataToFile(string fileName, ref List<Data> data)
        {
            try
            {
                string writePath = Directory.GetCurrentDirectory();
                using (StreamWriter sw = new StreamWriter(writePath + @"\" + fileName + ".txt", false, Encoding.Default))
                {
                    foreach (var item in data)
                    {
                        sw.WriteLine(item._id + ". " + item._text);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void SortFile(string fileName)
        {
            var timer = DateTime.Now;
            List<Data> data = new List<Data>();
            CreateDataCollectionFromFile(fileName, ref data);
            SimpleSort(ref data);
            SaveDataToFile(fileName, ref data);

            string path = writePath + @"\" + fileName + ".txt";
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                Console.Write($"Размер {fileName}: {Math.Round(fileInf.Length / 1048576.0, 3)}МБ. ");
                Console.WriteLine(Math.Round((DateTime.Now - timer).TotalSeconds, 2) + " сек." + " - время на сортировку");
            }
        }

        public static void PrintDataFromFile(string fileName)
        {

        }

        public static void SimpleMerge(ref Data data1, ref Data data2)
        {

        }

        public static void FileSplit(string name, int maxFiles)
        {

            string path = writePath + @"\" + name + ".txt";
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                Console.WriteLine( $"Размер основного файла: {Math.Round(fileInf.Length/1048576.0, 3)}МБ");
            }

            using (StreamReader sr = new StreamReader(writePath + @"\" + name + ".txt", Encoding.Default))
            {
                Dictionary<int, StreamWriter> streamWriters = new Dictionary<int, StreamWriter>(maxFiles);
                for (int i = 1; i <= maxFiles; i++)
                {
                    StreamWriter sw = new StreamWriter(writePath + @"\" + name + "_" + i + ".txt", false, Encoding.Default);
                    streamWriters.Add(i, sw);
                }

                string line;
                int m = 1;
                
                while ((line = sr.ReadLine()) != null)
                {
                   var sw = streamWriters[m];
                    sw.WriteLine(line);
                    m = (m < maxFiles) ? m+1 : 1;
                }

                for (int i = 1; i <= maxFiles; i++)
                {
                    streamWriters[i].Dispose();
                }
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
        public static void MergeManyFiles (string name, int maxFiles)
        {
            List<Data> data = new List<Data>();

            var timer = DateTime.Now;
            Console.WriteLine();
            Console.WriteLine("Начало разбивки");
            //Ocenit razmer faila i polichit fail max
            FileSplit(name, maxFiles);//Razdelit fail na malenkie
            Console.WriteLine(Math.Round((DateTime.Now - timer).TotalSeconds, 2) + " сек." + " - время на разбивку");
            Console.WriteLine("Окончание разбивки");


            timer = DateTime.Now;
            Console.WriteLine();
            Console.WriteLine("Начало сортировки");
            for (int i = 1; i <= maxFiles; i++) //Otsortirovat kashdi fail
            {
                SortFile(name + "_" + i);
            }
            Console.WriteLine(Math.Round((DateTime.Now - timer).TotalSeconds, 2) + " сек." + " - время на сортировку всех файлов разбивки");
            Console.WriteLine("Окончание сортировки");
            Console.WriteLine();

            Console.WriteLine("Начало сборки файла");
            timer = DateTime.Now;
            //Otkrivaem vse faili na chtenie
            string writePath = Directory.GetCurrentDirectory();
            Dictionary<string , StreamReader> streamReaders = new Dictionary<string, StreamReader>(maxFiles);
            for (int i = 1; i <= maxFiles; i++)
            {
                string srFileName = name + "_" + i;
                StreamReader sr = new StreamReader(writePath + @"\" + srFileName + ".txt", Encoding.Default);
                streamReaders.Add(srFileName, sr);
            }

            //Delaem list iz pervih strok vseh failov
            string line;
            foreach (var item in streamReaders)
            {
                if ((line = item.Value.ReadLine())!=null)
                {
                    char ch = '.';
                    int indexOfChar = line.IndexOf(ch);
                    int number;
                    int.TryParse(line.Substring(0, indexOfChar), out number);
                    string text = line.Substring(indexOfChar + 2);
                    data.Add(new Data() { _id = number, _text = text, _fileName = item.Key});
                }
            }

            StreamWriter sw = new StreamWriter(writePath + @"\" + name + "_out.txt", false, Encoding.Default);

            while (data.Count > 0)
            {
                //Otsortirovat list
                data = data.OrderBy(order => order._text).ThenBy(order => order._id).ToList();

                //Sohraniaem minimalnoe znachenie v buffer (fail)
                //List<Data> buffer = new List<Data>();

                sw.WriteLine(data[0]._id + ". " + data[0]._text);

                //Zameniaem v liste to znachenie, chto popalo v buffer
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

            //Poiskat v pervih strokah failov i liste stroki s takim she znacheniem
            //Sdelat novi spisok takih strok i otsortirovat po nomeru
            //Udalit iz list
            //Dobavit v itogovi fail
            //Povtoriat poka est stroki v failah
            Console.WriteLine(Math.Round((DateTime.Now - timer).TotalSeconds, 2) + " сек." + " - время на сборку файла");
            Console.WriteLine("Окончание сборки файла");
            Console.WriteLine();
        }
    }

    class Data
    {
        public int _id { get; set; }
        public string _text { get; set; }
        public string _fileName { get; set; }
    }
}