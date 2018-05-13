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

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GeneratePFX.xaml
    /// </summary>
    public partial class GenerateSignRequest : UserControl
    {
        // cuva podatak o putanji do request fajla ako je otvorena forma preko continue od opcije "Create Request"
        private string signedRequestFileNamePath = "";
        private string requestFileNamePrivateKeyPath = "";
        private string CARootFileNamePath = "";
        private string CARootPubKeyFileNamePath = "";
        public GenerateSignRequest()
        {
            InitializeComponent();

            btnContinue.IsEnabled = false;
            signedRequestFileNamePath = "";
            requestFileNamePrivateKeyPath = "";
            CARootFileNamePath = "";

            //tbPathCsr.Text = @"G:\_PKI\GenCert\GenCert\bin\Debug\Fortum\fortum_request.csr";
            //requestFileNamePrivateKeyPath = @"G:\_PKI\GenCert\GenCert\bin\Debug\Fortum\fortum_private.key";
            //tbPathPrivateKey.Text = CARootFileNamePath = @"G:\_PKI\GenCert\GenCert\bin\Debug\CA\ABC_CA_signed.pfx";
            //CARootPubKeyFileNamePath = @"G:\_PKI\GenCert\GenCert\bin\Debug\CA\ABC_CA_public.cer";
            //tbPathGenerateCer.Text = @"G:\_PKI\GenCert\GenCert\bin\Debug\Fortum";
            //pbPassword.Password = "1234";
            //tbCertFileName.Text = "generated";
        }   
        public GenerateSignRequest(string certFriendlyName) : this()
        {
            dpEndDate.SelectedDate = DateTime.Now.AddYears(1);
            tbFriendlyName.Text = certFriendlyName;
        }

        /// <summary>
        /// Generate certificate base on certificate request file (.csr)
        /// </summary>
        public GenerateSignRequest(string certFriendlyName, 
                                   string requestFileNamePathRF="",
                                   string requestFileNamePrivateKeyPathRF = "",
                                   string CARootFileNamePathRF = "",
                                   string CARootPubKeyFileNamePathRF = "",
                                   string CARootPasswordRF ="") : this()
        {

            dpEndDate.SelectedDate = DateTime.Now.AddYears(1);
            tbFriendlyName.Text = certFriendlyName;

            if (!String.IsNullOrEmpty(requestFileNamePathRF))
            {
                requestFileNamePrivateKeyPath = requestFileNamePathRF;
                tbPathCsr.Text = requestFileNamePathRF;
                tbPathGenerateCer.Text = System.IO.Path.GetDirectoryName(requestFileNamePathRF) ;
                tbCertFileName.Text = System.IO.Path.GetFileNameWithoutExtension(requestFileNamePathRF);
            }
            if (!String.IsNullOrEmpty(CARootFileNamePathRF))
            {
                CARootFileNamePath = CARootFileNamePathRF;
                tbPathPrivateKey.Text = CARootFileNamePathRF;
            }
            if (!String.IsNullOrEmpty(CARootPubKeyFileNamePathRF))
            {
                CARootPubKeyFileNamePath = CARootPubKeyFileNamePathRF;
            }
            if (!String.IsNullOrEmpty(CARootPasswordRF))
            {
                pbPassword.Password = CARootPasswordRF;
                //CARootPassword = CARootPasswordRF;
            }
            #region test data
            //tbPathCsr.Text = @"G:\_PKI\GenCert\GenCert\bin\Debug\Fortum\fortum_request.csr";
            ////tbPathPrivateKey.Text = @"J:\PKI\GenCert\GenCert\bin\Debug\test\EPSA_CA_private.key";
            //tbPathPrivateKey.Text = @"G:\_PKI\GenCert\GenCert\bin\Debug\CA\ABC_CA_signed.pfx";
            //tbPathGenerateCer.Text = @"G:\_PKI\GenCert\GenCert\bin\Debug\Fortum"; // System.Environment.CurrentDirectory;
            //tbCertFileName.Text = "client_generate_cer";
            //pbPassword.Password = "1234";
            #endregion

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
                    //Thread.Sleep(1 * 1000);

                    // Open document
                    tbPathCsr.Text = dlg.FileName;
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

            #region check errors
            if (string.IsNullOrEmpty(tbFriendlyName.Text))
            {
                errorMessages.Add("You MUST enter certificate Frendly Name.");
            }

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
                //string fileName = tbPathName.Text + "\\" + "filename.txt";
                //new FileInfo(fileName).Directory?.Create();
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
                //if (!Directory.Exists(tbPathGenerateCer.Text))
                //{
                //    errorMessages.Add("Folder " + tbPathGenerateCer.Text + " DOES NOT exist. Please check folder path");
                //}
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
            DateTime startDate = dpStartDate.DisplayDate;
            DateTime endDate = dpEndDate.DisplayDate;

            GenerateCerFile(tbPathCsr.Text,
                            tbPathPrivateKey.Text,
                            tbPathGenerateCer.Text + "\\" + tbCertFileName.Text + ".cer",
                            pbPassword.Password, tbFriendlyName.Text,
                            startDate, endDate);
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
            string password, string friendlyName,
            DateTime startDate, DateTime endDate)
        {
            #region LoadCertificate

            // read public & private key from file
            AsymmetricKeyParameter privateKey = null;
            AsymmetricKeyParameter publicKey = null;

            System.Security.Cryptography.X509Certificates.X509Certificate2 issuerCertificate = null;
            Org.BouncyCastle.X509.X509Certificate issuerCertificateX509 = null;

            // Ovo NE radi
            //issuerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
            //        privateKeyFile,
            //        password
            //        );

            // Ovo RADI
            issuerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                    privateKeyFile,
                    password,
                    System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable
                    );

            // This doesn't work for selfsign certificate
            //bool isOK = issuerCertificate.Verify();

            bool isHasPrivateKey = issuerCertificate.HasPrivateKey;
            DateTime noAfter = issuerCertificate.NotAfter;
            DateTime noBefore = issuerCertificate.NotBefore;
            X509ExtensionCollection x509extensions = issuerCertificate.Extensions;

            int errorNum=0;
            X509CertificateParser parser = new X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate bouncyCertificate = parser.ReadCertificate(issuerCertificate.RawData);
            BasicConstraints basicConstraints = null;
            bool isCa = false;
            Asn1OctetString str = bouncyCertificate.GetExtensionValue(new DerObjectIdentifier("2.5.29.19"));
            if (str != null)
            {
                basicConstraints = BasicConstraints.GetInstance(
                    X509ExtensionUtilities.FromExtensionValue(str));
                if (basicConstraints != null)
                {
                    isCa = basicConstraints.IsCA();
                }
            }

            if (!isCa)
            {
                errorNum++;
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "Loaded CA file: " + privateKeyFile + " IS NOT CA authority certificate file!" + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
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
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "File with CA certificate DOES NOT have a private key." + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
            }
            if (noBefore > startDate)
            {
                errorNum++;
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "File with CA certificate start date: "+startDate.ToLocalTime()+" DOES NOT valid value. Certificate start date is: "+ noBefore.ToLocalTime() + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
            }
            if (noAfter < endDate)
            {
                errorNum++;
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "File with CA certificate end date: " + endDate.ToLocalTime() + " DOES NOT valid value. Certificate end date is: " + noAfter.ToLocalTime() + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
            }

            if (errorNum>0)
            {
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "File with CA certificate has error!!!" + "\n";
                tbOutputMessageBox.Foreground = bckForeground;

                return;
            }
            bool isOk = issuerCertificate.Verify();

            AsymmetricCipherKeyPair issuerKeyPairTmp = DotNetUtilities.GetKeyPair(issuerCertificate.PrivateKey);
            privateKey = issuerKeyPairTmp.Private;
            publicKey = issuerKeyPairTmp.Public;

            issuerCertificateX509 = new Org.BouncyCastle.X509.X509CertificateParser().ReadCertificate(issuerCertificate.GetRawCertData());
            issuerCertificateX509.Verify(publicKey);

            Org.BouncyCastle.X509.X509Certificate x509 = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(issuerCertificate);
            x509.Verify(publicKey);
            x509.CheckValidity(startDate);

            #endregion

            // Read certificate request .csr file
            Pkcs10CertificationRequest cerRequest = null;
            try
            {

                String input_data = File.ReadAllText(certRequestFile);
                StringReader sr = new StringReader(input_data);
                PemReader pr = new PemReader(sr);
                cerRequest = (Pkcs10CertificationRequest)pr.ReadObject();

                tbOutputMessageBox.Text += "Verify file with certificate request : " + certRequestFile + "\n";
                bool requestIsOK = cerRequest.Verify();
                if (requestIsOK)
                {
                    tbOutputMessageBox.Text += "File with certificate request : " + certRequestFile + " is OK." + "\n";
                }
                else
                {
                    tbOutputMessageBox.Text += "File with certificate request : " + certRequestFile + " NOT valid." + "\n";
                    return;
                }
            }
            catch (Exception ex)
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR reading certificate request file (.csr)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
                return;
            }

            Org.BouncyCastle.X509.X509Certificate genCert = GenerateSignedCertificate(
                    cerRequest,
                    x509,
                    issuerKeyPairTmp,
                    startDate, endDate);

            try
            {
                File.WriteAllBytes(System.IO.Path.ChangeExtension(generateSignedCertificateFile, ".cer"), genCert.GetEncoded());
                tbOutputMessageBox.Text += "Certificate file: "+ generateSignedCertificateFile + " sucessfully saved." + "\n";

                signedRequestFileNamePath = generateSignedCertificateFile;
                btnContinue.IsEnabled = true;
            }
            catch (Exception)
            {
                tbOutputMessageBox.Text += "Certificate file sucessfully generated." + "\n";
            }

            #region Public Key
            //try
            //{
            //    var store = new Pkcs12Store();
            //    string friendlyName1 = issuerCertificateX509.SubjectDN.ToString();
            //    var certificateEntry = new X509CertificateEntry(issuerCertificateX509);
            //    store.SetCertificateEntry(friendlyName1, certificateEntry);
            //    store.SetKeyEntry(friendlyName1, new AsymmetricKeyEntry(privateKey), new[] { certificateEntry });

            //    var stream = new MemoryStream();
            //    var random1 = GetSecureRandom();
            //    store.Save(stream, "password".ToCharArray(), random1);

            //    //Verify that the certificate is valid.
            //    var convertedCertificate = new X509Certificate2(stream.ToArray(), "password", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            //    //Write the file.
            //    File.WriteAllBytes(generateSignedCertificateFile, stream.ToArray());

            //    File.WriteAllBytes(System.IO.Path.ChangeExtension(generateSignedCertificateFile, ".cer"), genCert.GetEncoded());

            //    //using (TextWriter tw = new StreamWriter(outputPublicKeyName))
            //    //{
            //    //    PemWriter pw = new PemWriter(tw);
            //    //    pw.WriteObject(subjectKeyPair.Public);
            //    //    tw.Flush();
            //    //}

            //    tbOutputMessageBox.Text += "File with private key: " + generateSignedCertificateFile + " sucessfully generated." + "\n";
            //}
            //catch (Exception ex)
            //{
            //    var metroWindow = (Application.Current.MainWindow as MetroWindow);
            //    await metroWindow.ShowMessageAsync("Info Warning",
            //         "ERROR creating certificate private key file (.key)" + "\n" +
            //         "Error: " + ex.Source + " " + ex.Message,
            //         MessageDialogStyle.Affirmative);
            //    return;
            //}

            //StringBuilder publicKeyStrBuilder = new StringBuilder();
            //PemWriter publicKeyPemWriter = new PemWriter(new StringWriter(publicKeyStrBuilder));
            //publicKeyPemWriter.WriteObject(genCert.GetPublicKey());
            //publicKeyPemWriter.Writer.Flush();

            //string publicKey = publicKeyStrBuilder.ToString();
            //try
            //{
            //    using (TextWriter tw = new StreamWriter(generateSignedCertificateFile))
            //    {
            //        PemWriter pw = new PemWriter(tw);
            //        pw.WriteObject(genCert.GetPublicKey());
            //        tw.Flush();
            //    }

            //    tbOutputMessageBox.Text += "File with private key: " + generateSignedCertificateFile + " sucessfully generated." + "\n";
            //}
            //catch (Exception ex)
            //{
            //    var metroWindow = (Application.Current.MainWindow as MetroWindow);
            //    await metroWindow.ShowMessageAsync("Info Warning",
            //         "ERROR creating certificate private key file (.key)" + "\n" +
            //         "Error: " + ex.Source + " " + ex.Message,
            //         MessageDialogStyle.Affirmative);
            //    return;
            //}
            #endregion Public Key

        }

        /// <summary>
        /// Enroll certificate file base on request
        /// </summary>
        /// <param name="csr"></param>
        /// <param name="rootCert"></param>
        /// <param name="issuerKeyPair"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private Org.BouncyCastle.X509.X509Certificate GenerateSignedCertificate(
                    Pkcs10CertificationRequest csr,
                    Org.BouncyCastle.X509.X509Certificate rootCert,
                    AsymmetricCipherKeyPair issuerKeyPair,
                    DateTime startDate, DateTime endDate)
        {
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();

            //List<ExtensionsItem> extensions = null;

            certGen.SetSerialNumber(BigInteger.One);

            certGen.SetIssuerDN(rootCert.SubjectDN);

            certGen.SetNotBefore(startDate);
            certGen.SetNotAfter(endDate);

            CertificationRequestInfo info = csr.GetCertificationRequestInfo();
            certGen.SetSubjectDN(info.Subject);

            certGen.SetPublicKey(csr.GetPublicKey());

            var sigAlg = csr.Signature;
            var sigAlg1 = csr.SignatureAlgorithm;

            certGen.SetSignatureAlgorithm("SHA1WithRSAEncryption");


            // Add certificate extensions
            Asn1Set attributes = csr.GetCertificationRequestInfo().Attributes;
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

                            // !!! NOT working !!!
                            //certGen.AddExtension(oid, ext.IsCritical, ext.Value);

                            //OK
                            certGen.AddExtension(oid, ext.IsCritical, ext.Value, true);
                        }
                    }
                }
            }

            Org.BouncyCastle.X509.X509Certificate issuedCert = null;
            try
            {
                issuedCert = certGen.Generate(issuerKeyPair.Private);
                tbOutputMessageBox.Text += "Certificate file sucessfully generated." + "\n";
            }
            catch (Exception ex)
            {
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "Error, generate certificate file." + "\n" + "ERROR: "+ex.GetHashCode().ToString()+" "+ex.Message+"\n";
                tbOutputMessageBox.Foreground = bckForeground;
            }

            try
            {
                tbOutputMessageBox.Text += "Check if generated certificate file is valid, plase wait ..." + "\n";
                issuedCert.CheckValidity(DateTime.UtcNow);
                tbOutputMessageBox.Text += "Generate certificate file is valid." + "\n";
            }
            catch (Exception ex)
            {
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "Error, generated certificate file is INVALID." + "\n" + "ERROR: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
            }

            try
            {
                tbOutputMessageBox.Text += "Verify generated certificate file, plase wait ..." + "\n";
                issuedCert.Verify(issuerKeyPair.Public);
                tbOutputMessageBox.Text += "Generate certificate file verification is OK." + "\n";
            }
            catch (Exception ex)
            {
                Brush bckForeground = tbOutputMessageBox.Foreground;
                tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                tbOutputMessageBox.Text += "Error, generated certificate file verification is INVALID." + "\n" + "ERROR: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n";
                tbOutputMessageBox.Foreground = bckForeground;
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
                    "Generated CA root certificate: " + CARootFileNamePath + "\n" +
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

                    GeneratePFX gr = new GeneratePFX(mw.certFriendlyName, signedRequestFileNamePath,
                                                     requestFileNamePrivateKeyPath, CARootPubKeyFileNamePath);

                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);

                    mw.sbiSelectedMenuOption.Content = header0;

                }
                else if (result == MessageDialogResult.Negative)
                {
                }
            }
        }

    }
}
