using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ModuloTESTES;

using parser.PROLOG;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser.Manutencao
{
    public class CorpoTestesManutencao
    {


        private void TesteConsultaBackTracking(Assercoes assercao)
        {
            string textoBaseConhecimento = "homem(jose), pai(jose,maria), pai(maria,joao)."; // forma o banco de dados, a base de conhecimento prolog.
            BaseDeConhecimento.Instance().AddPredicates(textoBaseConhecimento);



            Regra umaRegra = Regra.GetRegra("homem(X):-pai(X, Y),pai(Y, Z)"); // inicializa uma regra, para consulta sobre a base de conhecimento.


            List<Predicado> predicadosConsulta = Consultas.Instance().ConsultaBackTracking(umaRegra);
            assercao.IsTrue((predicadosConsulta != null) && (predicadosConsulta.Count == 1));
            assercao.IsTrue(predicadosConsulta[0].GetAtomos().Find(k => k.Equals("jose")) != null);


        }



        private void TesteObtencaoEModificacaoDeVariaveisDeUmaRegraProlog(Assercoes assercao)
        {
            string textoBaseConhecimento = "homem(jose), pai(jose,maria), pai(maria,joao)."; // forma o banco de dados, a base de conhecimento prolog.
            BaseDeConhecimento.Instance().AddPredicates(textoBaseConhecimento);


            Regra umaRegra = Regra.GetRegra("homem(X):-pai(X, Y),pai(Y, Z)"); // inicializa uma regra, para consulta sobre a base de conhecimento.

            Dictionary<string, string> variaveisGuardadasAnteriormente = new Dictionary<string, string>();

            Consultas.SubstituiVariaveis(BaseDeConhecimento.Instance().Base[0], umaRegra.PredicadoBase, variaveisGuardadasAnteriormente);
            Consultas.SubstituiVariaveis(BaseDeConhecimento.Instance().Base[1], umaRegra.PredicadosGoal[0], variaveisGuardadasAnteriormente);
            Consultas.SubstituiVariaveis(BaseDeConhecimento.Instance().Base[2], umaRegra.PredicadosGoal[1], variaveisGuardadasAnteriormente);



            assercao.IsTrue(umaRegra.PredicadosGoal[1].GetAtomos().Find(k => k.Equals("joao")) != null);
            assercao.IsTrue(umaRegra.PredicadoBase.GetAtomos().Find(k => k.Equals("jose")) != null);
        }

        private void TesteGetTokensProlog(Assercoes assercao)
        {
            string strRegraTexto = "homem(X):-pai(X,Y),pai(Y,Z).";
            List<string> tokens = ParserPROLOG.GetTokens(strRegraTexto);

            assercao.IsTrue(tokens.Count == 19);
            assercao.MsgSucess("obtencao de tokens prolog feita sem erros fatais.");
        }

        private void TesteGetPredicados(Assercoes assercao)
        {

            string strRegraTexto = "homem(X):-pai(X,Y),pai(Y,Z),tio().";
            List<Predicado> predicados = ParserPROLOG.GetPredicados(strRegraTexto);

            if (predicados == null)
                assercao.Fail("falha ao encontrar predicados de: " + strRegraTexto);
            else
            {
                assercao.MsgSucess("obtencao de predicados feita sem erros fatais.");
                assercao.IsTrue(predicados.Count == 4);
            }

            string strRegraTextoSemAtomos = "tio().";
            List<Predicado> predicadosSemAtomos = ParserPROLOG.GetPredicados(strRegraTextoSemAtomos);
            assercao.IsTrue(predicadosSemAtomos.Count == 1);


        }

        private void TesteDefinicaoDeMetodo(Assercoes assercao)
        {
            string codigo = "public class classeB { int x=0; int funcaoA(int x) { x=0;}; };";

            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigo });
            processador.Compile();
            assercao.IsTrue((processador.escopo.tabela.GetClasse("classeB", processador.escopo) != null) && (processador.escopo.tabela.GetClasse("classeB", processador.escopo).GetMetodos().Count > 0));
        }


        private void TesteDefinicaoDeFuncao(Assercoes assercao)
        {
            string codigo = "int funcaoA(int x) { x=0;};";
            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigo });
            processador.Compile();
            assercao.IsTrue((processador.escopo.tabela.GetFuncoes().Count > 0) && (processador.escopo.tabela.GetFuncao("funcaoA") != null));


        }

        private void TesteAtribuicaoDePropriedadesSemDefinicao(Assercoes assercao)
        {
            string codigo = "public class classeB { int x=0;};  classeB variavelDaClasse; variavelDaClasse.x=1;";
            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigo });
            processador.Compile();

            assercao.IsTrue(processador.escopo.tabela.GetVar("variavelDaClasse", processador.escopo) != null);
        }

        private void TestePropriedadesEncadeadas(Assercoes assercao)
        {
            string codigoPrincipal = "public class classeB {};  public class classeA { classeB propriedade1; }; public classeA objetoA ; objetoA.propriedade1; objetoA.propriedade3; ";

            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigoPrincipal });
            processador.Compile();

            assercao.MsgSucess("avaliação de propriedades encadeadas feitas sem erros fatais.");
        }


        private void TesteBuildDefinicaoVariaveisOuPropriedades(Assercoes assercao)
        {

            string codigoPropriedadeEstaticaSemAtribuicao = "public static int x;";
            string codigoPropriedadeEstaticaComAtribuicao = "private static int x= 4 ;";

            string codigoPropriedadeNaoEstaticaSemAtribuicao = "public int x;";
            string codigoPropriedadeNaoEstaticaComAtribuicao = "public int x= 4;";

            string codigoVariavelNaoEstaticaSemAtribuicao = "int x;";
            string codigoVariavelNaoEstaticaComAtribuicao = "int x=1;";

            assercao.IsTrue(GetProcessamentoCodigo(codigoPropriedadeEstaticaComAtribuicao, "x", "int", true));
            assercao.IsTrue(GetProcessamentoCodigo(codigoPropriedadeEstaticaSemAtribuicao, "x", "int", true));

            assercao.IsTrue(GetProcessamentoCodigo(codigoPropriedadeNaoEstaticaSemAtribuicao, "x", "int", false));
            assercao.IsTrue(GetProcessamentoCodigo(codigoPropriedadeNaoEstaticaComAtribuicao, "x", "int", false));


            assercao.IsTrue(GetProcessamentoCodigo(codigoVariavelNaoEstaticaSemAtribuicao, "x", "int", false));
            assercao.IsTrue(GetProcessamentoCodigo(codigoVariavelNaoEstaticaComAtribuicao, "x", "int", false));



        }

        private bool GetProcessamentoCodigo(string str_codigo, string nomeDaVariavel, string tipoDaVariavel, bool isStatic)
        {


            List<string> codigo = new List<string>() { str_codigo };
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.Compile();


            if (!isStatic)
                return processador.escopo.tabela.GetVar(nomeDaVariavel, processador.escopo) != null;
            if (isStatic)
            {
                Classe classeDaPropriedadeEstatica = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDaVariavel);
                return (classeDaPropriedadeEstatica.propriedadesEstaticas.Find(k => k.GetNome().Equals(nomeDaVariavel)) != null);
            }
            return false;
        }


        private void TesteBuildInstrucoes(Assercoes assercao)
        {

            List<string> codigoComMuitasInstrucoes = new List<string>() { "int y=1;  for (int x=0; x< 5; x++) {x=x*15;} while (y>0){ y=y+1;} if (y<0) {y=y-1;}" };
            ProcessadorDeID processaodoMultiplasInstrucoes = new ProcessadorDeID(codigoComMuitasInstrucoes);
            processaodoMultiplasInstrucoes.Compile();

            assercao.IsTrue(processaodoMultiplasInstrucoes.GetInstrucoes().Count == 4);


            List<string> codigoAtribuicao = new List<string>() { "int x=0;" };
            ProcessadorDeID processadorAtribuicacao = new ProcessadorDeID(codigoAtribuicao);
            processadorAtribuicacao.Compile();

            assercao.IsTrue(processadorAtribuicacao.GetInstrucoes().Count == 1);

            List<string> codigoAtribuicaoEIfs = new List<string>() { "int x=0; if (x<0) {x=x+1;}" };
            ProcessadorDeID processadorIf = new ProcessadorDeID(codigoAtribuicaoEIfs);
            processadorIf.Compile();

            assercao.IsTrue(processadorIf.GetInstrucoes().Count == 2);



        }

        private void TesteInstrucoesEmCompilacao(Assercoes assercao)
        {
            ParserAFile parserTesteAceite = new ParserAFile("arquivo5Orquidea.txt");
            List<string> codigo = parserTesteAceite.GetCode();


            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.Compile();


            assercao.IsTrue(processador.escopo.tabela.GetClasses().Find(k => k.nome == "classeA") != null);


            Instrucao instrucaoWhile = processador.GetInstrucoes().Find(k => k.code == ProgramaEmVM.codeWhile);
            assercao.IsTrue((instrucaoWhile != null) && (instrucaoWhile.blocos != null) && (instrucaoWhile.blocos.Count == 1));
        }


        private void TesteSalvarCarregarPropriedadesEmArquivoXML(Assercoes assercao)
        {

            XDocument document = new XDocument();

            propriedade propriedadeASalvar1 = new propriedade("propriedadeA", "int", "0", false);


            PropriedadesXML escritaEmArquivoXml = new PropriedadesXML("arquivoPropriedadesTeste.xml");



            XElement raiz = null;

            escritaEmArquivoXml.BeginWrite();
            escritaEmArquivoXml.Write(propriedadeASalvar1, null);
            escritaEmArquivoXml.EndWrite(raiz);



            PropriedadesXML leituraEmArquivosXml = new PropriedadesXML("arquivoPropriedadesTeste.xml");
            propriedade propriedadeALer = leituraEmArquivosXml.Read(null);

            assercao.IsTrue((propriedadeALer != null) &&
                            (propriedadeASalvar1.GetNome() == propriedadeALer.GetNome()));

        }

        private void TesteLeituraGravacaoExpressoesXML(Assercoes assercao)
        {

            List<string> codigo = new List<string>() { "int a= 1;" };
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.Compile();


            Expressao exprssWrite = processador.GetInstrucoes()[0].expressoes[0];

            ExpressaoXML expressaoGravacao = new ExpressaoXML("arquivoExpressoesTeste.xml");

            expressaoGravacao.BeginWrite();
            expressaoGravacao.Write(exprssWrite, null);
            expressaoGravacao.EndWrite(null);


            ExpressaoXML expressaoLeitura = new ExpressaoXML("arquivoExpressoesTeste.xml");
            Expressao exprssRead = expressaoLeitura.Read(null);


            assercao.IsTrue(exprssWrite.ToString().Equals((exprssRead).ToString()));
        }

        private void TesteSalvarCarregarInstrucoesEmArquivoXML(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "for (int x=0; x< 5;x++){ x=x-1;}" };
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.Compile();

            Instrucao umaInstrucaoDoCodigo = processador.GetInstrucoes()[0];


            InstrucaoXML instrucaoXmlSave = new InstrucaoXML("arquivoInstrucaoTeste.xml");
            XElement rootSave = null;

            instrucaoXmlSave.BeginWrite();
            instrucaoXmlSave.Write(umaInstrucaoDoCodigo, rootSave);
            instrucaoXmlSave.EndWrite(rootSave);


            InstrucaoXML instrucaoXmlRead = new InstrucaoXML("arquivoInstrucaoTeste.xml");
            XElement rootLoad = null;


            Instrucao umaIntrucaoDoCodigoLida = instrucaoXmlRead.Read(rootLoad);
            assercao.IsTrue(umaIntrucaoDoCodigoLida != null);

        }

        private void TesteSalvarCarregarFuncaoEmArquivoXML(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int funcaoA(int x) {x=0; return x;}" };
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.Compile();

            List<Funcao> funcao = processador.escopo.tabela.GetFuncao("funcaoA");
            if ((funcao == null) || (funcao.Count == 0))
            {
                assercao.MsgFail("falha no calculo da funcao do codigo do teste");
                return;
            }

            XElement raiz = null;
            MetodosXML umMetodoXML_write = new MetodosXML("arquivoFuncaoTeste.xml");
            umMetodoXML_write.BeginWrite();
            umMetodoXML_write.Write(funcao[0], raiz);
            umMetodoXML_write.EndWrite(raiz);

            MetodosXML umMetodoXML_read = new MetodosXML("arquivoFuncaoTeste.xml");
            Funcao funcaoLida = umMetodoXML_read.Read(null);


            assercao.IsTrue(funcaoLida.nome.Equals("funcaoA"));
        }

        private void TesteParaParserQualquerLinguagem(Assercoes assercao)
        {
            string codigo = "for (int x=0; x<5; x++){ int y=11;}";
            LinguagemOrquidea lng = new LinguagemOrquidea();
            List<string> tokens_termosChave = lng.GetTodosTermosChave();
            List<string> tokens_operadores = lng.GetTodosOperadores();

            ParserAnyLanguage parser = new ParserAnyLanguage();
            List<string> tokensEncontrados = parser.GetTokens(codigo, tokens_termosChave, tokens_operadores);
            assercao.IsTrue((tokensEncontrados != null) && (tokensEncontrados.Count == 21));

        }

        private void TesteParaOtimizacaoDeExtraiExpressoes(Assercoes assercao)
        {
            string codigo = "int x=x+1;";
            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigo });
            processador.Compile();
            assercao.IsTrue(processador.escopo.tabela.GetVariaveis().Count == 1);
        }

        private void testeBuildOperadorBinario(Assercoes assercao)
        {
            LinguagemOrquidea lng = new LinguagemOrquidea();
            string codigoFuncaoOperador = "int funcaoA( int D, int C) {return 1}";
            // /// operador ID ID ( ID ID, ID ID ) prioridade ID meodo ID ;
            string codigoDefinicaoOperador = "operador int Maior (int A, int B) prioridade 1 metodo funcaoA;";

            List<string> tokensDefinicaoOperador = new Tokens(lng, new List<string>() { codigoDefinicaoOperador }).GetTokens();
            List<string> tokensFuncaoOperador = new Tokens(lng, new List<string>() { codigoFuncaoOperador }).GetTokens();


            List<string> tokens = tokensFuncaoOperador;
            tokens.AddRange(tokensDefinicaoOperador);

            ProcessadorDeID processador = new ProcessadorDeID(tokens);
            processador.Compile();

            assercao.IsTrue(processador.escopo.tabela.GetFuncao("funcaoA") != null);
            assercao.IsTrue(processador.escopo.tabela.GetClasse("int", processador.escopo).GetOperadores().Find(k => k.nome.Equals("Maior")) != null);

        }

        private void testeBuildOperadorXML(Assercoes assercao)
        {
            LinguagemOrquidea lng = new LinguagemOrquidea();
            string codigoFuncaoOperador = "int funcaoA( int D, int C) {return 1}";
            // /// operador ID ID ( ID ID, ID ID ) prioridade ID meodo ID ;
            string codigoDefinicaoOperador = "operador int Maior (int A, int B) prioridade 1 metodo funcaoA;";

            List<string> tokensDefinicaoOperador = new Tokens(lng, new List<string>() { codigoDefinicaoOperador }).GetTokens();
            List<string> tokensFuncaoOperador = new Tokens(lng, new List<string>() { codigoFuncaoOperador }).GetTokens();


            List<string> tokens = tokensFuncaoOperador;
            tokens.AddRange(tokensDefinicaoOperador);

            ProcessadorDeID processador = new ProcessadorDeID(tokens);
            processador.Compile();

            Operador operadorDoTeste = processador.escopo.tabela.GetClasse("int", processador.escopo).GetOperador("Maior");

            OperadorXML operadorXML_write = new OperadorXML();
            operadorXML_write.BeginWrite();
            operadorXML_write.Write(null, operadorDoTeste);
            operadorXML_write.EndWrite(null);


            OperadorXML operadorXML_read = new OperadorXML();
            Operador operador_lido = operadorXML_read.Read(null);

            assercao.IsTrue((operador_lido != null) && (operador_lido.nome.Equals("Maior")));


        }

        public class Teste_01 : SuiteClasseTestes
        {
            int numeroId_teste = 0;

            public void TesteCenario1(AssercaoSuiteClasse assercao)
            {
                LoggerTests.AddMessage("teste cenario 1 executado.");
            }

            public void TesteCenario2(AssercaoSuiteClasse assercao)
            {
                LoggerTests.AddMessage("teste cenario 2 executado.");
            }

            public void TesteCenario3(AssercaoSuiteClasse assercao)
            {
                LoggerTests.AddMessage("teste cenario 3 executado.");
            }

            public void TesteGet_id(AssercaoSuiteClasse assercao)
            {
                assercao.IsTrue(this.numeroId_teste == 0);
            }
            public void Antes()
            {
                LoggerTests.AddMessage("metodo preparador executado.");
            }

            public void Depois()
            {
                LoggerTests.AddMessage("metodo finalizador executado.");
            }
        }


        private void TesteExtracaoDeMetodosEPropriedadesDeUmaClasse(Assercoes assercao)
        {
            /// teste para o metodo recodificado ProcessadorID.ResumeExpressoes().
            string codigo = "public class classeA { public int a ; public int funcaoB ( int x ){ while (x>0) {x= x+1;} } } ";

            LinguagemOrquidea lng = new LinguagemOrquidea();
            List<string> tokensDoCodigo = new Tokens(lng, new List<string>() { codigo }).GetTokens();
            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigo });
            processador.Compile();


            assercao.IsTrue((RepositorioDeClassesOO.Instance().classesRegistradas.Count == 7) && (RepositorioDeClassesOO.Instance().ObtemUmaClasse("classeA") != null));

        }


        private void TesteObjetosXML(Assercoes assercao)
        {

            string codigoDefinicaoDeClasse = "public class classeA { public int a ; public int funcaoB ( int x ){ while (x>0) {x= x+1;} } } ";
            string codigoDefinicaoDeObjeto = "classeA n= create(int n1, int n2);";  // "ID ID = create ( ID , ID "

            List<string> codigoTotal = new List<string>() { codigoDefinicaoDeClasse, codigoDefinicaoDeObjeto };

            ProcessadorDeID processador = new ProcessadorDeID(codigoTotal);
            processador.Compile();

            Objeto objetoEscrito = new Objeto("classeA", "umObjeto", "5", processador.escopo);



            FileForObjeto arquivoObjetoEscrever = new FileForObjeto("objetoDump.xml");
            arquivoObjetoEscrever.BeginWrite();
            arquivoObjetoEscrever.Write(objetoEscrito, null);
            arquivoObjetoEscrever.EndWrite(null);


            FileForObjeto arquivoObjetoLer = new FileForObjeto("objetoDump.xml");
            Objeto objeto1Lido = arquivoObjetoLer.Read(null);

            assercao.IsTrue(objeto1Lido != null);
            assercao.IsTrue(RepositorioDeClassesOO.Instance().ObtemUmaClasse("classeA") != null);
            assercao.IsTrue((objeto1Lido.GetNome().Equals(objetoEscrito.GetNome())) && ((objeto1Lido.GetField("a") != null)) && (objetoEscrito.GetField("a") != null));


        }

        private void TesteTokensPolemicos(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            string codigo = "int forma= inwhile+ ifa";
            List<string> tokens = new Tokens(linguagem, new List<string>() { codigo }).GetTokens();

            assercao.MsgSucess("teste para verificacao de tokens polemicos feito sem erros fatais.");
            assercao.IsTrue(tokens.Count == 6);

        }

        private void testeInstrucaoImporter(Assercoes assercao)
        {
            string nomeAssembly = "ParserLinguagemOrquidea.exe";
            string instrucaoImporter = "importer ( " + nomeAssembly + ")";

            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { instrucaoImporter });
            processador.Compile();

            assercao.MsgSucess("instrucao importer processada sem erros fatais.");


            ImportadorDeClasses importador = new ImportadorDeClasses(nomeAssembly);
            importador.ImportAllClassesFromAssembly();

            assercao.IsTrue(RepositorioDeClassesOO.Instance().classesRegistradas.Count > 6);
        }

        private void testeExtracaoDeExpressoes(Assercoes assercao)
        {
            // teste novamente para o metodo Expressao.ExtraiExpressoes, pois o codigo foi substituido por um outro, menos sujeito a erros, e com melhor legibilidade.

            string codigo = "int y; int z; int x(int y, int z) {y=1;} ";
            string codigoExpressao = "x(1,5)+y+z;";

            List<string> tokensDaExpressao = new Tokens(new LinguagemOrquidea(), new List<string>() { codigoExpressao }).GetTokens();
            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigo });
            processador.Compile();


            List<Expressao> exprss = Expressao.Instance.ExtraiExpressoes(processador.escopo, tokensDaExpressao);
            assercao.IsTrue(exprss[0].Elementos.Count == 5);
        }

        private void testeVerificacaoResumeCorretoDeExpressoes(Assercoes assercao)
        {
            string codigo = "public int a= 1";
            ProcessadorDeID procesador = new ProcessadorDeID(new List<string>() { codigo });
            procesador.Compile();
            assercao.MsgSucess("cenario de testes terminado sem erros fatais.");
            assercao.IsTrue(procesador.escopo.tabela.GetVariaveis().Count == 1);
        }

        private void testeChamadaDeMetodo(Assercoes assercao)
        {
            string codigoDefinicaoDeClasse = "public class classeA { public int a ; public int funcaoB ( int x ){ while (x>0) {x= x+1;} } } ";
            string codigoDefinicaoDeObjeto = "classeA umObjeto;";
            string codigoChamadaDeFuncao = "umObjeto.funcaoB(1);";

            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            List<string> tokensDefinicaoDeClasse = new Tokens(linguagem, new List<string>() { codigoDefinicaoDeClasse }).GetTokens();
            List<string> tokensDefinicaoDeObjeto = new Tokens(linguagem, new List<string>() { codigoDefinicaoDeObjeto }).GetTokens();
            List<string> tokensChamadaDeFuncao = new Tokens(linguagem, new List<string>() { codigoChamadaDeFuncao }).GetTokens();

            List<string> tokensDoCenario = tokensDefinicaoDeClasse.ToList<string>();
            tokensDoCenario.AddRange(tokensDefinicaoDeObjeto);
            tokensDoCenario.AddRange(tokensChamadaDeFuncao);



            ProcessadorDeID processador = new ProcessadorDeID(tokensDoCenario);
            processador.Compile();

            assercao.MsgSucess("processamento de chamada de metodo executado sem erros fatais.");
            assercao.IsTrue((processador.GetInstrucoes() != null) && (processador.GetInstrucoes().Count == 2));

        }


        private void testePosicaoECodigoClasse(Assercoes assercao)
        {
            string codigoAtribuicao = "int x= 0";
            string codigoWhileCondicional = "while (x<1)";
            string codigoWhileCorpo1 = "{";
            string codigoWhileCorpo2 = "x=x+1;";
            string codigowhileCorpo3 = "}";

            // cenario mais simples.
            List<string> codigoTotal = new List<string>() { codigoAtribuicao, codigoWhileCondicional, codigoWhileCorpo1, codigoWhileCorpo2, codigowhileCorpo3 };
            List<string> tokensALocalizar = new List<string>() { "while (x<1)" };

            PosicaoECodigo posicaoDeTokens = new PosicaoECodigo(tokensALocalizar, codigoTotal);

            assercao.MsgSucess("objeto posicao de tokens cenario simples instanciado sem erros fatais.");
            assercao.IsTrue((posicaoDeTokens.linha == 1) && (posicaoDeTokens.coluna == 0));


            // cenario mais complexo:
            List<string> tokensParaLocalizar = new Tokens(new LinguagemOrquidea(), tokensALocalizar).GetTokens();
            PosicaoECodigo posicaoDeTokensComplexo = new PosicaoECodigo(tokensParaLocalizar, codigoTotal);


            assercao.MsgSucess("objeto posicao de tokens cenario complexo instanciado sem erros fatais.");
            assercao.IsTrue((posicaoDeTokens.linha == 1) && (posicaoDeTokens.coluna == 0));


        }
        private void testeChamadaDeMetodoDeUmaClasseImportada(Assercoes assercao)
        {
            /// template para instrucao "create":
            /// ID ID = create ( ID , ID  ---> tipo nomeObjeto = create (TipoParametro1 parametro1,... )
          
            LinguagemOrquidea linguagem = new LinguagemOrquidea();


            string nomeAssembly = "ParserLinguagemOrquidea.exe";
            string instrucaoImporter = "importer ( " + nomeAssembly + ")";

            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { instrucaoImporter });
            processador.Compile();

            ImportadorDeClasses importador = new ImportadorDeClasses(nomeAssembly);
            importador.ImportAllClassesFromAssembly();

            string nomeOperador = "+";
            string objetoDeClasseImportado = "Matriz m=  create(1,1);";

            string chamadaDeObjeto = "m.GetElement(@" + nomeOperador + ",1,1)";
            

            List<string> codigoObjetoDeClasseImportado = new Tokens(linguagem, new List<string>() { objetoDeClasseImportado }).GetTokens();
            List<string> codigoChamadaDeMetodoImportado = new Tokens(linguagem, new List<string>() { chamadaDeObjeto }).GetTokens();

            List<string> codigoCenarioTeste = new List<string>();
            codigoCenarioTeste.AddRange(codigoObjetoDeClasseImportado);
            codigoCenarioTeste.AddRange(codigoChamadaDeMetodoImportado);

            ProcessadorDeID processadorChamadaDeMetodo = new ProcessadorDeID(codigoCenarioTeste);
            processadorChamadaDeMetodo.Compile();



            assercao.MsgSucess("processamento de chamada de metodo de uma classe importada sem erros fatais.");

            assercao.IsTrue((processadorChamadaDeMetodo.GetInstrucoes() != null) && (processadorChamadaDeMetodo.GetInstrucoes().Count == 2));

        }

        private void TesteCalcExpressaoCenarioSimples(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int a=1; int b=1; int c= a+b+1;" };
            List<string> codigoExpressao = new List<string>() { "a+b+1;" };
            List<string> tokensExpressao = new Tokens(new LinguagemOrquidea(), codigoExpressao).GetTokens();

            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.Compile();
            List<Expressao> expressaoCalc = Expressao.Instance.ExtraiExpressoes(processador.escopo, tokensExpressao);

            expressaoCalc[0] = expressaoCalc[0].PosOrdemExpressao();

            EvalExpression eval = new EvalExpression();
            object result=eval.EvalPosOrdem(expressaoCalc[0], processador.escopo);
            assercao.MsgSucess("calculo expressao sem erros fatais.");
            assercao.IsTrue((int)result == 3);

        }

        private void TesteCalcExpressaoCenarioComplexo(Assercoes assercao)
        {
            string codigoFuncao = "int a=1; int funcaoA(int x){ return x+1;}";
            string expressaoTeste = "a+funcaoA(2)";

            ProcessadorDeID processador = new ProcessadorDeID(new List<string>() { codigoFuncao });
            processador.Compile();

            List<string> tokensExpressao = new Tokens(new LinguagemOrquidea(), new List<string>() { expressaoTeste }).GetTokens();

            List<Expressao> expressaoComplexa = Expressao.Instance.ExtraiExpressoes(processador.escopo, tokensExpressao);
            
            EvalExpression eval = new EvalExpression();
            object result = eval.EvalPosOrdem(expressaoComplexa[0], processador.escopo);

            assercao.MsgSucess("calculo de expressao sem erros fatais.");
            assercao.IsTrue((int)result == 4);
        }


        public CorpoTestesManutencao()
        {
            /// testes para o interpretador do PROLOG.
            Teste testeConsultaPROLOG_teste = new Teste(this.TesteConsultaBackTracking, "teste para consulta prolog com backPropagation.");
            Teste testeObterTokensPROLOG_teste = new Teste(this.TesteGetTokensProlog, "teste para obter tokens de um texto prolog.");
            Teste testeObterPredicadosPROLOG_teste = new Teste(this.TesteGetPredicados, "teste automatizado para obter predicados prolog, a partir de um texto contendo uma regra.");
            Teste testeObterVariaveisPROLOG_teste = new Teste(this.TesteObtencaoEModificacaoDeVariaveisDeUmaRegraProlog, "teste para obtencao de variaveis, e substituicao dessas variaveis em uma regrao prolog,");


            // testes para POO e programacao estruturada.
            Teste testeDefinicaoDeFuncao_teste = new Teste(this.TesteDefinicaoDeFuncao, "teste automatizado para definicao de funcao.");
            Teste testeDefinicaoDeMetodo_teste = new Teste(this.TesteDefinicaoDeMetodo, "teste automatizado para definicao de metodo.");
            Teste testeAtribuicaoDeVariaveisSemDefinicao_teste = new Teste(this.TesteAtribuicaoDePropriedadesSemDefinicao, "teste automatizado de atribuicao de variavel sem definicao.");
            Teste testeInicializacaoDeVariaveisPropriedades_teste = new Teste(this.TesteBuildDefinicaoVariaveisOuPropriedades, "teste automatizado para inicializacao e atribuicao de variaveis e propriedades.");
            Teste testePropriedadesEncadeadas_teste = new Teste(this.TestePropriedadesEncadeadas, "teste automatizado para validacao de propriedades encadeadas.");
            Teste testeBuildInstrucoes_teste = new Teste(this.TesteBuildInstrucoes, "teste para validacao de instrucoes orquidea.");
            Teste testeCompilacaoDeClasse_teste = new Teste(this.TesteInstrucoesEmCompilacao, "teste automatizado para compilacao de uma classe, programacao orientada a objetos.");
            Teste testeParserQualquerLinguagem_teste = new Teste(this.TesteParaParserQualquerLinguagem, "teste automatizado para obter tokens a partir de lista de termos-chave e operadores.");
            Teste testeParaOtimizarMetodoExtraiExpressoes_teste = new Teste(this.TesteParaOtimizacaoDeExtraiExpressoes, "teste para verificar o desempenho do metodo Expressao.ExtraiExpressoes().");
            Teste testeConstrucaoOperadorBinadio_teste = new Teste(this.testeBuildOperadorBinario, "teste automatizado para construcao de um operador binario, para validar a construcao de um operadorXml.");
            Teste testeInstrucaoImporter_teste = new Teste(this.testeInstrucaoImporter, "teste automatizado para instrução importer.");
            Teste testeParaExtracaoDeExpressoes_teste = new Teste(this.testeExtracaoDeExpressoes, "teste automatizado para extrair expressoes de um codigo.");
            Teste testeTokensPolemicos_teste = new Teste(this.TesteTokensPolemicos, "teste para verificacao de nao divisão de tokens polêmicos.");
            Teste testeParaONovoMetodoDeResumoDeEsxpressoes_teste = new Teste(this.testeVerificacaoResumeCorretoDeExpressoes, "teste automatizado, para validar o novo método de extracoes de instrucoes de id.");


            // classes utilizadas no compilador.
            Teste testeParaComponenteDeAuxilio_teste = new Teste(this.testePosicaoECodigoClasse, "teste automatizado para validar codigo entre o codigo total.");
            
            // chamada de métodos POO.
            Teste testeChamadaDeMetodo_teste = new Teste(this.testeChamadaDeMetodo, "teste automatizado para validar chamada de metodo.");
            Teste testeChamadaDeMetodoDeClasseImportada_teste = new Teste(this.testeChamadaDeMetodoDeUmaClasseImportada, "teste automatizado para validar chamada de metodo de uma classe importada");
            Teste testeCalcExpressaoCenarioSimples_teste = new Teste(this.TesteCalcExpressaoCenarioSimples, "teste para calculo de expressao via codigo.");
            Teste testeCalcExpressaoCenarioComplexo_teste = new Teste(this.TesteCalcExpressaoCenarioComplexo, "teste automatizado, para validar a avaliação de uma expressao complexa.");


            // testes para funcionalidades XML.
            Teste testePropriedadesEmArquivoXml_teste = new Teste(this.TesteSalvarCarregarPropriedadesEmArquivoXML, "teste para leitura e gravacao de uma propriedade (variavel) em arquivo xml.");
            Teste testeExpressoesEmArquivoXml_teste = new Teste(this.TesteLeituraGravacaoExpressoesXML, "teste automatizado para validacao de gravar/carregar lista de expressoes.");
            Teste testeInstrucoesEmArquivoXml_teste = new Teste(this.TesteSalvarCarregarInstrucoesEmArquivoXML, "teste automatizado para validacao de gravar/carregar instrucao orquidea.");
            Teste testeFuncaoEmArquivoXml_teste = new Teste(this.TesteSalvarCarregarFuncaoEmArquivoXML, "teste automatizado para gravar/carregar funcao.");
            Teste testeOperadorXml_teste = new Teste(this.testeBuildOperadorXML, "teste automatizado para gravar/carregar operador.");
            Teste testeParaGravacaoObtencaoDeObjetosEmArquivoXML_teste = new Teste(this.TesteObjetosXML, "teste automatizado para validar leitura e gravacao de objetos em arquivo xml.");


            ContainerTestes container = new ContainerTestes(testeCalcExpressaoCenarioComplexo_teste);


            container.ExecutaTestesEExibeResultados();

        }
    }
}
