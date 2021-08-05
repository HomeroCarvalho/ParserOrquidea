﻿using System.Collections.Generic;
using System;
using System.Linq;
using Util;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Remoting.Messaging;
using parser.ProgramacaoOrentadaAObjetos;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Xml;
using System.IO;

namespace parser
{




    /// <summary>
    /// Uma expressão que encapsula uma chamada de função, tendo como parâmetros as expressões as sub-expressões.
    /// O resultado é liso! faz os cálculos como se os parâmetros não sejam objetos, mas expressões que são avaliadas,
    /// o que representa mais simplicidade e robustez do código.
    /// </summary>
    public class ExpressaoChamadaDeFuncao : Expressao
    {
        public Funcao funcao { get; set; }
        public List<Expressao> expressoesParametros { get; set; }
        public ExpressaoChamadaDeFuncao(Funcao umaFuncao) : base()
        {
            this.funcao = umaFuncao;
            this.Elementos = new List<Expressao>();
            this.expressoesParametros = new List<Expressao>();

        }
        public override object GetElemento()
        {
            return funcao;
        }

        public override string ToString()
        {
            List<string> parametros = new List<string>();
            string interfaceDeParametros = "";

            if (expressoesParametros != null)
            {
                // compoe os parametros da chamada de funcao.
                int x;
                for (x = 0; x < expressoesParametros.Count; x++)
                {
                    parametros.Add(expressoesParametros[x].ToString());
                    interfaceDeParametros += parametros[x];
                    if (x < expressoesParametros.Count - 1)
                        interfaceDeParametros += ",";
                }
            }

            return funcao.nome + "(" + interfaceDeParametros + ")";

        }
    }

    public class ExpressaoOperadorMatricial: Expressao
    {
        public List<Expressao> indices;

        public ExpressaoOperadorMatricial(List<Expressao>indicesMatriciais)
        {
            this.Elementos = new List<Expressao>();
            this.indices = indicesMatriciais.ToList<Expressao>();
        }

        public ExpressaoOperadorMatricial()
        {
            this.Elementos = new List<Expressao>();
            this.indices = new List<Expressao>();
        }

        public override string ToString()
        {
            if ((this.indices == null) || (this.indices.Count == 0))
                return "[]";

            string str = "[";
            for (int x = 0; x < this.indices.Count - 1; x++)
                str += this.indices[x].ToString() + " , ";

            str += this.indices[this.indices.Count - 1].ToString() + "]";
            return str;
        }
    }



    public class ExpressaoOperador : Expressao
    {
        public Operador operador;
        public ExpressaoOperador(Operador op):base()
        {
            this.operador = op;
            this.Elementos = new List<Expressao>();
        }
        public override object GetElemento()
        {
            return operador;
        }

        public override string ToString()
        {
            if (this.operador != null)
                return operador.nome;
            else
                return "operador";
        }
    }

    public class ExpressaoVariavel : Expressao
    {
        public Variavel variavel;
        public ExpressaoVariavel(Variavel v) : base()
        {
            this.variavel = v; // constroi com uma referencia.
            this.Elementos = new List<Expressao>();
        }
        public override object GetElemento()
        {
            return variavel;
        }

        public override string ToString()
        {
            return variavel.GetNome();
        }
    }

    public class ExpressaoNumero : Expressao
    {
        public string numero;
        public ExpressaoNumero(string numero) : base()
        {
            this.numero = numero;
            this.Elementos = new List<Expressao>();
        }
        public override object GetElemento()
        {
            return numero;
        }

        public override string ToString()
        {
            return numero.ToString();
        }
    }
    public class ExpressaoElemento : Expressao
    {
        public object elemento;

        public ExpressaoElemento(string token):base()
        {
            this.elemento = token;
        }
       
        public override object GetElemento()
        {
            return elemento;
        }
        public override string ToString()
        {
            return elemento.ToString();
        }
    }

    public class ExpressaoVariavelVetor : Expressao
    {
        public VariavelVetor variavel;
        public ExpressaoOperadorMatricial indicesVetor;

        public ExpressaoVariavelVetor(VariavelVetor variavel, ExpressaoOperadorMatricial matricial) : base()
        {
            this.variavel = variavel;
            this.indicesVetor = matricial;
            this.Elementos = new List<Expressao>();
        }

        public ExpressaoVariavelVetor(VariavelVetor variavel) : base()
        {
            this.variavel = variavel;
            this.indicesVetor = new ExpressaoOperadorMatricial();
            this.Elementos = new List<Expressao>();
        }

        public override object GetElemento()
        {
            return variavel;
        }


        public override string ToString()
        {
            return variavel.nome + "[ ]";
        }
    }


    /// <summary>
    /// expressoes! Os metodos (matemáticos, lógicos, de comparação) e as propriedades e variáveis. Uma expressão é
    /// tudo do programa, menos as sintaxes dos comandos da linguagem.
    /// </summary>
    public class Expressao
    {

        public virtual object GetElemento() { return null; }
        private static LinguagemOrquidea linguagem;

        public Escopo escopoDaExpressao; // contexto-escopo onde a expressão está codificada.
        private List<string> codigo; // trecho de código onde está localizado a expressão.

        public object oldValue;

        public string tipo = null; // tipo de retorno da expressão.



        public bool isModdfy = true; // otimização de expressão.
        public bool isInPosOrdem = false;

        public List<string> MsgErros; // lista de erros encontrados no processamento da expressão.
        public List<Expressao> Elementos; // lista de expressões-elementos da expressão.
        public List<string> elementosNaoProcessados { get; set; }


        private List<string> tokens; // tokens que compoe a expressão.


        public PosicaoECodigo posicaoDaExpressao;  // posição da expressão no código.



        private static Expressao InstanceSingleton;

        private int indiceProcessamentoDaProximaExpressao = -1;
        public static Expressao Instance
        {
            get
            {
                if (InstanceSingleton == null)
                    InstanceSingleton = new Expressao();
                return InstanceSingleton;
            } // get
        }// Instance()




        public Expressao()
        {
            this.Elementos = new List<Expressao>();
            this.MsgErros = new List<string>();
            if (linguagem == null)
                linguagem = new LinguagemOrquidea();
            this.isModdfy = true;
            this.oldValue = null;
            this.tokens = new List<string>();
            this.elementosNaoProcessados = new List<string>();
        }// Expressao().


        /// converte uma expressão para uma lista de tokens.
        public List<string> Convert()
        {
            List<string> tokens = ParserUniversal.GetTokens(this.ToString());
            return tokens;
        } // Convert()

        /// constroi a expressão, sem colocá-la em Pos-Ordem.
        public Expressao(string[] tokens, Escopo escopo)
        {


            if (escopo == null)
                return;

            if (linguagem == null)
                linguagem = new LinguagemOrquidea();

            this.elementosNaoProcessados = new List<string>();
            this.Elementos = new List<Expressao>();
            this.MsgErros = new List<string>();
            this.codigo = escopo.codigo;
            this.tokens = tokens.ToList<string>();

            if (tokens.Length == 1)
            {
                this.Elementos = new List<Expressao>();
                this.Elementos.Add(new ExpressaoElemento(tokens[0]));
                return;
            }// if

            
            escopoDaExpressao = escopo;
            for (int i = 0; i < tokens.Length; i++)
            {

                // ______________________________________________________________________________________________________________________________________________________
                // token de bloco, termina de processar os elementos da expressao currente.
                if (tokens[i] == "{")
                {
                    this.indiceProcessamentoDaProximaExpressao = -1;
                    return;
                }
                else
                //_________________________________________________________________________________________________________________________________________________________
                // token de termino de linha, termina o processamento dos elementos da expressao currente.
                if (tokens[i] == ";")
                {
                    this.indiceProcessamentoDaProximaExpressao = i;
                    return;
                }
                else
                //__________________________________________________________________________________________________________________________________________________________
                // o token é um operador matricial: [], retira os indices do operador, e adiciona na lista de expressoes currente.
                if (tokens[i] == "[")
                {
                    Expressao operadorMatricial= ExtraiOperadorMatricial(tokens, escopo, ref i);
                    this.Elementos.Add(operadorMatricial);
                }
                else
            //_______________________________________________________________________________________________________________________________________________________________
            // o token é um nome de função? (regra de uma chamada de um função).
            if (escopo.tabela.IsFunction(tokens[i], escopo) != null)
                {
                    Funcao funcao = escopo.tabela.GetFuncao(tokens[i], this.tipo, escopo);
                    if (funcao == null)
                    {
                        this.indiceProcessamentoDaProximaExpressao = -1;
                        return;
                    }
                    ExpressaoChamadaDeFuncao exprssFuncao = new ExpressaoChamadaDeFuncao(funcao);

                    // retira os tokens da chamada de função.
                    List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(i + 1, "(", ")", tokens.ToList<string>());
                    if ((tokensParametros == null) || (tokensParametros.Count == 0))
                    {
                        PosicaoECodigo posicao = new PosicaoECodigo(tokensParametros, escopo.codigo);
                        escopo.GetMsgErros().Add("Erro em chamada de função: " + funcao.nome + ",   linha: " + posicao.linha.ToString() + "  coluna: " + posicao.coluna.ToString());
                        this.indiceProcessamentoDaProximaExpressao = -1;
                        return;
                    }

                    if ((tokensParametros != null) && (tokensParametros.Count > 0))
                    {
                        tokensParametros.RemoveAt(0); // retira o primeiro parenteses.
                        tokensParametros.RemoveAt(tokensParametros.Count - 1); // retira o ultimo parenteses.


                        //compoe uma lista de expressoes, a partir da lista de tokens calculada acima.
                        List<Expressao> expressoesParametros = ExtraiExpressoes(escopo, tokensParametros);


                        if ((expressoesParametros != null) && (expressoesParametros.Count > 0))
                        {
                            for (int m = 0; m < expressoesParametros[0].Elementos.Count; m++)
                            {
                                string valorParametro = expressoesParametros[0].Elementos[m].ToString();
                                if ((IsTipoInteiro(valorParametro)) || (IsTipoFloat(valorParametro)))
                                    exprssFuncao.expressoesParametros.Add(new ExpressaoNumero(expressoesParametros[0].Elementos[m].ToString()));
                                else
                                    exprssFuncao.expressoesParametros.Add(new ExpressaoElemento(expressoesParametros[0].Elementos[m].ToString()));

                            }
                            // consome os tokens das expressões-parâmetros, da malha principal de tokens.
                            i += tokensParametros.Count + 2; //+2 porque eh a contagem inclui os parenteses abre e fecha da chamada de função.
                        } // if


                        this.indiceProcessamentoDaProximaExpressao = i;
                        this.Elementos.Add(exprssFuncao); // adiciona a expressão função para a lista de expressões da expressao currente.

                    }//if
                }
                //___________________________________________________________________________________________________________________________________________
                // expressao formada por um token palavra-chave, ou operador, mas não é palavra-chave ou operador, e sim um nome de termo-chave ou operador, como "@+", "+" nao é um operador, mas um nome de algum texto.

                if ((tokens[i] == "@") && ((i + 1) < tokens.Length))
                {
                    Expressao expressaoElementoID = new ExpressaoElemento(tokens[i + 1]);
                    expressaoElementoID.tipo = "string";
                    this.indiceProcessamentoDaProximaExpressao = i + 1;
                    return;
                }
                else
            //_______________________________________________________________________________________________________________________________________________
            // expressao entre parenteses, e nao eh chamada de funcao.
            if (tokens[i] == "(")
                {
                    // o token é de uma expressão entre parenteses. E não é uma chamada de função.
                    List<string> tokensDeUmaExpressaoEntreParenteses = UtilTokens.GetCodigoEntreOperadores(i,
                        "(", ")", tokens.ToList<string>());

                    if ((tokensDeUmaExpressaoEntreParenteses != null) && (tokensDeUmaExpressaoEntreParenteses.Count > 0))
                    {

                        string nextToken = tokens[tokensDeUmaExpressaoEntreParenteses.Count];
                        if (linguagem.VerificaSeEhOperadorBinario(nextToken))
                        {
                            tokensDeUmaExpressaoEntreParenteses.RemoveAt(0);
                            tokensDeUmaExpressaoEntreParenteses.RemoveAt(tokensDeUmaExpressaoEntreParenteses.Count - 1);

                            Expressao exprssEntreParenteses = new Expressao(tokensDeUmaExpressaoEntreParenteses.ToArray(), escopo);
                            if ((exprssEntreParenteses != null) && (exprssEntreParenteses.Elementos.Count > 0))
                                this.Elementos.Add(exprssEntreParenteses);

                            List<string> tokensRestantes = tokens.ToList<string>().GetRange(tokensDeUmaExpressaoEntreParenteses.Count + 2, tokens.Length - tokensDeUmaExpressaoEntreParenteses.Count - 1);
                            Expressao expressaoRestante = new Expressao(tokensRestantes.ToArray(), escopo);

                            Expressao exprssParentesesAbre = new ExpressaoElemento("(");
                            Expressao exprssParentesesFecha = new ExpressaoElemento(")");


                            expressaoRestante.Elementos.Insert(0, exprssParentesesAbre);
                            expressaoRestante.Elementos.Add(exprssParentesesFecha);

                            this.Elementos.Add(expressaoRestante);

                            this.indiceProcessamentoDaProximaExpressao = i + tokensDeUmaExpressaoEntreParenteses.Count + 2 + tokensRestantes.Count + 2;
                        }

                    }
                    else
                    {
                        tokensDeUmaExpressaoEntreParenteses.RemoveAt(0);
                        tokensDeUmaExpressaoEntreParenteses.RemoveAt(tokensDeUmaExpressaoEntreParenteses.Count - 1);

                        Expressao exprssEntreParenteses = new Expressao(tokensDeUmaExpressaoEntreParenteses.ToArray(), escopo);
                        if ((exprssEntreParenteses != null) && (exprssEntreParenteses.Elementos.Count > 0))
                        {
                            this.indiceProcessamentoDaProximaExpressao = i + 2 + tokensDeUmaExpressaoEntreParenteses.Count;
                            this.Elementos.AddRange(exprssEntreParenteses.Elementos.ToList<Expressao>());

                        }
                    }
                }
                else
            //_________________________________________________________________________________________________________________________________________________________
            // o token é uma variável singular?
            if (escopo.tabela.GetVar(tokens[i], escopo) != null)
                {
                    Variavel v = escopo.tabela.GetVar(tokens[i], escopo);
                    ExpressaoVariavel expressaoVariavel = new ExpressaoVariavel(v);
                    this.Elementos.Add(expressaoVariavel);

                } // if
                else
            if (escopo.tabela.GetVarVetor(tokens[i], escopo) != null)
                {
                    VariavelVetor vvt = escopo.tabela.GetVarVetor(tokens[i], escopo);
                    ExpressaoOperadorMatricial operadorMatricial= (ExpressaoOperadorMatricial)ExtraiOperadorMatricial(tokens, escopo, ref i);
                    ExpressaoVariavelVetor expressaoVariavelVetor = new ExpressaoVariavelVetor(vvt, operadorMatricial);

                    this.Elementos.Add(expressaoVariavelVetor);

                }
                else
                //__________________________________________________________________________________________________________________________________________________________
                // o token é um operador binario? operador unario?
                if ((linguagem.VerificaSeEhOperadorBinario(tokens[i])) || (linguagem.VerificaSeEhOperadorUnario(tokens[i]))) 
                {

                    Operador operador = linguagem.GetOperadorPeloNome(tokens[i]);
                    if (operador != null)
                    {
                        if (linguagem.VerificaSeEhOperadorBinario(tokens[i]))
                            operador.SetTipo("BINARIO");
                        else
                        if (linguagem.VerificaSeEhOperadorUnario(tokens[i]))
                            operador.SetTipo("UNARIO");

                        ExpressaoOperador expressaoOP = new ExpressaoOperador(operador);
                        this.Elementos.Add(expressaoOP);
                    }
                    else
                    {
                        PosicaoECodigo posicao = new PosicaoECodigo(tokens.ToList<string>(), escopo.codigo);
                        escopo.GetMsgErros().Add("operador nao localizado, linha: " + posicao.linha.ToString() + ", coluna: " + posicao.coluna.ToString() + ".");
                        return;
                    }


                } // else
                else
            //_________________________________________________________________________________________________________________________________________________________________
            // o token é um operador binario, unario, ternario?
            if (linguagem.VerificaSeEhOperador(tokens[i]))
                {
                    Operador operador = escopo.tabela.GetOperadores().Find(k => k.nome.Equals(tokens[i]));
                    ExpressaoOperador expressaoOP = new ExpressaoOperador(operador);
                    this.Elementos.Add(expressaoOP);
                }
                else
            //________________________________________________________________________________________________________________________________________________________________
            // o token é um numero?
            if (linguagem.VerificaSeEhNumero(tokens[i]))
                {
                    ExpressaoNumero numeroExpressao = new ExpressaoNumero(tokens[i]);
                    this.Elementos.Add(numeroExpressao);

                } //else
                //________________________________________________________________________________________________________________________________________________________________
                else
            // o token é uma variável não inicializada? Como uma arVetor? ou Variavel?
            if (linguagem.VerificaSeEhID(tokens[i]))
                    this.Elementos.Add(new ExpressaoElemento(tokens[i]));

            } // for i

            this.isModdfy = true;
        
        } // Expressao()

        private Expressao ExtraiOperadorMatricial(string[] tokens, Escopo escopo, ref int indexInicioOperador)
        {
            Expressao exprssOperadorMatricial = new Expressao();
            //___________________________________________________________________________________________________________________________________________________________________________________________
            // o token é um operador matricial?
            if (tokens[indexInicioOperador] == "[")
            {
                List<string> tokensMatriciais = UtilTokens.GetCodigoEntreOperadores(indexInicioOperador, "[", "]", tokens.ToList<string>());
                if ((tokensMatriciais == null) || (tokensMatriciais.Count == 0) || (tokensMatriciais[0] != "[") || (tokensMatriciais[tokensMatriciais.Count - 1] != "]"))
                    escopo.GetMsgErros().Add("Errro de sintaxe em um operador matricial: " + Utils.UneLinhasPrograma(tokens.ToList<string>()));
                else
                {
                    tokensMatriciais.RemoveAt(0);
                    tokensMatriciais.RemoveAt(tokensMatriciais.Count - 1);
                    List<Expressao> expressoesIndices = ExtraiExpressoes(escopo, tokensMatriciais);


                    if ((expressoesIndices == null) || (expressoesIndices.Count == 0))
                    {
                        PosicaoECodigo posicao = new PosicaoECodigo(tokensMatriciais, escopo.codigo);
                        escopo.GetMsgErros().Add("Erro na extracao de um operador matricial: " + Utils.UneLinhasPrograma(tokensMatriciais) + ", linha: " + posicao.linha + ", coluna: " + posicao.coluna);
                        this.indiceProcessamentoDaProximaExpressao = -1;
                        return exprssOperadorMatricial;
                    }
                    else
                    {
                        ExpressaoOperadorMatricial matricial = new ExpressaoOperadorMatricial(expressoesIndices);
                        indexInicioOperador += tokensMatriciais.Count + 2; // +2 dos operadores [ e ], + tokens.count das expressoes indices.
                        this.indiceProcessamentoDaProximaExpressao = indexInicioOperador;
                        return matricial;
                    }
                }

            }
            return null;
        }

        public List<Expressao> ExtraiExpressoes(Escopo escopo, List<string> tokensExpressoes)
        {

            List<Expressao> expressoesExtraidas = new List<Expressao>();
            List<string> tokensRaw = tokensExpressoes.ToList<string>();

            int indiceDeUmaExpressao = 0;
            string[] array_tokens = tokensRaw.ToArray();
            int x = 0;
            while ((x < tokensRaw.Count) && (indiceDeUmaExpressao != -1))
            {
               
                Expressao umaExpressao = new Expressao(tokensRaw.ToArray(), escopo);
                if (umaExpressao != null)
                {
                    if (umaExpressao.indiceProcessamentoDaProximaExpressao != -1)
                    {
                        int startSearch = umaExpressao.indiceProcessamentoDaProximaExpressao + 1;
                        if (startSearch == tokens.Count)
                            break;
                        else
                            tokensRaw = tokensRaw.GetRange(startSearch, tokensRaw.Count - startSearch);
                    }
                    else
                        tokensRaw.Clear();

                    expressoesExtraidas.Add(umaExpressao);
                }
                else
                    x++;
            }

            return expressoesExtraidas;

        }

        public bool IsTipoInteiro(string str_numero)
        {
            int numeroInt = 0;
            return int.TryParse(str_numero.Trim(' '), out numeroInt);
        }

        public bool IsTipoFloat(string str_numero)
        {
            float numeroFloat = 0.0f;
            return float.TryParse(str_numero.Trim(' '), out numeroFloat);
        }

        public bool ValidaExpressaoCondicional(Expressao expressao, Escopo escopo)
        {
            List<string> tokensDaExpressao = ParserUniversal.GetTokens(expressao.ToString());
            List<string> resumida = ObtemExpressaoCondicionalResumida(expressao, escopo); // obtém a expressão resumida dos tokens da expessão de entrada.
            int sinalExpressao = 1; // guarda a passagem entre ID e CONDICIONAL, como "ID CONDICIONAL ID"

            // o processo de validação de expressão resumida é para tokens na forma alternada: "ID CONDICIONAL ID CONDICIONAL ID", etc...
            foreach (string tokenDaExpressao in resumida)
            {
                if (tokenDaExpressao == "ID")
                {
                    if (sinalExpressao != 1)
                        return false;
                    sinalExpressao = 2;
                } // if
                if (tokenDaExpressao == "CONDICIONAL")
                {
                    if (sinalExpressao != 2)
                        return false;
                    sinalExpressao = 1;
                } // if
            } // foreach
            return true;
        } // ValidaExpressaoCondicional

        /// <summary>
        /// obtém uma lista resumida, com operadores, nomes de variaveis, nomes de funções.
        /// </summary>
        /// <param name="exprss">expressão a ser resumida.</param>
        /// <param name="escopo">escopo onde estão as definições de variáveis e funções.</param>
        /// <returns></returns>
        internal List<string> ObtemExpressaoGeralResumida(Expressao exprss, Escopo escopo)
        {

            List<string> resumida = new List<string>();
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            for (int x = 0; x < exprss.Elementos.Count; x++)
            {
                //a expessao é uma funcao?
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeFuncao))
                {
                    List<string> chamadaFuncao = ObtemExpressaoGeralResumida((Expressao)exprss.Elementos[x], escopo);
                    if ((chamadaFuncao == null) || (chamadaFuncao.Count == 0))
                        GeraMensagemDeErro(exprss, escopo);

                }  // if
                else
                // a expressao é uma variavel vetor?
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoVariavelVetor))
                {
                    List<string> chamadaFuncao = ObtemExpressaoGeralResumida((Expressao)exprss.Elementos[x], escopo);
                    if ((chamadaFuncao == null) || (chamadaFuncao.Count == 0))
                    {
                        if ((codigo != null) && (codigo.Count > 0))
                            GeraMensagemDeErro(exprss, escopo);
                    }
                }  // if

                else
                // a expressão é uma variável singular?
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoVariavel))
                {
                    string nomeVariavel = ((Variavel)exprss.Elementos[x].GetElemento()).GetNome();
                    if (linguagem.VerificaSeEhNumero(nomeVariavel))
                        resumida.Add("ID");
                    else
                    {

                        Variavel v = escopo.tabela.GetVar(nomeVariavel, escopo);
                        string tipoItem = v.GetTipo();
                        if (tipoItem != null)
                        {
                            resumida.Add("ID");
                        } // if

                        if (exprss.Elementos[x].GetType() == typeof(ExpressaoOperador))
                        {
                            string operadorItem = ((Operador)exprss.Elementos[x].GetElemento()).nome;

                            int indexOperadorBinario = linguagem.GetOperadoresBinarios().FindIndex(k => k.nome == operadorItem);
                            if (indexOperadorBinario != -1)
                                resumida.Add("OPERADOR_BINARIO");
                            else
                            {
                                int indexOperadorUnario = linguagem.GetOperadoresUnarios().FindIndex(k => k.nome == operadorItem);
                                if (indexOperadorUnario != -1)
                                    resumida.Add("OPERADOR_UNARIO");
                            } // else

                        } //else
                    } // else
                } // if exprss.subExpressaoes[x]
            } // for x
            return resumida;
        } // ObtemExpressaoGeralResumida()

        // gera uma mensagem de erro para invalidação do tipo de expressão: expressão singular, expressão vetor.
        private void GeraMensagemDeErro(Expressao exprss, Escopo escopo)
        {
            PosicaoECodigo posicao = new PosicaoECodigo(exprss.Convert(), this.codigo);
            escopo.GetMsgErros().Add("A Expressão não pode ser resumida. linha: " + posicao.linha + ", coluna: " + posicao.coluna);
        }

       
        /// valida a csolocação lógica sequencial de variáveis e operadores, e funções
        public bool ValidaExpressoesGeral(List<string> tokenDaExpressao)
        {
            int sinalExpressao = 1;
            for (int x = 0; x < tokenDaExpressao.Count; x++)
            {
                // o token é um ID? mas está na sequencia certa?
                if ((tokenDaExpressao[x] == "ID") && (sinalExpressao == 1))
                {
                    sinalExpressao = 2;
                    continue;
                } // if
                else
                // o token é um ID? mas está na sequencia errada?
                if ((tokenDaExpressao[x] == "ID") && (sinalExpressao == 1))
                {
                    return false;
                } // else
                else
                // o token é operador BINARIO? mas a sequencia está certa?
                if ((tokenDaExpressao[x] == "OPERADOR_BINARIO") && (sinalExpressao == 2))
                {
                    sinalExpressao = 1;
                    continue;
                } // if
                else
                // o token é operador BINARIO? mas a sequencia está errada?
                if ((tokenDaExpressao[x] == "OPERADOR_BINARIO") && (sinalExpressao == 1))
                {
                    return false;
                } // else
                else
                // o token é operador UNARIO?
                if (tokenDaExpressao[x] == "OPERADOR_UNARIO")
                {


                    // sequencia: "OPERADOR_UNARIO" "ID" "OPERADOR_BINARIO". exemplo: "++ c +"
                    if ((x >= 0) && ((x + 2) < tokenDaExpressao.Count) &&
                    (tokenDaExpressao[x + 1] == "ID") &&
                    (tokenDaExpressao[x + 2] == "OPERADOR_BINARIO"))
                    {
                        sinalExpressao = 1;
                        continue;
                    } // if
                    else

                // sequencia: "OPERADOR_UNARIO" "OPERADOR_BINARIO". exemplo:  "c ++ +"
                if ((x - 1) >= 0 &&
                    (tokenDaExpressao[x - 1] == "ID") &&
                    (tokenDaExpressao[x + 1] == "OPERADOR_BINARIO"))
                    {
                        sinalExpressao = 2;
                        continue;
                    } // if
                    else
                        return false; // exemplo: "c ++ b"

                } // if "OPERADOR_UNARIO"

            } // for x
            return true;
        } // ValidaExpressoesGeral()


    
        /// <summary>
        /// obtém uma expressão com dois elementos: ID e OPERADOR_CONDICIONAL, tornando possível analisar se a expressão condicional é valida.
        /// variável, chamadas de função, operadores não condifionais, são resumidos por um único ID, por expressões entre parênteses.
        /// </summary>
        /// <param name="exprss">expressão candidata a uma expressão condicional válida.</param>
        /// <returns></returns>
        internal List<string> ObtemExpressaoCondicionalResumida(Expressao exprss, Escopo escopo)
        {
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            List<string> resumidoSubExpressao = new List<string>();
            List<string> resumidoExressaoPrincipal = new List<string>();
            List<string> tokensDaExpressao = exprss.Convert();


            int pilhaInteiroParenteses = 0;


            for (int x = 0; x < exprss.Elementos.Count; x++)
            {

                // é nome de função? (chamada de função).
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeFuncao))
                {
                    List<string> tokensDaChamada = ObtemExpressaoCondicionalResumida(exprss.Elementos[x], escopo);
                    if ((tokensDaChamada == null) || (tokensDaChamada.Count == 0))
                        return null;
                    x += tokensDaChamada.Count;

                } // if

                // é nome de função? (chamada de função).
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoVariavelVetor))
                {
                    List<string> tokensVector = ObtemExpressaoCondicionalResumida(exprss.Elementos[x], escopo);
                    if ((tokensVector == null) || (tokensVector.Count == 0))
                        return null;
                    x += tokensVector.Count;

                } // if


                if (exprss.Elementos[x].GetType() == typeof(ExpressaoVariavel))
                {
                    string nomeVariavel = ((Variavel)exprss.Elementos[x].GetElemento()).GetNome();

                    //é operador não condicional, mas um operador?
                    if ((linguagem.VerificaSeEhOperador(nomeVariavel) && (!linguagem.IsOperadorCondicional(nomeVariavel))))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.
                    } // if

                    else
                    // é um ID não nome de função? 
                    if ((linguagem.VerificaSeEhID(nomeVariavel)) && (escopo.tabela.IsFunction(nomeVariavel, escopo) == null))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.
                    }
                    else
                    // é operador condicional?
                    if (linguagem.IsOperadorCondicional(nomeVariavel))
                    {
                        resumidoSubExpressao.Add("CONDICIONAL"); // acrescenta um resumo condicional, e começa uma nova sub-expressão.
                        resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);
                        resumidoSubExpressao.Clear();
                    } // if 
                    else
                    //é  parenteses abre?
                    if (nomeVariavel.Equals("("))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.

                        pilhaInteiroParenteses++; // incrementa a pilha de parenteses, para verificar se a expressão tem um termo final (parenteses fecha).
                    } // if
                    else
                    // é parenteses fecha?
                    if (nomeVariavel.Equals(")"))
                    {
                        pilhaInteiroParenteses--;
                        if (pilhaInteiroParenteses == 0) // se a pilha de parenteses zerar, retorna a expressão resumida principal.
                        {
                            resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);  // descarrega a lista da sub-expressão, que não foi acrescentada ainda.
                            return resumidoExressaoPrincipal;
                        }
                    } // if
                } // for x
            } //if Getype()==ItemExpressao
            resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);
            return resumidoExressaoPrincipal;
        } // ObtemExpressaoCondicionalResumida()


        public Expressao PosOrdemExpressao()
        {
          
            if ((this.Elementos == null) || (this.Elementos.Count == 0) || (this.Elementos.Count == 1))
                return this;

            Expressao expressaoRetorno = new Expressao();

            Pilha<Operador> pilha = new Pilha<Operador>("operadores");
            List<Operador> operadoresPresentes = new List<Operador>();
            int index = 0;
            int prioridadeParenteses = 0;

            // resolve ambiguidade de operadores que são unários e binários ao mesmo tempo. segue regras heurísticas para determinar se o operador é unário ou binário.
            OperadorUnarioEBinarioAoMesmoTempo(this);

            while (index < Elementos.Count)
            {

                if (RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.GetNome() == Elementos[index].ToString()) != null)
                {
                    index++;
                    continue; // definição do tipo da variavel não é avaliado em Expressao.PosOrdem.    
                }
                if (this.Elementos[index].ToString() == "(")
                    prioridadeParenteses += 25;
                else
                if (this.Elementos[index].ToString() == ")")
                    prioridadeParenteses -= 25;

                if (linguagem.VerificaSeEhNumero(this.Elementos[index].ToString()))
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                else
                if (this.Elementos[index].GetType() == (typeof(ExpressaoVariavel)))
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                else
                if (this.Elementos[index].GetType() == (typeof(ExpressaoVariavelVetor)))
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoChamadaDeFuncao))
                {
                    ExpressaoChamadaDeFuncao chamada = (ExpressaoChamadaDeFuncao)this.Elementos[index];
                    if (chamada.expressoesParametros != null)
                        for (int x = 0; x < chamada.expressoesParametros.Count; x++)
                            chamada.expressoesParametros[x].PosOrdemExpressao(); // coloca em pos ordem cada expressao que eh  um parametro da chamada de funcao.
           
                    expressaoRetorno.Elementos.Add(chamada);
                }
                else
                if (this.Elementos[index].GetType() == typeof(ExpressaoOperador))
                {
                    Operador op = ((ExpressaoOperador)this.Elementos[index]).operador;
                    op.prioridade += prioridadeParenteses;

                    // verificar o mecanismo de prioridade.
                    while ((!pilha.Empty()) && (pilha.Peek().prioridade >= op.prioridade)) 
                    {
                        Operador op_topo = pilha.Pop();
                        expressaoRetorno.Elementos.Add(new ExpressaoOperador(op_topo));
                    }
                    pilha.Push(op);
                }

                index++;
            }

            while (!pilha.Empty())
            {
                Operador operador = pilha.Pop();
                ExpressaoOperador expressaoOperador = new ExpressaoOperador(operador);
                expressaoRetorno.Elementos.Add(expressaoOperador);
            }
           return expressaoRetorno;
            

        } // PosOrdemExpressao()


        /// Resolve ambiguidades de operadores que são Unario e Binario ao mesmo tempo.
        private static void OperadorUnarioEBinarioAoMesmoTempo(Expressao expressao_main)
        {

            for (int x = 0; x < expressao_main.Elementos.Count; x++)
            {
                Expressao operadoresPolemicos = expressao_main.Elementos[x];

                if ((linguagem.VerificaSeEhOperadorBinario(operadoresPolemicos.ToString())) && (linguagem.VerificaSeEhOperadorUnario(operadoresPolemicos.ToString())))
                {
                    int k = x;

                    if (((k - 1) >= 0) && (linguagem.VerificaSeEhOperador(expressao_main.Elementos[k - 1].ToString())))
                        ((ExpressaoOperador)operadoresPolemicos).operador.SetTipo("UNARIO");
                    else
                    // regra de negócio: se há um operador logo no começo da expressão, ele será unário apenas.
                    if (k == 0)
                        ((ExpressaoOperador)operadoresPolemicos).operador.SetTipo("UNARIO");
                    else
                    // regra de negócio: se  o elemento currente for um parenteses abre, então será unário apenas.
                    if (((k - 1) >= 0) && (expressao_main.Elementos[k - 1].ToString() == "("))
                        ((ExpressaoOperador)operadoresPolemicos).operador.SetTipo("UNARIO");
                    else
                    // regra de negócio: se o elemento posterior for um parenteses fecha, e o elemento currente for um operador, então será unário apenas.
                    if (((k + 1) < expressao_main.Elementos[k].Elementos.Count) &&
                        (expressao_main.Elementos[k + 1].ToString() == ")"))
                        ((ExpressaoOperador)operadoresPolemicos).operador.SetTipo("UNARIO");
                    else
                        ((ExpressaoOperador)operadoresPolemicos).operador.SetTipo("BINARIO"); // se não tiver em nenhuma das regras heurísticas, então o operador é binário.
                } // if
            }
        }

        /// <summary>
        /// encontra um operador dentro de uma lista de operadores, através do nome e do tipo, procurando nas classes de tipos da linguagem.
        /// </summary>
        /// <param name="nomeOperador">nome do operador.</param>
        /// <param name="operadoresPresentes">lista de operadores a ser pesquisada.</param>
        /// <param name="tipoDoOperador">tipo de operando do operador.</param>
        /// <returns>retorna um operador.</returns>
        private Operador FindOperador(string nomeOperador, List<Operador> operadoresPresentes, string tipoDoOperador, string modalidadeOperador)
        {
            Classe classeOp = linguagem.GetClasses().Find(k => k.GetNome() == tipoDoOperador);
            if (classeOp == null)
                return null;
            Operador op = classeOp.GetOperadores().Find(k => k.nome == nomeOperador && k.GetTipo().Contains(modalidadeOperador));
            return op;

        } // FindOperador()


        public static Funcao EncontraFuncaoCompativel(string nomeFuncao, string tipoDaExpressao, Escopo escopo)
        {
            return escopo.tabela.GetFuncao(nomeFuncao, tipoDaExpressao, escopo);
        }

     
        public static bool isExpressionAritmetico(Expressao exprss, Escopo escopo)
        {
            int hasIntVariable = 0;
            int hasOperatorUnary = 0;
            for (int x = 0; x < exprss.Elementos.Count; x++)
            {
                Variavel v = escopo.tabela.GetVar(exprss.Elementos[x].ToString(), escopo);
                if ((v != null) && (v.GetTipo() == "int"))
                {
                    hasIntVariable++;
                    if (hasIntVariable > 1)
                        return false;
                }
                Operador operador = linguagem.GetOperador(exprss.Elementos[x].ToString(), "int");
                if ((operador != null) && (operador.tipo.Contains("UNARIO")))
                {
                    hasOperatorUnary++;
                    if (hasOperatorUnary > 1)
                        return false;
                }
            }
            return (hasIntVariable == 1 && hasOperatorUnary == 1);
        }

        public  bool IsExpressionAtibuicao(Expressao exprss)
        {
            List<string> tokensDaExpressao = exprss.Convert();
            List<string> expressaoComOperadorAtribuicao = tokensDaExpressao.FindAll(k => k.Equals("="));
            return expressaoComOperadorAtribuicao.Count == 1;
        }

        public override string ToString()
        {
            string str = "";
            for (int x = 0; x < this.Elementos.Count; x++)
            {
                if (this.Elementos[x].GetType() == typeof(ExpressaoNumero))
                    str += ((ExpressaoNumero)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoVariavel))
                    str += ((ExpressaoVariavel)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoVariavelVetor))
                    str += ((ExpressaoVariavelVetor)Elementos[x]).ToString() + "[ ]" + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoOperador))
                    str += ((ExpressaoOperador)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoChamadaDeFuncao))
                    str += ((ExpressaoChamadaDeFuncao)Elementos[x]).ToString();

                if (this.Elementos[x].GetType() == typeof(ExpressaoElemento))
                    str += ((ExpressaoElemento)Elementos[x]).ToString() + " ";

            } // for x
            str = str.Trim(' ');
            return str;
        } // ToString()
    } // class expressoes

  
} // namespace 
