using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using mdl;

namespace mdl_winform {
    /// <summary>
    /// Form to ask an operator  to use in a custom query conditions
    /// </summary>
    public partial class FrmFiltraColonna : MetaDataForm {

        /// <summary>
        /// Default constructor
        /// </summary>
        public FrmFiltraColonna() {
            InitializeComponent();
        }

        /// <summary>
        /// number of selected operator
        /// </summary>
        public int result = 0;
        private void btnOk_Click(object sender, EventArgs e) {

            foreach (RadioButton r in gboxOperatori.Controls) {
                if (r == null) continue;
                if (!r.Checked) continue;
                result = Convert.ToInt32(r.Tag);
                break;
            }
        }
    }
}
