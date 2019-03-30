using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using GenCert.Forms;
using System.IO;
using Dragablz;

namespace GenCert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //novo
        private readonly MainWindowViewModel _viewModel;

        private bool _shutdown;
        internal static bool isAdmin = false;
        internal static bool isTest = false;

        internal static string appVersion = "1.5.2.0";
        #region version description
        // 1.0 - inicijalna verzija sa dve opcije menija
        // 1.1 - upotrebljiva inicijalna verzija sa dve opcije menija, dokumentacija 
        // 1.2 - dodate dve nove opcije menija
        // 1.3 - ini fajl, friendly name, new user interface, nova dokumentacija
        // 1.4 - nova dokumentacija
        // 1.5 - nova forma za CA chain, koriscenje CA chain na formi za IssuCert i CreatePFX
        //       new class CertData da cuva podatke potrebne kada se koristi Coninue button na formama
        // 1.5.1 - Resen problem sa koriscenjem custom BouncyCastle library. Za citanje request fajla se sada koristi originalni BouncyCastle kod bez ispravki 
        // BouncyCastle:
        // Release 1.8.5, 31st January 2019
        // bccrypto-csharp-1.8.5-bin.zip
        // 1.5.1.1
        // bug fix - add domain name to "Subject Alternative Names" when click button "Gen.Alternative Names" on form CreateRequest
        // change default Signature Algorithm from SHA1WITHRSA to SHA512WITHRSA on all forms
        // 1.5.1.2
        // new feature - add new butons on CA Certificate form
        // 1.5.2.0
        // new feature - add password for crypt generated private key file on form CreateRequest
        // new feature - add password for decrypt private key file on form CreateCertificate
        #endregion

        private static bool _hackyIsFirstWindow = true;
        private static UserControl loadedUserForm = null;

        //LOG4NET - Here is the once-per-class call to initialize the log object
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Help Added
        private DependencyObject CurrentHelpDO { get; set; }
        private Popup CurrentHelpPopup { get; set; }
        private bool HelpActive { get; set; }
        private MouseEventHandler _helpHandler = null;
        static bool isHelpMode = false;
        #endregion

        MainWindowViewModel mwm;

        internal string iniFilePath = System.Environment.CurrentDirectory + "\\config.ini";
        internal string certFriendlyName = "Cert Friendly Name";
        public MainWindow()
        {
            InitializeComponent();

            #region ini
            IniFile ini = new IniFile(iniFilePath);
            if (!File.Exists(iniFilePath))
            {
                ini.IniWriteValue("Configuration", "FriendlyName", "Certificate Friendly Name");
            }
            certFriendlyName = ini.IniReadValue("Configuration", "FriendlyName");
            #endregion

            if (_hackyIsFirstWindow)
            {
                // tooltip to stay on the screen
                ToolTipService.ShowDurationProperty.OverrideMetadata(
                    typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

                //Programmatically change Taskbar icon in WPF
                Application.Current.MainWindow.Icon = IconMaker.Icon(System.Windows.Media.Colors.White);

                #region log4net
                //Log4Net messages
                log.Debug("GenCert started ...");
                log.Info("GenCert started ...");
                log.Warn("GenCert started ...");
                log.Error("GenCert started ...");
                log.Fatal("GenCert started ...");
                #endregion

                mainWindow.Height = Properties.Settings.Default.Height;
                mainWindow.Width = Properties.Settings.Default.Width;
                mainWindow.Left = Properties.Settings.Default.Left;
                mainWindow.Top = Properties.Settings.Default.Top;

                mwm = MainWindowViewModel.CreateWithSamples();
                DataContext = mwm;
            }

            _hackyIsFirstWindow = false;
        }

        private async void MetroWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mw = (MainWindow)sender;
            // Close button only close app on main form
            if (mw.DataContext == null)
            {
                return;
            }

            if (e.Cancel) return;
            e.Cancel = !_shutdown; // && _viewModel.QuitConfirmationEnabled;
            if (_shutdown) return;

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };

            MessageDialogResult result = await this.ShowMessageAsync("Quit application?",
                "Sure you want to quit application?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);


            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
            {
                MainWindowViewModel.SaveTheme();
                Application.Current.Shutdown();
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isHelpMode)
            {
                e.Handled = true;
                isHelpMode = false;
                Mouse.OverrideCursor = null;

                if (Help.MyHelpCommand.CanExecute(null, this))
                {
                    Help.MyHelpCommand.Execute(null, this);
                }
            }
        }

        private void winMain_MouseMove(object sender, MouseEventArgs e)
        {
            // You can check the HelpActive property if desired, however 
            // the listener should not be hooked up so this should not be firing
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(((Visual)sender), e.GetPosition(this));

            if (hitTestResult.VisualHit != null && CurrentHelpDO != hitTestResult.VisualHit)
            {
                // Walk up the tree in case a parent element has help defined
                DependencyObject checkHelpDO = hitTestResult.VisualHit;
                string helpText = AutomationProperties.GetHelpText(checkHelpDO);

                while (String.IsNullOrEmpty(helpText) && checkHelpDO != null && checkHelpDO != mainWindow)
                {
                    checkHelpDO = VisualTreeHelper.GetParent(checkHelpDO);
                    helpText = AutomationProperties.GetHelpText(checkHelpDO);
                }

                if (String.IsNullOrEmpty(helpText) && CurrentHelpPopup != null)
                {
                    CurrentHelpPopup.IsOpen = false;
                    CurrentHelpDO = null;
                }
                else if (!String.IsNullOrEmpty(helpText) && CurrentHelpDO != checkHelpDO)
                {
                    CurrentHelpDO = checkHelpDO;
                    // New visual "stack" hit, close old popup, if any
                    if (CurrentHelpPopup != null)
                    {
                        CurrentHelpPopup.IsOpen = false;
                    }

                    // Obviously you can make the popup look anyway you want it to look 
                    // with any number of options. I chose a simple tooltip look-and-feel.
                    CurrentHelpPopup = new Popup()
                    {
                        //AllowsTransparency = true,
                        PopupAnimation = PopupAnimation.Scroll,
                        PlacementTarget = (UIElement)hitTestResult.VisualHit,
                        IsOpen = true,
                        Child = new System.Windows.Controls.Border()
                        {
                            CornerRadius = new CornerRadius(10),
                            BorderBrush = new SolidColorBrush(Colors.Goldenrod),
                            BorderThickness = new Thickness(2),
                            Background = new SolidColorBrush(Colors.LightYellow),
                            Child = new TextBlock()
                            {
                                Margin = new Thickness(10),
                                Text = helpText.Replace("\\r\\n", "\r\n"),
                                FontSize = 14,
                                FontWeight = FontWeights.Normal
                            }
                        }
                    };
                    CurrentHelpPopup.IsOpen = true;
                }
            }
        }

        private void btnHelp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isHelpMode)
            {
                isHelpMode = false;
                Mouse.OverrideCursor = null;
            }
        }

        private async void About(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Generate Request and Certificate PKI Application About",
                "Generate Request and Certificate PKI Application\n\n" +
                "Version :" + appVersion + "\n" +
                "Author : Vladan Obradovic",
                MessageDialogStyle.Affirmative);
        }

        private void LaunchHelp(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(null, "GenCert.chm");
        }

        private void winMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                ToggleHelp();
            }
            else if (Keyboard.IsKeyDown(Key.Q) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                ExitMenuItem_Click(null, null);

                isHelpMode = false;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.R) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                GenerateCertRequest_Click(null, null);

                isHelpMode = false;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.C) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                GenerateCreateSignCert_Click(null, null);

                isHelpMode = false;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
        }

        private void GenerateCertRequest_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.DataContext = new CreateRequest();
        }

        private void GenerateCreateSignCert_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.DataContext = new CreateRequest();
        }

        private void ToggleHelp()
        {
            // Turn the current help off
            CurrentHelpDO = null;
            if (CurrentHelpPopup != null)
            {
                CurrentHelpPopup.IsOpen = false;
            }

            // Toggle current state; add/remove mouse handler
            HelpActive = !HelpActive;

            if (_helpHandler == null)
            {
                _helpHandler = new MouseEventHandler(winMain_MouseMove);
            }

            if (HelpActive)
            {
                mainWindow.MouseMove += _helpHandler;
            }
            else
            {
                mainWindow.MouseMove -= _helpHandler;
            }

            // Start recursive toggle at visual root
            ToggleHelp(mainWindow);
        }

        private void ToggleHelp(DependencyObject dependObj)
        {
            // Continue recursive toggle. Using the VisualTreeHelper works nicely.
            for (int x = 0; x < VisualTreeHelper.GetChildrenCount(dependObj); x++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependObj, x);
                ToggleHelp(child);
            }

            // BitmapEffect is defined on UIElement so our DependencyObject 
            // must be a UIElement also
            if (dependObj is UIElement)
            {
                UIElement element = (UIElement)dependObj;

                if (HelpActive)
                {
                    string helpText = AutomationProperties.GetHelpText(element);

                    if (!String.IsNullOrEmpty(helpText))
                    {
                    }
                }
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AppQuit();
        }

        public async void AppQuit()
        {
            var settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
            };
            MetroWindow mw = (MetroWindow)App.Current.MainWindow;
            var result = await mw.ShowMessageAsync("Quit application?", "Sure you want to quit application?",
                MessageDialogStyle.AffirmativeAndNegative, settings);

            if (result == MessageDialogResult.Affirmative)
            {
                MainWindowViewModel.SaveTheme();
                Application.Current.Shutdown();
            }
        }

        internal void GetTabablzData(out string header0, out IEnumerable<TabablzControl> tctrl)
        {
            MetroWindow wnd = (MetroWindow)App.Current.MainWindow;
            TabablzControl tc = (TabablzControl)wnd.FindName("InitialTabablzControl");

            TabContent itc0 = (TabContent)tc.SelectedItem;
            header0 = itc0.Header;
            tctrl = TabablzControl.GetLoadedInstances();
        }

        internal void AddTabablzData(string header0, IEnumerable<TabablzControl> tctrl, TabContent tc1)
        {
            TabablzControl lastTabablzControl = tctrl.Last();

            // adds a new tab after the last right tab
            IEnumerable<DragablzItem> orderedDragablzItem = lastTabablzControl.GetOrderedHeaders();
            DragablzItem lastTab = orderedDragablzItem.Last();
            TabablzControl.AddItem(tc1, lastTab.DataContext, AddLocationHint.After);

            TabablzControl.SelectItem(tc1);
        }

        internal void OpenCreateReguestForm_Click(object sender, RoutedEventArgs e)
        {
            string header0= "Create Request";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Create Request";

            CreateRequest gr = new CreateRequest();
            if (loadedUserForm==null || loadedUserForm!=gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        internal void OpenSignCertForm_Click(object sender, RoutedEventArgs e)
        {
            string header0= "Create Certificate";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Create Certificate";

            CreatePFX gr = new CreatePFX();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        internal void OpenSelfSignCertForm_Click(object sender, RoutedEventArgs e)
        {
            string header0= "Create SelfSign Cert.";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Create SelfSign Cert.";

            CreateSelfSign gr = new CreateSelfSign();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        internal void OpenSignRequestForm_Click(object sender, RoutedEventArgs e)
        {
            string header0= "Issue Certificate";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Issue Certificate";

            IssueCert gr = new IssueCert();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        private void CACertificatesForm_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "CA Certificate";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "CA Certificate";

            CreateCA gr = new CreateCA();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }
    }
}
