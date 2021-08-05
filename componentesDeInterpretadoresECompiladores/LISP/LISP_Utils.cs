using System.Linq;
using parser;
namespace parser.LISP
{
    public partial class ListaLISP:Atomo
    {

        /// <summary>
        /// Obtém a próxima lista da lista que chamou o método.
        /// </summary>
        public ListaLISP GetNextList()
        {
            return GetEnesimaLista(1);
        } //GetNextList()

        /// <summary>
        /// obtém a lista atual,dentro da estrutura de uma lista parâmetro.
        /// </summary>
        /// <param name="lista">lista parâmetro.</param>
        /// <returns></returns>
        public ListaLISP GetCurrentList()
        {
            return GetEnesimaLista(1);
        } // GetCurrentList()
        /// <summary>
        /// obtém a enésima lista, da lista que chamou o método.
        /// A contagem de listas começa em 1, e não 0, normalmente para índices.
        /// </summary>
        /// <param name="nElement">número da enésima lista./param>
        /// <returns></returns>
        public ListaLISP GetEnesimaLista(int nElement)
        {
            int contElements = 0;
            int indexLista = 0;
            
            while (((this.Listas != null) && (this.Listas.Count > 0)) && (indexLista<this.Listas.Count))
            {
                if (this.Listas[indexLista].GetType() == typeof(Atomo))
                    contElements++;
                if ((this.Listas[indexLista]).GetType() == typeof(ListaLISP))
                    contElements++;
                if (contElements == nElement)
                {
                    if (this.Listas[indexLista].GetType() == typeof(ListaLISP))
                        return new ListaLISP((ListaLISP)this.Listas[indexLista]);
                    else
                    if (this.Listas[indexLista].GetType() == typeof(Atomo))
                    {
                        ListaLISP lstReturn = new ListaLISP();
                        lstReturn.Listas.Add(this.Listas[indexLista]);
                        return lstReturn;
                    } //else
                } //if
                indexLista++;
            } //while
            return FuncaoLISP.NILL;
        } //GetSecondList()


        /// <summary>
        /// obtém o restante da lista, indo do indice [indexInicial-1] até o final das proximas listas.
        /// O contador de índices começa em 1.
        /// </summary>
        /// <param name="indexInitial">indice inicial a calcular.</param>
        /// <returns></returns>
        public ListaLISP GetAllListsUntilFinalList(int indexInitial)
        {
            ListaLISP listaRetorno = new ListaLISP();
            // obtém a posição nas proximas listas.
            indexInitial--;
            for (int x = indexInitial; x < this.Listas.Count; x++)
                listaRetorno.Listas.Add(this.Listas[x]);
            return listaRetorno;
        } // GetAllListsUntilFinalList()

        public Atomo GetCurrentAtom()
        {
            return this.Listas[0];
        } // GetCurrentAtom()

        /// <summary>
        /// retorna a 1a. instrução lisp de uma função lisp.
        /// (representado como uma lista lisp, contendo nome, lista de parâmetros, e corpo da função).
        /// </summary>
        /// <returns></returns>
        public ListaLISP FirstMemberList()
        {
            // pula o nome da lista (1o. elemento()),
            // a lista de parâmetros (2o. elemento), e vai até o corpo da lista (3o.elemento).
            return this.GetEnesimaLista(3);
            
        }

        /// <summary>
        /// retorna a 2a. instrução lisp de uma função lisp.
        /// (representado como uma lista lisp, contendo nome, lista de parâmetros, e corpo da função).
        /// </summary>
        /// <returns></returns>
        public ListaLISP SecondMemberList()
        {
            /// pula o nome da lista (1o. elemento),
            /// a lista de parâmetros (2o. elemento),
            /// 1o. elemento do corpo da lista (3o. elemento),
            /// chegando ao 2o. elemento do corpo da lista (3o. elemento).
            return this.GetEnesimaLista(4);
        }

        /// <summary>
        /// retorna a 3a. instrução lisp de uma função lisp.
        /// (representado como uma lista lisp, contendo nome, lista de parâmetros, e corpo da função).
        /// </summary>
        /// <returns></returns>
        public ListaLISP ThirdMemberList()
        {

            /// pula o nome da lista (1o. elemento),
            /// a lista de parâmetros (2o. elemento),
            /// 1o. elemento do corpo da lista (3o. elemento),
            /// 2o. elemento do corpo da lista (4o. elemento), chegando ao
            /// 3o. elemento do corpo da lista (5o. elemento).

            return this.GetEnesimaLista(5);
        }

        /// <summary>
        /// obtém a lista de parâmetros de uma função lisp. 
        /// (representado como uma lista lisp, contendo nome, lista de parâmetros, e corpo da função).
        /// </summary>
        /// <returns></returns>
        private ListaLISP GetParametersList()
        {
            //pula o nome da lista(1o. elemento),
            //chegando á lista de parâetros (2o. elemento).
            return this.GetEnesimaLista(2);
     
        }

    } // class partial
} // namespace.
