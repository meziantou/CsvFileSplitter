using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Documents;
using CsvFileSplitter.Annotations;

namespace CsvFileSplitter
{
    public class CsvSplitterOptions : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _inputFile;
        private string _outputDirectory;
        private string _outputFileFormat;
        private long _linesByFile = 10000;
        private long _headerLineCount = 1;
        public event PropertyChangedEventHandler PropertyChanged;

        public string InputFile
        {
            get { return _inputFile; }
            set
            {
                if (value == _inputFile) return;
                _inputFile = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(_inputFile))
                {
                    var directoryName = Path.GetDirectoryName(_inputFile);
                    if (OutputDirectory == null)
                    {
                        OutputDirectory = directoryName;
                    }

                    var fileName = Path.GetFileNameWithoutExtension(_inputFile);
                    var extension = Path.GetExtension(_inputFile);
                    OutputFileFormat = fileName + "_{0:#000}" + extension;
                }
            }
        }

        public string OutputDirectory
        {
            get { return _outputDirectory; }
            set
            {
                if (value == _outputDirectory) return;
                _outputDirectory = value;
                OnPropertyChanged();
            }
        }

        public string OutputFileFormat
        {
            get { return _outputFileFormat; }
            set
            {
                if (value == _outputFileFormat) return;
                _outputFileFormat = value;
                OnPropertyChanged();
            }
        }

        public long LinesByFile
        {
            get { return _linesByFile; }
            set
            {
                if (value == _linesByFile) return;
                _linesByFile = value;
                OnPropertyChanged();
            }
        }

        public long HeaderLineCount
        {
            get { return _headerLineCount; }
            set
            {
                if (value == _headerLineCount) return;
                _headerLineCount = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string this[string columnName]
        {
            get
            {
                List<string> errors = new List<string>();

                if (string.IsNullOrEmpty(columnName) || columnName == "InputFile")
                {
                    if (string.IsNullOrEmpty(InputFile))
                        errors.Add("Input file must be set.");
                    else if (!File.Exists(InputFile))
                        errors.Add("Input file not found.");
                }

                if (string.IsNullOrEmpty(columnName) || columnName == "OutputDirectory")
                {
                    if (string.IsNullOrEmpty(OutputDirectory))
                        errors.Add("Output directory must be set.");
                    else if (!Directory.Exists(OutputDirectory))
                        errors.Add("Output directory not found.");
                }

                if (string.IsNullOrEmpty(columnName) || columnName == "OutputFileFormat")
                {
                    if (string.IsNullOrEmpty(OutputFileFormat))
                        errors.Add("Output file format must be set.");
                }

                if (errors.Any())
                    return string.Join(Environment.NewLine, errors);
                return null;
            }
        }

        public string Error
        {
            get { return this[null]; }
        }
    }
}
