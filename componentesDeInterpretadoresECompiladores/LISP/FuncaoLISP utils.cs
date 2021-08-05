
namespace parser.LISP
{
    public partial class FuncaoLISP
    {

        /// <summary>
        /// retorna o nome da lista.
        /// </summary>
        /// <param name="lista">lista com o nome a retirar.</param>
        /// <returns>retorna o nome da lista.</returns>
        public static string GetName(ListaLISP lista)
        {
            return lista.car().Listas[0].nome;
        } // GetName()

    } // class FuncaoLISP
} // namespace
