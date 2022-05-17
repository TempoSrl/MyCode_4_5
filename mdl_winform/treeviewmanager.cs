using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using mdl;

namespace mdl_winform
{
	/// <summary>
	/// Base (empty) class able to create a tree_node, given Parent DataRow and
	///  a new Child DataRow
	/// </summary>
    public  class node_dispatcher {
		/// <summary>
		/// Creates a new tree node linking it to a Child Row
		/// </summary>
		/// <param name="Parent">Parent Row (or null if not present)</param>
		/// <param name="Child">new row to add to tree</param>
		/// <returns>new tree_node linked to Child Row</returns>
        virtual public tree_node GetNode(DataRow Parent, DataRow Child){
            return null;
        }
    }

	/// <summary>
	/// Class used to manage nodes of a treeview. Every TreeNode of the tree
	///  has a tag storing the corresponding tree_node.
	/// </summary>
	public class tree_node {
		/// <summary>
		/// DataRow associated to the tree_node
		/// </summary>
		public DataRow Row;
		//public int level;

		/// <summary>
		/// Creates the tree_node linking it to a DataRow
		/// </summary>
		/// <param name="R"></param>
        public tree_node(DataRow R){
            //this.level= level;
            this.Row=R;
		}

        /// <summary>
        /// Label that appears in treeview for each node
        /// </summary>
        /// <returns></returns>
        virtual public string Text(){
            return "";
        }

        /// <summary>
        /// String that should appear in tooltip
        /// </summary>
        /// <returns></returns>
        virtual public string ToolTip(){
            return "";
        }

        /// <summary>
        /// Returns true if node can be selected
        /// </summary>
        /// <returns></returns>
        virtual public bool CanSelect(){
            return true;
        }

        /// <summary>
        /// Returns true if node can be selected
        /// </summary>
        /// <returns></returns>
        virtual public string UnselectableMessage() {
            return "La riga selezionata non è operativa";
        }

        /// <summary>
        /// Get index in DataTable of tree_node linked row 
        /// </summary>
        /// <returns></returns>
        public int RowIndex(){
            DataTable T = Row.Table;
            for(int i=0; i< T.Rows.Count; i++){
                if (T.Rows[i].Equals(Row)) return i;
            }
            return -1;            
        }


        ///// <summary>
        ///// Gets the index of the row linked to a TreeNode.
        ///// </summary>
        ///// <param name="N"></param>
        ///// <returns></returns>
        //public static int RowIndex(TreeNode N){
        //    tree_node TN = (tree_node) N.Tag;
        //    return TN.RowIndex();
        //}


	}



    public interface ITreeViewManager {
        /// <summary>
        /// Gets a relation that connects a table with its self
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        DataRelation autoParentRelation(DataTable T);

        /// <summary>
        /// Gets a relation that connects a table with its self. Should be the same
        ///  as AutoParent
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        DataRelation autoChildRelation(DataTable T);

        /// <summary>
        /// Security manager
        /// </summary>
        ISecurity security { get; set; }

        /// <summary>
        /// Gives SQL condition to get roots rows from tree_table
        /// </summary>
        /// <returns></returns>
        string RootsCondition_C();

        /// <summary>
        /// Sql condition to get roots from DB
        /// </summary>
        /// <returns></returns>
        string RootsCondition_SQL();

        /// <summary>
        /// Fills the treeview with the nodes taken from all tree_table rows
        /// Selects no node.
        /// </summary>
        void FillNodes();

        /// <summary>
        /// Creates a new tree_node linked to a given DataRow, assuming 
        ///  a given TreeNode as parent
        /// </summary>
        /// <param name="Parent">Parent TreeNode</param>
        /// <param name="R">DataRow linked to Node to create</param>
        /// <returns></returns>
        tree_node GetNodeFromRow(TreeNode Parent, DataRow R);

        /// <summary>
        /// Gets the TreeNode linked to Child, assuming the parent node is
        ///  TreeNode. If the node does not exists, it is created. 
        /// </summary>
        /// <param name="Parent">Parent node of searched one</param>
        /// <param name="Child">DataRow linked to searched TreeNode</param>
        /// <returns>TreeNode linked to Child</returns>
        TreeNode AddRow(TreeNode Parent, DataRow Child);

        /// <summary>
        /// Reads child of a nodes to select. Used when a navigator is linked to tree.
        /// Ignored if FixedData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeview_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e);

        /// <summary>
        /// Adds dummy nodes to childs of expanded node.
        /// Does nothing if FixedData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeview_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e);

        /// <summary>
        /// Do necessary operation to handle AfterCollapse event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void treeview_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e);

        /// <summary>
        /// Deletes current node from tree (and all childs recursively)
        /// </summary>
        void DeleteCurrentNode();

        /// <summary>
        /// Selects the TreeNode corresponding to a given DataRow
        /// </summary>
        /// <param name="R"></param>
        void SelectNode(DataRow R);

        /// <summary>
        /// Returns added node, or previous if it was already present.
        /// </summary>
        /// <param name="Nodes"></param>
        /// <param name="NewNode"></param>
        /// <returns></returns>
        TreeNode AddNode(TreeNodeCollection Nodes, TreeNode NewNode);

        /// <summary>
        /// Get the row linked to currently selected TreeNode
        /// </summary>
        /// <returns></returns>
        DataRow SelectedRow();

        /// <summary>
        /// Gets the index of Row linked to Currenty selected TreeNode
        /// </summary>
        /// <returns></returns>
        int SelectedRowIndex();


        

        void calcTreeViewDataAccess(IGetData getData, QueryHelper q);


        void Start(string rootfilterSql, bool clear);

        DataRow selectRow(DataRow R, string ListType);

    }

    /// <summary>
	/// TreeView Manager
	/// </summary>
	public class TreeViewManager : IDisposable, ITreeViewManager {
		/// <summary>
		/// DataTable Linked to the TreeView
		/// </summary>
		protected DataTable TreeTable;

		//DataTable levels_table;

		/// <summary>
		/// TreeView linked to this TreeViewManager
		/// </summary>
		public TreeView  tree;

        /// <summary>
        /// When true, a double click on the tree closes the page. 
        /// </summary>
		public bool DoubleClickForSelect;

		DataRelation AutoChildRelation;

		/// <summary>
		/// GetData used for accessing to DB
		/// </summary>
		//[Obsolete]
        //public GetData getd;

	    private ISecurity _security;

        /// <summary>
        /// Security manager
        /// </summary>
	    public ISecurity security {
            get { return _security; }
            set { _security = value; }
	    }

        readonly node_dispatcher dispatcher;
        readonly string _rootConditionC;
        readonly string _rootConditionSql;

		/// <summary>
		/// Enables/Disables automatic events for the treeview
		/// </summary>
		public bool AutoEventsEnabled;

        readonly ToolTip Tip;

		/// <summary>
		/// DataGrid used as Navigator for the treeview
		/// </summary>
		public DataGrid Navigator;

		/// <summary>
		/// when true data are never deleted/re-read. They depends only on Tree-Table content
		/// Default is false
		/// </summary>
		public bool FixedData;

		
	    private mdl.ITreeViewDataAccess _treeDataAccess;

        /// <summary>
        /// Initializes the connection to db
        /// </summary>
        /// <param name="getData"></param>
        /// <param name="q"></param>
	    public virtual void calcTreeViewDataAccess(IGetData getData, QueryHelper q) {
	        _treeDataAccess = new mdl.TreeViewDataAccess(getData, q);
	    }
        /// <summary>
        /// Creates a manager for a tree-view
        /// </summary>
        /// <param name="treeTable">DataTable containing the tree structure</param>
        /// <param name="tree">TreeView to manage</param>
        /// <param name="dispatcher">node dispatcher</param>
        /// <param name="rootConditionC">condition that identifies roots</param>
        /// <param name="rootConditionSql">condition that identifies roots (for db)</param>
        public TreeViewManager(
			DataTable treeTable,
			TreeView tree,
			node_dispatcher dispatcher,
			string rootConditionC,
            string rootConditionSql
			) {
			DoubleClickForSelect=true;
			this.TreeTable= treeTable;
			this.tree = tree;
			this.dispatcher= dispatcher;
			this._rootConditionC = rootConditionC;
            this._rootConditionSql = rootConditionSql;
           
            this.AutoChildRelation = autoChildRelation(treeTable);
			tree.AfterCollapse+= this.treeview_AfterCollapse;
			tree.AfterExpand+= this.treeview_AfterExpand;
			tree.BeforeExpand+= this.treeview_BeforeExpand;
			tree.BeforeSelect += this.treeview_BeforeSelect;
			setManager(treeTable);				
			var sortnodes = treeTable.getSorting();
			if (sortnodes==null) tree.Sorted=true;
			//Form F = tree.FindForm();
			Tip = new ToolTip();
			//            Tip.AutomaticDelay = 600;
			//            Tip.AutoPopDelay = 30000;
			//            Tip.InitialDelay= 200;
			//            Tip.ReshowDelay= 600;
			tree.MouseMove+= new MouseEventHandler(this.treeview_MouseMove);
			AutoEventsEnabled=true;
			Navigator = (DataGrid) treeTable.ExtendedProperties["MetaDataTreeNavigator"];
			FixedData=false;
		}
        

		private void treeview_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (!AutoEventsEnabled) return;
			var n = tree.GetNodeAt(e.X,e.Y);
		    var tn = (tree_node) n?.Tag;
			if (tn == null) return;
			var s = tn.ToolTip();
	

			string old = Tip.GetToolTip(tree);
			if (old==s)return;
			Tip.SetToolTip(tree, s);
			Tip.Active=true;		
		}

		/// <summary>
		/// Gives SQL condition to get roots rows from tree_table
		/// </summary>
		/// <returns></returns>
		public virtual string RootsCondition_C(){
			return _rootConditionC;
		}
        /// <summary>
        /// Sql condition to get roots from DB
        /// </summary>
        /// <returns></returns>
        public virtual string RootsCondition_SQL() {
            return _rootConditionSql;
        }

		/// <summary>
		/// Gets the root condition of the treeviewmanager linked to a DataTable
		/// </summary>
		/// <param name="T"></param>
		/// <returns></returns>
		public static string RootsCondition_C(DataTable T){
			var m = GetManager(T);
		    return m?.RootsCondition_C();
		}

        /// <summary>
        /// Get the sql root condition from the treemanager attached to a table
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static string RootsCondition_SQL(DataTable T) {
            var m = GetManager(T);
            return m?.RootsCondition_SQL();
        }

		/// <summary>
		/// Fills the treeview with the nodes taken from tree_table rows
		/// Selects no nodes.
		/// </summary>
		public static void FillNodes(DataTable T){
			var m = GetManager(T);
		    m?.FillNodes();
		}

        private TreeNode createNewNode(DataRow parentRow, DataRow childRow){
			var  t = dispatcher.GetNode(parentRow, childRow);
		    var n = new TreeNode(t.Text()) {Tag = t};
		    return n;
		}

		/// <summary>
		/// Fills the treeview with the nodes taken from all tree_table rows
		/// Selects no node.
		/// </summary>
		public virtual void FillNodes(){
			//MarkEvent("FillNodes() start");
			var filter= RootsCondition_SQL(TreeTable);
            var cfilter = RootsCondition_C(TreeTable);

			var sort = TreeTable.getSorting();
			if (filter==null) return;
            var roots = TreeTable.Select(cfilter, sort);
			AutoEventsEnabled=false;
			tree.Enabled=false;
			foreach (var rootRow in roots){
				var rootNode = createNewNode(null, rootRow);
				
				var rootNode2 = AddNode(tree.Nodes,rootNode);                
				//TreeNode temp = new TreeNode("temporary");
				FillChildsNode(rootNode2, rootRow);
				
				rootNode2.Collapse();
				
			}
			tree.Enabled=true;
			AutoEventsEnabled=true;
			//MarkEvent("FillNodes() stop");
		}

		//Adds to (ParentNode-ParentRow) all childs nodes recursively
		void FillChildsNode(TreeNode parentNode, DataRow parentRow){
			//deletes temporary child if present
			if (parentNode.Nodes.Count==1){
				var test = parentNode.Nodes[0];
				if (test.Tag==null) parentNode.Nodes.Clear();
			}
			var childList = parentRow.iGetChildRows(AutoChildRelation);
			foreach(var childRow in childList){
				if (childRow==parentRow) continue;
				if (!security.CanSelect(childRow)){
					childRow.Delete();
					childRow.AcceptChanges();
					continue;
				}
				var childNode = createNewNode(parentRow, childRow);
				childNode = AddNode(parentNode.Nodes, childNode);
				FillChildsNode(childNode, childRow);
				//ChildNode.Expand();
				//ChildNode.Collapse();
			}

			//if fixed data don't add dummy nodes
			if (FixedData) return;

			if (parentNode.Nodes.Count==0){
				parentNode.Nodes.Add(new TreeNode("temporary"));				
			}
		}

		/// <summary>
		/// Creates a new tree_node linked to a given DataRow, assuming 
		///  a given TreeNode as parent
		/// </summary>
		/// <param name="parent">Parent TreeNode</param>
		/// <param name="r">DataRow linked to Node to create</param>
		/// <returns></returns>
		public tree_node GetNodeFromRow(TreeNode parent, DataRow r){
			DataRow rParent = null;
		    tree_node parentnode = (tree_node) parent?.Tag;
		    if (parentnode != null) rParent= parentnode.Row;
		    //RParent = ((tree_node)Parent.Tag).Row;
		    return dispatcher.GetNode(rParent, r);
		}

		/// <summary>
		/// Gets the TreeNode linked to Child, assuming the parent node is
		///  TreeNode. If the node does not exists, it is created. 
		/// </summary>
		/// <param name="parent">Parent node of searched one</param>
		/// <param name="child">DataRow linked to searched TreeNode</param>
		/// <returns>TreeNode linked to Child</returns>
		public TreeNode AddRow(TreeNode parent, DataRow child){            
			tree_node newnode = GetNodeFromRow(parent, child);
			TreeNode  newTreeNode = new TreeNode(newnode.Text());
			newTreeNode.Tag= newnode;
			if (parent==null) {
				return AddNode(tree.Nodes, newTreeNode);				
			}
			else {
				return AddNode(parent.Nodes, newTreeNode);
			}
		}

	

		private void treeview_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e) {
//			if (!AutoEventsEnabled) return;
//
//			if (FixedData) return; //don't re-read nodes
//
//			//if a navigator is present, child reading is done on node selection
//			if (Navigator!=null) {
//				//don't expand selected node again
//				if (tree.SelectedNode==e.Node) return;
//			}
//
			TreeNode N = e.Node;
			if (!N.IsSelected){
				tree.SelectedNode=N;
				if (!N.IsSelected) e.Cancel=true;
			}
//			if (N.Tag==null) return;
//			bool Added= expand_node(N);
//			if (!Added) e.Cancel=true;
		}

        /// <summary>
        /// Gets a relation that connects a table with its self
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public DataRelation autoParentRelation(DataTable T) {
            foreach (DataRelation r in T.ParentRelations) {
                if (r.ParentTable.TableName == T.TableName) return r;
            }
            return null;
        }

        /// <summary>
        /// Gets a relation that connects a table with its self. Should be the same
        ///  as AutoParent
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public DataRelation autoChildRelation(DataTable T) {
            foreach (DataRelation r in T.ChildRelations) {
                if ((r.ParentTable.TableName == T.TableName) &&
                    (r.ChildTable.TableName == T.TableName)
                ) return r;
            }
            return null;
        }


        /// <summary>
        /// reads child and return true if something was read
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        bool expand_node(TreeNode n){
            var r = ((tree_node) n.Tag)?.Row;
			if (r == null) return false;
            if (r.RowState == DataRowState.Detached) return false;
            n.Nodes.Clear();
			_treeDataAccess.expandChilds(new[] {r});
			var added=false;
			var filterChild = QueryCreator.WHERE_REL_CLAUSE(
						r, AutoChildRelation.ParentColumns, AutoChildRelation.ChildColumns,
						DataRowVersion.Default,false);
			var sort= r.Table.getSorting();
			//foreach(DataRow Rci in R.GetChildRows(AutoChildRelation)){
			foreach(var rci in r.Table.Select(filterChild,sort)){
				if (!security.CanSelect(rci)){
					rci.Delete();
					rci.AcceptChanges();
					continue;
				}		
				AddRow(n, rci);
				added=true;
			}			
			return added;

		}

		/// <summary>
		/// Reads child of a nodes to select. Used when a navigator is linked to tree.
		/// Ignored if FixedData
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void treeview_BeforeSelect(object sender, TreeViewCancelEventArgs e) {
			//if (Navigator==null)return;
			if (FixedData) return;
			var n = e.Node;
			if (n.Tag==null) return;
			if (n.Nodes.Count>1) return;
			if (n.Nodes.Count==1){
				if (n.Nodes[0].Tag!=null) return;
			}
			bool Added= expand_node(n);
		}
		

		/// <summary>
		/// Adds dummy nodes to childs of expanded node.
		/// Does nothing if FixedData
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void treeview_AfterExpand(object sender, TreeViewEventArgs e) {			
			if (!AutoEventsEnabled) return;
			if (FixedData) return;
			if (Navigator!=null)return;

			var n = e.Node;

			for (var i=0; i<n.Nodes.Count; i++){
				if (n.Nodes[i].Nodes.Count>0)continue;
				//Add a temporary node to the child node so it can be expanded
				n.Nodes[i].Nodes.Add(new TreeNode(""));				
			}
		}

		/// <summary>
		/// Set a datagrid as tree-navigator for a table T
		/// </summary>
		/// <param name="T"></param>
		/// <param name="G"></param>
		public static void setNavigator(DataTable T, DataGrid G){
			var tm = GetManager(T);
			if (tm==null){
				T.ExtendedProperties["MetaDataGridNavigator"]= G;
				return;
			}
			tm.Navigator= G;
		}


		void CascadeDelete(TreeNode node, bool onlychilds){
			foreach (TreeNode nn in node.Nodes) {
				CascadeDelete(nn,false);
			}
		    node.Nodes.Clear();            
			var n = (tree_node) node.Tag;
		    var r = n?.Row;
			if (r == null) return;
		    if (onlychilds) return;
		    r.Delete();
		    if (r.RowState==DataRowState.Deleted) r.AcceptChanges();

		}


//		public void tree_view_BeforeCollapse(object sender, System.Windows.Forms.TreeViewCancelEventArgs e) {			
//		}


		/// <summary>
		/// Do necessary operation to handle AfterCollapse event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void treeview_AfterCollapse(object sender, TreeViewEventArgs e) {
			if (!AutoEventsEnabled) return;
			if (FixedData) return;

			bool partialClear=tree.SelectedNode==e.Node;

		    AutoEventsEnabled=false;
			

			var collapsedNode = e.Node;
			var treeNode = (tree_node)collapsedNode.Tag;
			if (treeNode == null){
				foreach (TreeNode ni in collapsedNode.Nodes){
					ni.Collapse();					
					CascadeDelete(ni,true);
					if (partialClear){
						ni.Nodes.Add(new TreeNode("temp2"));
					}
				}
				AutoEventsEnabled=true;
				return;
			}

			if (collapsedNode.Nodes.Count==0) {
			    collapsedNode.Collapse();
				AutoEventsEnabled=true;
				return;
			}

			if (partialClear){
				foreach(TreeNode nn in collapsedNode.Nodes){
					nn.Collapse();
					CascadeDelete(nn,true);
				}
			}
			else {
				while (collapsedNode.Nodes.Count>0){
					CascadeDelete(collapsedNode.Nodes[0],false);
				    collapsedNode.Nodes.RemoveAt(0);			
				}		

			}

			if (collapsedNode.Nodes.Count==0) {
			    collapsedNode.Nodes.Add(new TreeNode("temp3"));
			    collapsedNode.Nodes[0].Collapse();
			}
			AutoEventsEnabled=true;

		}
		
		/// <summary>
		/// Deletes current node from tree (and all childs recursively)
		/// </summary>
		public virtual void DeleteCurrentNode(){
			var current = tree.SelectedNode;
			if (current==null) return;
			var parent = current.Parent;
		    var tc = parent?.Nodes ?? tree.Nodes;
			CascadeDelete(current,false);
			tc.Remove(current);
			if (tc.Count>0){
				tree.SelectedNode= tc[0];
			}
			else {
			    parent?.Collapse();
			    tree.SelectedNode=parent;
			}
		}

		/// <summary>
		/// Selects the TreeNode corresponding to a given DataRow
		/// </summary>
		/// <param name="r"></param>
		public void SelectNode(DataRow r){
			//MarkEvent("Select Node start");
			AutoEventsEnabled=false;
			SelectNode(tree.Nodes, r);	
			AutoEventsEnabled=true;
			//MarkEvent("Select Node stop");
		}
		bool compareNode(TreeNode n, DataRow r){
			if (n.Tag==null) return false;
			if (((tree_node)(n.Tag)).Row.Equals(r)) return true;
			return false;
		}
		bool compareNodes(TreeNode n1, TreeNode n2){
			if (n1.Tag==null) {
				if (n2.Tag!=null) return false;
				return (n1.Text==n2.Text);
			}
			if (n2.Tag==null) return false;
			return (((tree_node)n1.Tag).Row.Equals(((tree_node)n2.Tag).Row));
		}


		/// <summary>
		/// Returns added node, or previous if it was already present.
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="newNode"></param>
		/// <returns></returns>
		public virtual TreeNode AddNode(TreeNodeCollection nodes, TreeNode newNode){
			
			for(int i=0; i<nodes.Count;i++){
				if (compareNodes(newNode,   nodes[i])) return nodes[i];
				/*
				 if (NewNode.Text.CompareTo(Nodes[i].Text)<0) {
					Nodes.Insert(i,NewNode);
					return NewNode;
				}
				*/
			}
			
			nodes.Add(newNode);	
			return newNode;
		}

		bool SelectNode(TreeNodeCollection ns, DataRow r){
			//search in nodes
			foreach (TreeNode n in ns){
				if (compareNode(n,r)){
					n.EnsureVisible();
					tree.SelectedNode=n;
					return true;
				}
			}
			//search in childs
			foreach (TreeNode n in ns){
				if (SelectNode(n.Nodes, r)){
					//N.Expand();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get the row linked to currently selected TreeNode
		/// </summary>
		/// <returns></returns>
		public virtual DataRow SelectedRow(){
			var n= tree.SelectedNode;
		    var tn = (tree_node) n?.Tag;
			return tn?.Row;
		}

		/// <summary>
		/// Gets the index of Row linked to Currenty selected TreeNode
		/// </summary>
		/// <returns></returns>
		public virtual int SelectedRowIndex(){
			var n= tree.SelectedNode;
			if (n==null) return -1;
			var tn = (tree_node)n.Tag;
			return tn.RowIndex();
		}

		/// <summary>
		/// Gets the treeviewmanager linked to a DataTable
		/// </summary>
		/// <param name="T"></param>
		/// <returns></returns>
		public static TreeViewManager GetManager(DataTable T){
		    return (TreeViewManager) T.ExtendedProperties["treemanager"];
		}

		private void setManager(DataTable T){
			T.ExtendedProperties["treemanager"]=this;
		}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Implents IDisposable interface
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {                                                                                                                                             
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                    if (Tip != null) Tip.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Start ** CHECKED **
        /// </summary>
        /// <param name="rootfilterSql"></param>
        /// <param name="clear"></param>
        public virtual void Start(string rootfilterSql, bool clear) {
            if (TreeTable.ExtendedProperties[HelpForm.FilterTree] == null) {
                FixedData = false;
                var nonRoots = TreeTable.Select(); // T.Select("NOT (" + rootfilter + ")");

                _treeDataAccess.DO_GET_TABLE_ROOTS(TreeTable, rootfilterSql, clear);
                foreach (var child in nonRoots) {
                    _treeDataAccess.DO_GET_PARENTS(child, true, autoParentRelation(TreeTable));
                }
            }
            else {
                FixedData = true;
                var list = (DataTable)TreeTable.ExtendedProperties[HelpForm.FilterTree];
                mdl_utils.DataSetUtils.CopyPrimaryKey(list, TreeTable);
                foreach (DataRow toCopy in list.Rows) {
                    var searchfilter = QueryCreator.WHERE_KEY_CLAUSE(toCopy, DataRowVersion.Default, false);
                    if (TreeTable.Select(searchfilter).Length > 0) continue;

                    var newR = TreeTable.NewRow();
                    foreach (DataColumn col in TreeTable.Columns) newR[col] = toCopy[col.ColumnName];
                    TreeTable.Rows.Add(newR);
                    newR.AcceptChanges();
                }
                foreach (DataRow toCopy in list.Rows) {
                    var searchfilter = QueryCreator.WHERE_KEY_CLAUSE(toCopy, DataRowVersion.Default, false);
                    var found = TreeTable.Select(searchfilter)[0];
                    _treeDataAccess.DO_GET_PARENTS(found, false, autoParentRelation(TreeTable));
                }
            }
            FillNodes();
        }
		
		public static IMetaModel staticModel { get; set; } = MetaFactory.factory.getSingleton<IMetaModel>();

		/// <summary>
		/// Fills a tree given a start condition. Also Accepts FilterTree **CHECKED**
		/// </summary>
		/// <param name="startCondition"></param>
		/// <param name="startValueWanted"></param>
		/// <param name="startFieldWanted"></param>
		/// <returns></returns>
		public DataRow  startWithField(string startCondition,
            string startValueWanted,
            string startFieldWanted) {
			staticModel.clear(TreeTable); 
            var r = _treeDataAccess.GetSpecificChild(TreeTable, startCondition, startValueWanted, startFieldWanted);
            if (r == null) {
                return null;
            }

            //checks if any filter is present
            if (TreeTable.ExtendedProperties[HelpForm.FilterTree] != null) {
                var rowkey = QueryCreator.WHERE_KEY_CLAUSE(r, DataRowVersion.Default, false);
                var list = (DataTable)TreeTable.ExtendedProperties[HelpForm.FilterTree];
                var founded = list.Select(rowkey);
                if (founded.Length == 0) {
                    return null;
                }
            }

            var filter = GetData.MergeFilters(startCondition, RootsCondition_SQL());

            Start(filter, false);

            //search a node that has StartCondition + StartFieldWanted LIKE StartValueWanted			
            SelectNode(r);
            HelpForm.SetLastSelected(TreeTable, r);
            return r;
        }

        /// <summary>
        /// Legge tutte le righe parent di una riga data e la seleziona nel tree. ** CHECKED **
        /// </summary>
        /// <param name="r"></param>
        /// <param name="listType"></param>
        /// <returns></returns>
        public DataRow selectRow(DataRow r, string listType) {
            if (r == null) return null;
            //Verify if R is already in Tree
            var keyfilter = QueryCreator.WHERE_REL_CLAUSE(r, TreeTable.PrimaryKey,
                TreeTable.PrimaryKey, DataRowVersion.Default, false);
            var existent = TreeTable.Select(keyfilter);
            var toSelect = existent.Length == 0 ? _treeDataAccess.GetByKey(TreeTable, r) : existent[0];
            if (toSelect == null) {
                return null;
            }

            _treeDataAccess.DO_GET_PARENTS(toSelect, true, autoParentRelation(TreeTable));
            tree.SelectedNode = null;
            FillNodes(TreeTable);
            SelectNode(toSelect);
            return toSelect;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TreeViewManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // 
        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

  
}
