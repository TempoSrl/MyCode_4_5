using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace mdl_winform
{
	/// <summary>
	/// Summary description for FrmMeter.
	/// </summary>
	public class FrmMeter : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
        /// <summary>
        /// Progress bar displayed
        /// </summary>
		public System.Windows.Forms.ProgressBar pBar;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        /// <summary>
        /// Default constructor
        /// </summary>
		public FrmMeter()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.label1 = new System.Windows.Forms.Label();
			this.pBar = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = new System.Drawing.Point(8, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Attendere prego...";
			// 
			// pBar
			// 
			this.pBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pBar.Location = new System.Drawing.Point(8, 8);
			this.pBar.Name = "pBar";
			this.pBar.Size = new System.Drawing.Size(640, 24);
			this.pBar.Step = 1;
			this.pBar.TabIndex = 3;
			// 
			// FrmMeter
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(656, 61);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pBar);
			this.Name = "FrmMeter";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FrmMeter";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion
	}
}
