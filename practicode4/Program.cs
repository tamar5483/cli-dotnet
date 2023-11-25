
using System.CommandLine;
using System.Reflection;

var rootCommand = new RootCommand("This is a root command");

var bundle = new Command("bundle", "bundle files to one file");
var createRsp = new Command("create-rsp", "create rsp file");

var outputOption = new Option<FileInfo>("--output", "specify the output bundle file name");
outputOption.AddAlias("-o");
var noteOption = new Option<bool>("--note", "if write code source");
noteOption.AddAlias("-n");
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "if write code source");
removeEmptyLinesOption.AddAlias("-r");
var sortOption = new Option<string>("--sort", () => "name", "sort name or language");
sortOption.AddAlias("-s");
var authorOption = new Option<string>("--author", "add author name");
authorOption.AddAlias("-a");
var languageOption = new Option<string>("--language", "specify programming languages (all for all languages)");
languageOption.IsRequired = true;
languageOption.AddAlias("-l");

bundle.AddOption(languageOption);
bundle.AddOption(outputOption);
bundle.AddOption(noteOption);
bundle.AddOption(sortOption);
bundle.AddOption(authorOption);
bundle.AddOption(removeEmptyLinesOption);

createRsp.SetHandler(() =>
{
    using (StreamWriter sw = new StreamWriter("rsp-file.rst"))
    {
        sw.WriteLine("bundle ");
        for (int i = 0; i < bundle.Options.Count; i++)
        {
            Console.WriteLine("enter value for: " + bundle.Options[i].Name);
            var a = Console.ReadLine();
            if(a != "")
            sw.WriteLine($"--{bundle.Options[i].Name} {a} ");
        }
        sw.Close();
    }
});

bundle.SetHandler((output, language, note, sort, removeEmptyLines, author) =>
{

    var languages = language.Split(",").ToList();
    if (languages.Contains("all"))
    {
        languages = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories)
            .Select(file => Path.GetExtension(file).Remove(0, 1))
            .ToList();
    }

    var filesToBundle = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories)
           .Where(file =>
      languages.Contains(Path.GetExtension(file).Remove(0, 1)))
      .ToList();

    if (sort == "name")
    {
        filesToBundle.Sort((a, b) => a.CompareTo(b));
    }
    // סדר לפי סוג השפה
    else if (sort == "language")
    {
        filesToBundle.Sort((a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));
    }

    try
    {
        using (StreamWriter sw = new StreamWriter(output.FullName))
        {
            if (author != null&&author!="")
                sw.WriteLine("//author: " + author);

            foreach (string filePath in filesToBundle)
            {
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("//" + filePath);
                sw.WriteLine();
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim().Length > 0 || !removeEmptyLines) // Check if the line is not empty
                        {
                            sw.WriteLine(line);
                        }
                    }
                    sr.Close();
                }
            }
            sw.Close();
            Console.WriteLine("file created");
        }
    }
    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine("Error: Path not found");
    }


}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

rootCommand.AddCommand(bundle);
rootCommand.AddCommand(createRsp);
rootCommand.InvokeAsync(args);

