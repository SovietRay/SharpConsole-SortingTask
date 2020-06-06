using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingTask
{
    class WorkWithDataFile
    {
        static string writePath = Directory.GetCurrentDirectory();
        static Random random = new Random();
        static int fileStreamBuffSize = 4096 * 4;
        static string dictionaryNameWithNames = "Dictionary_names";
        static string dictionaryNameWithSurnames = "Dictionary_surnames";
        static string dictionaryNameWithAdress = "Dictionary_address";
        static int LogicCores = Environment.ProcessorCount;

        public static void CreateFileWithRandomData(string fileName, int sizeInMB, int minId, int maxId)
        {
            string pathToFile = writePath + @"\" + fileName + ".txt";
            FileInfo fileInf = new FileInfo(pathToFile);

            if (!fileInf.Exists)
            {
                try
                {
                    long sizeByte = (long)sizeInMB * 1024 * 1024;

                    Stopwatch timerSW = new Stopwatch();
                    List<string> dictionaryNames = new List<string>();
                    List<string> dictionarySurnames = new List<string>();
                    List<string> dictionaryAddress = new List<string>();

                    #region Message
                    Console.WriteLine($"The program began creating new unsorted file.");
                    Console.WriteLine($"Path: {pathToFile}");
                    Console.WriteLine($"...");
                    #endregion

                    //Check dictionary exist, if not, download?
                    //Work with big dictionary? optimization? need class with dictionary

                    timerSW.Start();

                    CreateCollectionFromFile(dictionaryNameWithNames, ref dictionaryNames);
                    CreateCollectionFromFile(dictionaryNameWithSurnames, ref dictionarySurnames);
                    CreateCollectionFromFile(dictionaryNameWithAdress, ref dictionaryAddress);

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

                    #region Message
                    Console.WriteLine($"Done!");
                    fileInf.Refresh();
                    Console.WriteLine($"Total size: {fileName}.txt ~ {Math.Round(fileInf.Length / 1024f / 1024f, 3)}MB");
                    Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                    Console.WriteLine();
                    #endregion
                }
                catch (Exception e)
                {
                    #region Exception
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("An error occurred while creating new unsorted file!");
                    Console.WriteLine(e);
                    Console.ResetColor();
                    #endregion
                    throw;
                }
            }
            else
            {
                #region Message
                Console.WriteLine($"File: {fileName}.txt alredy exist, its size~ {Math.Round(fileInf.Length / 1024f / 1024f, 3)}MB");
                #endregion
                //Check all data in file?
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
                #region Exception
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error creating collection from file.");
                Console.WriteLine(e);
                Console.ResetColor();
                #endregion
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
                #region Making big string
                content.Append(random.Next(minId, maxId));
                content.Append(". ");
                content.Append(dictionaryNames[random.Next(0, dictionaryNames.Count)]);
                content.Append(" ");
                content.Append(dictionarySurnames[random.Next(0, dictionarySurnames.Count)]);
                content.Append(", ");
                content.Append(dictionaryAddress[random.Next(0, dictionaryAddress.Count)]);
                content.AppendLine();
                #endregion
            }
            return content.ToString();
        }

        public static async Task DataSortTwoToOneAsync(string fileName, int splitFileSize)
        {
            int amountFiles = FileSplit(fileName, splitFileSize);
            await SortAllSplitFilesAsync(fileName, amountFiles);

            #region Message
            Console.WriteLine("Start building the file from several.");
            Console.WriteLine("...");
            #endregion

            Stopwatch timerSW = Stopwatch.StartNew();
            TwoToOneMarge(fileName);

            #region Message
            Console.WriteLine($"Done!");
            Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
            #endregion
        }

        public static async Task DataSortManyToOneAsync(string fileName, int splitFileSize)
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
                #region Message
                Console.WriteLine($"Start splitting a file {fileName}.txt into parts ~{splitFileSize}MB each.");
                Console.WriteLine($"...");
                #endregion

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
                #region Message
                Console.WriteLine($"Done!");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                Console.WriteLine();
                #endregion

                return (firstFileNumber - 1);
            }
            catch (Exception e)
            {
                #region Exception
                Console.WriteLine("An error occurred while splitting the file!");
                Console.WriteLine(e);
                Console.ReadLine();
                #endregion
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
                Console.WriteLine($"I whant try to sort - {splittedFileName}.txt");
                Task sortTask = Task.Run(() => SortFile(splittedFileName));
                tasks.Add(sortTask);
            }
            Console.WriteLine("...");
            await Task.WhenAll(tasks.ToArray());

            #region Message
            Console.WriteLine($"All files sorted!");
            Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
            Console.WriteLine();
            #endregion
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

                #region Message
                Console.WriteLine($"Done! Size of {fileName}: {Math.Round(fileInf.Length / 1024f / 1024f, 3)}MB.");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                #endregion
            }
            catch (Exception e)
            {
                #region Exception
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while sorting the splited file!");
                Console.WriteLine(e);
                Console.ResetColor();
                #endregion
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
                #region Message
                Console.WriteLine("Start building the file from several.");
                Console.WriteLine("...");
                #endregion

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
                //StreamWriter streamWriter = new FileStream(writePath + @"\" + fileName + "_out.txt", FileMode.Truncate, FileAccess.Write, FileShare.Write, fileStreamBuffSize)
                StreamWriter streamWriter = new StreamWriter(writePath + @"\" + fileName + "(outAllMerge).txt", false, Encoding.Default);
                StringBuilder content = new StringBuilder();
                long bufferContent = (long)10 * 1024 * 1024;
                dataForSorting.Sort(new SplitComparer().Compare);

                while (dataForSorting.Count > 0)
                {
                    var fileNameRef = dataForSorting[0].Key;
                    if (content.Length <= bufferContent)
                    {
                        content.AppendLine(dataForSorting[0].Value);
                        dataForSorting.RemoveAt(0);
                    }
                    else
                    {
                        content.AppendLine(dataForSorting[0].Value);
                        streamWriter.Write(content);
                        content.Clear();
                        dataForSorting.RemoveAt(0);
                    }

                    if ((line = streamReaders[fileNameRef].ReadLine()) != null)
                    {
                        var tempLine = new KeyValuePair<string, string>(fileNameRef, line);
                        InsertTo(ref dataForSorting, tempLine);
                    }

                }

                streamWriter.Write(content);
                content.Clear();

                foreach (var item in streamReaders)
                {
                    item.Value.Dispose();
                }
                streamWriter.Dispose();

                #region Message
                Console.WriteLine($"Done!");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                #endregion
            }
            catch (Exception e)
            {
                #region Exception
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while merge the slited file!");
                Console.WriteLine(e);
                Console.ResetColor();
                #endregion
                throw;
            }
        }

        public static void MergeManyFilesByTwo(string fileName, int amountFiles)
        {
            string line;
            List<KeyValuePair<string, string>> dataForSorting = new List<KeyValuePair<string, string>>();
            Dictionary<string, StreamReader> streamReaders = new Dictionary<string, StreamReader>(amountFiles);
            Stopwatch timerSW = Stopwatch.StartNew();

            try
            {
                #region Message
                Console.WriteLine("Start building the file from several.");
                Console.WriteLine("...");
                #endregion

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
                //StreamWriter streamWriter = new FileStream(writePath + @"\" + fileName + "_out.txt", FileMode.Truncate, FileAccess.Write, FileShare.Write, fileStreamBuffSize)
                StreamWriter streamWriter = new StreamWriter(writePath + @"\" + fileName + "_out.txt", false, Encoding.Default);
                StringBuilder content = new StringBuilder();
                long bufferContent = (long)200 * 1024 * 1024;
                dataForSorting.Sort(new SplitComparer().Compare);

                while (dataForSorting.Count > 0)
                {
                    var fileNameRef = dataForSorting[0].Key;
                    if (content.Length <= bufferContent)
                    {
                        content.AppendLine(dataForSorting[0].Value);
                        dataForSorting.RemoveAt(0);
                    }
                    else
                    {
                        content.AppendLine(dataForSorting[0].Value);
                        streamWriter.Write(content);
                        content.Clear();
                        dataForSorting.RemoveAt(0);
                    }

                    if ((line = streamReaders[fileNameRef].ReadLine()) != null)
                    {
                        var tempLine = new KeyValuePair<string, string>(fileNameRef, line);
                        InsertTo(ref dataForSorting, tempLine);
                    }
                }

                streamWriter.Write(content);
                content.Clear();

                #region Dispose streams
                foreach (var item in streamReaders)
                {
                    item.Value.Dispose();
                }
                streamWriter.Dispose();
                #endregion

                #region Message
                Console.WriteLine($"Done!");
                Console.WriteLine($"Time passed: {CheckSWTimer(timerSW)}");
                #endregion
            }
            catch (Exception e)
            {
                #region Exception
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while merge the slited file!");
                Console.WriteLine(e);
                Console.ResetColor();
                throw;
                #endregion
            }
        }
        private static void TwoToOneMarge(string fileName)
        {  
            var files = GetFilesNames(fileName);
            var fileList = files.ToList();
            var count = files.Count();

            if (fileList.Count() > 1)
            {          
                int num = 1;
                for (int i = 0; i < count - 1; i += 2)
                {
                    MakeOneFromTwoSorted(fileList[i].ToString(), fileList[i + 1].ToString(), fileName + "_" + num);
                    num++;
                }

                if (files.Count() > 0)
                {
                    foreach (var item in files)
                    {
                        var index = item.ToString().Count() - 5;
                            
                        if (item.ToString().ElementAt(index) == Char.Parse("m"))
                        {
                            File.Move(item.ToString(), item.ToString().Remove(index, 1));
                        }
                    }
                }
                TwoToOneMarge(fileName);

            }

            var tempFileName = fileList.First().ToString().Remove(fileList.First().ToString().Count() - 6);
            if (!File.Exists(tempFileName + "_out.txt"))
            {
                File.Move(fileList.First().ToString(), tempFileName + "_out.txt");
            }
        }

        private static IEnumerable<string> GetFilesNames(string fileName)
        {
            try
            {
                IEnumerable<string> myFiles = Directory.EnumerateFiles(writePath, fileName + "_*", SearchOption.TopDirectoryOnly);
                return myFiles;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        private static void MakeOneFromTwoSorted (string fileName1, string fileName2, string newName)
        {
            string line1, line2;
            long bufferContent = 10 * 1024 * 1024;

            StringBuilder content = new StringBuilder();
            StreamReader sr1 = new StreamReader(fileName1, Encoding.Default);
            StreamReader sr2 = new StreamReader(fileName2, Encoding.Default);
            FileStream sw = new FileStream(writePath + @"\" + newName + "m.txt", FileMode.Create, FileAccess.Write, FileShare.Write, fileStreamBuffSize);
           
            line1 = sr1.ReadLine();
            line2 = sr2.ReadLine();

            while (line1 != null && line2 != null)
            {
                if (new SplitComparer().Compare(line1, line2) != -1)
                {
                    content.AppendLine(line2);
                    line2 = sr2.ReadLine();
                }
                else
                {
                    content.AppendLine(line1);
                    line1 = sr1.ReadLine();
                }

                if (content.Length >= bufferContent)
                {
                    byte[] bytesTmp = Encoding.Default.GetBytes(content.ToString());
                    sw.Write(bytesTmp, 0, bytesTmp.Length);
                    content.Clear();
                }
            }

            if (line1 == null)
            {
                while (line2 != null)
                {
                    content.AppendLine(line2);
                    line2 = sr2.ReadLine();
                }
            }
            else
            {
                while (line1 != null)
                {
                    content.AppendLine(line1);
                    line1 = sr1.ReadLine(); //sr1?
                }
            }

            byte[] bytes = Encoding.Default.GetBytes(content.ToString());
            sw.Write(bytes, 0, bytes.Length);
            content.Clear();

            sw.Dispose();
            sr1.Dispose();
            sr2.Dispose();

            File.Delete(fileName1);
            File.Delete(fileName2);
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

        private static void InsertTo(ref List<KeyValuePair<string, string>> list, KeyValuePair<string, string> data)
        {
            int count = list.Count;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (new SplitComparer().Compare(list[i], data) != -1)
                    {
                        list.Insert(i, data);
                        break;
                    }
                    else
                    {
                        if (i == count - 1)
                            list.Add(data);
                    }
                }
            }
            else
            {
                list.Add(data);
            }
        }
    }

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