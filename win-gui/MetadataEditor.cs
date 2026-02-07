using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using Microsoft.Win32;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Synthesia
{
   public partial class MetadataEditor : Form, IGuiForm
   {
      private bool _handlingThemeChange;

      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public GuiController c { get; set; }

      public IEnumerable<SongEntry> SelectedSongs => from s in SongList.SelectedItems.Cast<SongEntry>() select s;
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public string WindowTitle { set { Text = value; } }
      public void DeselectAllSongs() { SongList.SelectedIndex = -1; }

      public bool AskYesNo(string message, string title) => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
      public void ShowInfo(string message, string title) { MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information); }
      public void ShowError(string message, string title) { MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error); }
      public void ShowExclamation(string message, string title) { MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }

      public string SaveMetadataFilename() => SaveMetadataDialog.ShowDialog(this) == DialogResult.OK ? SaveMetadataDialog.FileName : null;
      public string OpenMetadataFilename() => OpenMetadataDialog.ShowDialog(this) == DialogResult.OK ? OpenMetadataDialog.FileName : null;
      public string RetargetSongFilename() => RetargetSongDialog.ShowDialog(this) == DialogResult.OK ? RetargetSongDialog.FileName : null;
      public string PickImageFilename() => PickImageDialog.ShowDialog(this) == DialogResult.OK ? PickImageDialog.FileName : null;

      public bool OkayToProceed()
      {
         if (!c.Dirty) return true;

         switch (MessageBox.Show("Would you like to save your changes first?", "Save Changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
         {
            case DialogResult.Cancel: return false;
            case DialogResult.Yes: return c.SaveChanges();
         }
         return true;
      }

      public MetadataEditor(string initialFile)
      {
         InitializeComponent();
         ApplyThemeFromSettings();
         SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
         FormClosed += MetadataEditor_FormClosed;

         // NOTE: This c.set is actually superfluous.  The GuiController sets it for us.
         c = new GuiController(this, initialFile);
      }

      private void MetadataEditor_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (!OkayToProceed()) e.Cancel = true;
      }

      private void MetadataEditor_Load(object sender, EventArgs e)
      {
         Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
      }

      private void MetadataEditor_FormClosed(object sender, FormClosedEventArgs e)
      {
         SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
      }

      private void NewMenu_Click(object sender, EventArgs e) { c.CreateNew(); }
      private void OpenMenu_Click(object sender, EventArgs e) { c.Open(); }
      private void SaveMenu_Click(object sender, EventArgs e) { c.SaveChanges(); }
      private void SaveAsMenu_Click(object sender, EventArgs e) { c.SaveAs(); }
      private void ImportMenu_Click(object sender, EventArgs e) { c.Import(); }
      private void AboutMenu_Click(object sender, EventArgs e) { new About().ShowDialog(); }
      private void ExitMenu_Click(object sender, EventArgs e) { Close(); }

      private void ThemeSystemMenu_Click(object sender, EventArgs e)
      {
         SetThemeMode(ThemeMode.System);
      }

      private void ThemeLightMenu_Click(object sender, EventArgs e)
      {
         SetThemeMode(ThemeMode.Light);
      }

      private void ThemeDarkMenu_Click(object sender, EventArgs e)
      {
         SetThemeMode(ThemeMode.Dark);
      }

      private void RemoveSong_Click(object sender, EventArgs e) { c.RemoveSelectedSongs(); }
      private void SongGrouping_Click(object sender, EventArgs e) { c.Grouping(); }
      private void Md5Update_Click(object sender, EventArgs e) { c.RetargetUniqueId(); }
      private void AddSong_Click(object sender, EventArgs e)
      {
         if (OpenSongDialog.ShowDialog(this) != DialogResult.OK) return;
         c.AddSongs(OpenSongDialog.FileNames);
      }

      private void SongList_SelectedIndexChanged(object sender, EventArgs e) { c.SelectionChanged(); }

      public void RefreshSongList()
      {
         SongList.BeginUpdate();

         var selectedIds = (from s in SelectedSongs select s.UniqueId).ToList();
         SongList.Items.Clear();

         if (c.Metadata != null)
         {
            foreach (SongEntry s in c.Metadata.Songs)
            {
               SongList.Items.Add(s);
               if (selectedIds.Contains(s.UniqueId)) SongList.SetSelected(SongList.Items.Count - 1, true);
            }
         }

         SongList.EndUpdate();
      }

      public void ClearSongControls()
      {
         var primaryText = ThemeManager.GetPrimaryTextColor();
         var mutedText = ThemeManager.GetMutedTextColor();

         UniqueIdBox.Text = "(No song selected)";
         UniqueIdBox.ForeColor = mutedText;
         TitleBox.Clear();
         TitleBox.ForeColor = primaryText;
         SubtitleBox.Clear();
         SubtitleBox.ForeColor = primaryText;

         BackgroundBox.Clear();
         BackgroundBox.ForeColor = primaryText;

         ComposerBox.Clear();
         ComposerBox.ForeColor = primaryText;
         ArrangerBox.Clear();
         ArrangerBox.ForeColor = primaryText;
         CopyrightBox.Clear();
         CopyrightBox.ForeColor = primaryText;
         LicenseBox.Clear();
         LicenseBox.ForeColor = primaryText;
         MadeFamousByBox.Clear();
         MadeFamousByBox.ForeColor = primaryText;

         DifficultyBox.Value = 0;
         DifficultyBox.ForeColor = primaryText;
         RatingBox.Value = 0;
         RatingBox.ForeColor = primaryText;

         FingerHintBox.Clear();
         FingerHintBox.ForeColor = primaryText;
         HandsBox.Clear();
         HandsBox.ForeColor = primaryText;
         PartsBox.ForeColor = primaryText;

         TagBox.Clear();
         TagBox.ForeColor = primaryText;
         TagList.Items.Clear();

         BookmarkMeasureBox.Value = 1;
         BookmarkMeasureBox.ForeColor = primaryText;
         BookmarkDescriptionBox.Clear();
         BookmarkDescriptionBox.ForeColor = primaryText;
         BookmarkList.Items.Clear();

         SetPropertiesEnabled(false);
      }

      private void BindBox(TextBox box, PropertyInfo prop)
      {
         int values = (from e in SelectedSongs select prop.GetValue(e, null) as string).Distinct().Count();

         box.ForeColor = values == 1 ? ThemeManager.GetPrimaryTextColor() : ThemeManager.GetMutedTextColor();
         box.Text = values == 1 ? prop.GetValue(SelectedSongs.First(), null) as string : "(Various)";
      }

      private void BindNumericBox(NumericUpDown box, PropertyInfo prop)
      {
         int values = (from e in SelectedSongs select prop.GetValue(e, null) as int?).Distinct().Count();

         box.ForeColor = values == 1 ? ThemeManager.GetPrimaryTextColor() : ThemeManager.GetMutedTextColor();
         box.Value = values == 1 ? (prop.GetValue(SelectedSongs.First(), null) as int?) ?? 0 : 0;
      }

      public void BindSongControls()
      {
         SetPropertiesEnabled(true);

         BindBox(UniqueIdBox, typeof(SongEntry).GetProperty("UniqueId"));
         BindBox(TitleBox, typeof(SongEntry).GetProperty("Title"));
         BindBox(SubtitleBox, typeof(SongEntry).GetProperty("Subtitle"));

         BindBox(BackgroundBox, typeof(SongEntry).GetProperty("BackgroundImage"));

         BindBox(ComposerBox, typeof(SongEntry).GetProperty("Composer"));
         BindBox(ArrangerBox, typeof(SongEntry).GetProperty("Arranger"));
         BindBox(CopyrightBox, typeof(SongEntry).GetProperty("Copyright"));
         BindBox(LicenseBox, typeof(SongEntry).GetProperty("License"));
         BindBox(MadeFamousByBox, typeof(SongEntry).GetProperty("MadeFamousBy"));

         BindNumericBox(DifficultyBox, typeof(SongEntry).GetProperty("Difficulty"));
         BindNumericBox(RatingBox, typeof(SongEntry).GetProperty("Rating"));

         BindBox(FingerHintBox, typeof(SongEntry).GetProperty("FingerHints"));
         BindBox(HandsBox, typeof(SongEntry).GetProperty("HandParts"));
         BindBox(PartsBox, typeof(SongEntry).GetProperty("Parts"));

         int selectedCount = SongList.SelectedItems.Count;
         SortedDictionary<string, int> tagFrequency = new SortedDictionary<string, int>();
         Dictionary<KeyValuePair<int, string>, int> bookmarkFrequency = new Dictionary<KeyValuePair<int, string>, int>();

         Md5Update.Enabled = selectedCount == 1;

         foreach (SongEntry e in SelectedSongs)
         {
            foreach (string tag in e.Tags) tagFrequency[tag] = tagFrequency.ContainsKey(tag) ? tagFrequency[tag] + 1 : 1;
            foreach (var b in e.Bookmarks) bookmarkFrequency[b] = bookmarkFrequency.ContainsKey(b) ? bookmarkFrequency[b] + 1 : 1;
         }

         TagList.Items.Clear();
         foreach (var tag in tagFrequency) if (tag.Value == selectedCount) TagList.Items.Add(tag.Key);

         BookmarkList.Items.Clear();
         foreach (var b in bookmarkFrequency) if (b.Value == selectedCount) BookmarkList.Items.Add(new Bookmark(b.Key.Key, b.Key.Value));
      }

      private void SetPropertiesEnabled(bool enabled)
      {
         // Keep the group and labels enabled for legible text; disable inputs/actions only.
         PropertiesGroup.Enabled = true;

         // NOTE: Keep the labels readable while still disabling inputs when no song is selected. In WinForms, disabling
         //       the GroupBox disables all child controls and forces them into a “disabled” color which is too dark for
         //       readable contrast.
         UniqueIdBox.Enabled = enabled;
         TitleBox.Enabled = enabled;
         SubtitleBox.Enabled = enabled;
         BackgroundBox.Enabled = enabled;
         BackgroundBrowse.Enabled = enabled;
         ComposerBox.Enabled = enabled;
         ArrangerBox.Enabled = enabled;
         CopyrightBox.Enabled = enabled;
         LicenseBox.Enabled = enabled;
         MadeFamousByBox.Enabled = enabled;
         DifficultyBox.Enabled = enabled;
         RatingBox.Enabled = enabled;
         FingerHintBox.Enabled = enabled;
         PartsBox.Enabled = enabled;
         HandsBox.Enabled = enabled;
         TagBox.Enabled = enabled;
         TagList.Enabled = enabled;
         AddTag.Enabled = enabled && TagBox.Text.Length > 0 && !TagList.Items.Contains(TagBox.Text);
         RemoveTag.Enabled = enabled && TagList.SelectedIndex != -1;
         BookmarkMeasureBox.Enabled = enabled;
         BookmarkDescriptionBox.Enabled = enabled;
         BookmarkList.Enabled = enabled;
         AddBookmark.Enabled = enabled;
         RemoveBookmark.Enabled = enabled && BookmarkList.SelectedIndex != -1;
         Md5Update.Enabled = enabled && SongList.SelectedItems.Count == 1;
      }

      private void AddTag_Click(object sender, EventArgs e)
      {
         c.AddTag(TagBox.Text);
         TagBox.Clear();
      }

      private void RemoveTag_Click(object sender, EventArgs e)
      {
         if (TagList.SelectedItem == null) return;

         var tag = TagList.SelectedItem as string;
         c.RemoveTag(tag);
         TagBox.Text = tag;
      }

      private void TagBox_TextChanged(object sender, EventArgs e)
      {
         if (TagBox.Text.Contains(';')) TagBox.Text = TagBox.Text.Replace(";", "");
         AddTag.Enabled = TagBox.Text.Length > 0 && !TagList.Items.Contains(TagBox.Text);
      }

      private void TagList_SelectedIndexChanged(object sender, EventArgs e)
      {
         RemoveTag.Enabled = TagList.SelectedIndex != -1;
      }

      private void AddBookmark_Click(object sender, EventArgs e)
      {
         c.AddBookmark((int)BookmarkMeasureBox.Value, BookmarkDescriptionBox.Text);
         BookmarkDescriptionBox.Clear();
      }

      private void RemoveBookmark_Click(object sender, EventArgs e)
      {
         if (BookmarkList.SelectedItem == null) return;
         Bookmark b = BookmarkList.SelectedItem as Bookmark;
         c.RemoveBookmark(b.Measure);

         BookmarkMeasureBox.Value = b.Measure;
         BookmarkDescriptionBox.Text = b.Description;
      }

      private void BookmarkDescriptionBox_TextChanged(object sender, EventArgs e)
      {
         if (BookmarkDescriptionBox.Text.Contains(';')) BookmarkDescriptionBox.Text = BookmarkDescriptionBox.Text.Replace(";", "");
      }

      private void BookmarkList_SelectedIndexChanged(object sender, EventArgs e)
      {
         RemoveBookmark.Enabled = BookmarkList.SelectedIndex != -1;
      }

      private void TitleBox_TextChanged(object sender, EventArgs e) { c.TitleChanged(TitleBox.Text); }
      private void SubtitleBox_TextChanged(object sender, EventArgs e) { c.SubtitleChanged(SubtitleBox.Text); }
      private void BackgroundBox_TextChanged(object sender, EventArgs e) { c.BackgroundChanged(BackgroundBox.Text); }
      private void RatingBox_ValueChanged(object sender, EventArgs e) { c.RatingChanged(RatingBox.Value == 0 ? (int?)null : Convert.ToInt32(RatingBox.Value)); }
      private void DifficultyBox_ValueChanged(object sender, EventArgs e) { c.DifficultyChanged(DifficultyBox.Value == 0 ? (int?)null : Convert.ToInt32(DifficultyBox.Value)); }
      private void ComposerBox_TextChanged(object sender, EventArgs e) { c.ComposerChanged(ComposerBox.Text); }
      private void ArrangerBox_TextChanged(object sender, EventArgs e) { c.ArrangerChanged(ArrangerBox.Text); }
      private void CopyrightBox_TextChanged(object sender, EventArgs e) { c.CopyrightChanged(CopyrightBox.Text); }
      private void MadeFamousByBox_TextChanged(object sender, EventArgs e) { c.MadeFamousByChanged(MadeFamousByBox.Text); }
      private void FingerHintBox_TextChanged(object sender, EventArgs e) { c.FingerHintChanged(FingerHintBox.Text); }
      private void LicenseBox_TextChanged(object sender, EventArgs e) { c.LicenseChanged(LicenseBox.Text); }
      private void HandsBox_TextChanged(object sender, EventArgs e) { c.HandsChanged(HandsBox.Text); }
      private void PartsBox_TextChanged(object sender, EventArgs e) { c.PartsChanged(PartsBox.Text); }

      public void UpdateSelectedSongTitle()
      {
         SongList.SelectedIndexChanged -= SongList_SelectedIndexChanged;
         SongList.BeginUpdate();

         List<int> selected = SongList.SelectedIndices.Cast<int>().ToList();
         List<SongEntry> songs = SelectedSongs.ToList();
         if (selected.Count != songs.Count) return;

         SongList.ClearSelected();

         for (int i = 0; i < songs.Count; ++i) SongList.Items[selected[i]] = songs[i];
         foreach (int i in selected) SongList.SetSelected(i, true);

         SongList.EndUpdate();
         SongList.SelectedIndexChanged += SongList_SelectedIndexChanged;
      }

      private void MetadataEditor_DragDrop(object sender, DragEventArgs e)
      {
         if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
         c.DragDropFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
      }

      private void MetadataEditor_DragEnter(object sender, DragEventArgs e)
      {
         if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
         if (c.AllowDragDrop(e.Data.GetData(DataFormats.FileDrop) as string[])) e.Effect = DragDropEffects.All;
      }

      public ImportOptions AskImportOptions()
      {
         using (ImportSelection importDialog = new ImportSelection())
         {
            if (importDialog.ShowDialog(this) != DialogResult.OK) return ImportOptions.Cancel;

            ImportOptions result = 0;
            if (importDialog.ImportFromStandard) result |= ImportOptions.StandardPath;
            if (importDialog.ImportFingerHints) result |= ImportOptions.FingerHints;
            if (importDialog.ImportHandParts) result |= ImportOptions.HandParts;
            if (importDialog.ImportParts) result |= ImportOptions.Parts;
            return result;
         }
      }

      public bool LaunchGroupEditor(MetadataFile m)
      {
         using (GroupEditor editor = new GroupEditor(m))
         {
            editor.ShowDialog(this);
            return editor.MadeChanges;
         }
      }

      private void BackgroundBrowse_Click(object sender, EventArgs e)
      {
         var relative = c.BrowseBackground();
         if (relative != null) BackgroundBox.Text = relative;
      }

      private void ApplyThemeFromSettings()
      {
         ThemeManager.ApplyThemeFromSettings(this);
         UpdateThemeMenuState();
      }

      private void UpdateThemeMenuState()
      {
         ThemeMode userMode = ThemeManager.GetUserThemeMode();
         ThemeSystemMenu.Checked = userMode == ThemeMode.System;
         ThemeLightMenu.Checked = userMode == ThemeMode.Light;
         ThemeDarkMenu.Checked = userMode == ThemeMode.Dark;
      }

      private void SetThemeMode(ThemeMode mode)
      {
         if (_handlingThemeChange) return;

         try
         {
            _handlingThemeChange = true;
            ThemeManager.SetUserThemeMode(mode);
            ApplyThemeFromSettings();
         }
         finally
         {
            _handlingThemeChange = false;
         }
      }

      private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
      {
         if (!IsHandleCreated) return;
         if (ThemeManager.GetUserThemeMode() != ThemeMode.System) return;

         BeginInvoke((Action)(() => ApplyThemeFromSettings()));
      }
   }
}
