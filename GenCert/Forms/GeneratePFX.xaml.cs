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
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GeneratePFX.xaml
    /// </summary>
    public partial class GeneratePFX : UserControl
    {
        public GeneratePFX()
        {
            InitializeComponent();
        }
        public GeneratePFX(string certFriendlyName) : this()
        {
            tbPathPfx.Text = System.Environment.CurrentDirectory;

            #region test data
            tbFriendlyName.Text = certFriendlyName;
            //tbPathCer.Text = System.Environment.CurrentDirectory + "\\client_signed.cer";
            //tbPathKey.Text = System.Environment.CurrentDirectory + "\\client_private.key";
            //tbPathPfx.Text = System.Environment.CurrentDirectory;
            //tbCertFileName.Text = "client_generate";
            //pbPassword.Password = "password";
            #endregion
        }

        public GeneratePFX(string certFriendlyName, string signedRequestFileNamePath,
                           string requestFileNamePrivateKeyPath, string CARootPubKeyFileNamePath) : this()
        {
            tbPathPfx.Text = System.Environment.CurrentDirectory;

            if (!String.IsNullOrEmpty(certFriendlyName))
            {
                tbFriendlyName.Text = certFriendlyName;
            }
            if (!String.IsNullOrEmpty(signedRequestFileNamePath))
            {
                tbPathCer.Text = signedRequestFileNamePath;
                tbPathPfx.Text = System.IO.Path.GetDirectoryName(signedRequestFileNamePath);
            }
            if (!String.IsNullOrEmpty(requestFileNamePrivateKeyPath))
            {
                tbPathKey.Text = requestFileNamePrivateKeyPath;
            }
            if (!String.IsNullOrEmpty(CARootPubKeyFileNamePath))
            {
                tbCAPathCer.Text = CARootPubKeyFileNamePath;
            }
        }

        #region Browse
        private async void btnBrowseCer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension
                dlg.DefaultExt = ".cer";
                dlg.Filter = "Certificate files (.cer)|*.cer";

                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    tbPathCer.Text = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR reading data from signed request file (.cer)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
            }
        }

        /// <summary>
        /// Browse for private key file (.key)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnBrowseKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension
                dlg.DefaultExt = ".key";
                dlg.Filter = "Certificate files (.key)|*.key";

                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    tbPathKey.Text = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR reading data from private key file (.key)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
            }
        }

        /// <summary>
        /// Browse for folder to store generate cert.file .pfx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowsePfx_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                dlg.Description = "Select folder to save generate files";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbPathPfx.Text = dlg.SelectedPath;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Browse for root CA public key file (.cer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnBrowseCACer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension
                dlg.DefaultExt = ".cer";
                dlg.Filter = "Certificate files (.cer)|*.cer";

                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    tbCAPathCer.Text = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR reading data from signed request file (.cer)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
            }
        }
        #endregion

        /// <summary>
        /// Start to generate certificate with public and private key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            List<string> errorMessages = new List<string>();

            #region check errors
            if (string.IsNullOrEmpty(tbFriendlyName.Text))
            {
                errorMessages.Add("You MUST enter certificate Frendly Name.");
            }

            if (string.IsNullOrEmpty(tbPathCer.Text))
            {
                errorMessages.Add("You MUST enter file name for read signed certificate request (.cer extension).");
            }
            else
            {
                if (!File.Exists(tbPathCer.Text))
                {
                    errorMessages.Add("File "+ tbPathCer.Text+" DOES NOT exist. Please check file name and path");
                }
            }
            if (string.IsNullOrEmpty(tbPathKey.Text))
            {
                errorMessages.Add("You MUST enter file name to read certificate private key (.key extension).");
            }
            else
            {
                if (!File.Exists(tbPathKey.Text))
                {
                    errorMessages.Add("File " + tbPathKey.Text + " DOES NOT exist. Please check file name and path");
                }
            }
            if (string.IsNullOrEmpty(tbPathPfx.Text))
            {
                errorMessages.Add("You MUST enter Path to store generate certificate file (.pfx).");
            }
            else
            {
                if (!Directory.Exists(tbPathPfx.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(tbPathPfx.Text);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add("Can NOT create directory path: " + tbPathPfx.Text);
                    }
                }
            }
            if (string.IsNullOrEmpty(tbCertFileName.Text))
            {
                errorMessages.Add("You MUST enter name for generate certificate file (.pfx).");
            }
            else
            {
                if (File.Exists(tbPathPfx.Text+"\\"+ tbCertFileName.Text+".pfx"))
                {
                    errorMessages.Add("File: " + tbPathPfx.Text + "\\" + tbCertFileName.Text + ".pfx" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }
            if (string.IsNullOrEmpty(pbPassword.Password))
            {
                errorMessages.Add("You MUST enter password for export private key from certificate file.");
            }

            if (!string.IsNullOrEmpty(tbCAPathCer.Text))
            {
                if (!File.Exists(tbCAPathCer.Text))
                {
                    errorMessages.Add("File " + tbCAPathCer.Text + " DOES NOT exist. Please check file name and path");
                }
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
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Error",
                     errorMessage,
                     MessageDialogStyle.Affirmative);
                return;
            }
            #endregion
            // Proceed with generating certificate .pfx file
            else
            {
                GeneratePFXFile(tbPathCer.Text,
                                tbPathKey.Text,
                                tbPathPfx.Text + "\\" + tbCertFileName.Text + ".pfx",
                                pbPassword.Password,
                                tbFriendlyName.Text,
                                tbCAPathCer.Text);
            }
        }

        /// <summary>
        /// Read private key data from file
        /// </summary>
        /// <param name="privateKeyFileName"></param>
        /// <returns></returns>
        static AsymmetricKeyParameter ReadPrivateKey(string privateKeyFileName)
        {
            AsymmetricCipherKeyPair keyPair;

            using (var reader = File.OpenText(privateKeyFileName))
                keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

            return keyPair.Private;
        }

        /// <summary>
        /// Generate PFX File
        /// </summary>
        /// <param name="signedCERFile"></param>
        /// <param name="privateKeyFile"></param>
        /// <param name="v"></param>
        /// <param name="password"></param>
        private async void GeneratePFXFile(string signedCERFile, string privateKeyFile, string generateCertificateFile, 
            string password, string friendlyName, string signedCACERFile=null)
        {
            // Prepare the pkcs12 certificate store
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            AsymmetricKeyParameter privateKey = ReadPrivateKey(privateKeyFile);
            X509CertificateEntry[] chain = null;

            // Check if CA root public key file exist? If exist read data from file and verify certificate inside signed requested public key file with CA root public key
            // If Ca root public key file does not exist, certificate inside signed requested public key file CAN NOT be veryfied
            // Bundle together the private key, signed certificate and CA
            Org.BouncyCastle.X509.X509Certificate CAX509Cert = null;
            if (String.IsNullOrEmpty(signedCACERFile))
            {
                chain = new X509CertificateEntry[1];
                //chain[0] = certEntry;
            }
            else
            {
                chain = new X509CertificateEntry[2];
                //chain[0] = certEntry;

                try
                {
                    // Import the CA certificate
                    X509Certificate2 certCA = new X509Certificate2(signedCACERFile);
                    //then export it like so
                    byte[] p12CA = certCA.Export(X509ContentType.Cert);
                    CAX509Cert = new X509CertificateParser().ReadCertificate(p12CA);
                    X509CertificateEntry certCAEntry = new X509CertificateEntry(CAX509Cert);
                    chain[1] = certCAEntry;
                }
                catch (Exception ex)
                {
                    Brush bckForeground = tbOutputMessageBox.Foreground;
                    tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                    tbOutputMessageBox.Text += "Error reading root CA certificate file: " + signedCACERFile + "\n";
                    tbOutputMessageBox.Foreground = bckForeground;
                    return;
                }
            }

            // Import data from the signed requested certificate file => file with .cer extension (NOT CA root public key file)
            X509Certificate2 certSigned = new X509Certificate2(signedCERFile);

            int errorNum = 0;
            #region check - old
            // This is .cer public key file and it doesn't have private key => it's OK
            //bool isHasPrivateKey = certSigned.HasPrivateKey;
            //if (!isHasPrivateKey)
            //{
            //    errorNum++;
            //    Brush bckForeground = tbOutputMessageBox.Foreground;
            //    tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
            //    tbOutputMessageBox.Text += "Error, certificate file: "+ signedCERFile+" DOES NOT have a private key!!!" + "\n";
            //    tbOutputMessageBox.Foreground = bckForeground;
            //}

            // This is certificate signed with CA root that not yet been imported to Trusted Root Certification Authorities and can't be verified
            //bool isOK = certSigned.Verify();
            //if (!isOK)
            //{
            //    errorNum++;
            //    Brush bckForeground = tbOutputMessageBox.Foreground;
            //    tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
            //    tbOutputMessageBox.Text += "Error, certificate file: " + signedCERFile + " NOT valid!!!" + "\n";
            //    tbOutputMessageBox.Foreground = bckForeground;
            //}
            #endregion

            //then export it like so
            byte[] p12 = certSigned.Export(X509ContentType.Cert);
            Org.BouncyCastle.X509.X509Certificate signedX509Cert = new X509CertificateParser().ReadCertificate(p12);

            if (CAX509Cert != null)
            {
                try
                {
                    signedX509Cert.Verify(CAX509Cert.GetPublicKey());
                }
                catch (Exception ex)
                {
                    errorNum++;
                    Brush bckForeground = tbOutputMessageBox.Foreground;
                    tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                    tbOutputMessageBox.Text += "Error certificate file: " + signedCERFile + " Verification error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n";
                    tbOutputMessageBox.Foreground = bckForeground;
                }
            }
            else
            {
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Yellow);
                tbOutputMessageBox.Text += "Certificate file: " + signedCERFile + " CAN NOT be verified, because CA root public key file not provided" + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
            }
            if (errorNum > 0)
            {
                return;
            }

            X509CertificateEntry certEntry = new X509CertificateEntry(signedX509Cert);
            chain[0] = certEntry;
            store.SetKeyEntry(signedX509Cert.SubjectDN.ToString() + "_key", new AsymmetricKeyEntry(privateKey), chain);

            // Add the certificate.
            X509CertificateEntry certificateEntry = new X509CertificateEntry(signedX509Cert);
            store.SetCertificateEntry(friendlyName, certificateEntry);

            // Add the private key.
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(privateKey), new[] { certificateEntry });
            
            try
            {
                using (var filestream = new FileStream(generateCertificateFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    store.Save(filestream, password.ToCharArray(), new SecureRandom());
                }

                if (chain.Length>1)
                {
                    tbOutputMessageBox.Text += "Certificate file with private key: " + generateCertificateFile + " and CA public key sucessfully generated." + "\n";
                }
                else
                {
                    tbOutputMessageBox.Text += "Certificate file with private key: " + generateCertificateFile + " sucessfully generated." + "\n";
                }
            }
            catch (Exception ex)
            {
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                if (chain.Length > 1)
                {
                    tbOutputMessageBox.Text += "Error, certificate file with private key: " + generateCertificateFile + " and CA public key DOES NOT sucessfully generated." + "\n";
                }
                else
                {
                    tbOutputMessageBox.Text += "Certificate file with private key: " + generateCertificateFile + " DOES NOT sucessfully generated." + "\n";
                }
                tbOutputMessageBox.Foreground = bckForeground;

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR creating certificate file with private key file (.pfx)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
                return;
            }
        }

    }
}
