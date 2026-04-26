// _/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
// _/ Name      : exrm
// _/ InterFace : Windows Presentation Foundation (WPF)
// _/ Function  : Exclusive remove tool
// _/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using MessageBox = System.Windows.MessageBox;

using Forms = System.Windows.Forms;
using MediaBrushes = System.Windows.Media.Brushes;



namespace ExrmWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowStartupMessage();
        }

        private void Log(string s)
        {
            LogInfo(s);
        }
        private void ShowStartupMessage()
        {
            LogTextBox.Document.Blocks.Clear();
        
            LogInfo("Exclusive removal tool");
            LogInfo("");
        
            // "!!注意!!"を色変更+太字
            AppendMixedLine(
                ("!! 注意 !!", MediaBrushes.Magenta, FontWeights.Bold, FontStyles.Normal)
            );
        
            // 説明文 "含まない"だけ色変更+太字
            AppendMixedLine(
                ("  このツールは \"not removed words\" に指定された文字列を ", MediaBrushes.DeepSkyBlue, FontWeights.Normal, FontStyles.Normal),
                ("含まない", MediaBrushes.Magenta, FontWeights.Bold, FontStyles.Normal),
                (" ファイル (ディレクトリ) を", MediaBrushes.DeepSkyBlue, FontWeights.Normal, FontStyles.Normal)
            );
        
            AppendMixedLine(
                ("  問答無用で削除します !!", MediaBrushes.Red, FontWeights.Normal, FontStyles.Normal)
            );
        
            AppendMixedLine(
                ("  一応、Undoは可能です。", MediaBrushes.Yellow, FontWeights.Normal, FontStyles.Normal)
            );
        
            AppendMixedLine(
                ("  使用方法は [Help] ボタンで表示されます。", MediaBrushes.DeepSkyBlue, FontWeights.Normal, FontStyles.Normal)
            );
        }



        private static List<string> BuildTreeLines(string rootDir, int maxDepth /* -1=unlimited */)
        {
            var lines = new List<string>();
            if (!Directory.Exists(rootDir))
            {
                lines.Add(rootDir);
                lines.Add("└─ (not found)");
                return lines;
            }
        
            lines.Add(rootDir);
        
            void Walk(string dir, string indent, int depth)
            {
                if (maxDepth >= 0 && depth >= maxDepth) return;
        
                IEnumerable<string> dirs;
                IEnumerable<string> files;
        
                try
                {
                    dirs = Directory.EnumerateDirectories(dir)
                        .Where(p => !Path.GetFileName(p).StartsWith(".", System.StringComparison.Ordinal))
                        .OrderBy(p => Path.GetFileName(p), System.StringComparer.OrdinalIgnoreCase);
        
                    files = Directory.EnumerateFiles(dir)
                        .Where(p => !Path.GetFileName(p).StartsWith(".", System.StringComparison.Ordinal))
                        .OrderBy(p => Path.GetFileName(p), System.StringComparer.OrdinalIgnoreCase);
                }
                catch (System.UnauthorizedAccessException)
                {
                    lines.Add(indent + "└─ <access denied>");
                    return;
                }
                catch (System.Exception ex)
                {
                    lines.Add(indent + "└─ <error: " + ex.Message + ">");
                    return;
                }
        
                var entries = dirs.Concat(files).ToList();
                for (int i = 0; i < entries.Count; i++)
                {
                    bool isLast = (i == entries.Count - 1);
                    string branch = isLast ? "└─ " : "├─ ";
                    string nextIndent = indent + (isLast ? "   " : "│  ");
        
                    var path = entries[i];
                    var name = Path.GetFileName(path);
        
                    lines.Add(indent + branch + name);
        
                    if (Directory.Exists(path))
                    {
                        Walk(path, nextIndent, depth + 1);
                    }
                }
            }
        
            Walk(rootDir, "", 0);
            return lines;
        }




        private async Task RefreshDirListingAsync(string dir)
        {
            bool recursive = (RecursiveCheckBox.IsChecked == true);
    
            // tree表示：Recursive="ON":無制限, "OFF":1階層まで
            int maxDepth = recursive ? -1 : 1;
    
            await Task.Run(() =>
            {
                try
                {
                    var lines = BuildTreeLines(dir, maxDepth);
        
                    Dispatcher.Invoke(() =>
                    {
                        LogTextBox.Document.Blocks.Clear();
                        foreach (var line in lines)
                            Log(line);
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogTextBox.Document.Blocks.Clear();
                        Log("一覧表示に失敗: " + ex.Message);
                    });
                }
            });
        }

        private async void Select_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new Forms.FolderBrowserDialog
            {
                Description = "dir choose",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false,
            };
        
            if (dlg.ShowDialog() != Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dlg.SelectedPath))
                return;
        
            string dir = dlg.SelectedPath;
            ExecDirTextBox.Text = dir;
        
            await Task.Run(() =>
            {
                try
                {
                    ExrmCore.CreateBackup(dir, Log);
                }
                catch (Exception ex)
                {
                    Log("バックアップ作成に失敗: " + ex.Message);
                }
            });
        
            await RefreshDirListingAsync(dir);
        }



        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ExecDirTextBox.Text = "";
            NotRemovedWordsTextBox.Text = "";
            LogTextBox.Document.Blocks.Clear();
            Log("no message !");
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Document.Blocks.Clear();
            Log("name : exrm");
            Log("function : Exclusive Removal Tool (排他的ファイル (ディレクトリ) 削除ツール)");
            Log("usage :");
            Log("  Exec directory       : 指定ディレクトリ以下が処理対象となります。");
            Log("  not removed words    : 削除しないファイル (ディレクトリ) 名に含まれる文字列を指定して下さい。");
            Log("                       : カンマ区切りで複数指定できます。");
            Log("  Recursive processing : 下位階層のディレクトリも処理対象とします。");
        }

        private async void ExRemove_Click(object sender, RoutedEventArgs e)
        {
            string dir = ExecDirTextBox.Text?.Trim() ?? "";
            string wordsCsv = NotRemovedWordsTextBox.Text ?? "";

            bool hasError = false;
            LogTextBox.Document.Blocks.Clear();

            if (string.IsNullOrWhiteSpace(dir))
            {
                Log("'Exec directory' を指定して下さい。");
                hasError = true;
            }
            if (string.IsNullOrWhiteSpace(wordsCsv))
            {
                Log("'not removed words' を指定して下さい。");
                hasError = true;
            }
            if (hasError) return;

            if (!Directory.Exists(dir))
            {
                Log("指定フォルダが存在しません: " + dir);
                return;
            }

            var keepWords = ExrmCore.ParseKeepWords(wordsCsv);
            bool recursive = (RecursiveCheckBox.IsChecked == true);

            // 事故防止：最終確認
            // やっぱり問答無用!!
/*            
            var confirm = MessageBox.Show(
                "削除を実行します。\n" +
                "指定ワードを「含まない」ファイル/フォルダを削除します。\n\n" +
                $"対象: {dir}\n" +
                $"not removed words: {string.Join(", ", keepWords)}\n" +
                $"Recursive: {recursive}\n\n" +
                "よろしいですか？",
                "Confirm ExRemove",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                Log("キャンセルしました。");
                return;
            }
*/

            await Task.Run(() =>
            {
                try
                {
                    ExrmCore.CreateBackup(dir, Log);   // 削除前に毎回バックアップ
                    ExrmCore.ProcessDirectory(dir, keepWords, recursive, Log);
                }
                catch (Exception ex)
                {
                    Log("処理中にエラー: " + ex.Message);
                }
            });

            await RefreshDirListingAsync(dir);
        }

        private async void Undo_Click(object sender, RoutedEventArgs e)
        {
            string dir = ExecDirTextBox.Text?.Trim() ?? "";
            LogTextBox.Document.Blocks.Clear();

            if (string.IsNullOrWhiteSpace(dir))
            {
                Log("'Exec directory' を指定して下さい。");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    ExrmCore.Undo(dir, Log);
                }
                catch (Exception ex)
                {
                    Log("Undo中にエラー: " + ex.Message);
                }
            });

            if (Directory.Exists(dir))
                await RefreshDirListingAsync(dir);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ".bk"消去
                var dir = ExecDirTextBox.Text?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                    ExrmCore.CleanupBackup(dir, Log);
            }
            catch
            {
                // 終了は常に成功扱い(消去の成否とは無関係に)
            }

            Close();
        }
        private void LogInfo(string text)
        {
            AppendLog(text, MediaBrushes.DeepSkyBlue, FontWeights.Normal, FontStyles.Normal);
        }
        
        private void LogWarn(string text)
        {
            AppendLog(text, MediaBrushes.Magenta, FontWeights.Normal, FontStyles.Normal);
        }
        
        private void LogWarnStrong(string text)
        {
            AppendLog(text, MediaBrushes.Magenta, FontWeights.Bold, FontStyles.Italic);
        }
        
        private void AppendLog( string text, System.Windows.Media.Brush color, System.Windows.FontWeight weight, System.Windows.FontStyle style)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = LogTextBox.Document.Blocks.LastBlock as Paragraph;
                if (paragraph == null)
                {
                    paragraph = new Paragraph();
                    LogTextBox.Document.Blocks.Add(paragraph);
                }
        
                var run = new Run(text + Environment.NewLine)
                {
                    Foreground = color,
                    FontWeight = weight,
                    FontStyle = style
                };
        
                paragraph.Inlines.Add(run);
                LogTextBox.ScrollToEnd();
            });
        }

        private void AppendMixedLine( params (string text, System.Windows.Media.Brush color, System.Windows.FontWeight weight, System.Windows.FontStyle style)[] parts)

        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph();
    
                foreach (var part in parts)
                {
                    var run = new Run(part.text)
                    {
                        Foreground = part.color,
                        FontWeight = part.weight,
                        FontStyle = part.style
                    };
                    paragraph.Inlines.Add(run);
                }
    
                LogTextBox.Document.Blocks.Add(paragraph);
                LogTextBox.ScrollToEnd();
            });
        }
    }
}

