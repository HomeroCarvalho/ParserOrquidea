using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
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


        public TemporizadorParaDesempenho medicaoDesempenho = new TemporizadorParaDesempenho();

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


            medicaoDesempenho.AddTemporizador(200, "Desempenho total do cenario de teste: ");
            foreach (MethodInfo metodo in metodosTeste)
            {
                try
                {
                    TemporizadorParaDesempenho temporizadorUmMetodo = new TemporizadorParaDesempenho();

                    temporizadorUmMetodo.AddTemporizador(150, "Desempenho do método-teste: ");

                    int indiceAssercaoStart = AssercaoSuiteClasse.contadorValidacoes;


                    medicaoDesempenho.Begin(150);


                    if (metodoAntes != null)
                        metodoAntes.Invoke(this, null); // executa o metodo preparador para o teste.

                    if ((metodo.Name != "Antes") && (metodo.Name != "Depois"))
                        metodo.Invoke(this, new object[] { assercao }); // executa o teste. (nao contem parametros).

                    if (metodoDepois != null)
                        metodoDepois.Invoke(this, null); // executa o metodo finalizador para o teste.

                    medicaoDesempenho.End(150);

                    int indiceAssercaoEnd = AssercaoSuiteClasse.contadorValidacoes;

                    for (int x = indiceAssercaoStart; x < indiceAssercaoEnd; x++)
                    {
                        string resumoDoTesteEmUmMetodo = "teste: " + metodo.Name + " executado em: " +  + medicaoDesempenho.GetTimeElapsed(150) + "  mls.   " + AssercaoSuiteClasse.validacoesFeitas[x];
                        LoggerTests.AddMessage(resumoDoTesteEmUmMetodo);
                    }
                }
                catch (Exception exc)
                {
                    LoggerTests.AddMessage("teste: " + metodo.Name + ", na classe: " + this.GetType().Name + " gerou excecao que interrompeu o seu processamento." + " falha porque: " + exc.Message + ", Stack: " + exc.StackTrace);
                    LoggerTests.WriteEmptyLines();
                    continue;
                }

            }

            medicaoDesempenho.End(150);

            LoggerTests.WriteEmptyLines();


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
    }
    public class TemporizadorParaDesempenho
    {

        private static List<dataTemporizador> temporizadores { get; set; }

        public TemporizadorParaDesempenho()
        {
            if (temporizadores == null)
                temporizadores = new List<dataTemporizador>();


        }

        public long GetTimeElapsed(int id)
        {
            dataTemporizador dataTimer = temporizadores.Find(k => k.id == id);
            if (dataTimer != null)
                return dataTimer.timeElapsed;
            else
                return -1;

        }
        public void AddTemporizador(int id, string mensagemInformandoAMedicao)
        {
            dataTemporizador data = new dataTemporizador(id, mensagemInformandoAMedicao);
            TemporizadorParaDesempenho.temporizadores.Add(data);
        }

        public void Begin(int id)
        {

            dataTemporizador temporizador = temporizadores.Find(k => k.id == id);
            if (temporizador == null)
                throw new Exception("id de timer inexistente");
            else
                temporizador.Begin();
        }

        public void End(int id)
        {
            dataTemporizador temporizador = temporizadores.Find(k => k.id == id);
            if (temporizador == null)
                throw new Exception("id de timer inexistente");
            else
            {
                temporizador.End();
                LoggerTests.AddMessage(temporizador.mensagemExplicativaFinalidadeMensuracaoTempo + "  tempo: " + temporizador.timeElapsed+" mlsg.");
            }
        }
    }


    public class dataTemporizador
    {
        public int id { get; set; }

        public string mensagemExplicativaFinalidadeMensuracaoTempo;


        public Stopwatch temporizador { get; set; }

        public long timeElapsed = 0;

        public dataTemporizador(int id, string mensagemExplicativa)
        {
            this.id = id;
            this.mensagemExplicativaFinalidadeMensuracaoTempo = (string)mensagemExplicativa.Clone();
            this.temporizador = new Stopwatch();
        }

        public void Begin()
        {
            temporizador = new Stopwatch();
            temporizador.Start();
        }

        public void End()
        {
            temporizador.Stop();
            this.timeElapsed = temporizador.ElapsedMilliseconds;
        }
    }

} // namespace
