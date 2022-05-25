using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;
using mdl;

namespace mdl_winform {
    /// <summary>
    /// Manages control enabling / disabling
    /// </summary>
    public interface IControEnabler {

        /// <summary>
        /// Initializes an instance on a specified primary Table. 
        /// </summary>
        /// <param name="primaryTable"></param>
        void init(DataTable primaryTable);

        /// <summary>
        /// Set a control as enabled or disabled basing on tag and form data:
        /// -  design-time disabled controls are left disabled
        /// -  autoincrement fields are always disabled (also hidden on insert mode)
        /// -  other primary fields are left enabled
        /// -  primary key are disabled on edit mode
        /// -  other primary fields are left enabled
        /// </summary>
        /// <param name="c"></param>
        /// <param name="T"></param>
        /// <param name="field"></param>
        /// <param name="drawMode"></param>
        void enableDisable(Control c, DataTable T, string field, HelpForm.drawmode drawMode);

        /// <summary>
        /// Enables a control that has been disabled from the framework because it was on a primary key column or it was an autoincrement field
        /// </summary>
        /// <param name="c"></param>
        void reEnable(Control c);

        /// <summary>
        /// Enablesor disables a button
        /// </summary>
        /// <param name="b"></param>
        /// <param name="enable"></param>
        void enableButton(Button b, bool enable);
    }

    /// <inheritdoc />
    public class ControEnabler : IControEnabler {
        
        Hashtable _toEnable;
        private DataTable _primaryTable;

        
        /// <inheritdoc />
        /// <summary>
        /// Initialize class
        /// </summary>
        /// <param name="primaryTable"></param>
        public void init(DataTable primaryTable) {
            _toEnable= new Hashtable();
            _primaryTable = primaryTable;
        }
        /// <summary>
        /// Disable a control and adds it to the "toenable" list.
        /// Optionally tries to hide content of control
        /// </summary>
        /// <param name="c"></param>
        /// <param name="hideContent">When true, textbox are threated like passwords</param>
        void doDisable(Control c, bool hideContent) {
            if (c is Label) {
                return;
            }

            if(c is GroupBox box) {
                if(box.Enabled == false)
                    return;
                box.Enabled = false;
                enableDisableValueSigned(box, false);
                _toEnable[box.Name] = c;
				box.FindForm()?.getInstance<IFormController>()?.setColor(box);
                //MetaData.SetColor(box);
                return;
            }
            if(c is TextBox tx) {
                if(hideContent) {
                    if(tx.PasswordChar == Convert.ToChar(0))
                        tx.PasswordChar = ' ';
                }
                else {
                    if(tx.PasswordChar == ' ')
                        tx.PasswordChar = Convert.ToChar(0);
                }

                if(tx.ReadOnly) {
	                tx.FindForm()?.getInstance<IFormController>()?.setColor(tx);
                    return;
                }
                tx.ReadOnly = true;
                _toEnable[tx.Name] = c;
                tx.FindForm()?.getInstance<IFormController>()?.setColor(tx);
                return;
            }
            if (!c.Enabled) {
	            c.FindForm()?.getInstance<IFormController>()?.setColor(c);
                return;
            }
            c.Enabled = false;
            _toEnable[c.Name] = c;
            c.FindForm()?.getInstance<IFormController>()?.setColor(c);
        }

        void enableDisableValueSigned(GroupBox g, bool enable) {
            foreach (Control c in g.Controls) {
                if(c is TextBox box) {
                    box.ReadOnly = !enable;
                }

                if(c is RadioButton button) {
                    switch(button.Tag.ToString()) {
                        case "-":
                            button.Enabled = enable;
                            break;
                        case "+":
                            button.Enabled = enable;
                            break;
                    }
                }
            }
        }


        /// <inheritdoc />
        public virtual void enableDisable(Control c, DataTable T, string field, HelpForm.drawmode drawMode) {
            if (c is Label) return;

            if (drawMode == HelpForm.drawmode.setsearch) {
                reEnable(c);
                return;
            }
            var currRow = HelpForm.GetLastSelected(_primaryTable);
            if (drawMode == HelpForm.drawmode.edit && currRow == null) {
                //If there is no primary table row disable all controls
                doDisable(c, false);
                return;
            }

            if (T.TableName == _primaryTable.TableName) {
                //Table is primary
                //20/3/18: abilitiamo / disabilitiamo i controlli customautoincrement alla stessa stregua di quelli normali
                //21/3/18 rimuovo ||RowChange.IsCustomAutoIncrement(T.Columns[field]) per non 
                //  intervenire sulle gestioni custom
                if (T.Columns[field].IsAutoIncrement()) {
                    //Disable autoincrement properties on insert mode
                    doDisable(c, drawMode == HelpForm.drawmode.insert); //hide autoincrements on insert 
                    return;
                }
                if (drawMode == HelpForm.drawmode.insert) {
                    //Enable all other fields on insert mode
                    reEnable(c);
                    return;
                }
                if (QueryCreator.IsPrimaryKey(_primaryTable, field)) {
                    //Disable primary key fields on edit mode
                    doDisable(c, false);
                    return;
                }
                //Re enable all other fields in edit mode if they had been disabled 
                reEnable(c);
                return;
            }


            //Table is not primary
            var t1 = mdl_utils.tagUtils.GetStandardTag(c.Tag);
            var t2 = mdl_utils.tagUtils.GetSearchTag(c.Tag);
            if (t1 == t2) {
                if (!MetaModel.IsSubEntity(T, _primaryTable)) {
                    //Disable all controls except subentity fields
                    doDisable(c, false);
                    return;
                }
                var currChild = HelpForm.GetCurrChildRow(currRow, T);
                if (currChild == null || !c.Name.StartsWith("SubEntity")) {
                    doDisable(c, false);
                    return;
                }
                //leave the control enabled
                reEnable(c);
                return;
            }

            if (drawMode == HelpForm.drawmode.insert) {
                reEnable(c);
                return;
            }

            //Check if relation implies primary key fields of primary table
            if (QueryCreator.CheckKeyParent(T, _primaryTable)) {
                doDisable(c, false);
                return;
            }
            reEnable(c);
        }


        /// <inheritdoc />
        public virtual void reEnable(Control c) {
            if (_toEnable[c.Name] == null) return;
            if(c is TextBox box) {
                if(box.ReadOnly)
                    box.ReadOnly = false;
                if(box.PasswordChar == ' ')
                    box.PasswordChar = Convert.ToChar(0); // = '0'

            }
            else {
                if(c.Enabled == false)
                    c.Enabled = true;
            }
            _toEnable.Remove(c.Name);
            c.FindForm()?.getInstance<IFormController>()?.setColor(c);
        }

        /// <inheritdoc />
        public virtual void enableButton(Button b, bool enable) {
            if (enable) {
                reEnable(b);
            }
            else {
                doDisable(b, false);
            }
        }
    }
}
