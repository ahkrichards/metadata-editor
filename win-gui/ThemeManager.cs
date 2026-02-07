using System;
using System.Drawing;
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
         else if (control is TextBoxBase || control is ComboBox || control is ListBox)
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
            button.BackColor = DarkSurface;
            button.ForeColor = DarkText;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = DarkBorder;
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
         else if (control is TextBoxBase || control is ComboBox || control is ListBox)
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
   }
}
