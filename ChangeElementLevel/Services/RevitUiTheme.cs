using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.UI;

namespace ChangeElementLevel.Services;

/// <summary>
/// Aplica o tema claro/escuro do Revit à janela WPF (sem opções no add-in).
/// Os dicionários de tema têm de ser fundidos <b>antes</b> de <see cref="System.Windows.FrameworkElement.InitializeComponent"/>,
/// porque o XAML usa <c>StaticResource</c> para estilos definidos nesses dicionários.
/// </summary>
public static class RevitUiTheme
{
    public static bool IsRevitDarkTheme() =>
        UIThemeManager.CurrentTheme == UITheme.Dark;

    /// <summary>Funde o ResourceDictionary do tema na janela (chamar antes de InitializeComponent).</summary>
    public static void MergeThemeDictionaries(Window window)
    {
        bool dark = IsRevitDarkTheme();
        string pack = dark
            ? "pack://application:,,,/ChangeElementLevel;component/Themes/DialogDark.xaml"
            : "pack://application:,,,/ChangeElementLevel;component/Themes/DialogLight.xaml";

        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(pack, UriKind.Absolute)
        });
    }

    /// <summary>Define o fundo da janela a partir do tema (chamar depois de InitializeComponent).</summary>
    public static void ApplyWindowBackground(Window window)
    {
        if (window.TryFindResource("Dlg.WindowBackground") is Brush wb)
            window.Background = wb;
    }

    public static void ApplyToWindow(Window window)
    {
        MergeThemeDictionaries(window);
        ApplyWindowBackground(window);
    }
}
