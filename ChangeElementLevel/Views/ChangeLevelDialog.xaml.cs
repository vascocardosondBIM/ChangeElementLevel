using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ChangeElementLevel.Localization;
using ChangeElementLevel.Services;

namespace ChangeElementLevel.Views;

public partial class ChangeLevelDialog : Window
{
    public Level? SelectedLevel => (LevelsList.SelectedItem as LevelListRow)?.Level;

    public ChangeLevelDialog(Document doc, UIApplication uiApp)
    {
        PluginStrings.Load(uiApp.Application.Language);

        RevitUiTheme.MergeThemeDictionaries(this);
        InitializeComponent();
        RevitUiTheme.ApplyWindowBackground(this);

        Title = PluginStrings.T("dialog.title");
        TitleText.Text = PluginStrings.T("dialog.heading");
        InstructionText.Text = PluginStrings.T("dialog.instructions");
        FootnoteText.Text = PluginStrings.T("dialog.footnote");
        OkButton.Content = PluginStrings.T("dialog.ok");
        CancelButton.Content = PluginStrings.T("dialog.cancel");

        FilteredElementCollector levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level));

        foreach (Level level in levels.Cast<Level>().OrderBy(l => l.Elevation))
        {
            string elev = LevelListRow.FormatElevation(doc, level);
            LevelsList.Items.Add(new LevelListRow(level, elev));
        }

        if (LevelsList.Items.Count > 0)
            LevelsList.SelectedIndex = 0;
    }

    public void SetRevitOwner(UIApplication uiApp)
    {
        IntPtr handle = uiApp.MainWindowHandle;
        if (handle == IntPtr.Zero)
            return;

        _ = new WindowInteropHelper(this) { Owner = handle };
    }

    private void OkClick(object sender, RoutedEventArgs e)
    {
        if (SelectedLevel is null)
        {
            MessageBox.Show(this, PluginStrings.T("dialog.pickLevel"), Title, MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }
}
