using LanguagePatcher.Helpers;
using LanguagePatcher.Models;
using System;
using System.IO;


    var logFilePath = "languagePatcherLog.txt";
    var logWriter = new StreamWriter(logFilePath, append: false) { AutoFlush = true };
    Console.SetOut(logWriter);


if (args.Length == 0)
{
    Console.WriteLine("Drag and drop a file onto this executable to process it.");
    Console.WriteLine("Or use: <exe> <file> <language>");
    Console.WriteLine("For help: <exe> --help");
    return;
}

if (args.Length > 1 && (args[1].Equals("--help", StringComparison.OrdinalIgnoreCase) || args[1].Equals("/?", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine("Available languages:");
    foreach (var lang in Enum.GetNames(typeof(WormsLanguage)))
        Console.WriteLine($"- {lang}");
    return;
}

string filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

WormsLanguage language = WormsLanguage.Ukraini;
if (args.Length > 1)
{
    if (!Enum.TryParse<WormsLanguage>(args[1], true, out language))
    {
        Console.WriteLine($"Unknown language: {args[1]}");
        Console.WriteLine("Available languages:");
        foreach (var lang in Enum.GetNames(typeof(WormsLanguage)))
            Console.WriteLine($"- {lang}");
        return;
    }
}

Console.WriteLine($"File received: {filePath}");
FileInfo info = new(filePath);
Console.WriteLine($"File size: {info.Length} bytes");

if (info.Length < LanguageSegment.LanguageSegmentLength)
{
    Console.WriteLine("The file is too small to be a valid language segment.");
    return;
}

if (!await FileHelper.TryBackupFileAsync(filePath))
    return;

Console.WriteLine($"Setting language to: {language}");
if (!await LanguageHelper.TryValidateFileWithLanguageSegment(filePath))
{
    return;
}

if (!LanguageSegment.TryCreateNewLanguageSegment(language, out var newLanguageSegment) || newLanguageSegment == null)
{
    return;
}

if (await LanguageHelper.TryWriteLanguageSegmentAsync(filePath, newLanguageSegment))
{
    Console.WriteLine($"The language successfully changed to {language}");
}
