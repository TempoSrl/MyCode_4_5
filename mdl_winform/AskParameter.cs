using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using mdl;

namespace mdl_winform
{
	/// <summary>
	/// Descrizione di riepilogo per AskParameter.
	/// </summary>
	public class AskParameter : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtCampo;
		private System.Windows.Forms.Label lbOperatore;
		private System.Windows.Forms.TextBox txtOperatore;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtValue;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		int n_ops;
		/// <summary>
		/// string read in TextBox
		/// </summary>
		public string val;
		/// <summary>
		/// Variabile di progettazione necessaria.
		/// </summary>
		private System.ComponentModel.Container components = null;

	    private IMessageShower shower;
		/// <summary>
		/// Constructor of askParameter Form
		/// </summary>
		/// <param name="campo">name of field to ask</param>
		/// <param name="operatore">name of operator (es. >= )</param>
		/// <param name="n_ops">n. of operands to ask</param>
		public AskParameter(string campo, string operatore, int n_ops)
		{
			//
			// Necessario per il supporto di Progettazione Windows Form
			//
			InitializeComponent();
		    shower = MetaFactory.factory.getSingleton<IMessageShower>();
			txtCampo.Text= campo;
			txtOperatore.Text= operatore;
			this.n_ops  = n_ops;

		}

		/// <summary>
		/// Pulire le risorse in uso.
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
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.txtCampo = new System.Windows.Forms.TextBox();
			this.lbOperatore = new System.Windows.Forms.Label();
			this.txtOperatore = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Campo:";
			// 
			// txtCampo
			// 
			this.txtCampo.Location = new System.Drawing.Point(8, 32);
			this.txtCampo.Name = "txtCampo";
			this.txtCampo.ReadOnly = true;
			this.txtCampo.Size = new System.Drawing.Size(168, 20);
			this.txtCampo.TabIndex = 1;
			this.txtCampo.TabStop = false;
			this.txtCampo.Text = "";
			// 
			// lbOperatore
			// 
			this.lbOperatore.Location = new System.Drawing.Point(192, 16);
			this.lbOperatore.Name = "lbOperatore";
			this.lbOperatore.Size = new System.Drawing.Size(96, 16);
			this.lbOperatore.TabIndex = 2;
			this.lbOperatore.Text = "Operatore:";
			// 
			// txtOperatore
			// 
			this.txtOperatore.Location = new System.Drawing.Point(192, 32);
			this.txtOperatore.Name = "txtOperatore";
			this.txtOperatore.ReadOnly = true;
			this.txtOperatore.Size = new System.Drawing.Size(184, 20);
			this.txtOperatore.TabIndex = 3;
			this.txtOperatore.TabStop = false;
			this.txtOperatore.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Valore:";
			// 
			// txtValue
			// 
			this.txtValue.Location = new System.Drawing.Point(8, 88);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(368, 20);
			this.txtValue.TabIndex = 1;
			this.txtValue.Text = "";
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(192, 128);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "Ok";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(296, 128);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			// 
			// AskParameter
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(392, 165);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.txtValue);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtOperatore);
			this.Controls.Add(this.lbOperatore);
			this.Controls.Add(this.txtCampo);
			this.Controls.Add(this.label1);
			this.Name = "AskParameter";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Inserimento parametri (separati da \';\')";
			this.ResumeLayout(false);

		}
		#endregion

	
		private void btnOk_Click(object sender, System.EventArgs e) {
			val = txtValue.Text;
			if (n_ops!=-1){
				string [] result = val.Split(new char[]{';'}, n_ops);
				if (result.Length != n_ops) {
				    shower.Show(this,"L'operatore selezionato richiede "+
						n_ops.ToString()+" parametri, ma ne sono stati specificati "+
						result.Length+".","Errore");
					return;
				}
			}
			else{
				string []result = val.Split(new char[]{';'});
				if (result.Length==0) {
				    shower.Show(this,"Non è stato specificato alcun parametro",
						"Errore");
					return;
				}
			}		
			DialogResult = System.Windows.Forms.DialogResult.OK;	
			return;
		}
	}
}
