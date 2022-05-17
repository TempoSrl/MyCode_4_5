using mdl;
using HelpWeb;
using System.Data;


namespace mdl_webform
{
    public interface IWebFormMetaData: IMetaData{
         /// <summary>
        /// Is called when a tree_view is linked to a MetaData in a web form
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="T"></param>
        /// <param name="listingType"></param>
        void WebDescribeTree(hwTreeView tree, DataTable T, string listingType);
    }

    public class WebFormMetaData :MetaData, IWebFormMetaData {
       

        /// <summary>
        /// Is called when a tree_view is linked to a MetaData in a web form
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="T"></param>
        /// <param name="ListingType"></param>
        public virtual void WebDescribeTree(hwTreeView tree, DataTable T, string ListingType) {
            describeListType(conn, T, ListingType);
        }
    }

    
    /// <summary>
    /// TreeNode for web, with an object Tag
    /// </summary>
    public class hwTreeNode :  System.Web.UI.WebControls.TreeNode {
        private object _Tag;



        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="T"></param>
        public hwTreeNode(string T)
            : base(T) { }

        /// <summary>
        /// General pourpose tag
        /// </summary>
        public object Tag {
            get { return _Tag; }
            set { _Tag = value; }
        }

    }

}