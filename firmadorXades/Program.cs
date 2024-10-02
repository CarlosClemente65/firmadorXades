using System;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using GestionCertificadosDigitales;
using System.IO;
class XadesSigner
{
    static void Main(string[] args)
    {
        try
        {
            GestionarCertificados certificados = new GestionarCertificados();
            certificados.cargarCertificadosAlmacen();
            string serieCeritificado = "726e0db7a17efa04603b7f010ba43fa6";
            string ficheroEntrada = @"D:\Programacion\C#\firmadorXades\Pruebas\factura.xml";
            string ficheroSalida = Path.Combine(Path.GetDirectoryName(ficheroEntrada), Path.GetFileNameWithoutExtension(ficheroEntrada) + "_signed" + Path.GetExtension(ficheroEntrada));

            XmlDocument doc = new XmlDocument();
            doc.Load(ficheroEntrada);

            XmlElement elemToSign = doc.DocumentElement;
            //Este es el texto que se debe buscar en el certificado
            //string textoBusqueda = string.Empty;

            (X509Certificate2 certificado, bool encontrado) = certificados.exportaCertificadoDigital(serieCeritificado);
            if (certificado.SignatureAlgorithm.Value != "1.2.840.113549.1.1.11")
            {
                throw new Exception("El certificado no soporta el algoritmo de firma");
            }
            if (!certificado.HasPrivateKey)
            {
                throw new Exception("El certificado no tiene una clave privada");
            }
            RSA rsa = null;
            if (encontrado)
            {
                rsa = certificado.GetRSAPrivateKey();
            }

            SignedXml ficheroFirmado = new SignedXml(elemToSign);
            Reference reference = new Reference();
            if (rsa != null)
            {
                ficheroFirmado.SigningKey = rsa;
                reference.Uri = "";
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
            }
            else
            {
                Console.WriteLine("No se ha encontrado el certificado digital o no tiene una clave privada");
            }

            // Añadir la referencia a la firma
            ficheroFirmado.AddReference(reference);
            ficheroFirmado.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

            // Crear el nodo de política de firma
            XmlElement policyElement = doc.CreateElement("Policy", "http://www.w3.org/2000/09/xmldsig#");
            policyElement.SetAttribute("URI", "http://www.facturae.es/schemas/2.0/facturae-policies.xsd"); // Cambia esta URL si es necesario

            // Añadir el nodo de política de firma a la estructura del documento
            XmlElement signatureElement = ficheroFirmado.GetXml();
            signatureElement.AppendChild(policyElement);

            ficheroFirmado.ComputeSignature();

            XmlElement xmlSignature = ficheroFirmado.GetXml();
            elemToSign.AppendChild(xmlSignature);

            doc.Save(ficheroSalida);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    //static X509Certificate2 BuscarCertificado(string textoBusqueda)
    //{
    //    //Devuelve el certificado que contiene el texto a buscar en la serie, el NIF o nombre del titular

    //    string resultadoBusqueda = string.Empty;
    //    var buscaCertificado = listaCertificados.certificadosInfo.Find(cert =>
    //        cert.nifCertificado.Contains(textoBusqueda) ||
    //        cert.titularCertificado.Contains(textoBusqueda) ||
    //        cert.serieCertificado == textoBusqueda
    //        );
    //    if (buscaCertificado != null)
    //    {
    //        resultadoBusqueda = buscaCertificado.serieCertificado;
    //    }

    //    return resultadoBusqueda;
    //}


}
