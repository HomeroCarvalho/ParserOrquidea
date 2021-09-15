using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ModuloTESTES;
namespace parser
{
    public class SuiteClasseTestes 
    {
        private delegate void MetodoTeste(AssercaoSuiteClasse assercao);

        private List<MethodInfo> metodosTeste { get; set; }
        private MethodInfo metodoAntes { get; set; }
        private MethodInfo metodoDepois { get; set; }

        private string infoTextoCabacalho { get; set; }
    

  
        public SuiteClasseTestes(string infoTextoNomeClasse)
        {
            this.infoTextoCabacalho = infoTextoNomeClasse;      
            this.metodosTeste = new List<MethodInfo>();

            List<MethodInfo> metodos = this.GetType().GetMethods().ToList<MethodInfo>();

            foreach (MethodInfo umMetodo in metodos) 
            {
                List<ParameterInfo> parametrosDoMetodo = umMetodo.GetParameters().ToList<ParameterInfo>();

                if ((parametrosDoMetodo != null) && (parametrosDoMetodo.Count > 0) && (parametrosDoMetodo[0].ParameterType == typeof(AssercaoSuiteClasse))) 
                    this.metodosTeste.Add(umMetodo);

                if (umMetodo.Name.Equals("Antes"))
                    this.metodoAntes = umMetodo;

                if (umMetodo.Name.Equals("Depois"))
                    this.metodoDepois = umMetodo;
            }

            
        }

        public void ExecutaTestes()
        {
            
            if (metodosTeste == null)
            {
                LoggerTests.AddMessage("Nao ha testes a serem executados nesta classe para testes.");
                return;
            }

            AssercaoSuiteClasse assercao = new AssercaoSuiteClasse();

            LoggerTests.WriteEmptyLines();

            LoggerTests.AddMessage(this.infoTextoCabacalho);

            foreach (MethodInfo metodo in metodosTeste)
            {
                try
                {
                    int indiceAssercaoStart = AssercaoSuiteClasse.contadorValidacoes; 
                    int timeStart = DateTime.Now.Millisecond;


                    if (metodoAntes != null)
                        metodoAntes.Invoke(this, null); // executa o metodo preparador para o teste.

                    if ((metodo.Name != "Antes") && (metodo.Name != "Depois"))
                        metodo.Invoke(this, new object[] { assercao}); // executa o teste. (nao contem parametros).

                    if (metodoDepois != null)
                        metodoDepois.Invoke(this, null); // executa o metodo finalizador para o teste.

                    int timeEnd = DateTime.Now.Millisecond;
                    int timeElapsed = timeEnd - timeStart; // calcula o tempo gasto.

                    int indiceAssercaoEnd = AssercaoSuiteClasse.contadorValidacoes;

                    for (int x = indiceAssercaoStart; x < indiceAssercaoEnd; x++) 
                    {
                        string resumoDoTesteEmUmMetodo = "teste: " + metodo.Name + " executado em: " + timeElapsed.ToString() + "  milisegundos." + "   " + AssercaoSuiteClasse.validacoesFeitas[x];
                        LoggerTests.AddMessage(resumoDoTesteEmUmMetodo);
                    }
                }
                catch (Exception exc)
                {
                    LoggerTests.AddMessage("teste: " + metodo.Name + ", na classe: " + this.GetType().Name + " gerou excecao que interrompeu o seu processamento."+" falha porque: "+exc.Message+", Stack: "+exc.StackTrace);
                    LoggerTests.WriteEmptyLines();
                    continue;
                }

            }

            LoggerTests.WriteEmptyLines();
        }

     
    } // class

    public class AssercaoSuiteClasse
    {
        public static List<string> validacoesFeitas { get; private set; }
        public static int contadorValidacoes { get; private set; }


        public bool IsTrue(bool condicaoValidacao)
        {
            if (AssercaoSuiteClasse.validacoesFeitas == null)
            {
                AssercaoSuiteClasse.validacoesFeitas = new List<string>();
            }

            if (condicaoValidacao)
            {
                validacoesFeitas.Add("teste passou");
                contadorValidacoes++;
                return true;
            }
            if (!condicaoValidacao)
            {
                validacoesFeitas.Add("teste nao passou.");
                contadorValidacoes++;
                return false;
            }

            return false;
        }


    }
} // namespace
