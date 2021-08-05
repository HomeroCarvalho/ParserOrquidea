using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Util;
using ModuloTESTES;
using parser.ProgramacaoOrentadaAObjetos;

namespace parser
{
    public class Tokens
    {
        /// <summary>
        /// contém os termo-chaves, operadores, e todos nomes de variaveis, funcoes, classes, etc... da linguagem desta classe.
        /// </summary>
        private List<string> tokensTotal { get; set; }
        private List<string> codigo { get; set; }

        public List<string> MsgErros { get; set; }

        private UmaGramaticaComputacional linguagem = null;

        /// <summary>
        /// construtor, aceita uma linguagem para processamentos, e um trecho de codigo a ser analizado.
        /// </summary>
        public Tokens(UmaGramaticaComputacional lng, List<string> _codigo)
        {
            this.linguagem = lng;
            this.codigo = _codigo.ToList<string>();
            this.tokensTotal = new List<string>();
            this.MsgErros = new List<string>();
        } // Tokens()


        public List<producao> GetProducoes(List<string> tokens, Escopo escopo)
        {

            LinguagemOrquidea linguagem = new LinguagemOrquidea();

            ProcessadorDeID processador = new ProcessadorDeID(escopo.codigo);
           
            // obtém as produções da linguage, sem modificações.
            List<producao> producoesDaLinguagemOriginais = linguagem.GetProducoes();

            // produções encontradas na lista de tokens.
            List<producao> producoesEncontradas = new List<producao>();
            List<producao> producoesResumidos = this.ResumeProducoes(producoesDaLinguagemOriginais);

            int x = 0;

            while ((x >= 0) && (x < tokens.Count))
            {
                UmaSequenciaID sequenciaAProcurar = UmaSequenciaID.ObtemUmaSequenciaID(x, tokens, escopo.codigo); // extrai uma sequencia válida da lista de tokens.
                List<string> tokensAProcurar = this.ResumeTokens(sequenciaAProcurar.original); // obtém os tokens da sequencia a procurar.


                if (tokensAProcurar[0] == "ID")
                {
                    // constroi a produção contendo a sequencia id extraida.
                    producao prod = new producao("sequenciaID", "sequenciaID", null, null);
                    prod.sequencia = sequenciaAProcurar;
                    prod.nomeProducao= "umaSequenciaID";
                    prod.tipo = "expressao";
                    prod.maquinaDeEstados = sequenciaAProcurar.original;
                    producoesEncontradas.Add(prod);


                    // faz o procesamnento dentro de tokens num bloco.
                    this.ProcessaBlocos(escopo, producoesEncontradas, sequenciaAProcurar);

                    
                    x += sequenciaAProcurar.original.Count; // atualiza o contador da malha principal, consumindo os tokens da sequencia encontrada.
                } // if
                else
                    for (int p = 0; p < producoesDaLinguagemOriginais.Count; p++)
                    {
                        // obtém os tokens da maquina de estados da producao currente.
                        List<string> tokensJaMapeados = producoesResumidos[p].maquinaDeEstados;

                        // a sequencia é de uma instrução 
                        if (this.MatchSequencias(tokensAProcurar, tokensJaMapeados))
                        {
                            producao umaProducao = new producao(producoesDaLinguagemOriginais[p]); // faz uma copia da producao original, para setar a sequencia id associada.

                            UmaSequenciaID seq = UmaSequenciaID.ObtemUmaSequenciaID(0,tokens, escopo.codigo);// extrai a primeira sequencia id da lista de tokens.
                            umaProducao.sequencia = seq; // seta na produção encontrada, a sequencia encontrada.




                            // adiciona a produção encontrada, na lista de produções encontradas.
                            producoesEncontradas.Add(umaProducao); 

                            // processa semi-producoes, que são expressões.
                            ProcessaSemiProducoes(producoesEncontradas, umaProducao, seq);
                            // processa producoes entre tokens entre blocos.
                            ProcessaBlocos(escopo, producoesEncontradas, seq);

                            tokens.RemoveRange(0, sequenciaAProcurar.original.Count); // remove da malha, os tokens encontrados.

                            break; // para a malha de procura de producao.
                        } // if
                    } // else if
                x++;
            } // while indexTokens
            return producoesEncontradas;
        } // GetProducoes()

        private void ProcessaSemiProducoes(List<producao> producoesEncontradas, producao umaProducao, UmaSequenciaID seq)
        {

            // faz o processamento de semi-producoes, se houver.uma semi-producao é sempre uma expressao, na linguagem orquidea.
            List<List<string>> semiProducoes = this.ObtemSemiProducoes(seq.original, umaProducao); // retira semi-producoes da sequencia id currente.
            if ((semiProducoes != null) && (semiProducoes.Count > 0))
            {
                for (int k = 0; k < semiProducoes.Count; k++)
                {
                    // compoe uma producao-expressao.
                    producao prodSemiProducao = new producao("semiProducao", "expressao", new List<string>(), semiProducoes[k].ToArray());
                    producoesEncontradas.Add(prodSemiProducao);
                } // for x
            } // if
        }

        private void ProcessaBlocos(Escopo escopo, List<producao> producoesEncontradas, UmaSequenciaID seq)
        {
            // faz o processamento de tokens num bloco.
            if (seq.original.IndexOf("{") != -1)
            {
                List<string> bloco = UtilTokens.GetCodigoEntreOperadores(seq.original.IndexOf("{"), "{", "}", seq.original);
                bloco.RemoveAt(0);
                bloco.RemoveAt(bloco.Count - 1);
                List<producao> producaoBloco = this.GetProducoes(bloco, escopo);
                if ((producaoBloco != null) && (producaoBloco.Count > 0))
                    producoesEncontradas.AddRange(producaoBloco);
            } // if
        }

        private bool MatchSequencias(List<string> seqAMapear, List<string> seqJaMapeado)
        {
            for (int x = 0; x < seqJaMapeado.Count; x++) 
                if (seqAMapear[x] != seqJaMapeado[x])
                    return false;
            return true;
        }


        /// <summary>
        /// resume producoes, resumindo os tokens
        /// </summary>
        private List<producao> ResumeProducoes(List<producao> producao)
        {
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            List<producao> producoesResumidas = producao.ToList<producao>();
            for (int x = 0; x < producao.Count; x++)
            {
                List<string> codigoProducao = producao[x].maquinaDeEstados.ToList<string>(); // obtém a lista de maquina de estados, que podem ou não conter todos tokens da maquina de estados.
                List<string> tokensDeUmaProducao = new Tokens(linguagem, codigoProducao).GetTokens(); // obtem os tokens da maquina de estados da producao currente.
                List<string> tokensResumidos = ResumeTokens(tokensDeUmaProducao); // obtem os tokens resumidos dos tokens da maquina de estados da producao currente.

                producoesResumidas[x].maquinaDeEstados = tokensResumidos;

            } // for x

            ComparaProducoesPelaMaquinaDeEstados comparer = new ComparaProducoesPelaMaquinaDeEstados();
            producoesResumidas.Sort(comparer);
            return producoesResumidas;
        } // ResumeProducoes()

        private List<string> ResumeTokens(List<string> tokens)
        {

            List<string> tokensDeUmaProducao = tokens.ToList<string>();
            for (int i = 0; i < tokensDeUmaProducao.Count; i++)
            {
                // primeiro caso de uso: o token não é um token-chave, ou o token é um token-chave [BLOCO], ou o token é "[" ou "]".
                if ((!linguagem.isTermoChave(tokensDeUmaProducao[i])) ||
                    (tokensDeUmaProducao[i].Equals("[BLOCO]")) ||
                    (ObtemOperadorValido(tokensDeUmaProducao[i])) ||
                    (tokensDeUmaProducao[i].Equals("[")) || (tokensDeUmaProducao[i].Equals("]"))) 
                {
                    tokensDeUmaProducao.RemoveAt(i);
                    tokensDeUmaProducao.Insert(i, "ID");
                } // if
                else
                // segundo caso de uso: o toke é um operador bloco, deve ser retirado ou inserido um id.
                if ((tokensDeUmaProducao[i] == "{") || (tokensDeUmaProducao[i] == "}"))
                {
                    tokensDeUmaProducao.RemoveAt(i);
                    tokensDeUmaProducao.Insert(i, "ID");
                } // else
            } // for i
            for (int i = 0; i < tokensDeUmaProducao.Count; i++)
            {
                // terceiro caso de uso: o token e o token seguinte são ids, devem ser resumidos a um só id, para fins de resumo.
                if (((i + 1) < tokensDeUmaProducao.Count) &&
                    (linguagem.VerificaSeEhID(tokensDeUmaProducao[i])) &&
                    (linguagem.VerificaSeEhID(tokensDeUmaProducao[i + 1])))
                {
                    tokensDeUmaProducao.RemoveRange(i, 2);
                    tokensDeUmaProducao.Insert(i, "ID");
                    i -= 1; // necessário para registro correto da malha, foi retirado dois tokens e inserido um token (ID).
                } // if

            } // for i
            return tokensDeUmaProducao;
        } // ResumeTokens()

        private static bool ObtemOperadorValido(string token)
        {
            List<Classe> classes = RepositorioDeClassesOO.Instance().classesRegistradas;
            for (int x = 0; x < classes.Count; x++)
                if (classes[x].GetOperadores().Find(k => k.nome == token) != null)
                    return true;
            return false;
        }

        /// <summary>
        /// obtem semi-producoes, localizadas entre os tokens da maquina de estados da producao de entrada.
        /// </summary>
        private List<List<string>> ObtemSemiProducoes(List<string> tokensProducao, producao p)
        {
            List<List<string>> semiProducoes = new List<List<string>>();

            List<string> tokensTermosChave = p.termos_Chave;

            
            int ini = 0;
            int fini = 0;
            for (int x = 0; x < tokensTermosChave.Count-1; x ++)
            {
                ini = tokensProducao.IndexOf(tokensTermosChave[x], ini + 1) + 1;
                if (ini != -1)
                    fini = tokensProducao.IndexOf(tokensTermosChave[x + 1], ini + 1) - 1;

                if ((ini == -1) || (fini == -1))
                    return semiProducoes;
            
                
                List<string> semiProducao = new List<string>();
                if ((fini - ini) >= 1) 
                {
                    for (int k = ini; k <= fini; k++)
                    {
                        semiProducao.Add(tokensProducao[k]);
                        tokensProducao[k] = "";
                    }
                    if ((ini - 1) >= 0)
                        tokensProducao[ini - 1] = ""; // retira o termo-chave inicial da lista de tokens da producao.
                 
                    semiProducoes.Add(semiProducao);
                }   // if
 
            } // for x
            return semiProducoes;
        }

        
        
        /// <summary>
        /// obtem todos tokens do trecho de codigo fornecido no trecho de codigo do construtor.
        /// </summary>
        /// <returns>retorna uma lista de tokens identificados no trecho de codigo fornecido no construtor.</returns>
        public List<string> GetTokens()
        {
            return ParserUniversal.GetTokens(Util.UtilString.UneLinhasLista(this.codigo));
                
        } // GetTokens()

      
        public struct TokensEPosicao
        {
            public string token;
            public int posicao;
            public TokensEPosicao(string tk, int pos)
            {
                token = tk;
                posicao = pos;
            }
        }
       
        /// <summary>
        /// ordena produções decrecentemente pela lista maquina de estados associada à produção.
        /// </summary>
        public class ComparaProducoesPelaMaquinaDeEstados : IComparer<producao>
        {
            public int Compare(producao x, producao y)
            {
                if (x.maquinaDeEstados.Count > y.maquinaDeEstados.Count)
                    return -1;
                if (x.maquinaDeEstados.Count < y.maquinaDeEstados.Count)
                    return +1;
                return 0;
            } // Compare()
        }
        public override string ToString()
        {
            string strTodosTokens = "";
            for (int tk = 0; tk < tokensTotal.Count; tk++)
                strTodosTokens += tokensTotal[tk] + "  ";
            return strTodosTokens;
        }
    }// class Tokens



    public class TokenParser
    {
        public List<string> tokens { get; set; }
        private int index = 0;
        TokenParser parserGuardar = null;

        public TokenParser()
        {
            this.tokens = new List<string>();
            this.index = 0;
        }
        public TokenParser(List<string> programa)
        {
            this.tokens = new Tokens(new LinguagemOrquidea(), programa.ToList<string>()).GetTokens();
            this.index = 0;
        } // TokenParser()

        public TokenParser(string[] tokens)
        {
            this.tokens = tokens.ToList<string>();
            this.index = 0;
        }
        public List<string> AllTokens()
        {
            return tokens;
        } // GetAllTokens()

       
        public void SetCurrentToken(string tokenSubsituto)
        {
            tokens[index] = tokenSubsituto;
        }
        
        public void RemoveCurrentToken(int index)
        {
            this.tokens.RemoveAt(index);
        }
        public void RemoveRangeTokens(int ini, int fini)
        {
            tokens.RemoveRange(ini, fini - ini);
            this.index = ini;
        }

        public List<string> GetRange(int ini, int fini)
        {
            return tokens.GetRange(ini, fini - ini);
        }

        public void Insert(int index, string token)
        {
            this.tokens[index] = token;
        }

         /// <summary>
        /// guarda o parser num objeto temporário.
        /// </summary>
        public void PushParser()
        {
            this.parserGuardar = this.Clone();
        } // PushParser()

        /// <summary>
        /// recupera o parser a partir de um objeto temporário.
        /// </summary>
        public void PopParser()
        {
            if (this.parserGuardar != null)
            {
                this.tokens = this.parserGuardar.tokens.ToList<string>();
                this.index = this.parserGuardar.index;
                this.parserGuardar = null;
            } // if
        } // PopParser()


        public string Current()
        {
            if (this.index < this.tokens.Count)
                return this.tokens[this.index];
            else
                return null;
        } // GetCurrentToken()

        public string Next()
        {
            this.index++;
            if (this.index < tokens.Count)
                return this.tokens[this.index];
            return null;
        } // Next()

        public string Previous()
        {
            this.index--;
            if (this.index >= 0)
                return this.tokens[this.index];
            return null;
        }

        public string Peek(int incIndice)
        {
            if ((this.index + incIndice) < this.tokens.Count)
                return (this.tokens[this.index + incIndice]);
            return "";
        } // PeekToken()

        public int Index()
        {
            return this.index;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }
        /// <summary>
        /// obtém uma cópia do parser currente.
        /// </summary>
        /// <returns></returns>
        public TokenParser Clone()
        {
            TokenParser parser = new TokenParser();
            parser.tokens = this.tokens.ToList<string>();
            parser.index = 0;
            return parser;
        } // Clone()

        /// <summary>
        /// coloca o ponteiro dos tokens para o índice inicial.
        /// </summary>
        public void DoEmpty()
        {
            this.index = 0;
        }
    }

} //namespace
