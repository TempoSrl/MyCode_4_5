using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using mdl;

namespace mdl_winform {
    /// <summary>
    /// Form che chiede il nome dell'elenco da salvare
    /// </summary>
    public class FormCopyList : System.Windows.Forms.Form {
        private System.Windows.Forms.ListView lvList;
        private System.Windows.Forms.TextBox txtListname;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private VistaFormCustomView DSSource;
        private string[] m_elenchi;

        /// <summary>
        /// Listing Type selezionato
        /// </summary>
        public string newlisttype;

        /// <summary>
        /// Costruisce il form con l'elenco passato
        /// </summary>
        /// <param name="elenchi">elenco dei listingtype esistenti</param>
        public FormCopyList(string[] elenchi) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            m_elenchi = elenchi;
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
            this.lvList = new System.Windows.Forms.ListView();
            this.txtListname = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.DSSource = new VistaFormCustomView();
            ((System.ComponentModel.ISupportInitialize) (this.DSSource)).BeginInit();
            this.SuspendLayout();
            // 
            // lvList
            // 
            this.lvList.FullRowSelect = true;
            this.lvList.Location = new System.Drawing.Point(10, 10);
            this.lvList.MultiSelect = false;
            this.lvList.Name = "lvList";
            this.lvList.Size = new System.Drawing.Size(240, 130);
            this.lvList.TabIndex = 0;
            this.lvList.View = System.Windows.Forms.View.List;
            this.lvList.SelectedIndexChanged += new System.EventHandler(this.lvList_SelectedIndexChanged);
            // 
            // txtListname
            // 
            this.txtListname.Location = new System.Drawing.Point(10, 160);
            this.txtListname.Name = "txtListname";
            this.txtListname.Size = new System.Drawing.Size(240, 20);
            this.txtListname.TabIndex = 1;
            this.txtListname.Text = "";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(290, 110);
            this.btnSave.Name = "btnSave";
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Salva";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(290, 160);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Annulla";
            // 
            // DSSource
            // 
            this.DSSource.DataSetName = "VistaFormCustomView";
            this.DSSource.Locale = new System.Globalization.CultureInfo("en-US");
            // 
            // FormCopyList
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(382, 196);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtListname);
            this.Controls.Add(this.lvList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FormCopyList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Salva elenco con nome";
            this.Load += new System.EventHandler(this.formCopyList_Load);
            ((System.ComponentModel.ISupportInitialize) (this.DSSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private void btnSave_Click(object sender, System.EventArgs e) {
            //controllo su nome e attributo (di sistema)
            if (txtListname.Text.Trim() == "") {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(null, "Inserire il nome dell'elenco",
                    "Attenzione",
                    mdl.MessageBoxButtons.OK);
                return;
            }

            foreach (ListViewItem item in lvList.Items) {
                if (item.Text.ToLower() == txtListname.Text.ToLower()) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,
                        "L'elenco specificato è già presente. Indicare un altro nome.",
                        "Attenzione", mdl.MessageBoxButtons.OK);
                    return;
                }
            }

            newlisttype = txtListname.Text.Trim();
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void formCopyList_Load(object sender, EventArgs e) {
            foreach (string valore in m_elenchi) {
                lvList.Items.Add(valore);
            }
        }

        private void lvList_SelectedIndexChanged(object sender, System.EventArgs e) {
            foreach (ListViewItem item in lvList.SelectedItems) {
                txtListname.Text = item.Text;
            }
        }

    }
}
