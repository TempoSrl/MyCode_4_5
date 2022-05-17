using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;
#pragma warning disable IDE1006 // Naming Styles


namespace mdl_winform
{

	/// <summary>
	/// Helper class used to calc best size for column
	/// </summary>
    public class MyGridColumn: DataGridTextBoxColumn {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="C"></param>
		/// <param name="format"></param>
		public MyGridColumn(System.ComponentModel.PropertyDescriptor C,
			string format):base(C,format){
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public MyGridColumn():base(){
		}

		//private int SelectedRow = -1;
//		protected override void Edit(System.Windows.Forms.CurrencyManager
//			source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string
//			instantText, bool cellIsVisible) {
//			//make sure the selectrow is valid before trying to unselect
////			if(SelectedRow > -1 && SelectedRow < source.List.Count + 1)
////				this.DataGridTableStyle.DataGrid.UnSelect(SelectedRow);
//			SelectedRow = rowNum;
//			DataGrid G = this.DataGridTableStyle.DataGrid;			
//			//base.Edit(source,rowNum,bounds,readOnly, instantText, cellIsVisible);
//			G.Select(SelectedRow);
//			//this.DataGridTableStyle.DataGrid.Invalidate();
//			//this.DataGridTableStyle.DataGrid.Update();
//			
//
//		}

		/// <summary>
		/// Gets Best size of column to display a string value
		/// </summary>
		/// <param name="g"></param>
		/// <param name="thisString"></param>
		/// <returns></returns>
        public Size GetPrefSize(Graphics g, string thisString) {
            if (thisString!=null){
                int pos1 = thisString.IndexOf("\r");
                int pos2 = thisString.IndexOf("\n");
                if (pos1 < 0) {
                    pos1 = pos2;
                    pos2 = -1;
                }
                if (pos1 > 0 && pos2 > 0 && pos2 < pos1) pos1 = pos2;
                if (pos1 > 150 || (pos1 < 0 && thisString.Length > 150)) pos1 = 150;
                if (pos1 > 0) thisString = thisString.Substring(0, pos1);
            }
            return this.GetPreferredSize(g,thisString);
        }
    }

    /// <summary>
    /// Classe per la formattazione delle colonne di un DataGrid per la corretta
    /// visualizzazione del loro contenuto.
    /// E' necessario implementare nel form chiamante codice simile al seguente:
    /// DataGridTableStyle MyTableStyle = new DataGridTableStyle();
    /// MyTableStyle.MappingName = TableStyle;
    /// DataGridColumnStyle C1 = new MyGridColumn(); \r
    /// MyTableStyle.GridColumnStyles.Add(C1); \r
    /// dataGrid1.TableStyles.Add(MyTableStyle);
    /// </summary>
    public class formatgrids {
        DataGrid MyDataGrid;

		/// <summary>
		/// Creates a grid formatter linked to a given DataGrid
		/// </summary>
		/// <param name="MyDataGrid"></param>
		public formatgrids(DataGrid MyDataGrid) {
			this.MyDataGrid = MyDataGrid;
			
		}


        /// <summary>
        /// 
        /// </summary>
        public void AutosizeColumnWidth() {
            if (MyDataGrid==null) return;
			if (MyDataGrid.DataSource==null) return;
			if (MyDataGrid.DataMember==null) return;
            var MyTable = ((DataSet)MyDataGrid.DataSource).Tables[MyDataGrid.DataMember];	
			if (MyTable==null) return;
			MyDataGrid.ResumeLayout(true);
			
			DataRow []First = MyTable.Select(null,null,DataViewRowState.CurrentRows);
			int MyRowCount = First.Length;

			DataGridTableStyle MyGT=null;
			foreach(DataGridTableStyle DGT in MyDataGrid.TableStyles){
				if (DGT.MappingName== MyTable.TableName){
					MyGT=DGT;
					break;
				}
			}

			if (MyGT==null) return;


			var Styles = MyGT.GridColumnStyles;
            int MyColumnCount = Styles.Count;	//Legge il numero di colonne dal DataGrid
            //int[] MaxColumnSize = new int[MyColumnCount];	//Array delle larghezze massime delle colonne
            

            var g = MyDataGrid.CreateGraphics(); //Crea l'oggetto grafico che serve al metodo GetPrefSize
            for(int Co = 0;Co < MyColumnCount;Co++) {	//per ogni colonna del DataGrid                
                if(!(Styles[Co] is MyGridColumn myGridColumn)) continue;

                string header = Styles[Co].HeaderText;
                if (header.StartsWith(".")||header==" "){
                    Styles[Co].Width = 0;
                    continue;
                }
				string colname=myGridColumn.MappingName;
				var CurrCol= MyTable.Columns[colname];
//				DataRow []rows = MyTable.Select("",
//                           "LEN(Convert("+colname+",'System.String'))",
//					DataViewRowState.CurrentRows);
//				int countrows= MyRowCount; //rows.Length;
//				if (countrows>500) countrows= 500;
//                
				var MySizeF = (SizeF)myGridColumn.GetPrefSize(g, header );
                int MaxColumnSize = Convert.ToInt32(MySizeF.Width);
				if (CurrCol.DataType==typeof(decimal)){
					string S= mdl_utils.HelpUi.StringValue(12345678.12,"x.y");
					MySizeF = (SizeF)myGridColumn.GetPrefSize(g,S);
					int Result2 = Convert.ToInt32(MySizeF.Width);	//larghezza della cella corrente
					if(Result2 > MaxColumnSize) MaxColumnSize = Result2;
				}
				string tagtable = MyTable.TableName+"."+colname;
				int NR=0;
                foreach(var FF in First) {	//MyRowCount
					NR++;
					if (NR>100) break;
					var O = FF[colname];
					//Object O = rows[Ro][colname];
					if (O==null) continue;
					if (O.ToString()=="") continue;
					string S= mdl_utils.HelpUi.StringValue(O,tagtable,CurrCol);
					
                    MySizeF = (SizeF)myGridColumn.GetPrefSize(g,S);
                    var Result = Convert.ToInt32(MySizeF.Width);	//larghezza della cella corrente
                    if(Result > MaxColumnSize) MaxColumnSize = Result;
                }
				if (CurrCol.DataType==typeof(string)){
					string S= mdl_utils.HelpUi.StringValue("123456789012345678901234567890","x.y");
					MySizeF = (SizeF)myGridColumn.GetPrefSize(g,S);
					int Result2 = Convert.ToInt32(MySizeF.Width);	//larghezza della cella corrente
					if(MaxColumnSize > Result2) MaxColumnSize = Result2;
				}
				
                Styles[Co].Width = MaxColumnSize; //[Co]; //Imposta la larghezza della colonna
            }
            g.Dispose();
            //MyDataGrid.ResumeLayout();
//			MyDataGrid.Update();
        }

		/// <summary>
		/// Set the displaying of null values as empty strings
		/// </summary>
        public void SuppressNulls() {
			if (MyDataGrid==null) return;
			if (MyDataGrid.DataSource==null) return;  
			if (MyDataGrid.DataMember==null) return;
			var MyTable = ((DataSet)MyDataGrid.DataSource).Tables[MyDataGrid.DataMember];
			if (MyTable==null) return;

			var Styles = MyDataGrid.TableStyles[0].GridColumnStyles;
			if (Styles==null) return;
			if (Styles.Count==0) return;
            int MyColumnCount = Styles.Count;	//Legge il numero di colonne dal DataGrid
            //MyDataGrid.SuspendLayout();
            for(int Co = 0;Co < MyColumnCount;Co++) {	//per ogni colonna del DataGrid
                var myGridColumn = (MyGridColumn)Styles[Co];
                myGridColumn.NullText="";
            }
            //MyDataGrid.ResumeLayout();
        }



    }


}
