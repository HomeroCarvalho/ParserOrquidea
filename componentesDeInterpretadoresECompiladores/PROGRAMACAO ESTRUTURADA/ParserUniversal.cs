using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Util;
using stringUtilities;
namespace parser
{
    /// <summary>
    /// parser universal para retirada de tokens dentro de um texto, tendo como baliza os termos-chave da linguagem 
    /// ao qual os tokens pertence.
    /// </summary>
    public class ParserUniversal
    {

        public static List<string> GetTokens(string textoComTokens)
        {
            
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            List<string> todosTermosChave = linguagem.GetTodosTermosChave();
            List<string> todosOperadoresLinguagem = linguagem.GetTodosOperadores();


            // retira dos termos-chave os operadores, que também são termos-chave, mas nao convem.
            for (int x = 0; x < todosOperadoresLinguagem.Count; x++)
                todosTermosChave.RemoveAll(k => k.Contains(todosOperadoresLinguagem[x]));
  
            List<string> termosChavePresentes = new List<string>();

          
            string textoComTokensTMP = (string)textoComTokens.Clone();

            for (int x = 0; x < todosTermosChave.Count; x++)
            {

                int posicaoTermoChave = textoComTokens.IndexOf(todosTermosChave[x]);
               
      
                if (posicaoTermoChave >= 0)
                {
                    // verifica se o token termo-chave é um token polêmico, exemplo: termo-chave "if", e token "Verifica", o token "Verifica" contém o token "if", eñtão o token "if" não deve ser retirado.
                    if (!IsTokenPolemico(todosTermosChave[x], textoComTokens)) 
                    {
                        termosChavePresentes.Add(todosTermosChave[x]);
                        textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, todosTermosChave[x]);
                        x--;
                    } // if


                } // if


            } // for x
            
            List<string> todosOperadores = linguagem.GetTodosOperadores();
            todosOperadores.Add("(");
            todosOperadores.Add(")");
            todosOperadores.Add("{");
            todosOperadores.Add("}");

            ComparerTexts comparer = new ComparerTexts();
            todosOperadores.Sort(comparer); // ordena os operadores decrescentemente pelo comprimento de seus caracteres

            List<string> operadoresPresentes = new List<string>();
            for (int x = 0; x < todosOperadores.Count; x++)
            {
               
                int posicaoOperador = textoComTokens.IndexOf(todosOperadores[x]);

                if (!IsTokenPolemico(todosOperadores[x], textoComTokens)) 
                {
                    if (posicaoOperador != -1)
                    {
                        operadoresPresentes.Add(todosOperadores[x]);
                        textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, todosOperadores[x]);
                        x--;
                    }  // if
                } // if

                if (posicaoOperador >= 0)
                {

                    operadoresPresentes.Add(todosOperadores[x]);
                    textoComTokens = Util.PreencherVazios.PreencheVazio(textoComTokens, todosOperadores[x]);
                    x--;
                } // if

            } // for x
           
            for (int x = 0; x < termosChavePresentes.Count; x++)
                textoComTokens = ReplaceTokens(termosChavePresentes[x], textoComTokens);

            for (int x = 0; x < operadoresPresentes.Count; x++)
                textoComTokens = ReplaceTokens(operadoresPresentes[x], textoComTokens);

            List<string> idsPresentes = textoComTokens.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();        

     
            RetiraEmptyWords(ref operadoresPresentes); // retira items vazios.
            RetiraEmptyWords(ref idsPresentes);
            RetiraEmptyWords(ref termosChavePresentes);

            List<string> allTokens = new List<string>();
            allTokens.AddRange(termosChavePresentes);
            allTokens.AddRange(idsPresentes);
            allTokens.AddRange(operadoresPresentes);

            RetiraEmptyWords(ref allTokens);

            ComparerTexts comparerTexts = new ComparerTexts();
            allTokens.Sort(comparerTexts);

            List<TokenComPosicao> tokensNaoOrdenados = new List<TokenComPosicao>();
            for (int k = 0; k < allTokens.Count; k++)
            {
         

                int posicao = textoComTokensTMP.IndexOf(allTokens[k]);
                if (posicao != -1)
                {
                    TokenComPosicao umToken = new TokenComPosicao(allTokens[k], posicao);
                    tokensNaoOrdenados.Add(umToken);
                    textoComTokensTMP = UtilString.RetiraTokenDeUmaFrase(textoComTokensTMP, allTokens[k]);
                    k--;
                }
            } // for k

            ComparerTokensPosicao comparer1 = new ComparerTokensPosicao();
            tokensNaoOrdenados.Sort(comparer1);

            List<string> tokensOrdenados = new List<string>();
            foreach (TokenComPosicao umtokenNaoOrdenado in tokensNaoOrdenados)
                if (!isEmptyWord(umtokenNaoOrdenado.token))
                    tokensOrdenados.Add(umtokenNaoOrdenado.token);

            tokensOrdenados = ObtemPontosFlutuantes(tokensOrdenados); // fixa erros de obter tokens que são ponto flutuante (1.1, exemplo).

            return tokensOrdenados;

        } // GetTokens()

        private static bool isEmptyWord(string word)
        {
            if (word == "")
                return true;
            foreach (char umCaracter in word)
            {
                if (umCaracter != ' ')
                    return false;
            } // foreach
            return true;
        } // isEmptyWord()

        public static void RetiraEmptyWords(ref List<string> tokens)
        {
            List<string> tokensSemVazios = new List<string>();
            for (int x = 0; x < tokens.Count; x++)
            {
                if (!isEmptyWord(tokens[x]))
                    tokensSemVazios.Add(tokens[x]);
            } // for x
            tokens = tokensSemVazios;
        } // RetiraEmptyWords()




        private static string ReplaceTokens(string tokenASubstituir, string texto)
        {
            if (!IsTokenPolemico(tokenASubstituir, texto))
                texto = texto.Replace(tokenASubstituir, "");
            return texto;
        }


        static List<char> caracteresLetras = new List<char> {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','X','Y','W','Z',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','w','z'};


        // verifica se o tokens de entrada é um token polemico (exemplo, forA, nao eh um termo-chave pois posteriormente o caracter A indica que o token eh polemico, sainda da lista de termos-chave.)
        private static bool IsTokenPolemico(string token, string textoComOsTokens)
        {
            if (IsSomenteLetras(token, caracteresLetras))
            {

                int indexToken = textoComOsTokens.IndexOf(token);
                if (indexToken == -1)
                    return false;
 
               if (((indexToken - 1) >= 0) && (isLetter(textoComOsTokens[indexToken - 1])))
                    return true;
                else
                if (((indexToken + token.Length < textoComOsTokens.Length)) && (isLetter(textoComOsTokens[indexToken + token.Length])))
                    return true;
                else
                    return false;
            }
            else
            if (token == ".") // o token "." é um token polêmico, é utilizado como operador de ponto flutuante, e também como separador de propriedades/metodos de objetos.
                return true;

            return false;
        }

        private static bool isLetter(char c)
        {
            List<char> caracteresLetras = new List<char> {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','X','Y','W','Z',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','w','z'};

            return (caracteresLetras.IndexOf(c) != -1);
        }
        private static bool IsSomenteLetras(string token, List<char> caracteresLetras)
        {
            for (int indexLetra = 0; indexLetra < token.Length; indexLetra++)
                if (caracteresLetras.FindIndex(k => k.Equals(token[indexLetra])) == -1)
                    return false;
            return true;
        }

        public static List<string> ObtemPontosFlutuantes(List<string> todosTokensObtidos)
        {
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            for (int x = 0; x < todosTokensObtidos.Count; x++)
            {
                if ((todosTokensObtidos[x]==".") && ((x-1)>=0) && (linguagem.VerificaSeEhNumero(todosTokensObtidos[x-1])))
                {
                    int indiceDigitosAnteriores = x - 1;
                    int contadorDigitosAnteriores = 1;
                    while ((indiceDigitosAnteriores >= 0) && (linguagem.VerificaSeEhNumero(todosTokensObtidos[indiceDigitosAnteriores]))) 
                    {
                        indiceDigitosAnteriores--;
                        contadorDigitosAnteriores++;
                    }
                    indiceDigitosAnteriores++;

                    int indiceNumero = x + 1;
                    int indiceNumeroInicial = x - 1;
                    contadorDigitosAnteriores++; //+1 para abranger o operador ".".
                    string numeroPontoFlutuante = Util.UtilString.UneLinhasLista(todosTokensObtidos.GetRange(indiceDigitosAnteriores, contadorDigitosAnteriores )); 



                    todosTokensObtidos.RemoveRange(indiceNumeroInicial, contadorDigitosAnteriores); // retira o numero inicial
                    while ((indiceNumero< todosTokensObtidos.Count) && (linguagem.VerificaSeEhNumero(todosTokensObtidos[indiceNumero])))
                    {
                        numeroPontoFlutuante += todosTokensObtidos[indiceNumero].ToString();
                        todosTokensObtidos.RemoveAt(indiceNumero);
                        indiceNumero += +1 - 1; // registro que: a lista de tokens removeu um elemento (-1), e a malha passou para o próximo elemento (+1).
                    }
                    todosTokensObtidos.Insert(indiceNumeroInicial, numeroPontoFlutuante.Replace(" ", ""));
                }
                
            }
         
            return todosTokensObtidos;
        }


        public class TokenComPosicao
        {
            public string token { get; set; }
            public int coluna { get; set; }

            public TokenComPosicao(string _token, int _coluna)
            {
                this.token = (string)_token.Clone();
                this.coluna = _coluna;
            } //TokenComPosicao()

            public override string ToString()
            {
                return this.token;
            }
        } // class

        public class ComparerTokensPosicao : IComparer<TokenComPosicao>
        {
            public int Compare(TokenComPosicao x, TokenComPosicao y)
            {
               
                if (x.coluna < y.coluna)
                    return -1;
                if (x.coluna > y.coluna)
                    return +1;
                return 0;
            }
        }

        public class ComparerTexts : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x.Length < y.Length)
                    return +1;
                if (x.Length > y.Length)
                    return -1;
                return 0;
            } // Compare()
        } // class

    } // class ParserUniversal
} // namespace
