using System.Collections.Generic;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Reflection;
using System;
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
        internal static int codeCallerMethod = 25;
        internal static int codeExpressionCorrect = 26;

        public delegate object HandlerInstrucao(Instrucao umaInstrucao, Escopo escopo);

        public  int IP_contador = 0; // guarda o id das sequencias.
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
                dicHandlers[codeCallerMethod] = this.InstrucaoChamadaDeMetodo;
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
                dicHandlers[codeExpressionCorrect] = InstrucaoExpressaoValida;
            }
        } // InstrucoesVM()


        /// executa uma instrução dentro do programa contido na VM.
        internal object ExecutaUmaInstrucao(Instrucao umaInstrucao, Escopo escopo)
        {
            this.resultLastInstruction = dicHandlers[umaInstrucao.code](umaInstrucao, escopo); 
            return this.resultLastInstruction;
        } // ExecutaUmaInstrucao()


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

        public object InstrucaoChamadaDeMetodo(Instrucao instrucao, Escopo escopo)
        {
            Expressao expressaoChamadaDeMetodo = instrucao.expressoes[0].Elementos[1];


            EvalExpression eval = new EvalExpression();
            return eval.EvalPosOrdem(expressaoChamadaDeMetodo, escopo);


        } // InstrucaoCreateObject()

        public object InstrucaoExpressaoValida(Instrucao instrucao, Escopo escopo)
        {
            Expressao expressao = instrucao.expressoes[0];
            EvalExpression eval = new EvalExpression();
            return eval.EvalPosOrdem(expressao, escopo);

        } // InstrucaoCreateObject()


        public object InstrucaoCreateObject(Instrucao instrucao, Escopo escopo)
        {

            /// EXPRESSOES:
            /// 0- NOME "create"
            /// 1- tipo do objeto
            /// 2- nome do objeto
            /// 3- tipo template do objeto: Objeto/Vetor.
            /// 4- expressoes indices vetor.
            /// 5- expressoes parametros.
            /// 


            Expressao exprssDesempacotamento = instrucao.expressoes[0];
            if (exprssDesempacotamento.Elementos[0].ToString() != "create")
                return null;
            
            
            
            string tipoDoObjetoAReceberAInstanciacao = exprssDesempacotamento.Elementos[1].ToString();
            string nomeDoObjetoAReceberAInstanciacao = exprssDesempacotamento.Elementos[2].ToString();
            string tipoTemplateObjeto = exprssDesempacotamento.Elementos[3].ToString();
      


            Expressao expressoesParametros;
            if ((exprssDesempacotamento.Elementos.Count >= 5) && (exprssDesempacotamento.Elementos[5].Elementos.Count > 0))
                expressoesParametros = exprssDesempacotamento.Elementos[5];
            else
                expressoesParametros = new Expressao();




            if (tipoTemplateObjeto == "Objeto")
            {

                Objeto objJaInstanciado = escopo.tabela.GetObjeto(nomeDoObjetoAReceberAInstanciacao, escopo);
                if (objJaInstanciado != null)
                    escopo.tabela.GetObjetos().Remove(objJaInstanciado); // garante que apenas um objeto desse nome seja instanciado.



                int indexConstructor = ProcessadorDeID.FoundACompatibleConstructor(tipoDoObjetoAReceberAInstanciacao, expressoesParametros.Elementos);
                Funcao construtor = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoObjetoAReceberAInstanciacao).construtores[indexConstructor];

                Classe classeDoObjetoInstanciado = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoObjetoAReceberAInstanciacao);


                Escopo escopoCreate = escopo.Clone();
                if (classeDoObjetoInstanciado.GetPropriedades() != null)
                    foreach (Objeto propriedadeDaClasse in classeDoObjetoInstanciado.GetPropriedades())
                        escopoCreate.tabela.GetObjetos().Add(propriedadeDaClasse);

                // cria uma nova instancia do objeto.
                object novoObjeto = construtor.ExecuteAConstructor(expressoesParametros.Elementos, tipoDoObjetoAReceberAInstanciacao, escopoCreate);
                Objeto objCriado = (Objeto)novoObjeto;

                for (int x = 0; x < objCriado.GetFields().Count; x++)
                {
                    Objeto objetoCampo = escopoCreate.tabela.GetObjetos().Find(k => k.GetNome() == objCriado.GetFields()[x].GetNome());
                    if (objetoCampo != null)
                        objCriado.GetFields()[x].SetValor(objetoCampo.GetValor());
                }

                if (construtor.InfoMethod == null)
                {
                    // o objeto criado é um objeto orquidea!
                    Objeto objetoInstanciado = (Objeto)novoObjeto; 
                    objetoInstanciado.SetNome(nomeDoObjetoAReceberAInstanciacao);


                    escopo.tabela.GetObjetos().Add(objetoInstanciado);
                }
                else
                {
                    // o objeto criado é um objeto importado!
                    Objeto objetoInstanciado = (Objeto)novoObjeto;
                    objetoInstanciado.SetNome(nomeDoObjetoAReceberAInstanciacao);
                    objetoInstanciado.SetValor(novoObjeto);

                    escopo.tabela.GetObjetos().Add(objetoInstanciado);

                }
            }
            else
            if (tipoTemplateObjeto == "Vetor")
            {
                // cria objetos vetor! Um vetor é um objeto passado por referência!
                Vetor vetor = escopo.tabela.GetVetor(nomeDoObjetoAReceberAInstanciacao, escopo);
                return vetor; // os indices de criação do vetor são imutáveis, então a instância da variável e constante e calculada no build de compilação. 
            }
            return null;
        } // InstrucaoCreateObject()

   


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
            v.SetValor(((ExpressaoElemento)instrucao.expressoes[1]).elemento);
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
                exprssControle.isModify = true;
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
                exprsCondicional.isModify = true;
                if ((instrucao.blocos[0] != null) && (instrucao.blocos[0].Count > 0))
                {
                    Executa_bloco(instrucao, escopo, 0);
                    exprsIncremento.isModify = true;
                    varAtribuicao += (int)eval.EvalPosOrdem(exprsIncremento, escopo); // atualiza a expressão de incremento.
                    if ((int)varAtribuicao > (int)limiteMalha)
                        break;
                }
                else
                    break;
            } // while
 
            return null;
        } // ForInstrucao()

        private object InstrucaoAtribuicao(Instrucao instrucao, Escopo escopo)
        {

            /// estrutura de dados para atribuicao:
            /// 0- Elemento[0]: tipo do objeto.
            /// 1- Elemento[1]: nome do objeto.
            /// 2- Elemento[2]: se tiver propriedades/metodos aninhados: expressao de aninhamento. Se não tiver, ExpressaoElemento("") ".
            /// 3- expressao da atribuicao ao objeto/vetor. (se nao tiver: ExpressaoELemento("")

            string tipoAtribuicao = null;
            string nomeAtribuicao = null;
            string nomeCampo = null;
            Expressao atribuicao = null;
            ExpressaoAtribuicaoPropriedadesAninhadas propriedadesAninhadas = null;
            if (instrucao.expressoes[0].ToString() == "AtribuicaoChamadaDeMetodo") 
            {

                EvalExpression eval = new EvalExpression();
                object result= eval.EvalPosOrdem(instrucao.expressoes[1], escopo);

                tipoAtribuicao = instrucao.expressoes[1].Elementos[0].ToString();
                nomeAtribuicao = instrucao.expressoes[1].Elementos[1].ToString();

                escopo.tabela.GetObjetos().Add(new Objeto("private", tipoAtribuicao, nomeAtribuicao, result));
                return result;
            }
            else
            if (instrucao.expressoes[0].GetType() != typeof(ExpressaoAtribuicaoPropriedadesAninhadas))
            {
                if (RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.nome == instrucao.expressoes[0].Elementos[0].ToString()) != null)
                {
                    nomeAtribuicao = instrucao.expressoes[0].Elementos[1].ToString();
                    tipoAtribuicao = instrucao.expressoes[0].Elementos[0].ToString();
                }
                else
                {
                    nomeAtribuicao = instrucao.expressoes[0].Elementos[0].ToString();
                    tipoAtribuicao = escopo.tabela.GetObjeto(nomeAtribuicao, escopo).GetTipo();
                   
                }
                nomeCampo = "";
                atribuicao = instrucao.expressoes[0];
                
            }
            else
            {
                ExpressaoAtribuicaoPropriedadesAninhadas aninhadas = ((ExpressaoAtribuicaoPropriedadesAninhadas)instrucao.expressoes[0]);
                nomeAtribuicao = aninhadas.objetoInicial.GetNome();
                tipoAtribuicao = aninhadas.objetoInicial.GetTipo();
                propriedadesAninhadas = aninhadas;
                atribuicao = aninhadas.expresaoAtribuicao;
            }

            if (instrucao.flags.Contains(Instrucao.EH_MODIFICACAO))
            {

                switch (instrucao.flags[1])
                {
                    case Instrucao.EH_OBJETO:
                        if (propriedadesAninhadas == null)
                        {
                           
                            object novoValorDoCampoDoObjeto = new EvalExpression().EvalPosOrdem(atribuicao, escopo);

                            Objeto obj1 = escopo.tabela.GetObjeto(nomeAtribuicao, escopo);
                            if ((obj1 != null) && (atribuicao != null))
                            {
                                
                                    if (nomeCampo != "")
                                        obj1.SetValorField(nomeCampo, novoValorDoCampoDoObjeto); // guarda o valor calculado, na propriedade do aninhamento.
                                    else
                                        obj1.SetValorObjeto(novoValorDoCampoDoObjeto);

                                    return obj1;
                            }
                        }
                        else
                        {
                            Objeto objAAtribuir = propriedadesAninhadas.objetoInicial;
                            for (int x = 1; x < propriedadesAninhadas.aninhamento.Count; x++)
                                objAAtribuir = objAAtribuir.GetField(propriedadesAninhadas.aninhamento[x].GetNome());

                            EvalExpression eval = new EvalExpression();
                            object result = eval.EvalPosOrdem(atribuicao, escopo);
                            objAAtribuir.SetValor(result);

                        }
                        break;

                    case Instrucao.EH_VETOR:
                        object novoValor3 = new EvalExpression().EvalPosOrdem(instrucao.expressoes[1], escopo);

                        Vetor umVetor = escopo.tabela.GetVetor(nomeAtribuicao, escopo);
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

                            umVetor.SetValor(novoValor3);
                            umVetor.dimensoes = indices.ToArray();

                            return umVetor;
                        }
                        return null;

                    case Instrucao.EH_PRPOPRIEDADE_ESTATICA:
                        string tipoDaPropriedadeEstatica = tipoAtribuicao;
                        string nomeDaPropriedadeEstatica = nomeAtribuicao;

                        Classe classe = escopo.tabela.GetClasse(tipoDaPropriedadeEstatica, escopo);
                        if (classe != null)
                        {
                            object novoValor4 = new EvalExpression().EvalPosOrdem(instrucao.expressoes[1], escopo);
                            Objeto propriedadeEstatica = classe.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaPropriedadeEstatica));
                            propriedadeEstatica.SetValor(novoValor4);
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

                string tipoDoObjeto = null;
                string nomeDoObjeto = null;
                object novoValor = new EvalExpression().EvalPosOrdem(atribuicao, escopo);
                if (propriedadesAninhadas == null)
                {
                    if (RepositorioDeClassesOO.Instance().ObtemUmaClasse(instrucao.expressoes[0].Elementos[0].ToString()) == null)
                    {
                        nomeDoObjeto = instrucao.expressoes[0].Elementos[0].ToString();
                        Objeto objetoReferenciadoAnteriormente= escopo.tabela.GetObjeto(nomeDoObjeto, escopo);
                        if (objetoReferenciadoAnteriormente != null)
                            tipoDoObjeto = objetoReferenciadoAnteriormente.GetTipo();
                        else
                            return null;
                       
                    }
                    else
                    {
                        tipoDoObjeto = instrucao.expressoes[0].Elementos[0].ToString();
                        nomeDoObjeto = instrucao.expressoes[0].Elementos[1].ToString();

                    }
                }
                else
                {
                    tipoDoObjeto = propriedadesAninhadas.objetoInicial.GetTipo();
                    nomeDoObjeto = propriedadesAninhadas.objetoInicial.GetNome();

                }



                int tipoDeDados = instrucao.flags[1];
                switch (tipoDeDados)
                {
                    case Instrucao.EH_OBJETO:

                        Objeto obj1 = null;
                        if (escopo.tabela.GetObjeto(nomeDoObjeto, escopo) == null)
                        {
                            obj1 = new Objeto("private", tipoDoObjeto, nomeDoObjeto, novoValor);
                            escopo.tabela.RegistraObjeto(obj1);
                            return obj1;
                        }
                        else
                        {
                            escopo.tabela.GetObjeto(nomeDoObjeto, escopo).SetValor(novoValor);
                            return escopo.tabela.GetObjeto(nomeDoObjeto, escopo);
                        }
                        

                    case Instrucao.EH_VETOR:
                        Vetor vtJaExistente = escopo.tabela.GetVetor(nomeDoObjeto, escopo);
                        if (vtJaExistente != null)
                        {
                            vtJaExistente.SetValor(novoValor);
                            return vtJaExistente;
                        }
                        else
                            return null;


                    case Instrucao.EH_PRPOPRIEDADE_ESTATICA:
                        Objeto propriedadeEstatica = new Objeto("public", nomeDoObjeto, tipoDoObjeto, novoValor, escopo, true);
                        Classe classeDaPropriedadeEstatica = escopo.tabela.GetClasse(tipoDoObjeto, escopo);


                        if (classeDaPropriedadeEstatica.GetPropriedade(propriedadeEstatica.GetNome()) == null)
                            classeDaPropriedadeEstatica.propriedadesEstaticas.Add(propriedadeEstatica);

                        classeDaPropriedadeEstatica.GetPropriedade(propriedadeEstatica.GetNome()).SetValor(novoValor);

                        return propriedadeEstatica;

                }


            }

            return null;
        } // InstrucaoAtribuicao()

     
        private static Objeto ObtemPropriedadeAAtribuir_2(ExpressaoChamadaDeMetodo expressaoChamada, Escopo escopo)
        {
            Objeto objetoCurrente = escopo.tabela.GetObjeto(expressaoChamada.proprieades.objetoInicial.GetNome(), escopo); // é preciso acessar o objeto pelo escopo, pois através da expressão pode estar atualizado.
            List<Objeto> propriedadesAninhadas = expressaoChamada.proprieades.aninhamento;
            int x = 0;
            while ((x<propriedadesAninhadas.Count) &&  (objetoCurrente.GetField(propriedadesAninhadas[x].GetNome()) != null))
            {
                objetoCurrente = objetoCurrente.GetField(propriedadesAninhadas[x].GetNome());
                x++;
            }
            return objetoCurrente;
        }


        private object InstrucaoChamadaDeFuncao(Instrucao instrucao, Escopo escopo)
        {
        
            if (instrucao.expressoes[1].GetType() == typeof(ExpressaoChamadaDeFuncao ))
            {
                ExpressaoChamadaDeFuncao  funcaoExpressao = (ExpressaoChamadaDeFuncao )instrucao.expressoes[1];
                Funcao funcaoDaChamada = funcaoExpressao.funcao;
                List<Expressao> expressoesParametros = funcaoExpressao.expressoesParametros;
                return funcaoDaChamada.ExecuteAFunction(funcaoExpressao.expressoesParametros, funcaoDaChamada.caller, escopo);
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


        public delegate Instrucao handlerCompilador(UmaSequenciaID sequencia, Escopo escopo);
        
        /// POSSIBILITA A EXTENSÃO DE INSTRUÇÕES DA LINGUAGEM, REUNINDO EM UM SÓ LUGAR TODOS OBJETOS NECESSÁRIOS PARA IMPLEMENTAR UMA NOVA INSTRUÇÃO.

        
        /// adiciona um novo tipo de instrução, com id identificador, texto contendo a sintaxe da instrução, e um método para processamento da instrução, e
        /// um metodo para comiplar a instrucao, e também uma sequencia id para reconhecer a instrucao, no compilador.
        public void AddNewTypeOfInstruction(int code, string templateInstruction,ProgramaEmVM.HandlerInstrucao instruction, string sequenciaID_mapeada, handlerCompilador buildCompilador)
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
            this.IP_Instrucao++; // registra a instrução com um endereço para a VM.
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
            this.IP_Instrucao++; // registra a instrução com um endereço para a VM.
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
