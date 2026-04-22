using MudBlazor;

namespace Financial_ForeCast.Services
{
    public class ThemeDefinition
    {
        public string Name { get; set; }
        public MudTheme Theme { get; set; }
        public bool IsDark { get; set; }
    }

    public class ThemeService
    {
        private const string ThemePreferenceKey = "SelectedTheme";

        private readonly List<ThemeDefinition> _themes = new();
        private ThemeDefinition _current;

        public event Action OnThemeChanged;

        public IReadOnlyList<ThemeDefinition> AvailableThemes => _themes;
        public ThemeDefinition Current => _current;

        public ThemeService()
        {
            RegisterBuiltInThemes();

            var savedName = Preferences.Get(ThemePreferenceKey, "Dark");
            _current = _themes.FirstOrDefault(t => t.Name == savedName) ?? _themes[0];
        }

        public void SetTheme(string name)
        {
            var theme = _themes.FirstOrDefault(t => t.Name == name);
            if (theme == null) return;

            _current = theme;
            Preferences.Set(ThemePreferenceKey, name);
            OnThemeChanged?.Invoke();
        }

        public void RegisterTheme(ThemeDefinition theme)
        {
            if (_themes.Any(t => t.Name == theme.Name)) return;
            _themes.Add(theme);
        }

        private void RegisterBuiltInThemes()
        {
            _themes.Add(new ThemeDefinition
            {
                Name = "Dark",
                IsDark = true,
                Theme = new MudTheme
                {
                    PaletteDark = new PaletteDark
                    {
                        Primary = "#7c4dff",
                        Secondary = "#00bfa5",
                        AppbarBackground = "#1e1e2f",
                        Background = "#121212",
                        Surface = "#1e1e2f",
                        DrawerBackground = "#1e1e2f",
                        DrawerText = "rgba(255,255,255,0.7)",
                        TextPrimary = "rgba(255,255,255,0.87)",
                        TextSecondary = "rgba(255,255,255,0.6)",
                    }
                }
            });

            _themes.Add(new ThemeDefinition
            {
                Name = "Light",
                IsDark = false,
                Theme = new MudTheme
                {
                    PaletteLight = new PaletteLight
                    {
                        Primary = "#5e35b1",
                        Secondary = "#00897b",
                        AppbarBackground = "#5e35b1",
                        Background = "#f5f5f5",
                        Surface = "#ffffff",
                        DrawerBackground = "#ffffff",
                        DrawerText = "rgba(0,0,0,0.7)",
                        TextPrimary = "rgba(0,0,0,0.87)",
                        TextSecondary = "rgba(0,0,0,0.54)",
                    }
                }
            });

            _themes.Add(new ThemeDefinition
            {
                Name = "Midnight Blue",
                IsDark = true,
                Theme = new MudTheme
                {
                    PaletteDark = new PaletteDark
                    {
                        Primary = "#448aff",
                        Secondary = "#ff6e40",
                        AppbarBackground = "#0d1b2a",
                        Background = "#0d1b2a",
                        Surface = "#1b2838",
                        DrawerBackground = "#0d1b2a",
                        DrawerText = "rgba(255,255,255,0.7)",
                        TextPrimary = "rgba(255,255,255,0.87)",
                        TextSecondary = "rgba(255,255,255,0.6)",
                    }
                }
            });

            _themes.Add(new ThemeDefinition
            {
                Name = "Forest",
                IsDark = true,
                Theme = new MudTheme
                {
                    PaletteDark = new PaletteDark
                    {
                        Primary = "#66bb6a",
                        Secondary = "#ffca28",
                        AppbarBackground = "#1b2e1b",
                        Background = "#1a1a1a",
                        Surface = "#1b2e1b",
                        DrawerBackground = "#1b2e1b",
                        DrawerText = "rgba(255,255,255,0.7)",
                        TextPrimary = "rgba(255,255,255,0.87)",
                        TextSecondary = "rgba(255,255,255,0.6)",
                    }
                }
            });
        }
    }
}
