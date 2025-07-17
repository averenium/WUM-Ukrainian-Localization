using LanguagePatcher.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LanguagePatcher.Helpers;

internal static class LanguageHelper
{
    public const int LanguageSegmentStartOffset = 0x44FC30;

    /// <summary>
    /// Tries to asynchronously read a language segment from the specified file.
    /// </summary>
    private static async ValueTask<(bool Success, byte[]? Segment)> TryReadLanguageSegmentBytesAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Error: filePath is null or empty.");
                return (false, null);
            }

            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            if (stream.Length < LanguageSegmentStartOffset + LanguageSegment.LanguageSegmentLength)
            {
                Console.WriteLine("Error: File is too small to contain a language segment at the specified offset.");
                return (false, null);
            }

            stream.Seek(LanguageSegmentStartOffset, SeekOrigin.Begin);
            byte[] buffer = new byte[LanguageSegment.LanguageSegmentLength];
            int read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (read != buffer.Length)
            {
                Console.WriteLine("Error: Could not read the full language segment.");
                return (false, null);
            }

            return (true, buffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while reading language segment: {ex.Message}");
            return (false, null);
        }
    }

    /// <summary>
    /// Tries to asynchronously read a language segment and create a LanguageSegment object.
    /// </summary>
    public static async ValueTask<bool> TryValidateFileWithLanguageSegment(string filePath)
    {
        var (success, bytes) = await TryReadLanguageSegmentBytesAsync(filePath);
        if (!success || bytes is null)
            return false;

        try
        {
            var segment = new LanguageSegment(bytes);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: Invalid language segment: {ex.Message}");
            return false;
        }
    
    }



    /// <summary>
    /// Tries to asynchronously write a language segment to the specified file.
    /// </summary>
    private static async ValueTask<bool> TryWriteLanguageSegmentBytesAsync(string filePath, byte[] langSegmentBytes)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Error: filePath is null or empty.");
                return false;
            }

            if (langSegmentBytes is null)
            {
                Console.WriteLine("Error: langSegmentBytes is null.");
                return false;
            }

            if (langSegmentBytes.Length != LanguageSegment.LanguageSegmentLength)
            {
                Console.WriteLine($"Error: langSegmentBytes must be exactly {LanguageSegment.LanguageSegmentLength} bytes.");
                return false;
            }

            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
            if (stream.Length < LanguageSegmentStartOffset + LanguageSegment.LanguageSegmentLength)
            {
                Console.WriteLine("Error: File is too small to contain a language segment at the specified offset.");
                return false;
            }

            stream.Seek(LanguageSegmentStartOffset, SeekOrigin.Begin);
            await stream.WriteAsync(langSegmentBytes, 0, langSegmentBytes.Length).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: Failed writing language segment: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Tries to asynchronously write a language segment to the specified file.
    /// </summary>
    public static async ValueTask<bool> TryWriteLanguageSegmentAsync(string filePath, LanguageSegment languageSegment)
    {
        try
        {
            return await TryWriteLanguageSegmentBytesAsync(filePath, languageSegment.Bytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: Failed writing language segment: {ex.Message}");
            return false;
        }
    }
}
