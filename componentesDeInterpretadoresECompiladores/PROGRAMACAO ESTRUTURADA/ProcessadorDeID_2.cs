using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public  class BuildInstrucoes
    {

        private List<string> codigo = new List<string>();
        private Escopo escopo = null;
        private static LinguagemOrquidea linguagem = null;
        public BuildInstrucoes(List<string> codigo)
        {
            if (linguagem == null)
                linguagem = new LinguagemOrquidea();
            this.codigo = codigo.ToList<string>();
            this.escopo = new Escopo(codigo);
        }

  
        protected Instrucao BuildInstrucaoImporter(UmaSequenciaID sequencia, Escopo escopo)
        {
            // importer ( nomeAssembly).
            if (!sequencia.original.Contains("importer"))
                return null;
            if (sequencia.original.Count < 3)
                return null;
            List<string> tokensInstrucao = sequencia.original.ToList<string>(); // faz uma copia dos tokens, para evitar problemas no metodo CompileEscopos()

            // remove o nome da instrução, e os parenteses abre e fecha da instrucao, e o ponto e virgula do comando.
            tokensInstrucao.RemoveRange(0,2);
            tokensInstrucao.RemoveRange(tokensInstrucao.Count - 2, 2);

            string nomeArquivoAsssembly = Util.UtilString.UneLinhasLista(tokensInstrucao);
            nomeArquivoAsssembly = nomeArquivoAsssembly.Replace(" ", "");

            Expressao exprss_comando = new Expressao(new string[] { "importer", nomeArquivoAsssembly }, escopo);

            escopo.tabela.GetExpressoes().Add(exprss_comando);

            ImportadorDeClasses importador = new ImportadorDeClasses(nomeArquivoAsssembly);
            importador.ImportAllClassesFromAssembly(); // importa previamente as classes do arquivo assembly.

            Instrucao instrucaoImporter = new Instrucao(ProgramaEmVM.codeImporter, new List<Expressao> { exprss_comando }, new List<List<Instrucao>>());
            return instrucaoImporter;
        }  // BuildInstrucaoConstructor()

        /// <summary>
        /// Cria um novo objeto. pode criar um objeto simples, ou um objeto de variavel vetor.
        /// </summary>
        /// Estrutura de dados na lista de expressoes:
        /// 0- id "create".
        /// 1- tipo do objeto.
        /// 2- nome do objeto.
        /// 3- Lista de expressoes contendo objetos parametros (nome,tipo, valor).
        /// 4- indice do construtor na lista de construtores do tipo do objeto.
        /// 5- lista de expressoes indices, se for uma variavel vetor. (os indices são avaliados no momento que se cria o objeto, no programa VM).
        protected Instrucao BuildInstrucaoCreate(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
            
            string tipoDoObjeto = sequencia.original[0].ToString();
            string nomeDoObjeto = sequencia.original[1];

            
            ValidaTokensDaSintaxeDaInstrucao(sequencia, escopo);

            ExpressaoOperadorMatricial expressoesIndicesVetor = ObtemIndicesVetoriais(sequencia, escopo, nomeDoObjeto);
            List<Expressao> expressoesParametros = ObtemExpressoesParametros(sequencia, escopo);



            Expressao exprssDaIntrucao = new Expressao();
            Expressao exprssParametros = new Expressao();
            exprssParametros.Elementos.AddRange(expressoesParametros);


            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("create"));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(tipoDoObjeto));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(nomeDoObjeto));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("reservado"));
            exprssDaIntrucao.Elementos.Add(expressoesIndicesVetor);
            exprssDaIntrucao.Elementos.Add(exprssParametros);

            if (tipoDoObjeto == "Vetor")
            {
                List<int> indicesVetor = new List<int>();
                EvalExpression eval = new EvalExpression();
                try
                {


                    for (int tokenIndice = 0; tokenIndice < expressoesParametros[0].Elementos.Count; tokenIndice++)
                    {
                        object umIndice = eval.EvalPosOrdem(expressoesParametros[0].Elementos[tokenIndice], escopo);
                        if (umIndice != null)
                            indicesVetor.Add(int.Parse(umIndice.ToString()));

                    }
                }
                catch
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Os indices da variavel vetor: " + nomeDoObjeto + " devem ser apenas numeros inteiros.");
                    return null;
                }

                escopo.tabela.GetVetores().Add(new Vetor("private", nomeDoObjeto, indicesVetor.ToArray())); // adiciona a variavel vetor criada, para a compilação das próximas instruções, em outros builds.
            }
            else
            {
                Classe classeDoObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoObjeto);


                // adiciona a objeto criado, para a compilação das próximas instruções, em outros builds.
                escopo.tabela.GetObjetos().Add(new Objeto("private", tipoDoObjeto, nomeDoObjeto, null));
            }
            escopo.tabela.GetExpressoes().Add(exprssDaIntrucao); // registra as expressões, a fim de otimização de modificação.

            // cria a instrucao do objeto.
            Instrucao instrucaoCreate = new Instrucao(ProgramaEmVM.codeCreateObject, new List<Expressao>() { exprssDaIntrucao }, new List<List<Instrucao>>());
            return instrucaoCreate;

        } // BuildInstrucaoCreate()


        /// <summary>
        /// obtem expressoes de uma sequencia. Encontra o tipo dos elementos de cada expressão encontrada.
        /// </summary>
        protected List<Expressao> ObtemExpressoesParametros(UmaSequenciaID sequencia, Escopo escopo)
        {
            List<Expressao> parametros = null;

            int indexStartParams = sequencia.original.IndexOf("(");
            List<string> tokens = UtilTokens.GetCodigoEntreOperadores(indexStartParams, "(", ")", sequencia.original);

            if ((tokens != null) && (tokens.Count > 0))
            {
                tokens.RemoveAt(0);
                tokens.RemoveAt(tokens.Count - 1);

                parametros = Expressao.Instance.ExtraiExpressoes(escopo, tokens);
                for (int p = 0; p < parametros.Count; p++)
                {
                    for (int el = 0; el < parametros[p].Elementos.Count; el++)
                    {
                        string tipoElemento = Classe.EncontraClasseDoElemento(parametros[p].Elementos[el].ToString(), escopo);
                        parametros[p].Elementos[el].tipo = tipoElemento;
                    } // for el
                } // for p
            } // if
            else
                parametros = new List<Expressao>();
            return parametros;
        }



        protected ExpressaoOperadorMatricial ObtemIndicesVetoriais(UmaSequenciaID sequencia, Escopo escopo, string nomeDoObjeto)
        {
            Vetor vetor = escopo.tabela.GetVetores().Find(k => k.nome == nomeDoObjeto);
            List<Expressao> expressoesDaequencia = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
            ExpressaoOperadorMatricial indicesMatriciais = null;
         
            foreach (Expressao expressaoComando in expressoesDaequencia)
                if (expressaoComando.GetType() == typeof(ExpressaoOperadorMatricial))
                    return (ExpressaoOperadorMatricial)indicesMatriciais;

            return null;
        }

        protected bool ValidaTokensDaSintaxeDaInstrucao(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSignalEquals = sequencia.original.IndexOf("=");
            if (indexSignalEquals == -1)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "criacao de objeto sem variavel para recebe-lo.");
                return false;
            } // if


            int indexParenteses = sequencia.original.IndexOf("(");
            if (indexParenteses == -1)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Erro na instrucao Create.");
                return false;
            } // if
            return true;
        } // ValidaTokensDaSintaxeDaInstrucao()

     
        public static  int FoundACompatibleConstructor(string tipoObjeto, List<Expressao> parametros)
        {
            List<Funcao> construtores = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoObjeto).construtores;
            int contadorConstrutores = 0;
            foreach (Funcao umConstrutor in construtores)
            {
                bool isFoundConstrutor = true;
                foreach (Expressao umParametro in parametros)
                {
                    foreach (Objeto parametroDoConstrutor in umConstrutor.parametrosDaFuncao)
                    {
                        string tipoParametroCasting = UtilTokens.Casting(umParametro.tipo);
                        string tipoParametroConstrutorCasting = UtilTokens.Casting(parametroDoConstrutor.GetTipo());

                        if (tipoParametroCasting != tipoParametroConstrutorCasting) 
                        {
                            isFoundConstrutor = false;
                            break;
                        } // if
                    } // foreach parametroConstrutor

                    if (isFoundConstrutor)
                        return contadorConstrutores;
                    else
                        if (!isFoundConstrutor)
                        break;
                    
                } // foreach Expressao
                contadorConstrutores++;

            } //foreach Funcao

            return -1;
        }

     

        protected Instrucao BuildInstrucaoSetVar(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // SetVar ( ID , ID)

            if ((sequencia == null) || (sequencia.original.Count == 0))
                return null;
            if (sequencia.original[0] != "SetVar")
                return null;
            string nomeVar = sequencia.original[2];
            string valorVar = sequencia.original[4];
            
            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
                return null;
            Expressao expressaoNumero = new Expressao(new string[] { valorVar }, escopo);
            ProcessadorDeID.SetValorNumero(v, expressaoNumero, escopo);

            Expressao exprss = new Expressao(sequencia.original.ToArray(), escopo);
            escopo.tabela.GetExpressoes().Add(exprss);

            Instrucao instrucaoSet = new Instrucao(ProgramaEmVM.codeSetObjeto, new List<Expressao>() { exprss }, new List<List<Instrucao>>());
            return instrucaoSet;
        }


        protected Instrucao BuildInstrucaoGetObjeto(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // ID GetObjeto ( ID )

            if ((sequencia == null) || (sequencia.original.Count == 0))
                return null;
            if (sequencia.original[1] != "GetObjeto")
                return null;
            string nomeVar = sequencia.original[3];
            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
                return null;

            Expressao exprss = new Expressao(sequencia.original.ToArray(), escopo);
            escopo.tabela.GetExpressoes().Add(exprss);

            Instrucao instrucaoGet = new Instrucao(ProgramaEmVM.codeGetObjeto, new List<Expressao>() { exprss }, new List<List<Instrucao>>());
            return instrucaoGet;
        }

        protected Instrucao BuildInstrucaoWhile(UmaSequenciaID sequencia, Escopo escopo)
        {
            List<Expressao> expressoesWhile = null;
            /// while (exprss) bloco .
            if (sequencia.original[0] == "while")
            {
                List<string> tokensInstrucao = sequencia.original.ToList<string>();
                tokensInstrucao.RemoveAt(0);
                expressoesWhile = Expressao.Instance.ExtraiExpressoes(escopo, tokensInstrucao);

                if ((expressoesWhile == null) || (expressoesWhile.Count == 0))
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na expressão de controle da instrução while. ");
                    return null;
                }

                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesWhile[0], escopo))   // valida se a expressão contém um operador operacional.
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro , expressâo, não condicional: " + expressoesWhile[0].Convert());
                    return null;
                }

                 escopo.tabela.GetExpressoes().AddRange(expressoesWhile); // registra a expressão na lista de expressões do escopo currente.
  
                Instrucao instrucaoWhile = new Instrucao(ProgramaEmVM.codeWhile, new List<Expressao>() { expressoesWhile[0] }, new List<List<Instrucao>>());
                BuildBloco(0, sequencia.original, ref escopo, instrucaoWhile); // constroi as instruções contidas num bloco.
            
                return instrucaoWhile;

            } // if
            return null;
        } // InstrucaoWhileSemBloco()

        protected Instrucao BuildInstrucaoFor(UmaSequenciaID sequencia, Escopo escopo)
        {

            // for (int x=0; x<3; x++){ }; 
            if ((sequencia.original[0] == "for") && (sequencia.original.IndexOf("(") != -1))
            {



                List<string> tokensDaInstrucao = sequencia.original.ToList<string>();
                tokensDaInstrucao.RemoveAt(0); // remove o termo-chave: "for"


                List<string> tokensExpressoes = UtilTokens.GetCodigoEntreOperadores(0, "(", ")", tokensDaInstrucao);
                tokensExpressoes.RemoveAt(0);
                tokensExpressoes.RemoveAt(tokensExpressoes.Count - 1);

                bool hasProcessedVAriableLoop = false;
                List<Expressao> expressoesDaInstrucaoFor = Expressao.Instance.ExtraiExpressoes(escopo, tokensExpressoes);

                if ((expressoesDaInstrucaoFor == null) || (expressoesDaInstrucaoFor.Count == 0)) 
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "expressoes da instrucao for incorreto ou faltante.");
                    return null;
                }

                if (expressoesDaInstrucaoFor.Count == 1)
                {
                    // houve um nao processamento de todas expressoes, pois a instanciacao da  variavel de malha esta entre as expressoes. Faz
                    // o processamento da instanciacao da variavel de malha, e extrai as expressoes novamente.
                    ProcessadorDeID processadorVariavelDaMalha = new ProcessadorDeID(new List<string>() { expressoesDaInstrucaoFor[0].ToString() });
                    processadorVariavelDaMalha.Compile();
                    expressoesDaInstrucaoFor = Expressao.Instance.ExtraiExpressoes(escopo, tokensExpressoes);
                }



                if (RepositorioDeClassesOO.Instance().ObtemUmaClasse( expressoesDaInstrucaoFor[0].Elementos[0].ToString())!=null)
                {
                    // se a Objeto malha for definida na instrucao for, extrai a Objeto e adiciona no escopo esta Objeto.
                    // as expressoes posteriorees da instrucao for utilizam esta Objeto, ela já foi registrada.
                    Classe tipoDaObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(expressoesDaInstrucaoFor[0].Elementos[0].ToString());
                    string nomeObjeto = expressoesDaInstrucaoFor[0].Elementos[1].ToString();
                    object valorObjeto = expressoesDaInstrucaoFor[0].Elementos[3].ToString();

                    escopo.tabela.GetObjetos().Add(new Objeto("private", tipoDaObjeto.GetNome(), nomeObjeto, valorObjeto));
                    hasProcessedVAriableLoop = true;
                }

                if (!hasProcessedVAriableLoop)
                {
                    // processa a inicializacao da variavel de malha, se necessario.
                    ProcessadorDeID processadorVariavelDeMalha = new ProcessadorDeID(new List<string>() { expressoesDaInstrucaoFor[0].ToString() });
                    processadorVariavelDeMalha.Compile();
                    if ((processadorVariavelDeMalha.GetInstrucoes() != null) && (processadorVariavelDeMalha.GetInstrucoes().Count > 0))
                    {
                        List<Objeto> variaveisDaMalha = processadorVariavelDeMalha.escopo.tabela.GetObjetos();
                        for (int x = 0; x < variaveisDaMalha.Count; x++)
                            if (escopo.tabela.GetObjeto(variaveisDaMalha[x].GetNome(), escopo) == null)
                                escopo.tabela.GetObjetos().Add(variaveisDaMalha[x]);

                    } // if
                } // if

                if (!Expressao.Instance.IsExpressionAtibuicao(expressoesDaInstrucaoFor[0])) // valida a expressao de atribuicao.
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na expressão de atribuição for. ");


                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesDaInstrucaoFor[1], escopo)) // valida a expressão de controle condicional.
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "expressão condicional: " + sequencia.expressoes[1].ToString() + " de uma instrução for não válida. ");


                if (!Expressao.Instance.isExpressionAritmetico(expressoesDaInstrucaoFor[2], escopo)) // valida a expressão de incremento/decremento.
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "o tipo da expressão: " + sequencia.expressoes[2].Convert() + " da instrução de incremento for não é do tipo inteiro.");


                // registra as expressões no escopo.
                for (int x = 0; x < 3; x++)
                    escopo.tabela.GetExpressoes().Add(expressoesDaInstrucaoFor[x]);

                Instrucao instrucaoFor = null;
                int offsetIndexBloco = sequencia.original.FindIndex(k => k == "{"); // calcula se há um token de operador bloco abre.
                if (offsetIndexBloco == -1)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "instrução for precisa de um bloco de instruções, entre os operadores { e }.");
                    return null;
                }
                else
                {

                    instrucaoFor = new Instrucao(ProgramaEmVM.codeFor, expressoesDaInstrucaoFor, new List<List<Instrucao>>()); // cria a instrucao for principal.
                    BuildBloco(0, sequencia.original, ref escopo, instrucaoFor); // adiciona as instruções do bloco.
    
                    instrucaoFor.expressoes = new List<Expressao>();

                    instrucaoFor.expressoes.Add(expressoesDaInstrucaoFor[0]);
                    instrucaoFor.expressoes.Add(expressoesDaInstrucaoFor[1]);
                    instrucaoFor.expressoes.Add(expressoesDaInstrucaoFor[2]);
                   

                    return instrucaoFor;

                } //if
            } // if
            return null;
        } // InstrucaoWhileSemBloco()

        protected Instrucao BuildInstrucaoIFsComOuSemElse(UmaSequenciaID sequencia, Escopo escopo)
        {

            /// while (exprss) {} .
            if (sequencia.original[0] == "if")
            {
                List<string> tokensDeExpressoes = UtilTokens.GetCodigoEntreOperadores(sequencia.original.IndexOf("("), "(", ")", sequencia.original);

                tokensDeExpressoes.RemoveAt(0);
                tokensDeExpressoes.RemoveAt(tokensDeExpressoes.Count - 1);

                List<Expressao> expressoesIf = Expressao.Instance.ExtraiExpressoes(escopo, tokensDeExpressoes);


                if ((expressoesIf == null) || (expressoesIf.Count == 0)) // valida se há expressões validas para a instrução.
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro de sintaxe da instrução if. ");
                    return null;
                }

                if ((expressoesIf[0] == null) || (expressoesIf[0].Elementos.Count == 0)) // valida a expressão de atribuição da instrução "if".
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na validão de elementos da expressão, pode haver termos sem tipos válidos, ou operadores não válidos para o tipo da expressãp. ");
                    return null;
                }


                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesIf[0], escopo))   // valida se a expressão contém um operador operacional.
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro em instruçao if, a expressão não é condicional: " + Util.UtilString.UneLinhasLista(expressoesIf[0].Convert()));
                    return null;
                }



                escopo.tabela.GetExpressoes().AddRange(expressoesIf); // adiciona a expressão de atribuição na tabela de valores do escopo currente, para fins de otimização.

              


                int offsetBlocoIf = sequencia.original.IndexOf("{"); // offset para o primeiro token de bloco.
                if (offsetBlocoIf == -1)
                    return null; // se não for uma instrução com bloco, é uma instrução sem bloco, retornando null, pois a instrucao nao foi construida.




                int offsetBlocoElse = sequencia.original.IndexOf("{", offsetBlocoIf + 1); // verifica se a instrução else tem um bloco de instruções.

                if (offsetBlocoElse == -1) // instrução if sem bloco de uma instrução else.
                {

                    Instrucao instrucaoIfSemElse = new Instrucao(ProgramaEmVM.codeIfElse, expressoesIf, new List<List<Instrucao>>());
         
                    BuildBloco(0, sequencia.original, ref escopo, instrucaoIfSemElse);
                  
                    return instrucaoIfSemElse; // ok , é um comando if sem instrução else.
                } // if
                else // instrução if com bloco de uma instrução else.
                {
                   
                    Instrucao instrucaoElse = new Instrucao(ProgramaEmVM.codeIfElse, expressoesIf, new List<List<Instrucao>>());
    
                    // constroi o bloco da instrução else.
                    BuildBloco(0, sequencia.original, ref escopo, instrucaoElse);
                    BuildBloco(1, sequencia.original, ref escopo, instrucaoElse);
                    return instrucaoElse;
                } // else
            } // if
            return null;
        } // BuildInstrucaoIFsComOuSemElse

        protected Instrucao BuildInstrucaoCasesOfUse(UmaSequenciaID sequencia, Escopo escopo)
        {

            // template: casesOfUse ID ( case  ID_operador  ID : ".
            int iCabecalho = sequencia.original.IndexOf("(");
            if (iCabecalho == -1)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro de sintaxe para a instrução casesOfUse");
                return null;
            }

            // obtem as listas de cases, cada um contendo o bloco de um item case.
            List<List<string>> listaDeCases = UtilTokens.GetCodigoEntreOperadoresCases("(", ")", sequencia.original);

            string nomeObjetoPrincipal = sequencia.original[1];  // obtem a variavel principal, e valida.
            Objeto vMain = escopo.tabela.GetObjeto(nomeObjetoPrincipal, escopo);
            if (vMain == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "variavel principal: " + nomeObjetoPrincipal + " não definida.");
                return null;
            } // if


            List<Expressao> expressoesDeCadaCase = new List<Expressao>();
            expressoesDeCadaCase.Add(new ExpressaoElemento(nomeObjetoPrincipal)); // adiciona a lista de expressões o nome da variável principal.


            List<List<Instrucao>> blocoDeInstrucoesCase = new List<List<Instrucao>>(); // inicializa as listas de blocos de instrução.

            // percorre as listas, calculando: 1- a expressão condicional do case, 2- o bloco de instruções para o case.
            for (int UM_CASE = 0; UM_CASE < listaDeCases.Count; UM_CASE++)
            {

                /// forma o cabeçalho de um case._______________________________________________________________
                int indexCabecalhoCase = listaDeCases[UM_CASE].IndexOf("case");
                string nameOperationWhithVar = listaDeCases[UM_CASE][indexCabecalhoCase + 1];
                string nameVarCase = listaDeCases[UM_CASE][indexCabecalhoCase + 2];
                //______________________________________________________________________________________________
                object numero = null;
                object str_string = null;

                Objeto vCase = escopo.tabela.GetObjeto(nameVarCase, escopo); // tenta obter uma variavel, de um caso da instrução.

                if (vCase == null)
                    ObtemNumeroOuTextoDeControle(nameVarCase, ref numero, ref str_string, ref vCase);
                else
                if (vCase.GetTipo() != vMain.GetTipo()) // valida o tipo da variavel principal e tipo da variavel do case.
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "variavel principal: " + nomeObjetoPrincipal + " e variavel de case: " + nameVarCase + " sao de tipos diferentes.");
                    return null;
                }
                else
                if (Operador.GetOperador(nameOperationWhithVar, vMain.GetTipo(),"BINARIO", linguagem) == null) // valida o operador da expressao.
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "operador para tipo da expressao nao encontrado.");
                    return null;
                }

                // forma a operação condicional, entre a variável principal e a variável do case,
                // que pode ser ou uma variavel, ou numero, ou string.
                List<string> cabecalhoDoCase = new List<string>();


                if (numero != null)
                    cabecalhoDoCase.AddRange(new List<string>() { nomeObjetoPrincipal, nameOperationWhithVar, (string)numero });
                else
                if (str_string != null)
                    cabecalhoDoCase.AddRange(new List<string>() { nomeObjetoPrincipal, nameOperationWhithVar, (string)str_string });
                else
                if (vCase != null)
                    cabecalhoDoCase.AddRange(new List<string>() { nomeObjetoPrincipal, nameOperationWhithVar, vCase.GetNome() });
          
                
                Expressao exprssaoCabecalho = new Expressao(cabecalhoDoCase.ToArray(), escopo); // forma um bloco case.

                
                if (exprssaoCabecalho == null)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Erro na montagem da expressão do case: " + nameVarCase + "  numa instrução de casesOfUse");
                    return null;
                } // if
                escopo.tabela.GetExpressoes().Add(exprssaoCabecalho);
                if (!Expressao.Instance.ValidaExpressaoCondicional(exprssaoCabecalho, escopo))
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, " expressao nao condificional para o case: " + nameVarCase);
                    return null;
                }  // if

                expressoesDeCadaCase.Add(exprssaoCabecalho); // adiciona a expressão de cálculo de um case: pode ser "==", ">", "<" !!!

                int indexInicioCase = listaDeCases[UM_CASE].IndexOf(":");
                if (indexInicioCase == -1)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "formacao de um bloco case com sintaxe incorreta. Falta o token :");
                    return null;
                }

                List<string> blocoCase = listaDeCases[UM_CASE].ToList<string>(); // obtém o bloco do case currente.
                blocoCase.RemoveAt(0); // prepara para processamento.
                blocoCase.RemoveAt(blocoCase.Count - 1);

                int indexStartAsInstrucoes = blocoCase.IndexOf(":");


                blocoCase.RemoveRange(0, indexStartAsInstrucoes + 1);


                ProcessadorDeID processador = new ProcessadorDeID(blocoCase);
                processador.escopo = escopo.Clone(); // repassa o escopo contendo as variaveis, instrucoes, objetos, extraidos até aqui.
                processador.Compile(); // compila o bloco case, para conseguir as instruções do bloco.


                List<Instrucao> instrucoesDeUmBlocoCase = processador.GetInstrucoes(); // obtem as instruções do bloco case.
                blocoDeInstrucoesCase.Add(instrucoesDeUmBlocoCase);

            } //  for x

            Instrucao instrucaoCase = new Instrucao(ProgramaEmVM.codeCasesOfUse, expressoesDeCadaCase, blocoDeInstrucoesCase);
            return instrucaoCase;


        } // BuildInstrucaoCasesOfUse(()

        private static void ObtemNumeroOuTextoDeControle(string nameVarCase, ref object numero, ref object str_string, ref Objeto objetoCase)
        {
            // é o caso em que a variavel do case é um numero, ou string: caseOfUse a { case == 1: x++; case  < 5: y++;}
            if (Expressao.Instance.IsTipoInteiro(nameVarCase))
            {
                numero = int.Parse(nameVarCase);
                objetoCase = new Objeto("private", "int", "varCaseInt", numero);
            }
            else
            if (Expressao.Instance.IsTipoFloat(nameVarCase))
            {
                numero = float.Parse(nameVarCase);
                objetoCase = new Objeto("private", "float", "varCaseFloat", numero);
            }
            else
            if (Expressao.Instance.IsTipoDouble(nameVarCase))
            {
                numero = float.Parse(nameVarCase);
                objetoCase = new Objeto("private", "double", "varCaseFloat", numero);
            }
            else

            if (linguagem.VerificaSeEString(nameVarCase))
            {
                str_string = nameVarCase;
                objetoCase = new Objeto("private", "string", "varCaseString", str_string);
            }
        }

        protected void BuildBloco(int numeroDoBloco,List<string> tokens, ref Escopo escopo, Instrucao instrucaoPrincipal)
        {
            if ((!tokens.Contains("{")) || (!tokens.Contains("}")))
                return;

            int indexStart = 0;
            int offsetStart = 0;
            for (int x = 0; x <= numeroDoBloco; x++) 
            {
                indexStart = tokens.IndexOf("{", offsetStart);
                if (indexStart == -1)
                    break;


                List<string> blocoAnterior = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);
                offsetStart = blocoAnterior.Count;

            }

            List<string> bloco = UtilTokens.GetCodigoEntreOperadores(indexStart, "{", "}", tokens);

            
            bloco.RemoveAt(0); // remove os operadores bloco dos tokens do bloco.
            bloco.RemoveAt(bloco.Count - 1);

            Escopo escopoBloco = new Escopo(bloco);

            ProcessadorDeID processadorBloco = new ProcessadorDeID(bloco); 
            processadorBloco.escopo.tabela = TablelaDeValores.Clone(escopo.tabela); // copia a tabela de valores do escopo currente.
            processadorBloco.Compile(); // faz a compilacao do bloco.

            escopo.tabela = TablelaDeValores.Clone(processadorBloco.escopo.tabela);

            List<Instrucao> instrucoesBLOCO = processadorBloco.GetInstrucoes();
            instrucaoPrincipal.blocos.Add(instrucoesBLOCO);
       
        }


        protected Instrucao BuildInstrucaoBreak(UmaSequenciaID sequencia, Escopo escopo)
        {
            Instrucao instrucaoBreak= new Instrucao(ProgramaEmVM.codeBreak, new List<Expressao>(), new List<List<Instrucao>>());
            return instrucaoBreak;
        }

        protected Instrucao BuildInstrucaoContinue(UmaSequenciaID sequencia, Escopo escopo)
        {
            Instrucao instrucaoContinue=new Instrucao(ProgramaEmVM.codeContinue, new List<Expressao>(), new List<List<Instrucao>>());
            return instrucaoContinue;
        }

        protected Instrucao BuildInstrucaoReturn(UmaSequenciaID sequencia, Escopo escopo)
        {

            List<string> tokensExpressoes = sequencia.original.ToList<string>();
            tokensExpressoes.RemoveAt(0); // retira o nome da instrucao: token "return", para compor o corpo da expressão.

            List<Expressao> exprssRetorno = Expressao.Instance.ExtraiExpressoes(escopo, tokensExpressoes);


            if ((exprssRetorno == null) || (exprssRetorno.Count == 0)) 
            {
                Instrucao instrucaoRetornoSemExpressao = new Instrucao(ProgramaEmVM.codeReturn, new List<Expressao>(), new List<List<Instrucao>>());
                return instrucaoRetornoSemExpressao;
            }
            else
            {
                escopo.tabela.GetExpressoes().Add(exprssRetorno[0]);

                Instrucao instrucaoReturn = new Instrucao(ProgramaEmVM.codeReturn, new List<Expressao>() { exprssRetorno[0] }, new List<List<Instrucao>>());
                return instrucaoReturn;
            }
        }

        // gera mensagem de erros configurável.
        protected static void GeraMensagemDeErroEmUmaInstrucao(UmaSequenciaID sequencia, Escopo escopo, string mensagem)
        {
            PosicaoECodigo posicaoDoErro = new PosicaoECodigo(sequencia.original, escopo.codigo);
            escopo.GetMsgErros().Add(mensagem + "  na linha: " + posicaoDoErro.linha + ", coluna: " + posicaoDoErro.coluna);
        }

    } // class
} // namespace
