namespace GenCert.Forms
{
    public class KeyUsageData
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
         *         keyCertSign             (5),
         *         cRLSign                 (6),
         *         encipherOnly            (7),
         *         decipherOnly            (8) }
         * </pre>
         */
        //public const int DigitalSignature = (1 << 7);
        //public const int NonRepudiation = (1 << 6);
        //public const int KeyEncipherment = (1 << 5);
        //public const int DataEncipherment = (1 << 4);
        //public const int KeyAgreement = (1 << 3);
        //public const int KeyCertSign = (1 << 2);
        //public const int CrlSign = (1 << 1);
        //public const int EncipherOnly = (1 << 0);
        //public const int DecipherOnly = (1 << 15);

        public KeyUsageData()
        { }

        public KeyUsageData(string displayName, int keyUsageValue)
        {
            this.DisplayName = displayName;
            this.KeyUsageValue = keyUsageValue;
        }

        public string DisplayName { get; set; }
        public int KeyUsageValue { get; set; }

    }
}