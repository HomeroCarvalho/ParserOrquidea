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

     
        public static int lineCurrentProcessing = 0;
        private static List<tokenEPosicao> tokensPosicionados = new List<tokenEPosicao>();


        private static LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

        /// <summary>
        /// construtor.obtem a posição de um grupo de tokens, ante um trecho de codigo.
        /// tokensLocalizadores: lista dos tokens que se quer localizar.
        /// começa na linha=1, coluna=1, não é baseado na contagem a partir do 0, mas a partir de 1.
        /// </summary>
        public PosicaoECodigo(List<string> tokensLocalizadores)
        {

            tokenEPosicao posicaoNoCodigo = this.LocalizaToken(tokensLocalizadores);
            this.linha = posicaoNoCodigo.linha + 1;
            this.coluna = posicaoNoCodigo.coluna + 1;
        }

        public static void InitCalculoPosicoes()
        {
            PosicaoECodigo.lineCurrentProcessing = 0;
            PosicaoECodigo.tokensPosicionados = new List<tokenEPosicao>();

        }

        /// <summary>
        /// processa uma linha de codigo, calculando a posição (linha, coluna) de cada token.
        /// </summary>
        /// <param name="line">uma linha de um arquivo texto, contendo tokens da linguagem orquidea.</param>
        public static void AddLineOfCode(string line)
        {
            

            List<string> tokens = ParserUniversal.GetTokens(line);
            

            int indexLin = PosicaoECodigo.lineCurrentProcessing; // obtem a linha currente de processamento de posição de tokens no código.
            int indexCol = 0;

            int x = 0;
            while (x < tokens.Count)
            {
            

                indexCol = line.IndexOf(tokens[x]);
                if (indexCol != -1) // enquanto houver tokens de mesmo nome deste token currente, extrai linha, coluna, e apaga o primeiro token de nome igual ao currente token.
                {

                    PosicaoECodigo.tokensPosicionados.Add(new tokenEPosicao(indexLin, indexCol, tokens[x])); // adiciona o tokens currente, com sua linha e coluna ante ao código.
                    line = Util.PreencherVazios.PreencheVazio(line, tokens[x]); // apaga o token, para que outro token de mesmo nome acesse esta linha,coluna de posicao.

                    x++;
                }
                else
                    x++;
            }

            PosicaoECodigo.lineCurrentProcessing++;
        }


        private tokenEPosicao LocalizaToken(List<string> tokens)
        {
            if ((PosicaoECodigo.tokensPosicionados == null) || (PosicaoECodigo.tokensPosicionados.Count == 0))
                return new tokenEPosicao(-1, -1, "");

            List<tokenEPosicao> linhaLocalizada = new List<tokenEPosicao>();
            int indexTokenLocalizar = 0;
            foreach (tokenEPosicao codigoPosicionado in PosicaoECodigo.tokensPosicionados)
            {
                if (codigoPosicionado.token == "classeB")
                {
                    int k = 0;
                    k++;
                }
                if (codigoPosicionado.token == tokens[indexTokenLocalizar])
                {
                    linhaLocalizada.Add(codigoPosicionado);
                    indexTokenLocalizar++;
                    if (indexTokenLocalizar >= tokens.Count)
                        break;
                }
                else
                    linhaLocalizada.Clear();

            }
            if (linhaLocalizada.Count > 0)
            {
                int linha = linhaLocalizada[0].linha;
                int coluna = linhaLocalizada[0].coluna;
                return new tokenEPosicao(linha, coluna, linhaLocalizada[0].token);
            }
            else
                return new tokenEPosicao(-1, -1, "");

        }

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

            public override string ToString()
            {
                return "(" + linha + ",  " + coluna + ") : " + token;
            }
        }
    } // class PosicaoECodigo
} // namespace parser
