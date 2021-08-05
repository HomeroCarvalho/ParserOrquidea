using System.Collections.Generic;
using System.Linq;
using ModuloTESTES;
namespace parser.LISP
{
    public class CorpoTestesLISP
    {


        private void TesteListaComoElemento(Assercoes assercao)
        {
            ListaLISP listaElemento = new ListaLISP("1 4");
            ListaLISP listaSaida = new ListaLISP();
            listaSaida.Listas.Add(listaElemento);
            System.Console.WriteLine(listaSaida.ToString());
        } // TesteListaComoElemento()

        private void TesteListaHeadListTail(Assercoes assercao)
        {
            ListaLISP lista = new ListaLISP();
            ListaLISP listHeadEntrada = new ListaLISP("( 1 5 6)");
            lista.Listas.Add(listHeadEntrada);

            ListaLISP listaHeadSaida = lista.car();
            listaHeadSaida.EscreveLista();

            ListaLISP listaTailEntrada = new ListaLISP("(1 5 )");
            ListaLISP listaTailSaida = listaTailEntrada.cdr();
            listaTailSaida.EscreveLista();
            if (listaHeadSaida.GetAllElements().Count == 3)
                assercao.MsgSucess("Extração de lista cabeça bem sucedida.");
            if (listaTailSaida.GetAllElements().Count == 1)
                assercao.MsgSucess("Extração de lista cauda bem sucedida.");
        } // TesteListaHeadListTail()

        private void TesteElementosLista(Assercoes assercao)
        {
            List<string> elementosLista = new List<string>() { "1", "4", "3", "5", "6" };
            ListaLISP umaLista = new ListaLISP();

            umaLista.InsertElementsTail(elementosLista);
            List<string> elementosRetorno = umaLista.GetAllElements();

            umaLista.EscreveLista();
            if (elementosRetorno.Count == 5)
                assercao.MsgSucess("Operações com elementos da lista bem sucedida.");
            else
                assercao.MsgFail("Operações com elementos de lista falha."); 
        } // TesteGetElementsLISP()
        
        private void TesteConstrucaoListaComTexto(Assercoes assercao)
        {
            string textoLista = "( (2 3 ((1 4) (3 6)) 5 6))";
            //string textoLista = "( 2 3 (1 4))";

            //string textoLista = "((1 4) 3)";

            ListaLISP lstRetorno =  ListaLISP.GetLista(textoLista);
            ListaLISP listaGuarda = lstRetorno;

            lstRetorno.EscreveLista();

            if (listaGuarda.GetAllElements().Count == 8)
            {
                System.Console.WriteLine("Obter lista a partir de texto bem sucedido!");
                assercao.MsgSucess("Operações com obter lista a partir de texto bem sucedidas.");
            }
            else
                assercao.MsgFail("Operações para obter lista a partir de texto falha.");
        } //TesteConstrucaoListaComTexto()

        private void TesteFuncoesLispCdr(Assercoes assercao)
        {
            string textoLista = "( cdr (2 3) 5)";
            ListaLISP listaCdrEntrada = ListaLISP.GetLista(textoLista);

            FuncaoLISP funcao = new FuncaoLISP(textoLista);
            listaCdrEntrada.EscreveLista();
            funcao.InitComandosLISP();

            ListaLISP listaCdrSaida = funcao.AvaliaFuncao(listaCdrEntrada);
            System.Console.WriteLine("função cdr aplicada:");
            listaCdrSaida.EscreveLista();

            if ((listaCdrSaida.GetAllElements().Count == 1) && (listaCdrSaida.Listas[0].valor=="5"))
                assercao.MsgSucess("Função cdr processada com sucesso.");
            else
                assercao.MsgFail("Função cdr falhou.");
 
        } // TesteFuncoesLispCdr()

        private void TesteFuncoesLispCar(Assercoes assercao)
        {
            string textoLista = "( car (2 3) (5 6))";
            ListaLISP listaCarEntrada = ListaLISP.GetLista(textoLista);
            listaCarEntrada.EscreveLista();
            FuncaoLISP funcao = new FuncaoLISP(textoLista);
            funcao.InitComandosLISP();

            ListaLISP listaCarSaida = funcao.AvaliaFuncao(listaCarEntrada);
            System.Console.WriteLine("função car aplicada:");
            listaCarSaida.EscreveLista();
            if (listaCarSaida.GetAllElements().Count == 2)
                assercao.MsgSucess("Função car processada com sucesso.");
            else
                assercao.MsgFail("Função car falhou.");
        } // TesteFuncoesLispCar()
       
        public void TesteDeConversaoDeUmTextoDefunEmListaLISP(Assercoes assercao)
        {
            string textoLisp = "(defun discr (a b c) (- (* b b) (* 4 a c)))";
            ListaLISP umaFuncaoLisp = ListaLISP.GetLista(textoLisp);
            ListaLISP.WriteListaLISP(umaFuncaoLisp, 0, true);
            System.Console.ReadKey();
        } // TesteObterDadosDeUmaInstrucaoLisp()

        private void TesteObterTokensLisp(Assercoes assercao)
        {
            string textolisp = "((== A B))";
            List<string> tokens = ListaLISP.GetTokensLISP(textolisp);
            System.Console.WriteLine("trecho de código: " + textolisp);
            System.Console.WriteLine("tokens retirados: ");
            for (int x = 0; x < tokens.Count; x++)
            {
                System.Console.Write(tokens[x] + " ");
            } // for x
            if (tokens.Count == 7)
                assercao.MsgSucess("Método de retirada de tokens LISP bem sucedido");
            else
                assercao.MsgFail("Falha no método de retirada de tokens LISP");
        } // TesteObterTokensLisp()


        private void TesteConstrucaoDeFuncao(Assercoes assercao)
        {
            string textoFuncao = "(defun foo (x y) (+ x y 5))";
            // obtém uma lista função a partir de um texto.
            ListaLISP lstSaida = ListaLISP.GetLista(textoFuncao);
            string textoListaEscreve = ListaLISP.WriteListaLISP(lstSaida, 0, true);
            assercao.MsgSucess("Construção de uma função lisp bem sucedida, sem erros fatais.");
        } // TesteConstrucaoDeFuncao()

       
      
        private void TesteComandoCons(Assercoes assercao)
        {
            string listaCons = "(cons 'a '(b c))";
            FuncaoLISP funcao = new FuncaoLISP(listaCons);
            ListaLISP umaLista = new ListaLISP(listaCons);
            List<DadosDoParametro> lstParametros = new List<DadosDoParametro>();
            lstParametros.Add(new DadosDoParametro("a", "a"));
            lstParametros.Add(new DadosDoParametro("b", "b"));
            lstParametros.Add(new DadosDoParametro("c", "c"));
            funcao.InitComandosLISP();
            ListaLISP listaSaida = FuncaoLISP.ComandosLISP["cons"](umaLista, lstParametros);
            System.Console.WriteLine(listaSaida);
            assercao.MsgSucess("teste com instrução cons completado sem erros fatais.");
        } // TesteComandoCons()


        private void TesteComandoIF(Assercoes assercao)
        {
            string listaIF = "(if  (> 4 5) 6 3)";
            FuncaoLISP funcao = new FuncaoLISP(listaIF);

            ListaLISP lstSaida = funcao.AvaliaFuncao(new ListaLISP(listaIF));
            assercao.MsgSucess("Teste com comando IF bem sucedido.");
        } // TesteComandoIF()

        private void TesteComandoCond(Assercoes assercao)
        {
            string textoCond = "(cond ((> a 7)(/ a 2)) ((< a 5) (- a 1)) (T 17))";                
  
            ListaLISP umalista = new ListaLISP(textoCond);
            FuncaoLISP funcao = new FuncaoLISP(textoCond);

            List<DadosDoParametro> lstParametros = new List<DadosDoParametro>();
            lstParametros.Add(new DadosDoParametro("a", 4));
            ListaLISP listaSaida =funcao.AvaliaFuncao(umalista);
            System.Console.WriteLine(listaSaida);
            if (listaSaida.Listas[0].nome == "3")
                assercao.MsgSucess("Condicional cond calculado corretamente.");
            else
                assercao.MsgFail("Condicional cond com falhas.");
            assercao.MsgSucess("Teste com comando Cond bem sucedido.");
        } // TesteComandoIF()

        private void TesteComandoMember(Assercoes assercao)
        {
            string textoLista = "( member a '( a b c))";
            FuncaoLISP funcao = new FuncaoLISP(textoLista);

            ListaLISP listaSaida = funcao.AvaliaFuncao(new ListaLISP(textoLista));
            assercao.MsgSucess("Member função calculado sem falhas.");

            if (FuncaoLISP.IsTrue(listaSaida))
                assercao.MsgSucess("Saída da função calculado positivamente.");
            else
                assercao.MsgFail("Fallha no cálculo da saída da função.");
        } // TesteComandoMember()

        private void TesteSetq(Assercoes assercao)
        {
            string textoLista = "(setq a 5)";
            FuncaoLISP funcao = new FuncaoLISP(textoLista);
            ListaLISP lstResult = funcao.AvaliaFuncao(new ListaLISP(textoLista));
          
            assercao.MsgSucess("Comando setq calculado sem erros fatais.");
          } // TesteSetq()

        private void TesteMultiplicacaoDivisao(Assercoes assercao)
        {
            string textolistaDivisao = "(/ 5 5)";
            FuncaoLISP funcaoDiv = new FuncaoLISP(textolistaDivisao);

            ListaLISP umaLista2 = new ListaLISP(textolistaDivisao);
            ListaLISP lstSaida2 = funcaoDiv.AvaliaFuncao(umaLista2);


            string textolistaMultiplicacao = "(* 5 5)";
            FuncaoLISP funcaoMult = new FuncaoLISP(textolistaMultiplicacao);
            ListaLISP umaLista1 = new ListaLISP(textolistaMultiplicacao);
            ListaLISP lstSaida1 = funcaoMult.AvaliaFuncao(umaLista1);

          

            string textolistaAdicao = "(+ 1 6)";
            FuncaoLISP funcaoAdd = new FuncaoLISP(textolistaAdicao);

            ListaLISP umalista3 = new ListaLISP(textolistaAdicao);
            ListaLISP lstSaida3 = funcaoAdd.AvaliaFuncao(umalista3);

            string textolistaSubtracao = "(- 1 7)";
            FuncaoLISP funcaoSub = new FuncaoLISP(textolistaSubtracao);

            ListaLISP umalista4 = new ListaLISP(textolistaSubtracao);
            ListaLISP lsSaida4 = funcaoSub.AvaliaFuncao(umalista4);

            assercao.MsgSucess("Testes com as 4 operações matemáticas avaliados sem erros fatais.");

            if (lstSaida3.Listas[0].valor == "7")
                assercao.MsgSucess("Adição efetuada corretamente");
            else
                assercao.MsgFail("Adição efetuada com falhas.");


            if (lsSaida4.Listas[0].valor == "-6")
                assercao.MsgSucess("Subtração efetuada positivamente.");
            else
                assercao.MsgFail("Suvtração efetuada com falhas.");


            if (lstSaida1.Listas[0].valor == "25")
                assercao.MsgSucess("Multiplicação efetuada positivamente");
            else
                assercao.MsgFail("Multiplicação efetuada com falhas.");


            if (lstSaida2.Listas[0].valor == "1")
                assercao.MsgSucess("Divisão efetuada positivamente");
            else
                assercao.MsgFail("Divisão efetuada com falhas.");
        } // TesteMultiplicacaoDivisao()


        private void TesteInstrucaoCADXXR(Assercoes assercao)
        {
            string textoFuncao = "caddaar( a b (c d))";
            FuncaoLISP funcao = new FuncaoLISP(textoFuncao);
            List<DadosDoParametro> parametros = new List<DadosDoParametro>();
    
            ListaLISP listaSaida =  funcao.AvaliaFuncao(funcao.definicaoDaFuncao);
            if (listaSaida.Listas[0].nome == "c")
            {
                assercao.MsgSucess("comando variável cadxxr avaliada corretamente.");
            } // if
            else
                assercao.MsgFail("comando variável cadxxr avaliada com falha");
        } // TesteInstrucaoCADXXR

        private void TesteInstrucaoCondicional(Assercoes assercao)
        {
            string textoLista = "(= 5 5)";
            List<DadosDoParametro> parametros = new List<DadosDoParametro>();
            parametros.Add(new DadosDoParametro("x", "1"));
            parametros.Add(new DadosDoParametro("y", "5"));
            FuncaoLISP funcao = new FuncaoLISP(textoLista);

            ListaLISP lstSaida = funcao.AvaliaFuncao(funcao.definicaoDaFuncao);
            if (FuncaoLISP.IsNILL(lstSaida))
                assercao.MsgSucess("Avaliação da condicional testada positivamente.");

        } // TesteInstrucaoCondicional()

        private void TesteVerificacaoObtencaoParametros(Assercoes assercao)
        {
            
            // inicializa uma função para averiguação de obtenção de parametros.
            string textoFuncao = "(defun foo (x y) (+ x y 5))";
            FuncaoLISP funcaolisp = new FuncaoLISP(textoFuncao);

            // inicializa uma lista, que será utilizada para ter retirado parametros, desta lista.
            string textoChamada = "(foo 1 5)";
            ListaLISP lstChamada = new ListaLISP(textoChamada);
            List<DadosDoParametro> parametroExtraidos = funcaolisp.ObtemParametrosDeUmaChamadaDeFuncao(lstChamada);
            if ((parametroExtraidos[0].valor.ToString() == "1") && (parametroExtraidos[1].valor.ToString() == "5"))

                assercao.MsgSucess("Chamada a função não recursiva completada sem erros fatais. Cálculos obtidos exatos.");


            string textoFuncaoRecursiva = "(defun recursive (x)(if (= x 0) (1) (* x recursive(- x  1))))";
            FuncaoLISP funcaolisp2 = new FuncaoLISP(textoFuncaoRecursiva);

            string textoChamada2 = "(foo ( (+ 5 2) (recursive (1))";
            ListaLISP lstChamada2 = new ListaLISP(textoChamada2);
            List<DadosDoParametro> parametrosExtraidos2 = funcaolisp2.ObtemParametrosDeUmaChamadaDeFuncao(lstChamada2);

            assercao.MsgSucess("Chamada a função recursiva completada sem erros fatais.");


        } // TesteAvaliacaoFuncaoLispRecursiva()


        private void TesteAvaliacaoFuncaoLispRecursiva1(Assercoes assercao)
        {
            string textoFuncao = "(defun foo (x y) (+ x y 5))";
            //string textoFuncao = "(defun fatorialSimples(x) (if (= x 0) 1 (fatorialSimples (- n 1)))";
            //string textoFuncao = "(defun fact (x) (if (> x 0) (* x (fact (- x 1)))  1) )";

            //string textoFuncao = "(defun fact (x) (if (> x 0) (*x(fact(-x 1))) 1 ) )";
            FuncaoLISP funcaolisp = new FuncaoLISP(textoFuncao);
            string textoChamada = "(foo 1 5)";
            ListaLISP lstChamada = funcaolisp.AvaliaFuncao(new ListaLISP(textoChamada));
            assercao.MsgSucess("Chamada a função recursiva completada sem erros fatais.");
            if (lstChamada.Listas[0].valor == "11")
                assercao.MsgSucess("Avaliação de função com resultado positivo.");
            else
                assercao.MsgFail("Avaliação de função com falhas");

        } // TesteAvaliacaoFuncaoLispRecursiva()

        private void TesteAvaliacaoFuncaoLispRecursiva2(Assercoes assercao)
        {

             string textoFuncao = "(defun recursive (x)(if (= x 0) (1) (* x recursive(- x  1))))";
            //string textoFuncao = "(defun recursive(x) (if (= x 0) (1) (recursive (- x 1)))";
            //string textoFuncao = "defun recursive (x) (if (= x 5) (recursive (- x 1)) (x)))";

            //string textoFuncao = "(defun foo (x y) (+ x y 5))";
            //string textoFuncao = "(defun fact (x) (if (> x 0) (* x (fact(- x 1))) (1) ) )";

            // registra a função, pois o texto tem o comando defun.
            FuncaoLISP funcao = new FuncaoLISP(textoFuncao);

            string textoChamada = "(recursive 1)";
            ListaLISP lstResult = funcao.AvaliaFuncao(new ListaLISP(textoChamada));
            assercao.MsgSucess("avaliação de funçao lisp sem erros fatais.");
            if (lstResult.Listas[0].nome == "1")
                assercao.MsgSucess("avaliação de função lisp feita corretamente.");
            else
                assercao.MsgFail("avaliação de função lisp feita com falhas.");
        } // AvaliacaoFuncaoLisp()

        private void TesteAvaliacaoFuncaoLispRecursiva3(Assercoes assercao)
        {
            
            //string textoFuncao = "(defun foo (x y) (+ x y 5))";
            //string textoFuncao = "(defun fact (x) (if (> x 0) (* x (fact(- x 1))) (1) ) )";
            //string textoFuncao = "(defun recursive (x)(if (= x 0) (1) (* x recursive(- x  1))))";
            string textoFuncao = "(defun recursive(x) (if (= x 0) (1) (recursive (- x 1)))";

            // registra a função, pois o texto tem o comando defun.
            FuncaoLISP funcao = new FuncaoLISP(textoFuncao);

            string textoChamada = "(recursive 2)";
            ListaLISP lstResult = funcao.AvaliaFuncao(new ListaLISP(textoChamada));
            assercao.MsgSucess("avaliação de funçao lisp sem erros fatais.");
            if (lstResult.Listas[0].nome == "1")
                assercao.MsgSucess("avaliação de função lisp feita corretamente.");
            else
                assercao.MsgFail("avaliação de função lisp feita com falhas.");
        } // AvaliacaoFuncaoLisp()

        private void TesteAvaliacaoFuncaoLispRecursiva4(Assercoes assercao)
        { 

            string textoFuncaoSimples = "(defun foo (z w) (+ z z 0))";
            string textoFuncaoRecursiva= "(defun recursive(x) (if (= x 0) 1 (recursive (- x 1)))";
   
            // registra a função, pois o texto tem o comando defun.
            FuncaoLISP funcaoSimples = new FuncaoLISP(textoFuncaoSimples);
            FuncaoLISP funcaoRecursiva = new FuncaoLISP(textoFuncaoRecursiva);

            string textoChamada1 = "(recursive (* 1 (foo (0 0)))";
            ListaLISP lstChamada = new ListaLISP(textoChamada1);

            ListaLISP listaChamada = new ListaLISP();
            for (int x = 0; x < lstChamada.Listas.Count; x++)
            {

                if ((ListaLISP.IsNumber(lstChamada.Listas[x].valor)) ||
                    (funcaoRecursiva.IsBasicFunction(lstChamada.Listas[x].nome)) ||
                    (RepositoryFunctionsLISP.Instance().IsFunctionStored(lstChamada.Listas[x].nome)) ||
                    (lstChamada.Listas[x].GetType() == typeof(Atomo)))

                {
                    string valorParametroChamada = lstChamada.Listas[x].nome;
                    listaChamada.Listas.Add(new Atomo(valorParametroChamada, valorParametroChamada));
                }
                if (lstChamada.Listas[x].GetType() == typeof(ListaLISP))
                {
                    ListaLISP lstParametroChamada = funcaoRecursiva.AvaliaFuncao((ListaLISP)lstChamada.Listas[x]);
                    for (int k = 0; k < lstParametroChamada.Listas.Count; k++)
                    {
                        string valorParametroChamada = lstParametroChamada.Listas[k].nome;
                        listaChamada.Listas.Add(new Atomo(valorParametroChamada, valorParametroChamada));
                    } // if
                } // if
            } // for x


            assercao.MsgSucess("avaliação de funçao lisp sem erros fatais.");

            ListaLISP lstResult = funcaoRecursiva.AvaliaFuncao(listaChamada);
            if (lstResult.Listas[0].valor == "1")
                assercao.MsgSucess("avaliação da função feita corretamente.");
            
        } // AvaliacaoFuncaoLisp()

        private void TesteOperacaoMatematicaComListaEmbutida(Assercoes assercao)
        {

            string textoLista = "(* (+ 1 5) 1)";

            ListaLISP listaFuncao = new ListaLISP(textoLista);
            FuncaoLISP funcao = new FuncaoLISP(textoLista);

            ListaLISP result = funcao.AvaliaFuncao(new ListaLISP(textoLista));

            assercao.MsgSucess("Operação dentre operadores realizado sem erros fatais.");
            if (result.Listas[0].valor.ToString().Equals("6"))
                assercao.MsgSucess("Operação calculada com sucesso.");
            else
                assercao.MsgFail("Operação calculada com falhas.");
        } // TesteOperacaoEntreParametros()

        private void TesteOperacoesEmbutidasCombinadasComFuncoesNaoPredifinidas(Assercoes assercao)
        {
            string textoFuncaoSimples = "(defun foo (z) z)";
            string textoListaChamada= "(* (foo 1 ) 3)";
            List<DadosDoParametro> parametros = new List<DadosDoParametro>() { new DadosDoParametro("z", "2") };
            FuncaoLISP funcaoFoo = new FuncaoLISP(textoFuncaoSimples);
            FuncaoLISP funcaoChamada = new FuncaoLISP(textoListaChamada);
            ListaLISP result = funcaoFoo.AvaliaFuncao(funcaoChamada.definicaoDaFuncao);

            assercao.MsgSucess("Cálculo de funções não predefinidas compostas avaliada sem erros fatais.");
            if (result.Listas[0].valor == "3")
                assercao.MsgSucess("Resultado do cálculos exatos.");
            else
                assercao.MsgFail("Falha nos cálculos resultados");
        } // TesteOperacoesEmbutidasCombinadasComFuncoesNaoPredifinidas()

   
        private void TesteOperacoesMatematicsDentroDaListaDeParametos(Assercoes assercao)
        {
            /// (defun foo (z) (+ z 5))
            string textoFuncaoEmbutida = "(defun foo (x) (+ x 1))";
            string textoFuncaoDefinicao = " (defun fact(x) (foo (- x 3)))";
            FuncaoLISP funcaoNaoRecursiva = new FuncaoLISP(textoFuncaoEmbutida);
            FuncaoLISP funcaoRecursiva = new FuncaoLISP(textoFuncaoDefinicao);

            ListaLISP listaChamada = new ListaLISP("fact 1");

            ListaLISP result = funcaoRecursiva.AvaliaFuncao(listaChamada);

            assercao.MsgSucess("Operação com llsta de parâmetos sem falhas fatais.");
            if (result.Listas[0].valor == "-1")
                assercao.MsgSucess("Resultados da avaliação sem erros.");
            else
                assercao.MsgFail("Falha na avaliação dos resultados.");
        } // TesteOperacoesMatematicsDentroDaListaDeParametos()

        public CorpoTestesLISP()
        {
            Teste testeGetTokens = new Teste(TesteObterTokensLisp, "teste para obter tokens da lingugem lisp.");
            Teste testeListaDentroDeLista = new Teste(TesteListaComoElemento, "teste de inserção de uma lista para uma lista");
            Teste testeElementosListLISP = new Teste(TesteElementosLista, "obtenção de elementos de uma lista lisp.");
            Teste testeListaCabecaListaCauda = new Teste(TesteListaHeadListTail, "teste para obtenção de lista cabeça e lista cauda.");
            Teste testeListaAPartirDeTexto = new Teste(TesteConstrucaoListaComTexto, "teste de lista a partir de texto.");
            Teste testeFuncaoCar = new Teste(TesteFuncoesLispCar, "teste para função car.");
            Teste testeFuncaoCdr = new Teste(TesteFuncoesLispCdr, "teste para função cdr.");
            Teste testeInstrucoesLISP = new Teste(TesteDeConversaoDeUmTextoDefunEmListaLISP, "teste de obter instruções lisp de uma função LISP");
            Teste testeBuildFunction = new Teste(TesteConstrucaoDeFuncao, "teste de obter os tokens a partir de um texto.");
            Teste TesteComInstrucaoCons = new Teste(TesteComandoCons, "teste com a instrução cons.");
            Teste TesteIFComando = new Teste(TesteComandoIF, "teste com comando IF.");
            Teste TesteCondComando = new Teste(TesteComandoCond, "teste do comando cond.");
            Teste TesteMemberComando = new Teste(TesteComandoMember, "teste para o comando member.");
            Teste TesteSetqComando = new Teste(TesteSetq, "teste comando setq");
            Teste TesteCondificional = new Teste(TesteInstrucaoCondicional, "teste para operações matemáticas..");
            Teste TesteOperacoesMatamaticas = new Teste(TesteMultiplicacaoDivisao, "testes com operação de multiplicação e divisão.");
            Teste TesteAvaliaInstrucaoCADXXR = new Teste(TesteInstrucaoCADXXR, "teste da instrução variável CADXXR");
            Teste TesteOperacaoEmbutida = new Teste(TesteOperacaoMatematicaComListaEmbutida,"teste operação matemática com lista embutida no cálculo.");
            Teste TesteOperacaoCompostaEmbutida = new Teste( TesteOperacoesEmbutidasCombinadasComFuncoesNaoPredifinidas, "Teste operação com funções compostas.");
            Teste TesteFuncaoRecursiva1 = new Teste(TesteAvaliacaoFuncaoLispRecursiva1, "teste de avaliação da função recursiva [foo].");
            Teste TesteFuncaoRecursiva2 = new Teste(TesteAvaliacaoFuncaoLispRecursiva2, "teste de avaliação de função lisp, com passagem de parâmetros, e construção de função  lisp via texto.");
            Teste TesteFuncaoRecursiva3 = new Teste(TesteAvaliacaoFuncaoLispRecursiva3, "teste de avaliação de função lisp, com passagem de parâmetros e exercendo múltiplas recursões.");
            Teste TesteFuncaoRecursiva4 = new Teste(TesteAvaliacaoFuncaoLispRecursiva4, "teste de avaliação de função lisp, com passagem de parâmetros sobre funções recursiva e simples.");
            Teste TesteFuncoesEmbutidasECalculoEntreParametros = new Teste(TesteOperacoesMatematicsDentroDaListaDeParametos, "teste de passagem de parâmetros em uma função não recursiva.");
            Teste TesteObtencaoDeParametros = new Teste(TesteVerificacaoObtencaoParametros, "teste para obtenção de parâmetros.");

            ContainerTestes container = new ContainerTestes(TesteFuncaoRecursiva4);
            container.ExecutaTestesEExibeResultados();
        }// CorpoTestesLISP()
    } // class CorpoTestesLISP
}// namespace parser
