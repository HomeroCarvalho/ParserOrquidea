using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser.ProgramacaoOrentadaAObjetos
{

    /// <summary>
    /// classe que guarda uma sequencias de tokens id, que representa uma sequencia id válida.
    /// </summary>
    public class UmaSequenciaID
    {

        public List<string> original { get; set; } //  sequencia ID com os tokens sem modificações.
  
        public List<Expressao> expressoes { get; set; } // lista de expressões relacionadas a uma sequencia id. Essa lista é cruscial para processamento de sequencias id que envolvem instruções da linguagem.


        public List<List<UmaSequenciaID>> sequenciasDeBlocos { get; set; }
       
        public producao producao { get; set; } // produção associada à sequencia.

   

        private static LinguagemOrquidea linguagem = new LinguagemOrquidea(); // linguagem utilizada: orquidea.
        public int indexHandler; // indice do método handler da sequencia id.

     
        public UmaSequenciaID(string[] seOriginal, List<string> codigo)
        {
            
            List<string> tokens = new Tokens(new LinguagemOrquidea(), codigo).GetTokens();
      
            this.sequenciasDeBlocos = new List<List<UmaSequenciaID>>();
         
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            Random aleatorizador = new Random(50000); // compoe um nome aleatório para a sequenciaID.
        
            this.original = seOriginal.ToList<string>(); // inicializa os  a sequencia com os tokens originais;
            this.expressoes = new List<Expressao>();//inicializa a lista de expressões associadas a sequencia id.


            this.indexHandler = -1;  // inicializa o indice do método principal.

        } // UmaSequenciaID()

        public override string ToString()
        {
            string str = "";
            if (this.expressoes != null)
            {
                for (int x = 0; x < this.expressoes.Count; x++)
                    str += this.expressoes[x].ToString();
            } // if
            for (int x = 0; x < original.Count; x++)
                str += original[x] + " ";
            return str.TrimEnd(' ').TrimStart(' ');

        } // ToString()

        public override int GetHashCode()
        {
            int hash = 0;
            for (int c = 0; c < this.original[0].Length; c++)
                hash += (int)this.original[0][c];
            return hash;
        } // GetHashCode()

        /*
       * 1 ---> metodo de obter sequenciaid: termos-chave, parenteses--(tokens entre operadores parenteses), ; (final de sequencia), { ( tokens entre operadores bloco).
          --> se token="(", obtém tokens entre operadores parenteses, acrescenta a lista de tokens da sequencia, e continua o loop de tokens.
          --> se token= ";", retorna a sequencia.
          --> se token="{", obtém tokens entre operadores bloco, acrescenta a lista de tokens da sequencia, e retorna a sequencia.
       */


        public static UmaSequenciaID ObtemUmaSequenciaID(int startIndex, List<string> tokens, List<string> codigo)
        {
            List<string> tokensDaSequencia = new List<string>();
            int umToken = startIndex;
            while (umToken < tokens.Count)
            {
                if (tokens[umToken] == ";")
                {
                    tokensDaSequencia.Add(";");
                    return new UmaSequenciaID(tokensDaSequencia.ToArray(), codigo);
                } // if
                else
                if (tokens[umToken] == "{")
                {
                    List<string> bloco = UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);
                    if ((bloco != null) && (bloco.Count > 0))
                    {
                        tokensDaSequencia.AddRange(bloco);
                        umToken += bloco.Count;
                    } // if bloco
                } // if
                else
                if (tokens[umToken] == "(")
                {
                    List<string> tokensEntreParentes = UtilTokens.GetCodigoEntreOperadores(umToken, "(", ")", tokens);
                    if ((tokensEntreParentes != null) && (tokensEntreParentes.Count >= 1))
                    {
                        tokensDaSequencia.AddRange(tokensEntreParentes);
                        umToken += tokensEntreParentes.Count;
                    } // if
                } // if
                else
                {
                    tokensDaSequencia.Add(tokens[umToken]); // adiciona o token currente na lista de tokens da sequencia.
                    umToken++;
                } //else
            } // for umToken

            return new UmaSequenciaID(tokensDaSequencia.ToArray(), codigo);
        } // ObtemUmaSequenciaID()

    } // class UmaSequenciaID


    // ordena em ordem decrescente strings, tendo como parãmetro de ordenação o cumprimento das strings.
    // Utilizado principalmente na construção de sequencias e processamento de sequencias, no método MatchSequencia().
    public class ComparerSequenciasID : IComparer<string>
    {
        int IComparer<string>.Compare(string x, string y)
        {
            if (x.Length > y.Length)
                return -1;
            if (x.Length < y.Length)
                return +1;
            return 0;
        }// Compare()
    } // ComarerSequenciasID
}// namespace
