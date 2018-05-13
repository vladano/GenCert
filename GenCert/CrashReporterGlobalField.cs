using CrashReporterWPF.Net;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace GenCert
{
     /// <summary>
    /// Libreria condivisa
    /// </summary>
    public class CrashReporterGlobalField
    {
        /// <summary>
        /// FirstRun
        /// </summary>
        /// 
        public static ReportCrash _ReportCrash = new ReportCrash()
        {
            FromEmail = UserPrincipal.Current.EmailAddress,

            ToEmail = "vladan.obradovic@gmail.com",
            SMTPHost = "smtp.gmail.com",
            Port = 25,
            UserName = "",
            Password = "",
            EnableSSL = true,
        };


    }
}
