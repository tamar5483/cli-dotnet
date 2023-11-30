
using System.CommandLine;
using System.Reflection;

//פקודת השורש
var rootCommand = new RootCommand("This is a root command");

//שתי פקודות-הראשונה אוספת את כל הקבצים לקובץ אחד לפי האופתציות ששלוחים לה, והשנייה קולטת מהמשתמש את האופציות ויוצרת קובץ עם הפקודה הראשונה והאופציות שקלטה משמשתמש
var bundle = new Command("bundle", "bundle files to one file");
var createRsp = new Command("create-rsp", "create rsp file");

//אופציות, לכל אופציה יש גם אליאס - קיצור
//שם הקובץ שיווצר ואליו יועתקו שאר הקבצים
var outputOption = new Option<FileInfo>("--output", "specify the output bundle file name");
outputOption.AddAlias("-o");
//האם לכתוב מעל כל חלק בקובץ החדש מאיזה קובץ הוא הועתק
var noteOption = new Option<bool>("--note", "if write code source");
noteOption.AddAlias("-n");
//האם למחוק שורות ריקות 
var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "if write code source");
removeEmptyLinesOption.AddAlias("-r");
//איך למיין-לפי שם או שפה
var sortOption = new Option<string>("--sort", () => "name", "sort name or language");
sortOption.AddAlias("-s");
//שם יוצר הקובץ שיהיה כתוב למעלה
var authorOption = new Option<string>("--author", "add author name");
authorOption.AddAlias("-a");
//רשימת שפות שהקבצים שלהם יועתקו-חובה
//שרושור של סיומות קבצים עם פסיקים בינהם
var languageOption = new Option<string>("--language", "specify programming languages (all for all languages)");
languageOption.IsRequired = true;
languageOption.AddAlias("-l");

//הוספת כל האופציות לפקודה
bundle.AddOption(languageOption);
bundle.AddOption(outputOption);
bundle.AddOption(noteOption);
bundle.AddOption(sortOption);
bundle.AddOption(authorOption);
bundle.AddOption(removeEmptyLinesOption);

//הפונקציה שמתבצעת בפקודה השנייה
createRsp.SetHandler(() =>
{
    //קולטת מהשתמש ערך לכל האופציות של הפקודה הראשונה ויוצרת קובץ עם הפקודה
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

//הפונקציה שמתבצעת בפקודה הראשונה
bundle.SetHandler((output, language, note, sort, removeEmptyLines, author) =>
{
    //המרת השפות לרשימה
    var languages = language.Split(",").ToList();
    //אם המשתמש רוצה את כל השפות, אוסף את כל הקבצים בתקייה
    if (languages.Contains("all"))
    {
        languages = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories)
            .Select(file => Path.GetExtension(file).Remove(0, 1))
            .ToList();
    }
    //אוסף את כל הקבצים של השפות שהמשתמש ביקש
    var filesToBundle = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories)
           .Where(file =>
      languages.Contains(Path.GetExtension(file).Remove(0, 1)))
      .ToList();

    //ממיין
    if (sort == "name")
    {
        filesToBundle.Sort((a, b) => a.CompareTo(b));
    }
    // סדר לפי סוג השפה
    else if (sort == "language")
    {
        filesToBundle.Sort((a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));
    }

    //העתקה לקובץ אחד
    try
    {
        using (StreamWriter sw = new StreamWriter(output.FullName))
        {
            //אם המשתמש הכניס שם יוצר- כותב אותו בראש הקובץ
            if (author != null&&author!="")
                sw.WriteLine("//author: " + author);

            foreach (string filePath in filesToBundle)
            {
                sw.WriteLine();
                sw.WriteLine();
                //אם המשתמש הכניס note כותב את קובץ המקור לפני כל קטע
                if (note)
                sw.WriteLine("//" + filePath);
                sw.WriteLine();
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //אן השורה לא ריקה, או שהמשתמש לא ביקש למחוק שורות ריקות
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

