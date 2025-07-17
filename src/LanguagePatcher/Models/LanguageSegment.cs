using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace LanguagePatcher.Models;

/// <summary>
/// Represents a language segment for patching language-specific data.
/// </summary>
public class LanguageSegment
{
    private static readonly Regex StructureRegex = new(@"^(?=.{" + (LoadingPos + 3) + @"}Loading)(?=.{" + (LSPos + 3) + @"}LS)(?=.{" + (FEPos + 3)  + @"}FE)",
        RegexOptions.Compiled);

    public const int LanguageSegmentLength = 475;

    private const int LoadingPos = 144;
    private const int LSPos = 252;
    private const int FEPos = 356;
    private const int NamePos = 468;

    /// <summary>
    /// Gets the underlying byte array.
    /// </summary>
    public byte[] Bytes { get; private set; }


    public LanguageSegment(byte[] bytes)
    {
        if (!Validate(bytes))
        {
            throw new ArgumentException("Invalid language segment bytes.", nameof(bytes));
        }
        Bytes = bytes;
    }

    /// <summary>
    /// Sets the language in the segment.
    /// </summary>
    public LanguageSegment(WormsLanguage language)
    {
        var bArray = new byte[LanguageSegmentLength];
        var langName = language.ToString();
        if(langName.Length > 7)
        {
            langName = langName[..7];
        }
        var shortLang = langName[..3];
        Dictionary<string, int> files = new()
        {
            [$"{shortLang}Loading"] = LoadingPos,
            [$"{shortLang}LS"] = LSPos,
            [$"{shortLang}FE"] = FEPos,
            [langName] = NamePos
        };
        foreach (var file in files)
        {
            AddLanguageFile(bArray, file.Key, file.Value);
        }

        if (!Validate(bArray))
        {
            throw new ArgumentException("Invalid new language segment bytes.", nameof(bArray));
        }
        Bytes = bArray;
    }

    public static bool TryCreateNewLanguageSegment(WormsLanguage language, out LanguageSegment? languageSegment)
    {
        try
        {
            languageSegment = new LanguageSegment(language);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: When creating new language segment: {ex.Message}");
            languageSegment = null!;
            return false;
        }
    }
    public static void AddLanguageFile(byte[] bArray, string name, int position)
    {
        for (int i = position, j = 0; j < name.Length && i < LanguageSegmentLength; i++, j++)
            bArray[i] = (byte)name[j];
    }

    private static bool Validate(byte[] bytes)
    {
        if (bytes == null)
        {
            Console.WriteLine("Error: bytes is null.");
            return false;
        }
        if (bytes.Length != LanguageSegmentLength)
        {
            Console.WriteLine("Error: The language segment has the wrong length.");
            return false;
        }

        string text = Encoding.ASCII.GetString(bytes);
        if (!StructureRegex.IsMatch(text))
        {
            Console.WriteLine("Error: The language segment has the wrong structure.");
            return false;
        }
        return true;
    }

}
