using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ModuloTESTES;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    class CorpoTestes_2
    {

        private void TesteCalculoDeVariasSequenciasID(Assercoes assercao)
        {

            // teste para obter várias sequencias id, mas está codificado inicialmente para obter a primeira sequencia id.
            UmaGramaticaComputacional linguagem = LinguagemOrquidea.Instance();

            List<string> codigo1 = new List<string> { "public int funcaoSemParamsComCorpo(){int a=1;}",
                                                      "public int C=1+3;",
                                                      "public int funcaoComParamsSemCorpo(int A);",
                                                      "public int funcaoSemParamsSemCorpo();",
                                                      "public int funcaoComParamsComCorpo(int A,int B ){int a=1;}"};

            List<string> tokens = new Tokens(linguagem, codigo1).GetTokens();
            int iteracoes = 0;
            List<UmaSequenciaID> sequenciasEncontradas = new List<UmaSequenciaID>();
            while (((iteracoes++)<100) && (tokens.Count>0))
            {
                UmaSequenciaID sequencia = UmaSequenciaID.ObtemUmaSequenciaID(0,tokens, codigo1);

                if (sequencia != null)
                {
                    sequenciasEncontradas.Add(sequencia);
                    tokens.RemoveRange(0, sequencia.tokens.Count);
                    if (tokens.Count == 0)
                        break;
                } // if
            } // while
            assercao.MsgSucess("Sequencias ID calculadas sem erros fatais.");
            if (sequenciasEncontradas.Count == 5)
                assercao.MsgSucess("Sequencias ID calculadas com exatidão.");
        } // TesteConstruirEscopos()

        private void TesteGetExpressoes(Assercoes assercao)
        {
            List<string> codigoPolemico1 = new List<string>() { "int a= form +b(); int A(a,b);  int a; int b; " };

            List<string> tokensPolemico1 = ParserUniversal.GetTokens(Util.UtilString.UneLinhasLista(codigoPolemico1));

            Escopo escopo = new Escopo(codigoPolemico1);

            List<Expressao> expressoesEncontradas = new List<Expressao>();
            while (tokensPolemico1.Count > 0)
            {
                Expressao expressao = new Expressao(tokensPolemico1.ToArray(), escopo);
                if ((expressao == null) || (expressao.Elementos.Count == 0))
                    break;
                expressoesEncontradas.Add(expressao);
                tokensPolemico1.RemoveRange(0, expressao.Elementos.Count);
            } // while()

            assercao.MsgSucess("Teste de obter expressoes sem erros fatais.");
            if (expressoesEncontradas.Count == 4)
                assercao.MsgSucess("Teste para obter expressões feito com exatidão");


            expressoesEncontradas.Clear();

            List<string> codigoPolemico2 = new List<string>() { " int a= mfor +b(); int A(a,b);  int a; int b; " };
            List<string> tokensPolemico2 = ParserUniversal.GetTokens(Util.UtilString.UneLinhasLista(codigoPolemico2));

            while (tokensPolemico2.Count > 0)
            {
                Expressao expressao = new Expressao(tokensPolemico2.ToArray(), escopo);
                if (expressao == null) 
                    break;
                expressoesEncontradas.Add(expressao);
                tokensPolemico2.RemoveRange(0, expressao.Elementos.Count);
            } // while()

            assercao.MsgSucess("Teste de obter expressoes sem erros fatais.");
            if (expressoesEncontradas.Count == 4)
                assercao.MsgSucess("Teste para obter expressões feito com exatidão");


        } // TesteGetExpressoes()


        private void TesteObterProducoes(Assercoes assercao)
        {
            LinguagemOrquidea linguageem = LinguagemOrquidea.Instance();
            List<string> codigo = new List<string>
            {
                "for (a=1;a<10;a++)",
                "{",
                "int k=1;",
                "}"
            };

            List<string> tokens = ParserUniversal.GetTokens(Util.UtilString.UneLinhasLista(codigo));

            List<producao> producoes = new Tokens(linguageem, codigo).GetProducoes(tokens, new Escopo(codigo));

            assercao.MsgSucess("Produçõs de tokens e producoes sem erros fatais.");
            if ((producoes.Count == 5))
                assercao.MsgSucess("Calculo de produções enccontrado com exatidão.");

            List<string> codigoSequenciaID = new List<string>
            {
                "int funcaoB(int x);"
            };


            List<string> tokensSeq = new Tokens(linguageem, codigoSequenciaID).GetTokens();
            List<producao> producoesSeq = new Tokens(linguageem, codigoSequenciaID).GetProducoes(tokensSeq, new Escopo(codigoSequenciaID));

            assercao.MsgSucess("Producoes de tokens e producoes para uma sequencia id feita sem erros fatais.");
            if (producoesSeq.Count == 1)
                assercao.MsgSucess("Producao de sequencia de id feita com exatidao.");
        } // TesteObterProducoes()


     
        private void TesteExpressaoCondicionalResumida(Assercoes assercao)
        {
            string expressaoCondicional_b = "((a+b)<(c+d))";


            List<string> tokensExpressaoCondicional = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { expressaoCondicional_b }).GetTokens();
            Escopo escopo = new Escopo(new List<string>() { expressaoCondicional_b });


            Expressao exprss = new Expressao(tokensExpressaoCondicional.ToArray(), escopo);
            List<string> resumida= Expressao.Instance.ObtemExpressaoCondicionalResumida(exprss, escopo);

            assercao.MsgSucess("Teste para obtenção de expressão condicional resumida feito sem erros fatais");
            if (resumida.Count == 3)
                assercao.MsgSucess("Teste para obtenção de expressão condicional resumida feito com exatidão.");
        } // TesteExpressaoCondicionalResumida()

        private void TesteValidacaoDeExpressaoCondicional(Assercoes assercao)
        {
            string expressaoCondicional_b = "((a+b)<(c+d))";


            List<string> tokensExpressaoCondicional = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { expressaoCondicional_b }).GetTokens();
            Escopo escopo = new Escopo(new List<string>() { expressaoCondicional_b });

            Expressao exprss = new Expressao(tokensExpressaoCondicional.ToArray(), escopo);
            bool result = Expressao.Instance.ValidaExpressaoCondicional(exprss, escopo);

            assercao.MsgSucess("Validação de Expressão condicional feita com exatidão.");
            if (result)
                assercao.MsgSucess("Validação de Expressão condicional feita com exatidão.");
        } // TesteValidacaoDeExpressaoCondicional()


        private void TesteCapturaErrosPosOrdem(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            List<string> codigoComErro = new List<string> { "a=b+c;" };
            List<string> tokensComErro = new Tokens(linguagem, codigoComErro).GetTokens();
            Escopo escopoErro = new Escopo(codigoComErro);
            Expressao expressaoComErro = new Expressao(tokensComErro.ToArray(), escopoErro);

            Expressao expressaoComErroPosOrdem = expressaoComErro.PosOrdemExpressao();

            assercao.MsgSucess("processamento pos-ordem sem erros fatais.");

        } // TesteCapturaErrosPosOrdem()

        private void TesteObtemValidarExpressaoGeralResumida(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

            List<string> codigoTotal = new List<string>() { "int a; int b; int c; int d; c=a+b+1+b*2; d=++a+b+1+ --b*2; "};

            List<string> codigoExpressaoCenarioUM = new List<string>() { "c=a+b+1+b*2;" };
            List<string> codigoExpressaoCenarioDOIS = new List<string>() { "c=++a+b+1+ --b*2;" };

            List<string> tokensDoCodigoTotal = new Tokens(linguagem, codigoTotal).GetTokens();

            List<string> tokensDaExpressaoCasoUM = new Tokens(linguagem, codigoExpressaoCenarioUM).GetTokens();

            ProcessadorDeID processador = new ProcessadorDeID(codigoTotal);
           

            processador.CompileEmDoisEstagios(); // criação de escopo (validação de variáveis, funções, classes, blocos, etc..).

            assercao.MsgSucess("Construção de escopos das expressões feito sem erros fatais.");

            if (processador.escopo.tabela.GetExpressoes().Count == 1)
                assercao.MsgSucess("Construção de escopos para expressões e definições de variáveis feito com exatidão.");
///________________________________________________________________________________________________________________________________________________________________
            // teste com operadores binarios.
            Expressao expressaoAResumir = new Expressao(tokensDaExpressaoCasoUM.ToArray(), processador.escopo);
            List<string> tokensExpressaoResumidaCasoUM = Expressao.Instance.ObtemExpressaoGeralResumida(expressaoAResumir,processador.escopo);

            assercao.MsgSucess("Construção de expressão resumida para expressao 1  feita sem erros fatais.");

            bool result = Expressao.Instance.ValidaExpressoesGeral(tokensExpressaoResumidaCasoUM);
            if (result)
                assercao.MsgSucess("teste para expressao 1 completado sucessamente.");

            
            List<string> tokensDaExpressaoCasoDOIS = new Tokens(linguagem, codigoExpressaoCenarioDOIS).GetTokens(); // teste com operadores unários e binários.
            Expressao expressaoCasoDOIS = new Expressao(tokensDaExpressaoCasoDOIS.ToArray(), processador.escopo);
            List<string> tokensExpressaoGeralResumidaCasoDOIS = Expressao.Instance.ObtemExpressaoGeralResumida(expressaoCasoDOIS, processador.escopo);


            assercao.MsgSucess("Construção de expressão resumida para expressao 2  feita sem erros fatais.");

            bool result2 = Expressao.Instance.ValidaExpressoesGeral(tokensExpressaoGeralResumidaCasoDOIS);
            if (result2)
                assercao.MsgSucess("teste para expressao 2 completado sucessamente.");

        } // TesteObtemListaGeralResumida()

        private void TesteExpressoesComVetor(Assercoes assercao)
        {

            // a primeira passada do teste  serve para validar corretamente o teste, para futuros testes com este teste, para detectar quebra de código, seja feito.
            string codigo = "a[b]";
            Escopo escopo = new Escopo(new List<string>() { codigo });
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { codigo }).GetTokens();

            Expressao expressao = new Expressao(tokens.ToArray(), escopo);

            assercao.MsgSucess("construção de expressão com vetores feita sem erros fatais.");

            if (((Expressao)expressao.Elementos[0]).Elementos.Count == 4)
                assercao.MsgSucess("Construção de expressão com vetores calculada com extadidão.");
        }


        private void TesteVariaveisVetor(Assercoes assercao)
        {
            string codigo = "int a[20];";
           
            Escopo escopoVariavelVetor = new Escopo(new List<string>() { codigo });
            Vetor valorVariavelVetor = new Vetor("public", "a", "int", escopoVariavelVetor, new int[] { 20 });


            escopoVariavelVetor.tabela.AddObjetoVetor("public", "a", "int", new int[] { 20 }, escopoVariavelVetor, false);
            assercao.MsgSucess("construção de variável vetor construído sem erros fatais.");

            if (escopoVariavelVetor.tabela.GetVetores().Count == 1)
                assercao.MsgSucess("construção de variável vetor construido com exatidão.");

        } 
        
        private void TesteEvalExpression(Assercoes assercao)
        {
            List<string> codigo = codigo = new List<string>() { "a*b+1+c" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
            Escopo escopo = new Escopo(codigo);

            escopo.tabela.AddObjeto("public", "a", "int", 1, escopo);
            escopo.tabela.AddObjeto("public", "b", "int", 5, escopo);
            escopo.tabela.AddObjeto("public", "c", "int", 3, escopo);

            Expressao expressao = new Expressao(tokens.ToArray(), escopo);

            expressao = expressao.PosOrdemExpressao();
            EvalExpression eval = new EvalExpression();
            object result = eval.EvalPosOrdem(expressao, escopo);

            assercao.MsgSucess("Avaliação de expressão pela classe Eval feita sem erros fatais.");
            if ((int)result == 9) 
                assercao.MsgSucess("Avaliação de expressão pela classe Eval feita com exatidão.");


            codigo = new List<string>() { "a + b * c" };
            tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
            escopo = new Escopo(codigo);

            escopo.tabela.AddObjeto("public", "a", "int", 1, escopo);
            escopo.tabela.AddObjeto("public", "b", "int", 5, escopo);
            escopo.tabela.AddObjeto("public", "c", "int", 3, escopo);

            expressao = new Expressao(tokens.ToArray(), escopo);

            eval = new EvalExpression();
            result = eval.EvalPosOrdem(expressao, escopo);

            assercao.MsgSucess("Avaliação de expressão pela classe Eval feita sem erros fatais.");
            if ((int)result == 16)
                assercao.MsgSucess("Avaliação de expressão pela classe Eval feita com exatidão.");


            codigo = new List<string>() { "a+b" };
            tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
        
            escopo = new Escopo(codigo);
            escopo.tabela.AddObjeto("public", "a", "int", 1, escopo);
            escopo.tabela.AddObjeto("public", "b", "int", 5, escopo);
            expressao = new Expressao(tokens.ToArray(), escopo);

            eval = new EvalExpression();
            result = eval.EvalPosOrdem(expressao, escopo);

            assercao.MsgSucess("Avaliação de expressão pela classe Eval feita sem erros fatais.");
            if ((int)result == 6)
                assercao.MsgSucess("Avaliação de expressão pela classe Eval feita com exatidão.");

            codigo = new List<string>() { "a- b" };
            tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
            expressao = new Expressao(tokens.ToArray(), escopo);
            escopo = new Escopo(codigo);

            escopo.tabela.AddObjeto("public", "a", "int", 1, escopo);
            escopo.tabela.AddObjeto("public", "b", "int", 5, escopo);
            
            
            eval = new EvalExpression();
            result = eval.EvalPosOrdem(expressao, escopo);

            assercao.MsgSucess("Avaliação de expressão pela classe Eval feita sem erros fatais.");
            if ((int)result == -4)
                assercao.MsgSucess("Avaliação de expressão pela classe Eval feita com exatidão.");


            codigo = new List<string>() { "a * b" };
            tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
            escopo = new Escopo(codigo);

            escopo.tabela.AddObjeto("public", "a", "int", 1, escopo);
            escopo.tabela.AddObjeto("public", "b", "int", 5, escopo);

            expressao = new Expressao(tokens.ToArray(), escopo);

            eval = new EvalExpression();
            result = eval.EvalPosOrdem(expressao, escopo);

            assercao.MsgSucess("Avaliação de expressão pela classe Eval feita sem erros fatais.");
            if ((int)result == 5)
                assercao.MsgSucess("Avaliação de expressão pela classe Eval feita com exatidão.");

        }

        private void TesteExtracaoDeExpressoes(Assercoes assercoes)
        {
            string codigo = "a+b+(c);d+c;";
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { codigo }).GetTokens();
            Escopo escopo = new Escopo(new List<string>() { codigo });


            Objeto v1 = new Objeto("public", "a", "int", null);
            Objeto v2 = new Objeto("public", "b", "int", null);
            Objeto v3 = new Objeto("public", "c", "int", null);
            Objeto v4 = new Objeto("public", "d", "int", null);
            escopo.tabela.GetObjetos().Add(v1);
            escopo.tabela.GetObjetos().Add(v2);
            escopo.tabela.GetObjetos().Add(v3);
            escopo.tabela.GetObjetos().Add(v4);


            
            List<Expressao> expressoes = Expressao.Instance.ExtraiExpressoes(escopo, tokens);
            assercoes.MsgSucess("Extração de expressões feita sem erros fatais.");

            if (expressoes.Count == 2)
                assercoes.MsgSucess("Extracao de expressoes feita com exatidão");

            string codigo2 = "c+b;";
            List<string> tokens2 = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { codigo2 }).GetTokens();
            Escopo escopo2 = new Escopo(new List<string>() { codigo2 });

            escopo2.tabela.GetObjetos().Add(v1);
            escopo2.tabela.GetObjetos().Add(v2);
            escopo2.tabela.GetObjetos().Add(v3);
            escopo2.tabela.GetObjetos().Add(v4);

            List<Expressao> expressoes2 = Expressao.Instance.ExtraiExpressoes(escopo2, tokens2);

            assercoes.MsgSucess("Extração de expressões feita sem erros fatais.");

            if (expressoes2.Count == 1)
                assercoes.MsgSucess("Extracao de expressoes feita com exatidão");
        }


        public void TesteComInstrucoesEConstrucaoDeEscopos(Assercoes assercao)
        {


            List<string> codigo1 = new List<string>() { "while (i>=0) {i=i-1;}" };
            Escopo escopo1 = new Escopo(codigo1);
            TesteDeInstrucoes(assercao, codigo1, ref escopo1);


            if ((escopo1.escopoFolhas.Count == 1) && (escopo1.escopoFolhas[0].GetMsgErros().Count == 1))
                assercao.MsgSucess("calculos para instrução while com erro, feito com exatidão.");
            else
                assercao.MsgFail("erro na instrução while com erros.");



            List<string> codigo2 = new List<string>() { "i=i+1;" };
            Escopo escopo2 = new Escopo(codigo2);

            TesteDeInstrucoes(assercao, codigo2, ref escopo2);

            if (escopo2.GetMsgErros().Count == 1)
                assercao.MsgSucess("calculos para instrução de atribuicao com erros feitos, feito com exatidão.");
            else
                assercao.MsgFail("erro na instrução de atribuição com erros");




            List<string> codigo3 = new List<string>() { "int b=0; for (int x=0; x<5; x++){ int a=5;}" };
            Escopo escopo3 = new Escopo(codigo3);
            TesteDeInstrucoes(assercao, codigo3, ref escopo3);

            if ((escopo3.escopoFolhas.Count == 1) && (escopo3.tabela.GetObjetos().Count == 1) && (escopo3.escopoFolhas[0].tabela.GetObjetos().Count == 1))
                assercao.MsgSucess("calculos para instrução de for sem erros, feito com exatidão.");
            else
                assercao.MsgFail("erro na instrução for com erros.");




            List<string> codigo4 = new List<string>() { "int i=0; int j=1; if (i<j){ i=i+1; }" };
            Escopo escopo4 = new Escopo(codigo4);
            TesteDeInstrucoes(assercao, codigo4, ref escopo4);
            if ((escopo4.tabela.GetObjetos().Count == 2) && (escopo4.escopoFolhas.Count == 1))
                assercao.MsgSucess("calculos para instrução if sem erros de inicialização de variáveis, feito com exatidão.");
            else
                assercao.MsgFail("erro na instrução if com erros.");








       
          

            List<string> codigo6 = new List<string>() { "int funcaoA(int n, int m);" };
            Escopo escopo6 = new Escopo(codigo6);
            TesteDeInstrucoes(assercao, codigo6, ref escopo6);


            if (escopo6.tabela.GetFuncoes().Count == 1)
                assercao.MsgSucess("calculo de definição de função com dois parâmetros feito com exatidão.");
            else
                assercao.MsgFail("erro na definição de função com dois parâmetros.");

            List<string> codigo7 = new List<string>() { "int funcaoA(int n);" };
            Escopo escopo7 = new Escopo(codigo7);
            TesteDeInstrucoes(assercao, codigo7, ref escopo7);
            if (escopo7.tabela.GetFuncoes().Count == 1)
                assercao.MsgSucess("calculo de definição de função de um parâmetro feito com exatidão.");
            else
                assercao.MsgFail("erro na definição de função com um parâmetro.");

        }

        ///  método auxiliar para testes de compilação de instruções.
        private static void TesteDeInstrucoes(Assercoes assercao, List<string> codigo, ref Escopo escopo)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
        
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();
            escopo = processador.escopo;

            assercao.MsgSucess("construção de escopos com instruções feito sem erros fatais.");

        }


        private void TesteExtracaoDeClasses(Assercoes assercao)
        {
            string fileClasse = "classe_classeA.txt";

            List<string> codigo = ReadClassFile(fileClasse);

            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();

            Escopo escopo = new Escopo(codigo);
            ExtratoresOO extrator = new ExtratoresOO(escopo, LinguagemOrquidea.Instance(), tokens);

            Classe umaClasse = extrator.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);

            System.Console.WriteLine(umaClasse);
            System.Console.ReadLine();
        }

        private List<string>  ReadClassFile(string fileClasse)
        {
            FileStream stream = new FileStream(fileClasse, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            List<string> codigo = new List<string>();
            while (!reader.EndOfStream)
            {
                codigo.Add(reader.ReadLine());
            }
            return codigo;
        }

        /// <summary>
        ///  compila um arquivo contendo codigo de instruções orquidea.
        /// </summary>
        private void ConstroiInstrucoes(string nomeArquivo, ProcessadorDeID processador)
        {
            ParserAFile parser = new ParserAFile(nomeArquivo);
            processador = new ProcessadorDeID(parser.GetCode());
            processador.CompileEmDoisEstagios();

        }
        private void TesteConstrucaoDeProgramasOrquideaAtravesDeArquivos(Assercoes assercao)
        {

            ProcessadorDeID processador = null;
            ConstroiInstrucoes("arquivo5Orquidea.txt", processador);

            if (processador.GetInstrucoes().Count == 2)
                assercao.MsgSucess("arquivo com definição de função e com uso de instrução while feitas com extatidao.");
            else
                assercao.MsgFail("falha no arquivo com definição de função e com uso de instrução while feitas");



            ConstroiInstrucoes("arquivo2Orquidea.txt", processador); // class classeB { public int a ; public int funcaoB ( int x ){ if (x>1) { x=0; } } } 
            if (processador.GetInstrucoes().Count == 4)
                assercao.MsgSucess("arquivo com definição de função e com uso de instrução if/else feitas com exatidão.");
            else
                assercao.MsgFail("falha no arquivo com definição de função e com uso de instrução if/else.");



            ConstroiInstrucoes("arquivo3Orquidea.txt", processador); // class classeA { public int a ; public int b = 1 ; } 
            
            if (processador.GetInstrucoes().Count == 6)
                assercao.MsgSucess("2 instrucoes de atribuicao feitas com exatidão.");
            else
                assercao.MsgFail("falha em 2 instrucoes de atribuicao.");


            ConstroiInstrucoes("arquivo1Orquidea.txt", processador); // class classeA { public int a ; public int funcaoB ( int x ){ int c=1;} } 
            if (processador.GetInstrucoes().Count == 5 + 3)
                assercao.MsgSucess("isntrução com definição de função feito com exatidão.");
            else
                assercao.MsgFail("isntrução com definição de função.");


            assercao.MsgSucess("compilacao de arquivos de instruções feito sem erros fatais.");
        }

  
 
        private void TesteChamadaDeFuncao(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            string codigoFuncao = "int funcaoB(int x, int y){x=1;}";
            string chamadaDeFuncao = "funcaoB(x, x)";

            List<string> codigo = new List<string>() { codigoFuncao, chamadaDeFuncao };

            List<string> tokens = new Tokens(linguagem, codigo).GetTokens();

            ProcessadorDeID processador = new ProcessadorDeID(tokens);
            
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("instrucao de chamada de funcao feito sem erros fatais.");

            if (processador.GetInstrucoes().Count == 2)
                assercao.MsgSucess("instrucoes do codigo feito com exatidao.");

        } // TesteChamadaDeFuncao()

        private void TesteNovoMetodoExtraiExpressoes(Assercoes assercao)
        {

            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            string codigo = "int A; int B; int C= A+B";

            List<string> code = new List<string>() { codigo };
            List<string> tokens = new Tokens(linguagem, code).GetTokens();

            Escopo escopo = new Escopo(code);

            List<Expressao> expressoes = Expressao.Instance.ExtraiExpressoes(escopo, tokens);

            assercao.MsgSucess("Extracao de expressoes feito sem erros fatais");
            if (expressoes.Count == 5)
                assercao.MsgSucess("Extracao de expressoes feito com exatidao");
        }

        private void TesteManipulacaoDeVariaveisEstaticas(Assercoes assercao)
        {
            LinguagemOrquidea lng = LinguagemOrquidea.Instance();
            string str_codigo = "static int A;";
            List<string> codigo = new List<string>() { str_codigo };
         
            
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("obtenção de variáveis estáticas feito sem erros fatais.");

            if (processador.escopo.tabela.GetObjetos().Count == 1)
                assercao.MsgSucess("registro de variável estatica feito com exatidão.");

        } // TesteManipulacaoDeVariaveisEstaticas().

        public void testeInstrucaoCasesOfUse(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();


            // cenario de teste: var case é uma string.
            char aspas = '\"';
            List<string> codigo2 = new List<string>()
            {
               "string a=1; string b=1; casesOfUse a: ( case >" + aspas +"maquina"+ aspas+": a=a+1; case < b: b=b+1; )"
            };

            cenarioTesteParaCasesOfUse(assercao, linguagem, codigo2, 3);


            // cenario de teste: var case são variáveis.
            List<string> codigo = new List<string>()
            {
               "int a=1; int b=1; casesOfUse a: ( case > b: a=a+1; case < b: b=b+1; )"
            };

            cenarioTesteParaCasesOfUse(assercao, linguagem, codigo, 3);


        }

        private static void cenarioTesteParaCasesOfUse(Assercoes assercao, LinguagemOrquidea linguagem, List<string> codigo, int contadorValidacao)
        {
            List<string> tokens = new Tokens(linguagem, codigo).GetTokens();
            Escopo escopo = new Escopo(codigo);
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("compilacao de instrucao casesOfUse feita sem erros fatais.");
            if (processador.GetInstrucoes().Count == contadorValidacao)  // para futuros testes automatizados, a fim de verificar se o código não quebrou, por causa de modificações no código.
                assercao.MsgSucess("instrucoes do codigo do cenario feito com exatidao.");
        }

        public CorpoTestes_2()
        {

            Teste testeObterExpressoes_testes = new Teste(this.TesteGetExpressoes, "teste de obter expressões por método heuristico.");
            Teste testeObterSequenciasID_testes = new Teste(this.TesteCalculoDeVariasSequenciasID, "teste de obter sequencias id com novo método ObterSequenciasID().");
            Teste testeObterProducoes_testes = new Teste(this.TesteObterProducoes, "teste para obter produções com o método Tokens.GetProducoes()");
            Teste testeExpresaoCondicionalAResumir_testes = new Teste(this.TesteExpressaoCondicionalResumida, "teste para extrair uma expressão condicional resumida.");
            Teste testeValidarExpressaoCondicional_testes = new Teste(this.TesteValidacaoDeExpressaoCondicional, "teste para validar uma expressão condicional ");
            Teste testeExpressaoComErroParaPorOrdem_testes = new Teste(this.TesteCapturaErrosPosOrdem, "verificação de uma expressão com erro colocada em pos-ordem");
            Teste testeObterExpressaoResumida_testes = new Teste(this.TesteObtemValidarExpressaoGeralResumida,"teste para construção de expressão resumida, para fins de validações de expressões.");
            Teste testeConstrucaoExpressaoComVetor_teste = new Teste(this.TesteExpressoesComVetor, "teste para expressão com vetores.");
            Teste testeConstrucaoDeVariavelVetor_teste = new Teste(this.TesteVariaveisVetor, "teste de construção de variável vetor.");
            Teste testeEvalExpression_teste = new Teste(this.TesteEvalExpression, "teste para avaliação de uma expressão.");
            Teste testeExtracaoDeExpressoes_teste = new Teste(this.TesteExtracaoDeExpressoes, "teste com o método Extração de Expressões.");
            Teste testeConstrucaoDeEscoposComInstrucoes_teste = new Teste(this.TesteComInstrucoesEConstrucaoDeEscopos, "teste para construção de escopos com instruções.");
            Teste testeExtracaoDeClassesAPartirDeArquivo_teste = new Teste(this.TesteExtracaoDeClasses, "teste para visualizar a extração de classes, métodos, e propriedades.");
            Teste testeCompilaComArquivos_teste = new Teste(TesteConstrucaoDeProgramasOrquideaAtravesDeArquivos, "teste para construção de programas com multiplos arquivos.");
            Teste testeChamadaDeFuncao_teste = new Teste(this.TesteChamadaDeFuncao, "teste para instrução chamada de funcao.");
            Teste testeExtracaoDeExpressoesNovoMetodo_teste = new Teste(this.TesteNovoMetodoExtraiExpressoes, "teste para a extração de expressoes");
            Teste testeVariaveisEstaticas_teste = new Teste(this.TesteManipulacaoDeVariaveisEstaticas, "testes para manipulação de variáveis estáticas");
            Teste testeCasesOfUse_teste = new Teste(this.testeInstrucaoCasesOfUse, "testes para compilacao instrucao casesOfUse");

            ContainerTestes ContainerTestes = new ContainerTestes(testeConstrucaoDeEscoposComInstrucoes_teste);


            ContainerTestes.ExecutaTestesEExibeResultados();
        } // CorpoTestesMaquinaVirtual()
    } // class
} //namespace