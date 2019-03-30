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
using System.Threading.Tasks;
using System.Threading;
using Org.BouncyCastle.Crypto.Parameters;

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GeneratePFX.xaml
    /// </summary>
    public partial class CreatePFX : UserControl
    {
        public CreatePFX()
        {
            InitializeComponent();

            #region fill data
            if (CertData.cerPublicFilePath != null)
            {
                tbPathCer.Text = CertData.cerPublicFilePath;
            }
            if (CertData.cerPrivateFilePath != null)
            {
                tbPathKey.Text = CertData.cerPrivateFilePath;
            }
            if (!String.IsNullOrEmpty(tbPathCer.Text))
            {
                tbPathPfx.Text = System.IO.Path.GetDirectoryName(tbPathCer.Text);
            }

            if (CertData.cerMasterPubFilePath != null)
            {
                tbMasterCAPathCer.Text = CertData.cerMasterPubFilePath;
            }
            if (CertData.cerIntermediatePubFilePath != null)
            {
                tbInterMediateCAPathCer.Text = CertData.cerIntermediatePubFilePath;
            }
            if (CertData.cerIssuerPubFilePath != null)
            {
                tbIssuerCAPathCer.Text = CertData.cerIssuerPubFilePath;
            }
            if (CertData.cerPrivateKeyPassword != null)
            {
                pbPasswordPrivateKey.Password = CertData.cerPrivateKeyPassword;
            }

            #endregion
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
                    tbMasterCAPathCer.Text = dlg.FileName;
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

            tbOutputMessageBox.Text = "";

            #region check errors
            if (string.IsNullOrEmpty(tbFriendlyName.Text))
            {
                errorMessages.Add("You MUST enter certificate Friendly Name.");
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

            if (!string.IsNullOrEmpty(tbMasterCAPathCer.Text))
            {
                if (!File.Exists(tbMasterCAPathCer.Text))
                {
                    errorMessages.Add("File for Master CA certificate " + tbMasterCAPathCer.Text + " DOES NOT exist. Please check file name and path");
                }
            }

            if (!string.IsNullOrEmpty(tbInterMediateCAPathCer.Text))
            {
                if (!File.Exists(tbInterMediateCAPathCer.Text))
                {
                    errorMessages.Add("File for Intermediate ceretificate" + tbInterMediateCAPathCer.Text + " DOES NOT exist. Please check file name and path");
                }
            }

            if (!string.IsNullOrEmpty(tbIssuerCAPathCer.Text))
            {
                if (!File.Exists(tbIssuerCAPathCer.Text))
                {
                    errorMessages.Add("File for Issuer CA certificate" + tbIssuerCAPathCer.Text + " DOES NOT exist. Please check file name and path");
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
                                tbMasterCAPathCer.Text,
                                tbInterMediateCAPathCer.Text,
                                tbIssuerCAPathCer.Text,
                                pbPasswordPrivateKey.Password
                                );

            }
        }

        /// <summary>
        /// Read private key data from file
        /// </summary>
        /// <param name="privateKeyFileName"></param>
        /// <returns></returns>
        static AsymmetricKeyParameter ReadPrivateKey(string privateKeyFileName, string password=null)
        {
            AsymmetricCipherKeyPair keyPair;

            if (password == null)
            {
                using (var reader = File.OpenText(privateKeyFileName))
                    keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
            }
            else
            {
                using (var reader = File.OpenText(privateKeyFileName))
                    keyPair = (AsymmetricCipherKeyPair)new PemReader(reader, new PasswordFinder(password)).ReadObject();

            }
            return keyPair.Private;
        }

        /// <summary>
        /// Generate PFX File
        /// </summary>
        /// <param name="signedCERFile"></param>
        /// <param name="privateKeyFile"></param>
        /// <param name="v"></param>
        /// <param name="password"></param>
        private async void GeneratePFXFile(
            string signedCERFile, 
            string privateKeyFile, 
            string generateCertificateFile, 
            string password, 
            string friendlyName, 
            string signedMasterCACERFile=null, 
            string signedIntermediateCACERFile = null, 
            string signedIssuerCACERFile = null,
            string passwordPrivateKey = null)
        {
            createPFX.IsEnabled = false;
            progressring.Visibility = Visibility.Visible;
            await System.Threading.Tasks.TaskEx.Delay(1000);

            // Prepare the pkcs12 certificate store
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            AsymmetricKeyParameter privateKey = ReadPrivateKey(privateKeyFile, passwordPrivateKey);

            X509CertificateEntry[] chain = null;
            int certChainLength = 1;

            tbOutputMessageBox.Text = "";

            // Check if CA root public key file exist? If exist read data from file and verify certificate inside signed requested public key file with CA root public key
            // If Ca root public key file does not exist, certificate inside signed requested public key file CAN NOT be veryfied
            // Bundle together the private key, signed certificate and CA
            Org.BouncyCastle.X509.X509Certificate masterCAX509Cert = null;
            Org.BouncyCastle.X509.X509Certificate intermediateCAX509Cert = null;
            Org.BouncyCastle.X509.X509Certificate issuerCAX509Cert = null;

            if (!String.IsNullOrEmpty(signedMasterCACERFile))
            {
                certChainLength++;
            }
            if (!String.IsNullOrEmpty(signedIntermediateCACERFile))
            {
                certChainLength++;
            }
            if (!String.IsNullOrEmpty(signedIssuerCACERFile))
            {
                certChainLength++;
            }
            chain = new X509CertificateEntry[certChainLength];

            if (!String.IsNullOrEmpty(signedMasterCACERFile))
            {
                try
                {
                    await System.Threading.Tasks.TaskEx.Delay(1000);

                    // Import the CA certificate
                    X509Certificate2 certMasterCA = new X509Certificate2(signedMasterCACERFile);
                    //then export it like so
                    byte[] p12MasterCA = certMasterCA.Export(X509ContentType.Cert);
                    masterCAX509Cert = new X509CertificateParser().ReadCertificate(p12MasterCA);
                    X509CertificateEntry certMasterCAEntry = new X509CertificateEntry(masterCAX509Cert);
                    chain[certChainLength-1] = certMasterCAEntry;
                    certChainLength--;
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    createPFX.IsEnabled = true;

                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error reading Master CA certificate file: " + signedMasterCACERFile + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });

                    return;
                }
            }
            if (!String.IsNullOrEmpty(signedIntermediateCACERFile))
            {
                try
                {
                    await System.Threading.Tasks.TaskEx.Delay(1000);
                    // Import the CA certificate
                    X509Certificate2 certIntermediateCA = new X509Certificate2(signedIntermediateCACERFile);
                    //then export it like so
                    byte[] p12IntermediateCA = certIntermediateCA.Export(X509ContentType.Cert);
                    intermediateCAX509Cert = new X509CertificateParser().ReadCertificate(p12IntermediateCA);
                    X509CertificateEntry certIntermediateCAEntry = new X509CertificateEntry(intermediateCAX509Cert);
                    chain[certChainLength - 1] = certIntermediateCAEntry;
                    certChainLength--;
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    createPFX.IsEnabled = true;

                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error reading Intermediate CA certificate file: " + signedMasterCACERFile + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                    return;
                }
            }
            if (!String.IsNullOrEmpty(signedIssuerCACERFile))
            {
                try
                {
                    await System.Threading.Tasks.TaskEx.Delay(1000);
                    // Import the CA certificate
                    X509Certificate2 certIssuerCA = new X509Certificate2(signedIssuerCACERFile);
                    //then export it like so
                    byte[] p12IssuerCA = certIssuerCA.Export(X509ContentType.Cert);
                    issuerCAX509Cert = new X509CertificateParser().ReadCertificate(p12IssuerCA);
                    X509CertificateEntry certIntermediateCAEntry = new X509CertificateEntry(issuerCAX509Cert);
                    chain[certChainLength - 1] = certIntermediateCAEntry;
                    certChainLength--;
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    createPFX.IsEnabled = true;

                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error reading Intermediate CA certificate file: " + signedMasterCACERFile + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                    return;
                }
            }

            // Import data from the signed requested certificate file => file with .cer extension (NOT CA root public key file)
            X509Certificate2 certSigned = new X509Certificate2(signedCERFile);

            int errorNum = 0;

            await System.Threading.Tasks.TaskEx.Delay(1000);
            //then export it like so
            byte[] p12 = certSigned.Export(X509ContentType.Cert);
            Org.BouncyCastle.X509.X509Certificate signedX509Cert = new X509CertificateParser().ReadCertificate(p12);

            bool isVeryfyChainCer = false;
            if (!isVeryfyChainCer)
            {
                if (issuerCAX509Cert != null)  // Issuer
                {
                    try
                    {
                        await System.Threading.Tasks.TaskEx.Delay(1000);
                        isVeryfyChainCer = true;
                        signedX509Cert.Verify(issuerCAX509Cert.GetPublicKey());
                    }
                    catch (Exception ex)
                    {
                        errorNum++;
                        tbOutputMessageBox.Inlines.Add(new Run
                        {
                            Text = "Error Issuer certificate file: " + signedCERFile + " Verification error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                            Foreground = System.Windows.Media.Brushes.Red
                        });
                    }
                }
            }

            if (!isVeryfyChainCer)
            {
                if (intermediateCAX509Cert != null) // Intermediate
                {
                    try
                    {
                        await System.Threading.Tasks.TaskEx.Delay(1000);
                        isVeryfyChainCer = true;
                        signedX509Cert.Verify(intermediateCAX509Cert.GetPublicKey());
                    }
                    catch (Exception ex)
                    {
                        errorNum++;
                        tbOutputMessageBox.Inlines.Add(new Run
                        {
                            Text = "Error Intermediate certificate file: " + signedCERFile + " Verification error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                            Foreground = System.Windows.Media.Brushes.Red
                        });
                    }
                }
            }

            if (!isVeryfyChainCer)
            {
                if (masterCAX509Cert != null) // Mster
                {
                    try
                    {
                        await System.Threading.Tasks.TaskEx.Delay(1000);
                        isVeryfyChainCer = true;
                        signedX509Cert.Verify(masterCAX509Cert.GetPublicKey());
                    }
                    catch (Exception ex)
                    {
                        errorNum++;
                        tbOutputMessageBox.Inlines.Add(new Run
                        {
                            Text = "Error Master certificate file: " + signedCERFile + " Verification error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                            Foreground = System.Windows.Media.Brushes.Red
                        });
                    }
                }
            }

            if (errorNum > 0)
            {
                progressring.Visibility = Visibility.Hidden;
                createPFX.IsEnabled = true;
                return;
            }

            //---------------------------------------------------------------------------------
            X509CertificateEntry certEntry = new X509CertificateEntry(signedX509Cert);

            await System.Threading.Tasks.TaskEx.Delay(1000);
            chain[0] = certEntry;
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(privateKey), chain);

            try
            {
                using (var filestream = new FileStream(generateCertificateFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    await System.Threading.Tasks.TaskEx.Delay(1000);
                    store.Save(filestream, password.ToCharArray(), new SecureRandom());
                }

                if (chain.Length>3)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file with private key: " + generateCertificateFile + " and Master, Intermediate, Issuer CA public key sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                else if (chain.Length > 2)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file with private key: " + generateCertificateFile + " and Master, Intermediate CA public key sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                else if (chain.Length > 1)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file with private key: " + generateCertificateFile + " and Master CA public key sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
                else
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file with private key: " + generateCertificateFile + " sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }
            }
            catch (Exception ex)
            {
                if (chain.Length > 3)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, certificate file with private key: " + generateCertificateFile + " and Master, Intermediate, Issuer CA public key DOES NOT sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }
                if (chain.Length > 2)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, certificate file with private key: " + generateCertificateFile + " and Master, IntermediateCA public key DOES NOT sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }
                if (chain.Length > 1)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, certificate file with private key: " + generateCertificateFile + " and Master CA public key DOES NOT sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }
                else
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file with private key: " + generateCertificateFile + " DOES NOT sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }

                progressring.Visibility = Visibility.Hidden;
                createPFX.IsEnabled = true;

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR creating certificate file with private key file (.pfx)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
                return;
            }

            progressring.Visibility = Visibility.Hidden;
            createPFX.IsEnabled = true;
        }

        private async void btnBrowseIntermediateCACer_Click(object sender, RoutedEventArgs e)
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
                    tbInterMediateCAPathCer.Text = dlg.FileName;
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

        private async void btnBrowseIssuerCACer_Click(object sender, RoutedEventArgs e)
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
                    tbIssuerCAPathCer.Text = dlg.FileName;
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
    }
    class PasswordFinder : IPasswordFinder
    {
        private string password;

        public PasswordFinder(string password)
        {
            this.password = password;
        }


        public char[] GetPassword()
        {
            return password.ToCharArray();
        }
    }
}
