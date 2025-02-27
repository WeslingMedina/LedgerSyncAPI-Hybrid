using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Domain.Entities;

namespace Application.Services
{
    public class XmlSigner : IXmlSigner
    {
        private readonly Dictionary<string, string> _nodeNamespaces = new()
        {
            ["URL"] = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/",
            ["01"] = "facturaElectronica",
            ["02"] = "notaDebitoElectronica",
            ["03"] = "notaCreditoElectronica",
            ["04"] = "tiqueteElectronico",
            ["05"] = "mensajeReceptor",
            ["06"] = "mensajeReceptor",
            ["07"] = "mensajeReceptor"
        };

        private readonly Dictionary<string, string> _policyInfo = new()
        {
            ["name"] = "",
            ["url"] = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/Resoluci%C3%B3n_General_sobre_disposiciones_t%C3%A9cnicas_comprobantes_electr%C3%B3nicos_para_efectos_tributarios.pdf",
            ["digest"] = "DWxin1xWOeI8OuWQXazh4VjLWAaCLAA954em7DMh0h8=" // digest in sha256 and base64
        };

        private string _signatureId = string.Empty;
        private string _signatureValueId = string.Empty;
        private string _xadesObjectId = string.Empty;
        private string _keyInfoId = string.Empty;
        private string _reference0Id = string.Empty;
        private string _reference1Id = string.Empty;
        private string _signedPropertiesId = string.Empty;
        private string _documentType = "01";
        private string? _modulus;
        private string? _exponent;

        public async Task<SigningResult> SignXmlAsync(string certificatePath, string certificatePassword, string xmlContent, string documentType)
        {
            try
            {
                _documentType = documentType;

                // Generate new GUIDs for various parts of the signature
                _signatureId = $"Signature-{Guid.NewGuid()}";
                _signatureValueId = $"SignatureValue-{Guid.NewGuid()}";
                _xadesObjectId = $"XadesObjectId-{Guid.NewGuid()}";
                _keyInfoId = $"KeyInfoId-{_signatureId}";
                _reference0Id = $"Reference-{Guid.NewGuid()}";
                _reference1Id = "ReferenceKeyInfo";
                _signedPropertiesId = $"SignedProperties-{_signatureId}";

                // Load the certificate from file
                var certificate = new X509Certificate2(certificatePath, certificatePassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                // Extract the RSA key details
                using (var rsa = certificate.GetRSAPrivateKey())
                {
                    if (rsa == null)
                        return new SigningResult { Success = false, ErrorMessage = "Failed to extract RSA keys from certificate" };

                    var parameters = rsa.ExportParameters(false);
                    _modulus = Convert.ToBase64String(parameters.Modulus ?? Array.Empty<byte>());
                    _exponent = Convert.ToBase64String(parameters.Exponent ?? Array.Empty<byte>());
                }

                // Insert signature into the XML
                var signedXml = InsertSignature(xmlContent, certificate);

                return new SigningResult
                {
                    Success = true,
                    SignedXml = signedXml
                };
            }
            catch (Exception ex)
            {
                return new SigningResult
                {
                    Success = false,
                    ErrorMessage = $"Error during XML signing: {ex.Message}"
                };
            }
        }

        private string InsertSignature(string xml, X509Certificate2 certificate)
        {
            // Load the XML document
            var doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(xml);

            // Canonicalize the document for digest calculation
            var canonicalXml = Canonicalize(doc);

            // Get the document digest
            var documentDigest = ComputeSha256Hash(canonicalXml);

            // Define namespaces
            var xmlnsKeyInfo = $"xmlns=\"{_nodeNamespaces["URL"]}{_nodeNamespaces[_documentType]}\" ";
            var xmlnsSignedProps = xmlnsKeyInfo;
            var xmlnsSignedInfo = xmlnsKeyInfo;

            xmlnsKeyInfo += "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                           "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                           "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

            xmlnsSignedProps += "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                               "xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\" " +
                               "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                               "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

            xmlnsSignedInfo += "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                              "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                              "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

            // Get signing time
            var signTime = DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss-06:00");

            // Get certificate data
            var certDigest = ComputeSha256Hash(certificate.RawData);

            var certIssuer = certificate.Issuer;
            var serialNumber = certificate.SerialNumber ?? "";

            // Convert serial number to decimal if needed
            if (serialNumber.StartsWith("0x"))
            {
                serialNumber = HexToDecimal(serialNumber.Substring(2));
            }

            // Create the signed properties
            var signedProperties = $@"<xades:SignedProperties Id=""{_signedPropertiesId}"">
                <xades:SignedSignatureProperties>
                    <xades:SigningTime>{signTime}</xades:SigningTime>
                    <xades:SigningCertificate>
                        <xades:Cert>
                            <xades:CertDigest>
                                <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
                                <ds:DigestValue>{certDigest}</ds:DigestValue>
                            </xades:CertDigest>
                            <xades:IssuerSerial>
                                <ds:X509IssuerName>{certIssuer}</ds:X509IssuerName>
                                <ds:X509SerialNumber>{serialNumber}</ds:X509SerialNumber>
                            </xades:IssuerSerial>
                        </xades:Cert>
                    </xades:SigningCertificate>
                    <xades:SignaturePolicyIdentifier>
                        <xades:SignaturePolicyId>
                            <xades:SigPolicyId>
                                <xades:Identifier>{_policyInfo["url"]}</xades:Identifier>
                                <xades:Description />
                            </xades:SigPolicyId>
                            <xades:SigPolicyHash>
                                <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
                                <ds:DigestValue>{_policyInfo["digest"]}</ds:DigestValue>
                            </xades:SigPolicyHash>
                        </xades:SignaturePolicyId>
                    </xades:SignaturePolicyIdentifier>
                    <xades:SignerRole>
                        <xades:ClaimedRoles>
                            <xades:ClaimedRole>Emisor</xades:ClaimedRole>
                        </xades:ClaimedRoles>
                    </xades:SignerRole>
                </xades:SignedSignatureProperties>
                <xades:SignedDataObjectProperties>
                    <xades:DataObjectFormat ObjectReference=""#{_reference0Id}"">
                        <xades:MimeType>text/xml</xades:MimeType>
                        <xades:Encoding>UTF-8</xades:Encoding>
                    </xades:DataObjectFormat>
                </xades:SignedDataObjectProperties>
            </xades:SignedProperties>";

            // Prepare KeyInfo
            var keyInfo = $@"<ds:KeyInfo Id=""{_keyInfoId}"">
                <ds:X509Data>
                    <ds:X509Certificate>{Convert.ToBase64String(certificate.RawData)}</ds:X509Certificate>
                </ds:X509Data>
                <ds:KeyValue>
                    <ds:RSAKeyValue>
                        <ds:Modulus>{_modulus}</ds:Modulus>
                        <ds:Exponent>{_exponent}</ds:Exponent>
                    </ds:RSAKeyValue>
                </ds:KeyValue>
            </ds:KeyInfo>";

            // Calculate digests
            var signedPropsWithNs = signedProperties.Replace("<xades:SignedProperties", $"<xades:SignedProperties {xmlnsSignedProps}");
            var signedPropsDigest = ComputeC14nDigest(signedPropsWithNs);

            var keyInfoWithNs = keyInfo.Replace("<ds:KeyInfo", $"<ds:KeyInfo {xmlnsKeyInfo}");
            var keyInfoDigest = ComputeC14nDigest(keyInfoWithNs);

            // Create SignedInfo
            var signedInfo = $@"<ds:SignedInfo>
                <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
                <ds:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"" />
                <ds:Reference Id=""{_reference0Id}"" URI="""">
                    <ds:Transforms>
                        <ds:Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
                    </ds:Transforms>
                    <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
                    <ds:DigestValue>{documentDigest}</ds:DigestValue>
                </ds:Reference>
                <ds:Reference Id=""{_reference1Id}"" URI=""#{_keyInfoId}"">
                    <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
                    <ds:DigestValue>{keyInfoDigest}</ds:DigestValue>
                </ds:Reference>
                <ds:Reference Type=""http://uri.etsi.org/01903#SignedProperties"" URI=""#{_signedPropertiesId}"">
                    <ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
                    <ds:DigestValue>{signedPropsDigest}</ds:DigestValue>
                </ds:Reference>
            </ds:SignedInfo>";

            // Canonicalize SignedInfo for signing
            var signedInfoWithNs = signedInfo.Replace("<ds:SignedInfo", $"<ds:SignedInfo {xmlnsSignedInfo}");

            var docSignedInfo = new XmlDocument();
            docSignedInfo.PreserveWhitespace = false;
            docSignedInfo.LoadXml(signedInfoWithNs);
            var canonicalSignedInfo = Canonicalize(docSignedInfo);

            // Sign the canonicalized SignedInfo
            byte[] signature;
            using (var privateKey = certificate.GetRSAPrivateKey())
            {
                if (privateKey == null)
                    throw new InvalidOperationException("Could not get RSA private key from certificate");

                signature = privateKey.SignData(
                    Encoding.UTF8.GetBytes(canonicalSignedInfo),
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
            }

            var signatureValue = Convert.ToBase64String(signature);

            // Create the complete signature
            var completeSignature = $@"<ds:Signature xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Id=""{_signatureId}"">
                {signedInfo}
                <ds:SignatureValue Id=""{_signatureValueId}"">{signatureValue}</ds:SignatureValue>
                {keyInfo}
                <ds:Object Id=""{_xadesObjectId}"">
                    <xades:QualifyingProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" Id=""QualifyingProperties-{Guid.NewGuid()}"" Target=""#{_signatureId}"">
                        {signedProperties}
                    </xades:QualifyingProperties>
                </ds:Object>
            </ds:Signature>";

            // Insert the signature into the document
            string closingTag;
            switch (_documentType)
            {
                case "01":
                    closingTag = "</FacturaElectronica>";
                    break;
                case "02":
                    closingTag = "</NotaDebitoElectronica>";
                    break;
                case "03":
                    closingTag = "</NotaCreditoElectronica>";
                    break;
                case "04":
                    closingTag = "</TiqueteElectronico>";
                    break;
                case "05":
                case "06":
                case "07":
                    closingTag = "</MensajeReceptor>";
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported document type: {_documentType}");
            }

            // Find and replace the closing tag
            var pos = xml.LastIndexOf(closingTag);
            if (pos == -1)
                throw new InvalidOperationException($"Could not find closing tag: {closingTag}");

            return xml.Substring(0, pos) + completeSignature + closingTag;
        }

        private string Canonicalize(XmlDocument doc)
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = new UTF8Encoding(false)
            };

            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, settings);

            doc.PreserveWhitespace = false;
            doc.WriteTo(writer);
            writer.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        private string ComputeSha256Hash(byte[] input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(input);
            return Convert.ToBase64String(hash);
        }

        private string ComputeC14nDigest(string xml)
        {
            // Load the XML
            var doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(xml);

            // Canonicalize
            var canonicalXml = Canonicalize(doc);

            // Compute hash
            return ComputeSha256Hash(canonicalXml);
        }

        private string HexToDecimal(string hex)
        {
            // Convert hex string to decimal string without precision issues
            var dec = new List<int>();

            foreach (var c in hex)
            {
                var carry = Convert.ToInt32(c.ToString(), 16);

                for (var i = 0; i < dec.Count; i++)
                {
                    var val = dec[i] * 16 + carry;
                    dec[i] = val % 10;
                    carry = val / 10;
                }

                while (carry > 0)
                {
                    dec.Add(carry % 10);
                    carry /= 10;
                }
            }

            dec.Reverse();
            return string.Join("", dec);
        }
    }
}
