using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CsvFileSplitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CsvSplitterOptions _csvSplitterOptions;

        public MainWindow()
        {
            InitializeComponent();
            _csvSplitterOptions = new CsvSplitterOptions();
            DataContext = _csvSplitterOptions;
        }

        private void ButtonInputFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    _csvSplitterOptions.InputFile = dialog.FileName;
                }
            }
        }

        private void ButtonOutputDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK || dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    _csvSplitterOptions.OutputDirectory = dialog.SelectedPath;
                }
            }
        }

        private async void ButtonSplit_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonSplit.IsEnabled = false;
                await Split();
            }
            finally
            {
                ButtonSplit.IsEnabled = true;
            }
        }

        private async Task Split()
        {
            var options = _csvSplitterOptions;
            if (!string.IsNullOrEmpty(options.Error))
            {
                MessageBox.Show(options.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var inputStream = new StreamReader(options.InputFile))
                {
                    string header = null;
                    if (options.HeaderLineCount > 0)
                    {
                        TextBlockProgress.Text = "Reading header...";
                        StringBuilder headerStringBuilder = new StringBuilder();
                        for (int i = 0; i < options.HeaderLineCount && !inputStream.EndOfStream; i++)
                        {
                            if (i > 0)
                            {
                                headerStringBuilder.AppendLine();
                            }
                            headerStringBuilder.Append(await inputStream.ReadLineAsync());
                        }

                        header = headerStringBuilder.ToString();
                    }

                    int fileIndex = 0;
                    while (!inputStream.EndOfStream)
                    {
                        // Create output file
                        string outputFileName = string.Format(options.OutputFileFormat, fileIndex++);
                        string outputPath = Path.Combine(options.OutputDirectory, outputFileName);
                        TextBlockProgress.Text = string.Format("Creating part {0}...", fileIndex);
                        using (StreamWriter writer = new StreamWriter(outputPath, false))
                        {
                            // Write header
                            if (header != null)
                            {
                                TextBlockProgress.Text = string.Format("Part {0}: writing header...", fileIndex);
                                await writer.WriteAsync(header);
                            }

                            // Write content
                            for (int i = 0; i < options.LinesByFile && !inputStream.EndOfStream; i++)
                            {
                                if (i % 50 == 0)
                                {
                                    TextBlockProgress.Text = string.Format("Part {0}: writing line {1}...", fileIndex, i);
                                }

                                if (header != null || i > 0)
                                {
                                    await writer.WriteLineAsync();
                                }
                                string line = await inputStream.ReadLineAsync();
                                await writer.WriteAsync(line);
                            }
                        }
                    }
                }

                MessageBox.Show("File splitted successfully!", "Success", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                TextBlockProgress.Text = string.Empty;
            }
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string data = ((string[])e.Data.GetData(DataFormats.FileDrop)).FirstOrDefault();
                if (data != null)
                {
                    if (File.Exists(data))
                    {
                        _csvSplitterOptions.InputFile = data;
                    }
                    else if (Directory.Exists(data))
                    {
                        _csvSplitterOptions.OutputDirectory = data;
                    }
                }
            }
        }
    }
}
