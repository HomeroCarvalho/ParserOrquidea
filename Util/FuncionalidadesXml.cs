using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
namespace parser
{
    public class FuncionalidadeXml
    {


        public XDocument document { get; protected set; }
        protected string fileNameXml;
        protected Escopo escopo;

        public XElement raiz { get; set; }

        public FuncionalidadeXml(string fileNameXml)
        {

            this.fileNameXml = fileNameXml;

        }


        public FuncionalidadeXml()
        {

        }

        // termina o processamento, e grava em arquivo.
        public void EndWrite(XElement root)
        {
            if (root != null)
                this.document.Root.Add(root);
            this.document.Save(this.fileNameXml, SaveOptions.None);

        }
        public void BeginWrite()
        {
            FileStream stream = new FileStream(this.fileNameXml, FileMode.Create, FileAccess.Write);
            stream.Close();
            stream.Dispose();
            this.GetRootWriteMode();
        }

        protected XElement GetRootWriteMode()
        {
            this.document = XDocument.Parse("<file></file>"); // cria o arquivo, pois foi resetado seu conteudo (apagado).
            this.raiz = this.document.Root;
            return this.raiz;
        }

        protected XElement GetRootReadMode()
        {
            XElement root;
            // é o caso em que queremos ler a partir do começo do arquivo, não de um no determinado.
            this.document = XDocument.Load(this.fileNameXml);
            root = this.document.Root;
            return root;
        }

    }
    public class PropriedadesXML : FuncionalidadeXml
    {
        public PropriedadesXML(string fileNameXml) : base(fileNameXml)
        {


        }

    

        public PropriedadesXML()
        {

        }

        public void Write(Objeto propriedades, XElement root)
        {

            if (root == null)
                root = this.GetRootWriteMode();


            string acessor = propriedades.GetAcessor();
            if (acessor == null)
                acessor = "private";
            string valor = propriedades.GetValor().ToString();
            if (valor == null)
                valor = "";

            XAttribute attributeAcessor = new XAttribute("acessor", acessor);
            XAttribute attributeTipo = new XAttribute("tipo", propriedades.GetTipo());
            XAttribute attributeNome = new XAttribute("nome", propriedades.GetNome());
            XAttribute attributeValor = new XAttribute("valor", valor);
            XAttribute attributeIsStatic = new XAttribute("isStatic", propriedades.isStatic);

            XElement umPropriedade = new XElement("propriedade", attributeAcessor, attributeTipo, attributeNome, attributeValor, attributeIsStatic);


            root.Add(umPropriedade);

        }


        public Objeto Read(XElement root)
        {
            if (root == null)
                root = GetRootReadMode();

            XElement umElemento = root.Element("propriedade");
            if (umElemento.Name.ToString().Equals("propriedade"))
            {
                string acessor = umElemento.Attribute("acessor").Value;
                string tipo = umElemento.Attribute("tipo").Value;
                string nome = umElemento.Attribute("nome").Value;
                string valor = umElemento.Attribute("valor").Value;
                bool isStatic = bool.Parse(umElemento.Attribute("isStatic").Value);

                Objeto umaPropriedade = new Objeto(acessor, nome, tipo, valor, isStatic);
                return umaPropriedade;
            }

            return null;
        }

    }

    public class MetodosXML : FuncionalidadeXml
    {
        public Funcao Read(XElement raiz)
        {
      
            /*
             * lista da estrutura de uma funcao em xml:
             * 1- cabecalho definicao de funcao "funcao".
             * 2- acessor
             * 3- nome da funcao
             * 4- classe da funcao.
             * 5- parametros da funcao
             * 6- instrucoes do corpo da funcao.
             */

            if (raiz == null)
                raiz = this.GetRootReadMode();



            List<Objeto> parametros = new List<Objeto>();
            

            XElement noAcessor = raiz.Element("acessor");
            XElement noNome = raiz.Element("nomeFuncao");
            XElement noClasseDaFuncao = raiz.Element("classe");
            XElement noTipoRetorno = raiz.Element("tipoRetorno");
            XElement noParametros = raiz.Element("parametros");
            ReadParameters(parametros, noParametros);


            string acessorFuncao = noAcessor.Value;
            string nomeFuncao = noNome.Value;
            string retornoDaFuncao = noTipoRetorno.Value;
            string classeDaFuncao = noClasseDaFuncao.Value;




            if (noNome.Attribute("isReflexao").Value.Equals("nao"))
            {
                // a funcao tem instrucoes que formam seu corpo.
                List<Instrucao> instrucoesDaFuncao = new List<Instrucao>();

                XElement cabecalhoInstrucoes = raiz.Element("instrucoes");

                List<XElement> listaInstrucoes = cabecalhoInstrucoes.Descendants("instrucao").ToList();
                if (listaInstrucoes != null)
                    foreach (XElement noUmInstrucao in listaInstrucoes)
                    {
                        InstrucaoXML instrucaoXmlCurrente = new InstrucaoXML();
                        instrucoesDaFuncao.Add(instrucaoXmlCurrente.Read(noUmInstrucao));
                    }
                
                if ((instrucoesDaFuncao != null) && (instrucoesDaFuncao.Count > 0))
                {
                    Funcao funcaoLida = new Funcao(classeDaFuncao, "public", nomeFuncao, parametros.ToArray(), retornoDaFuncao, instrucoesDaFuncao, escopo);
                    return funcaoLida;
                }
                else
                    return null;
            } // if




            if (noNome.Attribute("isReflexao").Value.Equals("Sim")) // a funcao nao tem instrucoes que formam seu corpo, eh uma funcao conseguida via API Reflexao.
            {
                Funcao funcaoLida = new Funcao(retornoDaFuncao, acessorFuncao, nomeFuncao, parametros.ToArray(), retornoDaFuncao, new List<Instrucao>(), escopo);
                return funcaoLida;
            }

            return null;

            throw new Exception("metodo nao terminado ainda, falta gravar/ler a classe do metodo. do jeito que esta, está o nome do tipo de retorno como o nome da classe");

        }

        private void ReadParameters(List<Objeto> parametros, XElement umElemento)
        {
            IEnumerable<XElement> noListaDeParametros = umElemento.Descendants();
            foreach (XElement umParametroXML in noListaDeParametros)
            {


                string nomeParametro = umParametroXML.Attribute("nome").Value;
                string tipoParametro = umParametroXML.Attribute("tipo").Value;

                parametros.Add(new Objeto("private", tipoParametro, nomeParametro, null, false));
            }  // while
        }



        public void Write(Funcao funcao, XElement root)
        {
            if (root == null)
                root = this.GetRootWriteMode();

            XElement cabecalho = new XElement("funcao");
            root.Add(cabecalho);


            XElement nomeFuncao = null;

            if (funcao.acessor == null)
            {
                XElement noAcessor = new XElement("acessor", new XAttribute("acessor", "protected"));
                root.Add(noAcessor);
            }
            else
            {
                XElement noAcessor = new XElement("acessor", new XAttribute("acessor", funcao.acessor));
                root.Add(noAcessor);
            }

            if (funcao.instrucoesFuncao != null)
            {

               

                nomeFuncao = new XElement("nomeFuncao", funcao.nome, new XAttribute("isReflexao", "nao"));
                root.Add(nomeFuncao);  // nome da funcao

                string classeDaFuncao = funcao.nomeClasse;
                root.Add(classeDaFuncao);
                
                XElement noTipoRetorno = new XElement("tipoRetorno", funcao.tipoReturn);
                root.Add(noTipoRetorno);

                XElement noParametros= WriteALLParameters(funcao); // parametros da funcao.
                root.Add(noParametros);


                XElement noInstrucoes = new XElement("instrucoes"); // instrucoes do corpo da funcao
                InstrucaoXML instrucoesXml = new InstrucaoXML(this.fileNameXml);

                foreach (Instrucao umaInstrucaoDaFuncao in funcao.instrucoesFuncao)
                    instrucoesXml.Write(umaInstrucaoDaFuncao, noInstrucoes);

                root.Add(noInstrucoes);

            }
            else
            if (funcao.instrucoesFuncao == null)
            {
                MethodInfo metodoComReflexao = funcao.InfoMethod;
                nomeFuncao = new XElement("nomeFuncao", funcao.nome, new XAttribute("isReflexao", "sim"));
                root.Add(nomeFuncao);

                XElement noParameetros = WriteALLParameters(funcao);
                root.Add(noParameetros);
            } // if
            throw new Exception("metodo nao terminado ainda, falta gravar/ler a classe do metodo. do jeito que esta, está o nome do tipo de retorno como o nome da classe");

        }

        private XElement WriteALLParameters(Funcao funcao)
        {
            XElement rootParametros = new XElement("parametros");

            if (funcao.parametrosDaFuncao != null) // escreve a lista de parametros da funcao.
                WriteParameters(funcao.parametrosDaFuncao.ToList<Objeto>(), rootParametros);

            return rootParametros;

        }

        private void WriteParameters(List<Objeto> parametros, XElement rootParams)
        {
            List<string> nomesParametros = new List<string>();
            List<string> tiposParametros = new List<string>();
            for (int x = 0; x < parametros.Count; x++)
            {
                nomesParametros.Add(parametros[x].GetNome());
                tiposParametros.Add(parametros[x].GetTipo());

                XElement umParametroEmXML = new XElement("parametro", new XAttribute("nome", nomesParametros[x]), new XAttribute("tipo", parametros[x].GetTipo()));
                rootParams.Add(umParametroEmXML);
            }

        }



        public MetodosXML(string fileNameXml) : base(fileNameXml)
        {
           
        }

        public MetodosXML()
        {

        }


    }

    public class ExpressaoXML : FuncionalidadeXml
    {
        public ExpressaoXML(string fileNameXml) : base(fileNameXml)
        {

        }

        public ExpressaoXML()
        {

        }
  

        public void Write(Expressao expressaoAGravar, XElement raiz)
        {

            if (raiz == null)
                raiz = GetRootWriteMode();

            XElement umaExpressaoXML = new XElement("expressao", new XElement("elementos", expressaoAGravar.ToString()));
            raiz.Add(umaExpressaoXML);

        }

        public Expressao Read(XElement raiz)
        {
            XElement expressaoXml;
            if (raiz == null)
            {
                raiz = this.GetRootReadMode();
                XElement raizDaExpressao = raiz.Element("expressao");
                expressaoXml = raizDaExpressao.Element("elementos");
            }
            else
                expressaoXml = raiz.Element("elementos");

            Expressao ExpressaLida2 = new ExpressaoSemValidacao(new List<string>() { expressaoXml.Value.ToString() });
            return ExpressaLida2;

        } // Read()

    }

    public class InstrucaoXML : FuncionalidadeXml
    {
       
        public InstrucaoXML(string fileNameXml) : base(fileNameXml)
        {

        }

        public InstrucaoXML()
        {

        }

        public void Write(Instrucao umaInstrucao, XElement root)
        {
            if (root == null)
                root = this.GetRootWriteMode();

            XElement xml_instrucao = new XElement("instrucao");

            WriteCodeAndExpressionsInstruction(umaInstrucao, xml_instrucao); // escreve os dados da instrucao.

            root.Add(xml_instrucao);


            if ((umaInstrucao.blocos != null) && (umaInstrucao.blocos.Count > 0))
            {
                XElement xml_blocos = new XElement("blocos");
                foreach (List<Instrucao> instrucoesDeUmBloco in umaInstrucao.blocos)
                    if (umaInstrucao.blocos.Count > 0)
                    {
                        XElement elementoBloco = new XElement("bloco");
                        if ((instrucoesDeUmBloco != null) && (instrucoesDeUmBloco.Count > 0))
                        {
                            foreach (Instrucao instrucaoBloco in instrucoesDeUmBloco)
                                Write(instrucaoBloco, elementoBloco); // escrita recursiva para todos as instruções dos blocos.

                            xml_blocos.Add(elementoBloco);
                        }
                    } // if



                if (xml_blocos.Element("bloco") != null)
                    root.Add(xml_blocos);
            }
           
        }

        private void WriteCodeAndExpressionsInstruction(Instrucao umaInstrucao, XElement root)
        {


            XElement codigoInstrucaoEmXML = new XElement("codigo_instrucao", umaInstrucao.code.ToString());
            XElement expressoes_xml = new XElement("expressoes");


            if ((umaInstrucao.expressoes != null) && (umaInstrucao.expressoes.Count > 0))
            {
                ExpressaoXML expressos_xml = new ExpressaoXML();

                if (umaInstrucao.expressoes != null)
                {
                    foreach (Expressao umaExpressao in umaInstrucao.expressoes)
                        expressos_xml.Write(umaExpressao, expressoes_xml);
                }

                root.Add(codigoInstrucaoEmXML);
                root.Add(expressoes_xml);
            }
            else
                root.Add(codigoInstrucaoEmXML);
        }



        public Instrucao Read(XElement root)
        {

            if (root == null)
                root = this.GetRootReadMode();


            
            string codeInText = root.Element("codigo_instrucao").Value;  // codigo
            int code = int.Parse(codeInText);


            List<Expressao> expressoesDaInstrucao = new List<Expressao>();
            ExpressaoXML funcionalidadeExpressoesXml = new ExpressaoXML(this.fileNameXml);


            XElement rootExpressoes = root.Element("expressoes");


            List<XElement> listaExpressoesEmXml = rootExpressoes.Descendants("expressao").ToList<XElement>();
            if (listaExpressoesEmXml != null)
            {

                foreach (XElement umaExpressao_xml in listaExpressoesEmXml)
                {
                    Expressao expressao = funcionalidadeExpressoesXml.Read(umaExpressao_xml);
                    expressoesDaInstrucao.Add(expressao);
                }
            }
            List<List<Instrucao>> blocosDeInstrucao = new List<List<Instrucao>>();
            XElement blocos = root.Element("blocos");
            if (blocos != null)
            {

                foreach (XElement umBloco in blocos.Descendants("bloco"))
                {
                    blocosDeInstrucao.Add(new List<Instrucao>());
                    if (umBloco != null)
                    {
                        InstrucaoXML instruncoesDeUmBloco = new InstrucaoXML();
                        Instrucao umaInstrucaoDeUmBloco = instruncoesDeUmBloco.Read(umBloco); // faz uma leitura recursiva, para ler todas instrucoes de um bloco.
   
                        if (umaInstrucaoDeUmBloco != null)
                            blocosDeInstrucao[blocosDeInstrucao.Count - 1].Add(umaInstrucaoDeUmBloco);

                    } // if
                }
            }

            Instrucao instrucaoLida = new Instrucao(code, expressoesDaInstrucao, blocosDeInstrucao);

            return instrucaoLida;

        } // Read()


    } // class

    public class OperadorXML : FuncionalidadeXml
    {
        public OperadorXML()
        {
        }

        public Operador Read(XElement root)
        {
            if (root == null)
                root = this.GetRootReadMode();

            XElement noDadosOperador = root.Element("operador");

            string nomeClasse = noDadosOperador.Element("classe").Value;
            string acessor = noDadosOperador.Element("acessor").Value;
            string tipoRetorno = noDadosOperador.Element("tipoRetorno").Value;
            string nome = noDadosOperador.Element("nome").Value;
            string tipoDeOperador = noDadosOperador.Element("tipoDeOperador").Value;
            string prioridade = noDadosOperador.Element("prioridade").Value;
            string nomeMetodo = noDadosOperador.Element("metodo").Value;

            MethodInfo[] metodo = Type.GetType(nomeClasse).GetMethods();
            MethodInfo metodoEncontrado = null;
            foreach (MethodInfo metodoDaClasse in metodo)
                if (metodoDaClasse.Name == nomeMetodo)
                {
                    metodoEncontrado = metodoDaClasse;
                    break;
                }


            Objeto[] tipoParametros = ReadParametrosOperador(noDadosOperador);

            List<Instrucao> instrucoesCorpoOperador = ReadInstrucoesOperador(noDadosOperador);
            if ((instrucoesCorpoOperador != null) && (instrucoesCorpoOperador.Count > 0))
            {
                Operador op = new Operador(nomeClasse, nome, int.Parse(prioridade), tipoParametros, tipoRetorno, null, escopo);
                return op;
            }
            else
            {
                Operador operadorLido1 = new Operador(nomeClasse, nome, int.Parse(prioridade), tipoParametros, tipoDeOperador, metodoEncontrado, escopo);
                return operadorLido1;
            }


        }
        public void Write(XElement root, Operador operador)
        {
            if ((operador.instrucoesFuncao == null) || (operador.instrucoesFuncao.Count == 0))
                throw new Exception("Operador: " + operador.ToString() + " nao é definido com instrucoes da linguagem. Nao eh possivel guardar operadores nativos, ou com função definida no escopo da linguagem.");

            if (root == null)
                root = this.GetRootWriteMode();

            string acessor = "private";
            if (operador.acessor != null)
                acessor = operador.acessor;

          
            
            
            XElement noClasse = new XElement("classe", operador.nomeClasseDoOperador);
            XElement noAcessor = new XElement("acessor", acessor);
            XElement noTipoRetorno = new XElement("tipoRetorno", operador.tipoRetorno);
            XElement noNome = new XElement("nome",operador.nome);
            XElement noTipoDeOperaddor = new XElement("tipoDeOperador",operador.GetTipo());
            XElement noPrioridade = new XElement("prioridade", operador.GetPrioridade().ToString());
            XElement noMetodoInfo = new XElement("metodo", operador.InfoMethod.Name);
          

            XElement noDadosOPerador = new XElement("operador");
            noDadosOPerador.Add(noAcessor);
            noDadosOPerador.Add(noTipoRetorno);
            noDadosOPerador.Add(noNome);
            noDadosOPerador.Add(noTipoDeOperaddor);
            noDadosOPerador.Add(noPrioridade);
            noDadosOPerador.Add(noMetodoInfo);

            WriteParametrosOperador(operador, noDadosOPerador);
            WriteInstrucoesOperador(operador, noDadosOPerador);


            root.Add(noDadosOPerador);

            
        }

        private void WriteInstrucoesOperador(Operador operador, XElement root)
        {
            List<Instrucao> instrucoesDoCorpoDoOperador = operador.instrucoesFuncao;
            XElement cabecalho = new XElement("instrucoes");
            foreach (Instrucao umaInstrucao in instrucoesDoCorpoDoOperador)
            {
                InstrucaoXML instrucaoXML = new InstrucaoXML();
                instrucaoXML.Write(umaInstrucao, cabecalho);
            } // foreach
            root.Add(cabecalho);
        }

        private List<Instrucao> ReadInstrucoesOperador(XElement root)
        {
            List<Instrucao> instrucoesLidas = new List<Instrucao>();
            List<XElement> nosInstrucoes = root.Elements("instrucao").ToList();
            foreach (XElement noDeUmaInstrucao in nosInstrucoes)
            {
                InstrucaoXML instrucaoXML = new InstrucaoXML();
                instrucoesLidas.Add(instrucaoXML.Read(noDeUmaInstrucao));
            }
            return instrucoesLidas;

        }
        private void WriteParametrosOperador(Operador operador, XElement root)
        {
            
            XElement _umParametroXml = new XElement("parametros");
            foreach (Objeto umParametro in operador.parametrosDaFuncao)
            {
                PropriedadesXML propriedadesXML = new PropriedadesXML();
                propriedadesXML.Write(umParametro, _umParametroXml);
                
            }
            root.Add(_umParametroXml);
        }

        private Objeto[] ReadParametrosOperador(XElement root)
        {
            List<Objeto> propriedadesLidas = new List<Objeto>();
            List<XElement> nosParametros= root.Elements("parametro").ToList<XElement>();
            foreach (XElement noParams in nosParametros)
            {
                PropriedadesXML propriedadesXML = new PropriedadesXML();

                Objeto umParemtro = propriedadesXML.Read(noParams);
                propriedadesLidas.Add(umParemtro);
            }

            return propriedadesLidas.ToArray();
        }
    }


    public class ClasseXML : FuncionalidadeXml
    {
        private Classe _classe { get; set; }
     

        public ClasseXML(string fileNameXml, Classe classe):base(fileNameXml)
        {
            this._classe = classe;

        }

        public Classe Read(XElement root)
        {
            if (root == null)
                root = this.GetRootReadMode();

            string acessorDaClasse = root.Element("acessor").Value;
            string nomeDaClasse = root.Element("nome").Value;
       
            XElement rootPropriedades = root.Element("propriedades");
            XElement rootMetodos = root.Element("metodos");
            XElement rootOperadores = root.Element("operadores");

            List<Objeto> propriedadesLidas = ReadPropriedadesDaClasse(rootPropriedades);
            List<Funcao> metodosLidos = ReadMetodosDaClasse(rootMetodos);
            List<Operador> operadoresLidos = ReadOperadoresDaClasse(rootOperadores);

            Classe classe = new Classe(acessorDaClasse, nomeDaClasse, metodosLidos, new List<Operador>(), propriedadesLidas);
            return classe;

  
        }

        private List<Funcao> ReadMetodosDaClasse(XElement rootMetodos)
        {
            List<Funcao> metodosLidos = new List<Funcao>();

            List<XElement> mtdsXML = rootMetodos.Elements("metodo").ToList();
            if (mtdsXML != null)
                foreach (XElement noMetodo in mtdsXML)
                {
                    MetodosXML metodo = new MetodosXML();
                    Funcao funcaoLida = metodo.Read(rootMetodos);
                    metodosLidos.Add(funcaoLida);
                }
            return metodosLidos;
        }

        private List<Objeto> ReadPropriedadesDaClasse(XElement rootPropriedades)
        {
            List<Objeto> propriedadesDaClasseLida = new List<Objeto>();

            List<XElement> propsXML = rootPropriedades.Elements("propriedade").ToList();
            if (propsXML != null)
            {
                foreach (XElement umaPropriedadeXml in propsXML)
                {
                    PropriedadesXML propriedadesXML = new PropriedadesXML();
                    Objeto prop = propriedadesXML.Read(umaPropriedadeXml);
                    propriedadesDaClasseLida.Add(prop);
                }
            }
            return propriedadesDaClasseLida;
        }

        private void WriteOperadoresDaClasse(XElement root)
        {
            List<Operador> operadoresDaClasse = _classe.GetOperadores();
            if (operadoresDaClasse == null)
                return;

            XElement dadosDoOperador = new XElement("operadores");
            foreach (Operador umOperadorDaClasse in operadoresDaClasse)
            {
                OperadorXML operadorXML = new OperadorXML();
                operadorXML.Write(dadosDoOperador, umOperadorDaClasse);
            }
                
        }

        private List<Operador> ReadOperadoresDaClasse(XElement root)
        {
            List<Operador> operadoresDaClasse = new List<Operador>();


            List<XElement> operadoresGuardados = root.Elements("operador").ToList();
            foreach (XElement umOperadorXml in operadoresGuardados)
            {
                OperadorXML operadorXML = new OperadorXML();
                Operador operadorLido = operadorXML.Read(umOperadorXml);
                operadoresDaClasse.Add(operadorLido);
            }

            return operadoresDaClasse;
        }

        public void Write(XElement root)
        {
            if (root == null)
                root = this.GetRootWriteMode();
            List<Objeto> propriedadesDaClasse = this._classe.GetPropriedades();
            List<Funcao> metodosDaClasse = this._classe.GetMetodos();

            root.Add(new XElement("acessor", _classe.acessor));
            root.Add(new XElement("nome", _classe.nome));

            WritePropriedadesDaClasse(root);
            WriteMetodosDaClasse(root);
            WriteOperadoresDaClasse(root);
        }

        private void WriteMetodosDaClasse(XElement root)
        {
            List<Funcao> metodosDaClasse = this._classe.GetMetodos();
            if (metodosDaClasse != null)
            {
                XElement noMetodos = new XElement("metodos");
                foreach (Funcao umaFuncaoDaClasse in metodosDaClasse)
                {
                    MetodosXML metodoXML = new MetodosXML();
                    metodoXML.Write(umaFuncaoDaClasse, noMetodos);
                }
                root.Add(noMetodos);
            }
        }

        private void WritePropriedadesDaClasse(XElement root)
        {
            List<Objeto> propriedadesDaClasse = _classe.GetPropriedades();
            if (propriedadesDaClasse != null)
            {
                XElement noPropriedades = new XElement("propriedades");
                foreach (Objeto umaPropriedadeDaClasse in propriedadesDaClasse)
                {
                    PropriedadesXML propXML = new PropriedadesXML();
                    propXML.Write(umaPropriedadeDaClasse, noPropriedades);
                }
                root.Add(noPropriedades);
            }
        }
    }

} // namespace
