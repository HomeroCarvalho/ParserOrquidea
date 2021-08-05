using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stringUtilities
{
    /// <summary>
    /// classe para guardar o nome e o indice dentro de uma lista ou vetor de itens.
    /// </summary>
    class itemlocalizadorDeStrings
    {
        public string strItem {get;set;}
        public int iIndexItem {get;set;}

        public itemlocalizadorDeStrings(string item, int index)
        {
            this.strItem = item;
            this.iIndexItem = index;
        } // itemlocalizadorDeStrings()

    }
    /// <summary>
    /// classe baseada no [string.Split()], mas localiza dentro de uma string
    /// itens a partir de itens de entrada, e não partes separadas da  string 
    /// (tendo como comparadores os itens de entrada).
    /// </summary>
    public class localizadorDeStrings
    {

        public static List<string> localizaStringsSemRepeticoes(List<string> lstItensAPesquisar)
        {
            int x, y;
            List<int> lstIndicesIttensARetirar = new List<int>();
            List<string> lstItensAPesquisarSemRepeticoes = new List<string>();
            List<string> lstItensSemRepeticoes = new List<string>();
            // retira da lista de entrada todas repetições de palavras.
            for (x = 0; x < lstItensAPesquisar.Count; x++)
            {
                for (y = (x+1); y < lstItensAPesquisar.Count; y++)
                {
                    // se os índices são distintos, faz o teste de igualdade.
                    if (x != y)
                    {
                        if (lstItensAPesquisar[x].Equals(lstItensAPesquisar[y]))
                            // se as palavras são iguais, coloca a palavra repetida na lista para retirar, posteriormente.
                            lstIndicesIttensARetirar.Add(y);
                    } // if x!=y
                } // for y
            } // for x
            lstItensAPesquisarSemRepeticoes = lstItensAPesquisar.ToList<string>();
            // remove as palavras repetidas na lista de entrada, que estão registradas na lista de repetições.
            for (x = 0; x < lstIndicesIttensARetirar.Count; x++) 
                lstItensAPesquisarSemRepeticoes.RemoveAt(lstIndicesIttensARetirar[x]);
            return lstItensAPesquisarSemRepeticoes;
        } // localizaStringsSemRepeticoes()

        /// <summary>
        /// localiza tokens de [vtItensAPesquisar] na string [strAPesquisar], mas sem repetições,
        /// tanto do vetor de tokens [vtItensAPesquisar], quanto dentro da string [strAPesquisar].
        /// </summary>
        /// <param name="strAPesquisar">string a ser investigada.</param>
        /// <param name="vtIensAPesquisar">lista de palavras a serem pesquisadas.</param>
        /// <returns>retorna todas palavras da lista que estão na string, mas sem repetições.</returns>
        public static List<string> localizaStringsSemRepeticoes(string strAPesquisar, List<string> vtIensAPesquisar)
        {
            int x,y;
            List<int> lstIndicesItensARetirar= new List<int>();
            List<string> lstItensAPesquisarSemRepeticoes= new List<string>();
            List<string> lstItensSemRepeticoes= new List<string>();
            // retira da lista de entrada todas repetições de palavras.
            for (x = 0; x < vtIensAPesquisar.Count; x++)
            {
                for (y = (x+1); y < vtIensAPesquisar.Count; y++)
                {
                    // se os índices são distintos, faz o teste de igualdade.
                    if (x != y)
                    {
                        if (vtIensAPesquisar[x].Equals(vtIensAPesquisar[y]))
                            // se as palavras são iguais, coloca a palavra repetida na lista para retirar, posteriormente.
                            lstIndicesItensARetirar.Add(y);
                    } // if x!=y
                } // for yF:\PROGRAMMING\Projetos C Sharp - REPOSITORIO CURRENTE\PROJETOS SENDO TRABALHADOS\LING.ORQUIDEA (EM FASE DE CONCLUSÃO)\testesComponentes\strlocalizadorDeStrings.cs
            } // for x
            lstItensAPesquisarSemRepeticoes= vtIensAPesquisar.ToList<string>();
            // remove as palavras repetidas na lista de entrada, que estão registradas na lista de repetições.
            for (x = 0; x < lstIndicesItensARetirar.Count; x++)
                lstItensAPesquisarSemRepeticoes.RemoveAt(lstIndicesItensARetirar[x]);
            int index = 0;
            List<string> lstItensRetorno = new List<string>();
            // pesquisa se cada palavra sem repetição está presente na string de entrada [strAPesquisar].
            for (x = 0; x < lstItensAPesquisarSemRepeticoes.Count; x++)
            {
                index = -1;
                index = strAPesquisar.IndexOf(lstItensAPesquisarSemRepeticoes[x]);
                if (index != (-1))
                   lstItensRetorno.Add(lstItensAPesquisarSemRepeticoes[x]);
            } // for x
            // retorna um vetor de strings, sem repetições e pesquisadas.
            return lstItensRetorno;
        } // localizaStringsSemRepeticoes()


        /// <summary>
        /// localiza todos items em [vtItensAPesquisar] presentes na string [strAPesquisar]
        /// </summary>
        /// <param name="strAPesquisar">string a ser pesquisar a ocorrência de itens</param>
        /// <param name="vtItensAPesquisar">vetor de itens string a ser encontrados.</param>
        /// <returns>retorna um vetor de strings com todos itens de entrada encontrados na string de entrada.</returns>
        public static List<string> localizaStrings(string strAPesquisar, List<string> vtItensAPesquisar)
        {

            int contadorTexto = 0, index = 0;
            List<itemlocalizadorDeStrings> lstItens= new List<itemlocalizadorDeStrings>();

            // a malha calcula todos indices do vetor de entrada [vtItensAPesquisar] dentro da string de entrada [strAPesquisar].
            foreach(string item in vtItensAPesquisar)
            {
                bool saida= false;
                // faz a pesquisa enquanto houver possibilidades de novos índices.
                contadorTexto = 0;
                while ((!saida) && (contadorTexto < strAPesquisar.Length)) 
                {
                    index = (-1);
                    // tenta obter um índice para o item currente.
                    // o [contadorTexto] garante que o primeiro índice seja a partir do contador.
                    // isto garante a pesquisa de múltiplos índices para um mesmo item.
                    index = strAPesquisar.IndexOf(item, contadorTexto);
                    saida= true;
                    // se localizou um indice para o item currente,
                    // guarda o item currente e seu indice dentro da string [strAPesquisar].
                    if (index!=(-1))
                    {
                        // seta [saida] para continuar a pesquisar índices para o currente item.
                        saida=false;
                        lstItens.Add(new itemlocalizadorDeStrings(item, index));
                        contadorTexto=index+item.Length;
                    } // if index!=(-1)
                } // while (!saida)

            } // foreach item
            // inicializa um comparador para ordenar a lista de itens.
            compareritemlocalizadorDeStrings cmp = new compareritemlocalizadorDeStrings();
            // ordena a lista de itens com um comparador que ordena segundo o índice guardado.
            lstItens.Sort(cmp);
            List<string> lstStr = new List<string>();
            // prepara o retorno dos itens localizador, para uma lista de strings
            foreach (itemlocalizadorDeStrings item in lstItens)
            {
                lstStr.Add(item.strItem);
            } // foreach item
            // retorna a lista de strings, transformada num array.
            return (lstStr);
        } // calcItens()

        /// <summary>
        /// localiza as strings delimitadas por [strItensSeparadores].
        /// Trabalha como [string.Split()], mas corrige o problema de
        /// dois separadores vizinhos, pois chama o método desta clas-
        /// se [localizaStrings()] 
        /// </summary>
        /// <param name="strAPesquisar">texto a pesquisar.</param>
        /// <param name="strItensSeparadores">itens [string] que funcionam como separadores</param>
        /// <returns>retorna um vetor com itens separados por [strItensSeparadores], dentro da
        /// string [strAPesquisar].</returns>
        public static List<string> localizaComplementoStrings(string strAPesquisar, List<string> strItensSeparadores)
        {
            List<string> itensSeparadores = localizaStrings(strAPesquisar, strItensSeparadores);
            List<string> itensRetorno = localizaStrings(strAPesquisar, itensSeparadores.ToList<string>());
            return itensRetorno;
        } // localizaComplementoStrings()


        /// <summary>
        /// remove todas strings de [itensASeremRemovidos] dentro da string [strASerProcessada].
        /// </summary>
        /// <param name="strASerProcessada">String a ser editada.</param>
        /// <param name="itensASeremRemovidos">itens [string] que devem ser removidos da string a ser processada.</param>
        /// <returns></returns>
        public string removeStrings(String strASerProcessada, string[] itensASeremRemovidos)
        {
            String strRetorno = (string)strASerProcessada.Clone();
            foreach (string item in itensASeremRemovidos)
            {
                strRetorno=strRetorno.Replace(item, "");
            } // foreach
            return strRetorno;
        } // void removeStrings()

        /// <summary>
        /// classe de comparação que a ordenação é em função do índice do [itemlocalizadorDeStrings]
        /// </summary>
        class compareritemlocalizadorDeStrings : IComparer<itemlocalizadorDeStrings>
        {
            public int Compare(itemlocalizadorDeStrings item1, itemlocalizadorDeStrings item2)
            {
                if (item1.iIndexItem > item2.iIndexItem)
                    return (+1);
                if (item1.iIndexItem < item2.iIndexItem)
                    return (-1);
                return (0);
            } // Compare()
        } // class compareritemlocalizadorDeStrings 
    } // class localizadorDeStrings
} // namespace stringUtilies
