using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LM=mdl_language.LanguageManager;
using mdl;

namespace mdl_winform {

    /// <summary>
    /// Manager of Main ToolBar
    /// </summary>
    public interface IMainToolBarManager {
        /// <summary>
        /// Enables / Disables buttons of the bar
        /// </summary>
        void freshButtons();

        /// <summary>
        /// links the toolbar to a form controller
        /// </summary>
        /// <param name="m"></param>
        /// <param name="formController"></param>
        void linkTo(WinFormMetaData m, IFormController formController);

        /// <summary>
        /// Unlinks the toolbar from any formController
        /// </summary>
        void unlink();

        /// <summary>
        /// Unlinks the toolbar from a specified metadata
        /// </summary>
        /// <param name="m"></param>
        void unlink(WinFormMetaData m);

        /// <summary>
        /// Gets height  for the menu Bar, used to position forms in the desktop area
        /// </summary>
        /// <returns></returns>
        int getSizeBarHeight();

    }

    #region Gestione MainToolBar

    /// <inheritdoc />
    /// <summary>
    /// Manager of Main ToolBar
    /// </summary>
    public class MainToolBarManager : IMainToolBarManager {

       

        /// <summary>
        /// Gets height  for the menu Bar, used to position forms in the desktop area
        /// </summary>
        /// <returns></returns>
        public virtual int getSizeBarHeight() {
            return 85;
        }

        readonly ToolBar _bar;
        private WinFormMetaData _meta;
        private IFormController _formController;
        
        
        /// <summary>
        /// returns the manager of a toolbar, creating a new one if it does not yet exists
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        public static MainToolBarManager GetToolBarManager(ToolBar bar) {
            if (bar.Tag == null) {
                bar.Tag = new MainToolBarManager(bar);
                bar.AutoSize = true;
                bar.Wrappable = true;
                bar.Update();
                bar.Refresh();
            }
            if (!(bar.Tag is MainToolBarManager)) {
                bar.Tag = new MainToolBarManager(bar);
            }
            return (MainToolBarManager)bar.Tag;
        }

        /// <summary>
        /// Builds a manager giving it a toolbar
        /// </summary>
        /// <param name="bar"></param>
        public MainToolBarManager(ToolBar bar) {
            _bar = bar;
            bar.ButtonClick += onClick;
        }

        bool _evaluated;
        bool _mustdofix = true;
        bool mustDoFix() {
            if (_evaluated) return _mustdofix;
            if (_formController.linkedForm.Modal) {
                _evaluated = true;
                _mustdofix = false;
                return false;
            }
            if (_formController?.linkedForm?.MdiParent == null) {
                _evaluated = true;
                _mustdofix = false;
                return false;
            }
            if (Screen.PrimaryScreen.WorkingArea.Width <= 800) {
                _evaluated = true;
                _mustdofix = false;
                return false;
            }
            _evaluated = true;
            _mustdofix = true;
            return true;
        }

        private bool setVisible(ToolBarButton but, bool visible) {
            if (mustDoFix()) {
                if (but.Enabled == visible) return false;
                but.Enabled = visible;
                return true;
            }
            if (but.Visible == visible) return false;
            but.Visible = visible;
            return true;
            
        }

        /// <summary>
        /// Update button status
        /// </summary>
        public void freshButtons() {
            if (_bar == null) return;
            var somethingdone = false;
            foreach (ToolBarButton b in _bar.Buttons) {
                string cmd = null;
                if (b.Tag != null) cmd = b.Tag.ToString();
                if (cmd == null) continue; //button unchanged
                if (_meta != null && _meta.CommandEnabled(cmd)) {
                    if (cmd == "maindelete") {
                        if (_formController.formState == form_states.insert) {
                            if (b.Text != LM.cancel) b.Text = LM.cancel;
                        }
                        else {
                            if (b.Text != LM.Delete) b.Text = LM.Delete;
                        }
                    }

                    b.Enabled = true;
                    if (cmd == "editnotes") {
                        if (!b.PartialPush) b.PartialPush = true;
                        if (_meta.NotesAvailable(_meta.CurrentRow)) {
                            if (!b.Pushed) b.Pushed = true;
                        }
                        else {
                            if (b.Pushed) b.Pushed = false;
                        }
                    }
                    somethingdone |= setVisible(b, true);
                }
                else {
                    b.Enabled = false;
                    if ((_meta != null) && ((cmd == "gotonext") || (cmd == "gotoprev"))) {
                        if (cmd == "gotonext")
                            somethingdone |= setVisible(b, _meta.CommandEnabled("gotoprev"));
                        else
                            somethingdone |= setVisible(b, _meta.CommandEnabled("gotonext"));
                    }
                    else {
                        somethingdone |= setVisible(b, false);
                    }
                }
            }
            if (!_bar.Visible) {

                _bar.Visible = true;
                somethingdone = true;
            }
            if (!_bar.AutoSize) _bar.AutoSize = true;
            if (!_bar.Wrappable) _bar.Wrappable = true;
            if (!somethingdone) return;
            _bar.Update();
            _bar.Refresh();
        }

       

        /// <summary>
        /// Links the toolbar to a formPresentation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="formController"></param>
        public void linkTo(WinFormMetaData m, IFormController formController) {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (formController == null) throw new ArgumentNullException(nameof(formController));

            _bar.Visible = true;
            if (formController.linkedForm?.MdiParent != null && _meta != null) {
                if (formController.linkedForm.MdiParent.ActiveMdiChild == _formController.linkedForm) {
                    freshButtons();
                    return;
                }
            }
            if (_meta == m) {
                freshButtons();
                return;
            }
            _meta = m;
            _formController = formController;
            freshButtons();
        }
        /// <summary>
        /// Link toolbar to a new metadata
        /// </summary>
        /// <param name="m"></param>
        [Obsolete]
        public void LinkTo(WinFormMetaData m) {
            linkTo(m,m.controller);          
        }

        /// <summary>
        /// Unlink the toolbar from any MetaData
        /// </summary>
        public void unlink() {
            unlink(null);
        }

        /// <summary>
        /// Unlink the bar from a metadata
        /// </summary>
        /// <param name="m"></param>
        public void unlink(WinFormMetaData m) {
            if (m != null && _meta != m) return;
            _meta = null;
            _formController = null;
            _bar.Visible = false;            
        }

        /// <summary>
        /// Unlink the toolbar from a specific MetaData
        /// </summary>
        [Obsolete("Use IMetaData.unlink")]
        public void Unlink(WinFormMetaData m) {
            unlink(m);
        }

        /// <summary>
        /// Unlink the toolbar from a specific MetaData
        /// </summary>
        [Obsolete("Use IMetaData.unlink")]
        public void Unlink() {
            unlink(null);
        }

        void onClick(object sender, ToolBarButtonClickEventArgs e) {
            if (_meta == null) return;
            if (_formController.locked) return;
            if (!_formController.DrawStateIsDone) return;
            _formController.linkedForm.ActiveControl = null;
            _bar.Focus();
            if (!_bar.Focused) return;
            var b = e.Button;
            if (b.Tag == null) return;
            var cmd = b.Tag.ToString();
            if (!_meta.CommandEnabled(cmd)) return;
            _formController.DoMainCommand(cmd); //_meta.DoMainCommand(cmd);
            freshButtons();
        }

        /// <summary>
        /// Gets the main toolbar of the form
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static ToolBar GetToolBar(Form f) {
            if (f == null) return null;
            if (f.IsDisposed) return null;
            var formType = f.GetType();
            var toolBarInfo = formType.GetField("MetaDataToolBar");
            if (toolBarInfo == null) return null;

            if (!typeof(ToolBar).IsAssignableFrom(toolBarInfo.FieldType)) return null;

            var tb = (ToolBar)toolBarInfo.GetValue(f);
            return tb;
        }
    }
    #endregion

}
