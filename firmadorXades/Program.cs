using System;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;

class XadesSigner
{
    static void Main(string[] args)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("./examples/ejemploRegistro.xml");

            XmlElement elemToSign = doc.DocumentElement;
            //Este es el texto que se debe buscar en el certificado
            string textoBusqueda = string.Empty;

            X509Certificate2 certificate = BuscarCertificado("keyingProviderMy");
            RSA rsa = (RSA)certificate.PrivateKey;

            SignedXml signedXml = new SignedXml(elemToSign);
            signedXml.SigningKey = rsa;

            Reference reference = new Reference();
            reference.Uri = "";
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            XmlElement xmlSignature = signedXml.GetXml();
            elemToSign.AppendChild(xmlSignature);

            doc.Save("./examples/ejemploRegistro-signed-epes-xades4j.xml");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static X509Certificate2 BuscarCertificado(string textoBusqueda)
    {
        //Devuelve el certificado que contiene el texto a buscar en la serie, el NIF o nombre del titular

        string resultadoBusqueda = string.Empty;
        var buscaCertificado = listaCertificados.certificadosInfo.Find(cert =>
            cert.nifCertificado.Contains(textoBusqueda) ||
            cert.titularCertificado.Contains(textoBusqueda) ||
            cert.serieCertificado == textoBusqueda
            );
        if (buscaCertificado != null)
        {
            resultadoBusqueda = buscaCertificado.serieCertificado;
        }

        return resultadoBusqueda;
    }


}
