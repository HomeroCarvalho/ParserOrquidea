using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms.VisualStyles;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class Escopo
    {

        public enum tipoEscopo { escopoGlobal, escopoNormal};

        // tipo do escopo currente.
        public tipoEscopo ID = tipoEscopo.escopoNormal;
        
        // escopo da linguagem. É o escopo raiz.
        public static Escopo EscopoRaiz = null;

        
        // escopos de mesmo nivel horizontal.
        public List<Escopo> escopoFolhas = null;

        // escopo um nivel acima.
        private Escopo _escopoPai = null;

        // lista de sequencias id encontradas neste escopo.
        public List<UmaSequenciaID> sequencias;

        // escopo raiz do escopo currente. Se o escopo pai for null, e o escopo não for escopoGlobal, retorna o escopoGlobal.
        public Escopo escopoPai
        {
            get
            {
                if (this.ID == tipoEscopo.escopoNormal)
                    return _escopoPai;
                if (this.ID == tipoEscopo.escopoGlobal)
                    return EscopoRaiz;
                return _escopoPai;
            }
            set
            {
                _escopoPai = value;
            } 
        } // escopoPai

        // contém as variáveis, funções, métodos, propriedades, classes registradas neste escopo.       
        public TablelaDeValores tabela { get; set; }




        private List<string> MsgErros = new List<string>();

      
        public List<string> codigo { get; set; }

        private static  UmaGramaticaComputacional linguagem = new LinguagemOrquidea();
   
        public List<string> GetMsgErros()
        {
            return MsgErros;
        }

    
        /// <summary>
        /// constroi a rede de escopos para um programa.
        /// </summary>
        /// <param name="codigo">trecho de código bruto, sem conversao de tokens.</param>
        /// 
        public Escopo(List<string> codigo)
        {
            if (codigo != null)
                this.codigo = codigo.ToList<string>();
            else
                this.codigo = new List<string>();
      
            this.ID = tipoEscopo.escopoNormal;
            this.MsgErros = new List<string>();


            this.tabela = new TablelaDeValores(codigo);
            this.escopoFolhas = new List<Escopo>();
            ConstroiEscopoRaiz();
            this._escopoPai = EscopoRaiz;
            this.sequencias = new List<UmaSequenciaID>();

            if (PosicaoECodigo.lineCurrentProcessing == 0)
                PosicaoECodigo.AddLineOfCode(Utils.UneLinhasPrograma(codigo));
        } // ContextoEscopo()

        public Escopo(Escopo escopo)
        {

            this.ID = escopo.ID;
            this.tabela = new TablelaDeValores(codigo);
            this.MsgErros = new List<string>();
            
            
            
            this.codigo = escopo.codigo.ToList<string>();

            
            this.tabela = escopo.tabela.Clone();
           
            this.escopoFolhas = escopo.escopoFolhas.ToList<Escopo>();
            this.sequencias = escopo.sequencias.ToList<UmaSequenciaID>();

            for (int x = 0; x < escopo.escopoFolhas.Count; x++)
                this.escopoFolhas.Add(new Escopo(escopo.escopoFolhas[x]));

            ConstroiEscopoRaiz();
            this._escopoPai = EscopoRaiz;
        } // Escopo()

        private Escopo()
        {
           
        }

        public void ConstroiEscopoRaiz()
        {
            if (EscopoRaiz == null)
            {
                Escopo.EscopoRaiz = new Escopo()
                {
                    codigo = this.codigo,
                    MsgErros = new List<string>(),
                    tabela = new TablelaDeValores(this.codigo),
                    ID = tipoEscopo.escopoGlobal
                };

                List<Classe> classes = ((LinguagemOrquidea)linguagem).GetClasses();

                foreach (Classe umaClasse in classes)
                {
                    // registra classes presentes na linguagem orquidea.
                    Escopo.EscopoRaiz.tabela.RegistraClasse(umaClasse);
                } // foreach

                // não há escopo anterior ao escopo raiz, então é setado para null.
                this.escopoPai = null;
            } //if
        } //ConstroiEscopoRaiz()

   
        public Escopo Clone()
        {
            Escopo escopo = new Escopo(this.codigo);
            escopo.tabela = this.tabela.Clone();
            return escopo;
        } // Clone()

        public void Dispose()
        {
            this.codigo = null;
            this.tabela = null;
            this.sequencias = null;
            this.MsgErros = null;
            this.escopoFolhas = null;

        }
        public string WriteCallAtMethod(List<List<string>> chamadaAMetodo, int indexChamada)
        {
            string str = "";
            string nomeFuncaoChamada = chamadaAMetodo[indexChamada][0];
            str += nomeFuncaoChamada;
            str += "( ";
            for (int x = 1; x < chamadaAMetodo[indexChamada].Count - 1; x++)
                str += chamadaAMetodo[indexChamada][x] + ",";
            str += chamadaAMetodo[indexChamada][chamadaAMetodo[indexChamada].Count - 1] + ")";
            return str;
        }  //WriteCallMethods()


        /*     
 *    métodos da classe TabelaDeValores
 *    
 *  GetTipoVariavel(nome, escopo): string
 *  GetObjetos(): List<Variavel>
 *  GetFuncao(nome, tipoRetorno)
 *  GetClasses()
 *  GetValorVariavel(nomeVariavel, escopo): object
 *  RegistraClasse(nomeClasse): bool
 *  RegistraMetodo(funcao): bool
 *  RegistraChamadaAMetodo(nomeMetodo, List<string> parametros. escopo): bool.
 *  RegistraExpressao(string[] expressao, escopo): bool
 *  RegistraVariavel(nomeVariavel, tipoVariavel, valorVariavel, posicaoNoCodigo):bool
 *  RecuperaTipoVariavel(nomeVariavel, escopo)
 *  ValidaOperador(nome,tipoOperando, escopo).
 *  ValidaVariavel(nome,tipo, escopo)
 *  
 *  propriedades da classe TabelaDeValores:
 *  
 *   Classes: List<Classe>.
 *   Operadores: List<Operador>.
 *  lng: UmaLinguagemComputacional.
 */

        /* classe ContextoEscopo
         * 
         * ---->métodos:
         * 
         * GetContextoCurrente(): ContextoEscopo
         * tabela: TabelaDeValores
         * ContextoEscopo(codigo,linguagem).
         * ContextoEscopo(linguagem).
         * IsFunction(nome, escopo)
         * 
         * 
         * ---> campos (propriedades):
         *  tokens: List<string>.
         *  codigo: List<string> (get; private set;}
         *  linguagem: UmaLinguagemComputacional.
         *  MsErros: List<string>.
         * 
         * métodos:
         * ContextoEscopo(codigo, linguagem).
         * ContextoEscopo(linguagem)
         * GetPreviousContext():ContextoEscopo.
         * IsFuncao(nome, escopo).
         */


    } // class ContextoEscopo
} // namespace
