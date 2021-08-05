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
            List<propriedade> propriedadesParaOrquidea = new List<propriedade>();

            RegistraMetodosImportados(metodosImportados, metodosParaOrquidea);
            RegistraPropriedadesImportadas(propriedadesImportadas, propriedadesParaOrquidea);
            RegistraCamposImportados(camposImportados, propriedadesParaOrquidea);
          

            Classe classeImportada = new Classe("public", myType.Name, metodosParaOrquidea, new List<Operador>(), propriedadesParaOrquidea);
            RegistraConstrutoresImportados(construtoresImportados, classeImportada);


            RegistraPropriedadesEstaticasImportadas(camposImportados, classeImportada);
            RepositorioDeClassesOO.Instance().RegistraUmaClasse(classeImportada); // registra a classe como uma classe orquidea! seus metodos e propriedades podem ser escritos em codigo para o compilador, nao havera diferença...

        }

        private void RegistraConstrutoresImportados(ConstructorInfo[] construtoresImportados, Classe classeImportada)
        {
            classeImportada.construtores = new List<Funcao>();
            foreach (var umConstrutor in construtoresImportados)
            {
                ParameterInfo[] parametrosDoConstrutor = umConstrutor.GetParameters();
                List<propriedade> parametros = new List<propriedade>();
                ObtemOsParametrosDoMetodoImportado(umConstrutor, parametros);
                string acessor = "";
                if (umConstrutor.IsPublic)
                    acessor = "public";
                else
                if (umConstrutor.IsPrivate)
                    acessor = "private";
                else
                    acessor = "protected";

                classeImportada.construtores.Add(new Funcao(umConstrutor.DeclaringType.Name, acessor, umConstrutor.Name, umConstrutor, umConstrutor.DeclaringType.Name, null, parametros.ToArray()));
            }

        }

        private void RegistraMetodosImportados(MethodInfo[] metodosImportados, List<Funcao> metodosOrquidea)
        {
            List<propriedade> parametros = new List<propriedade>();
            foreach(MethodInfo umMetodo in metodosImportados)
            {
                ObtemOsParametrosDoMetodoImportado(umMetodo, parametros);
                string acessor = GetMethodAcessor(umMetodo);

              
                // a função importada é adicionada na lista de métodos para a classe importada.
                metodosOrquidea.Add(new Funcao(umMetodo.DeclaringType.Name, acessor, umMetodo.Name, umMetodo, umMetodo.DeclaringType.Name, parametros.ToArray()));


            } // for x
        }
        

        private void RegistraPropriedadesEstaticasImportadas(FieldInfo[] camposImportados, Classe classeImportada)
        {
            for (int x = 0; x < camposImportados.Length; x++)
                // os campos estáticos são importador, para a lista de variáveis orquidea estáticas na classe a ser importada.
                if (camposImportados[x].IsStatic)
                {
                    string acessor = GetFieldAcessor(camposImportados, x);
                    classeImportada.propriedadesEstaticas.Add(new Variavel(acessor, camposImportados[x].Name, camposImportados[x].FieldType.Name, null));
                } // if
        }

        private void RegistraCamposImportados(FieldInfo[] camposImportados, List<propriedade> propriedadesOrquidea)
        {
            // os campos não estáticos são importados, para a lista de propriedades orquidea.
            for (int x = 0; x < camposImportados.Length; x++)
                if (!camposImportados[x].IsStatic)
                {
                    string acessor = GetFieldAcessor(camposImportados, x);
                    propriedadesOrquidea.Add(new propriedade(acessor, camposImportados[x].Name, camposImportados[x].FieldType.Name, null, false));
                } // if
        }



        private void RegistraPropriedadesImportadas(PropertyInfo[] propriedadesImportadas, List<propriedade> propriedadesOrquidea)
        {
            // as propriedades são importadas, para a lista de propriedades orquidea.
            for (int x = 0; x < propriedadesImportadas.Length; x++)
                propriedadesOrquidea.Add(new propriedade("public", propriedadesImportadas[x].Name, propriedadesImportadas[x].PropertyType.Name, null, false));
        }



        private void ObtemOsParametrosDoMetodoImportado(MethodInfo metodoImportado, List<propriedade> parametros)
        {
            ParameterInfo[] parametrosInfo = metodoImportado.GetParameters();
            for (int k = 0; k < parametrosInfo.Length; k++)
            {
                string tipoParametroCasting = UtilTokens.Casting(parametrosInfo[k].ParameterType.Name);
                parametros.Add(new propriedade(parametrosInfo[k].Name, tipoParametroCasting, null, false));
            }
        }

        private void ObtemOsParametrosDoMetodoImportado(ConstructorInfo construtorImportado, List<propriedade> parametros)
        {
         
            
            ParameterInfo[] parametrosInfo = construtorImportado.GetParameters();
            for (int k = 0; k < parametrosInfo.Length; k++)
            {
                string tipoParametroCasting = UtilTokens.Casting(parametrosInfo[k].ParameterType.Name);
                parametros.Add(new propriedade(parametrosInfo[k].Name, tipoParametroCasting, null, false));
            }
        }

   
        private string GetMethodAcessor(MethodInfo metodosImportados)
        {

            // obtem o tipo de acessor do método.
            string acessor = "";
            if (metodosImportados.IsPublic)
                acessor = "public";
            else
            if (metodosImportados.IsPrivate)
                acessor = "private";
            else
                acessor = "protected";
            return acessor;
        }

        private string GetFieldAcessor(FieldInfo[] camposImportados, int x)
        {
            string acessor = "";
            if (camposImportados[x].IsPublic)
                acessor = "public";
            if (camposImportados[x].IsPrivate)
                acessor = "private";
            return acessor;
        }


    } // class
} // namespace
