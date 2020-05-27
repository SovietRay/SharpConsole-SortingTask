using System;
using System.Collections;
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
        static int fileStreamBuffSize = 4096 * 4;

        public static void CreateFileWithData(string fileName, int sizeInMB, int minId, int maxId)
        {
            try
            {
                string pathToFile = writePath + @"\" + fileName + ".txt";
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error creating collection.");
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
            MergeManyFiles(fileName, amountFiles);
            DeleteAllSplittedFiles(fileName, amountFiles);
        }

        public static int FileSplit(string fileName, int splitFileSize)
        {

            string pathToFile = writePath + @"\" + fileName + ".txt";
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
            List<Task> tasks = new List<Task>();
            Stopwatch timerSW = Stopwatch.StartNew();

            Console.WriteLine("Start sorting split files:");
            for (int i = 1; i <= amountFiles; i++)
            {
                string splittedFileName = $"{fileName}_{i}";
                Console.WriteLine($"I whant try to sort - {splittedFileName}.");
                Task sortTask = Task.Run(() => SortFile(splittedFileName));
                //SortFile(splittedFileName);
                tasks.Add(sortTask);
            }
            Console.WriteLine("...");
            await Task.WhenAll(tasks.ToArray());
            Console.WriteLine($"All files sorted!");
            Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
            Console.WriteLine();
        }

        public static void SortFile(string fileName)
        {
            try
            {
                string path = writePath + @"\" + fileName + ".txt";

                Stopwatch timerSW = Stopwatch.StartNew();
                FileInfo fileInf = new FileInfo(path);
                var content = File.ReadAllLines(path);

                Array.Sort(content, new SplitComparer());
                using (var sw = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Write, fileStreamBuffSize))
                {
                    foreach (var item in content)
                    {
                        byte[] bytes = Encoding.Default.GetBytes(item + "\r\n");
                        sw.Write(bytes, 0, bytes.Length);
                    }
                }

                Console.WriteLine($"Done! Size of {fileName}: {Math.Round(fileInf.Length / 1024f / 1024f, 3)}MB.");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while sorting the splited file!");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }

        public static void MergeManyFiles(string fileName, int amountFiles)
        {
            string line;
            List<KeyValuePair<string, string>> dataForSorting = new List<KeyValuePair<string, string>>();
            Dictionary<string, StreamReader> streamReaders = new Dictionary<string, StreamReader>(amountFiles);
            Stopwatch timerSW = Stopwatch.StartNew();

            try
            {
                Console.WriteLine("Start building the file from several.");
                Console.WriteLine("...");

                for (int i = 1; i <= amountFiles; i++)
                {
                    string sortedFileName = fileName + "_" + i;
                    StreamReader sr = new StreamReader(writePath + @"\" + sortedFileName + ".txt", Encoding.Default);
                    streamReaders.Add(sortedFileName, sr);
                }

                foreach (var item in streamReaders)
                {
                    if ((line = item.Value.ReadLine()) != null)
                        dataForSorting.Add(new KeyValuePair<string, string>(item.Key, line));
                }

                StreamWriter streamWriter = new StreamWriter(writePath + @"\" + fileName + "_out.txt", false, Encoding.Default);

                while (dataForSorting.Count > 0)
                {
                    dataForSorting.Sort(new SplitComparer().Compare);
                    streamWriter.WriteLine(dataForSorting.First().Value);

                    if ((line = streamReaders[dataForSorting.First().Key].ReadLine()) != null)
                    {
                        dataForSorting[0] = new KeyValuePair<string, string>(dataForSorting.First().Key, line);
                    }
                    else
                        dataForSorting.RemoveAt(0);                 
                }

                foreach (var item in streamReaders)
                {
                    item.Value.Dispose();
                }
                streamWriter.Dispose();

                Console.WriteLine($"Done!");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while merge the slited file!");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
            }
        }

        private static void DeleteAllSplittedFiles(string fileName, int amount)
        {
            for (int i = 1; i <= amount; i++)
            {
                File.Delete(writePath + @"\" + fileName + "_" + i + ".txt");
            }
        }


        private static string CheckSWTimer(Stopwatch timerSW)
        {
            TimeSpan ts = timerSW.Elapsed;
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }

/*        private static void treeSort(string[] str)
        {
            foreach (var item in str)
            {

            }
        }*/
    }

/*    class Data
    {
        public int _id { get; set; }
        public string _text { get; set; }
    }*/

    public class SplitComparer : IComparer
    {
        public int Compare(object left, object right)
        {
            try
            {
                ParseCompareString(left, out int leftNumber, out string leftData);
                ParseCompareString(right, out int rightNumber, out string rightData);

                if (!leftData.Equals(rightData))       
                    return leftData.CompareTo(rightData);
                else
                {
                    if (leftNumber < rightNumber)
                        return -1;

                    if (leftNumber > rightNumber)
                        return 1;
                   
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int Compare(KeyValuePair<string, string> left, KeyValuePair<string, string> right)
        {
            try
            {
                ParseCompareString(left.Value, out int leftNumber, out string leftData);
                ParseCompareString(right.Value, out int rightNumber, out string rightData);

                if (!leftData.Equals(rightData))
                    return leftData.CompareTo(rightData);
                else
                {
                    if (leftNumber < rightNumber)
                        return -1;

                    if (leftNumber > rightNumber)
                        return 1;

                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void ParseCompareString(object obj, out int num, out string str)
        {
            char ch = '.';
            int indexofchar = obj.ToString().IndexOf(ch);
            str = obj.ToString().Substring(indexofchar + 1);
            num = int.Parse(obj.ToString().Substring(0, indexofchar));
        }
    }
}