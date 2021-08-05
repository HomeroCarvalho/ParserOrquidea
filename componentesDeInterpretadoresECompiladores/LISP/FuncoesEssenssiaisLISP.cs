using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser.LISP
{
    public class FuncoesEssenciaisLISP

    {
        /// <summary>
        /// função clássica Car (Obtém o primeiro elemento, ou primeira lista).
        /// </summary>
        /// <param name="listaEntrada">lista de entrada.</param>
        /// <returns></returns>
        public static ListaLISP Car(ListaLISP listaEntrada)
        {
            ListaLISP lista = listaEntrada.car();
            if (lista != null)
                return lista;
            else
                return FuncaoLISP.NILL;
        } // Car()



        /// <summary>
        /// função clássica Cdr (Obtém a lista cauda).
        /// </summary>
        /// <param name="listaEntrada">lista de entrada.</param>
        /// <returns></returns>
        public static ListaLISP Cdr(ListaLISP listaEntrada)
        {
            ListaLISP lista = listaEntrada.GetAllListsUntilFinalList(3);
            if (lista != null)
                return lista;
            else
                return FuncaoLISP.NILL;
        }// Cdr()


        public static ListaLISP Adicao(ListaLISP listaEntrada)
        {
            ListaLISP nomeFuncao = Car(listaEntrada);
            int soma = 0;
            if (nomeFuncao.GetAllElements()[0] == "+")
            {
                for (int x = 1; x < listaEntrada.GetAllElements().Count; x++)
                {
                    int umElemento = int.Parse(listaEntrada.GetAllElements()[x]);
                    soma += umElemento;
                } // for x
            } // if
            ListaLISP listaAdicaoSaida = new ListaLISP(soma.ToString());
            return listaAdicaoSaida;
        }  // Adicao()

        public static ListaLISP Subtracao(ListaLISP listaSubtracaoEntrada)
        {
            ListaLISP nomeFuncao = Car(listaSubtracaoEntrada);
            int subratacao = 0;
            if (nomeFuncao.GetAllElements()[0] == "-")
            {
                for (int x = 1; x < listaSubtracaoEntrada.GetAllElements().Count; x++)
                {
                    int umElemento = int.Parse(listaSubtracaoEntrada.GetAllElements()[x]);
                    subratacao += umElemento;
                } // for x
            } // if
            ListaLISP listaSubracaoSaida = new ListaLISP(subratacao.ToString());
            return listaSubracaoSaida;
        }  // Adicao()

        /// <summary>
        /// adiciona um elemento à uma lista de entrada.
        /// </summary>
        /// <param name="elemento">lista para adição.</param>
        /// /// <param name="listaEntrada">lista a ser adicionada o elemento.</param>
        /// <returns></returns>
        public static ListaLISP Cons(ListaLISP elemento, ListaLISP listaEntrada)
        {
            ListaLISP listaSaida = new ListaLISP();
            listaSaida = listaSaida.ConstroiLista(listaEntrada.GetAllElements().ToArray());
            foreach (string umElemento in elemento.GetAllElements())
            {
                listaSaida.GetAllElements().InsertRange(0,elemento.GetAllElements());
            }//for each
            return listaSaida;
        }//Cons()

        public static ListaLISP Member(ListaLISP listaElemento, ListaLISP listaEntrada)
        {
            foreach(string umElemento in listaElemento.GetAllElements())
            {
                if (listaEntrada.GetAllElements().IndexOf(umElemento) == -1)
                    return FuncaoLISP.NILL;
             } //foreach()
            return FuncaoLISP.T;
        } // Member()
    }//class FuncoesEssenciaisLISP

    public class PredicadosLisp
    {
        /// <summary>
        /// retorna true se a entrada é nula.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Null(string value)
        {
            return (value == null);
        }
        /// <summary>
        /// retorna true se a entrada é um Átomo.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool atom(string value)
        {
            return ((value != "(") && (value != ")"));
        }
        /// <summary>
        /// retorna true se a entrada é uma lista.
        /// </summary>
        /// <param name="valuelist"></param>
        /// <returns></returns>
        public static bool listp(object valuelist)
        {
            return (valuelist.GetType() == typeof(ListaLISP));
        }
        /// <summary>
        /// retorn true se  entrada é um número [int] ou [float].
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool numberp(string value)
        {
            try
            {
                int.Parse(value);
            } //try
            catch
            {
                try
                {
                    float.Parse(value);

                } //try
                catch
                {
                    return false;
                } //catch
            } // catch

            return true;
        } //memberp()
      
    } // class
} // namespace
