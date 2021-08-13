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
        /// ConstroiClasse()
        /// ObtemMetodosEPropriedadesDeClassesHerdadas(): obtém métodos e propriedades públicas ou protegidas da hierarquia de herança.
        /// ObtemUmaHeranca():obtem uma classe ou interface herdada a partir de seu nome.
        /// Desheranca(): retira da hierarquia classes ou interfaces a serem deserdadas.
        /// 



        private LinguagemOrquidea linguagem { get; set; }
        private Escopo escopo { get; set; }
        private List<string> codigo { get; set; }

        private List<string> tokensDaClasse { get; set; }

        private List<string> tokensRaw { get; set; }
        public List<string> MsgErros { get; set; }
        public Escopo escopoDaClasse { get; set; }


        public ExtratoresOO(Escopo escopo, LinguagemOrquidea lng, List<string> tokensRaw)
        {
            this.escopo = escopo;
            this.linguagem = lng;
            this.codigo = escopo.codigo;
            this.MsgErros = new List<string>();
            this.tokensRaw = tokensRaw.ToList<string>();
        } // ExtratoresOO()

        /// <summary>
        /// constroi classe dentro do código especificado no construtor da classe.
        /// </summary>
        public Classe ExtaiUmaClasse()
        {


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

            ProcessadorDeID procesador = new ProcessadorDeID(tokensDoCorpoDaClasse);
          
           

            // constroi o corpo da classe, com os tokens formadores do conteúdo da classe.
            procesador.Compile();

            this.escopoDaClasse = new Escopo(procesador.escopo);

            //**********************************************************************************************
            // obtém os métodos da classe.
            List<Funcao> metodosDaClasse = this.ExtraiMetodos(nomeDeClasseOuInterface, escopoDaClasse);

            //********************************************************************************************************
            // obtém as propriedades da classe.
            List<Objeto> propriedadesDaClasse = this.ExtraiPropriedades(nomeDeClasseOuInterface, escopoDaClasse);

            //**********************************************************************************************       


            // constroi a classe, com nome, métodos e propriedades.
            Classe umaClasse = new Classe(acessorDaClasseOuInterface, nomeDeClasseOuInterface, metodosDaClasse, null, null);

            //********************************************************************************************************
            umaClasse.tokensDaClasse = tokensTotais; // guarda os tokens da classe.
            umaClasse.escopoDaClasse = escopoDaClasse; // guarda o escopo da classe.

            List<string> interfacesHerdadas = new List<string>();

            // registra as propriedades encontradas na classe. O registro
            // foi feito depois,pois é preciso registrar a classe para obter os tipos presentes na classe currente.
            if (propriedadesDaClasse != null)
                umaClasse.GetPropriedades().AddRange(propriedadesDaClasse);

            // obtem itens de heranca, deseranca, fazendo a adição ou remoção dos itens herdados ou deserdados.
            this.ProcessamentoDeHerancaEDeseranca(tokensDoCabecalhoDaClasse, umaClasse, interfacesHerdadas);


            if (nomeDaClasse != null)
            {
                // registra a classe no repositório de classes.
                RepositorioDeClassesOO.Instance().RegistraUmaClasse(umaClasse);
                // registra a classe no escopo da classe.
                escopo.tabela.RegistraClasse(umaClasse);
            } // if
            else
            if (nomeDaInterface != null)
                // registra a interface no repositório de interfaces.
                RepositorioDeClassesOO.Instance().RegistraUmaInterface(umaClasse);

            if (tokensDoCorpoDaClasse.Count == 0)
                return umaClasse;


            // Faz a validacao de interfaces herdadas.
            if ((interfacesHerdadas != null) || (interfacesHerdadas.Count > 0))
            {
                for (int i = 0; i < interfacesHerdadas.Count; i++)
                {
                    if (RepositorioDeClassesOO.Instance().ObtemUmaInterface(interfacesHerdadas[i]) != null)
                    {
                        bool valida = this.ValidaInterface(umaClasse, RepositorioDeClassesOO.Instance().ObtemUmaInterface(interfacesHerdadas[i]));
                        if (!valida)
                        {
                            PosicaoECodigo posicao = new PosicaoECodigo(tokensDaClasse, escopo.codigo);
                            this.MsgErros.Add("Interface:" + interfacesHerdadas[i] + " nao implementada completamente. linha: " + posicao.linha + " coluna: " + posicao.coluna);
                        } // if

                    }
                }
            }
            if (umaClasse != null)
                // recompoe os tokens consumidos pela construção da classe.
                umaClasse.tokensDaClasse = tokensTotais.ToList<string>();

            return umaClasse;
        }  // ConstroiClasses() 



        /// <summary>
        /// extrai nomes de heranca e deseranca.
        /// </summary>
        /// <param name="code">codigo mesmo, não tokens.</param>
        private void ProcessamentoDeHerancaEDeseranca(
              List<string> tokens, Classe classeHerdeira, List<string> txt_interfacesHerdadas)
        {
         

            List<string> classesHerdadas = new List<string>();
            List<string> classesDeserdadas = new List<string>();

            List<string> interfacesHerdadas = new List<string>();
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
                    classesHerdadas.Add(tokens[posicaoTokenHeranca]);
                        
                if (EhInterface(tokens[posicaoTokenHeranca]))
                    interfacesHerdadas.Add(tokens[posicaoTokenHeranca]);

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

            txt_interfacesHerdadas = interfacesHerdadas.ToList<string>();

            // obtem os metodos, propriedades e operadores a serem adicionados ou removidos na classe herdeira.
            AdicionarItensDeClassesHerdadas(classeHerdeira, classesHerdadas);
            AdicionarItensDeClassesHerdadas(classeHerdeira, interfacesHerdadas);

            RemoveItensDeClassesDeserdadas(classeHerdeira, classesDeserdadas);
            RemoveItensDeClassesDeserdadas(classeHerdeira, interfacesDeserdadas);
        }  // ExtraiClassesHerdeirasEInterfaces()


       

     
        /// <summary>
        ///  adiciona metodos, propriedades, e operadores vindos das classes que herda.
        /// </summary>
        private  void AdicionarItensDeClassesHerdadas(Classe classeHerdeira, List<string> txt_nomesHeranca)
        {
           
            // adciona os metodos e propriedades herdados.
            for (int index = 0; index < txt_nomesHeranca.Count; index++)
            {
                Classe classeHerdada = RepositorioDeClassesOO.Instance().ObtemUmaClasse(txt_nomesHeranca[index]);
                if (classeHerdada != null)
                {

                    for (int m = 0; m < classeHerdada.GetMetodos().Count; m++)
                        // adiciona metodos das classes herdadeas, para a classe herdeira, se são publico ou protegido.
                        if ((classeHerdada.GetMetodos()[m].acessor == "public") ||
                            (classeHerdada.GetMetodos()[m].acessor == "protected"))
                        {
                            classeHerdeira.GetMetodos().Add(classeHerdada.GetMetodos()[m]);
                            int indexMetodo = classeHerdeira.GetMetodos().Count - 1;

                            // adiciona o nome da classe ao nome do método, evitando problemas como o losango mortal de classes com multiplas herancas.
                            classeHerdeira.GetMetodos()[indexMetodo].nomeClasseDoOperador = classeHerdada.GetNome();
                            if (classeHerdeira.GetMetodos().Find(k => k.nome.Equals(classeHerdada.GetMetodos()[indexMetodo].nome)) != null)
                            {
                                this.MsgErros.Add("Aviso: metodo: " + classeHerdeira.GetMetodos()[indexMetodo].nome + " ja foi definido em outras classes herdadas. Utilize o nome longo do metodo (nomeDaClasse.nomeDoMetodo) para acessa-lo");
                                classeHerdeira.GetMetodos()[indexMetodo].SetNomeLongo(); // adiciona o nome da classe herdada ao nome do metodo, para impedir o problema do losango mortal.
                            }
                        } // if

                    for (int p = 0; p < classeHerdada.GetPropriedades().Count; p++)
                        // adiciona propriedades das classes herdadas, para a classe herdeira, para a classe herdeira, se são publico ou protegido.
                        if ((classeHerdada.GetPropriedades()[p].GetAcessor() == "public") || (classeHerdada.GetPropriedades()[p].GetAcessor() == "protected")) 
                        {
                            classeHerdeira.GetPropriedades().Add(classeHerdada.GetPropriedades()[p]);

                            int indexPropriedade = classeHerdeira.GetPropriedades().Count - 1;
                            string nomeDaPropriedade = classeHerdada.GetPropriedades()[p].GetNome();
                            if (classeHerdeira.GetPropriedade(nomeDaPropriedade) != null) 
                            {
                                this.MsgErros.Add("Aviso: propriedade: " + nomeDaPropriedade + " ja foi definida em outras classes herdadas. Utilize o nome longo da propriedade (nomeDaClasse.nomeDaPropriedade) para acessa-la.");
                                // o caso em que a classe herdeira ja tem uma propriedade de mesmo nome da classe herdada. ajusta os nomes com o nome longo.
                                classeHerdeira.GetPropriedade(nomeDaPropriedade).SetNomeLongo();
                                classeHerdada.GetPropriedade(nomeDaPropriedade).SetNomeLongo();// adiciona o nome da classe ao nome da propriedade, evitando problemas como losango mortal de classes com multiplas herancas.
          
                            }
                        } // if


                    for (int op = 0; op < classeHerdada.GetOperadores().Count; op++)
                        // adiciona operadores das classes herdadas, para a classe herdeira.
                        classeHerdeira.GetOperadores().AddRange(classeHerdada.GetOperadores());

                } // if
            } // for x

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
            } // for x
        }

        /// <summary>
        /// valida o nome de uma classe, se existe no repositório de classes.
        /// </summary>
        private bool EhClasse(string nomeClasse)
        {

            return (RepositorioDeClassesOO.Instance().classesRegistradas.FindIndex(k => k.nome == nomeClasse) != -1);
        } // EhClasseHerdada()

        /// <summary>
        /// valida o nome de interface, se existe no repositório de interfaces.
        /// </summary>
        private bool EhInterface(string nomeInterface)
        {
            // trata do caso em que há classes no repositório.
            return (RepositorioDeClassesOO.Instance().interfacesRegistradas.FindIndex(k=>k.nome==nomeInterface)!=-1);

        } // EhClasseHerdada()

        /// <summary>
        /// Verifica se uma interface foi implementada na classe herdeira.
        /// </summary>
        private bool ValidaInterface(Classe _classe, Classe _interface)
        {
          

            if ((_interface.GetMetodos() == null) || (_interface.GetMetodos().Count == 0))
                return true;

            bool isImplementada = true;
            for (int x = 0; x < _interface.GetMetodos().Count; x++)
            {

                int index = _classe.GetMetodos().FindIndex(k => k.nome == _interface.GetMetodos()[x].nome);
                if (index == -1)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(tokensDaClasse, escopo.codigo);
                    this.MsgErros.Add("metodo: " + _interface.GetMetodos()[x].nome + "  da interface: " + _interface.nome + " nao implementado. linha: " + posicao.linha + ", coluna: " + posicao.coluna);
                    isImplementada = false;
                    // continua a malha, para ver se há mais métodos não implementados.
                }

            } // for k


            if ((_interface.GetPropriedades() == null) || (_interface.GetPropriedades().Count == 0))
                return true;

            for (int x = 0; x < _interface.GetPropriedades().Count; x++)
            {

                int index = _interface.GetPropriedades().FindIndex(p => p.GetNome() == _interface.GetNome());
                if (index == -1)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(tokensDaClasse, escopo.codigo);
                    this.MsgErros.Add("propriedade: " + _interface.GetMetodos()[x].nome + "  da interface: " + _interface.nome + " nao implementada. linha: " + posicao.linha + ", coluna: " + posicao.coluna);
                    isImplementada = false;
                    // continua a malha, para veer se há mais propriedades não implementadas.
                }
            }  /// for k
            return isImplementada;
        } // ValidaInterface()


        /// extrai métodos a partir de codigo da clase.
        private List<Funcao> ExtraiMetodos(string nomeClasse, Escopo escopo)
        {

            List<Funcao> metodosDaClasse = new List<Funcao>();

            if ((escopo.tabela.GetFuncoes() != null) &&
                (escopo.tabela.GetFuncoes().Count > 0)) // é um escopo folha porque a própria definição de classe tem um bloco, que delimita um escopo da classe.
            {

                foreach (Funcao classCaller in escopo.tabela.GetFuncoes()) // adiciona as funções-métodos encontrados no procesamento do escopo.
                {

                    int indexFuncaoDefinidaAnteriormente = metodosDaClasse.FindIndex(k => k.nome.Equals(classCaller.nome));
                    if (indexFuncaoDefinidaAnteriormente != -1)
                    {
                        classCaller.SetNomeLongo(); // muda o nome da funcao para o nome-longo, pois há conflito entre nomes de funções em classes herdadas.
                        metodosDaClasse[indexFuncaoDefinidaAnteriormente].SetNomeLongo();
                        metodosDaClasse.Add(classCaller);
                    }
                    else
                    {
                        metodosDaClasse.Add(classCaller);
                    }

                    classCaller.caller = nomeClasse;
                } // foreach

            } // if
            if ((escopo.tabela.GetClasse(nomeClasse, escopo) != null) && (escopo.tabela.GetClasse(nomeClasse, escopo).GetOperadores() != null))
                foreach (Operador umOperador in escopo.tabela.GetClasse(nomeClasse, escopo).GetOperadores())
                    metodosDaClasse.Add(umOperador);

            return metodosDaClasse;
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
        private List<Objeto> ExtraiPropriedades(string nomeClasse, Escopo escopo)
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


                    int  indexPropriedadeJaDefinidaEmOutraClasseHerdada= propriedadesDaClasse.FindIndex(k => k.GetNome().Equals(nome));
                    if (indexPropriedadeJaDefinidaEmOutraClasseHerdada != -1)
                    {
                        // muda o nome das variaveis para o seu nome longo, pois há conflito em nomes de variaveis de diferentes classes herdadas.
                        propriedadesDaClasse[indexPropriedadeJaDefinidaEmOutraClasseHerdada].SetNomeLongo();
                        propriedadesDaClasse.Add(new Objeto(umObjeto.GetAcessor(), umObjeto.GetTipo(), umObjeto.GetNome(), umObjeto.GetValor()));

                    }
                    else
                    {
                        // inicializa uma propriedade, com tipo obtido no repositório de classes.
                        propriedadesDaClasse.Add(new Objeto(umObjeto.GetAcessor(), umObjeto.GetTipo(), umObjeto.GetNome(),  umObjeto.GetValor()));
                    }
                } // foreach
                return propriedadesDaClasse;
            } //if
            return null;
        } // ExtraiPropriedades()


        /// <summary>
        /// obtém o corpo da classe ou interface, a partir de uma lista de codigo, não tokens.
        /// </summary>
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
