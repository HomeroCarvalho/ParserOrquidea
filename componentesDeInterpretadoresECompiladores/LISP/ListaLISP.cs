using System;
using System.Collections.Generic;
using System.Linq;

namespace parser.LISP
{

    public class Atomo
    {
        public string valor { get; set; }
        public string nome { get; set; }

        public Atomo(string name, string value)
        {
            this.valor = value;
            this.nome = name;
        }
        public override string ToString()
        {
            return ("nome: " + this.nome + "  valor: " + this.valor);
        }

    } // class Atomo



    public partial class ListaLISP : Atomo
    {
        /// <summary>
        /// próximas lista guarda os dados inclusive da currente lista, e também das próximas listas (listas ou atomos).
        /// </summary>
        public List<Atomo> Listas { get; set; }
        public bool isQuoted = false;

        public ListaLISP(string textoLista) : base("", "")
        {
            ListaLISP listaSaida = ListaLISP.GetLista(textoLista);
            this.Listas = listaSaida.Listas;
            ExtraiNome(listaSaida);
            this.isQuoted = listaSaida.isQuoted;
        } // ListaLISP()

        private void ExtraiNome(ListaLISP listaSaida)
        {
            if (listaSaida.Listas.Count == 0)
                return;
            if (listaSaida.Listas[0].nome != "defun")
                this.nome = listaSaida.Listas[0].nome;
            if (listaSaida.Listas[0].nome == "defun")
                this.nome = listaSaida.Listas[1].nome;
        }// ExtraiNome()

        public ListaLISP() : base("", "")
        {
            this.Listas = new List<Atomo>();
        } //ListaLISP()

        public ListaLISP(ListaLISP lista) : base("", "")
        {
            this.Listas = lista.Listas.ToList<Atomo>();
            this.isQuoted = lista.isQuoted;
            this.ExtraiNome(lista);
        } // ListaLISP()

        public ListaLISP(Atomo elemento) : base("", "")
        {
            this.Listas = new List<Atomo>();
            this.isQuoted = false;
            this.nome = elemento.nome;
            this.Listas.Add(elemento);
        } // ListaLISP()

        public static ListaLISP GetLista(string textolista)
        {
            List<string> elementos = GetTokensLISP(textolista);

            ListaLISP[] listaCurrente = new ListaLISP[elementos.Count];

            // seta o objeto que percorrerá todas listas currentes, adicionando à lista de saída Atomos e ListaLISP.
            ListaLISP listaPonteiro = listaCurrente[0];

            int ponteiroListas = 0;
            bool isQuoteNext = false;
            for (int x = 0; x < elementos.Count; x++)
            {
                if (elementos[x] == "'")
                {
                    /// aplica o operador quote para a próxima lista.
                    isQuoteNext = true;
                }// if
                else
                if (elementos[x] == "(")
                {
                    ponteiroListas++;
                    listaCurrente[ponteiroListas] = new ListaLISP();

                    // verifica se é para cotar a lista currente.
                    if (isQuoteNext)
                    {
                        // aplica regras quote para a lista currente.
                        listaCurrente[ponteiroListas].isQuoted = true;
                        isQuoteNext = false;
                    } // if
                    if ((x >= 0) && (listaCurrente[ponteiroListas - 1] != null))
                        listaCurrente[ponteiroListas - 1].Listas.Add((ListaLISP)listaCurrente[ponteiroListas]);
                    else
                    {
                        listaCurrente[ponteiroListas - 1] = new ListaLISP(listaCurrente[ponteiroListas]);
                        listaPonteiro = listaCurrente[ponteiroListas];
                    } // else

                } // if  
                else
                if (elementos[x] == ")")
                {
                    ponteiroListas--;
                }// if
                else
                {
                    if (FuncaoLISP.IsAtomo(elementos[x]))
                    {
                        if (listaCurrente[ponteiroListas] == null)
                            listaCurrente[ponteiroListas] = new ListaLISP();
                        listaCurrente[ponteiroListas].Listas.Add(new Atomo(elementos[x], elementos[x]));
                    }// if
                } // else
            } // for x
            if (listaPonteiro != null)
            {
                return listaPonteiro;
            }
            else
                return listaCurrente[0];
        } // GetLista()

        public static bool IsNumber(string token)
        {
            try
            {
                int isNumberInt = int.Parse(token);
                return true;
            } //try
            catch
            {
                try
                {
                    float isNumberFloat = float.Parse(token);
                    return true;
                } //try
                catch
                {
                    return false;
                }  // catch
            } //catch
        } // IsNumber()

        public string EscreveLista()
        {
            string textoLista = this.ToString();
            System.Console.WriteLine(textoLista);
            return textoLista;
        } // EscreveLista().

        /// <summary>
        /// retorna os elementos ou elemento do primeiro elemento de uma lista lisp.
        /// </summary>
        /// <returns></returns>
        public List<string> GetElementsHead()
        {
            Atomo at = (Atomo)this.Listas[0];
            // se o primeiro elemento for uma lista, retorna os elementos desta lista.
            if (this.Listas[0].GetType() == typeof(ListaLISP))
                return ((ListaLISP)this.Listas[0]).GetAllElements();
            else
                return null;
        }// GetElementsHead(

        /// <summary>
        /// retorna os elementos da cauda de uma lista lisp.
        /// </summary>
        /// <returns></returns>
        public List<string> GetELementsTail()
        {
            ListaLISP listaTail = new ListaLISP(this);
            listaTail.Listas.RemoveAt(0);
            List<string> elementosCauda = new List<string>();
            for (int umElemento = 0; umElemento < listaTail.Listas.Count; umElemento++)
            {
                // se o elemento for um atomo, adiciona o atomo.
                if (listaTail.Listas[umElemento].GetType() == typeof(Atomo))
                    elementosCauda.Add(((Atomo)listaTail.Listas[umElemento]).valor);
                // se o elemento for uma lista, adiciona todos seus elementos.
                if (listaTail.Listas[umElemento].GetType() == typeof(ListaLISP))
                    elementosCauda.AddRange(listaTail.GetAllElements());
            } // for umAtomo
            return elementosCauda;
        } // GetELementsTail()

        /// <summary>
        /// retorna o primeiro elemento de uma lista, se for um atomo ou uma lista lisp, numa lista própria.
        /// </summary>
        /// <returns></returns>
        public ListaLISP car()
        {
            // se a lista for vazia, ou não contém nenhum elemento, retorna NILL.
            if ((this.Listas == null) || (this.Listas.Count == 0))
                return FuncaoLISP.NILL;

            // se o primeiro elemento for uma lista, retorna esta lista.
            if (this.Listas[0].GetType() == typeof(ListaLISP))
            {
                ListaLISP listaCar = new ListaLISP((ListaLISP)this.Listas[0]);
                return listaCar;
            } // if

            // se o primeiro elemento for um atomo, retorna este atomo, como o elemento da 01a. próxima lista.
            if (this.Listas[0].GetType() == typeof(Atomo))
            {
                ListaLISP listaHead = new ListaLISP();
                listaHead.Listas.Add(Listas[0]);
                return listaHead;
            } // if
            return FuncaoLISP.NILL;
        } // car()

        /// <summary>
        /// retorna a lista cauda de uma lista, retirando o primeiro elemento da lista, numa lista cópia.
        /// </summary>
        /// <returns></returns>
        public ListaLISP cdr()
        {
            if ((this.Listas == null) || (this.Listas.Count == 0))
                return FuncaoLISP.NILL;
            ListaLISP listaTail = new ListaLISP(this);
            listaTail.Listas.RemoveAt(0);
            return listaTail;
        } // cdr()

        public ListaLISP ConstroiLista(params string[] elementos)
        {
            ListaLISP lista = new ListaLISP();
            foreach (string umElemento in elementos)
                lista.Listas.Add(new Atomo(umElemento, ""));
            return lista;
        } // ConstroiLista()


        /// <summary>
        /// obtém tokens, com termos-chave da linguagem LISP.
        /// </summary>
        /// <param name="textoLisp">texto com tokens</param>
        /// <returns>retorna uma lista de tokens, a partir de termos-chave da linguagem LISP.</returns>
        public static List<string> GetTokensLISP(string textoLisp)
        {
            string[] termosChaveLisp = textoLisp.Split(new string[] { "(", ")", " ", "'" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), termosChaveLisp.ToList<string>()).GetTokens();
            return tokens;
        } // GetTokensLISP()


        /// <summary>
        /// escreve na tela os elementos de uma lista lisp, se os elementos forem listas lisp.
        /// </summary>
        /// <param name="umaListaLisp">lista lisp a ser escrita na tela.</param>
        /// <param name="level">escreve a lista apenas no nivel de recursão do método.</param>
        /// <param name="escreveNaTela">escreve na tela o texto resultante, se [true].</param>
        /// <returns></returns>
        public static string WriteListaLISP(ListaLISP umaListaLisp, int level, bool escreveNaTela)
        {
            if (umaListaLisp == null)
                return "";
            else
            {
                string str = "";
                str = "";
                for (int sublista = 0; sublista < umaListaLisp.Listas.Count; sublista++)
                {
                    if (umaListaLisp.Listas[sublista].GetType() == typeof(Atomo))
                        str += umaListaLisp.Listas[sublista].nome + " ";

                    if (umaListaLisp.Listas[sublista].GetType() == typeof(ListaLISP))
                        str += "( " + WriteListaLISP((ListaLISP)umaListaLisp.Listas[sublista], level + 1, escreveNaTela) + " ) ";


                } // for sublista
                if ((level == 0) && (escreveNaTela))
                    System.Console.WriteLine(str);
                return str;
            } // else
        } // ListaListasLISP()


        public List<string> GetAllElements()
        {
            List<string> listaAtomos = new List<string>();
            foreach (Atomo umElemento in this.Listas)
            {
                // se o elemento for uma lista, adiciona todos elementos desta lista.
                if (umElemento.GetType() == typeof(ListaLISP))
                {
                    listaAtomos.AddRange(((ListaLISP)umElemento).GetAllElements());
                } // if
                // se o elemento for um atomo, adiciona este atomo.
                if (umElemento.GetType() == typeof(Atomo))
                {
                    if ((umElemento.nome != "(") && (umElemento.nome != ")"))
                        listaAtomos.Add(umElemento.nome);
                } // if
            } // foreach
            return listaAtomos;
        } // GetAllElements()


        public void InsertElementsTail(List<string> elementos)
        {
            foreach (string umElemento in elementos)
                this.Listas.Add(new Atomo(umElemento, ""));
        } // InsertElementsTail()

        public void InsertElementsHead(List<string> elementos)
        {
            List<Atomo> elementosAtomo = new List<Atomo>();
            foreach (string umElemento in elementos)
                elementosAtomo.Add(new Atomo(umElemento, ""));
            this.Listas.InsertRange(0, elementosAtomo);
        } // InsertElementsHead()

        public override string ToString()
        {
            string str = "( ";
            for (int umAtomo = 0; umAtomo < this.Listas.Count; umAtomo++)
            {
                if (this.Listas[umAtomo].GetType() == typeof(ListaLISP))
                    str += "(" + WriteListaLISP((ListaLISP)this.Listas[umAtomo], 0, false) + ")  ";
                else
                if (this.Listas[umAtomo].GetType() == typeof(Atomo)) 
                {
                  //  if (this.ProximasListas[umAtomo].valor != "")
                    //    str += " " + this.ProximasListas[umAtomo].nome + " ";
                    //else
                        str += " " + this.Listas[umAtomo].nome;
                } // else
            } // for umAtomo
            str += ")";
            return  str;
        }// ToString()
    } // class
} // namespace
