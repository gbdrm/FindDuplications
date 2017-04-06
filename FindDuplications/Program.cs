using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

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

        var files = Directory.GetFiles(path)
                     .Select(f => new
                         {
                             Name = f,
                             FileHash = GetHash(f)
                         })
                     .GroupBy(f => f.FileHash)
                     .Where(grp => grp.Count() > 1)
                     .Select(item => new
                     {
                         Files = item.Select(file => file.Name).ToList()
                     });

        int counter = 0;
        foreach (var file in files)
        {
            Console.WriteLine($"Duplication {++counter}");
            Console.WriteLine(string.Join("\n", file.Files));
            Console.WriteLine();
        }
    }

    static string GetHash(string path)
    {
        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            return BitConverter.ToString(new SHA1Managed().ComputeHash(fileStream));
        }
    }
}