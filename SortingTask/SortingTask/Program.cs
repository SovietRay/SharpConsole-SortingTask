using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingTask
{
    class Program
    {
        static void Main(string[] args)
        {
            string writePath = Directory.GetCurrentDirectory() + "\\TestData.txt";
            Console.WriteLine(writePath);
            List<Data> allData = new List<Data>();
            //Console.ReadLine();
            //string writePath = @"C:\Users\Лалитта\Documents\GitHub";
            try
            {
                Random random = new Random();
                using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default))
                {
                    for (int i = 0; i < 500; i++)
                    {
                        
                        sw.WriteLine(random.Next(1, 100001) + ". " + LoremNET.Lorem.Words(1));
                    }
                }

                using (StreamReader sr = new StreamReader(writePath, System.Text.Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        char ch = '.';
                        int indexOfChar = line.IndexOf(ch);
                        int number;
                        int.TryParse(line.Substring(0, indexOfChar), out number);
                        string text = line.Substring(indexOfChar+2);

                        allData.Add(new Data() { _id = number, _text = text});
                    }
                }

                List<Data> SortedList = allData.OrderBy(order => order._text).ThenBy(order => order._id).ToList();

                foreach (var item in allData)
                {
                    Console.WriteLine(item._id + " ! " + item._text);
                }

                Console.WriteLine("---------");
                foreach (var item in SortedList)
                {
                    Console.WriteLine(item._id + " ! " + item._text);
                }

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

    class Data
    {
        public int _id { get; set; }
        public string _text { get; set; }
    }
}
