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
using System.Windows.Shapes;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO.Packaging;
using Ionic.Zip;
using MahApps.Metro.Controls;

namespace CrashReporterWPF.Net
{
    /// <summary>
    /// Interaction logic for wdoCrashReport.xaml
    /// </summary>
    public partial class wdoCrashReport : MetroWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="wdoCrashReport"/> class.
        /// </summary>
        public wdoCrashReport()
        {
            InitializeComponent();
        }

        ReportEx mReportEx;
        /// <summary>
        /// IsQuit.
        /// </summary>
        public bool isQuit=false;

        /// <summary>
        /// Initializes a new instance of the <see cref="wdoCrashReport"/> class.
        /// </summary>
        /// <param name="pExcep">The p excep.</param>
        /// <param name="pFromAddress">The p from address.</param>
        /// <param name="pToAddress">The p to address.</param>
        /// <param name="pSmtpCl">The p SMTP cl.</param>
        public wdoCrashReport(Exception pExcep, MailAddress pFromAddress, MailAddress pToAddress, SmtpClient pSmtpCl):this()
        {
            InitializeComponent();
            Excep = pExcep;
            ToAddress = pToAddress;
            FromAddress = pFromAddress;
            SmtpCl = pSmtpCl;
            LoadReportEx();
            this.Title= string.Format("{0} {1} crashed", System.Reflection.Assembly.GetEntryAssembly().GetName().Name, 
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
            //saveFileDialog.FileName = _appTitle + " " + _appVersion + " Crash Report";
            string osArchitecture;
            try
            {
                osArchitecture = IsOS64Bit() ? "64" : "32";
            }
            catch (Exception)
            {
                osArchitecture = "32/64 bit (Undetermine)";
            }
            if (File.Exists(string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)))
            {
                //pictureBoxScreenshot.ImageLocation = string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), AppTitle);
                //pictureBoxScreenshot.Show();
            }
            switch (Environment.OSVersion.Version.Major)
            {
                case 5:
                    switch (Environment.OSVersion.Version.Minor)
                    {
                        case 0:
                            mReportEx.Windows_Version = string.Format("Windows 2000 {0} {1} Version {2}", Environment.OSVersion.ServicePack, osArchitecture, Environment.OSVersion.Version);
                            break;
                        case 1:
                            mReportEx.Windows_Version = string.Format("Windows XP {0} {1} Version {2}", Environment.OSVersion.ServicePack, osArchitecture, Environment.OSVersion.Version);
                            break;
                    }
                    break;
                case 6:
                    switch (Environment.OSVersion.Version.Minor)
                    {
                        case 0:
                            mReportEx.Windows_Version = string.Format("Windows Vista {0} {1} bit Version {2}", Environment.OSVersion.ServicePack, osArchitecture, Environment.OSVersion.Version);
                            break;
                        case 1:
                            mReportEx.Windows_Version = string.Format("Windows 7 {0} {1} bit Version {2}", Environment.OSVersion.ServicePack, osArchitecture, Environment.OSVersion.Version);
                            break;
                        case 2:
                            mReportEx.Windows_Version = string.Format("Windows 8 {0} {1} bit Version {2}", Environment.OSVersion.ServicePack, osArchitecture, Environment.OSVersion.Version);
                            break;
                    }
                    break;
            }
        }


        ///<summary>
        /// 
        ///</summary>
        private void LoadReportEx()
        {
            try
            {
                mReportEx = new ReportEx();
                mReportEx.DataEx = DateTime.Now;
                mReportEx.Exception_Type = Excep.GetType().ToString();
                mReportEx.Stack_Trace = Excep.StackTrace.ToString();
                mReportEx.Source = Excep.Source.ToString();
                mReportEx.Error_Message = Excep.Message;
                mReportEx.ApplicationName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                mReportEx.ApplicationVersione = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                mReportEx.ImagePath = string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), mReportEx.ApplicationName);
            }
            catch (Exception ex)
            {               
                throw new ApplicationException(" " + mClassName + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }


        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="libraryName">Name of the library.</param>
        /// <returns></returns>
        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr LoadLibrary(string libraryName);

        /// <summary>
        /// Gets the proc address.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns></returns>
        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

        private delegate bool IsWow64processsDelegate([System.Runtime.InteropServices.In] IntPtr handle, [System.Runtime.InteropServices.Out] out bool isWow64processs);
       
        private static IsWow64processsDelegate GetIsWow64processsDelegate()
        {
            var handle = LoadLibrary("kernel32");

            if (handle != IntPtr.Zero)
            {
                var fnPtr = GetProcAddress(handle, "IsWow64processs");

                if (fnPtr != IntPtr.Zero)
                {
                    return (IsWow64processsDelegate)Marshal.GetDelegateForFunctionPointer(fnPtr, typeof(IsWow64processsDelegate));
                }
            }

            return null;
        }

        /// <summary>
        /// Is32s the bit processs on64 bit processsor.
        /// </summary>
        /// <returns></returns>
        private static bool Is32BitprocesssOn64Bitprocesssor()
        {
            IsWow64processsDelegate fnDelegate = GetIsWow64processsDelegate();

            if (fnDelegate == null)
            {
                return false;
            }

            bool isWow64;
            var retVal = fnDelegate.Invoke(System.Diagnostics.Process.GetCurrentProcess().Handle, out isWow64);

            if (retVal == false)
            {
                return false;
            }

            return isWow64;
        }
        /// <summary>
        /// Determines whether [is O S64 bit].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is O S64 bit]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOS64Bit()
        {
            return IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitprocesssOn64Bitprocesssor());
        }

        private SmtpClient mSmtpCl;
        /// <summary>
        /// Gets or sets the Property SmtpCl.
        /// </summary>
        /// <value>The Property SmtpCl.</value>
        public SmtpClient SmtpCl
        {
            get { return mSmtpCl; }
            set { mSmtpCl = value; }
        }

        private MailAddress mToAddress;
        /// <summary>
        /// Gets or sets the Property ToAddress.
        /// </summary>
        /// <value>The Property ToAddress.</value>
        public MailAddress ToAddress
        {
            get { return mToAddress; }
            set { mToAddress = value; }
        }

        private MailAddress mFromAddress;
        /// <summary>
        /// Gets or sets the Property FromAddress.
        /// </summary>
        /// <value>The Property FromAddress.</value>
        public MailAddress FromAddress
        {
            get { return mFromAddress; }
            set { mFromAddress = value; }
        }
        private Exception mExcep;

        /// <summary>
        /// Gets or sets the Property Excep.
        /// </summary>
        /// <value>The Property Excep.</value>
        public Exception Excep
        {
            get { return mExcep; }
            set { mExcep = value; }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder mess1 = new StringBuilder();
            switch (((Button)sender).Name)
            {
                case "btnSendReport":
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                    string mess = @"Crash report by " + mReportEx.ApplicationName + " " + mReportEx.ApplicationVersione;
                    string subject;           
                    subject = string.Format("{0} {1} Crash Report", mReportEx.ApplicationName, mReportEx.ApplicationVersione);

                    mess1.AppendLine(mReportEx.ApplicationName + " " + mReportEx.ApplicationVersione + " Crash Report" + "<br>");
                    mess1.AppendLine(mReportEx.DataEx.ToString() + "<br>");
                    mess1.AppendLine("****************************************************" + "<br>");
                    mess1.AppendLine("Exception Type:  " + Excep.GetType() + "<br>");
                    mess1.AppendLine("Windows Version: " + mReportEx.Windows_Version + "<br>");
                    mess1.AppendLine("Error Message:   " + Excep.Message + "<br>");
                    mess1.AppendLine("InnerException:  " + Excep.InnerException + "<br>");
                    mess1.AppendLine("Source:          " + Excep.Source + "<br>");
                    mess1.AppendLine("Stack Trace:     " + Excep.StackTrace + "<br>");
                    mess1.AppendLine("****************************************************" + "<br>");
                    mess1.AppendLine(" User Message:    " + "<br>");
                    mess1.AppendLine(mReportEx.MessageForUser + "<br>");

                    //MailMessage message = new MailMessage(FromAddress, ToAddress)
                    //{
                    //    Subject = subject,
                    //    Body = mess1.ToString(),
                    //};
                    mReportEx.SaveToXMLFile(string.Format("{0}\\{1}CrashScreenshot.xml", System.IO.Path.GetTempPath(), mReportEx.ApplicationName));

                    //if (File.Exists(string.Format("{0}\\{1}CrashScreenshot.xml", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)))
                    //{
                    //    message.Attachments.Add(new Attachment(string.Format("{0}\\{1}CrashScreenshot.xml", System.IO.Path.GetTempPath(), 
                    //    mReportEx.ApplicationName)));
                    //}
                    //if (File.Exists(string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)))
                    //{
                    //    message.Attachments.Add(new Attachment(string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)));
                    //}
                    //const string crashReport = "Crash report";

                    //SmtpCl.SendCompleted += new SendCompletedEventHandler(SmtpCl_SendCompleted);
                    //SmtpCl.SendAsync(message, crashReport);
                    //----------------------------------------------------------
                    //string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    var mailMessage = new MailMessage(FromAddress, ToAddress);
                    ////mailMessage.From = new MailAddress("someone@yourdomain.com");
                    //mailMessage.From = new MailAddress(FromAddress.Address);
                    //mailMessage.Subject = "Your subject here";
                    mailMessage.Subject = subject;
                    mailMessage.IsBodyHtml = true;
                    //mailMessage.Body = "<span style='font-size: 12pt; color: red;'>My HTML formatted body</span>";
                    mailMessage.Body = mess1.ToString();

                    //mailMessage.Attachments.Add(new Attachment("C://Myfile.pdf"));
                    if (File.Exists(string.Format("{0}\\{1}CrashScreenshot.xml", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)))
                    {
                        mailMessage.Attachments.Add(new Attachment(string.Format("{0}\\{1}CrashScreenshot.xml", System.IO.Path.GetTempPath(),
                        mReportEx.ApplicationName)));
                    }

                    if (File.Exists(string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)))
                    {
                        mailMessage.Attachments.Add(new Attachment(string.Format("{0}\\{1}CrashScreenshot.png", System.IO.Path.GetTempPath(), mReportEx.ApplicationName)));
                    }

                    //var filename = "C://Temp/mymessage.eml";
                    var filename = System.IO.Path.GetTempPath()+"/mymessage.eml";
                    //save the MailMessage to the filesystem
                    mailMessage.Save(filename);

                    //Open the file with the default associated application registered on the local machine
                    Process.Start(filename);

                    mailMessage.Dispose();
                    //--------------------------------------------------------
                    Mouse.OverrideCursor = null;
                    break;
                case "btnSaveReport":
                    SaveReport();
                    break;
                case "btnAnnulla":
                    this.Close();
                    break;
                case "btnCopy":
                    mess1.AppendLine(mReportEx.ApplicationName + " " + mReportEx.ApplicationVersione + " Crash Report" + "<br>");
                    mess1.AppendLine(mReportEx.DataEx.ToString() + "<br>");
                    mess1.AppendLine("****************************************************" + "<br>");
                    mess1.AppendLine("Exception Type:  " + Excep.GetType() + "<br>");
                    mess1.AppendLine("Windows Version: " + mReportEx.Windows_Version + "<br>");
                    mess1.AppendLine("Error Message:   " + Excep.Message + "<br>");
                    mess1.AppendLine("InnerException:  " + Excep.InnerException + "<br>");
                    mess1.AppendLine("Source:          " + Excep.Source + "<br>");
                    mess1.AppendLine("Stack Trace:     " + Excep.StackTrace + "<br>");
                    mess1.AppendLine("****************************************************" + "<br>");
                    mess1.AppendLine(" User Message:   " + "<br>");
                    mess1.AppendLine(mReportEx.MessageForUser + "<br>");

                    Clipboard.SetText(mess1.ToString());
                    //this.Close();
                    break;
                case "btnQuit":
                    isQuit = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        ///<summary>
        /// 
        ///</summary>
        private void SaveReport()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog CReport = new Microsoft.Win32.SaveFileDialog();
                CReport.DefaultExt = ".xml";
                CReport.Filter = "CrashReport (.xml)|*.xml";
                Nullable<bool> result = CReport.ShowDialog();
                if (result == true)
                {
                    string onlyFileName = System.IO.Path.GetFileNameWithoutExtension(CReport.FileName);
                    string folderPath = System.IO.Path.GetDirectoryName(CReport.FileName);

                    string XMLFileName = folderPath + "\\" + onlyFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml";
                    string screenShotPath = System.IO.Path.GetTempPath() + "CrashReporterWPF.Net.TestCrashScreenshot.png";
                    string pictureFileName=folderPath + "\\" + onlyFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".png";
                    //string ZIPFileName = folderPath + "\\" + onlyFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".zip";
                    string ZIPFileName = folderPath + "\\" + onlyFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".zip";

                    if (File.Exists(XMLFileName))
                    {
                        File.Delete(XMLFileName);
                    }
                    if (File.Exists(pictureFileName))
                    {
                        File.Delete(pictureFileName);
                    }
                    if (File.Exists(ZIPFileName))
                    {
                        File.Delete(ZIPFileName);
                    }

                    mReportEx.SaveToXMLFile(XMLFileName);
                    if (File.Exists(screenShotPath))
                    {
                        File.Copy(screenShotPath, pictureFileName);
                    }

                    //ZipFile zip = new ZipFile(onlyFileName+".zip");
                    ZipFile zip = new ZipFile(ZIPFileName);
                    if (File.Exists(XMLFileName))
                    {
                        //AddFileToZip(ZIPFileName, XMLFileName);
                        zip.AddFile(XMLFileName,".");
                    }
                    if (File.Exists(pictureFileName))
                    {
                        //AddFileToZip(ZIPFileName, pictureFileName);
                        zip.AddFile(pictureFileName, ".");
                    }
                    zip.Save();
                    zip.Dispose();

                }   
            }
            catch (Exception ex)
            {
                throw new ApplicationException(" " + mClassName + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private const long BUFFER_SIZE = 4096;

        private static void AddFileToZip(string zipFilename, string fileToAdd)
        {
            using (Package zip = System.IO.Packaging.Package.Open(zipFilename, FileMode.OpenOrCreate))
            {
                string destFilename = ".\\" + System.IO.Path.GetFileName(fileToAdd);
                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                {
                    zip.DeletePart(uri);
                }
                PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read))
                {
                    using (Stream dest = part.GetStream())
                    {
                        CopyStream(fileStream, dest);
                    }
                }
            }
        }

        private static void CopyStream(System.IO.FileStream inputStream, System.IO.Stream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bytesRead;
            }
        }

        private string mWindowsVersion;
        /// <summary>
        /// Gets or sets the Property WindowsVersion.
        /// </summary>
        /// <value>The Property WindowsVersion.</value>
        public string WindowsVersion
        {
            get { return mWindowsVersion; }
            set { mWindowsVersion = value; }
        }

        ///<summary>
        /// 
        ///</summary>
        void SmtpCl_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, e.Error.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(string.Format("Crash report di {0} {1} is sent to the developer. Grazie per il supporto.", mReportEx.ApplicationName,
                   mReportEx.ApplicationVersione), "Crash report Inviato", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #region Field
        /// <summary>
        /// Nome della classe usato per debug nella generazione delle eccezioni 
        /// </summary>
        private readonly static string mClassName = System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name;

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = mReportEx;

            //textBoxApplicationName.Text = _appTitle;
            //textBoxApplicationVersion.Text = _appVersion;
            //textBoxMessage.Text = _exception.Message;
            //textBoxTime.Text = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            //textBoxSource.Text = _exception.Source;
            //textBoxStackTrace.Text = string.Format("{0}\n{1}", _exception.InnerException, _exception.StackTrace);
        }
    }
}
