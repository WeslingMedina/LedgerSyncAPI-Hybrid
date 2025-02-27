using Application.Interfaces;
using Domain.Enums;
using System.Numerics;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public class XadesEpesSignerService : IXmlSignerService
{
    private const string XadesNamespace = "http://uri.etsi.org/01903/v1.3.2#";
    private const string XmlDsigNamespace = "http://www.w3.org/2000/09/xmldsig#";

    public dynamic signPolicy { get; set; }
    public string signatureID { get; set; }
    public string signatureValue { get; set; }
    public string XadesObjectId { get; set; }
    public string KeyInfoId { get; set; }
    public string Reference0Id { get; set; }
    public string Reference1Id { get; set; }
    public string SignedProperties { get; set; }
    public string tipoDoc { get; set; }
    public string xml1 { get; set; }
    private DateTime? signTime = null;
    private RSA? publicKey = null;
    private RSA? privateKey = null;
    private string? cerROOT = null;
    private string? cerINTERMEDIO = null;

    private static readonly Dictionary<string, string> NODOS_NS = new Dictionary<string, string>
    {
        {"URL", "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/"},
        {"01", "facturaElectronica"},
        {"02", "notaDebitoElectronica"},
        {"03", "notaCreditoElectronica"},
        {"04", "tiqueteElectronico"},
        {"05", "mensajeReceptor"},
        {"06", "mensajeReceptor"},
        {"07", "mensajeReceptor"}
    };

    private static readonly dynamic POLITICA_FIRMA = new
    {
        name = "",
        url = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/Resoluci%C3%B3n_General_sobre_disposiciones_t%C3%A9cnicas_comprobantes_electr%C3%B3nicos_para_efectos_tributarios.pdf",
        digest = "DWxin1xWOeI8OuWQXazh4VjLWAaCLAA954em7DMh0h8=" // digest en sha256 y base64
    };

    public string SignXml(string P12Url, string pin, string xmlContent, DocumentType documentType)
    {
        byte[] pfxData = File.ReadAllBytes(P12Url);

        // Cargar el certificado con flags para claves exportables
        X509Certificate2 cert = new X509Certificate2(
            pfxData,
            pin,
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.MachineKeySet |
            X509KeyStorageFlags.PersistKeySet
        );

        // Verificar clave privada
        if (!cert.HasPrivateKey)
            throw new InvalidOperationException("El certificado no contiene una clave privada");

        // Obtener claves con verificación adicional
        this.privateKey = cert.GetRSAPrivateKey() ?? throw new InvalidOperationException("Clave privada RSA no disponible");
        this.publicKey = cert.GetRSAPublicKey() ?? throw new InvalidOperationException("Clave pública RSA no disponible");

        // Intento seguro de exportación
        RSAParameters privateKeyParams;
        try
        {
            privateKeyParams = this.privateKey.ExportParameters(true);
        }
        catch (CryptographicException ex)
        {
            // Intento alternativo para proveedores de Windows
            if (this.privateKey is RSACryptoServiceProvider rsaCsp)
            {
                privateKeyParams = new RSAParameters
                {
                    Modulus = rsaCsp.ExportParameters(true).Modulus,
                    Exponent = rsaCsp.ExportParameters(true).Exponent,
                    D = rsaCsp.ExportParameters(true).D,
                    P = rsaCsp.ExportParameters(true).P,
                    Q = rsaCsp.ExportParameters(true).Q,
                    DP = rsaCsp.ExportParameters(true).DP,
                    DQ = rsaCsp.ExportParameters(true).DQ,
                    InverseQ = rsaCsp.ExportParameters(true).InverseQ
                };
            }
            else
            {
                throw new SecurityException("No se puede exportar la clave privada. Asegure que:", ex);
            }
        }

        // Codificar el módulo (n) y el exponente (e) en base64
        string modulusBase64 = Convert.ToBase64String(privateKeyParams.Modulus);
        string exponentBase64 = Convert.ToBase64String(privateKeyParams.Exponent);

        // Asignar a las variables públicas
        string publicKeyString = Convert.ToBase64String(publicKey.ExportSubjectPublicKeyInfo());

        // Mostrar los resultados
        Console.WriteLine("Clave pública en base64: " + publicKeyString);
        Console.WriteLine("Módulo (n) en base64: " + modulusBase64);
        Console.WriteLine("Exponente (e) en base64: " + exponentBase64);


        this.signPolicy = XadesEpesSignerService.POLITICA_FIRMA;
        this.signatureID = "Signature-ddb543c7-ea0c-4b00-95b9-d4bfa2b4e411";
        this.signatureValue = "SignatureValue-ddb543c7-ea0c-4b00-95b9-d4bfa2b4e411";
        this.XadesObjectId = "XadesObjectId-43208d10-650c-4f42-af80-fc889962c9ac";
        this.KeyInfoId = "KeyInfoId-" + this.signatureID;

        this.Reference0Id = "Reference-0e79b719-635c-476f-a59e-8ac3ba14365d";
        this.Reference1Id = "ReferenceKeyInfo";

        this.SignedProperties = "SignedProperties-" + this.signatureID;

        this.tipoDoc = documentType.ToString();
        this.xml1 = Encoding.UTF8.GetString(Convert.FromBase64String(xmlContent)); // Decodificación base64
        xml1 = this.InsertaFirma(this.xml1, pfxData, modulusBase64, exponentBase64,pin);

        return xml1;
    }

    private string InsertaFirma(string xml, byte[] pfxData, string modulusBase64, string exponentBase64,string pin)
    {
        if (this.publicKey == null || this.privateKey == null)
        {
            return xml;
        }

        XmlDocument d = new XmlDocument();
        d.PreserveWhitespace = true;
        d.LoadXml(xml);

        // Proceso de canonización
        XmlDsigC14NTransform canonizador = new XmlDsigC14NTransform();
        canonizador.LoadInput(d);

        // Forma correcta de obtener la salida canonizada
        Stream outputStream = (Stream)canonizador.GetOutput(typeof(Stream));
        using StreamReader reader = new StreamReader(outputStream);
        string canonizadoreal = reader.ReadToEnd();

        string xmlnsKeyInfo = $"xmlns=\"{XadesEpesSignerService.NODOS_NS["URL"]}{XadesEpesSignerService.NODOS_NS["01"]}\" ";
        string xmnlsSignedProps = xmlnsKeyInfo;
        string xmnlsSigneg = xmlnsKeyInfo;

        string xmlns = "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                       "xmlns:fe=\"http://www.dian.gov.co/contratos/facturaelectronica/v1\" " +
                       "xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\"";

        xmlnsKeyInfo += "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                       "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                       "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

        xmnlsSignedProps += "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                           "xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\" " +
                           "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                           "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

        xmnlsSigneg += "xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" " +
                      "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                      "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";

        string signTime1 = DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss-06:00");

        // Asumiendo que tienes el certificado en una variable (puede ser un archivo o una cadena)
        // Cargar el certificado (debe reemplazar "ruta/del/certificado.pem" con la correcta)
        X509Certificate2 cert = new X509Certificate2(pfxData,pin);

        // Obtener la huella digital en SHA-256 y codificarla en Base64
        string certDigest;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(cert.RawData);
            certDigest = Convert.ToBase64String(hashBytes);
        }

        // Obtener y formatear el emisor del certificado
        List<string> certIssuer = ParseIssuer(cert.Issuer)
            .Select(kv => kv.Key + "=" + kv.Value)
            .Reverse() // Equivalente a array_reverse en PHP
            .ToList();

        string certIssuerString = string.Join(", ", certIssuer);

        // Obtener el número de serie del certificado en formato hexadecimal
        string serialNumberHex = cert.SerialNumber;

        // Verificar si el número de serie comienza con "0x" y convertirlo a decimal si es necesario
        string serialNumber = serialNumberHex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? StringHexToStringDec(serialNumberHex.Substring(2)) // Eliminar "0x" antes de convertir
            : serialNumberHex;

        // Mostrar resultados
        Console.WriteLine("Cert Digest: " + certDigest);
        Console.WriteLine("Cert Issuer: " + certIssuerString);
        Console.WriteLine("Serial Number (Decimal): " + serialNumber);


        string prop = "<xades:SignedProperties Id=\"" + this.SignedProperties + "\">" +
    "<xades:SignedSignatureProperties>" +
        "<xades:SigningTime>" + signTime1 + "</xades:SigningTime>" +
        "<xades:SigningCertificate>" +
            "<xades:Cert>" +
                "<xades:CertDigest>" +
                    "<ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" />" +
                    "<ds:DigestValue>" + certDigest + "</ds:DigestValue>" +
                "</xades:CertDigest>" +
                "<xades:IssuerSerial>" +
                    "<ds:X509IssuerName>" + certIssuer + "</ds:X509IssuerName>" +
                    "<ds:X509SerialNumber>" + serialNumber + "</ds:X509SerialNumber>" +
                "</xades:IssuerSerial>" +
            "</xades:Cert>" +
        "</xades:SigningCertificate>" +
        "<xades:SignaturePolicyIdentifier>" +
            "<xades:SignaturePolicyId>" +
                "<xades:SigPolicyId>" +
                    "<xades:Identifier>" + this.signPolicy.url + "</xades:Identifier>" +
                    "<xades:Description />" +
                "</xades:SigPolicyId>" +
                "<xades:SigPolicyHash>" +
                    "<ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" />" +
                    "<ds:DigestValue>" + this.signPolicy.digest + "</ds:DigestValue>" +
                "</xades:SigPolicyHash>" +
            "</xades:SignaturePolicyId>" +
        "</xades:SignaturePolicyIdentifier>" +
        "<xades:SignerRole>" +
            "<xades:ClaimedRoles>" +
                "<xades:ClaimedRole>Emisor</xades:ClaimedRole>" +
            "</xades:ClaimedRoles>" +
        "</xades:SignerRole>" +
    "</xades:SignedSignatureProperties>" +
    "<xades:SignedDataObjectProperties>" +
        "<xades:DataObjectFormat ObjectReference=\"#" + this.Reference0Id + "\">" +
            "<xades:MimeType>text/xml</xades:MimeType>" +
            "<xades:Encoding>UTF-8</xades:Encoding>" +
        "</xades:DataObjectFormat>" +
    "</xades:SignedDataObjectProperties>" +
    "</xades:SignedProperties>";

        string publicPEM = GetCertificatePEM(new X509Certificate2(pfxData,pin));

        // Construir el XML de KeyInfo
        string kInfo = $@"<ds:KeyInfo Id=""{this.KeyInfoId}"">
                            <ds:X509Data>
                                <ds:X509Certificate>{publicPEM}</ds:X509Certificate>
                            </ds:X509Data>
                            <ds:KeyValue>
                                <ds:RSAKeyValue>
                                    <ds:Modulus>{modulusBase64}</ds:Modulus>
                                    <ds:Exponent>{exponentBase64}</ds:Exponent>
                                </ds:RSAKeyValue>
                            </ds:KeyValue>
                          </ds:KeyInfo>";
        // Reemplazar <xades:SignedProperties con el valor de xmnls_signedprops
        string aconop = prop.Replace("<xades:SignedProperties", "<xades:SignedProperties " + xmnlsSignedProps);

        // Calcular el digest usando la función retC14DigestSha1
        string propDigest = RetC14DigestSha1(aconop);
        Console.WriteLine("Digest de prop: " + propDigest);

        // Reemplazar <ds:KeyInfo con el valor de xmlns_keyinfo
        string keyinfo_para_hash1 = kInfo.Replace("<ds:KeyInfo", "<ds:KeyInfo " + xmlnsKeyInfo);

        // Calcular el digest usando la función retC14DigestSha1
        string kInfoDigest = RetC14DigestSha1(keyinfo_para_hash1);
        Console.WriteLine("Digest de KeyInfo: " + kInfoDigest);

        var documentDigest = CalculateDocumentDigest(canonizadoreal);

        StringBuilder sInfoBuilder = new StringBuilder();

        sInfoBuilder.AppendLine("<ds:SignedInfo>");
        sInfoBuilder.AppendLine("<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />");
        sInfoBuilder.AppendLine("<ds:SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\" />");

        sInfoBuilder.AppendLine("<ds:Reference Id=\"" + Reference0Id + "\" URI=\"\">");
        sInfoBuilder.AppendLine("<ds:Transforms>");
        sInfoBuilder.AppendLine("<ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />");
        sInfoBuilder.AppendLine("</ds:Transforms>");
        sInfoBuilder.AppendLine("<ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" />");
        sInfoBuilder.AppendLine("<ds:DigestValue>" + documentDigest + "</ds:DigestValue>");
        sInfoBuilder.AppendLine("</ds:Reference>");

        sInfoBuilder.AppendLine("<ds:Reference Id=\"" + Reference1Id + "\" URI=\"#" + KeyInfoId + "\">");
        sInfoBuilder.AppendLine("<ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" />");
        sInfoBuilder.AppendLine("<ds:DigestValue>" + kInfoDigest + "</ds:DigestValue>");
        sInfoBuilder.AppendLine("</ds:Reference>");

        sInfoBuilder.AppendLine("<ds:Reference Type=\"http://uri.etsi.org/01903#SignedProperties\" URI=\"#" + SignedProperties + "\">");
        sInfoBuilder.AppendLine("<ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" />");
        sInfoBuilder.AppendLine("<ds:DigestValue>" + propDigest + "</ds:DigestValue>");
        sInfoBuilder.AppendLine("</ds:Reference>");

        sInfoBuilder.AppendLine("</ds:SignedInfo>");

        var sInfo = sInfoBuilder.ToString();


        // Reemplazar SignedInfo con los atributos XML de namespaces
        string signaturePayload = sInfo.Replace("<ds:SignedInfo", "<ds:SignedInfo " + xmnlsSigneg);

        // Crear el documento XML
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(signaturePayload);

        // Canonicalizar el XML
        string signaturePayloadCanonicalized = CanonicalizeXml(doc);

        // Firmar el contenido
        string signatureResult = "";
        string algo = "SHA256";

        // Firmar los datos
        string base64Signature = SignData(signaturePayload, algo);

        // Construir la firma en formato XML
        var sigBuilder = new StringBuilder();
        sigBuilder.AppendLine($"<ds:Signature xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" Id=\"{this.signatureID}\">");
        sigBuilder.AppendLine(sInfo);
        sigBuilder.AppendLine($"<ds:SignatureValue Id=\"{this.signatureValue}\">{base64Signature}</ds:SignatureValue>");
        sigBuilder.AppendLine(kInfo);
        sigBuilder.AppendLine($"<ds:Object Id=\"{this.XadesObjectId}\">");
        sigBuilder.AppendLine($"<xades:QualifyingProperties xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\" Id=\"QualifyingProperties-012b8df6-b93e-4867-9901-83447ffce4bf\" Target=\"#{this.signatureID}\">");
        sigBuilder.AppendLine(prop);
        sigBuilder.AppendLine("</xades:QualifyingProperties>");
        sigBuilder.AppendLine("</ds:Object>");
        sigBuilder.AppendLine("</ds:Signature>");
        var sig = sigBuilder.ToString();


        string buscar = "";
        string reemplazar = "";
        string tipoDoc = this.tipoDoc; // Asumimos que 'tipoDoc' es una propiedad de la clase

        if (tipoDoc == "01")
        {
            buscar = "</FacturaElectronica>";
            reemplazar = sig + "</FacturaElectronica>";
        }else if (tipoDoc == "FacturaElectronica")
        {
            buscar = "</FacturaElectronica>";
            reemplazar = sig + "</FacturaElectronica>";
        }
        else if (tipoDoc == "02")
        {
            buscar = "</NotaDebitoElectronica>";
            reemplazar = sig + "</NotaDebitoElectronica>";
        }
        else if (tipoDoc == "03")
        {
            buscar = "</NotaCreditoElectronica>";
            reemplazar = sig + "</NotaCreditoElectronica>";
        }
        else if (tipoDoc == "04")
        {
            buscar = "</TiqueteElectronico>";
            reemplazar = sig + "</TiqueteElectronico>";
        }
        else if (tipoDoc == "05" || tipoDoc == "06" || tipoDoc == "07")
        {
            buscar = "</MensajeReceptor>";
            reemplazar = sig + "</MensajeReceptor>";
        }

        int pos = xml.LastIndexOf(buscar);
        if (pos != -1)
        {
            xml = xml.Remove(pos, buscar.Length).Insert(pos, reemplazar);
        }

        // Lógica para modificar el XML y agregar la firma
        return xml; // Retornar el XML modificado
    }
    public string SignData(string signaturePayload, string algorithm)
    {
        // Convertir el payload a bytes
        byte[] payloadBytes = Encoding.UTF8.GetBytes(signaturePayload);

        // Seleccionar el algoritmo de hash
        HashAlgorithmName hashAlgorithmName = HashAlgorithmName.SHA256; // Usamos SHA256 como en PHP
        RSASignaturePadding signaturePadding = RSASignaturePadding.Pkcs1;

        // Firmar los datos
        byte[] signatureResult = this.privateKey.SignData(payloadBytes, hashAlgorithmName, signaturePadding);

        // Convertir la firma a base64
        string base64Signature = Convert.ToBase64String(signatureResult);

        return base64Signature;
    }

    private string ConvertPemToBase64(string pem)
    {
        // Eliminar las líneas "-----BEGIN PRIVATE KEY-----" y "-----END PRIVATE KEY-----"
        var lines = pem.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string base64 = string.Join("", lines.Skip(1).Take(lines.Length - 2));
        return base64;
    }

    // Función para realizar la canonicalización del XML
    private string CanonicalizeXml(XmlDocument doc)
    {
        using (var stream = new System.IO.MemoryStream())
        {
            // Canonicalizar el XML (equivalente al C14N de PHP)
            XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Document, OmitXmlDeclaration = true };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                doc.Save(writer);
            }
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public string CalculateDocumentDigest(string canonizadoreal)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonizadoreal));
            return Convert.ToBase64String(hashBytes);
        }
    }

    // Función RetC14DigestSha1, que ya se implementó antes
    public string RetC14DigestSha1(string strcadena)
    {
        // Remover saltos de línea y retornos de carro
        strcadena = strcadena.Replace("\r", "").Replace("\n", "");

        // Cargar el XML
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(strcadena);

        // Obtener el XML en formato C14N (Canonical XML)
        string c14nString = GetC14N(xmlDoc);

        // Realizar el hash SHA256 y codificarlo en base64
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(c14nString));
        return Convert.ToBase64String(hashBytes);
    }

    // Método auxiliar para obtener el C14N del XML
    private string GetC14N(XmlDocument xmlDoc)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            // Crear el objeto XmlWriterSettings para C14N
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;  // Omite la declaración XML
            settings.Encoding = Encoding.UTF8;

            // Crear un XmlWriter que escribe el XML en formato C14N
            using (XmlWriter writer = XmlWriter.Create(ms, settings))
            {
                xmlDoc.WriteContentTo(writer);
            }

            // Convertir el resultado en un string
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }


    // Función para obtener el certificado en formato PEM
    private string GetCertificatePEM(X509Certificate2 certificate)
    {
        // Obtener el certificado en formato DER (binario)
        byte[] certBytes = certificate.Export(X509ContentType.Cert);

        // Convertir el contenido binario a base64
        string base64Cert = Convert.ToBase64String(certBytes);

        // Formatear el certificado con las cabeceras y pies PEM
        StringBuilder pemBuilder = new StringBuilder();
        pemBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
        pemBuilder.AppendLine(base64Cert);
        pemBuilder.AppendLine("-----END CERTIFICATE-----");

        return pemBuilder.ToString();
    }

    // Función para parsear el emisor del certificado
    static Dictionary<string, string> ParseIssuer(string issuer)
    {
        Dictionary<string, string> issuerData = new Dictionary<string, string>();
        string[] parts = issuer.Split(',');

        foreach (string part in parts)
        {
            string[] keyValue = part.Trim().Split('=');
            if (keyValue.Length == 2)
            {
                issuerData[keyValue[0]] = keyValue[1];
            }
        }

        return issuerData;
    }

    // Convierte una cadena hexadecimal a una cadena decimal sin pérdida de precisión
    static string StringHexToStringDec(string hex)
    {
        BigInteger decimalValue = BigInteger.Zero;
        foreach (char hexDigit in hex)
        {
            int digitValue = Convert.ToInt32(hexDigit.ToString(), 16);
            decimalValue = decimalValue * 16 + digitValue;
        }
        return decimalValue.ToString();
    }
}