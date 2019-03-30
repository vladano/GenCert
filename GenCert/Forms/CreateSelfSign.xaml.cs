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

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GenerateRequest.xaml
    /// </summary>
    public partial class CreateSelfSign : UserControl
    {
        public List<SubjectName> alternativSubjectNames { get; set; }
        private static readonly IDictionary algorithms = Platform.CreateHashtable();

        public CreateSelfSign()
        {
            InitializeComponent();

            btnContinue.IsEnabled = false;
        }

        /// <summary>
        /// Browse for folder to store generate certificate file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

                dlg.Description = "Select folder to save generate files";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbPathName.Text = dlg.SelectedPath;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Start process of creating selfsign certificate file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            List<string> errorMessages = new List<string>();

            tbOutputMessageBox.Text = "";

            #region check for errors
            if (string.IsNullOrEmpty(tbFriendlyName.Text))
            {
                errorMessages.Add("You MUST enter certificate Friendly Name.");
            }

            if (string.IsNullOrEmpty(tbDomainName.Text))
            {
                errorMessages.Add("You MUST enter Domain Name.");
            }
            ComboBoxItem cbThisIsCACert = (ComboBoxItem)cbIsCACert.SelectedItem;
            string txtThisIsCACert = cbThisIsCACert.Content.ToString();

            if (string.IsNullOrEmpty(tbCountryCode.Text))
            {
                errorMessages.Add("You MUST enter Country Code.");
            }
            if (string.IsNullOrEmpty(tbStateOrProvince.Text))
            {
                errorMessages.Add("You MUST enter name for State or Province.");
            }
            if (string.IsNullOrEmpty(tbLocalityName.Text))
            {
                errorMessages.Add("You MUST enter Location name.");
            }
            if (string.IsNullOrEmpty(tbOrganization.Text))
            {
                errorMessages.Add("You MUST enter Organization name.");
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

            if (string.IsNullOrEmpty(tbPathName.Text))
            {
                errorMessages.Add("You MUST enter Path to store generate files.");
            }
            else
            {
                if (!Directory.Exists(tbPathName.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(tbPathName.Text);
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add("Can NOT create directory path: " + tbPathName.Text);
                    }
                }
            }

            if (string.IsNullOrEmpty(tbPrivateKeyName.Text))
            {
                errorMessages.Add("You MUST enter file name to save certificate private key (.key extension).");
            }
            else
            {
                if (File.Exists(tbPathName.Text + "\\" + tbPrivateKeyName.Text+".key"))
                {
                    errorMessages.Add("File " + tbPathName.Text + "\\" + tbPrivateKeyName.Text + ".key" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }

            if (string.IsNullOrEmpty(tbPublicKeyName.Text))
            {
                errorMessages.Add("You MUST enter file name to save certificate public key (.cer extension).");
            }
            else
            {
                if (File.Exists(tbPathName.Text + "\\" + tbPublicKeyName.Text + ".cer"))
                {
                    errorMessages.Add("File " + tbPathName.Text + "\\" + tbPublicKeyName.Text + ".csr" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }

            if (string.IsNullOrEmpty(tbSignedCertName.Text))
            {
                errorMessages.Add("You MUST enter file name to save signed certificate file (.pfx extension).");
            }
            else
            {
                if (File.Exists(tbPathName.Text + "\\" + tbSignedCertName.Text + ".pfx"))
                {
                    errorMessages.Add("File " + tbPathName.Text + "\\" + tbSignedCertName.Text + ".pfx" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }
            if (string.IsNullOrEmpty(pbPassword.Password))
            {
                errorMessages.Add("You MUST enter password for export private key from certificate file.");
            }
            #endregion

            if (errorMessages.Count>0)
            {
                string errorMessage = "";
                for (int i = 0; i < errorMessages.Count; i++)
                {
                    if (i==0)
                    {
                        errorMessage += errorMessages[i];
                    }
                    else
                    {
                        errorMessage += "\n"+errorMessages[i];
                    }
                }
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Error",
                     errorMessage,
                     MessageDialogStyle.Affirmative);
                return;
            }
            // Proceed with generating certificate 
            else
            {
                ComboBoxItem typeItem = (ComboBoxItem)cbKeyLength.SelectedItem;
                string stKeyLength = typeItem.Content.ToString();

                int keyLength = Int32.Parse(stKeyLength);
                DateTime startDate = dpStartDate.DisplayDate;
                DateTime endDate = dpEndDate.DisplayDate;

                tbOutputMessageBox.Text = "";
                bool isThisCACert = false;
                if (txtThisIsCACert=="Yes")
                {
                    isThisCACert = true;
                }
                GenerateCertificateRequest(tbDomainName.Text, isThisCACert, keyLength, cbSinatuteAlgorithm.Text,
                    tbCountryCode.Text, tbStateOrProvince.Text, tbLocalityName.Text,
                    tbOrganization.Text, startDate, endDate, tbPathName.Text,
                    tbPrivateKeyName.Text+".key", tbPublicKeyName.Text+".cer", tbSignedCertName.Text + ".pfx",
                    pbPassword.Password, tbFriendlyName.Text);
            }
        }
        /// <summary>
        /// Generate certificate request file and private key file
        /// </summary>
        /// <param name="commonName"></param>
        /// <param name="isCertificateAuthority"></param>
        /// <param name="keyLength"></param>
        /// <param name="signatureAlgorithm"></param>
        /// <param name="countryCode"></param>
        /// <param name="stateOrProvinceName"></param>
        /// <param name="localityName"></param>
        /// <param name="organization"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="path"></param>
        /// <param name="privateKeyName"></param>
        /// <param name="publicKeyName"></param>
        /// <param name="signedCertName"></param>
        /// <param name="password"></param>
        private void GenerateCertificateRequest(string commonName, bool isCertificateAuthority, int keyLength, 
            string signatureAlgorithm, string countryCode, string stateOrProvinceName, string localityName, 
            string organization, DateTime startDateTime, DateTime endDateTime, string path, 
            string privateKeyName, string publicKeyName, string signedCertName, string password, string certFriendlyName)
        {

            var random = GetSecureRandom();
            AsymmetricCipherKeyPair subjectKeyPair = GenerateKeyPair(random, keyLength);

            // It's self-signed, so these are the same.
            var issuerKeyPair = subjectKeyPair;

            var serialNumber = GenerateSerialNumber(random);
            var issuerSerialNumber = serialNumber; // Self-signed, so it's the same serial number.

            KeyPurposeID[] usages=null;
            // is this CA root certificate ?
            if (!isCertificateAuthority)
            {
                usages = new[] {
                    KeyPurposeID.IdKPServerAuth
                };
            }
            else // CA root certificate
            {
                usages = new[] {
                    KeyPurposeID.IdKPServerAuth,
                    KeyPurposeID.IdKPClientAuth,
                    KeyPurposeID.IdKPCodeSigning,
                    KeyPurposeID.IdKPIpsecEndSystem,
                    KeyPurposeID.IdKPIpsecTunnel,
                    KeyPurposeID.IdKPIpsecUser,
                    KeyPurposeID.IdKPOcspSigning,
                    KeyPurposeID.IdKPTimeStamping
                };
            }
            string outputPrivateKeyFileName = path+ "\\" + privateKeyName;
            string outputPublicKeyFileName = path + "\\" + publicKeyName;
            string outputSignedCertFileName = path + "\\" + signedCertName;

            GenerateCertificate(random,
                                subjectKeyPair, 
                                serialNumber,
                                commonName,
                                issuerKeyPair,
                                issuerSerialNumber, 
                                isCertificateAuthority,
                                usages,
                                signatureAlgorithm,
                                countryCode,
                                stateOrProvinceName,
                                localityName,
                                organization,
                                startDateTime,
                                endDateTime,
                                outputPrivateKeyFileName,
                                outputPublicKeyFileName,
                                outputSignedCertFileName,
                                password,
                                certFriendlyName
                            );
        }

        /// <summary>
        /// Generate certificate request and private key file
        /// </summary>
        /// <param name="random"></param>
        /// <param name="subjectKeyPair"></param>
        /// <param name="subjectSerialNumber"></param>
        /// <param name="commonName"></param>
        /// <param name="issuerKeyPair"></param>
        /// <param name="issuerSerialNumber"></param>
        /// <param name="isCertificateAuthority"></param>
        /// <param name="usages"></param>
        /// <param name="signatureAlgorithm"></param>
        /// <param name="countryCode"></param>
        /// <param name="stateOrProvinceName"></param>
        /// <param name="localityName"></param>
        /// <param name="organization"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <param name="outputPrivateKeyName"></param>
        /// <param name="outputPublicKeyName"></param>
        /// <param name="outputSignedKeyName"></param>
        /// <param name="password"></param>
        private async void GenerateCertificate(SecureRandom random, 
                                                    AsymmetricCipherKeyPair subjectKeyPair, 
                                                    BigInteger subjectSerialNumber,
                                                    string commonName,
                                                    AsymmetricCipherKeyPair issuerKeyPair, 
                                                    BigInteger issuerSerialNumber, 
                                                    bool isCertificateAuthority, 
                                                    KeyPurposeID[] usages, 
                                                    string signatureAlgorithm, 
                                                    string countryCode, 
                                                    string stateOrProvinceName, 
                                                    string localityName, 
                                                    string organization, 
                                                    DateTime startDateTime, 
                                                    DateTime endDateTime, 
                                                    string outputPrivateKeyName, 
                                                    string outputPublicKeyName,
                                                    string outputSignedKeyName,
                                                    string password,
                                                    string certFriendlyName
                                                    )
        {
            createSelfSign.IsEnabled = false;
            progressring.Visibility = Visibility.Visible;
            await System.Threading.Tasks.TaskEx.Delay(1000);

            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            certificateGenerator.SetSerialNumber(subjectSerialNumber);
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);

            IDictionary attrs = new Hashtable();
            attrs[X509Name.CN] = commonName;
            attrs[X509Name.C] = countryCode;
            attrs[X509Name.ST] = stateOrProvinceName;
            attrs[X509Name.L] = localityName;
            attrs[X509Name.O] = organization;

            IList ord = new ArrayList();
            ord.Add(X509Name.CN);
            ord.Add(X509Name.C);
            ord.Add(X509Name.ST);
            ord.Add(X509Name.L);
            ord.Add(X509Name.O);

            X509Name issuerDN = new X509Name(ord, attrs);
            certificateGenerator.SetIssuerDN(issuerDN);

            certificateGenerator.SetNotBefore((DateTime)startDateTime);
            certificateGenerator.SetNotAfter((DateTime)endDateTime);
            certificateGenerator.SetSubjectDN(new X509Name(ord, attrs));

            // The subject's public key goes in the certificate.
            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            AddAuthorityKeyIdentifier(certificateGenerator, issuerDN, issuerKeyPair, issuerSerialNumber);

            AddSubjectKeyIdentifier(certificateGenerator, subjectKeyPair);

            bool isCritical = true;
            if (!isCertificateAuthority)
            {
                isCritical = false;
            }
            certificateGenerator.AddExtension(
                            X509Extensions.BasicConstraints.Id, isCritical, new BasicConstraints(isCertificateAuthority));

            if (usages != null && usages.Any())
                AddExtendedKeyUsage(certificateGenerator, usages);

            await System.Threading.Tasks.TaskEx.Delay(1000);
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(subjectKeyPair.Private);

            int errorNum = 0;
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                certificate.CheckValidity(startDateTime);
            }
            catch (Exception ex)
            {
                errorNum++;

                progressring.Visibility = Visibility.Hidden;
                createSelfSign.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "Generate certificate Check validity period error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("ERROR",
                     "Generate certificate Check validity period  error: " + ex.GetHashCode().ToString() + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
            }

            try
            {
                certificate.Verify(subjectKeyPair.Public);
            }
            catch (Exception ex)
            {
                errorNum++;

                progressring.Visibility = Visibility.Hidden;
                createSelfSign.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "Generate certificate Verification error: " + ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("ERROR",
                     "Generate certificate Verification  error: " + ex.GetHashCode().ToString() + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
            }

            if (errorNum>0)
            {
                return;
            }

            #region Save Public Key
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                File.WriteAllBytes(System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer"), certificate.GetEncoded());
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                createSelfSign.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "ERROR to save generated public key to file: " + System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer") + "\n" +
                        ex.GetHashCode().ToString() + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("ERROR",
                     "ERROR to save generated public key to file: " + System.IO.Path.ChangeExtension(outputPublicKeyName, ".cer") + "\n" +
                    ex.GetHashCode().ToString() + " " + ex.Message,
                     MessageDialogStyle.Affirmative);

                return;
            }
            #endregion

            #region Private Key
            await System.Threading.Tasks.TaskEx.Delay(1000);

            StringBuilder privateKeyStrBuilder = new StringBuilder();
            PemWriter privateKeyPemWriter = new PemWriter(new StringWriter(privateKeyStrBuilder));
            privateKeyPemWriter.WriteObject(subjectKeyPair.Private);
            privateKeyPemWriter.Writer.Flush();

            string privateKey = privateKeyStrBuilder.ToString();
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                using (TextWriter tw = new StreamWriter(outputPrivateKeyName))
                {
                    PemWriter pw = new PemWriter(tw);
                    pw.WriteObject(subjectKeyPair.Private);
                    tw.Flush();
                }

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "File with private key: " + outputPrivateKeyName + " sucessfully generated." + "\n",
                    Foreground = System.Windows.Media.Brushes.Green
                });
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                createSelfSign.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "ERROR to save file with private key: " + outputPrivateKeyName + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR creating certificate private key file (.key)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);

                return;
            }
            #endregion Private Key

            #region signed cert file
            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);

                X509Certificate2 convCert = ConvertCertificate(certificate, subjectKeyPair, random, pbPassword.Password, certFriendlyName);

                bool isHasPrivateKey = convCert.HasPrivateKey;

                // This doesn't work for selfsign certificate
                //bool isOK = convCert.Verify();

                errorNum = 0;
                if (!isHasPrivateKey)
                {
                    errorNum++;

                    progressring.Visibility = Visibility.Hidden;
                    createSelfSign.IsEnabled = true;

                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "Error, generated certificate DOES NOT have a private key!!!" + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });
                }

                // This doesn't work for selfsign certificate
                //if (!isOK)
                //{
                //    errorNum++;
                //    Brush bckForeground = tbOutputMessageBox.Foreground;
                //    tbOutputMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                //    tbOutputMessageBox.Text += "Error, generated certificate NOT valid!!!" + "\n";
                //    tbOutputMessageBox.Foreground = bckForeground;
                //}

                if (errorNum>0)
                {
                    return;
                }

                await System.Threading.Tasks.TaskEx.Delay(1000);

                // This password is the one attached to the PFX file. Use 'null' for no password.
                var bytes = convCert.Export(X509ContentType.Pfx, password);
                File.WriteAllBytes(outputSignedKeyName, bytes);

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "Certificate file with private key: " + outputSignedKeyName + " sucessfully generated." + "\n",
                    Foreground = System.Windows.Media.Brushes.Green
                });

                if (cbIsCACert.SelectedIndex == 0) // root CA certificate
                {
                    btnContinue.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                createSelfSign.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "ERROR creating certificate file with private key (.pfx)" + "\n" + "Error: " + ex.Source + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR creating certificate file with private key (.pfx)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);

                return;
            }
            #endregion

            progressring.Visibility = Visibility.Hidden;
            createSelfSign.IsEnabled = true;
        }

        /// <summary>
        /// Generate random number for key generator
        /// </summary>
        /// <returns></returns>
        private static SecureRandom GetSecureRandom()
        {
            // Since we're on Windows, we'll use the CryptoAPI one (on the assumption
            // that it might have access to better sources of entropy than the built-in
            // Bouncy Castle ones):
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            return random;
        }

        /// <summary>
        /// The certificate needs a serial number. This is used for revocation,
        /// and usually should be an incrementing index (which makes it easier to revoke a range of certificates).
        /// Since we don't have anywhere to store the incrementing index, we can just use a random number.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private static BigInteger GenerateSerialNumber(SecureRandom random)
        {
            var serialNumber =
                BigIntegers.CreateRandomInRange(
                    BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            return serialNumber;
        }

        /// <summary>
        /// Generate a key pair.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="strength">The key length in bits. For RSA, 2048 bits should be considered the minimum acceptable these days.</param>
        /// <returns></returns>
        private static AsymmetricCipherKeyPair GenerateKeyPair(SecureRandom random, int strength)
        {
            var keyGenerationParameters = new KeyGenerationParameters(random, strength);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            return subjectKeyPair;
        }

        /// <summary>
        /// Add the Authority Key Identifier. According to http://www.alvestrand.no/objectid/2.5.29.35.html, this
        /// identifies the public key to be used to verify the signature on this certificate.
        /// In a certificate chain, this corresponds to the "Subject Key Identifier" on the *issuer* certificate.
        /// The Bouncy Castle documentation, at http://www.bouncycastle.org/wiki/display/JA1/X.509+Public+Key+Certificate+and+Certification+Request+Generation,
        /// shows how to create this from the issuing certificate. Since we're creating a self-signed certificate, we have to do this slightly differently.
        /// </summary>
        /// <param name="certificateGenerator"></param>
        /// <param name="issuerDN"></param>
        /// <param name="issuerKeyPair"></param>
        /// <param name="issuerSerialNumber"></param>
        private static void AddAuthorityKeyIdentifier(X509V3CertificateGenerator certificateGenerator,
                                                      X509Name issuerDN,
                                                      AsymmetricCipherKeyPair issuerKeyPair,
                                                      BigInteger issuerSerialNumber)
        {
            var authorityKeyIdentifierExtension = new AuthorityKeyIdentifier(
                    SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(issuerKeyPair.Public),
                    new GeneralNames(new GeneralName(issuerDN)),
                    issuerSerialNumber);

            certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier.Id, false, authorityKeyIdentifierExtension);
        }

        /// <summary>
        /// Add the "Extended Key Usage" extension, specifying (for example) "server authentication".
        /// </summary>
        /// <param name="certificateGenerator"></param>
        /// <param name="usages"></param>
        private static void AddExtendedKeyUsage(X509V3CertificateGenerator certificateGenerator, KeyPurposeID[] usages)
        {
            certificateGenerator.AddExtension(
                X509Extensions.ExtendedKeyUsage.Id, false, new ExtendedKeyUsage(usages));
        }

        /// <summary>
        /// Add the Subject Key Identifier.
        /// </summary>
        /// <param name="certificateGenerator"></param>
        /// <param name="subjectKeyPair"></param>
        private static void AddSubjectKeyIdentifier(X509V3CertificateGenerator certificateGenerator,
                                                    AsymmetricCipherKeyPair subjectKeyPair)
        {
            var subjectKeyIdentifierExtension =
                new SubjectKeyIdentifier(
                    SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(subjectKeyPair.Public));
            certificateGenerator.AddExtension(
                X509Extensions.SubjectKeyIdentifier.Id, false, subjectKeyIdentifierExtension);
        }

        /// <summary>
        /// Convert the Bouncy Castle certificate to a .NET certificate (.pfx)
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="subjectKeyPair"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static X509Certificate2 ConvertCertificate(Org.BouncyCastle.X509.X509Certificate certificate,
                                                           AsymmetricCipherKeyPair subjectKeyPair,
                                                           SecureRandom random,
                                                           string password,
                                                           string friendlyName)
        {
            // Now to convert the Bouncy Castle certificate to a .NET certificate.
            // See http://web.archive.org/web/20100504192226/http://www.fkollmann.de/v2/post/Creating-certificates-using-BouncyCastle.aspx
            // ...but, basically, we create a PKCS12 store (a .PFX file) in memory, and add the public and private key to that.
            var store = new Pkcs12Store();

            // Add the certificate.
            var certificateEntry = new X509CertificateEntry(certificate);
            store.SetCertificateEntry(friendlyName, certificateEntry);

            // Add the private key.
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(subjectKeyPair.Private), new[] { certificateEntry });

            // Convert it to an X509Certificate2 object by saving/loading it from a MemoryStream.
            // It needs a password. Since we'll remove this later, it doesn't particularly matter what we use.
            var stream = new MemoryStream();
            store.Save(stream, password.ToCharArray(), random);

            var convertedCertificate =
                new X509Certificate2(stream.ToArray(),
                                     password,
                                     X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            return convertedCertificate;
        }
        /// <summary>
        /// Open wizard what will be next step
        /// If you generate self-sign CA root certificate, next step can be to open form form issue certificate base on certificate request file ganerated 
        /// using menu option "Create Request"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            string CARootFileNamePath = tbPathName.Text;
            string CARootPubKeyFileNamePath = tbPathName.Text+"\\"+ tbPublicKeyName.Text;

            if (!String.IsNullOrEmpty(CARootFileNamePath) && File.Exists(CARootFileNamePath))
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
                    "Generated CA root certificate: " + tbSignedCertName.Text + "\n" +
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

                    CertData.cerPFXFilePath = tbPathName.Text + "\\" + tbPrivateKeyName.Text;

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
    }
}
