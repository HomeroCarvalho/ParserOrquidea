using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace parser
{
    public class FileForObjeto : FuncionalidadeXml
    {

        private PropriedadesXML fileXML { get; set; }
        public FileForObjeto(string fileName)
        {
            this.fileNameXml = fileName;

        }

        public bool Write(Objeto objetoToWrite, XElement root)
        {
            if (root == null)
                root = this.GetRootWriteMode();

            if (objetoToWrite.GetFields().Count == 0)
                return false;

            XElement noNomeClasse = new XElement("nomeClasse", objetoToWrite.GetTipo());
            XElement noNomeObjeto = new XElement("nome", objetoToWrite.GetNome());
            XElement noValorObjeto = new XElement("valor", objetoToWrite.GetValor().ToString());


            XElement noContainerData = new XElement("Objeto");
            XElement dadosPropriedadesObjetos = new XElement("objetosPropriedades");


            if ((objetoToWrite.GetFields() != null) && (objetoToWrite.GetFields().Count > 0))
            {
                foreach (Objeto aPropertyObject in objetoToWrite.GetFields())
                {
                    PropriedadesXML propriedadeXml = new PropriedadesXML();
                    propriedadeXml.Write(aPropertyObject, dadosPropriedadesObjetos);
                }

            }
            noContainerData.Add(noNomeClasse);
            noContainerData.Add(noNomeObjeto);
            noContainerData.Add(noValorObjeto);
            noContainerData.Add(dadosPropriedadesObjetos);

            root.Add(noContainerData);
            

            return true;

        }

        public Objeto Read(XElement raiz)
        {
            if (raiz == null)
                raiz = this.GetRootReadMode();

            Escopo escopo = new Escopo(new List<string>());

            List<Objeto> lstPropriedadesLidas = new List<Objeto>();
    

            XElement root = raiz.Element("Objeto");
            string nomeClasse = root.Element("nomeClasse").Value;
            string nomeDoObjeto = root.Element("nome").Value;
            object valorDoObjeto = root.Element("valor").Value;


            XElement rootPropriedadesObjetos = root.Element("objetosPropriedades");
            if ((rootPropriedadesObjetos != null) && (rootPropriedadesObjetos.Elements("Objeto") != null)) 
            {
                List<XElement> listaXMLPropriedadesObjetos = rootPropriedadesObjetos.Elements("Objeto").ToList<XElement>();
                foreach (XElement nodePropriedadeObjeto in listaXMLPropriedadesObjetos)
                {
                    PropriedadesXML propriedadeXml = new PropriedadesXML();
                    Objeto umaPropriedade = propriedadeXml.Read(nodePropriedadeObjeto); // le recursivamente as propriedades.
                    lstPropriedadesLidas.Add(umaPropriedade);
                }

            }
            Objeto objetoLido = new Objeto("public", nomeClasse, nomeDoObjeto, valorDoObjeto);
            objetoLido.GetFields().AddRange(lstPropriedadesLidas);
            return objetoLido;

        }
    }
}
