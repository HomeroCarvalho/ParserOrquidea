using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace parser
{
    public class ExtensionLanguage
    {

        private parser.ProcessadorDeID.MetodoTratador handlerBuild;
        private parser.ProgramaEmVM.HandlerInstrucao handlerRun;
        private int codeInstruction;
        private string str_sequencia_id;

        public ExtensionLanguage()
        {
            this.LoadData();
        }


        public ExtensionLanguage(List<string> codigo, parser.ProcessadorDeID.MetodoTratador handlerBuilder, parser.ProgramaEmVM.HandlerInstrucao handlerRuns, int codeInstruction, string str_sequencia)
        {

            this.handlerBuild = handlerBuilder;
            this.handlerRun = handlerRuns;
            this.codeInstruction = codeInstruction;
            this.str_sequencia_id = str_sequencia;
            
        }


        /// <summary>
        /// carrega os dados da extensao da linguagem.
        /// </summary>
        private void LoadData()
        {
            // em classes derivadas, carrega os dados da extensão, como handlers, codigo id, e string id.
        }

        public void ExecuteExtension()
        {
            ProcessadorDeID.LoadHandler(this.handlerBuild, this.str_sequencia_id); // carrega o builder da instrução!
            ProgramaEmVM.dicHandlers[codeInstruction] = this.handlerRun; // carrega o Runner da instrução!
            ProgramaEmVM.codesInstructions.Add(this.codeInstruction); // cria o id do Runner da instrução!
        }

        
    }
    public class ExtensionLanguageIntoFile
    {
        /// <summary>
        /// carrega extensoes de linguagem, a partir de um arquivo assembly. Separa as extensoes de instruções do codigo Parser Orquidea.
        /// </summary>
        public void Extend(string pathAssembly)
        {
            Assembly assembly_extension = Assembly.LoadFile(pathAssembly);
            Type[] types_of_assembly = assembly_extension.GetTypes();
            foreach (Type type_for_location_extension in types_of_assembly)
            {
                if (type_for_location_extension.IsSubclassOf(typeof(ExtensionLanguage))) 
                {
                    ExtensionLanguage objeto_target = new ExtensionLanguage();
                    objeto_target.ExecuteExtension();
                } // if
            } // foreach

        }
    }

}
