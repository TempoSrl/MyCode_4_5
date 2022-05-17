using System;
using System.Drawing;

namespace mdl_winform {
    /// <summary>
    /// Class containing all form colors 
    /// </summary>
    public class ColorPalette {
#pragma warning disable 1591
        public Color MainBackColor;
        public Color TabControlHeaderColor;
        public Color MainForeColor;
        public Color TreeBackColor;
        public Color TreeForeColor;
        public Color GboxBorderColor;
        public Color GboxForeColor;
        public Color GridHeaderBackColor;
        public Color GridHeaderForeColor;
        public Color GridSelectionBackColor;
        public Color GridSelectionForeColor;
        public Color GridAlternatingBackColor;
        public Color GridBackColor;
        public Color GridForeColor;
        public Color AutoChooseBackColor;
        public Color DisabledButtonBackColor;
        public Color DisabledButtonForeColor;
        public Color ButtonBackColor;
        public Color ButtonForeColor;
        public Color TextBoxEditingForeColor;
        public Color TextBoxEditingBackColor;
        public Color TextBoxNormalForeColor;
        public Color TextBoxNormalBackColor;
        public Color TextBoxReadOnlyForeColor;
        public Color TextBoxReadOnlyBackColor;
        public Color GridButtonBackColor;
        public Color GridButtonForeColor;
        public Color GridBackgroundColor;
        public Color MainFormBackColor;


        /// <summary>
        /// Default constructor
        /// </summary>
        public ColorPalette() {
        }

        /// <summary>
        /// Creates a default palette
        /// </summary>
        /// <param name="paletteDefaultName"></param>
        public ColorPalette(string paletteDefaultName) {
            if (paletteDefaultName == "modern") {
                MainBackColor = Color.Azure;
                TabControlHeaderColor = MainBackColor;
                MainForeColor = Color.MidnightBlue;
                TreeBackColor = Color.White;
                TreeForeColor = Color.MidnightBlue;
                GboxForeColor = MainForeColor;
                GboxBorderColor = Color.SteelBlue;
                GridHeaderBackColor = Color.LightSteelBlue;
                GridHeaderForeColor = MainForeColor;
                GridSelectionBackColor = Color.Yellow;
                GridSelectionForeColor = Color.Black;
                GridAlternatingBackColor = Color.LightCyan;
                GridBackColor = Color.White;
                GridForeColor = Color.Black;
                AutoChooseBackColor = Color.MistyRose;
                ButtonBackColor =  GridHeaderBackColor;
                ButtonForeColor = Color.Navy;
                TextBoxEditingForeColor = Color.Black;
                TextBoxEditingBackColor = Color.Khaki;
                TextBoxNormalForeColor = Color.MidnightBlue;
                TextBoxNormalBackColor = Color.White;
                TextBoxReadOnlyForeColor = Color.Black;
                TextBoxReadOnlyBackColor = Color.Lavender;
                GridButtonBackColor = ButtonBackColor;
                GridButtonForeColor = ButtonForeColor;
                DisabledButtonBackColor = TextBoxReadOnlyBackColor;
                DisabledButtonForeColor = Color.Navy;
                GridBackgroundColor = MainBackColor;
                MainFormBackColor = Color.LightSteelBlue;
            }

            if (paletteDefaultName == "old") {
                MainBackColor = System.Drawing.SystemColors.Control;
                TabControlHeaderColor = System.Drawing.SystemColors.Control;
                MainForeColor = System.Drawing.SystemColors.ControlText;
                TreeBackColor = Color.White;
                TreeForeColor = System.Drawing.SystemColors.ControlText;
                GboxForeColor = MainForeColor;
                GboxBorderColor = System.Drawing.SystemColors.ActiveBorder;
                GridHeaderForeColor = System.Drawing.SystemColors.ControlText;
                GridSelectionBackColor = System.Drawing.SystemColors.ActiveCaption;
                GridSelectionForeColor = System.Drawing.SystemColors.ActiveCaptionText;
                GridAlternatingBackColor = System.Drawing.Color.FromArgb(0xff, 0xCC, 0xCC);
                GridBackColor = System.Drawing.SystemColors.Window;
                GridForeColor = System.Drawing.SystemColors.WindowText;
                AutoChooseBackColor = System.Drawing.Color.FromArgb(0x99, 0xcc, 0xcc);
                ButtonBackColor = System.Drawing.SystemColors.Control;//System.Drawing.Color.RosyBrown;
                ButtonForeColor = System.Drawing.SystemColors.HotTrack; //.FromArgb(0x33,0x33,0x99); //Blue
                TextBoxEditingForeColor = System.Drawing.Color.FromArgb(0, 0, 0); //Blue		
                TextBoxEditingBackColor = System.Drawing.Color.FromArgb(0xff, 0xff, 0x66);
                TextBoxNormalForeColor = System.Drawing.SystemColors.WindowText;
                TextBoxNormalBackColor = System.Drawing.SystemColors.Window;
                TextBoxReadOnlyForeColor = System.Drawing.SystemColors.WindowText;
                TextBoxReadOnlyBackColor = System.Drawing.SystemColors.Control;
                GridButtonBackColor = System.Drawing.Color.FromArgb(0xff, 0x99, 0x66);
                GridHeaderBackColor = GridButtonBackColor;
                GridButtonForeColor = System.Drawing.Color.FromArgb(0, 0, 0); //Black
                DisabledButtonBackColor = System.Drawing.SystemColors.Control; //come normale
                DisabledButtonForeColor = System.Drawing.SystemColors.GrayText;
                GridBackgroundColor = System.Drawing.SystemColors.AppWorkspace;
                MainFormBackColor = System.Drawing.SystemColors.AppWorkspace;
            }
        }

        /// <summary>
        /// Copy data from another Palette
        /// </summary>
        /// <param name="P"></param>
        public void SetTo(ColorPalette P) {
            MainBackColor = P.MainBackColor;
            TabControlHeaderColor = P.TabControlHeaderColor;
            MainForeColor = P.MainForeColor;
            TreeBackColor = P.TreeBackColor;
            TreeForeColor = P.TreeForeColor;
            GboxForeColor = P.GboxForeColor;
            GboxBorderColor = P.GboxBorderColor;
            GridHeaderBackColor = P.GridHeaderBackColor;
            GridHeaderForeColor = P.GridHeaderForeColor;
            GridSelectionBackColor = P.GridSelectionBackColor;
            GridSelectionForeColor = P.GridSelectionForeColor;
            GridAlternatingBackColor = P.GridAlternatingBackColor;
            GridBackColor = P.GridBackColor;
            GridForeColor = P.GridForeColor;
            AutoChooseBackColor = P.AutoChooseBackColor;
            ButtonBackColor = P.ButtonBackColor;
            ButtonForeColor = P.ButtonForeColor;
            TextBoxEditingForeColor = P.TextBoxEditingForeColor;
            TextBoxEditingBackColor = P.TextBoxEditingBackColor;
            TextBoxNormalForeColor = P.TextBoxNormalForeColor;
            TextBoxNormalBackColor = P.TextBoxNormalBackColor;
            TextBoxReadOnlyForeColor = P.TextBoxReadOnlyForeColor;
            TextBoxReadOnlyBackColor = P.TextBoxReadOnlyBackColor;
            GridButtonBackColor = P.GridButtonBackColor;
            GridButtonForeColor = P.GridButtonForeColor;
            DisabledButtonBackColor = P.DisabledButtonBackColor;
            DisabledButtonForeColor = P.DisabledButtonForeColor;
            GridBackgroundColor = P.GridBackgroundColor;
            MainFormBackColor = P.MainFormBackColor;

        }


    }


    /// <summary>
    /// Get color for application components
    /// </summary>
    /// 
    public class formcolors {
       


        /// <summary>
        /// Standard palette for metadata
        /// </summary>
        public static ColorPalette metaPalette= new ColorPalette("modern");

        
        /// <summary>
        /// Default constructor
        /// </summary>
        public formcolors() {
            //
            // TODO: Add constructor logic here
            //
        }
        public static Color MainBackColor() {
            return metaPalette.MainBackColor;
            ;
        }
        public static Color TabControlHeaderColor() {
            return metaPalette.TabControlHeaderColor;
        }
        public static Color MainForeColor() {
            return metaPalette.MainForeColor;
        }

        public static Color TreeBackColor() {
            return metaPalette.TreeBackColor;
            ;
        }
        public static Color TreeForeColor() {
            return metaPalette.TreeForeColor;
        }
        public static Color GboxBorderColor() {
            return metaPalette.GboxBorderColor;
        }
        public static Color GboxForeColor() {
            return metaPalette.GboxForeColor;
        }
        public static Color GridHeaderBackColor() {
            return metaPalette.GridHeaderBackColor;
            //System.Drawing.Color.FromArgb(0xff,0x99,0x66);

        }
        public static Color GridHeaderForeColor() {
            return metaPalette.GridHeaderForeColor;
        }

        public static Color GridSelectionBackColor() {
            return metaPalette.GridSelectionBackColor;
        }
        public static Color GridSelectionForeColor() {
            return metaPalette.GridSelectionForeColor;
        }

        public static Color GridAlternatingBackColor() {
            return metaPalette.GridAlternatingBackColor;
        }

        public static Color GridBackColor() {
            return metaPalette.GridBackColor;
        }
        public static Color GridForeColor() {
            return metaPalette.GridForeColor;
        }

        public static Color AutoChooseBackColor() {
            return metaPalette.AutoChooseBackColor; //            System.Drawing.Color.FromArgb(0x99, 0xcc, 0xcc);
        }

        public static Color DisabledButtonBackColor() {
            return metaPalette.DisabledButtonBackColor; //            System.Drawing.SystemColors.Control;//System.Drawing.Color.RosyBrown;
        }
        public static Color DisabledButtonForeColor() {
            return metaPalette.DisabledButtonForeColor;//            System.Drawing.SystemColors.HotTrack; //.FromArgb(0x33,0x33,0x99); //Blue
        }


        public static Color ButtonBackColor() {
            return metaPalette.ButtonBackColor;//            System.Drawing.SystemColors.Control;//System.Drawing.Color.RosyBrown;
        }
        public static Color ButtonForeColor() {
            return metaPalette.ButtonForeColor; //            System.Drawing.SystemColors.HotTrack; //.FromArgb(0x33,0x33,0x99); //Blue
        }

        public static Color TextBoxEditingForeColor() {
            return metaPalette.TextBoxEditingForeColor; //System.Drawing.Color.FromArgb(0,0,0); //Blue			
            //return System.Drawing.Color.FromArgb(0x33,0x33,0xFF); //Blue
        }
        public static Color TextBoxEditingBackColor() {
            return metaPalette.TextBoxEditingBackColor; // System.Drawing.Color.FromArgb(0xff,0xff,0x66);
            //return System.Drawing.Color.FromArgb(0xff,0xff,0x66);
            //return System.Drawing.Color.FromArgb(0xff,0xff,0xcc);
        }
        public static Color TextBoxNormalForeColor() {
            return metaPalette.TextBoxNormalForeColor;  // System.Drawing.SystemColors.WindowText;
        }
        public static Color TextBoxNormalBackColor() {
            return metaPalette.TextBoxNormalBackColor; //System.Drawing.SystemColors.Window;
        }
        public static Color TextBoxReadOnlyForeColor() {
            return metaPalette.TextBoxReadOnlyForeColor; // System.Drawing.SystemColors.WindowText;
        }
        public static Color TextBoxReadOnlyBackColor() {
            return metaPalette.TextBoxReadOnlyBackColor;//System.Drawing.SystemColors.Window;
        }
        public static Color GridButtonBackColor() {
            return metaPalette.GridButtonBackColor; //            System.Drawing.Color.FromArgb(0xff, 0x99, 0x66);
        }
        public static Color GridButtonForeColor() {
            return metaPalette.GridButtonForeColor; //            System.Drawing.Color.FromArgb(0, 0, 0); //Black
        }
        public static Color GridBackgroundColor() {
            return metaPalette.GridBackgroundColor;
            //Color.CornflowerBlue;//Color.GhostWhite;
        }
        public static Color MainFormBackColor() {
            return metaPalette.MainFormBackColor;
        }

    }
}
