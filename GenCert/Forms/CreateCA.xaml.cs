using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Collections;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.TeleTrust;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls.Dialogs;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.OpenSsl;
using System.IO;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Security.Certificates;
using Dragablz;
using System.Threading;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509.Extension;

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GenerateRequest.xaml
    /// </summary>
    public partial class CreateCA : UserControl
    {
        // novo
        private readonly IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;

        IDictionary bagAttr = new Hashtable();
        private static readonly X509V1CertificateGenerator v1CertGen = new X509V1CertificateGenerator();
        private static readonly X509V3CertificateGenerator v3CertGen = new X509V3CertificateGenerator();

        double expander1Old = 0;
        double expander2Old = 0;
        double expander3Old = 0;

        private static readonly IDictionary algorithms = Platform.CreateHashtable();

        private static string organization = "";
        private static string domainName = "";
        private static string countryCode = "";
        private static string stateOrProvince = "";
        private static string locality = "";
        private static string password = "";

        public CreateCA()
        {
            InitializeComponent();

            btnCAContinue.IsEnabled = false;
            btnInterContinue.IsEnabled = false;
            btnIssuerContinue.IsEnabled = false;

            if (algorithms.Count == 0)
            {
                #region fill algorithms
                algorithms.Add("MD2WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD2WithRsaEncryption);
                algorithms.Add("MD2WITHRSA", PkcsObjectIdentifiers.MD2WithRsaEncryption);
                algorithms.Add("MD5WITHRSAENCRYPTION", PkcsObjectIdentifiers.MD5WithRsaEncryption);
                algorithms.Add("MD5WITHRSA", PkcsObjectIdentifiers.MD5WithRsaEncryption);
                algorithms.Add("SHA1WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
                algorithms.Add("SHA1WITHRSA", PkcsObjectIdentifiers.Sha1WithRsaEncryption);
                algorithms.Add("SHA224WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
                algorithms.Add("SHA224WITHRSA", PkcsObjectIdentifiers.Sha224WithRsaEncryption);
                algorithms.Add("SHA256WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
                algorithms.Add("SHA256WITHRSA", PkcsObjectIdentifiers.Sha256WithRsaEncryption);
                algorithms.Add("SHA384WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
                algorithms.Add("SHA384WITHRSA", PkcsObjectIdentifiers.Sha384WithRsaEncryption);
                algorithms.Add("SHA512WITHRSAENCRYPTION", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
                algorithms.Add("SHA512WITHRSA", PkcsObjectIdentifiers.Sha512WithRsaEncryption);
                algorithms.Add("SHA1WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
                algorithms.Add("SHA224WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
                algorithms.Add("SHA256WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
                algorithms.Add("SHA384WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
                algorithms.Add("SHA512WITHRSAANDMGF1", PkcsObjectIdentifiers.IdRsassaPss);
                algorithms.Add("RIPEMD160WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
                algorithms.Add("RIPEMD160WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD160);
                algorithms.Add("RIPEMD128WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
                algorithms.Add("RIPEMD128WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD128);
                algorithms.Add("RIPEMD256WITHRSAENCRYPTION", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
                algorithms.Add("RIPEMD256WITHRSA", TeleTrusTObjectIdentifiers.RsaSignatureWithRipeMD256);
                algorithms.Add("SHA1WITHDSA", X9ObjectIdentifiers.IdDsaWithSha1);
                algorithms.Add("DSAWITHSHA1", X9ObjectIdentifiers.IdDsaWithSha1);
                algorithms.Add("SHA224WITHDSA", NistObjectIdentifiers.DsaWithSha224);
                algorithms.Add("SHA256WITHDSA", NistObjectIdentifiers.DsaWithSha256);
                algorithms.Add("SHA384WITHDSA", NistObjectIdentifiers.DsaWithSha384);
                algorithms.Add("SHA512WITHDSA", NistObjectIdentifiers.DsaWithSha512);
                algorithms.Add("SHA1WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha1);
                algorithms.Add("ECDSAWITHSHA1", X9ObjectIdentifiers.ECDsaWithSha1);
                algorithms.Add("SHA224WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha224);
                algorithms.Add("SHA256WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha256);
                algorithms.Add("SHA384WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha384);
                algorithms.Add("SHA512WITHECDSA", X9ObjectIdentifiers.ECDsaWithSha512);
                algorithms.Add("GOST3411WITHGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
                algorithms.Add("GOST3411WITHGOST3410-94", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x94);
                algorithms.Add("GOST3411WITHECGOST3410", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
                algorithms.Add("GOST3411WITHECGOST3410-2001", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
                algorithms.Add("GOST3411WITHGOST3410-2001", CryptoProObjectIdentifiers.GostR3411x94WithGostR3410x2001);
                #endregion
            }

            dpCAStartDate.SelectedDate = DateTime.Now.AddDays(-3);
            dpCAEndDate.SelectedDate = DateTime.Now.AddYears(10);
            dpIssuerStartDate.SelectedDate = DateTime.Now.AddDays(-1);
            dpIssuerEndDate.SelectedDate = DateTime.Now.AddYears(8);
            dpInterStartDate.SelectedDate = DateTime.Now.AddDays(-2);
            dpInterEndDate.SelectedDate = DateTime.Now.AddYears(9);

            cbCASinatuteAlgorithm.ItemsSource = algorithms;
            cbInterSinatuteAlgorithm.ItemsSource = algorithms;
            cbIssuerSinatuteAlgorithm.ItemsSource = algorithms;

            tbCAPathName.Text = System.Environment.CurrentDirectory;
            tbInterPathName.Text = System.Environment.CurrentDirectory;
            tbIssuerPathName.Text = System.Environment.CurrentDirectory;

            expander1Old = exp2.MinWidth;
        }

        private void FillTestData(int CALevel, string organization, string domainName, string countryCode, string stateOrProvince, string locality, string password)
        {
            string currentDir = Directory.GetCurrentDirectory();

            if (CALevel > 2)
            {
                // Issuer CA
                tbIssuerDomainName.Text = organization+" issuerCA."+ domainName;
                cbIssuerSinatuteAlgorithm.Text = "SHA512WITHRSA";
                tbIssuerCountryCode.Text = countryCode;
                tbIssuerStateOrProvince.Text = stateOrProvince;
                tbIssuerLocalityName.Text = locality;
                tbIssuerOrganization.Text = "Issuer CA CO";
                tbIssuerPathName.Text = currentDir + "\\issuerCA"; 
                tbIssuerPublicKeyName.Text = "issuerCA_public";
                tbIssuerSignedCertName.Text = "issuerCA";
                pbIssuerPassword.Password = password;
                tbIssuerFriendlyName.Text = organization+" Issuer CA";
                dpIssuerStartDate.SelectedDate = DateTime.Now.AddDays(-1);
                dpIssuerEndDate.SelectedDate = DateTime.Now.AddYears(8);
                tbIssuerPathName.Text = System.Environment.CurrentDirectory + "\\issuerCA";
            }

            if (CALevel > 1)
            {
                // Intermediate CA
                tbInterDomainName.Text = organization + " intermediateCA." + domainName;
                cbInterSinatuteAlgorithm.Text = "SHA512WITHRSA";
                tbInterCountryCode.Text = countryCode;
                tbInterStateOrProvince.Text = stateOrProvince;
                tbInterLocalityName.Text = locality;
                tbInterOrganization.Text = "Intermediate CA CO";
                tbInterPathName.Text = currentDir + "\\intermediateCA"; 
                tbInterPublicKeyName.Text = "intermediateCA_public";
                tbInterSignedCertName.Text = "intermediateCA";
                pbInterPassword.Password = password;
                tbInterFriendlyName.Text = organization + " Intermediate CA";
                dpInterStartDate.SelectedDate = DateTime.Now.AddDays(-2);
                dpInterEndDate.SelectedDate = DateTime.Now.AddYears(9);
                tbInterPathName.Text = System.Environment.CurrentDirectory + "\\intermediateCA";
            }

            // Master CA
            tbCADomainName.Text = organization + " masterCA." + domainName;
            cbCASinatuteAlgorithm.Text = "SHA512WITHRSA";
            tbCACountryCode.Text = countryCode;
            tbCAStateOrProvince.Text = stateOrProvince;
            tbCALocalityName.Text = locality;
            tbCAOrganization.Text = "Master CA CO";
            tbCAPathName.Text = currentDir + "\\masterCA"; 
            tbCAPublicKeyName.Text = "masterCA_public";
            tbCASignedCertName.Text = "masterCA";
            pbCAPassword.Password = password;
            tbCAFriendlyName.Text = organization + " Master CA";
            dpCAStartDate.SelectedDate = DateTime.Now.AddDays(-3);
            dpCAEndDate.SelectedDate = DateTime.Now.AddYears(10);
            tbCAPathName.Text = System.Environment.CurrentDirectory + "\\masterCA";

        }

        private void CleanData(int CALevel = 1)
        {
            string currentDir = Directory.GetCurrentDirectory();

            if (CALevel > 2)
            {
                // Issuer CA
                tbIssuerDomainName.Text = "";
                cbIssuerSinatuteAlgorithm.Text = "SHA512WITHRSA";
                tbIssuerCountryCode.Text = "";
                tbIssuerStateOrProvince.Text = "";
                tbIssuerLocalityName.Text = "";
                tbIssuerOrganization.Text = "";
                tbIssuerPathName.Text = System.Environment.CurrentDirectory;
                tbIssuerPublicKeyName.Text = "";
                tbIssuerSignedCertName.Text = "";
                pbIssuerPassword.Password = "";
                tbIssuerFriendlyName.Text = "";
                dpIssuerStartDate.SelectedDate = DateTime.Now.AddDays(-1);
                dpIssuerEndDate.SelectedDate = DateTime.Now.AddYears(8);
            }

            if (CALevel > 1)
            {
                // Intermediate CA
                tbInterDomainName.Text = "";
                cbInterSinatuteAlgorithm.Text = "SHA512WITHRSA";
                tbInterCountryCode.Text = "";
                tbInterStateOrProvince.Text = "";
                tbInterLocalityName.Text = "";
                tbInterOrganization.Text = "";
                tbInterPathName.Text = System.Environment.CurrentDirectory;
                tbInterPublicKeyName.Text = "";
                tbInterSignedCertName.Text = "";
                pbInterPassword.Password = "";
                tbInterFriendlyName.Text = "";
                dpInterStartDate.SelectedDate = DateTime.Now.AddDays(-2);
                dpInterEndDate.SelectedDate = DateTime.Now.AddYears(9);
            }

            // Master CA
            tbCADomainName.Text = "";
            cbCASinatuteAlgorithm.Text = "SHA512WITHRSA";
            tbCACountryCode.Text = "";
            tbCAStateOrProvince.Text = "";
            tbCALocalityName.Text = "";
            tbCAOrganization.Text = "";
            tbCAPathName.Text = System.Environment.CurrentDirectory;
            tbCAPublicKeyName.Text = "";
            tbCASignedCertName.Text = "";
            pbCAPassword.Password = "";
            tbCAFriendlyName.Text = "";
            dpCAStartDate.SelectedDate = DateTime.Now.AddDays(-3);
            dpCAEndDate.SelectedDate = DateTime.Now.AddYears(10);

        }

        #region Continue to next form
        /// <summary>
        /// Open wizard what will be next step
        /// If you generate self-sign CA root certificate, next step can be to open form form issue certificate 
        /// base on certificate request file ganerated  using menu option "Create Request"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnCAContinue_Click(object sender, RoutedEventArgs e)
        {
            string certCAFileNamePath = "";
            string certCAPubKeyFileNamePath = "";

            if (tbIssuerPathName.Text!="" && tbIssuerPublicKeyName.Text!="" && tbIssuerSignedCertName.Text!="")
            {
                certCAFileNamePath = tbIssuerPathName.Text + "\\" + tbIssuerSignedCertName.Text + ".pfx";
                certCAPubKeyFileNamePath = tbIssuerPathName.Text + "\\" + tbIssuerPublicKeyName.Text + ".cer";
            }
            else if (tbInterPathName.Text != "" && tbInterPublicKeyName.Text != "" && tbInterSignedCertName.Text != "")
                {
                certCAFileNamePath = tbInterPathName.Text + "\\" + tbInterSignedCertName.Text + ".pfx";
                certCAPubKeyFileNamePath = tbInterPathName.Text + "\\" + tbInterPublicKeyName.Text + ".cer";
            }
            else if (tbCAPathName.Text != "" && tbCAPublicKeyName.Text != "" && tbCASignedCertName.Text != "")
            {
                certCAFileNamePath = tbCAPathName.Text + "\\" + tbCASignedCertName.Text + ".pfx";
                certCAPubKeyFileNamePath = tbCAPathName.Text + "\\" + tbCAPublicKeyName.Text + ".cer";
            }

            MetroWindow metroWin = (MetroWindow)Application.Current.MainWindow;
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Issue certificate base on request",
                NegativeButtonText = "Cancel",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await metroWin.ShowMessageAsync("INFO",
                    "If you wish to generate certificate base on certificate request file (.csr) inside menu option 'Create Request'," + "\n" +
                    "use button 'Issue certificate base on request'." + "\n" +
                    "Generated CA root certificate: " + tbCASignedCertName.Text + "\n" +
                    "will be use to sing certificate requst (.csr) file, generated inside menu option 'Create Request'.",
                    MessageDialogStyle.AffirmativeAndNegative, mySettings);

            if (File.Exists(certCAFileNamePath) && File.Exists(certCAPubKeyFileNamePath))
            {
                if (result == MessageDialogResult.Affirmative)
                {
                    // open menu "Issue Certificate" 
                    MainWindow mw = new MainWindow();

                    string header0 = "Issue Certificate";
                    IEnumerable<TabablzControl> tctrl;
                    mw.GetTabablzData(out header0, out tctrl);
                    header0 = "Issue Certificate";

                    CertData.cerPFXFilePath = certCAFileNamePath;
                    CertData.cerPassword = pbCAPassword.Password;
                    IssueCert gr = new IssueCert();

                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);

                    mw.sbiSelectedMenuOption.Content = header0;
                }
                else if (result == MessageDialogResult.Negative)
                {
                    metroWin = (MetroWindow)Application.Current.MainWindow;
                    mySettings = new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Ok",
                        ColorScheme = MetroDialogColorScheme.Theme
                    };
                    result = await metroWin.ShowMessageAsync("Error",
                        "You must enter generate or entervalid value data to use Continue button",
                        MessageDialogStyle.Affirmative, mySettings);
                }
            }
        }

        private async void btnInterContinue_Click(object sender, RoutedEventArgs e)
        {
            string certCAFileNamePath = tbInterPathName.Text + "\\" + tbInterSignedCertName.Text + ".pfx";
            string certCAPubKeyFileNamePath = tbInterSignedCertName.Text + "\\" + tbInterPublicKeyName.Text + ".cer";

            if (File.Exists(certCAFileNamePath) && File.Exists(certCAPubKeyFileNamePath))
            {
                MetroWindow metroWin = (MetroWindow)Application.Current.MainWindow;
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Issue certificate base on request",
                    NegativeButtonText = "Cancel",
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                MessageDialogResult result = await metroWin.ShowMessageAsync("INFO",
                    "If you wish to generate certificate base on certificate request file (.csr) inside menu option 'Create Request'," + "\n" +
                    "use button 'Issue certificate base on request'." + "\n" +
                    "Generated CA root certificate: " + tbCASignedCertName.Text + "\n" +
                    "will be use to sing certificate requst (.csr) file, generated inside menu option 'Create Request'.",
                    MessageDialogStyle.AffirmativeAndNegative, mySettings);

                if (result == MessageDialogResult.Affirmative)
                {
                    // open menu "Issue Certificate" 
                    MainWindow mw = new MainWindow();

                    string header0 = "Issue Certificate";
                    IEnumerable<TabablzControl> tctrl;
                    mw.GetTabablzData(out header0, out tctrl);
                    header0 = "Issue Certificate";

                    CertData.cerPFXFilePath = certCAFileNamePath;
                    CertData.cerPassword = pbInterPassword.Password;
                    IssueCert gr = new IssueCert();

                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);

                    mw.sbiSelectedMenuOption.Content = header0;

                }
                else if (result == MessageDialogResult.Negative)
                {
                }
            }
        }

        private async void btnIssuerContinue_Click(object sender, RoutedEventArgs e)
        {
            string certCAFileNamePath = tbIssuerPathName.Text + "\\" + tbIssuerSignedCertName.Text + ".pfx";
            string certCAPubKeyFileNamePath = tbIssuerSignedCertName.Text + "\\" + tbIssuerPublicKeyName.Text + ".cer";

            if (File.Exists(certCAFileNamePath) && File.Exists(certCAPubKeyFileNamePath))
            {
                MetroWindow metroWin = (MetroWindow)Application.Current.MainWindow;
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Issue certificate base on request",
                    NegativeButtonText = "Cancel",
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                MessageDialogResult result = await metroWin.ShowMessageAsync("INFO",
                    "If you wish to generate certificate base on certificate request file (.csr) inside menu option 'Create Request'," + "\n" +
                    "use button 'Issue certificate base on request'." + "\n" +
                    "Generated CA root certificate: " + tbCASignedCertName.Text + "\n" +
                    "will be use to sing certificate requst (.csr) file, generated inside menu option 'Create Request'.",
                    MessageDialogStyle.AffirmativeAndNegative, mySettings);

                if (result == MessageDialogResult.Affirmative)
                {
                    // open menu "Issue Certificate" 
                    MainWindow mw = new MainWindow();

                    string header0 = "Issue Certificate";
                    IEnumerable<TabablzControl> tctrl;
                    mw.GetTabablzData(out header0, out tctrl);
                    header0 = "Issue Certificate";
                    CertData.cerPFXFilePath = certCAFileNamePath;
                    CertData.cerPassword = pbIssuerPassword.Password;
                    IssueCert gr = new IssueCert();

                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);

                    mw.sbiSelectedMenuOption.Content = header0;

                }
                else if (result == MessageDialogResult.Negative)
                {
                }
            }
        }
        #endregion

        #region Expander_Collapse
        private void Expander_Expanded1(object sender, RoutedEventArgs e)
        {
            expander1Old = exp1.ActualWidth;
            exp1.MinWidth = 521;
        }

        private void Expander_Collapsed1(object sender, RoutedEventArgs e)
        {
            exp1.MinWidth = expander1Old;
        }

        private void Expander_Expanded2(object sender, RoutedEventArgs e)
        {
            expander2Old = exp2.ActualWidth;
            exp2.MinWidth = 521;
        }

        private void Expander_Collapsed2(object sender, RoutedEventArgs e)
        {
            exp2.MinWidth = expander2Old;
        }

        private void Expander_Expanded3(object sender, RoutedEventArgs e)
        {
            expander3Old = exp3.ActualWidth;
            exp3.MinWidth = 521;
        }

        private void Expander_Collapsed3(object sender, RoutedEventArgs e)
        {
            exp3.MinWidth = expander3Old;
        }
        #endregion Expander_Collapse

        #region Browse button
        /// <summary>
        /// Browse for folder to store generate certificate file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCABrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                dlg.Description = "Select folder to save generate files";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbCAPathName.Text = dlg.SelectedPath;
                }
            }
            catch (Exception)
            {

            }
        }

        private void btnInterBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                dlg.Description = "Select folder to save generate files";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbInterPathName.Text = dlg.SelectedPath;
                }
            }
            catch (Exception)
            {

            }
        }

        private void btnIssuerBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                dlg.Description = "Select folder to save generate files";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbIssuerPathName.Text = dlg.SelectedPath;
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        public static async System.Threading.Tasks.Task<AsymmetricCipherKeyPair> GenerateRsaKeyPair(int keyLength, SecureRandom random)
        {
            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyLength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            AsymmetricCipherKeyPair subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            return subjectKeyPair;
        }

        private async void btnMasterGenerate_Click(object sender, RoutedEventArgs e)
        {
            MetroWindow metroWin = (MetroWindow)Application.Current.MainWindow;
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
                ColorScheme = MetroDialogColorScheme.Theme
            };
            MessageDialogResult result = await metroWin.ShowMessageAsync("INFO-Warning",
                "If you selected \"Key Length\" with a value of 4096, performing this operation can be very demanding" + "\n" +
                "in terms of using CPU resources on this computer and may take a long time, depending on the CPU speed" + "\n" +  
                "of the computer where it is executed. Please be patient....",
                MessageDialogStyle.Affirmative, mySettings);

            btnIssuerContinue.IsEnabled = false;
            btnInterContinue.IsEnabled = false;
            btnCAContinue.IsEnabled = false;

            // clean all output messages
            tbCAOutputMessageBox.Text = "";
            tbInterOutputMessageBox.Text = "";
            tbIssuerOutputMessageBox.Text = "";

            #region intialization
            X509CertificateEntry[] chain = null;

            string commonName = "";
            string signatureAlgorithm = "";
            string countryCode = "";
            string stateOrProvinceName = "";
            string localityName = "";
            string organization = "";
            DateTime startDateTime = new DateTime();
            DateTime endDateTime = new DateTime();
            string friendlyName = "";

            IDictionary attrsMasterCA = null;
            IList ordMasterCA = null;
            IDictionary attrsInterCA = null;
            IList ordInterCA = null;
            IDictionary attrsIssuerCA = null;
            IList ordIssuerCA = null;

            // Check entered data
            int certCANum = ValidateUIData();
            if (certCANum==0)
            {
                return;
            }

            svCreateCA.IsEnabled = false;

            progressring.Visibility = Visibility.Visible;
            await System.Threading.Tasks.TaskEx.Delay(1000);


            chain = new X509CertificateEntry[certCANum];

            ComboBoxItem typeMasterItem = (ComboBoxItem)cbCAKeyLength.SelectedItem;
            int intMasterKeyLength = Convert.ToInt32(typeMasterItem.Content.ToString());
            ComboBoxItem typeInterItem = (ComboBoxItem)cbInterKeyLength.SelectedItem;
            int intInterKeyLength = Convert.ToInt32(typeInterItem.Content.ToString());
            ComboBoxItem typeIssuerItem = (ComboBoxItem)cbIssuerKeyLength.SelectedItem;
            int intIssuerKeyLength = Convert.ToInt32(typeIssuerItem.Content.ToString());
            #endregion

            SecureRandom randomMasterCA = new SecureRandom(); // GetSecureRandom();
            SecureRandom randomInterCA = new SecureRandom(); // GetSecureRandom();
            SecureRandom randomIssuerCA = new SecureRandom(); // GetSecureRandom();

            await System.Threading.Tasks.TaskEx.Delay(2000);
            AsymmetricCipherKeyPair masterCAPair = await GenerateRsaKeyPair(intMasterKeyLength, randomMasterCA);

            await System.Threading.Tasks.TaskEx.Delay(1000);
            AsymmetricCipherKeyPair interCAPair = await GenerateRsaKeyPair(intInterKeyLength, randomInterCA);

            await System.Threading.Tasks.TaskEx.Delay(1000);
            AsymmetricCipherKeyPair issuerCAPair = await GenerateRsaKeyPair(intIssuerKeyLength, randomIssuerCA);

            await System.Threading.Tasks.TaskEx.Delay(1000);

            int currentChainNum = certCANum - 1;

            // Master CA
            #region Get Master data
            commonName = tbCADomainName.Text;
            signatureAlgorithm = cbCASinatuteAlgorithm.Text;
            countryCode = tbCACountryCode.Text;
            stateOrProvinceName = tbCAStateOrProvince.Text;
            localityName = tbCALocalityName.Text;
            organization = tbCAOrganization.Text;
            startDateTime = (DateTime)dpCAStartDate.SelectedDate;
            endDateTime = (DateTime)dpCAEndDate.SelectedDate;
            friendlyName = tbCAFriendlyName.Text;

            attrsMasterCA = new Hashtable();
            attrsMasterCA[X509Name.CN] = commonName;
            attrsMasterCA[X509Name.C] = countryCode;
            attrsMasterCA[X509Name.ST] = stateOrProvinceName;
            attrsMasterCA[X509Name.L] = localityName;
            attrsMasterCA[X509Name.O] = organization;

            ordMasterCA = new ArrayList();
            ordMasterCA.Add(X509Name.CN);
            ordMasterCA.Add(X509Name.C);
            ordMasterCA.Add(X509Name.ST);
            ordMasterCA.Add(X509Name.L);
            ordMasterCA.Add(X509Name.O);

            #endregion Get Master data

            await System.Threading.Tasks.TaskEx.Delay(2000);
            chain[currentChainNum] = CreateMasterCert(
                attrsMasterCA,
                ordMasterCA,
                startDateTime,
                endDateTime,
                signatureAlgorithm,
                masterCAPair.Public,
                masterCAPair.Private,
                friendlyName);
            await System.Threading.Tasks.TaskEx.Delay(2000);


            // Intermediate CA
            if ((certCANum == 3) || (certCANum == 2))
            {
                currentChainNum--;
                #region Get Intermediate data
                commonName = tbInterDomainName.Text;
                signatureAlgorithm = cbInterSinatuteAlgorithm.Text;
                countryCode = tbInterCountryCode.Text;
                stateOrProvinceName = tbInterStateOrProvince.Text;
                localityName = tbInterLocalityName.Text;
                organization = tbInterOrganization.Text;
                startDateTime = (DateTime)dpInterStartDate.SelectedDate;
                endDateTime = (DateTime)dpInterEndDate.SelectedDate;
                friendlyName = tbInterFriendlyName.Text;

                attrsInterCA = new Hashtable();
                attrsInterCA[X509Name.CN] = commonName;
                attrsInterCA[X509Name.C] = countryCode;
                attrsInterCA[X509Name.ST] = stateOrProvinceName;
                attrsInterCA[X509Name.L] = localityName;
                attrsInterCA[X509Name.O] = organization;

                ordInterCA = new ArrayList();
                ordInterCA.Add(X509Name.CN);
                ordInterCA.Add(X509Name.C);
                ordInterCA.Add(X509Name.ST);
                ordInterCA.Add(X509Name.L);
                ordInterCA.Add(X509Name.O);

                #endregion Get Intermediate data

                await System.Threading.Tasks.TaskEx.Delay(2000);
                chain[currentChainNum] = CreateIntermediateCert(
                    attrsInterCA,
                    ordInterCA,
                    startDateTime,
                    endDateTime,
                    signatureAlgorithm,
                    interCAPair.Public,
                    masterCAPair.Private,
                    chain[currentChainNum+1].Certificate, friendlyName);
                await System.Threading.Tasks.TaskEx.Delay(2000);
            }

            // Issuer CA
            if (certCANum == 3)
            {
                currentChainNum--;
                #region Get Issuer data
                commonName = tbIssuerDomainName.Text;
                signatureAlgorithm = cbIssuerSinatuteAlgorithm.Text;
                countryCode = tbIssuerCountryCode.Text;
                stateOrProvinceName = tbIssuerStateOrProvince.Text;
                localityName = tbIssuerLocalityName.Text;
                organization = tbIssuerOrganization.Text;
                startDateTime = (DateTime)dpIssuerStartDate.SelectedDate;
                endDateTime = (DateTime)dpIssuerEndDate.SelectedDate;
                friendlyName = tbIssuerFriendlyName.Text;

                attrsIssuerCA = new Hashtable();
                attrsIssuerCA[X509Name.CN] = commonName;
                attrsIssuerCA[X509Name.C] = countryCode;
                attrsIssuerCA[X509Name.ST] = stateOrProvinceName;
                attrsIssuerCA[X509Name.L] = localityName;
                attrsIssuerCA[X509Name.O] = organization;

                ordIssuerCA = new ArrayList();
                ordIssuerCA.Add(X509Name.CN);
                ordIssuerCA.Add(X509Name.C);
                ordIssuerCA.Add(X509Name.ST);
                ordIssuerCA.Add(X509Name.L);
                ordIssuerCA.Add(X509Name.O);

                #endregion Get Issuer data           

                await System.Threading.Tasks.TaskEx.Delay(2000);
                chain[currentChainNum] = CreateIssuerCert(
                    attrsIssuerCA, 
                    ordIssuerCA, 
                    startDateTime, 
                    endDateTime, 
                    signatureAlgorithm, 
                    issuerCAPair.Public, 
                    interCAPair.Private, 
                    chain[currentChainNum+1].Certificate, 
                    friendlyName);
                await System.Threading.Tasks.TaskEx.Delay(2000);
            }

            IDictionary bagAttr = new Hashtable();

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            string fileName = "";
            string outputPublicKeyName = "";

            if (certCANum == 3)
            {                                                                                       // issuer  intermediate  master
                await System.Threading.Tasks.TaskEx.Delay(2000);
                store.SetKeyEntry("iccuerCA Key", new AsymmetricKeyEntry(issuerCAPair.Private), chain);

                fileName = tbIssuerPathName.Text + "\\" + tbIssuerSignedCertName.Text + ".pfx";
                try
                {
                    using (var filestream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        store.Save(filestream, pbIssuerPassword.Password.ToCharArray(), new SecureRandom());
                    }
                    tbIssuerOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = fileName + " sucessfully created." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    svCreateCA.IsEnabled = true;

                    tbIssuerOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "ERROR to save generated certificate to file: " + fileName + "\n" +
                            ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                    return;
                }

                #region Save Public Key - Issuer CA
                outputPublicKeyName = tbIssuerPathName.Text + "\\" + tbIssuerPublicKeyName.Text;
                try
                {                                                                               // issuer  intermediate  master
                    await System.Threading.Tasks.TaskEx.Delay(2000);
                    File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), chain[0].Certificate.GetEncoded());
                    CertData.cerIssuerPubFilePath = outputPublicKeyName+ ".cer";
                    tbIssuerOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = outputPublicKeyName + " sucessfully created." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    svCreateCA.IsEnabled = true;
                    tbIssuerOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "ERROR to save generated public key to file: " + System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer") + "\n" +
                            ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                    return;
                }
                #endregion

                btnIssuerContinue.IsEnabled = true;
            }

            if ((certCANum == 3) || (certCANum == 2))
            {
                await System.Threading.Tasks.TaskEx.Delay(2000);
                store = new Pkcs12StoreBuilder().Build();
                if (certCANum == 3)
                {                                                                           // issuer  intermediate  master
                    store.SetKeyEntry("intermediateCA Key", new AsymmetricKeyEntry(interCAPair.Private), new[] { chain[1], chain[2] });
                }
                else if (certCANum == 2)
                {                                                                                       // intermediate  master
                    store.SetKeyEntry("intermediateCA Key", new AsymmetricKeyEntry(interCAPair.Private), chain);
                }
                fileName = tbInterPathName.Text + "\\" + tbInterSignedCertName.Text + ".pfx";
                try
                {
                    using (var filestream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        store.Save(filestream, pbInterPassword.Password.ToCharArray(), new SecureRandom());
                    }
                    tbInterOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = fileName+" sucessfully created." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    svCreateCA.IsEnabled = true;
                    tbInterOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "ERROR to save generated certificate to file: " + fileName + "\n" +
                            ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                    return;
                }

                #region Save Public Key - Intermediate CA
                outputPublicKeyName = tbInterPathName.Text + "\\" + tbInterPublicKeyName.Text;
                try
                {
                    await System.Threading.Tasks.TaskEx.Delay(2000);
                    if (certCANum == 3)
                    {                                                                             // issuer  intermediate  master          
                        File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), chain[1].Certificate.GetEncoded());
                    }
                    if (certCANum == 2)
                    {                                                                           //  intermediate  master
                        File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), chain[0].Certificate.GetEncoded());
                    }
                    CertData.cerIntermediatePubFilePath = outputPublicKeyName + ".cer";
                    tbInterOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = outputPublicKeyName + " sucessfully created." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    svCreateCA.IsEnabled = true;
                    tbInterOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "ERROR to save generated public key to file: " + System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer") + "\n" +
                        ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                    return;
                }
                #endregion

                btnInterContinue.IsEnabled = true;
            }

            store = new Pkcs12StoreBuilder().Build();
            if (certCANum == 3)
            {                                                                                       // issuer  intermediate  master
                store.SetKeyEntry("masterCA Key", new AsymmetricKeyEntry(masterCAPair.Private), new[] { chain[2] });
            }
            else if (certCANum == 2)
            {                                                                                       // intermediate  master
                store.SetKeyEntry("masterCA Key", new AsymmetricKeyEntry(masterCAPair.Private), new[] { chain[1] });
            }
            else if (certCANum == 1)
            {                                                                                       //  master
                store.SetKeyEntry("masterCA Key", new AsymmetricKeyEntry(masterCAPair.Private), chain);
            }
            fileName = tbCAPathName.Text + "\\" + tbCASignedCertName.Text+".pfx";
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(2000);
                using (var filestream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    store.Save(filestream, pbCAPassword.Password.ToCharArray(), new SecureRandom());
                }
                tbCAOutputMessageBox.Inlines.Add(new Run
                {
                    Text = fileName + " sucessfully created." + "\n",
                    Foreground = System.Windows.Media.Brushes.Green
                });
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                svCreateCA.IsEnabled = true;
                tbCAOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "ERROR to save generated certificate to file: " + fileName + "\n" +
                        ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });
                return;
            }

            #region Save Public Key - Master CA
            outputPublicKeyName = tbCAPathName.Text + "\\" + tbCAPublicKeyName.Text;
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(2000);
                if (certCANum == 3)
                {                                                                                       // issuer  intermediate  master
                    File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), chain[2].Certificate.GetEncoded());
                }
                else if (certCANum == 2)
                {                                                                                       // intermediate  master
                    File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), chain[1].Certificate.GetEncoded());
                }
                else if (certCANum == 1)
                {                                                                                       // master
                    File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), chain[0].Certificate.GetEncoded());
                }
                CertData.cerMasterPubFilePath = outputPublicKeyName + ".cer";
                tbCAOutputMessageBox.Inlines.Add(new Run
                {
                    Text = outputPublicKeyName + " sucessfully created." + "\n",
                    Foreground = System.Windows.Media.Brushes.Green
                });
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                svCreateCA.IsEnabled = true;
                tbCAOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "ERROR to save generated public key to file: " + System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer") + "\n" +
                        ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });
                return;
            }
            #endregion

            progressring.Visibility = Visibility.Hidden;

            svCreateCA.IsEnabled = true;
            btnCAContinue.IsEnabled = true;
        }

        #region check UI data
        /// <summary>
        /// Validate data on form. Check if data entered on the form are valid.
        /// Each Expander on the form has own set of data
        /// Return number of valid data on form
        /// 3 - masterCA + intermediateCA + issuerCA
        /// 2 - masterCA + intermediateCA
        /// 1 - masterCA
        /// 0 - No valid data
        /// </summary>
        /// <returns></returns>
        private int ValidateUIData()
        {
            int retValue = 0;
            bool isMasterDataOK = true;
            bool isintermediateDataOK = true;
            bool isissuerDataOK = true;

            // This Expander data is MANDATORY 
            List<string> errorMessages = new List<string>();
            CheckMasterCAData(errorMessages);
            if (errorMessages.Count>0)
            {
                isMasterDataOK = false;
            }
            else
            {
                retValue++;
            }
            // This Expander data is OPTIONAL - if common name NOT empty check data
            if (!string.IsNullOrEmpty(tbInterDomainName.Text))
            {
                errorMessages = new List<string>();
                CheckIntermediateCAData(errorMessages);
                if (errorMessages.Count > 0)
                {
                    isintermediateDataOK = false;
                }
                else
                {
                    retValue++;
                }
            }

            // This Expander data is OPTIONAL - if common name NOT empty check data
            if (!string.IsNullOrEmpty(tbIssuerDomainName.Text))
            {
                errorMessages = new List<string>();
                CheckIssuerCAData(errorMessages);
                if (errorMessages.Count > 0)
                {
                    isissuerDataOK = false;
                }
                else
                {
                    retValue++;
                }
            }

            return retValue;
        }

        private async void CheckMasterCAData(List<string> errorMessages)
        {
            #region check errors
            if (string.IsNullOrEmpty(tbCADomainName.Text))
            {
                errorMessages.Add("You MUST enter Common Name.");
            }

            if (string.IsNullOrEmpty(tbCACountryCode.Text))
            {
                errorMessages.Add("You MUST enter Country Code.");
            }
            if (string.IsNullOrEmpty(tbCAStateOrProvince.Text))
            {
                errorMessages.Add("You MUST enter name for State or Province.");
            }
            if (string.IsNullOrEmpty(tbCALocalityName.Text))
            {
                errorMessages.Add("You MUST enter Location name.");
            }
            if (string.IsNullOrEmpty(tbCAOrganization.Text))
            {
                errorMessages.Add("You MUST enter Organization name.");
            }
            if (string.IsNullOrEmpty(tbCAPathName.Text))
            {
                errorMessages.Add("You MUST enter Path to store generate files.");
            }
            else
            {
                if (!Directory.Exists(tbCAPathName.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(tbCAPathName.Text);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add("Can NOT create directory path: " + tbCAPathName.Text);
                    }
                }
            }

            if (string.IsNullOrEmpty(tbCASignedCertName.Text))
            {
                errorMessages.Add("You MUST enter file name to save certificate request (.csr extension).");
            }
            else
            {
                if (File.Exists(tbCAPathName.Text + "\\" + tbCASignedCertName.Text + ".csr"))
                {
                    errorMessages.Add("File " + tbCAPathName.Text + "\\" + tbCASignedCertName.Text + ".csr" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }
            if (string.IsNullOrEmpty(tbCAFriendlyName.Text))
            {
                errorMessages.Add("You MUST friendly name for certificate.");
            }

            if (errorMessages.Count > 0)
            {
                string errorMessage = "";
                for (int i = 0; i < errorMessages.Count; i++)
                {
                    if (i == 0)
                    {
                        errorMessage += errorMessages[i];
                    }
                    else
                    {
                        errorMessage += "\n" + errorMessages[i];
                    }
                }
                tbCAOutputMessageBox.Inlines.Add(new Run
                {
                    Text = errorMessage,
                    Foreground = System.Windows.Media.Brushes.Red
                });

            }
            #endregion
        }

        private async void CheckIntermediateCAData(List<string> errorMessages)
        {
            #region check errors

            if (string.IsNullOrEmpty(tbInterDomainName.Text))
            {
                errorMessages.Add("You MUST enter Domain Name.");
            }

            if (string.IsNullOrEmpty(tbInterCountryCode.Text))
            {
                errorMessages.Add("You MUST enter Country Code.");
            }
            if (string.IsNullOrEmpty(tbInterStateOrProvince.Text))
            {
                errorMessages.Add("You MUST enter name for State or Province.");
            }
            if (string.IsNullOrEmpty(tbInterLocalityName.Text))
            {
                errorMessages.Add("You MUST enter Location name.");
            }
            if (string.IsNullOrEmpty(tbInterOrganization.Text))
            {
                errorMessages.Add("You MUST enter Organization name.");
            }
            if (string.IsNullOrEmpty(tbInterPathName.Text))
            {
                errorMessages.Add("You MUST enter Path to store generate files.");
            }
            else
            {
                if (!Directory.Exists(tbInterPathName.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(tbInterPathName.Text);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add("Can NOT create directory path: " + tbInterPathName.Text);
                    }
                }
            }

            if (string.IsNullOrEmpty(tbInterSignedCertName.Text))
            {
                errorMessages.Add("You MUST enter file name to save certificate request (.csr extension).");
            }
            else
            {
                if (File.Exists(tbInterPathName.Text + "\\" + tbInterSignedCertName.Text + ".csr"))
                {
                    errorMessages.Add("File " + tbInterPathName.Text + "\\" + tbInterSignedCertName.Text + ".csr" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }
            if (string.IsNullOrEmpty(tbInterFriendlyName.Text))
            {
                errorMessages.Add("You MUST friendly name for certificate.");
            }

            if (errorMessages.Count > 0)
            {
                string errorMessage = "";
                for (int i = 0; i < errorMessages.Count; i++)
                {
                    if (i == 0)
                    {
                        errorMessage += errorMessages[i];
                    }
                    else
                    {
                        errorMessage += "\n" + errorMessages[i];
                    }
                }
                tbInterOutputMessageBox.Inlines.Add(new Run
                {
                    Text = errorMessage,
                    Foreground = System.Windows.Media.Brushes.Red
                });
            }
            #endregion
        }

        private async void CheckIssuerCAData(List<string> errorMessages)
        {
            #region check errors

            if (string.IsNullOrEmpty(tbIssuerDomainName.Text))
            {
                errorMessages.Add("You MUST enter Domain Name.");
            }

            if (string.IsNullOrEmpty(tbIssuerCountryCode.Text))
            {
                errorMessages.Add("You MUST enter Country Code.");
            }
            if (string.IsNullOrEmpty(tbIssuerStateOrProvince.Text))
            {
                errorMessages.Add("You MUST enter name for State or Province.");
            }
            if (string.IsNullOrEmpty(tbIssuerLocalityName.Text))
            {
                errorMessages.Add("You MUST enter Location name.");
            }
            if (string.IsNullOrEmpty(tbIssuerOrganization.Text))
            {
                errorMessages.Add("You MUST enter Organization name.");
            }
            if (string.IsNullOrEmpty(tbIssuerPathName.Text))
            {
                errorMessages.Add("You MUST enter Path to store generate files.");
            }
            else
            {
                if (!Directory.Exists(tbIssuerPathName.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(tbIssuerPathName.Text);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add("Can NOT create directory path: " + tbIssuerPathName.Text);
                    }
                }
            }

            if (string.IsNullOrEmpty(tbIssuerSignedCertName.Text))
            {
                errorMessages.Add("You MUST enter file name to save certificate request (.csr extension).");
            }
            else
            {
                if (File.Exists(tbIssuerPathName.Text + "\\" + tbIssuerSignedCertName.Text + ".csr"))
                {
                    errorMessages.Add("File " + tbIssuerPathName.Text + "\\" + tbIssuerSignedCertName.Text + ".csr" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }
            if (string.IsNullOrEmpty(tbIssuerFriendlyName.Text))
            {
                errorMessages.Add("You MUST friendly name for certificate.");
            }

            if (errorMessages.Count > 0)
            {
                string errorMessage = "";
                for (int i = 0; i < errorMessages.Count; i++)
                {
                    if (i == 0)
                    {
                        errorMessage += errorMessages[i];
                    }
                    else
                    {
                        errorMessage += "\n" + errorMessages[i];
                    }
                }
                tbIssuerOutputMessageBox.Inlines.Add(new Run
                {
                    Text = errorMessage,
                    Foreground = System.Windows.Media.Brushes.Red
                });
            }
            #endregion
        }
        #endregion

        private void btnFillTestData_Click(object sender, RoutedEventArgs e)
        {
            bool isContinue = false;

            CA_Parameters mwDialog = new CA_Parameters(organization, domainName, countryCode, stateOrProvince, locality, password);
            isContinue = (bool)mwDialog.ShowDialog();

            if (isContinue)
            {
                Button btnSender = (Button)sender;

                // clean all output messages
                tbCAOutputMessageBox.Text = "";
                tbInterOutputMessageBox.Text = "";
                tbIssuerOutputMessageBox.Text = "";

                btnIssuerContinue.IsEnabled = false;
                btnInterContinue.IsEnabled = false;
                btnCAContinue.IsEnabled = false;

                organization = mwDialog.organization;
                domainName = mwDialog.domainName;
                countryCode = mwDialog.countryCode;
                stateOrProvince = mwDialog.stateOrProvince;
                locality = mwDialog.locality;
                password = mwDialog.password;

                if (btnSender.Name == "btnFillTestDataL3")
                {
                    FillTestData(3, organization, domainName, countryCode, stateOrProvince, locality, password);
                }
                else if (btnSender.Name == "btnFillTestDataL2")
                {
                    FillTestData(2, organization, domainName, countryCode, stateOrProvince, locality, password);
                }
                else
                {
                    FillTestData(1, organization, domainName, countryCode, stateOrProvince, locality, password);
                }

                // Expand all expander UI elements
                List<Expander> expanders = GetLogicalChildCollection<Expander>(CAForm);
                foreach (Expander exp in expanders)
                {
                    exp.IsExpanded = true;
                }
            }
        }

        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        #region copy data
        private void btnCACopyData_Click(object sender, RoutedEventArgs e)
        {
            btnIssuerContinue.IsEnabled = false;
            btnInterContinue.IsEnabled = false;

            // clean all output messages
            tbCAOutputMessageBox.Text = "";
            tbInterOutputMessageBox.Text = "";
            tbIssuerOutputMessageBox.Text = "";

            string currentDir = Directory.GetCurrentDirectory();
            string serverName = "";
            string domainName = "";
            string[] domName;
            if (String.IsNullOrEmpty(tbCADomainName.Text))
            {
                // ERROR:
                // field "Common Naem" is empty
                return;
            }

            if (tbCADomainName.Text.Contains("."))
            {
                domName = tbCADomainName.Text.Split('.');
                serverName = domName[0];
                for (int i = 1; i < domName.Length; i++)
                {
                    domainName += domName[i];
                    if (i < domName.Length)
                    {
                        domainName += ".";
                    }
                }
            }
            else if (tbCADomainName.Text.Contains(" "))
            {
                domName = tbCADomainName.Text.Split(' ');
                serverName = domName[0];
                for (int i = 1; i < domName.Length; i++)
                {
                    domainName += domName[i].ToLower();
                    if (i < domName.Length)
                    {
                        domainName += ".";
                    }
                }
            }
            else
            {
                serverName = "intermediateCA";
                domainName = "test.local";
            }

            // Intermediate CA
            tbInterDomainName.Text = "intermediateCA."+ domainName; // "intermediateCA.test.local";
            tbInterCountryCode.Text = tbCACountryCode.Text; // "RS";
            tbInterStateOrProvince.Text = tbCAStateOrProvince.Text; // "Serbia";
            tbInterLocalityName.Text = tbCALocalityName.Text; // "Novi Sad";
            tbInterOrganization.Text = tbCAOrganization.Text; // "Intermediate CA CO";
            tbInterPathName.Text = currentDir+"\\IntermediateCA"; // @"G:\_PKI\GenCert\GenCert\bin\Debug\intermediateCA";
            //tbInterPrivateKeyName.Text = serverName+ "_private"; // "intermediateCA_private";
            tbInterPublicKeyName.Text = serverName + "_public"; // "intermediateCA_public";
            tbInterSignedCertName.Text = serverName; // "intermediateCA";
            pbInterPassword.Password = "1234";
            tbInterFriendlyName.Text = serverName + " CA"; //"Intermediate CA Friendly name";

            tbCAOutputMessageBox.Inlines.Add(new Run
            {
                Text = "Data from Master CA form sucessfully copied." + "\n",
                Foreground = System.Windows.Media.Brushes.Green
            });
        }

        private void btnInterCopyData_Click(object sender, RoutedEventArgs e)
        {
            btnIssuerContinue.IsEnabled = false;

            // clean all output messages
            tbCAOutputMessageBox.Text = "";
            tbInterOutputMessageBox.Text = "";
            tbIssuerOutputMessageBox.Text = "";

            string currentDir = Directory.GetCurrentDirectory();
            string serverName = "";
            string domainName = "";
            string[] domName;
            if (String.IsNullOrEmpty(tbInterDomainName.Text))
            {
                // ERROR:
                // field "Common Naem" is empty
                return;
            }

            if (tbInterDomainName.Text.Contains("."))
            {
                domName = tbInterDomainName.Text.Split('.');
                serverName = domName[0];
                for (int i = 1; i < domName.Length; i++)
                {
                    domainName += domName[i];
                    if (i < domName.Length)
                    {
                        domainName += ".";
                    }
                }
            }
            else if (tbInterDomainName.Text.Contains(" "))
            {
                domName = tbInterDomainName.Text.Split(' ');
                serverName = domName[0];
                for (int i = 1; i < domName.Length; i++)
                {
                    domainName += domName[i].ToLower();
                    if (i < domName.Length)
                    {
                        domainName += ".";
                    }
                }
            }
            else
            {
                serverName = "issuerCA";
                domainName = "test.local";
            }

            // Issuer CA
            tbIssuerDomainName.Text = "issuerCA." + domainName; 
            tbIssuerCountryCode.Text = tbInterCountryCode.Text; 
            tbIssuerStateOrProvince.Text = tbInterStateOrProvince.Text; 
            tbIssuerLocalityName.Text = tbInterLocalityName.Text; 
            tbIssuerOrganization.Text = tbInterOrganization.Text;  
            tbIssuerPathName.Text = currentDir + "\\IssuerCA"; 
            tbIssuerPublicKeyName.Text = serverName + "_public"; 
            tbIssuerSignedCertName.Text = serverName; 
            pbIssuerPassword.Password = "1234";
            tbIssuerFriendlyName.Text = serverName + " CA"; 

            tbInterOutputMessageBox.Inlines.Add(new Run
            {
                Text = "Data from Intermediate CA form sucessfully copied." + "\n",
                Foreground = System.Windows.Media.Brushes.Green
            });
        }
        #endregion

        #region Create CA certificates
        public static X509CertificateEntry CreateMasterCert(
            IDictionary attrs,
            IList order,
            DateTime startDate,
            DateTime endDate,
            string algorithmName,
            AsymmetricKeyParameter pubKey,
            AsymmetricKeyParameter privKey,
            string friendlyName = null)
        {

            //
            // create the certificate - version 3
            //
            v3CertGen.Reset();

            v3CertGen.AddExtension(
                            X509Extensions.BasicConstraints, true, new BasicConstraints(true));

            v3CertGen.SetSerialNumber(BigInteger.One);
            v3CertGen.SetIssuerDN(new X509Name(order, attrs));
            v3CertGen.SetNotBefore(startDate);
            v3CertGen.SetNotAfter(endDate);
            v3CertGen.SetSubjectDN(new X509Name(order, attrs));
            v3CertGen.SetPublicKey(pubKey);
            v3CertGen.SetSignatureAlgorithm(algorithmName);

            KeyPurposeID[] usages = new[] {
                    KeyPurposeID.IdKPServerAuth,
                    KeyPurposeID.IdKPClientAuth,
                    KeyPurposeID.IdKPCodeSigning,
                    KeyPurposeID.IdKPIpsecEndSystem,
                    KeyPurposeID.IdKPIpsecTunnel,
                    KeyPurposeID.IdKPIpsecUser,
                    KeyPurposeID.IdKPOcspSigning,
                    KeyPurposeID.IdKPTimeStamping
                };

            v3CertGen.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign));

            v3CertGen.AddExtension(
                X509Extensions.ExtendedKeyUsage, false, new ExtendedKeyUsage(usages));

            Org.BouncyCastle.X509.X509Certificate cert = v3CertGen.Generate(privKey);

            cert.CheckValidity(DateTime.UtcNow);

            cert.Verify(pubKey);

            IDictionary bagAttr = new Hashtable();

            if (friendlyName != null)
            {
                bagAttr.Add(PkcsObjectIdentifiers.Pkcs9AtFriendlyName.Id,
                    new DerBmpString(friendlyName));
            }

            return new X509CertificateEntry(cert, bagAttr);
        }

        public static X509CertificateEntry CreateIntermediateCert(
            IDictionary attrs,
            IList order,
            DateTime startDate,
            DateTime endDate,
            string algorithmName,
            AsymmetricKeyParameter pubKey,
            AsymmetricKeyParameter caPrivKey,
            Org.BouncyCastle.X509.X509Certificate caCert,
            string friendlyName=null)
        {
            //
            // create the certificate - version 3
            //
            v3CertGen.Reset();

            v3CertGen.SetSerialNumber(BigInteger.Two);
            v3CertGen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(caCert));

            v3CertGen.SetNotBefore(startDate);
            v3CertGen.SetNotAfter(endDate);

            v3CertGen.SetSubjectDN(new X509Name(order, attrs));

            v3CertGen.SetPublicKey(pubKey);

            v3CertGen.SetSignatureAlgorithm(algorithmName);

            //
            // extensions
            //
            v3CertGen.AddExtension(
                            X509Extensions.BasicConstraints, true, new BasicConstraints(true));

            KeyPurposeID[] usages = new[] {
                    KeyPurposeID.IdKPServerAuth,
                    KeyPurposeID.IdKPClientAuth,
                    KeyPurposeID.IdKPCodeSigning,
                    KeyPurposeID.IdKPIpsecEndSystem,
                    KeyPurposeID.IdKPIpsecTunnel,
                    KeyPurposeID.IdKPIpsecUser,
                    KeyPurposeID.IdKPOcspSigning,
                    KeyPurposeID.IdKPTimeStamping
                };

            v3CertGen.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign));

            v3CertGen.AddExtension(
                X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage(usages));

            v3CertGen.AddExtension(
                X509Extensions.SubjectKeyIdentifier,
                false,
                new SubjectKeyIdentifierStructure(pubKey));

            v3CertGen.AddExtension(
                X509Extensions.AuthorityKeyIdentifier,
                false,
                new AuthorityKeyIdentifierStructure(caCert));

            Org.BouncyCastle.X509.X509Certificate cert = v3CertGen.Generate(caPrivKey);

            cert.CheckValidity(DateTime.UtcNow);

            cert.Verify(caCert.GetPublicKey());

            IDictionary bagAttr = new Hashtable();

            if (friendlyName != null)
            {
                bagAttr.Add(PkcsObjectIdentifiers.Pkcs9AtFriendlyName.Id,
                    new DerBmpString(friendlyName));
            }

            return new X509CertificateEntry(cert, bagAttr);
        }

        public static X509CertificateEntry CreateIssuerCert(
            IDictionary issuerAttrs,
            IList issuerOrder,
            DateTime startDate,
            DateTime endDate,
            string algorithmName,
            AsymmetricKeyParameter pubKey,
            AsymmetricKeyParameter caPrivKey,
            Org.BouncyCastle.X509.X509Certificate caCert,
            string friendlyName = null)
        {
            //
            // create the certificate - version 3
            //
            v3CertGen.Reset();

            v3CertGen.SetSerialNumber(BigInteger.Three);
            v3CertGen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(caCert));
            v3CertGen.SetIssuerDN(PrincipalUtilities.GetSubjectX509Principal(caCert));
            v3CertGen.SetNotBefore(startDate);
            v3CertGen.SetNotAfter(endDate);
            v3CertGen.SetSubjectDN(new X509Name(issuerOrder, issuerAttrs));
            v3CertGen.SetPublicKey(pubKey);
            v3CertGen.SetSignatureAlgorithm(algorithmName);

            //
            // add the extensions
            //
            v3CertGen.AddExtension(
                            X509Extensions.BasicConstraints, true, new BasicConstraints(true));

            KeyPurposeID[] usages = new[] {
                    KeyPurposeID.IdKPServerAuth,
                    KeyPurposeID.IdKPClientAuth,
                    KeyPurposeID.IdKPCodeSigning,
                    KeyPurposeID.IdKPIpsecEndSystem,
                    KeyPurposeID.IdKPIpsecTunnel,
                    KeyPurposeID.IdKPIpsecUser,
                    KeyPurposeID.IdKPOcspSigning,
                    KeyPurposeID.IdKPTimeStamping
                };

            v3CertGen.AddExtension(
                X509Extensions.KeyUsage,
                true,
                new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign));

            v3CertGen.AddExtension(
                X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage(usages));

            v3CertGen.AddExtension(
                X509Extensions.SubjectKeyIdentifier,
                false,
                new SubjectKeyIdentifierStructure(pubKey));

            v3CertGen.AddExtension(
                X509Extensions.AuthorityKeyIdentifier,
                false,
                new AuthorityKeyIdentifierStructure(caCert));

            Org.BouncyCastle.X509.X509Certificate cert = v3CertGen.Generate(caPrivKey);

            cert.CheckValidity(DateTime.UtcNow);

            cert.Verify(caCert.GetPublicKey());

            IDictionary bagAttr = new Hashtable();
            if (friendlyName != null)
            {
                bagAttr.Add(PkcsObjectIdentifiers.Pkcs9AtFriendlyName.Id,
                    new DerBmpString(friendlyName));
            }

            return new X509CertificateEntry(cert);
        }
        #endregion

        /// <summary>
        /// Clean data from expander form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = (Button)sender;

            // clean all output messages
            tbCAOutputMessageBox.Text = "";
            tbInterOutputMessageBox.Text = "";
            tbIssuerOutputMessageBox.Text = "";

            btnIssuerContinue.IsEnabled = false;
            btnInterContinue.IsEnabled = false;
            btnCAContinue.IsEnabled = false;

            if (btnSender.Name == "btnCleanL3")
            {
                CleanData(3);
            }
            else if (btnSender.Name == "btnCleanL2")
            {
                CleanData(2);
            }
            else // btnCleanL1
            {
                CleanData(1);
            }

            // Expand all expander UI elements
            List<Expander> expanders = GetLogicalChildCollection<Expander>(CAForm);
            foreach (Expander exp in expanders)
            {
                exp.IsExpanded = true;
            }
        }
    }
}
