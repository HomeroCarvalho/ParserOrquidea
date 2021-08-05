using System.Collections.Generic;
using System.Linq;
using System;
namespace parser.PROLOG
{

    public class ListaProlog : Predicado
    {
        ///  MODULO LISTA. Uma lista é um Predicado. Contém átomos de um predicado, elementos da lista, 
        ///  estrutura interna de árvore para elementos, e especificação de listas, como [X|Y].
        ///  
        /// 
        ///  FUNCIONALIDADES:
        ///  1-  Lista(): construtor. Aceita um texto com a descrição de um predicado lista, como: Homem([X|Y],0,B).
        ///  2-  EspecificacoesDaLista().Aceita texto, como [X|Y], [X Y|Z], [X Y Z], e guarda items como Head, Tail, e Element.
        ///  3-  ReconstroiRegra().: Modifica os Predicado Base e Predicados Meta, colocando especificações da lista nos seus átomos.
        ///  4-  Append(): une duas listas, copiando para uma nova lista a união de duas listas.
        ///  5-  AplicaUmaListaFuncao(): método cruscial para o processamento de listas, aplica uma função como GetHead(),GetTail(),Append() F() ,
        ///      em uma  lista variável (X),resultando em F(X).
        ///  6-  HasList(): verifica se um predicado contém especificação de listas, em seus átomos.
        ///  7-  ConstroiLista(): recebe um vetor de elementos (texto), e constrói uma árvore lista, contendo os elementos da lista.
        ///  8-  GetAllElements(): retorna todos elemento guardado na árvore da lista.
        ///  9-  CopiaNos(): retorna uma cópia de todos nós da árvore da lista.
        ///  10- IsMember(): verifica se um elemento está nos elementos da lista.
        ///  11- GetElementsHead(): retorna os elementos da cabeça da lista. (uma lista pode ter mais de um elemento cabeça).
        ///  12- GetElementsTail(): retorna os elementos da cauda da lista.
        ///  13- car(): retorna uma lista formada com os elementos da cabeça da lista.
        ///  14- cdr(): retorna uma lista formada com os elementos da cauda da lista.
        ///  15- ToString(): retorna um texto contendo inforrmações da lista. Bastante utilizada para
        ///      visualizar a lista quando na escrita no Console da Tela.
        ///      
        /// 
        ///  PROPRIEDADES:
        ///  1- ArvoreDaLista: contém numa árvore binária modificada todos elementos da lista. Útil para se obter listas como ListHead() e ListTail().
        ///  2- Especificacoes: contém especificações da lista, como [X|Y], [X Y], [X Y Z|W].
        ///  3- PartesListaVariaveis: contém variáveis num dicionário, representando especificações da lista.



        private TreeListProlog ArvoreDaLista { get; set; }
        private string Especificacoes { get; set; }
        /// <summary>
        /// contém variáveis de especificação de lista, como #0#Head=X, em [X|Y].
        /// </summary>
        //public List<Dictionary<string, string>> PartesLista { get; set; }
        /// <summary>
        /// contém variáveis de especificação de lista, como X = #0#Head, em [X|Y].
        /// </summary>
        public List<Dictionary<string,string>> PartesListaVariaveis { get; set; }
        /// <summary>
        /// construtor. Retira de  um texto predicado as especificações de uma lista.
        /// </summary>
        /// <param name="textoLista">texto com a descrição da lista.</param>
        public ListaProlog(string textoLista)
        {
            this.Especificacoes = textoLista;
            this.PartesListaVariaveis = new List<Dictionary<string, string>>();
            // guarda o texto que contém as especificações da lista.
            string textoEspecificacoes = (string)textoLista.Clone();
            //obtém o predicado que contém a lista.
            Predicado predicadoDaLista = Predicado.GetPredicado(textoLista);
            if (textoLista.IndexOf("(") != -1)
                // retira o nome da lista, como o nome do predicado.
                this.Nome = textoLista.Substring(0, textoLista.IndexOf("("));
            else
                this.Nome = "";
            // retira os átomos da lista, como os átomos do predicado.
            this.GetAtomos().AddRange(predicadoDaLista.GetAtomos());

            // constroi uma estrutura de dados que guarda os elementos da lista.
            this.ArvoreDaLista = new TreeListProlog();
            this.ArvoreDaLista.root = new Node("");

            // retira os elementos da lista que não são variáveis.
            if (this.Especificacoes.IndexOf("[") != -1)
            {
                List<string> lstElementosSomenteDados = new List<string>();
                // retira o nome da lista.
                string nomeLista = this.Especificacoes.Substring(0, this.Especificacoes.IndexOf("["));
                // retira os dados da lista, como [1 3 5].Retira os operadores "[" e "]", deixando os elementos separados por espaços vazios.
                string dadosLista = this.Especificacoes.Substring(Especificacoes.IndexOf("[") + 1, Especificacoes.IndexOf("]") - Especificacoes.IndexOf("[")-1);
                // quebra o conjunto de dados em dados separados por vazios.
                string[] elementosLista = dadosLista.Split(new String[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                // insere elementos numa lista de textos, se o elemento não for uma variável (primeira letra maiúscula do elemento).
                for (int x = 0; x < elementosLista.Length; x++)
                {
                    if (!Predicado.IsVariavel(elementosLista[x]))
                        lstElementosSomenteDados.Add(elementosLista[x]);
                }// for x
                // constroi a lista com dados da lista de elementos não variáveis.
                this.ConstroiLista(nomeLista, lstElementosSomenteDados.ToArray());
            } // if

            // inicializa a estrutura que guarda especificações da lista, como elementos head elementos tail.
          
            int atomo = 0;
            while (atomo < this.GetAtomos().Count)
            {
                if (HasList(this.GetAtomos()[atomo]))
                {
                    // se o átomo currente tiver especificações de lista, retira as especificações de lista do átomo.
                    EspecificacoesDaLista(this.GetAtomos()[atomo]);
                } // if
                atomo++;
            } // while
        } // Lista()
        /// <summary>
        /// processa os elementos representativos da lista, e os coloca em [PartesListaVariaveis].
        /// </summary>
        /// <param name="textoEspecificacao">texto, como [X|Y], [X Y|Z], [X Y Z].</param>
        /// <returns></returns>
        private void EspecificacoesDaLista(string textoEspecificacao)
        {
            if (!textoEspecificacao.Contains("[") || (!textoEspecificacao.Contains("]")))
                return;
            // cumprimento do texto que especifica a lista.
            int lenghtTextoEspecificacao = textoEspecificacao.IndexOf("]") - textoEspecificacao.IndexOf("[") + 1;
            // retira o texto que especifica a lista.
            textoEspecificacao = textoEspecificacao.Substring(textoEspecificacao.IndexOf("["), lenghtTextoEspecificacao);


            PartesListaVariaveis.Add(new Dictionary<string, string>());

            // salva a especificação da lista, para posterior extração dos elementos da lista.
            string especificacaoCopia = (string)textoEspecificacao.Clone();

            if (textoEspecificacao.Contains("|"))
            {
                // obtém o elemento cabeça da lista.
                string elementoCabeca = textoEspecificacao.Substring(textoEspecificacao.IndexOf("[") + 1, textoEspecificacao.IndexOf("|") - 1);
                string[] elementsHead = elementoCabeca.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if ((elementsHead != null) || (elementsHead.Length > 0))
                    elementoCabeca = elementsHead[elementsHead.Length - 1];


                string elementoCauda = textoEspecificacao.Substring(textoEspecificacao.IndexOf("|") + 1, textoEspecificacao.IndexOf("]") - 1 - textoEspecificacao.IndexOf("|"));
                elementoCauda = elementoCauda.Replace("]", "");

              
                // adiciona a variável Head ou Tail para partes da lista.
                PartesListaVariaveis[PartesListaVariaveis.Count - 1][elementoCabeca] = "#" + (PartesListaVariaveis.Count - 1).ToString() + "#" + "Head";
                PartesListaVariaveis[PartesListaVariaveis.Count - 1][elementoCauda] = "#" + (PartesListaVariaveis.Count - 1).ToString() + "#" + "Tail";


                // remove do texto de entrada os elementos cauda e cabeça da lista.
                textoEspecificacao = textoEspecificacao.Replace(elementoCabeca, "").Replace(elementoCauda, "");
            } // if texto
            // restaura as especificação da lista, para extrair os elementos da lista, e posição da lista.
            textoEspecificacao = especificacaoCopia;

            string[] elementosDaLista = textoEspecificacao.Split(new string[] { "[", "]", " " }, StringSplitOptions.RemoveEmptyEntries);

            if (elementosDaLista[0] != textoEspecificacao)
            {
                // consegue os elementos da lista, dentro da especificção de listas de entrada, no construtor.
                // o elemento não pode ser uma variável (com letra maiúscula no inicio do texto do elemento.
                for (int x = 0; x < elementosDaLista.Length; x++)
                {

                    if ((!elementosDaLista[x].Contains("|")) && (!Predicado.IsVariavel(elementosDaLista[x])))
                    {
                        PartesListaVariaveis[PartesListaVariaveis.Count - 1].Add("#" + (PartesListaVariaveis.Count - 1).ToString() + "#" + "Elemento#" + x.ToString(), elementosDaLista[x]);
                    } // if
                } //for x
            } // if elementosDaLista
        } // EspecificacoesDaLista()
        /// <summary>
        /// reconstroi as listas, acrescentando tags como: Head, Tail, Element, a partir das definições de lista em predicado base,
        /// colocando a especificações de lista nos predicados meta prontos para ser processados.
        /// </summary>
        /// <param name="listaPredicadoBase">lista com as especificações a serem inseridas.</param>
        /// <param name="predicadosMetas">predicados com elmentos de lista, a ser processada.</param>
        public void ReconstroiRegra(ListaProlog listaPredicadoBase, List<Predicado> predicadosMetas)
        {
            List<string[]> especificacaoListaPredicadoBase = new List<string[]>();

            foreach (Dictionary<string, string> umaParteLista in listaPredicadoBase.PartesListaVariaveis)
            {
                foreach (KeyValuePair<string, string> variaveisParteLista in umaParteLista)
                {
                    especificacaoListaPredicadoBase.Add(new string[] { variaveisParteLista.Key, variaveisParteLista.Value });
                } // foreach
            } // foreach

            // percorre todos predicados meta (goal), retirando variáveis do predicado currente, e verificando se é uma variável Head ou Tail.
            for (int meta = 0; meta < predicadosMetas.Count; meta++)
            {
                List<string[]> variaveisPredicadoMeta = new List<string[]>();
                List<string> especificacaoUmaListaMeta = predicadosMetas[meta].RetiraVariaveis();
                for (int varsBase = 0; varsBase < especificacaoListaPredicadoBase.Count; varsBase++)
                {
                    string umaVarBase = especificacaoListaPredicadoBase[varsBase][0];
                    for (int umaParte = 0; umaParte < PartesListaVariaveis.Count; umaParte++)
                    {
                        string nomeLista = "#" + (umaParte).ToString() + "#";
                        string valueHeadVarBase = null;
                        PartesListaVariaveis[umaParte].TryGetValue(umaVarBase, out valueHeadVarBase);
                        if ((valueHeadVarBase!=null) &&  (valueHeadVarBase.Contains("Head")))
                        {
                            for (int varsLista = 0; varsLista < especificacaoUmaListaMeta.Count; varsLista++)
                            {
                                if (especificacaoUmaListaMeta[varsLista] == especificacaoListaPredicadoBase[varsBase][0])
                                {
                                    string novoValorVariavel = nomeLista+"(" + especificacaoUmaListaMeta[varsLista][0] + ":" + "Head" + ")";
                                    predicadosMetas[meta].GetAtomos().Remove(especificacaoUmaListaMeta[varsLista]);
                                    predicadosMetas[meta].GetAtomos().Add(novoValorVariavel);
                                }// if
                            } //for varLista
                        } // if
                        string valueTailVarBase = null;
                        PartesListaVariaveis[umaParte].TryGetValue(umaVarBase, out valueTailVarBase);
                        if ((valueTailVarBase!=null) && (valueTailVarBase.Contains("Tail")))
                          {
                            
                            for (int varsLista = 0; varsLista < especificacaoUmaListaMeta.Count; varsLista++)
                            {
                                if (especificacaoUmaListaMeta[varsLista] == especificacaoListaPredicadoBase[varsBase][0])
                                {
                                    string novoValorVariavel = nomeLista + "(" + especificacaoUmaListaMeta[varsLista] + ":" + "Tail" + ")";
                                    predicadosMetas[meta].GetAtomos().Remove(especificacaoUmaListaMeta[varsLista]);
                                    predicadosMetas[meta].GetAtomos().Add(novoValorVariavel);
                                } // if
                            } //for varLista
                        }//if
                    } // for umaParte
                } //for varsBase
                listaPredicadoBase.ReconstroiLista(variaveisPredicadoMeta, predicadosMetas[meta]);
            } // for meta
        }// EquilibraAtomosLista()
         /// <summary>
        /// une duas listas, copiando seus elementos para uma nova lista.
        /// </summary>
        /// <param name="nome">nome da nova lista.</param>
        /// <param name="l1">lista 1 a ser unida.></param>
        /// <param name="l2">lista 2 a ser unida.</param>
        /// <returns></returns>
        private static ListaProlog Append(string nome, ListaProlog l1, ListaProlog l2)
        {
            ListaProlog lResult = new ListaProlog(l1.Especificacoes);
            lResult.Nome = l1.Nome + l2.Nome;
            List<string> elementos = l1.GetElementsHead();
            elementos.AddRange(l1.GetElementsTail());
            elementos.AddRange(l2.GetElementsHead());
            elementos.AddRange(l2.GetElementsTail());
            lResult.ConstroiLista(lResult.Nome, elementos.ToArray());
            return lResult;
        } // Merge()
        /// <summary>
        /// Aplica uma função (GetHead(),GetTail(), Append(Head,Tail), Append(Tail,Head))
        /// especificada numa lista funcao [listaFuncao]: F(X) a uma variável [listaVariavel]: (X).
        /// </summary>
        /// <param name="listaFuncao">lista molde, contém as especificações da função lista.</param>
        /// <param name="listaVariavel">lista contendo a variável a ser aplicada a lista função.</param>
        /// <returns>retorna um lista resultante de especificações como: Head(), Tail().</returns>
        public static ListaProlog AplicaUmaListaFuncao(ListaProlog listaFuncao, ListaProlog listaVariavel)
        {
            for (int umaParteLista = 0; umaParteLista < listaFuncao.PartesListaVariaveis.Count; umaParteLista++) 
            {
                for (int atomo = 0; atomo < listaFuncao.GetAtomos().Count; atomo++)
                {
                    string valueHead = null;
                    listaFuncao.PartesListaVariaveis[umaParteLista].TryGetValue(listaFuncao.GetAtomos()[atomo], out valueHead);

                    if ((valueHead != null) && (valueHead.Contains("Head"))) 
                    {
                        return listaVariavel.car();
                    } // if
                    string valueTail = null;
                    listaFuncao.PartesListaVariaveis[umaParteLista].TryGetValue(listaFuncao.GetAtomos()[atomo], out valueTail);
                    if ((valueTail!=null) && (valueTail.Contains("Tail")))
                    {
                        return listaVariavel.cdr();
                    } // if

                    // trata da função append, que une duas partes da lista, como X|Y, sem operadores [ e ].
                    string mergeLista = listaFuncao.GetAtomos()[atomo]; // variável que guarda a especificações para append.
                    if ((mergeLista.Contains("|")) && (!mergeLista.Contains("[")) && (!mergeLista.Contains("]")))
                    {
                        // separa os dois elementos necessários para o append.
                        string[] variaveisLista = mergeLista.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        // obtém um possível valor para a chave [variaveisLista[0]].
                        string valueMerge1 = null;
                        listaFuncao.PartesListaVariaveis[umaParteLista].TryGetValue(variaveisLista[0], out valueMerge1);
                        // obtém um possívl valor para a chave [variaveisLista[1]].
                        string valueMerge2 = null; 
                        listaFuncao.PartesListaVariaveis[umaParteLista].TryGetValue(variaveisLista[1], out valueMerge2);
                        // merge1 contém a especificação para um valor que contém Head ou Tail.
                        // merge2 contém a especificação para um valor que contém Head ou Tail.
                        if ((valueMerge1.Contains("Head")) && (valueMerge1.Contains("Head")))
                            return Append(listaVariavel.Nome, listaVariavel.car(), listaVariavel.car());
                        if ((valueMerge1.Contains("Head")) && (valueMerge2.Contains("Tail")))
                            return Append(listaVariavel.Nome, listaVariavel.car(), listaVariavel.cdr());
                        if ((valueMerge1.Contains("Tail")) && (valueMerge2.Contains("Head")))
                            return Append(listaVariavel.Nome, listaVariavel.cdr(), listaVariavel.car());
                        if ((valueMerge1.Contains("Tail")) && (valueMerge2.Contains("Tail")))
                            return Append(listaVariavel.Nome, listaVariavel.cdr(), listaVariavel.cdr());
                    } //if
                } // for atomo
            } // for umaParteLista
            return null;
        } //AplicaUmaListaFuncao()
        public static bool HasList(Predicado p)
        {
            if (p.GetAtomos().Count == 0)
                return true;
            for (int umAtomo = 0; umAtomo < p.GetAtomos().Count; umAtomo++)
            {
                if (HasList(p.GetAtomos()[umAtomo]))
                    return true;
            } //for umAtomo
            return false;
        } // IsLista()
        public static bool HasList(string atomoPredicado)
        {
            return (atomoPredicado.Contains("[") && (atomoPredicado.Contains("]")));
        } // IsLista()
        /// <summary>
        /// constrói a lista com  os elementos de entrada, numa árvore binária com elementos com valor à esquerda, e ligação com nós a direita.
        /// </summary>
        /// <param name="nomeLista"></param>
        /// <param name="items"></param>
        public void ConstroiLista(string nomeLista,params string[] items)
        {
            ArvoreDaLista.root = new Node("raiz");    
            this.Nome = nomeLista;
            Node noCurrent = new Node(ArvoreDaLista.root);
            for (int x = 0; x < items.Length; x++)
            {
                this.ArvoreDaLista.Insert(items[x], noCurrent);
            } // for x
            ArvoreDaLista.root = noCurrent;
         }// ConstroiLista()
        public List<string> GetAllElements()
        {
            List<string> elementos = new List<string>();
            if (this.GetElementsHead() != null)
                elementos.AddRange(this.GetElementsHead());
            if (this.GetElementsTail() != null)
                elementos.AddRange(this.GetElementsTail());
            return elementos;
        } //GetAllElemnts()
         /// <summary>
         /// retorna uma cópia dos nós da lista.
         /// </summary>
         /// <param name="no"></param>
         /// <returns></returns>
        private Node CopiaNos(Node no)
        {
            if (no != null)
            {
                return new Node(no.Key, CopiaNos(no.Left), CopiaNos(no.Right));
            }// if no.
            else
                return null;
        }//CopiaNos()
        /// <summary>
        /// predicado para consultar se um elemento está numa lista.
        /// </summary>
        /// <param name="umElemento">elemento a procurar.</param>
        /// <param name="listaEntrada">lista a ser consultada.</param>
        /// <returns>[true] se  [umElmento] é membro da lista [listaEntrada]. [false], se não.</returns>
        public static bool IsMember(string umElemento, ListaProlog listaEntrada)
        {
            if (listaEntrada.cdr() == null)
                return (umElemento.Equals(listaEntrada.GetElementsHead()[0]));
            else
                return (IsMember(umElemento, listaEntrada.cdr()));
        } // IsMember()
        /// <summary>
        /// retorna os elementos da cabeça da lista.
        /// </summary>
        /// <returns>retorna uma lista de textos, os elementos da cabeça da lista.</returns>
        public List<string> GetElementsHead()
        {
            Node no = new Node(this.ArvoreDaLista.root);
            List<string> elements = new List<string>();
            while (no.Left != null)
            {
                elements.Add(no.Left.Key);
                no = no.Left;
            } // while()
            return elements;
        } // Head()
        /// <summary>
        /// retorna os elementos da cauda da lista.
        /// </summary>
        /// <returns>retorna uma lista de textos, os elementos da cauda da lista.</returns>
        public List<string> GetElementsTail()
        {
            List<string> elementsTail = new List<string>();
            Node noTail = this.ArvoreDaLista.root.Right;
            while (noTail != null)
            {
                if (noTail.Left != null)
                    elementsTail.Add(noTail.Left.Key);
                // percorre a árvore, acessando os elementos de suporte da lista (faz a relação head-tail da lista).
                noTail = noTail.Right;
            } // while
            return elementsTail;
        } // Tail()
        public ListaProlog car()
        {
            List<string> elementosCabeca = this.GetElementsHead();
            ListaProlog lstCabeca = new ListaProlog("");
            lstCabeca.GetAllElements().Clear();
            lstCabeca.ConstroiLista(this.Nome, elementosCabeca.ToArray());
            return lstCabeca;
        } // GetListaCabeca()
        public ListaProlog cdr()
        {
            List<string> elementos = this.GetElementsTail();
            ListaProlog lstCauda = new ListaProlog(this.Especificacoes);
            lstCauda.GetAllElements().Clear();
            lstCauda.ConstroiLista(this.Nome, elementos.ToArray());
            return lstCauda;
        } //GetListaCauda()
       
        /// <summary>
        /// Reconstroi um predicado, a partir de variáveis extraídas anteriormente.
        /// </summary>
        /// <param name="variaveis">variáveis, contendo: [0]: nome da variável [1]: novo valor da variável.</param>
        /// <param name="p">predicado da lista a ser modificada.</param>
        private void ReconstroiLista(List<string[]> variaveis, Predicado p)
        {
            string s = "";

            for (int umAtomo = 0; umAtomo < p.GetAtomos().Count; umAtomo++)
            {
                s = "";
                for (int x = 0; x < variaveis.Count; x++)
                {
                    if (p.GetAtomos()[umAtomo].Contains(variaveis[x][0]))
                        s += "(" + variaveis[x][0] + ":" + variaveis[x][1] + ") ";
                } // for x
                if (s != "")
                    p.GetAtomos()[umAtomo] = s;
            } // for umAtomo
        } // ReconstroiLista()


        public override string ToString()
        {
            string str = "";
            str = "nome: "+this.Nome;
            str += " ";
            str += "elementos: [";
            for (int x = 0; x < this.GetAllElements().Count; x++)
                str += " " + this.GetAllElements()[x];
            str += " ]";
            return str;
        } // ToString()
    } // class Lista
    
    public class TreeListProlog
    {
        public Node root = new Node("");

        /// <summary>
        /// insere o nó folha com conteúdo à esquerda do novo nó à direita, conforme especificações da Lista em Prolog.
        /// </summary>
        /// <param name="aKey">conteúdo do novo nó à esquerda.</param>
        /// <param name="no">nó raiz, para inserir  o novo nó à direita e deste nó à direita, inserir um nó a esquerda.</param>
        public Node Insert(string aKey, Node no)
        {
            if (no == null)
            {
                Node noValor = new Node(aKey);
                return noValor;
            } // if
            if (no.Right != null) 
            {
                Node noDireita = Insert(aKey, no.Right);
                return no;
            } // else
            no.Right = new Node("100");
            no.Left = new Node(aKey);
            return no;
        }// Insert()


        private void Remove(string aKey, Node no)
        {
            if (no == null)
                return;
            if (no.Left.Key==aKey)
            {
                no.Left = null;
            } // if
            else
            {
                Remove(aKey, no.Right);
            } // else
        } // Remove()

        /// <summary>
        /// caminha pela árvore, guardando os valores de cada nó
        /// numa ordem pós-ordem.
        /// </summary>
        /// <param name="no">nó currente.</param>
        public List<string> PosOrder(Node no)
        {
            List<string> valores = new List<string>();
            if (no!=null)
            {
                valores.AddRange(PosOrder(no.Left));
                valores.AddRange(PosOrder(no.Right));
                valores.Add(no.Key);
            } // if
            return valores;
        } // PosOrder()
        /// <summary>
        /// caminha pela árvore, guardando valores de cada nó numa ordem em in-order.
        /// </summary>
        /// <param name="no">nó  currente</param:
        /// <returns></returns>
        public List<string> InOrder(Node no)
        {
            List<string> valores = new List<string>();
            if (no != null)
            {
                valores.AddRange(InOrder(no.Left));
                valores.Add(no.Key);
                valores.AddRange(InOrder(no.Right));
            } // if
            return valores;
        } // PosOrder()
    } // class TreelistProlog


    public class Node
    {
        public string Key { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(string akey)
        {
            this.Key = akey;
            this.Left = null;
            this.Right = null;
        }
        public Node(string aKey, Node leftNode, Node rightNode)
        {
            this.Key = aKey;
            this.Left = leftNode;
            this.Right = rightNode;
        } // Node()

        public Node(Node no)
        {
            this.Key = no.Key;
            this.Left = no.Left;
            this.Right = no.Right;
        } //Node()
    } // class Node
} // namespace
