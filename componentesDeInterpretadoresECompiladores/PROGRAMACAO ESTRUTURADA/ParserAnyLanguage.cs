using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    public class ParserAnyLanguage
    {

        /// <summary>
        /// obtem tokens de uma linguagem, com termos-chave presentes, operadores presentes, e ids presentes (nome de variveis, funcoes, classes..).
        /// Nao verifica a sequencias de comandos, apenas os tokens.
        /// </summary>
        /// <param name="codigo">codigo contendo os tokens a serem extraidos.</param>
        /// <param name="tokensTermosChave">tokens de comandos da linguagem.</param>
        /// <param name="tokensOperadores">tokens de operadores da linguagem.</param>
        /// <returns>retorna uma lista de tokens presentes: termos-chave, operadores, ids.</returns>
        public List<string> GetTokens(string codigo, List<string> tokensTermosChave, List<string> tokensOperadores)
        {

            List<string> todosTokensDefinicaoDeLinguagem = new List<string>();

            // ordena decrescentemente os operadores, para obter tokens como: ++, e nao uma sequencia: + +.
            parser.ParserUniversal.ComparerTexts comparerTexts = new ParserUniversal.ComparerTexts();
            tokensOperadores.Sort(comparerTexts);

            todosTokensDefinicaoDeLinguagem.AddRange(tokensOperadores);
            todosTokensDefinicaoDeLinguagem.AddRange(tokensTermosChave);



            string textCopy = (string)codigo.Clone();
            List<string> termosChaveEncontrados = new List<string>();

            int tokenTermoChave = 0;
            while (tokenTermoChave < todosTokensDefinicaoDeLinguagem.Count)
            {
                int index = textCopy.IndexOf(todosTokensDefinicaoDeLinguagem[tokenTermoChave]);

                if (index != -1)
                {
                    if (IsTokenPolemico(todosTokensDefinicaoDeLinguagem[tokenTermoChave], textCopy))
                    {
                        tokenTermoChave++;
                        continue;
                    }
                    textCopy = Util.PreencherVazios.PreencheVazio(textCopy, todosTokensDefinicaoDeLinguagem[tokenTermoChave]);
                    termosChaveEncontrados.Add(todosTokensDefinicaoDeLinguagem[tokenTermoChave]); // encontrou um termo-chave presente no texto.
                    tokenTermoChave--; // volta a malha, pois pode haver mais tokens do token termo-chave currente.
                }
                tokenTermoChave++;
            }

            textCopy = (string)codigo.Clone();
            for (int x = 0; x < termosChaveEncontrados.Count; x++)
                if (textCopy.IndexOf(termosChaveEncontrados[x]) != -1)
                    textCopy = textCopy.Replace(termosChaveEncontrados[x], " ");

            List<string> todosTokensEncontrados = termosChaveEncontrados.ToList<string>();
            List<string> ids = textCopy.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();


            if ((ids != null) && (ids.Count > 0))
            {
                ParserUniversal.RetiraEmptyWords(ref ids);
                todosTokensEncontrados.AddRange(ids.ToList<string>());
            }

            textCopy = (string)codigo.Clone();

            List<parser.ParserUniversal.TokenComPosicao> tokensNaoOrdenados = new List<ParserUniversal.TokenComPosicao>();
            for (int umToken = 0; umToken < todosTokensEncontrados.Count; umToken++)
            {
                int index = textCopy.IndexOf(todosTokensEncontrados[umToken]);
                parser.ParserUniversal.TokenComPosicao tokenComPosicao = new ParserUniversal.TokenComPosicao(todosTokensEncontrados[umToken], index);
                tokensNaoOrdenados.Add(tokenComPosicao);
                textCopy = Util.PreencherVazios.PreencheVazio(textCopy, todosTokensEncontrados[umToken]);
            }

            parser.ParserUniversal.ComparerTokensPosicao comparer = new ParserUniversal.ComparerTokensPosicao();
            tokensNaoOrdenados.Sort(comparer);

            List<string> tokensOrdenados = new List<string>();
            for (int umToken = 0; umToken < tokensNaoOrdenados.Count; umToken++)
                tokensOrdenados.Add(tokensNaoOrdenados[umToken].token);

            return tokensOrdenados;

        }
        // verifica se o tokens de entrada é um token polemico (exemplo, forA, nao eh um termo-chave pois posteriormente o caracter A indica que o token eh polemico, sainda da lista de termos-chave.)
        private static bool IsTokenPolemico(string token, string textoComOsTokens)
        {
            int indexToken = textoComOsTokens.IndexOf(token);
            if (indexToken == -1)
                return false;

            if (token == ".")
                return false;

            List<char> caracteresLetras = new List<char> {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','X','Y','W','Z',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','w','z'};

            if (IsSomenteLetras(token, caracteresLetras))
            {

                if (((indexToken - 1) >= 0) && (caracteresLetras.FindIndex(k => k.Equals(textoComOsTokens[indexToken - 1])) != -1))
                    return false;

                if (((indexToken + 1 + token.Length) < textoComOsTokens.Length) && (caracteresLetras.FindIndex(k => k.Equals(textoComOsTokens[indexToken + 1 + token.Length - 1])) != -1))
                    return false;

                return true;
            }
            else
                return false;
        }

        private static bool IsSomenteLetras(string token, List<char> caracteresLetras)
        {
            for (int indexLetra = 0; indexLetra < token.Length; indexLetra++)
                if (caracteresLetras.FindIndex(k => k.Equals(token[indexLetra])) == -1)
                    return false;
            return true;
        }




    }
}
