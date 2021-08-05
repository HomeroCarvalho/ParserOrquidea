using System;
using System.Collections.Generic;

namespace parser
{
    /// <summary>
    /// registras as produções encontradas no analisador léxico, para
    /// posterior utilização pelo compilador.
    /// </summary>
    public class RegistradorBNF
    {
        private List<string> programa = null;
        /// <summary>
        /// lista das produções encontrada no analisador léxico.
        /// </summary>
        private List<producao> producoesEncontradas;
        public List<producao> GetProducaoEncontradas()
        {
            return producoesEncontradas;
        }

        private List<PosicaoECodigo> posicaoProducoes;
        public List<PosicaoECodigo> GetPosicaoProducao()
        {
            return posicaoProducoes;
        }

        /// <summary>
        /// construtor.
        /// <param name="programaTotal">programa sendo analisado.</param>
        /// </summary>
        public RegistradorBNF(List<string> programaTotal)
        {
            this.programa = programaTotal;
            this.producoesEncontradas = new List<producao>();
            this.posicaoProducoes = new List<PosicaoECodigo>();
        } // RegistradorBNF()
   
        public void AtualizaRegistradorBNF()
        {
            try
            {
                List<int> indexPos = new List<int>();
                List<producao> producoesOrdenadas = new List<producao>();
                string programaEmUmaLinha = Util.UtilString.UneLinhasLista(programa);
                for (int p = 0; p < producoesEncontradas.Count; p++)
                {
                    int index = programaEmUmaLinha.IndexOf(producoesEncontradas[p].trechoPrograma);
                    indexPos.Add(index);
                } // for p
                for (int x = 0; x < indexPos.Count; x++)
                {
                    producoesOrdenadas.Add(this.producoesEncontradas[indexPos[x]]);
                } // for x
                this.producoesEncontradas = producoesOrdenadas;
            } // try
            catch (Exception e)
            {
                ModuloTESTES.LoggerTests.AddMessage("Erro na reorganizacao de producoes do registrador BNF. Metodo: [AtualizaRegistradorBNF()]. Erro:" + e.Message + ". Stack: " + e.StackTrace);
            } // catch
        } //AtualizaRegistradorBNF()

       
    } // class RegistradorBNF
} // namespace parser
