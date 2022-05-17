using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mdl;

namespace mdl_winform {
#pragma warning disable 1591
    public class NativeMethods {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int SendMessage(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam);
    }

    public class Messages {
        public const int WM_USER = 0x0400;
        public const int EM_GETOLEINTERFACE = WM_USER + 60;
    }
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00020D00-0000-0000-c000-000000000046")]
    public interface IRichEditOle {
        int GetClientSite(IntPtr lplpolesite);
        int GetObjectCount();
        int GetLinkCount();
        int GetObject(int iob, REOBJECT lpreobject, [MarshalAs(UnmanagedType.U4)]GetObjectOptions flags);
        int InsertObject(REOBJECT lpreobject);
        int ConvertObject(int iob, CLSID rclsidNew, string lpstrUserTypeNew);
        int ActivateAs(CLSID rclsid, CLSID rclsidAs);
        int SetHostNames(string lpstrContainerApp, string lpstrContainerObj);
        int SetLinkAvailable(int iob, int fAvailable);
        int SetDvaspect(int iob, uint dvaspect);
        int HandsOffStorage(int iob);
        int SaveCompleted(int iob, IntPtr lpstg);
        int InPlaceDeactivate();
        int ContextSensitiveHelp(int fEnterMode);
        //int GetClipboardData(CHARRANGE FAR * lpchrg, uint reco, IntPtr lplpdataobj);
        //int ImportDataObject(IntPtr lpdataobj, CLIPFORMAT cf, HGLOBAL hMetaPict);
    }

    public enum GetObjectOptions {
        REO_GETOBJ_NO_INTERFACES = 0x00000000,
        REO_GETOBJ_POLEOBJ = 0x00000001,
        REO_GETOBJ_PSTG = 0x00000002,
        REO_GETOBJ_POLESITE = 0x00000004,
        REO_GETOBJ_ALL_INTERFACES = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CLSID {
        public int a;
        public short b;
        public short c;
        public byte d;
        public byte e;
        public byte f;
        public byte g;
        public byte h;
        public byte i;
        public byte j;
        public byte k;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZEL {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class REOBJECT {
        public REOBJECT() {
        }

        public int cbStruct = Marshal.SizeOf(typeof(REOBJECT));		// Size of structure
        public int cp = 0;												// Character position of object
        public CLSID clsid = new CLSID();								// Class ID of object
        private IntPtr poleobj = IntPtr.Zero;								// OLE object interface
        private IntPtr pstg = IntPtr.Zero;									// Associated storage interface
        private IntPtr polesite = IntPtr.Zero;								// Associated client site interface
        public SIZEL sizel = new SIZEL();								// Size of object (may be 0,0)
        public uint dvaspect = 0;										// Display aspect to use
        public uint dwFlags = 0;										// Object status flags
        public uint dwUser = 0;											// Dword for user's use
    }


    public class RichTextBoxPlus : RichTextBox {
        private IRichEditOle IRichEditOleValue = null;
        private IntPtr IRichEditOlePtr = IntPtr.Zero;

        /// <summary>
        /// Create the RichTextBoxPlus object.
        /// </summary>
        public RichTextBoxPlus() {
        }

        /// <summary>
        /// Get the IRichEditOle interface from the RichTextBox.
        /// </summary>
        /// <returns>The <see cref="IRichEditOle"/> interface.</returns>
        public IRichEditOle GetRichEditOleInterface() {
            if (this.IRichEditOleValue == null) {
                //REOBJECT reObject = new REOBJECT();
                //reObject.cp = 0;
                //reObject.dwFlags = GetObjectOptions.REO_GETOBJ_POLEOBJ;
                //IntPtr ptr = Marshal.AllocCoTaskMem(reObject.cbStruct);
                //Marshal.StructureToPtr(reObject, ptr, false);

                // Allocate the ptr that EM_GETOLEINTERFACE will fill in.
                IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr)));	// Alloc the ptr.
                Marshal.WriteIntPtr(ptr, IntPtr.Zero);	// Clear it.
                try {
                    if (0 != NativeMethods.SendMessage(this.Handle, Messages.EM_GETOLEINTERFACE, IntPtr.Zero, ptr)) {
                        // Read the returned pointer.
                        IntPtr pRichEdit = Marshal.ReadIntPtr(ptr);
                        try {
                            if (pRichEdit != IntPtr.Zero) {
                                // Query for the IRichEditOle interface.
                                Guid guid = new Guid("00020D00-0000-0000-c000-000000000046");
                                Marshal.QueryInterface(pRichEdit, ref guid, out this.IRichEditOlePtr);

                                // Wrap it in the C# interface for IRichEditOle.
                                this.IRichEditOleValue = (IRichEditOle)Marshal.GetTypedObjectForIUnknown(this.IRichEditOlePtr, typeof(IRichEditOle));
                                if (this.IRichEditOleValue == null) {
                                    throw new Exception("Failed to get the object wrapper for the interface.");
                                }
                            }
                            else {
                                throw new Exception("Failed to get the pointer.");
                            }
                        }
                        finally {
                            Marshal.Release(pRichEdit);
                        }
                    }
                    else {
                        throw new Exception("EM_GETOLEINTERFACE failed.");
                    }
                }
                catch (Exception err) {
                    Trace.WriteLine(err.ToString());
                    this.ReleaseRichEditOleInterface();
                }
                finally {
                    // Free the ptr memory.
                    Marshal.FreeCoTaskMem(ptr);
                    //Marshal.DestroyStructure(ptr, typeof(REOBJECT));
                }
            }
            return this.IRichEditOleValue;
        }

        /// <summary>
        /// Releases the IRichEditOle interface if it hasn't been already.
        /// </summary>
        /// <remarks>This is automatically called in Dispose if needed.</remarks>
        public void ReleaseRichEditOleInterface() {
            if (this.IRichEditOlePtr != IntPtr.Zero) {
                Marshal.Release(this.IRichEditOlePtr);
            }

            this.IRichEditOlePtr = IntPtr.Zero;
            this.IRichEditOleValue = null;
        }

        #region IDisposable Members
        bool disposed = false;
        protected override void Dispose(bool disposing) {
            if (!disposed) {
                // Dispose of resources held by this instance.
              
                disposed = true;
               
                // Suppress finalization of this disposed instance.
                if (disposing) {
                    // Dispose managed resources.              
                    
                }
                ReleaseRichEditOleInterface();

            }
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        ~RichTextBoxPlus() {
            Dispose(false);
        }

        /// <summary>
        /// Destroy data
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        new public  void Dispose() {
            Dispose(true);
            //this.ReleaseRichEditOleInterface();
            GC.SuppressFinalize(this);

        }

        



        #endregion


        protected override void OnDragEnter(DragEventArgs drgevent) {
            //drgevent.Effect = DragDropEffects.Link;
            base.OnDragEnter(drgevent);
        }
        protected override void OnDragDrop(DragEventArgs drgevent) {
            //if (!Meta.HasOleNotes()) return;
            base.OnDragDrop(drgevent);
        }
        protected override void OnLinkClicked(LinkClickedEventArgs e) {
            base.OnLinkClicked(e);
            Process.Start(e.LinkText);
        }

    }

}
