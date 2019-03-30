using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using GenCert.Properties;
using Dragablz;

namespace GenCert
{
    public class MainWindowViewModel
    {
        private readonly IInterTabClient _interTabClient;
        private readonly ObservableCollection<TabContent> _tabContents = new ObservableCollection<TabContent>();

        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }
        MainWindowViewModel mwm;

        public MainWindowViewModel()
        {
            _interTabClient = new DefaultInterTabClient();
            LoadTheme();
        }

        public ObservableCollection<TabContent> TabContents
        {
            get { return _tabContents; }
        }
        public IInterTabClient InterTabClient
        {
            get { return _interTabClient; }
        }

        public static Func<object> NewItemFactory
        {
            get { return () => new TabContent("Introduction", new IntroductionPage()); }
        }

        #region Theme
        public void LoadTheme()
        {

            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.Accents
                .Select(
                    a =>
                        new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush })
                .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.AppThemes
                .Select(
                    a =>
                        new AppThemeMenuData()
                        {
                            Name = a.Name,
                            BorderColorBrush = a.Resources["BlackColorBrush"] as Brush,
                            ColorBrush = a.Resources["WhiteColorBrush"] as Brush
                        })
                .ToList();

            AppTheme theme = MahApps.Metro.ThemeManager.AppThemes.FirstOrDefault(t => t.Name.Equals(Properties.Settings.Default.ThemeName));
            Accent accent = MahApps.Metro.ThemeManager.Accents.FirstOrDefault(a => a.Name.Equals(Properties.Settings.Default.AccentName));

            if ((theme != null) && (accent != null))
            {
                MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            }
        }

        public static void SaveTheme()
        {

            Settings.Default.Height = Application.Current.MainWindow.Height;
            Settings.Default.Width = Application.Current.MainWindow.Width;
            Settings.Default.Save();
        }
        #endregion


        public static MainWindowViewModel CreateWithSamples()
        {

            var result = new MainWindowViewModel();

            TabContent tc = new TabContent("Main", new IntroductionPage());
            result.TabContents.Add(tc);

            return result;
        }

    }
}