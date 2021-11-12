using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace parser.ProgramacaoOrentadaAObjetos
{

    /// <summary>
    /// Repositorio de classes de um projeto OO Orquidea.
    /// </summary>
    public class RepositorioDeClassesOO
    {
        /// <summary>
        /// constem as classes publicas do projeto.
        /// </summary>
        private List<Classe> classesRegistradas { get; set; }

        private List<Classe> interfacesRegistradas { get; set; }

        /// <summary>
        /// padrão de projeto Singleton para o Repositorio, que deve ser um para o projeto. 
        /// Isto evita instanciações várias, que podem registrar uma classe em diferentes repositorios.
        /// </summary>
        private static RepositorioDeClassesOO instanciaRepositorio { get; set; }


        /// <summary>
        /// construtor privado, para o Repositorio.
        /// </summary>
        private RepositorioDeClassesOO()
        {
            this.classesRegistradas = new List<Classe>();
            this.interfacesRegistradas = new List<Classe>();

        } // RepositorioDeClassesOO()

        /// <summary>
        /// método de instanciação do Repositório, melhhor, ponto de acesso ao Repositório.
        /// </summary>
        /// <returns></returns>
        public static RepositorioDeClassesOO Instance()
        {
            if (instanciaRepositorio == null)
                instanciaRepositorio = new RepositorioDeClassesOO();
            return instanciaRepositorio;
        } // Instance().

        public List<Classe> GetClasses()
        {
            return this.classesRegistradas;
        }

        public List<Classe> GetInterfaces()
        {
            return this.interfacesRegistradas;
        }
        public bool isOperator(string nameClass, string nameOperator, string tipo)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nameClass);

            // tenta obter um indice do operador dentro da lista de operadores da claase de entrada.
            List<Operador> operadores = classe.GetOperadores().FindAll(k => k.nome == nameOperator);
            if (operadores == null)
                return false;
            Operador operador = operadores.Find(k => k.GetTipo().Contains(tipo));
            if (operador == null)
                return false;
            return true;
        } // isOperator()

        /// <summary>
        /// se o nome da classe não estiver em qualquer classe no reposiório, adiciona a classe de entrada ao repositório, .
        /// Se o nome da classe já estiver no repositorio, remove a classe que estava no repositorio, e adiciona a classe da entrada.
        /// </summary>
        /// <param name="classe">classe a ser registrada.</param>
        /// <returns>retorna [true] se a inserção foi sucedida, [false] se houver outra classe com mesmo nome da classe de entrada.</returns>
        public bool RegistraUmaClasse(Classe classe)
        {
            
            if (classe.GetNome() == null)
                return false;
            int indexClasse = classesRegistradas.FindIndex(k => k.nome == classe.nome);
            if (indexClasse != -1)
            {
                this.classesRegistradas.Remove(classesRegistradas[indexClasse]);
                this.classesRegistradas.Add(classe);
                return true;
            } // if
            // registra a classe no repositório de classes.
            this.classesRegistradas.Add(classe);
            return true;
        } // RegistraUmaClasse()
        
        public bool RegistraUmaInterface(Classe umaInterface)
        {
            if (interfacesRegistradas.Find(k => k.nome == umaInterface.nome) != null)
                return false;
            else
            {
                this.interfacesRegistradas.Add(umaInterface);
                return true;
            }
        } // RegistraUmaInterface()
    
        /// <summary>
        /// obtém uma classe no Repositório, com  o nome [nomeClasse].
        /// </summary>
        public Classe GetClasse(string nomeClasse)
        {
            return classesRegistradas.Find(k => k.nome == nomeClasse);
        } // ObtemUmaClasse()

        public Classe GetInterface(string nomeInterface)
        {
            return interfacesRegistradas.Find(k => k.nome == nomeInterface);
        }

        /// <summary>
        /// obtém uma classe no Repositório, com  o nome [nomeClasse].
        /// </summary>
        public Classe ObtemUmaInterface(string nomeInterface)
        {
            return interfacesRegistradas.Find(k => k.nome == nomeInterface);
        } // ObtemUmaInterface()


      

        public Operador GetOperador(string nomeOperador, string tipoDeRetorno)
        {
            foreach (Classe umaClasse in classesRegistradas)
            {

                List<Operador> operadores = umaClasse.GetOperadores().FindAll(k => k.nome == nomeOperador);
                Operador operador = operadores.Find(k => k.tipoRetorno == tipoDeRetorno);
                if (operador != null)
                    return operador;
            }
            return null;
        } // GetOperador()

        
    } //class RepositorioDeClassesOO
} // namespace
