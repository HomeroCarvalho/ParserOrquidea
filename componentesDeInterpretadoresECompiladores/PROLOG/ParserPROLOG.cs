using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser.PROLOG

{
    public class ParserPROLOG
    {
        public static List<Predicado> GetPredicados(string texto)
        {
            List<Predicado> predicadosEncontrados = new List<Predicado>();
            List<string> tokens = ParserPROLOG.GetTokens(texto);
            int indexToken = tokens.IndexOf("(");
            int pilhaInteiroParenteses = 0;
            while ((indexToken != -1) && (indexToken < tokens.Count))
            {


                if (tokens[indexToken] == ".")
                    break;
                if (tokens[indexToken] == ":-")
                {
                    tokens.RemoveAt(indexToken);
                    continue;
                }
                if (tokens[indexToken] == ",")
                {
                    tokens.RemoveAt(indexToken);
                    continue;
                }
                else
                if (tokens[indexToken] == "(")
                    pilhaInteiroParenteses++;
                else
                if (tokens[indexToken] == ")")
                {
                    pilhaInteiroParenteses--;
                    if (pilhaInteiroParenteses == 0)
                    {
                        Predicado umPredicado = new Predicado();
                        List<string> atomosPredicado = tokens.GetRange(0, indexToken + 1);
                        int length = atomosPredicado.Count;

                        int indexStartParametersPredicate = atomosPredicado.IndexOf("(");
                        atomosPredicado.RemoveAt(indexStartParametersPredicate);

                        int indexEndParametersPredicate = atomosPredicado.LastIndexOf(")");
                        atomosPredicado.RemoveAt(indexEndParametersPredicate);

                        string nomePredicado = tokens[0];
                        atomosPredicado.RemoveAt(0);

                        umPredicado.Nome = nomePredicado;
                        if (atomosPredicado.Count > 0)
                            umPredicado.GetAtomos().AddRange(atomosPredicado);

                        predicadosEncontrados.Add(umPredicado);

                        tokens.RemoveRange(0, length);
                        indexToken = 0;
                        pilhaInteiroParenteses = 0;
                        continue;
                    }
                }
                if (tokens.Count == 0)
                    break;

                indexToken++;
            }
            return predicadosEncontrados;
        }


        /// <summary>
        /// obtem tokens de uma linguagem, com termos-chave presentes, operadores presentes, e ids presentes (nome de variveis, funcoes, classes..).
        /// </summary>
        /// <param name="codigo">codigo contendo os tokens a serem extraidos.</param>
        /// <param name="tokensTermosChave">tokens de comandos da linguagem.</param>
        /// <param name="tokensOperadores">tokens de operadores da linguagem.</param>
        /// <returns>retorna uma lista de tokens presentes: termos-chave, operadores, ids.</returns>
        public List<string> GetTokensQualquerLinguagem(string codigo, List<string> tokensTermosChave, List<string> tokensOperadores)
        {

            List<string> todosTokensDefinicaoDeLinguagem = tokensTermosChave.ToList<string>();

            // ordena decrescentemente os operadores, para obter tokens como: ++, e nao uma sequencia: + +.
            parser.ParserUniversal.ComparerTexts comparerTexts = new ParserUniversal.ComparerTexts();
            tokensOperadores.Sort(comparerTexts);

            todosTokensDefinicaoDeLinguagem.AddRange(tokensOperadores);



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

        public static List<string> GetTokens(string texto)
        {
            // termos chave da linguagem Prolog.
            List<string> tokensChave = new List<string>() { ".", ",", ":-", "(", ")", "[", "]", "|", ".", "+", "-", "*", "/" };

            ComandosProlog.Instance();

            Dictionary<string, ComandosProlog.Comando>.KeyCollection comandosPrologNome = ComandosProlog.PredicativosComando.Keys;
            foreach (string nomeComando in comandosPrologNome)
                tokensChave.Add(nomeComando);

            string textCopy = (string)texto.Clone();

            List<string> termosChaveEncontrados = new List<string>();
            int tokenTermoChave = 0;
            while (tokenTermoChave < tokensChave.Count)
            {
                int index = textCopy.IndexOf(tokensChave[tokenTermoChave]);

                if (index != -1)
                {
                    if (IsTokenPolemico(tokensChave[tokenTermoChave], textCopy))
                    {
                        tokenTermoChave++;
                        continue;
                    }
                    textCopy = Util.PreencherVazios.PreencheVazio(textCopy, tokensChave[tokenTermoChave]);
                    termosChaveEncontrados.Add(tokensChave[tokenTermoChave]); // encontrou um termo-chave presente no texto.
                    tokenTermoChave--; // volta a malha, pois pode haver mais tokens do token termo-chave currente.
                }
                tokenTermoChave++;
            }

            textCopy = (string)texto.Clone();
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

            textCopy = (string)texto.Clone();

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




    } // class


} // namespace
