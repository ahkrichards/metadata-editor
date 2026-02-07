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
      private static readonly Color DarkText = Color.Gainsboro;

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
            ApplyDarkThemeToToolStripItems(menuStrip.Items);
         }
         else if (control is ToolStrip toolStrip)
         {
            toolStrip.BackColor = DarkSurface;
            toolStrip.ForeColor = DarkText;
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
            ApplyLightThemeToToolStripItems(menuStrip.Items);
         }
         else if (control is ToolStrip toolStrip)
         {
            toolStrip.BackColor = SystemColors.Control;
            toolStrip.ForeColor = SystemColors.ControlText;
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
            item.ForeColor = DarkText;
            if (item is ToolStripDropDownItem dropDownItem && dropDownItem.DropDown is ToolStripDropDownMenu menu)
            {
               menu.BackColor = DarkSurface;
               menu.ForeColor = DarkText;
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
               ApplyLightThemeToToolStripItems(dropDownItem.DropDownItems);
            }
         }
      }
   }
}
