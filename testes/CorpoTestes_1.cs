using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using parser;
using parser.ProgramacaoOrentadaAObjetos;
using parser.PROLOG;
using ModuloTESTES;
namespace parser
{
    public class CorpoTestes_1
    {

        String LeArquivoCodigo(string nomeArquivo)
        {
            Stream stream = new FileStream(nomeArquivo, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            
            List<string> codigo = new List<string>();
            
            while (!reader.EndOfStream)
                codigo.Add(reader.ReadLine());
            
            
            reader.Close();
            stream.Close();
            string codigoEmUmaLinha = "";
            for (int linha = 0; linha < codigo.Count; linha++)
                codigoEmUmaLinha += codigo[linha];
            return codigoEmUmaLinha;
        } // LeArquivoCodigo()

        private List<string> GetProgramaExemplo0()
        {
            List<string> programa = new List<string>()
            {
                "int k=k1+1"
            };
           
            return programa;
        } // GetProgramaExemplo0()

        private List<string> GetProgramaExemplo1()
        {
            List<string> programa = new List<string>
            {
                "for (a=1;a<10;a++)",
                "{",
                "int k=1;",
                "}"
            };
            return programa;
        } // GetProgramaExemplo()

        public List<string> GetProgramaExemplo2()
        {
            List<string> programa = new List<string>()
            {
                "int funcaoA(int x, int y)",
                "{",
                "int k1=0",
                "}",
                "int funcaoB(int i)",
                "{",
                "int k2-=0",
                "}"
            };
            return programa;
        } // GetProgramaExemplo2()
        private string GetExpressaoExemplo1()
        {
            
            string expressao = "A+B";
            return expressao;
        } // ExpressaoExemplo()
        private List<string> GetProgramaExemploExpressao1()
        {
            List<string> programa = new List<string>()
            {
                "int k=A+B"
            }; //programa
            return programa;
        } // GetProgramaExemploExpressao()
        private string GetExpressaoExemplo2()
        {
            string expressao = "A+B*C/D";
            return expressao;
        }// GetExpressaoExemplo2()

        private List<string> GetProgramaExemploExpressao2()
        {
            List<string> programa = new List<string>()
            {
                "int k1= A+B*C/D"
            };
            return programa;
        } // GetProgramaExemplo3()

        private List<string> GetProgramaExemploExpressao3()
        {
            List<string> programa = new List<string>()
            {
                "int k1= (A+B)*B"
            };
            return programa;
        } // GetProgramaExemploExpressao3()

        private string GetExpressaoExemplo3()
        {
            string expressao = "(A+B)*(C/D)+F";
            return expressao;
        }//GetExpressaoExemplo3()

        private List<string> GetProgramaExemplo4()
        {
            List<string> programa = new List<string>()
            {
                "int funcaoA(int x){",
                "k=A+B}"
            };
            return programa;
        } // GetProgramaExemplo()
        private string GetExpressaoExemplo4()
        {
            string expressao = "A+B";
            return expressao;
        } // GetExpressaoExemplo4()

      
     
        private void TesteConversaoDeTextoEmPredicadoProlog(Assercoes assercao)
        {
            try
            {
                string strPredicadoTexto = "mapa(paraguacu,americaLatina)";

                if (Predicado.AdicionaUmPredicado(strPredicadoTexto)) 
                    assercao.MsgSucess("Teste do Interpretador Prolog, escrita em texto e convertida em predicdo, testado com sucsso positivo!");
                else
                    assercao.MsgFail("Teste do Interpetador Prolog, inserção de um texto gerando um predicado, testado com sucesso negativo.");
                if (BaseDeConhecimento.Instance().Base[0].GetAtomos().Count != 2)
                    assercao.MsgFail("Teste de Insercao de predicado a partir de um texto, falhou!");
            } // try
            catch (Exception ex)
            {
                assercao.MsgFail("Teste do Interpetador Prolog, inserção de um texto gerando um predicado, testado com sucesso negativo. Mensagem de erro: "+ex);
            } // catch
        } // TesteConversaoDeTextoEmPredicadoProlog()


        private void TesteConversaoDeTextoParaRegraProlog(Assercoes assercao)
        {
            string strRegraTexto = "homem(X):-pai(X,Y),pai(Y,Z).";
            if (Regra.AdicionaUmaRegra(strRegraTexto))
                assercao.MsgSucess("Teste de Inserção de uma regra a partir de um texto, bem sucedido.");
            else
                assercao.MsgFail("Teste de Inserção de Regra na Base de Conhecimento, falhou!");
             if (BaseDeConhecimento.Instance().Base[0].Nome.Equals("homem"))
                assercao.MsgSucess("Inserção de Regra na base de conhecimento bem sucedido.");
            else
                assercao.MsgFail("Inserção de Regra: [homem] falhou.");
            System.Console.WriteLine((Regra)BaseDeConhecimento.Instance().Base[0]);
        } // TesteConversaoDeTextoParaRegraProlog()


        private void TesteConsultaPrologComUmFato(Assercoes assercao)
        {
            Predicado.AdicionaPredicados("homem(joao)", "homem(joaquim)", "homem(jose)", "homem(manoel)");
            
            Consultas umaConsulta = Consultas.Instance();
            List<Predicado> predicados = umaConsulta.ConsultaBackTracking(Regra.GetRegra("homem(X)"));
            if ((predicados != null) && (predicados.Count > 0))
            {
                foreach (Predicado p in predicados)
                    System.Console.WriteLine(p.GetAtomos()[0]);
            } // if 
            assercao.MsgSucess("Consulta feita com sucesso.");
        } // TesteConsultaPrologComUmFato()

        private void TesteConsultaPrologComUmaRegra(Assercoes assercao)
        {
            Predicado.AdicionaPredicados(
                "homem(joao)", "homem(joaquim)", "homem(jose)", "homem(manoel)",
                " racional(golfinho)", "racional(lula)", "racional(manoel)");

            Regra umaRegra = Regra.GetRegra("mortal(X):- homem(X), racional(X)");
            List<Predicado> lstPredicadosSolucao = Consultas.Instance().ConsultaBackTracking(umaRegra);
            if (lstPredicadosSolucao != null)
            {
                System.Console.WriteLine(umaRegra);
                for (int umaVariavel = 0; umaVariavel < Consultas.Instance().variaveis.Count; umaVariavel++)
                    System.Console.WriteLine(Consultas.Instance().variaveis[umaVariavel]);
            } // if predicados<>null
            if (lstPredicadosSolucao.Count == 2)
                assercao.MsgSucess("Consulta feita com sucesso.");
            else
                assercao.MsgFail("Falha na consulta de uma regra.");
        } // TesteConsultaPrologComUmFato()

        private void TesteComValidacaoDeVariaveis(Assercoes assercao)
        {
            Predicado p1 = new Predicado("homem(joao");
            Predicado p2 = new Predicado("homem(joaquim)");
            Predicado p3 = new Predicado("homem(jose)");
            Predicado p4 = new Predicado("homem(manoel)");
            Predicado p1_1 = new Predicado("racional(golfinho)");
            Predicado p2_1 = new Predicado("racional(lula)");
            Predicado p2_2 = new Predicado("racional(manoel)");
            Predicado p3_1 = new Predicado("mamifero(golfinho)");
            Predicado p3_2 = new Predicado("mamifero(manoel)");
            Regra umaRegra = Regra.GetRegra("mortal(X):- homem(X), racional(X), mamifero(X)");

            List<Predicado> lstSolucao1 = new List<Predicado>() { p4, p2_2, p3_2 };
            bool bResult = Consultas.ValidaVariaveis(umaRegra, lstSolucao1);
            if (bResult)
                assercao.MsgSucess("Teste com variáveis iguais realizado com sucesso.");
            else
                assercao.MsgFail("Teste com variáveis iguais realizado com falhas.");
            List<Predicado> lstSolucao2 = new List<Predicado>() { p1, p2_2, p3_2 };
            bool bResult2 = Consultas.ValidaVariaveis(umaRegra, lstSolucao2);
            if (!bResult2)
                assercao.MsgSucess("Teste com variáveis diferentes realizado com sucesso.");
            else
                assercao.MsgFail("Teste com variáveis diferentes realizado com falhas.");

        } // TesteComValidacaoDeVariaveis()

        private void TesteComValidacaoDeVariaveis2(Assercoes assercao)
        {
            Predicado p1 = new Predicado("homem(joao, pai");
            Predicado p2 = new Predicado("homem(joaquim, filho)");
            Predicado p3 = new Predicado("homem(jose, filho)");
            Predicado p4 = new Predicado("homem(manoel, avo)");
            Predicado p1_1 = new Predicado("racional(golfinho, mamifero)");
            Predicado p2_1 = new Predicado("racional(lula, artoprode)");
            Predicado p2_2 = new Predicado("racional(manoel, avo)");
            Predicado p3_1 = new Predicado("mamifero(golfinho, mamifero)");
            Predicado p3_2 = new Predicado("mamifero(manoel, avo)");
            Regra umaRegra = Regra.GetRegra("mortal(X,Y):- homem(X,Y), racional(X,Y), mamifero(X,Y)");
            System.Console.WriteLine(umaRegra);
            List<Predicado> lstSolucao1 = new List<Predicado>() { p4, p2_2, p3_2 };
            bool bResult = Consultas.ValidaVariaveis(umaRegra, lstSolucao1);
            if (bResult)
                assercao.MsgSucess("Teste com variáveis iguais realizado com sucesso.");
            else
                assercao.MsgFail("Teste com variáveis iguais realizado com falhas.");
            List<Predicado> lstSolucao2 = new List<Predicado>() { p1, p2_2, p3_2 };
            bool bResult2 = Consultas.ValidaVariaveis(umaRegra, lstSolucao2);
            if (!bResult2)
                assercao.MsgSucess("Teste com variáveis diferentes realizado com sucesso.");
            else
                assercao.MsgFail("Teste com variáveis diferentes realizado com falhas.");

        } // TesteComValidacaoDeVariaveis()

        private void TesteConsultaPrologComUmaRegra2(Assercoes assercao)
        {

            Predicado.AdicionaPredicados(
                "homem(joao)", "homem(joaquim)", "homem(jose)", "homem(manoel)",
                " racional(golfinho)", "racional(lula)", "racional(manoel)",
                "mamifero(golfinho)", "mamifero(manoel)");
            Regra umaRegra = Regra.GetRegra("mortal(X):- homem(X), racional(X), mamifero(X)");
            Predicado predicadoSolucao = new Predicado();

            List<Predicado> lstPredicadosSolucao =  Consultas.Instance().ConsultaBackTracking(umaRegra);
            System.Console.WriteLine(umaRegra.ToString());
            if (lstPredicadosSolucao.Count == 1)
                assercao.MsgSucess("Consulta feita com sucesso.");
            else
                assercao.MsgFail("Falha na consulta de regras a simplificar.");
        } // TesteConsultaPrologComUmFato()

        private void TesteCriacaoDeRegraComLista(Assercoes assercao)
        {
            ListaProlog listaUm = new ListaProlog("umaLista([X|Y], 0,B)");
            ListaProlog listDois = new ListaProlog("umaLista([Z|W],B,0)");
            ListaProlog listaTres = new ListaProlog("umalista[1 2 3],0,B");

            Predicado predicadoMeta1 = Predicado.GetPredicado("(umaLista(X,Y)");

            Regra umaRegra = Regra.GetRegra("umaLista([X|Y], 0,B):-umaLista(X,Y)");
            if (umaRegra.PredicadosGoal.Count == 1)
                assercao.MsgSucess("Regra construida com sucesso positivo.");
        } // TesteCriacaoDeRegraComLista()

        private void TesteCriacaoDeRegraComListaDupla(Assercoes assercao)
        {
            ListaProlog listaUm = new ListaProlog("umaLista([X|Y], 0,B)");
            ListaProlog listDois = new ListaProlog("umaLista([Z|W],B,0)");
            Predicado predicadoMeta1 = Predicado.GetPredicado("(umaLista(X,Y)");

            Regra umaRegra = Regra.GetRegra("umaLista([X|Y], [Z|W],0,B):-umaLista(X,Y,W)");
            if (umaRegra.PredicadosGoal.Count == 1)
                assercao.MsgSucess("Regra construida com sucesso positivo.");
        } // TesteCriacaoDeRegraComListaDupla()

        private void TesteCriacaoDeRegraComListaDuplaEUmElementoEmListas(Assercoes assercao)
        {

            Regra umaRegra = Regra.GetRegra("umaLista([a X|Y], [b Z|W],0,B):-umaLista(X,Y,W)");
            if (umaRegra.PredicadosGoal.Count == 1)
                assercao.MsgSucess("Regra construida com sucesso positivo.");
        } // TesteCriacaoDeRegraComListaDupla()

        private void TesteAPlicacaoFuncaoListaAppend(Assercoes assercao)
        {
            ListaProlog umaListaFuncao = new ListaProlog("Homem([X|Y]),Y|X");
            ListaProlog umaListaVariavel = new ListaProlog("[1 3 5]");
            ListaProlog lstResult = ListaProlog.AplicaUmaListaFuncao(umaListaFuncao, umaListaVariavel);
            System.Console.WriteLine("Lista REsultado Append: " + lstResult.ToString());
            if (lstResult.GetAllElements()[0] == "3")
                assercao.MsgSucess("append feita com sucesso positivo.");
        } // TesteAPlicacaoFuncaoListaAppend()


        private void TesteExecucaoProgramaRecursivoProlog(Assercoes assercao)
        {
            // uma lista de entrada para ser processado no algoritmo recursivo.
            ListaProlog umaListaVariavel = new ListaProlog("homem([1 3 5],1)");

           
            // gera uma regra para o algoritmo processar.
            Regra umaRegraLista = Regra.GetRegra("homem([X|Y],X)");
            Consultas umaConsulta = Consultas.Instance();
            ListaProlog lstResult = umaConsulta.ProgramaParaListas(umaRegraLista, umaListaVariavel, 0);
            // escreve na tela a lista resultante do programa.
            System.Console.WriteLine(lstResult);
            if (lstResult.GetAllElements().Count == 1)
                assercao.MsgSucess("Método de algoritmos prolog executado com sucesso.");
            else
                assercao.MsgFail("Métodode algoritmos executado com falha.");
        } // TesteExecucaoProgramaRecursivoProlog()

        private void TesteExecucaoProgramaRecursivoProlog2(Assercoes assercao)
        {
            // uma lista de entrada para ser processado no algoritmo recursivo.
            ListaProlog umaListaVariavel = new ListaProlog("homem([1 3 5],1)");

            // gera uma regra para o algoritmo processar.
            Regra umaRegraLista = Regra.GetRegra("homem([X|Y],Y|X)");
            Consultas umaConsulta = Consultas.Instance();
            ListaProlog lstResult = umaConsulta.ProgramaParaListas(umaRegraLista, umaListaVariavel, 0);
            // escreve na tela a lista resultante do programa.
            System.Console.WriteLine(lstResult);
            if (lstResult.GetAllElements().Count == 3)
                assercao.MsgSucess("Método de algoritmos prolog executado com sucesso.");
        } // TesteExecucaoProgramaRecursivoProlog2()

        private void TesteExecucaoDeComandoProlog(Assercoes assercao)
        {
            Predicado.AdicionaPredicados("homem(joao)", "homem(joaquim)", "homem(jose)", "homem(manoel)");
            Regra umaRegra = Regra.GetRegra("homem(Y):-homem(manoel), tell(teste.txt, homem(X))");
            List<Predicado> lstResultados = Consultas.Instance().ConsultaBackTracking(umaRegra);
            if (lstResultados.Count == 1)
                assercao.MsgSucess("Consulta feita com sucesso.");
            else
                assercao.MsgFail("Falha na consulta e gravação de predicados");
        } // TesteExecucaoDeComandoProlog()

        private void TesteVerificacaoDeFuncaoListaGetHead(Assercoes assercoes)
        {
            string textoLista1 = "nomeLista1(" + "[X|Y]" + ",X)";
            ListaProlog funcaolista = new ListaProlog(textoLista1);
            string textoLista2 = "nomeLista1([1,3,4]) ";
            
            ListaProlog listavariavel = new ListaProlog(textoLista2);

            ListaProlog listaResultdao = ListaProlog.AplicaUmaListaFuncao(funcaolista, listavariavel);
            Console.WriteLine("Resultado da Função de lista:  " + listaResultdao.ToString());
            if (listaResultdao.GetAllElements()[0] == "1")
                assercoes.MsgSucess("Teste feita com sucesso.");
        }  // TesteVerificacaoDeFuncaoLista()

        private void TesteVerificacaoDeFuncaoListaGetTail(Assercoes assercoes)
        {
            string textoListaFuncao = "nomeLista1(" + "[X|Y]" + ",Y)";
            ListaProlog funcaolista = new ListaProlog(textoListaFuncao);
            string textoListaVariavel = "nomeLista1([1,3,4]) ";
           
            ListaProlog listavariavel = new ListaProlog(textoListaVariavel);

            ListaProlog listaResultdao = ListaProlog.AplicaUmaListaFuncao(funcaolista, listavariavel);
            
            Console.WriteLine("Resultado da Função de lista:  " + listaResultdao.ToString());
            if (listaResultdao.GetAllElements()[0] == "3")
                assercoes.MsgSucess("Teste feita com sucesso.");
        }  // TesteVerificacaoDeFuncaoLista()

        private void TestesListasProlog(Assercoes assercao)
        {
            ListaProlog lst1 = new ListaProlog("helllo,World!");

            lst1.ConstroiLista("lst1", "2", "3", "5", "6", "4", "2", "8");
            List<string> elementoHead = lst1.GetAllElements();
            if ((elementoHead == null) || (elementoHead.Count == 0))
                assercao.MsgFail("falha na procura do primeiro elemento.");
            else
            {
                assercao.MsgSucess("Experimento com listas Parte 1 bem sucedido.");
                System.Console.WriteLine(elementoHead[0]);
            } // else

            List<string> elementoTail = lst1.GetAllElements();
            elementoTail.RemoveAt(0);
            if ((elementoTail == null) || (elementoTail.Count == 0))
                assercao.MsgFail("falha na procura dos elementos da cauda da lista.");
            else
            {
                assercao.MsgSucess("Experimento com listas Parte 2 bem sucedido.");
                for (int x = 0; x < elementoTail.Count; x++)
                {
                    System.Console.WriteLine(elementoTail[x]);
                } // for x
            } // else
            if (elementoTail.Count == 6)
                assercao.MsgSucess("Teste com resultado positivo.");
            else
                assercao.MsgFail("Falha com sucesso negativo no resultado dos testes.");
        }//TestesListasProlog()


        private void TesteEspecificacoesListaProlog(Assercoes assercao)
        {
            string EspecificacaoDaLista = "[X|Y]";
            ListaProlog listaTeste = new ListaProlog(EspecificacaoDaLista);

            foreach (KeyValuePair<string, string> umElemento in listaTeste.PartesListaVariaveis[0]) 
            {
                System.Console.WriteLine("elemento {0}: {1}", umElemento.Key, umElemento.Value);
            } // foreach
            if ((listaTeste.PartesListaVariaveis[0]["X"].Contains("Head")) && (listaTeste.PartesListaVariaveis[0]["Y"].Contains("Tail")))
                assercao.MsgSucess("Teste de especificações de listas prolog bem sucedido!");
            else
                assercao.MsgFail("Teste de especificações de lista prolog sucesso negativo.");

            string EspecificacaoDaListaParte2 = "[X Y Z K|G]";
            ListaProlog listaTeste2 = new ListaProlog(EspecificacaoDaListaParte2);

            foreach (KeyValuePair<string, string> umElemento in listaTeste2.PartesListaVariaveis[0])
            {
                System.Console.WriteLine("elemento {0}: {1}", umElemento.Key, umElemento.Value);
            } // foreach

            if ((listaTeste2.PartesListaVariaveis[0]["X"].Contains("Head")) &&
                (listaTeste2.PartesListaVariaveis[0]["Elemento#0"] == "X"))
                assercao.MsgSucess("Teste de especificacoes de listas prolog bem sucedido!");
            else
                assercao.MsgSucess("Teste de especificações de listas prolog com sucesso negativo.");
        } // TesteEspecificacoesListaProlog()

        private void TesteExtracaoPredicadoDeUmTextoProlog(Assercoes assercao)
        {
            string umPredicadoComplexo = "tell(teste.txt, homem(X))";
            Predicado predicadoComplexo = Predicado.GetPredicado(umPredicadoComplexo);
            assercao.MsgSucess("Criação de predicado feita com sucesso.");
        } // TesteExtracaoPredicadoDeUmTextoProlog()

        private void TesteVariaveisAnonima(Assercoes assercao)
        {
            Predicado.AdicionaPredicados(
              "homem(joao)", "homem(joaquim)", "homem(jose)", "homem(manoel)",
              " racional(golfinho)", "racional(lula)", "racional(manoel)",
              "mamifero(golfinho)", "mamifero(manoel)");
            Predicado predicadoConsulta = Predicado.SetPredicado("homem", "_");


            Regra regra = new Regra();
            regra.PredicadoBase = predicadoConsulta;


            List<Predicado> lstResultado = Consultas.Instance().ConsultaBackTracking(regra);
            
            
            
            foreach (Predicado predicado in lstResultado)
            {
                System.Console.WriteLine(predicado);
            } //foreach
            if (lstResultado.Count == 4)
                assercao.MsgSucess("Teste com variáveis anônimas bem sucedidio.");
            else
                assercao.MsgSucess("Teste com variáveis anônimas com falhas.");
        } // TesteVariaveisAnonima()

        private void TesteConsultaSimplesListas(Assercoes assercao)
        {
            string defList = "umaLista([X|Y],0,0)";
            string listaAConsultar = "umaLista([1],0,0)";
            ListaProlog predicadoaConsultar = new ListaProlog(listaAConsultar);
            BaseDeConhecimento.Instance().Base.Add(new ListaProlog(defList));
            Consultas umaConsulta = Consultas.Instance();
            List<Predicado> predicadosResultado = umaConsulta.ConsultaLista(predicadoaConsultar);
            if (predicadosResultado.Count == 1)
                assercao.MsgSucess("Teste com consulta por linhas resultou em positivo.");
        } // TesteConsultaSimplesListas()
        private void TesteGetTokensDeUmTrechoDeCodigo(Assercoes assercao)
        {
            List<string> trechoDeCodigo = GetProgramaExemplo1();
            List<string> todosTokens = new Tokens(LinguagemOrquidea.Instance(), trechoDeCodigo).GetTokens();

            assercao.MsgSucess("Obtenção de tokens pela classe Tokens sem erros fatais.");
            if (todosTokens.Count == 20)
                assercao.MsgSucess("Obtenção de tokens extraiu todos tokens corretamente.");

        } // TesteGetTokensDeUmTrechoDeCodigo()


        private void TesteObterProducoesPelaClasseTokens(Assercoes assercao)
        {
            List<string> programa = GetProgramaExemplo1();
            UmaGramaticaComputacional linguagem = LinguagemOrquidea.Instance();
            Tokens tokensProducao = new Tokens(linguagem, programa);

            List<producao> producoes = new List<producao>();
            tokensProducao.GetProducoes(tokensProducao.GetTokens(), new Escopo(programa));
            for (int p = 0; p < producoes.Count; p++)
                System.Console.WriteLine(producoes[p].ToString());
            assercao.MsgSucess("Método de obter produções bem sucedido.");
        } // TesteEscolhaProducoesPorPontuacao()



        private void TesteAvaliaExpressao(Assercoes assercao)
        {
            // expessao infixa: (a-b)*c, expressão posfixa: ab-c*
            string expr1 = "-a";
            string expr2 = "(a-b)*c";
            string expr3 = "a*b+c";
            string expr3Esperada = "a b * c +";
            string expr2Esperada = "a b - c *";
            string expr1Esperada = "a -";
            FuncaoTesteExpressoes(assercao, expr3, expr3Esperada, new List<string>() {expr1 });
            FuncaoTesteExpressoes(assercao, expr2, expr2Esperada, new List<string>() { expr2 });
            FuncaoTesteExpressoes(assercao, expr1, expr1Esperada, new List<string>() { expr3 });
        } // TesteAvaliaExpressao()

        private static void FuncaoTesteExpressoes(Assercoes assercao, string expressaoInfixa, string expressaoPosFixaEsperada, List<string> codigo)
        {
            // constroi os parametros necessários para o parser de Expressão ser inicializado.
            RegistradorBNF regBNF = new RegistradorBNF(new List<string>() { expressaoInfixa });
            Escopo escopoCurrente = new Escopo(codigo);
            List<string> msgErros = new List<string>();

            // constroi um objeto Expressao, para converter expressoes infixa para posfixa.
            Expressao parserExpressao = new Expressao(new string[] { expressaoInfixa }, escopoCurrente);

          
            // chama o método que converte a expressão infixa para posfixa (facilita a avaliação automática pelo computador.
            parserExpressao = parserExpressao.PosOrdemExpressao();
            
            // obtém a expressão original, para comparações.
            string expressaoOriginal = (string)expressaoInfixa.Clone();
            System.Console.WriteLine("Expressao em posfixa:");
            string expressaoGerada = Utils.UneLinhasPrograma(parserExpressao.Convert());
            System.Console.WriteLine(expressaoGerada);
            System.Console.WriteLine();
            System.Console.WriteLine("Expressao original");
            System.Console.WriteLine(expressaoOriginal);

            // registra no relatório se o valor encontrado é igual ao valor esperado.
            assercao.EqualsString(expressaoPosFixaEsperada, expressaoGerada);
            

        } // FuncaTesteExpressoes()

        

        private void TesteObtencaoTokensDeUmPredicado(Assercoes assercao)
        {
            string textoPredicado = "homem(X,Y).";
            List<string> tokens = Predicado.ExtraiDeTextoUmPredicado(textoPredicado);
            for (int umToken = 0; umToken < tokens.Count; umToken++)
            {
                System.Console.Write(tokens[umToken] + " ");
            } // for umToken
            if (tokens.Count == 7)
                assercao.MsgSucess("Obtenção de tokens de um Predicado retirados com sucesso.");
            else
                assercao.MsgFail("Obtenção de tokens de um Predicado com falhas");
        } // TesteObtencaoTokensDeUmPredicado()

        LinguagemOrquidea lng = LinguagemOrquidea.Instance();
       
        
        private void TesteComponentePosicaoDeCodigo(Assercoes assercao)
        {
            List<string> codigo = new List<string>(){ "int funcaoA(int x, int y)", "{",
                "int k1=0",
                "}",
                "int funcaoB(int i)",
                "{",
                "int k2-=0",
                "}" };

            List<string> codigoProcurar = new List<string>() { "funcaoA", "(", "int" };

            PosicaoECodigo posicao = new PosicaoECodigo(codigoProcurar);

            assercao.MsgSucess("Teste de Posicionamento feito sem erros fatais.");
            if (posicao.coluna == 4)
                assercao.MsgSucess("Calculo de Posicionamento com precisão.");
        } // TesteComponentePosicaoDeCodigo()


        private void TestePontuacaoSequenciasID(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int b= a; " };
            List<string> tokens = new Tokens(this.lng, codigo).GetTokens();
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("Processamento de obtencao de indices de metodos realizado sem erros fatais.");

            if (processador.escopo.sequencias[0].indexHandler == 1)
                assercao.MsgSucess("escolha de indice de método tratador calculado exato.");
            else
                assercao.MsgSucess("calculo de metodo tratador calculado com falhas.");
        } // TestePontuacaoSequenciasID()

        private void TestePontuacaoSequenciasIDComVariantes(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int b= a+1; " };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();

            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("Processamento de obtencao de indices variantes de metodos tratadores realizado sem erros fatais.");
            if  (processador.escopo.sequencias[0].indexHandler == 12)
            assercao.MsgSucess("Processamento de obtencao de indices constantes e variantes calculados sucessamente.");
            else
            assercao.MsgFail("Processamento de obtencao de indices constantes e variantes calculados com falha.");
            
        } // TestePontuacaoSequenciasID()

        private void TesteSequenciaID_1(Assercoes assercao)
        {
            List<string> codigo = new List<string>() {" int a; int b= a; "};
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();

            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            
            processador.CompileEmDoisEstagios();
            assercao.MsgSucess("Processamento de sequencias ID realizado sem erros fatais.");
        } // TesteSequenciaID_1()

        private void TesteChamadaFuncaoComExpressoesComoParametros(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { " int a; int b= a;  int funcaoA(int c); funcaoA(a+b);" };

            ProcessadorDeID processador = new ProcessadorDeID(codigo);

            processador.CompileEmDoisEstagios();
            assercao.MsgSucess("Processamento de chamada de função com expressão como parâmetro realizado sem erros fatais.");
        } // TesteSequenciaID_1()


        private void TesteVariasSequenciasID(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int C; int D; int funcaoA(int A,int B ); funcaoA(C,D);" };
            List<string> codigo2 = new List<string>() { "int funcaoA(int A,int B );" };
            ProcessadorDeID processador = new ProcessadorDeID(codigo2);
            
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de escopos feito sem erros fatais.");

            if (processador.escopo.sequencias[0].indexHandler == 2)
                assercao.MsgSucess("calculos de indices de métodos tratadores feitos exatos.");
        } // TesteVariasSequenciasID()


        private void TesteVariasSequenciasID_2(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int C; int D; int funcaoA(int A,int B ); funcaoA(C,D);" };
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
   
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de escopos feito sem erros fatais.");

            if (processador.escopo.tabela.GetObjetos().Count == 2)
                assercao.MsgSucess("construção de escopos com vária sequencias id feita exato.");
        } // void TesteVariasSequenciasID_2()

        private void TesteBuildEscoposDefinicaoDeVariaveisRegistroDeFuncaoEChamadaDeFuncao(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int funcaoA(int B , int C);" };
            ProcessadorDeID processador = new ProcessadorDeID(codigo);

            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de escopos feito sem erros fatais.");
            if (processador.escopo.tabela.GetFuncoes().Count == 1) 
                assercao.MsgSucess("construção de tabelas de valores de escopos calculados sucessamente.");
        } // TesteConstruirEscopos()


        private void TesteBuildEscoposAtribuicaoComExpressaoRegistroDeFuncaoComCorpo(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { " int funcaoA(int A,int B ){int a=1;}" };


            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de escopos feito sem erros fatais.");
            if (processador.escopo.escopoFolhas[0].tabela.GetFuncoes().Count == 1)
                assercao.MsgSucess("construção de tabelas de valores de escopos calculados sucessamente.");
        } // TesteConstruirEscopos()


        private void TestePropriedadeEMetodos(Assercoes assercao)
        {
      
            List<string> codigo1 = new List<string> { "public int funcaoSemParamsComCorpo(){int a=1;}",
                                                     "public int C=1+3;",
                                                      "public int funcaoComParamsSemCorpo(int A);",
                                                      "public int funcaoSemParamsSemCorpo();",
                                                      "public int funcaoComParamsComCorpo(int A,int B ){int a=1;}"};



            ProcessadorDeID processador = new ProcessadorDeID(codigo1);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de escopos feito sem erros fatais.");
            if (processador.escopo.tabela.GetFuncoes().Count == 4)
                assercao.MsgSucess("construção de tabelas de valores de escopos calculados sucessamente.");
        } // TesteConstruirEscopos()


        private void TesteCalculoDeVariasSequenciasID(Assercoes assercao)
        {

            List<string> codigo1 = new List<string> { "public int funcaoSemParamsComCorpo(){int a=1;}",
                                                      "public int C=1+3;",
                                                      "public int funcaoComParamsSemCorpo(int A);",
                                                      "public int funcaoSemParamsSemCorpo();",
                                                      "public int funcaoComParamsComCorpo(int A,int B ){int a=1;}"};

            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo1).GetTokens();

            UmaGramaticaComputacional linguagem = LinguagemOrquidea.Instance();

            ProcessadorDeID processador = new ProcessadorDeID(codigo1);

        
            assercao.MsgSucess("Sequencias ID calculadas sem erros fatais.");
            if (processador.escopo.sequencias.Count == 5) 
                assercao.MsgSucess("Numero de sequencias calculado exatamente.");
        } // TesteConstruirEscopos()


        private void TesteObterProducoes(Assercoes assercao)
        {
            List<string> codigo = new List<string>
            {
                "for (a=1;a<10;a++)",
                "{",
                "int k=1;",
                "}"
            };
            
            List<string> tokens = ParserUniversal.GetTokens(Util.UtilString.UneLinhasLista(codigo));

            List<producao> producoes = new List<producao>();

            new Tokens(LinguagemOrquidea.Instance(), codigo).GetProducoes(tokens, new Escopo(codigo));

            assercao.MsgSucess("Produçõs de tokens e producoes sem erros fatais.");
            if ((producoes.Count == 5) && (producoes[4].GetType() == typeof(UmaSequenciaID)))
                assercao.MsgSucess("Calculo de produções enccontrado com exatidão.");
        } // TesteObterProducoes()

        private void TesteObterProducoesVersaoBeta(Assercoes assercao)
        {
            List<string> codigo = new List<string>
            {
                "for (a=1;a<10;a++)",
                "{",
                "int k=1;",
                "}"
            };

            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            List<string> tokens = ParserUniversal.GetTokens(Util.UtilString.UneLinhasLista(codigo));


            List<producao> producoes = new List<producao>();


            new Tokens(LinguagemOrquidea.Instance(), codigo).GetProducoes(tokens, processador.escopo);

            assercao.MsgSucess("Produçõs de tokens e producoes sem erros fatais.");
            if (producoes.Count == 5)
                assercao.MsgSucess("Calculo de produções enccontrado com exatidão.");
        } // TesteObterProducoes()


        private void TesteObtencaoIndicesMetodosTratadoresParaFuncaoComParametroEComCorpo(Assercoes assercao)
        {
            List<string> codigoEmArquivo = new List<string>() { this.LeArquivoCodigo("codigo1.txt") };
            List<string> codigo1 = new List<string>() { "public int funcaoComParamsSemCorpo(int A);" };
   
            ProcessadorDeID processador = new ProcessadorDeID(codigoEmArquivo);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de combinação de sequencia id feito sem erros fatais.");

            if ((processador.escopo.tabela.GetFuncoes().Count == 4) && (processador.escopo.tabela.GetObjetos().Count == 1))
                assercao.MsgSucess("construção de escopo feito com cálculos exatos.");

        } // TesteObtencaoIndicesMetodosTratadoresParaFuncaoComParametroEComCorpo()

        private void TesteRegistraExpressao(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int d= a+b+c;", "int a=1;", "int b=3;", "int c=4;" };
    
            ProcessadorDeID processador = new ProcessadorDeID(codigo);

            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de código para expressão feito sem erros fatais.");

            if ((processador.escopo.tabela.GetExpressoes().Count == 4) && (processador.escopo.tabela.GetObjetos().Count == 4))
                assercao.MsgSucess("construção de escopo feito com cálculos exatos.");
        } // TesteRegistraExpressao()

        private void TesteRegistraExpressaoComChamadasFuncao(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int funcaoA(int b);", "int a=funcaoA(3)+1;" };
  
            ProcessadorDeID processador = new ProcessadorDeID(codigo);

            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de código para expressão com chamada de função feito sem erros fatais.");

            if ((processador.escopo.tabela.GetFuncoes().Count == 1) && (processador.escopo.tabela.GetObjetos().Count == 1))
                assercao.MsgSucess("construção de escopo feito com cálculos exatos.");

        } // TesteRegistraExpressao()

        private void TesteExtracaoDeChamadasFuncao(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "int funcaoA(int b);", "int a=funcaoA(4)+5+ 6;" };

            ProcessadorDeID processador = new ProcessadorDeID(codigo);

            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construção de código para expressão com chamada de função feito sem erros fatais.");
            if ((processador.escopo.tabela.GetFuncoes().Count == 1) && (processador.escopo.tabela.GetObjetos().Count == 1))
                    assercao.MsgSucess("construção de escopo feito com cálculos exatos.");
        } // TesteRegistraExpressao()

        private void TesteProcessamentoDeClasses(Assercoes assercao)
        {
            // É PRECISO CONSTRUIR O TESTE DO JEITO QUE ELE PASSE, PARA UTILIZAR EM OUTROS MOMENTOS PARA VALIDAR UM CÓDIGO 
            // MODIFICADO, E QUE PODE ESTAR ERRADO, O TESTE CAPTURA ESTE ERRO NO CÓDIGO MODIFICADO. DAÍ A CONSTRUÇÃO FINAL
            // DA AVERIGUAÇÃO OU PREVISÃO DO QUE SE ESPERAR APÓS OS TESTES.

            List<string> codigo = new List<string>() { "class classeA { public int a; public int funcaoB(int x); }" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
            Escopo escopo = new Escopo(codigo);
            ExtratoresOO extratores = new ExtratoresOO(escopo, LinguagemOrquidea.Instance(), tokens);
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
            assercao.MsgSucess("extração de codigo de uma classe feito sem exceções fatais.");
            if ((umaClasse != null) && (umaClasse.GetPropriedades().Count == 1) && (umaClasse.GetMetodos().Count == 1))
                assercao.MsgSucess("extração de propriedades e metodos com exatidão.");

        } // TesteProcessamentoDeClasses()

        private void TesteProcessamentoDeClassesComHeranca(Assercoes assercao)
        {
            // É PRECISO CONSTRUIR O TESTE DO JEITO QUE ELE PASSE, PARA UTILIZAR EM OUTROS MOMENTOS PARA VALIDAR UM CÓDIGO 
            // MODIFICADO, E QUE PODE ESTAR ERRADO, O TESTE CAPTURA ESTE ERRO NO CÓDIGO MODIFICADO. DAÍ A CONSTRUÇÃO FINAL
            // DA AVERIGUAÇÃO OU PREVISÃO DO QUE SE ESPERAR APÓS OS TESTES.

            List<string> codigo =
                new List<string>() { "class classeA { public int a; public int funcaoA(int x); }", "class B: +classeA {public int c;}" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();
            Escopo escopo = new Escopo(codigo);
            ExtratoresOO extratores = new ExtratoresOO(escopo, LinguagemOrquidea.Instance(), tokens);
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
           

            assercao.MsgSucess("extração de codigo de uma classe feito sem exceções fatais.");

            if ((umaClasse != null) &&
                (umaClasse.GetPropriedades().Count == 1) &&
                (umaClasse.GetMetodos().Count == 1) &&
                (umaClasse.GetPropriedades().Count == 2) &&
                (umaClasse.GetMetodos().Count == 1))
                assercao.MsgSucess("extração de propriedades e metodos com exatidão.");

        } // TesteProcessamentoDeClasses()

        private void TesteSaveLoadClasse(Assercoes assercao)
        {

            List<string> codigo = new List<string>() { "class classeA { public int a; public int funcaoB(int x); }" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();

            Escopo escopo = new Escopo(codigo);
            ExtratoresOO extratores = new ExtratoresOO(escopo, LinguagemOrquidea.Instance(), tokens);
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);
            if (umaClasse == null) 
            {
                assercao.MsgSucess("Extração de Classes do código falha.");
                return;
            } // if
            // salva a classe em um artigo.
            umaClasse.Save(umaClasse);
            // carrega a classe a partir de um arquivo.
            Classe classeRetornoDoArquivo = Classe.Load("classeA");

            assercao.MsgSucess("Gravação/Leitura de uma classe a partir de um arquivo feito sem exceções fatais.");
            if ((classeRetornoDoArquivo.GetPropriedades().Count == 1) && (classeRetornoDoArquivo.GetMetodos().Count == 1))
                assercao.MsgSucess("métodos Save() e Load() funcionando adequadamente.");
        } // TesteProcessamentoDeClasses()

        private void TesteValidacaoInterfaceNaoImplementado(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "interface IComparer {int funaoE();} class classeA: + IComparer { public int a; public int funcaoB(int x); }" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();

            Escopo escopo = new Escopo(codigo);
            ExtratoresOO extratores = new ExtratoresOO(escopo, LinguagemOrquidea.Instance(), tokens);
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_INTERFACE);

            assercao.MsgSucess("construção e validação de interfaces sem erros fatais.");

            if (extratores.MsgErros.Count == 1)
                assercao.MsgSucess("validação de interface implementada com exatidão.");
        } // TesteValidacaoInterface()

        private void TesteValidacaoInterfaceImplementado(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "interface IComparer {int funcaoE();} class classeA: + IComparer { public int a; public int funcaoB(int x); int funcaoE(); }" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();

            Escopo escopo = new Escopo(codigo);
            ExtratoresOO extratores = new ExtratoresOO(escopo, LinguagemOrquidea.Instance(), tokens);
            Classe umaClasse = extratores.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);

            assercao.MsgSucess("construção e validação de interfaces sem erros fatais.");
            if (extratores.MsgErros.Count == 0)
                assercao.MsgSucess("Não  implementação de interface a uma classe feita com exatidão.");
        } // TesteValidacaoInterface(

        private void TesteDefinicaoDeUmOperadorBinario(Assercoes assercao)
        {
            List<string> codigoOperador = new List<string>() { "operador int DOT ( int a, int b ) prioridade 1;" };
          
            ProcessadorDeID processador = new ProcessadorDeID(codigoOperador);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("processamento de sequencia de definição de um operador binário feita sem erros fatais.");
            if (processador.escopo.tabela.GetClasses().Count == 1)
                assercao.MsgSucess("processamento de definição de um operador binário feita com exatidão.");
        } // TesteDefinicaoDeUmOperador()

        private void TesteDefinicaoDeUmOperadorUnario(Assercoes assercao)
        {
            List<string> codigoOperador = new List<string>() { "operador int DOT ( int a ) prioridade 1;" };
   
            ProcessadorDeID processador = new ProcessadorDeID(codigoOperador);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("processamento de sequencia de definição de um operador unário feita sem erros fatais.");
            if (processador.escopo.tabela.GetClasses().Count == 1)
                assercao.MsgSucess("processamento de definição de um operador unário feita com exatidão.");
        } // TesteDefinicaoDeUmOperador()

        private void TesteAtribuicaoPropriedade(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "class C{int propriedadeC;} class B {C propriedadeB;} class A {B propriedadeA; } class D { int D= A.propriedadeA.propriedadeB;} " };
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();
            
           
            assercao.MsgSucess("processamento de atribuição de propriedades feita sem erros fatais.");

            if (processador.escopo.tabela.GetClasses().Count == 4)
                assercao.MsgSucess("processamento de atribuiçao de propriedades feita com exatidão.");

        } // TesteAtribuicaoPropriedade()

        private void TesteExpressaoPropriedade(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "class C {int propriedadeC;} class B {C propriedadeB;} class A {B propriedadeA; } class D { int a= A.propriedadeA.propriedadeB;} " };
      
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();


            assercao.MsgSucess("processamento de expressao de propriedades feita sem erros fatais.");
            if (processador.escopo.tabela.GetClasses().Count == 4)
                assercao.MsgSucess("processamento de expressão de propriedades feitas com exatidão.");


        } // TesteAtribuicaoPropriedade()

        private void TesteExpressaoMetodo(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "class C {int propriedadeC;} class B {C propriedadeB();} class A {B propriedadeA; } class D { int a= A.propriedadeA.propriedadeB();} " };
  
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();


            assercao.MsgSucess("processamento de expressao de propriedades feita sem erros fatais.");

        } // TesteAtribuicaoPropriedade()


        private void TesteAtribuicaoDePropriedadesComMetodos(Assercoes assercao)
        {
            List<string> codigo = new List<string>() { "class C {int propriedadeC; int funcaoC(); int propriedadeD= propriedadeC+ funcaoC();}" };
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), codigo).GetTokens();


            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();


            assercao.MsgSucess("processamento de expressao de propriedades feita sem erros fatais.");

            if ((processador.escopo.escopoFolhas[0].tabela.GetObjetos().Count == 2) &&
                (processador.escopo.escopoFolhas[0].tabela.GetExpressoes().Count == 1) &&
                (processador.escopo.escopoFolhas[0].tabela.GetFuncoes().Count == 1))
                assercao.MsgSucess("processamento de expressão de propriedades feita com exatidão.");

        } // TesteAtribuicaoPropriedade()

   

        private void TesteClassificacaoDeOperadores(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            if (linguagem.IsOperadorBinario("+"))
                assercao.MsgSucess("operador: + é binário.");
            if (linguagem.IsOperadorUnario("++"))
                assercao.MsgSucess("operador: ++ é unário.");
            if (linguagem.IsOperadorCondicional(">"))
                assercao.MsgSucess("operador: > é condicional");

        } // TesteClassificacaoDeOperadores(


        private void TesteGetTokensPolemicos(Assercoes assercao)
        {


            List<string> codigo5 = new List<string>() { "for ( x=0;x< 0;x++)" };
            List<string> tokens5 = new Tokens(LinguagemOrquidea.Instance(), codigo5).GetTokens();

            if (tokens5.Count == 13)
                assercao.MsgSucess("obtencao de tokens extaidos, com exatidao.");


            List<string> trechoDeCodigo3 = new List<string>() { "int mfor=0;" }; // o ParserUniversal pode confundir [mfor] com [m for]..
            List<string> todosTokens3 = new Tokens(LinguagemOrquidea.Instance(), trechoDeCodigo3).GetTokens();

            assercao.MsgSucess("Obtenção de tokens pela classe Tokens sem erros fatais.");

            if (todosTokens3.Count == 5)
                assercao.MsgSucess("Obtenção de tokens termos-chave juntos extraiu todos tokens corretamente.");




            List<string> trechoDeCodigo = new List<string>() { "int form=0;" }; // o ParserUniversal pode confundir [form] com [form m].
            List<string> todosTokens = new Tokens(LinguagemOrquidea.Instance(), trechoDeCodigo).GetTokens();

            assercao.MsgSucess("Obtenção de tokens pela classe Tokens sem erros fatais.");
            if (todosTokens.Count == 5)
                assercao.MsgSucess("Obtenção de tokens termos-chave juntos extraiu todos tokens corretamente.");



            List<string> trechoDeCodigo2 = new List<string>() { "k++;" }; // o ParserUniversal pode confundir  [++] com [+ +].
            List<string> todosTokens2 = new Tokens(LinguagemOrquidea.Instance(), trechoDeCodigo2).GetTokens();

            assercao.MsgSucess("Obtenção de tokens pela classe Tokens sem erros fatais.");

            if (todosTokens2.Count == 3)
                assercao.MsgSucess("Obtenção de tokens operadores unidos extraiu todos tokens corretamente.");

            assercao.MsgSucess("determinacao de tokens feito sem erros fatais.");


        } // TesteGetTokensPolemicos()


        public CorpoTestes_1()
        
        
        {
            // teste para obtenção de produções.
            Teste testeAvaliadorDeExpressao = new Teste(TesteAvaliaExpressao, "Teste para avaliação de expressões");
            Teste testeLocalizadorProducoes = new Teste(this.TesteObterProducoesPelaClasseTokens, "localiza producoes atraves da classe LocalizadorDeProducoes");
            Teste testeComponentePosicionamentoCodigo = new Teste(TesteComponentePosicaoDeCodigo, "teste para posicionamento de uma sequencia de tokens dentro do codigo total.");
            Teste testeObterProducoesVersaoBeta_teste = new Teste(TesteObterProducoesVersaoBeta, "teste para o novo método de obter produções.");
            Teste testeClassificacaoOperadores_teste = new Teste(TesteClassificacaoDeOperadores, "teste para verificar classificação de operadores.");

            // testes para obtenção de tokens.
            Teste testeGetTokens_teste = new Teste(TesteGetTokensDeUmTrechoDeCodigo, "teste para obter todos tokens através da classe Tokens.");
            Teste testeGetTokensPolemicos_teste = new Teste(TesteGetTokensPolemicos, "teste para obter tokens de codigo com tokens unidos.");
            
            // Testes para o Interpretador Prolog.
            Teste testePrologInsercaoDePredicadoAPartirDeUmTexto = new Teste(this.TesteConversaoDeTextoEmPredicadoProlog, "Teste de Inserção de Predicado na Base de conhecimento.");
            Teste testePrologInsercaoDeRegraAPartirDeUmTexto = new Teste(this.TesteConversaoDeTextoParaRegraProlog, "Teste Interpretador Prolog de inserção de Regra na Base de Conhecimneto");
            Teste testePrologConsultaSimples = new Teste(this.TesteConsultaPrologComUmFato, "Teste para consulta simples no Interpretador Prolog.");
            Teste testePrologConsultaComplexa = new Teste(this.TesteConsultaPrologComUmaRegra, "Teste para consulta complexa com uma regra.");
            Teste testePrologConsultaComplexa2 = new Teste(this.TesteConsultaPrologComUmaRegra2, "Teste para consulta complexa, com três predicados conexos.");
            Teste testePrologExecutaComando = new Teste(this.TesteExecucaoDeComandoProlog, "Teste de um comando Prolog");
            Teste testeExtracaoPredicadoComplexo = new Teste(this.TesteExtracaoPredicadoDeUmTextoProlog, "extração de um predicado complexo a partir de uma linha de texto.");
            Teste testesComListasProlog = new Teste(this.TestesListasProlog, "testes com listas prolog.");
            Teste testesListasPrologEspecificacoes = new Teste(this.TesteEspecificacoesListaProlog, "Listagem de elementos de uma especificação de listas prolog.");
            Teste testeConsultaListas = new Teste(TesteConsultaSimplesListas, "Teste inicial de consultas por linhas.");
            Teste testeRegraComListaUnica = new Teste(TesteCriacaoDeRegraComLista, "Teste para retirar elementos variáveis de uma lista");
            Teste testeRegraComListaDupla = new Teste(TesteCriacaoDeRegraComListaDupla, "Teste para retirar elementos variáveis de duas listas.");
            Teste testeComElementosComListas = new Teste(TesteCriacaoDeRegraComListaDuplaEUmElementoEmListas, "Teste para duas listas com mais um elemento por lista");
            Teste testeAplicacaoFuncaoLista = new Teste(TesteVerificacaoDeFuncaoListaGetHead, "Teste de funções lista.");
            Teste testeAplicacaoFuncaoListaCauda = new Teste(TesteVerificacaoDeFuncaoListaGetTail, "Teste de funções lista.");
            Teste testeAplicacaoFuncaoListaAppend = new Teste(TesteAPlicacaoFuncaoListaAppend, "Teste de aplicaçãp de função Append");
            Teste testeAlgoritmoRecursivo = new Teste(TesteExecucaoProgramaRecursivoProlog, "teste de algoritmos recursivos para listas.");
            Teste testeAlgorittmoRecursivo2 = new Teste(TesteExecucaoProgramaRecursivoProlog2, "teste de algoritmos recursivos para listas.");
            Teste testeComVariaveisAnonima = new Teste(TesteVariaveisAnonima, "teste de consulta com variáveis anônimas.");
            Teste testeValidacaoVariaveisSolucao = new Teste(TesteComValidacaoDeVariaveis, "teste de validação de variáveis.");
            Teste testeValidacaoVariaveisSolucao2 = new Teste(TesteComValidacaoDeVariaveis2, "teste de validação de variáveis 2");
            Teste testeObtencaoTokensPredicado = new Teste(TesteObtencaoTokensDeUmPredicado, "teste de obtenção de tokens de Predicado Prolog.");

            // testes para POO requisitos.
            Teste testeExtracaoSequenciasID_1 = new Teste(TesteSequenciaID_1, "teste de obtenção de sequencias de ID");
            Teste testeGetPontuacaoSequenciasID = new Teste(TestePontuacaoSequenciasID, "teste de obter indices de metodos tratadores de sequencias ID");
            Teste testeGetPontuacaoSequenciasIDComVariante = new Teste(TestePontuacaoSequenciasIDComVariantes, "teste de sequencia com variantes");
         
            // teste para ProgramacaoEStruturada
            Teste testeBuildUmEscopo = new Teste(TesteVariasSequenciasID, "Teste de construção de um escopo de um codigo.");
            Teste testeBuildUmEscopo_2 = new Teste(TesteVariasSequenciasID_2, "Teste de construção de um escopo, de vária sequencias de ID.");
            Teste testeChamadaFuncaoComExpressoes = new Teste(TesteChamadaFuncaoComExpressoesComoParametros, "Teste de chamada de função com expressão como parâmetro.");
            Teste testeCalcVariosEscopos1 = new Teste(TesteBuildEscoposDefinicaoDeVariaveisRegistroDeFuncaoEChamadaDeFuncao, "Teste de cálculo de vários escopos.");
            Teste testeCalcVariosEscopos2 = new Teste(TesteBuildEscoposAtribuicaoComExpressaoRegistroDeFuncaoComCorpo, "teste de construção de escopos, atribuição de variável com expressão, e registro de função com corpo.");
            Teste testeCalcVariosEscopos3 = new Teste(TestePropriedadeEMetodos, "teste para atribuição de propriedade, e métodos.");
            Teste testeCalcVariasSequencias = new Teste(TesteCalculoDeVariasSequenciasID, "teste para cálculos de várias sequencias ID");
            Teste testeMatchSequencias2 = new Teste(TesteObtencaoIndicesMetodosTratadoresParaFuncaoComParametroEComCorpo, "teste para encontrar indice de método tratador para sequencia de função com parâmetros e com corpo");
            Teste testeObterTokensEProducoes = new Teste(TesteObterProducoes, "teste para o novo método de obter tokens e produções.");
            Teste testeRegistrarExpressoes = new Teste(TesteRegistraExpressao, "teste para  registrar expressões.");
            Teste testeRegistrarExpressoes_2 = new Teste(TesteRegistraExpressaoComChamadasFuncao, "teste para registro de expressões com chamada de expressão.");
            Teste testeExtracaoChamadasDeFuncao = new Teste(TesteExtracaoDeChamadasFuncao, "teste para resumo de funções em sequencias resumidas.");
   

            //teste para Programação a Objetos.
            Teste testeExtraiClasses = new Teste(TesteProcessamentoDeClasses, "teste de extração de classe");
            Teste testeExtraiClassesComHeranca = new Teste(TesteProcessamentoDeClassesComHeranca, "teste para extração de classes com herança.");
            Teste testeSaveLoadClasse_1 = new Teste(TesteSaveLoadClasse, "teste para carregar e salvar uma classe em um arquivo.");
            Teste testeValidaInterface_1 = new Teste(TesteValidacaoInterfaceNaoImplementado, "teste para validar interfaces.");
            Teste testeValidaInterface_2 = new Teste(TesteValidacaoInterfaceImplementado, "teste para validar não implementação de interfaces.");
            Teste testeConstrucaoSequenciaDeOperador_1 = new Teste(TesteDefinicaoDeUmOperadorBinario, "teste de processamento de um operador binário, palavra chave da linguagem orquidea.");
            Teste testeConstrucaoSequenciaDeOperador_2 = new Teste(TesteDefinicaoDeUmOperadorUnario, "teste para definição de operador unário, palavra chave da linguagem orquidea.");
            Teste testeAtribuicaoDePropriedade_1 = new Teste(TesteAtribuicaoPropriedade, "teste de atribuição de propriedades, com multiplos acessos de objetos.");
            Teste testeExpressaoPropriedade_1 = new Teste(TesteExpressaoPropriedade, "teste de expressões formadas por propriedades encadeadas.");
            Teste testeExpressaoComMetodos_1 = new Teste(TesteExpressaoMetodo, "teste para construção de expressão com chamadas de métodos.");
            Teste testeDefinicaoDePropriedadeComAtribuicaoComMetodos_1 = new Teste(TesteAtribuicaoDePropriedadesComMetodos, "teste de extração de definição propriedades com atribuição com métodos.");


            ContainerTestes ContainerTestes = new ContainerTestes(testeGetTokensPolemicos_teste); 

            ContainerTestes.ExecutaTestesEExibeResultados(); 

        } // ContainerTestes()
    } // class CorpoTestes
} // namespace ModuloTESTES
