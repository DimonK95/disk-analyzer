using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Analizatior
{
    class Program
    {
        static IEnumerable<string> EnumFile(string path, string pattern, SearchOption so)
        {
            try
            {
                IEnumerable<string> Dir = Directory.EnumerateDirectories(path).SelectMany(x => EnumFile(x, pattern, so));
                return Dir.Concat(Directory.EnumerateFiles(path, pattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }


        static int Add(string path)
        {
            int cnt = 0;
           
            IEnumerable<string> Dir = EnumFile(path, "*", SearchOption.AllDirectories);
            foreach (string dir in Dir)
            {
                try {
                    EntityFile file = new EntityFile(dir);
                    try {
                        DataBase.InsertRecord(file);
                        cnt++;
                    }
                    catch (Exception e)
                    {
                        LogWriter.AddLog(e.Message);
                    }
                }
                catch (Exception e)
                {
                    LogWriter.AddLog(e.Message);
                }
            }
            return cnt;
        }



        static void Analize(string needPath)
        {
            SQLiteDataReader reader = DataBase.GetDataReader(needPath);
            bool ok = true;
            HashSet<string> onlyBase = new HashSet<string>();
            HashSet<string> baseAndDisk = new HashSet<string>();
            HashSet<string> onlyDisk = new HashSet<string>();

            foreach (System.Data.Common.DbDataRecord record in reader)
            {
                string path = record["path"].ToString();
                
                if (!File.Exists(path))
                {
                    onlyBase.Add(path);
                }
                else
                {
                    baseAndDisk.Add(path);
                    string hash = record["hash"].ToString();
                    string size = record["size"].ToString();
                    string date = record["date"].ToString();

                    try
                    {
                        EntityFile nE = new EntityFile(path);
                        bool hashDif = (hash != nE.Hash);
                        bool sizeDif = (Convert.ToInt32(size) != nE.Size);
                        bool dateDif = (date != nE.ModificationDate.ToString());
                        if (hashDif || sizeDif || dateDif)
                        {
                            Console.WriteLine("Следующий файл был изменен: " + path);
                            if (hashDif) Console.WriteLine("Изменен хэш " + 
                                "\nстарый хэш: " + hash + "\nновый хэш: " + nE.Hash);
                            if (sizeDif) Console.WriteLine("Изменен размер "+
                                "\nстарый размер: " + Convert.ToInt32(size) + "\nновый размер: " + nE.Size);
                            if (dateDif) Console.WriteLine("Изменена дата модификации"+
                                "\nстарая дата: " + date + "\nновая дата: " + nE.ModificationDate.ToString());
                            Console.WriteLine();
                            ok = false;
                        }
                    }
                    catch (Exception e)
                    {
                        LogWriter.AddLog(e.Message);
                    }
                }
            }

            try
            {
                IEnumerable<string> Dir = EnumFile(needPath, "*", SearchOption.AllDirectories);
                foreach (string dir in Dir)
                {
                    if (!baseAndDisk.Contains(dir))
                        onlyDisk.Add(dir);
                }
            }
            catch (Exception e)
            {
                LogWriter.AddLog(e.Message);
            }

            if (onlyBase.Count != 0)
            {
                Console.WriteLine("Следующие файлы в базе есть, а на диске не обнаружены");
                foreach (string file in onlyBase)
                {
                    Console.WriteLine(file);
                    ok = false;
                }
            }

            Console.WriteLine();

            if (onlyDisk.Count != 0)
            {
                Console.WriteLine("Следующие файлы на диске есть, а в базе не обнаружены");
                foreach (string file in onlyDisk)
                {
                    Console.WriteLine(file);
                    ok = false;
                }
            }

            if (ok)
                Console.WriteLine("Различий в базе и на диске не обнаружено.");
        }

        static void PrintBase(string needPath)
        {
            SQLiteDataReader reader = DataBase.GetDataReader(needPath);
            Boolean empty = true;

            foreach (System.Data.Common.DbDataRecord record in reader)
            {
                string path = record["path"].ToString();
                string hash = record["hash"].ToString();
                string size = record["size"].ToString();
                string date = record["date"].ToString();
                string result = path + " " + hash + " " + size + " " + date;
                Console.WriteLine(result);
                empty = false;
            }

            if (empty)
            {
                Console.WriteLine("По данному пути в базе ничего нет");
            }
        }

        static void Help()
        {
            Console.WriteLine();
            Console.WriteLine("Введите:");
            Console.WriteLine(" 1 - для добавления в базу");
            Console.WriteLine(" 2 - для анализа");
            Console.WriteLine(" 3 - для просмотра содержимого базы");
            Console.WriteLine(" 4 - для очиcтки базы");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            LogWriter.Init();
            DataBase.Init();
            ConsoleKeyInfo cki;
            string path = "";

            while (true)
            {
                Help();
                cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case (ConsoleKey.D1):
                        Console.WriteLine();
                        Console.WriteLine("Введите путь:");
                        path = Console.ReadLine().Replace('/', '\\');
                        try {
                            int cnt = Add(path);
                            Console.WriteLine("Добавлено " + cnt + " файлов");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Некорректный путь. Подробности в лог-файле.");
                            LogWriter.AddLog(e.Message);
                        }
                        break;
                    case (ConsoleKey.D2):
                        Console.WriteLine();
                        Console.WriteLine("Введите путь:");
                        path = Console.ReadLine().Replace('/', '\\');
                        try
                        {
                            Analize(path);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Некорректный путь. Подробности в лог-файле.");
                            LogWriter.AddLog(e.Message);
                        }
                        break;
                    case (ConsoleKey.D3):
                        Console.WriteLine();
                        Console.WriteLine("Введите путь:");
                        path = Console.ReadLine().Replace('/', '\\');
                        try {
                            Console.WriteLine(" Вывод файлов в базе по данному пути:");
                            PrintBase(path);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Некорректный путь. Подробности в лог-файле.");
                            LogWriter.AddLog(e.Message);
                        }
                        break;
                    case (ConsoleKey.D4):
                        Console.WriteLine();
                        try
                        {
                            DataBase.ClearTable();
                            Console.WriteLine(" Очистка базы произведена успешно.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Ошибка очистки. Подробности в лог-файле.");
                            LogWriter.AddLog(e.Message);
                        }
                        break;
                    default:
                        Console.WriteLine(" Введена некорректная команда.");
                        break;
                }
            }

            DataBase.Close();
        }
    }
}
