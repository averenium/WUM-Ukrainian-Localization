using System;
using System.IO;
using System.Threading.Tasks;

namespace LanguagePatcher.Helpers;

public static class FileHelper
{
    public static async ValueTask<bool> TryBackupFileAsync(string filePath, string backupDir = "backups")
    {
        try
        {
            ArgumentNullException.ThrowIfNull(backupDir);
            Directory.CreateDirectory(backupDir);
            string nowPath = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            backupDir = Path.Combine(backupDir, nowPath);
            Directory.CreateDirectory(backupDir);

            string backupPath = Path.Combine(backupDir, Path.GetFileName(filePath));
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            using (FileStream sourceStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
            using (FileStream destStream = new(backupPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                await sourceStream.CopyToAsync(destStream).ConfigureAwait(false);
            }
            Console.WriteLine($"Backup created successfully: {backupPath}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: Creating backup failed: {ex.Message}");
            return false;
        }
    }
}