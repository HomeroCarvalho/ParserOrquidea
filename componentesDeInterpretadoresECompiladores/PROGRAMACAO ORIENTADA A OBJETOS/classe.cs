using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using System.Reflection;
using System;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// bloco básico para linguagens orientada a objeto, como o orquidea.
    /// </summary>
    public class Classe
    {

        public string nome { get; set; }
        public string nomeLongo { get; set; }
        public string acessor { get; set; }
        public Escopo escopoDaClasse { get; set; }
        public List<string> tokensDaClasse { get; set; }


        public delegate object UmMetodoDaClasse(params object[] parametros);


        public List<Classe> classesHerdadas { get; set; }
        public List<Classe> interfacesHerdadas { get; set; }

        public List<Instrucao> instrucoesDoCorpoDaClasse { get; set; }
        public List<Funcao> construtores { get; set; }


        public List <Objeto> propriedadesEstaticas { get; set; }
        public string GetNome()
        {
            return nome;
        }
        public List<Funcao> GetMetodos()
        {
            return metodos;
        }

        public List<Funcao> GetMetodo(string nomeMetodo)
        {
            return metodos.FindAll(k => k.nome.Equals(nomeMetodo));
        }
        public List<Operador> GetOperadores()
        {
            return operadores;
        }
        public List<Objeto> GetPropriedades()
        {
            return propriedades;
        }

        public Objeto GetPropriedade(string nomeProp)
        {
            return propriedades.Find(k => k.GetNome() == this.nome + "." + nomeProp);
        }

        public Operador GetOperador(string nomeOperador)
        {
            return operadores.Find(k => k.nome.Equals(nomeOperador));
        }

        private List<Funcao> metodos = new List<Funcao>();
        private List<Objeto> propriedades = new List<Objeto>();
        private List<Operador> operadores = new List<Operador>();


        private static LinguagemOrquidea linguagem;


        MethodInfo[] metodoReflexao { get; set; }
        ConstructorInfo[] construtoresReflexao { get; set; }
        
        public Classe()
        {
            if (linguagem == null)
                linguagem = new LinguagemOrquidea();
            this.metodos = new List<Funcao>();
            this.operadores = new List<Operador>();
            this.propriedades = new List<Objeto>();
            this.tokensDaClasse = new List<string>();
            this.construtores = new List<Funcao>();
            this.propriedadesEstaticas = new List<Objeto>();
            this.classesHerdadas = new List<Classe>();
            this.interfacesHerdadas = new List<Classe>();
            this.GetInfoReflexao();
        }
        public Classe(string acessor, string name, List<Funcao> methods, List<Operador> operadores, List<Objeto> propriedades):base()
        {
            if (linguagem == null)
                linguagem = new LinguagemOrquidea();

            this.acessor = acessor;
            this.nome = name;
            this.tokensDaClasse = new List<string>();
            this.metodos = new List<Funcao>();
            this.propriedades = new List<Objeto>();
            this.propriedadesEstaticas = new List<Objeto>();
            this.operadores = new List<Operador>();
            this.construtores = new List<Funcao>();
            this.classesHerdadas = new List<Classe>();
            this.interfacesHerdadas = new List<Classe>();
            // adiciona os métodos da classe.
            if (methods != null)
                for (int i = 0; i < methods.Count; i++)
                    this.metodos.Add(methods[i]);

            // adiciona as propriedades da classe.
            if (propriedades != null)
                for (int i = 0; i < propriedades.Count; i++)
                    this.propriedades.Add(propriedades[i]);

            // adiciona os operadores da classe.
            if (operadores != null)
                for (int i = 0; i < operadores.Count; i++)
                {
                    operadores[i].tipoRetorno = this.nome; // seta a classe a qual o operador pertence.
                    this.operadores.Add(operadores[i]);
                }
            this.GetInfoReflexao();
       } // constructor


        private void GetInfoReflexao()
        {
            string pathAssembly = Path.GetFullPath("ParserLinguagemOrquidea.exe");
            Assembly assemblyParserOrquidea = Assembly.LoadFrom(pathAssembly);

            List<Type> types = assemblyParserOrquidea.GetTypes().ToList<Type>();
            Type tipoDaClasse = types.Find(k => k.Name.Equals(this.nome));
            
            
            if (tipoDaClasse != null)
            {
                this.metodoReflexao = tipoDaClasse.GetMethods();
                this.construtoresReflexao = tipoDaClasse.GetConstructors();
            }
        }

        /// <summary>
        ///  encontra o tipo de um elemento.
        ///  ---> se o elemento for um operador, retorna o termo "operador", o que é suficiente para validar com null para elementos não encontrados.
        ///  ---> se o elemento for um nome de função, retorna o termo "funcao", o que é suficiente para validar com null para elementos não encontrados.
        ///  ---> se o elemento for um numero, retorna o termo "numero", o que é suficiente para validar com null para elementos não encontrados.
        ///  ---> se o elemento for um termoChave, retorna o termo "termoChave", o que é suficiente para validar com null para elementos não encontrados.
        ///  ---> o elemento for uma varivel, variavel vetor, objeto, ou nome de classe, retorna o tipo do elemento.
        /// </summary>
        public static string EncontraClasseDoElemento(string nomeElemento, Escopo escopo)
        {

            if (linguagem.IsOperadorBinario(nomeElemento))
                return "operador";
            if (linguagem.IsOperadorUnario(nomeElemento))
                return "operador";
            if (linguagem.IsOperadorAritmetico(nomeElemento))
                return "operador";
            if (linguagem.IsOperadorCondicional(nomeElemento))
                return "operador";

            if (escopo.tabela.IsFunction(escopo, nomeElemento))
                return "funcao";
            if (linguagem.isTermoChave(nomeElemento))
                return "termoChave";

            if (Expressao.Instance.IsTipoInteiro(nomeElemento))
                return "Int32";
            if (Expressao.Instance.IsTipoFloat(nomeElemento))
                return "float";
            if (Expressao.Instance.IsTipoDouble(nomeElemento))
                return "double";

            Objeto v = escopo.tabela.GetObjeto(nomeElemento, escopo);
            if (v != null)
                return v.GetTipo();
            Vetor vetor = escopo.tabela.GetVetor(nomeElemento, escopo);
            if (vetor != null)
                return v.GetTipo();
            Objeto objeto = escopo.tabela.GetObjeto(nomeElemento, escopo);
            if (objeto != null)
                return objeto.GetTipo();

            Classe classeElemento = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeElemento);
            if (classeElemento != null)
                return classeElemento.nome;

            return null;
        }


        /// <summary>
        /// faz uma chamada do método constructor de objeto importados, que tem os parâmetros especificados na entrada.
        /// </summary>
        public object CallConstructor(Type tipoClasse, object[] parametrosConstructor)
        {
            ConstructorInfo[] construtores = tipoClasse.GetConstructors();
            if (construtores.Length > 0)
                return construtores[0].Invoke(parametrosConstructor);
            else
                return null;
        } // GetConstructor()

        /// faz uma chamada de um método via reflexao.
        public object CallAMethod(object objectCaller, string nameMethod, object[] parametersCall)
        {
            Funcao metodoInvocado = this.GetMetodos().Find(k => k.nome.Equals(nameMethod));
            return metodoInvocado.InfoMethod.Invoke(objectCaller, parametersCall);
        }

        /// <summary>
        /// salva os tokens da classe.
        /// </summary>
        public void Save()
        {
            Stream stream = new FileStream("classe_" + this.nome + ".txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            for (int x = 0; x < this.tokensDaClasse.Count; x++)
            {
                writer.Write(this.tokensDaClasse[x] + " ");
            } // for x

            writer.Close();
            stream.Close();

            writer.Dispose();
            stream.Dispose();
        }  // Save()

        /// <summary>
        /// carrega uma classe em um arquivo.
        /// </summary>
        /// <param name="nomeClasse">nome da classe a carregar.</param>
        /// <returns></returns>
        public static Classe Load(string nomeClasse)
        {
            LinguagemOrquidea linguage = new LinguagemOrquidea();
            Stream stream = new FileStream("classe_" + nomeClasse + ".txt", FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);

            // le todos tokens da classe, em uma linha só.
            string todosTokensDaClasse = reader.ReadLine();
            todosTokensDaClasse = todosTokensDaClasse.Trim(' ');

            // cria um extrator de classes, métodos e propriedades.
            ExtratoresOO extratores = new ExtratoresOO(new Escopo(new List<string>() { todosTokensDaClasse }), linguage, new List<string>() { todosTokensDaClasse });
            Classe umaClasse = extratores.ExtaiUmaClasse();
            if (umaClasse == null)
                return null;
            reader.Close();
            stream.Close();

            reader.Dispose();
            stream.Dispose();
            return umaClasse;
        } // Load()


        public override string ToString()
        {
            string str = "classe: " + this.nome + "\n";
            if ((this.propriedades != null) && (this.propriedades.Count > 0))
            {
                str += "  propriedades: \n";
                for (int x = 0; x < this.propriedades.Count; x++)
                    str += this.propriedades[x].GetNome() + ", tipo:  " + this.propriedades[x].GetTipo()+ "\n";
            } // if

            if ((this.metodos != null) && (this.metodos.Count > 0))
            {
                str += "  metodos: \n";
                for (int x = 0; x < this.metodos.Count; x++)
                    str += this.metodos[x].ToString();
            } // if
            return str;
        } // ToString()

    } // class Classe

} // namespace parser
