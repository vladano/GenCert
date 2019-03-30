namespace GenCert
{
    static class CertData
    {
        public static void Init()
        {
            cerRequestFilePath = null;
            cerPrivateFilePath = null;
            cerPassword = null;
            cerPFXFilePath = null;
            cerMasterPubFilePath = null;
            cerIntermediatePubFilePath = null;
            cerIssuerPubFilePath = null;
        }

        //Form: CreateRequest	=> IssueCert
        public static string cerRequestFilePath { get; set; } //cerRequestFilePath(.csr) -> IssuCert
        public static string cerPrivateFilePath { get; set; } //cerPrivateFilePath (.key) -> PFX
        public static string cerPrivateKeyPassword { get; set; } //password for private key fill if file encrypted

        //Form: IssueCert=>  PFX
        public static string cerPublicFilePath { get; set; } //cerPublicFilePath (.cer) -> PFX

        //Form: CreateCA => CreateRequest	=> IssueCert
        public static string cerPassword { get; set; } //cerMasterPass/cerIntermediatePass/cerIssuerPass			-> IssuCert
        public static string cerPFXFilePath { get; set; } //cerMaster(.pfx)/cerIntermediate(.pfx)/cerIssuer(.pfx)    -> IssuCert

        //Form: CreateCA => CreatePFX
        public static string cerMasterPubFilePath { get; set; } // cerMasterPub(.cer) -> PFX
        public static string cerIntermediatePubFilePath { get; set; } // cerIntermediatePub(.cer) -> PFX
        public static string cerIssuerPubFilePath { get; set; } //cerIssuerPub(.cer) -> PFX

        //Form: IssueCert <= CreateCA

        //Form: CreatePFX <= CreateCA
    }
}