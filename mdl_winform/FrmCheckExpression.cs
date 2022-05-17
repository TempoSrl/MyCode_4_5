using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using System.Reflection;
using System.Drawing.Imaging;
using mdl;

namespace mdl_winform
{
	/// <summary>
	/// Summary description for FrmCheckExpression.
	/// </summary>
	public class FrmCheckExpression : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbTabella;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtExpr;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtResult;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtError;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnCalcola;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtFilter;
		private System.Windows.Forms.CheckBox chkSelect;
		private System.Windows.Forms.TabControl tabCollect;
		private System.Windows.Forms.TabPage tabCompute;
		private System.Windows.Forms.TabPage tabSelect;
		private System.Windows.Forms.TabPage tabError;
		private System.Windows.Forms.TabPage tabParams;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Button btnViewOutput;
		private System.Windows.Forms.Button btnEnableProfile;
		private System.Windows.Forms.Button btnDisableProfiler;
		private System.Windows.Forms.Button btnResetTimers;
		private System.Windows.Forms.TextBox txtOutput;
		private System.Windows.Forms.TabPage tabMemory;
		private System.Windows.Forms.TextBox txtMemory;
		private System.Windows.Forms.Button btnViewMemory;
		private System.Windows.Forms.Button btnGCCollect;
		private System.Windows.Forms.Button btnWaitForPending;
		private System.Windows.Forms.Button BtnDataSetToExcel;
		private System.Windows.Forms.TabPage tabComandi;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		DataSet DS;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label labHelpFileName;
        private Label label7;
        private Label label6;
        private CheckBox chkDiff;
        private CheckBox chkNuove;
        private CheckBox chkCancellate;
        private CheckBox chkOriginali;
        private CheckBox chkTabMod;
        private Button btnPingServer;
		IWinFormMetaData Meta;
        private TabPage tabGeneraSQL;
        private TextBox txtFiltro;
        private Label label9;
        private Button btnGenera;
        private TextBox txtOutputFile;
        private Label label8;
        private GroupBox groupBoxTipoAggiornamento;
        private RadioButton radioButBulkInsert;
        private RadioButton radioButtonOnlyUpdate;
        private RadioButton radioButtonOnlyInsert;
        private RadioButton radioButtonInsertAndUpdate;
        private TextBox txtSelectCond;
        private Label label10;
        private TextBox txtAssembly;
        private Label label11;
        private Button btnSaveScreen;
        private Button btnElencaDiff;
        private Button btnSbloccaTutto;
        IDataAccess Conn;
        private Button btnCheckIndexes;
		private DataGridView gridSelect;
		private DataGridView gridoriginal;
		private ISecurity _security;
		IFormController controller;
        /// <summary>
        /// Debug form, used by developers to evaluate expressions run time
        /// </summary>
        /// <param name="Meta"></param>
        /// <param name="DS"></param>
		public FrmCheckExpression(IFormController controller, DataSet DS)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			utils.SetColorOneTime(this, true);
			this.DS=DS;
            PostData.RemoveFalseUpdates(DS);
			this.Meta= controller.meta;
			this.controller = controller;

			this.Conn = controller.conn;
			this._security = controller.linkedForm.getInstance<ISecurity>();
			FillComboTables();
			Form F = controller.linkedForm;
			labHelpFileName.Text="";
			if (F!=null) {
				labHelpFileName.Text= FormController.GetHelpFileName(F);
			}
            Assembly [] currAss = AppDomain.CurrentDomain.GetAssemblies();
            StringBuilder sb = new StringBuilder();
            foreach (Assembly A in currAss) {
                sb.AppendLine(A.GetName().Name.PadLeft(40) + ":" + A.GetName().Version);
            }
            txtAssembly.Text = sb.ToString();
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
			this.cmbTabella = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtExpr = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtResult = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtError = new System.Windows.Forms.TextBox();
			this.btnCalcola = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.txtFilter = new System.Windows.Forms.TextBox();
			this.chkSelect = new System.Windows.Forms.CheckBox();
			this.tabCollect = new System.Windows.Forms.TabControl();
			this.tabParams = new System.Windows.Forms.TabPage();
			this.txtSelectCond = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.chkTabMod = new System.Windows.Forms.CheckBox();
			this.chkOriginali = new System.Windows.Forms.CheckBox();
			this.chkNuove = new System.Windows.Forms.CheckBox();
			this.chkCancellate = new System.Windows.Forms.CheckBox();
			this.chkDiff = new System.Windows.Forms.CheckBox();
			this.tabCompute = new System.Windows.Forms.TabPage();
			this.tabSelect = new System.Windows.Forms.TabPage();
			this.gridoriginal = new System.Windows.Forms.DataGridView();
			this.gridSelect = new System.Windows.Forms.DataGridView();
			this.btnElencaDiff = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.tabError = new System.Windows.Forms.TabPage();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.btnCheckIndexes = new System.Windows.Forms.Button();
			this.BtnDataSetToExcel = new System.Windows.Forms.Button();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.btnResetTimers = new System.Windows.Forms.Button();
			this.btnDisableProfiler = new System.Windows.Forms.Button();
			this.btnEnableProfile = new System.Windows.Forms.Button();
			this.btnViewOutput = new System.Windows.Forms.Button();
			this.tabMemory = new System.Windows.Forms.TabPage();
			this.btnWaitForPending = new System.Windows.Forms.Button();
			this.btnGCCollect = new System.Windows.Forms.Button();
			this.btnViewMemory = new System.Windows.Forms.Button();
			this.txtMemory = new System.Windows.Forms.TextBox();
			this.tabComandi = new System.Windows.Forms.TabPage();
			this.btnSbloccaTutto = new System.Windows.Forms.Button();
			this.btnSaveScreen = new System.Windows.Forms.Button();
			this.btnPingServer = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.txtAssembly = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.labHelpFileName = new System.Windows.Forms.Label();
			this.tabGeneraSQL = new System.Windows.Forms.TabPage();
			this.txtFiltro = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.btnGenera = new System.Windows.Forms.Button();
			this.txtOutputFile = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBoxTipoAggiornamento = new System.Windows.Forms.GroupBox();
			this.radioButBulkInsert = new System.Windows.Forms.RadioButton();
			this.radioButtonOnlyUpdate = new System.Windows.Forms.RadioButton();
			this.radioButtonOnlyInsert = new System.Windows.Forms.RadioButton();
			this.radioButtonInsertAndUpdate = new System.Windows.Forms.RadioButton();
			this.tabCollect.SuspendLayout();
			this.tabParams.SuspendLayout();
			this.tabCompute.SuspendLayout();
			this.tabSelect.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridoriginal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridSelect)).BeginInit();
			this.tabError.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabMemory.SuspendLayout();
			this.tabComandi.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabGeneraSQL.SuspendLayout();
			this.groupBoxTipoAggiornamento.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Tabella";
			// 
			// cmbTabella
			// 
			this.cmbTabella.Location = new System.Drawing.Point(16, 24);
			this.cmbTabella.Name = "cmbTabella";
			this.cmbTabella.Size = new System.Drawing.Size(272, 21);
			this.cmbTabella.TabIndex = 1;
			this.cmbTabella.SelectedIndexChanged += new System.EventHandler(this.cmbTabella_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(160, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Espressione da valutare";
			// 
			// txtExpr
			// 
			this.txtExpr.AcceptsReturn = true;
			this.txtExpr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtExpr.Location = new System.Drawing.Point(16, 72);
			this.txtExpr.Multiline = true;
			this.txtExpr.Name = "txtExpr";
			this.txtExpr.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtExpr.Size = new System.Drawing.Size(899, 120);
			this.txtExpr.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(224, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "Risultato della valutazione";
			// 
			// txtResult
			// 
			this.txtResult.AcceptsReturn = true;
			this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtResult.Location = new System.Drawing.Point(8, 32);
			this.txtResult.Multiline = true;
			this.txtResult.Name = "txtResult";
			this.txtResult.ReadOnly = true;
			this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtResult.Size = new System.Drawing.Size(915, 470);
			this.txtResult.TabIndex = 5;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 8);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Errore (eventuale)";
			// 
			// txtError
			// 
			this.txtError.AcceptsReturn = true;
			this.txtError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtError.Location = new System.Drawing.Point(8, 32);
			this.txtError.Multiline = true;
			this.txtError.Name = "txtError";
			this.txtError.ReadOnly = true;
			this.txtError.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtError.Size = new System.Drawing.Size(907, 470);
			this.txtError.TabIndex = 7;
			// 
			// btnCalcola
			// 
			this.btnCalcola.Location = new System.Drawing.Point(388, 25);
			this.btnCalcola.Name = "btnCalcola";
			this.btnCalcola.Size = new System.Drawing.Size(75, 23);
			this.btnCalcola.TabIndex = 8;
			this.btnCalcola.Text = "Calcola";
			this.btnCalcola.Click += new System.EventHandler(this.btnCalcola_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(25, 331);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 23);
			this.label5.TabIndex = 9;
			this.label5.Text = "Filtro";
			// 
			// txtFilter
			// 
			this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilter.Location = new System.Drawing.Point(16, 357);
			this.txtFilter.Multiline = true;
			this.txtFilter.Name = "txtFilter";
			this.txtFilter.Size = new System.Drawing.Size(891, 148);
			this.txtFilter.TabIndex = 10;
			// 
			// chkSelect
			// 
			this.chkSelect.Location = new System.Drawing.Point(304, 24);
			this.chkSelect.Name = "chkSelect";
			this.chkSelect.Size = new System.Drawing.Size(104, 24);
			this.chkSelect.TabIndex = 11;
			this.chkSelect.Text = "SELECT";
			// 
			// tabCollect
			// 
			this.tabCollect.Controls.Add(this.tabParams);
			this.tabCollect.Controls.Add(this.tabCompute);
			this.tabCollect.Controls.Add(this.tabSelect);
			this.tabCollect.Controls.Add(this.tabError);
			this.tabCollect.Controls.Add(this.tabPage1);
			this.tabCollect.Controls.Add(this.tabMemory);
			this.tabCollect.Controls.Add(this.tabComandi);
			this.tabCollect.Controls.Add(this.tabPage2);
			this.tabCollect.Controls.Add(this.tabGeneraSQL);
			this.tabCollect.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabCollect.Location = new System.Drawing.Point(0, 0);
			this.tabCollect.Name = "tabCollect";
			this.tabCollect.SelectedIndex = 0;
			this.tabCollect.Size = new System.Drawing.Size(939, 558);
			this.tabCollect.TabIndex = 12;
			// 
			// tabParams
			// 
			this.tabParams.Controls.Add(this.txtSelectCond);
			this.tabParams.Controls.Add(this.label10);
			this.tabParams.Controls.Add(this.chkTabMod);
			this.tabParams.Controls.Add(this.chkOriginali);
			this.tabParams.Controls.Add(this.chkNuove);
			this.tabParams.Controls.Add(this.chkCancellate);
			this.tabParams.Controls.Add(this.chkDiff);
			this.tabParams.Controls.Add(this.btnCalcola);
			this.tabParams.Controls.Add(this.txtFilter);
			this.tabParams.Controls.Add(this.chkSelect);
			this.tabParams.Controls.Add(this.label1);
			this.tabParams.Controls.Add(this.label5);
			this.tabParams.Controls.Add(this.txtExpr);
			this.tabParams.Controls.Add(this.cmbTabella);
			this.tabParams.Controls.Add(this.label2);
			this.tabParams.Location = new System.Drawing.Point(4, 22);
			this.tabParams.Name = "tabParams";
			this.tabParams.Size = new System.Drawing.Size(931, 532);
			this.tabParams.TabIndex = 3;
			this.tabParams.Text = "Parametri calcolo";
			// 
			// txtSelectCond
			// 
			this.txtSelectCond.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSelectCond.Location = new System.Drawing.Point(16, 229);
			this.txtSelectCond.Multiline = true;
			this.txtSelectCond.Name = "txtSelectCond";
			this.txtSelectCond.ReadOnly = true;
			this.txtSelectCond.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtSelectCond.Size = new System.Drawing.Size(899, 99);
			this.txtSelectCond.TabIndex = 18;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(16, 213);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(166, 13);
			this.label10.TabIndex = 17;
			this.label10.Text = "Condizione di sicurezza per select";
			// 
			// chkTabMod
			// 
			this.chkTabMod.AutoSize = true;
			this.chkTabMod.Location = new System.Drawing.Point(594, 8);
			this.chkTabMod.Name = "chkTabMod";
			this.chkTabMod.Size = new System.Drawing.Size(112, 17);
			this.chkTabMod.TabIndex = 16;
			this.chkTabMod.Text = "Tabelle modificate";
			this.chkTabMod.UseVisualStyleBackColor = false;
			// 
			// chkOriginali
			// 
			this.chkOriginali.AutoSize = true;
			this.chkOriginali.Location = new System.Drawing.Point(828, 28);
			this.chkOriginali.Name = "chkOriginali";
			this.chkOriginali.Size = new System.Drawing.Size(63, 17);
			this.chkOriginali.TabIndex = 15;
			this.chkOriginali.Text = "Originali";
			this.chkOriginali.UseVisualStyleBackColor = false;
			// 
			// chkNuove
			// 
			this.chkNuove.AutoSize = true;
			this.chkNuove.Location = new System.Drawing.Point(756, 28);
			this.chkNuove.Name = "chkNuove";
			this.chkNuove.Size = new System.Drawing.Size(68, 17);
			this.chkNuove.TabIndex = 14;
			this.chkNuove.Text = "Aggiunte";
			this.chkNuove.UseVisualStyleBackColor = false;
			// 
			// chkCancellate
			// 
			this.chkCancellate.AutoSize = true;
			this.chkCancellate.Location = new System.Drawing.Point(675, 28);
			this.chkCancellate.Name = "chkCancellate";
			this.chkCancellate.Size = new System.Drawing.Size(76, 17);
			this.chkCancellate.TabIndex = 13;
			this.chkCancellate.Text = "Cancellate";
			this.chkCancellate.UseVisualStyleBackColor = false;
			// 
			// chkDiff
			// 
			this.chkDiff.AutoSize = true;
			this.chkDiff.Location = new System.Drawing.Point(594, 29);
			this.chkDiff.Name = "chkDiff";
			this.chkDiff.Size = new System.Drawing.Size(75, 17);
			this.chkDiff.TabIndex = 12;
			this.chkDiff.Text = "Modificate";
			this.chkDiff.UseVisualStyleBackColor = false;
			// 
			// tabCompute
			// 
			this.tabCompute.Controls.Add(this.txtResult);
			this.tabCompute.Controls.Add(this.label3);
			this.tabCompute.Location = new System.Drawing.Point(4, 22);
			this.tabCompute.Name = "tabCompute";
			this.tabCompute.Size = new System.Drawing.Size(931, 532);
			this.tabCompute.TabIndex = 0;
			this.tabCompute.Text = "Risultato di Compute";
			// 
			// tabSelect
			// 
			this.tabSelect.Controls.Add(this.gridoriginal);
			this.tabSelect.Controls.Add(this.gridSelect);
			this.tabSelect.Controls.Add(this.btnElencaDiff);
			this.tabSelect.Controls.Add(this.label7);
			this.tabSelect.Controls.Add(this.label6);
			this.tabSelect.Location = new System.Drawing.Point(4, 22);
			this.tabSelect.Name = "tabSelect";
			this.tabSelect.Size = new System.Drawing.Size(931, 532);
			this.tabSelect.TabIndex = 1;
			this.tabSelect.Text = "Risultato di SELECT";
			// 
			// gridoriginal
			// 
			this.gridoriginal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridoriginal.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridoriginal.Location = new System.Drawing.Point(16, 318);
			this.gridoriginal.Name = "gridoriginal";
			this.gridoriginal.Size = new System.Drawing.Size(899, 206);
			this.gridoriginal.TabIndex = 6;
			this.gridoriginal.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.gridoriginal_DataError);
			this.gridoriginal.Scroll += new System.Windows.Forms.ScrollEventHandler(this.gridoriginal_Scroll);
			// 
			// gridSelect
			// 
			this.gridSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridSelect.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridSelect.Location = new System.Drawing.Point(16, 43);
			this.gridSelect.Name = "gridSelect";
			this.gridSelect.Size = new System.Drawing.Size(899, 244);
			this.gridSelect.TabIndex = 5;
			this.gridSelect.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.gridSelect_DataError);
			this.gridSelect.Scroll += new System.Windows.Forms.ScrollEventHandler(this.gridSelect_Scroll);
			// 
			// btnElencaDiff
			// 
			this.btnElencaDiff.Location = new System.Drawing.Point(612, 3);
			this.btnElencaDiff.Name = "btnElencaDiff";
			this.btnElencaDiff.Size = new System.Drawing.Size(197, 23);
			this.btnElencaDiff.TabIndex = 4;
			this.btnElencaDiff.Text = "Elenca campi differenza";
			this.btnElencaDiff.UseVisualStyleBackColor = false;
			this.btnElencaDiff.Click += new System.EventHandler(this.btnElencaDiff_Click);
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(13, 302);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(71, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Valori originali";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(13, 14);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(71, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "Valori correnti";
			// 
			// tabError
			// 
			this.tabError.Controls.Add(this.txtError);
			this.tabError.Controls.Add(this.label4);
			this.tabError.Location = new System.Drawing.Point(4, 22);
			this.tabError.Name = "tabError";
			this.tabError.Size = new System.Drawing.Size(931, 532);
			this.tabError.TabIndex = 2;
			this.tabError.Text = "Errore";
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.btnCheckIndexes);
			this.tabPage1.Controls.Add(this.BtnDataSetToExcel);
			this.tabPage1.Controls.Add(this.txtOutput);
			this.tabPage1.Controls.Add(this.btnResetTimers);
			this.tabPage1.Controls.Add(this.btnDisableProfiler);
			this.tabPage1.Controls.Add(this.btnEnableProfile);
			this.tabPage1.Controls.Add(this.btnViewOutput);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(931, 532);
			this.tabPage1.TabIndex = 4;
			this.tabPage1.Text = "Output";
			// 
			// btnCheckIndexes
			// 
			this.btnCheckIndexes.Location = new System.Drawing.Point(614, 19);
			this.btnCheckIndexes.Name = "btnCheckIndexes";
			this.btnCheckIndexes.Size = new System.Drawing.Size(108, 23);
			this.btnCheckIndexes.TabIndex = 7;
			this.btnCheckIndexes.Text = "Check Indexes";
			this.btnCheckIndexes.Click += new System.EventHandler(this.btnCheckIndexes_Click);
			// 
			// BtnDataSetToExcel
			// 
			this.BtnDataSetToExcel.Location = new System.Drawing.Point(464, 16);
			this.BtnDataSetToExcel.Name = "BtnDataSetToExcel";
			this.BtnDataSetToExcel.Size = new System.Drawing.Size(120, 23);
			this.BtnDataSetToExcel.TabIndex = 6;
			this.BtnDataSetToExcel.Text = "DataSetToExcel";
			this.BtnDataSetToExcel.Click += new System.EventHandler(this.BtnDataSetToExcel_Click);
			// 
			// txtOutput
			// 
			this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtOutput.Location = new System.Drawing.Point(16, 48);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOutput.Size = new System.Drawing.Size(899, 473);
			this.txtOutput.TabIndex = 5;
			// 
			// btnResetTimers
			// 
			this.btnResetTimers.Location = new System.Drawing.Point(328, 16);
			this.btnResetTimers.Name = "btnResetTimers";
			this.btnResetTimers.Size = new System.Drawing.Size(88, 23);
			this.btnResetTimers.TabIndex = 4;
			this.btnResetTimers.Text = "Azzera timers";
			this.btnResetTimers.Click += new System.EventHandler(this.btnResetTimers_Click);
			// 
			// btnDisableProfiler
			// 
			this.btnDisableProfiler.Location = new System.Drawing.Point(216, 16);
			this.btnDisableProfiler.Name = "btnDisableProfiler";
			this.btnDisableProfiler.Size = new System.Drawing.Size(96, 23);
			this.btnDisableProfiler.TabIndex = 2;
			this.btnDisableProfiler.Text = "Disabilita timers";
			this.btnDisableProfiler.Click += new System.EventHandler(this.btnDisableProfiler_Click);
			// 
			// btnEnableProfile
			// 
			this.btnEnableProfile.Location = new System.Drawing.Point(112, 16);
			this.btnEnableProfile.Name = "btnEnableProfile";
			this.btnEnableProfile.Size = new System.Drawing.Size(88, 23);
			this.btnEnableProfile.TabIndex = 1;
			this.btnEnableProfile.Text = "Abilita Timers";
			this.btnEnableProfile.Click += new System.EventHandler(this.btnEnableProfile_Click);
			// 
			// btnViewOutput
			// 
			this.btnViewOutput.Location = new System.Drawing.Point(16, 16);
			this.btnViewOutput.Name = "btnViewOutput";
			this.btnViewOutput.Size = new System.Drawing.Size(75, 23);
			this.btnViewOutput.TabIndex = 0;
			this.btnViewOutput.Text = "View";
			this.btnViewOutput.Click += new System.EventHandler(this.btnViewOutput_Click);
			// 
			// tabMemory
			// 
			this.tabMemory.Controls.Add(this.btnWaitForPending);
			this.tabMemory.Controls.Add(this.btnGCCollect);
			this.tabMemory.Controls.Add(this.btnViewMemory);
			this.tabMemory.Controls.Add(this.txtMemory);
			this.tabMemory.Location = new System.Drawing.Point(4, 22);
			this.tabMemory.Name = "tabMemory";
			this.tabMemory.Size = new System.Drawing.Size(931, 532);
			this.tabMemory.TabIndex = 5;
			this.tabMemory.Text = "Memory";
			// 
			// btnWaitForPending
			// 
			this.btnWaitForPending.Location = new System.Drawing.Point(224, 16);
			this.btnWaitForPending.Name = "btnWaitForPending";
			this.btnWaitForPending.Size = new System.Drawing.Size(176, 23);
			this.btnWaitForPending.TabIndex = 3;
			this.btnWaitForPending.Text = "Wait For Pending Finalizers";
			this.btnWaitForPending.Click += new System.EventHandler(this.btnWaitForPending_Click);
			// 
			// btnGCCollect
			// 
			this.btnGCCollect.Location = new System.Drawing.Point(112, 16);
			this.btnGCCollect.Name = "btnGCCollect";
			this.btnGCCollect.Size = new System.Drawing.Size(88, 23);
			this.btnGCCollect.TabIndex = 2;
			this.btnGCCollect.Text = "GC.Collect";
			this.btnGCCollect.Click += new System.EventHandler(this.btnGCCollect_Click);
			// 
			// btnViewMemory
			// 
			this.btnViewMemory.Location = new System.Drawing.Point(16, 16);
			this.btnViewMemory.Name = "btnViewMemory";
			this.btnViewMemory.Size = new System.Drawing.Size(75, 23);
			this.btnViewMemory.TabIndex = 1;
			this.btnViewMemory.Text = "View";
			this.btnViewMemory.Click += new System.EventHandler(this.btnViewMemory_Click);
			// 
			// txtMemory
			// 
			this.txtMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtMemory.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtMemory.Location = new System.Drawing.Point(16, 56);
			this.txtMemory.Multiline = true;
			this.txtMemory.Name = "txtMemory";
			this.txtMemory.Size = new System.Drawing.Size(899, 457);
			this.txtMemory.TabIndex = 0;
			// 
			// tabComandi
			// 
			this.tabComandi.Controls.Add(this.btnSbloccaTutto);
			this.tabComandi.Controls.Add(this.btnSaveScreen);
			this.tabComandi.Controls.Add(this.btnPingServer);
			this.tabComandi.Controls.Add(this.button2);
			this.tabComandi.Controls.Add(this.button1);
			this.tabComandi.Location = new System.Drawing.Point(4, 22);
			this.tabComandi.Name = "tabComandi";
			this.tabComandi.Size = new System.Drawing.Size(931, 532);
			this.tabComandi.TabIndex = 6;
			this.tabComandi.Text = "Comandi";
			// 
			// btnSbloccaTutto
			// 
			this.btnSbloccaTutto.Location = new System.Drawing.Point(561, 48);
			this.btnSbloccaTutto.Name = "btnSbloccaTutto";
			this.btnSbloccaTutto.Size = new System.Drawing.Size(112, 23);
			this.btnSbloccaTutto.TabIndex = 4;
			this.btnSbloccaTutto.Text = "Sblocca tutto";
			this.btnSbloccaTutto.UseVisualStyleBackColor = false;
			this.btnSbloccaTutto.Click += new System.EventHandler(this.btnSbloccaTutto_Click);
			// 
			// btnSaveScreen
			// 
			this.btnSaveScreen.Location = new System.Drawing.Point(66, 227);
			this.btnSaveScreen.Name = "btnSaveScreen";
			this.btnSaveScreen.Size = new System.Drawing.Size(112, 23);
			this.btnSaveScreen.TabIndex = 3;
			this.btnSaveScreen.Text = "Save screen";
			this.btnSaveScreen.UseVisualStyleBackColor = false;
			this.btnSaveScreen.Visible = false;
			this.btnSaveScreen.Click += new System.EventHandler(this.btnSaveScreen_Click);
			// 
			// btnPingServer
			// 
			this.btnPingServer.Location = new System.Drawing.Point(66, 144);
			this.btnPingServer.Name = "btnPingServer";
			this.btnPingServer.Size = new System.Drawing.Size(141, 23);
			this.btnPingServer.TabIndex = 2;
			this.btnPingServer.Text = "Ping Server";
			this.btnPingServer.UseVisualStyleBackColor = false;
			this.btnPingServer.Click += new System.EventHandler(this.btnPingServer_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(32, 88);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(384, 23);
			this.button2.TabIndex = 1;
			this.button2.Text = "Metti i dati del DataSet nel Form (FreshForm(tue))";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(32, 48);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(384, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Leggi i dati dal form e mettili nel DataSet (GetFormData)";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.txtAssembly);
			this.tabPage2.Controls.Add(this.label11);
			this.tabPage2.Controls.Add(this.labHelpFileName);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(931, 532);
			this.tabPage2.TabIndex = 7;
			this.tabPage2.Text = "Help del Form";
			// 
			// txtAssembly
			// 
			this.txtAssembly.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtAssembly.Location = new System.Drawing.Point(19, 67);
			this.txtAssembly.Multiline = true;
			this.txtAssembly.Name = "txtAssembly";
			this.txtAssembly.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtAssembly.Size = new System.Drawing.Size(890, 457);
			this.txtAssembly.TabIndex = 2;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(16, 51);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(101, 13);
			this.label11.TabIndex = 1;
			this.label11.Text = "Versioni di assembly";
			// 
			// labHelpFileName
			// 
			this.labHelpFileName.Location = new System.Drawing.Point(16, 16);
			this.labHelpFileName.Name = "labHelpFileName";
			this.labHelpFileName.Size = new System.Drawing.Size(536, 23);
			this.labHelpFileName.TabIndex = 0;
			this.labHelpFileName.Text = "label6";
			this.labHelpFileName.Click += new System.EventHandler(this.label6_Click);
			// 
			// tabGeneraSQL
			// 
			this.tabGeneraSQL.Controls.Add(this.txtFiltro);
			this.tabGeneraSQL.Controls.Add(this.label9);
			this.tabGeneraSQL.Controls.Add(this.btnGenera);
			this.tabGeneraSQL.Controls.Add(this.txtOutputFile);
			this.tabGeneraSQL.Controls.Add(this.label8);
			this.tabGeneraSQL.Controls.Add(this.groupBoxTipoAggiornamento);
			this.tabGeneraSQL.Location = new System.Drawing.Point(4, 22);
			this.tabGeneraSQL.Name = "tabGeneraSQL";
			this.tabGeneraSQL.Padding = new System.Windows.Forms.Padding(3);
			this.tabGeneraSQL.Size = new System.Drawing.Size(931, 532);
			this.tabGeneraSQL.TabIndex = 8;
			this.tabGeneraSQL.Text = "Genera";
			// 
			// txtFiltro
			// 
			this.txtFiltro.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFiltro.Location = new System.Drawing.Point(19, 163);
			this.txtFiltro.Name = "txtFiltro";
			this.txtFiltro.Size = new System.Drawing.Size(495, 20);
			this.txtFiltro.TabIndex = 2;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(19, 139);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 16);
			this.label9.TabIndex = 44;
			this.label9.Text = "Filtro dati";
			// 
			// btnGenera
			// 
			this.btnGenera.Location = new System.Drawing.Point(316, 265);
			this.btnGenera.Name = "btnGenera";
			this.btnGenera.Size = new System.Drawing.Size(75, 23);
			this.btnGenera.TabIndex = 4;
			this.btnGenera.Text = "Genera";
			this.btnGenera.Click += new System.EventHandler(this.btnGenera_Click);
			// 
			// txtOutputFile
			// 
			this.txtOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutputFile.Location = new System.Drawing.Point(19, 225);
			this.txtOutputFile.Name = "txtOutputFile";
			this.txtOutputFile.Size = new System.Drawing.Size(519, 20);
			this.txtOutputFile.TabIndex = 3;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(19, 201);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(328, 16);
			this.label8.TabIndex = 40;
			this.label8.Text = "Inserisci il nome del file che verrà generato";
			// 
			// groupBoxTipoAggiornamento
			// 
			this.groupBoxTipoAggiornamento.Controls.Add(this.radioButBulkInsert);
			this.groupBoxTipoAggiornamento.Controls.Add(this.radioButtonOnlyUpdate);
			this.groupBoxTipoAggiornamento.Controls.Add(this.radioButtonOnlyInsert);
			this.groupBoxTipoAggiornamento.Controls.Add(this.radioButtonInsertAndUpdate);
			this.groupBoxTipoAggiornamento.Location = new System.Drawing.Point(22, 23);
			this.groupBoxTipoAggiornamento.Name = "groupBoxTipoAggiornamento";
			this.groupBoxTipoAggiornamento.Size = new System.Drawing.Size(216, 104);
			this.groupBoxTipoAggiornamento.TabIndex = 1;
			this.groupBoxTipoAggiornamento.TabStop = false;
			this.groupBoxTipoAggiornamento.Text = "Tipo modifica";
			// 
			// radioButBulkInsert
			// 
			this.radioButBulkInsert.Location = new System.Drawing.Point(8, 64);
			this.radioButBulkInsert.Name = "radioButBulkInsert";
			this.radioButBulkInsert.Size = new System.Drawing.Size(200, 24);
			this.radioButBulkInsert.TabIndex = 3;
			this.radioButBulkInsert.Text = "Bulk Insert (solo su tabelle vuote)";
			// 
			// radioButtonOnlyUpdate
			// 
			this.radioButtonOnlyUpdate.Location = new System.Drawing.Point(8, 48);
			this.radioButtonOnlyUpdate.Name = "radioButtonOnlyUpdate";
			this.radioButtonOnlyUpdate.Size = new System.Drawing.Size(200, 16);
			this.radioButtonOnlyUpdate.TabIndex = 2;
			this.radioButtonOnlyUpdate.Text = "Solo Aggiornamento righe esistenti";
			// 
			// radioButtonOnlyInsert
			// 
			this.radioButtonOnlyInsert.Location = new System.Drawing.Point(8, 32);
			this.radioButtonOnlyInsert.Name = "radioButtonOnlyInsert";
			this.radioButtonOnlyInsert.Size = new System.Drawing.Size(200, 16);
			this.radioButtonOnlyInsert.TabIndex = 1;
			this.radioButtonOnlyInsert.Text = "Solo Inserimento nuove righe";
			// 
			// radioButtonInsertAndUpdate
			// 
			this.radioButtonInsertAndUpdate.Checked = true;
			this.radioButtonInsertAndUpdate.Location = new System.Drawing.Point(8, 16);
			this.radioButtonInsertAndUpdate.Name = "radioButtonInsertAndUpdate";
			this.radioButtonInsertAndUpdate.Size = new System.Drawing.Size(176, 16);
			this.radioButtonInsertAndUpdate.TabIndex = 0;
			this.radioButtonInsertAndUpdate.TabStop = true;
			this.radioButtonInsertAndUpdate.Text = "Inserimento e Aggiornamento";
			// 
			// FrmCheckExpression
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(939, 558);
			this.Controls.Add(this.tabCollect);
			this.MinimumSize = new System.Drawing.Size(432, 408);
			this.Name = "FrmCheckExpression";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FrmCheckExpression";
			this.TopMost = true;
			this.tabCollect.ResumeLayout(false);
			this.tabParams.ResumeLayout(false);
			this.tabParams.PerformLayout();
			this.tabCompute.ResumeLayout(false);
			this.tabCompute.PerformLayout();
			this.tabSelect.ResumeLayout(false);
			this.tabSelect.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridoriginal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridSelect)).EndInit();
			this.tabError.ResumeLayout(false);
			this.tabError.PerformLayout();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabMemory.ResumeLayout(false);
			this.tabMemory.PerformLayout();
			this.tabComandi.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.tabGeneraSQL.ResumeLayout(false);
			this.tabGeneraSQL.PerformLayout();
			this.groupBoxTipoAggiornamento.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		void FillComboTables(){
			foreach(DataTable T in DS.Tables){
				cmbTabella.Items.Add(T.TableName);
			}
		}

        
		private void btnCalcola_Click(object sender, System.EventArgs e) {
            if (chkTabMod.Checked) {
                string res = "";
                foreach (DataTable T in DS.Tables) {
                    if (!PostData.hasChanges(T)) continue;
                    res += T.TableName + " ";
                }
                txtResult.Text = res;
                tabCollect.SelectedTab = tabCompute;
                chkTabMod.Checked = false;
                chkDiff.Checked = true;
                return;
            }

			string tablename= cmbTabella.Text;
			if (tablename=="") return;

			try {
				DataTable T = DS.Tables[tablename];
				string expr = txtExpr.Text;
				string filter = txtFilter.Text;

				if (chkSelect.Checked){
                    bool c = false;
                    if (chkDiff.Checked) {
                        DataView DV = new DataView(T, filter, null, DataViewRowState.ModifiedCurrent);
                        gridSelect.DataSource = DV;
                        DataView DV2 = new DataView(T, filter, null, DataViewRowState.ModifiedOriginal);
                        gridoriginal.DataSource = DV2;
                        c = true;
                    }
                    if (chkNuove.Checked) {
                        DataView DV = new DataView(T, filter, null, DataViewRowState.Added);
                        gridSelect.DataSource = DV;
                        gridoriginal.DataSource = null;
                        c = true;
                    }
                    if (chkCancellate.Checked) {
                        DataView DV = new DataView(T, filter, null, DataViewRowState.Deleted);
                        gridSelect.DataSource = DV;
                        gridoriginal.DataSource = null;
                        c = true;
                    }
                    if (chkOriginali.Checked) {
                        DataView DV = new DataView(T, filter, null, DataViewRowState.OriginalRows);
                        gridSelect.DataSource = DV;
                        gridoriginal.DataSource = null;
                        c = true;
                    }
                    if (c == false) {
                        DataView DV = new DataView(T, filter, null, DataViewRowState.CurrentRows);
                        gridSelect.DataSource = DV;
						gridSelect.ResetBindings();

                        DataView DV2 = new DataView(T, filter, null, DataViewRowState.OriginalRows);
                        gridoriginal.DataSource = DV2;
                        gridoriginal.ResetBindings();

                    }

                    tabCollect.SelectedTab= tabSelect;
					txtError.Text="";
					txtResult.Text="";
				}
				else {
					object O = T.Compute(expr,filter);
					txtResult.Text= O.ToString();
					txtError.Text="";
					gridSelect.DataSource=null;
					tabCollect.SelectedTab= tabCompute;
				}
			}
			catch (Exception E) {
				txtResult.Text="";				
				txtError.Text= E.Message;
				gridSelect.DataSource=null;
				tabCollect.SelectedTab= tabError;
			}
		}

		private void btnViewOutput_Click(object sender, System.EventArgs e) {
            txtOutput.Text = mdl_utils.metaprofiler.ShowAll(); ;
		}

		private void btnEnableProfile_Click(object sender, System.EventArgs e) {
			mdl_utils.metaprofiler.Enabled=true;
		}

		private void btnDisableProfiler_Click(object sender, System.EventArgs e) {
			mdl_utils.metaprofiler.Enabled=false;
		}

		private void btnResetTimers_Click(object sender, System.EventArgs e) {
			mdl_utils.metaprofiler.Reset();
            txtOutput.Text = "";
		}

		private void btnViewMemory_Click(object sender, System.EventArgs e) {
			long free = GC.GetTotalMemory(false);
			long freetot = GC.GetTotalMemory(true);
			string mem= "GC.GetTotalMemory(false)="+free+"\r\n";
			mem+= "GC.GetTotalMemory(true)="+freetot+"\r\n";
			txtMemory.Text+= QueryCreator.GetPrintable(mem);
		}

		private void btnGCCollect_Click(object sender, System.EventArgs e) {
			GC.Collect();
		}

		private void btnWaitForPending_Click(object sender, System.EventArgs e) {
			GC.WaitForPendingFinalizers();
		}

		private void BtnDataSetToExcel_Click(object sender, System.EventArgs e) {
			DS.WriteXml("TestXML", XmlWriteMode.WriteSchema);
		
		}

		private void button1_Click(object sender, System.EventArgs e) {
			controller.GetFormData(true);
		}

		private void button2_Click(object sender, System.EventArgs e) {
			controller.FreshForm();
		}

		private void label6_Click(object sender, System.EventArgs e) {
		
		}

	    private IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();

        private void btnPingServer_Click(object sender, EventArgs e) {
            DateTime myA = DateTime.Now;
            object Ta = Conn.DO_SYS_CMD("select getdate()");
            DateTime myB = DateTime.Now;
            object Tb = Conn.DO_SYS_CMD("select getdate()");
            DateTime T1 = (DateTime) (Ta);
            DateTime T2 = (DateTime) (Tb);

            double client_span = (myB.Subtract(myA)).TotalMilliseconds;
            double span = T2.Subtract(T1).TotalMilliseconds;

            shower.Show(this,"Server pinged. " + span.ToString() + 
                        " ms elapsed on server. Client time between calls:"+client_span.ToString()+ "ms", "Info");
        }


        private bool Validazioni() {
            if (cmbTabella.Text.Trim() == "") {
                shower.Show(this,"Selezionare il nome della tabella", "Attenzione",
                    mdl.MessageBoxButtons.OK); //, MessageBoxIcon.Exclamation
                return false;
            }
            if (txtOutputFile.Text.Trim() == "") {
                shower.Show(this, "Inserire il nome del file che verrà generato", "Attenzione",
					mdl.MessageBoxButtons.OK); //, MessageBoxIcon.Exclamation
                return false;
            }
            if (File.Exists(txtOutputFile.Text.Trim())) {
				var res = shower.Show(this, "Il file esiste, sovrascriverlo?", "Attenzione",
					mdl.MessageBoxButtons.YesNoCancel);//, MessageBoxIcon.Question
                if (res != mdl.DialogResult.Yes) return false;
            }
            return true;
        }
        /// <summary>
        /// Kind of sql generation
        /// </summary>
        public enum UpdateType : int {
            /// <summary>
            /// Only insert
            /// </summary>
            onlyInsert = 1,
            /// <summary>
            /// Onlu updates of existings rows
            /// </summary>
            onlyUpdate = 2,
            /// <summary>
            /// Insert and updates
            /// </summary>
            insertAndUpdate = 3,
            /// <summary>
            /// unchecked insert
            /// </summary>
            bulkinsert = 4 };

        private void btnGenera_Click(object sender, System.EventArgs e) {
            if (!Validazioni()) return;
            string tablename = cmbTabella.Text;
            Cursor.Current = Cursors.WaitCursor;
            string filter = null;
            if (txtFiltro.Text.Trim() != "") filter = txtFiltro.Text.Trim();
            DataTable t = DS.Tables[tablename];
            //if (chkSoloDati.Checked && chkSoloDati.Enabled && (txtColonne.Text.Trim() != "")) columns = txtColonne.Text;
            //t = Conn.CreateTableByName(cboTable.Text.Trim(), columns, true);
            if (t.Select(filter).Length==0) {
                    shower.Show(this, "Nessuna riga trovata. Filtro:" + filter );
                    return;            
            }
                        
            Conn.AddExtendedProperty(t);
            
            
            UpdateType updateType = UpdateType.insertAndUpdate;
            if (radioButtonOnlyInsert.Checked) {
                updateType = UpdateType.onlyInsert;
            }
            if (radioButtonOnlyUpdate.Checked) {
                updateType = UpdateType.onlyUpdate;
            }
            if (radioButBulkInsert.Checked) {
                updateType = UpdateType.bulkinsert;
            }
            if (tablename == "audit" && ( updateType == UpdateType.insertAndUpdate || updateType == UpdateType.onlyUpdate)) {
                shower.Show(this, "Sulla tabella audit non sono ammesse generazioni in update, cambio impostazione in insert", "Avviso");
                updateType = UpdateType.onlyInsert;
            }
            try {


                StreamWriter writer = new StreamWriter(txtOutputFile.Text.Trim(), false, System.Text.Encoding.Default);

                DO_GENERATE(Conn, t, writer, updateType, filter);

                writer.Close();

                shower.Show(this, "Script generato con successo.");
            }
            catch (Exception E) {
                shower.ShowException(controller.linkedForm, "Errore salvando il file " + txtOutputFile.Text.Trim(), E);
            }
            Cursor.Current = Cursors.Default;
        }
       


        private static void DO_GENERATE(IDataAccess dataAccess, DataTable T, TextWriter writer,
            UpdateType updateType, string cfilter) {
            
            
           
            
            GetSQLData(T, updateType, writer,10,cfilter);
            
            writer.Write("-- FINE GENERAZIONE SCRIPT --\r\n\r\n");
            writer.Flush();
        }



        /// <summary>
        /// Crea uno script di soli dati di una tabella
        /// </summary>
        /// <param name="T">Tabella di cui si vuole creare lo script</param>
        /// <param name="updateType">tipo di aggiornamento (onlyInsert, bulkInsert, InsertAndUpdate)</param>
        /// <param name="writer">TextWriter su cui scrivere</param>
        /// <param name="rowsPerBlock">Se maggiore di 0, indica ogni quante righe nello script deve essere inserito un go;
        /// Se minore o uguale a 0 allora nello script non verranno inseriti i go</param>
        /// <param name="cfilter"></param>
        public static void GetSQLData(DataTable T, UpdateType updateType, TextWriter writer, int rowsPerBlock,string cfilter) {
            bool ConvertCRLFData = false;
            bool HasKey;
            string tablename = T.TableName;

            //--- è indipendente dalla riga  ----
            string insert = "INSERT INTO [" + tablename + "] (";
            foreach (DataColumn C in T.Columns) {
                insert += C.ColumnName + ",";
            }
            insert = insert.Remove(insert.Length - 1, 1);
            insert += ") VALUES (";
            // ----------------------------------

            int pkLenght = T.PrimaryKey.Length;
            int colcount = T.Columns.Count;

            writer.Write("\r\n-- GENERAZIONE DATI PER " + tablename + " --\r\n");
            int count = 0;
            string s = "";

            foreach (DataRow row in T.Select(cfilter)) {
                int i = 0;
                count++;
                string wherecond = "";
                HasKey = false;
                for (i = 0; i < pkLenght; i++) {
                    HasKey = true;
                    wherecond += T.PrimaryKey[i].ColumnName + " = " +
						mdl_utils.Quoting.quotedstrvalue(row[T.PrimaryKey[i].ColumnName], true) +
                        " AND ";
                }
                if ((updateType != UpdateType.bulkinsert) && (!HasKey)) continue;
                if (wherecond != "") wherecond = wherecond.Remove(wherecond.Length - 5, 5);

                switch (updateType) {
                    case UpdateType.insertAndUpdate: {
                            string update = GetSQLDataForUpdate( wherecond, ConvertCRLFData, row);
                            string values = GetSQLDataValues(row/*, ConvertCRLFData*/);
                            s += "IF exists(SELECT * FROM [" + tablename + "] WHERE " + wherecond + ")\r\n"
                                + update
                                + "ELSE\r\n"
                                + insert
                                + values;
                            break;
                        }
                    case UpdateType.onlyInsert: {
                            string values = GetSQLDataValues(row/*, ConvertCRLFData*/);
                            s += "IF not exists(SELECT * FROM [" + tablename + "] WHERE " + wherecond + ")\r\n"
                                + insert
                                + values;
                            break;
                        }
                    case UpdateType.bulkinsert: {
                            string values = GetSQLDataValues(row/*, ConvertCRLFData*/);
                            s += //"IF not exists(SELECT * FROM "+dbo+"["+tablename+"] WHERE "+wherecond+")\r\n"+
                                insert
                                + values;
                            break;
                        }


                    case UpdateType.onlyUpdate: {
                            //s = "IF exists(SELECT * FROM "+dbo+"["+tablename+"] WHERE "+wherecond+")\r\n"
                            s += GetSQLDataForUpdate(wherecond, ConvertCRLFData, row);
                            break;
                        }
                }
                if ((updateType == UpdateType.bulkinsert) || (updateType == UpdateType.onlyInsert)) {
                    if (count == rowsPerBlock) {
                        s += "GO\r\n\r\n";
                        writer.Write(s);
                        writer.Flush();
                        s = "";
                        count = 0;
                    }
                }
                else {
                    s += "GO\r\n\r\n";
                    writer.Write(s);
                    writer.Flush();
                    s = "";
                }
            }
            if (s != "") {
                if (rowsPerBlock > 0) {
                    s += "GO\r\n\r\n";
                }
                writer.Write(s);
                writer.Flush();
                s = "";
            }
        }

        /// <summary>
        /// Returns quoted values of a row separated by commas
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string GetSQLDataValues(DataRow row/*, bool ConvertCRLFData*/) {
            bool ConvertCRLFData = false;
            string s = "";
            int colcount = row.Table.Columns.Count;
            for (int i = 0; i < colcount; i++) {
                string valore = ConvertCRLFData
                    ? QueryCreator.GetPrintable(mdl_utils.Quoting.quotedstrvalue(row[i], true))
                    : mdl_utils.Quoting.quotedstrvalue(row[i], true);
                s += valore + ",";
            }
            s = s.Remove(s.Length - 1, 1);
            return s + ")\r\n";
        }

        private static string GetSQLDataForUpdate(string wherecond, bool ConvertCRLFData, DataRow row) {
            DataTable T = row.Table;
            string s = "UPDATE [" + T.TableName + "] SET ";

            for (int i = 0; i < T.Columns.Count; i++) {
                if (T.Columns[i].ExtendedProperties["iskey"].ToString().ToUpper() == "S") continue;
                string valore = "";
                if (ConvertCRLFData)
                    valore = QueryCreator.GetPrintable(mdl_utils.Quoting.quotedstrvalue(row[i], true));
                else
                    valore = mdl_utils.Quoting.quotedstrvalue(row[i], true);
                s += T.Columns[i].ColumnName + " = " + valore + ",";
            }
            s = s.Remove(s.Length - 1, 1);
            s += " WHERE " + wherecond + "\r\n";
            return s;
        }

        private void cmbTabella_SelectedIndexChanged(object sender, EventArgs e) {
            if (cmbTabella.SelectedItem == null || cmbTabella.SelectedItem.ToString()=="") return;
            txtSelectCond.Text = _security.SelectCondition(cmbTabella.SelectedItem.ToString(), true);
        }

        private void btnSaveScreen_Click(object sender, EventArgs e) {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) {
                this.Hide();
                using (Graphics g = Graphics.FromImage(bitmap)) {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                this.Show();
                bitmap.Save("E:\\nino\\My_Img.jpg", ImageFormat.Jpeg);
                
            }
        }

        private void btnElencaDiff_Click(object sender, EventArgs e) {
            if (gridSelect.DataSource == null) return;
            string diff = "";
            foreach(DataRowView r2 in ((DataView)gridSelect.DataSource)) {
                DataRow r1 = r2.Row;
                if (r1.RowState != DataRowState.Modified) continue;
                foreach (DataColumn c in r1.Table.Columns) {
                    if (r1[c.ColumnName, DataRowVersion.Original].ToString() != r1[c.ColumnName, DataRowVersion.Current].ToString()) {
                        diff += " " + c.ColumnName;
                    }
                }
                string k = QueryCreator.WHERE_KEY_CLAUSE(r1,DataRowVersion.Current,false);
                shower.Show(this, "Riga:" + k + " campi:" + diff, "Differenze");
            }
        }

	    void Enable(Control c) {
	        c.Enabled = true;
	        c.Visible = true;
	        if (c.GetType() == typeof (TextBox)) {
                ((TextBox)c).ReadOnly = false;
	        }
	        if (c.Controls != null) {
                foreach (Control cc in c.Controls) {
                    Enable(cc);
                }

            }

        }
        private void btnSbloccaTutto_Click(object sender, EventArgs e) {
            // sblocca comandi
            Meta.CanSave = true;
            Meta.CanInsert = true;
            Meta.CanInsertCopy = true;
            Meta.SearchEnabled = true;

            //Abilita tutti i 
            foreach (Control cc in controller.linkedForm.Controls) {
                Enable(cc);
            }            

            shower.Show(this, "Ricordarsi di chiudere il form dopo aver agito in modo molto prudente", "Avviso");
        }


		string checkUniqueIndex(MetaTableUniqueIndex index, DataTable t) {
			var fields = index.hash.keys;
			string me = $"{t.TableName} unique index on fields " + string.Join(",", fields);
			//Check that all rows in tables are found in the index
			int nRealRow = 0;
			foreach (DataRow r in t.Select()) {
				nRealRow++;
				var rFound = index.getRow(index.hash.get(r));
				if (rFound == null)
					return $"Some row is not found in {me}";
				if (rFound != r) return $"Wrong row found in {me}";
			}

			int nIndexedRow = 0;
			foreach (var r in index.lookup.Values) {
				if (r == null) return $"Disposed row found in {me}";
				if (r.RowState == DataRowState.Detached) return $"Detached row found  in {me}";
				if (r.RowState == DataRowState.Deleted) return $"Deleted row found  in {me}";
				nIndexedRow++;
			}

			if (nIndexedRow != nRealRow) return $"Expected {nRealRow} in index but {nIndexedRow} found in {me}";
			return null;
		}

		string checkNotUniqueIndex(MetaTableNotUniqueIndex index, DataTable t) {
			var fields = index.hash.keys;
			string me = $"{t.TableName} not unique index on fields " + string.Join(",", fields);
			//Check that all rows in tables are found in the index
			int nRealRow = 0;
			foreach (DataRow r in t.Select()) {
				nRealRow++;
				var rFound = index.getRows(index.hash.get(r));
				if (rFound == null || rFound.Length == 0) {
					return $"Some row is not found in {me}";
				}

				bool reallyFound = rFound._Filter(x => x == r)._HasRows();
				if (!reallyFound) {
					return $"Some row found in {me}  but not the searched one.";
				}
			}

			int nIndexedRow = 0;
			foreach (var rList in index.lookup.Values) {
				foreach (var r in rList) {
					if (r == null) return $"Disposed row found in {me}";
					if (r.RowState == DataRowState.Detached) return $"Detached row found  in {me}";
					if (r.RowState == DataRowState.Deleted) return $"Deleted row found  in {me}";
					nIndexedRow++;
				}
			}

			if (nIndexedRow != nRealRow) return $"Expected {nRealRow} in index but {nIndexedRow} found in {me}";
			return null;
		}

		string checkDataSetIndexes(DataSet d) {
			var idm = d.getIndexManager();
			if (idm == null) return "Index not found on Dataset in form ";

			foreach (var idx in idm.getIndexes()) {
				if (idx is MetaTableUniqueIndex) {
					var result = checkUniqueIndex((MetaTableUniqueIndex) idx, d.Tables[idx.tableName]);
					if (result != null) return result;
				}

				if (idx is MetaTableNotUniqueIndex) {
					var result = checkNotUniqueIndex((MetaTableNotUniqueIndex) idx, d.Tables[idx.tableName]);
					if (result != null) return result;
				}

			}

			return null;
		}


		private void btnCheckIndexes_Click(object sender, EventArgs e) {
			txtOutput.Text = checkDataSetIndexes(DS)??"DataSet OK";
		}

		

		private void gridSelect_Scroll(object sender, ScrollEventArgs e) {
			if (gridSelect.FirstDisplayedScrollingColumnIndex != gridoriginal.FirstDisplayedScrollingColumnIndex) {
				//gridSelect.CurrentCell = gridSelect.Cells[gridSelect.CurrentRowIndex, gridSelect.FirstVisibleColumn];
				try {
					gridoriginal.FirstDisplayedScrollingColumnIndex = gridSelect.FirstDisplayedScrollingColumnIndex;
				}
				catch {
				}


			}
		}

		private void gridoriginal_Scroll(object sender, ScrollEventArgs e) {
			if (gridSelect.FirstDisplayedScrollingColumnIndex != gridoriginal.FirstDisplayedScrollingColumnIndex) {
				//gridSelect.CurrentCell = gridSelect.Cells[gridSelect.CurrentRowIndex, gridSelect.FirstVisibleColumn];
				try {
					gridSelect.FirstDisplayedScrollingColumnIndex = gridoriginal.FirstDisplayedScrollingColumnIndex;
				}
				catch {
				}

				


			}
		}

		private void gridSelect_DataError(object sender, DataGridViewDataErrorEventArgs e) {
			e.ThrowException = false;
		}

		private void gridoriginal_DataError(object sender, DataGridViewDataErrorEventArgs e) {
			e.ThrowException = false;
		}
	}

}
