using System.Collections.Generic;

namespace Util
{
    public class UtilString
    {
        public static string UneLinhasLista(List<string> lines)
        {
            if (lines == null)
                return null;
            string umaLinhaSo = "";
            foreach (string linha in lines)
                umaLinhaSo += linha + " ";
            return umaLinhaSo;
        } // UneLinhasLista()

        public static string PreencheBuraco(int length)
        {
            string str_buraco_preenchido = "";
            for (int x = 0; x < length; x++)
                str_buraco_preenchido += " ";
            return str_buraco_preenchido;
        } // preencherOBuraco()

        public static string RetiraTokenDeUmaFrase(string program, string palavraASerRetirada)
        {
            int indexDeAparicao = program.IndexOf(palavraASerRetirada);

            if (indexDeAparicao!=-1)
            {
                string strBuracoASerPreenchido = PreencheBuraco(palavraASerRetirada.Length);
                program = program.Remove(indexDeAparicao, palavraASerRetirada.Length);
                program = program.Insert(indexDeAparicao, strBuracoASerPreenchido);
            }
            return program;
        } // RetiraTokenDeUmaFrase()
    } // class UtilString
} // Util
