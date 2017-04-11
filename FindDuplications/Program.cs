using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FindDups
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string path;
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Please specify directory in parameters");
                return;
            }
            else
            {
                //path = @"C:\Users\Viktor\Desktop\input";
                path = args[0];
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Specified directory doesn't exist");
                    return;
                }
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // get all files
            var files = Directory.GetFiles(path);

            // get file groups with the same size
            var fileGroups = GroupBySize(files);

            // final filter - by hash
            fileGroups = GroupByCustomCriteria(fileGroups, GetHash);

            // split into groups by first line
            // Much slower than hash
            //fileGroup = GroupByCustomCriteria(fileGroup, GetFirstLine);
            
            int counter = 0;
            foreach (var file in fileGroups)
            {
                Console.WriteLine($"Duplication {++counter}");
                Console.WriteLine(string.Join("\n", file));
                Console.WriteLine();
            }

            if (counter == 0)
            {
                Console.WriteLine("No duplications found");
            }

            sw.Stop();
            Console.WriteLine($"Time spent - {sw.Elapsed}");
        }

        private static IEnumerable<IEnumerable<string>> GroupBySize(string[] files)
        {
            var result = files.Select(f => new
            {
                Name = f,
                Size = GetFileSize(f)
            })
                .GroupBy(f => f.Size)
                .Where(grp => grp.Count() > 1)
                .Select(item => item.Select(file => file.Name));

            return result;
        }

        private static IEnumerable<IEnumerable<string>> GroupByCustomCriteria<T>(
            IEnumerable<IEnumerable<string>> fileGroups,
            Func<string, T> getCriteria)
        {
            // each group is independent, so we can make it in parallel
            var result = fileGroups.AsParallel().SelectMany(fileGroup => fileGroup.Select(f => new
                {
                    Name = f,
                    Criteria = getCriteria(f)
                })
                .GroupBy(f => f.Criteria)
                .Where(grp => grp.Count() > 1)
                .Select(item => item.Select(file => file.Name)));

            return result;
        }

        private static string GetFirstLine(string filePath)
        {
            // this will read only the first line, not all lines
            return File.ReadLines(filePath).First();
        }

        static string GetHash(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return BitConverter.ToString(new SHA1Managed().ComputeHash(fileStream));
            }
        }

        private static long GetFileSize(string f)
        {
            return new FileInfo(f).Length;
        }
    }
}