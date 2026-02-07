using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using Synthesia.Properties;

namespace Synthesia
{
   internal enum ThemeMode
   {
      System,
      Light,
      Dark
   }

   internal static class ThemeManager
   {
      private static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
      private static readonly Color DarkSurface = Color.FromArgb(45, 45, 45);
      private static readonly Color DarkBorder = Color.FromArgb(70, 70, 70);
      private static readonly Color DarkHover = Color.FromArgb(60, 60, 60);
      private static readonly Color DarkPressed = Color.FromArgb(80, 80, 80);
      private static readonly Color DarkDisabledText = Color.FromArgb(140, 140, 140);
      private static readonly Color DarkText = Color.Gainsboro;
      private static readonly Color DarkImageMargin = Color.FromArgb(38, 38, 38);
      private static readonly Color DarkInputBorder = Color.FromArgb(120, 120, 120);

      public static ThemeMode GetUserThemeMode()
      {
         if (Enum.TryParse(Settings.Default.ThemeMode, out ThemeMode mode)) return mode;
         return ThemeMode.System;
      }

      public static void SetUserThemeMode(ThemeMode mode)
      {
         Settings.Default.ThemeMode = mode.ToString();
         Settings.Default.Save();
      }

      public static ThemeMode GetEffectiveThemeMode(ThemeMode userMode)
      {
         if (userMode == ThemeMode.System) return IsSystemDarkMode() ? ThemeMode.Dark : ThemeMode.Light;
         return userMode;
      }

      public static Color GetPrimaryTextColor()
      {
         ThemeMode mode = GetEffectiveThemeMode(GetUserThemeMode());
         return mode == ThemeMode.Dark ? DarkText : SystemColors.ControlText;
      }

      public static Color GetMutedTextColor()
      {
         ThemeMode mode = GetEffectiveThemeMode(GetUserThemeMode());
         return mode == ThemeMode.Dark ? DarkDisabledText : SystemColors.GrayText;
      }

      public static bool IsSystemDarkMode()
      {
         try
         {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
               object value = key?.GetValue("AppsUseLightTheme");
               if (value is int intValue) return intValue == 0;
            }
         }
         catch
         {
         }

         return false;
      }

      public static void ApplyThemeFromSettings(Control root)
      {
         ThemeMode userMode = GetUserThemeMode();
         ThemeMode mode = GetEffectiveThemeMode(userMode);
         ApplyTheme(root, mode);
      }

      public static void ApplyTheme(Control root, ThemeMode mode)
      {
         if (root == null) return;

         root.SuspendLayout();
         if (root is Form form)
         {
            ApplyWindowChromeTheme(form, mode);
         }
         if (mode == ThemeMode.Dark)
         {
            ApplyDarkTheme(root);
         }
         else
         {
            ApplyLightTheme(root);
         }
         root.ResumeLayout(true);
      }

      private static void ApplyWindowChromeTheme(Form form, ThemeMode mode)
      {
         if (!IsWindows10OrGreater()) return;

         try
         {
            int useDark = mode == ThemeMode.Dark ? 1 : 0;
            // Prefer the newer attribute on newer Windows, fallback to the older one.
            if (DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int)) != 0)
            {
               DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDark, sizeof(int));
            }
         }
         catch
         {
         }
      }

      private static bool IsWindows10OrGreater()
      {
         try
         {
            Version version = Environment.OSVersion.Version;
            return version.Major >= 10;
         }
         catch
         {
            return false;
         }
      }

      private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
      private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

      [DllImport("dwmapi.dll")]
      private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

      private static void ApplyDarkTheme(Control root)
      {
         ApplyDarkThemeToControl(root);
      }

      private static void ApplyLightTheme(Control root)
      {
         ApplyLightThemeToControl(root);
      }

      private static void ApplyDarkThemeToControl(Control control)
      {
         if (control is Form)
         {
            control.BackColor = DarkBackground;
            control.ForeColor = DarkText;
         }
         else if (control is GroupBox)
         {
            control.BackColor = DarkBackground;
            control.ForeColor = DarkText;
         }
         else if (control is MenuStrip menuStrip)
         {
            menuStrip.BackColor = DarkSurface;
            menuStrip.ForeColor = DarkText;
            menuStrip.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            ApplyDarkThemeToToolStripItems(menuStrip.Items);
         }
         else if (control is ToolStrip toolStrip)
         {
            toolStrip.BackColor = DarkSurface;
            toolStrip.ForeColor = DarkText;
            toolStrip.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            ApplyDarkThemeToToolStripItems(toolStrip.Items);
         }
         else if (control is TextBoxBase textBox)
         {
            control.BackColor = DarkBackground;
            control.ForeColor = DarkText;
            EnsureBorderWrapper(textBox, DarkInputBorder);
         }
         else if (control is ListBox listBox)
         {
            control.BackColor = DarkBackground;
            control.ForeColor = DarkText;
            EnsureBorderWrapper(listBox, DarkInputBorder);
         }
         else if (control is ComboBox)
         {
            control.BackColor = DarkBackground;
            control.ForeColor = DarkText;
         }
         else if (control is NumericUpDown numericUpDown)
         {
            numericUpDown.BackColor = DarkBackground;
            numericUpDown.ForeColor = DarkText;
         }
         else if (control is TreeView treeView)
         {
            treeView.BackColor = DarkBackground;
            treeView.ForeColor = DarkText;
         }
         else if (control is Button button)
         {
            button.UseVisualStyleBackColor = false;
            button.BackColor = DarkSurface;
            button.ForeColor = button.Enabled ? DarkText : DarkDisabledText;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = DarkBorder;
            button.FlatAppearance.MouseOverBackColor = DarkHover;
            button.FlatAppearance.MouseDownBackColor = DarkPressed;
         }
         else if (control is Label || control is CheckBox || control is RadioButton)
         {
            control.ForeColor = DarkText;
            control.BackColor = DarkBackground;
         }
         else
         {
            control.BackColor = DarkBackground;
            control.ForeColor = DarkText;
         }

         foreach (Control child in control.Controls)
         {
            ApplyDarkThemeToControl(child);
         }
      }

      private static void ApplyLightThemeToControl(Control control)
      {
         if (control is Form)
         {
            control.BackColor = SystemColors.Control;
            control.ForeColor = SystemColors.ControlText;
         }
         else if (control is GroupBox)
         {
            control.BackColor = SystemColors.Control;
            control.ForeColor = SystemColors.ControlText;
         }
         else if (control is MenuStrip menuStrip)
         {
            menuStrip.BackColor = SystemColors.Control;
            menuStrip.ForeColor = SystemColors.ControlText;
            menuStrip.Renderer = null;
            ApplyLightThemeToToolStripItems(menuStrip.Items);
         }
         else if (control is ToolStrip toolStrip)
         {
            toolStrip.BackColor = SystemColors.Control;
            toolStrip.ForeColor = SystemColors.ControlText;
            toolStrip.Renderer = null;
            ApplyLightThemeToToolStripItems(toolStrip.Items);
         }
         else if (control is TextBoxBase textBox)
         {
            control.BackColor = SystemColors.Window;
            control.ForeColor = SystemColors.WindowText;
            EnsureDefaultBorder(textBox);
         }
         else if (control is ListBox listBox)
         {
            control.BackColor = SystemColors.Window;
            control.ForeColor = SystemColors.WindowText;
            EnsureDefaultBorder(listBox);
         }
         else if (control is ComboBox)
         {
            control.BackColor = SystemColors.Window;
            control.ForeColor = SystemColors.WindowText;
         }
         else if (control is NumericUpDown numericUpDown)
         {
            numericUpDown.BackColor = SystemColors.Window;
            numericUpDown.ForeColor = SystemColors.WindowText;
         }
         else if (control is TreeView treeView)
         {
            treeView.BackColor = SystemColors.Window;
            treeView.ForeColor = SystemColors.WindowText;
         }
         else if (control is Button button)
         {
            button.UseVisualStyleBackColor = true;
            button.FlatStyle = FlatStyle.Standard;
            button.ForeColor = SystemColors.ControlText;
         }
         else if (control is Label || control is CheckBox || control is RadioButton)
         {
            control.ForeColor = SystemColors.ControlText;
            control.BackColor = SystemColors.Control;
         }
         else
         {
            control.BackColor = SystemColors.Control;
            control.ForeColor = SystemColors.ControlText;
         }

         foreach (Control child in control.Controls)
         {
            ApplyLightThemeToControl(child);
         }
      }

      private static void ApplyDarkThemeToToolStripItems(ToolStripItemCollection items)
      {
         foreach (ToolStripItem item in items)
         {
            item.ForeColor = item.Enabled ? DarkText : DarkDisabledText;
            if (item is ToolStripDropDownItem dropDownItem && dropDownItem.DropDown is ToolStripDropDownMenu menu)
            {
               menu.BackColor = DarkSurface;
               menu.ForeColor = DarkText;
               menu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
               ApplyDarkThemeToToolStripItems(dropDownItem.DropDownItems);
            }
         }
      }

      private static void ApplyLightThemeToToolStripItems(ToolStripItemCollection items)
      {
         foreach (ToolStripItem item in items)
         {
            item.ForeColor = SystemColors.ControlText;
            if (item is ToolStripDropDownItem dropDownItem && dropDownItem.DropDown is ToolStripDropDownMenu menu)
            {
               menu.BackColor = SystemColors.Control;
               menu.ForeColor = SystemColors.ControlText;
               menu.Renderer = null;
               ApplyLightThemeToToolStripItems(dropDownItem.DropDownItems);
            }
         }
      }

      private sealed class DarkColorTable : ProfessionalColorTable
      {
         public override Color ToolStripBorder => DarkBorder;
         public override Color MenuBorder => DarkBorder;
         public override Color MenuItemBorder => DarkBorder;
         public override Color MenuItemSelected => DarkHover;
         public override Color MenuItemSelectedGradientBegin => DarkHover;
         public override Color MenuItemSelectedGradientEnd => DarkHover;
         public override Color MenuItemPressedGradientBegin => DarkPressed;
         public override Color MenuItemPressedGradientMiddle => DarkPressed;
         public override Color MenuItemPressedGradientEnd => DarkPressed;
         public override Color ToolStripDropDownBackground => DarkBackground;
         public override Color ImageMarginGradientBegin => DarkImageMargin;
         public override Color ImageMarginGradientMiddle => DarkImageMargin;
         public override Color ImageMarginGradientEnd => DarkImageMargin;
         public override Color SeparatorDark => DarkBorder;
         public override Color SeparatorLight => DarkBorder;
         public override Color ToolStripGradientBegin => DarkSurface;
         public override Color ToolStripGradientMiddle => DarkSurface;
         public override Color ToolStripGradientEnd => DarkSurface;
      }

      private sealed class BorderWrapState
      {
         public Control Control { get; }
         public Control Parent { get; }
         public int ChildIndex { get; }
         public DockStyle Dock { get; }
         public AnchorStyles Anchor { get; }
         public Point Location { get; }
         public Size Size { get; }
         public Padding Margin { get; }
         public BorderStyle BorderStyle { get; }

         public BorderWrapState(Control control, Control parent)
         {
            Control = control;
            Parent = parent;
            ChildIndex = parent.Controls.GetChildIndex(control);
            Dock = control.Dock;
            Anchor = control.Anchor;
            Location = control.Location;
            Size = control.Size;
            Margin = control.Margin;
            if (control is TextBoxBase textBox)
            {
               BorderStyle = textBox.BorderStyle;
            }
            else if (control is ListBox listBox)
            {
               BorderStyle = listBox.BorderStyle;
            }
            else
            {
               BorderStyle = BorderStyle.Fixed3D;
            }
         }
      }

      private static void EnsureBorderWrapper(Control control, Color borderColor)
      {
         if (control.Parent is BorderPanel panel && panel.Tag is BorderWrapState)
         {
            panel.BorderColor = borderColor;
            return;
         }

         if (control.Parent == null) return;

         Control parent = control.Parent;
         var state = new BorderWrapState(control, parent);
         var wrapper = new BorderPanel
         {
            BackColor = DarkBackground,
            BorderColor = borderColor,
            Margin = control.Margin,
            Tag = state,
            TabStop = false
         };

         if (control.Dock != DockStyle.None)
         {
            wrapper.Dock = control.Dock;
         }
         else
         {
            wrapper.Anchor = control.Anchor;
            wrapper.Location = control.Location;
            wrapper.Size = control.Size;
         }

         parent.Controls.Add(wrapper);
         parent.Controls.SetChildIndex(wrapper, state.ChildIndex);

         if (control is TextBoxBase textBox)
         {
            textBox.BorderStyle = BorderStyle.None;
         }
         else if (control is ListBox listBox)
         {
            listBox.BorderStyle = BorderStyle.None;
         }

         control.Margin = Padding.Empty;
         control.Dock = DockStyle.None;
         wrapper.Controls.Add(control);
         wrapper.LayoutChild();
      }

      private static void EnsureDefaultBorder(Control control)
      {
         if (control.Parent is BorderPanel panel && panel.Tag is BorderWrapState state)
         {
            Control parent = state.Parent;
            if (parent == null) return;

            panel.Controls.Remove(control);
            parent.Controls.Add(control);
            parent.Controls.SetChildIndex(control, state.ChildIndex);

            control.Dock = state.Dock;
            control.Anchor = state.Anchor;
            control.Location = state.Location;
            control.Size = state.Size;
            control.Margin = state.Margin;
            if (control is TextBoxBase textBox)
            {
               textBox.BorderStyle = state.BorderStyle;
            }
            else if (control is ListBox listBox)
            {
               listBox.BorderStyle = state.BorderStyle;
            }

            panel.Dispose();
         }
      }

      private sealed class BorderPanel : Panel
      {
         [System.ComponentModel.Browsable(false)]
         [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
         public Color BorderColor { get; set; } = DarkInputBorder;

         public BorderPanel()
         {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
         }

         protected override void OnPaint(PaintEventArgs e)
         {
            base.OnPaint(e);
            using (var pen = new Pen(BorderColor))
            {
               var rect = new Rectangle(0, 0, Width - 1, Height - 1);
               e.Graphics.DrawRectangle(pen, rect);
            }
         }

         protected override void OnLayout(LayoutEventArgs levent)
         {
            base.OnLayout(levent);
            LayoutChild();
         }

         public void LayoutChild()
         {
            if (Controls.Count == 0) return;
            Control child = Controls[0];
            child.Location = new Point(1, 1);
            child.Size = new Size(Math.Max(0, Width - 2), Math.Max(0, Height - 2));
         }
      }
   }
}
