using System.Collections.Generic;
using System.Linq;

namespace parser
{
    /// <summary>
    /// Classe que mapeia as posições de cada instrução, dentro de uma lista de instruções.
    /// </summary>
    public class PosicaoECodigo
    {

        public int coluna { get; set; } // coluna onde o primeiro elemento do trecho de codigo aparece.

        public int linha { get; set; } // linha  onde esta o primeiro elemento do trecho de codigo.

        public string trechoCodigo { get; set; } // trecho de código a ter seu posicionamento determninado

        public int indexParaOrdenacao { get; set; }  // indice utilizado para ordenação de listas de posicaoEcodigo.



        private static string str_codigoCurrente = "";
        private static List<string> tokensMaiorQtdTokens = new List<string>();
        private static LinguagemOrquidea linguagem = null;

        /// <summary>
        /// construtor.obtem a posição de um grupo de tokens, ante um trecho de codigo do programa.
        /// </summary>
        public PosicaoECodigo(List<string> tokensLocalizadores, List<string> codigo)
        {
            if ((tokensLocalizadores == null) || (tokensLocalizadores.Count == 0))
                return;

            /// inicializa a linguagem utilizada
            if (linguagem == null)
                linguagem = new LinguagemOrquidea();

            /// 1- atualizar o codigo. Se o codigo da entrada for maior que o codigo guardado, guardar o codigo de entrada.
            /// 2- procurar (linhas,colunas) para o primeiro token a localizar.
            /// 3- malha para cada token encontrado, validar se os tokens a localizar estão na sequencia correta da entrada.
            /// 4- se nao encontrar o token currente, passar para o proximo tokens [0].



            /// atualizando o codigo guardado.
            string str_codigo_params = Util.UtilString.UneLinhasLista(codigo);
            if (str_codigo_params.Length > str_codigoCurrente.Length) // inicializa o codigo com maior cumprimento.
            {
                tokensMaiorQtdTokens = new Tokens(PosicaoECodigo.linguagem, new List<string>() { str_codigo_params }).GetTokens();
                str_codigoCurrente = (string)str_codigo_params.Clone();

            }

            string str_codigo_copy = (string)str_codigoCurrente.Clone();

            /// procurando token primeiro, dentro do codigo.
            List<tokenEPosicao> tokenPrimeiros = new List<tokenEPosicao>();

            int linhaLineCode = 0;
            int colunaLinhaCode = 0;
            while (linhaLineCode < codigo.Count)
            {
                colunaLinhaCode = 0;
                while (linhaLineCode < codigo.Count) 
                {
                    colunaLinhaCode = codigo[linhaLineCode].IndexOf(tokensLocalizadores[0], colunaLinhaCode);
                    if (colunaLinhaCode != -1)
                    {
                        tokenPrimeiros.Add(new tokenEPosicao(linhaLineCode, colunaLinhaCode, tokensLocalizadores[0]));

                        str_codigoCurrente = Util.PreencherVazios.PreencheVazio(str_codigoCurrente, tokensLocalizadores[0]); // remove daqui o token localizado.

                        colunaLinhaCode += tokensLocalizadores[0].Length;
                        if (colunaLinhaCode > codigo[linha].Length)
                        {
                            colunaLinhaCode = 0;
                            linhaLineCode++;
                            break;
                        }

                    }
                    else
                    {
                        linhaLineCode++; // passa para a proxima linha, nao localizou o primeiro token da sequencia para localizar.
                        break;
                    }

                }
                if ((linhaLineCode < codigo.Count) && (colunaLinhaCode >= codigo[linhaLineCode].Length))
                    linhaLineCode++;
            }

            if (tokenPrimeiros.Count == 0)
            {
                this.linha = -1;
                this.coluna = -1;
                return;
            }
           

            str_codigoCurrente = (string)str_codigo_copy;
            /// malha para os tokens seguintes.
            int tokenPrimeiroCurrente = 0;
            for ( tokenPrimeiroCurrente = 0; tokenPrimeiroCurrente < tokenPrimeiros.Count; tokenPrimeiroCurrente++)
            {
                int linha = tokenPrimeiros[tokenPrimeiroCurrente].linha;
                int coluna = tokenPrimeiros[tokenPrimeiroCurrente].coluna;
                bool isFound = true;
                for (int tokensNext = 0; tokensNext < tokensLocalizadores.Count; tokensNext++)
                {

                    int indexTokenCurrente = str_codigoCurrente.IndexOf(tokensLocalizadores[tokensNext]);
                    if (indexTokenCurrente == -1)
                    {
                        isFound = false;
                        break;
                    }
                    else
                      str_codigoCurrente = Util.PreencherVazios.PreencheVazio(str_codigoCurrente, tokensLocalizadores[tokensNext]);               
                }
                if (isFound)
                {
                    this.linha = tokenPrimeiros[tokenPrimeiroCurrente].linha;
                    this.coluna = tokenPrimeiros[tokenPrimeiroCurrente].coluna;
                    this.trechoCodigo = (string)codigo[this.linha].Clone();

                    str_codigoCurrente = (string)str_codigo_copy.Clone();
                    return;
                }
            }

            this.linha = -1;
            this.coluna = -1;
            this.trechoCodigo = "";
            str_codigoCurrente = (string)str_codigo_copy.Clone();
        }

        
        /// localiza se o token está na posicao da entrada. localiza mesmo se ha caracteres vazios entre os tokens.
        private bool Match(string token, int posicao, string str_codigo)
        {
            int indiceCaracterVazio = 0;
            while (indiceCaracterVazio < token.Length)
            {
                if (token[indiceCaracterVazio] != ' ')
                    break;
                else
                    indiceCaracterVazio++;
            }

            for (int caracterToken = 0; caracterToken < token.Length; caracterToken++)
            {
                char caracterProcura = str_codigo[posicao + caracterToken + indiceCaracterVazio];
                if (caracterProcura != token[caracterToken])
                    return false;
            }
            return true;
        }

        private List<string> EliminaVazios(List<string> tokensTotal)
        {
            List<string> tokens = tokensTotal.ToList<string>();

            for (int x = 0; x < tokens.Count; x++)
                for (int c = 0; c < tokens[x].Length; c++)
                {
                    if (tokens[x][c] == ' ')
                    {
                        tokens[x] = tokens[x].Remove(c, 1);
                        c--;
                    }
                } // for c

            return tokens;
        } // EliminaVazios()




        public override string ToString()
        {

            return ("linha: (" + this.linha + " ) coluna: (" + coluna + ")--> " + trechoCodigo);
        } // ToString()


        public class tokenEPosicao
        {
            public int linha { get; set; }
            public int coluna { get; set; }

            public string token { get; set; }

            public tokenEPosicao(int lin, int col, string _token)
            {
                this.linha = lin;
                this.coluna = col;
                this.token = _token;
            }
        }
    } // class PosicaoECodigo
} // namespace parser
