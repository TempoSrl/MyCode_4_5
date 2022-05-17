using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using wf=System.Windows.Forms;
using mdl;

namespace mdl_winform {


    /// <summary>
    /// Implements IResponder interface
    /// </summary>
    public class DefaultResponder :IResponder {
        public  bool skipMessagesBox { get; set; }=false;
        public bool registerErrorMessages { get; set; } =false;
        List<string> errorMessages = new List<string>();

        public void clearMessages() {
            errorMessages.Clear();
        }

        public List<string> getMessages() {
            return errorMessages;
        }

        protected mdl.DialogResult standardResponse(mdl.MessageBoxButtons btns) {
            if (btns.Equals(mdl.MessageBoxButtons.OKCancel)) return mdl.DialogResult.Cancel;
            if (btns.Equals(mdl.MessageBoxButtons.OK)) return mdl.DialogResult.OK;
            if (btns.Equals(mdl.MessageBoxButtons.AbortRetryIgnore)) return mdl.DialogResult.Abort;
            if (btns.Equals(mdl.MessageBoxButtons.RetryCancel)) return mdl.DialogResult.Cancel;
            if (btns.Equals(mdl.MessageBoxButtons.YesNo)) return mdl.DialogResult.No;
            if (btns.Equals(mdl.MessageBoxButtons.YesNoCancel)) return mdl.DialogResult.Cancel;
            return mdl.DialogResult.None;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrl">IWin32Window</param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="result"></param>
        /// <returns>true when response was provided</returns>
        public virtual bool getResponse(object ctrl,string text, string caption,  out mdl.DialogResult result) {
            result = mdl.DialogResult.None;
            if (registerErrorMessages)errorMessages.Add(text);
            return skipMessagesBox;//this means that responder has no answer for this message
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrl">IWin32Window</param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <param name="result"></param>
        /// <returns>true when response was provided</returns>
        public virtual bool getResponse(object ctrl,string text, string caption, mdl.MessageBoxButtons btns, out mdl.DialogResult result) {
            result = standardResponse(btns);
            if (registerErrorMessages)errorMessages.Add(text);
            return skipMessagesBox;
        }

        public virtual void showError(object o, string msg, string LongMessage, string logUrl) {
            wf.Form F = o as wf.Form;
            if (registerErrorMessages)errorMessages.Add($"{msg}:{LongMessage}");
            if (skipMessagesBox) return;
            var MSG = new FrmShowException(msg,LongMessage);
            MSG.logurl=logUrl;
            if (F != null  && !F.Disposing) {
                MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, F);
                MSG.ShowDialog(F);
            }
            else
            {
                MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, null);
                MSG.ShowDialog();
            }
            return;
        }

        public void showException(object o, string msg, Exception e) {
             wf.Form f = o as wf.Form;
            if (registerErrorMessages)errorMessages.Add($"{msg}");
            if (skipMessagesBox) return;
            var m = FormController.GetMetaData(f);            
            var MSG = new FrmShowException(msg,e,m);
            try {
                if (f != null && !f.Disposing) {
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, f);
                    MSG.ShowDialog(f);
                }
                else {
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, null);
                    MSG.ShowDialog();
                }
                
            }
            catch {
                if (!MSG.Visible)
                {
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, null);
                    MSG.ShowDialog();
                }
            }
            return;
        }

        public virtual void showException(object o, string msg, Exception e, string logUrl) {
             wf.Form f = o as wf.Form;
            if (registerErrorMessages)errorMessages.Add($"{msg}:{QueryCreator.GetErrorString(e)}");
            if (skipMessagesBox) return;
            var m = FormController.GetMetaData(f);
            var MSG = new FrmShowException(msg, e, m) {logurl = logUrl};
            try
            {
                MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, f);
                MSG.ShowDialog(f);
            }
            catch {
                if (!MSG.Visible)
                {
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(MSG, null);
                    MSG.ShowDialog();
                }
            }
        }

        public virtual void showNoRowFound(object o, string mainMessage, string longMessage) {
              wf.Form F = o as wf.Form;
            if (registerErrorMessages)errorMessages.Add($"{mainMessage}:{longMessage}");
            if (skipMessagesBox) return;
            var f = new frmAvvisoNessunaRigaTrovata(mainMessage, longMessage);
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, F);
            f.ShowDialog(F);
        }
    }

    /// <summary>
    /// Default IMessageShower
    /// </summary>
    public class DefaultMessageShower :IMessageShower {
        public IResponder responder= new DefaultResponder();

        public void setAutoResponder(IResponder responder) {
            this.responder = responder;
        }

        public IResponder getResponder() {
            return responder;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrl">IWin32Window</param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <returns></returns>
        public mdl.DialogResult Show(object ctrl, string text, string caption,mdl.MessageBoxButtons btns) {
            mdl.DialogResult result;
            if (responder.getResponse(ctrl as wf.IWin32Window, text, caption, btns, out result)) return result;
            return (mdl.DialogResult) wf.MessageBox.Show(ctrl as wf.IWin32Window, text, caption, (wf.MessageBoxButtons) btns);
        }
        public mdl.DialogResult Show(string text, string caption, mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons,
	        mdl.MessageBoxDefaultButton defBtn) {
	        mdl.DialogResult result;
	        if (responder.getResponse(null, text, caption, btns, out result)) return result;
	        return (mdl.DialogResult) wf.MessageBox.Show( text, caption, (wf.MessageBoxButtons) btns, (wf.MessageBoxIcon) icons, (wf.MessageBoxDefaultButton) defBtn);
        }

        public mdl.DialogResult Show(object o, string text, string caption, mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons,
	        mdl.MessageBoxDefaultButton defBtn) {
            var ctrl = o as wf.IWin32Window;
	        mdl.DialogResult result;
	        if (responder.getResponse(ctrl, text, caption, btns, out result)) return result;
	        return (mdl.DialogResult) wf.MessageBox.Show(ctrl, text, caption, 
                (wf.MessageBoxButtons) btns,  
                (wf.MessageBoxIcon)icons, 
                (wf.MessageBoxDefaultButton)defBtn);
        }

        public mdl.DialogResult Show(object o, string text, string caption,mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons) {
	        mdl.DialogResult result;
            var ctrl = o as wf.IWin32Window;
	        if (responder.getResponse(ctrl, text, caption, btns, out result)) return result;
	        return  (mdl.DialogResult) wf.MessageBox.Show(ctrl, text, caption, 
                    (wf.MessageBoxButtons) btns,
                     (wf.MessageBoxIcon)icons);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public mdl.DialogResult Show(object o, string text, string caption) {
            mdl.DialogResult result;
            var ctrl = o as wf.IWin32Window;
            if (responder.getResponse(ctrl, text, caption,  out result)) return result;
            return  (mdl.DialogResult) wf.MessageBox.Show(ctrl, text, caption);
        }
        public mdl.DialogResult Show(string text, string caption, mdl.MessageBoxButtons btns) {
	        mdl.DialogResult result;
	        if (responder.getResponse(null, text, null,  out result)) return result;
	        return (mdl.DialogResult) wf.MessageBox.Show(text,caption, (wf.MessageBoxButtons) btns);
        }

        public mdl.DialogResult Show(string text, string caption, MessageBoxButtons btns, mdl.MessageBoxIcon icons) {
	        mdl.DialogResult result;
	        if (responder.getResponse(null, text, null,  out result)) return result;
	        return  (mdl.DialogResult) wf.MessageBox.Show(text,caption, (wf.MessageBoxButtons) btns,  (wf.MessageBoxIcon)icons);
        }

        public mdl.DialogResult Show(object o, string text) {
            var ctrl = o as wf.IWin32Window;
            mdl.DialogResult result;
            if (responder.getResponse(ctrl, text, null,  out result)) return result;
            return (mdl.DialogResult) wf.MessageBox.Show(ctrl, text);
        }

        public mdl.DialogResult Show(string text, string caption) {
            mdl.DialogResult result;
            if (responder.getResponse(null, text, caption,  out result)) return result;
            return  (mdl.DialogResult) wf.MessageBox.Show(text, caption);
        }

        public mdl.DialogResult Show(string text) {
            mdl.DialogResult result;
            if (responder.getResponse(null, text, null,  out result)) return result;
            return  (mdl.DialogResult) wf.MessageBox.Show(text, null);
        }

        public void ShowException(object o, string msg, Exception e, string logUrl) {
            wf.Form f =  o as wf.Form;
            responder.showException(f,msg,e,logUrl);
        }

        public void ShowException(object o, string msg, Exception e) {
            wf.Form f =  o as wf.Form;
            responder.showException(f,msg,e);
        }

        public void ShowError(object o, string MainMessage, string LongMessage, string logUrl) {
            wf.Form f =  o as wf.Form;
            responder.showError(f,MainMessage,LongMessage,logUrl);
        }

        public void ShowNoRowFound(object o, string mainMessage, string longMessage) {
           wf.Form f =  o as wf.Form;
           responder.showNoRowFound(f,mainMessage,longMessage);
        }
    }
    
}
