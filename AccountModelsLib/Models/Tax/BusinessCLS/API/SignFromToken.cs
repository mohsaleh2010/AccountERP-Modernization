using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ess;
   
 
 
namespace AccountModelsLib.Models
{

    static class SignFromToken
    {
        private readonly static string DllLibPath = "eps2003csp11.dll";

        private readonly static string TokenPin = Common.CommonTaxConfig.TokenPin;
        public static string Sign(byte[] data)
        {
            // _hasher = new Hasher();
            var factories = new Pkcs11InteropFactories();
            using (var pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, DllLibPath, AppType.MultiThreaded))
            {
                var slot = pkcs11Library.GetSlotList(SlotsType.WithTokenPresent).FirstOrDefault();

                if (slot is null)
                {
                    return "No slots found";
                }

                var tokenInfo = slot.GetTokenInfo();

                var slotInfo = slot.GetSlotInfo();


                using (var session = slot.OpenSession(SessionType.ReadWrite))
                {

                    session.Login(CKU.CKU_USER, Encoding.UTF8.GetBytes(TokenPin));

                    var certificateSearchAttributes = new List<IObjectAttribute>() { session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE), session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true), session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509) };

                    var certificate = session.FindAllObjects(certificateSearchAttributes).FirstOrDefault();

                    if (certificate is null)
                    {
                        return "Certificate not found";
                    }

                    var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.MaxAllowed);


                    X509Certificate2 cert = null;
                    foreach (X509Certificate2 item in store.Certificates)
                    {
                        if (item.Issuer.ToUpper().Contains("test") || item.Issuer.ToUpper().Contains("test"))
                        {
                            cert = item;
                            break;
                        }
                    }

                    if (cert is null)
                    {
                        return "Certificate not found";
                    }
                    store.Close();

                    var content = new ContentInfo(new Oid("1.2.840.113549.1.7.5"), data);


                    var cms = new SignedCms(content, true);

                    var sha = SHA256.Create();
                    byte[] byteCert = sha.ComputeHash(cert.RawData);



                    var bouncyCertificate = new EssCertIDv2(new Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier(new DerObjectIdentifier("1.2.840.113549.1.9.16.2.47")), byteCert); // اجرب  ؟؟

                    var signerCertificateV2 = new SigningCertificateV2(new EssCertIDv2[] { bouncyCertificate });


                    var signer = new CmsSigner(cert);

                    signer.DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1");

                    signer.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));
                    signer.SignedAttributes.Add(new AsnEncodedData(new Oid("1.2.840.113549.1.9.16.2.47"), signerCertificateV2.GetEncoded()));

                    // System.Windows.Forms.MessageBox.Show("b sign");
                    cms.ComputeSignature(signer);
                    // System.Windows.Forms.MessageBox.Show("Invoice Send !");

                    byte[] output = cms.Encode();

                    return Convert.ToBase64String(output);

                } // ok go
            }
        }
    }
}