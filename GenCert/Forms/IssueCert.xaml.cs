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
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Operators;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using Org.BouncyCastle.Cms;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.X509.Extension;
using Dragablz;
using Org.BouncyCastle.Crypto.Parameters;
using System.Threading;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.CryptoPro;

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GeneratePFX.xaml
    /// </summary>
    public partial class IssueCert : UserControl
    {
        private static readonly IDictionary algorithms = Platform.CreateHashtable();


        public IssueCert()
        {
            InitializeComponent();

            btnContinue.IsEnabled = false;
            if (CertData.cerRequestFilePath!=null)
            {
                tbPathCsr.Text = CertData.cerRequestFilePath;
            }
            if (CertData.cerRequestFilePath != null)
            {
                tbPathGenerateCer.Text = System.IO.Path.GetDirectoryName(CertData.cerRequestFilePath) ;
            }
            if (CertData.cerPFXFilePath != null)
            {
                tbPathPrivateKey.Text = CertData.cerPFXFilePath;
            }
            if (CertData.cerPassword != null)
            {
                pbPassword.Password = CertData.cerPassword;
            }
            dpEndDate.SelectedDate = DateTime.Now.AddYears(1);

        }

        #region browse
        /// <summary>
        /// Browse for certificate request file (.csr)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnBrowseCsr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension
                dlg.DefaultExt = ".csr";
                dlg.Filter = "Certificate files (.csr)|*.csr";

                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    // Open document
                    tbPathCsr.Text = dlg.FileName;
                    if (String.IsNullOrEmpty(tbPathGenerateCer.Text))
                    {
                        tbPathGenerateCer.Text = System.IO.Path.GetDirectoryName(tbPathCsr.Text);
                    }
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

        private async void tbPathPrivateKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension
                dlg.DefaultExt = ".pfx";
                dlg.Filter = "Certificate files (.pfx)|*.pfx";

                // Display OpenFileDialog by calling ShowDialog method
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox
                if (result == true)
                {
                    //Thread.Sleep(1 * 1000);

                    // Open document
                    tbPathPrivateKey.Text = dlg.FileName;
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

        private void btnBrowseCer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                dlg.Description = "Select folder to save generate files";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbPathGenerateCer.Text = dlg.SelectedPath;
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        /// <summary>
        /// Start to generate certificate file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            List<string> errorMessages = new List<string>();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                tbOutputMessageBox.Text = "";
            }));

            #region check errors
            if (string.IsNullOrEmpty(tbPathCsr.Text))
            {
                errorMessages.Add("You MUST enter file name for read certificate request (.csr extension).");
            }
            else
            {
                if (!File.Exists(tbPathCsr.Text))
                {
                    errorMessages.Add("File " + tbPathCsr.Text + " DOES NOT exist. Please check file name and path");
                }
            }

            if (string.IsNullOrEmpty(tbPathPrivateKey.Text))
            {
                errorMessages.Add("You MUST enter file name to read private key (.key or .pfx extension).");
            }
            else
            {
                if (!File.Exists(tbPathPrivateKey.Text))
                {
                    errorMessages.Add("File " + tbPathPrivateKey.Text + " DOES NOT exist. Please check file name and path");
                }
            }

            if (string.IsNullOrEmpty(tbPathGenerateCer.Text))
            {
                errorMessages.Add("You MUST enter Path to store generate certificate file (.cer).");
            }
            else
            {
                if (!Directory.Exists(tbPathGenerateCer.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(tbPathGenerateCer.Text);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add("Can NOT create directory path: " + tbPathGenerateCer.Text);
                    }
                }
            }
            if (string.IsNullOrEmpty(tbCertFileName.Text))
            {
                errorMessages.Add("You MUST enter name for generate certificate file (.pfx).");
            }
            else
            {
                if (File.Exists(tbPathGenerateCer.Text + "\\" + tbCertFileName.Text + ".pfx"))
                {
                    errorMessages.Add("File: " + tbPathGenerateCer.Text + "\\" + tbCertFileName.Text + ".pfx" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }
            if (string.IsNullOrEmpty(tbCertFileName.Text))
            {
                errorMessages.Add("You MUST enter name for generate signed certificate request file (.cer).");
            }
            if (string.IsNullOrEmpty(pbPassword.Password))
            {
                errorMessages.Add("You MUST enter password for export private key from certificate file.");
            }
            if (string.IsNullOrEmpty(dpStartDate.Text))
            {
                errorMessages.Add("You MUST enter Start date for certificate.");
            }
            if (string.IsNullOrEmpty(dpEndDate.Text))
            {
                errorMessages.Add("You MUST enter End date for certificate.");
            }
            if (!string.IsNullOrEmpty(dpStartDate.Text) && !string.IsNullOrEmpty(dpEndDate.Text))
            {
                DateTime dpStartDateT = dpStartDate.DisplayDate;
                DateTime dpEndDateT = dpEndDate.DisplayDate;
                int result = DateTime.Compare(dpStartDateT, dpEndDateT);

                if (result == 0)
                {
                    errorMessages.Add("Begin date is equeal to End date.");
                }
                else if (result > 0)
                {
                    errorMessages.Add("Begin date is equeal to End date.");
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
            DateTime requestStartDate = dpStartDate.DisplayDate;
            DateTime requestEndDate = dpEndDate.DisplayDate;

            GenerateCerFile(tbPathCsr.Text,
                            tbPathPrivateKey.Text,
                            tbPathGenerateCer.Text + "\\" + tbCertFileName.Text + ".cer",
                            pbPassword.Password, 
                            requestStartDate, requestEndDate);
        }

        /// <summary>
        /// Read CA private key file from .key or pfx file
        /// Read data from certificate request file .csr
        /// Generate signed certificate request file .cer
        /// </summary>
        /// <param name="signedCERFile"></param>
        /// <param name="privateKeyFile"></param>
        /// <param name="v"></param>
        /// <param name="password"></param>
        private async void GenerateCerFile(string certRequestFile,
            string privateKeyFile,
            string generateSignedCertificateFile,
            string password, 
            DateTime requestStartDate, DateTime requestEndDate)
        {
            int errorNum = 0;

            issueCertificate.IsEnabled = false;
            progressring.Visibility = Visibility.Visible;
            await System.Threading.Tasks.TaskEx.Delay(1000);

            tbOutputMessageBox.Text = "";

            #region LoadCertificate

            X509Certificate2 issuerCertificate = null;
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                issuerCertificate = new X509Certificate2(
                        privateKeyFile,
                        password,
                        X509KeyStorageFlags.Exportable
                        );
            }
            // Exceptions:
            //   T:System.Security.Cryptography.CryptographicException:
            //     An error with the certificate occurs. For example:The certificate file does not
            //     exist.The certificate is invalid.The certificate's password is incorrect.
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                errorNum++;

                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Cryptographic ERROR=" + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));

                return;
            }
            catch (Exception ex)
            {
                errorNum++;

                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "General ERROR=" + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));

                return;
            }

            // This doesn't work for selfsign certificate
            //bool isOK = issuerCertificate.Verify();

            bool isHasPrivateKey = issuerCertificate.HasPrivateKey;
            DateTime noAfter = issuerCertificate.NotAfter;
            DateTime noBefore = issuerCertificate.NotBefore;
            X509ExtensionCollection x509extensions = issuerCertificate.Extensions;

            await System.Threading.Tasks.TaskEx.Delay(1000);
            X509CertificateParser parser = new X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate bouncyCertificate = parser.ReadCertificate(issuerCertificate.RawData);
            BasicConstraints basicConstraints = null;
            bool isCa = false;
            Asn1OctetString str = bouncyCertificate.GetExtensionValue(new DerObjectIdentifier("2.5.29.19")); // Basic Constraints -> DerObjectIdentifier BasicConstraints = new DerObjectIdentifier("2.5.29.19");
            if (str != null)
            {
                basicConstraints = BasicConstraints.GetInstance(X509ExtensionUtilities.FromExtensionValue(str));
                if (basicConstraints != null)
                {
                    isCa = basicConstraints.IsCA();
                }
            }

            if (!isCa)
            {
                errorNum++;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Loaded CA file: " + privateKeyFile + " IS NOT CA authority certificate file!" + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }

            // This doesn't work for selfsign certificate
            //if (!isOK)
            //{
            //    errorNum++;
            //    Brush bckForeground = tbOutputMessageBox.Foreground;
            //    tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
            //    tbOutputMessageBox.Text += "File with CA certificate NOT valid." + "\n";
            //    tbOutputMessageBox.Foreground = bckForeground;
            //}
            if (!isHasPrivateKey)
            {
                errorNum++;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "File with CA certificate DOES NOT have a private key." + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }
            if (noBefore > requestStartDate)
            {
                errorNum++;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "File with CA certificate start date: " + requestStartDate.ToLocalTime() + " DOES NOT valid value. Certificate start date is: " + noBefore.ToLocalTime() + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }
            if (noAfter < requestEndDate)
            {
                errorNum++;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "File with CA certificate end date: " + requestEndDate.ToLocalTime() + " DOES NOT valid value. Certificate end date is: " + noAfter.ToLocalTime() + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }

            if (errorNum>0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "File with CA certificate has error!!!" + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));

                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                return;
            }

            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                bool isOk = issuerCertificate.Verify();
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error Issuer certificate file: " + issuerCertificate + " Verification error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
                return;
            }


            Org.BouncyCastle.X509.X509Certificate issuerCertificateX509 = new X509CertificateParser().ReadCertificate(issuerCertificate.GetRawCertData());
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                bool isOK = VerifyCertChain(issuerCertificate);
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error Issuer certificate file: " + issuerCertificate + " public key error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
                return;
            }


            Org.BouncyCastle.X509.X509Certificate x509IssuerCert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(issuerCertificate);
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                x509IssuerCert.CheckValidity(requestStartDate);
                await System.Threading.Tasks.TaskEx.Delay(1000);
                x509IssuerCert.CheckValidity(requestEndDate);
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error Issuer certificate file: " + issuerCertificate + " X509 certificate error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
                return;
            }

            #endregion

            // Read certificate request .csr file
            Pkcs10CertificationRequest cerRequest = null;
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                String input_data = File.ReadAllText(certRequestFile);
                StringReader sr = new StringReader(input_data);
                PemReader pr = new PemReader(sr);
                cerRequest = (Pkcs10CertificationRequest)pr.ReadObject();

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Verify file with certificate request : " + certRequestFile + "\n",
                        Foreground = System.Windows.Media.Brushes.Black
                    });
                }));

                try
                {
                    await System.Threading.Tasks.TaskEx.Delay(1000);
                    bool requestIsOK = cerRequest.Verify();
                    if (requestIsOK)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            tbOutputMessageBox.Inlines.Add(new Run
                            {
                                Text = "File with certificate request : " + certRequestFile + " is OK." + "\n",
                                Foreground = System.Windows.Media.Brushes.Black
                            });
                        }));
                    }
                    else
                    {
                        progressring.Visibility = Visibility.Hidden;
                        issueCertificate.IsEnabled = true;

                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            tbOutputMessageBox.Inlines.Add(new Run
                            {
                                Text = "File with certificate request : " + certRequestFile + " NOT valid." + "\n",
                                Foreground = System.Windows.Media.Brushes.Red
                            });
                        }));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    progressring.Visibility = Visibility.Hidden;
                    issueCertificate.IsEnabled = true;

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        tbOutputMessageBox.Inlines.Add(new Run
                        {
                            Text = "Error certificate request file: " + cerRequest + " Verify error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                            Foreground = System.Windows.Media.Brushes.Red
                        });
                    }));
                    return;
                }
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                issueCertificate.IsEnabled = true;

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR reading certificate request file (.csr)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
                return;
            }

            await System.Threading.Tasks.TaskEx.Delay(1000);
            AsymmetricCipherKeyPair issuerKeyPair = DotNetUtilities.GetKeyPair(issuerCertificate.PrivateKey);

            await System.Threading.Tasks.TaskEx.Delay(1000);
            Org.BouncyCastle.X509.X509Certificate genCert = GenerateSignedCertificate(
                    cerRequest,
                    x509IssuerCert,
                    issuerKeyPair,
                    requestStartDate, 
                    requestEndDate);

            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                File.WriteAllBytes(System.IO.Path.ChangeExtension(generateSignedCertificateFile, ".cer"), genCert.GetEncoded());

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file: " + generateSignedCertificateFile + " sucessfully saved." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }));

                btnContinue.IsEnabled = true;
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }));
            }
            CertData.cerPublicFilePath = tbPathGenerateCer.Text+"\\"+ tbCertFileName.Text+".cer";

            progressring.Visibility = Visibility.Hidden;
            issueCertificate.IsEnabled = true;
        }

        /// <summary>
        /// Enroll certificate file base on request
        /// </summary>
        /// <param name="cerRequest"></param>
        /// <param name="rootCert"></param>
        /// <param name="issuerKeyPair"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private Org.BouncyCastle.X509.X509Certificate GenerateSignedCertificate(
                    Pkcs10CertificationRequest cerRequest,
                    Org.BouncyCastle.X509.X509Certificate rootCert,
                    AsymmetricCipherKeyPair issuerKeyPair,
                    DateTime startDate, DateTime endDate)
        {
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();

            certGen.SetSerialNumber(BigInteger.One);
            certGen.SetIssuerDN(rootCert.SubjectDN);
            certGen.SetNotBefore(startDate);
            certGen.SetNotAfter(endDate);

            CertificationRequestInfo info = cerRequest.GetCertificationRequestInfo();
            certGen.SetSubjectDN(info.Subject);

            certGen.SetPublicKey(cerRequest.GetPublicKey());

            AlgorithmIdentifier sigAlg = cerRequest.SignatureAlgorithm;
            string algName = GetAlgorithmName(sigAlg.Algorithm.Id);
            certGen.SetSignatureAlgorithm(algName);

            // Add certificate extensions
            Asn1Set attributes = cerRequest.GetCertificationRequestInfo().Attributes;
            if (attributes != null)
            {
                for (int i = 0; i != attributes.Count; i++)
                {
                    AttributePkcs attr = AttributePkcs.GetInstance(attributes[i]);

                    if (attr.AttrType.Equals(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest))
                    {
                        X509Extensions extensions1 = X509Extensions.GetInstance(attr.AttrValues[0]);

                        foreach (DerObjectIdentifier oid in extensions1.ExtensionOids)
                        {
                            Org.BouncyCastle.Asn1.X509.X509Extension ext = extensions1.GetExtension(oid);
                            certGen.AddExtension(oid, ext.IsCritical, ext.GetParsedValue());
                        }
                    }
                }
            }
            
            Org.BouncyCastle.X509.X509Certificate issuedCert = null;
            try
            {
                issuedCert = certGen.Generate(issuerKeyPair.Private);
                tbOutputMessageBox.Text += "Certificate file sucessfully generated." + "\n";
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Certificate file sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, generate certificate file." + "\n" + "ERROR: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }

            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Check if generated certificate file is valid, plase wait ..." + "\n",
                        Foreground = System.Windows.Media.Brushes.Black
                    });
                }));
                issuedCert.CheckValidity(DateTime.UtcNow);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Generate certificate file is valid." + "\n",
                        Foreground = System.Windows.Media.Brushes.Black
                    });
                }));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, generated certificate file is INVALID." + "\n" + "ERROR: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }

            try
            {
                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "Verify generated certificate file, plase wait ..." + "\n",
                    Foreground = System.Windows.Media.Brushes.Black
                });
                issuedCert.Verify(issuerKeyPair.Public);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Generate certificate file verification is OK." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });
                }));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, generated certificate file verification is INVALID." + "\n" + "ERROR: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }));
            }
            return issuedCert;
        }

        #region helper methods
        private class ExtensionsItem
        {
            public ExtensionsItem()
            {
                ident = null;
                isTrue = false;
                asn1OctetString = null;
            }

            public DerObjectIdentifier ident { get; set; }
            public bool isTrue { get; set; }
            public Asn1OctetString asn1OctetString { get; set; }
        }

        #endregion

        /// <summary>
        /// Open wizard what will be next step
        /// If you generate self-sign CA root certificate, next step can be to open form form issue certificate base on certificate request file ganerated 
        /// using menu option "Create Request"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            string signedRequestFileNamePath = tbPathGenerateCer.Text+"\\"+ tbCertFileName.Text+".cer";      // keep path for cert request file (.csr) -> form: CreateRequest
            if (!String.IsNullOrEmpty(signedRequestFileNamePath) && File.Exists(signedRequestFileNamePath))
            {
                MetroWindow metroWin = (MetroWindow)Application.Current.MainWindow;
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Create Certificate base on generated (.cer) file",
                    NegativeButtonText = "Cancel",
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                MessageDialogResult result = await metroWin.ShowMessageAsync("INFO",
                    "If you wish to generate certificate base on certificate request file (.csr) inside menu option 'Create Request'," + "\n" +
                    "use button 'Issue certificate base on request'." + "\n" +
                    "Generated CA root certificate: " + CertData.cerPFXFilePath + "\n" +
                    "will be use to sing certificate requst (.csr) file, generated inside menu option 'Create Request'.",
                    MessageDialogStyle.AffirmativeAndNegative, mySettings);

                if (result == MessageDialogResult.Affirmative)
                {
                    // open menu "Issue Certificate" 
                    MainWindow mw = new MainWindow();

                    string header0 = "Create Certificate";
                    IEnumerable<TabablzControl> tctrl;
                    mw.GetTabablzData(out header0, out tctrl);
                    header0 = "Create Certificate";

                    CreatePFX gr = new CreatePFX();
                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);

                    mw.sbiSelectedMenuOption.Content = header0;

                }
                else if (result == MessageDialogResult.Negative)
                {
                }
            }
        }

        internal void X509Utilities()
        {
            if (algorithms.Count == 0)
            {
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
            }
            return;
        }

        internal string GetAlgorithmName(string algorithmOid)
        {
            X509Utilities();

            IDictionaryEnumerator e = algorithms.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Value.ToString() == algorithmOid)
                    return e.Key.ToString();
            }
            return "";
        }

        ///This will merely ensure that your CA's are valid. 
        ///If you want to ensure that the chain is identical you can check the thumbprints manually. 
        ///You can use the following method to ensure that the certification chain is correct, 
        ///it expects the chain in the order: ..., INTERMEDIATE2, INTERMEDIATE1 (Signer of INTERMEDIATE2), CA (Signer of INTERMEDIATE1)
        bool VerifyCertificate(byte[] primaryCertificate, IEnumerable<byte[]> additionalCertificates)
        {
            X509Chain chain = new X509Chain();
            foreach (X509Certificate2 cert in additionalCertificates.Select(x => new X509Certificate2(x)))
            {
                chain.ChainPolicy.ExtraStore.Add(cert);
            }

            // You can alter how the chain is built/validated.
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;

            // Do the preliminary validation.
            X509Certificate2 primaryCert = new X509Certificate2(primaryCertificate);
            if (!chain.Build(primaryCert))
                return false;

            // Make sure we have the same number of elements.
            if (chain.ChainElements.Count != chain.ChainPolicy.ExtraStore.Count + 1)
                return false;

            // Make sure all the thumbprints of the CAs match up.
            // The first one should be 'primaryCert', leading up to the root CA.
            for (int i = 1; i < chain.ChainElements.Count; i++)
            {
                if (chain.ChainElements[i].Certificate.Thumbprint != chain.ChainPolicy.ExtraStore[i - 1].Thumbprint)
                {
                    // CA thumbprints not identical
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        tbOutputMessageBox.Inlines.Add(new Run
                        {
                            Text = "CA thumbprints not identical." + chain.ChainElements[i].Certificate.Subject + " diferent than:" + chain.ChainPolicy.ExtraStore[i - 1].Subject + "\n",
                            Foreground = System.Windows.Media.Brushes.Red
                        });
                    }));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Verify certificate chain
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        bool VerifyCertChain(X509Certificate2 cert)
        {
            // Validation with the specific flags
            X509Chain ch1= new X509Chain();
            ch1.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            ch1.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            ch1.ChainPolicy.VerificationFlags =  X509VerificationFlags.IgnoreCtlSignerRevocationUnknown |
                                                 X509VerificationFlags.IgnoreRootRevocationUnknown |
                                                 X509VerificationFlags.IgnoreEndRevocationUnknown |
                                                 X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown |
                                                 X509VerificationFlags.IgnoreCtlNotTimeValid |
                                                 X509VerificationFlags.AllowUnknownCertificateAuthority;

            bool bRet1 = ch1.Build(cert);
            if (!bRet1)
            {
                return bRet1;
            }
            foreach (X509ChainStatus status1 in ch1.ChainStatus)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = status1.Status.ToString() + " --> " + status1.StatusInformation + "\n",
                        Foreground = System.Windows.Media.Brushes.Orange
                    });
                }));
            }

            List<byte[]> additionalCertificates = new List<byte[]>();
            if (ch1.ChainElements.Count > 1)
            {
                for (int i = 1; i < ch1.ChainElements.Count; i++)
                {
                    additionalCertificates.Add(ch1.ChainElements[i].Certificate.GetRawCertData());
                }
                bool isOK = VerifyCertificate(cert.GetRawCertData(), additionalCertificates);
            }

            X509Chain ch2 = new X509Chain();
            ch2.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            ch2.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            ch2.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            bool bRet2 = ch1.Build(cert);
            if (!bRet2)
            {
                return bRet2;
            }
            foreach (X509ChainStatus status2 in ch2.ChainStatus)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = status2.Status.ToString() + " --> " + status2.StatusInformation + "\n",
                        Foreground = System.Windows.Media.Brushes.Orange
                    });
                }));
            }

            return true;
        }
    }
}
