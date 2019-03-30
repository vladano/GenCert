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

namespace GenCert.Forms
{
    /// <summary>
    /// Interaction logic for CA_Parameters.xaml
    /// </summary>
    public partial class CA_Parameters : MetroWindow
    {
        public string organization = "";
        public string domainName = "";
        public string countryCode = "";
        public string stateOrProvince = "";
        public string locality = "";
        public string password = "";

        public CA_Parameters()
        {
            InitializeComponent();
        }

        public CA_Parameters(string organization, string domainName, string countryCode, string stateOrProvince, string locality, string password) : this()
        {
            tbOrganization.Text=this.organization = organization;
            tbDomainName.Text = this.domainName = domainName;
            tbCountryCode.Text = this.countryCode = countryCode;
            tbStateOrProvince.Text = this.stateOrProvince = stateOrProvince;
            tbLocality.Text = this.locality = locality;
            pbCAPassword.Password = this.password = password;

            this.Owner = App.Current.MainWindow;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            // check filled data
            // <to do>

            // fill data
            organization = tbOrganization.Text;
            domainName = tbDomainName.Text;
            countryCode = tbCountryCode.Text;
            stateOrProvince = tbStateOrProvince.Text;
            locality = tbLocality.Text;
            password = pbCAPassword.Password;

            DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
