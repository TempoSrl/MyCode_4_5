using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using mdl;

namespace mdl_winform
{
	/// <summary>
	/// Summary description for FrmShowException.
	/// </summary>
	public class FrmShowException : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox MainMessage;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.TextBox txtMsg;
		private System.Windows.Forms.CheckBox chkDettagli;
		private System.Windows.Forms.PictureBox pictureBox1;
        /// <summary>
        /// remote url for log errors
        /// </summary>
		public string logurl;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

	    

        /// <summary>
        /// Display a message with an error
        /// </summary>
        /// <param name="MainMsg"></param>
        /// <param name="E"></param>
        /// <param name="D">used to log additional informations</param>
        public FrmShowException(string MainMsg, Exception E, EntityDispatcher D) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            utils.SetColorOneTime(this, true);

            if (MainMsg == null) MainMsg = ErrorLogger.GetErrorString(E); 
            MainMessage.Text = MainMsg;
            txtMsg.Text = ErrorLogger.GetErrorString(E);
            if (D != null) {
                D.logException(MainMsg, E);
            }
        }

        /// <summary>
        ///  Displays an exception with a main message
        /// </summary>
        /// <param name="MainMsg"></param>
        /// <param name="E"></param>
        /// <param name="m">used to log additional informations</param>
        public FrmShowException(string MainMsg, Exception E, IMetaData m) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            utils.SetColorOneTime(this, true);

            if (MainMsg == null) MainMsg = HelpUi.GetPrintable(E.Message);
            MainMessage.Text = MainMsg;
            string err = ErrorLogger.GetErrorString(E);
            
            if (err.Contains("OutOfMemoryException")) {
                err += "\r\nSi è verificato un problema di 'out of memory', è necessario CHIUDERE il programma.";
            }
            txtMsg.Text = err;
            if (m != null) {
                ErrorLogger.Logger.logException(MainMsg, E);
            }
        }

        /// <summary>
        /// Shows an exception with a message, doesnt log to remote server
        /// </summary>
        /// <param name="MainMsg"></param>
        /// <param name="E"></param>
        public FrmShowException(string MainMsg, Exception E)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            utils.SetColorOneTime(this, true);

			if (MainMsg==null) MainMsg = ErrorLogger.GetErrorString(E);
			MainMessage.Text= MainMsg;
			txtMsg.Text= ErrorLogger.GetErrorString(E);
		}

        /// <summary>
        /// Displays a message with an optional detailed message, doesnt log to remote server
        /// </summary>
        /// <param name="MainMsg"></param>
        /// <param name="Long"></param>
		public FrmShowException(string MainMsg, string Long){
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            utils.SetColorOneTime(this, true);
            MainMessage.Text= MainMsg;
		    if (Long != null) {
                txtMsg.Text = Long;
            }
			
			if (Long==null){
				chkDettagli.Visible=false;
			}

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmShowException));
            this.MainMessage = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.chkDettagli = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // MainMessage
            // 
            this.MainMessage.AcceptsReturn = true;
            this.MainMessage.AcceptsTab = true;
            this.MainMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainMessage.Location = new System.Drawing.Point(99, 9);
            this.MainMessage.Multiline = true;
            this.MainMessage.Name = "MainMessage";
            this.MainMessage.ReadOnly = true;
            this.MainMessage.Size = new System.Drawing.Size(319, 64);
            this.MainMessage.TabIndex = 0;
            this.MainMessage.TabStop = false;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(194, 79);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(96, 24);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "Ok";
            // 
            // txtMsg
            // 
            this.txtMsg.AcceptsReturn = true;
            this.txtMsg.AcceptsTab = true;
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(12, 144);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsg.Size = new System.Drawing.Size(406, 0);
            this.txtMsg.TabIndex = 7;
            this.txtMsg.TabStop = false;
            // 
            // chkDettagli
            // 
            this.chkDettagli.Location = new System.Drawing.Point(2, 109);
            this.chkDettagli.Name = "chkDettagli";
            this.chkDettagli.Size = new System.Drawing.Size(128, 20);
            this.chkDettagli.TabIndex = 6;
            this.chkDettagli.TabStop = false;
            this.chkDettagli.Text = "Visualizza dettagli";
            this.chkDettagli.CheckStateChanged += new System.EventHandler(this.chkDettagli_CheckStateChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(11, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 69);
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // FrmShowException
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(424, 138);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.MainMessage);
            this.Controls.Add(this.chkDettagli);
            this.Name = "FrmShowException";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Errore";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void chkDettagli_CheckStateChanged(object sender, System.EventArgs e) {
			if (chkDettagli.Checked)
				this.Height=350;
			else
				this.Height=177;
		}
	}
}
