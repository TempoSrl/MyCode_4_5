namespace mdl_winform {
	partial class FrmSimpleList {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.btnCsv = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.comboTOP = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.btnPreview = new System.Windows.Forms.Button();
			this.btnPrint = new System.Windows.Forms.Button();
			this.BtnExcel = new System.Windows.Forms.Button();
			this.cboList = new System.Windows.Forms.ComboBox();
			this.g = new System.Windows.Forms.DataGrid();
			((System.ComponentModel.ISupportInitialize)(this.g)).BeginInit();
			this.SuspendLayout();
			// 
			// btnCsv
			// 
			this.btnCsv.Location = new System.Drawing.Point(517, 11);
			this.btnCsv.Name = "btnCsv";
			this.btnCsv.Size = new System.Drawing.Size(101, 23);
			this.btnCsv.TabIndex = 27;
			this.btnCsv.Text = "Esporta in CSV";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(277, 15);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(34, 13);
			this.label8.TabIndex = 26;
			this.label8.Text = "Limite";
			// 
			// comboTOP
			// 
			this.comboTOP.FormattingEnabled = true;
			this.comboTOP.Items.AddRange(new object[] {
            "100",
            "500",
            "1000"});
			this.comboTOP.Location = new System.Drawing.Point(317, 10);
			this.comboTOP.Name = "comboTOP";
			this.comboTOP.Size = new System.Drawing.Size(90, 21);
			this.comboTOP.TabIndex = 25;
			this.comboTOP.TabStop = false;
			this.comboTOP.Text = "1000";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(7, 11);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(49, 20);
			this.label7.TabIndex = 24;
			this.label7.Text = "Elenco";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnPreview
			// 
			this.btnPreview.Location = new System.Drawing.Point(714, 11);
			this.btnPreview.Name = "btnPreview";
			this.btnPreview.Size = new System.Drawing.Size(75, 23);
			this.btnPreview.TabIndex = 23;
			this.btnPreview.Text = "Anteprima...";
			this.btnPreview.Visible = false;
			// 
			// btnPrint
			// 
			this.btnPrint.Location = new System.Drawing.Point(633, 10);
			this.btnPrint.Name = "btnPrint";
			this.btnPrint.Size = new System.Drawing.Size(75, 23);
			this.btnPrint.TabIndex = 22;
			this.btnPrint.Text = "Stampa";
			this.btnPrint.Visible = false;
			// 
			// BtnExcel
			// 
			this.BtnExcel.Location = new System.Drawing.Point(413, 11);
			this.BtnExcel.Name = "BtnExcel";
			this.BtnExcel.Size = new System.Drawing.Size(98, 23);
			this.BtnExcel.TabIndex = 21;
			this.BtnExcel.Text = "Esporta in Excel";
			// 
			// cboList
			// 
			this.cboList.DropDownHeight = 162;
			this.cboList.IntegralHeight = false;
			this.cboList.Location = new System.Drawing.Point(62, 12);
			this.cboList.Name = "cboList";
			this.cboList.Size = new System.Drawing.Size(209, 21);
			this.cboList.TabIndex = 20;
			this.cboList.TabStop = false;
			// 
			// g
			// 
			this.g.AllowNavigation = false;
			this.g.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.g.DataMember = "";
			this.g.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.g.Location = new System.Drawing.Point(10, 40);
			this.g.Name = "g";
			this.g.ReadOnly = true;
			this.g.Size = new System.Drawing.Size(941, 436);
			this.g.TabIndex = 28;
			this.g.CurrentCellChanged += new System.EventHandler(this.gridElenchiCell_Click);
			this.g.Click += new System.EventHandler(this.gridElenchiCell_Click);
			this.g.DoubleClick += new System.EventHandler(this.gridElenchiCell_DoubleClick);
			// 
			// FrmSimpleList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(952, 488);
			this.Controls.Add(this.g);
			this.Controls.Add(this.btnCsv);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.comboTOP);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.btnPreview);
			this.Controls.Add(this.btnPrint);
			this.Controls.Add(this.BtnExcel);
			this.Controls.Add(this.cboList);
			this.Name = "FrmSimpleList";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "FrmSimpleList";
			((System.ComponentModel.ISupportInitialize)(this.g)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnCsv;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox comboTOP;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnPreview;
		private System.Windows.Forms.Button btnPrint;
		private System.Windows.Forms.Button BtnExcel;
		private System.Windows.Forms.ComboBox cboList;
		private System.Windows.Forms.DataGrid g;
	}
}