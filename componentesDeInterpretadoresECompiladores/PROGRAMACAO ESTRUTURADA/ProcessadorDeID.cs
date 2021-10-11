using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using parser;
using Util;
using parser.ProgramacaoOrentadaAObjetos;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices.WindowsRuntime;

namespace parser
{

    /// <summary>
    /// sistema de determinação de sequências IDs, que são partes no código, e que são necessárias na construção de um escopo.
    /// </summary>
    public class ProcessadorDeID : BuildInstrucoes
    {

        private  List<string> codigoTotal { get; set; }
       
        public  List<string> codigo { get; set; }

        // linguagem contendo classes, métodos, propriedades, funções, variáveis, e operadores.
        private static LinguagemOrquidea linguagem { get; set; }

        public Escopo escopo { get; set; }

        // assinatura de uma função processadora de sequencias ID.
        public delegate Instrucao MetodoTratador(UmaSequenciaID sequencia, Escopo escopo);

        // lista de métodos tratadores para ordenação.
        private static List<MetodoTratadorOrdenacao> tratadores;


        // lista de instruções construidas na compilação.
        private List<Instrucao> instrucoes = new List<Instrucao>();

        private static List<List<string>> sequenciasJaMapeadas = new List<List<string>>(); // guarda as sequencias já mapeadas e resumidas.


        private static List<string> acessorsValidos = new List<string>() { "public", "private", "protected" };

        /// <summary>
        /// guarda os tokens que não são resumiveis, no processamento de match de sequencias do codigo e sequencias resumidas.
        /// </summary>
        public static List<string> tokensSemResumir { get; set; }

        public enum TipoAtribuicao { INICIALIZACAO, MODIFICACAO };

        public enum TipoPropriedadeAtribuicao { OBJETO, VARIAVEL, VARIAVEL_VETOR, PROPRIEDADE };

  
    
        public List<Instrucao> GetInstrucoes()
        {
            return this.instrucoes;
        }

       

        /// <summary>
        /// construtor. Extrai e constroi instruções para serem consumidas em um programaVM.
        /// O codigo de um escopo é unido ao codigo de outros escopos antereriores.
        /// </summary>
        public ProcessadorDeID(List<string> code) : base(code)
        {

            

            if (tratadores == null)
                tratadores = new List<MetodoTratadorOrdenacao>();
            if (linguagem == null)
                linguagem = LinguagemOrquidea.Instance();

            this.instrucoes = new List<Instrucao>();
            codigo = new List<string>();  
            codigo.AddRange(code);
            this.escopo = new Escopo(code); // obtém um escopo para o processador de sequencias ID.

            this.InitMapeamento(); // inicializa as sequencias id mapeadas resumidas.

        } // ProcessadorDeID()


        public static void LoadHandler(MetodoTratador umTratador, string sequenciaID)
        {
            tratadores.Add(new MetodoTratadorOrdenacao(umTratador, sequenciaID));
        } //LoadHandler()

        private void InitMapeamento()
        {
            if ((tratadores == null) || (tratadores.Count == 0))
            {
                //__________________________________________________________________________________________________________________________________
                //          DEFINIÇÃO DE SEQUENCIAS ID.
                /// Lembrando que um ID pode ser um único elemento ou uma expressão, que é um conjunto de elementos.

                // definição sequências ID Estruturadas.
                string str_chamadaAFuncaoSemRetornoESemParametros = "ID ( )";
                string str_definicaoDeVariavelComAtribuicaoDeChamadaDeFuncao = "ID ID = ID (";
                string str_definicaoDeVariavelComAtribuicaoDeChamadaDeMetodo = "ID ID = ID. ID (";
                string str_definicaoComTipoDefinidoEOperadorUnarioPosOrdem = "OP ID";
                string str_definicaoComTipoDefinidoEOperadorUnarioPreOrdem = "ID OP";
                string str_definicaoVariavelNaoEstaticaSemAtribuicao = "ID ID ;";

                string str_definicaoVariavelVetor = "vector ID (ID, dims)";


                // definicao de funções estruturadas, e defnição de funções.
                string str_definicaoDeFuncaoComRetornoComUmOuMaisParametrosComCorpoOuSemCorpo = "ID ID ( ID ID";
                string str_definicaoDeFuncaoComRetornoSemParametrosSemCorpo = "ID ID ( ) ;";
                string str_defnicaoDeFuncaoComRetornoSemParametrosComCorpo = "ID ID ( )";

                // definição sequências POO.
                string str_inicializacaoPropriedadeEstaticaComAtribuicao = "ID static ID ID = ID";
                string str_inicializacaoPropriedadeEstaticaSemAtribuicao = "ID static ID ID";

                // definicao de sequencias POO.
                string str_inicializacaoPropriedadeEAtribuicao = "ID ID ID = ID ;";
                string str_inicializacaoPropriedadeSemAtribuicao = "ID ID ID ;";


                // sequencias de propriedades/chamada de metodos, aninhados.
                string str_propriedadesEncadeadasComOuSemAtribuicao = "ID . ID  =";
                string str_chamadaAMetodoSemParametros = "ID . ID ( ";
                string str_chamadaDeMetodoComAtribuicao = "ID ID = ID . ID (";
                string str_chamadaDeMetodoComAtribuicaoSemInicializacao = "ID = ID . ID (";

                // definicao de metodos POO.
                string str_definicaoDeMetodoComParametrosComCorpoOuSemCorpo = "ID ID ID ( ID ";
                string str_definicaoMetodoSemParametrosComOuSemCorpo = "ID ID ID ( )";


                string str_chamadaAFuncaoComParametrosESemRetorno = "ID ( ID";
                

                // sequencias de instruções da linguagem.


                // instrucao create
                string str_CreateNewObject = "ID ID = create (";
                string str_CreateNewObjectSemInicializacao= "ID = create (";

                // instrucao construtorUp
                string str_ConstrutorUp = "ID . construtorUP ( ID"; 


                // seuencias de interapolidade.
                string str_Importer = "importer ( ID . ID ) ;";

                string DefinicaoInstrucaoWhileComBlocoOuSemBloco = "while ( ID ) BLOCO";
                string DefinicaoInstrucaoForComBlocoOuSemBloco = "for ( ID = ID ; ID ; ID ) BLOCO";
                string DefinicaoInstrucaoForComBlocoOuSemBlocoComAtribuicao = "for ( ID ID = ID ; ID ; ID ) BLOCO";
                string DefinicaoInstrucaoIfComBlocoOuSemBlocoSemElse = "if ( ID )";
                string DefinicaoInstrucaoIfComBlocoComElse = "if ( ID ) else";

                string DefinicaoDeOperadorBinario = "operador ID ID ( ID ID , ID ID ) prioridade ID metodo ID ;";
                string DefinicaoDeOperadorUnario = "operador ID ID ( ID ID ) prioridade ID metodo ID ;";

                string DefinicaoInstrucaoBreak = "continue";
                string DefinicaoInstrucaoContinue = "break";
                string DefinicaoInstrucaoReturn = "return ID ;";

                string DefinicaoInstrucaoSetVar = "SetVar ( ID )";
                string DefinicaoInstrucaoGetObjeto = "ID GetObjeto ( ID )";


                
                string AtribuicaoDeVariavelSemDefinicao = "ID = ID ;";

                string str_definicaoDeVariavelNaoEsttaticaComAtribuicao = "ID ID = ID ;";
                string DefinicaoInstrucaoCasesOfUse = "casesOfUse ID : ( case ID ID :";


                //____________________________________________________________________________________________________________________________
                // CARREGA OS METODO TRATADORES E AS SEQUENCIAS DE ID ASSOCIADAS.

                // SEQUENCIAS OPERACAO
                LoadHandler(OperacaoUnarioPosOrder, str_definicaoComTipoDefinidoEOperadorUnarioPosOrdem);
                LoadHandler(OperacaoUnarioPreOrder, str_definicaoComTipoDefinidoEOperadorUnarioPreOrdem);

                // SEQUENCIAS ESTRUTURADAS.
                LoadHandler(Atribuicao, str_definicaoVariavelNaoEstaticaSemAtribuicao);
                LoadHandler(Atribuicao, str_definicaoDeVariavelComAtribuicaoDeChamadaDeFuncao);
             
                LoadHandler(Atribuicao, str_definicaoDeVariavelNaoEsttaticaComAtribuicao);
                LoadHandler(Atribuicao, str_chamadaAFuncaoSemRetornoESemParametros);

            
                // propriedades estaticas.
                LoadHandler(AtribuicaoEstatica, str_inicializacaoPropriedadeEstaticaComAtribuicao);
                LoadHandler(AtribuicaoEstatica, str_inicializacaoPropriedadeEstaticaSemAtribuicao);
                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeAninhadas, str_propriedadesEncadeadasComOuSemAtribuicao);


                // propriedaedes nao estaticas.
                LoadHandler(Atribuicao, str_inicializacaoPropriedadeEAtribuicao);
                LoadHandler(Atribuicao, str_inicializacaoPropriedadeSemAtribuicao);





                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeAninhadas, str_definicaoDeVariavelComAtribuicaoDeChamadaDeMetodo);
                LoadHandler(ChamadaMetodo, str_chamadaAMetodoSemParametros);
                LoadHandler(ChamadaFuncao, str_chamadaAFuncaoComParametrosESemRetorno);

                LoadHandler(ChamadaDeMetodoComAtribuicao, str_chamadaDeMetodoComAtribuicao);
                LoadHandler(ChamadaDeMetodoComAtribuicao, str_chamadaDeMetodoComAtribuicaoSemInicializacao);

                // SEQUENCIAS INTEROPABILIDADE:
                LoadHandler(BuildInstrucaoImporter, str_Importer);

                // sequencias definição de métodos.
                LoadHandler(BuildDefinicaoDeMetodo, str_definicaoDeMetodoComParametrosComCorpoOuSemCorpo);
                LoadHandler(BuildDefinicaoDeMetodo, str_definicaoMetodoSemParametrosComOuSemCorpo);

                // sequencias definicao de funcoes.
                LoadHandler(BuildDefinicaoDeFuncao, str_definicaoDeFuncaoComRetornoComUmOuMaisParametrosComCorpoOuSemCorpo);
                LoadHandler(BuildDefinicaoDeFuncao, str_definicaoDeFuncaoComRetornoSemParametrosSemCorpo);
                LoadHandler(BuildDefinicaoDeFuncao, str_defnicaoDeFuncaoComRetornoSemParametrosComCorpo);

                // sequencias de instruções da linguagem,
                LoadHandler(BuildInstrucaoWhile, DefinicaoInstrucaoWhileComBlocoOuSemBloco);
                LoadHandler(BuildInstrucaoFor, DefinicaoInstrucaoForComBlocoOuSemBloco);
                LoadHandler(BuildInstrucaoFor, DefinicaoInstrucaoForComBlocoOuSemBlocoComAtribuicao);
                LoadHandler(BuildInstrucaoIFsComOuSemElse, DefinicaoInstrucaoIfComBlocoOuSemBlocoSemElse);
                LoadHandler(BuildInstrucaoIFsComOuSemElse, DefinicaoInstrucaoIfComBlocoComElse);
                LoadHandler(BuildInstrucaoBreak, DefinicaoInstrucaoBreak);
                LoadHandler(BuildInstrucaoContinue, DefinicaoInstrucaoContinue);
                LoadHandler(BuildInstrucaoReturn, DefinicaoInstrucaoReturn);

                LoadHandler(BuildInstrucaoCreate, str_CreateNewObject);
                LoadHandler(BuildInstrucaoCreate, str_CreateNewObjectSemInicializacao);

                LoadHandler(Atribuicao, AtribuicaoDeVariavelSemDefinicao);


                LoadHandler(BuildVariavelVetor, str_definicaoVariavelVetor);
                LoadHandler(BuildInstrucaoGetObjeto, DefinicaoInstrucaoGetObjeto);
                LoadHandler(BuildInstrucaoSetVar, DefinicaoInstrucaoSetVar);
                LoadHandler(BuildInstrucaoOperadorBinario, DefinicaoDeOperadorBinario);
                LoadHandler(BuildInstrucaoOperadorUnario, DefinicaoDeOperadorUnario);
                LoadHandler(BuildInstrucaoCasesOfUse, DefinicaoInstrucaoCasesOfUse);
                LoadHandler(BuildInstrucaoConstrutorUP, str_ConstrutorUp);


                ProcessadorDeID.tokensSemResumir = new List<string>(); // tokens presentes nas sequencia de definicoes, que nao sejam ID.
                                                                       // utilizado no metodo de Resumir Expessoes.

                // ordena a lista de métodos tratadores, pelo cumprimento de seus testes de sequencias ID.            
                ProcessadorDeID.MetodoTratadorOrdenacao.ComparerMetodosTratador comparer = new MetodoTratadorOrdenacao.ComparerMetodosTratador();
                tratadores.Sort(comparer);

                if (sequenciasJaMapeadas.Count == 0) // resume todas sequencias já mapeadas, otimizando o método MatchSequencias().
                {
                    sequenciasJaMapeadas = new List<List<string>>();
                    for (int umHandler = 0; umHandler < tratadores.Count; umHandler++) // varre as sequencias já mapeadas, procurando um match entre sequencias já mapeadas, e a sequencia de entrada resumida.
                    {
                        sequenciasJaMapeadas.Add(new List<string>());
                        List<string> tokensDaSequenciaMapeada = new Tokens(linguagem, new List<string>() { tratadores[umHandler].sequenciaID }).GetTokens();
                        sequenciasJaMapeadas[umHandler].AddRange(tokensDaSequenciaMapeada);

                        foreach (string umTokenAResumir in tokensDaSequenciaMapeada)
                        {
                            if (!umTokenAResumir.Equals("ID"))
                            {
                                if (tokensSemResumir.FindIndex(k => k.Equals(umTokenAResumir)) == -1) // retira token currente que ja esta na lista de tokens.
                                    tokensSemResumir.Add(umTokenAResumir);
                            }
                        }
                    } // for mHandler
                } // if

                // retira oa acessor, pois sao resumiveis, e termos-chave.
                tokensSemResumir.RemoveAll(k => k.Equals("public"));
                tokensSemResumir.RemoveAll(k => k.Equals("private"));
                tokensSemResumir.RemoveAll(k => k.Equals("protected"));

                tokensSemResumir.Add("{"); // adiciona os operadores bloco, que não estavam na lista de tokens nao resumiveis, por nas sequencias id de mapeamento estarem resumidas pelo token "BLOCO".
                tokensSemResumir.Add("}");


    

            } // if tratadores

        } // InitMapeamento()


        public void Compile()
        {
            List<string> tokens = new Tokens(linguagem, codigo).GetTokens();
            this.CompileEscopos(this.escopo, tokens); // primeira compilação. pode haver erros de uso de codigo antes da definição deste código.


            this.instrucoes.Clear();
           
            
            this.CompileEscopos(this.escopo, tokens); // segunda compilação, para erros de posição de um metodo/proriedade estiverem o uso antes  da definicao.


            this.EliminaRedundanciasCausadosPeloSistemeDeRecuperacao(this.escopo);
            AtualizaOtimizacaoDeExpressoes(this.instrucoes);

        }

        /// <summary>
        /// compila o codigo, saida no objeto escopo desta classe.
        /// </summary>
        public void CompileEmDoisEstagios()
        {

            try
            {
                
                List<string> tokens = new Tokens(linguagem, codigo).GetTokens();
                this.CompileEscopos(this.escopo, tokens);
            }

            catch (Exception excecao)
            {
                ModuloTESTES.LoggerTests.AddMessage("Erro no processamento de tokens. Stack: " + excecao.StackTrace);
            }
         
        }

        /// <summary>
        /// faz a compilacao de um escopo.
        /// </summary>
        internal void CompileEscopos(Escopo escopo, List<string> tokens)
        {
            
      
            int umToken = 0;
            while (umToken < tokens.Count)
            {

                if (((umToken + 1) < tokens.Count) &&
                    (acessorsValidos.Find(k => k.Equals(tokens[umToken])) != null) &&
                    ((tokens[umToken + 1].Equals("class")) ||
                    (tokens[umToken + 1].Equals("interface")))) 
                { 
                    try
                    {
                        string classeOuInterface = tokens[umToken + 1];
                        if (classeOuInterface == "interface")
                        {

                            List<string> tokensDaInterface= UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);


                            ExtratoresOO extratorDeClasses = new ExtratoresOO(escopo, linguagem, tokensDaInterface);
                            Classe umaInterface = extratorDeClasses.ExtraiUmaInterface();
                            if (extratorDeClasses.MsgErros.Count > 0)
                                this.escopo.GetMsgErros().AddRange(extratorDeClasses.MsgErros);

                            if (umaInterface != null)
                            {

                                umToken += umaInterface.tokensDaClasse.Count;
                                continue;
                            }

                        }
                        else
                        if (classeOuInterface == "class")
                        {

                

                            List<string> tokensDaClasse = UtilTokens.GetCodigoEntreOperadores(umToken, "{", "}", tokens);
                            ExtratoresOO extratorDeClasses = new ExtratoresOO(escopo, linguagem, tokensDaClasse);


                            Classe umaClasse = extratorDeClasses.ExtaiUmaClasse(Classe.tipoBluePrint.EH_CLASSE);

                            if (umaClasse != null)
                            {
                                umToken += umaClasse.tokensDaClasse.Count; // +1 do acessor (public, private, protected).

                                if (extratorDeClasses.MsgErros.Count > 0)
                                    this.escopo.GetMsgErros().AddRange(extratorDeClasses.MsgErros); // retransmite erros na extracao da classe, para a mensagem de erros do escopo.
                                continue;

                            } // if

                        }
                    }
                    catch
                    {
                        UtilTokens.WriteAErrorMensage(escopo, "Erro na extracao de tokens de uma classe ou interface, verifique a sintaxe das especificacoes de classe.", tokens);
                        return;
                    }
                }
                else
                if (linguagem.VerificaSeEhID(tokens[umToken]) || (linguagem.isTermoChave(tokens[umToken])))
                {

                    
                    UmaSequenciaID sequenciaCurrente = UmaSequenciaID.ObtemUmaSequenciaID(umToken, tokens, codigo); // obtem a sequencia  seguinte.

                

                    if (sequenciaCurrente == null)
                        UtilTokens.WriteAErrorMensage(escopo, "sequencia de tokens não reconhecida: " + sequenciaCurrente.ToString(), sequenciaCurrente.tokens);  // continua o processamento, para verificar se há mais erros no codigo orquidea.
           
                    MatchSequencias(sequenciaCurrente, escopo); // obtém o indice de metodo tratador.

                    if (sequenciaCurrente.indexHandler == -1)
                    {
                        Instrucao instrucaoExpressaoCorreta = BuildExpressaoValida(sequenciaCurrente, escopo); // a sequencia id pode ser uma expressao correta, há build para expressoes corretas.
                        if (instrucaoExpressaoCorreta != null)
                        {
                            this.instrucoes.Add(instrucaoExpressaoCorreta); // a sequencia id é uma expressao correta, processa e adiciona a instrucao de expressao correta.
                            umToken += sequenciaCurrente.tokens.Count;
                            continue;
                        }

                        // trata de problemas de sintaxe da sequencia currente, emitindo uma mensagem de erro.
                        UtilTokens.WriteAErrorMensage(escopo, "sequencia de tokens não reconhecida: ", sequenciaCurrente.tokens);
                    
                        umToken += sequenciaCurrente.tokens.Count;
                        continue;  // continua, para capturar mais erros em outras sequencias currente.   
                    }
                    else
                    {
                        
                        try
                        {
                            // chamada do método tratador para processar a costrução de escopos, da 0sequencia de entrada.
                            Instrucao instrucaoTratada = tratadores[sequenciaCurrente.indexHandler].metodo(sequenciaCurrente, escopo);
                            if (instrucaoTratada != null)
                                this.instrucoes.Add(instrucaoTratada);
                            else
                            {
                                Instrucao instrucaoExpressaoCorreta = BuildExpressaoValida(sequenciaCurrente, escopo); // a sequencia id pode ser uma expressao correta, há build para expressoes corretas.
                                if (instrucaoExpressaoCorreta != null)
                                {
                                    this.instrucoes.Add(instrucaoExpressaoCorreta); // a sequencia id é uma expressao correta, processa e adiciona a instrucao de expressao correta.
                                    umToken += sequenciaCurrente.tokens.Count;
                                    continue;
                                }
                                else
                                    UtilTokens.WriteAErrorMensage(escopo, "sequencia de tokens: " + sequenciaCurrente + " nao reconhecida, verifique a sintaxe, incluindo operadores ponto-e-virgula.", sequenciaCurrente.tokens);
                            }
                            umToken += sequenciaCurrente.tokens.Count; // atualiza o iterator de tokens, consumindo os tokens que foram utilizados no processamento da seuencia id currente.
                            continue;

                        }
                        catch
                        {
                            UtilTokens.WriteAErrorMensage(escopo, "Erro de compilacao da sequencia: " + sequenciaCurrente.ToString() + ". Verifique a sintaxe, com terminos de fim de instrucao, tambem. ", tokens);
                            return;
                        }
                    } // if tokensSequenciaOriginais.Count>0

                } // if linguagem.VerificaSeEID()
                umToken++;
            } // while

        }// CompileEcopos()
      

        private void AtualizaOtimizacaoDeExpressoes(List<Instrucao> instrucoes)
        {
            if (this.instrucoes == null)
                return;

            for (int x = 0; x < instrucoes.Count; x++)
            {
                if (instrucoes[x].expressoes != null)
                {
                    for (int exprss = 0; exprss < instrucoes[x].expressoes.Count; exprss++)
                    {
                        Expressao expressao = instrucoes[x].expressoes[exprss];
                        List<Objeto> objetosDestaExpressao = ExtraiObjetosDeExpressao(expressao);
                        if (objetosDestaExpressao == null)
                            continue;
                        else
                            for (int obj = 0; obj < objetosDestaExpressao.Count; obj++)
                                if (objetosDestaExpressao[obj] != null)
                                    objetosDestaExpressao[obj].GetExpressoesComOObjeto().Add(expressao);

                    }

                    if (instrucoes[x].blocos != null)
                        for (int b = 0; b < instrucoes[x].blocos.Count; b++)
                            if ((instrucoes[x] != null) && (instrucoes[x].blocos != null) && (instrucoes[x].blocos[b].Count > 0))
                                AtualizaOtimizacaoDeExpressoes(instrucoes[x].blocos[b]);

                }
            }
        }

        private List<Objeto> ExtraiObjetosDeExpressao(Expressao umaExpressao)
        {
            List<Objeto> objetosPresentes = new List<Objeto>();
            for (int x = 0; x < umaExpressao.Elementos.Count; x++)
            {
                if (umaExpressao.Elementos[x] != null)
                {
                    if (umaExpressao.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                        objetosPresentes.Add(((ExpressaoObjeto)umaExpressao.Elementos[x]).objeto);
                    if (umaExpressao.Elementos[x].GetType() == typeof(ExpressaoVetor))
                        objetosPresentes.Add(((ExpressaoVetor)umaExpressao).vetor);
                }
            }
            return objetosPresentes;

        }

       
        /// <summary>
        /// encontra indices de métodos tratadores para a sequencia ID de entrada.
        /// </summary>
        private void MatchSequencias(UmaSequenciaID sequencia, Escopo escopo)
        {

            if (tokensSemResumir.FindIndex(k => k.Equals(sequencia.tokens[0])) != -1)
            {

                for (int seqMapeada = 0; seqMapeada < tratadores.Count; seqMapeada++)
                {
                    string[] tokensTratafores = tratadores[seqMapeada].sequenciaID.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToArray<string>();

                    if (tokensTratafores[0] == sequencia.tokens[0])
                    {
                        sequencia.indexHandler = seqMapeada;
                        MatchBlocos(sequencia, escopo);
                        return;
                    }
                } // seqMapeada
            }
            else
            {


                List<string> AMapear = ResumeExpressoes(sequencia.tokens, ProcessadorDeID.tokensSemResumir);

                for (int seqMapeada = 0; seqMapeada < tratadores.Count; seqMapeada++) // varre as sequencias já mapeadas, procurando um match entre sequencias já mapeadas, e a sequencia de entrada resumida.
                {

                    if (isEqualsExpressoes(AMapear, sequenciasJaMapeadas[seqMapeada]))
                    {
                        sequencia.indexHandler = seqMapeada; // encontrou a sequencia mapeada.

                        // procura bloco de tokens na sequencia de entrada. insere o  bloco encontrado para a sequencia.
                        MatchBlocos(sequencia, escopo);
                        return;

                    } // if

                } // for seqMapeada
                  // adiciona o indice -1, pois não encontrou uma sequencia mapeada, nem expressão complexa.
                sequencia.indexHandler = -1;
            }

        } // MatchSequenciasMapeadas()



        /// Faz um  resumo das expressõoes, convertendo para ID: operadores, ids, numeros, e tokens componentes de sequencias id mapeados.
        private List<string> ResumeExpressoes(List<string> tokensExpressao, List<string> tokensNaoResumiveis)
        {
            List<string> aResumir = new List<string>();
            int umToken = 0;
           

            while ((umToken >= 0) && (umToken < tokensExpressao.Count))
            {
                // adiciona o token se estiver na lista de tokens nao resumiveis, ou se o token for um operador, ou se for o operador de ids: @.
                if (IsTokensPresentesEmDefinicoes(tokensExpressao[umToken], tokensNaoResumiveis))
                    aResumir.Add(tokensExpressao[umToken]);
                else
                {
                    if (!linguagem.isOperador(tokensExpressao[umToken]))
                        aResumir.Add("ID");
                    else
                    if (tokensExpressao[umToken].Contains("@"))
                        aResumir.Add("ID");
                    else
                        aResumir.Add(tokensExpressao[umToken]);
                }
                umToken++;
            }

            SimplificarUmaExpressaoSemTermosChave(aResumir, tokensNaoResumiveis);

            return aResumir;
        } // ResumeTodasExprss()


        /// <summary>
        /// a ideia deste método é identificar blocos, e colocar os tokens de blocos, na sequencia de entrada.
        /// </summary>
        private static void MatchBlocos(UmaSequenciaID sequencia, Escopo escopo)
        {
            int indexSearchBlocks = sequencia.tokens.IndexOf("{");

            while (indexSearchBlocks != -1)
            {
                // retira um bloco a partir dos tokens sem modificações (originais).
                List<string> umBloco = UtilTokens.GetCodigoEntreOperadores(indexSearchBlocks, "{", "}", sequencia.tokens);
                // encontrou um bloco de tokens, adiciona à sequencia de entrada.
                if ((umBloco != null) && (umBloco.Count > 0))
                {
                    umBloco.RemoveAt(0);
                    umBloco.RemoveAt(umBloco.Count - 1);
                    sequencia.sequenciasDeBlocos.Add(new List<UmaSequenciaID>() { new UmaSequenciaID(umBloco.ToArray(), escopo.codigo) });
                } // if
                indexSearchBlocks = sequencia.tokens.IndexOf("{", indexSearchBlocks + 1);
            } // while
        } // MatchBlocos()


        private bool IsTokensPresentesEmDefinicoes(string token, List<string> tokensNaoResumiveis)
        {
            return tokensNaoResumiveis.Find(k => k.Equals(token)) != null;
        }

        // resume expressoes em id, unindo "id+operador+id" em um unico "id", se o operador for binario.
        // e resume tb expressoes em id, unindo tb "id+operadorUnario" em "id".
        // e resume tb expressoes em id, unindo "operadorUnario+id".
        // e resume tb expressoes de um token id, para em "id".
        private void SimplificarUmaExpressaoSemTermosChave(List<string> aResumir, List<string> tokensNaoResumiveis)
        {

            int umTokenResumido = 0;
            while ((umTokenResumido >= 0) && (umTokenResumido < aResumir.Count))
            {
                string umToken = aResumir[umTokenResumido];

                if (umToken.Equals("{"))
                {
                    List<string> umBloco = UtilTokens.GetCodigoEntreOperadores(umTokenResumido, "{", "}", aResumir);
                    if ((umBloco != null) && (umBloco.Count > 0))
                    {
                        aResumir.RemoveRange(umTokenResumido, umBloco.Count);
                        aResumir.Insert(umTokenResumido, "BLOCO");
                        umTokenResumido += umBloco.Count;
                        continue;
                    }
                }
                else
                if ((umToken != "ID") && (!IsTokensPresentesEmDefinicoes(umToken, tokensNaoResumiveis)))
                {
                    if (
                        ((umTokenResumido - 1) >= 0) &&
                        ((umTokenResumido + 1) < aResumir.Count) &&
                        (linguagem.IsOperadorBinario(aResumir[umTokenResumido])) &&
                        (aResumir[umTokenResumido - 1] == "ID") &&
                        (aResumir[umTokenResumido + 1] == "ID"))
                    {
                        aResumir.RemoveRange(umTokenResumido - 1, 3);
                        aResumir.Insert(umTokenResumido - 1, "ID");

                        continue;
                    }
                    else
                    if (((umTokenResumido - 1) >= 0) && (linguagem.IsOperadorUnario(umToken)) && (aResumir[umTokenResumido - 1] == "ID"))
                    {
                        aResumir.RemoveRange(umTokenResumido - 1, 2);
                        aResumir.Insert(umTokenResumido - 1, "ID");
                        continue;

                    }
                    else
                    if (((umTokenResumido + 1) < aResumir.Count) && (linguagem.IsOperadorUnario(umToken)) && (aResumir[umTokenResumido + 1] == "ID"))
                    {
                        aResumir.RemoveRange(umTokenResumido, 2);
                        aResumir.Insert(umTokenResumido, "ID");
                        continue;
                    }
                    else
                    if (linguagem.VerificaSeEhID(umToken))
                    {
                        aResumir.RemoveAt(umTokenResumido);
                        aResumir.Insert(umTokenResumido, "ID");
                    }
                }

                umTokenResumido++;
            }
        }

        private bool IsResumivel(string token, List<string> tokensNaoResumiveis)
        {
            return tokensNaoResumiveis.Find(k => k.Equals(token)) == null;
        }
      

        /// <summary>
        /// verifica as expressões Amapear, e JaMapeado são iguais, contando como elementos da expressão de JaMapeado.
        /// </summary>
        private bool isEqualsExpressoes(List<string> AMapear, List<string> JaMapeado)
        {
           

            if ((JaMapeado != null) && (JaMapeado.Count > 0) && (AMapear != null) && (AMapear.Count > 0))
            {
                int lgth = AMapear.Count;

                if (lgth > JaMapeado.Count)
                    lgth = JaMapeado.Count;

                for (int x = 0; x < lgth; x++)
                    if (AMapear[x].Trim(' ') != JaMapeado[x].Trim(' '))
                        return false;
            }
            return true;
        }

        protected Instrucao OperacaoUnarioPreOrder(UmaSequenciaID sequencia, Escopo escopo)
        {
            //   OP. ID

            if ((linguagem.VerificaSeEhID(sequencia.tokens[0]) && (linguagem.VerificaSeEhOperadorUnario(sequencia.tokens[1]))))
            {


                string nomeObjeto = sequencia.tokens[1];
                Objeto v = escopo.tabela.GetObjeto(nomeObjeto, escopo);
                
                if (v == null)
                    UtilTokens.WriteAErrorMensage(escopo, "objeto: " + nomeObjeto + "  inexistente.", sequencia.tokens);
                

                string nomeOperador = sequencia.tokens[0];
                Operador umOperador = Operador.GetOperador(nomeOperador, v.GetTipo(), "UNARIO", linguagem);

                if (umOperador == null)
                    UtilTokens.WriteAErrorMensage(escopo, "operador: " + nomeOperador + "  não é unário.", sequencia.tokens);
        
                
                
                bool isFoundClass = false;
                foreach (Classe umaClasse in escopo.tabela.GetClasses())

                    if (umaClasse.GetNome() == nomeOperador)
                    {
                        isFoundClass = true;
                        break;
                    } //if
                if (!isFoundClass)
                {

                    UtilTokens.WriteAErrorMensage(escopo, "operador: " + nomeObjeto + "  inexistente. linha: ", sequencia.tokens);
                    return null;
                }
                else
                {
                    escopo.tabela.GetExpressoes().Add(new Expressao(new string[] { nomeOperador + " " + nomeObjeto }, escopo));
                    return new Instrucao(ProgramaEmVM.codeOperadorUnario, new List<Expressao>(), new List<List<Instrucao>>());
                }
            } // if
            return null;
        } // OperacaoUnarioPreOrder()

        protected Instrucao OperacaoUnarioPosOrder(UmaSequenciaID sequencia, Escopo escopo)
        {
            // ID OPERADOR

            if ((linguagem.VerificaSeEhID(sequencia.tokens[1]) && (linguagem.VerificaSeEhOperadorUnario(sequencia.tokens[0]))))
            {

                string nomeObjeto = sequencia.tokens[1];
                Objeto v = escopo.tabela.GetObjeto(nomeObjeto, escopo);

                if (v == null)
                    UtilTokens.WriteAErrorMensage(escopo, "objeto: " + nomeObjeto + "  inexistente.", sequencia.tokens);
      
                string nomeOperador = sequencia.tokens[0];
                Operador umOperador = Operador.GetOperador(nomeOperador, v.GetTipo(), "UNARIO", linguagem);
                if (umOperador.GetTipo() != "OPERADOR UNARIO")
                    UtilTokens.WriteAErrorMensage(escopo, "operador: " + nomeOperador + "  não é unário.", sequencia.tokens);
     
                bool isFoundClass = false;
                foreach (Classe umaClasse in escopo.tabela.GetClasses())

                    if (umaClasse.GetNome() == nomeOperador)
                    {
                        isFoundClass = true;
                        break;
                    } //if
                if (!isFoundClass)
                    UtilTokens.WriteAErrorMensage(escopo, "operador: " + nomeObjeto + "  inexistente.", sequencia.tokens);
                else
                {
                    escopo.tabela.GetExpressoes().Add(new Expressao(new string[] { nomeObjeto + " " + nomeOperador }, escopo));
                    return new Instrucao(ProgramaEmVM.codeOperadorUnario, new List<Expressao>(), new List<List<Instrucao>>());
                }
            } // if
            return null;
        } // OperacaoUnarioPreOrder()

        protected Instrucao BuildVariavelVetor(UmaSequenciaID sequencia, Escopo escopo)
        {
            if (sequencia.tokens.Count < 6)
            {
                UtilTokens.WriteAErrorMensage(escopo, "erro na sintaxe da instrucao vetor.", sequencia.tokens);
                return null;
            }
            // sintaxe: vector ID (ID, dims).
            if (!sequencia.tokens[0].Equals("vector"))
            {
                UtilTokens.WriteAErrorMensage(escopo, "erro de sintaxe na definição de variável vetor.", sequencia.tokens);
                return null;
            }

            string tipoDaVariavelVetor = sequencia.tokens[3];
            string nomeDaVariavelVetor = sequencia.tokens[1];
            int dimensoesVariavelVetor = int.Parse(sequencia.tokens[5]);

            Vetor vvt = new Vetor("private", nomeDaVariavelVetor, tipoDaVariavelVetor, escopo, new int[dimensoesVariavelVetor]);
            escopo.tabela.AddObjetoVetor(vvt.GetAcessor(), nomeDaVariavelVetor, tipoDaVariavelVetor, vvt.dimensoes, escopo, false);

            Expressao exprssDeclaracaoPropriedade = new Expressao();
            Instrucao instrucaoDeclaracao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(tipoDaVariavelVetor, nomeDaVariavelVetor, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VETOR, instrucaoDeclaracao, null);


            return instrucaoDeclaracao;

        }

        protected Instrucao BuildExpressaoValida(UmaSequenciaID sequencia, Escopo escopo)
        {
            Expressao expressaoCorreta = new Expressao(sequencia.tokens.ToArray(), escopo);
            if ((expressaoCorreta != null) && (expressaoCorreta.Elementos.Count > 0))
            {
                // adiciona a expressao correta, para a lista de expressoes do escopo, para fins de otimização.
                escopo.tabela.GetExpressoes().Add(expressaoCorreta);

                // cria a instrucao para a expressao correta.
                Instrucao instrucaoExpressaoCorreta = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>() { expressaoCorreta }, new List<List<Instrucao>>());
                return instrucaoExpressaoCorreta;       
            }
            return null;
        }
        protected Instrucao Atribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {

         

            string acessorObjetoAtribuicao = "private";
            //int indiceNomeTipo = 0;
            bool isInstanciacao = false;
            if (acessorsValidos.IndexOf(sequencia.tokens[0]) != -1)
                acessorObjetoAtribuicao = sequencia.tokens[0];

            string nomeObjetoAtribuicao = "";
            string tipoObjetoAtribuicao = "";
            Expressao expressaoTotal = new Expressao(sequencia.tokens.ToArray(), escopo);

            if (RepositorioDeClassesOO.Instance().GetClasse(expressaoTotal.Elementos[0].ToString()) != null)
            {

                tipoObjetoAtribuicao = expressaoTotal.Elementos[0].ToString();
                nomeObjetoAtribuicao = expressaoTotal.Elementos[1].ToString();
                isInstanciacao = true;
                escopo.tabela.GetObjetos().Add(new Objeto(acessorObjetoAtribuicao, tipoObjetoAtribuicao, nomeObjetoAtribuicao, null));
                expressaoTotal.Elementos.RemoveAt(0); // para compatibilizar com a instrucao de atribuicao.
            }
            else
            {
                nomeObjetoAtribuicao = expressaoTotal.Elementos[0].ToString();
                Objeto objJaInstanciado = escopo.tabela.GetObjeto(nomeObjetoAtribuicao, escopo);
                if (objJaInstanciado == null)
                {
                        UtilTokens.WriteAErrorMensage(escopo, "Objeto: " + nomeObjetoAtribuicao + " nao instanciado.", sequencia.tokens);
                    return null;
                }   
                tipoObjetoAtribuicao = objJaInstanciado.GetTipo();
                isInstanciacao = false;
            }

            Expressao expressaoAtribuicao = new Expressao();

            if (sequencia.tokens.Contains("="))
                expressaoAtribuicao = expressaoTotal;
            else
                expressaoAtribuicao = new Expressao();

            Instrucao instrucaoAtribuicao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            
            
            instrucaoAtribuicao.expressoes.Add(expressaoTotal);
            escopo.tabela.GetExpressoes().Add(expressaoTotal);
            
            
            int flag_tipoInstanciacao = 0;
            if (isInstanciacao)
                flag_tipoInstanciacao = Instrucao.EH_DEFINICAO;
            else
                flag_tipoInstanciacao = Instrucao.EH_MODIFICACAO;
                this.SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(tipoObjetoAtribuicao, nomeObjetoAtribuicao, null, flag_tipoInstanciacao, Instrucao.EH_OBJETO, instrucaoAtribuicao, expressaoAtribuicao);

            return instrucaoAtribuicao;
        }

        
        protected Instrucao BuildInstrucaoDefinicaoDePropriedadeAninhadas(UmaSequenciaID sequencia, Escopo escopo)
        {
            // processamento de expressoes que envolvem expressoesAninhadas, como "objA.a= obj.a+x";

            Expressao aninhadas = new Expressao(sequencia.tokens.ToArray(), escopo);

            
            if (aninhadas == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "expressao de propriedades aninhadas nao reconhecida.", sequencia.tokens);
                return null;
            }
            string tipoObjeto = sequencia.tokens[0];
            string nomeObjeto = sequencia.tokens[1];
         
            
            escopo.tabela.GetExpressoes().Add(aninhadas); // registra a expressao, para fins de otimização sobre o valor da expressão.
            Expressao exprssDeclarativa = new Expressao();


     

            Instrucao instrucaoPropriedade = new Instrucao(ProgramaEmVM.codeExpressionValid, new List<Expressao>(), new List<List<Instrucao>>());
            
            
            instrucaoPropriedade.expressoes.Add(aninhadas);
           

            if (sequencia.tokens.IndexOf("=") != -1)
                this.SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(tipoObjeto, nomeObjeto, new List<Expressao> { aninhadas}, Instrucao.EH_MODIFICACAO, Instrucao.EH_OBJETO, instrucaoPropriedade, null);
            else
                this.SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(tipoObjeto, nomeObjeto, aninhadas.Elementos, Instrucao.EH_DEFINICAO, Instrucao.EH_OBJETO, instrucaoPropriedade, null);
           


            return instrucaoPropriedade;
        }
 
          

        protected Instrucao AtribuicaoEstatica(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessorDaVariavel = sequencia.tokens[0];
            Classe tipoDaVariavel = RepositorioDeClassesOO.Instance().GetClasse(sequencia.tokens[2]);
            if (tipoDaVariavel == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "tipo da variavel estatica nao definida anteriormente.", sequencia.tokens);
                return null;
            }

            string nomeDaVariavel = sequencia.tokens[3];

            if (tipoDaVariavel.GetPropriedade(nomeDaVariavel) != null)
            {

                UtilTokens.WriteAErrorMensage(escopo, "propriedade estatica ja definida anteriormente.", sequencia.tokens);
                return null;
            }

            Objeto propriedadeEstatica = new Objeto(acessorDaVariavel, tipoDaVariavel.GetNome(), nomeDaVariavel, null, escopo, (Boolean)true);
            propriedadeEstatica.isStatic = true;


            Classe classeDoRepostorio = RepositorioDeClassesOO.Instance().GetClasse(tipoDaVariavel.nome);

            if (classeDoRepostorio != null)
            {
                if (classeDoRepostorio.propriedadesEstaticas.Find(k => k.GetNome().Equals(propriedadeEstatica.GetNome())) == null)
                    classeDoRepostorio.propriedadesEstaticas.Add(propriedadeEstatica);
            }

            Expressao exprssAtribuicaoDePropriedadeEstatica = new Expressao(sequencia.tokens.ToArray(), escopo);

            if (exprssAtribuicaoDePropriedadeEstatica != null)
                SetValorNumero(propriedadeEstatica, exprssAtribuicaoDePropriedadeEstatica, escopo);
            else
                exprssAtribuicaoDePropriedadeEstatica = new Expressao();

            Expressao exprssDeclaracaoPropriedadeEstatica = new Expressao();
            Instrucao instrucaoAtribuicao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(tipoDaVariavel.GetNome(), nomeDaVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_PRPOPRIEDADE_ESTATICA, instrucaoAtribuicao, exprssAtribuicaoDePropriedadeEstatica);

            if (exprssAtribuicaoDePropriedadeEstatica == null)
            {
                UtilTokens.WriteAErrorMensage(escopo, "sequencia nao completamente definida ainda.", sequencia.tokens);
                return null;
            }

            if (exprssAtribuicaoDePropriedadeEstatica.Elementos.Count > 0)
                escopo.tabela.GetExpressoes().Add(exprssAtribuicaoDePropriedadeEstatica); // registra as expressoes de atribuicao no escopo, para fins de otimização.



            instrucaoAtribuicao.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoPropriedadeEstatica, exprssAtribuicaoDePropriedadeEstatica });
            return instrucaoAtribuicao;


        }

 
 

        internal static void SetValorNumero(Objeto v, Expressao expressaoNumero, Escopo escopo)
        {
            if (expressaoNumero == null)
                return;

            string possivelNumero = expressaoNumero.ToString().Replace(" ", "");

            if (Expressao.Instance.IsTipoInteiro(possivelNumero))
                v.SetValorObjeto(int.Parse(possivelNumero)); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoFloat(possivelNumero))
                v.SetValorObjeto(float.Parse(possivelNumero)); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoDouble(possivelNumero))
                v.SetValorObjeto(double.Parse(possivelNumero));
        }

        ///______________________________________________________________________________________________________________________________________________________
        /// MÉTODOS TRATADORES DE CHAMADA DE FUNÇÃO.
        protected Instrucao ChamadaFuncao(UmaSequenciaID sequencia, Escopo escopo)
        {
            // ID ( ID )

            string nomeFuncao = sequencia.tokens[0];

            Expressao exprssChamadaDeFuncao = new Expressao(sequencia.tokens.ToArray(), escopo);
            if (exprssChamadaDeFuncao.GetType() != typeof(ExpressaoChamadaDeFuncao))
            {
                UtilTokens.WriteAErrorMensage(escopo, "chamada de funcao invalido!", sequencia.tokens);
                return null;
            }

            Funcao funcaoCompativel = ((ExpressaoChamadaDeFuncao)exprssChamadaDeFuncao).funcao;
            List<Expressao> expressoesParametros = ((ExpressaoChamadaDeFuncao)exprssChamadaDeFuncao).expressoesParametros;

            ExpressaoChamadaDeFuncao exprssFuncaoChamada = (ExpressaoChamadaDeFuncao)exprssChamadaDeFuncao;
  
            Expressao exprssDefinicaoDaChamada = new Expressao();
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento("chamada"));
            exprssDefinicaoDaChamada.Elementos.Add(exprssFuncaoChamada);



            escopo.tabela.GetExpressoes().AddRange(expressoesParametros); // registra as expessoes parametros, no escopo, para fins de otimização sobre o valor.
            Instrucao instrucaoChamada = new Instrucao(ProgramaEmVM.codeCallerFunction, new List<Expressao>() { exprssDefinicaoDaChamada }, new List<List<Instrucao>>());
            return instrucaoChamada;
        } // ChamadaFuncaoSemRetornoEComParametros()


        protected Instrucao ChamadaMetodo(UmaSequenciaID sequencia, Escopo escopo)
        {

            /// expressao containerr: 
            /// elemento 0: "chamada"
                
            //  ID . ID ( ID )";
            Expressao chamadaAMetodo = new Expressao(sequencia.tokens.ToArray(), escopo);
            if ((chamadaAMetodo == null) || (chamadaAMetodo.Elementos.Count == 0) || (chamadaAMetodo.Elementos[0].GetType() != typeof(ExpressaoChamadaDeMetodo))) 
            {
                UtilTokens.WriteAErrorMensage(escopo, "erro em chamada de metodo.", sequencia.tokens);
                return null;
            }

            List<Expressao> expressoesParametros = ((ExpressaoChamadaDeMetodo)chamadaAMetodo.Elementos[0]).chamadaDoMetodo[0].expressoesParametros;
            

            // forma a estrutura de dados que contem os dados da chamada do metodo: chamadas de metodos aninhados, propriedades aninhadas.
            Expressao exprssDefinicaoDaChamada = new Expressao();
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento("chamada"));
            exprssDefinicaoDaChamada.Elementos.Add(chamadaAMetodo);

            escopo.tabela.GetExpressoes().AddRange(expressoesParametros); // inclui na funcionalidade de otimização de expressoes, as expressões parâmetros.

            Instrucao instrucaoChamada = new Instrucao(ProgramaEmVM.codeCallerMethod, new List<Expressao>() { exprssDefinicaoDaChamada }, new List<List<Instrucao>>());
            return instrucaoChamada;

        } // ChamadaAMetodoComParametro()

        protected Instrucao ChamadaDeMetodoComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {
            Expressao chamadaMetodoComAtribuicao = new Expressao(sequencia.tokens.ToArray(), escopo);
            ExpressaoElemento expressaoCabecalho = new ExpressaoElemento("AtribuicaoChamadaDeMetodo");

            Instrucao instrucaoPropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            instrucaoPropriedade.expressoes.Add(expressaoCabecalho);
            instrucaoPropriedade.expressoes.Add(chamadaMetodoComAtribuicao);
            return instrucaoPropriedade;
        }

        protected Instrucao BuildDefinicaoDeMetodo(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessor = null;
            string tipoRetornoMetodo = null;
            string nomeMetodo = null;

            if (ProcessadorDeID.acessorsValidos.Find(k => k.Equals(sequencia.tokens[0])) != null)
            {
                acessor = sequencia.tokens[0];
                tipoRetornoMetodo = sequencia.tokens[1];
                nomeMetodo = sequencia.tokens[2];
            } // if


            List<Objeto> parametrosDoMetodo = new List<Objeto>();

            int indexParentesAbre = sequencia.tokens.FindIndex(k => k == "(");
            if (indexParentesAbre == -1)
            {
                UtilTokens.WriteAErrorMensage(escopo, "funcao sem interface de parametros, parte entre ( e ), exemplo deveria ser FuncaoA(){} e encontrou FuncaoA{}", sequencia.tokens);
                return null;
            }
            // constroi os parâmetros da definição da função.
            ExtraiParametrosDaFuncao(sequencia, parametrosDoMetodo, indexParentesAbre, escopo);



            // constroi a função.
            Funcao umMetodoComCorpo = new Funcao();// escopo.tabela.GetFuncao(nomeMetodo, tipoRetornoFuncao.tipo);
            umMetodoComCorpo.nome = nomeMetodo;
            umMetodoComCorpo.tipoReturn = tipoRetornoMetodo;
            umMetodoComCorpo.acessor = acessor;

            if (parametrosDoMetodo.Count > 0)
                umMetodoComCorpo.parametrosDaFuncao = parametrosDoMetodo.ToArray();



            // REGISTRA OS PARÂMETROS DA FUNÇÃO, COMO UMA ATRIBUICAO, O QUE É LOGICO POIS A DEFINICAO DE PARAMETROS É UMA ATRIBUICAO!!!
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
            {
                Objeto objParametro = new Objeto("private", parametrosDoMetodo[x].GetTipo(), parametrosDoMetodo[x].GetNome(), null);
                escopo.tabela.GetObjetos().Add(objParametro);
            }

            escopo.tabela.RegistraFuncao(umMetodoComCorpo); // registra a função com bloco.
            umMetodoComCorpo.escopo = new Escopo(escopo); // faz uma copia do escopo principal, que será modificado no corpo da função.

            escopo.escopoFolhas.Add(umMetodoComCorpo.escopo); // escopos de metodos são escopos folhas de escopos de classe ou escopo principal.

            Instrucao instrucoesCorpoDaFuncao = new Instrucao(ProgramaEmVM.codeDefinitionFunction, null, null);

            if (sequencia.tokens.Contains("{"))
            {
                ProcessadorDeID processadorBloco = null;
                this.BuildBloco(0, sequencia.tokens, ref umMetodoComCorpo.escopo, instrucoesCorpoDaFuncao, ref processadorBloco);
      
                umMetodoComCorpo.instrucoesFuncao.AddRange(instrucoesCorpoDaFuncao.blocos[0]);
            }
        
            // RETIRA OS PARÂMETROS DA FUNÇÃO, o que é logico tambem, pois o escopo da funcao ja foi copiado,paa a funcao, e a variavel parametro nao tem que ficar no escopo principal.
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.RemoveObjeto(parametrosDoMetodo[x].GetNome());



            return instrucoesCorpoDaFuncao;

        }
        protected Instrucao BuildDefinicaoDeFuncao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string tipoRetornoFuncao = null;
            string nomeFuncao = null;
            string acessorFuncao = null;
            if (acessorsValidos.Contains(sequencia.tokens[0]))
            {
                acessorFuncao = sequencia.tokens[0];
                tipoRetornoFuncao = sequencia.tokens[1];
                nomeFuncao = sequencia.tokens[2];
                if (nomeFuncao == "(")
                {
                    nomeFuncao = tipoRetornoFuncao;
                    tipoRetornoFuncao = "void"; // não tem tipo de retorno, o retorno é vazio.
                }
            }
            else
            {

                acessorFuncao = "protected";
                tipoRetornoFuncao = sequencia.tokens[0];
                nomeFuncao = sequencia.tokens[1];
             
            }
            List<Objeto> parametrosDoMetodo = new List<Objeto>();

            int indexParentesAbre = sequencia.tokens.FindIndex(k => k == "(");
            if (indexParentesAbre == -1)
            {
                UtilTokens.WriteAErrorMensage(escopo, "funcao sem interface de parametros, parte entre ( e ), exemplo deveria ser FuncaoA(){} e encontrou FuncaoA{}", sequencia.tokens);
                return null;
            }
            // constroi os parâmetros da definição da função.
            ExtraiParametrosDaFuncao(sequencia, parametrosDoMetodo, indexParentesAbre, escopo);

            // REGISTRA OS PARÂMETROS DA FUNÇÃO, COMO UMA ATRIBUICAO, O QUE É LOGICO POIS A DEFINICAO DE PARAMETROS É UMA ATRIBUICAO!!!
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.GetObjetos().Add(new Objeto("private", parametrosDoMetodo[x].GetTipo(), parametrosDoMetodo[x].GetNome(), null));

            // constroi a função.
            Funcao umaFuncaoComCorpo = new Funcao();
            umaFuncaoComCorpo.nome = nomeFuncao;
            umaFuncaoComCorpo.tipoReturn = tipoRetornoFuncao;
            umaFuncaoComCorpo.acessor = acessorFuncao;


            if (parametrosDoMetodo.Count > 0)
                umaFuncaoComCorpo.parametrosDaFuncao = parametrosDoMetodo.ToArray();


            // registra a função e adiciona as instruções do corpo da função.
            escopo.tabela.RegistraFuncao(umaFuncaoComCorpo); // registra a função com bloco.
            umaFuncaoComCorpo.escopo = new Escopo(escopo); // faz uma copia do escopo principal, que será modificado no corpo da função.

            escopo.escopoFolhas.Add(umaFuncaoComCorpo.escopo); // escopos de funcoes são escopos folhas do escopo principal.

            Instrucao instrucaoDefinicaoDeMetodo = new Instrucao(ProgramaEmVM.codeDefinitionFunction, null, null);


            ProcessadorDeID processadorBloco = null;
            this.BuildBloco(0, sequencia.tokens, ref umaFuncaoComCorpo.escopo, instrucaoDefinicaoDeMetodo, ref processadorBloco); // constroi o bloco de instruções, retornado o bloco de instrucoes, e o processador id do bloco.
            
            
    
            // RETIRA OS PARÂMETROS DA FUNÇÃO, o que é logico tambem, pois o escopo da funcao ja foi copiado,paa a funcao, e a variavel parametro nao tem que ficar no escopo principal.
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.RemoveObjeto(parametrosDoMetodo[x].GetNome());

            umaFuncaoComCorpo.instrucoesFuncao.AddRange(instrucaoDefinicaoDeMetodo.blocos[0]);
            return instrucaoDefinicaoDeMetodo;

        } // BuildDefinicaoDeFuncao()

        protected static void ExtraiParametrosDaFuncao(UmaSequenciaID sequencia, List<Objeto> parametrosDoMetodo, int indexParentesAbre, Escopo escopo)
        {
            if (indexParentesAbre != -1)
            {

                int start = indexParentesAbre;
                List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(start, "(", ")", sequencia.tokens);
                tokensParametros.RemoveAt(0);
                tokensParametros.RemoveAt(tokensParametros.Count - 1);

                if (tokensParametros.Count > 0)
                {
                    int indexToken = 0;
                    int pilhaInteiropParenteses = 0;

                    while (((indexToken + 1) < tokensParametros.Count) && (tokensParametros[indexToken] != "{"))
                    {

                        // exmemplo do codigo seguinte: funcaoA(), uma funcao sem parmetros.
                        if ((tokensParametros[indexToken] == "(") && (tokensParametros[indexToken + 1] == ")"))
                            break;

                        // o token é um parênteses fecha?
                        if (tokensParametros[indexToken] == ")")
                        {
                            pilhaInteiropParenteses--;
                            if (pilhaInteiropParenteses == 0)
                                break;
                        }
                        //  o  tokens é um parenteses abre?
                        if (tokensParametros[indexToken] == "(") // verifica se 
                        {
                            pilhaInteiropParenteses++;
                        }
                        // inicializa um parâmetro da função/método.
                        Objeto umParametro = new Objeto("private", tokensParametros[indexToken], tokensParametros[indexToken + 1], null, escopo, false);
                        parametrosDoMetodo.Add(umParametro); // adiciona o parametro construido.
                        indexToken += 3; // 1 token para o nome do parametro; 1 token para o tipo do parametro, 1 token para a vigula.

                    } // while
                } // if

            } // if tokenParametros.Count
        }

        protected Instrucao BuildInstrucaoOperadorBinario(UmaSequenciaID sequencia, Escopo escopo)
        {

            /// operador ID ID ( ID ID, ID ID ) prioridade ID meodo ID ;
            if ((sequencia.tokens[0].Equals("operador")) &&
               (linguagem.VerificaSeEhID(sequencia.tokens[1])) &&
               (linguagem.VerificaSeEhID(sequencia.tokens[2])) &&
               (sequencia.tokens[3] == "(") &&
               (linguagem.VerificaSeEhID(sequencia.tokens[4])) &&
               (linguagem.VerificaSeEhID(sequencia.tokens[5])) &&
               (sequencia.tokens[6] == ",") &&
               (linguagem.VerificaSeEhID(sequencia.tokens[7])) &&
               (linguagem.VerificaSeEhID(sequencia.tokens[8])) &&
               (sequencia.tokens[9] == ")") &&
               (sequencia.tokens[10] == "prioridade") &&
               (linguagem.VerificaSeEhID(sequencia.tokens[11])) &&
               (sequencia.tokens[12] == "metodo") &&
               (linguagem.VerificaSeEhID(sequencia.tokens[13]) &&
               ((sequencia.tokens[14] == ";"))))
            {
                string nomeClasseOperadorETipoDeRetorno = sequencia.tokens[1];
                string nomeOperador = sequencia.tokens[2];
                string nomeMetodoOperador = sequencia.tokens[13];
                string tipoOperando1 = sequencia.tokens[4];
                string tipoOperando2 = sequencia.tokens[7];

                string nomeOperando1 = sequencia.tokens[5];
                string nomeOperando2 = sequencia.tokens[8];

                List<Funcao> metodos = escopo.tabela.GetFuncao(nomeMetodoOperador).FindAll(k => k.nome.Equals(nomeMetodoOperador));
                Funcao funcaoOPeradorEncontrada = null;
                foreach (Funcao umaFuncaoDeOperador in metodos)
                {
                    if ((umaFuncaoDeOperador.parametrosDaFuncao.Length == 2) && (umaFuncaoDeOperador.tipoReturn.Equals(nomeClasseOperadorETipoDeRetorno)))
                    {
                        funcaoOPeradorEncontrada = umaFuncaoDeOperador;
                        break;
                    }
                }
                if (funcaoOPeradorEncontrada == null)
                {

                   UtilTokens.WriteAErrorMensage(escopo, "Funcao para Operador nao encontrada, tipos de parametros nao encontrados, ou classe e retorno nao encontrado.", sequencia.tokens);
                    return null;
                }


                if (RepositorioDeClassesOO.Instance().GetClasse(tipoOperando1) == null)
                {

                    UtilTokens.WriteAErrorMensage(escopo, "Erro na definição do operador binario" + nomeOperador + ", tipo do operando: " + tipoOperando1 + " nao existente", sequencia.tokens);
                    return null;
                }



                if (RepositorioDeClassesOO.Instance().GetClasse(tipoOperando2) == null) 
                {

                    UtilTokens.WriteAErrorMensage(escopo, "Erro na definição do operador binario: " + nomeOperador + ", tipo do operando: " + tipoOperando2 + " nao existente", sequencia.tokens);
                    return null;
                }


                int prioridade = -1;
                try
                {
                    prioridade = int.Parse(sequencia.tokens[11]);
                    if (prioridade < -1)
                    {

                        UtilTokens.WriteAErrorMensage(escopo, "prioridade: " + prioridade + " não valida para o operador: " + nomeOperador, sequencia.tokens);
                        return null;
                    }
                } //try
                catch
                {
                    UtilTokens.WriteAErrorMensage(escopo, "prioridade: " + sequencia.tokens[11] + " não valida para o operador: " + nomeOperador, sequencia.tokens);
                    return null;
                } // catch

                List<Expressao> expressaoElementosOperador = new List<Expressao>();
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeClasseOperadorETipoDeRetorno));
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeOperador));

                expressaoElementosOperador.Add(new ExpressaoElemento(tipoOperando1));
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeOperando1));

                expressaoElementosOperador.Add(new ExpressaoElemento(tipoOperando2));
                expressaoElementosOperador.Add(new ExpressaoElemento(nomeOperando2));


                expressaoElementosOperador.Add(new ExpressaoElemento(prioridade.ToString()));
                expressaoElementosOperador.Add(new ExpressaoChamadaDeFuncao(escopo.tabela.GetFuncao(nomeMetodoOperador, nomeClasseOperadorETipoDeRetorno, escopo)));

                Objeto operandoA = new Objeto("private", tipoOperando1, nomeOperando1, null, escopo, false);
                Objeto operandoB = new Objeto("private", tipoOperando2, nomeOperando2, null, escopo, false);

                Instrucao instrucaoOperadorBinario = new Instrucao(ProgramaEmVM.codeOperadorBinario, expressaoElementosOperador, new List<List<Instrucao>>());
                Operador opNovo = new Operador(nomeClasseOperadorETipoDeRetorno, nomeOperador, prioridade, new Objeto[] { operandoA, operandoB }, "BINARIO", funcaoOPeradorEncontrada.InfoMethod, escopo);

                escopo.tabela.GetOperadores().Add(opNovo);
                Classe classe = RepositorioDeClassesOO.Instance().GetClasse(nomeClasseOperadorETipoDeRetorno);
                if (classe != null)
                    classe.GetOperadores().Add(opNovo);

                return instrucaoOperadorBinario;
            }
            return null;
        } // DefinicaoDeUmOperadorBinario()

        protected Instrucao BuildInstrucaoOperadorUnario(UmaSequenciaID sequencia, Escopo escopo)
        {



            if ((sequencia.tokens[0] == "operador") &&
                (linguagem.VerificaSeEhID(sequencia.tokens[1])) &&
                (linguagem.VerificaSeEhID(sequencia.tokens[2])) &&
                (sequencia.tokens[3] == "(") &&
                (linguagem.VerificaSeEhID(sequencia.tokens[4])) &&
                (linguagem.VerificaSeEhID(sequencia.tokens[5])) &&
                (sequencia.tokens[6] == ")") &&
                (sequencia.tokens[7] == "prioridade") &&
                (linguagem.VerificaSeEhID(sequencia.tokens[8])) &&
                (sequencia.tokens[9] == "metodo") &&
                (linguagem.VerificaSeEhID(sequencia.tokens[10])) &&
                (sequencia.tokens[11] == ";"))
            {
                string tipoRetornoDoOperador = sequencia.tokens[1];
                if (RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador) == null) 
                {
                    UtilTokens.WriteAErrorMensage(escopo, "tipo: " + tipoRetornoDoOperador + " de retorno do operador nao existente.", sequencia.tokens);
                    return null;
                }



                string nomeOperador = sequencia.tokens[2];

                string tipoOperando1 = sequencia.tokens[4];
                string nomeOperando1 = sequencia.tokens[5];
                string nomeDaFuncaoQueImplementaOperador = sequencia.tokens[9];

                // valida a prioridade do operador;
                int prioridade = -100;
                try
                {
                    prioridade = int.Parse(sequencia.tokens[8]);
                } //try
                catch
                {
                    UtilTokens.WriteAErrorMensage(escopo, "prioridade: " + sequencia.tokens[8] + " nao valida para operador unario: " + nomeOperador, sequencia.tokens);
                    return null;
                } // catch

                if (prioridade <= -100)
                {
                    UtilTokens.WriteAErrorMensage(escopo, "prioridade: " + prioridade + "nao valida para o operador unario: " + nomeOperador, sequencia.tokens);
                    return null;
                }

                // tenta obter uma classe compatível com o tipo de operação (tipo do operador= o tipo do operando1.
                Classe classTipoOperando1 = RepositorioDeClassesOO.Instance().GetClasse(tipoOperando1);
                if (classTipoOperando1 == null)
                {
                    UtilTokens.WriteAErrorMensage(escopo, "tipo do operando: " + tipoOperando1 + " não existente para o operador unario: " + nomeOperador, sequencia.tokens);
                    return null;
                } // if

                List<Expressao> expressaoOperador = new List<Expressao>();
                expressaoOperador.Add(new ExpressaoElemento(tipoRetornoDoOperador));
                expressaoOperador.Add(new ExpressaoElemento(nomeOperador));
                expressaoOperador.Add(new ExpressaoElemento(tipoOperando1));
                expressaoOperador.Add(new ExpressaoElemento(nomeOperando1));
                expressaoOperador.Add(new ExpressaoElemento(prioridade.ToString()));
                expressaoOperador.Add(new ExpressaoChamadaDeFuncao(escopo.tabela.GetFuncao(nomeDaFuncaoQueImplementaOperador, tipoOperando1, escopo)));


                Instrucao instrucaoOperadorUnario = new Instrucao(ProgramaEmVM.codeOperadorUnario, expressaoOperador, new List<List<Instrucao>>());

                Objeto operandoA = new Objeto("private", tipoOperando1, nomeOperando1, null, escopo, false);

                Operador opNovo = new Operador(tipoRetornoDoOperador, nomeOperador, prioridade, new Objeto[] { operandoA }, "UNARIO", escopo.tabela.GetFuncao(nomeDaFuncaoQueImplementaOperador, tipoRetornoDoOperador, escopo).InfoMethod, escopo);
                escopo.tabela.GetOperadores().Add(opNovo);

                Classe classe = RepositorioDeClassesOO.Instance().GetClasse(tipoRetornoDoOperador);
                if (classe != null)
                    classe.GetOperadores().Add(opNovo);

                return instrucaoOperadorUnario;

            } // if
            return null;
        } // DefinicaoDeUmOperador()

        /// seta dados da instrucao de atribuicao, para variaveis, objetos, ou propriedades. seta também as propriedades/metodos aninhados.
        private void SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(string tipoPropriedade, string nomePropriedade, List<Expressao> aninhamento, int flag_tipoAtribuicao, int flag_tipoPropriedade, Instrucao instrucaoDeclarativa, Expressao atribuicao)
        {
            Expressao exprssCabecalho = new Expressao();
           
            if (aninhamento != null) // propriedades aninhadas.
            {
                exprssCabecalho.Elementos.Add(new Expressao());
                exprssCabecalho.Elementos[exprssCabecalho.Elementos.Count - 1].Elementos.AddRange(aninhamento);
            }
            else // objeto nao aninhados.
            {
                exprssCabecalho.Elementos.Add(new ExpressaoElemento(tipoPropriedade));
                exprssCabecalho.Elementos.Add(new ExpressaoElemento(nomePropriedade));
            }
            if (atribuicao != null)
                exprssCabecalho.Elementos.Add(atribuicao);
            else
                exprssCabecalho.Elementos.Add(new ExpressaoElemento(""));

            instrucaoDeclarativa.flags.Add(flag_tipoAtribuicao);
            instrucaoDeclarativa.flags.Add(flag_tipoPropriedade);
        }

        private void EliminaRedundanciasCausadosPeloSistemeDeRecuperacao(Escopo escopo)
        {
            List<Classe> classesNoEscopo = escopo.tabela.GetClasses();
            for (int x = 0; x < classesNoEscopo.Count; x++)
            {
                int index = escopo.tabela.GetClasses().FindIndex(k => k.GetNome() == classesNoEscopo[x].GetNome()); // encontra a classe do 1o. estágio.
                int contadorClasses = escopo.tabela.GetClasses().FindAll(k => k.GetNome() == classesNoEscopo[x].GetNome()).Count;
                if ((index != -1) && (contadorClasses > 1))
                    escopo.tabela.GetClasses().RemoveAt(index); // remove a classe do 1o. estágio.
            }

            List<Objeto> objetosNoEscopo = escopo.tabela.GetObjetos();
            for (int x = 0; x < objetosNoEscopo.Count; x++) // encontra os objetos instanciado do 1o. estágio.
            {
                int index = escopo.tabela.GetObjetos().FindIndex(k => k.GetNome() == objetosNoEscopo[x].GetNome());
                int contadorObjetos = escopo.tabela.GetObjetos().FindAll(k => k.GetNome() == objetosNoEscopo[x].GetNome()).Count;
                if ((index != -1) && (contadorObjetos > 1))
                {
                    escopo.tabela.GetObjetos().RemoveAt(index);
                    x--; // atualiza a variavel da malha, pois foi retirado uma posição.
                }
            }

            List<Vetor> vetoresNoEscopo = escopo.tabela.GetVetores();
            for (int x = 0; x < vetoresNoEscopo.Count; x++)
            {
                int index = escopo.tabela.GetVetores().FindIndex(k => k.GetNome() == vetoresNoEscopo[x].GetNome()); // encontra os vetores instanciados no 1o estagoo;
                int contadorVetores = escopo.tabela.GetVetores().FindAll(k => k.GetNome() == vetoresNoEscopo[x].GetNome()).Count;
                if ((index != -1) && (contadorVetores > 1))
                {
                    escopo.tabela.GetVetores().RemoveAt(index);
                    x--; // atualiza a variavel da malha, pois foi retirado uma posição.
                }
            }


            List<Expressao> expressoesNoEscopo = escopo.tabela.GetExpressoes(); // elimina redundâncias em expressões, pelo sistema de recuperação de sequencias nao tratadas.
            for (int x = 0; x < expressoesNoEscopo.Count; x++)
                for (int y = x + 1; y < expressoesNoEscopo.Count; y++)
                    if (IguaisFuncoes(expressoesNoEscopo[x], expressoesNoEscopo[y]))
                        expressoesNoEscopo.RemoveAt(y);

            escopo.tabela.GetExpressoes().Clear();
            escopo.tabela.GetExpressoes().AddRange(expressoesNoEscopo);
        }



        private bool IguaisFuncoes(Expressao exprss1, Expressao exprss2)
        {
            if (exprss1.tipo != exprss2.tipo)
                return false;
            if (exprss1.Elementos.Count != exprss2.Elementos.Count)
                return false;

            List<string> nomesObjetosExprss1 = NomesObjetosEmUmaExpressao(exprss1);
            List<string> nomesObjetosExprss2 = NomesObjetosEmUmaExpressao(exprss2);

            if ((nomesObjetosExprss1 == null) && (nomesObjetosExprss2 == null))
                return true;
            if ((nomesObjetosExprss1 == null) && (nomesObjetosExprss2 != null))
                return false;
            if ((nomesObjetosExprss1 != null) && (nomesObjetosExprss2 == null))
                return false;
            if (nomesObjetosExprss1.Count != nomesObjetosExprss2.Count)
                return false;

            for (int x = 0; x < nomesObjetosExprss1.Count; x++)
                if (nomesObjetosExprss1[x] != nomesObjetosExprss2[x])
                    return false;
            return true;
        }


        private List<string> NomesObjetosEmUmaExpressao(Expressao exprssCurrente)
        {
            List<string> nomesDeObjetos = new List<string>();
            for (int x = 0; x < exprssCurrente.Elementos.Count; x++)
                if ((exprssCurrente.Elementos[x].GetType() == typeof(ExpressaoObjeto)) && (((ExpressaoObjeto)exprssCurrente.Elementos[x]).objeto != null))
                    nomesDeObjetos.Add(((ExpressaoObjeto)exprssCurrente.Elementos[x]).objeto.GetNome());
            return nomesDeObjetos;
        }

        //______________________________________________________________________________________________________________________

        


        private class ComparerStringDecrescentemente : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                string[] seqX = x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] seqY = y.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (seqX.Length < seqY.Length)
                    return +1;
                if (seqX.Length > seqY.Length)
                    return -1;
                return 0;
            } //Compare()
        } // class ComparerStringDecrescentemente




        public class MetodoTratadorOrdenacao
        {
            public MetodoTratador metodo { get; set; }

            public string sequenciaID { get; set; }
            public MetodoTratadorOrdenacao(MetodoTratador umMetodo, string sequenciaIDMapeada)
            {
                this.metodo = umMetodo;
                this.sequenciaID = sequenciaIDMapeada;
            } // MetodoTratadorOrdenacao()

            public override string ToString()
            {
                string str = "";
                str += sequenciaID;
                return str;
            }
            /// <summary>
            /// ordena decrescentemente pelo cumprimento da sequencia do metodo tratador.
            /// </summary>
            public class ComparerMetodosTratador : IComparer<MetodoTratadorOrdenacao>
            {
                public int Compare(MetodoTratadorOrdenacao x, MetodoTratadorOrdenacao y)
                {
                    int c1 = x.sequenciaID.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;
                    int c2 = y.sequenciaID.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length;

                    if (c1 > c2)
                        return -1;
                    if (c1 < c2)
                        return +1;
                    return 0;
                } // Compare()
            } // class ComparerMetodosTratador
        } // class MetodoTratador


    } // class ProcessadorDeID

    public class DadosRecuperacao
    {
        public UmaSequenciaID sequenciasNaoTratadas { get; set; }
        public Funcao funcoesIntratadas { get; set; }

        public ProcessadorDeID processador { get; set; }

        public string nomeObjetoNaoTratado { get; set; }
        
  
      
        public DadosRecuperacao(Funcao funcao, UmaSequenciaID sequencia, ProcessadorDeID processador)
        {
            this.sequenciasNaoTratadas = sequencia;
            this.funcoesIntratadas = funcao;
            this.processador = processador;
            
        }

        public DadosRecuperacao(UmaSequenciaID sequencia, string nomeObjeto, ProcessadorDeID processador)
        {
          
            this.nomeObjetoNaoTratado = nomeObjeto;

            this.sequenciasNaoTratadas = sequencia;
            this.processador = processador;
           
        }
    }

} // namespace
