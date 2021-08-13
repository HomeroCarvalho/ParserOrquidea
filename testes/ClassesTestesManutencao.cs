using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser;
using ModuloTESTES;
using parser.ProgramacaoOrentadaAObjetos;

namespace ParserLinguagemOrquidea.testes
{
    class ClassesTestesManutencao
    {
        private class TestesExpressoes : SuiteClasseTestes
        {
            private ProcessadorDeID processador;
            private Escopo escopo;
            private object resultEval;

            public TestesExpressoes():base("Testes para avaliacao de expressoes")
            {

            }
            private void ProcessaAExpressao(string definicoes, string chamadaExpressao)
            {
                List<string> codigo_definicoes = new List<string>() { definicoes };
                List<string> codigo_chamada = new List<string>() { chamadaExpressao };
                List<string> tokens_chamada = new Tokens(new LinguagemOrquidea(), codigo_chamada).GetTokens();

                this.processador = new ProcessadorDeID(codigo_definicoes);
                this.processador.Compile();

                Expressao exprssaoAProcessar = new Expressao(tokens_chamada.ToArray(), processador.escopo);
                EvalExpression eval = new EvalExpression();
                this.resultEval = eval.EvalPosOrdem(exprssaoAProcessar, processador.escopo);

                this.escopo = this.processador.escopo;
            }



            public void Teste_Tres(AssercaoSuiteClasse assercao)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                string expressao = "int x=1; int t=2; int dx=100; int c; int funcaoA(int x, int y, int z){return (x+y)*z;};";
                string expressao_chamada = "int c= funcaoA(x,t,dx);";
                this.ProcessaAExpressao(expressao, expressao_chamada);

                assercao.IsTrue(
                    (this.escopo.tabela.GetObjeto("c", this.escopo) != null) &&
                    (int.Parse(this.escopo.tabela.GetObjeto("c", this.escopo).GetValor().ToString()) == 300));

            }


            public void Teste_Um(AssercaoSuiteClasse assercao)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                string expressao = "int a=1; int b=2; int c; int funcaoA(int x, int y){return x+y;};";
                string expressao_chamada = "int c= funcaoA(a,b);";
                this.ProcessaAExpressao(expressao, expressao_chamada);

                assercao.IsTrue(int.Parse(this.escopo.tabela.GetObjeto("c", this.escopo).GetValor().ToString()) == 3);
            }


            public void Teste_Dois(AssercaoSuiteClasse assercao)
            {
                /// cultura en-US: float numero=1.1
                /// cultura invariante: float numero=1.1
                /// cultura pt-br: float numero=1,1
                /// 
                /// declarando um numero float: float numero=1.1f

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                string expressao = "float a=1.1; float b=2.0; float k=0.0; float c; float funcaoA(float x, float y){return x*y;};";
                string expressao_chamada = "float c= funcaoA(a,b);";
                this.ProcessaAExpressao(expressao, expressao_chamada);

                assercao.IsTrue(
                    (this.escopo.tabela.GetObjeto("c", this.escopo) != null) &&
                    (float.Parse(this.escopo.tabela.GetObjeto("c", this.escopo).GetValor().ToString()) == 2.2f));
            }

            public void Teste_Quatro(AssercaoSuiteClasse assercao)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                string expressaoNumeroPontoFlutuante = "1.0";
                List<string> tokensNumero = ParserUniversal.GetTokens(expressaoNumeroPontoFlutuante);
                List<string> tokensCorretos = ParserUniversal.ObtemPontosFlutuantes(tokensNumero);

                float numeroResultante = float.NaN;
                assercao.IsTrue(float.TryParse(tokensCorretos[0], out numeroResultante));
            }
        }



        public class TestesAvaliacaoDeInstrucoes : SuiteClasseTestes
        {
            private ProcessadorDeID processador;

            public TestesAvaliacaoDeInstrucoes() : base("Testes para avaliacao de instrucoes.")
            {

            }
            private void ProcessaTestes(string codigo)
            {
                this.processador = new ProcessadorDeID(new List<string>() { codigo });
                this.processador.Compile();
                ProgramaEmVM programa = new ProgramaEmVM(this.processador.GetInstrucoes());
                programa.Run(this.processador.escopo);
            }


            public void TesteImporter(AssercaoSuiteClasse assercao)
            {
                string codigo = "importer (ParserLinguagemOrquidea.exe);";
                ProcessaTestes(codigo);
                assercao.IsTrue(RepositorioDeClassesOO.Instance().classesRegistradas.Count > 15);

            }
            public void TesteIF(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=5; if (a<b){a=3*b;}";
                this.ProcessaTestes(codigo);

                assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetObjeto("a", processador.escopo).GetValor().ToString()) == 15);
            }

            public void TesteWhile(AssercaoSuiteClasse assercao)
            {
                string codigo = "int x=1; int dx=5; while (x<=4){dx=dx+1; x=x+1;}";
                this.ProcessaTestes(codigo);

                assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetObjeto("x", processador.escopo).GetValor().ToString()) == 5);
            }
            public void TesteCasesOfUse_2(AssercaoSuiteClasse assercao)
            {
                // casesOfUse ID ( case  ID_operador  ID : "

                string codigo = "int b=1; int a=5; casesOfUse b ( case > b: a=1; );";
                this.ProcessaTestes(codigo);
                assercao.IsTrue(int.Parse(processador.escopo.tabela.GetObjeto("a", processador.escopo).GetValor().ToString()) == 5);
            }



            public void TesteChamadaDeObjetoImportado(AssercaoSuiteClasse assercao)
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentCulture; // para compatibilizar os numeros float como: 1.0.

                /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
                /// importer ( nomeAssembly).

                string codigoCreate = "Matriz M= create(1,1);";
                string codigoChamadaMetodo = "M.SetElement(0,0, 1.0)";

                string codigoTotal = codigoCreate + " " + codigoChamadaMetodo;
                ProcessaTestes(codigoTotal);



                assercao.IsTrue(processador.escopo.tabela.GetObjeto("M", processador.escopo) != null);
            }

            public void TesteVariavelVetor(AssercaoSuiteClasse assercao)
            {
                string codigo = "Vetor v= create(1,1);";
                ProcessaTestes(codigo);
                assercao.IsTrue(processador.escopo.tabela.GetVetor("v", processador.escopo) != null);
            }


            public void TesteCreateObjects(AssercaoSuiteClasse assercao)
            {
                /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
                /// importer ( nomeAssembly).
                string codigoCreate = "Matriz M= create(1,1);";

                string codigoTotal = codigoCreate;
                ProcessaTestes(codigoTotal);

                assercao.IsTrue(processador.escopo.tabela.GetObjeto("M", processador.escopo) != null);
            }


            public void TesteCasesOfUse(AssercaoSuiteClasse assercao)
            {
                // casesOfUse ID ( case  ID_operador  ID : "

                string codigo = "int b=1; int a=5; casesOfUse a ( case > b: a=6; );";
                this.ProcessaTestes(codigo);
                assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetObjeto("a", processador.escopo).GetValor().ToString()) == 6);
                // agora podemos rodar estes cenarios, sempre que modificamos o código, pois pode haver quebra do código com alguma modificação.
            }

            public void Atribuicao(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=2; int b=1; int c= 3*b;";
                this.ProcessaTestes(codigo);
                assercao.IsTrue(
                    (processador.escopo.tabela.GetObjeto("c", processador.escopo) != null) &&
                    (int.Parse(processador.escopo.tabela.GetObjeto("c", processador.escopo).GetValor().ToString()) == 3));
            }

            public void TesteIF_Else(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=1; int b=5; if (a>b){a=3*b;} else {a=6;}";
                this.ProcessaTestes(codigo);

                assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetObjeto("a", processador.escopo).GetValor().ToString()) == 6);
            }

            public void TesteFor(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=0; int b=5; for (int x=0;x< 3;x++) {a= a+ x;};";
                this.ProcessaTestes(codigo);

                assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetObjeto("a", processador.escopo).GetValor().ToString()) == 1);

            }
        }


        public ClassesTestesManutencao()
        {
             //TestesExpressoes testesDeExpressoes = new TestesExpressoes();
             //testesDeExpressoes.ExecutaTestes();


             TestesAvaliacaoDeInstrucoes  testesInstrucoes = new TestesAvaliacaoDeInstrucoes();
             testesInstrucoes.ExecutaTestes();
        }
    }
}
