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
        private class TestesCompilacaoExecucaoClasses : SuiteClasseTestes
        {
            private ProcessadorDeID processador;
            public TestesCompilacaoExecucaoClasses() : base("Testes para compilacao e execução de chamadas de metodos aninhados e propriedades aninhadas.")
            {

            }

           


            private void FazProcessamento(string codigoDefinicao)
            {
                List<string> codigo = ParserUniversal.GetTokens(codigoDefinicao);
                this.processador = new ProcessadorDeID(codigo);
                this.processador.Compile();

                ProgramaEmVM program = new ProgramaEmVM(this.processador.GetInstrucoes());
                program.Run(this.processador.escopo);
                
            }


           
            public void Teste_02(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int a ; public classeA(){ a=1; };  public int funcaoB ( int x ){ a=a+x; };}; classeA objetoA = create(); objetoA.funcaoB(1);";

                this.FazProcessamento(codigo);

                assercao.IsTrue(int.Parse(processador.escopo.tabela.GetObjeto("objetoA",processador.escopo).GetField("a").GetValor().ToString()) == 2);
            }

            public void Teste_01(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int a ;  public classeA(){ a=1; }; public int funcaoB ( int x ){ while (x>0) {x= x-1;}} }; classeA objetoA= Create();  objetoA.a=2;";

                this.FazProcessamento(codigo);

                assercao.IsTrue((this.processador.escopo.tabela.GetObjeto("objetoA", processador.escopo) != null) &&
                    (int.Parse(processador.escopo.tabela.GetObjeto("objetoA", processador.escopo).GetField("a").GetValor().ToString()) == 2));
            }

        }


        private class TestesCompilacaoDeExpressoesAninhadas : SuiteClasseTestes
        {
            private ProcessadorDeID processador;
            public TestesCompilacaoDeExpressoesAninhadas() : base("Testes para compilacao de chamadas de metodos aninhados e propriedades aninhadas.")
            {

            }

            private void FazProcessamento(string codigoDefinicao)
            {
                List<string> codigo = ParserUniversal.GetTokens(codigoDefinicao);
                this.processador = new ProcessadorDeID(codigo);
                this.processador.Compile();
            }

            public void Teste_02(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public classeA(){ a=1; };  public int a ;  public int funcaoB ( int x ){ while (x>0) {x= x-1;}} } ";
                codigo += " classeA objetoA= Create(); objetoA.funcaoB(1)";

                this.FazProcessamento(codigo);

                assercao.IsTrue(this.processador.escopo.tabela.GetObjeto("objetoA", processador.escopo) != null);
            }

            public void Teste_01(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int a ;  public classeA(){ a=1; }; public int funcaoB ( int x ){ while (x>0) {x= x-1;}} } ";
                codigo += " classeA objetoA= Create();  objetoA.a=2";

                this.FazProcessamento(codigo);

                assercao.IsTrue(this.processador.escopo.tabela.GetObjeto("objetoA", processador.escopo) != null);
            }


        }



        private class TestesExpressoesMatriciais:SuiteClasseTestes
        {
            public TestesExpressoesMatriciais():base("teste para operadores de matriz")
            {

            }

            public void TesteExpressaoComOperadorMatricial(AssercaoSuiteClasse assercao)
            {
                string codigo = "vetorTeste[1,1]=5";

                Escopo escopo = new Escopo(new List<string>() { codigo });
                escopo.tabela.AddObjetoVetor("private", "vetorTeste", "int", new int[] { 2, 2 }, escopo, false);
                
                
                List<string> tokensExpressao = ParserUniversal.GetTokens(codigo);
                Expressao exprssVetor = new Expressao(tokensExpressao.ToArray(), escopo);

                assercao.IsTrue(exprssVetor != null);
               
            }


            public void TesteOperadorMatriz_1(AssercaoSuiteClasse assercao)
            {
                string codigo = "m[1,1]=5;";
                List<string> tokensCodigo = GetExpressaoMatricial(codigo);
                assercao.IsTrue(tokensCodigo.Contains("["));

            }


            public void TesteOperadorMatriz_2(AssercaoSuiteClasse assercao)
            {
                string codigo = "m[x+1,y*b]=5;";
                List<string> tokensCodigo = GetExpressaoMatricial(codigo);
                assercao.IsTrue(tokensCodigo.Contains("[") && (tokensCodigo.Contains("]")));
            }


            private static List<string> GetExpressaoMatricial(string codigo)
            {
                List<string> codigoTokens = new List<string>() { codigo };
                codigoTokens = ParserUniversal.GetTokens(codigo);

                return codigoTokens;

            }


        }



        



        private class TestesRecoverInstructions : SuiteClasseTestes
        {
            private ProcessadorDeID processador;
        
            public TestesRecoverInstructions():base("Testes para recuperar instrucoes invalidas")
            {
                
            }

            public void TesteRecuperacaoAtribuicao(AssercaoSuiteClasse assercao)
            {
                string codigo = "int funcaoA(){c=1;}; int c=3;"; // a definição está no escopo-pai, mas o escopo-pai está depois da atribuição.
                this.processador = new ProcessadorDeID(new List<string>() { codigo });
                this.processador.Compile();

                assercao.IsTrue(this.processador.GetInstrucoes().Count == 2);
            }

            public void TesteRecuperacaoChamadaDeFuncao(AssercaoSuiteClasse assercao)
            {
                string codigo = "int b=1; int c=funcaoA(b); int funcaoA(int x){return x;};";
                this.processador = new ProcessadorDeID(new List<string>() { codigo });
                this.processador.Compile();

                assercao.IsTrue(this.processador.GetInstrucoes().Count==3); // essa assercao foi definida apos o teste com exito, com o fim de em outros testes, gere um cenario de testes válido, em casos como verificação se o código não quebrou, ao mexer em outras partes do código;
            }
        }
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
                List<string> tokensDaExpressao = new Tokens(new LinguagemOrquidea(), new List<string>() { chamadaExpressao }).GetTokens();

                this.processador = new ProcessadorDeID(codigo_definicoes);
                this.processador.Compile();

                ProgramaEmVM programa = new ProgramaEmVM(this.processador.GetInstrucoes());
                programa.Run(this.processador.escopo);

                Expressao exprssaoAProcessar = new Expressao(tokensDaExpressao.ToArray(), processador.escopo);
                EvalExpression eval = new EvalExpression();
                this.resultEval = eval.EvalPosOrdem(exprssaoAProcessar, processador.escopo);

                this.escopo = this.processador.escopo;
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





            public void Teste_Nove(AssercaoSuiteClasse assercao)
            {
                string expressao = "int a=1; int b=2; int c; int funcaoA(int x, int y){a=a+1;}; int funcaoB(int y){return y+1;}; funcaoA(1, 2) + funcaoB(a);";
                string expressao_chamada = "int d=1;";
                this.ProcessaAExpressao(expressao, expressao_chamada);
                assercao.IsTrue((this.processador.escopo.tabela.GetObjeto("a", escopo) != null) && (int.Parse(this.escopo.tabela.GetObjeto("a", escopo).GetValor().ToString()) == 2));
            }

            public void Teste_Oito(AssercaoSuiteClasse assercao)
            {
                string expressao = "int a=1; int b=2; int c; int funcaoA(int x, int y){a=a+1;}; int funcaoB(int y){return y+1;}; ";
                string expressao_chamada = "funcaoA(1,2)+ funcaoB(a)";
                this.ProcessaAExpressao(expressao, expressao_chamada);
                assercao.IsTrue((this.processador.escopo.tabela.GetObjeto("a", escopo) != null) && (int.Parse(this.escopo.tabela.GetObjeto("a", escopo).GetValor().ToString()) == 2));
            }


            public void Teste_Seis(AssercaoSuiteClasse assercao)
            {
                string codigoDefinicaoDeClasse = "public class classeA {  public int a; public classeA() { a = 1; }; public int funcaoB(int x) { while (x > 0) { x = x - 1; } } }";
                codigoDefinicaoDeClasse += " classeA c= create()";
                string expressao = "c.a=1;";



                List<string> codigo_definicoes = new List<string>() { codigoDefinicaoDeClasse };
                List<string> tokensDaExpressao = new Tokens(new LinguagemOrquidea(), new List<string>() { expressao }).GetTokens();

                this.processador = new ProcessadorDeID(codigo_definicoes);
                this.processador.Compile();

                Expressao exprssaoAProcessar = new Expressao(tokensDaExpressao.ToArray(), processador.escopo);
                EvalExpression eval = new EvalExpression();
                this.resultEval = eval.EvalPosOrdem(exprssaoAProcessar.Elementos[0], processador.escopo);

                this.escopo = this.processador.escopo;

                assercao.IsTrue(this.escopo.tabela.GetObjeto("c", escopo) != null && (int.Parse(this.escopo.tabela.GetObjeto("c", escopo).GetField("a").GetValor().ToString()) == 1));
            }
            public void Teste_Sete(AssercaoSuiteClasse assercao)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                string expressao = "int a=1; int b=2; int c; int funcaoA(int x, int y){return x+y;};";
                string expressao_chamada = "c=1;";
                this.ProcessaAExpressao(expressao, expressao_chamada);

                assercao.IsTrue(int.Parse(this.escopo.tabela.GetObjeto("c", this.escopo).GetValor().ToString()) == 1); // valor anterior era do teste Teste_Um.
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


            public void Teste_Cinco(AssercaoSuiteClasse assercao)
            {
                // cria os objetos da expressao.
                Objeto obj1 = new Objeto("private", "int", "a", "1");
                Objeto obj2 = new Objeto("private", "int", "b", "5");
                Objeto objResult = new Objeto("private", "int", "c", "0");

                // cria os tokens da expressao a avaliar.
                string codigoExpressao = "int c= a+b";
                List<string> tokensExpressao = new Tokens(new LinguagemOrquidea(), new List<string>() { codigoExpressao }).GetTokens();

                // cria o escopo, e adiciona os objetos da expressao.
                Escopo escopo = new Escopo(tokensExpressao);
                escopo.tabela.GetObjetos().Add(obj1);
                escopo.tabela.GetObjetos().Add(obj2);
                escopo.tabela.GetObjetos().Add(objResult);

                // cria a expressao a avaliar.
                Expressao exprssAProcessar = new Expressao(tokensExpressao.ToArray(), escopo);

                // avalia a expressao.
                object resultExpressao = new EvalExpression().EvalPosOrdem(exprssAProcessar, escopo);

                assercao.IsTrue(int.Parse(escopo.tabela.GetObjeto("c", escopo).GetValor().ToString()) == 6);
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

            public void TesteFor(AssercaoSuiteClasse assercao)
            {
                string codigo = "int a=0; int b=5; for (int x=0;x< 3;x++) {a= a+ x;};";
                this.ProcessaTestes(codigo);

                assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetObjeto("a", processador.escopo).GetValor().ToString()) == 3);

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

         
        }


        public class TestesProcessamentoDeClasses : SuiteClasseTestes
        {
            ProcessadorDeID processador = null;

            public TestesProcessamentoDeClasses() : base("testeParaCompilacaoDeClasses.")
            {


            }

            public void FazACompilacao(string fileNameCode)
            {
                ParserAFile parser = new ParserAFile(fileNameCode);
                List<string> tokens = parser.GetTokens();
                this.processador = new ProcessadorDeID(tokens);
                this.processador.Compile();

            }
            
            public void TestesCompilacaoDeClasse_5(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes_2.txt";
                this.FazACompilacao(fileNameCode);

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 3 && (RepositorioDeClassesOO.Instance().interfacesRegistradas.Count == 1));

            }


            //"E:\ProjetoLinguagensOrquideaVisualStudio2019_2\bin\Debug\codigoClasseTestes_2.txt"
            public void TestesCompilacaoDeClasse_4(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes_3.txt";
                this.FazACompilacao(fileNameCode);

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 1);
                
            }


            public void TestesCompilacaoDeClasse_3(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes_3.txt";
                this.FazACompilacao(fileNameCode);

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 1 && (RepositorioDeClassesOO.Instance().interfacesRegistradas.Count == 1));
            }

            public void TesteCompilacaoDeClasse_2(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes_2.txt";
                this.FazACompilacao(fileNameCode);

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 3);

            }

            public void TestesCompilacaoDeClasse_1(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes.txt";
                this.FazACompilacao(fileNameCode);

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 1);

            }

 
        }

        private class TesteGuardarRecuperarClasses:SuiteClasseTestes
        {
            ProcessadorDeID processador;

            public TesteGuardarRecuperarClasses() : base("teste para operações de guardar/salvar classes na forma de texto")
            {

            }

            public void TestesCompilacaoDeClasse_1(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes_2.txt";
                this.FazACompilacao(fileNameCode);


                try
                {
                    Classe classeTestada = processador.escopo.tabela.GetClasses()[2];
                    assercao.IsTrue(classeTestada != null);

                    classeTestada.Save(classeTestada);
                    assercao.IsTrue(true);
                }
                catch
                {
                    assercao.IsTrue(false);
                }
            }

            public void FazACompilacao(string fileNameCode)
            {
                ParserAFile parser = new ParserAFile(fileNameCode);
                List<string> tokens = parser.GetTokens();
                this.processador = new ProcessadorDeID(tokens);
                this.processador.Compile();

            }


        }

        private class TesteExecucaoProgramaVMComUmaClasse : SuiteClasseTestes
        {
            public TesteExecucaoProgramaVMComUmaClasse() : base("execucao de codigo de uma classe, com instrucoes em um programa vm")
            {

            }



            public void testeExecucaoCodigoClasse1(AssercaoSuiteClasse assercao)
            {
                string arquivoPrograma = @"ProgramaVM_1_classes.txt";
                this.Processamento(arquivoPrograma);

                assercao.IsTrue(true); // inicialmente, verifica se nao houve erros fatais.


            }

            public void Processamento(string fileProgram)
            {
                ParserAFile parser = new ParserAFile(fileProgram);
                List<string> tokensCodigo = parser.GetTokens();

                ProcessadorDeID processador = new ProcessadorDeID(tokensCodigo);
                processador.Compile();
                ProgramaEmVM programa = new ProgramaEmVM(processador.GetInstrucoes());
                programa.Run(processador.escopo);
            }

        }
        public ClassesTestesManutencao()
        {

            TestesCompilacaoExecucaoClasses testesBuildAndExecutationExpressoesOrientadoAObjeto = new TestesCompilacaoExecucaoClasses();
            testesBuildAndExecutationExpressoesOrientadoAObjeto.ExecutaTestes();

            //TestesCompilacaoDeExpressoesAninhadas testeCompilacaoAninhados = new TestesCompilacaoDeExpressoesAninhadas();
            //testeCompilacaoAninhados.ExecutaTestes();

            //TesteExecucaoProgramaVMComUmaClasse testeParaAvalacaoDeFuncoesEmUmaClasse = new TesteExecucaoProgramaVMComUmaClasse();
            //testeParaAvalacaoDeFuncoesEmUmaClasse.ExecutaTestes();


            //TestesExpressoes testesDeExpressoes = new TestesExpressoes();
            //testesDeExpressoes.ExecutaTestes();


            // TestesAvaliacaoDeInstrucoes  testesInstrucoes = new TestesAvaliacaoDeInstrucoes();
            // testesInstrucoes.ExecutaTestes();

            // TestesRecoverInstructions testesRecuperarInstrucoes = new TestesRecoverInstructions();
            // testesRecuperarInstrucoes.ExecutaTestes();

            //TestesExpressoesMatriciais testeExpressoesDeMatrizes = new TestesExpressoesMatriciais();
            // testeExpressoesDeMatrizes.ExecutaTestes();

            //TestesProcessamentoDeClasses testeParaClasses= new TestesProcessamentoDeClasses();
            //testeParaClasses.ExecutaTestes();

            //TesteGuardarRecuperarClasses testesParaOperacoesArquivoClasse = new TesteGuardarRecuperarClasses();
            //testesParaOperacoesArquivoClasse.ExecutaTestes();

           
        }
    }
}
