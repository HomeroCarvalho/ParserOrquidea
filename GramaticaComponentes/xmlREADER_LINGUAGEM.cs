using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
namespace parser
{
    public class xmlREADER_LINGUAGEM
    {



        public void LE_ARQUIVO_LINGUAGEM(List<producao> producoes, string nomearquivo)
        {
            XmlTextReader reader = null;
            string name = "";
            string tipo = "";
            string mqEstados = "";
            string palavrasChave = "";
            string VM = "";
            string fileName = Path.GetFullPath(nomearquivo + ".xml");
            reader = new XmlTextReader(fileName);
            
            while (reader.EOF == false)
            {
                while ((reader.EOF == false) && (!reader.Name.Equals("nome")))
                    reader.Read();
                if (!reader.EOF)
                {
                    name = reader.ReadElementString("nome");
                }

                while (((reader.EOF == false) && !reader.Name.Equals("tipo")))
                    reader.Read();
                if (!reader.EOF)
                {
                    tipo = reader.ReadString();
                }

                while ((reader.EOF == false) && (!reader.Name.Equals("maquinaDeEstados")))
                    reader.Read();
                if (!reader.EOF)
                {
                    mqEstados = reader.ReadString();
                }


                while ((reader.EOF == false) && (!reader.Name.Equals("palavrasChave")))
                    reader.Read();
                if (!reader.EOF)
                {
                    palavrasChave = reader.ReadString();
                    
                }

                while ((reader.EOF == false) && (!reader.Name.Equals("VM")))
                    reader.Read();
                if (!reader.EOF)
                {
                    VM = reader.ReadString();
                }
                string[] todasPalavrasChave = palavrasChave.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                List<string> maquinaDeEstados = new List<string>() { mqEstados };
                producoes.Add(new producao(name, tipo, maquinaDeEstados, todasPalavrasChave));
            } // while
            reader.Close();
        }//void LE_ARQUIVO_IMAGEM()
    }// class
} // namespace