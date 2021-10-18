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



            if (this.nomeClasse == "classeHerdeira")
            {
                int k = 0;
                k++;
            }


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
            processador.CompileEmDoisEstagios(); // compila o corpo da classe, obtendo propriedades, metodos e operadores da classe.

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
            //**********************************************************************************************
            // obtém os operadores da classe.
            this.ExtraiOperadores(umaClasse, escopoDaClasse);
            //***********************************************************************************************************

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

            if (this.nomeClasse == "classeHerdeira")
            {
                int k = 0;
                k++;
            }

            // verifica se há conflitos de nomes de metodos, propriedaddes, e operadores, que tem o mesmo nome, mas vêem de classes herdadas diferentes.
            this.VerificaConflitoDeNomesEmPropriedadesEMetodosEOperadores(umaClasse);


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
                    Classe classeHerdada = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseHerdada);
                    if (classeHerdada != null)
                        classeHerdeira.classesHerdadas.Add(classeHerdada);
                    else
                        MsgErros.Add("classe: " + nomeClasseHerdada + " inexistente. verifique a sintaxe do nome da classe.");
                }

                if (EhInterface(tokens[posicaoTokenHeranca]))
                {
                    Classe classeInterface = RepositorioDeClassesOO.Instance().GetInterface(tokens[posicaoTokenHeranca]);
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
        private void VerificaConflitoDeNomesEmPropriedadesEMetodosEOperadores(Classe classeHerdeira)
        {
            if ((classeHerdeira.classesHerdadas == null) || (classeHerdeira.classesHerdadas.Count == 0))
                return;
            if ((classeHerdeira.GetPropriedades() != null) && (classeHerdeira.classesHerdadas!=null))
            {

                if ((classeHerdeira.GetPropriedades() != null) && (classeHerdeira.GetPropriedades().Count > 0))
                {
                    List<Objeto> todasPropriedadesHerdadas = new List<Objeto>();
                    for (int x = 0; x < classeHerdeira.classesHerdadas.Count; x++)
                    {
                        List<Objeto> propriedadesDeUmaClasseHerdada = classeHerdeira.classesHerdadas[x].GetPropriedades();
                        if ((propriedadesDeUmaClasseHerdada != null) && (propriedadesDeUmaClasseHerdada.Count > 0))
                            todasPropriedadesHerdadas.AddRange(propriedadesDeUmaClasseHerdada);
                    }

                    for (int x = 0; x < todasPropriedadesHerdadas.Count; x++)
                        for (int y = 0; y < todasPropriedadesHerdadas.Count; y++)
                            if ((x != y) && (todasPropriedadesHerdadas[x].GetNome() == todasPropriedadesHerdadas[y].GetNome()))
                            {

                                // informa ao programador que as propriedades herdadas de nomes iguais, foram setadas para nomelongo: nomeClasse+nomePropriedade, para evitar conflitos de nomes.
                                string avisoNomeLongo = "Aviso: propriedades: " + todasPropriedadesHerdadas[x].GetNome() + " de classes herdadas:" +
                               " possuem nomes iguais. Fazendo nome longo para as propriedades, para evitar conflitos de chamada destas propriedades, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomePropriedade) para acessar estas propriedades.";
                                UtilTokens.WriteAErrorMensage(escopo, avisoNomeLongo, escopo.codigo);

                                classeHerdeira.GetPropriedades().Remove(todasPropriedadesHerdadas[x]);
                                classeHerdeira.GetPropriedades().Remove(todasPropriedadesHerdadas[y]);

                                todasPropriedadesHerdadas[x].SetNomeLongo(todasPropriedadesHerdadas[x].GetTipo());
                                todasPropriedadesHerdadas[y].SetNomeLongo(todasPropriedadesHerdadas[y].GetTipo());

                                classeHerdeira.GetPropriedades().Add(todasPropriedadesHerdadas[x]);
                                classeHerdeira.GetPropriedades().Add(todasPropriedadesHerdadas[y]);

                            
                            }
                }

            }
            if ((classeHerdeira.GetMetodos() != null) && (classeHerdeira.classesHerdadas != null)) 
            {

                List<Funcao> todosMetodoHerdados = new List<Funcao>();
                for (int x = 0; x < classeHerdeira.classesHerdadas.Count; x++)
                {
                    List<Funcao> metodosDeUmaClasseHerdada = classeHerdeira.classesHerdadas[x].GetMetodos();
                    if ((metodosDeUmaClasseHerdada != null) && (metodosDeUmaClasseHerdada.Count > 0))
                    {
                        for (int m = 0; m < metodosDeUmaClasseHerdada.Count; m++)
                        {
                            metodosDeUmaClasseHerdada[m].nomeClasse = classeHerdeira.classesHerdadas[x].GetNome();
                            todosMetodoHerdados.Add(metodosDeUmaClasseHerdada[m]);
                        }
                    }
                }

                for (int x = 0; x < todosMetodoHerdados.Count; x++)
                    for (int y = 0; y < todosMetodoHerdados.Count; y++) 
                        if ((x!=y) &&  (Funcao.IguaisFuncoes(todosMetodoHerdados[x], todosMetodoHerdados[y])))
                        {
                            string avisoNomeLongo = "Aviso: operador: " + todosMetodoHerdados[x].nome + " de classes herdadas:" +
                                                   " possuem nomes iguais. Fazendo nome longo para estes metodos, para evitar conflitos de chamada destes metodos, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomeMetodo) para acessar estes metodos.";

                            UtilTokens.WriteAErrorMensage(escopo, avisoNomeLongo, escopo.codigo);

                            classeHerdeira.GetMetodos().Remove(todosMetodoHerdados[x]);
                            classeHerdeira.GetMetodos().Remove(todosMetodoHerdados[y]);

                            todosMetodoHerdados[x].SetNomeLongo(todosMetodoHerdados[x].nomeClasse);
                            todosMetodoHerdados[y].SetNomeLongo(todosMetodoHerdados[y].nomeClasse);

                            classeHerdeira.GetMetodos().Add(todosMetodoHerdados[x]);
                            classeHerdeira.GetMetodos().Add(todosMetodoHerdados[y]);


                        }
            }

            if (classeHerdeira.GetOperadores() != null)
            {

                List<Operador> todosOperadoresHerdados = new List<Operador>();

                for (int x = 0; x < classeHerdeira.classesHerdadas.Count; x++)
                {
                    List<Operador> operadoresDeUmaClasseHerdada = classeHerdeira.classesHerdadas[x].GetOperadores();
                    if ((operadoresDeUmaClasseHerdada != null) && (operadoresDeUmaClasseHerdada.Count > 0))
                    {
                        for (int op = 0; op < operadoresDeUmaClasseHerdada.Count; op++)
                        {
                            operadoresDeUmaClasseHerdada[op].nomeClasse = classeHerdeira.classesHerdadas[x].GetNome();
                            todosOperadoresHerdados.Add(operadoresDeUmaClasseHerdada[op]);
                        }
                    }
                }

                for (int x = 0; x < todosOperadoresHerdados.Count; x++)
                    for (int y = 0; y < todosOperadoresHerdados.Count; y++)
                        if ((x != y) && (Operador.IguaisOperadores(todosOperadoresHerdados[x], todosOperadoresHerdados[y])))
                        {
                            string avisoNomeLongo = "Aviso: metodos: " + todosOperadoresHerdados[y].nome + " de classes herdadas:" +
                                                            " possuem nomes iguais. Fazendo nome longo para estes metodos, para evitar conflitos de chamada destes metodos, pela classe herdeira. Utilize o nome longo ( nomeClasse+nomeMetodo) para acessar estes metodos.";

                            UtilTokens.WriteAErrorMensage(escopo, avisoNomeLongo, escopo.codigo);


                            classeHerdeira.GetOperadores().Remove(todosOperadoresHerdados[x]);
                            classeHerdeira.GetOperadores().Remove(todosOperadoresHerdados[y]);


                            todosOperadoresHerdados[x].SetNomeLongo(todosOperadoresHerdados[x].nomeClasse);
                            todosOperadoresHerdados[x].SetNomeLongo(todosOperadoresHerdados[y].nomeClasse);


                            
                            classeHerdeira.GetOperadores().Add(todosOperadoresHerdados[x]);
                            classeHerdeira.GetOperadores().Add(todosOperadoresHerdados[y]);

                        }

            }

        }
        private static void RemoveItensDeClassesDeserdadas(Classe classeHerdeira, List<string> txt_nomesDeseranca)
        {

            // retira os metodos e propriedades deserherdados.
            for (int indexClass = 0; indexClass < txt_nomesDeseranca.Count; indexClass++)
            {
                Classe umaClasseDeserdada = RepositorioDeClassesOO.Instance().GetClasse(txt_nomesDeseranca[indexClass]);
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

            return (RepositorioDeClassesOO.Instance().GetClasse(nomeClasse) != null);
        } // EhClasseHerdada()

        /// valida o nome de interface, se existe no repositório de interfaces.
        private bool EhInterface(string nomeInterface)
        {
            // trata do caso em que há classes no repositório.
            return (RepositorioDeClassesOO.Instance().GetInterface(nomeInterface) != null);

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
                // obtem metodos herdados, mas só se o metodo herdado nao for um metodo herdeiro
                if ((metodosHerdados != null) && (metodosHerdados.Count > 0))
                {
                    for (int fnc = 0; fnc < metodosHerdados.Count; fnc++)
                        if (classeCurrente.GetMetodos().Find(k => Funcao.IguaisFuncoes(k, metodosHerdados[fnc])) == null)
                            classeCurrente.GetMetodos().Add(metodosHerdados[fnc]);
                }
            }


            

        }// ExtraiMetodos()

        private void ExtraiOperadores(Classe classeCurrente, Escopo escopo)
        {
            if (escopo.tabela.GetOperadores() != null)
                classeCurrente.GetOperadores().AddRange(escopo.tabela.GetOperadores());
           

            if (classeCurrente.classesHerdadas != null)
            {
                for (int x = 0; x < classeCurrente.classesHerdadas.Count; x++)
                {

                    List<Operador> operadoresHerdados = classeCurrente.classesHerdadas[x].GetOperadores();
                    if ((operadoresHerdados != null) && (operadoresHerdados.Count > 0))
                        classeCurrente.GetOperadores().AddRange(operadoresHerdados);

                }
            }
        }

        // calcula o escopo da classe currente.
        private Escopo ObtemEscopoDaClasse(Escopo escopo)
        {
            List<string> codigo = escopo.codigo.ToList<string>();

            List<string> tokensDaClasse = new Tokens(LinguagemOrquidea.Instance(), escopo.codigo).GetTokens();
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
                escopo.GetMsgErros().Add("classe com erro de sintaxe, linha: " + new PosicaoECodigo(tokens).linha.ToString());
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
