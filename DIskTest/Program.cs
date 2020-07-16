using DIskTest.Model;
using DIskTest.Model.Operations;
using System;
using System.IO;

namespace DIskTest
{
    class Program
    {
        public const string unit = "MB/s";

        private static string PickDrive(long freeSpace)
        {
            var i = 0;
            var k = 0;
            var drives = DiskInfo.GetEligibleDrives();
            var driveIndexes = new int[drives.Length];
            try
            {
                foreach (var d in drives)
                {
                    var flag = false;
                    if (d.TotalFreeSpace < freeSpace)
                        flag = true;
                    else
                    {
                        driveIndexes[i] = k;
                        i++;
                    }
                    Console.WriteLine("[{0}] {1} {2:0.00} Gb свободного пространства {3}", flag ? " " : i.ToString(), d.Name,
                            (double)d.TotalFreeSpace / 1024 / 1024 / 1024,
                            flag ? "- недостаточно свободного места" : ""
                            );
                    k++;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            Console.Write("- пожалуйста, выберите диск для тестирования: ");
            int index;
            do
            {
                var input = Console.ReadLine();
                if (!int.TryParse(input, out index))
                    index = -1;
            }
            while ((index < 1) || (index > i));
            return drives[driveIndexes[--index]].Name;
        }

        static void Main()
        {
            try
            {
                const long fileSize = 1024 * 1024 * 1024;
                var drivePath = PickDrive(fileSize);
                if (drivePath == null) 
                    return;
                var testSuite = new TestOperations(drivePath, fileSize);
                using (testSuite)
                {
                    Console.WriteLine("\nПуть к тестовому файлу: {0}, Размер: {1:0.00}Gb", testSuite.FilePath, (double)testSuite.FileSize / 1024 / 1024 / 1024);
                    string currentTest = null;
                    var breakTest = false;
                    testSuite.StatusUpdate += (sender, e) =>
                    {
                        if (breakTest) 
                            return;
                        if (e.Status == OperationStatus.NotStarted) 
                            return;
                        if ((sender as Operation).DisplayName != currentTest)
                        {
                            currentTest = (sender as Operation).DisplayName;
                            Console.Write("\n{0}", (sender as Operation).DisplayName);
                        }
                        if ((e.Status == OperationStatus.Completed) && (e.Results != null))
                        {
                            Console.Write(string.Format("\nСреднее значение скорости: {1:0.00} {0}\t", unit, e.Results.AvgThroughput));
                            Console.Write(
                                string.Format("\nСкорость выполнения: Min/Max: {1:0.00} {0} / {2:0.00} {0}, Время выполнения: {3}m{4:00}s",
                                unit,
                                e.Results.Min,
                                e.Results.Max,
                                e.ElapsedMs / 1000 / 60,
                                e.ElapsedMs / 1000 % 60)
                            );
                        }
                    };
                    var results = testSuite.Execute();
                    if (!breakTest)
                        Console.WriteLine("\nТестовый файл удален.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nПрограмма прервана из-за непредвиденной ошибки: {0}", ex.Message.ToString());
            }
            Console.ReadLine();
        }
    }
}
