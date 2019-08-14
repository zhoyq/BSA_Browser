﻿using SharpBSABA2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace BSA_Browser_CLI
{
    enum Filtering
    {
        None, Simple, Regex
    }

    [Flags]
    enum ListOptions
    {
        None = 0,
        Archive = 1,
        FullPath = 2,
        FileSize = 4
    }

    class Arguments
    {
        public bool Extract { get; private set; }
        public bool Help { get; private set; }
        public bool List { get; private set; }
        public bool ATI { get; private set; }
        public bool IgnoreErrors { get; private set; }

        public Filtering Filtering { get; private set; } = Filtering.None;
        public ListOptions ListOptions { get; private set; } = ListOptions.None;

        public string Destination { get; private set; }
        public string FilterString { get; private set; }

        public Encoding Encoding { get; private set; } = Encoding.UTF7;

        public IReadOnlyCollection<string> Inputs { get; private set; }

        public Arguments(string[] args)
        {
            var input = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.StartsWith("/") || arg.StartsWith("-") || arg.StartsWith("--"))
                {
                    switch (arg.ToLower().Split(':', '=')[0])
                    {
                        case "/?":
                        case "-h":
                        case "--help":
                            this.Help = true;
                            break;
                        case "/e":
                        case "-e":
                            this.Extract = true;
                            break;
                        case "/f":
                        case "-f":
                            this.Filtering = Filtering.Simple;
                            this.FilterString = args[++i];
                            break;
                        case "/i":
                        case "-i":
                            this.IgnoreErrors = true;
                            break;
                        case "/l":
                        case "-l":
                            this.List = true;

                            char[] options = arg.Split(':', '=').Last().ToLower().ToCharArray();

                            if (options.Contains('a')) this.ListOptions = ListOptions.Archive;
                            if (options.Contains('f')) this.ListOptions = (this.ListOptions | ListOptions.FullPath);
                            if (options.Contains('s')) this.ListOptions = (this.ListOptions | ListOptions.FileSize);

                            break;
                        case "/ati":
                        case "--ati":
                            this.ATI = true;
                            break;
                        case "/regex":
                        case "--regex":
                            this.Filtering = Filtering.Regex;
                            this.FilterString = args[++i];
                            break;
                        case "/enc":
                        case "--enc":
                        case "/encoding":
                        case "--encoding":
                            this.Encoding = this.ParseEncoding(args[++i]);
                            break;
                        default:
                            throw new ArgumentException("Unrecognized argument: " + arg);
                    }
                }
                else
                {
                    if (i == args.Length - 1 && this.Extract) // Last item is destination when extracting
                    {
                        if (Directory.Exists(arg))
                            this.Destination = arg;
                        else
                            throw new DirectoryNotFoundException("Destination directory not found.");
                    }
                    else if (File.Exists(arg))
                    {
                        input.Add(arg);
                    }
                    else
                    {
                        throw new FileNotFoundException("File not found.", arg);
                    }
                }
            }

            this.Inputs = input.AsReadOnly();
        }

        private Encoding ParseEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "utf7": return Encoding.UTF7;
                case "system": return Encoding.Default;
                case "ascii": return Encoding.ASCII;
                case "unicode": return Encoding.Unicode;
                case "utf32": return Encoding.UTF32;
                case "utf8": return Encoding.UTF8;
                default:
                    throw new ArgumentException("Unrecognized encoding: " + encoding, nameof(encoding));
            }
        }
    }

    class Program
    {
        private const int ERROR_INVALID_FUNCTION = 1;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_PATH_NOT_FOUND = 3;
        private const int ERROR_BAD_ARGUMENTS = 160;

        static Arguments _arguments;
        static Regex _regex;
        static WildcardPattern _pattern;

        static void Main(string[] args)
        {
            try
            {
                _arguments = new Arguments(args);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Input file not found: " + ex.FileName);
                Environment.ExitCode = ERROR_FILE_NOT_FOUND;
                goto exit;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = ERROR_PATH_NOT_FOUND;
                goto exit;
            }

            // Print help screen. Ignore other arguments
            if (args.Length == 0 || _arguments.Help)
            {
                PrintHelp();
                goto exit;
            }

            if (_arguments.Inputs.Count == 0)
            {
                Console.WriteLine("No input file(s) found");
                Environment.ExitCode = ERROR_FILE_NOT_FOUND;
                goto exit;
            }

            // Setup filtering
            if (_arguments.Filtering != Filtering.None)
            {
                if (_arguments.Filtering == Filtering.Simple)
                {
                    _pattern = new WildcardPattern(
                        $"*{WildcardPattern.Escape(_arguments.FilterString).Replace("`*", "*")}*",
                        WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
                }
                else if (_arguments.Filtering == Filtering.Regex)
                {
                    try
                    {
                        _regex = new Regex(_arguments.FilterString, RegexOptions.Compiled | RegexOptions.Singleline);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid regex filter string");
                        Environment.ExitCode = ERROR_BAD_ARGUMENTS;
                        goto exit;
                    }
                }
            }

            if (_arguments.List || (!_arguments.List && !_arguments.Extract && !_arguments.Help))
            {
                try
                {
                    PrintFileList(_arguments.Inputs.ToList(), _arguments.ListOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured opening archive:");
                    Console.WriteLine(ex.Message);
                    Environment.ExitCode = ERROR_INVALID_FUNCTION;
                }
            }

            if (_arguments.Extract)
            {
                try
                {
                    ExtractFiles(_arguments.Inputs.ToList(), _arguments.ATI, _arguments.Destination);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured opening archive:");
                    Console.WriteLine(ex.Message);
                    Environment.ExitCode = ERROR_INVALID_FUNCTION;
                }
            }

        exit:;

#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
#endif
        }

        static void ExtractFiles(List<string> archives, bool ati, string destination)
        {
            archives.ForEach(archivePath =>
            {
                Archive archive = null;

                try
                {
                    archive = OpenArchive(archivePath, ati);
                }
                catch (Exception ex)
                {
                    if (!_arguments.IgnoreErrors)
                        throw ex;
                    else
                        Console.WriteLine($"An error occured opening '{Path.GetFileName(archivePath)}'. Skipping...");
                }

                int count = 0;
                int total = archive.Files.Count(x => Filter(x.FullPath));
                int line = -1;
                int prevLength = 0;

                // Some Console properties might not be available in certain situations, 
                // e.g. when redirecting stdout. To prevent crashing, setting the cursor position should only
                // be done if there actually is a cursor to be set.
                try
                {
                    line = Console.CursorTop;
                }
                catch (IOException) { }

                foreach (var entry in archive.Files)
                {
                    if (!Filter(entry.FullPath))
                        continue;

                    string output = $"Extracting: {++count}/{total} - {entry.FullPath}".PadRight(prevLength);

                    if (line > -1)
                    {
                        Console.SetCursorPosition(0, line);
                        Console.Write(output);
                    }
                    else
                    {
                        Console.WriteLine(output);
                    }
                    prevLength = output.Length;

                    try
                    {
                        entry.Extract(destination, true);
                    }
                    catch (Exception ex)
                    {
                        if (!_arguments.IgnoreErrors)
                            throw ex;
                        else
                            Console.WriteLine($"An error occured extracting '{entry.FullPath}'. Skipping...");
                    }
                }

                Console.WriteLine();
            });
        }

        static Archive OpenArchive(string file, bool ati)
        {
            Archive archive = null;
            string extension = Path.GetExtension(file);

            switch (extension.ToLower())
            {
                case ".bsa":
                case ".dat":
                    archive = new SharpBSABA2.BSAUtil.BSA(file, _arguments.Encoding);
                    break;
                case ".ba2":
                    archive = new SharpBSABA2.BA2Util.BA2(file, _arguments.Encoding) { UseATIFourCC = ati };
                    break;
                default:
                    throw new Exception($"Unrecognized archive file type ({extension}).");
            }

            archive.Files.Sort((a, b) => string.CompareOrdinal(a.LowerPath, b.LowerPath));
            return archive;
        }

        static bool Filter(string input)
        {
            if (_arguments.Filtering == Filtering.Simple)
            {
                return _pattern.IsMatch(input);
            }
            else if (_arguments.Filtering == Filtering.Regex)
            {
                return _regex.IsMatch(input);
            }

            return true;
        }

        static void PrintFileList(List<string> archives, ListOptions options)
        {
            archives.ForEach(archivePath =>
            {
                if (archives.Count > 1)
                    Console.WriteLine($"{Path.GetFileName(archivePath)}:");

                Archive archive = null;

                try
                {
                    archive = OpenArchive(archivePath, false);
                }
                catch (Exception ex)
                {
                    if (!_arguments.IgnoreErrors)
                        throw ex;
                    else
                        Console.WriteLine($"An error occured opening '{Path.GetFileName(archivePath)}'. Skipping...");
                }

                bool filesize = false;
                string prefix = string.Empty;

                if (options.HasFlag(ListOptions.Archive))
                    prefix = Path.GetFileName(archive.FullPath);

                if (options.HasFlag(ListOptions.FullPath))
                    prefix = Path.GetFullPath(archive.FullPath);

                filesize = options.HasFlag(ListOptions.FileSize);

                foreach (var entry in archive.Files)
                {
                    if (!Filter(entry.FullPath))
                        continue;

                    string indent = string.IsNullOrEmpty(prefix) && archives.Count > 1 ? "\t" : string.Empty;
                    string filesizeString = filesize ? entry.RealSize + "\t\t" : string.Empty;

                    Console.WriteLine($"{indent}{filesizeString}{Path.Combine(prefix, entry.FullPath)}");
                }

                Console.WriteLine();
            });
        }

        static void PrintHelp()
        {
            Console.WriteLine("Extract or list files inside .bsa and .ba2 archives.");
            Console.WriteLine();
            Console.WriteLine("bsab [OPTIONS] FILE [FILE...] [DESTINATION]");
            Console.WriteLine();
            Console.WriteLine("  -h, --help             Display this help page");
            Console.WriteLine("  -i                     Ignore errors with opening archives or extracting files");
            Console.WriteLine("  -e                     Extract all files. Options:");
            Console.WriteLine("  -l:[OPTIONS]           List all files");
            Console.WriteLine("     options               A   Prepend each line with archive filename");
            Console.WriteLine("                           F   Prepend each line with full archive file path");
            Console.WriteLine("                           S   Display file size");
            Console.WriteLine("  -f FILTER              Simple filtering. Wildcard supported");
            Console.WriteLine("  --regex REGEX          Regex filtering");
            Console.WriteLine("  --ati                  Use ATI header for textures");
            Console.WriteLine("  --encoding ENCODING    Set encoding to use");
            Console.WriteLine("     encodings             utf7     (Default)");
            Console.WriteLine("                           system   Use System's default encoding");
            Console.WriteLine("                           ascii");
            Console.WriteLine("                           unicode");
            Console.WriteLine("                           utf32");
            Console.WriteLine("                           utf8");
            Console.WriteLine();
        }
    }
}
