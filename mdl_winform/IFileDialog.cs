using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

#pragma warning disable CS1591

namespace mdl_winform {
    /// <summary>
    /// OpenFileDialog > FileDialog > CommonDialog
    /// Interfaccia proxy
    /// </summary>
    public interface IOpenFileDialog {
	    IOpenFileDialog init(OpenFileDialog _diag);
        bool CheckFileExists { get; set; }
        bool Multiselect { get; set; }
        bool ReadOnlyChecked { get; set; }
        bool ShowReadOnly { get; set; }
        string SafeFileName { get; }
        string[] SafeFileNames { get; }
        Stream OpenFile();
        void Reset();
        FileDialogCustomPlacesCollection CustomPlaces { get; }
        bool ValidateNames { get; set; }
        string Title { get; set; }
        bool SupportMultiDottedExtensions { get; set; }
        bool ShowHelp { get; set; }
        bool RestoreDirectory { get; set; }
        string InitialDirectory { get; set; }
        int FilterIndex { get; set; }
        string Filter { get; set; }
        bool AutoUpgradeEnabled { get; set; }
        string[] FileNames { get; }
        bool DereferenceLinks { get; set; }
        string DefaultExt { get; set; }
        bool CheckPathExists { get; set; }
        bool AddExtension { get; set; }
        string FileName { get; set; }
        string ToString();
        object Tag { get; set; }
        DialogResult ShowDialog();
        DialogResult ShowDialog(IWin32Window owner);

        event EventHandler HelpRequest;

        event CancelEventHandler FileOk;
    }

    /// <summary>
    /// SaveFileDialog > FileDialog > CommonDialog
    /// Interfaccia proxy
    /// </summary>
    public interface ISaveFileDialog {
	    ISaveFileDialog init(SaveFileDialog _diag);
        bool CreatePrompt { get; set; }
        bool OverwritePrompt { get; set; }
        Stream OpenFile();
        void Reset();
        FileDialogCustomPlacesCollection CustomPlaces { get; }
        bool ValidateNames { get; set; }
        string Title { get; set; }
        bool SupportMultiDottedExtensions { get; set; }
        bool ShowHelp { get; set; }
        bool RestoreDirectory { get; set; }
        string InitialDirectory { get; set; }
        int FilterIndex { get; set; }
        string Filter { get; set; }
        bool AutoUpgradeEnabled { get; set; }
        string[] FileNames { get; }
        bool DereferenceLinks { get; set; }
        string DefaultExt { get; set; }
        bool CheckPathExists { get; set; }
        bool AddExtension { get; set; }
        string FileName { get; set; }
        string ToString();
        object Tag { get; set; }
        DialogResult ShowDialog();
        DialogResult ShowDialog(IWin32Window owner);

        event CancelEventHandler FileOk;

        event EventHandler HelpRequest;
    }

    /// <summary>
    /// FolderBrowserDialog > CommonDialog
    /// Interfaccia proxy
    /// </summary>
    public interface IFolderBrowserDialog {
	    IFolderBrowserDialog init(FolderBrowserDialog _diag);
        bool ShowNewFolderButton { get; set; }
        string SelectedPath { get; set; }
        Environment.SpecialFolder RootFolder { get; set; }
        string Description { get; set; }
        void Reset();
        object Tag { get; set; }
        DialogResult ShowDialog();
        DialogResult ShowDialog(IWin32Window owner);

        event EventHandler HelpRequest;
    }
}