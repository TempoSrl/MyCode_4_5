using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Xml;
using System.Runtime.InteropServices;
//using Crownwood.Magic.Controls;
using LM=mdl_language.LanguageManager;
using System.Collections.Specialized;
using mdl;
using System.Data;

namespace mdl_winform {

    /// <summary>
    /// Summary description for NotesOleNotes.
    /// </summary>
    public class NotesOleNotes : System.Windows.Forms.Form {
        private TabControl tabControl1;
        private TabPage tabNotes;
        private TabPage tabOleNotes;
        private IContainer components;
        private System.Windows.Forms.TextBox txtNotes;
        private RichTextBoxPlus txtOleNotes;
        
        IFormController controller;
        private System.Windows.Forms.Button btnPaste;
        private ContextMenuStrip cntMenuOleNotes;
        private ToolStripMenuItem cmdOleAnnulla;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem cmdOleTaglia;
        private ToolStripMenuItem cmdOleCopia;
        private ToolStripMenuItem cmdOleIncolla;
        private ToolStripMenuItem cmdOleElimina;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem cmdOleSelezionaTutto;
        private Button btnFile;
        //string strPastedText;

        IMetaData meta;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Meta">MetaData to edit</param>
        public NotesOleNotes(IFormController controller) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.controller = controller;

            utils.SetColorOneTime(this, true);
            txtOleNotes.AllowDrop = true;


            this.meta = controller.meta;
            //strPastedText="";
            if (meta.HasNotes()) {
                System.Data.DataTable main = controller.primaryTable;
                txtNotes.Text = meta.GetNotes(main._getLastSelected());
                txtNotes.DeselectAll();
                txtNotes.SelectionLength = 0;
                txtNotes.SelectionStart = 0;
                txtChanges = false;
            }
            else {
                tabNotes.Visible = false;
            }
            if (meta.HasOleNotes()) {
                System.Data.DataTable main = controller.primaryTable;

                Byte[] OleNotes = meta.GetOleNotes(main._getLastSelected());


                //res.AddRange(new byte[] { 141, 132, 53, 13 });
                byte[] data = OleNotes;
                if (data.Length > 4) {
                    if (data[0] == 141 && data[1] == 132 && data[2] == 53 && data[3] == 13) {
                        byte[] newarr = new byte[data.Length - 4];
                        Array.Copy(data, 4, newarr, 0, data.Length - 4);
                        OleNotes = DataSetUtils.Unzip(newarr);
                    }
                }
                if (OleNotes.Length > 0) {
                    MemoryStream MS = new MemoryStream(OleNotes);
                    try {
                        txtOleNotes.LoadFile(MS, RichTextBoxStreamType.RichText);
                    }
                    catch (Exception E) {
                        ErrorLogger.Logger.MarkEvent(E.ToString());
                    }
                }


            }
            else {
                tabOleNotes.Visible = false;
            }
        }

      

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new TabControl();
            this.tabOleNotes = new TabPage();
            this.btnFile = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.txtOleNotes = new mdl_winform.RichTextBoxPlus();
            this.cntMenuOleNotes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmdOleAnnulla = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmdOleTaglia = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdOleCopia = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdOleIncolla = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdOleElimina = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmdOleSelezionaTutto = new System.Windows.Forms.ToolStripMenuItem();
            this.tabNotes = new TabPage();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabOleNotes.SuspendLayout();
            this.cntMenuOleNotes.SuspendLayout();
            this.tabNotes.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Location = new System.Drawing.Point(8, 8);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 1;
            this.tabControl1.SelectedTab = this.tabOleNotes;
            this.tabControl1.Size = new System.Drawing.Size(768, 432);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.TabPages.AddRange(new TabPage[] {
            this.tabNotes,
            this.tabOleNotes});
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectionChanged);
            // 
            // tabOleNotes
            // 
            this.tabOleNotes.Controls.Add(this.btnFile);
            this.tabOleNotes.Controls.Add(this.btnPaste);
            this.tabOleNotes.Controls.Add(this.txtOleNotes);
            this.tabOleNotes.Location = new System.Drawing.Point(0, 25);
            this.tabOleNotes.Name = "tabOleNotes";
            this.tabOleNotes.Size = new System.Drawing.Size(768, 407);
            this.tabOleNotes.TabIndex = 4;
            this.tabOleNotes.Text = "Allegati (immagini, documenti di ogni genere)";
            // 
            // btnFile
            // 
            this.btnFile.Location = new System.Drawing.Point(89, 19);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(75, 23);
            this.btnFile.TabIndex = 4;
            this.btnFile.Text = "Allega file";
            this.btnFile.UseVisualStyleBackColor = true;
            this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Location = new System.Drawing.Point(8, 19);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(75, 23);
            this.btnPaste.TabIndex = 2;
            this.btnPaste.Text = "Incolla";
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // txtOleNotes
            // 
            this.txtOleNotes.AllowDrop = true;
            this.txtOleNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOleNotes.ContextMenuStrip = this.cntMenuOleNotes;
            this.txtOleNotes.EnableAutoDragDrop = true;
            this.txtOleNotes.Location = new System.Drawing.Point(8, 48);
            this.txtOleNotes.Name = "txtOleNotes";
            this.txtOleNotes.Size = new System.Drawing.Size(752, 352);
            this.txtOleNotes.TabIndex = 0;
            this.txtOleNotes.Text = "";
            // 
            // cntMenuOleNotes
            // 
            this.cntMenuOleNotes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdOleAnnulla,
            this.toolStripSeparator1,
            this.cmdOleTaglia,
            this.cmdOleCopia,
            this.cmdOleIncolla,
            this.cmdOleElimina,
            this.toolStripSeparator2,
            this.cmdOleSelezionaTutto});
            this.cntMenuOleNotes.Name = "contextMenuStrip1";
            this.cntMenuOleNotes.Size = new System.Drawing.Size(153, 148);
            this.cntMenuOleNotes.Opening += new System.ComponentModel.CancelEventHandler(this.cntMenuOleNotes_Opening);
            // 
            // cmdOleAnnulla
            // 
            this.cmdOleAnnulla.Name = "cmdOleAnnulla";
            this.cmdOleAnnulla.Size = new System.Drawing.Size(152, 22);
            this.cmdOleAnnulla.Text = "Annulla";
            this.cmdOleAnnulla.Click += new System.EventHandler(this.cmdOleAnnulla_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // cmdOleTaglia
            // 
            this.cmdOleTaglia.Name = "cmdOleTaglia";
            this.cmdOleTaglia.Size = new System.Drawing.Size(152, 22);
            this.cmdOleTaglia.Text = "Taglia";
            this.cmdOleTaglia.Click += new System.EventHandler(this.cmdOleTaglia_Click);
            // 
            // cmdOleCopia
            // 
            this.cmdOleCopia.Name = "cmdOleCopia";
            this.cmdOleCopia.Size = new System.Drawing.Size(152, 22);
            this.cmdOleCopia.Text = "Copia";
            this.cmdOleCopia.Click += new System.EventHandler(this.cmdOleCopia_Click);
            // 
            // cmdOleIncolla
            // 
            this.cmdOleIncolla.Name = "cmdOleIncolla";
            this.cmdOleIncolla.Size = new System.Drawing.Size(152, 22);
            this.cmdOleIncolla.Text = "Incolla";
            this.cmdOleIncolla.Click += new System.EventHandler(this.cmdOleIncolla_Click);
            // 
            // cmdOleElimina
            // 
            this.cmdOleElimina.Name = "cmdOleElimina";
            this.cmdOleElimina.Size = new System.Drawing.Size(152, 22);
            this.cmdOleElimina.Text = "Elimina";
            this.cmdOleElimina.Click += new System.EventHandler(this.cmdOleElimina_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // cmdOleSelezionaTutto
            // 
            this.cmdOleSelezionaTutto.Name = "cmdOleSelezionaTutto";
            this.cmdOleSelezionaTutto.Size = new System.Drawing.Size(152, 22);
            this.cmdOleSelezionaTutto.Text = "Seleziona tutto";
            this.cmdOleSelezionaTutto.Click += new System.EventHandler(this.cmdOleSelezionaTutto_Click);
            // 
            // tabNotes
            // 
            this.tabNotes.Controls.Add(this.txtNotes);
            this.tabNotes.Location = new System.Drawing.Point(0, 25);
            this.tabNotes.Name = "tabNotes";
            this.tabNotes.Size = new System.Drawing.Size(768, 407);
            this.tabNotes.TabIndex = 3;
            this.tabNotes.Text = "Appunti (testo)";
            // 
            // txtNotes
            // 
            this.txtNotes.AcceptsReturn = true;
            this.txtNotes.AcceptsTab = true;
            this.txtNotes.AllowDrop = true;
            this.txtNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNotes.Location = new System.Drawing.Point(8, 8);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtNotes.Size = new System.Drawing.Size(752, 384);
            this.txtNotes.TabIndex = 0;
            this.txtNotes.TabStop = false;
            this.txtNotes.TextChanged += new System.EventHandler(this.txtNotes_TextChanged);
            // 
            // NotesOleNotes
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(784, 445);
            this.Controls.Add(this.tabControl1);
            this.Name = "NotesOleNotes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Annotazioni";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.NotesOleNotes_Closing);
            this.tabControl1.ResumeLayout(false);
            this.tabOleNotes.ResumeLayout(false);
            this.cntMenuOleNotes.ResumeLayout(false);
            this.tabNotes.ResumeLayout(false);
            this.tabNotes.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();

        private void NotesOleNotes_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            var meta = controller.meta;
            DataRow r  = controller.primaryTable._getLastSelected();
            try {
                if (txtChanges) {
                    meta.SetNotes(r, txtNotes.Text);
                }
                if (true) {                    //oleChanges, non intercettava la modifica all'interno di app.esterne (es. foglio excel)
                    MemoryStream MS = new MemoryStream();
                    txtOleNotes.SaveFile(MS, RichTextBoxStreamType.RichText);
                    byte[] data = MS.ToArray();
                    if (data.Length > 100) {
                        List<byte> res = new List<byte>();
                        res.AddRange(new byte[] { 141, 132, 53, 13 });
                        res.AddRange(DataSetUtils.Zip(data));
                        data = res.ToArray();
                    }

                    if (data.Length > 1024 * 1024) {
                        if (shower.Show(this, LM.attachmentTooBig, LM.confirmTitle, mdl.MessageBoxButtons.OKCancel) == mdl.DialogResult.OK) {
                            //Esce comunque ma senza salvare
                            return;
                        };
                        e.Cancel = true;
                        return;
                    }

                    meta.SetOleNotes(r,data);
                }
            }
            catch (Exception ee) {
                shower.ShowException(controller.linkedForm,null,ee);
            }

            txtOleNotes.Dispose();
        }




        private void btnPaste_Click(object sender, System.EventArgs e) {
            try {
                txtOleNotes.Paste();
                txtOleNotes.Invalidate();
            }
            catch { }

        }

        //private void btnBullett_Click(object sender, System.EventArgs e) {
        //    txtOleNotes.SelectionBullet = !txtOleNotes.SelectionBullet;
        //}

        private void tabControl1_SelectionChanged(object sender, System.EventArgs e) {

        }

        private void cntMenuOleNotes_Opening(object sender, CancelEventArgs e) {
            cmdOleAnnulla.Enabled = txtOleNotes.CanUndo;
            cmdOleCopia.Enabled = (txtOleNotes.SelectionType != RichTextBoxSelectionTypes.Empty);
            cmdOleElimina.Enabled = (txtOleNotes.SelectionType != RichTextBoxSelectionTypes.Empty);
            cmdOleTaglia.Enabled = (txtOleNotes.SelectionType != RichTextBoxSelectionTypes.Empty);
            string[] fmts = Clipboard.GetDataObject().GetFormats(true);
            bool canpaste = false;
            string available = "";
            foreach (string s in fmts) {
                if (txtOleNotes.CanPaste(DataFormats.GetFormat(s))) {
                    canpaste = true;
                    available += "," + s;
                }
            }
            //bool X= Clipboard.ContainsFileDropList();
            //IDataObject IDO= Clipboard.GetDataObject();
            //string []ff = IDO.GetFormats();
            //object OFIleDrop = IDO.GetData("FileDrop");
            //object PrefDropEff = IDO.GetData("Preferred DropEffect");
            //object ShellOffs = IDO.GetData("Shell Object Offsets");
            cmdOleIncolla.Enabled = canpaste;


            cmdOleSelezionaTutto.Enabled = txtOleNotes.Text.Length > 0;
        }

        private void cmdOleAnnulla_Click(object sender, EventArgs e) {
            txtOleNotes.Undo();
        }

        private void cmdOleTaglia_Click(object sender, EventArgs e) {
            txtOleNotes.Cut();
        }

        private void cmdOleCopia_Click(object sender, EventArgs e) {
            txtOleNotes.Copy();
        }

        private void cmdOleIncolla_Click(object sender, EventArgs e) {
            txtOleNotes.Paste();

        }



        private void cmdOleElimina_Click(object sender, EventArgs e) {
            txtOleNotes.SelectedRtf = "";
        }

        private void cmdOleSelezionaTutto_Click(object sender, EventArgs e) {
            txtOleNotes.SelectAll();

        }


        bool txtChanges = false;
        private void txtNotes_TextChanged(object sender, EventArgs e) {
            txtChanges = true;
        }

        private void btnFile_Click(object sender, EventArgs e) {

            var allowed = new List<string>() {
                ".abw",
                ".arc",
                ".avif",
                ".bmp",
                ".bz",
                ".bz2",
                ".csv",
                ".doc",
                ".docx",
                ".gz",
                ".jpeg",
                ".jpg",
                ".odp",
                ".ods",
                ".odt",
                ".png",
                ".pdf",
                ".ppt",
                ".pptx",
                ".rar",
                ".rtf",
                ".svg",
                ".tar",
                ".tif",
                ".tiff",
                ".txt",
                ".webp",
                ".xls",
                ".xlsx",
                ".zip",
                ".7z"
            };

            string filePath = null;

            List<string> filterItems = new List<string>();

            filterItems.Add("tutti i file|*.*");
            allowed._forEach(ext => { filterItems.Add(string.Format("file {0}|*{0}", ext)); });

            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFileDialog.Filter = string.Join("|", filterItems); // "txt files (*.txt)|*.txt|All files (*.*)|*.*"
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    filePath = openFileDialog.FileName;
                }
            }

            if (filePath != null) {
                string ext = Path.GetExtension(filePath);
                if (!allowed.Contains(ext)) {
                    shower.Show("l'estensione " + ext + " non è ammessa", "Errore");
                    return;
                }
                Clipboard.SetFileDropList(new StringCollection() { filePath });
                txtOleNotes.Paste();
            }
        }
    }


}
