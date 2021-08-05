using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ModuloTESTES
{
    public class Teste
    {

        /// <summary>
        /// banco de dados simples para ser utilizado no teste do objeto Assercoes currente.
        /// </summary>
        private Dictionary<string, List<string>> BancoDeDadosTeste = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> GetBancoDadosTeste()
        {
            return BancoDeDadosTeste;
        }

        public void SetBancoDadosTeste(Dictionary<string, List<string>> bancoDB)
        {
            this.BancoDeDadosTeste = bancoDB;
        }

        private string NomeTeste;

        public string GetNomeDoTeste()
        {
            return this.NomeTeste;
        }
        /// <summary>
        /// recebe mensagens de objetos como Assercoes, útil para automação e simplificação do framework.
        /// </summary>
        public string mensagens { get; set; }

        /// <summary>
        /// objeto que faz os testes de condição sobre o teste objeto.
        /// </summary>
        private Assercoes assercao { get; set; }

        public Assercoes GetAssercoes()
        {
            return assercao;
        }

        /// <summary>
        /// assinatura que o método que executa tem que ter. O objeto [assercao]
        /// deve ser utilizado para fazer testes de condição, que serão mostrados
        /// no [ContainerTeste]
        /// </summary>
        /// <param name="assercao"></param>
        public delegate void MetodoDoTeste(Assercoes assercao);

        /// <summary>
        /// método a ser executado para se realizar o teste, contém o teste em si.
        /// tudo que precisa é ter a assinatura com uma asserção como parâmetro,
        /// que o teste será executado por passagem da função por delegate.
        /// </summary>
        public MetodoDoTeste METHOD { get; set; }


        public double TIME_PASSED = 0.0;



        /// <summary>
        /// inicializa o teste, tendo o método com assinatura tendo como parãmetro
        /// um Assercoes, e um nome para o teste para diferenciar dos demais.
        /// </summary>
        /// <param name="metodoTeste">método a ser executado para realizar o teste.</param>
        /// <param name="nomeDoTeste">nome do teste, útil para diferenciar entre muitos testes.</param>
        public Teste(MetodoDoTeste metodoTeste, string nomeDoTeste)
        {
            this.NomeTeste = nomeDoTeste;
            this.METHOD = metodoTeste;
            this.assercao = new Assercoes(this);
        } // Teste()

        public bool TesteFalhou = false;

        /// <summary>
        /// executa o teste.
        /// </summary>
        public void ExecutaTeste()
        {
            double tempoComeco = this.GetTime();
            this.METHOD(this.assercao);
            double tempoFim = this.GetTime();
            this.TIME_PASSED = tempoFim - tempoComeco;
            this.mensagens += MostraTempoDecorrido(this);
            this.mensagens += MostraNomeMetodo(this);

        } // ExecutaTeste()

        /// <summary>
        /// inicializa um novo teste, e carrega um banco de dados (formato simples, TXT, com todos campos separados por vírgulas).
        /// </summary>
        /// <param name="nomeDoTeste">nome do teste, para identificação do teste perante uma lista de testes.</param>
        /// <param name="metodoTeste">método a ser executado para captar os dados a serem mensurados.Observe que este método está feito no programa principal.</param>
        /// <param name="fileNameBancoDeDadosTXT">nome do banco de dados no format TXT. Cada linha e cada coluna devem ser separados por vírgulas.</param>
        public void TesteComBancoDeDados(string nomeDoTeste, MetodoDoTeste metodoTeste, string fileNameBancoDeDadosTXT)
        {
            BancoDeDadosTeste = CarregaDados(fileNameBancoDeDadosTXT);
            this.NomeTeste = nomeDoTeste;
            this.METHOD = metodoTeste;
        }

        private static string MostraTempoDecorrido(Teste testeCurrente)
        {
            return (" Teste executado em: " + testeCurrente.TIME_PASSED.ToString() + " miliSegundos. ");
        }

        private static string MostraNomeMetodo(Teste testeCurrente)
        {
            return (" Nome do método do Teste: void " + testeCurrente.METHOD.Method.Name + "(Assercoes). ");
        }

        private double GetTime()
        {

            double tempo = DateTime.Now.Day * 24 * 60 * 60 * 1000.0 +
                            DateTime.Now.Hour * 60 * 60 * 1000.0 +
                            DateTime.Now.Minute * 60 * 1000.0 +
                            DateTime.Now.Second * 1000.0 +
                            DateTime.Now.Millisecond;
            return tempo;
        } //getTime()

        public static Dictionary<string, List<string>> CarregaDados(string fileName)
        {
            try
            {
                StreamReader file = new StreamReader(fileName);
                Dictionary<string, List<string>> bancoDB = new Dictionary<string, List<string>>();

                while (!file.EndOfStream)
                {
                    List<string> umaLinha = file.ReadLine().Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                    string nomeobjeto = umaLinha[0];
                    umaLinha.RemoveAt(0);
                    bancoDB.Add(umaLinha[0], umaLinha);
                } // while()
                return bancoDB;
            } // try
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro no carregamento do arquivo de registros: " + fileName + ". Mensagem de erro: " + e.Message);
                return null;
            }  // catch
        } // ArquivoAutomacaoTesteUnitario()


    } // class Teste

}
