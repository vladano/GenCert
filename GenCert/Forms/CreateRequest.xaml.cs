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
using Org.BouncyCastle.X509.Extension;
using Xceed.Wpf.Toolkit;
using Dragablz;
using System.Threading;

//****************************************************************************************************************
// KeyUsage i ExtendedKeyUsage
////* KeyUsage ::= BIT STRING {
//    * digitalSignature(0),
//     * nonRepudiation(1),
//     * keyEncipherment(2),
//     * dataEncipherment(3),
//     * keyAgreement(4),
//     * keyCertSign(5),
//     * cRLSign(6),
//     * encipherOnly(7),
//     * decipherOnly(8) }
//--------------------------------------------------------------------------
//Key usage extension Description
//-----------------------------------------------------
//Digital signature   Use when the public key is used with a digital signature mechanism to support security services other than non-repudiation, certificate signing, or
//                    CRL signing.A digital signature is often used for entity authentication and data origin authentication with integrity.
//Non-repudiation     Use when the public key is used to verify digital signatures used to provide a non-repudiation service.Non-repudiation protects against the signing
//                    entity falsely denying some action (excluding certificate or CRL signing).
//Key encipherment    Use when a certificate will be used with a protocol that encrypts keys.An example is S/MIME enveloping, where a fast(symmetric) key is encrypted
//                    with the public key from the certificate.SSL protocol also performs key encipherment.
//Data encipherment   Use when the public key is used for encrypting user data, other than cryptographic keys.
//Key agreement       Use when the sender and receiver of the public key need to derive the key without using encryption. This key can then can be used to encrypt messages
//                    between the sender and receiver.Key agreement is typically used with Diffie-Hellman ciphers.
//Certificate signing Use when the subject public key is used to verify a signature on certificates.This extension can be used only in CA certificates.
//CRL signing         Use when the subject public key is to verify a signature on revocation information, such as a CRL.
//Encipher only       Use only when key agreement is also enabled. This enables the public key to be used only for enciphering data while performing key agreement.
//Decipher only       Use only when key agreement is also enabled.This enables the public key to be used only for deciphering data while performing key agreement.
//--------------------------------------------------------------------------
//ExtendedKeyUsage
//    IdKP = "1.3.6.1.5.5.7.3";
//    AnyExtendedKeyUsage = new KeyPurposeID(X509Extensions.ExtendedKeyUsage.Id + ".0");
//    +IdKPServerAuth = new KeyPurposeID(IdKP + ".1");  -> (default option for App)
//    IdKPClientAuth = new KeyPurposeID(IdKP + ".2");
//    IdKPCodeSigning = new KeyPurposeID(IdKP + ".3");
//    IdKPEmailProtection = new KeyPurposeID(IdKP + ".4");
//    IdKPIpsecEndSystem = new KeyPurposeID(IdKP + ".5");
//    IdKPIpsecTunnel = new KeyPurposeID(IdKP + ".6");
//    IdKPIpsecUser = new KeyPurposeID(IdKP + ".7");
//    IdKPTimeStamping = new KeyPurposeID(IdKP + ".8");
//    IdKPOcspSigning = new KeyPurposeID(IdKP + ".9");
//--------------------------------------------------------------------------
//Extended key                    Enable for these key usage extensions
//---------------------------------------------------------------------------------------------
//TLS Web server authentication   Digital signature, key encipherment or key agreement
//TLS Web client authentication   Digital signature and/or key agreement
//Sign(downloadable) executable   Digital signature
//code
//Email protection                Digital signature, non-repudiation, and/or key encipherment or key agreement
//IPSEC End System                Digital signature and/or key encipherment or key agreement
//(host or router)
//IPSEC Tunnel                    Digital signature and/or key encipherment or key agreement
//IPSEC User                      Digital signature and/or key encipherment or key agreement
//Timestamping                    Digital signature, non-repudiation.
//--------------------------------------------------------------------------

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for GenerateRequest.xaml
    /// </summary>
    public partial class CreateRequest : UserControl
    {
        public List<SubjectName> alternativSubjectNames { get; set; }
        private static readonly IDictionary algorithms = Platform.CreateHashtable();

        #region local properties
        public static readonly DependencyProperty KeyUsageCCBDataProperty = DependencyProperty.Register(
            "KeyUsageCCBData", typeof(ObservableCollection<KeyUsageData>), typeof(MainWindow),
            new PropertyMetadata(new ObservableCollection<KeyUsageData>()));

        public ObservableCollection<KeyUsageData> KeyUsageCCBData
        {
            get { return (ObservableCollection<KeyUsageData>)GetValue(KeyUsageCCBDataProperty); }
            set { SetValue(KeyUsageCCBDataProperty, value); }
        }

        public static readonly DependencyProperty ExtendedKeyUsageCCBDataProperty = DependencyProperty.Register(
            "ExtendedKeyUsageCCBData", typeof(ObservableCollection<ExtendedKeyUsageData>), typeof(MainWindow),
            new PropertyMetadata(new ObservableCollection<ExtendedKeyUsageData>()));

        public ObservableCollection<ExtendedKeyUsageData> ExtendedKeyUsageCCBData
        {
            get { return (ObservableCollection<ExtendedKeyUsageData>)GetValue(ExtendedKeyUsageCCBDataProperty); }
            set { SetValue(ExtendedKeyUsageCCBDataProperty, value); }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public CreateRequest()
        {
            InitializeComponent();

            btnContinue.IsEnabled = false;

            ccbKeyUsage.ItemsSource = KeyUsageCCBData;
            ccbExtendedKeyUsage.ItemsSource = ExtendedKeyUsageCCBData;

            // Set default valu -> for web server authetification request
            ccbExtendedKeyUsage.SelectedItem = ExtendedKeyUsageCCBData[1];

            cbSinatuteAlgorithm.ItemsSource = algorithms;

            alternativSubjectNames = new List<SubjectName>();
            dgAlternativSubjectNames.ItemsSource = alternativSubjectNames;
            tbPathName.Text = System.Environment.CurrentDirectory;

            #region test data
            //tbDomainName.Text = "webserver.local";
            //tbCountryCode.Text = "11";
            //tbStateOrProvince.Text = "222";
            //tbLocalityName.Text = "333";
            //tbOrganization.Text = "444";
            //tbPathName.Text = @"G:\_PKI\GenCert\GenCert\bin\Debug\webserver";
            //tbPrivateKeyName.Text = "webserver_private";
            //tbRequestName.Text = "webserver_request";
            #endregion

        }

        #region buttnons
        /// <summary>
        /// Generate alternative names base on enetered domain name inside field tbDomainName
        /// For example for enetered value "webdmd.local", that will be:
        /// webserver.local
        /// s1.webserver.local
        /// s2.webserver.local
        /// s3.webserver.local
        /// s4.webserver.local
        /// and fill generated values to DataGrid element dgAlternativSubjectNames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private async void btnGenAlternativeNames_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(tbDomainName.Text))
            {
               var metroWindow = (Application.Current.MainWindow as MetroWindow);
               await metroWindow.ShowMessageAsync("Info Warning",
                    "To generate Alternativ web server names, you must\n"+
                    "fill field 'Domain Name' with appropriate domain name.",
                    MessageDialogStyle.Affirmative);
                return;
            }

            string[] domainNameArr = tbDomainName.Text.Split('.');
            string domainName = "";
            if (domainNameArr.Length==1)
            {
                domainName = domainNameArr[0];
            }
            else
            {
                domainName = tbDomainName.Text;
            }
            alternativSubjectNames = new List<SubjectName>();
            for (int i = 0; i < 5; i++)
            {
                if (i==0)
                {
                    alternativSubjectNames.Add(new SubjectName() { AlternativSubjectName = domainName });
                }
                else
                {
                    alternativSubjectNames.Add(new SubjectName() { AlternativSubjectName = "s" + i.ToString() + "." + domainName });
                }
            }
            dgAlternativSubjectNames.ItemsSource = alternativSubjectNames;
        }

        /// <summary>
        /// 
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
        #endregion

        /// <summary>
        /// Start process for generating private key file and certificate request file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            List<string> errorMessages = new List<string>();

            tbOutputMessageBox.Text = "";

            #region check errors
            ComboBoxItem cbThisIsCACert = (ComboBoxItem)cbIsCACert.SelectedItem;
            string txtThisIsCACert = cbThisIsCACert.Content.ToString();

            if (string.IsNullOrEmpty(tbDomainName.Text))
            {
                errorMessages.Add("You MUST enter Domain Name.");
            }

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
                        errorMessages.Add("Can NOT create directory path: "+ tbPathName.Text);
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
            if (string.IsNullOrEmpty(tbRequestName.Text))
            {
                errorMessages.Add("You MUST enter file name to save certificate request (.csr extension).");
            }
            else
            {
                if (File.Exists(tbPathName.Text + "\\" + tbRequestName.Text + ".csr"))
                {
                    errorMessages.Add("File " + tbPathName.Text + "\\" + tbRequestName.Text + ".csr" + " ALREADY exist. Please check file name and path or delete existing file");
                }
            }

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
            #endregion

            // Proceed with generating certificate 
            else
            {
                ComboBoxItem typeItem = (ComboBoxItem)cbKeyLength.SelectedItem;
                string stKeyLength = typeItem.Content.ToString();

                bool isThisCACert = false;
                if (txtThisIsCACert == "Yes")
                {
                    isThisCACert = true;
                }

                int keyLength = Int32.Parse(stKeyLength);
                string[] subjectAlternativeNames = new string[alternativSubjectNames.Count];
                int i = 0;
                foreach (var item in alternativSubjectNames)
                {
                    subjectAlternativeNames[i++] = item.AlternativSubjectName;
                }
                tbOutputMessageBox.Text = "";

                List<int> arrccbKeyUsage = new List<int>();
                List<string> arrExtendedKeyUsage = new List<string>();

                // if this is CA certificate request automatically add KeyUsage.KeyCertSign if NOT selected
                if (isThisCACert)
                {
                    /**
                     * The KeyUsage object.
                     * <pre>
                     *    id-ce-keyUsage OBJECT IDENTIFIER ::=  { id-ce 15 }
                     *
                     *    KeyUsage ::= BIT STRING {
                     *         digitalSignature        (0),
                     *         nonRepudiation          (1),
                     *         keyEncipherment         (2),
                     *         dataEncipherment        (3),
                     *         keyAgreement            (4),
                     *    +    keyCertSign             (5),
                     *         cRLSign                 (6),
                     *         encipherOnly            (7),
                     *         decipherOnly            (8) }
                     * </pre>
                     */
                }
                else
                {
                    if (ccbKeyUsage.SelectedItems.Count > 0)
                    {
                        foreach (KeyUsageData keyUsageData in ccbKeyUsage.SelectedItems)
                        {
                            arrccbKeyUsage.Add(keyUsageData.KeyUsageValue);
                        }
                    }
                    if (ccbExtendedKeyUsage.SelectedItems.Count > 0)
                    {
                        foreach (ExtendedKeyUsageData extendedKeyUsageData in ccbExtendedKeyUsage.SelectedItems)
                        {
                            arrExtendedKeyUsage.Add(extendedKeyUsageData.ExtendedKeyUsageValueName.Id);
                        }
                    }

                }

                GenerateCertificateRequest(tbDomainName.Text, subjectAlternativeNames, keyLength, cbSinatuteAlgorithm.Text,
                    tbCountryCode.Text, tbStateOrProvince.Text, tbLocalityName.Text, tbOrganization.Text,
                    arrccbKeyUsage, arrExtendedKeyUsage,
                    tbPathName.Text, tbPrivateKeyName.Text + ".key", tbRequestName.Text + ".csr", isThisCACert, pbPasswordPrivateKey.Password);
            }
        }

        /// <summary>
        /// Generate certificate request file and private key file
        /// </summary>
        /// <param name="commonName"></param>
        /// <param name="subjectAlternativeNames"></param>
        /// <param name="keyLength"></param>
        /// <param name="signatureAlgorithm"></param>
        /// <param name="countryCode"></param>
        /// <param name="stateOrProvinceName"></param>
        /// <param name="localityName"></param>
        /// <param name="organization"></param>
        /// <param name="arrccbKeyUsage"></param>
        /// <param name="arrExtendedKeyUsage"></param>
        /// <param name="path"></param>
        /// <param name="privateFileKeyName"></param>
        /// <param name="requestFileName"></param>
        private void GenerateCertificateRequest(string commonName, string[] subjectAlternativeNames, int keyLength,
            string signatureAlgorithm, string countryCode, string stateOrProvinceName, string localityName,
            string organization, List<int> arrccbKeyUsage, List<string> arrExtendedKeyUsage,
            string path,string privateFileKeyName, string requestFileName, bool isThisCACert=false, string password=null)
        {


            var random = GetSecureRandom();
            AsymmetricCipherKeyPair subjectKeyPair = GenerateKeyPair(random, keyLength);

            // It's self-signed, so these are the same.
            var issuerKeyPair = subjectKeyPair;

            var serialNumber = GenerateSerialNumber(random);
            var issuerSerialNumber = serialNumber; // Self-signed, so it's the same serial number.

            const bool isCertificateAuthority = false;
            KeyPurposeID[] usages = new[] { KeyPurposeID.IdKPServerAuth };
            string outputPrivateKeyName = path+ "\\" + privateFileKeyName;
            string outputCertReqFile = path + "\\" + requestFileName;

            GenerateCertificate(random,
                                subjectKeyPair,
                                serialNumber,
                                commonName,
                                subjectAlternativeNames,
                                issuerKeyPair,
                                issuerSerialNumber,
                                isCertificateAuthority,
                                usages,
                                signatureAlgorithm,
                                countryCode,
                                stateOrProvinceName,
                                localityName,
                                organization,
                                arrccbKeyUsage,
                                arrExtendedKeyUsage,
                                outputPrivateKeyName,
                                outputCertReqFile,
                                isThisCACert,
                                password
                            );
        }

        /// <summary>
        /// Generate certificate request and private key file
        /// </summary>
        /// <param name="random"></param>
        /// <param name="subjectKeyPair"></param>
        /// <param name="subjectSerialNumber"></param>
        /// <param name="commonName"></param>
        /// <param name="subjectAlternativeNames"></param>
        /// <param name="issuerKeyPair"></param>
        /// <param name="issuerSerialNumber"></param>
        /// <param name="isCertificateAuthority"></param>
        /// <param name="usages"></param>
        /// <param name="signatureAlgorithm"></param>
        /// <param name="countryCode"></param>
        /// <param name="stateOrProvinceName"></param>
        /// <param name="localityName"></param>
        /// <param name="organization"></param>
        /// <param name="arrccbKeyUsage"></param>
        /// <param name="arrExtendedKeyUsage"></param>
        /// <param name="outputPrivateKeyName"></param>
        /// <param name="outputCertReqFile"></param>
        private async void GenerateCertificate(SecureRandom random,
                                                    AsymmetricCipherKeyPair subjectKeyPair,
                                                    BigInteger subjectSerialNumber,
                                                    string commonName,
                                                    string[] subjectAlternativeNames,
                                                    AsymmetricCipherKeyPair issuerKeyPair,
                                                    BigInteger issuerSerialNumber,
                                                    bool isCertificateAuthority,
                                                    KeyPurposeID[] usages,
                                                    string signatureAlgorithm,
                                                    string countryCode,
                                                    string stateOrProvinceName,
                                                    string localityName,
                                                    string organization,
                                                    List<int> arrccbKeyUsage,
                                                    List<string> arrExtendedKeyUsage,
                                                    string outputPrivateKeyName,
                                                    string outputCertReqFile,
                                                    bool isThisCACert = false,
                                                    string password=null)
        {
            createRequest.IsEnabled = false;
            progressring.Visibility = Visibility.Visible;
            await System.Threading.Tasks.TaskEx.Delay(1000);

            #region Private Key
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

            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);

                var textWriter = new StringWriter();
                var pemWriter = new PemWriter(textWriter);
                if (!String.IsNullOrEmpty(password))
                {
                    pemWriter.WriteObject(subjectKeyPair.Private, "DESEDE", password.ToCharArray(), new SecureRandom());
                }
                else
                {
                    pemWriter.WriteObject(subjectKeyPair.Private);
                }
                pemWriter.Writer.Flush();
                string privateKeyPem = textWriter.ToString();
                using (var writer = new StreamWriter(outputPrivateKeyName))
                {
                    writer.WriteLine(privateKeyPem);
                }

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "File with private key: " + outputPrivateKeyName + " sucessfully generated and saved." + "\n",
                    Foreground = System.Windows.Media.Brushes.Green
                });
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                createRequest.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "ERROR creating certificate private key file (.key)" + "\n" + "Error: " + ex.Source + " " + ex.Message + "\n",
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

            #region CSR - Certificate Request file (.csr)
            // BasicConstraints
            var extensions = new Dictionary<DerObjectIdentifier, Org.BouncyCastle.Asn1.X509.X509Extension>()
            {
                //{X509Extensions.BasicConstraints,
                //    new Org.BouncyCastle.Asn1.X509.X509Extension(true, new DerOctetString(new BasicConstraints(false)))}

            };

            //BasicConstraints
            if (!isThisCACert)
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                Asn1OctetString asn1ost0 = new DerOctetString(new BasicConstraints(false));
                extensions.Add(X509Extensions.BasicConstraints,
                    new Org.BouncyCastle.Asn1.X509.X509Extension(true, asn1ost0));
            }
            else
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                Asn1OctetString asn1ost0 = new DerOctetString(new BasicConstraints(true));
                extensions.Add(X509Extensions.BasicConstraints,
                    new Org.BouncyCastle.Asn1.X509.X509Extension(true, asn1ost0));
            }

            //KeyUsage
            if (!isThisCACert)
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                if (arrccbKeyUsage.Count > 0)
                {
                    int usage = 0;
                    for (int i = 0; i < arrccbKeyUsage.Count; i++)
                    {
                        usage = usage | arrccbKeyUsage[i];
                    }
                    KeyUsage keyUsage = new KeyUsage(usage);

                    Asn1OctetString value = new DerOctetString(keyUsage);
                    extensions.Add(X509Extensions.KeyUsage, new Org.BouncyCastle.Asn1.X509.X509Extension(true, value));
                }
            }
            else
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);

                int usage = KeyUsage.DigitalSignature | KeyUsage.KeyCertSign | KeyUsage.CrlSign; // KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment;
                KeyUsage keyUsage = new KeyUsage(usage);

                Asn1OctetString value = new DerOctetString(keyUsage);
                extensions.Add(X509Extensions.KeyUsage, new Org.BouncyCastle.Asn1.X509.X509Extension(true, value));
            }

            //ExtendedKeyUsage
            if (!isThisCACert)
            {
                if (arrExtendedKeyUsage.Count > 0)
                {
                    await System.Threading.Tasks.TaskEx.Delay(1000);

                    KeyPurposeID[] extendedKeyUsageArr = new KeyPurposeID[arrExtendedKeyUsage.Count];
                    for (int i = 0; i < arrExtendedKeyUsage.Count; i++)
                    {
                        if (KeyPurposeID.AnyExtendedKeyUsage.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.AnyExtendedKeyUsage;
                        }
                        else if (KeyPurposeID.IdKPServerAuth.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPServerAuth;
                        }
                        else if (KeyPurposeID.IdKPClientAuth.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPClientAuth;
                        }
                        else if (KeyPurposeID.IdKPCodeSigning.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPCodeSigning;
                        }
                        else if (KeyPurposeID.IdKPEmailProtection.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPEmailProtection;
                        }
                        else if (KeyPurposeID.IdKPIpsecEndSystem.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPIpsecEndSystem;
                        }
                        else if (KeyPurposeID.IdKPIpsecTunnel.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPIpsecTunnel;
                        }
                        else if (KeyPurposeID.IdKPIpsecUser.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPIpsecUser;
                        }
                        else if (KeyPurposeID.IdKPTimeStamping.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPTimeStamping;
                        }
                        else if (KeyPurposeID.IdKPOcspSigning.ToString() == arrExtendedKeyUsage[i])
                        {
                            extendedKeyUsageArr[i] = KeyPurposeID.IdKPOcspSigning;
                        }
                        await System.Threading.Tasks.TaskEx.Delay(500);
                    }

                    ExtendedKeyUsage extendedKeyUsage = new ExtendedKeyUsage(extendedKeyUsageArr);

                    Asn1OctetString asn1ost = new DerOctetString(extendedKeyUsage);
                    extensions.Add(X509Extensions.ExtendedKeyUsage,
                        new Org.BouncyCastle.Asn1.X509.X509Extension(false, asn1ost));
                }
            }

            //SubjectAlternativeName
            if (!isThisCACert)
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);

                if (subjectAlternativeNames.Length > 0)
                {
                    GeneralNames names = new GeneralNames(
                        subjectAlternativeNames.Select(n => new GeneralName(GeneralName.DnsName, n)).ToArray()
                        );

                    await System.Threading.Tasks.TaskEx.Delay(1000);

                    Asn1OctetString asn1ost = new DerOctetString(names);
                    extensions.Add(X509Extensions.SubjectAlternativeName,
                        new Org.BouncyCastle.Asn1.X509.X509Extension(false, asn1ost));
                }
            }

            await System.Threading.Tasks.TaskEx.Delay(1000);
            //SubjectKeyIdentifier
            Asn1OctetString asn2ost = new DerOctetString(new SubjectKeyIdentifierStructure(subjectKeyPair.Public));
            extensions.Add(X509Extensions.SubjectKeyIdentifier, new Org.BouncyCastle.Asn1.X509.X509Extension(false, asn2ost));

            await System.Threading.Tasks.TaskEx.Delay(1000);
            ISignatureFactory sigFactory = new Asn1SignatureFactory(signatureAlgorithm, subjectKeyPair.Private);

            Pkcs10CertificationRequest csr = new Pkcs10CertificationRequest(
                sigFactory,
                issuerDN,
                subjectKeyPair.Public,
                new DerSet(new AttributePkcs(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(new X509Extensions(extensions)))),
                subjectKeyPair.Private);

            await System.Threading.Tasks.TaskEx.Delay(1000);
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                bool isOK = csr.Verify();
                if (isOK)
                {
                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "File with certificate request : " + outputCertReqFile + " sucessfully generated." + "\n",
                        Foreground = System.Windows.Media.Brushes.Green
                    });

                    btnContinue.IsEnabled = true;
                }
                else
                {
                    progressring.Visibility = Visibility.Hidden;
                    createRequest.IsEnabled = true;

                    tbOutputMessageBox.Inlines.Add(new Run
                    {
                        Text = "File with certificate request : " + outputCertReqFile + " DOES NOT generated sucessfully!!!" + "\n",
                        Foreground = System.Windows.Media.Brushes.Red
                    });

                    var metroWindow = (Application.Current.MainWindow as MetroWindow);
                    await metroWindow.ShowMessageAsync("ERROR",
                         "File with certificate request : " + outputCertReqFile + " DOES NOT generated sucessfully!!!",
                         MessageDialogStyle.Affirmative);
                    return;
                }
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                createRequest.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "Error, verify certificate request : " + ex.Source + " " + ex.Message + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Error",
                     "Error, verify certificate request : " + "\n" + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
                return;
            }

            try
            {
                await System.Threading.Tasks.TaskEx.Delay(1000);
                using (TextWriter tw = new StreamWriter(outputCertReqFile))
                {
                    PemWriter pw = new PemWriter(tw);
                    pw.WriteObject(csr);
                    tw.Flush();
                }

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "File with certificate request : " + outputCertReqFile + " sucessfully saved." + "\n",
                    Foreground = System.Windows.Media.Brushes.Green
                });
            }
            catch (Exception ex)
            {
                progressring.Visibility = Visibility.Hidden;
                createRequest.IsEnabled = true;

                tbOutputMessageBox.Inlines.Add(new Run
                {
                    Text = "Error, file with certificate request : " + outputCertReqFile + " DOES NOT saved." + "\n",
                    Foreground = System.Windows.Media.Brushes.Red
                });

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                await metroWindow.ShowMessageAsync("Info Warning",
                     "ERROR to save certificate reguest file (.csr)" + "\n" +
                     "Error: " + ex.Source + " " + ex.Message,
                     MessageDialogStyle.Affirmative);
                return;
            }
            #endregion CSR

            //CertData.Init();
            CertData.cerPrivateFilePath = tbPathName.Text + "\\" + tbPrivateKeyName.Text + ".key";
            CertData.cerRequestFilePath = tbPathName.Text + "\\" + tbRequestName.Text + ".csr";
            CertData.cerPrivateKeyPassword = pbPasswordPrivateKey.Password;

            progressring.Visibility = Visibility.Hidden;
            createRequest.IsEnabled = true;
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
        /// Initialize items for CheckComboBox ccbExtendedKeyUsage - ExtendedKeyUsage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ccbExtendedKeyUsage_Initialized(object sender, EventArgs e)
        {
            FillExtendedKeyUsage();
        }
        /// <summary>
        /// Fill data for ccbExtendedKeyUsage
        /// </summary>
        private void FillExtendedKeyUsage()
        {
            ExtendedKeyUsageCCBData = new ObservableCollection<ExtendedKeyUsageData>();
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("AnyExtendedKeyUsage", KeyPurposeID.AnyExtendedKeyUsage));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("ServerAuthetification", KeyPurposeID.IdKPServerAuth));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("ClientAuthetification", KeyPurposeID.IdKPClientAuth));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("CodeSigning", KeyPurposeID.IdKPCodeSigning));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("PEmailProtection", KeyPurposeID.IdKPEmailProtection));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("IpsecEndSystem", KeyPurposeID.IdKPIpsecEndSystem));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("IpsecTunnel", KeyPurposeID.IdKPIpsecTunnel));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("IpsecUser", KeyPurposeID.IdKPIpsecUser));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("TimeStamping", KeyPurposeID.IdKPTimeStamping));
            ExtendedKeyUsageCCBData.Add(new ExtendedKeyUsageData("OcspSigning", KeyPurposeID.IdKPOcspSigning));
        }
        /// <summary>
        /// Initialize items for CheckComboBox ccbKeyUsage - KeyUsage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ccbKeyUsage_Initialized(object sender, EventArgs e)
        {
            FillKeyUsage();
        }
        /// <summary>
        /// Fil data for ccbKeyUsage
        /// </summary>
        private void FillKeyUsage()
        {
            KeyUsageCCBData = new ObservableCollection<KeyUsageData>();
            KeyUsageCCBData.Add(new KeyUsageData("DigitalSignature", KeyUsage.DigitalSignature));
            KeyUsageCCBData.Add(new KeyUsageData("NonRepudiation", KeyUsage.NonRepudiation));
            KeyUsageCCBData.Add(new KeyUsageData("KeyEncipherment", KeyUsage.KeyEncipherment));
            KeyUsageCCBData.Add(new KeyUsageData("DataEncipherment", KeyUsage.DataEncipherment));
            KeyUsageCCBData.Add(new KeyUsageData("KeyAgreement", KeyUsage.KeyAgreement));
            KeyUsageCCBData.Add(new KeyUsageData("KeyCertSign", KeyUsage.KeyCertSign));
            KeyUsageCCBData.Add(new KeyUsageData("CrlSign", KeyUsage.CrlSign));
            KeyUsageCCBData.Add(new KeyUsageData("EncipherOnly", KeyUsage.EncipherOnly));
            KeyUsageCCBData.Add(new KeyUsageData("DecipherOnly", KeyUsage.DecipherOnly));
        }

        /// <summary>
        /// Open wizard what will be next step
        /// You need to send generated .csr file to internal or external certificate authority to generate certificate base on data inside 
        /// certificate request file (.csr)
        /// To test generated .csr file you can generate selfsign certificate file that can act as CA root and use it to signe certificate
        /// request and ganerate new certificate base on data inside certificate request (.csr) file.
        /// If you wish to use generate certificate file inside internal network, tehnicaly speaking you can use selfsign CA generate certificate
        /// file for that purpose. You need to import generate CA root certificate file to Trusted Root Certification Authorities inside 
        /// certification store and everithing will be work fine for certificate generated base on certificate request and signed with generated
        /// CA root certificate from inside menu option "Create SelfSign Cert."
        /// Show dialog with following options:
        /// 1.Send to CA   - this option will close dialog and show message: "You need to send generated certificate request file to internal or external CA authority for sign request"
        /// 2.Sign localy  - this option will open new dialog: "Do you already have generated root CA certificate or not?" (Yes / No)
        /// 3.Cancel       - this option clos current dialog and return to current form
        /// ----------------------------------------------------------------------------------
        /// For option 2.:
        /// If answer is Yes -> open new form from menu option "Issue Certificate" 
        /// If naswer is No -> open new form from menu option "Create SelfSign Cert." (with value "Is this CA certificate:"=Yes) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            string requestFileNamePath = tbPathName.Text + "\\" + tbPrivateKeyName.Text + ".key";
            string requestFileNamePrivateKeyPath = tbPathName.Text + "\\" + tbRequestName.Text + ".csr";
            if (!String.IsNullOrEmpty(requestFileNamePath) && File.Exists(requestFileNamePath))
            {
                MetroWindow metroWin = (MetroWindow)Application.Current.MainWindow;
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Send to CA authority",
                    NegativeButtonText = "Cancel",
                    FirstAuxiliaryButtonText = "Sign locally-Don't have CA cert",
                    SecondAuxiliaryButtonText = "Sign locally-Have CA cert",
                    ColorScheme = MetroDialogColorScheme.Theme
                };
                MessageDialogResult result = await metroWin.ShowMessageAsync("INFO",
                    "You need to send generated .csr file to internal or external certificate authority to" + "\n" +
                    "generate certificate based on data inside certificate request file (.csr) => 'Send to CA authority'" + "\n\n" +
                    "To test generated .csr file you can generate self sign certificate file that can act as CA root and use it to sign certificate" + "\n" +
                    "request and generate new certificate based on data inside certificate request (.csr) file  => 'Sign locally - Don't have CA cert'" + "\n\n" +
                    "If you wish to use generate certificate file inside internal network, technically speaking you can use self-sign CA generate certificate" + "\n" +
                    "file for that purpose. You need to import generate CA root certificate file (inside menu option 'Create SelfSign Cert.')" + "\n" +
                    "to Trusted Root Certification Authorities inside certification store and everything will be work fine" + "\n" +
                    "for certificate generated base on certificate request and signed with generated CA root certificate." + "\n\n" +
                    "If you already have generated certificate for CA root authority you can sign request with that certificate => 'Sign locally - Have CA cert'",
                    MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, mySettings);

                if (result == MessageDialogResult.Affirmative)
                {
                    mySettings = new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "OK",
                        ColorScheme = MetroDialogColorScheme.Theme
                    };
                    result = await metroWin.ShowMessageAsync("Info",
                        "You need to send generated .csr file " + tbRequestName.Text + "\n" +
                        " to internal or external certificate authority to" + "\n" +
                        "generate certificate based on data inside certificate request file (.csr)" + "\n\n" +
                        "After that you need to start menu option 'Create Certificate' to generate certificate file" + "\n" +
                        "that conatin public key file (.cer), that generated by CA root authority base on request file (.csr)" + "\n" +
                        "and private key file (.key): " + tbPrivateKeyName.Text + "\n\n" +
                        "New generate file fith private+public key (.pfx) optinal can contain public key from CA root authority ",
                        MessageDialogStyle.Affirmative, mySettings);
                }
                else if (result == MessageDialogResult.Negative)
                {
                    int a = 1;
                }
                else if (result == MessageDialogResult.FirstAuxiliary)
                {
                    // open menu "Create SelfSign Cert."
                    MainWindow mw = new MainWindow();

                    string header0 = "CA Certificate";
                    IEnumerable<TabablzControl> tctrl;
                    mw.GetTabablzData(out header0, out tctrl);
                    header0 = "CA Certificate";

                    CreateCA gr = new CreateCA();

                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);
                    mw.sbiSelectedMenuOption.Content = header0;

                }
                else if (result == MessageDialogResult.SecondAuxiliary)
                {
                    // open menu "Issue Certificate" 
                    MainWindow mw = new MainWindow();

                    string header0 = "Issue Certificate";
                    IEnumerable<TabablzControl> tctrl;
                    mw.GetTabablzData(out header0, out tctrl);
                    header0 = "Issue Certificate";

                    IssueCert gr = new IssueCert();

                    TabContent tc1 = new TabContent(header0, gr);
                    mw.AddTabablzData(header0, tctrl, tc1);

                    mw.sbiSelectedMenuOption.Content = header0;

                }
            }
        }

        private void cbSinatuteAlgorithm_Initialized(object sender, EventArgs e)
        {
            FillSinatuteAlgorithm();
        }

        private void FillSinatuteAlgorithm()
        {
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
        }
    }
}
