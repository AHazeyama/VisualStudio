// _/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
// _/ Name      : exrm
// _/ InterFace : Windows Presentation Foundation (WPF)
// _/ Function  : Exclusive remove tool
// _/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExrmWpf
{
    internal static class ExrmCore
    {
        public static string BackupDirOf(string dir) => dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + ".bk";

        public static List<string> ParseKeepWords(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
            return csv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
        }

        public static void CreateBackup(string sourceDir, Action<string> log)
        {
            string backupDir = BackupDirOf(sourceDir);

            if (Directory.Exists(backupDir))
            {
                log($"[bk] delete old backup: {backupDir}");
                Directory.Delete(backupDir, recursive: true);
            }

            log($"[bk] copy: {sourceDir} -> {backupDir}");
            CopyDirectory(sourceDir, backupDir);
            log("[bk] done");
        }

        public static void Undo(string sourceDir, Action<string> log)
        {
            string backupDir = BackupDirOf(sourceDir);

            if (!Directory.Exists(backupDir))
            {
                log("バックアップされていないため、リカバリ出来ません。");
                return;
            }

            log($"[undo] delete current: {sourceDir}");
            Directory.Delete(sourceDir, recursive: true);

            log($"[undo] restore: {backupDir} -> {sourceDir}");
            Directory.Move(backupDir, sourceDir);

            log("リカバリ完了しました。");
        }

        public static void CleanupBackup(string sourceDir, Action<string> log)
        {
            string backupDir = BackupDirOf(sourceDir);
            if (Directory.Exists(backupDir))
            {
                log($"[bk] delete: {backupDir}");
                Directory.Delete(backupDir, recursive: true);
            }
        }

        public static void ProcessDirectory(string targetDir, List<string> keepWords, bool recursive, Action<string> log)
        {
            Walk(targetDir, keepWords, recursive, indent: "", log);
        }

        private static void Walk(string dir, List<string> keepWords, bool recursive, string indent, Action<string> log)
        {
            log($"{indent}=> {dir}");

            // "."開始のファイルは除外
            var entries = Directory.EnumerateFileSystemEntries(dir)
                .Where(p => !Path.GetFileName(p).StartsWith(".", StringComparison.Ordinal));

            string nextIndent = indent + "    ";

            foreach (var path in entries)
            {
                string name = Path.GetFileName(path);
                bool keep = keepWords.Any(w => name.Contains(w, StringComparison.Ordinal));
                
                if (keep)
                {
                    // フォルダは「自分は残す」が、配下は recursive なら処理する
                    if (Directory.Exists(path) && recursive)
                    {
                        log($"{nextIndent}un remove : {name}");
                        Walk(path, keepWords, true, nextIndent, log);
                        continue;
                    }
                
                    // ファイル(または recursive=false のフォルダ)は従来どおり触らない
                    log($"{nextIndent}un remove : {name}");
                    continue;
                }
                try
                {
                    if (Directory.Exists(path))
                    {
                        if (recursive)
                        {
                            Walk(path, keepWords, true, nextIndent, log);

                            // 中身が空になっていれば削除(中身が残っている/権限NGなら例外)
                            Directory.Delete(path, recursive: false);
                            log($"{nextIndent}rmdir     : {name}");
                        }
                        else
                        {
                            log($"{nextIndent}un remove : {name}");
                        }
                    }
                    else
                    {
                        File.Delete(path);
                        log($"{nextIndent}remove    : {name}");
                    }
                }
                catch
                {
                    log($"{nextIndent}un remove : {name}");
                }
            }

            log($"{indent}<= {dir}");
        }

        private static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.EnumerateFiles(source))
            {
                var to = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, to, overwrite: true);
            }

            foreach (var dir in Directory.EnumerateDirectories(source))
            {
                // バックアップの中にさらに .bk を作らない
                if (dir.EndsWith(".bk", StringComparison.OrdinalIgnoreCase)) continue;

                var to = Path.Combine(dest, Path.GetFileName(dir));
                CopyDirectory(dir, to);
            }
        }
    }
}
