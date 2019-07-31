using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SecureData.Cyphers;
using SecureData.IO;

namespace SecureDir
{
    class Program
    {
        static void Main(string[] args)
        {
            SecureDirectory dir;

            Console.WriteLine("SecureDir\n---------");
            Console.WriteLine("Enter 'C' to create or 'O' to open");

            bool create = false;
            while (true)
            {
                string input = Console.ReadLine();

                if (input.Length == 1 && (input[0] == 'C' || input[0] == 'O'))
                {
                    create = input[0] == 'C';
                    break;
                }
                else
                {
                    Console.WriteLine("Wrong input!");
                }
            }

            if (create)
            {
                dir = Create();
                Console.WriteLine("SecureDirectory created!");
            }
            else
            {
                dir = Open();
                Console.WriteLine("SecureDirectory opened!");
            }

            while (true)
            {
                Console.Write(DisplayActiveDir(dir));
                string cmd = Console.ReadLine();

                RunCommand(cmd, dir);

                dir.Save();

                if (cmd == "exit")
                {
                    break;
                }
            }

            dir.Dispose();
        }

        private static SecureDirectory Open()
        {
            var dir = new SecureDirectory();
            string path = "";
            string key = "";

            while (true)
            {
                Console.Write("File: ");
                path = Console.ReadLine();
                if (File.Exists(path))
                    break;
                else
                    Console.WriteLine("File not found!");
            }

            while (true)
            {
                Console.Write("Key: ");
                key = Console.ReadLine();

                if (SecureDirectory.ValidateKeyForDecryption(path, Cypher.GetByIdentifier("AES"), key))
                    break;
                else
                    Console.WriteLine("Key incorrect");
            }

            dir.Open(path, Cypher.GetByIdentifier("AES"), key);

            return dir;
        }

        private static SecureDirectory Create()
        {
            var dir = new SecureDirectory();
            string path = "";
            string key = "";

            while (true)
            {
                Console.Write("FilePath: ");
                path = Console.ReadLine();
                if (Directory.Exists(Path.GetDirectoryName(path)))
                    break;
                else
                    Console.WriteLine("Path incorrect!");
            }

            while (true)
            {
                Console.Write("Key: ");
                key = Console.ReadLine();

                if (key.Length > 0)
                    break;
                else
                    Console.WriteLine("Key invalid");
            }

            dir.Create(path, Cypher.GetByIdentifier("AES"), key);

            return dir;
        }

        private static void RunCommand(string input, SecureDirectory dir)
        {
            string cmd = input;
            if (cmd.Contains(" "))
            {
                cmd = input.Substring(0, input.IndexOf(" "));
            }

            if (cmd == Commands.Exit)
            {

            }
            else if (cmd == Commands.Help)
            {
                throw new NotImplementedException();
            }
            //file
            else if (cmd == Commands.AddFile)
            {
                string path = input.Replace(cmd, "").Replace(" ", "");

                if (File.Exists(path))
                {
                    dir.AddFile(path);
                    Console.WriteLine("File added.");
                }
                else
                {
                    Console.WriteLine("File not found!");
                }
            }
            else if (cmd == Commands.DeleteFile)
            {
                throw new NotImplementedException();
            }
            else if (cmd == Commands.MoveFile)
            {
                throw new NotImplementedException();
            }

            //direcories
            else if (cmd == Commands.AddDirectory)
            {
                string name = input.Replace(cmd, "").Replace(" ", "");

                dir.AddDirectory(name);
                Console.WriteLine("Dir added.");
            }
            else if (cmd == Commands.MoveDirectory)
            {
                throw new NotImplementedException();
            }
            else if (cmd == Commands.DeleteDirectory)
            {
                throw new NotImplementedException();
            }

            //browsing
            else if (cmd == Commands.PreviousDirectory)
            {
                dir.MoveToParent();
            }
            else if (cmd == Commands.NextDirectory)
            {
                int index = Convert.ToInt32(input.Replace(cmd, "").Replace(" ", ""));
                dir.MoveToChild(index);
            }
            else if (cmd == Commands.ViewContent)
            {
                var active = dir.ActiveDirectory;
                string content = "";

                for (int i = 0; i < active.Children.Length; i++)
                {
                    content += $"Dir[{i}]: {active.Children[i].Name} (Content: {active.Children[i].Children.Length})\n";
                }
                for (int i = 0; i < active.Files.Length; i++)
                {
                    content += $"File[{i}]: {active.Files[i].Name} (Size: {active.Files[i].Size} bytes)\n";
                }

                Console.WriteLine(content);
            }
            else
            {
                Console.WriteLine("Command not found!");
            }
        }

        private static string DisplayActiveDir(SecureDirectory dir)
        {
            return $"{dir.ActiveDirectory.SecurePath} >";
        }

        private struct Commands
        {
            public const string Exit = "exit";
            public const string Help = "help";

            public const string AddFile = "addF";
            public const string DeleteFile = "delF";
            public const string MoveFile = "movF";

            public const string AddDirectory = "addD";
            public const string DeleteDirectory = "delD";
            public const string MoveDirectory = "movD";
            public const string ViewDirectoryContent = "dir";

            public const string PreviousDirectory = "prev";
            public const string NextDirectory = "next";
            public const string ViewContent = "dir";
        }
    }
}
