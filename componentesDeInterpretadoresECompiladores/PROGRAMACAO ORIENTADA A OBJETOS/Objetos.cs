using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class Objeto
    {
        private string tipo;
        private string nome;
        private object valor;


        private List<propriedade> campos { get; set; }
        
        public List<Funcao> construtores = new List<Funcao>();
        public Objeto()
        {
            this.nome = "";
            this.tipo = "";
            this.valor = null;
            this.campos = new List<propriedade>();
        }
        public Objeto(Objeto objeto)
        {
            this.nome = objeto.nome;
            this.tipo = objeto.tipo;
            this.valor = objeto.valor;
            if ((objeto.campos != null) && (objeto.campos.Count > 0))
                this.campos = objeto.campos.ToList<propriedade>();
        }


        public Objeto(string nomeClasse, string nomeObjeto, string nomeCampo, object valorCampo, Escopo escopo)
        {
            InitObjeto(nomeClasse, nomeObjeto, null);
            propriedade campoModificar = this.campos.Find(k => k.GetNome() == nomeCampo);
            campoModificar.valor = valorCampo;
        }

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeClasse, string nomeObjeto, object valor, Escopo escopo)
        {
            InitObjeto(nomeClasse, nomeObjeto, valor);
        }// Objeto()

        private void InitObjeto(string nomeClasse, string nomeObjeto, object valor)
        {
            this.nome = nomeObjeto;
            this.tipo = nomeClasse;
            if (valor == null)
                this.valor = valor;
            Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);

            if ((classe.GetPropriedades() != null) && (classe.GetPropriedades().Count > 0))
                this.campos = classe.GetPropriedades().ToList<propriedade>();
            else
                this.campos = new List<propriedade>();
        }

        public Funcao GetMetodo(string nome)
        {
            return  RepositorioDeClassesOO.Instance().ObtemUmaClasse(this.tipo).GetMetodos().Find(k => k.nome == nome);
        }

        public propriedade GetField(string nome)
        {
            return this.campos.Find(k => k.GetNome() == nome);
        }


              
        /// <summary>
        /// implementa a otimizacao de expressoes. Se uma expressao conter a variavel
        /// que está sendo modificada, a expressao é setada para modificacao=true.
        /// isso auxilia nos calculos de valor da expressao, que é avaliada apenas se 
        /// se alguma variavel-componente da expressao for modificada.
        /// </summary>
        /// <param name="nome">nome da propriedade.</param>
        /// <param name="novoValor">novo valor para a propriedade.</param>
        /// <param name="esopo">contexto onde a propriedade está.</param>
        public void SetValor(string nome, object novoValor, Escopo escopo)
        {
            if (this.GetField(nome) == null)
                return;

            this.GetField(nome).valor = novoValor;
            int index = this.campos.FindIndex(k => k.tipo == nome);
            if (index != -1)
            {
                List<Expressao> expressoes = escopo.tabela.GetExpressoes();
                for (int umaExpressao = 0; umaExpressao < expressoes.Count; umaExpressao++)
                {
                    for (int variavel = 0; variavel < expressoes[umaExpressao].Elementos.Count; variavel++)
                    {
                        if (expressoes[umaExpressao].Elementos[variavel].ToString().Equals(nome))
                        {
                            expressoes[umaExpressao].isModdfy = true;
                            break;
                        }
                    } // for variavel


                } // for index
            } // if index

        } // SetValor()

        public object GetValor()
        {
            return this.valor;
        }


        public string GetNome()
        {
            return this.nome;
        }

        public void SetNome(string nome)
        {
            this.nome = nome;
        }
        public string GetClasse()
        {
            return this.tipo;
        }

      


        public List<propriedade> GetFields()
        {

            return this.campos;
        }

        public override string ToString()
        {
            if ((this.nome == null) || (this.tipo == null))
                return "";
            else
                return this.nome + ": " + this.tipo; 
        }
    } // class Objetos
} // namespace parser
