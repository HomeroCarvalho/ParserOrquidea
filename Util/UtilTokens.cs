using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class UtilTokens
    {

        public static List<string> GetCodigoEntreOperadores(int indiceInicio, string operadorAbre, string operadorFecha, List<string> tokensEntreOperadores)
        {
            List<string> tokens = new List<string>();
            int pilhaInteiros = 0;
            pilhaInteiros = 0;
            int indexToken = indiceInicio;



            while (indexToken < tokensEntreOperadores.Count)
            {
                if (tokensEntreOperadores[indexToken] == operadorAbre)
                {
                    tokens.Add(operadorAbre);
                    pilhaInteiros++;
                }
                else
                if (tokensEntreOperadores[indexToken] == operadorFecha)
                {
                    tokens.Add(operadorFecha);
                    pilhaInteiros--;
                    if (pilhaInteiros == 0)
                        return tokens;

                } // if
                else
                    tokens.Add(tokensEntreOperadores[indexToken]);
                indexToken++;
            } // While

            return tokens;
        } // GetCodigoEntreOperadores()



        /// <summary>
        /// método especializado para retirar listas de tokens, como na instrução "casesOfUse".
        /// 
        /// se houver [ini] e [fini1], retira a lista de tokens entre [ini] e [fini1].
        /// se não houver mais [ini1], retorna as listas de tokens.
        /// se não houver mais [fini1], e houver [ini], retira a lista de tokens entre [ini] até o final da lista de tokens.
        /// </summary>
        public static List<List<string>> GetCodigoEntreOperadoresCases(string tokenMarcadorAbre, string tokenMarcadorFecha, List<string> tokens)
        {
            List<List<string>> tokensRetorno = new List<List<string>>();
            int x = tokens.IndexOf(tokenMarcadorAbre);
        
            int offsetMarcadores = 0;
            while ((x >= 0) && (x < tokens.Count))
            {
                int indexStartBloco = tokens.IndexOf(tokenMarcadorAbre, offsetMarcadores);
                List<string> blocoDoCaseCurrente = GetCodigoEntreOperadores(indexStartBloco, tokenMarcadorAbre, tokenMarcadorFecha, tokens);
               
                
                if ((blocoDoCaseCurrente != null) && (blocoDoCaseCurrente.Count> 0))
                    tokensRetorno.Add(blocoDoCaseCurrente);

                offsetMarcadores += blocoDoCaseCurrente.Count;
                indexStartBloco = tokens.IndexOf(tokenMarcadorAbre, offsetMarcadores); // passa para o proximo bloco da instrucao case.
                if (indexStartBloco == -1)
                    return tokensRetorno;
            }
  
            return tokensRetorno; 

        }  // GetCodigoEntreOperadoresCases()



 

        /// faz a conversao de tipos basicos de classes importados, para o sistema de tipos da linguagem.
        public static string Casting(string tipo)
        {
            if (tipo == "Int32")
                return "int";

            if (tipo == "Float")
                return "float";

            if (tipo == "Double")
                return "float";

            if (tipo == "Bool")
                return "bool";

            if (tipo == "String")
                return "string";

            if (tipo == "Char")
                return "char";

            if (tipo == "Boolean")
                return "bool";
            return tipo;
        }
    }

}
