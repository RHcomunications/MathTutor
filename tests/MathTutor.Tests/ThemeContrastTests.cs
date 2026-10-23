using System.Globalization;
using System.Text.RegularExpressions;

namespace MathTutor.Tests;

public class ThemeContrastTests
{
    // Texto base de lectura (párrafos y texto secundario) se exige WCAG AAA (7:1,
    // 2.2 H. 64 AAA). Los pares OnAccent/OnSuccess son texto grueso sobre botones
    // y chips de color, que conservan el mínimo AA grande (4.5:1).
    [Theory]
    [InlineData("DefaultPlayful", "AppTextPrimaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("DefaultPlayful", "AppTextPrimaryBrush", "AppCardBrush", 7.0)]
    [InlineData("DefaultPlayful", "AppTextSecondaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("DefaultPlayful", "AppOnAccentBrush", "AppAccentBrush", 4.5)]
    [InlineData("DefaultPlayful", "AppOnSuccessBrush", "AppSuccessBrush", 4.5)]
    [InlineData("DefaultPlayful", "AppErrorBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("DefaultPlayful", "AppBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("DefaultPlayful", "AppFocusBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("HighContrastDark", "AppTextPrimaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("HighContrastDark", "AppTextPrimaryBrush", "AppCardBrush", 7.0)]
    [InlineData("HighContrastDark", "AppTextSecondaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("HighContrastDark", "AppOnAccentBrush", "AppAccentBrush", 4.5)]
    [InlineData("HighContrastDark", "AppOnSuccessBrush", "AppSuccessBrush", 4.5)]
    [InlineData("HighContrastDark", "AppErrorBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("HighContrastDark", "AppBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("HighContrastDark", "AppFocusBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("HighContrastLight", "AppTextPrimaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("HighContrastLight", "AppTextPrimaryBrush", "AppCardBrush", 7.0)]
    [InlineData("HighContrastLight", "AppTextSecondaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("HighContrastLight", "AppOnAccentBrush", "AppAccentBrush", 4.5)]
    [InlineData("HighContrastLight", "AppOnSuccessBrush", "AppSuccessBrush", 4.5)]
    [InlineData("HighContrastLight", "AppErrorBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("HighContrastLight", "AppBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("HighContrastLight", "AppFocusBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("LowSensory", "AppTextPrimaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("LowSensory", "AppTextPrimaryBrush", "AppCardBrush", 7.0)]
    [InlineData("LowSensory", "AppTextSecondaryBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("LowSensory", "AppOnAccentBrush", "AppAccentBrush", 4.5)]
    [InlineData("LowSensory", "AppOnSuccessBrush", "AppSuccessBrush", 4.5)]
    [InlineData("LowSensory", "AppErrorBrush", "AppBackgroundBrush", 7.0)]
    [InlineData("LowSensory", "AppBorderBrush", "AppBackgroundBrush", 3.0)]
    [InlineData("LowSensory", "AppFocusBorderBrush", "AppBackgroundBrush", 3.0)]
    public void ThemeColors_MeetWcagContrast(string theme, string foreground, string background, double minimum)
    {
        var colors = LoadThemeColors(theme);

        Assert.True(colors.ContainsKey(foreground), $"Falta el color {foreground} en {theme}.");
        Assert.True(colors.ContainsKey(background), $"Falta el color {background} en {theme}.");

        double ratio = ContrastRatio(colors[foreground], colors[background]);

        Assert.True(
            ratio >= minimum,
            $"{theme}: {foreground} sobre {background} tiene contraste {ratio:0.00}:1; se requiere {minimum:0.0}:1.");
    }

    private static Dictionary<string, (int R, int G, int B)> LoadThemeColors(string theme)
    {
        string source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "MathTutor.WinUI", "Services", "AccessibilityManager.cs"));
        var block = Regex.Match(
            source,
            $@"case HighContrastTheme\.{Regex.Escape(theme)}:[\s\S]*?break;");

        Assert.True(block.Success, $"No se encontró el bloque del tema {theme}.");

        var colors = new Dictionary<string, (int R, int G, int B)>();
        foreach (Match match in Regex.Matches(
            block.Value,
            @"SetResource\(""(?<name>\w+)"", new SolidColorBrush\(Rgb\((?<r>\d+), (?<g>\d+), (?<b>\d+)\)\)\);"))
        {
            colors[match.Groups["name"].Value] = (
                int.Parse(match.Groups["r"].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups["g"].Value, CultureInfo.InvariantCulture),
                int.Parse(match.Groups["b"].Value, CultureInfo.InvariantCulture));
        }

        return colors;
    }

    private static string FindRepositoryRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "src", "MathTutor.WinUI", "Services", "AccessibilityManager.cs")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("No se encontró la raíz del repositorio.");
    }

    private static double ContrastRatio((int R, int G, int B) first, (int R, int G, int B) second)
    {
        double firstLuminance = RelativeLuminance(first);
        double secondLuminance = RelativeLuminance(second);
        double lighter = Math.Max(firstLuminance, secondLuminance);
        double darker = Math.Min(firstLuminance, secondLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance((int R, int G, int B) color) =>
        0.2126 * LinearChannel(color.R) +
        0.7152 * LinearChannel(color.G) +
        0.0722 * LinearChannel(color.B);

    private static double LinearChannel(int channel)
    {
        double value = channel / 255.0;
        return value <= 0.03928
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
