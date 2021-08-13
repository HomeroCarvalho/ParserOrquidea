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
        private string acessor;
        private string tipo;
        private string nome;
        private object valor;

        public bool isStatic { get; set; }
        private List<Objeto> campos { get; set; }
        
        public List<Funcao> construtores = new List<Funcao>();
        public Objeto()
        {
            this.nome = "";
            this.tipo = "";
            this.valor = null;
            this.campos = new List<Objeto>();
            this.isStatic = false;
        }
        public Objeto(Objeto objeto)
        {
            this.nome = objeto.nome;
            this.tipo = objeto.tipo;
            this.valor = objeto.valor;
            this.isStatic = objeto.isStatic;
            if ((objeto.campos != null) && (objeto.campos.Count > 0))
                this.campos = objeto.campos.ToList<Objeto>();
        }


        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, string nomeCampo, object valorCampo, Escopo escopo)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, null);
            Objeto campoModificar = this.campos.Find(k => k.GetNome() == nomeCampo);
            campoModificar.SetValor(valorCampo, escopo); // aciona a otimização de cálculo de expressões.
            this.isStatic = isStatic;
        }

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = false;
        }// Objeto()

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor, bool isStatic)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = isStatic;
        }// Objeto()

        public Objeto(string nomeAcessor, string nomeClasse, string nomeOObjeto, object valor, List<Objeto> campos)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeOObjeto, valor);
            this.campos = campos.ToList<Objeto>();
            this.isStatic = false;
        }
        private void InitObjeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
            this.acessor = nomeAcessor;
            this.nome = nomeObjeto;
            this.tipo = nomeClasse;
            this.valor = valor;
            Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);
            
            if (classe != null)
            {
                if ((classe.GetPropriedades() != null) && (classe.GetPropriedades().Count > 0))
                    this.campos = classe.GetPropriedades().ToList<Objeto>();
                else
                    this.campos = new List<Objeto>();
            } 
        }

        public Funcao GetMetodo(string nome)
        {
            return  RepositorioDeClassesOO.Instance().ObtemUmaClasse(this.tipo).GetMetodos().Find(k => k.nome == nome);
        }

        public Objeto GetField(string nome)
        {
            return this.campos.Find(k => k.GetNome() == nome);
        }

        public string GetAcessor()
        {
            return this.acessor;
        }
              
        public void SetAcessor(string acessor)
        {
            this.acessor = acessor;
        }
        public void SetNomeLongo()
        {
            this.nome = this.GetTipo() + "." + this.GetNome();
        }
   
        public void SetValorObjeto(object newValue)
        {
            this.valor = newValue;
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
        public void SetValorField(string nome, object novoValor, Escopo escopo)
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


        public void SetValor(object novoValor, Escopo escopo)
        {
            this.valor = novoValor;
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
        public string GetTipo()
        {
            return this.tipo;
        }

      


        public List<Objeto> GetFields()
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
