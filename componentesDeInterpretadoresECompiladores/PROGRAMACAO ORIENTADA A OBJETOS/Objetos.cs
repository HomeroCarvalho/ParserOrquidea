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
        public object valor;

        public bool isStatic { get; set; }
        private List<Objeto> campos = new List<Objeto>();

        private List<Expressao> expressoes = new List<Expressao>();

      

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
            this.campos = new List<Objeto>();
            this.valor = objeto.valor;
            this.isStatic = objeto.isStatic;
            if ((objeto.campos != null) && (objeto.campos.Count > 0))
                this.campos = objeto.campos.ToList<Objeto>();

          
        }


        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, string nomeCampo, object valorCampo)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, null);
            Objeto campoModificar = this.campos.Find(k => k.GetNome() == nomeCampo);
            campoModificar.SetValor(valorCampo); // aciona a otimização de cálculo de expressões.
            this.isStatic = isStatic;
        }

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = false;
        }// Objeto()

        // inicializa uma instância de um objeto, criando memória para a lista de propriedade, nome do objeto, e o tipo do objeto.
        public Objeto(string nomeAcessor, string nomeClasse, string nomeObjeto, object valor, Escopo escopo, bool isStatic)
        {
            InitObjeto(nomeAcessor, nomeClasse, nomeObjeto, valor);
            this.isStatic = isStatic;
        }// Objeto()

        public Objeto(string nomeAcessor, string nomeClasse, string nomeOObjeto, object valor, List<Objeto> campos, Escopo escopo)
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
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
            
            if (classe != null)
            {
                if ((classe.GetPropriedades() != null) && (classe.GetPropriedades().Count > 0))
                    this.campos = classe.GetPropriedades().ToList<Objeto>();
                else
                    this.campos = new List<Objeto>();
            } 
        }

        public List<Expressao> exprssPresentes()
        {
            return this.expressoes;
        }

        public Funcao GetMetodo(string nome)
        {
            return  RepositorioDeClassesOO.Instance().GetClasse(this.tipo).GetMetodos().Find(k => k.nome == nome);
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
        public void SetNomeLongo(string nomeClasseDaPropriedade)
        {
            this.nome = nomeClasseDaPropriedade + "@" + this.GetNome();
        }
   
        public void SetValorObjeto(object newValue)
        {
            this.valor = newValue;
        }

        public void SetField(Objeto novoField)
        {
            int index=this.campos.FindIndex(k => k.GetNome() == novoField.GetNome());
            if (index != -1)
            {
                this.campos[index] = novoField;
                if (campos[index].expressoes != null)
                    for (int x = 0; x < this.campos[index].expressoes.Count; x++)
                        this.campos[index].expressoes[x].isModify = true;
            }
        }

        /// <summary>
        /// implementa a otimizacao de expressoes. Se uma expressao conter a variavel
        /// que está sendo modificada, a expressao é setada para modificacao=true.
        /// isso auxilia nos calculos de valor da expressao, que é avaliada apenas se 
        /// se alguma variavel-componente da expressao for modificada. Util para
        /// expressoes com variaveis que mudam de valor em tempo de reação humana, ou em tempo-real.
        /// </summary>
        public void SetValorField(string nome, object novoValor)
        {
            if (this.GetField(nome) == null)
                return;

            this.GetField(nome).valor = novoValor;
            int index = this.campos.FindIndex(k => k.tipo == nome);
            if (index != -1)
                for (int x = 0; x < this.campos[index].expressoes.Count; x++)
                    if (campos[index].expressoes[x] != null)
                        this.campos[index].expressoes[x].isModify = true;
    
        } // SetValor()

        /// <summary>
        /// implementa a otimizacao de expressoes. Se uma expressao conter a variavel
        /// que está sendo modificada, a expressao é setada para modificacao=true.
        /// isso auxilia nos calculos de valor da expressao, que é avaliada apenas se 
        /// se alguma variavel-componente da expressao for modificada. Util para
        /// expressoes com variaveis que mudam de valor em tempo de reação humana, ou em tempo-real.
        /// </summary>
        public void SetValor(object novoValor)
        {
            this.valor = novoValor;
            int index = this.campos.FindIndex(k => k.tipo == nome);
            if (index != -1)
                if (expressoes != null)
                    for (int x = 0; x < this.expressoes.Count; x++)
                        this.expressoes[x].isModify = true;

        } // SetValor()


        public static Objeto GetCampo(string classeObjeto, string nomeCampo)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(classeObjeto);
            if (classe == null)
                return null;
            else
            {
                Objeto objetoCampo = classe.GetPropriedades().Find(k => k.GetNome() == nomeCampo);
                if (objetoCampo == null)
                   return null;
                else
                    return new Objeto(objetoCampo);
            }
        }
     
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
