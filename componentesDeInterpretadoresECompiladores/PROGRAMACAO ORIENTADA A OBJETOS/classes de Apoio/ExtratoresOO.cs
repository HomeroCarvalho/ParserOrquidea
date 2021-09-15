using System;
using System.Collections.Generic;
using System.Linq;
using Util;
using parser.ProgramacaoOrentadaAObjetos;
using System.Security.Principal;
using System.Windows.Forms;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms.VisualStyles;
using System.Text;
using parser.LISP;

namespace parser
{
    public class ExtratoresOO
    {
        // constroi a estrutura de uma classe ou interface, a partir de tokens vindo da compilação.
        
        private LinguagemOrquidea linguagem { get; set; }
        private Escopo escopo { get; set; }
        private List<string> codigo { get; set; }

        private List<string> tokensDaClasse { get; set; }

        private List<string> tokensRaw { get; set; }
        public List<string> MsgErros { get; set; }
        public Escopo escopoDaClasse { get; set; }

        public string nomeClasse { get; set; }
        public ExtratoresOO(Escopo escopo, LinguagemOrquidea lng, List<string> tokensRaw)
        {
            this.escopo = escopo;
            this.linguagem = lng;
            this.codigo = escopo.codigo;
            this.MsgErros = new List<string>();
            this.tokensRaw = tokensRaw.ToList<string>();
        } // ExtratoresOO()





        /// extrai uma interface do codigo fonte texto.
        public Classe ExtraiUmaInterface()
        {
            if (tokensRaw[1] == "interface")
            {
                Classe classeInterface = ExtaiUmaClasse(Classe.tipoBluePrint.EH_INTERFACE);
                if (classeInterface != null)
                {
                    if (classeInterface.GetPropriedades().Count > 0)
                    {
                        this.MsgErros.Add("interface: " + tokensRaw[2] + "  nao pode ter propriedades!");
                        return null;
                    }
                    else
                    {
                        for (int x = 0; x < classeInterface.GetMetodos().Count; x++)

                        {
                            if (classeInterface.GetMetodos()[x].acessor != "public")
                                this.MsgErros.Add("metodo: " + classeInterface.GetMetodos()[x].nome + " precisa ser public!");

                        }
                    }
                    return classeInterface;
                } // if
                else return null;
            }

            return null;
        }



        private Classe.tipoBluePrint templateBluePrint;

        /// constroi classe dentro do código especificado no construtor da classe.
        public Classe ExtaiUmaClasse(Classe.tipoBluePrint bluePrint)
        {

            this.templateBluePrint = bluePrint;
           
            List<string> tokensTotais = new List<string>();

            // nome da classe, a ser encontrada.
            string nomeDeClasseOuInterface = null;
            string nomeDaClasse = null;
            string nomeDaInterface = null;
            string acessorDaClasseOuInterface = null;
            List<string> tokensDoCabecalhoDaClasse = null;
            List<string> tokensDoCorpoDaClasse = new List<string>();

            // obtém o código da classe, incluindo nome, cabeçalho da herança, e o corpo da classe.
            this.ExtraiCodigoDeUmaClasse(this.tokensRaw, out nomeDaClasse, out nomeDaInterface, out tokensDoCabecalhoDaClasse,out acessorDaClasseOuInterface, out tokensDoCorpoDaClasse);

            this.nomeClasse = nomeDaClasse;

            tokensTotais = tokensDoCabecalhoDaClasse.ToList<string>();
            tokensTotais.AddRange(tokensDoCorpoDaClasse.ToList<string>());
            
   
            if (nomeDaClasse != null)
                nomeDeClasseOuInterface = nomeDaClasse;
            else
                nomeDeClasseOuInterface = nomeDaInterface;

            
            if (tokensTotais == null)
                return null;


            tokensDoCorpoDaClasse.RemoveAt(0);
            tokensDoCorpoDaClasse.RemoveAt(tokensDoCorpoDaClasse.Count - 1);

            ProcessadorDeID processador = new ProcessadorDeID(tokensDoCorpoDaClasse);
            // constroi o corpo da classe, com os tokens formadores do conteúdo da classe.
            processador.Compile();


            if (escopo.tabela.GetClasse(this.nomeClasse, escopo) != null)
            {
                // sistema de correcao de codigo com erro por posicao de expressoes antes da instanciacao.
                List<Objeto> objetos = processador.escopo.tabela.GetObjetos();
                for (int x = 0; x < objetos.Count; x++)
                {
                    string token = objetos[x].GetTipo() + " " + objetos[x].GetNome();
                    if (objetos[x].GetValor() != null)
                        token += "= " + objetos[x].GetValor().ToString();
                    token += ";";

                    List<string> tokensDoObjeto = ParserUniversal.GetTokens(token);


                    for (int i = 0; i < tokensDoObjeto.Count; i++)
                        tokensDoCorpoDaClasse.Insert(i, tokensDoObjeto[i]); // garante que a propriedade seja instanciada antes de expressoes com essa propriedade.
                }

                this.escopoDaClasse = new Escopo(escopo);
                processador.CompileEscopos(this.escopoDaClasse, tokensDoCorpoDaClasse);
                
            }
            else
                this.escopoDaClasse = new Escopo(processador.escopo);


            List<Classe> interfacesHerdadas = new List<Classe>();

            // constroi a classe, com nome, métodos e propriedades.
            Classe umaClasse = new Classe(acessorDaClasseOuInterface, nomeDeClasseOuInterface, new List<Funcao>(), new List<Operador>(), new List<Objeto>());


            // a nome de classes e interfaces herdados ou deserdados.
            this.ProcessamentoDeHerancaEDeseranca(tokensDoCabecalhoDaClasse, umaClasse);

            //********************************************************************************************************
            // obtém as propriedades da classe.
            this.ExtraiPropriedades(umaClasse, escopoDaClasse);

            //**********************************************************************************************
            // obtém os métodos da classe.
            this.ExtraiMetodos(umaClasse, escopoDaClasse);


            umaClasse.construtores = new List<Funcao>();
            List<Funcao> construtoresDestaClasse = umaClasse.GetMetodos().FindAll(k => k.nome == umaClasse.GetNome());

            if ((construtoresDestaClasse != null) && (construtoresDestaClasse.Count > 0))
                umaClasse.construtores.AddRange(construtoresDestaClasse);
            else
            if (bluePrint == Classe.tipoBluePrint.EH_CLASSE)
            {
                UtilTokens.WriteAErrorMensage(escopo, "nao ha nenhum construtores codificado para esta classe!", codigo);
                return null;
            }

            
            //********************************************************************************************************
            umaClasse.tokensDaClasse = tokensTotais;
            umaClasse.escopoDaClasse = escopoDaClasse.Clone(); // guarda o escopo da classe.
           

            // verifica se há conflitos de nomes de metodos, propriedaddes, e operadores, que tem o mesmo nome, mas vêem de classes herdadas diferentes.
            this.VerificaConflitoDeMetodosHerdados(umaClasse);


            if (nomeDaClasse != null)
            {
                // registra a classe no repositório de classes.
                RepositorioDeClassesOO.Instance().RegistraUmaClasse(umaClasse);
                // registra a classe no escopo da classe.
                escopo.tabela.RegistraClasse(umaClasse);
                // recompoe os tokens consumidos pela construção da classe.
                umaClasse.tokensDaClasse = tokensTotais.ToList<string>();

            } // if
            else
            if (nomeDaInterface != null)
                // registra a interface no repositório de interfaces.
                RepositorioDeClassesOO.Instance().RegistraUmaInterface(umaClasse);

            if (tokensDoCorpoDaClasse.Count == 0)
                return umaClasse;


            // Faz a validacao de interfaces herdadas (se foram implementadas pela classe ou outra interface.
            if ((umaClasse.interfacesHerdadas != null) && (umaClasse.interfacesHerdadas.Count > 0)) 
            {
                for (int i = 0; i < umaClasse.interfacesHerdadas.Count; i++)
                {
                    bool valida = this.ValidaInterface(umaClasse, umaClasse.interfacesHerdadas[i]);
                    if (!valida)
                        this.MsgErros.Add("Interface:" + umaClasse.interfacesHerdadas[i].nome + " nao implementada completamente na classe: " + umaClasse.nome + ".");
                }
            }
           

            return umaClasse;
        }  // ConstroiClasses() 



        /// extrai nomes de heranca e deseranca.
        private void ProcessamentoDeHerancaEDeseranca(
              List<string> tokens, Classe classeHerdeira)
        {

            List<string> classesDeserdadas = new List<string>();
            List<string> interfacesDeserdadas = new List<string>();
          

            int startTokensHeranca = tokens.IndexOf("+");
            int startTokensDeseheranca = tokens.IndexOf("-");
            int endTokensHeranca = codigo.IndexOf("{");

            if ((startTokensHeranca == -1) && (startTokensDeseheranca == -1)) // é o caso da classe não tiver heranca ou deseranca, volta, pois está certo.
                return;

            
            int posicaoTokenHeranca = tokens.IndexOf("+") + 1; // posiciona o ponteiro de inteiros para o primeiro nome da classe herdada.
            while (posicaoTokenHeranca >0)
            {
                if (EhClasse(tokens[posicaoTokenHeranca]))
                {
                    string nomeClasseHerdada = tokens[posicaoTokenHeranca];
                    Classe classeHerdada = RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.nome == nomeClasseHerdada);
                    if (classeHerdada != null)
                        classeHerdeira.classesHerdadas.Add(classeHerdada);
                    else
                        MsgErros.Add("classe: " + nomeClasseHerdada + " inexistente. verifique a sintaxe do nome da classe.");
                }

                if (EhInterface(tokens[posicaoTokenHeranca]))
                {
                    Classe classeInterface = RepositorioDeClassesOO.Instance().interfacesRegistradas.Find(k => k.nome == tokens[posicaoTokenHeranca]);
                    if (classeInterface != null)
                        classeHerdeira.interfacesHerdadas.Add(classeInterface);
                    else
                        MsgErros.Add("interface: " + tokens[posicaoTokenHeranca] + "  nao existente.");
                }
                posicaoTokenHeranca = tokens.IndexOf("+", posicaoTokenHeranca + 1) + 1; // posiciona o ponteiro de inteiros para o proximo nome da classe herdada.
            } // while


            int posicaoTokenDeseranca = tokens.IndexOf("-") + 1; //posiciona o ponteiro de inteiros para o primeiro nome da classe deserdada.

            while (posicaoTokenDeseranca>0)
            {
                if (this.EhClasse(tokens[posicaoTokenDeseranca]))
                    classesDeserdadas.Add(tokens[posicaoTokenDeseranca]);

                if (this.EhInterface(tokens[posicaoTokenDeseranca]))
                    interfacesDeserdadas.Add(tokens[posicaoTokenDeseranca]);

                posicaoTokenDeseranca = tokens.IndexOf("-", posicaoTokenDeseranca) + 1; // posiciona o ponteiro de inteiros para o proximo nome de classe deserdada.
            } // while

            RemoveItensDeClassesDeserdadas(classeHerdeira, classesDeserdadas);
            RemoveItensDeClassesDeserdadas(classeHerdeira, interfacesDeserdadas);
        }  // ExtraiClassesHerdeirasEInterfaces()




        /// <summary>
        /// resolve problemas de conflito de nomes de metodos,propriedades, operadores, que vem de classes herdadas diferentes, mas que possuem nome igual.
        /// Os metodo, propriedades, e operaores de todas classes herdadas ja foram adicionadas a classe herdeira (classe currente que está sendo construida.
        /// </summary>
        private void VerificaConflitoDeMetodosHerdados(Classe currente)
        {

            if (currente.GetPropriedades() != null)
            {
                for (int x = 0; x < currente.GetPropriedades().Count; x++)
                {
                    List<Objeto> propriedadesNomesIguais = currente.GetPropriedades().FindAll(k => k.GetNome() == currente.GetPropriedades()[x].GetNome());
                    if (propriedadesNomesIguais.Count > 1) 
                    {


                        string avisoNomeLongo = "Aviso: propriedades: " + propriedadesNomesIguais[0].GetNome() + " de classes herdadas:" +
                       " possuem nomes iguais. Fazendo nome longo para as propriedades, para evitar conflitos de chamada destas propriedades, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomePropriedade) para acessar estas propriedades.";
                        UtilTokens.WriteAErrorMensage(escopo, avisoNomeLongo, this.tokensDaClasse);


                        string nomePropriedade = currente.GetPropriedades()[x].GetNome();
                        currente.GetPropriedades().RemoveAll(k => k.GetNome() == nomePropriedade); // remove as propriedades em conflito, pois possuem o mesmo nome e nao se pode codificar com uma propriedade com dois nomes iguais.
                        for (int c = 0; c < currente.classesHerdadas.Count; c++)
                        {
                            Objeto obj1 = currente.classesHerdadas[c].GetPropriedades().Find(k => k.GetNome() == nomePropriedade);
                            if (obj1 != null) 
                            {


                                // cria uma nova propriedade com nome longo, e adiciona a classe herdeira, que teve as propriedades em conflito retiradas, justamente por nao ser possivel distinguir com nomes iguais.
                                Objeto propriedadeComNomeLongo = new Objeto(obj1);
                                propriedadeComNomeLongo.SetNomeLongo(currente.classesHerdadas[c].GetNome());

                                currente.GetPropriedades().Add(propriedadeComNomeLongo);
                            }
                        }

                    }
                }
            }
            if (currente.GetMetodos() != null)
            {
                // obtem os metodos em conflito, remove da classe herdeira, obtem os metodos em conflito nas classes herdeiras, seta para nome longo, e adiciona a classe herdeira.

                for (int x = 0; x < currente.GetMetodos().Count; x++)
                {
                    List<Funcao> metodosNomesIguais = currente.GetMetodos().FindAll(k => k.nome == currente.GetMetodos()[x].nome);
                    if (metodosNomesIguais.Count > 1)
                    {

                        string avisoNomeLongo = "Aviso: metodos: " + metodosNomesIguais[0].nome + " de classes herdadas:" +
                       " possuem nomes iguais. Fazendo nome longo para estes metodos, para evitar conflitos de chamada destes metodos, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomeMetodo) para acessar estes metodos.";
                        escopo.GetMsgErros().Add(avisoNomeLongo);

                        string nomeMetodoNomeIgual = metodosNomesIguais[0].nome;
                      
                        currente.GetMetodos().RemoveAll(k => k.nome == nomeMetodoNomeIgual);
                        for (int c = 0; c < currente.classesHerdadas.Count; c++)
                        {
                            List<Funcao> metodosEmConflito = currente.classesHerdadas[c].GetMetodos().FindAll(k => k.nome == nomeMetodoNomeIgual).ToList<Funcao>();
                            for (int m = 0; m < metodosEmConflito.Count; m++)
                            {
                                Funcao metodoComNomeLongo = metodosEmConflito[m].Clone();
                                metodoComNomeLongo.SetNomeLongo(currente.classesHerdadas[c].GetNome());
                                currente.GetMetodos().Add(metodoComNomeLongo);
                            }
                        }
                    }
                }
            }

            if (currente.GetOperadores() != null)
            {
                for (int x = 0; x < currente.GetOperadores().Count; x++)
                {
                    List<Operador> operadoresNomesIguais = currente.GetOperadores().FindAll(k => k.nome == currente.GetOperadores()[x].nome);
                    if (operadoresNomesIguais.Count > 1)
                    {
                        string avisoNomeLongo = "Aviso: operadores: " + operadoresNomesIguais[0].nome + " de classes herdadas:" +
                      " possuem nomes iguais. Fazendo nome longo para estes operadores, para evitar conflitos de chamada destes operadores, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomeOperador) para acessar estes operadores.";
                        
                        UtilTokens.WriteAErrorMensage(escopo, avisoNomeLongo, this.tokensDaClasse);

                        string nomeOperadorIgual = operadoresNomesIguais[x].nome;

                        currente.GetOperadores().RemoveAll(k => k.nome == nomeOperadorIgual);
                        for (int c = 0; c < currente.classesHerdadas.Count; c++)
                        {
                            Operador operadorEmConflito = currente.classesHerdadas[c].GetOperadores().Find(k => k.nome == nomeOperadorIgual);
                            if (operadorEmConflito != null)
                            {
                                Operador operador = operadorEmConflito.Clone();
                                operador.prioridade = operadorEmConflito.prioridade;
                                operador.SetNomeLongo(currente.classesHerdadas[c].nome);
                                currente.GetOperadores().Add(operador);
                            }
                        }
                    }
                }
            }

  
        }

        private static void RemoveItensDeClassesDeserdadas(Classe classeHerdeira, List<string> txt_nomesDeseranca)
        {

            // retira os metodos e propriedades deserherdados.
            for (int indexClass = 0; indexClass < txt_nomesDeseranca.Count; indexClass++)
            {
                Classe umaClasseDeserdada = RepositorioDeClassesOO.Instance().ObtemUmaClasse(txt_nomesDeseranca[indexClass]);
                if (umaClasseDeserdada != null)
                {

                    for (int m = 0; m < umaClasseDeserdada.GetMetodos().Count; m++)
                    {
                        // remove metodos que são publicos ou protegidos, com o mesmo nome de metodo da classe herdeira. (a classe que recebe a herança).
                        if ((umaClasseDeserdada.GetMetodos()[m].acessor == "public") || (umaClasseDeserdada.GetMetodos()[m].acessor == "protected"))
                        {
                            int indexRemocao = classeHerdeira.GetMetodos().FindIndex(k => k.nome == umaClasseDeserdada.GetNome());
                            if (indexRemocao != -1)
                                classeHerdeira.GetMetodos().RemoveAt(indexRemocao);
                        } // if

                    }
                    for (int p = 0; p < umaClasseDeserdada.GetPropriedades().Count; p++)
                        // remove propriedades publicas ou protegidas, com o mesmo nome de propriedade da classe herdeira (a classe que receber a herança).
                        if ((umaClasseDeserdada.GetPropriedades()[p].GetAcessor() == "public") ||
                            (umaClasseDeserdada.GetPropriedades()[p].GetAcessor() == "private")) 
                        {
                            int indexRemocao = classeHerdeira.GetPropriedades().FindIndex(k => k.GetNome() == umaClasseDeserdada.GetPropriedades()[p].GetNome());
                            if (indexRemocao == -1)
                                classeHerdeira.GetPropriedades().RemoveAt(indexRemocao);
                        }  // if

                    for (int op = 0; op < umaClasseDeserdada.GetOperadores().Count; op++)
                    {
                        // remove operadores com o mesmo nome de operador da classe herdeira.
                        int indexRemocao = classeHerdeira.GetOperadores().FindIndex(k => k.nome == umaClasseDeserdada.GetOperadores()[op].nome);
                        if (indexRemocao != -1)
                            classeHerdeira.GetOperadores().RemoveAt(indexRemocao);
                    }

                } // if

                classeHerdeira.classesHerdadas.Remove(umaClasseDeserdada);
            } // for x

           
        }

        /// <summary>
        /// valida o nome de uma classe, se existe no repositório de classes.
        /// </summary>
        private bool EhClasse(string nomeClasse)
        {

            return (RepositorioDeClassesOO.Instance().classesRegistradas.FindIndex(k => k.nome == nomeClasse) != -1);
        } // EhClasseHerdada()

        /// valida o nome de interface, se existe no repositório de interfaces.
        private bool EhInterface(string nomeInterface)
        {
            // trata do caso em que há classes no repositório.
            return (RepositorioDeClassesOO.Instance().interfacesRegistradas.FindIndex(k=>k.nome==nomeInterface)!=-1);

        } // EhClasseHerdada()


        /// Verifica se uma interface foi implementada na classe herdeira.
        private bool ValidaInterface(Classe _classe, Classe _interface)
        {
            if ((_interface.GetMetodos() == null) || (_interface.GetMetodos().Count == 0))
                return true;

            for (int x = 0; x < _interface.GetMetodos().Count; x++)
            {

                int index = _classe.GetMetodos().FindIndex(k => k.nome == _interface.GetMetodos()[x].nome);
                if (index == -1)
                {
                    this.MsgErros.Add("metodo: " + _interface.GetMetodos()[x].nome + "  da interface: " + _interface.nome + " nao implementado.");
                    return false;
                }
            } // for x


            if ((_interface.GetPropriedades() == null) || (_interface.GetPropriedades().Count == 0))
                return true;

            return true;
        } // ValidaInterface()


        /// extrai métodos e operadores a partir de codigo da clase.
        private void ExtraiMetodos(Classe classeCurrente, Escopo escopo)
        {



            if ((escopo.tabela.GetFuncoes() != null) &&
                (escopo.tabela.GetFuncoes().Count > 0)) // é um escopo folha porque a própria definição de classe tem um bloco, que delimita um escopo da classe.
                classeCurrente.GetMetodos().AddRange(escopo.tabela.GetFuncoes());


            if ((escopo.tabela.GetOperadores() != null) && (escopo.tabela.GetOperadores().Count > 0))
                classeCurrente.GetOperadores().AddRange(escopo.tabela.GetOperadores());

            for (int x = 0; x < classeCurrente.classesHerdadas.Count; x++)
            {
                List<Funcao> metodosHerdados = classeCurrente.classesHerdadas[x].GetMetodos().FindAll(k => k.acessor == "public" || k.acessor == "protected");
                if ((metodosHerdados != null) && (metodosHerdados.Count > 0))
                    classeCurrente.GetMetodos().AddRange(metodosHerdados);

                List<Operador> operadoresHerdados = classeCurrente.classesHerdadas[x].GetOperadores();
                if ((operadoresHerdados != null) && (operadoresHerdados.Count > 0))
                    classeCurrente.GetOperadores().AddRange(operadoresHerdados);
            }



    }// ExtraiMetodos()



        // calcula o escopo da classe currente.
        private Escopo ObtemEscopoDaClasse(Escopo escopo)
        {
            List<string> codigo = escopo.codigo.ToList<string>();

            List<string> tokensDaClasse = new Tokens(new LinguagemOrquidea(), escopo.codigo).GetTokens();
            int k = 0;
            while ((k < tokensDaClasse.Count) && (tokensDaClasse[k] != "{"))
                k++;

            if ((k < tokensDaClasse.Count) && (tokensDaClasse[k] == "{"))
            {
                // o escopo é de um bloco, retira os operadores bloco antes de continuar.
                tokensDaClasse.RemoveRange(0, k + 1);
                tokensDaClasse.RemoveAt(tokensDaClasse.Count - 1);
            } // if
            // constroi o escopo da classe, com o trecho de código da classe.
            escopo = new Escopo(tokensDaClasse);
            return escopo;
        }

        /// Extrai propriedades (campos) a partir do código da classe.
        private void ExtraiPropriedades(Classe classeCurrente, Escopo escopo)
        {

            // obtém uma lista de variáveis declaradas na construção do escopo da classe.
            List<Objeto> objetosCampos = escopo.tabela.GetObjetos();

            // se a lista de variaveis não for vazia, registra as variáveis da classe, a partir do escopo principal.
            if ((objetosCampos != null) && (objetosCampos.Count > 0))
            {

                List<Objeto> propriedadesDaClasse = new List<Objeto>();

                // faz uma varredura em busca de variáveis, que também tem acessor: public, private, ou protected.
                foreach (Objeto umObjeto in objetosCampos)
                {


                    string acessor = umObjeto.GetAcessor();

                    string nome = umObjeto.GetNome();


                    if (umObjeto.isStatic) // a variável, propriedade é estática, modifica o nome.
                        umObjeto.SetNome("static." + umObjeto.GetNome());


                    // inicializa uma propriedade, com tipo obtido no repositório de classes.
                    classeCurrente.GetPropriedades().Add(new Objeto(umObjeto.GetAcessor(), umObjeto.GetTipo(), umObjeto.GetNome(), umObjeto.GetValor()));

                } // foreach

                for (int x = 0; x < classeCurrente.classesHerdadas.Count; x++)
                {
                    List<Objeto> propriedadesHerdadas = classeCurrente.classesHerdadas[x].GetPropriedades().FindAll(k => k.GetAcessor() == "public" || (k.GetAcessor() == "protected"));
                    classeCurrente.GetPropriedades().AddRange(propriedadesHerdadas);
                }

               
            } //if
        
        } // ExtraiPropriedades()


      
        /// obtém o corpo da classe ou interface, a partir de uma lista de codigo, não tokens.
        private void ExtraiCodigoDeUmaClasse(List<string> codigo, out string nomeDaClasse, out string nomeDaInterface, out List<string> tokensCabecalhoDaClasse,out string acessor, out List<string> tokensCorpoDaClasse)
        {
            

            nomeDaClasse = null; 
            nomeDaInterface = null;

            List<string> tokens = new Tokens(this.linguagem, codigo).GetTokens();


            List<string> acessorVAlidos = new List<string>() { "public", "private", "protected" };
            acessor = acessorVAlidos.Find(k => k.Equals(tokens[0]));
            if (acessor == null)
                acessor = "protected";

            
            
            int indexCorpo = tokens.IndexOf("{");
            if (indexCorpo == -1)
            {
                escopo.GetMsgErros().Add("classe com erro de sintaxe, linha: " + new PosicaoECodigo(tokens, escopo.codigo).linha.ToString());
                tokensCabecalhoDaClasse = new List<string>();
                this.tokensDaClasse = new List<string>();

            }

            tokensCorpoDaClasse = UtilTokens.GetCodigoEntreOperadores(indexCorpo, "{", "}", tokens);
            tokensCabecalhoDaClasse = tokens.GetRange(0, indexCorpo);
            



            int indexNomeDaClasse = tokens.IndexOf("class");
            if (indexNomeDaClasse != -1)
            {
                nomeDaClasse = tokens[indexNomeDaClasse + 1];
                nomeDaInterface = null;

            }
            int indexNomeDaInterface = tokens.IndexOf("interface");
            if (indexNomeDaInterface != -1)
            {
                nomeDaClasse = null;
                nomeDaInterface = tokens[indexNomeDaInterface + 1];

            }

        } // ExtraiCodigoDeUmaClasse()

        /// <summary>
        /// método utilizado para gerar mensagens de erro, no processamento de classes ou interfaces.
        /// </summary>
        /// <param name="nomeDaClasse">nome de uma classe, ou...</param>
        /// <param name="nomeDaInterface">... ou nome de uma interface...</param>
        /// <param name="msgErro">lista de erros.</param>
        private void EmiteMensagemDeErro(string nomeDaClasse, string nomeDaInterface, string msgErro)
        {
            if (nomeDaClasse != null)
                this.MsgErros.Add("Erro no processamento da classe: " + nomeDaClasse + msgErro + ".");
            else
                this.MsgErros.Add("Erro no processamento da interface: " + nomeDaInterface + msgErro + ".");

        } // EmiteMensagemDeErro()
    } // ExtratoresOO
  
} // namespace
