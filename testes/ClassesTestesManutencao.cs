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
    class ClassesTestesProgramacaoOrientadaAspcecto : SuiteClasseTestes
    {


        public ClassesTestesProgramacaoOrientadaAspcecto() : base("testes para o sistema programacao com Aspectos.")
        {

    
        }

        public void Teste_05(AssercaoSuiteClasse assercao)
        {
            ParserAFile.InitSystem();
            string fileCode = @"codigoClasseTestes_6.txt";
            ParserAFile parser = new ParserAFile(fileCode);

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetCode());
            processador.CompileEmDoisEstagios();

            ProgramaEmVM aplicativo = new ProgramaEmVM(processador.GetInstrucoes());
            aplicativo.Run(processador.escopo);



            assercao.IsTrue(
                (processador.escopo.tabela.GetObjeto("objA", processador.escopo) != null) &&
                (processador.escopo.tabela.GetObjeto("c", processador.escopo).GetValor() != null) &&
                (int.Parse(processador.escopo.tabela.GetObjeto("c", processador.escopo).GetValor().ToString()) == 0));

        }


        public void Teste_04(AssercaoSuiteClasse assercao)
        {

            ParserAFile.InitSystem();
            string fileCode = @"codigoClasseTestes_7.txt";
            ParserAFile parser = new ParserAFile(fileCode);

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetCode());
            processador.CompileEmDoisEstagios();

            ProgramaEmVM aplicativo = new ProgramaEmVM(processador.GetInstrucoes());
            aplicativo.Run(processador.escopo);



            assercao.IsTrue(processador.escopo.tabela.GetObjeto("c", processador.escopo) != null &&
                (int.Parse(processador.escopo.tabela.GetObjeto("c", processador.escopo).GetValor().ToString()) == 0));
        }

        public void Teste_02(AssercaoSuiteClasse assercao)
        {
            ParserAFile.InitSystem();
            string fileCode = @"codigoClasseTestes_5.txt";
            ParserAFile parser = new ParserAFile(fileCode);

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetCode());
            processador.CompileEmDoisEstagios();

            ProgramaEmVM aplicativo = new ProgramaEmVM(processador.GetInstrucoes());
            aplicativo.Run(processador.escopo);


            assercao.IsTrue(processador.escopo.tabela.GetObjeto("objA", processador.escopo) != null);
        }

        public void Teste_01(AssercaoSuiteClasse assercao)
        {
            ParserAFile.InitSystem();
            string fileCode = @"codigoClasseTestes_3.txt";
            ParserAFile parser = new ParserAFile(fileCode);

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetCode());
            processador.CompileEmDoisEstagios();

            assercao.IsTrue(LinguagemOrquidea.Instance().Aspectos != null && LinguagemOrquidea.Instance().Aspectos.Count == 1);
        }

       

        public void Teste_03(AssercaoSuiteClasse assercao)
        {
            ParserAFile.InitSystem();
            string fileCode = @"codigoClasseTestes_7.txt";
            ParserAFile parser = new ParserAFile(fileCode);

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetCode());
            processador.CompileEmDoisEstagios();

            ProgramaEmVM aplicativo = new ProgramaEmVM(processador.GetInstrucoes());
            aplicativo.Run(processador.escopo);


            assercao.IsTrue(true); // teste feito sem execuções fatais.
            assercao.IsTrue(
                (processador.escopo.tabela.GetObjeto("objA", processador.escopo) != null) &&
                (processador.escopo.tabela.GetObjeto("c", processador.escopo).GetValor() != null) &&
                (int.Parse(processador.escopo.tabela.GetObjeto("c", processador.escopo).GetValor().ToString()) == 0));

        }

    }

    class ClasseTesteExpressoesNulas :SuiteClasseTestes
    {
        public ClasseTesteExpressoesNulas() : base("testes para expressoes nulas")
        {

        }

        public void Teste_02(AssercaoSuiteClasse assercao)
        {
            string codigoExpressao = "Objeto a=nill; a==nill";
            List<string> tokensCodigo = ParserUniversal.GetTokens(codigoExpressao);
            Escopo escopo = new Escopo(tokensCodigo);

            Objeto objetoNuloTeste = new Objeto("private", "Objeto", "a", null);
            escopo.tabela.GetObjetos().Add(objetoNuloTeste);

            List<Expressao> expressaoNula = Expressao.Instance.ExtraiExpressoes(escopo, tokensCodigo);

            EvalExpression eval = new EvalExpression();
            object result=eval.EvalPosOrdem(expressaoNula[1], escopo);

            assercao.IsTrue(result.ToString() == "True"); 
        }

        public void Teste_01(AssercaoSuiteClasse assercao)
        {
            string codigoExpressao = "int a=0;  a==nill;";
            List<string> tokensCodigo = ParserUniversal.GetTokens(codigoExpressao);
            Escopo escopo = new Escopo(tokensCodigo);

            List<Expressao> expressaoNula = Expressao.Instance.ExtraiExpressoes(escopo, tokensCodigo);

            assercao.IsTrue(expressaoNula!=null && expressaoNula.Count==2 && expressaoNula[1].Elementos[2].GetType()==typeof(ExpessaoNILL));
        }

    }


 
    class ClasseTestesLigacoesDeEscopos: SuiteClasseTestes
    {
        public ClasseTestesLigacoesDeEscopos():base("classe para verificacao entre escopo pai com escopos filhos")
        {

        }

        public void Teste1(AssercaoSuiteClasse assercao)
        {
            ParserAFile parser = new ParserAFile(@"classesHerdadas_1.txt");

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetTokens());
            processador.CompileEmDoisEstagios();

            assercao.IsTrue(true); // execução do teste feito sem erros fatais.
        }

        public void Teste2(AssercaoSuiteClasse assercao)
        {
            ParserAFile parser = new ParserAFile(@"classeVerificarEscoposAninhados.txt");
            ProcessadorDeID processador = new ProcessadorDeID(parser.GetTokens());
            processador.CompileEmDoisEstagios();
            assercao.IsTrue(true);
        }
    }

    class ClasseTestesExtracaoDeExpressoesSemValidacao:SuiteClasseTestes
    {
        public ClasseTestesExtracaoDeExpressoesSemValidacao() : base("testes automatizados para extração de expressões, sem identificação")
        {

        }

        private Expressao Processamento(string expressao)
        {
            List<string> tokensDaExpressao = ParserUniversal.GetTokens(expressao);
            Escopo escopo = new Escopo(tokensDaExpressao);


            Expressao umaExpressao = Expressao.Instance.ExtraiUmaExpressaoSemValidar(tokensDaExpressao, escopo);

            AssercaoSuiteClasse assercao = new AssercaoSuiteClasse();
            assercao.IsTrue(umaExpressao.Elementos.Count == tokensDaExpressao.Count);
            return umaExpressao;

        }

     
        public void Teste_02(AssercaoSuiteClasse assercao)
        {


            string expressaoResumivel4 = "x=x+y+z*(x+1)";
            ProcessamentoExpressaoResumivel(expressaoResumivel4, 2);

            string expressaoResumivel1 = "public funcaoA ( int x, int y)";
            ProcessamentoExpressaoResumivel(expressaoResumivel1, 6);


            string expressaoResumivel2 = "Vetor v= create(int,1,1);";
            ProcessamentoExpressaoResumivel(expressaoResumivel2, 5);

            string expressaoResumivel3 = "objetoA.a=2";
            ProcessamentoExpressaoResumivel(expressaoResumivel3, 3);


        }

        private void ProcessamentoExpressaoResumivel(string expressaoAResumir, int quantidadeDeIDs)
        {
            List<string> tokensDaExpressao = ParserUniversal.GetTokens(expressaoAResumir);
            Escopo escopo = new Escopo(tokensDaExpressao);


            ProcessadorDeID processador = new ProcessadorDeID(tokensDaExpressao);
            List<string> tokensResumidos = processador.ResumeExpressoes(tokensDaExpressao, escopo);


            AssercaoSuiteClasse assercao = new AssercaoSuiteClasse();
            assercao.IsTrue(ContadorIDs(tokensResumidos) == quantidadeDeIDs); 
        }

        private int ContadorIDs(List<string> tokensResumidos)
        {
            if ((tokensResumidos == null) || (tokensResumidos.Count == 0))
                return 0;
            else
            {
                int countID = 0;
                for (int x = 0; x < tokensResumidos.Count; x++)
                    if (tokensResumidos[x] == "ID")
                        countID++;

                return countID;
            }
        }

        public void Teste_01(AssercaoSuiteClasse assercao)
        {
            string expressao6 = "(x+1)+a";
            Processamento(expressao6);


            string expressao1 = "x+1;";
            Processamento(expressao1);

            string expressao2 = "x=x+1;";
            Processamento(expressao2);

            string expressao3 = "x=x+1";
            Processamento(expressao3);

            string expressao4 = "x";
            Processamento(expressao4);

            string expressao5 = "1";
            Processamento(expressao5);
        }


    }


    class ClassteArremate :SuiteClasseTestes
    {
        private ProcessadorDeID processador;

        // "E:\ProjetoLinguagensOrquideaVisualStudio2019_2\bin\Debug\classesHerdadas_2.txt"
        public ClassteArremate() : base("testes para atribuicao de vetor, verificacao do padrao strategy na programacao orientada a objetos.")
        {

        }


        private void FazProcessamentoDeCodigoNaClasse(List<string> tokens)
        {



            this.processador = new ProcessadorDeID(tokens);
            this.processador.CompileEmDoisEstagios();

            ProgramaEmVM program = new ProgramaEmVM(this.processador.GetInstrucoes());
            program.Run(this.processador.escopo);



        }

        public void TesteProgramacaoOrientadaObjeto_PadraoEstrategy(AssercaoSuiteClasse assercao)
        {
            string fileCode = "classesHerdadas_2.txt";
            ParserAFile parser = new ParserAFile(fileCode);
            FazProcessamentoDeCodigoNaClasse(parser.GetTokens());

            assercao.IsTrue(true); // execução do teste feito sem erros fatais.

        }

        public void TesteProgramacaoOrientadaAObjeto_ClassesHerdadas(AssercaoSuiteClasse assercao)
        {
            string fileCode = "classesHerdadas_1.txt";
            ParserAFile parser = new ParserAFile(fileCode);
            FazProcessamentoDeCodigoNaClasse(parser.GetTokens());

            assercao.IsTrue((this.processador.escopo.tabela.GetClasses() != null) && (this.processador.escopo.tabela.GetClasses().Count == 3));

        }




        public void TesteAtribuicaoEmVetores(AssercaoSuiteClasse assercao)
        {
            string codigo = "Vetor v= create(int, 1,1);  v[0,0]=1";
            List<string> tokensDoCodigo = ParserUniversal.GetTokens(codigo);

            FazProcessamentoDeCodigoNaClasse(tokensDoCodigo);

            assercao.IsTrue(int.Parse(this.processador.escopo.tabela.GetVetor("v", this.processador.escopo).tailVetor[0].GetValor().ToString()) == 1);
        }

    }


    class ClasseTestesConstructor: SuiteClasseTestes
    {
        public ClasseTestesConstructor() : base("teste para o construtor de classes herdadas.")
        {

        }

        public void Teste_01(AssercaoSuiteClasse assercao)
        {
            ParserAFile parser = new ParserAFile("codigoTesteConstructor_UP.txt");

            ProcessadorDeID processador = new ProcessadorDeID(parser.GetTokens());
            processador.CompileEmDoisEstagios();



            ProgramaEmVM programa = new ProgramaEmVM(processador.GetInstrucoes());
            programa.Run(processador.escopo);

            assercao.IsTrue(int.Parse(processador.escopo.tabela.GetObjeto("objetoA", processador.escopo).GetField("a").GetValor().ToString()) == 1);
        }
    }

    class ClasseTestesPosicaoECodigo : SuiteClasseTestes
    {
        public ClasseTestesPosicaoECodigo() : base("testes para posicionar tokens entre o codigo")
        {

        }
 


        public void Teste_01(AssercaoSuiteClasse assercao)
        {
            string arquivoPrograma = @"codigoClasseTestes_2.txt";
            ParserAFile parser = new ParserAFile(arquivoPrograma);
            // classeB : + interfaceD
            List<string> tokensALocalizar = new List<string>() { "classeB", ":", "+", "interfaceD" };
            PosicaoECodigo posicao = new PosicaoECodigo(tokensALocalizar);

            assercao.IsTrue(posicao.linha == 8 && (posicao.coluna == 14));
       
        }


        public void Teste_ParserTokens(AssercaoSuiteClasse assercao)
        {
            string codigoPolemico = "interfaceB";
            List<string> tokens = ParserUniversal.GetTokens(codigoPolemico);

            assercao.IsTrue(tokens.Count == 1);
        }
    }
    class ClassesTestesManutencao
    {
        private class TestesCompilacaoExecucaoClasses : SuiteClasseTestes
        {
            private ProcessadorDeID processador;
            public TestesCompilacaoExecucaoClasses() : base("Testes para compilacao e execução de chamadas de metodos aninhados e propriedades aninhadas.")
            {

            }

           


            private void FazProcessamentoDeCodigoEmArquivo(string fileNameCode)
            {


                ParserAFile parser = null;
                if (fileNameCode != null)
                    parser = new ParserAFile(fileNameCode);
               
                this.processador = new ProcessadorDeID(parser.GetTokens());
                this.processador.CompileEmDoisEstagios();


                ProgramaEmVM program = new ProgramaEmVM(this.processador.GetInstrucoes());
                program.Run(this.processador.escopo);

              

            }

            private void FazProcessamentoDeCodigoNaClasse(List<string> tokens)
            {



                this.processador = new ProcessadorDeID(tokens);
                this.processador.CompileEmDoisEstagios();

                ProgramaEmVM program = new ProgramaEmVM(this.processador.GetInstrucoes());
                program.Run(this.processador.escopo);



            }



            public void Teste_06(AssercaoSuiteClasse assercao)
            {

                /*
                 * 
                 * public class classeC { public int objD ; public classeC(){ objD= 1; };  public int funcaoE( int x ){ objD=objD+x; };}
                 * 
                 * classeC obj_C = create(); 
                   obj_C.funcaoE(1);
                   obj_C.objD=obj_C.objD+1;
                 * 
                 * 
                 */


                // testes_aceite1
                this.FazProcessamentoDeCodigoEmArquivo("teste_complexo5.txt");



                assercao.IsTrue((processador.escopo.tabela.GetObjeto("obj_C", processador.escopo).GetField("objD").GetValor() != null) && int.Parse(processador.escopo.tabela.GetObjeto("obj_C", processador.escopo).GetField("objD").GetValor().ToString()) == 3);

            }


            public void Teste_03(AssercaoSuiteClasse assercao)
            {

                /*
                 * 
                 * public class classeA { public int a ; public classeA(){ a=1; };  public int funcaoB ( int x ){ a=a+x; }}
                   public class classeB { public classeA objA ; public classeB(){ objA= create(); };  public int funcaoC ( int x ){ objA.a=objA.a+x; }}
                   public class classeC { public classeB objB ; public classeC(){ objB= create(); };  public classeB funcaoD (){ return objB; }}

                   classeC obj_C = create(); 
                   obj_C.funcaoD().funcaoC(1);
                 * 
                 */
                this.FazProcessamentoDeCodigoEmArquivo("teste_complexo3.txt");


                assercao.IsTrue((this.processador.escopo.tabela.GetObjeto("obj_C", processador.escopo).GetField("objB").GetField("objA").GetField("a").GetValor() != null) && int.Parse(this.processador.escopo.tabela.GetObjeto("obj_C", processador.escopo).GetField("objB").GetField("objA").GetField("a").GetValor().ToString()) == 2);

            }




            public void Teste_05(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int a ;  public classeA(){ a=1; }; public int funcaoB ( int x ){ while (x>0) {x= x-1;}}; }; classeA objetoA= create();  objetoA.a=2;";
                List<string> tokens = ParserUniversal.GetTokens(codigo);

                this.FazProcessamentoDeCodigoNaClasse(tokens);

                assercao.IsTrue((this.processador.escopo.tabela.GetObjeto("objetoA", processador.escopo) != null) &&
                    (processador.escopo.tabela.GetObjeto("objetoA", processador.escopo).GetField("a").GetValor() != null) &&
                    (int.Parse(processador.escopo.tabela.GetObjeto("objetoA", processador.escopo).GetField("a").GetValor().ToString()) == 2));
            }


            public void Teste_01(AssercaoSuiteClasse assercao)
            {
               
                this.FazProcessamentoDeCodigoEmArquivo("teste_complexo1.txt");
                // objetoB.objA.a=1
                assercao.IsTrue((this.processador.escopo.tabela.GetObjeto("objetoB", processador.escopo).GetField("objA").GetField("a").GetValor() != null) && int.Parse(this.processador.escopo.tabela.GetObjeto("objetoB", processador.escopo).GetField("objA").GetField("a").GetValor().ToString()) == 2);

            }

       

            public void Teste_04(AssercaoSuiteClasse assercao)
            {
                // classeC obj_C = create(); obj_C.funcaoD(1)
                // testes_aceite1
                this.FazProcessamentoDeCodigoEmArquivo("teste_complexo4.txt");


                assercao.IsTrue((processador.escopo.tabela.GetObjeto("obj_C", processador.escopo).GetField("objB").GetValor() != null) && (int.Parse(processador.escopo.tabela.GetObjeto("obj_C", processador.escopo).GetField("objB").GetValor().ToString()) == 2));

            }

            public void Teste_02(AssercaoSuiteClasse assercao)
            {
                string codigo = "public class classeA { public int a ; public classeA(){ a=1; };  public int funcaoB ( int x ){ a=a+x; }}; classeA objetoA = create(); objetoA.funcaoB(1);";
                List<string> tokens = ParserUniversal.GetTokens(codigo);

                this.FazProcessamentoDeCodigoNaClasse(tokens);

                assercao.IsTrue((processador.escopo.tabela.GetObjeto("objetoA", processador.escopo).GetField("a").GetValor() != null) && int.Parse(processador.escopo.tabela.GetObjeto("objetoA", processador.escopo).GetField("a").GetValor().ToString()) == 2);
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
                this.processador.CompileEmDoisEstagios();
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
                this.processador.CompileEmDoisEstagios();

                assercao.IsTrue(this.processador.GetInstrucoes().Count == 2);
            }

            public void TesteRecuperacaoChamadaDeFuncao(AssercaoSuiteClasse assercao)
            {
                string codigo = "int b=1; int c=funcaoA(b); int funcaoA(int x){return x;};";
                this.processador = new ProcessadorDeID(new List<string>() { codigo });
                this.processador.CompileEmDoisEstagios();

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
                List<string> tokensDaExpressao = ParserUniversal.GetTokens(chamadaExpressao);

                this.processador = new ProcessadorDeID(codigo_definicoes);
                this.processador.CompileEmDoisEstagios();

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
                string expressao_chamada = "c= funcaoA(a,b);";
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
                List<string> tokensDaExpressao = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { expressao }).GetTokens();

                this.processador = new ProcessadorDeID(codigo_definicoes);
                this.processador.CompileEmDoisEstagios();

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
                List<string> tokensExpressao = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { codigoExpressao }).GetTokens();

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
                this.processador.CompileEmDoisEstagios();
                ProgramaEmVM programa = new ProgramaEmVM(this.processador.GetInstrucoes());
                programa.Run(this.processador.escopo);
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



            public void TesteVariavelVetor(AssercaoSuiteClasse assercao)
            {
                string codigo = "Vetor v= create(int, 1,1);";
                ProcessaTestes(codigo);
                assercao.IsTrue(processador.escopo.tabela.GetVetor("v", processador.escopo) != null);
            }

            public void TesteChamadaDeObjetoImportado(AssercaoSuiteClasse assercao)
            {
                CultureInfo.CurrentCulture = CultureInfo.CurrentCulture; // para compatibilizar os numeros float como: 1.0.

                /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
                /// importer ( nomeAssembly).

                string codigoCreate = "importer (ParserLinguagemOrquidea.exe);  Matriz M= create(1,1);";
                string codigoChamadaMetodo = "M.SetElement(0,0, 1.0, 5.0)";

                string codigoTotal = codigoCreate + " " + codigoChamadaMetodo;
                ProcessaTestes(codigoTotal);



                assercao.IsTrue(processador.escopo.tabela.GetObjeto("M", processador.escopo) != null);
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
                assercao.IsTrue(RepositorioDeClassesOO.Instance().GetClasses().Count > 15);

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



         


            public void TesteCreateObjects(AssercaoSuiteClasse assercao)
            {
                /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
                /// importer ( nomeAssembly).
                string codigoCreate = "importer (ParserLinguagemOrquidea.exe); Matriz M= create(1,1);";

                string codigoTotal = codigoCreate;
                ProcessaTestes(codigoTotal);

                assercao.IsTrue(processador.escopo.tabela.GetObjeto("M", processador.escopo) != null);
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
                this.processador.CompileEmDoisEstagios();

            }
            
            public void TestesCompilacaoDeClasse_5(AssercaoSuiteClasse assercao)
            {
                string fileNameCode = @"codigoClasseTestes_2.txt";
                this.FazACompilacao(fileNameCode);

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 3 && (RepositorioDeClassesOO.Instance().GetInterfaces().Count== 1));

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

                assercao.IsTrue(processador.escopo.tabela.GetClasses().Count == 1 && (RepositorioDeClassesOO.Instance().GetInterfaces().Count == 1));
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
                this.processador.CompileEmDoisEstagios();

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
                processador.CompileEmDoisEstagios();
                ProgramaEmVM programa = new ProgramaEmVM(processador.GetInstrucoes());
                programa.Run(processador.escopo);
            }

        }
        public ClassesTestesManutencao()
        {
            ClassesTestesProgramacaoOrientadaAspcecto testesAspecto = new ClassesTestesProgramacaoOrientadaAspcecto();
            testesAspecto.ExecutaTestes();

            //ClasseTesteExpressoesNulas testesExpressoesNulas = new ClasseTesteExpressoesNulas();
            //testesExpressoesNulas.ExecutaTestes();

            //ClasseTestesLigacoesDeEscopos testesLigacoesEscopo = new ClasseTestesLigacoesDeEscopos();
            //testesLigacoesEscopo.ExecutaTestes();

            //ClasseTestesExtracaoDeExpressoesSemValidacao testesExtracaoExpressoes = new ClasseTestesExtracaoDeExpressoesSemValidacao();
            //testesExtracaoExpressoes.ExecutaTestes();

            //ClassteArremate classeArremate = new ClassteArremate();
            //classeArremate.ExecutaTestes();


            //ClasseTestesConstructor testesConstructors = new ClasseTestesConstructor();
            //testesConstructors.ExecutaTestes();


            //TestesCompilacaoExecucaoClasses testesBuildAndExecutationExpressoesOrientadoAObjeto = new TestesCompilacaoExecucaoClasses();
            //testesBuildAndExecutationExpressoesOrientadoAObjeto.ExecutaTestes();

            //ClasseTestesPosicaoECodigo testesPosicionamentoTokens = new ClasseTestesPosicaoECodigo();
            //testesPosicionamentoTokens.ExecutaTestes();

            //TestesCompilacaoDeExpressoesAninhadas testeCompilacaoAninhados = new TestesCompilacaoDeExpressoesAninhadas();
            //testeCompilacaoAninhados.ExecutaTestes();

            //TesteExecucaoProgramaVMComUmaClasse testeParaAvalacaoDeFuncoesEmUmaClasse = new TesteExecucaoProgramaVMComUmaClasse();
            //testeParaAvalacaoDeFuncoesEmUmaClasse.ExecutaTestes();


            //TestesExpressoes testesDeExpressoes = new TestesExpressoes();
            //testesDeExpressoes.ExecutaTestes();


            //TestesAvaliacaoDeInstrucoes  testesInstrucoes = new TestesAvaliacaoDeInstrucoes();
            //testesInstrucoes.ExecutaTestes();

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
