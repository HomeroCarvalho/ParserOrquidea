using System;
using System.Collections.Generic;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using ModuloTESTES;
namespace parser
{
    class CorpoTestes_3
    {
        LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
   
        private static void CenarioTestesProgramaOrquidea(List<string> codigo, out Escopo escopo)
        {
            escopo = null;
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
         
            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();

            ProgramaEmVM programa = new ProgramaEmVM(processador.GetInstrucoes());
            programa.Run(escopo);
        }

        object funcaoVazia(object[] parametros)
        {
            return null;
        }
  
 
        private static void CenarioTesteExtraiExpressoes(out Escopo escopo, List<string> codigo3, out List<Expressao> expressoes)
        {
            expressoes = null;
            List<string> tokens3 = new Tokens(LinguagemOrquidea.Instance(), codigo3).GetTokens();
            escopo = new Escopo(codigo3);

            Objeto v_a = new Objeto("public", "a", "int", 1);
            Objeto v_b = new Objeto("public", "b", "int", 1);
            Objeto v_c = new Objeto("public", "c", "int", 1);

            escopo.tabela.GetObjetos().Add(v_a);
            escopo.tabela.GetObjetos().Add(v_b);
            escopo.tabela.GetObjetos().Add(v_c);
            


            expressoes = Expressao.Instance.ExtraiExpressoes(escopo, tokens3);
        }

        private void TesteParaDefinicaoDeObjetoComExpressoesEmAtribuicao(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            List<string> codigo1 = new List<string>() { "int c=funcaoA()+a+b;" };


            ProcessadorDeID processador = new ProcessadorDeID(codigo1);
            processador.escopo.tabela.RegistraFuncao(new Funcao("public", "funcaoA", funcaoVazia, null));
            processador.escopo.tabela.AddObjeto("public", "a", "int", "1", processador.escopo);
            processador.escopo.tabela.AddObjeto("public", "b", "int", "5", processador.escopo);


            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construcao de codigo de defnicoes de variaveis feito sem erros fatais.");

            if ((processador.escopo.tabela.GetObjetos().Count == 3) &&
                (processador.escopo.tabela.GetObjeto("c", processador.escopo) != null))
                assercao.MsgSucess("definição de variável a partir de expressao completa feita sem erros.");
        }


        private void TesteImportarClasseDaLinguagemSuporte(Assercoes assercao)
        {

            ImportadorDeClasses importador = new ImportadorDeClasses("ParserLinguagemOrquidea.exe");
            importador.ImportAClassFromApplication(typeof(MATRIZES.Matriz));

            assercao.MsgSucess("Importação de classe feita sem erros fatais.");

            if (RepositorioDeClassesOO.Instance().GetClasse("MyMATRIX") != null)
                assercao.MsgSucess("importacao da classe MyMATRIX feita exatamente.");


            importador.ImportAClassFromApplication(typeof(Expressao));
            assercao.MsgSucess("Importação de classe Expressao feita sem erros fatais.");
            if (RepositorioDeClassesOO.Instance().GetClasse(typeof(Expressao).Name) != null)  
                assercao.MsgSucess("importacao da classe Expressao feita exatamente.");
        }

 

        ProcessadorDeID processador = null;
        private void TestePropriedadesAninhadas(Assercoes assercao)
        {

            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

            List<string> code = new List<string>() { "public class A { int varA=1;} protected class B {A varB;}" };
            Escopo escopo = new Escopo(code);

            CenarioTesteProcessamentoParaPropriedadesAninhadas(code, escopo);



            List<string> codeObject = new List<string>() { "obj1.varB.varA=1;" };

            escopo = new Escopo(codeObject);
            Objeto obj = new Objeto("private", "B", "obj1", null);
            escopo.tabela.RegistraObjeto(obj);

            CenarioTesteProcessamentoParaPropriedadesAninhadas(codeObject, escopo);

            assercao.MsgSucess("instruçao compilada sem erros fatais.");
            if ((processador.GetInstrucoes().Count == 1) && (processador.GetInstrucoes()[0].code == ProgramaEmVM.codeAtribution))
                assercao.MsgSucess("instrucao avaliada exatamente.");




            ProgramaEmVM programa = new ProgramaEmVM(processador.GetInstrucoes());
            programa.Run(escopo);

            assercao.MsgSucess("programa rodado sem erros fatais.");

            if ((RepositorioDeClassesOO.Instance().GetClasse("A") != null) &&
                (RepositorioDeClassesOO.Instance().GetClasse("B") != null))
                assercao.MsgSucess("processamento de classes feito exatamente.");



        }

        private string CenarioTesteProcessamentoParaPropriedadesAninhadas(List<string> code, Escopo escopo)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
          
            this.processador = new ProcessadorDeID(code);
            this.processador.CompileEmDoisEstagios();

            return "processamento de propriedades aninhadas feito sem erros fatais.";
        }

        private void TesteManipulacaoDeObjetos(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            List<string> codigoClasseTeste = new List<string>() { "public class A {int VarA=1; int funcaoA(){ int k=2;}}" };

            ProcessadorDeID processador = new ProcessadorDeID(codigoClasseTeste);

            // compila os tokens da classe A,
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("construcao da classe teste feita com sucesso.");

            // inicializa um objeto do tipo A.
            Objeto objeto = new Objeto("private", "A", "obj1", null);


            assercao.MsgSucess("inicializacao indireta de objeto feita sucessamente");
            if (objeto.GetFields().Count == 1)
                assercao.MsgSucess("construcao de objeto via indiretamente, feita sucessamente");

            List<string> codigoManipulacaoDeObjetos = new List<string>() { "obj1.VarA=1;" };
  
 
            ProcessadorDeID processadorObjetos = new ProcessadorDeID(codigoManipulacaoDeObjetos);
            processador.escopo.tabela.RegistraObjeto(objeto); // registra via codigo o objeto do cenario do teste.


            processadorObjetos.CompileEmDoisEstagios();

            assercao.MsgSucess("compilacao de um objeto feita com sucesso.");
            if ((processadorObjetos.GetInstrucoes().Count == 1) &&
                (processadorObjetos.escopo.tabela.GetObjetos().Count == 1))
                assercao.MsgSucess("processamento de build e instrucao para manipulacao de propriedades do objeto.");

            ProgramaEmVM programa = new ProgramaEmVM(processadorObjetos.GetInstrucoes());
            programa.Run(processador.escopo);

        }

    
        private void TesteDefinicaoDeMetodos(Assercoes assercao)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            List<string> codigo = new List<string>() { "public int funcaoB(int a, int b){int x=1;}" };
       
            ProcessadorDeID processador = new ProcessadorDeID(codigo);

            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("Definicao de um metodo compilado sem erros fatais.");

            if (processador.escopo.tabela.GetFuncoes().Count == 1)
                assercao.MsgSucess("Definicao de um metodo feito exatamente.");

            //____________________________________________________________________________________________________________________________________

            List<string> codigo1 = new List<string>() { "public class classeA { public int varA; public int funcaoB(int y, int z){int x=1;} }" };
 
            ProcessadorDeID processador1 = new ProcessadorDeID(codigo1);

            processador1.CompileEmDoisEstagios();

            assercao.MsgSucess("compilacao de uma classe com metodos feito sem erros fatais.");
            if ((RepositorioDeClassesOO.Instance().GetClasse("classeA") != null) &&
                (RepositorioDeClassesOO.Instance().GetClasse("classeA").GetMetodos().Count == 1))
                assercao.MsgSucess("compilacao de uma classe com metodos feito exatamente.");

        } // TesteDefinicaoDeMetodos()

        private ProcessadorDeID CenarioTesteConstrucaoDaClasseParaOTeste(List<string> code, out Escopo escopo)
        {
            LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();
            ProcessadorDeID processador = new ProcessadorDeID(code);

            processador.CompileEmDoisEstagios();
            escopo = processador.escopo;
            return processador;
        }


        private void TesteInstrucaoCreate(Assercoes assercao)
        {

            /// ID ID = create ( ID , ID  ---> tipo nomeObjeto = create (TipoParametro1 parametro1,... )
            /// ou:
            /// ID= create (ID , ID
            /// 
            List<string> codeClasse = new List<string>() { "public class ClasseB { int a; int b; }" };
            List<string> codeCreate = new List<string>() { "ClasseB obj1=  create ( a , b);" };

            ProcessadorDeID processador = null;
            processador = new ProcessadorDeID(codeClasse);


            // cria objetos parametros.
            Objeto objetoA = new Objeto("private", "int", "a", 1);
            Objeto objetoB = new Objeto("private", "int", "b", 1);
            processador.escopo.tabela.RegistraObjeto(objetoA);
            processador.escopo.tabela.RegistraObjeto(objetoB);

            processador.CompileEmDoisEstagios(); // constroi a classe teste.

            Funcao umConstrutor = new Funcao("public", "ClasseB", funcaoVazia, null, new Objeto[] {
                new Objeto("private","int","a", null, null, isStatic: false),
                new Objeto("private","int", "b", null, null,isStatic: false) });
            Classe classeTeste = RepositorioDeClassesOO.Instance().GetClasse("ClasseB");
            classeTeste.construtores.Add(umConstrutor);

            if (processador.GetInstrucoes().Count == 1)
                assercao.MsgSucess();
            else
                assercao.MsgFail();

            ProgramaEmVM programa = new ProgramaEmVM(processador.GetInstrucoes());
            programa.Run(processador.escopo);

            assercao.MsgSucess("instrucoes VM executadas");

        } // TesteInstrucaoCreate()

       
        private void TesteAceiteConstrucaoDeClasse(Assercoes assercao)
        {
            /*
             * 
             * public class umaClasse
              {
	                public int propA;
	                private int propB;

	            public int metodoA()
	            {
		            return -1;
	            }

	            private bool metodoB(int a, int b)
	            {
		            bool r= a<b;
		            return r;
	            }
              } 
             * 
             */
            ParserAFile parser= new ParserAFile("cenario2TesteAceite.txt");

            List<string> codigo = parser.GetCode();

            ProcessadorDeID processador = new ProcessadorDeID(codigo);
            processador.CompileEmDoisEstagios();
            assercao.MsgSucess("processamento de arquivo com classe feito sem erros fatais.");

            Classe classeTeste = RepositorioDeClassesOO.Instance().GetClasse("umaClasse");
            if ((classeTeste != null) && (classeTeste.GetMetodos().Count == 2) && (classeTeste.GetPropriedades().Count == 2))
                assercao.MsgSucess("processamento de arquivo com classe feito com exatidão.");
        }

        private void TesteAceiteClasseComHeranca(Assercoes assercao)
        {

            string codigo = "public class AncestralA{ public int propAncestralA; public string propNomeAncestralA; public bool GetName(){ return propNomeAncestralA;} private bool VerificaID( int id) { if (id==propAncestralA) return 	true; return false;}} public class AncestralB { public int propAncestralB; public string propNomeAncestralB; private int propPrivateAncestralB; public bool GetNomeAncestral(){ int k=0; return (k==1);	}}  public class ClasseQueHerda: + AncestralA, + AncestralB {}";
            ParserAFile parser = new ParserAFile("cenario3TesteAceite.txt");
            List<string> tokens = new Tokens(LinguagemOrquidea.Instance(), new List<string>() { codigo }).GetTokens();


            ProcessadorDeID processador = new ProcessadorDeID(tokens);
            processador.CompileEmDoisEstagios();
            assercao.MsgSucess("processamento de arquivo com classe feito sem erros fatais.");

        }

        private void TesteAceiteFuncao(Assercoes assercao)
        {
   
            /*
             * 
             * int funcaoA(int x)
                {
                    int a=x+1;
                    for (a=1;a< 5;a++)
                    {
	                    x=x+3;
                    }
                    return x;
                }
             * 
             * 
             */
            ParserAFile parserTesteAceite = new ParserAFile("cenario1TesteAceite.txt");
            List<string> code = parserTesteAceite.GetCode();
            ProcessadorDeID processador = new ProcessadorDeID(code);
            processador.CompileEmDoisEstagios();

            assercao.MsgSucess("processamento de arquivo de funcao feito sem erros fatais.");

            if ((processador.escopo.tabela.GetFuncoes() != null) && (processador.escopo.tabela.GetFuncoes().Count > 0))
                assercao.MsgSucess();
        }


        
        public CorpoTestes_3()
        {
          
            Teste testeDefinicoesDeVariaveis_teste = new Teste(this.TesteParaDefinicaoDeObjetoComExpressoesEmAtribuicao, "teste para verificacao de variaveis construidas com atribuicoes de expressoes.");
            Teste testeImportacaoDeClasse_teste = new Teste(this.TesteImportarClasseDaLinguagemSuporte, "teste de importacao de classe da matriz de suporte.");
            Teste testePropriedadesAninhadas_teste = new Teste(this.TestePropriedadesAninhadas, "teste para processamento de propriedades aninhadas.");
            Teste testeManipulacaoDePropriedadeDeObjetos_teste = new Teste(this.TesteManipulacaoDeObjetos, "teste para manipular atribuicao de propriedades de um objeto.");
            Teste testeDefinicaoDeMetodos_teste = new Teste(this.TesteDefinicaoDeMetodos, "teste para validacao de metodos.");
            Teste testeCreateInstruction_teste = new Teste(this.TesteInstrucaoCreate, "teste para a instrução Create");
     

            Teste testeAceiteFuncaoEstruturada_teste = new Teste(this.TesteAceiteFuncao, "teste de aceite para codigo completo de funcao estruturada.");
            Teste testeAceiteConstrucaoDeClasse_teste = new Teste(this.TesteAceiteConstrucaoDeClasse, "teste de aceite para codigo completo de uma classe.");
            Teste testeAceiteConstrucaoDeClasseComHeranca_teste = new Teste(this.TesteAceiteClasseComHeranca, "teste de aceite para o codigo completo de classes com heranca.");

            ContainerTestes container = new ContainerTestes(testeAceiteConstrucaoDeClasse_teste);
            container.ExecutaTestes();

        } // CorpoTestes_3()
       
    } // class
} // namespace
