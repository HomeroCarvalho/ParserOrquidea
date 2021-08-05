
using System.Collections.Generic;
namespace Util
{
    public class PreencherVazios
    {
        /// <summary>
        /// Elimina o texto [token] em sua primeira ocorrência na lista [programa].
        /// Preserva os espaçamentos dentro da lista.
        /// </summary>
        /// <param name="programa">lista a ser processada.</param>
        /// <param name="token">texto a ser deletado em sua primeira ocorrência.</param>
        public static void  EliminaPalavraDeLista(ref List<string> programa, string token)
        {
            for (int x = 0; x < programa.Count; x++)
                if (programa[x].Contains(token))
                {
                    string termo = Util.PreencherVazios.PreencheVazio(programa[x], token);
                    programa.RemoveAt(x);
                    if ((termo != "") && (termo != " "))
                        programa.Insert(x, termo);
                    return;
                } // if
        }// EliminaPalavraDeLista()

        /// <summary>
        /// deleta a primeira ocorrência de [token], no texto [programaTotal].
        /// </summary>
        /// <param name="programaTotal">texto a ser processado.</param>
        /// <param name="token">elemento a ser deletado em sua primeira ocorrência em [programaTotal].</param>
        /// <returns>retorna o texto sem a primeira ocorrência do texto [token].</returns>
        public static string PreencheVazio(string programaTotal, string token)
        {
            
            int indexStart = programaTotal.IndexOf(token);
            if (indexStart == -1)
                return programaTotal;
            else
            {
                string caracteresBranco = Util.UtilString.PreencheBuraco(token.Length);
                programaTotal = programaTotal.Remove(indexStart, token.Length);
                programaTotal = programaTotal.Insert(indexStart, caracteresBranco);
                return programaTotal;
            }// else
        }

        /// <summary>
        /// obtém um trecho de código, que se inicia em [termos_Chave[0]] e [termos_Chave[termos_Chave-1]] 
        /// </summary>
        /// <param name="programa">trecho de código a ser processado.</param>
        /// <param name="termos_Chave">lista contendo o código a ser extraído.</param>
        /// <returns>retorna o trecho de código do [progrma] entre os [termos_Chave]</returns>
        public static string GetCodigoPrograma(string programa, params string[] termos_Chave)
        {
            if ((termos_Chave.Length > 0) && (programa != null))
            {
                int startIndiceTrechoDePrograma = programa.IndexOf(termos_Chave[0]);
                int endIndiceTrechoDePrograma = programa.IndexOf(termos_Chave[1]);
                string trecho = programa.Substring(startIndiceTrechoDePrograma, endIndiceTrechoDePrograma + 1 - startIndiceTrechoDePrograma);
                return trecho;
            } // if termos_Chave.Length
            return "";
        } // GetCodigoProgrma()
    } // class PreencherVazios
} // namespace parser
