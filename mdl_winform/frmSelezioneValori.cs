using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace mdl_winform
{
	/// <summary>
	/// Allow the user to select a value between all possible values assumed
	///  by a field into a table.
	/// </summary>
	public class frmSelezioneValori : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// ListBox containing all selectable values
		/// </summary>
		public System.Windows.Forms.ListBox listBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Form constructor
		/// </summary>
		/// <param name="T">Table containing data</param>
		/// <param name="field">field to select data</param>
		public frmSelezioneValori(DataTable T, string field)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

		    if (T.Columns.Contains(field)) {
		        foreach (DataRow R in T.Rows) {
		            string val = R[field].ToString();
		            if (listBox1.Items.IndexOf(val) == -1)
		                listBox1.Items.Add(val);
		        }
		    }
		    if (listBox1.Items.Count==0)
				listBox1.Items.Add("");
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(72, 320);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 0;
			this.btnOk.Text = "Ok";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(168, 320);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			// 
			// listBox1
			// 
			this.listBox1.Location = new System.Drawing.Point(16, 16);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(264, 290);
			this.listBox1.TabIndex = 2;
			this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
			// 
			// frmSelezioneValori
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(296, 357);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Name = "frmSelezioneValori";
			this.Text = "Selezione valori";
			this.ResumeLayout(false);

		}
		#endregion

		private void listBox1_DoubleClick(object sender, System.EventArgs e) {
			DialogResult = DialogResult.OK;
		}
	}
}
