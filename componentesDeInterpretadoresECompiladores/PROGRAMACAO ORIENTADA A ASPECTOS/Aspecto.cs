using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class Aspecto
    {
        /// algoritmo:
        ///  1 - Faz uma analise, procurando tipo de objeto ou nome de metodo presente em cada expressao de cada classe orquidea. Classes importadas
        ///  não é possível implementar o asprecto, justamente porque não tem instruções orquidea, mas código importado da linguagem base.
        ///  2- encontrando um tipo de objeto ou chamada de metodo, calcula e compila uma chamada de instrucao de chamada do funcao "Cut".
        ///  3- ao inserir a chamada da funcao cut, está completo a compilação do aspecto, inserido nas classes da linguagem orquidea.

        ///  1- as classes precisam já estar compiladas.
        ///  2- é preciso criar e compilar uma chamada "[nomeAspecto]Cut", para ser invocado que implementa o aspecto.
        ///             2.1- uma constacao é que a funcao "Cut" é codificado com instruções da linguagem orquidea.
       
        /// se a funcao corte possuir um parâmetro, o objeto a ser monitorado, não será preciso de um nome alias para o objeto,
        /// dentro da função de corte.


        public List<string> tipoDeInsercao= new List<string>(){"before", "after", "all" };
        public enum TypeAlgoritmInsertion { ByObject, ByCallingMethod};

        public delegate bool InsercaoAlgoritmo(Instrucao instrucao, string nomeClasse, string nomeMetodo,string tipoInsercao, ref List<Instrucao> instrucoes, int indexInsercao, Escopo escopo);

        private Funcao funcaoCorte { get; set; }

        public string nomeAspecto { get; private set; }

      
        public string nomeTipoDoobjeto { get; private set; }

        private string nomeMetodo { get;  set; }

        private List<Instrucao> instrucoesDeCorte { get; set; }

        private string tipoInsercao { get; set; }



        public Aspecto(string nameAspect, string nomeTipoDoObjeto, string nomeMetodo, List<Instrucao> instrucoesDoCorte, Escopo escopo, TypeAlgoritmInsertion tipoAlgoritmo, string tipoInsercao)
        {
            this.nomeAspecto = "aspect_" + nameAspect;
            this.nomeTipoDoobjeto = nomeTipoDoObjeto;
            this.nomeMetodo = nomeMetodo;
            this.tipoInsercao = tipoInsercao;
            this.instrucoesDeCorte = instrucoesDoCorte.ToList<Instrucao>();

        }


        public Aspecto(string nameAspect, string nomeTipoDoObjeto, string nomeMetodo, Funcao functionCutter, Escopo escopo, TypeAlgoritmInsertion tipoAlgoritmo, string tipoInsercao)
        {
            if (functionCutter == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "funcao de corte nao definida, ou com erros de sintaxe.", escopo.codigo);
                return;
            }
            this.nomeAspecto = "aspect_" + nameAspect;
            this.nomeTipoDoobjeto = nomeTipoDoObjeto;
            this.nomeMetodo = nomeMetodo;

            this.tipoInsercao = tipoInsercao;
            this.funcaoCorte = functionCutter;
            this.funcaoCorte.nome = this.nomeAspecto;

            escopo.tabela.RegistraFuncao(this.funcaoCorte);




        }

        public void AnaliseAspecto(Escopo escopo)
        {
            LinguagemOrquidea lng = LinguagemOrquidea.Instance();
            if (nomeMetodo != null)
                BuildAspect(lng, this.nomeTipoDoobjeto, this.nomeMetodo, tipoInsercao, InsertionHighLevelByCallingMethod, escopo);
            else
            if (nomeMetodo == null)
                BuildAspect(lng, this.nomeTipoDoobjeto, this.nomeMetodo, tipoInsercao, InsertionHighLevelByObjects, escopo);
        }

 
        /// <summary>
        /// faz uma varredura sobre cada expressao dos metodos de cada metodo de cada classe, encontrando um match, 
        /// faz a insercao da chamada da funcao de corte, OU inserção da lista de instruções de corte.
        /// </summary>
        /// <param name="lng">linguagem contendo as classes.</param>
        /// <param name="classeObjeto">nome da classe do objeto para a insersao.</param>
        /// <param name="tipo">tipo da insercao (antes da instrucao Match, depois da instrucao Match, ou depois de cada instrucao de um metodo.</param>
        /// <param name="insercaoMetodo">algoritmo para insercao (por objeto, por chamada de metodo.</param>
        private void BuildAspect(LinguagemOrquidea lng, string classeObjeto, string nomeMetodo, string tipo, InsercaoAlgoritmo insercaoMetodo, Escopo escopo)
        {
            if (escopo.tabela.GetClasses() == null)
                return;
            else
            {
                for (int indexClasse = 0; indexClasse < escopo.tabela.GetClasses().Count; indexClasse++) 
                {
                    Classe umaClasse = escopo.tabela.GetClasses()[indexClasse]; 
                    if (umaClasse.GetMetodos() != null)

                        for (int indexMetodo = 0; indexMetodo < umaClasse.GetMetodos().Count; indexMetodo++)
                        {
                            Funcao umMetodo = umaClasse.GetMetodos()[indexMetodo];
                            if (umMetodo.instrucoesFuncao != null)
                                if (tipo != "all") 
                                {
                                    for (int indexInstrucao = 0; indexInstrucao < umMetodo.instrucoesFuncao.Count; indexInstrucao++)
                                    {
                                        Instrucao instrucaoCurrente = umMetodo.instrucoesFuncao[indexInstrucao];

                                        // chama o metodo de insercao da instrucao do aspecto, numa lista de inserções.
                                        List<Instrucao> instrucoesDoMetodo = umMetodo.instrucoesFuncao;
                                        if (insercaoMetodo(instrucaoCurrente, umaClasse.GetNome(), nomeMetodo, tipo, ref instrucoesDoMetodo, indexInstrucao, escopo))
                                        {

                                            umMetodo.instrucoesFuncao = instrucoesDoMetodo;
                                        }

                                        if (instrucaoCurrente.blocos != null)
                                        {
                                            for (int indexBloco = 0; indexBloco < instrucaoCurrente.blocos.Count; indexBloco++)
                                            {
                                                for (int indexInstrucaoDoBloco = 0; indexInstrucaoDoBloco < instrucaoCurrente.blocos[indexBloco].Count; indexInstrucaoDoBloco++)
                                                {
                                                    List<Instrucao> instrucoesDoBloco = instrucaoCurrente.blocos[indexBloco];
                                                    // chama o metodo de insercao da instrucao do aspecto, numa lista de inserções.
                                                    if (insercaoMetodo(instrucaoCurrente.blocos[indexBloco][indexInstrucaoDoBloco], classeObjeto, nomeMetodo, tipo, ref instrucoesDoBloco, indexInstrucaoDoBloco, escopo))
                                                    {
                                                        instrucaoCurrente.blocos[indexBloco] = instrucoesDoBloco;
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                                else
                                if (tipo == "all")
                                {
                                    
                                    List<Instrucao> instrucoesDoMetodo = umMetodo.instrucoesFuncao;
                                    if (insercaoMetodo != null)
                                    {
                                        for (int indexInstrucao = 0; indexInstrucao < instrucoesDoMetodo.Count; indexInstrucao++)
                                            // insere uma chamada do metodo cut em todas instrucoes do metodo.
                                            if (insercaoMetodo(instrucoesDoMetodo[indexInstrucao], classeObjeto, nomeMetodo, tipo, ref instrucoesDoMetodo, indexInstrucao, escopo))
                                            {
                                                umMetodo.instrucoesFuncao = instrucoesDoMetodo;
                                            }
                                    }
                                    umMetodo.instrucoesFuncao = instrucoesDoMetodo;
                                }
                        }
                }
            }
        }

        /// <summary>
        /// faz a inserção da instrução aspecto, em uma lista de instruções.
        /// </summary>
        /// <param name="instrucao">instrucao base, em que a insercao será feita. </param>
        /// <param name="tipoObjeto">classe do objeto monitorado.</param>
        /// <param name="tipo">tipo de inserção: before, after, ou all.</param>
        /// <param name="instrucoesDoMetodo">lista de todas instruções do método.</param>
        /// <param name="indexInsercao">indice de inserção, perante a lista de instruções do método monitorado.</param>
        private bool InsertionHighLevelByObjects(Instrucao instrucao, string tipoObjeto, string nomeMetodo,string tipo, ref List<Instrucao> instrucoesDoMetodo, int indexInsercao, Escopo escopo)
        {
            // caso base da recursão.
            if (instrucao == null)
                return false;
            // caso: instrucao principal.
            if ((instrucao != null) && (instrucao.expressoes != null))
            {
                for (int indexExpressao = 0; indexExpressao < instrucao.expressoes.Count; indexExpressao++)
                    if ((instrucao.expressoes[indexExpressao].Elementos != null) || (instrucao.expressoes[indexExpressao].Elementos.Count > 0))
                        for (int indexSubExpressao = 0; indexSubExpressao < instrucao.expressoes[indexExpressao].Elementos.Count; indexSubExpressao++)
                        {

                            if (instrucao.expressoes[indexExpressao].Elementos[indexSubExpressao].GetType() == typeof(ExpressaoObjeto))
                            {
                                ExpressaoObjeto exprssObject = (ExpressaoObjeto)instrucao.expressoes[indexExpressao].Elementos[indexSubExpressao];
                                Objeto objetoToWatch = exprssObject.objeto; // nao é preciso verificar o valor do objeto, apenas seu nome.
                                                                            // o valor será calculado no momento da execução da instrução "aspect", em tempo de execução.

                                if (objetoToWatch != null)
                                {
                                    if (objetoToWatch.GetTipo() == this.nomeTipoDoobjeto)
                                    {
                                        int offsetCorte = 0;
                                        if (tipo == "before")
                                            offsetCorte = -1;
                                        else
                                        if (tipo == "after")
                                            offsetCorte = +1;


                                        if (this.funcaoCorte != null)
                                        {
                                            // insercao com uma funcao de corte.
                                            instrucoesDoMetodo.Insert(indexInsercao + offsetCorte, BuildCallingMethodCut(exprssObject));
                                            return true;
                                        }
                                        else
                                        if ((this.funcaoCorte == null) && (this.instrucoesDeCorte != null))
                                        {
                                            for (int x = instrucoesDeCorte.Count - 1; x >= 0; x--)
                                                instrucoesDoMetodo.Insert(indexInsercao + offsetCorte, instrucoesDeCorte[x]);
                                            return true;
                                        }

                                    }
                                }
                            }

                        }
                // caso instrucoes de blocos. insercao recursiva.
                if (instrucao.blocos != null)
                    for (int indexBloco = 0; indexBloco < instrucao.blocos.Count; indexBloco++)
                    {
                        for (int indexInstrucaoBloco = 0; indexInstrucaoBloco < instrucao.blocos[indexBloco].Count; indexInstrucaoBloco++)
                        {
                            List<Instrucao> instrucoesDoBloco = instrucao.blocos[indexBloco];
                            // insercao recursiva, há instruções que podem conter mais instruções (em sua lista de blocos de instruções), que pode conter mais listas de blocos, ...
                            InsertionHighLevelByObjects(instrucao.blocos[indexBloco][indexInstrucaoBloco], tipoObjeto,nomeMetodo, tipo, ref instrucoesDoBloco, indexInstrucaoBloco, escopo);
                            instrucao.blocos[indexBloco] = instrucoesDoBloco;
                        }
                    }
            }

            return false;
        }

        private bool InsertionHighLevelByCallingMethod(Instrucao instrucao, string tipoObjeto, string nomeMetodo, string tipoDeInsercao, ref List<Instrucao> instrucoesDoMetodo, int indexInsercao, Escopo escopo)
        {

            // caso base: instrucao principal.
            if ((instrucao != null) && (instrucao.expressoes != null))
            {
                for (int indexExpressao = 0; indexExpressao < instrucao.expressoes.Count; indexExpressao++)
                    for (int indexSubExpressao = 0; indexSubExpressao < instrucao.expressoes[indexExpressao].Elementos.Count; indexSubExpressao++)
                    {
                        if (instrucao.expressoes[indexExpressao].Elementos[indexSubExpressao].GetType() == typeof(ExpressaoChamadaDeMetodo))
                        {
                            ExpressaoChamadaDeMetodo exprssMethodCalling = (ExpressaoChamadaDeMetodo)instrucao.expressoes[indexExpressao].Elementos[indexSubExpressao];

                            if ((exprssMethodCalling.chamadaDoMetodo!=null) &&
                                (exprssMethodCalling.chamadaDoMetodo.Count>0) &&
                                (exprssMethodCalling.objectCaller!=null) &&
                                (exprssMethodCalling.chamadaDoMetodo[0].funcao.nome == nomeMetodo) &&
                                (exprssMethodCalling.objectCaller.GetTipo() == tipoObjeto))
                            {
                                int offsetCorte = 0;
                                if (tipoDeInsercao == "before")
                                    offsetCorte = -1;
                                else
                                if (tipoDeInsercao == "after")
                                    offsetCorte = +1;


                                if (this.funcaoCorte != null)
                                {
                                    // caso de funcao de corte.
                                    instrucoesDoMetodo.Insert(indexInsercao + offsetCorte, BuildCallingMethodCut_CallingMethod(new ExpressaoObjeto(exprssMethodCalling.objectCaller), this.funcaoCorte.nome));
                                    return true;
                                }
                                else
                                    if (this.funcaoCorte == null)
                                {
                                    for (int x = instrucoesDeCorte.Count - 1; x >= 0; x--)
                                        instrucoesDoMetodo.Insert(indexInsercao + offsetCorte, instrucoesDeCorte[x]);
                                    return true;
                                }
                  
                            }
                        }
                    }
                // caso instrucoes de blocos. insercao recursiva.
                if (instrucao.blocos != null)
                    for (int indexBloco = 0; indexBloco < instrucao.blocos.Count; indexBloco++)
                    {
                        List<Instrucao> instrucoesDoBloco = instrucao.blocos[indexBloco];
                        for (int indexInstrucaoBloco = 0; indexInstrucaoBloco < instrucao.blocos[indexBloco].Count; indexInstrucaoBloco++)
                            InsertionHighLevelByCallingMethod(instrucao.blocos[indexBloco][indexInstrucaoBloco], tipoObjeto, nomeMetodo,tipoDeInsercao, ref instrucoesDoBloco, indexInstrucaoBloco, escopo);
                        instrucao.blocos[indexBloco] = instrucoesDoBloco;
                    }
            }
            return false;

        }

        /// constroi e compila a instrucao que implementa a chamada da funcao de corte!, para o caso de monitoracao de um objeto.
        public Instrucao BuildCallingMethodCut(ExpressaoObjeto instanciaObjeto)
        {
            ExpressaoChamadaDeFuncao expresssaoChamada = new ExpressaoChamadaDeFuncao(this.funcaoCorte);
            ExpressaoElemento expressaoNomeobjetoMonitorado = new ExpressaoElemento(instanciaObjeto.objeto.GetNome());

            Instrucao instrucaoChamada = new Instrucao(ProgramaEmVM.codeAspectos,
                new List<Expressao>() { expresssaoChamada, expressaoNomeobjetoMonitorado }, new List<List<Instrucao>>());
           
            
            return instrucaoChamada;
        }

        /// constroi e compila a instrucao que implementa a chamada da funcao de corte!, para o caso de monitoracao de um objeto, E um metodo.
        public Instrucao BuildCallingMethodCut_CallingMethod(ExpressaoObjeto instanciaObjeto, string nomeMetodo)
        {
            ExpressaoChamadaDeFuncao expresssaoChamada = new ExpressaoChamadaDeFuncao(this.funcaoCorte);
            ExpressaoElemento expressaoNomeobjetoMonitorado = new ExpressaoElemento(instanciaObjeto.objeto.GetNome());


            Instrucao instrucaoChamadaFuncao = new Instrucao(ProgramaEmVM.codeAspectos,
                new List<Expressao>() { expresssaoChamada, expressaoNomeobjetoMonitorado }, new List<List<Instrucao>>());

            return instrucaoChamadaFuncao;

        }


    }
}
