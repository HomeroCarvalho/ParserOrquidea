using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Schema;
using System.Xml;
using System.IO;

namespace parser
{
    public class xmlREADER_LINGUAGEM
    {



        public void LE_ARQUIVO_LINGUAGEM(ref List<producao> producoes, string nomearquivo)
        {
            XmlTextReader reader = null;
            string name = "";
            string tipo = "";
            string mqEstados = "";
            string palavrasChave = "";
            string VM = "";

            try
            {
                string fileName = Path.GetFullPath("..\\..\\..\\BNFdefinitions\\" + nomearquivo + ".xml");   
                reader = new XmlTextReader(fileName);
            }
            catch (Exception e)
            {
                reader.Close();
                throw (new Exception("Mensagem: " + e.Message + " Pilha: " + e.StackTrace + " Exception String: " + e.ToString()));
            }

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
                producoes.Add(new producao(name, tipo, mqEstados, palavrasChave));
            } // while


            reader.Close();




        } // LE_ARQUIVO_LINGUAGEM()


    }// class xmlREADER_LINGUAGEM
} // namespace parser