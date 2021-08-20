using System.Collections.Generic;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class ProgramaEmVM
    {



        internal List<Instrucao> instrucoes = new List<Instrucao>();// lista todas instruções do programa, rorando dentro da VM.
        internal static Dictionary<int, HandlerInstrucao> dicHandlers = null;


        private static LinguagemOrquidea linguagem = null; // linguagem de computação utilizada.


        //private Pilha<int> pilhaDeChamadas = new Pilha<int>("pilha de chamadas de métodos"); // pilha de chamadas de métodos.


        public static List<int> codesInstructions;// lista de tags das instruções a ser executadas.


        internal static int codeWhile = 0;
        internal static int codeIfElse = 7;
        internal static int codeFor = 3;
        internal static int codeAtribution = 4;
        internal static int codeCallerFunction = 5;
        internal static int codeReturn = 2;
        internal static int codeBlock = 6;
        internal static int codeBreak = 9;
        internal static int codeContinue = 10;
        internal static int codeDefinitionFunction = 11; // a definição de função não é uma instrução, mas resultado da compilação.
        internal static int codeGetObjeto = 14;
        internal static int codeSetObjeto = 16;
        internal static int codeOperadorBinario = 17;
        internal static int codeOperadorUnario = 18;
        internal static int codeCasesOfUse = 19;
        internal static int codeCreateObject = 20;
        internal static int codeImporter = 22;

        public delegate object HandlerInstrucao(Instrucao umaInstrucao, Escopo escopo);

        public static int IP_contador = 0; // guarda o id das sequencias.
        private bool isQuit = false;

        public object resultLastInstruction;

        // inicia o programa na VM.
        public void Run(Escopo escopo)
        {
       
            IP_contador = 0; // inicia a primeira instrução do software.

            while (IP_contador < instrucoes.Count)
            {
                ExecutaUmaInstrucao(instrucoes[IP_contador], escopo);
                IP_contador++;

                if (isQuit)
                    break;
            } // while
        } // Run()

        public ProgramaEmVM(List<Instrucao> instrucoesPrograma)
        {
            if (ProgramaEmVM.linguagem == null)
                ProgramaEmVM.linguagem = new LinguagemOrquidea();

            if (ProgramaEmVM.codesInstructions == null)
                ProgramaEmVM.codesInstructions = new List<int>();

            instrucoes = instrucoesPrograma.ToList<Instrucao>();

            if (dicHandlers == null)
            {
                dicHandlers = new Dictionary<int, HandlerInstrucao>();

                // nem todas instruções são acessíveis pelo dicionario: break, e continue ficam dentro das instruções de repetição. codeBlock não precisa ter handler.
                dicHandlers[codeAtribution] = this.InstrucaoAtribuicao;
                dicHandlers[codeCallerFunction] = this.InstrucaoChamadaDeFuncao;
                dicHandlers[codeDefinitionFunction] = this.InstrucaoDefinicaoDeFuncao;
                dicHandlers[codeIfElse] = this.InstrucaoIfElse;
                dicHandlers[codeFor] = InstrucaoFor;
                dicHandlers[codeWhile] = InstrucaoWhile;
                dicHandlers[codeGetObjeto] = InstrucaoGetObjeto;
                dicHandlers[codeSetObjeto] = InstrucaoSetObjeto;
                dicHandlers[codeOperadorUnario] = InstrucaoOperadorUnario;
                dicHandlers[codeOperadorBinario] = InstrucaoOperadorBinario;
                dicHandlers[codeCasesOfUse] = InstrucaoCasesOfUse;
                dicHandlers[codeCreateObject] = InstrucaoCreateObject;
                dicHandlers[codeImporter] = InstrucaoImporter;
                dicHandlers[codeReturn] = InstrucaoReturn;
            }
        } // InstrucoesVM()


        /// executa uma instrução dentro do programa contido na VM.
        internal void ExecutaUmaInstrucao(Instrucao umaInstrucao, Escopo escopo)
        {
              this.resultLastInstruction = dicHandlers[umaInstrucao.code](umaInstrucao, escopo);
        } // ExecutaUmaInstrucao()



        /// <summary>
        /// Encontra o endereço do ponto de entrada do programa.
        /// </summary>
        /// <param name="nomeClasse">classe que contém o metodo que inicilizara a computacao.</param>
        /// <param name="nomeMetodo">nome do método que inicia o programa.</param>
        /// 
        /// <returns>retorna o endereço virtual do método a começar o programa.</returns>
        public long FindEntryPointProgram(string nomeClasse, string nomeMetodo)
        {

            Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);
            if (classe == null)
                return -1;

            Funcao funcao = classe.GetMetodos().Find(k => k.nome == nomeClasse + "." + nomeMetodo);
            if (funcao == null)
                return -1;

            if ((funcao.instrucoesFuncao == null) || (funcao.instrucoesFuncao.Count > 0))
                return -1; // não localizou a funcao dentro do escopo.
            else
                return funcao.instrucoesFuncao[0].IP_Instrucao; // retorna o ponto de entrada para o programa rodar.
        }

        /// padrão de projetos [Command], permitindo que a lista de instruções da VM possa ser extendida.
        public void AddTipoInstrucao(HandlerInstrucao novoTipoInstrucao, int code)
        {
            // para inserir um novo comando, construa a string de definicao, o metodo Build, o indice de codigo, e o metodo de execução do comando.
            dicHandlers[code] = novoTipoInstrucao;
        }


        public object InstrucaoImporter(Instrucao instrucao, Escopo escopo)
        {
            // as classes já foram importadas no compilador..
            return null;

        } // InstrucaoCreateObject()




        public object InstrucaoCreateObject(Instrucao instrucao, Escopo escopo)
        {

            /// EXPRESSOES:
            /// 0- NOME "create"
            /// 1- tipo do objeto
            /// 2- nome do objeto
            /// 3- indice do construtor.
            /// 4- indices de vetor
            /// 5 em diante- expressoes parametros.



            Expressao exprssDesempacotamento = instrucao.expressoes[0];
            if (exprssDesempacotamento.Elementos[0].ToString() != "create")
                return null;
            string tipoDoObjeto = exprssDesempacotamento.Elementos[1].ToString();
            string nomeDoObjeto = exprssDesempacotamento.Elementos[2].ToString();
            string propriedadeDoObjeto = exprssDesempacotamento.Elementos[3].ToString();
            Expressao expressoesParametros = exprssDesempacotamento.Elementos[5].Elementos[0];

            if (tipoDoObjeto == "Objeto")
            {
            
                List<object> valoresParametrosObjeto = new List<object>();
                for (int x = 0; x < expressoesParametros.Elementos.Count; x++)
                {
                    object valor = new EvalExpression().EvalPosOrdem(expressoesParametros.Elementos[x], escopo);
                    valoresParametrosObjeto.Add(valor);
                }
                Objeto objetoCriado = new Objeto();
                objetoCriado.SetNome(nomeDoObjeto);

                int indexConstructor = ProcessadorDeID.FoundACompatibleConstructor(tipoDoObjeto, expressoesParametros.Elementos);

                Classe classeDoObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoObjeto);
                object caller = classeDoObjeto.construtores[indexConstructor].caller;

                object novoValorDaPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoObjeto).construtores[indexConstructor].ExecuteAConstructor(expressoesParametros.Elementos, classeDoObjeto.GetType());

                objetoCriado.SetValorField(propriedadeDoObjeto, novoValorDaPropriedade, escopo);
                return objetoCriado;

            }
            else
            if (tipoDoObjeto == "Vetor") 
            {
                Vetor vetor = escopo.tabela.GetVetor(nomeDoObjeto, escopo);
                return vetor; // os indices de criação do vetor são imutáveis, então a instância da variável e constante e calculada no build de compilação. 
            }
            else
            {
                object caller = CalcObjectConstructed(escopo, tipoDoObjeto, expressoesParametros);// obtem a variavel, já criada no build de create, para mudar seu valor como sendo o valor retornado do construtor.
                Objeto v = escopo.tabela.GetObjeto(nomeDoObjeto, escopo);
                v.SetValor(caller, escopo);

                return caller;
            }
        } // InstrucaoCreateObject()

        private static object CalcObjectConstructed(Escopo escopo, string tipoDoObjeto, Expressao expressoesParametros)
        {
            List<object> valoresParametros = ConstroiValoresParametros(expressoesParametros, escopo);

            int indexConstructor = ProcessadorDeID.FoundACompatibleConstructor(tipoDoObjeto, expressoesParametros.Elementos);

            // pode ser inclusive uma Variavel ou VariavelVetor, pois há construtores reflexão para essas classes.
            object caller = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoObjeto).construtores[indexConstructor].InfoConstructor.Invoke(valoresParametros.ToArray());

            return caller;
        }

        private static List<object> ConstroiValoresParametros(Expressao expressoes, Escopo escopo)
        {
            List<Expressao> exprssParametros = expressoes.Elementos;

            Objeto novoObjeto = new Objeto();
            List<object> valoresParametros = new List<object>();
            for (int x = 0; x < exprssParametros.Count; x++)
            {
                object valor = new EvalExpression().EvalPosOrdem(exprssParametros[x], escopo);
                valoresParametros.Add(valor);
            }

            return valoresParametros;
        }


        //  instrução de construção do casesOfUses. como não é uma instrução feita massivamente, a construção de um objeto ProcessadorDeID não afeta o desempenho.
        public object InstrucaoCasesOfUse(Instrucao instrucao, Escopo escopo)
        {
            List<Expressao> expressoes = instrucao.expressoes;
            string nomeObjetoPrincipal = instrucao.expressoes[0].GetElemento().ToString();


            List<Expressao> exprssCodicionaisDoCase = instrucao.expressoes.GetRange(1, instrucao.expressoes.Count - 1);
     
            EvalExpression eval = new EvalExpression(); // inicializa o avaliador de expressões.

            // percorre os cases da instrução, se a expressão condicional do case for true, roda a lista de instruções do case.
            for (int x = 0; x < exprssCodicionaisDoCase.Count; x++)
            {
                bool resultCondicionalDoCase = (bool)eval.EvalPosOrdem(exprssCodicionaisDoCase[x], escopo);
                if (resultCondicionalDoCase)
                {
                    List<Instrucao> instrucoesDoCase = instrucao.blocos[x]; // obtém o bloco de instruções para o case avaliado.
                    for (int i = 0; i < instrucoesDoCase.Count; i++)
                        this.ExecutaUmaInstrucao(instrucoesDoCase[i], escopo);
                } // if
            } // for x
            return null;
        } // InstrucaoCasesOfUse()


        public object InstrucaoOperadorBinario(Instrucao instrucao, Escopo escopo)
        {
            // obtém alguns dados do novo operador binario.
            string tipoRetornoDoOperador = ((ExpressaoElemento)instrucao.expressoes[0]).GetElemento().ToString();
            string nomeOperador = ((ExpressaoElemento)instrucao.expressoes[1]).GetElemento().ToString();
            string tipoOperando1 = ((ExpressaoElemento)instrucao.expressoes[2]).GetElemento().ToString();
            string nomeOpérando1 = ((ExpressaoElemento)instrucao.expressoes[3]).GetElemento().ToString();
            string tipoOperando2 = ((ExpressaoElemento)instrucao.expressoes[4]).GetElemento().ToString();
            string nomeOperando2 = ((ExpressaoElemento)instrucao.expressoes[5]).GetElemento().ToString();
            int prioridade = int.Parse(((ExpressaoElemento)instrucao.expressoes[6]).GetElemento().ToString());
            Funcao metodoDeImplantacaoDoOperador = ((ExpressaoChamadaDeFuncao)instrucao.expressoes[7]).funcao;


            // encontra a classe em que se acrescentará o novo operador binnario.
            Classe classeDoOperador = escopo.tabela.GetClasses().Find(k => k.GetNome() == tipoRetornoDoOperador);
            if (classeDoOperador == null)
                return null;
            // consroi o novo operador binario, a partir dos dados coletados.
            Operador novoOperadorBinario = new Operador(classeDoOperador.GetNome(), nomeOperador, prioridade, new string[] { tipoOperando1, tipoOperando2 }, tipoRetornoDoOperador, metodoDeImplantacaoDoOperador, escopo);
            novoOperadorBinario.funcaoImplementadoraDoOperador = metodoDeImplantacaoDoOperador; // seta a função de cálculo do operador.

            if (novoOperadorBinario == null)
                return null;

            // adiciona o novo operador binario para a classe do tipo de retorno do operador.
            classeDoOperador.GetOperadores().Add(novoOperadorBinario);

            // atualiza a classe no repositório.
            Classe classeRepositorio = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoRetornoDoOperador);
            if (classeRepositorio != null)
                RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoRetornoDoOperador).GetOperadores().Add(novoOperadorBinario);
            return null;
        }

        // instrução de construção de operador binario. como não é uma instrução feita massivamente, a construção de um objeto ProcessadorDeID não afeta o desempenho.
        public object InstrucaoOperadorUnario(Instrucao instrucao, Escopo escopo)
        {
            // obtém alguns dados do novo operador unario.
            string tipoRetornoDoOperador = ((ExpressaoElemento)instrucao.expressoes[0]).GetElemento().ToString();
            string nomeOperador = ((ExpressaoElemento)instrucao.expressoes[1]).GetElemento().ToString();
            string tipoOperando1 = ((ExpressaoElemento)instrucao.expressoes[2]).GetElemento().ToString();
            string nomeOpérando1 = ((ExpressaoElemento)instrucao.expressoes[3]).GetElemento().ToString();
            int prioridade = int.Parse(((ExpressaoElemento)instrucao.expressoes[4]).GetElemento().ToString());
            Funcao funcaoOperador = ((ExpressaoChamadaDeFuncao)instrucao.expressoes[5]).funcao;


            // encontra a classe em que se acrescentará o novo operador unario.
            Classe classeDoOperador = escopo.tabela.GetClasses().Find(k => k.GetNome() == tipoRetornoDoOperador);
            if (classeDoOperador == null)
                return null;
            // consroi o novo operador unario, a partir dos dados coletados.
            Operador novoOperadorUnario = new Operador(classeDoOperador.GetNome(), nomeOperador, prioridade, new string[] { tipoOperando1 }, tipoRetornoDoOperador, ((ExpressaoChamadaDeFuncao)instrucao.expressoes[5]).funcao, escopo);

            novoOperadorUnario.funcaoImplementadoraDoOperador = funcaoOperador;
            if (novoOperadorUnario == null)
                return null;

            // atualiza a classe no escopo.
            classeDoOperador.GetOperadores().Add(novoOperadorUnario); // adiciona o novo operador unario para a classe do tipo de retorno do operador.


            // atualiza a classe no repositorio.
            Classe classeRepositorioOperador = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoRetornoDoOperador);
            if (classeRepositorioOperador != null)
                RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoRetornoDoOperador).GetOperadores().Add(novoOperadorUnario);
            return null;
        }


        /// obtém o valor de uma variável.
        /// a variavel está na expressão[0].
        private object InstrucaoGetObjeto(Instrucao instruvcao, Escopo escopo)
        {
            object valor = ((ExpressaoObjeto)instruvcao.expressoes[0]).objeto.GetValor();
            return valor;
        }

        // seta o valor de uma variável.
        // o valor está na expressao[1].
        private object InstrucaoSetObjeto(Instrucao instrucao, Escopo escopo)
        {
            Objeto v = ((ExpressaoObjeto)instrucao.expressoes[0]).objeto;
            v.SetValor(((ExpressaoElemento)instrucao.expressoes[1]).elemento, escopo);
            return null;
        }

        public object InstrucaoReturn(Instrucao instrucao, Escopo escopo)
        {
            if ((instrucao.expressoes == null) || (instrucao.expressoes.Count == 0))
                return null;
            EvalExpression eval = new EvalExpression();
            return eval.EvalPosOrdem(instrucao.expressoes[0], escopo);

        }

        private object InstrucaoWhile(Instrucao instrucao, Escopo escopo)
        {

            Expressao exprssControle = instrucao.expressoes[0];
            EvalExpression eval = new EvalExpression();
            while ((bool)eval.EvalPosOrdem(exprssControle, escopo))
            {
                Executa_bloco(instrucao, escopo, 0);
                exprssControle.isModdfy = true;
            }
            return null;
        } // WhileInstrucao()

        private object InstrucaoIfElse(Instrucao instrucao, Escopo escopo)
        {

            // expresssao controle: expressao[0]
            // blocos: instrucoes.Blocos. (2 para else).

            Expressao exprssControle = instrucao.expressoes[0];
            EvalExpression eval = new EvalExpression();
            if ((bool)eval.EvalPosOrdem(exprssControle, escopo))
                Executa_bloco(instrucao, escopo, 0);
            else
            {
                if (instrucao.blocos.Count > 1)  //procesamento da instrução else. O segundo bloco é para instrução else.
                Executa_bloco(instrucao, escopo, 1);
            }
            return null;
        } // IfElseInstrucao()

        private object Executa_bloco(Instrucao instrucao, Escopo escopo, int bloco)
        {
            object result = null;
            for (int umaInstrucao = 0; umaInstrucao < instrucao.blocos[bloco].Count; umaInstrucao++)
            {
                if (instrucao.blocos[bloco][umaInstrucao].code == codeBreak)
                    break;
                if (instrucao.blocos[bloco][umaInstrucao].code == codeContinue)
                    continue;
                if (instrucao.blocos[bloco][umaInstrucao].code == codeReturn)
                    return new EvalExpression().EvalPosOrdem(instrucao.blocos[bloco][umaInstrucao].expressoes[1], escopo);
                ExecutaUmaInstrucao(instrucao.blocos[bloco][umaInstrucao], escopo);
                result = new object();
            } // for bloco
            return result;
        }

        private object InstrucaoFor(Instrucao instrucao, Escopo escopo)
        {

            /// template: for (int x=0;x.controleLimite; x++)
            /// template instruções:
            ///    instrucao.expressoes[0]: expressão de atribuição da variável controle. 
            ///    instrucao.expressoes[1]; expressão de controle para a instrução.
            ///    instrucao.expressoes[2]: expressão de incremento da instrucao.
            ///    
        
            Expressao exprssAtribuicao = instrucao.expressoes[0];
            Expressao exprsCondicional = instrucao.expressoes[1];
            Expressao exprsIncremento = instrucao.expressoes[2];

            EvalExpression eval = new EvalExpression();


            int varAtribuicao = int.Parse(eval.EvalPosOrdem(exprssAtribuicao, escopo).ToString()); // faz a primeira atribuição do controle da malha.

            // calcula o limite do contador para o "for".
            Expressao exprssControle = new Expressao(new string[] { instrucao.expressoes[1].Elementos[2].ToString() }, escopo);
            int limiteMalha = int.Parse(eval.EvalPosOrdem(exprssControle, escopo).ToString());
   
            while ((bool)eval.EvalPosOrdem(exprsCondicional, escopo))  // avalia a expressão de controle.
            {
                exprsCondicional.isModdfy = true;
                if ((instrucao.blocos[0] != null) && (instrucao.blocos[0].Count > 0))
                {
                    Executa_bloco(instrucao, escopo, 0);
                    exprsIncremento.isModdfy = true;
                    varAtribuicao += (int)eval.EvalPosOrdem(exprsIncremento, escopo); // atualiza a expressão de incremento.
                    if ((int)varAtribuicao >= (int)limiteMalha)
                        break;
                }
                else
                    break;
            } // while
 
            return null;
        } // ForInstrucao()

        private object InstrucaoAtribuicao(Instrucao instrucao, Escopo escopo)
        {

            /// Estrutura de dados da chamada:
            /// 1- se for propriedade aninhada: 
            ///         2- tipo do propriedade,
            ///         3- nome da propriedade.
            if (instrucao.flags.Contains(Instrucao.EH_MODIFICACAO))
            {

                switch (instrucao.flags[1])
                {
                    case Instrucao.EH_OBJETO:

                        string tipoDoObjeto = instrucao.expressoes[0].Elementos[0].ToString();
                        string nomeDoObjeto = instrucao.expressoes[0].Elementos[1].ToString();
                        string campoDoObjeto = instrucao.expressoes[0].Elementos[2].ToString();
                        Expressao exprssAtribuicao = instrucao.expressoes[1];
                        object novoValorDoCampoDoObjeto = new EvalExpression().EvalPosOrdem(exprssAtribuicao, escopo);

                        Objeto obj1 = escopo.tabela.GetObjeto(nomeDoObjeto, escopo);
                        if (obj1 != null)
                        {
                            if (exprssAtribuicao != null)
                            {
                                if (campoDoObjeto != "")
                                    obj1.SetValorField(campoDoObjeto, novoValorDoCampoDoObjeto, escopo); // guarda o valor calculado, na propriedade do aninhamento.
                                else
                                    obj1.SetValorObjeto(novoValorDoCampoDoObjeto);

                                return obj1;
                            } // if
                            return obj1;
                        }
                        break;
                   
                    case Instrucao.EH_VETOR:
                        string nomeObjeto = instrucao.expressoes[0].Elementos[1].ToString();
                        object novoValor3 = new EvalExpression().EvalPosOrdem(instrucao.expressoes[1], escopo);

                        Vetor umVetor = escopo.tabela.GetVetor(nomeObjeto, escopo);
                        if (umVetor != null)
                        {
                            List<Expressao> expressoesIndices = instrucao.expressoes[5].Elementos;
                            List<int> indices = new List<int>();

                            if (expressoesIndices == null)
                                expressoesIndices = new List<Expressao>();
                            else
                            {
                                EvalExpression eval = new EvalExpression();
                                for (int x = 0; x < expressoesIndices.Count; x++)
                                    indices.Add(int.Parse(eval.EvalPosOrdem(expressoesIndices[x], escopo).ToString()));
                                    
                            }

                            umVetor.SetValor(novoValor3, escopo);
                            umVetor.dimensoes = indices.ToArray();

                            return umVetor;
                        }
                        return null;

                    case Instrucao.EH_PRPOPRIEDADE_ESTATICA:
                        string tipoDaPropriedadeEstatica = instrucao.expressoes[0].Elementos[0].ToString();
                        string nomeDaPropriedadeEstatica = instrucao.expressoes[0].Elementos[1].ToString();
                        Classe classe = escopo.tabela.GetClasse(tipoDaPropriedadeEstatica, escopo);
                        if (classe != null)
                        {
                            object novoValor4 = new EvalExpression().EvalPosOrdem(instrucao.expressoes[1], escopo);
                            Objeto propriedadeEstatica = classe.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaPropriedadeEstatica));
                            propriedadeEstatica.SetValor(novoValor4, escopo);
                            return propriedadeEstatica;

                        }
                        return null;


                }



            } // if MODIFICACAO
            if (instrucao.flags.Contains(Instrucao.EH_DEFINICAO))
            {
                /// na natureza, nada ser cria, nada se perde, tudo se transforma...
                /// se a variavel ja estiver sido criada, muda o seu valor, nao precisando do Garbage Collector
                /// para retirar a variavel do programa...


                string nomeDoObjeto = instrucao.expressoes[0].Elementos[1].ToString();
                string tipoDoObjeto = instrucao.expressoes[0].Elementos[0].ToString();
              

                object novoValor = new EvalExpression().EvalPosOrdem(instrucao.expressoes[1], escopo);

                int tipoDeDados = instrucao.flags[1];
                switch (tipoDeDados)
                {
                    case Instrucao.EH_OBJETO:
                        string campoDoObjeto = instrucao.expressoes[0].Elementos[2].ToString();
                        Objeto obj1 = new Objeto(tipoDoObjeto, nomeDoObjeto, campoDoObjeto, novoValor);
                        escopo.tabela.RegistraObjeto(obj1);
                        return obj1;
                
                   
                    case Instrucao.EH_VETOR:
                        Vetor vtJaExistente = escopo.tabela.GetVetor(nomeDoObjeto, escopo);
                        if (vtJaExistente != null)
                        {
                            vtJaExistente.SetValor(novoValor, escopo);
                            return vtJaExistente;
                        }
                        else
                            return null;


                    case Instrucao.EH_PRPOPRIEDADE_ESTATICA:
                        Objeto propriedadeEstatica = new Objeto("public", nomeDoObjeto, tipoDoObjeto, novoValor, true);
                        Classe classeDaPropriedadeEstatica = escopo.tabela.GetClasse(tipoDoObjeto, escopo);
                        
                        
                        if (classeDaPropriedadeEstatica.GetPropriedade(propriedadeEstatica.GetNome()) != null)
                            classeDaPropriedadeEstatica.GetPropriedade(propriedadeEstatica.GetNome()).SetValor(novoValor, escopo);
                        else
                            classeDaPropriedadeEstatica.propriedadesEstaticas.Add(propriedadeEstatica);
                        
                        
                        return propriedadeEstatica;

                }


            }

            return null;
        } // InstrucaoAtribuicao()


        private object InstrucaoChamadaDeFuncao(Instrucao instrucao, Escopo escopo)
        {
        
            if (instrucao.expressoes[1].GetType() == typeof(ExpressaoChamadaDeFuncao ))
            {
                ExpressaoChamadaDeFuncao  funcaoExpressao = (ExpressaoChamadaDeFuncao )instrucao.expressoes[1];
                Funcao funcaoDaChamada = funcaoExpressao.funcao;
                List<Expressao> expressoesParametros = funcaoExpressao.expressoesParametros;
                return funcaoDaChamada.ExecuteAFunction(funcaoExpressao.expressoesParametros, funcaoDaChamada.caller);
            } // if
            return null;
        }

      

        private object InstrucaoDefinicaoDeFuncao(Instrucao instrucao, Escopo escopo)
        {
            // a instrucao de definicao nao eh executada no programa VM
            return null;
        }


       




    

    } // class ProcessamentoInstrucoes()


    // uma instrução da linguagem orquidea  tem 4 objetos: 1- um id do tipo int, para controle de chamadas de métodos/funções, 2- o codigo da instrução, 3- a lista de expressões utilizadas, 4- a lista de blocos de sequencias que comporarão bloos associados à instrução.
    public class Instrucao
    {
        public int code; // tipo da instrução.
        public int IP_Instrucao = 0; // ponteiro de obter instruções a serem avaliadas.

        public List<Expressao> expressoes { get; set; } // expressões da instrução.
        public List<List<Instrucao>> blocos { get; set; } // blocos de instrução associada à instrução.
        public List<int> flags { get; set; }

        public delegate void BuildInstruction(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos, UmaSequenciaID sequencia);


        public const int EH_OBJETO = 1; //: a atribuica é feita sobre um objeto.
        public const int EH_VETOR = 7; // a atribuicao é feita sobre uma variavel vetor.
        public const int EH_PRPOPRIEDADE_ESTATICA = 4; //a atribuição é feita sobre uma propriedade estatica.
     
        public const int EH_DEFINICAO = 5; //é definição (criação)
        public const int EH_MODIFICACAO = 6; //sem definicao, apenas modificacao do valor.

        private static Dictionary<int, string> dicNamesOfInstructions;
        private static System.Random random = new System.Random();

        public void InitNames()
        {
            
            dicNamesOfInstructions = new Dictionary<int, string>();
            dicNamesOfInstructions = new Dictionary<int, string>();
            dicNamesOfInstructions[ProgramaEmVM.codeAtribution] = "Atribution";
            dicNamesOfInstructions[ProgramaEmVM.codeCallerFunction] = "Caller of a Function";
            dicNamesOfInstructions[ProgramaEmVM.codeDefinitionFunction] = "Definition of a Function";
            dicNamesOfInstructions[ProgramaEmVM.codeIfElse] = "if/else";
            dicNamesOfInstructions[ProgramaEmVM.codeFor] = "for";
            dicNamesOfInstructions[ProgramaEmVM.codeWhile] = "while";
            dicNamesOfInstructions[ProgramaEmVM.codeReturn] = "return";
            dicNamesOfInstructions[ProgramaEmVM.codeContinue] = "continue flux";
            dicNamesOfInstructions[ProgramaEmVM.codeBreak] = "break flux";
            dicNamesOfInstructions[ProgramaEmVM.codeGetObjeto] = "GetObjeto";
            dicNamesOfInstructions[ProgramaEmVM.codeSetObjeto] = "SetVar";
            dicNamesOfInstructions[ProgramaEmVM.codeOperadorBinario] = "operador binario";
            dicNamesOfInstructions[ProgramaEmVM.codeOperadorUnario] = "operador unario";
            dicNamesOfInstructions[ProgramaEmVM.codeCasesOfUse] = "casesOfUse";
            dicNamesOfInstructions[ProgramaEmVM.codeCreateObject] = "Create a Object";
    
        }


        /// adiciona um novo tipo de instrução, com id identificador, texto contendo a sintaxe da instrução, e um método para processamento da instrução.
        public void AddNewTypeOfInstruction(int code, string templateInstruction,ProgramaEmVM.HandlerInstrucao instruction)
        {
            dicNamesOfInstructions[code] = templateInstruction;
            ProgramaEmVM.dicHandlers[code] = instruction;
        }

   

        // construtor. contém os elementos de uma instrução VM: codigo ID, expressoes associadas, e blocos de instruções.
        public Instrucao(int code, List<Expressao> expressoesDaInstrucao, List<List<Instrucao>> blocos)
        {
            this.flags = new List<int>();
            if (dicNamesOfInstructions == null)
                this.InitNames();
            this.IP_Instrucao = ProgramaEmVM.IP_contador++; // registra a instrução com um endereço para a VM.
            this.code = code;
            if ((expressoesDaInstrucao != null) && (blocos != null))
            {
                this.expressoes = expressoesDaInstrucao.ToList<Expressao>();
                this.blocos = blocos.ToList<List<Instrucao>>();
            }  // if
            else
            {
                this.expressoes = new List<Expressao>();
                this.blocos = new List<List<Instrucao>>();
            }

        } // Instrucao()


        // construtor. contém os elementos de uma instrução VM: codigo ID, expressoes associadas, e blocos de instruções.
        public Instrucao(int code, List<ExpressaoSemValidacao> expressoesDaInstrucao, List<List<Instrucao>> blocos, Escopo escopo)
        {
            this.flags = new List<int>();
            if (dicNamesOfInstructions == null)
                this.InitNames();
            this.IP_Instrucao = ProgramaEmVM.IP_contador++; // registra a instrução com um endereço para a VM.
            this.code = code;
            if ((expressoesDaInstrucao != null) && (blocos != null))
            {
                this.expressoes = ValidaExpressoes(expressoesDaInstrucao, escopo);
                this.blocos = blocos.ToList<List<Instrucao>>();
            }  // if

        } // Instrucao()

        private List<Expressao> ValidaExpressoes(List<ExpressaoSemValidacao> expressoes, Escopo escopo)
        {
            throw new System.Exception("metodo nao terminado, falta a conversao de expressoes sem verificacao de tipos, para uma lista de expressoes com escopo.").InnerException;
        }

        public override string ToString()
        {
            return dicNamesOfInstructions[this.code].ToString();
            
        }
    } // class Instrucao


    public class ComparerInstruction : IComparer<Instrucao>
    {
        public int Compare(Instrucao x, Instrucao y)
        {
            if (x.IP_Instrucao < y.IP_Instrucao)
                return -1;
            if (x.IP_Instrucao > y.IP_Instrucao)
                return +1;
            return 0;
        }
    }
} //  namespace paser
