using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ModuloTESTES
{
    public class ContainerTestes
    {
        /// <summary>
        /// contém os testes que serão executados no Container currente.
        /// </summary>
        private List<Teste> TestesUnidade;


        /// <summary>
        /// inicializa, executa , dos testes contidos na lista de testes da entrada.
        /// </summary>
        /// <param name="testes">Lista de testes a serem executados.</param>
        public ContainerTestes(params Teste[] testes)
        {
            TestesUnidade = testes.ToList<Teste>();
        } // testes()

        /// <summary>
        /// apenas inicializa o objeto container, não realiza nenhum cálculo.
        /// </summary>
        public ContainerTestes()
        {
            TestesUnidade = new List<Teste>();
        }

        /// <summary>
        /// adiciona um teste para o container de testes.
        /// </summary>
        /// <param name="test"></param>
        public void addTeste(Teste test)
        {
            this.TestesUnidade.Add(test);
        } // addTeste()

        /// <summary>
        /// adiciona uma lista de testes ao container já inicializado.
        /// </summary>
        /// <param name="test"></param>
        public void addTeste(List<Teste> test)
        {
            this.TestesUnidade.AddRange(test);
        } // addTeste()

        /// <summary>
        /// adiciona um vetor variável de Testes ao container de testes, já inicializados.
        /// </summary>
        /// <param name="tests"></param>
        public void addTeste(params Teste[] tests)
        {
            this.TestesUnidade.AddRange(tests);
        }



        /// <summary>
        /// executa os testes, mas não mostra nenhum resultado.
        /// Útil para executar os testes, mas não quiser mostrar
        /// os resultados no Console, preferindo extrair dos testes
        /// cada mensagem de resultado, para colocá-los então em
        /// objetos derivados de Forms, como textField.Text.
        /// </summary>
        public void ExecutaTestes()
        {
            foreach (Teste teste in TestesUnidade)
                try
                {

                    teste.ExecutaTeste();
                    LoggParaResumo(teste, "Sucess: ");
                }  // try
                catch (Exception e)
                {
                    teste.GetAssercoes().Fail("Erro fatal no Teste: " + teste.METHOD.Method.Name + "(Assercoes). " + " Mensagem do Erro: " + e.Message);
                    LoggerTests.AddMessage(e.Message + ":  " + e.ToString() + "  Stack Trace: " + e.StackTrace);
                    LoggParaResumo(teste, "Fail: "); continue;
                } // catch


            GravaLogFails();
        } // ExecutaTestes()


        /// <summary>
        /// escreve no logg de resumo de testes, uma mensagem de falha ou passou.
        /// </summary>
        private static void LoggParaResumo(Teste teste, string mensagem)
        {
           
            string fileLoggMain = LoggerTests.getFileName();
            LoggerTests.SetFileName("Resumo Testes.txt");
            LoggerTests.ClearLoggFile();
            LoggerTests.AddMessage(mensagem + teste.GetNomeDoTeste());
            LoggerTests.SetFileName(fileLoggMain);
        }

        public void ExecutaUmTesteComDadosConfigurados(string bd_dados_arquivo_nome_entrada, Teste testeASerExecutado)
        {
            try
            {
                testeASerExecutado.SetBancoDadosTeste(Teste.CarregaDados(bd_dados_arquivo_nome_entrada));
                testeASerExecutado.ExecutaTeste();
            } // try
            catch (Exception e)
            {
                testeASerExecutado.GetAssercoes().Fail(
                    "Erro no carregamento do arquivo de dados para o teste unitário: " +
                    testeASerExecutado.GetNomeDoTeste() +
                    ". Mensagem de Erro: " + e.Message);
            } // catch
        } // ExecutaUmTesteComDadosConfigurados

        public bool ValidaUmaVariavel(string nomeVariavel, string valorSaida)
        {
            for (int x = 0; x < TestesUnidade.Count; x++)
            {
                List<string> valoresDB = TestesUnidade[x].GetBancoDadosTeste()[nomeVariavel];
                for (int y = 0; y < valoresDB.Count; y++)
                    if (valoresDB[y] == valorSaida)
                        return true;
            }
            return false;
        }
        /// <summary>
        /// grava o arquivo de log de testes que falharam.
        /// </summary>
        private void GravaLogFails()
        {
            // GRAVA UM LOG DE TESTES QUE FALHARAM, ASSIM PODE-SE RODAR APENAS OS TESTES QUE FALHARAM.
            StreamWriter fileLogFails = new StreamWriter("FailsMethods.txt");
            int contador_falhas = 0;
            foreach (Teste teste in TestesUnidade)
                if (teste.TesteFalhou)
                    contador_falhas++;

            string afield = "";

            fileLogFails.WriteLine(contador_falhas.ToString());

            foreach (Teste teste in TestesUnidade)
            {
                if (teste.TesteFalhou)
                {
                    afield = GetDescriptorAndField("Fail TEST NAME", teste.GetNomeDoTeste());
                    fileLogFails.WriteLine(afield);
                } // if
            } // foreach
            fileLogFails.Close();
        } // GravaLogFails




        private string GetDescriptorAndField(string descriptor, string key)
        {
            return ("[" + descriptor + "] " + key);
        }

        private string GetField(string line)
        {
            return (line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }



        /// <summary>
        /// executa os testes e exibe os resultados.
        /// LEMBRAR: os resultados são exibidos no modo Console, se precisar passar em outro modo que não o Console
        /// como um TextArea em um Form, adicione os testes com o método [addTeste], execute o container com 
        /// [ExecutaTestes], e depois leia de cada teste o resultado, através do campo: [mensagens]
        /// (ex., TextField1.Text=umTeste.mensagens).
        /// </summary>
        public void ExecutaTestesEExibeResultados()
        {
            this.ExecutaTestes();
            System.Console.WriteLine("Resultados dos Testes: ");
            foreach (Teste teste in TestesUnidade)
                System.Console.WriteLine(teste.mensagens);
        } // ExecutaTestesEExibeResultados()



        /// <summary>
        /// executa os testes e grava os resultados em arquivo, colocando data da
        /// realização dos testes e a mensagem de resultado para cada teste.
        /// </summary>
        /// <param name="nomeArquivo">nome de arquivo para ser gravado os resultados dos testes.</param>
        public void ExecutaTestesEGuardaEmArquivo(string nomeArquivo)
        {
            this.ExecutaTestes();
            StreamWriter fileLog = new StreamWriter(nomeArquivo);
            fileLog.WriteLine("Resultado dos testes realizados em: " + DateTime.Now.ToShortDateString());
            foreach (Teste umteste in this.TestesUnidade)
            {
                fileLog.WriteLine(umteste.mensagens);
            } // foreach
            fileLog.Close();
        } //ExecutaTestesEGuardaEmArquivo()

    } // class ContainerTestes
}
