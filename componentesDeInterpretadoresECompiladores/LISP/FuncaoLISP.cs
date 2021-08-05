using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace parser.LISP
{
    /// <summary>
    /// Uma função LISP contem um nome, um conjuntos de parâmetros, e um corpo (onde estão o escopo de código da função).
    /// </summary>
    public partial class FuncaoLISP
    {
        /// <summary>
        /// nome da função lisp.
        /// </summary>
        public string nomeFuncao = null;

        /// <summary>
        /// conjunto de instruções que compõem a função lisp; 
        /// </summary>
        public ListaLISP definicaoDaFuncao = null;

        /// <summary>
        /// parâmetros de chamada da função da função lisp;
        /// </summary>
        //public List<DadosDoParametro> parametrosDaFuncao = null;

        public ListaLISP parametros;
        /// <summary>
        /// texto contendo a definição da função lisp.
        /// </summary>
        public string textoFuncao = null;

        /// <summary>
        /// protótipo de função para chamadas a algoritmos de instruções lisp, acessada pelo nome da função.
        /// </summary>
        /// <param name="listaComando">lista contendo o nome da instrução lisp.</param>
        /// <param name="parametros">parâmetros para a função lisp, se necessário.</param>
        /// <returns>retorna uma lista resultante do processamento da função lisp.</returns>
        public delegate ListaLISP Comando(ListaLISP listaComando, List<DadosDoParametro> parametros);

        /// <summary>
        /// contém um mapa de [nome comando]= [funcao comando].
        /// </summary>
        public static Dictionary<string, Comando> ComandosLISP = new Dictionary<string, Comando>();

        /// <summary>
        /// mapa para operações matemáticas genéricas.
        /// </summary>
        private static Dictionary<string, OperacaoMatematicaGenerica> OperacoesMatematicas = new Dictionary<string, OperacaoMatematicaGenerica>();

        /// <summary>
        /// protótipo de função para operações matemáticas com parâmetros genéricos.
        /// </summary>
        /// <param name="p1">operando 1 da operação.</param>
        /// <param name="p2">operando 2 da operação.</param>
        /// <returns></returns>
        public delegate object OperacaoMatematicaGenerica(object p1, object p2);

        public delegate bool ComapracoesGenerica(object p1, object p2);

        /// <summary>
        /// lista clássica para retorno NILL (Vazio, ou Falso).
        /// </summary>
        public static ListaLISP NILL { get; set; }

        /// <summary>
        /// lista clássica para retorno T (verdadeiro).
        /// </summary>
        public static ListaLISP T { get; set; }

        
        public FuncaoLISP()
        {
            // nothing, do while.
        }

        /// <summary>
        /// constroi uma função lisp, a partir de uma lista lisp contendo nome, parâmetros,  um conjunto de parâmetros, e um retorno da função (na forma de uma lista lisp).
        /// </summary>
        /// <param name="lstEntrada">lista contendo o nome, parâmetros e corpo da função.</param>
        public FuncaoLISP(ListaLISP listaFuncao)
        {
            InitComandosLISP();
            ListaLISP lstEntrada = new ListaLISP(listaFuncao);
            if (lstEntrada.nome == "defun")
            {
                this.nomeFuncao = GetName(lstEntrada.cdr());
                // exemplo de função lisp a construir: (defun foo (x y) (+ x y 5)).
                // extrai o corpo da função (contendo as instruções lisp que compõem a função).
                this.definicaoDaFuncao = lstEntrada.GetAllListsUntilFinalList(2);
                this.parametros = lstEntrada.GetAllListsUntilFinalList(3);
                this.textoFuncao = this.definicaoDaFuncao.ToString();
                // adiciona a nova função ao repositório de funções lisp.
                RepositoryFunctionsLISP.Instance().AddFuncion(this);
            } // if
            else
            {
                this.nomeFuncao = GetName(lstEntrada);
                this.definicaoDaFuncao = lstEntrada.GetAllListsUntilFinalList(1);
                this.parametros = this.definicaoDaFuncao.GetEnesimaLista(2);
                this.textoFuncao = this.definicaoDaFuncao.GetEnesimaLista(2).ToString();
            } // else
        } //  FuncaoLISP()

        public FuncaoLISP(string textoFuncao)
        {
            InitComandosLISP();
            ListaLISP lstFuncao = new ListaLISP(textoFuncao);
            // se não for uma chamada de construção de função, adiciona a função no repositório de funções lisp.
            if (GetName(lstFuncao) == "defun")
            {

                // exemplo de função lisp a construir: (defun foo (x y) (+ x y 5)).
                this.definicaoDaFuncao = lstFuncao.GetAllListsUntilFinalList(2);
                // extrai a lista contendo os parâmetros de entrada da função.
                this.parametros = this.definicaoDaFuncao.GetEnesimaLista(3);
                // extrai o nome da função.
                this.nomeFuncao = GetName(this.definicaoDaFuncao);
                // extrai o texto contendo a função.
                this.textoFuncao = this.definicaoDaFuncao.ToString();
                // registra a função no repositório de funções lisp.
                RepositoryFunctionsLISP.Instance().AddFuncion(this);
            } //if
            else
            {
                // extrai o nome da função lisp.
                this.nomeFuncao = GetName(lstFuncao);
                // extrai a definicao da função.
                this.definicaoDaFuncao = lstFuncao.GetAllListsUntilFinalList(1);
                // extrai a lista contendo os parâmetros de entrada da função.
                this.parametros = this.definicaoDaFuncao.GetEnesimaLista(2);
                // extrai o texto contendo a definição da função.
                this.textoFuncao = this.definicaoDaFuncao.ToString();
            } // else

        } // FuncoesLISP()

   
        /// <summary>
        /// carrega as instruções lisp, com um padrâo de projeto Command: permite uma expansão de instruções fácil de se 
        /// fazer: AddComando(string nomeComando, Comando funcaoComando). criando um método de acordo com o protótipo de função [Comando],e um nome para o novo comando lisp.
        /// </summary>
        public  void InitComandosLISP()
        {
            // Se não tiver carregado os comandos (verificando se a lista lisp NILL).
            if (NILL == null)
            {
                // inicia as listas lisp padrão: NILL e T.
                if (NILL == null)
                {
                    NILL = new ListaLISP();
                    NILL.Listas.Add(new LISP.Atomo("F", "F"));
                } // if
                if (T == null)
                {
                    T = new ListaLISP();
                    T.Listas.Add(new LISP.Atomo("T", "T"));
                } // if T

                // inicializa os comandos nativos de lispl.
                ComandosLISP["car"] = Car;
                ComandosLISP["cdr"] = Cdr;
                ComandosLISP["quote"] = Quote;
                ComandosLISP["'"] = Quote;
                ComandosLISP["+"] = Adicao;
                ComandosLISP["-"] = Subtracao;
                ComandosLISP["*"] = Multiplicacao;
                ComandosLISP["/"] = Divisao;
                ComandosLISP["cons"] = Cons;
                ComandosLISP["member"] = Member;
                ComandosLISP["atom"] = Atomo;
                ComandosLISP["cond"] = Cond;
                ComandosLISP["if"] = If;
                ComandosLISP["<="] = MenorOuIgual;
                ComandosLISP[">="] = MaiorOuIgual;
                ComandosLISP["<"] = Menor;
                ComandosLISP[">"] = Maior;
                ComandosLISP["="] = Igual;
                ComandosLISP["setq"] = Setq;
                ComandosLISP["defun"] = DeFun;

                OperacoesMatematicas["+"] = AdicaoGenerica;
                OperacoesMatematicas["-"] = SubtracaoGenerica;
                OperacoesMatematicas["*"] = MultiplicacaoGenerica;
                OperacoesMatematicas["/"] = DivisaoGenerica;
                OperacoesMatematicas["="] = IgualdadeGenerica;
            } // if NILL

        } // InitComandosLISP()
        /// <summary>
        /// verifica se é uma função predefinida.
        /// </summary>
        /// <param name="nameFunction">nome da função a ser verificada.</param>
        /// <returns>retorna [true] se a função com nome da entrada é uma função predifinida.</returns>
        public  bool IsBasicFunction(string nameFunction)
        {
            if (ComandosLISP.Count == 0)
                InitComandosLISP();
            Comando umComando = null;
            ComandosLISP.TryGetValue(nameFunction, out umComando);
            return (umComando != null);
        } //  IsBasicFunction()

        public static bool IsListaLisp(Atomo lista)
        {

            try
            {
                ListaLISP lst = (ListaLISP)lista;
                return true;
            } // try
            catch
            {
                return false;
            } // catch
        }// IsListaLisp()

        /// <summary>
        /// verifica se a função de nome de entrada é a função de nome variável Cadxxr.
        /// </summary>
        /// <param name="nomeFuncao">nome da função a verificar.</param>
        /// <returns></returns>
        private static bool IsComandoCadXXXr(string nomeFuncao)
        {
            if (nomeFuncao == "")
                return false;
            if (nomeFuncao[0] != 'c')
                return false;
            if (nomeFuncao[nomeFuncao.Length - 1] != 'r')
                return false;

            for (int x = 1; x < nomeFuncao.Length - 1; x++)
            {
                if (nomeFuncao[x] == 'a')
                    continue;
                else
                if (nomeFuncao[x] == 'd')
                    continue;
                else
                    return false;
            } // for
            return true;
        }// IsComandoCadXXXr()


        public static bool IsTrue(ListaLISP lista)
        {
            if (lista == null)
                return false;
            if (lista == T)
                return true;
            if (lista.Listas[0] == T)
                return true;
            if (lista.Listas.Count != T.Listas.Count)
                return false;
            if (lista.Listas[0].nome == T.Listas[0].nome)
                return true;
            return false;
        }// IsTrue()


        public static bool IsNILL(ListaLISP lista)
        {
            if (lista == null)
                return true;
            if (lista.Listas.Count == 0)
                return true;
            if ((lista.Listas[0].nome == "F") && (lista.Listas[0].valor == "F"))
                return true;
            for (int x = 0; x < lista.Listas.Count; x++)
                if ((lista.GetCurrentList().GetType() == typeof(ListaLISP)) && (IsNILL(lista.GetCurrentList())))
                    return true;
            return false;
        } // IsFalse()



        /// <summary>
        /// avalia uma função pré-definida ou armazanada no repositório.
        /// avalia textos como: (foo 5 5). Os parâmetros neste exemplo é: (5 5), extraídos dos parãmetros de entrada.
        /// </summary>
        /// <param name="lstChamadaDeFuncao">lista contendo a chamada da função a ser avaliada, como foo(4 4).</param>
        /// <param name="parametros">parâmetros de entrada para a  função.</param>
        /// <returns>retorna uma lista lisp cujos elementos são os resultados da função.</returns>
        public ListaLISP AvaliaFuncao(ListaLISP lstChamadaDeFuncao)
        {
           // se o operador Quoted estiver na lista de chamada, retorna a lista, sem processamento.
            if (lstChamadaDeFuncao.isQuoted)
                return lstChamadaDeFuncao;
            
            ListaLISP listaChamada = new ListaLISP(lstChamadaDeFuncao);
         
            //ListaLISP listaChamada = new ListaLISP();
            if (lstChamadaDeFuncao.Listas.Count == 1)
                lstChamadaDeFuncao = lstChamadaDeFuncao.car();
            
            if ((!IsBasicFunction(GetName(listaChamada))) && (!RepositoryFunctionsLISP.Instance().IsFunctionStored(GetName(listaChamada))))
                return lstChamadaDeFuncao;

            // obtém uma cópia original da lista de entrada (a chamada de função).
            ListaLISP lstFuncao = new ListaLISP(listaChamada);

            //obtém o nome da lista-função;            
            string nomeFuncao = GetName(lstFuncao);


            // se a lista lisp de entrada for NILL ou T, retorna a lista de entrada.
            if ((lstFuncao == NILL) || (lstFuncao == T))
                return NILL;

            // se a lista de entrada não for uma lista, retorna esta não-lista.
            if (lstFuncao.GetType() != typeof(ListaLISP)) 
                return lstFuncao;

            // se a lista for formado por um átomo apenas, retorna esta lista.
            if ((lstFuncao.Listas.Count == 1) && (lstFuncao.Listas[0].GetType() == typeof(Atomo)))
                return lstFuncao;

            // valores para variáveis dentro da lista- função lisp.
            List<DadosDoParametro> parametros = new List<DadosDoParametro>();


            // se a função estiver armazenada no repositório, forma os parâmetros com a definição da lista de parâmetros (nomes) e da lista de chamada da função (valores).
            if (RepositoryFunctionsLISP.Instance().IsFunctionStored(GetName(lstFuncao)))
            {
                FuncaoLISP funcaoAAvaliar = null;
                // obtém os dados dos parâmetros vindos da chamada da avaliação da função.
                funcaoAAvaliar = RepositoryFunctionsLISP.Instance().FindFunction(nomeFuncao);

                parametros = ObtemParametrosDeUmaChamadaDeFuncao(lstFuncao);

            } // if
            else
            if (IsBasicFunction(GetName(lstFuncao)))  
            {
                parametros = ObtemParametrosDeUmaChamadaDeFuncao(lstFuncao);
            } // else
            FuncaoLISP funcaoCorpo = null;

            //___________________________________________________________________________________________________________________________________________________________
            // se a lista de entrada for de uma instrução não básica, carrega a definição da função.
            if (RepositoryFunctionsLISP.Instance().IsFunctionStored(nomeFuncao))
            {
                funcaoCorpo = new FuncaoLISP(RepositoryFunctionsLISP.Instance().FindFunction(nomeFuncao).definicaoDaFuncao);
            }// if
            else
            // se a lista de entrada for de uma instrução básica, constrói uma função a partir da lista de entrada.
            if (IsBasicFunction(nomeFuncao))
                funcaoCorpo = new FuncaoLISP(lstFuncao);
            //_______________________________________________________________________________________________________________________________________________________________

            // se a função for a função de nome variável cadXXr, retorna a avaliação desta função de nome variável.
            if ((IsComandoCadXXXr(nomeFuncao)) && (nomeFuncao != "car") && (nomeFuncao != "cdr"))
                return AvaliaFuncao(Cadxxxr(funcaoCorpo.definicaoDaFuncao, parametros));
            else
            // se a função for um nome de uma função pré-definida, retorna a avaliação desta função pré-definida.
            if (IsBasicFunction(nomeFuncao))
            {
                SubstituiParametros(lstFuncao, parametros);
                // faz a passagem de parâmetros conseguidos no processamento da lista lisp [lstFuncao].
                ListaLISP lstResult = ComandosLISP[nomeFuncao](lstFuncao, parametros);
                return lstResult;
            } // if
            else
            // se for uma função, tem que calcular o parâmetro da chamada.
            if (!IsBasicFunction(nomeFuncao))
            {
                // listas componentes de uma função lisp: nome, parâmetros, e instruções.
                // o primeiro elemento de uma função é o nome da função
                // o segundo elemento de uma função é a lista de parâmetros.
                // o terceiro elemento de uma função, em diante, é o corpo da função.


                ListaLISP listaParametrosDaChamdaDaFuncao = new ListaLISP(lstFuncao.GetAllListsUntilFinalList(2));  ///lista dos parâmetros da função. A diferença é que uma chamada como foo(- x 3) não tem corpo de função,
                                                                                                                    ///é preciso compor o corpo a partir do repositório. Como a lista a ser avaliada é foo(-x 3), 
                                                                                                                    ///avalia os parâmetros de (- x 3).

                ListaLISP listaParametrosDaDefinicaoDaFuncao = new ListaLISP(funcaoCorpo.parametros);
                ListaLISP listaDefinicoes = new ListaLISP(funcaoCorpo.definicaoDaFuncao.GetAllListsUntilFinalList(3));  // lista de instruções da função.

                ListaLISP listaInstrucoes = new ListaLISP(listaDefinicoes);

                // inicializa a lista de retorno da avaliação.
                ListaLISP lstResult = null;


                /// percorre todas instruções lisp da função (uma função lisp pode ter várias instruções, mas o resultado vem da última instrução avaliada.
                while ((listaInstrucoes!=null) &&  (listaInstrucoes!=NILL) &&(listaInstrucoes.Listas.Count > 0))
                {

                    // avalia qualquer função que esteja dentro da lista de parâmetros.
                    parametros = AvaliaParametros(listaParametrosDaChamdaDaFuncao, listaParametrosDaDefinicaoDaFuncao);

                    // substitui os valoes de variáveis da função, pelos valores contidos na lista de parâmetros calculados no início do método.
                    SubstituiParametros(listaInstrucoes, parametros);

                    // restaura os dados da função armazenada, para evitar problemas de referências (ponteiros de memória).
                    RepositoryFunctionsLISP.Instance().RestauraFuncao(nomeFuncao);
                    
                    // chama a função de uma instrução, com os parâmetros atualizados pelos valores da lista de entrada (lista de chamada, como: (foo 5 5).
                    lstResult = AvaliaFuncao(listaInstrucoes);

                    // salta para a próxima instrução lisp da função, contabilizando as instruções que já foram processadas.
                    listaInstrucoes = listaInstrucoes.cdr();
                } // while
                // retorna a última avaliação, da lista de instruções lisp.
                return lstResult;
            }// if !IsBasicFunction();
            return NILL;
        } // AvaliaFuncao()


        /// <summary>
        /// calcula eventuais funções listas-lisp dentro da listagem de parâmetros.
        /// </summary>
        /// <param name="listaDeParametrosDaChamada">lista contendo os nomes dos parâmetros.</param>
        /// 
        /// <returns>retorna uma lista de parâmetros calculados, com ou sem listas-lisp dentro dos parâmetros.</returns>
        private  List<DadosDoParametro> AvaliaParametros(ListaLISP listaDeParametrosDaChamada, ListaLISP listaDefinicaoParametros)
        {
            
            ListaLISP lstParametros = new ListaLISP(listaDeParametrosDaChamada);
            if (lstParametros.Listas.Count == 1)
                lstParametros = lstParametros.car();

         
            List<DadosDoParametro> parametros = new List<DadosDoParametro>();

            for (int elementoLista = 0; elementoLista < lstParametros.Listas.Count; elementoLista++)
            {
                // se o elemento da lista de parâmetros for uma lista, avalia a lista (se for uma função lisp), ou retira elementos de uma lista que não é função lisp.
                if (lstParametros.Listas[elementoLista].GetType() == typeof(ListaLISP))
                {
                    // verifica se a lista é uma função lisp (básica ou armazenada),se for, faz a avaliação da lista, e retorna os parâmetros resultantes da avaliação da lista.
                    ListaLISP lstResult = AvaliaFuncao((ListaLISP)lstParametros.Listas[elementoLista]);
                    
                    // se a lista é vazia ou nula, segue para o próximo elemento.
                    if ((lstResult == null) || (lstResult.Listas.Count == 0) || (lstResult == NILL))
                        continue;
                    for (int x = 0; x < lstResult.Listas.Count; x++)
                    {
                        string valor = lstResult.Listas[x].valor;
                        parametros.Add(new DadosDoParametro(valor, valor));
                    } // for x
                } // if
                // se o elemento da lista de parâmetros for um átomo, adiciona este elemento para a lista de parâmetros, como um parâmetro.
                if ((lstParametros.Listas[elementoLista].GetType() == typeof(Atomo))  && (!IsBasicFunction(lstParametros.Listas[elementoLista].nome)) &&
                   (!RepositoryFunctionsLISP.Instance().IsFunctionStored(lstParametros.Listas[elementoLista].nome)))
                {
                    parametros.Add(new DadosDoParametro(listaDefinicaoParametros.Listas[elementoLista].nome, lstParametros.Listas[elementoLista].valor));
                } // if
            } // for elementoLista

            return parametros;

        } // AvaliaParametros()


        /// <summary>
        /// obtém parâmetros, analisando uma lista contendo valoroes de uma chamada de uma função lisp como (foo 1 5), 
        /// os valores dos parâmetros a serem obtidos são [1] e [5], e os nomes vem da definição da função: (foo(x y)(+ x y))
        /// </summary>
        /// <param name="lstChamada">lista contendo os parâmetros da função, como (foo 5 5), ou foo( -(3 1) (fatorial(3)).</param>
        /// <returns>retorna os parâmetros extraídos da chamada da função  e da lista de definição de parâmetros de um</returns>
        public List<DadosDoParametro> ObtemParametrosDeUmaChamadaDeFuncao(ListaLISP lstChamada)
        {
            ListaLISP lista = new ListaLISP(lstChamada);
            List<DadosDoParametro> parametrosExtraidos = new List<DadosDoParametro>();
            string nomeFuncao = GetName(lista);
            // a lista de entrada é uma função pré-definida.            
            if (IsBasicFunction(nomeFuncao))
            {
                lista = lista.cdr();
                for (int x = 0; x < lista.Listas.Count; x++)
                {
                    if (lista.Listas[x].GetType() == typeof(Atomo))
                    {
                        string valor = lista.Listas[x].valor;
                        parametrosExtraidos.Add(new DadosDoParametro(valor, valor));
                    } // if

                    if (lista.Listas[x].GetType() == typeof(ListaLISP))
                    {
                        List<DadosDoParametro> parametrosDeUmaLista = ObtemParametrosDeUmaChamadaDeFuncao((ListaLISP)lista.Listas[x]);
                        if (parametrosDeUmaLista.Count > 0)
                            parametrosExtraidos.AddRange(parametrosDeUmaLista);
                    } // if
                } // for x
            } //if
            else
            // a lista de entrada é uma função armazenada.
            if (RepositoryFunctionsLISP.Instance().IsFunctionStored(nomeFuncao))
            {
                FuncaoLISP funcaoParametro = RepositoryFunctionsLISP.Instance().FindFunction(nomeFuncao);
                ListaLISP lstParametrosDaFuncao = funcaoParametro.parametros;
                ListaLISP lstParametrosChamada = new ListaLISP((ListaLISP)lista.cdr());
                if (lstParametrosDaFuncao.Listas.Count == lstParametrosChamada.Listas.Count)
                {
                    for (int x = 0; x < lstParametrosChamada.Listas.Count; x++)
                   {
                        string nomeParametro = lstParametrosDaFuncao.Listas[x].nome;
                        string valoParametro = lstParametrosChamada.Listas[x].valor;
                        parametrosExtraidos.Add(new DadosDoParametro(nomeParametro, valoParametro));
                    } // for x
                } // if
            } // if
            else
            // a lista de entrada é uma lista, mas não é função pr=e-definida ou função armazenada no repositório.
            if (lista.GetType() == typeof(ListaLISP))
            {
                for (int x = 0; x < lista.Listas.Count; x++)
                {
                    string valor = lista.Listas[x].valor;
                    parametrosExtraidos.Add(new DadosDoParametro(valor, valor));
                } // for x
            } // if
            else
            // a lista de entrada é um átomo.
            if (lista.GetType() == typeof(Atomo))
            {
                string valor = lista.valor;
                parametrosExtraidos.Add(new DadosDoParametro(valor, valor));
            } // if

            return parametrosExtraidos;
        } // ObtemParametrosDeUmaChamadaDeFuncao()

        private void AvaliaFuncaoParametroEExtraiParametros(List<DadosDoParametro> parametrosExtraidos, ListaLISP lstFuncaoBasica)
        {
            ListaLISP lstResult = AvaliaFuncao(lstFuncaoBasica);

            if ((lstResult != null) && (lstResult != NILL) && (lstResult.Listas.Count > 0))
            {
                // adiciona os parâmetros encontrados.
                foreach (Atomo umParametro in lstResult.Listas)
                {
                    parametrosExtraidos.Add(new DadosDoParametro(umParametro.nome, umParametro.valor));
                } // foreach
            } // if
        }

        /// <summary>
        /// obtém um valor vindo da entrada da chamada de método, modificando uma lista lisp.
        /// é preciso especificar em [parametros] o nome e o valor das variáveis.
        /// A idéia é que ao iniciar uma avaliação de uma lista ou função, os parâmetros as variáveis da função recebam 
        /// os dados da chamada da função ou lista, como em: (foo(5 5)).
        /// 
        /// </summary>
        /// <param name="lista">lista de definição de parâmetros da função lisp.</param>
        /// <param name="parametros">vetor de parâmetros, com nome e valor.</param>
        /// 
        private  void SubstituiParametros(ListaLISP lista, List<DadosDoParametro> parametros)
        {
            string nomeFuncao = GetName(lista);
            ListaLISP listaComando = null;
            if (lista.Listas.Count == 1)
                // pula o nome da lista de listaComando.
                listaComando = new ListaLISP(lista.car().cdr());
            else
                listaComando = new ListaLISP(lista);
            // se a lista de instruções se é vazia ou nula, retorna sem processamento.
            if ((listaComando == null) || (listaComando == NILL) || (listaComando.Listas.Count == 0))
                return;

            for (int elementoLista = 0; elementoLista < listaComando.Listas.Count; elementoLista++) 
            {
                // retorna se a lista resultante for nula ou vazia.
                if ((listaComando == null) || (listaComando == NILL) || (listaComando.Listas.Count == 0))
                {
                    // com vazamento de referências, restaura a função, seus dados, a partir da texto da função.
                    RepositoryFunctionsLISP.Instance().RestauraFuncao(nomeFuncao);
                    return;
                }
                // se a lista for um átomo,
                // retorna:
                // 1- se o átomo for um número, continua (não é necessário modificá-lo com um número, o átomo é uma constante).
                // 2- se o átomo não for um número, substitue o nome do átomo pelo valor correspondente na lista de parâmetros.
                if (listaComando.Listas[elementoLista].GetType() == typeof(Atomo))
                {
                    // o átmo é um número, continua sem modificações.
                    if (ListaLISP.IsNumber(listaComando.Listas[elementoLista].nome))
                        continue;
                    // se o átomo não for um número, mas uma variável, faz uma varredura da lista de entrada procurando variáveis com o mesmo nome do elemento currente.
                    for (int parametro = 0; parametro < parametros.Count; parametro++)
                    {
                        if (ListaLISP.IsNumber(listaComando.Listas[elementoLista].nome))
                        {
                            if (elementoLista == listaComando.Listas.Count)
                                return;
                        } //if 
                        else
                        {
                            if (listaComando.Listas[elementoLista].nome == parametros[parametro].nome)
                            {
                                listaComando.Listas[elementoLista].nome = parametros[parametro].valor.ToString();
                            } //if
                            else
                            if (ListaLISP.IsNumber(parametros[parametro].nome))
                            {
                                listaComando.Listas[elementoLista].valor = parametros[parametro].nome.ToString();
                            }
                            if (elementoLista == listaComando.Listas.Count)
                                return;
                       } // else
                    } // for parametro
                } // for elementoLista

                // se a lista for uma ListaLISP, percorre recursivamente a lista, substituindo valores.
                if (listaComando.Listas[elementoLista].GetType() == typeof(ListaLISP))
                {
                    ListaLISP listaParametro = new ListaLISP((ListaLISP)listaComando.Listas[elementoLista]);
                    SubstituiParametros(listaParametro, parametros);
                    // garante que a lista currente é a lista processada na recursão.
                    listaComando.Listas[elementoLista] = new ListaLISP(listaParametro);
                } // if
            } // for elementoLista
            // restaura os dados da função, se houver problemas por referência.
            RepositoryFunctionsLISP.Instance().RestauraFuncao(nomeFuncao);
        } // ObtemVAloresDosParametros()


        /// <summary>
        /// esta função não é predefinida, é chamada quando se verifica se uma função lisp está no repositório de funções.
        /// </summary>
        /// <param name="listaComando">lista contendo os tokens do comando.</param>
        /// <param name="parametro">não utilizado.</param>
        /// <returns></returns>
        public static ListaLISP Cadxxxr(ListaLISP listaFuncao, List<DadosDoParametro> parametro)
        {
            ListaLISP lstSaida = listaFuncao.GetNextList();

            string tokenComando = listaFuncao.GetCurrentList().Listas[0].nome;

            if (tokenComando[0] != 'c')
                return NILL;
            if (tokenComando[tokenComando.Length - 1] != 'r')
                return NILL;

            for (int x = 1; x < tokenComando.Length - 1; x++)
            {
                if (lstSaida == NILL)
                    return NILL;
                if (tokenComando[x] == 'a')
                    lstSaida = lstSaida.car();
                else
                if (tokenComando[x] == 'd')
                    lstSaida = lstSaida.GetAllListsUntilFinalList(2);
                else
                    return NILL;
            } // foreach
            return lstSaida;

        } // Cadxxxr()

        /// <summary>
        /// comando defun, define uma função lisp, com nome, e corpo da função (conjunto de instruções).
        /// Para acessar os parâmetros da função, é preciso primeiro avaliar a função após o registro da função.
        /// </summary>
        /// <param name="listaComando">lista contendo os parãmetros e corpo da função.</param>
        /// <param name="parametrosDaFuncao">parâmetros, não utilizado, mas para padronizar, junto com outras funções lisp pré-definidas.</param>
        /// <returns></returns>
        public static ListaLISP DeFun(ListaLISP listaComando, List<DadosDoParametro> parametrosDaFuncao)
        {
            // "(defun foo (x y) (+ x y 5))"
            // "(defun quadrado (x) (* x x))"
            string tokenComando = listaComando.car().Listas[0].nome;
            if (tokenComando == "defun")
            {
                // inicializa uma nova função, a partir da lista comando.
                FuncaoLISP funcao = new FuncaoLISP(listaComando);
                // todos dados da função foram extraídos na inicialização da função, retorna a lista contendo a definição da função.
                return funcao.definicaoDaFuncao;
            } // if
            return null;
        } // DeFun()

        /// <summary>
        /// faz um tipo de comparação, determinada por [compInt] e [compFloat].
        /// </summary>
        /// <param name="listaComando">elementos da lista lisp que compõe a instrução lisp.</param>
        /// <param name="parametros">parâmetros para a lista lisp, não utilizada aqui.</param>
        /// <param name="compInt">comparações para o tipo int.</param>
        /// <param name="compFloat">comparações para o tipo float.</param>
        /// <returns></returns>
        private  ListaLISP ComparacaoGenerica(ListaLISP listaComando, List<DadosDoParametro> parametros, ComapracoesGenerica comparacao)
        {

            if (listaComando == T)
                return T;
            if (listaComando == NILL)
                return NILL;
            string tokenComando = listaComando.Listas[0].nome;
            SubstituiParametros(listaComando, parametros);
            ListaLISP lstOperandos = listaComando;


            // comparações para operandos símbolos (que não são números), faz a comparação se nomes e valores são iguais.
            Atomo operando1 = lstOperandos.GetEnesimaLista(2).Listas[0];
            Atomo operando2 = lstOperandos.GetEnesimaLista(3).Listas[0];

            if ((operando1.valor == operando2.valor) && (operando1.nome == operando2.nome))
                return T;
            // comparações para operandos que são números.
            string valorPrimeiroOperando = ExtraiValorOperando(lstOperandos.GetEnesimaLista(2));
            string valorSegundoOperando = ExtraiValorOperando(lstOperandos.GetEnesimaLista(3));
            object primeiroOperando = valorPrimeiroOperando;
            object segundoOperando = valorSegundoOperando;

            if (comparacao(primeiroOperando, segundoOperando))
                return T;
            return NILL;
        } // ComparacaoGenerica

        /// <summary>
        /// extrai um valor numérico, do átomo o valor da lista, se valor é vazio, retorna o nome do átomo da lista
        /// </summary>
        /// <param name="lstOperandos"></param>
        /// <returns></returns>
        private static string ExtraiValorOperando(ListaLISP lstOperandos)
        {
            string valorOperando = lstOperandos.Listas[0].valor;

            if (!ListaLISP.IsNumber(valorOperando))
                valorOperando = lstOperandos.Listas[0].nome;
            return valorOperando;
        }


        private  ListaLISP Maior(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {

            return ComparacaoGenerica(listaComando, parametro, CondicionalMaior);

        } // Maior

        private ListaLISP Menor(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {
            return ComparacaoGenerica(listaComando, parametro, CondicionalMenor);
        } // Menor()

        private  ListaLISP MaiorOuIgual(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {
            return ComparacaoGenerica(listaComando, parametro, CondicionalMaiorOuIgual);
        } // Menor()

        private ListaLISP MenorOuIgual(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {
            return ComparacaoGenerica(listaComando, parametro, CondicionalMenorOuIgual);
        } // Menor()

        private ListaLISP Igual(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {
            return ComparacaoGenerica(listaComando, parametro, CondicionalIgual);
        } // Menor()




        /// <summary>
        /// comando swhitch da linguagem LiSP.
        /// avalia por pares os casos: o primeiro é um teste condicional. Se T, retorna o segundo do par.
        /// pode ser vários pares teste-retorna valor.
        /// Exemplo de cond: (cond ((> x y) x) (t y) )
        ///
        /// </summary>
        /// <param name="parametros">Lista LISP contendo o comando completo [cond]</param>
        /// <returns></returns>
        private ListaLISP Cond(ListaLISP listaComando, List<DadosDoParametro> parametros)
        {
            //(cond ((> a 7)(/ a 2)) ((< a 5) (-a 1)) (T 17))  
            string nomeComando = GetName(listaComando);

            // obtém uma lista com valores de parâmetros modificados.
            SubstituiParametros(listaComando, parametros);

            if (nomeComando == "cond")
            {
                listaComando = listaComando.GetAllListsUntilFinalList(2);
                while (listaComando != NILL)
                {
                    ListaLISP condicional = listaComando.GetEnesimaLista(1).GetEnesimaLista(1);
                    //  avalia a lista condicional, se T (true)
                    if (IsTrue(AvaliaFuncao(condicional)))
                        return AvaliaFuncao(listaComando.GetEnesimaLista(1).GetEnesimaLista(2));
                    // passa para o próximo case.
                    listaComando = listaComando.GetAllListsUntilFinalList(2);
                }// while
            } //if
            return NILL;
        } // Cond()

        /// <summary>
        /// recebe três parâmetros: um que avalia uma condicional. Se
        /// [true], executa o segundo parâmetro, se [false] executa o segundo parâmetro.
        /// Se os parametros 1 e 2 estiver quotado, retorna o parâmetro sem avaliar a lista.
        /// Se os parametros 1 e 2 não estiver quotado, avalia primeiro os parâmetros e retorna o resultado.
        /// </summary>
        /// <param name="parametros">parâmetros para o comando, se necesário.</param>
        /// <returns>retorna uma lista lisp contendo em seus elementos o resultado da avaliação.</returns>
        private  ListaLISP If(ListaLISP listaComand, List<DadosDoParametro> parametros)
        {

            //obtém o nome do comando, que é IF.
            string nomeComando = GetName(listaComand);

            if (nomeComando != "if")
                return NILL;
            ListaLISP lstAtual= new ListaLISP(listaComand);
            /// preenche as variáveis da lista comando de entrada, com os parâmetros contendo nomes e valores para inserir.
            SubstituiParametros(listaComand, parametros);

            if (lstAtual.Listas.Count == 1)
            {
                listaComand = listaComand.car();
            }
            // (if (> 1 5) 4 5) 
            // obtém o processamento da lista condicioanl.
            ListaLISP condicional = AvaliaFuncao(listaComand.GetAllListsUntilFinalList(2).GetCurrentList());
            // ramo de código vindo da operação condicional. ( a enesima lista [2] é a lista do condicional).
            ListaLISP branch1 = listaComand.GetEnesimaLista(3).GetCurrentList();
            // ramo de código vindo da operação condicional.
            ListaLISP branch2 = listaComand.GetAllListsUntilFinalList(4).GetCurrentList();

            if (FuncaoLISP.IsTrue(condicional))
            {
                if (!branch1.isQuoted)
                    return AvaliaFuncao(branch1);
                else
                    return branch1; // o código é para não avaliar a lista, então retorna a lista (está em quote).
            } // if
            else
            {
                if (!branch2.isQuoted)
                    return AvaliaFuncao(branch2);
                else
                    return branch2;
            } //else
        
        }// If()

        /// função clássica Car (Obtém o primeiro elemento, ou primeira lista).
        /// </summary>
        /// <param name="listaEntrada">lista de entrada.</param>
        /// <returns></returns>
        public static ListaLISP Car(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {

            string nomeComando = GetName(listaComando);
            if (nomeComando == "car")
            {
                ListaLISP listaSaida = new ListaLISP(listaComando.GetAllListsUntilFinalList(2).GetCurrentList());
                if (listaSaida == null)
                    return NILL;
                else
                    return listaSaida;
            } // if
            else
                return NILL;
        } // Car()

        /// <summary>
        /// função clássica Cdr (Obtém a lista cauda).
        /// </summary>
        /// <param name="listaEntrada">lista de entrada.</param>
        /// <returns></returns>
        public static ListaLISP Cdr(ListaLISP lista, List<DadosDoParametro> parametro)
        {
            ListaLISP listaEntrada = new ListaLISP(lista);
            string tokenInstrucao = GetName(listaEntrada);
            listaEntrada.Listas.RemoveAt(0);
            if (tokenInstrucao == "cdr")
            {
                listaEntrada.Listas.RemoveAt(0);
                return listaEntrada;
            } //if
            else
                return NILL;
        }// Cdr()

        private static ListaLISP Quote(ListaLISP listaEntrada, List<DadosDoParametro> parametro)
        {
            listaEntrada.isQuoted = true;
            return listaEntrada;
        } // Quote()

        /// <summary>
        /// faz uma chamada de uma operação matemática, tendo como valores dos parãmetros dados na lista [parametros].
        /// Se os operandos forem listas, faz a avaliação da lista.
        /// Se a lista estiver quotada, retor a lista.
        /// </summary>
        /// <param name="listaLisp">lista lisp com instruções matemáticas.</param>
        /// <param name="umaOperacaoMatematica">operação matemática, na versão para números inteiros.</param>
        /// <param name="parametros">lista contendo o nome e o valor das variáveis operandos.</param>
        /// <returns>retorna uma lista lisp contendo o elemento resultante da operação.</returns>
        private  ListaLISP OperacaoMatematica(ListaLISP listaOperacao, OperacaoMatematicaGenerica umaOperacaoMatematica, List<DadosDoParametro> parametros)
        {

            ListaLISP listaLisp = new ListaLISP(listaOperacao);

            if (listaOperacao.Listas.Count == 1)
                listaLisp = listaLisp.car();
            
            listaLisp = listaLisp.cdr();

            // verifica se a lista está quotada, evidenciando que não é uma lista a ser avaliada.
            if (listaLisp.isQuoted)
                return listaLisp;

            // verifica se a lista de entrada é uma lista vazia ou sem itens.
            if ((listaLisp == NILL) || (listaLisp.Listas.Count == 0))
                return NILL;

            // verifica se a lista tem somente um elemento. Se tiver retorna este elemento, pois a operação matemática precisa de dois ou mais elementos.
            if (listaLisp.GetAllElements().Count == 1)
                return listaLisp;

            // faz o carregamento dos parametros para dentro da lista lisp de entrada.
            SubstituiParametros(listaLisp, parametros);

            object resultOperacao = null;
            // a próxima lista de índice 1, é a que tem o primeiro operando da operação matemática a ser realizada.

            
            // percorre as demais próximas listas, procurando operandos da operação matemática a ser realizada.
            for (int x = 0; x < listaLisp.Listas.Count; x++)
            {
                object operando1 = null;
                // verifica se a próxima lista de índice [x] é o nome de uma função lisp armazenada no repositório.
                if ((listaLisp.Listas[x].GetType() == typeof(ListaLISP)) && (RepositoryFunctionsLISP.Instance().IsFunctionStored(GetName((ListaLISP)listaLisp.Listas[x]))))
                {
                    // compõe a chamada de função, a ser avaliada.
                    ListaLISP lstResultUmOperando= AvaliaFuncao((ListaLISP)listaLisp.Listas[x]);
                    operando1 = lstResultUmOperando.Listas[0].valor;
                } // if
                else
                // verifica se a próxima lista de índice [x] é uma lista.
                if (listaLisp.Listas[x].GetType() == typeof(ListaLISP))
                {
                    // avalia um item da lista como se fosse uma lista a ser avaliada.
                    ListaLISP lstResultUmOperando = AvaliaFuncao((ListaLISP)listaLisp.Listas[x]);
                    // guarda o resultado da avaliação da lista- item.
                    operando1 = lstResultUmOperando.Listas[0].valor;
                } // if
                else
                // o átomo é o nome de uma função -lisp armazenada, retira o nome e a próxima lista (parâmetros de chamada), componto uma lista para avaliação da função.

                if (RepositoryFunctionsLISP.Instance().IsFunctionStored(listaLisp.Listas[x].nome))
                {

                    ListaLISP lstRsult = null;
                    if (listaLisp.Listas[x + 1].GetType() == typeof(ListaLISP))
                        lstRsult = AvaliaFuncao((ListaLISP)listaLisp.Listas[x + 1]);

                    string textoListaParametros = "";
                    if (lstRsult != null)
                        textoListaParametros = lstRsult.Listas[0].valor.ToString();
       
                    // compõe o texto contendo o nome da função lisp, e os parâmetros de chamada, que estão na próxima lista.
                    string textoChamada = "(" + listaLisp.Listas[x].nome.ToString() + " " + textoListaParametros + ")";
                    
                    ListaLISP lstParametroChamada = new ListaLISP(textoChamada);
                    ListaLISP lstChamaFuncaoDentroDaOperacaoMatematica = new FuncaoLISP().AvaliaFuncao(lstParametroChamada);
                    operando1 = lstChamaFuncaoDentroDaOperacaoMatematica.Listas[0].valor;
                    x++;
                }  
                else
                {

                    // a próxima lista de índice [x] é um Átomo, armazenando seu valor na variável operando.
                    operando1 = ObtemValorValido(listaLisp, x);
                } // else
                if (operando1 == null)
                    return NILL;

                // verifica se a próxima lista é o do primeiro operando (lista próxima 1).
                if (x == 0)
                    resultOperacao = operando1; // a lista próxima é do primeiro operando.
                else
                    resultOperacao = umaOperacaoMatematica(resultOperacao, operando1); // a lista próxima é dos demais operandos.
            } //for x

            ListaLISP lstResultadoOperacao = new ListaLISP();
            lstResultadoOperacao.Listas.Add(new LISP.Atomo(resultOperacao.ToString(), resultOperacao.ToString()));
            return lstResultadoOperacao;
        } // OperacaoMatematica()

        private static object ObtemValorValido(ListaLISP listaLisp, int x)
        {
            object operando1;
            if (!ListaLISP.IsNumber(listaLisp.Listas[x].valor))
                operando1 = listaLisp.Listas[x].nome;
            else
                operando1 = listaLisp.Listas[x].valor;
            return operando1;
        }


        /// <summary>
        /// compoe um texto para chamada a uma função lisp matemática armazenada no repositório.
        /// É preciso garantir que a função lisp armazenada retorne um objeto-número, pois a avaliação 
        /// é sobre uma função matemática.
        /// </summary>
        /// <param name="listaLisp">lista com o nome da função e uma lista de parâmetros (a lista seguinte ao lista contendo o nome).</param>
        /// <returns></returns>
        private  object CompoeChamadaAFuncao(ListaLISP listaLisp, List<DadosDoParametro> parametros)
        {
            ListaLISP resultOperacao = AvaliaFuncao(listaLisp);
            return resultOperacao.Listas[0].valor;
        }

        private static object GetOperandoCorrigido(ListaLISP dadoParametro)
        {
            string opCorrigido = dadoParametro.Listas[0].valor;

            if (opCorrigido == "")
                opCorrigido = dadoParametro.Listas[0].nome;
            return opCorrigido;
        } // GetOperandoCorrigido()



        private ListaLISP Subtracao(ListaLISP listaEntrada, List<DadosDoParametro> parametros)
        {
            return OperacaoMatematica(listaEntrada, SubtracaoGenerica, parametros);
        } // Adicao()

        private ListaLISP Adicao(ListaLISP listaEntrada, List<DadosDoParametro> parametros)
        {
            return OperacaoMatematica(listaEntrada, AdicaoGenerica, parametros);
        } // Adicao()

        private  ListaLISP Multiplicacao(ListaLISP listaComando, List<DadosDoParametro> parametros)
        {
            return OperacaoMatematica(listaComando, MultiplicacaoGenerica, parametros);
        } // Multiplicacao()

        private  ListaLISP Divisao(ListaLISP listaComando, List<DadosDoParametro> parametros)
        
        {
            return OperacaoMatematica(listaComando, DivisaoGenerica, parametros); 
        } // Divisao()

        private ListaLISP Cons(ListaLISP listaEntrada,  List<DadosDoParametro> parametros)
        {
            string tokenInstrucao =  listaEntrada.car().Listas[0].nome;
            SubstituiParametros(listaEntrada, parametros);
            
            if (tokenInstrucao == "cons")
            {
                // (cons (A B) (C D))
                ListaLISP lista1 = listaEntrada.GetEnesimaLista(2).GetCurrentList();
                ListaLISP lista2 = listaEntrada.GetEnesimaLista(3).GetCurrentList();

                if ((!lista1.isQuoted) && (IsListaLisp(lista1)))
                    lista1 = AvaliaFuncao(lista1);

                if ((!lista2.isQuoted) && (IsListaLisp(lista2)))
                    lista2 = AvaliaFuncao(lista2);
       
                ListaLISP lstSaida = new ListaLISP();
                foreach (Atomo at in lista1.Listas)
                    lstSaida.Listas.Add(at);
                foreach (Atomo at in lista2.Listas)
                    lstSaida.Listas.Add(at);
                return lstSaida;
            } //if
            return NILL;
        } // Cons()

        private  ListaLISP Atomo(ListaLISP listaComando, List<DadosDoParametro> parametros)
        {
            SubstituiParametros(listaComando, parametros);

            string nomeComado = GetName(listaComando);
            if (nomeComado != "atom")
                return NILL;
            if (listaComando.GetCurrentAtom().GetType() == typeof(Atomo))
                return T;
            else
                return NILL;
        } // Atomo()

        private static ListaLISP Member(ListaLISP listaEntrada, List<DadosDoParametro> parametro)
        {
            string tokenComando = GetName(listaEntrada);
            if (tokenComando != "member")
                return NILL;
            else
            {
                listaEntrada = listaEntrada.cdr();

                string nomeElemento = listaEntrada.GetCurrentAtom().nome;
                
                List<string> elementos = listaEntrada.GetAllElements();
                for (int x = 0; x < elementos.Count; x++)
                    if (elementos[x] == nomeElemento)
                        return T;
            } //if
            return NILL;
        } // Member()


        private static ListaLISP Setq(ListaLISP listaComando, List<DadosDoParametro> parametro)
        {
            string nomeComando = GetName(listaComando);
            if (nomeComando != "setq")
                return NILL;
            // carrega a variável a ser passada como parãmetro.
            string nomeVariavelASetar = listaComando.GetEnesimaLista(1).GetCurrentAtom().nome;
            // carrega o valor da variável a ser passada como parãmetro.
            string novoValorVariavel = listaComando.GetEnesimaLista(2).GetCurrentAtom().nome;
            // constrói o novo parâmetro.
            DadosDoParametro dadosVariavel = new DadosDoParametro(nomeVariavelASetar, novoValorVariavel);
            // adiciona o parãmetro novo para a lista de parametros.A idéia é que ao passa como parãmetro, dentrod de uma função lisp, poderá ser utilizado.
            parametro.Add(dadosVariavel);
            // retorna lista T (true).
            return T;
        } // Setqr()

        public static bool IsAtomo(ListaLISP lista)
        {
            string nomeLista = GetName(lista);
            if (RepositoryFunctionsLISP.Instance().IsFunctionStored(nomeLista))
                return false;
            foreach (string nomeOperacao in ComandosLISP.Keys)
            {
                if (nomeOperacao == nomeLista)
                    return false;
            } // foreach
            return true;
        } // IsAtomo()

        public static bool IsAtomo(string nomeAtomo)
        {
            if ((nomeAtomo != "(") && (nomeAtomo != ")"))
                return true;
            else
                return false;
        } // IsAtomo()

        /// <summary>
        /// adiciona um comando LISP, acionado por um nome.
        /// </summary>
        /// <param name="nomeComando">nome chave para chamar o comando.</param>
        /// <param name="funcaoComando">função que implementa o comando..</param>
        public void AddComando(string nomeComando, Comando funcaoComando)
        {
            Comando comandoTmp = null;
            ComandosLISP.TryGetValue(nomeComando, out comandoTmp);
            if (comandoTmp == null)
               ComandosLISP[nomeComando] = funcaoComando;
        } // AddComando()




        private static bool CondicionalIgual(object operando1, object operando2)
        {
            try
            {
                int op1 = int.Parse(operando1.ToString());
                int op2 = int.Parse(operando2.ToString());
                return (op1 == op2);
            } // try
            catch
            {
                try
                {
                    float op1 = float.Parse(operando1.ToString());
                    float op2 = float.Parse(operando2.ToString());
                    return (op1 == op2);
                } // try
                catch
                {
                    return false;
                } // cacth
            } // catch
        }



        private static bool CondicionalMaior(object operando1, object operando2)
        {
            try
            {
                int op1 = int.Parse(operando1.ToString());
                int op2 = int.Parse(operando2.ToString());
                return (op1 > op2);
            } // try
            catch
            {
                try
                {
                    float op1 = float.Parse(operando1.ToString());
                    float op2 = float.Parse(operando2.ToString());
                    return (op1 > op2);
                } // try
                catch
                {
                    return false;
                } // cacth
            } // catch
        }

        private static bool CondicionalMenor(object operando1, object operando2)
        {
            try
            {
                int op1 = int.Parse(operando1.ToString());
                int op2 = int.Parse(operando2.ToString());
                return (op1 < op2);
            } // try
            catch
            {
                try
                {
                    float op1 = float.Parse(operando1.ToString());
                    float op2 = float.Parse(operando2.ToString());
                    return (op1 < op2);
                } // try
                catch
                {
                    return false;
                } // cacth
            } // catch
        }

        private static bool CondicionalMaiorOuIgual(object operando1, object operando2)
        {
            try
            {
                int op1 = int.Parse(operando1.ToString());
                int op2 = int.Parse(operando2.ToString());
                return (op1 >= op2);
            } // try
            catch
            {
                try
                {
                    float op1 = float.Parse(operando1.ToString());
                    float op2 = float.Parse(operando2.ToString());
                    return (op1 >= op2);
                } // try
                catch
                {
                    return false;
                } // cacth
            } // catch
        }

        private static bool CondicionalMenorOuIgual(object operando1, object operando2)
        {
            try
            {
                int op1 = int.Parse(operando1.ToString());
                int op2 = int.Parse(operando2.ToString());
                return (op1 <= op2);
            } // try
            catch
            {
                try
                {
                    float op1 = float.Parse(operando1.ToString());
                    float op2 = float.Parse(operando2.ToString());
                    return (op1 <= op2);
                } // try
                catch
                {
                    return false;
                } // cacth
            } // catch
        }

        








        //**************************************************************************************************************

        public static object AdicaoGenerica(object p1, object p2)
        {
            try
            {
                int op1 = int.Parse(p1.ToString());
                int op2 = int.Parse(p2.ToString());
                return op1 + op2;
            }
            catch
            {
                float op1 = float.Parse(p1.ToString());
                float op2 = float.Parse(p2.ToString());
                return op1 + op2;
            }
        }


        private static object SubtracaoGenerica(object p1, object p2)
        {
            try
            {
                int op1 = int.Parse(p1.ToString());
                int op2 = int.Parse(p2.ToString());
                return op1 - op2;
            }
            catch
            {
                float op1 = float.Parse(p1.ToString());
                float op2 = float.Parse(p2.ToString());
                return op1 - op2;
            }
        }

        private static object MultiplicacaoGenerica(object p1, object p2)
        {
            try
            {
                int op1 = int.Parse(p1.ToString());
                int op2 = int.Parse(p2.ToString());
                return op1 * op2;
            }
            catch
            {
                float op1 = float.Parse(p1.ToString());
                float op2 = float.Parse(p2.ToString());
                return op1 * op2;
            }
        }

        private static object DivisaoGenerica(object p1, object p2)
        {
            try
            {
                int op1 = int.Parse(p1.ToString());
                int op2 = int.Parse(p2.ToString());
                return op1 / op2;
            }
            catch
            {
                float op1 = float.Parse(p1.ToString());
                float op2 = float.Parse(p2.ToString());
                return op1 / op2;
            }
        } // DivisãoGenérica()


        private static object IgualdadeGenerica(object p1, object p2)
        {
            try
            {
                int op1 = int.Parse(p1.ToString());
                int op2 = int.Parse(p2.ToString());
                return op1 == op2;
            } // try
            catch
            {
                float op1 = float.Parse(p1.ToString());
                float op2 = float.Parse(p2.ToString());
                return op1 == op2;
            } // catch

        }



        public override string ToString()
        {
            /*
            string strParametros = "(";
            for (int x = 0; x < parametrosDaFuncao.Count; x++)
                strParametros +=  parametrosDaFuncao[x].nome.ToString() + " ";
            strParametros = strParametros.Remove(strParametros.Length - 1);
            strParametros += ")";
            */
            string str = "(" + this.definicaoDaFuncao.ToString() + ")";
            return str;
        } //ToString()
    } // class FuncaoLISP


    public class RepositoryFunctionsLISP
    {
        // lista contendo as funções construidas, não pré-definidas.
        private static List<FuncaoLISP> repositorioListas = new List<FuncaoLISP>();

        // singleto da classe.
        private static RepositoryFunctionsLISP repositorioListasSingleton = null;


        public static RepositoryFunctionsLISP Instance()
        {
            if (repositorioListasSingleton == null)
                repositorioListasSingleton = new RepositoryFunctionsLISP();
            return repositorioListasSingleton;
        } // Instance()

        /// <summary>
        /// restaura os dados da função, caso haja vazamento por referências envolvendo funções.
        /// </summary>
        /// <param name="nomeFuncao">nome da função.</param>
        public void RestauraFuncao(string nomeFuncao)
        {
            for (int x = 0; x < repositorioListas.Count; x++)
            {
                if (repositorioListas[x].nomeFuncao == nomeFuncao)
                {
                    repositorioListas[x] = new FuncaoLISP(repositorioListas[x].textoFuncao);
                } // if
            } // for 
        }

        public List<FuncaoLISP> GetFunctions()
        {
            return repositorioListas.ToList<FuncaoLISP>();
        } // GetFunctions()

        /// <summary>
        /// verifica se a função do nome de entrada está no repositório.
        /// </summary>
        /// <param name="nomeFuncao">nome da função a se verificar.</param>
        /// <returns></returns>
        public bool IsFunctionStored(string nomeFuncao)
        {
            foreach (FuncaoLISP umaFuncao in repositorioListas)
            {
                if (umaFuncao.nomeFuncao == nomeFuncao)
                    return true;
            } //foreach
            return false;

        } // IsFuncaoArmazenada()

        /// <summary>
        /// Encontra uma função armazenada no repositório.
        /// Se não estiver, retorna null;
        /// </summary>
        /// <param name="nomeFuncao">nome da função a ser procurada.</param>
        /// <returns></returns>
        public FuncaoLISP FindFunction(string nomeFuncao)
        {
            foreach(FuncaoLISP umaFuncao in repositorioListas)
            {
                if (umaFuncao.nomeFuncao == nomeFuncao)
                    return new FuncaoLISP(umaFuncao.definicaoDaFuncao);
            } //foreach
            return null;
        } // EncontraFuncaoNoRepositorio()
        /// <summary>
        /// adiciona uma função lisp ao repositorio de funções.
        /// </summary>
        /// <param name="umaFuncaoLisp">função a ser registrada no repositório.</param>
        public void AddFuncion(FuncaoLISP umaFuncaoLisp)
        {
            repositorioListas.Add(umaFuncaoLisp);
        }// AddFuncaoLisp()

        /// <summary>
        /// Grava em Arquivo o texto de uma função.
        /// </summary>
        /// <param name="funcao"></param>
        public void SaveInFile(FuncaoLISP funcao)
        {
            FileStream stream = new FileStream(funcao.nomeFuncao + ".txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(funcao.textoFuncao);
            writer.Close();
            stream.Close();
            writer.Dispose();
            stream.Dispose();
        }//WriteInFile()

        /// <summary>
        /// Lê de um Arquivo um função.
        /// </summary>
        /// <param name="nomeFuncao">nome da função a ser lida.</param>
        /// <returns>retorna uma função lisp como o nome de entrada.</returns>
        public FuncaoLISP ReadInFile(string nomeFuncao)
        {
            try
            {
                FileStream stream = new FileStream(nomeFuncao + ".txt", FileMode.Open, FileAccess.Write);
                StreamReader reader = new StreamReader(stream);
                string textoFuncao = reader.ReadLine();
                FuncaoLISP funcaoSaida = new FuncaoLISP(textoFuncao);
                reader.Close();
                stream.Close();
                reader.Dispose();
                stream.Dispose();
                return funcaoSaida;
            } // try
            catch
            {
                return new FuncaoLISP(FuncaoLISP.NILL);
            } // catch
        } // ReadInFile()
    } // class RepositorioListasLISP

    public struct DadosDoParametro
    {
        public string nome { get; set; }
        public object valor { get; set; }
        public DadosDoParametro(string name, object value)
        {
            this.nome = name;
            this.valor = value;
        } // DadoDoParametro()

        public override string ToString()
        {
            return "nome: " + this.nome + "  valor: " + this.valor;
        }//ToString()

    } // struct DadosDoParametro

}  // namespace
