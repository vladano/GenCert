using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GenCert
{
    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void ChangeTheme(object sender)
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);
            AppTheme appTheme = ThemeManager.GetAppTheme(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
            SaveTheme(theme.Item2, appTheme);
        }
    }
}
