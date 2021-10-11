using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// obtém classes da linguagem de suporte, e as transfere para o repositório de classes do orquidea.
    /// importa métodos, campos e propriedades da classe.
    /// </summary>
    class ImportadorDeClasses
    {
        private Assembly assemblyToImporter { get; set; }
 
        public ImportadorDeClasses(string aplicacaoEXEouBibliotecaDLL)
        {

            string pathAssembly = Path.GetFullPath(aplicacaoEXEouBibliotecaDLL);
            this.assemblyToImporter = Assembly.LoadFile(pathAssembly);
            this.ImportAllClassesFromAssembly();

        }

        public ImportadorDeClasses()
        {
            // do nothing, for calls methods importer one type.
        }
        /// <summary>
        /// importa métodos e propriedades de uma classe, vinda de um objeto Assembly
        /// </summary>
        public void ImportAClassFromAssembly(string nameClass)
        {
            if (this.assemblyToImporter != null) 
            {
                Type myType = this.assemblyToImporter.GetType(nameClass);
                if (myType != null)
                    TransfereMetodosEPropriedadesParaOReposicorioDeClassesOrquidea(myType);
            }
            else
                throw new Exception("Tentativa de obter uma classe: "+nameClass+" de um objeto Assmebly null. Checar se o nome da classe e o nome do Assembly (.exe ou .dll) está no diretorio da Aplicaçao.");
        }

        public void ImportAllClassesFromAssembly()
        {
            Type[] classesDaAssembly = this.assemblyToImporter.GetTypes();
            foreach (Type umaClasseDaAssembly in classesDaAssembly)
                if (!umaClasseDaAssembly.Name.Contains("<"))
                    this.ImportAClassFromApplication(umaClasseDaAssembly);
            
        }

        public void ImportAClassFromApplication(Type umTipoClasse)
        {
            TransfereMetodosEPropriedadesParaOReposicorioDeClassesOrquidea(umTipoClasse);
        }

        private void TransfereMetodosEPropriedadesParaOReposicorioDeClassesOrquidea(Type myType)
        {
            MethodInfo[] metodosImportados = myType.GetMethods();
            PropertyInfo[] propriedadesImportadas = myType.GetProperties();
            FieldInfo[] camposImportados = myType.GetFields();
            ConstructorInfo[] construtoresImportados = myType.GetConstructors();

         



            List<Funcao> metodosParaOrquidea = new List<Funcao>();
            List<Funcao> construtoresParaOrquidea = new List<Funcao>();
            List<Objeto> propriedadesParaOrquidea = new List<Objeto>();

            
            RegistraPropriedadesImportadas(propriedadesImportadas, propriedadesParaOrquidea);
            RegistraCamposImportados(camposImportados, propriedadesParaOrquidea);
            RegistraMetodosImportados(metodosImportados, metodosParaOrquidea);

            Classe classeImportada = new Classe("public", myType.Name, metodosParaOrquidea, new List<Operador>(), propriedadesParaOrquidea);
            RegistraConstrutoresImportados(construtoresImportados, classeImportada);


            RegistraPropriedadesEstaticasImportadas(camposImportados, classeImportada);
            RepositorioDeClassesOO.Instance().RegistraUmaClasse(classeImportada); // registra a classe como uma classe orquidea! seus metodos e propriedades podem ser escritos em codigo para o compilador, nao havera diferença...

        }

        private void RegistraConstrutoresImportados(ConstructorInfo[] infoConstrutores, Classe classe)
        {
            classe.construtores = new List<Funcao>();
            foreach (var umConstrutor in infoConstrutores)
            {
                ParameterInfo[] parametrosDoConstrutor = umConstrutor.GetParameters();
                List<Objeto> parametros = new List<Objeto>();
                ObtemOsParametrosDoMetodoImportado(umConstrutor, parametros);
                string acessor = "";
                if (umConstrutor.IsPublic)
                    acessor = "public";
                else
                if (umConstrutor.IsPrivate)
                    acessor = "private";
                else
                    acessor = "protected";

                classe.construtores.Add(new Funcao(umConstrutor.DeclaringType.Name, acessor, umConstrutor.Name, umConstrutor, umConstrutor.DeclaringType.Name, null, parametros.ToArray()));
            }

        }

        private void RegistraMetodosImportados(MethodInfo[] infoMetodos, List<Funcao> metodos)
        {
            List<Objeto> parametros = new List<Objeto>();
            foreach(MethodInfo umMetodo in infoMetodos)
            {
                ObtemOsParametrosDoMetodoImportado(umMetodo, parametros);
                string acessor = GetMethodAcessor(umMetodo);

              
                // a função importada é adicionada na lista de métodos para a classe importada.
                metodos.Add(new Funcao(umMetodo.DeclaringType.Name, acessor, umMetodo.Name, umMetodo, umMetodo.DeclaringType.Name, parametros.ToArray()));


            } 
        }
        

        private void RegistraPropriedadesEstaticasImportadas(FieldInfo[] infoCampos, Classe classeImportada)
        {
            for (int x = 0; x < infoCampos.Length; x++)
                // os campos estáticos são importador, para a lista de variáveis orquidea estáticas na classe a ser importada.
                if (infoCampos[x].IsStatic)
                {
                    string acessor = GetFieldAcessor(infoCampos, x);
                    classeImportada.propriedadesEstaticas.Add(new Objeto(acessor, infoCampos[x].Name, classeImportada.GetNome(), null));
                } // if
        }

        private void RegistraCamposImportados(FieldInfo[] infoCampos, List<Objeto> propriedades)
        {
            // os campos não estáticos são importados, para a lista de propriedades orquidea.
            for (int x = 0; x < infoCampos.Length; x++)
                if (!infoCampos[x].IsStatic)
                {
                    string acessor = GetFieldAcessor(infoCampos, x);
                    propriedades.Add(new Objeto(acessor, infoCampos[x].FieldType.Name, infoCampos[x].Name, null, null, isStatic: false));
                } // if
        }



        private void RegistraPropriedadesImportadas(PropertyInfo[] infoPropriedades, List<Objeto> propriedades)
        {
            // as propriedades são importadas, para a lista de propriedades orquidea.
            for (int x = 0; x < infoPropriedades.Length; x++)
                propriedades.Add(new Objeto("public", infoPropriedades[x].PropertyType.Name, infoPropriedades[x].Name, null, null, isStatic: false));
        }



        private void ObtemOsParametrosDoMetodoImportado(MethodInfo infoMetodos, List<Objeto> parametrosMetodo)
        {
            ParameterInfo[] parametrosInfo = infoMetodos.GetParameters();
            for (int k = 0; k < parametrosInfo.Length; k++)
            {
                string tipoParametroCasting = UtilTokens.Casting(parametrosInfo[k].ParameterType.Name);
                parametrosMetodo.Add(new Objeto("private", tipoParametroCasting, parametrosInfo[k].Name, null, null, isStatic: false));
            }
        }

        private void ObtemOsParametrosDoMetodoImportado(ConstructorInfo infoConstrutor, List<Objeto> parametrosConstrutor)
        {
         
            
            ParameterInfo[] parametrosInfo = infoConstrutor.GetParameters();
            for (int k = 0; k < parametrosInfo.Length; k++)
            {
                string tipoParametroCasting = UtilTokens.Casting(parametrosInfo[k].ParameterType.Name);
                parametrosConstrutor.Add(new Objeto("private", tipoParametroCasting, parametrosInfo[k].Name, null, null, isStatic: false));
            }
        }

   
        private string GetMethodAcessor(MethodInfo infoMetodo)
        {

            // obtem o tipo de acessor do método.
            string acessor = "";
            if (infoMetodo.IsPublic)
                acessor = "public";
            else
            if (infoMetodo.IsPrivate)
                acessor = "private";
            else
                acessor = "protected";
            return acessor;
        }

        private string GetFieldAcessor(FieldInfo[] infoCampos, int x)
        {
            string acessor = "";
            if (infoCampos[x].IsPublic)
                acessor = "public";
            if (infoCampos[x].IsPrivate)
                acessor = "private";
            return acessor;
        }


    } // class
} // namespace
