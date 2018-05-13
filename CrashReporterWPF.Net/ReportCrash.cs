using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Input;

namespace CrashReporterWPF.Net
{
    /// <summary>
    /// ReportCrash
    /// </summary>
    public class ReportCrash
    {

        private string mFromEmail;
        /// <summary>
        /// Gets or sets the Property FromEmail.
        /// </summary>
        /// <value>The Property FromEmail.</value>
        public string FromEmail
        {
            get { return mFromEmail; }
            set { mFromEmail = value; }
        }

        private string mToEmail;
        /// <summary>
        /// Gets or sets the Property ToEmail.
        /// </summary>
        /// <value>The Property ToEmail.</value>
        public string ToEmail
        {
            get { return mToEmail; }
            set { mToEmail = value; }
        }


        private string mSMTPHost;
        /// <summary>
        /// Gets or sets the Property SMTPHost.
        /// </summary>
        /// <value>The Property SMTPHost.</value>
        public string SMTPHost
        {
            get { return mSMTPHost; }
            set { mSMTPHost = value; }
        }


        private int mPort;
        /// <summary>
        /// Gets or sets the Property Port.
        /// </summary>
        /// <value>The Property Port.</value>
        public int Port
        {
            get { return mPort; }
            set { mPort = value; }
        }


        private bool mEnableSSL;
        /// <summary>
        /// Gets or sets the Property EnableSSL.
        /// </summary>
        /// <value>The Property EnableSSL.</value>
        public bool EnableSSL
        {
            get { return mEnableSSL; }
            set { mEnableSSL = value; }
        }


        private string mUserName;
        /// <summary>
        /// Gets or sets the Property UserName.
        /// </summary>
        /// <value>The Property UserName.</value>
        public string UserName
        {
            get { return mUserName; }
            set { mUserName = value; }
        }


        private string mPassword;
        /// <summary>
        /// Gets or sets the Property Password.
        /// </summary>
        /// <value>The Property Password.</value>
        public string Password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }

        private bool isQuit;
        /// <summary>
        /// Gets or sets the Property IsQuit.
        /// </summary>
        /// <value>The Property IsQuit.</value>
        public bool IsQuit
        {
            get { return isQuit; }
            set { isQuit = value; }
        }


        /// <summary>
        /// Sends the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Send(Exception exception)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            isQuit = false;
            CaptureScreenshot captureScreenshot = new CaptureScreenshot();
            captureScreenshot.CaptureScreenToFile(string.Format("{0}\\{1}CrashScreenshot.png", Path.GetTempPath(), System.Reflection.Assembly.GetEntryAssembly().GetName().Name), ImageFormat.Png);
            
            if (String.IsNullOrEmpty(mFromEmail) || String.IsNullOrEmpty(mToEmail) || String.IsNullOrEmpty(mSMTPHost))
                return;
            MailAddress fromAddress = new MailAddress(mFromEmail);
            MailAddress toAddress = new MailAddress(mToEmail);
            SmtpClient smtp = new SmtpClient
            {
                Host = mSMTPHost,
                Port = mPort,
                EnableSsl = mEnableSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mUserName, mPassword)
            };
            Mouse.OverrideCursor = null;
            wdoCrashReport crashReport = new wdoCrashReport(exception, fromAddress, toAddress, smtp);
            crashReport.ShowDialog();
            IsQuit = crashReport.isQuit;
        }
    }
}
