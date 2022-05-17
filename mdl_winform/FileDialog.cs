using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

#pragma warning disable CS1591
#pragma warning disable CS0108

namespace mdl_winform {
    // Usage: è necessario in tutti i form rinominare la OpenFileDialog con l'underscore
    // ed introdurre OpenFileDialogFactory
    // Es:
    // //private System.Windows.Forms.OpenFileDialog openFileDialog1;
    // private System.Windows.Forms.OpenFileDialog _openFileDialog1;
    // public IOpenFileDialog openFileDialog1 = MetaFactory.factory.getSingleton<IOpenFileDialog>();
    public class DefaultOpenFileDialog : IOpenFileDialog {

        OpenFileDialog diag;

        public IOpenFileDialog init(OpenFileDialog _diag) {
            diag = _diag;
            return this;
        }

        public Stream OpenFile() {
            return diag.OpenFile();
        }

        public void Reset() {
            diag.Reset();
        }

        public DialogResult ShowDialog(IWin32Window owner) {
            return diag.ShowDialog(owner);
        }

        public DialogResult ShowDialog() {
            return diag.ShowDialog();
        }

        public bool CheckFileExists { get => diag.CheckFileExists; set => diag.CheckFileExists = value; }
        public bool Multiselect { get => diag.Multiselect; set => diag.Multiselect = value; }
        public bool ReadOnlyChecked { get => diag.ReadOnlyChecked; set => diag.ReadOnlyChecked = value; }
        public bool ShowReadOnly { get => diag.ShowReadOnly; set => diag.ShowReadOnly = value; }
        public string SafeFileName { get => diag.SafeFileName; }
        public string[] SafeFileNames { get => diag.SafeFileNames; }
        public FileDialogCustomPlacesCollection CustomPlaces { get => diag.CustomPlaces; }
        public bool ValidateNames { get => diag.ValidateNames; set => diag.ValidateNames = value; }
        public string Title { get => diag.Title; set => diag.Title = value; }
        public bool SupportMultiDottedExtensions { get => diag.SupportMultiDottedExtensions; set => diag.SupportMultiDottedExtensions = value; }
        public bool ShowHelp { get => diag.ShowHelp; set => diag.ShowHelp = value; }
        public bool RestoreDirectory { get => diag.RestoreDirectory; set => diag.RestoreDirectory = value; }
        public string InitialDirectory { get => diag.InitialDirectory; set => diag.InitialDirectory = value; }
        public int FilterIndex { get => diag.FilterIndex; set => diag.FilterIndex = value; }
        public string Filter { get => diag.Filter; set => diag.Filter = value; }
        public bool AutoUpgradeEnabled { get => diag.AutoUpgradeEnabled; set => diag.AutoUpgradeEnabled = value; }
        public string[] FileNames { get => diag.FileNames; }
        public bool DereferenceLinks { get => diag.DereferenceLinks; set => diag.DereferenceLinks = value; }
        public string DefaultExt { get => diag.DefaultExt; set => diag.DefaultExt = value; }
        public bool CheckPathExists { get => diag.CheckPathExists; set => diag.CheckPathExists = value; }
        public bool AddExtension { get => diag.AddExtension; set => diag.AddExtension = value; }
        public string FileName { get => diag.FileName; set => diag.FileName = value; }
        public object Tag { get => diag.Tag; set => diag.Tag = value; }

        public event CancelEventHandler FileOk;
        public event EventHandler HelpRequest;
    }

    public class DefaultSaveFileDialog : ISaveFileDialog {

        SaveFileDialog diag;

        public ISaveFileDialog init(SaveFileDialog _diag) {
            diag = _diag;
            return this;
        }

        public Stream OpenFile() {
            return diag.OpenFile();
        }

        public void Reset() {
            diag.Reset();
        }

        public DialogResult ShowDialog(IWin32Window owner) {
            return diag.ShowDialog(owner);
        }

        public DialogResult ShowDialog() {
            return diag.ShowDialog();
        }
        public bool CheckFileExists { get => diag.CheckFileExists; set => diag.CheckFileExists = value; }
        public bool CreatePrompt { get => diag.CreatePrompt; set => diag.CreatePrompt = value; }
        public bool OverwritePrompt { get => diag.OverwritePrompt; set => diag.OverwritePrompt = value; }
        public FileDialogCustomPlacesCollection CustomPlaces { get => diag.CustomPlaces; }
        public bool ValidateNames { get => diag.ValidateNames; set => diag.ValidateNames = value; }
        public string Title { get => diag.Title; set => diag.Title = value; }
        public bool SupportMultiDottedExtensions { get => diag.SupportMultiDottedExtensions; set => diag.SupportMultiDottedExtensions = value; }
        public bool ShowHelp { get => diag.ShowHelp; set => diag.ShowHelp = value; }
        public bool RestoreDirectory { get => diag.RestoreDirectory; set => diag.RestoreDirectory = value; }
        public string InitialDirectory { get => diag.InitialDirectory; set => diag.InitialDirectory = value; }
        public int FilterIndex { get => diag.FilterIndex; set => diag.FilterIndex = value; }
        public string Filter { get => diag.Filter; set => diag.Filter = value; }
        public bool AutoUpgradeEnabled { get => diag.AutoUpgradeEnabled; set => diag.AutoUpgradeEnabled = value; }
        public string[] FileNames { get => diag.FileNames; }
        public bool DereferenceLinks { get => diag.DereferenceLinks; set => diag.DereferenceLinks = value; }
        public string DefaultExt { get => diag.DefaultExt; set => diag.DefaultExt = value; }
        public bool CheckPathExists { get => diag.CheckPathExists; set => diag.CheckPathExists = value; }
        public bool AddExtension { get => diag.AddExtension; set => diag.AddExtension = value; }
        public string FileName { get => diag.FileName; set => diag.FileName = value; }
        public object Tag { get => diag.Tag; set => diag.Tag = value; }

        public event CancelEventHandler FileOk;
        public event EventHandler HelpRequest;
    }

    public class DefaultFolderBrowserDialog : IFolderBrowserDialog {

        FolderBrowserDialog diag;

        public IFolderBrowserDialog init(FolderBrowserDialog _diag) {
            diag = _diag;
            return this;
        }

        public void Reset() {
            diag.Reset();
        }

        public DialogResult ShowDialog(IWin32Window owner) {
            return diag.ShowDialog(owner);
        }

        public DialogResult ShowDialog() {
            return diag.ShowDialog();
        }
        
        public object Tag { get => diag.Tag; set => diag.Tag = value; }
		public bool ShowNewFolderButton { get => diag.ShowNewFolderButton; set => diag.ShowNewFolderButton = value; }
		public string SelectedPath { get => diag.SelectedPath; set => diag.SelectedPath = value; }
        public Environment.SpecialFolder RootFolder { get => diag.RootFolder; set => diag.RootFolder = value; }
        public string Description { get => diag.Description; set => diag.Description = value; }

        public event EventHandler HelpRequest;
    }
}
