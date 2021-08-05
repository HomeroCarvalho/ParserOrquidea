using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Util;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// esta Classe é para o termo cunhado na teoria dos automatos: a producao de uma linguagem
    /// que é onde se define o caminho na máquina de estados onde o automato irá percorrer, bem
    /// como define os termos da linguagem.
    /// </summary>
    public class producao
    {
        public List<string> maquinaDeEstados { get; set; }
        public List<string> termos_Chave { get; set; }
        public string nomeProducao { get; set; }
        public string tipo { get; set; }

        public string trechoPrograma { get; set; }
        public string str_termoschave { get; set; }

        public List<string> semi_producoes { get; set; }

        public List<string> semiProducoesTrechoDeCodigo { get; set; }

        // posição da produção dentro do código.
        public PosicaoECodigo posicao;

        // índice da posição da produção perante as outras produções encontradas.
        public string indexOrdenacao;

        // sequencia id para produções que não são produções pré-definidas na linguagem.
        public UmaSequenciaID sequencia;

        public override string ToString()
        {
            string s = "";
            // previne do caso em que todas as propriedades, menos os trecho de programas, são nulas.
            if (this.nomeProducao != null)
            {
                s = "producao: " + nomeProducao + ". Maq. de Estados: " + Utils.UneLinhasPrograma(maquinaDeEstados).ToString();
            } // if
            return s;
        } // ToString()
     
        public producao(producao p)
        {

            this.nomeProducao = p.nomeProducao.ToString();
            this.tipo = p.tipo;
            this.str_termoschave = Util.UtilString.UneLinhasLista(p.termos_Chave.ToList<string>());
            this.semiProducoesTrechoDeCodigo = new List<string>();
            this.termos_Chave = new List<string>();
            this.termos_Chave.AddRange(p.termos_Chave);
            this.semi_producoes = p.semi_producoes.ToList<string>();
            //**************************************************************************
            // MONTA A MÁQUINA DE ESTADOS.
            this.maquinaDeEstados = p.maquinaDeEstados.ToList<string>();
            //*****************************************************************************

        }
        public producao(string nomeProd, string tipoProd, List<string> maqDeEstados, string[] termoschave)
        {
            if ((nomeProd == null) ||
                (tipoProd == null) ||
                (maqDeEstados == null) ||
                (termoschave == null))
                return;

            if (nomeProd.Contains("while"))
            {
                int k = 0;
                k++;
            }

            string maqEstadosNumaSoLinha = Utils.UneLinhasPrograma(maqDeEstados);
            this.maquinaDeEstados = maqEstadosNumaSoLinha.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            this.nomeProducao = nomeProd.ToString();
            this.tipo = tipoProd.ToString();
            this.str_termoschave = Util.UtilString.UneLinhasLista(termoschave.ToList<string>());
            this.semiProducoesTrechoDeCodigo = new List<string>();
            this.termos_Chave = new List<string>();
            this.termos_Chave.AddRange(termoschave);

            //*******************************************************************************************************
            // constroi as semiproducoes, numa lista própria.
            // continua com a maquina de estados temporária: [maqEstdadosParaIndices]
            this.semi_producoes = maqEstadosNumaSoLinha.Split(termos_Chave.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            for (int x = 0; x < semi_producoes.Count; x++)
                this.semi_producoes[x] = semi_producoes[x].TrimStart(' ').TrimEnd(' ');
            //*****************************************************************************
        } // public producao


        // ordena produçoes por posição que aparecem no código.
        public static void OrdenaProducoes(List<producao> producoes)
        {
            if (producoes == null)
                return;
            if (producoes.Count == 1)
                return;

            List<PosicaoECodigo> posicoes = new List<PosicaoECodigo>();
            for (int p = 0; p < producoes.Count; p++)
            {
                PosicaoECodigo posicao = producoes[p].posicao;
                posicao.indexParaOrdenacao = p;
            } // for p
            ComparerPosicaoECodigo comparer = new ComparerPosicaoECodigo();
            posicoes.Sort(comparer);

            for (int x = 0; x < posicoes.Count; x++)
                producoes[x] = new producao(producoes[posicoes[x].indexParaOrdenacao]);
        } // OrdenaProducoes()


        public override int GetHashCode()
        {
            int hashCode = 0;
            for (int c = 0; c < this.nomeProducao.Length; c++)
            {
                hashCode += (int)this.nomeProducao[c];
            } // for termo
            for (int termo = 0; termo < this.termos_Chave.Count; termo++)
            {
                for (int c = 0; c < this.termos_Chave[termo].Length; c++)
                {
                    hashCode += (int)(this.termos_Chave[termo][c]);
                } // for c
            } // for termo
            return hashCode;
        }

    } // class producao

    public class ComparerPosicaoECodigo : IComparer<PosicaoECodigo>
    {
        public int Compare(PosicaoECodigo x, PosicaoECodigo y)
        {
            if (x.linha < y.linha) 
                return -1;
            if (x.linha > y.linha) 
                return +1;
            if (x.coluna < y.coluna)
                return -1;
            if (x.coluna > y.coluna)
                return +1;
            return 0;
        }
    } // class ComparerPosicaoECodigo
} //namespace parser
