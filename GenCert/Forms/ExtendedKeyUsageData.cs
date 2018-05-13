using Org.BouncyCastle.Asn1.X509;

namespace GenCert.Forms
{
    public class ExtendedKeyUsageData
    {
        //private const string IdKP = "1.3.6.1.5.5.7.3";
        //public static readonly KeyPurposeID AnyExtendedKeyUsage = new KeyPurposeID(X509Extensions.ExtendedKeyUsage.Id + ".0");
        //public static readonly KeyPurposeID IdKPServerAuth = new KeyPurposeID(IdKP + ".1");
        //public static readonly KeyPurposeID IdKPClientAuth = new KeyPurposeID(IdKP + ".2");
        //public static readonly KeyPurposeID IdKPCodeSigning = new KeyPurposeID(IdKP + ".3");
        //public static readonly KeyPurposeID IdKPEmailProtection = new KeyPurposeID(IdKP + ".4");
        //public static readonly KeyPurposeID IdKPIpsecEndSystem = new KeyPurposeID(IdKP + ".5");
        //public static readonly KeyPurposeID IdKPIpsecTunnel = new KeyPurposeID(IdKP + ".6");
        //public static readonly KeyPurposeID IdKPIpsecUser = new KeyPurposeID(IdKP + ".7");
        //public static readonly KeyPurposeID IdKPTimeStamping = new KeyPurposeID(IdKP + ".8");
        //public static readonly KeyPurposeID IdKPOcspSigning = new KeyPurposeID(IdKP + ".9");

        //new ExtendedKeyUsage(
        //    KeyPurposeID.IdKPServerAuth,
        //    KeyPurposeID.IdKPClientAuth)))},

        public ExtendedKeyUsageData()
        { }

        public ExtendedKeyUsageData(string displayName, KeyPurposeID extendedKeyUsageValueName)
        {
            this.DisplayName = displayName;
            this.ExtendedKeyUsageValueName = extendedKeyUsageValueName;
        }

        public string DisplayName { get; set; }
        public KeyPurposeID ExtendedKeyUsageValueName { get; set; }

    }

}