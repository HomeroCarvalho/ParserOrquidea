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


        // trecho de código a ser analisados.
        public static List<string> codigo { get; set; }

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
            return this.instrucoes.ToList<Instrucao>();
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
                linguagem = new LinguagemOrquidea();

            this.instrucoes = new List<Instrucao>();
            codigo = new List<string>();  // guarda o trecho de código.
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
                string str_definicaoComTipoDefinidoEOperadorUnarioPosOrdem = "OP ID";
                string str_definicaoComTipoDefinidoEOperadorUnarioPreOrdem = "ID OP";
                string str_definicaoVariavelNaoEstaticaSemAtribuicao = "ID ID ;";
                string str_definicaoVariavelVetor = "vector ID (ID, dims)";


                // definicao de funções estruturadas, e defnição de métodos.
                string str_definicaoDeFuncaoComRetornoComUmOuMaisParametrosComCorpoOuSemCorpo = "ID ID ( ID ID";
                string str_definicaoDeFuncaoComRetornoSemParametrosSemCorpo = "ID ID ( ) ;";
                string str_defnicaoDeFuncaoComRetornoSemParametrosComCorpo = "ID ID ( )";

                // definição sequências estruturada.
                string str_inicializacaoPropriedadeEstaticaComAtribuicao = "ID static ID ID = ID";
                string str_inicializacaoPropriedadeEstaticaSemAtribuicao = "ID static ID ID";

                // definicao de sequencias POO.
                string str_inicializacaoPropriedadeEAtribuicao = "ID ID ID = ID ;";
                string str_inicializacaoPropriedadeSemAtribuicao = "ID ID ID ;";

                // definicao de metodos POO.
                string str_definicaoDeMetodoComParametrosComCorpoOuSemCorpo = "ID ID ID ( ID ID";
                string str_definicaoMetodoSemParametrosComOuSemCorpo = "ID ID ID ( )";


                string str_chamadaAFuncaoComParametrosESemRetorno = "ID ( ID";
                string str_chamadaAMetodoSemParametros = "ID . ID ( )";
                string str_chamadaAMetodoComParametros = "ID . ID ( ID ";


                // sequencias de instruções da linguagem.
                string str_CreateNewObject = "ID ID = create ( ID , ID ";
             
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
                string DefinicaoInstrucaoGetVar = "ID GetVar ( ID )";


                string AtribuicaoDeVariavelSemDefinicao = "ID = ID ;";

                string str_definicaoDeVariavelNaoEsttaticaComAtribuicao = "ID ID = ID ;";
                string DefinicaoInstrucaoCasesOfUse = "casesOfUse ID : ( case ID ID :";


                //____________________________________________________________________________________________________________________________
                // CARREGA OS METODO TRATADORES E AS SEQUENCIAS DE ID ASSOCIADAS.

                // SEQUENCIAS OPERACAO
                LoadHandler(OperacaoUnarioPosOrder, str_definicaoComTipoDefinidoEOperadorUnarioPosOrdem);
                LoadHandler(OperacaoUnarioPreOrder, str_definicaoComTipoDefinidoEOperadorUnarioPreOrdem);

                // SEQUENCIAS ESTRUTURADAS.
                LoadHandler(BuildInstrucaoDefinicaoDeVariavelSemAtribuicao, str_definicaoVariavelNaoEstaticaSemAtribuicao);
                LoadHandler(BuildInstrucaoDeVariavelJaDefinidaComAtribuicao, str_definicaoDeVariavelNaoEsttaticaComAtribuicao);
                LoadHandler(ChamadaFuncao, str_chamadaAFuncaoSemRetornoESemParametros);


                // propriedades estaticas.
                LoadHandler(BuildInstrucaoDefinicaoPropriedadeEstaticaComAtribuicao, str_inicializacaoPropriedadeEstaticaComAtribuicao);
                LoadHandler(BuildInstrucaoDefinicaoPropriedadeEstaticaoSemAtribuicao, str_inicializacaoPropriedadeEstaticaSemAtribuicao);

                // propriedaedes nao estaticas.
                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeComAcessorComAtribuicao, str_inicializacaoPropriedadeEAtribuicao);
                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeSemAtribuicao, str_inicializacaoPropriedadeSemAtribuicao);




                LoadHandler(ChamadaMetodo, str_chamadaAMetodoSemParametros);
                LoadHandler(ChamadaMetodo, str_chamadaAMetodoComParametros);
                LoadHandler(ChamadaFuncao, str_chamadaAFuncaoComParametrosESemRetorno);

                // SEQUENCIAS INTORAPABILIDADE:
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
                LoadHandler(BuildInstrucaoDefinicaoDePropriedadeSemInicializacao, AtribuicaoDeVariavelSemDefinicao);


                LoadHandler(BuildVariavelVetor, str_definicaoVariavelVetor);
                LoadHandler(BuildInstrucaoGetVar, DefinicaoInstrucaoGetVar);
                LoadHandler(BuildInstrucaoSetVar, DefinicaoInstrucaoSetVar);
                LoadHandler(BuildInstrucaoOperadorBinario, DefinicaoDeOperadorBinario);
                LoadHandler(BuildInstrucaoOperadorUnario, DefinicaoDeOperadorUnario);
                LoadHandler(BuildInstrucaoCasesOfUse, DefinicaoInstrucaoCasesOfUse);



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

                        foreach(string umTokenAResumir in tokensDaSequenciaMapeada)
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




        /// <summary>
        /// compila o codigo, saida no objeto escopo desta classe.
        /// </summary>
        /// <param name="code">codigo, nao tokens.</param>
        public void Compile()
        {
            List<string> tokens = new Tokens(linguagem, codigo).GetTokens();
            this.CompileEscopos(this.escopo, tokens);
        }

        private int FindIndexEndClasse(List<string> tokens, int indexStart)
        {
            int pilhaOperadores = 0;
            int indiceCurrente = tokens.IndexOf("{", indexStart);
            while (indiceCurrente < tokens.Count)
            {
                if (tokens[indiceCurrente] == "{")
                    pilhaOperadores++;
                if (tokens[indiceCurrente] == "}")
                {
                    pilhaOperadores--;
                    if (pilhaOperadores == 0)
                        return ++indiceCurrente;
                }
                indiceCurrente++;
            }
            return -1;
        }

        /// <summary>
        /// compoe entradas e saidas de escopos, construindo classes, variáveis/propriedades, funções/métodos, operadores.
        /// </summary>
        private void CompileEscopos(Escopo escopo, List<string> tokens)
        {
            List<string> originais = tokens.ToList<string>();
            int umToken = 0;
            while (umToken < originais.Count)
            {

                if (((umToken + 1) < originais.Count) &&
                    (acessorsValidos.Find(k => k.Equals(originais[umToken])) != null) &&
                    (originais[umToken + 1].Equals("class")))
                {
                    try
                    {
                        int indexEndClass = FindIndexEndClasse(tokens, umToken);
                        List<string> tokensDaClasse = originais.GetRange(umToken, indexEndClass - umToken);

                        ExtratoresOO extratorDeClasses = new ExtratoresOO(escopo, linguagem, tokensDaClasse);
                        Classe umaClasse = extratorDeClasses.ExtaiUmaClasse();

                        if (umaClasse != null)
                        {
                            umToken += umaClasse.tokensDaClasse.Count; // atualiza o ponteiro da malha de tokens, pois foi utilizados os tokens na compilacao da classe.
                            umToken--; // para obter o token seguinte, passando pela malha de tokens.
                            if (extratorDeClasses.MsgErros.Count > 0)
                                this.escopo.GetMsgErros().AddRange(extratorDeClasses.MsgErros); // retransmite erros na extracao da classe, para a mensagem de erros do escopo.
                        } // if
                    }
                    catch
                    {
                        escopo.GetMsgErros().Add("Erro na extracao de tokens de uma classe, verifique a sintaxe das especificacoes de classe.");
                        return;
                    }
                }
                else
                if (linguagem.VerificaSeEhID(originais[umToken]) || (linguagem.isTermoChave(originais[umToken])))
                {

                    UmaSequenciaID sequenciaCurrente = UmaSequenciaID.ObtemUmaSequenciaID(umToken, originais, codigo); // obtem a sequencia  seguinte.
                    
                 
                    if (sequenciaCurrente == null)
                    {
                        PosicaoECodigo posicao = new PosicaoECodigo(originais.GetRange(umToken, originais.Count - umToken), codigo);
                        escopo.GetMsgErros().Add("sequencia de tokens não reconhecida: " + sequenciaCurrente.ToString() + ", linha: " + posicao.linha + ", coluna: " + posicao.coluna + "  verifique a sintaxe, incluindo operadores ponto-e-virgula.");
                        break;
                    } // if

                    MatchSequencias(sequenciaCurrente, escopo); // obtém o indice de metodo tratador.
                    if (sequenciaCurrente.indexHandler == -1)
                    {
                        GeraMensagemDeErroEmUmaInstrucao(sequenciaCurrente, escopo, "sequencia de tokens não reconhecida: " + sequenciaCurrente.ToString()+" ");
                        // permite validar as proximas instrucoes, e registra o erro na lista de mensagens de erro.
                    }
                    if ((sequenciaCurrente != null) && (sequenciaCurrente.indexHandler >= 0))
                    {
                        try
                        {
                            // chamada do método tratador para processar a costrução de escopos, da 0sequencia de entrada.
                            Instrucao instrucaoTratada = tratadores[sequenciaCurrente.indexHandler].metodo(sequenciaCurrente, escopo);
                            if (instrucaoTratada != null)
                                this.instrucoes.Add(instrucaoTratada);
                            else
                                GeraMensagemDeErroEmUmaInstrucao(sequenciaCurrente, escopo, "sequencia de tokens: " + sequenciaCurrente + " nao reconhecida, verifique a sintaxe, incluindo operadores ponto-e-virgula.");
                            umToken += sequenciaCurrente.original.Count; // atualiza o iterator de tokens, consumindo os tokens que foram utilizados no processamento da seuencia id currente.
                            continue;
                        }
                        catch
                        {
                            escopo.GetMsgErros().Add("Erro de compilacao da sequencia: " + sequenciaCurrente.ToString() + ". Verifique a sintaxe, com terminos de fim de instrucao, tambem. ");
                            return;
                        }
                    } // if tokensSequenciaOriginais.Count>0

                } // if linguagem.VerificaSeEID()
                umToken++;
            } // while

        }// CompileEcopos()



        /// <summary>
        /// encontra indices de métodos tratadores para a sequencia ID de entrada.
        /// </summary>
        private void MatchSequencias(UmaSequenciaID sequencia, Escopo escopo)
        {
           
            if (tokensSemResumir.FindIndex(k => k.Equals(sequencia.original[0])) != -1)
            {
               
                for (int seqMapeada = 0; seqMapeada < tratadores.Count; seqMapeada++)
                {
                    string[] tokensTratafores = tratadores[seqMapeada].sequenciaID.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToArray<string>();
                    
                    if (tokensTratafores[0] == sequencia.original[0])
                    {
                        sequencia.indexHandler = seqMapeada;
                        MatchBlocos(sequencia, escopo);
                        return;
                    }
                } // seqMapeada
            }
            else
            {


                List<string> AMapear = ResumeExpressoes(sequencia.original, ProcessadorDeID.tokensSemResumir);

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
            int indexSearchBlocks = sequencia.original.IndexOf("{");

            while (indexSearchBlocks != -1)
            {
                // retira um bloco a partir dos tokens sem modificações (originais).
                List<string> umBloco = UtilTokens.GetCodigoEntreOperadores(indexSearchBlocks, "{", "}", sequencia.original);
                // encontrou um bloco de tokens, adiciona à sequencia de entrada.
                if ((umBloco != null) && (umBloco.Count > 0))
                {
                    umBloco.RemoveAt(0);
                    umBloco.RemoveAt(umBloco.Count - 1);
                    sequencia.sequenciasDeBlocos.Add(new List<UmaSequenciaID>() { new UmaSequenciaID(umBloco.ToArray(), escopo.codigo) });
                } // if
                indexSearchBlocks = sequencia.original.IndexOf("{", indexSearchBlocks + 1);
            } // while
        } // MatchBlocos()


        private bool IsTokensPresentesEmDefinicoes(string token, List<string> tokensNaoResumiveis)
        {
            return tokensNaoResumiveis.Find(k => k.Equals(token)) != null;
        }

        // resume expressoes em id, unindo "id+operador+id" em um unico "id", se o operador for binario.
        // e resume tb expressoes em id, unindo tb "id+operadorUnario" em "id".
        // e resume tb expressoes em id, unindo "operadorUnario+id.
        private void SimplificarUmaExpressaoSemTermosChave(List<string> aResumir, List<string> tokensNaoResumiveis)
        {
          
            int umTokenResumido = 0;
            while ((umTokenResumido >= 0) && (umTokenResumido < aResumir.Count)) 
            {
                string umToken=aResumir[umTokenResumido];

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
        ///  este metodo resume items id como: id.id, ou id.id(.
        ///  1- se for "x.y", substitui por "x".
        ///  2- se for "x.y()", substitui por "y()".
        ///  3- se for x.y().z, substitui por z;
        ///  4- a regra geral é: apenas o ultimo aninhamento de propriedades ou metodos é que retorna.
        /// </summary>
        private List<string> ResumeAninhamento(List<string> tokens)
        {
            int lastIndexDot = tokens.LastIndexOf(".");
            if (lastIndexDot == -1)
                return tokens;
            List<string> tokensAninhamentoFeito = tokens.GetRange(lastIndexDot + 1, tokens.Count - lastIndexDot - 1);
            return tokensAninhamentoFeito;
        }

        // verifica se o token é um token não resumivel: 1- se tiver na lista de tokens essenciais para sequencias, ou 2- se for um termo-chave.
        private static bool isTokenNaoResumivel(string token, List<string> tokensNaoResumiveis, LinguagemOrquidea linguagem)
        {

            int indexToken = tokensNaoResumiveis.FindIndex(k => k.Equals(token));
            if (indexToken != -1)
                return true;

            if ((linguagem.isTermoChave(token)) && (!linguagem.VerificaSeEhOperador(token)))
                return true;

            return false;
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

        protected Instrucao OperacaoUnarioPreOrder(UmaSequenciaID umaSequenciaID, Escopo escopo)
        {
            //   OP. ID

            if ((linguagem.VerificaSeEhID(umaSequenciaID.original[0]) && (linguagem.VerificaSeEhOperadorUnario(umaSequenciaID.original[1]))))
            {


                string nomeVariavel = umaSequenciaID.original[1];
                Variavel v = escopo.tabela.GetVar(nomeVariavel, escopo);
                if (v == null)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(umaSequenciaID.original, escopo.codigo);
                    escopo.GetMsgErros().Add("variável: " + nomeVariavel + "  inexistente. linha: " + posicao.linha + " coluna: " + posicao.coluna + ".");
                }// if

                string nomeOperador = umaSequenciaID.original[0];
                Operador umOperador = Operador.GetOperador(nomeOperador, v.GetTipo(), "UNARIO", linguagem);

                if (umOperador == null)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(umaSequenciaID.original, escopo.codigo);
                    escopo.GetMsgErros().Add("operador: " + nomeOperador + "  não é unário. linha: " + posicao.linha + " coluna: " + posicao.coluna + ".");
                } // if

                bool isFoundClass = false;
                foreach (Classe umaClasse in escopo.tabela.GetClasses())

                    if (umaClasse.GetNome() == nomeOperador)
                    {
                        isFoundClass = true;
                        break;
                    } //if
                if (!isFoundClass)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(umaSequenciaID.original, escopo.codigo);
                    escopo.GetMsgErros().Add("operador: " + nomeVariavel + "  inexistente. linha: " + posicao.linha + " coluna: " + posicao.coluna + ".");
                    return null;
                }
                else
                {
                    escopo.tabela.GetExpressoes().Add(new Expressao(new string[] { nomeOperador + " " + nomeVariavel }, escopo));
                    return new Instrucao(ProgramaEmVM.codeOperadorUnario, new List<Expressao>(), new List<List<Instrucao>>());
                }
            } // if
            return null;
        } // OperacaoUnarioPreOrder()

        protected Instrucao OperacaoUnarioPosOrder(UmaSequenciaID umaSequenciaID, Escopo escopo)
        {
            // ID OPERADOR

            if ((linguagem.VerificaSeEhID(umaSequenciaID.original[1]) && (linguagem.VerificaSeEhOperadorUnario(umaSequenciaID.original[0]))))
            {

                string nomeVariavel = umaSequenciaID.original[1];
                Variavel v = escopo.tabela.GetVar(nomeVariavel, escopo);

                if (v == null)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(umaSequenciaID.original, escopo.codigo);
                    escopo.GetMsgErros().Add("variável: " + nomeVariavel + "  inexistente. linha: " + posicao.linha + " coluna: " + posicao.coluna + ".");
                }// if

                string nomeOperador = umaSequenciaID.original[0];
                Operador umOperador = Operador.GetOperador(nomeOperador, v.GetTipo(), "UNARIO", linguagem);
                if (umOperador.GetTipo() != "OPERADOR UNARIO")
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(umaSequenciaID.original, escopo.codigo);
                    escopo.GetMsgErros().Add("operador: " + nomeOperador + "  não é unário. linha: " + posicao.linha + " coluna: " + posicao.coluna + ".");
                } //if

                bool isFoundClass = false;
                foreach (Classe umaClasse in escopo.tabela.GetClasses())

                    if (umaClasse.GetNome() == nomeOperador)
                    {
                        isFoundClass = true;
                        break;
                    } //if
                if (!isFoundClass)
                {
                    PosicaoECodigo posicao = new PosicaoECodigo(umaSequenciaID.original, escopo.codigo);
                    escopo.GetMsgErros().Add("operador: " + nomeVariavel + "  inexistente. linha: " + posicao.linha + " coluna: " + posicao.coluna + ".");
                }
                else
                {
                    escopo.tabela.GetExpressoes().Add(new Expressao(new string[] { nomeVariavel + " " + nomeOperador }, escopo));
                    return new Instrucao(ProgramaEmVM.codeOperadorUnario, new List<Expressao>(), new List<List<Instrucao>>());
                }
            } // if
            return null;
        } // OperacaoUnarioPreOrder()

        protected Instrucao BuildVariavelVetor(UmaSequenciaID sequencia, Escopo escopo)
        {
            if (sequencia.original.Count < 6)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na sintaxe da instrucao vector.");
                return null;
            }
            // sintaxe: vector ID (ID, dims).
            if (!sequencia.original[0].Equals("vector"))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Erro de sintaxe na definição de variável vetor.");
                return null;
            }

            string tipoDaVariavelVetor = sequencia.original[3];
            string nomeDaVariavelVetor = sequencia.original[1];
            int dimensoesVariavelVetor = int.Parse(sequencia.original[5]);

            VariavelVetor vvt = new VariavelVetor("private", tipoDaVariavelVetor, nomeDaVariavelVetor, new int[dimensoesVariavelVetor]);
            escopo.tabela.AddVarVetor(vvt.GetAcessor(), nomeDaVariavelVetor, tipoDaVariavelVetor, vvt.dimensoes, escopo, false);

            Expressao exprssDeclaracaoPropriedade = new Expressao();
            Instrucao instrucaoDeclaracao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoPropriedade, tipoDaVariavelVetor, nomeDaVariavelVetor, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL_VETOR, instrucaoDeclaracao);
            
            
            return instrucaoDeclaracao;
            
        }
        protected Instrucao BuildInstrucaoDefinicaoDePropriedadeSemAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string acessorVariavel = sequencia.original[0];
            string tipoVariavel = sequencia.original[1];
            string nomeVariavel = sequencia.original[2];

            if (escopo.tabela.GetVar(nomeVariavel, escopo) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade/variavel já definida anteriormente neste escopo.");
                return null;
            }
            Classe tipoPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(sequencia.original[1]);
            if (tipoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            escopo.tabela.GetVariaveis().Add(new Variavel(acessorVariavel, nomeVariavel, tipoVariavel, null));


            Instrucao instrucaoPropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            Expressao exprssDeclaracaoDePropriedade = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoDePropriedade, tipoVariavel, nomeVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoPropriedade);
         

            return instrucaoPropriedade;
        }

        protected Instrucao BuildInstrucaoDefinicaoDePropriedadeSemInicializacao(UmaSequenciaID sequencia, Escopo escopo)
        {
            /// registra também o tipo da variavel. ainda que a instrucao de atribuicao eh sem inicializacao (sem definicao de variavel).


            string nomeVariavel = sequencia.original[0];



            object propriedadeAninhada = ValidaPropriedadesEncadeadas(sequencia.original, escopo);
            if (propriedadeAninhada == null)
            {
                Variavel v = escopo.tabela.GetVar(nomeVariavel, escopo);
                if (v != null)
                    propriedadeAninhada = new propriedade(v.GetNome(), v.GetTipo(), v.GetValor(), false);

                if ((escopo.tabela.GetVar(nomeVariavel, escopo) == null) && (escopo.tabela.GetVarVetor(nomeVariavel, escopo) == null) && ((escopo.tabela.GetObjeto(nomeVariavel) == null)))
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade aninhada nao reconhecida.");
                    return null;
                } // if


            }
            propriedade propriedadeAReceberAAtribuicao = (propriedade)propriedadeAninhada;
            string tipoVariavel = propriedadeAReceberAAtribuicao.tipo;

            if (sequencia.original.IndexOf(".") != -1)
            {
                int indexEndPropriedadeAninhada = sequencia.original.IndexOf("=");
                if (indexEndPropriedadeAninhada == -1)
                    nomeVariavel = Util.UtilString.UneLinhasLista(sequencia.original);
                else
                    nomeVariavel = Util.UtilString.UneLinhasLista(sequencia.original.GetRange(0, indexEndPropriedadeAninhada));  // modifica o nome da variavel para conter a sequencia de propriedades aninhadas.
            }

            List<Expressao> exprssAtribuicao = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
            if ((exprssAtribuicao == null) || (exprssAtribuicao.Count == 0))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "atribuicao a variavel sem expressao de atribuicao.");
                return null;
            }

            if ((sequencia.original.IndexOf(".") == -1) && (escopo.tabela.GetVar(nomeVariavel, escopo) == null))
            {
                // a variavel é um objeto, não uma propriedade. Remove a propriedade, e adiciona um objeto com o mesmo nome.
                Objeto objetoAninhado = new Objeto(tipoVariavel, nomeVariavel, null, escopo);
                escopo.tabela.RemoveVar(nomeVariavel);
                escopo.tabela.RegistraObjeto(objetoAninhado);
            }

            escopo.tabela.GetExpressoes().AddRange(exprssAtribuicao); // registra a expressao, para fins de otimização sobre o valor da expressão.

            Expressao exprssNomeObjeto = new Expressao();

            Instrucao instrucaoPropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            this.SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssNomeObjeto, tipoVariavel, nomeVariavel, null, Instrucao.EH_MODIFICACAO, Instrucao.EH_VARIAVEL, instrucaoPropriedade);
            instrucaoPropriedade.expressoes.AddRange(new List<Expressao>() { exprssNomeObjeto, exprssAtribuicao[0] });


            return instrucaoPropriedade;
        }
        protected Instrucao BuildInstrucaoDefinicaoDePropriedadeComAcessorComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {
            string nomeVariavel = sequencia.original[2];

            if (escopo.tabela.GetVar(nomeVariavel, escopo) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade/variavel já definida anteriormente neste escopo.");
                return null;
            }



            Classe tipoVariavel = RepositorioDeClassesOO.Instance().ObtemUmaClasse(sequencia.original[1]);
            if (tipoVariavel == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            Instrucao instrucaoDefinicaoPropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            Expressao exprssDeclaracaoPropriedde = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoPropriedde, tipoVariavel.GetNome(), nomeVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoDefinicaoPropriedade);



            if (exprssDeclaracaoPropriedde == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na declaracao da propriedade.");
                return null;
            }

            List<Expressao> exprssAtribuicaoPropriedade = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
 
            if ((exprssAtribuicaoPropriedade == null) || (exprssAtribuicaoPropriedade.Count==0))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na atribuicao da propriedade.");
                return null;
            }


            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            Variavel v = new Variavel(sequencia.original[0], sequencia.original[2], sequencia.original[1], null, false);
            SetValorNumero(v, exprssAtribuicaoPropriedade[0], escopo); // se a atribuicao for por numero, seta o valor da variavel, para fins de debug,


            escopo.tabela.GetVariaveis().Add(v); // registra a propriedade, para fins de debug.
            escopo.tabela.GetExpressoes().AddRange(exprssAtribuicaoPropriedade); // registra as expressoes de atribuicao, para fins de otimização sobre o valor.

            instrucaoDefinicaoPropriedade.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoPropriedde, exprssAtribuicaoPropriedade[0] });
            return instrucaoDefinicaoPropriedade;


        }

        protected Instrucao BuildInstrucaoDefinicaoDePropriedadeSemAcessorESemTipoComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string acessorVariavel = "private";
            string tipoVariavel = sequencia.original[0];
            string nomeVariavel = sequencia.original[1];

            Classe classetipoPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoVariavel);
            if (classetipoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            if (escopo.tabela.GetVar(nomeVariavel, escopo) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade/variavel já definida anteriormente neste escopo.");
                return null;
            }

            Instrucao instrucaoDefinicaoPropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            Expressao exprssDeclaracaoDePropriedade = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoDePropriedade, tipoVariavel, nomeVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoDefinicaoPropriedade);

            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            Variavel v = new Variavel(acessorVariavel, nomeVariavel, tipoVariavel, null);

            escopo.tabela.GetVariaveis().Add(v); // registra a variavel na tabela de valores.


            if (exprssDeclaracaoDePropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na declaracao da propriedade.");
                return null;
            }

            List<Expressao> expressaoAtribuicaoProprieade = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
            escopo.tabela.GetExpressoes().AddRange(expressaoAtribuicaoProprieade); // registra as expressoes de atribuição, para fins de otimização sobre o valor.


            if ((expressaoAtribuicaoProprieade == null) || (expressaoAtribuicaoProprieade.Count == 0)) 
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na atribuicao da propriedade.");
                return null;
            }

            SetValorNumero(v, expressaoAtribuicaoProprieade[0], escopo); // seta o valor da variavel, se for um numero, para fins de debug.

            instrucaoDefinicaoPropriedade.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoDePropriedade, expressaoAtribuicaoProprieade[0] });
            return instrucaoDefinicaoPropriedade;

        }


        protected Instrucao BuildInstrucaoDefinicaoPropriedadeSemAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessorVariavel = sequencia.original[0];
            string tipoVariavel = sequencia.original[1];
            string nomeVariavel = sequencia.original[2];

            Classe classetipoPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoVariavel);
            if (classetipoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            if (escopo.tabela.GetVar(nomeVariavel, escopo) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade/variavel já definida anteriormente neste escopo.");
                return null;
            }

            Instrucao instrucaoPropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            Expressao exprssDeclaracaoPropriedade = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoPropriedade, tipoVariavel, nomeVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoPropriedade);
            if (exprssDeclaracaoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na declaracao de propriedade.");
                return null;
            }

            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            Variavel v = new Variavel(acessorVariavel, nomeVariavel, tipoVariavel, null);
            escopo.tabela.GetVariaveis().Add(v);  // registra a variavel na tabela de valores.

            instrucaoPropriedade.expressoes.Add(exprssDeclaracaoPropriedade);
            return instrucaoPropriedade;
        }


        protected Instrucao BuildInstrucaoDefinicaoPropriedadeComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string acessorVariavel = sequencia.original[0];
            string tipoVariavel = sequencia.original[1];
            string nomeVariavel = sequencia.original[2];

            Classe classetipoPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoVariavel);
            if (classetipoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            if (escopo.tabela.GetVar(nomeVariavel, escopo) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade/variavel já definida anteriormente neste escopo.");
                return null;
            }

            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            Variavel v = new Variavel(acessorVariavel, nomeVariavel, tipoVariavel, null);
            escopo.tabela.GetVariaveis().Add(v);  // registra a variavel na tabela de valores.

     
            Instrucao instrucaoPropriedde = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            Expressao exprssDeclaracaoDaPropriedade = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoDaPropriedade, nomeVariavel, tipoVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoPropriedde);



            if (exprssDeclaracaoDaPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na declaracao de propriedade.");
                return null;
            }

            int indexTokenAtribuicao = sequencia.original.IndexOf("=");
            if (indexTokenAtribuicao == -1)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na declaracao de propriedade.");
                return null;
            }

            List<Expressao> exprssAtribuicao = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
            escopo.tabela.GetExpressoes().AddRange(exprssAtribuicao); // registra as expressões de atribuição, para fins de otimização sobre o valor modificado.

            if ((exprssAtribuicao == null) || (exprssAtribuicao.Count == 0))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na atribuicao da propriedade.");
                return null;
            }

            SetValorNumero(v, exprssAtribuicao[0], escopo);

            instrucaoPropriedde.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoDaPropriedade, exprssAtribuicao[0] });
            return instrucaoPropriedde;

        }

        protected Instrucao BuildInstrucaoDefinicaoPropriedadeEstaticaComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessorDaVariavel = sequencia.original[0];
            Classe tipoDaVariavel = RepositorioDeClassesOO.Instance().ObtemUmaClasse(sequencia.original[2]);
            if (tipoDaVariavel == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "tipo da variavel estatica nao definida anteriormente.");
                return null;
            }

            string nomeDaVariavel = sequencia.original[3];

            if (tipoDaVariavel.GetPropriedade(nomeDaVariavel) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade estatica ja definida anteriormente.");
                return null;
            }

            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            Variavel propriedadeEstatica = new Variavel(acessorDaVariavel, nomeDaVariavel, tipoDaVariavel.GetNome(), null);
            propriedadeEstatica.isStatic = true;


            Classe classeDoRepostorio = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDaVariavel.nome);

            if (classeDoRepostorio != null)
            {
                if (classeDoRepostorio.propriedadesEstaticas.Find(k => k.GetNome().Equals(propriedadeEstatica.GetNome())) == null)
                    classeDoRepostorio.propriedadesEstaticas.Add(propriedadeEstatica);
            }


            Expressao exprssDeclaracaoPropriedadeEstatica = new Expressao();
            Instrucao instrucaoAtribuicao = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoPropriedadeEstatica, tipoDaVariavel.GetNome(), nomeDaVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoAtribuicao);

            List<Expressao> exprssAtribuicaoDePropriedadeEstatica = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
            escopo.tabela.GetExpressoes().AddRange(exprssAtribuicaoDePropriedadeEstatica); // registra as expressoes de atribuicao no escopo, para fins de otimização.


            if ((exprssAtribuicaoDePropriedadeEstatica == null) || (exprssAtribuicaoDePropriedadeEstatica.Count == 0))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na atribuicao da propriedade.");
                return null;
            }

            SetValorNumero(propriedadeEstatica, exprssAtribuicaoDePropriedadeEstatica[0], escopo);

            instrucaoAtribuicao.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoPropriedadeEstatica, exprssAtribuicaoDePropriedadeEstatica[0] });
            return instrucaoAtribuicao;


        }

        protected Instrucao BuildInstrucaoDefinicaoPropriedadeEstaticaoSemAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessor = sequencia.original[0];
            Classe tipoVariavel = RepositorioDeClassesOO.Instance().ObtemUmaClasse(sequencia.original[2]);
            if (tipoVariavel == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "tipo da variavel estatica nao definida anteriormente.");
                return null;
            }

            string nomeVariavel = sequencia.original[3];


            if (tipoVariavel.GetPropriedade(nomeVariavel) != null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade estatica ja definida anteriormente.");
                return null;
            }



            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            Variavel propriedadeEstatica = new Variavel(acessor, nomeVariavel, tipoVariavel.GetNome(), null);
            propriedadeEstatica.isStatic = true;


            Classe classeDoRepostorio = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoVariavel.nome);
            if (classeDoRepostorio != null)
            {
                if (classeDoRepostorio.propriedadesEstaticas.Find(k => k.GetNome().Equals(propriedadeEstatica.GetNome())) == null)
                    classeDoRepostorio.propriedadesEstaticas.Add(propriedadeEstatica);
            }

            Instrucao instrucaoDefinicaoDePropriedade = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());

            Expressao exprssDefinicaoDePropriedadeEstatica = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDefinicaoDePropriedadeEstatica, nomeVariavel, tipoVariavel.GetNome(), null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoDefinicaoDePropriedade);

            instrucaoDefinicaoDePropriedade.expressoes.AddRange(new List<Expressao>() { exprssDefinicaoDePropriedadeEstatica });
            return instrucaoDefinicaoDePropriedade;


        }

        protected Instrucao BuildInstrucaoDefinicaoDeVariavelSemAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string tipoDaVariavel = sequencia.original[0];
            string nomeDaVariavel = sequencia.original[1];

            Classe classetipoPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDaVariavel);
            if (classetipoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            PosicaoECodigo posicao = new PosicaoECodigo(sequencia.original, escopo.codigo);
            escopo.tabela.GetVariaveis().Add(new Variavel("private", nomeDaVariavel, tipoDaVariavel, null));

            Expressao exprssDeclaracaoDeVariavel = new Expressao();
            Instrucao instrucaoDeclaracaoVariavel = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoDeVariavel, tipoDaVariavel, nomeDaVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoDeclaracaoVariavel);


            instrucaoDeclaracaoVariavel.expressoes.Add(exprssDeclaracaoDeVariavel);

            return instrucaoDeclaracaoVariavel;
        }


        protected Instrucao BuildInstrucaoDeVariavelJaDefinidaComAtribuicao(UmaSequenciaID sequencia, Escopo escopo)
        {

            string tipoDaVariavel = sequencia.original[0];
            string nomeDaVariavel = sequencia.original[1];

            Classe classetipoPropriedade = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDaVariavel);
            if (classetipoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro no tipo de propriedade, tipo nao existente ou erro de sintaxe");
                return null;
            }

            Variavel v = new Variavel("private", nomeDaVariavel, tipoDaVariavel, null);
            escopo.tabela.GetVariaveis().Add(v);


            Instrucao instrucaoDefinicaoVariavel = new Instrucao(ProgramaEmVM.codeAtribution, new List<Expressao>(), new List<List<Instrucao>>());
            Expressao exprssDeclaracaoPropriedade = new Expressao();
            SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(exprssDeclaracaoPropriedade, tipoDaVariavel, nomeDaVariavel, null, Instrucao.EH_DEFINICAO, Instrucao.EH_VARIAVEL, instrucaoDefinicaoVariavel);


            if (exprssDeclaracaoPropriedade == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na declaracao da variavel.");
                return null;
            }


            List<Expressao> exprssAtribuicaoDeVariavel = Expressao.Instance.ExtraiExpressoes(escopo, sequencia.original);
            escopo.tabela.GetExpressoes().AddRange(exprssAtribuicaoDeVariavel); // registra as expressoes de atribuicao, para fins de otimização sobre o valor.
                


            if ((exprssAtribuicaoDeVariavel == null) || (exprssAtribuicaoDeVariavel.Count == 0))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "erro na atribuicao da propriedade.");
                return null;
            }

            SetValorNumero(v, exprssAtribuicaoDeVariavel[0], escopo);

            instrucaoDefinicaoVariavel.expressoes.AddRange(new List<Expressao>() { exprssDeclaracaoPropriedade, exprssAtribuicaoDeVariavel[0] });
            return instrucaoDefinicaoVariavel;
        }

        private static void SetValorNumero(Variavel v, Expressao expressaoNumero, Escopo escopo)
        {
            string possivelNumero = expressaoNumero.ToString();

            if (Expressao.Instance.IsTipoInteiro(possivelNumero))
                v.SetValor(int.Parse(expressaoNumero.ToString()), escopo); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
            else
            if (Expressao.Instance.IsTipoFloat(possivelNumero))
                v.SetValor(float.Parse(expressaoNumero.ToString()), escopo); // seta o valor previamente, pois em modo de depuracao, é necessario este valor. Em um programa, a atribuicao é feito pela instrucao atribuicao.
        }


        ///______________________________________________________________________________________________________________________________________________________
        /// MÉTODOS TRATADORES DE CHAMADA DE FUNÇÃO.
        protected Instrucao ChamadaFuncao(UmaSequenciaID sequencia, Escopo escopo)
        {
            // ID ( ID )

            string nomeFuncao = sequencia.original[0];
            if (escopo.tabela.GetFuncoes().Find(k => k.nome.Equals(nomeFuncao)) == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "metodo: " + nomeFuncao + " inexistente no escopo atual.");
                return null;
            }
            int indexChamadaUltimoParentesesFecha = sequencia.original.LastIndexOf(")");
            int indexChamadaUltimoParentesesAbre = sequencia.original.LastIndexOf("(");

            List<string> tokensParametros = sequencia.original.GetRange(indexChamadaUltimoParentesesAbre, indexChamadaUltimoParentesesFecha - indexChamadaUltimoParentesesAbre);


            int tokenUltimoParentesesFecha = sequencia.original.LastIndexOf(")");
            int tokenPrimeiroParentesesAbre = sequencia.original.IndexOf("(");


            List<string> tokensChamada = sequencia.original.GetRange(tokenPrimeiroParentesesAbre, tokenUltimoParentesesFecha - tokenPrimeiroParentesesAbre + 1);
            tokensChamada.RemoveAt(0);
            tokensChamada.RemoveAt(tokensChamada.Count - 1);

            List<Expressao> expressoesChamada = Expressao.Instance.ExtraiExpressoes(escopo, tokensChamada);


            Funcao funcaoCompativel = this.ObtemFuncaoCompativelComAChamadaDeFuncao(nomeFuncao, expressoesChamada, escopo);
            if (funcaoCompativel == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Nao encontrada funcao/metodo com mesmos parametros.");
                return null;
            }

            List<Expressao> expressoesParametros = Expressao.Instance.ExtraiExpressoes(escopo, tokensParametros);

            funcaoCompativel.nomeClasse = null; // a funcao nao pertence a uma classe, e programacao estruturada.
            ExpressaoChamadaDeFuncao exprssFuncaoChamada = new ExpressaoChamadaDeFuncao(funcaoCompativel);
            exprssFuncaoChamada.expressoesParametros.AddRange(expressoesParametros);

            Expressao exprssDefinicaoDaChamada = new Expressao();
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento("chamada"));
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento(funcaoCompativel.nome));
            exprssDefinicaoDaChamada.Elementos.Add(exprssFuncaoChamada);


            if (expressoesParametros != null)
                exprssDefinicaoDaChamada.Elementos.AddRange(expressoesParametros); // inclui as expressoes de parametros da chamada.

            escopo.tabela.GetExpressoes().AddRange(expressoesParametros); // registra as expessoes parametros, no escopo, para fins de otimização sobre o valor.


            Instrucao instrucaoChamada = new Instrucao(ProgramaEmVM.codeCallerFunction, new List<Expressao>() { exprssDefinicaoDaChamada }, new List<List<Instrucao>>());
            return instrucaoChamada;
        } // ChamadaFuncaoSemRetornoEComParametros()


        protected Instrucao ChamadaMetodo(UmaSequenciaID sequencia, Escopo escopo)
        {
            //  ID . ID ( ID )";
            int tokenFirstDot = sequencia.original.IndexOf(".");

            List<string> tokens = sequencia.original.GetRange(0, tokenFirstDot + 1 + 1); // +1 do token da propriedae, +1 do token abre parenteses.
            List<string> tokensInterFaceParametros = UtilTokens.GetCodigoEntreOperadores(tokenFirstDot + 1 + 1, "(", ")", sequencia.original);

            if ((tokensInterFaceParametros == null) || (tokensInterFaceParametros.Count==0))
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "chamada de método sem definicao de interface de parâmetros: ()");
                return null;
            }

            tokens.AddRange(tokensInterFaceParametros);
            Funcao caller = (Funcao)ValidaPropriedadesEncadeadas(tokens, escopo);

            if (caller == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "objeto da chamada é nulo.");
                return null;
            }




            string nomeMetodo = sequencia.original[tokenFirstDot + 1];
            string nomeClasseDoMetodo = caller.nomeClasse;


            int tokenUltimoParentesesFecha = sequencia.original.LastIndexOf(")");
            int tokenPrimeiroParentesesAbre = sequencia.original.IndexOf("(");

            List<string> tokensChamada = sequencia.original.GetRange(tokenPrimeiroParentesesAbre, tokenUltimoParentesesFecha - tokenPrimeiroParentesesAbre + 1);
            tokensChamada.RemoveAt(0);
            tokensChamada.RemoveAt(tokensChamada.Count - 1);



            List<Expressao> expressoesParametros = Expressao.Instance.ExtraiExpressoes(escopo, tokensChamada); // obtem as expressoes que formam os parametros da chamada.
            escopo.tabela.GetExpressoes().AddRange(expressoesParametros); // registra as expressoes parametros, para fins de otimização sobre a modificacao sobre o valor.
 
            Funcao funcaoCompativel= this.ObtemFuncaoCompativelComAChamadaDeFuncao(nomeMetodo, expressoesParametros, escopo);

            if (funcaoCompativel == null)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Nao encontrada funcao/metodo com mesmos parametros.");
                return null;
            }

            funcaoCompativel.nomeClasse = nomeClasseDoMetodo; // faz a ligacao do metodo à classe que definiu o metodo.
            funcaoCompativel.caller = caller; // consegue o sonhado objeto que chamou o método!

            ExpressaoChamadaDeFuncao expressaoDaChamada = new ExpressaoChamadaDeFuncao(funcaoCompativel);
            if (expressoesParametros != null)
                expressaoDaChamada.expressoesParametros.AddRange(expressoesParametros);

            Expressao exprssDefinicaoDaChamada = new Expressao();
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento("chamada"));
            exprssDefinicaoDaChamada.Elementos.Add(new ExpressaoElemento(funcaoCompativel.nome));
            exprssDefinicaoDaChamada.Elementos.Add(expressaoDaChamada);


            Instrucao instrucaoChamada = new Instrucao(ProgramaEmVM.codeCallerFunction, new List<Expressao>() { exprssDefinicaoDaChamada }, new List<List<Instrucao>>());
            return instrucaoChamada;

        } // ChamadaAMetodoComParametro()

        private Funcao ObtemFuncaoCompativelComAChamadaDeFuncao(string nomeMetodo, List<Expressao> expressoesChamada, Escopo escopo)
        {
            List<Funcao> FuncoesCandidatosDaChamada = new List<Funcao>();

            for (int x = 0; x < RepositorioDeClassesOO.Instance().classesRegistradas.Count; x++) 
            {
                Classe classeAProcurarMetodo = RepositorioDeClassesOO.Instance().classesRegistradas[x];
                List<Funcao> metodosDaClasse = classeAProcurarMetodo.GetMetodos();
                if (metodosDaClasse != null)
                {
                    List<Funcao> metodosCompativeis = metodosDaClasse.FindAll(k => k.nome.Equals(nomeMetodo));
                    if ((metodosCompativeis != null) && (metodosCompativeis.Count > 0))
                        FuncoesCandidatosDaChamada.AddRange(metodosCompativeis);
                }
            }
            Funcao funcaoCompativel = null;

            for (int umaFuncao = 0; umaFuncao < FuncoesCandidatosDaChamada.Count; umaFuncao++)
            {
                if (FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao.Length != expressoesChamada.Count) // numero de parametros nao combinam.
                    continue;

                bool isFound = true;
                for (int x = 0; x < expressoesChamada.Count; x++)
                {
                    string tipoExpressaoChamada = UtilTokens.Casting(expressoesChamada[x].tipo);
                    string tipoFuncaoCandidata = UtilTokens.Casting(FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao[x].tipo);

                    if (tipoExpressaoChamada != tipoFuncaoCandidata) 
                    {
                        isFound = false;
                        break;
                    }

                }
                if (isFound)
                {
                    funcaoCompativel = FuncoesCandidatosDaChamada[umaFuncao];
                    break;
                }
            }
            return funcaoCompativel;
        }

        private  object ValidaPropriedadesEncadeadas(List<string> tokens, Escopo escopo)
        {
            /// exemplo: x.y.z É preciso validar y (tokens[n+1]) como propriedade de x (token[n]), e validar z (token[n+2]) como propriedade da classe de y (token[n+1]).
            UmaSequenciaID sequencia = new UmaSequenciaID(tokens.ToArray(), escopo.codigo);
      
            try
            {
                if (tokens.IndexOf(".") == -1)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade aninhada sem operador de composicao de propriedades ou metodos.");
                    return null;
                }
                int firsDot = tokens.IndexOf(".");
                string nomePropriedadeEncadeada = tokens[firsDot - 1];
                object propriedadeOuMetodoFinal = null;
                string nomeClasseCurrente = "";


                Variavel variavelInicial = escopo.tabela.GetVar(nomePropriedadeEncadeada, escopo);
                if (variavelInicial != null)
                    nomeClasseCurrente = variavelInicial.GetTipo();
                else
                if (variavelInicial == null) 
                {
                    Objeto objetoInicial = escopo.tabela.GetObjeto(nomePropriedadeEncadeada);
                    if (objetoInicial == null)
                    {
                        GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade ou objeto encadeado nao encontrado.");
                        return null;
                    }
                    nomeClasseCurrente = objetoInicial.GetClasse();
                }

                for (int x = 1; x < tokens.Count; x++)
                {
                    if (tokens[x] == ";")
                        return propriedadeOuMetodoFinal;

                    if (tokens[x] == "=")
                        return propriedadeOuMetodoFinal;

                    if (tokens[x].Equals(".")) // pula o operador de especificacao de propriedade.
                        continue;

                    if (nomeClasseCurrente == null)
                    {
                        GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "tipo do objeto: " + tokens[x - 1] + " nao reconhecida.");
                        return null;
                    }

                    propriedade propriedadeCurrente = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasseCurrente).GetPropriedade(tokens[x]);
         
                    if (propriedadeCurrente != null)
                        propriedadeOuMetodoFinal = propriedadeCurrente;

                    if (propriedadeCurrente == null)
                    {
                        // se nao eh propriedade, tenta verificar se eh uma chamada de metodo.
                        List<Funcao> metodosAninhados = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasseCurrente).GetMetodo(tokens[x]);
                
                        if ((metodosAninhados == null) || (metodosAninhados.Count == 0))
                        {
                            GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "propriedade ou metodo encadeada: " + tokens[x] + " nao reconhecida na classe: " + nomeClasseCurrente + ".");
                            return null;
                        } // if

                        propriedadeOuMetodoFinal = metodosAninhados[0];


                        List<string> chamadaFuncao = UtilTokens.GetCodigoEntreOperadores(x + 1, "(", ")", tokens);
                        if ((chamadaFuncao == null) || (chamadaFuncao.Count == 0))
                        {
                            GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "metodo: " + tokens[x] + " sem chamada de metodo valido " + nomeClasseCurrente + ".");
                            return null;
                        }

                        x += chamadaFuncao.Count;
                        continue;
                    } // if
                    nomeClasseCurrente = propriedadeCurrente.tipo;

                } // for x
                return propriedadeOuMetodoFinal;
            }
            catch
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Erro no processamento de propriedades ou metodos encadeados.");
                return false;

            }
        }

        /// seta dados da instrucao de atribuicao, para variaveis, objetos, ou propriedades.
        private void SetCabecalhoDefinicaoVariavelPropriedadeOuObjeto(Expressao exprssCabecalho, string tipoPropriedade, string nomePropriedade, string nomeCampoObjeto, int flag_tipoAtribuicao, int flag_tipoPropriedade, Instrucao instrucaoDeclarativa)
        {
            if (exprssCabecalho == null)
                exprssCabecalho = new ExpressaoElemento("configuracao");
            
            exprssCabecalho.Elementos.Add(new ExpressaoElemento(tipoPropriedade));
            exprssCabecalho.Elementos.Add(new ExpressaoElemento(nomePropriedade));
            if (nomeCampoObjeto != null)
                exprssCabecalho.Elementos.Add(new ExpressaoElemento(nomeCampoObjeto));

            instrucaoDeclarativa.flags.Add(flag_tipoAtribuicao);
            instrucaoDeclarativa.flags.Add(flag_tipoPropriedade);
        }

        protected Instrucao BuildDefinicaoDeMetodo(UmaSequenciaID sequencia, Escopo escopo)
        {
            string acessor = null;
            string tipoRetornoMetodo = null;
            string nomeMetodo = null;

            if (ProcessadorDeID.acessorsValidos.Find(k => k.Equals(sequencia.original[0])) != null)
            {
                acessor = sequencia.original[0];
                tipoRetornoMetodo = sequencia.original[1];
                nomeMetodo = sequencia.original[2];
            } // if


            List<propriedade> parametrosDoMetodo = new List<propriedade>();

            int indexParentesAbre = sequencia.original.FindIndex(k => k == "(");
            if (indexParentesAbre == -1)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "funcao sem interface de parametros, parte entre ( e ), exemplo deveria ser FuncaoA(){} e encontrou FuncaoA{}");
                return null;
            }
            // constroi os parâmetros da definição da função.
            ExtraiParametrosDaFuncao(sequencia, parametrosDoMetodo, indexParentesAbre);



            // constroi a função.
            Funcao umaFuncaoComCorpo = new Funcao();// escopo.tabela.GetFuncao(nomeMetodo, tipoRetornoFuncao.tipo);
            umaFuncaoComCorpo.nome = nomeMetodo;
            umaFuncaoComCorpo.tipoDoRetornoDaFuncao = tipoRetornoMetodo;
            if (parametrosDoMetodo.Count > 0)
                umaFuncaoComCorpo.parametrosDaFuncao = parametrosDoMetodo.ToArray();



            // REGISTRA OS PARÂMETROS DA FUNÇÃO, COMO UMA ATRIBUICAO, O QUE É LOGICO POIS A DEFINICAO DE PARAMETROS É UMA ATRIBUICAO!!!
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
            {
                PosicaoECodigo posicao2 = new PosicaoECodigo(sequencia.original, escopo.codigo);
                escopo.tabela.AddVar("private", parametrosDoMetodo[x].GetNome(), parametrosDoMetodo[x].tipo, null, escopo, false);
            }

            
            escopo.tabela.RegistraFuncao(umaFuncaoComCorpo); // registra a função com bloco.
            umaFuncaoComCorpo.escopo = new Escopo(escopo); // faz uma copia do escopo principal, que será modificado no corpo da função.

            Instrucao instrucoesCorpoDaFuncao = new Instrucao(ProgramaEmVM.codeDefinitionFunction, null, null);
            this.BuildBloco(0, sequencia.original, ref umaFuncaoComCorpo.escopo, instrucoesCorpoDaFuncao);


            // RETIRA OS PARÂMETROS DA FUNÇÃO, o que é logico tambem, pois o escopo da funcao ja foi copiado,paa a funcao, e a variavel parametro nao tem que ficar no escopo principal.
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.RemoveVar(parametrosDoMetodo[x].GetNome());


            return instrucoesCorpoDaFuncao;

        }
        protected Instrucao BuildDefinicaoDeFuncao(UmaSequenciaID sequencia, Escopo escopo)
        {
            string tipoRetornoFuncao = null;
            string nomeFuncao = null;

            tipoRetornoFuncao = sequencia.original[0];
            nomeFuncao = sequencia.original[1];

            List<propriedade> parametrosDoMetodo = new List<propriedade>();

            int indexParentesAbre = sequencia.original.FindIndex(k => k == "(");
            if (indexParentesAbre == -1)
            {
                GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "funcao sem interface de parametros, parte entre ( e ), exemplo deveria ser FuncaoA(){} e encontrou FuncaoA{}");
                return null;
            }
            // constroi os parâmetros da definição da função.
            ExtraiParametrosDaFuncao(sequencia, parametrosDoMetodo, indexParentesAbre);
            
            // REGISTRA OS PARÂMETROS DA FUNÇÃO, COMO UMA ATRIBUICAO, O QUE É LOGICO POIS A DEFINICAO DE PARAMETROS É UMA ATRIBUICAO!!!
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
            {
                PosicaoECodigo posicao2 = new PosicaoECodigo(sequencia.original, escopo.codigo);
                escopo.tabela.AddVar("private", parametrosDoMetodo[x].GetNome(), parametrosDoMetodo[x].tipo, null, escopo, false);
            }

            // constroi a função.
            Funcao umaFuncaoComCorpo = new Funcao();
            umaFuncaoComCorpo.nome = nomeFuncao;
            umaFuncaoComCorpo.tipoDoRetornoDaFuncao = tipoRetornoFuncao;
            if (parametrosDoMetodo.Count > 0)
                umaFuncaoComCorpo.parametrosDaFuncao = parametrosDoMetodo.ToArray();


            // registra a função e adiciona as instruções do corpo da função.
            escopo.tabela.RegistraFuncao(umaFuncaoComCorpo); // registra a função com bloco.
            umaFuncaoComCorpo.escopo = new Escopo(escopo); // faz uma copia do escopo principal, que será modificado no corpo da função.


            Instrucao instrucaoDefinicaoDeMetodo= new Instrucao(ProgramaEmVM.codeDefinitionFunction, null, null);
            this.BuildBloco(0, sequencia.original, ref umaFuncaoComCorpo.escopo, instrucaoDefinicaoDeMetodo);


            // RETIRA OS PARÂMETROS DA FUNÇÃO, o que é logico tambem, pois o escopo da funcao ja foi copiado,paa a funcao, e a variavel parametro nao tem que ficar no escopo principal.
            for (int x = 0; x < parametrosDoMetodo.Count; x++)
                escopo.tabela.RemoveVar(parametrosDoMetodo[x].GetNome());

            return instrucaoDefinicaoDeMetodo;

        } // BuildDefinicaoDeFuncao()

        protected static void ExtraiParametrosDaFuncao(UmaSequenciaID sequencia, List<propriedade> parametrosDoMetodo, int indexParentesAbre)
        {
            if (indexParentesAbre != -1)
            {

                int start = indexParentesAbre;
                List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(start, "(", ")", sequencia.original);
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
                        propriedade umParametro = new propriedade(tokensParametros[indexToken + 1], tokensParametros[indexToken], null, false);
                        parametrosDoMetodo.Add(umParametro); // adiciona o parametro construido.
                        indexToken += 3; // 1 token para o nome do parametro; 1 token para o tipo do parametro, 1 token para a vigula.

                    } // while
                } // if

            } // if tokenParametros.Count
        }

        protected Instrucao BuildInstrucaoOperadorBinario(UmaSequenciaID sequencia, Escopo escopo)
        {

            /// operador ID ID ( ID ID, ID ID ) prioridade ID meodo ID ;
            if ((sequencia.original[0].Equals("operador")) &&
               (linguagem.VerificaSeEhID(sequencia.original[1])) &&
               (linguagem.VerificaSeEhID(sequencia.original[2])) &&
               (sequencia.original[3] == "(") &&
               (linguagem.VerificaSeEhID(sequencia.original[4])) &&
               (linguagem.VerificaSeEhID(sequencia.original[5])) &&
               (sequencia.original[6] == ",") &&
               (linguagem.VerificaSeEhID(sequencia.original[7])) &&
               (linguagem.VerificaSeEhID(sequencia.original[8])) &&
               (sequencia.original[9] == ")") &&
               (sequencia.original[10] == "prioridade") &&
               (linguagem.VerificaSeEhID(sequencia.original[11])) &&
               (sequencia.original[12] == "metodo") &&
               (linguagem.VerificaSeEhID(sequencia.original[13]) &&
               ((sequencia.original[14] == ";"))))
            {
                string nomeClasseOperadorETipoDeRetorno = sequencia.original[1];
                string nomeOperador = sequencia.original[2];
                string nomeMetodoOperador = sequencia.original[13];
                string tipoOperando1 = sequencia.original[4];
                string tipoOperando2 = sequencia.original[7];

                string nomeOperando1 = sequencia.original[5];
                string nomeOperando2 = sequencia.original[8];

                List<Funcao> metodos = escopo.tabela.GetFuncao(nomeMetodoOperador).FindAll(k => k.nome.Equals(nomeMetodoOperador));
                Funcao funcaoOPeradorEncontrada = null;
                foreach(Funcao umaFuncaoDeOperador in metodos)
                {
                    if ((umaFuncaoDeOperador.parametrosDaFuncao.Length == 2) && (umaFuncaoDeOperador.tipoDoRetornoDaFuncao.Equals(nomeClasseOperadorETipoDeRetorno)))
                    {
                        funcaoOPeradorEncontrada = umaFuncaoDeOperador;
                        break;
                    }
                }
                if (funcaoOPeradorEncontrada == null)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Funcao para Operador nao encontrada, tipos de parametros nao encontrados, ou classe e retorno nao encontrado.");
                    return null;
                }


                if (RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.GetNome() == tipoOperando1) == null)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Erro na definição do operador binario" + nomeOperador + ", tipo do operando: " + tipoOperando1 + " nao existente");
                    return null;
                }



                if (RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.GetNome() == tipoOperando2) == null)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "Erro na definição do operador binario: " + nomeOperador + ", tipo do operando: " + tipoOperando2 + " nao existente");
                    return null;
                }


                int prioridade = -1;
                try
                {
                    prioridade = int.Parse(sequencia.original[11]);
                    if (prioridade < -1)
                    {
                        GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "prioridade: " + prioridade + " não valida para o operador: " + nomeOperador);
                        return null;
                    }
                } //try
                catch
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "prioridade: " + sequencia.original[11] + " não valida para o operador: " + nomeOperador);
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

                propriedade operandoA = new propriedade(nomeOperando1, tipoOperando1, null, false);
                propriedade operandoB = new propriedade(nomeOperando2, tipoOperando2, null, false);

                Instrucao instrucaoOperadorBinario = new Instrucao(ProgramaEmVM.codeOperadorBinario, expressaoElementosOperador, new List<List<Instrucao>>());
                Operador opNovo = new Operador(nomeClasseOperadorETipoDeRetorno, nomeOperador, prioridade, new propriedade[] { operandoA, operandoB }, "BINARIO", funcaoOPeradorEncontrada.InfoMethod, escopo);

                escopo.tabela.GetOperadores().Add(opNovo);
                Classe classe = RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.GetNome() == nomeClasseOperadorETipoDeRetorno);
                if (classe != null)
                    classe.GetOperadores().Add(opNovo);

                return instrucaoOperadorBinario;
            }
            return null;
        } // DefinicaoDeUmOperadorBinario()

        protected Instrucao BuildInstrucaoOperadorUnario(UmaSequenciaID sequencia, Escopo escopo)
        {



            if ((sequencia.original[0] == "operador") &&
                (linguagem.VerificaSeEhID(sequencia.original[1])) &&
                (linguagem.VerificaSeEhID(sequencia.original[2])) &&
                (sequencia.original[3] == "(") &&
                (linguagem.VerificaSeEhID(sequencia.original[4])) &&
                (linguagem.VerificaSeEhID(sequencia.original[5])) &&
                (sequencia.original[6] == ")") &&
                (sequencia.original[7] == "prioridade") &&
                (linguagem.VerificaSeEhID(sequencia.original[8])) &&
                (sequencia.original[9] == "metodo") &&
                (linguagem.VerificaSeEhID(sequencia.original[10])) &&
                (sequencia.original[11] == ";"))
            {
                string tipoRetornoDoOperador = sequencia.original[1];
                if (RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.GetNome() == tipoRetornoDoOperador) == null)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "tipo: " + tipoRetornoDoOperador + " de retorno do operador nao existente");
                    return null;
                }



                string nomeOperador = sequencia.original[2];

                string tipoOperando1 = sequencia.original[4];
                string nomeOperando1 = sequencia.original[5];
                string nomeDaFuncaoQueImplementaOperador = sequencia.original[9];

                // valida a prioridade do operador;
                int prioridade = -100;
                try
                {
                    prioridade = int.Parse(sequencia.original[8]);
                } //try
                catch
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "prioridade: " + sequencia.original[8] + " nao valida para operador unario: " + nomeOperador);
                    return null;
                } // catch

                if (prioridade <= -100)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "prioridade: " + prioridade + "nao valida para o operador unario: " + nomeOperador);
                    return null;
                }

                // tenta obter uma classe compatível com o tipo de operação (tipo do operador= o tipo do operando1.
                Classe classTipoOperando1 = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoOperando1);
                if (classTipoOperando1 == null)
                {
                    GeraMensagemDeErroEmUmaInstrucao(sequencia, escopo, "tipo do operando: " + tipoOperando1 + " não existente para o operador unario: " + nomeOperador);
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

                propriedade operandoA = new propriedade(nomeOperando1, tipoOperando1, null, false);

                Operador opNovo = new Operador(tipoRetornoDoOperador, nomeOperador, prioridade, new propriedade[] { operandoA }, "UNARIO", escopo.tabela.GetFuncao(nomeDaFuncaoQueImplementaOperador, tipoRetornoDoOperador, escopo).InfoMethod, escopo);
                escopo.tabela.GetOperadores().Add(opNovo);

                Classe classe = RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.GetNome() == tipoRetornoDoOperador);
                if (classe != null)
                    classe.GetOperadores().Add(opNovo);

                return instrucaoOperadorUnario;

            } // if
            return null;
        } // DefinicaoDeUmOperador()

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


} // namespace
