using System;
using System.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
#pragma warning disable 1591
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace mdl {
[Serializable,DesignerCategory("code"),System.Xml.Serialization.XmlSchemaProvider("GetTypedDataSetSchema")]
[System.Xml.Serialization.XmlRoot("dbstructure"),System.ComponentModel.Design.HelpKeyword("vs.data.DataSet")]
public partial class dbstructure: DataSet {

	#region Table members declaration
	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customobject 		=> Tables["customobject"];

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customview 		=> Tables["customview"];

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customviewcolumn 		=> Tables["customviewcolumn"];

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customvieworderby 		=> Tables["customvieworderby"];

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customviewwhere 		=> Tables["customviewwhere"];

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customtablestructure 		=> Tables["customtablestructure"];

	///<summary>
	///Serve a redirigere un listtype di una tabella ad un listtype di un'altra (tipicamente una vista)
	///</summary>
	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customredirect 		=> Tables["customredirect"];

	///<summary>
	///Descrittore form
	///</summary>
	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable customedit 		=> Tables["customedit"];

	///<summary>
	///Descrizione 
	///</summary>
	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable viewcolumn 		=> Tables["viewcolumn"];

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)]
	public DataTable columntypes 		=> Tables["columntypes"];

	#endregion


	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new DataTableCollection Tables => base.Tables;

	[DebuggerNonUserCode,DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
// ReSharper disable once MemberCanBePrivate.Global
	public new DataRelationCollection Relations => base.Relations;

[DebuggerNonUserCode]
public dbstructure(){
	BeginInit();
	initClass();
	EndInit();
}
[DebuggerNonUserCode]
protected dbstructure (SerializationInfo info,StreamingContext ctx):base(info,ctx) {}
[DebuggerNonUserCode]
private void initClass() {
	DataSetName = "dbstructure";
	Prefix = "";
	Namespace = "http://tempuri.org/dbstructure.xsd";

	#region create DataTables
	DataColumn C;
	//////////////////// CUSTOMOBJECT /////////////////////////////////
	var tcustomobject= new DataTable("customobject");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomobject.Columns.Add(C);
	tcustomobject.Columns.Add( new DataColumn("description", typeof(string)));
	tcustomobject.Columns.Add( new DataColumn("isreal", typeof(string)));
	tcustomobject.Columns.Add( new DataColumn("realtable", typeof(string)));
	tcustomobject.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomobject.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomobject);
	tcustomobject.PrimaryKey =  new DataColumn[]{tcustomobject.Columns["objectname"]};


	//////////////////// CUSTOMVIEW /////////////////////////////////
	var tcustomview= new DataTable("customview");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomview.Columns.Add(C);
	C= new DataColumn("viewname", typeof(string));
	C.AllowDBNull=false;
	tcustomview.Columns.Add(C);
	tcustomview.Columns.Add( new DataColumn("header", typeof(string)));
	tcustomview.Columns.Add( new DataColumn("footer", typeof(string)));
	tcustomview.Columns.Add( new DataColumn("topmargin", typeof(double)));
	tcustomview.Columns.Add( new DataColumn("bottommargin", typeof(double)));
	tcustomview.Columns.Add( new DataColumn("rightmargin", typeof(double)));
	tcustomview.Columns.Add( new DataColumn("leftmargin", typeof(double)));
	tcustomview.Columns.Add( new DataColumn("lefttoright", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("hcenter", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("vcenter", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("gridlines", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("rowheading", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("colheading", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("landscape", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("scale", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("fittopage", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("vpages", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("hpages", typeof(short)));
	tcustomview.Columns.Add( new DataColumn("isreal", typeof(string)));
	tcustomview.Columns.Add( new DataColumn("issystem", typeof(string)));
	tcustomview.Columns.Add( new DataColumn("staticfilter", typeof(string)));
	tcustomview.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomview.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomview);
	tcustomview.PrimaryKey =  new DataColumn[]{tcustomview.Columns["objectname"], tcustomview.Columns["viewname"]};


	//////////////////// CUSTOMVIEWCOLUMN /////////////////////////////////
	var tcustomviewcolumn= new DataTable("customviewcolumn");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomviewcolumn.Columns.Add(C);
	C= new DataColumn("viewname", typeof(string));
	C.AllowDBNull=false;
	tcustomviewcolumn.Columns.Add(C);
	C= new DataColumn("colnumber", typeof(short));
	C.AllowDBNull=false;
	tcustomviewcolumn.Columns.Add(C);
	tcustomviewcolumn.Columns.Add( new DataColumn("heading", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("colwidth", typeof(int)));
	tcustomviewcolumn.Columns.Add( new DataColumn("visible", typeof(short)));
	tcustomviewcolumn.Columns.Add( new DataColumn("fontname", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("fontsize", typeof(short)));
	tcustomviewcolumn.Columns.Add( new DataColumn("bold", typeof(short)));
	tcustomviewcolumn.Columns.Add( new DataColumn("italic", typeof(short)));
	tcustomviewcolumn.Columns.Add( new DataColumn("underline", typeof(short)));
	tcustomviewcolumn.Columns.Add( new DataColumn("strikeout", typeof(short)));
	tcustomviewcolumn.Columns.Add( new DataColumn("color", typeof(int)));
	tcustomviewcolumn.Columns.Add( new DataColumn("format", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("isreal", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("expression", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("colname", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("systemtype", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomviewcolumn.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	tcustomviewcolumn.Columns.Add( new DataColumn("listcolpos", typeof(int)));
	Tables.Add(tcustomviewcolumn);
	tcustomviewcolumn.PrimaryKey =  new DataColumn[]{tcustomviewcolumn.Columns["objectname"], tcustomviewcolumn.Columns["viewname"], tcustomviewcolumn.Columns["colnumber"]};


	//////////////////// CUSTOMVIEWORDERBY /////////////////////////////////
	var tcustomvieworderby= new DataTable("customvieworderby");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomvieworderby.Columns.Add(C);
	C= new DataColumn("viewname", typeof(string));
	C.AllowDBNull=false;
	tcustomvieworderby.Columns.Add(C);
	C= new DataColumn("periodnumber", typeof(short));
	C.AllowDBNull=false;
	tcustomvieworderby.Columns.Add(C);
	tcustomvieworderby.Columns.Add( new DataColumn("columnname", typeof(string)));
	tcustomvieworderby.Columns.Add( new DataColumn("direction", typeof(int)));
	tcustomvieworderby.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomvieworderby.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomvieworderby);
	tcustomvieworderby.PrimaryKey =  new DataColumn[]{tcustomvieworderby.Columns["objectname"], tcustomvieworderby.Columns["viewname"], tcustomvieworderby.Columns["periodnumber"]};


	//////////////////// CUSTOMVIEWWHERE /////////////////////////////////
	var tcustomviewwhere= new DataTable("customviewwhere");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomviewwhere.Columns.Add(C);
	C= new DataColumn("viewname", typeof(string));
	C.AllowDBNull=false;
	tcustomviewwhere.Columns.Add(C);
	C= new DataColumn("periodnumber", typeof(short));
	C.AllowDBNull=false;
	tcustomviewwhere.Columns.Add(C);
	tcustomviewwhere.Columns.Add( new DataColumn("connector", typeof(int)));
	tcustomviewwhere.Columns.Add( new DataColumn("columnname", typeof(string)));
	tcustomviewwhere.Columns.Add( new DataColumn("operator", typeof(int)));
	tcustomviewwhere.Columns.Add( new DataColumn("value", typeof(string)));
	tcustomviewwhere.Columns.Add( new DataColumn("runtime", typeof(int)));
	tcustomviewwhere.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomviewwhere.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomviewwhere);
	tcustomviewwhere.PrimaryKey =  new DataColumn[]{tcustomviewwhere.Columns["objectname"], tcustomviewwhere.Columns["viewname"], tcustomviewwhere.Columns["periodnumber"]};


	//////////////////// CUSTOMTABLESTRUCTURE /////////////////////////////////
	var tcustomtablestructure= new DataTable("customtablestructure");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomtablestructure.Columns.Add(C);
	C= new DataColumn("colname", typeof(string));
	C.AllowDBNull=false;
	tcustomtablestructure.Columns.Add(C);
	C= new DataColumn("autoincrement", typeof(string));
	C.AllowDBNull=false;
	tcustomtablestructure.Columns.Add(C);
	tcustomtablestructure.Columns.Add( new DataColumn("step", typeof(int)));
	tcustomtablestructure.Columns.Add( new DataColumn("prefixfieldname", typeof(string)));
	tcustomtablestructure.Columns.Add( new DataColumn("middleconst", typeof(string)));
	C= new DataColumn("length", typeof(int));
	C.AllowDBNull=false;
	tcustomtablestructure.Columns.Add(C);
	C= new DataColumn("linear", typeof(string));
	C.AllowDBNull=false;
	tcustomtablestructure.Columns.Add(C);
	C= new DataColumn("selector", typeof(string));
	C.AllowDBNull=false;
	tcustomtablestructure.Columns.Add(C);
	tcustomtablestructure.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomtablestructure.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomtablestructure);
	tcustomtablestructure.PrimaryKey =  new DataColumn[]{tcustomtablestructure.Columns["objectname"], tcustomtablestructure.Columns["colname"]};


	//////////////////// CUSTOMREDIRECT /////////////////////////////////
	var tcustomredirect= new DataTable("customredirect");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomredirect.Columns.Add(C);
	C= new DataColumn("viewname", typeof(string));
	C.AllowDBNull=false;
	tcustomredirect.Columns.Add(C);
	C= new DataColumn("objecttarget", typeof(string));
	C.AllowDBNull=false;
	tcustomredirect.Columns.Add(C);
	C= new DataColumn("viewtarget", typeof(string));
	C.AllowDBNull=false;
	tcustomredirect.Columns.Add(C);
	tcustomredirect.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomredirect.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomredirect);
	tcustomredirect.PrimaryKey =  new DataColumn[]{tcustomredirect.Columns["objectname"], tcustomredirect.Columns["viewname"]};


	//////////////////// CUSTOMEDIT /////////////////////////////////
	var tcustomedit= new DataTable("customedit");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	C= new DataColumn("edittype", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	C= new DataColumn("dllname", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	C= new DataColumn("caption", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	C= new DataColumn("list", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	C= new DataColumn("startempty", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	C= new DataColumn("tree", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	tcustomedit.Columns.Add( new DataColumn("defaultlisttype", typeof(string)));
	C= new DataColumn("searchenabled", typeof(string));
	C.AllowDBNull=false;
	tcustomedit.Columns.Add(C);
	tcustomedit.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcustomedit.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tcustomedit);
	tcustomedit.PrimaryKey =  new DataColumn[]{tcustomedit.Columns["objectname"], tcustomedit.Columns["edittype"]};


	//////////////////// VIEWCOLUMN /////////////////////////////////
	var tviewcolumn= new DataTable("viewcolumn");
	C= new DataColumn("objectname", typeof(string));
	C.AllowDBNull=false;
	tviewcolumn.Columns.Add(C);
	C= new DataColumn("colname", typeof(string));
	C.AllowDBNull=false;
	tviewcolumn.Columns.Add(C);
	C= new DataColumn("realtable", typeof(string));
	C.AllowDBNull=false;
	tviewcolumn.Columns.Add(C);
	C= new DataColumn("realcolumn", typeof(string));
	C.AllowDBNull=false;
	tviewcolumn.Columns.Add(C);
	tviewcolumn.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tviewcolumn.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	Tables.Add(tviewcolumn);
	tviewcolumn.PrimaryKey =  new DataColumn[]{tviewcolumn.Columns["objectname"], tviewcolumn.Columns["colname"]};


	//////////////////// COLUMNTYPES /////////////////////////////////
	var tcolumntypes= new DataTable("columntypes");
	C= new DataColumn("tablename", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	C= new DataColumn("field", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	C= new DataColumn("iskey", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	C= new DataColumn("sqltype", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	tcolumntypes.Columns.Add( new DataColumn("col_len", typeof(int)));
	tcolumntypes.Columns.Add( new DataColumn("col_precision", typeof(int)));
	tcolumntypes.Columns.Add( new DataColumn("col_scale", typeof(int)));
	tcolumntypes.Columns.Add( new DataColumn("systemtype", typeof(string)));
	C= new DataColumn("sqldeclaration", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	C= new DataColumn("allownull", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	tcolumntypes.Columns.Add( new DataColumn("defaultvalue", typeof(string)));
	tcolumntypes.Columns.Add( new DataColumn("format", typeof(string)));
	C= new DataColumn("denynull", typeof(string));
	C.AllowDBNull=false;
	tcolumntypes.Columns.Add(C);
	tcolumntypes.Columns.Add( new DataColumn("lastmodtimestamp", typeof(DateTime)));
	tcolumntypes.Columns.Add( new DataColumn("lastmoduser", typeof(string)));
	tcolumntypes.Columns.Add( new DataColumn("createuser", typeof(string)));
	tcolumntypes.Columns.Add( new DataColumn("createtimestamp", typeof(DateTime)));
	Tables.Add(tcolumntypes);
	tcolumntypes.PrimaryKey =  new DataColumn[]{tcolumntypes.Columns["tablename"], tcolumntypes.Columns["field"]};


	#endregion


	#region DataRelation creation
	var cPar = new []{customobject.Columns["objectname"]};
	var cChild = new []{columntypes.Columns["tablename"]};
	Relations.Add(new DataRelation("customobjectcolumntypes",cPar,cChild,false));

	cPar = new []{customobject.Columns["objectname"]};
	cChild = new []{viewcolumn.Columns["objectname"]};
	Relations.Add(new DataRelation("customobjectviewcolumn",cPar,cChild,false));

	cPar = new []{customobject.Columns["objectname"]};
	cChild = new []{customedit.Columns["objectname"]};
	Relations.Add(new DataRelation("customobjectcustomedit",cPar,cChild,false));

	cPar = new []{customobject.Columns["objectname"]};
	cChild = new []{customredirect.Columns["objectname"]};
	Relations.Add(new DataRelation("customobjectcustomredirect",cPar,cChild,false));

	cPar = new []{customobject.Columns["objectname"]};
	cChild = new []{customtablestructure.Columns["objectname"]};
	Relations.Add(new DataRelation("customobjectcustomtablestructure",cPar,cChild,false));

	cPar = new []{customview.Columns["objectname"], customview.Columns["viewname"]};
	cChild = new []{customviewwhere.Columns["objectname"], customviewwhere.Columns["viewname"]};
	Relations.Add(new DataRelation("customviewcustomviewwhere",cPar,cChild,false));

	cPar = new []{customview.Columns["objectname"], customview.Columns["viewname"]};
	cChild = new []{customvieworderby.Columns["objectname"], customvieworderby.Columns["viewname"]};
	Relations.Add(new DataRelation("customviewcustomvieworderby",cPar,cChild,false));

	cPar = new []{customview.Columns["objectname"], customview.Columns["viewname"]};
	cChild = new []{customviewcolumn.Columns["objectname"], customviewcolumn.Columns["viewname"]};
	Relations.Add(new DataRelation("customviewcustomviewcolumn",cPar,cChild,false));

	cPar = new []{customobject.Columns["objectname"]};
	cChild = new []{customview.Columns["objectname"]};
	Relations.Add(new DataRelation("customobjectcustomview",cPar,cChild,false));

	#endregion

}
}
}
