using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;


namespace mdl_winform
{
	/// <summary>
	/// Summary description for frmAvvisoNessunaRigaTrovata.
	/// </summary>
	public class frmAvvisoNessunaRigaTrovata : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.CheckBox chkDettagli;
		private System.Windows.Forms.Button btnOk;
        /// <summary>
        /// Label for "no found rows"
        /// </summary>
		public System.Windows.Forms.Label labMessage;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        /// <summary>
        /// Show a message for "no rows was found with the given conditions"
        /// </summary>
        /// <param name="mainmessage"></param>
        /// <param name="longmsg"></param>
		public frmAvvisoNessunaRigaTrovata(string mainmessage, string longmsg)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            utils.SetColorOneTime(this, true);

			this.Height=180;
			labMessage.Text= mainmessage;
			txtMsg.Text= longmsg;
		}

        /// <summary>
        /// Shows a message for "no row was found"
        /// </summary>
        /// <param name="longmsg"></param>
		public frmAvvisoNessunaRigaTrovata(string longmsg) {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.Height=180;
			txtMsg.Text= longmsg;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAvvisoNessunaRigaTrovata));
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labMessage = new System.Windows.Forms.Label();
            this.chkDettagli = new System.Windows.Forms.CheckBox();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(72, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ricerca fallita:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 50);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // labMessage
            // 
            this.labMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labMessage.Location = new System.Drawing.Point(74, 28);
            this.labMessage.Name = "labMessage";
            this.labMessage.Size = new System.Drawing.Size(300, 39);
            this.labMessage.TabIndex = 2;
            this.labMessage.Text = "Nessun oggetto trovato";
            // 
            // chkDettagli
            // 
            this.chkDettagli.Location = new System.Drawing.Point(8, 104);
            this.chkDettagli.Name = "chkDettagli";
            this.chkDettagli.Size = new System.Drawing.Size(128, 24);
            this.chkDettagli.TabIndex = 3;
            this.chkDettagli.Text = "Visualizza dettagli";
            this.chkDettagli.CheckStateChanged += new System.EventHandler(this.chkDettagli_CheckStateChanged);
            // 
            // txtMsg
            // 
            this.txtMsg.AcceptsReturn = true;
            this.txtMsg.AcceptsTab = true;
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(8, 155);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(366, 0);
            this.txtMsg.TabIndex = 4;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(149, 74);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(96, 24);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            // 
            // frmAvvisoNessunaRigaTrovata
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(384, 146);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.chkDettagli);
            this.Controls.Add(this.labMessage);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Name = "frmAvvisoNessunaRigaTrovata";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Esito Ricerca";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void chkDettagli_CheckStateChanged(object sender, System.EventArgs e) {
			if (chkDettagli.Checked)
				this.Height=400;
			else
				this.Height=180;
		}

	
	}
}
