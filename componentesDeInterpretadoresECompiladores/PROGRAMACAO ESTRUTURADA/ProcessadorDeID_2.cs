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
                linguagem = LinguagemOrquidea.Instance();
            this.codigo = codigo.ToList<string>();
            this.escopo = new Escopo(codigo);
        }

  
        protected Instrucao BuildInstrucaoImporter(UmaSequenciaID sequencia, Escopo escopo)
        {
            // importer ( nomeAssembly).
            if (!sequencia.tokens.Contains("importer"))
                return null;
            if (sequencia.tokens.Count < 3)
                return null;
            List<string> tokensInstrucao = sequencia.tokens.ToList<string>(); // faz uma copia dos tokens, para evitar problemas no metodo CompileEscopos()

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


        protected Instrucao BuildInstrucaoConstrutorUP(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// template= classeHerdeira.construtorUP(nomeClasseHerdada, List<Expressao> parametrosDoConstrutor).
            /// pode ser o objeto "actual";

            string nomeClasseHedeira = sequencia.tokens[0];
            string nomeClasseHerdada = sequencia.tokens[4];

            if (RepositorioDeClassesOO.Instance().GetClasse(nomeClasseHedeira) == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "classe herdeira da instrucao nao existe.", sequencia.tokens);
                return null;
            }
            if (RepositorioDeClassesOO.Instance().GetClasse(nomeClasseHerdada) == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "classe herdada da instrucao nao existe", sequencia.tokens);
                return null;
            }



            int indexStartParametros = sequencia.tokens.IndexOf("(");
            if (indexStartParametros == 1)
            {
                UtilTokens.WriteAErrorMensage(escopo, "erro de sintaxe para instrucao: contrutorUp", sequencia.tokens);
                return null;
            }

            List<string> tokensParametros = sequencia.tokens.GetRange(indexStartParametros, sequencia.tokens.Count - indexStartParametros);
            List<Expressao> expressoesParametros = null;

            if ((tokensParametros != null)  && (tokensParametros.Count>0))
            {
                tokensParametros.Remove(";");
                tokensParametros.Remove(","); // remove da lista de parâmetros, o primeiro token ",", pois faz parte da especificação do token da classe herdada.
                tokensParametros.RemoveAt(0);
                tokensParametros.RemoveAt(tokensParametros.Count - 1);
          
                
                tokensParametros.Remove(nomeClasseHerdada);  // remove da lista de parâmetros, o token do nome da classe herdada..
               

                expressoesParametros = Expressao.Instance.ExtraiExpressoes(escopo, tokensParametros);
                if (expressoesParametros == null)
                {
                    UtilTokens.WriteAErrorMensage(escopo, "erro na definicao das expressoes de parametros da instrucao construtorUp", sequencia.tokens);
                    return null;
                }
            }
            else
                expressoesParametros = new List<Expressao>(); // sem parametros para passar ao construtor.

            int indexConstrutor = FoundACompatibleConstructor(nomeClasseHerdada, expressoesParametros);
            if (indexConstrutor < 0)
            {
                UtilTokens.WriteAErrorMensage(escopo, "nao encontrado um construtor da classe herdada, para as expressoes parametros de um construtor.", sequencia.tokens);
                return null;
            }


            Expressao pacoteParametros = new Expressao();
            pacoteParametros.Elementos.AddRange(expressoesParametros);

            Expressao expressaoCabecalho = new Expressao();
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(nomeClasseHedeira));
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(nomeClasseHerdada));
            expressaoCabecalho.Elementos.Add(new ExpressaoElemento(indexConstrutor.ToString()));
            expressaoCabecalho.Elementos.Add(pacoteParametros);

            Instrucao instrucaoConstrutorUP = new Instrucao(ProgramaEmVM.codeConstructorUp, expressaoCabecalho.Elementos, new List<List<Instrucao>>());
            return instrucaoConstrutorUP;
        }


        /// <summary>
        /// Cria um novo objeto. pode criar um objeto simples, ou um objeto de variavel vetor.
        /// </summary>
        /// Estrutura de dados na lista de expressoes:
        /// 0- id "create".
        /// 1- tipo do objeto.
        /// 2- nome do objeto.
        /// 3- reservado.
        /// 4- Lista de expressoes-indice para objetos vetor.
        /// 5- lista de expressoes parametros, para o construtor.
        protected Instrucao BuildInstrucaoCreate(UmaSequenciaID sequencia, Escopo escopo)
        {

         
            string tipoDoObjetoAReceberAInstantiacao = "";
            string nomeDoObjetoAReceberAInstanciacao = "";

            /// ID ID = create ( ID , ID ) --> exemplo: int m= create(1,1).
            if (RepositorioDeClassesOO.Instance().GetClasse(sequencia.tokens[0]) != null)
            {
                tipoDoObjetoAReceberAInstantiacao = sequencia.tokens[0].ToString();
                nomeDoObjetoAReceberAInstanciacao = sequencia.tokens[1];

            }

            else
            {
                nomeDoObjetoAReceberAInstanciacao = sequencia.tokens[0];
                Objeto objetoJaInicializado = escopo.tabela.GetObjeto(nomeDoObjetoAReceberAInstanciacao, escopo);
                if (objetoJaInicializado != null)
                    tipoDoObjetoAReceberAInstantiacao = objetoJaInicializado.GetTipo();
                else
                if (objetoJaInicializado == null)
                {
                    Vetor vetorJaInicializado = escopo.tabela.GetVetor(nomeDoObjetoAReceberAInstanciacao, escopo);
                    if (vetorJaInicializado == null)
                    {
                        UtilTokens.WriteAErrorMensage(escopo, "objeto nao definido anteriormente, numa expressao que requer objeto definido anteriormente", sequencia.tokens);
                        return null;
                    }
                    else
                        tipoDoObjetoAReceberAInstantiacao = vetorJaInicializado.GetTiposElemento();

                }
            }

            
            ValidaTokensDaSintaxeDaInstrucao(sequencia, escopo);

            ExpressaoOperadorMatricial expressoesIndicesVetor = new ExpressaoOperadorMatricial();
            if (tipoDoObjetoAReceberAInstantiacao == "vetor")
                expressoesIndicesVetor= ObtemIndicesVetoriais(sequencia, escopo, nomeDoObjetoAReceberAInstanciacao);



            int indexFirstParenteses = sequencia.tokens.IndexOf("(");
            List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(indexFirstParenteses, "(", ")", sequencia.tokens);
            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);

            if (tipoDoObjetoAReceberAInstantiacao == "Vetor")
                tokensParametros.RemoveRange(0, 2); // retira o tipo de um elemento do vetor, mais a virgula delimitadora depois do tipo de elemento.



            List<Expressao> expressoesParametros = Expressao.Instance.ExtraiExpressoes(escopo, tokensParametros);
          
                
            if ((expressoesParametros == null) || (expressoesParametros[0].Elementos == null) || (expressoesParametros[0].Elementos.Count == 0))
                expressoesParametros = new List<Expressao>();

        

            int indexConstrutor=FoundACompatibleConstructor(tipoDoObjetoAReceberAInstantiacao, expressoesParametros);
            if (indexConstrutor < 0)
            {
                UtilTokens.WriteAErrorMensage(escopo, "Nao encotrado um construtor compativel. ", sequencia.tokens);
                return null;
            }

            Expressao exprssDaIntrucao = new Expressao();
            Expressao exprssParametros = new Expressao();
            exprssParametros.Elementos.AddRange(expressoesParametros);


            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("create"));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(tipoDoObjetoAReceberAInstantiacao));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(nomeDoObjetoAReceberAInstanciacao));
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento("Objeto"));
            exprssDaIntrucao.Elementos.Add(expressoesIndicesVetor);
            exprssDaIntrucao.Elementos.Add(exprssParametros);
            exprssDaIntrucao.Elementos.Add(new ExpressaoElemento(indexConstrutor.ToString()));

            
            
            
            if (tipoDoObjetoAReceberAInstantiacao == "Vetor")
            {
                // tipo da variavel: vetor.
                List<int> indicesVetor = new List<int>();
                string tipoDosElementosDoVetor = sequencia.tokens[5]; // o primeiro elemento do create é reservado para o tipo do elemento do vetor, se for um vetor.
                EvalExpression eval = new EvalExpression();
                
                try
                {


                    for (int tokenIndice = 0; tokenIndice < expressoesParametros.Count; tokenIndice++)
                    {
                        object umIndice = eval.EvalPosOrdem(expressoesParametros[tokenIndice], escopo);
                        if (umIndice != null)
                            indicesVetor.Add(int.Parse(umIndice.ToString()));

                    }
                }
                catch
                {
                    UtilTokens.WriteAErrorMensage(escopo, "Os indices das dimensoes da variavel vetor a ser criado: " + nomeDoObjetoAReceberAInstanciacao + " devem ser apenas numeros inteiros.", sequencia.tokens);
                    return null;
                }

                escopo.tabela.GetVetores().Add(new Vetor("private", nomeDoObjetoAReceberAInstanciacao, tipoDosElementosDoVetor, escopo, indicesVetor.ToArray())); // adiciona a variavel vetor criada, para a compilação das próximas instruções, em outros builds.
                exprssDaIntrucao.Elementos[3] = new ExpressaoElemento("Vetor");

                   
            }
            else
            {
                // tipo da variável: Objeto.
                Classe classeDoObjeto = RepositorioDeClassesOO.Instance().GetClasse(tipoDoObjetoAReceberAInstantiacao);

                if (escopo.tabela.GetObjeto(nomeDoObjetoAReceberAInstanciacao, escopo) == null) // se o objeto não já foi criado, instancia o objeto, no escopo.
                    escopo.tabela.GetObjetos().Add(new Objeto("private", tipoDoObjetoAReceberAInstantiacao, nomeDoObjetoAReceberAInstanciacao, null));
            }
            escopo.tabela.GetExpressoes().Add(exprssDaIntrucao); // registra as expressões, a fim de otimização de modificação.

            // cria a instrucao do objeto.
            Instrucao instrucaoCreate = new Instrucao(ProgramaEmVM.codeCreateObject, new List<Expressao>() { exprssDaIntrucao }, new List<List<Instrucao>>());
            return instrucaoCreate;

        } // BuildInstrucaoCreate()




        protected ExpressaoOperadorMatricial ObtemIndicesVetoriais(UmaSequenciaID sequencia, Escopo escopo, string nomeDoObjeto)
        {
            Vetor vetor = escopo.tabela.GetVetores().Find(k => k.nome == nomeDoObjeto);
            List<Expressao> expressoesDaequencia = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.tokens);
            ExpressaoOperadorMatricial indicesMatriciais = null;
         
            foreach (Expressao expressaoComando in expressoesDaequencia)
                if (expressaoComando.GetType() == typeof(ExpressaoOperadorMatricial))
                    return (ExpressaoOperadorMatricial)indicesMatriciais;

            return null;
        }

        protected bool ValidaTokensDaSintaxeDaInstrucao(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSignalEquals = sequencia.tokens.IndexOf("=");
            if (indexSignalEquals == -1)
            {
                UtilTokens.WriteAErrorMensage(escopo, "criacao de objeto sem variavel para recebe-lo.", sequencia.tokens);
                return false;
            } // if


            int indexParenteses = sequencia.tokens.IndexOf("(");
            if (indexParenteses == -1)
            {
                UtilTokens.WriteAErrorMensage(escopo, "Erro na instrucao Create.", sequencia.tokens);
                return false;
            } // if
            return true;
        } // ValidaTokensDaSintaxeDaInstrucao()


        public static int FoundACompatibleConstructor(string tipoObjeto, List<Expressao> parametros)
        {
            List<Funcao> construtores = RepositorioDeClassesOO.Instance().GetClasse(tipoObjeto).construtores;
            int contadorConstrutores = 0;

            if ((parametros == null) || (parametros.Count == 0))
            {
                int indexConstrutorSemParametros = construtores.FindIndex(k => k.parametrosDaFuncao == null || k.parametrosDaFuncao.Length == 0);
                return indexConstrutorSemParametros;

            }
            int x_parametros = 0;
            foreach (Funcao umConstrutor in construtores)
            {
                bool isFoundConstrutor = true;
                if (umConstrutor.parametrosDaFuncao == null)
                {
                    if ((parametros == null) || (parametros.Count == 0) || (parametros[x_parametros].Elementos.Count == 0))
                        return contadorConstrutores;
                }
      
                for (int x = 0; x < parametros.Count; x++)
                { 
                    
                
                    foreach (Objeto parametroDoConstrutor in umConstrutor.parametrosDaFuncao)
                    {
                        string tipoParametroCasting = UtilTokens.Casting(parametros[x].ToString());
                        if (Expressao.Instance.IsTipoInteiro(parametros[x].ToString()))
                            tipoParametroCasting = "int";
                        else
                        if (Expressao.Instance.IsTipoFloat(parametros[x].ToString()))
                            tipoParametroCasting = "float";
                        else
                        if (Expressao.Instance.IsTipoDouble(parametros[x].ToString()))
                            tipoParametroCasting = "double";
                     
                        
                        
                        string tipoParametroConstrutorCasting = UtilTokens.Casting(parametroDoConstrutor.GetTipo());

                        if ((tipoParametroCasting == "float") && (tipoParametroConstrutorCasting == "double"))
                            continue;
                        if ((tipoParametroCasting == "double") && (tipoParametroCasting == "float"))
                            continue;

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

            if ((sequencia == null) || (sequencia.tokens.Count == 0))
                return null;
            if (sequencia.tokens[0] != "SetVar")
                return null;
            string nomeVar = sequencia.tokens[2];
            string valorVar = sequencia.tokens[4];
            
            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
                return null;
            Expressao expressaoNumero = new Expressao(new string[] { valorVar }, escopo);
            ProcessadorDeID.SetValorNumero(v, expressaoNumero, escopo);

            Expressao exprss = new Expressao(sequencia.tokens.ToArray(), escopo);
            escopo.tabela.GetExpressoes().Add(exprss);

            Instrucao instrucaoSet = new Instrucao(ProgramaEmVM.codeSetObjeto, new List<Expressao>() { exprss }, new List<List<Instrucao>>());
            return instrucaoSet;
        }


        protected Instrucao BuildInstrucaoGetObjeto(UmaSequenciaID sequencia, Escopo escopo)
        {
            // template: 
            // ID GetObjeto ( ID )

            if ((sequencia == null) || (sequencia.tokens.Count == 0))
                return null;
            if (sequencia.tokens[1] != "GetObjeto")
                return null;
            string nomeVar = sequencia.tokens[3];
            Objeto v = escopo.tabela.GetObjeto(nomeVar, escopo);
            if (v == null)
                return null;

            Expressao exprss = new Expressao(sequencia.tokens.ToArray(), escopo);
            escopo.tabela.GetExpressoes().Add(exprss);

            Instrucao instrucaoGet = new Instrucao(ProgramaEmVM.codeGetObjeto, new List<Expressao>() { exprss }, new List<List<Instrucao>>());
            return instrucaoGet;
        }

        protected Instrucao BuildInstrucaoWhile(UmaSequenciaID sequencia, Escopo escopo)
        {
            List<Expressao> expressoesWhile = null;
            /// while (exprss) bloco .
            if (sequencia.tokens[0] == "while")
            {
                List<string> tokensInstrucao = sequencia.tokens.ToList<string>();
                tokensInstrucao.RemoveAt(0); // retira o termo-chave "while".


                tokensInstrucao.RemoveAt(0); // retira o parenteses abre.
                tokensInstrucao.RemoveAt(tokensInstrucao.Count - 1); // retira o parenteses fecha.
                expressoesWhile = Expressao.Instance.ExtraiExpressoes(escopo, tokensInstrucao);

                if ((expressoesWhile == null) || (expressoesWhile.Count == 0))
                {
                    UtilTokens.WriteAErrorMensage(escopo, "erro na expressão de controle da instrução while. ", sequencia.tokens);
                    return null;
                }

                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesWhile[0], escopo))   // valida se a expressão contém um operador operacional.
                {

                    UtilTokens.WriteAErrorMensage(escopo, "erro , expressâo, não condicional: " + expressoesWhile[0].Convert(), sequencia.tokens);
                    return null;
                }

                 escopo.tabela.GetExpressoes().AddRange(expressoesWhile); // registra a expressão na lista de expressões do escopo currente.

                ProcessadorDeID processador = null;
                Instrucao instrucaoWhile = new Instrucao(ProgramaEmVM.codeWhile, new List<Expressao>() { expressoesWhile[0] }, new List<List<Instrucao>>());
                BuildBloco(0, sequencia.tokens, ref escopo, instrucaoWhile, ref processador); // constroi as instruções contidas num bloco.
            
                return instrucaoWhile;

            } // if
            return null;
        } // InstrucaoWhileSemBloco()

        protected Instrucao BuildInstrucaoFor(UmaSequenciaID sequencia, Escopo escopo)
        {

            // for (int x=0; x<3; x++){ }; 
            if ((sequencia.tokens[0] == "for") && (sequencia.tokens.IndexOf("(") != -1))
            {



                List<string> tokensDaInstrucao = sequencia.tokens.ToList<string>();
                tokensDaInstrucao.RemoveAt(0); // remove o termo-chave: "for"


                List<string> tokensExpressoes = UtilTokens.GetCodigoEntreOperadores(0, "(", ")", tokensDaInstrucao);
                tokensExpressoes.RemoveAt(0);
                tokensExpressoes.RemoveAt(tokensExpressoes.Count - 1);


              
                Objeto variavelMalha = null;
                int indexClasseVariavelMalha = sequencia.tokens.IndexOf("=");
                if ((indexClasseVariavelMalha - 2) >= 0) // verifica se a variavel de malha é definida dentro da instrução for.
                {
                    Classe classe = RepositorioDeClassesOO.Instance().GetClasse(sequencia.tokens[indexClasseVariavelMalha - 2]);
                    if (classe != null) 
                    {
                        string tipoDaVariavelMalha = classe.nome;
                        string nomeVariavelMalha = sequencia.tokens[indexClasseVariavelMalha - 1]; // nome da variavel de malha.
                        string valorVariavelMalha = sequencia.tokens[indexClasseVariavelMalha + 1]; // consegue o valor inicial da variavel de malha.
                        variavelMalha = new Objeto("private", tipoDaVariavelMalha, nomeVariavelMalha, valorVariavelMalha);

                        escopo.tabela.GetObjetos().Add(variavelMalha);
                    }
                }
                List<Expressao> expressoesDaInstrucaoFor = Expressao.Instance.ExtraiExpressoes(escopo, tokensExpressoes);

                if ((expressoesDaInstrucaoFor == null) || (expressoesDaInstrucaoFor.Count == 0)) 
                {
                    UtilTokens.WriteAErrorMensage(escopo, "expressoes da instrucao for incorreto ou faltante.", sequencia.tokens);
                    return null;
                }

                if (expressoesDaInstrucaoFor.Count == 1)
                {
                    // houve um nao processamento de todas expressoes, pois a instanciacao da  variavel de malha esta entre as expressoes. Faz
                    // o processamento da instanciacao da variavel de malha, e extrai as expressoes novamente.
                    ProcessadorDeID processadorVariavelDaMalha = new ProcessadorDeID(new List<string>() { expressoesDaInstrucaoFor[0].ToString() });
                    processadorVariavelDaMalha.CompileEmDoisEstagios();
                    expressoesDaInstrucaoFor = Expressao.Instance.ExtraiExpressoes(escopo, tokensExpressoes);
                }



                if (RepositorioDeClassesOO.Instance().GetClasse( expressoesDaInstrucaoFor[0].Elementos[0].ToString())!=null)
                {
                    // se a Objeto malha for definida na instrucao for, extrai a Objeto e adiciona no escopo esta Objeto.
                    // as expressoes posteriorees da instrucao for utilizam esta Objeto, ela já foi registrada.
                    Classe tipoDaObjeto = RepositorioDeClassesOO.Instance().GetClasse(expressoesDaInstrucaoFor[0].Elementos[0].ToString());
                    string nomeObjeto = expressoesDaInstrucaoFor[0].Elementos[1].ToString();
                    object valorObjeto = expressoesDaInstrucaoFor[0].Elementos[3].ToString();

                    escopo.tabela.GetObjetos().Add(new Objeto("private", tipoDaObjeto.GetNome(), nomeObjeto, valorObjeto));
                }
                if (!Expressao.Instance.IsExpressionAtibuicao(expressoesDaInstrucaoFor[0])) // valida a expressao de atribuicao.
                    UtilTokens.WriteAErrorMensage(escopo, "erro na expressão de atribuição for. ", sequencia.tokens);


                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesDaInstrucaoFor[1], escopo)) // valida a expressão de controle condicional.
                    UtilTokens.WriteAErrorMensage(escopo, "expressão condicional: " + sequencia.expressoes[1].ToString() + " de uma instrução for não válida. ", sequencia.tokens);


                if (!Expressao.Instance.isExpressionAritmetico(expressoesDaInstrucaoFor[2], escopo)) // valida a expressão de incremento/decremento.
                    UtilTokens.WriteAErrorMensage(escopo, "o tipo da expressão: " + sequencia.expressoes[2].Convert() + " da instrução de incremento for não é do tipo inteiro.", sequencia.tokens);
         

                // registra as expressões no escopo.
                for (int x = 0; x < 3; x++)
                    escopo.tabela.GetExpressoes().Add(expressoesDaInstrucaoFor[x]);

                Instrucao instrucaoFor = null;
                int offsetIndexBloco = sequencia.tokens.FindIndex(k => k == "{"); // calcula se há um token de operador bloco abre.
                if (offsetIndexBloco == -1)
                {
                    UtilTokens.WriteAErrorMensage(escopo, "instrução for precisa de um bloco de instruções, entre os operadores { e }.", sequencia.tokens);
                    return null;
                }
                else
                {
                    ProcessadorDeID processador = null;
                    instrucaoFor = new Instrucao(ProgramaEmVM.codeFor, expressoesDaInstrucaoFor, new List<List<Instrucao>>()); // cria a instrucao for principal.
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoFor, ref processador); // adiciona as instruções do bloco.
    
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
            if (sequencia.tokens[0] == "if")
            {
                List<string> tokensDeExpressoes = UtilTokens.GetCodigoEntreOperadores(sequencia.tokens.IndexOf("("), "(", ")", sequencia.tokens);

                tokensDeExpressoes.RemoveAt(0);
                tokensDeExpressoes.RemoveAt(tokensDeExpressoes.Count - 1);

                List<Expressao> expressoesIf = Expressao.Instance.ExtraiExpressoes(escopo, tokensDeExpressoes);


                if ((expressoesIf == null) || (expressoesIf.Count == 0)) // valida se há expressões validas para a instrução.
                {

                    UtilTokens.WriteAErrorMensage(escopo, "erro de sintaxe da instrução if. ", sequencia.tokens);
                    return null;
                }

                if ((expressoesIf[0] == null) || (expressoesIf[0].Elementos.Count == 0)) // valida a expressão de atribuição da instrução "if".
                {
                    UtilTokens.WriteAErrorMensage(escopo, "erro na validão de elementos da expressão, pode haver termos sem tipos válidos, ou operadores não válidos para o tipo da expressão. ", sequencia.tokens);   
                    return null;
                }


                if (!Expressao.Instance.ValidaExpressaoCondicional(expressoesIf[0], escopo))   // valida se a expressão contém um operador operacional.
                {
                    UtilTokens.WriteAErrorMensage(escopo, "erro em instruçao if, a expressão não é condicional: " + Util.UtilString.UneLinhasLista(expressoesIf[0].Convert()), sequencia.tokens);
                    return null;
                }



                escopo.tabela.GetExpressoes().AddRange(expressoesIf); // adiciona a expressão de atribuição na tabela de valores do escopo currente, para fins de otimização.

              


                int offsetBlocoIf = sequencia.tokens.IndexOf("{"); // offset para o primeiro token de bloco.
                if (offsetBlocoIf == -1)
                    return null; // se não for uma instrução com bloco, é uma instrução sem bloco, retornando null, pois a instrucao nao foi construida.



                ProcessadorDeID processador = null;

                int offsetBlocoElse = sequencia.tokens.IndexOf("{", offsetBlocoIf + 1); // verifica se a instrução else tem um bloco de instruções.

                if (offsetBlocoElse == -1) // instrução if sem bloco de uma instrução else.
                {

                    Instrucao instrucaoIfSemElse = new Instrucao(ProgramaEmVM.codeIfElse, expressoesIf, new List<List<Instrucao>>());
         
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoIfSemElse, ref processador);
                  
                    return instrucaoIfSemElse; // ok , é um comando if sem instrução else.
                } // if
                else // instrução if com bloco de uma instrução else.
                {
                   
                    Instrucao instrucaoElse = new Instrucao(ProgramaEmVM.codeIfElse, expressoesIf, new List<List<Instrucao>>());
    
                    // constroi o bloco da instrução else.
                    BuildBloco(0, sequencia.tokens, ref escopo, instrucaoElse, ref processador);
                    BuildBloco(1, sequencia.tokens, ref escopo, instrucaoElse, ref processador);
                    return instrucaoElse;
                } // else
            } // if
            return null;
        } // BuildInstrucaoIFsComOuSemElse

        protected Instrucao BuildInstrucaoCasesOfUse(UmaSequenciaID sequencia, Escopo escopo)
        {

            // template: casesOfUse ID ( case  ID_operador  ID : ".
            int iCabecalho = sequencia.tokens.IndexOf("(");
            if (iCabecalho == -1)
            {
                UtilTokens.WriteAErrorMensage(escopo, "erro de sintaxe para a instrução casesOfUse", sequencia.tokens);
                return null;
            }

            // obtem as listas de cases, cada um contendo o bloco de um item case.
            List<List<string>> listaDeCases = UtilTokens.GetCodigoEntreOperadoresCases("(", ")", sequencia.tokens);

            string nomeObjetoPrincipal = sequencia.tokens[1];  // obtem a variavel principal, e valida.
            Objeto vMain = escopo.tabela.GetObjeto(nomeObjetoPrincipal, escopo);
            if (vMain == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "variavel principal: " + nomeObjetoPrincipal + " não definida.", sequencia.tokens);
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
                    ObtemNumeroOuTextoDeControle(nameVarCase, ref numero, ref str_string, ref vCase, escopo);
                else
                if (vCase.GetTipo() != vMain.GetTipo()) // valida o tipo da variavel principal e tipo da variavel do case.
                {
                    UtilTokens.WriteAErrorMensage(escopo, "variavel principal: " + nomeObjetoPrincipal + " e variavel de case: " + nameVarCase + " sao de tipos diferentes.", sequencia.tokens);
                    return null;
                }
                else
                if (Operador.GetOperador(nameOperationWhithVar, vMain.GetTipo(), "BINARIO", linguagem) == null) // valida o operador da expressao.
                {
                    UtilTokens.WriteAErrorMensage(escopo, "operador para tipo da expressao nao encontrado.", sequencia.tokens);
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
                    UtilTokens.WriteAErrorMensage(escopo, "Erro na montagem da expressão do case: " + nameVarCase + "  numa instrução de casesOfUse", sequencia.tokens);
                    return null;
                } // if
                escopo.tabela.GetExpressoes().Add(exprssaoCabecalho);
                if (!Expressao.Instance.ValidaExpressaoCondicional(exprssaoCabecalho, escopo))
                {
                    UtilTokens.WriteAErrorMensage(escopo, " expressao nao condificional para o case: " + nameVarCase, sequencia.tokens);
                    return null;
                }  // if

                expressoesDeCadaCase.Add(exprssaoCabecalho); // adiciona a expressão de cálculo de um case: pode ser "==", ">", "<" !!!

                int indexInicioCase = listaDeCases[UM_CASE].IndexOf(":");
                if (indexInicioCase == -1)
                {
                    UtilTokens.WriteAErrorMensage(escopo, "formacao de um bloco case com sintaxe incorreta. Falta o token :", sequencia.tokens);
                    return null;
                }

                List<string> blocoCase = listaDeCases[UM_CASE].ToList<string>(); // obtém o bloco do case currente.
                blocoCase.RemoveAt(0); // prepara para processamento.
                blocoCase.RemoveAt(blocoCase.Count - 1);

                int indexStartAsInstrucoes = blocoCase.IndexOf(":");


                blocoCase.RemoveRange(0, indexStartAsInstrucoes + 1);


                ProcessadorDeID processador = new ProcessadorDeID(blocoCase);
                processador.escopo = escopo.Clone(); // repassa o escopo contendo as variaveis, instrucoes, objetos, extraidos até aqui.
                processador.CompileEmDoisEstagios(); // compila o bloco case, para conseguir as instruções do bloco.


                List<Instrucao> instrucoesDeUmBlocoCase = processador.GetInstrucoes(); // obtem as instruções do bloco case.
                blocoDeInstrucoesCase.Add(instrucoesDeUmBlocoCase);

            } //  for x

            Instrucao instrucaoCase = new Instrucao(ProgramaEmVM.codeCasesOfUse, expressoesDeCadaCase, blocoDeInstrucoesCase);
            return instrucaoCase;


        } // BuildInstrucaoCasesOfUse(()

        private static void ObtemNumeroOuTextoDeControle(string nameVarCase, ref object numero, ref object str_string, ref Objeto objetoCase, Escopo escopo)
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

        protected void BuildBloco(int numeroDoBloco,List<string> tokens, ref Escopo escopo, Instrucao instrucaoPrincipal, ref ProcessadorDeID processadorBloco)
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

            processadorBloco = new ProcessadorDeID(bloco); 
            processadorBloco.escopo.tabela = TablelaDeValores.Clone(escopo.tabela); // copia a tabela de valores do escopo currente.

            escopo.escopoFolhas.Add(processadorBloco.escopo); // faz o escopo do bloco como escopo folha do escopo da instrucao, ou função, ou escopo principal.
            processadorBloco.CompileEmDoisEstagios(); // faz a compilacao do bloco.

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

            List<string> tokensExpressoes = sequencia.tokens.ToList<string>();
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
        //protected static void GeraMensagemDeErroEmUmaInstrucao(UmaSequenciaID sequencia, Escopo escopo, string mensagem)
        //{

         //   UtilTokens.WriteAErrorMensage(escopo, mensagem, sequencia.original);

        //}

    } // class

 
} // namespace
