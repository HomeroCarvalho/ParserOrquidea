using System.Collections.Generic;
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


        public ExpressaoVariavelVetor(VariavelVetor variavel) : base()
        {
            this.variavel = variavel;
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

        public Expressao ProcessaExpressao(Escopo escopo)
        {
            return new Expressao(this.elementosNaoProcessados.ToArray(), escopo);
        }


        /// <summary>
        /// constroi a expressão, sem colocá-la em Pos-Ordem.
        /// </summary>         
        public Expressao(string[] tokensDaExpressao, Escopo escopo)
        {
            if (tokensDaExpressao.Length == 7)
            {
                int k = 0;
                k++;
            }

            if (escopo == null)
                return;

            if (linguagem == null)
                linguagem = new LinguagemOrquidea();

            this.elementosNaoProcessados = new List<string>();

            GetTipoExpressao(tokensDaExpressao, escopo);

            if (tokensDaExpressao.Length == 1)
            {
                this.Elementos = new List<Expressao>();
                this.Elementos.Add(new ExpressaoElemento(tokensDaExpressao[0]));
                this.Elementos[0].GetTipoExpressao(tokensDaExpressao, escopo);
                return;
            }// if

            this.tokens = tokensDaExpressao.ToList<string>();


            this.codigo = new List<string>();
            this.codigo = this.codigo == null ? new List<string>() : escopo.codigo;


            this.Elementos = new List<Expressao>();
            this.MsgErros = new List<string>();

            escopoDaExpressao = escopo;


            for (int i = 0; i < tokensDaExpressao.Length; i++)
            {
                // o token é um nome de função? (obedece um chamda de um função).
                if (escopo.tabela.IsFunction(tokensDaExpressao[i], escopo) != null)
                {
                    Funcao funcao = escopo.tabela.GetFuncao(tokensDaExpressao[i], this.tipo, escopo);

                    if (funcao == null)
                        return;

                    ExpressaoChamadaDeFuncao exprssFuncao = new ExpressaoChamadaDeFuncao(funcao);

                    if (tokensDaExpressao.Length == 1)
                        return;

                    // retira os tokens da chamada de função.
                    List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(i + 1, "(", ")", tokensDaExpressao.ToList<string>());
                    if ((tokensParametros != null) && (tokensParametros.Count > 0))
                    {
                        tokensParametros.RemoveAt(0); // retira o primeiro parenteses.
                        tokensParametros.RemoveAt(tokensParametros.Count - 1); // retira o ultimo parenteses.

                        //compoe uma lista de expressoes, a partir da lista de tokens calculada acima.
                        List<Expressao> expressoesParametros = ExtraiExpressoes(escopo, tokensParametros);

                        if ((expressoesParametros != null) && (expressoesParametros.Count > 0))
                        {
                            for (int m = 0; m < expressoesParametros.Count; m++)
                                exprssFuncao.Elementos.Add(expressoesParametros[m]);
                            i += tokensDaExpressao.Length; // consome os tokens das expressões-parâmetros, da malha principal de tokens.
                        } // if


                    }//if

                    this.Elementos.Add(exprssFuncao); // adiciona a expressão função para a lista de expressões da expressão construindo.
                } //if
                else
                // o token é um nome de uma variável vetor?
                if (escopo.tabela.GetVarVetor(tokensDaExpressao[i], escopo) != null)
                {


                    // cálculo do seletor de indices da expressão matriz vetor.
                    List<string> expressaoIndice = UtilTokens.GetCodigoEntreOperadores(i, "[", "]", tokensDaExpressao.ToList<string>());


                    int lengthVetor = expressaoIndice.Count;

                    expressaoIndice.RemoveRange(0, 2); // retira o nome do vetor e o operador abre [.
                    expressaoIndice.RemoveAt(expressaoIndice.Count - 1); // retira o operador fecha ].

                    Expressao exprssIndice = new Expressao(expressaoIndice.ToArray(), escopo);
                    VariavelVetor variavelVetor = escopo.tabela.GetVarVetor(tokensDaExpressao[i], escopo);


                    ExpressaoVariavelVetor vExprss = new ExpressaoVariavelVetor(variavelVetor); // constroi a expressao variavel vetor.
                    vExprss.Elementos.Add(exprssIndice); // adiciona o seletor de indices, como uma expressão.

                    this.Elementos.Add(vExprss);

                    i += lengthVetor; // registra o consumo dos tokens do vetor.

                }// if
                else
                // o token é uma variável singular?
                if (escopo.tabela.GetVar(tokensDaExpressao[i], escopo) != null)
                {
                    Variavel v = escopo.tabela.GetVar(tokensDaExpressao[i], escopo);
                    ExpressaoVariavel expressaoVariavel = new ExpressaoVariavel(v);
                    this.Elementos.Add(expressaoVariavel);
                } // if
                else
                // o token é um operador binario?
                if (linguagem.VerificaSeEhOperadorBinario(tokensDaExpressao[i]))
                {

                    if (tokensDaExpressao[i].Equals("="))
                    {
                        int k = 0;
                        k++;
                    }
                    Operador operador = linguagem.GetOperador(tokensDaExpressao[i], this.tipo);
                    ExpressaoOperador expressaoOP = new ExpressaoOperador(operador);

                    this.Elementos.Add(expressaoOP);

                } // else
                else
                // o tokené um operador unario?
                if (linguagem.VerificaSeEhOperadorUnario(tokensDaExpressao[i]))
                {
                    Operador operador = linguagem.GetOperador(tokensDaExpressao[i], this.tipo);
                    ExpressaoOperador expressaoOP = new ExpressaoOperador(operador);

                    this.Elementos.Add(expressaoOP);

                }
                else
                if (linguagem.VerificaSeEhOperador(tokensDaExpressao[i]))
                {
                    Operador operador = linguagem.GetOperador(tokensDaExpressao[i], this.tipo);
                    ExpressaoOperador expressaoOP = new ExpressaoOperador(operador);

                    this.Elementos.Add(expressaoOP);

                }

                else
                // o token é um numero?
                if (linguagem.VerificaSeEhNumero(tokensDaExpressao[i]))
                {
                    ExpressaoNumero numeroExpressao = new ExpressaoNumero(tokensDaExpressao[i]);
                    this.Elementos.Add(numeroExpressao);
                } //else
                else
                // o token é uma variável não inicializada?
                if (linguagem.VerificaSeEhID(tokensDaExpressao[i]))
                    this.Elementos.Add(new ExpressaoElemento(tokensDaExpressao[i]));
                else
                if ((tokensDaExpressao[i] == "(") || (tokensDaExpressao[i] == ")") || (tokensDaExpressao[i] == ","))
                    this.Elementos.Add(new ExpressaoElemento(tokensDaExpressao[i]));
                else
                if (tokensDaExpressao[i] == ";")
                {
                    this.GetTipoExpressao(tokensDaExpressao, escopo);
                    return;
                }
            } // for i

            this.isModdfy = true;
            this.GetTipoExpressao(tokensDaExpressao, escopo);
        } // Expressao()

        // obtem o tipo da expressao, verificando seus elementos, os elementos podem ser variavel, variavelvetor, Objeto, numero, operador, funcao.
        public void GetTipoExpressao(string[] tokensExpressao, Escopo escopo)
        {
            string tipoDaExpressaoParametro = "int";
            this.tipo = "int";

            // a ideia aqui é procurar o tipo da expressão, analizando as variaveis ou objetos presentes na expressão.
            // é de se supor que todos itens da expressão são de variáveis, ou objetos...
            // faz também um  de conversão de tipos.
            for (int x = 0; x < tokensExpressao.Length; x++)
            {
                Variavel v = escopo.tabela.GetVar(tokensExpressao[x].ToString(), escopo);
                if (v != null)
                    tipoDaExpressaoParametro = CastingExpressao(tipo, v.GetTipo());


                VariavelVetor vetor = escopo.tabela.GetVarVetor(tokensExpressao[x].ToString(), escopo);
                if (vetor != null)
                    tipoDaExpressaoParametro = CastingExpressao(tipo, vetor.GetTipo());


                Objeto objeto = escopo.tabela.GetObjeto(tokensExpressao[x].ToString());
                if (objeto != null)
                    tipoDaExpressaoParametro = CastingExpressao(tipo, objeto.GetClasse());


                if (escopo.tabela.IsFunction(escopo, tokensExpressao[x]))
                {
                    List<Funcao> fnc = escopo.tabela.GetFuncao(tokensExpressao[x]);
                    if (fnc.Count > 0)
                    {
                        string tipoFuncao = fnc[0].tipoDoRetornoDaFuncao;
                        tipo = CastingExpressao(tipo, tipoFuncao);
                    }
                }


                if (linguagem.isOperador(tokensExpressao[x]))
                {
                    Operador operador = linguagem.GetOperador(tokensExpressao[x], this.tipo);
                    if (operador != null)
                        tipo = CastingExpressao(tipo, operador.GetTipo());
                }


                if (linguagem.VerificaSeEhNumero(tokensExpressao[x]))
                {
                    string tipoNumero;
                    string str_numero = tokensExpressao[x];
                    try
                    {
                        int iNumero = 0;
                        iNumero = int.Parse(str_numero);
                        tipoNumero = "int";
                    }
                    catch
                    {
                        try
                        {
                            float fNumero = 0.0f;
                            fNumero = float.Parse(str_numero);
                            tipoNumero = "float";
                        }
                        catch
                        {
                            tipoNumero = "int";
                        }
                    }
                    tipo = CastingExpressao(tipo, tipoNumero);
                }

            } // for x
            this.tipo = CastingExpressao(tipo, tipoDaExpressaoParametro);
        }

        private string CastingExpressao(string currenteTipo, string castingTipo)
        {
            if (currenteTipo == "Int32")
                currenteTipo = "int";
            if ((currenteTipo == "int") && (castingTipo == "int"))
                return "int";
            if ((currenteTipo == "int") && (castingTipo == "float"))
                return "float";
            if ((currenteTipo == "float") && (castingTipo == "int"))
                return "float";
            return currenteTipo;
        } // ()



        public List<Expressao> ExtraiExpressoes(Escopo escopo, List<string> tokensRaw)
        {
            if (tokensRaw.Count == 1)
                return new List<Expressao>() { new ExpressaoElemento(tokensRaw[0]) };

            List<string> tokens = tokensRaw.ToList<string>();
            List<Expressao> TOKENS_EXPRESSAO_CURRENTE = new List<Expressao>();

            List<Expressao> expressoesExtraidas = new List<Expressao>();
            int pilhaInteiroParenteses = 0;
            int indexToken = 0;
            while ((indexToken >= 0) && (indexToken < tokens.Count))
            {
                if ((tokens[indexToken] == "@") && ((indexToken + 1) < tokens.Count))
                {
                    Expressao expressaoElementoID = new ExpressaoElemento(tokens[indexToken + 1]);
                    expressaoElementoID.tipo = "string";

                    TOKENS_EXPRESSAO_CURRENTE.Add(expressaoElementoID);

                    tokens.RemoveRange(0, 2);
                    indexToken = 0;

                    if ((indexToken < tokens.Count) && (tokens[indexToken] == ","))
                        tokens.RemoveAt(0);
                }
                else
                if (linguagem.isOperador(tokens[indexToken]))
                {
                    List<string> tokensExprss = tokens.GetRange(0, indexToken);
                    Expressao umOperando = new Expressao(tokensExprss.ToArray(), escopo);
                    Expressao umOperador = new ExpressaoElemento(tokens[indexToken]);


                    TOKENS_EXPRESSAO_CURRENTE.Add(umOperando);
                    TOKENS_EXPRESSAO_CURRENTE.Add(umOperador);

                    tokens.RemoveRange(0, tokensExprss.Count + 1);
                    indexToken = 0;
                }
                else
                if (tokens[indexToken] == "(")
                {
                    pilhaInteiroParenteses++;
                    indexToken++;
                }
                else
                if (tokens[indexToken] == ")")
                {
                    pilhaInteiroParenteses--;

                    if (pilhaInteiroParenteses == 0)
                    {
                        if (((indexToken + 1) < tokens.Count) && (linguagem.isOperador(tokens[indexToken + 1])))
                            indexToken++; // há mais tokens na expressao para extrair.

                        int indexTokensAbreParenteses = tokens.IndexOf("(");
                        int indexTokensFechaParenteses = tokens.IndexOf(")");

                        if ((indexTokensAbreParenteses < 0) || (indexTokensAbreParenteses - 1 < 0))
                        {
                            indexToken++;
                            continue;
                        }


                        string nomeFuncao = tokens[indexTokensAbreParenteses - 1];
                        int lengthChamadaFuncao = indexTokensFechaParenteses - indexTokensAbreParenteses + 1;
                        List<string> tokensDeUmaChamadaDeFuncao = tokens.GetRange(indexTokensAbreParenteses, indexTokensFechaParenteses - indexTokensAbreParenteses + 1);

                        tokensDeUmaChamadaDeFuncao.RemoveAt(0);
                        tokensDeUmaChamadaDeFuncao.RemoveAt(tokensDeUmaChamadaDeFuncao.Count - 1);



                        List<Expressao> expressoesDaChamadaDeFuncao = ExtraiExpressoes(escopo, tokensDeUmaChamadaDeFuncao); // chamada recursiva para extrai expressoes de uma chamada de funcao.
                        Funcao fnc = this.ObtemFuncaoCompativelComAChamadaDeFuncao(nomeFuncao, expressoesDaChamadaDeFuncao, escopo);
                        if (fnc == null)
                        {
                            escopo.GetMsgErros().Add("funcao compativel nao encontrada, em: " + Util.UtilString.UneLinhasLista(tokens));
                            return new List<Expressao>();
                        }


                        ExpressaoChamadaDeFuncao expressaoFuncao = new ExpressaoChamadaDeFuncao(fnc);
                        if ((expressoesDaChamadaDeFuncao != null) && (expressoesDaChamadaDeFuncao.Count > 0))
                            expressaoFuncao.expressoesParametros = expressoesDaChamadaDeFuncao; // adiciona as expressoes que formam os parametros da chamada de funcao!
                        else
                            expressaoFuncao.expressoesParametros = new List<Expressao>();

                        TOKENS_EXPRESSAO_CURRENTE.Add(expressaoFuncao);

                        indexToken = 0;
                        tokens.RemoveRange(0, lengthChamadaFuncao + indexTokensAbreParenteses);
                    }

                    if (pilhaInteiroParenteses < 0)
                    {
                        escopo.GetMsgErros().Add("Erro de colocacao de parenteses em chamada de funcao;");
                        return new List<Expressao>();
                    }
                }
                else
                if ((tokens[indexToken] == ",") && ((indexToken + 1) < tokens.Count))
                {

                    List<string> tokensExpressao = tokens.GetRange(0, indexToken + 1 - 1); // +1 porque o indice está sendo usado como contador.-1 para retirar o operador virgula.
                    if (tokensExpressao.Count > 0)
                    {
                        Expressao expressao = new Expressao(tokensExpressao.ToArray(), escopo);
                        TOKENS_EXPRESSAO_CURRENTE.Add(expressao);
                    }
                    tokens.RemoveRange(0, indexToken + 1 - 1 + 1); // retira os tokens da expressao formada, e o operador virgula.
                    indexToken = 0;
                }
                else
                if (tokens[indexToken] == ";")
                {
                    List<string> tokensDeUmaExpressao = tokens.GetRange(0, indexToken + 1); // +1 pois o indice está sendo utilizado como cumprimento de lista.
                    Expressao expressao = new Expressao(tokensDeUmaExpressao.ToArray(), escopo);
                    if (expressao != null)
                    {

                        TOKENS_EXPRESSAO_CURRENTE.Add(expressao);
                        expressoesExtraidas.Add(UneExpressoes(TOKENS_EXPRESSAO_CURRENTE, escopo));
                        tokens.RemoveRange(0, tokensDeUmaExpressao.Count);
                        indexToken = 0;
                        TOKENS_EXPRESSAO_CURRENTE.Clear();
                    }
                    else
                        indexToken++;
                }
                else
                if (tokens[indexToken] == "{")
                    return expressoesExtraidas;
                else
                    indexToken++;
            }

            if (tokens.Count > 0)
            {
                Expressao expressao = new Expressao(tokens.ToArray(), escopo);
                if (expressao != null)
                    TOKENS_EXPRESSAO_CURRENTE.Add(expressao);
            }


            if (TOKENS_EXPRESSAO_CURRENTE.Count > 0)
                expressoesExtraidas.Add(UneExpressoes(TOKENS_EXPRESSAO_CURRENTE, escopo));


            return expressoesExtraidas;
        }


        private Expressao UneExpressoes(List<Expressao> expressoes, Escopo escopo)
        {
            Expressao expressRetorno = new Expressao();
            if ((expressoes != null) && (expressoes.Count > 0))
            {
                foreach (Expressao umaExpressaoParte in expressoes)
                    expressRetorno.Elementos.Add(umaExpressaoParte);
            }
            return expressRetorno;

        }

        private Funcao ObtemFuncaoCompativelComAChamadaDeFuncao(string nomeMetodo, List<Expressao> expressoesChamada, Escopo escopo)
        {
            List<Funcao> FuncoesCandidatosDaChamada = escopo.tabela.GetFuncao(nomeMetodo);
            for (int umaFuncao = 0; umaFuncao < FuncoesCandidatosDaChamada.Count; umaFuncao++)
            {
                int x = 0;
                bool isFound = true;

                for (x = 0; x < expressoesChamada.Count; x++)
                {
                    isFound = TipoNumericoExpressao(expressoesChamada[x], FuncoesCandidatosDaChamada[umaFuncao]);
                    if (isFound)
                        continue;
                    else
                    if (!isFound)
                    {
                        isFound = true;
                        expressoesChamada[x].GetTipoExpressao(expressoesChamada[x].tokens.ToArray(), escopo);
                        if (expressoesChamada[x].tipo != FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao[x].tipo)
                        {
                            isFound = false;
                            break;
                        }
                    } // if isFound
                    if (RepositorioDeClassesOO.Instance().classesRegistradas.Find(k => k.nome == expressoesChamada[x].tipo) == null)
                    {
                        isFound = false;
                        break;
                    }

                }
                if (isFound)
                    return FuncoesCandidatosDaChamada[umaFuncao];
            }
            return null;
        }

        private bool TipoNumericoExpressao(Expressao expressoesChamada, Funcao FuncaoCandidataDaChamada)
        {
            if (linguagem.VerificaSeEhNumero(expressoesChamada.ToString()))
            {
                int x = 0;
                for (x = 0; x < FuncaoCandidataDaChamada.parametrosDaFuncao.Length; x++)
                {
                    if (IsTipoInteiro(expressoesChamada.ToString()))
                    {
                        if ((FuncaoCandidataDaChamada.parametrosDaFuncao[x].tipo == "int") ||
                           (FuncaoCandidataDaChamada.parametrosDaFuncao[x].tipo == "Int32"))
                            continue;
                        else
                            return false;
                    }
                    if (IsTipoFloat(expressoesChamada.ToString()))
                        if (FuncaoCandidataDaChamada.parametrosDaFuncao[x].tipo == "float")
                            return true;
                        else
                            return false;
                }

            }

            return false;
        }

        public bool IsTipoInteiro(string str_numero)
        {
            try
            {
                int result = int.Parse(str_numero);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsTipoFloat(string str_numero)
        {
            try
            {
                float result = float.Parse(str_numero);
                return true;
            }
            catch
            {
                return false;
            }
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

        /// <summary>
        /// valida a solocação lógica sequencial de variáveis e operadores, e funções
        /// </summary>
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



        // valida uma expressão totalmente aritimetica binaria.
        public bool ValidaExpressaoAritmeticaBinaria(Expressao exprss)
        {
            LinguagemOrquidea linguagem = new LinguagemOrquidea();
            int index = linguagem.GetOperadores().FindIndex(k => k.GetTipo().Contains("OPERADOR") && k.GetTipo().Contains("BINARIO"));
            return index != -1;
        }
        // valida uma expressão totalmente aritimetica e unária.
        public bool ValidaExpressaoAritmeticaUnaria(Expressao exprss)
        {
            LinguagemOrquidea linguagem = new LinguagemOrquidea();

            int index = linguagem.GetOperadores().FindIndex(k => k.GetTipo().Contains("OPERADOR") && k.GetTipo().Contains("UNARIO"));
            return index != -1;
        } // ValidaExpressaoAritmeticaUnaria()

        public bool ValidaExpressaoAritmetica(Expressao exprss)
        {
            if (ValidaExpressaoAritmeticaUnaria(exprss))
                return true;
            if (ValidaExpressaoAritmeticaBinaria(exprss))
                return true;
            return false;
        } // ValidaExpressaoAritmetica()

        public Expressao PosOrdemExpressao()
        {

            if ((this.Elementos == null) || (this.Elementos.Count == 0) || (this.Elementos.Count == 1))
                return this;

            Expressao expss = this;


            Expressao expressaoRetorno = new Expressao();
                    
            posicaoDaExpressao = new PosicaoECodigo(expss.Convert(), escopoDaExpressao.codigo);

            List<string> elementos = expss.Convert();
            List<Operador> operadoresPresentes = new List<Operador>();

            int x = 0;
            while (x < elementos.Count)
            {

                // o elemento é um nome de função?
                if (escopoDaExpressao.tabela.IsFunction(elementos[x], escopoDaExpressao) != null)
                {

                    Expressao chamadaDeFuncao = (Expressao)expss.Elementos[x];  // obtém uma chamada de função da função encontrada. Os parâmetros da chamada estarão em pos-ordem.

                    if (chamadaDeFuncao.GetType() == typeof(ExpressaoChamadaDeFuncao))
                    {
                        Funcao umaFuncao = (Funcao)chamadaDeFuncao.GetElemento();  // obtém o nome da função que faz a chamada.


                        List<string> tokensDaChamadaDeFuncao = chamadaDeFuncao.Convert(); // obtém os tokens da chamada.
                        if (umaFuncao != null)

                        {
                            // seta uma prioridade muito alta, pois funções são as primeira a avaliar.
                            umaFuncao.prioridade += 300;
                            string[] parametrosOperador = new string[umaFuncao.parametrosDaFuncao.Length];
                            for (int i = 0; i < parametrosOperador.Length; i++)
                                parametrosOperador[i] = umaFuncao.parametrosDaFuncao[i].tipo;

                            // adiciona a função como se for um operador. o operador criado é unário,pois a função normalmente retorna um valor só.
                            operadoresPresentes.Add(new Operador(umaFuncao.tipoDoRetornoDaFuncao, umaFuncao.nome, umaFuncao.prioridade, parametrosOperador, "UNARIO", umaFuncao));
                        } //if

                        // remove os elementos da chamada da função, menos o nome da expressão que está no indice 0;
                        elementos.RemoveRange(1, tokensDaChamadaDeFuncao.Count - 1);


                    } //if
                } // if isFunction.
                else
                if ((linguagem.GetOperador(elementos[x], tipo) != null) && (elementos[x] != "."))
                {
                    // o elemento é um operador.
                    operadoresPresentes.Add(linguagem.GetOperador(elementos[x], tipo));
                    operadoresPresentes[operadoresPresentes.Count - 1].indexPosOrdem = x;
                }
                x++; // //passa para o próximo token.
            } // while x

            // resolve ambiguidade de operadores que são unários e binários ao mesmo tempo. segue regras heurísticas para determinar se o operador é unário ou binário.
            OperadorUnarioEBinarioAoMesmoTempo(elementos, operadoresPresentes);

            // inicializa a pilha de operadores a ser processados da notação infixa para posfixa.
            Stack<Operador> pilha = new Stack<Operador>();
            int umToken = 0;
            try
            {
                while (umToken < elementos.Count)
                {
                    // obtém o elemento currente a ser processado.
                    string elemento = elementos[umToken];
                    if (RepositorioDeClassesOO.Instance().ObtemUmaClasse(elemento) != null)
                    {
                        umToken++;
                        continue;
                    }
                    else
                    // o elemento seguinte é um token de operador de matriz?
                    if (((umToken + 1) < elementos.Count) && (elementos[umToken] == "["))
                    {
                        Expressao exprssMatriz = (Expressao)expss.Elementos[umToken];
                        Expressao exprssMatrizSaida = exprssMatriz.PosOrdemExpressao();

                        if ((exprssMatrizSaida != null) && (exprssMatrizSaida.Elementos != null) &&
                            (exprssMatrizSaida.Elementos.Count > 0))
                            // adiciona a expressão matriz na lista de sub-expressoes.
                            expressaoRetorno.Elementos.Add(exprssMatrizSaida);

                    } // if


                    if (elemento.Equals("("))
                    {
                        // do nothing.

                    } // if elemento=="("
                    else
                    if (elemento.Equals(")"))
                    {
                        // adiciona os operadores armazenados, pois possuem prioridade maior entre os demais fora do parenteses
                        while (pilha.Count > 0)
                        {
                            expressaoRetorno.Elementos.Add(new ExpressaoElemento(pilha.Pop().ToString()));
                        }

                        // retira o parentes da pilha.
                        pilha.Pop();
                    } // if elemento==")".
                    else

                    if ((linguagem.VerificaSeEhOperador(elemento)) && (!elemento.Equals("."))) 
                    {
                        // verifica se o operador é binário.
                        Operador op = FindOperador(elemento, operadoresPresentes, expss.tipo, "BINARIO");
                        if (op != null)
                        {
                            if (pilha.Count == 0)
                                pilha.Push(op);
                            else
                            {
                                // obtém o operador de nome contido em [elemento].
                                Operador opElemento = FindOperador(elemento, operadoresPresentes, expss.tipo, "BINARIO");
                                // retira o operador do topo da pilha.
                                Operador opTopoPilha = pilha.Peek();


                                // operador binário, varia a posição de acordo com a prioridade.
                                while ((pilha.Count > 0) &&
                                    (opElemento.GetPrioridade() <= opTopoPilha.GetPrioridade()) &&
                                    (opElemento.GetPrioridade() < 200) &&
                                    (opTopoPilha.GetPrioridade() < 200))
                                {

                                    string nomeOperadorTopoPilha = pilha.Pop().nome;
                                    Operador operador = operadoresPresentes.Find(k => k.nome == nomeOperadorTopoPilha);
                                    ExpressaoOperador opExprssBinario = new ExpressaoOperador(operador);
                                    expressaoRetorno.Elementos.Add(opExprssBinario);
                                } // while
                                pilha.Push(op);
                            } // else
                        } // if elemento== Operador Binário.
                        else
                        {
                            Operador opUnario = FindOperador(elemento, operadoresPresentes, expss.tipo, "UNARIO");
                            if (opUnario == null)
                            {
                                MsgErros.Add("Operador: " + elemento + " nao encontrado, para o tipo: " + expss.tipo);
                                return null;
                            }

                            if (opUnario.GetTipo().Contains("UNARIO"))
                            {
                                ExpressaoOperador opExpss = new ExpressaoOperador(opUnario);
                                // o operado é unário.
                                expressaoRetorno.Elementos.Add(opExpss);// prioridade imediata (a mais alta prioridade).

                            } // if opUnario
                        }// else
                    } // if SeEh Operador Unário.
                    else
                    // o elemento é um operando?
                    if ((linguagem.VerificaSeEhID(elemento)) && (!linguagem.VerificaSeEhNumero(elemento)))
                    {
                        Variavel v = escopoDaExpressao.tabela.GetVar(elemento, escopoDaExpressao);
                        
                        ExpressaoVariavel vexpss = new ExpressaoVariavel(v);
                        
                        expressaoRetorno.Elementos.Add(vexpss);

                    }
                    else
                    // o elemento é um número? 
                    if (linguagem.VerificaSeEhNumero(elemento))
                    {
                        ExpressaoNumero numero = new ExpressaoNumero(elemento);
                        expressaoRetorno.Elementos.Add(numero);
                    } // if

                    // passa para o próximo elemento.
                    umToken++;
                } // while
                // completa a expressão, acrescentando operandos  e operadores que não entraram ainda na expressão.
                while (pilha.Count > 0)
                {
                    string operadorNaPilha = pilha.Pop().nome;
                    Operador op = operadoresPresentes.Find(k => k.nome == operadorNaPilha);
                    expressaoRetorno.Elementos.Add(new ExpressaoOperador(op));
                } // while()
            } // try
            catch
            {
                MsgErros.Add("Erro ao converter a expressão: " + expss.ToString() + " para a notação pós-fixa (posição da expressão: linha: " + posicaoDaExpressao.linha + " coluna: " + posicaoDaExpressao.coluna + ")");
                return null;
            } // catch

            expressaoRetorno.isInPosOrdem = true;
            return expressaoRetorno;
        } // PosOrdemExpressao()

        
        /// <summary>
        /// Resolve ambiguidades de operadores que são Unario e Binario ao mesmo tempo.
        /// </summary>
        private static void OperadorUnarioEBinarioAoMesmoTempo(List<string> elementos, List<Operador> operadoresPresentes)
        {
            for (int k = 0; k < elementos.Count; k++)
            {
                if ((linguagem.VerificaSeEhOperadorBinario(elementos[k])) && (linguagem.VerificaSeEhOperadorUnario(elementos[k])))
                {
                    int index = operadoresPresentes.FindIndex(m => m.indexPosOrdem == k);
                    // regra de negócio: se há dois operadores em sequencia, e o operador currente é binário e unário, então será unário apenas.
                    if ((index != -1) && ((k - 1) >= 0) && (linguagem.VerificaSeEhOperador(elementos[k])) && (linguagem.VerificaSeEhOperador(elementos[k - 1])))
                        operadoresPresentes[index].SetTipo("OPERADOR UNARIO");
                    else
                    // regra de negócio: se há um operador logo no começo da expressão, ele será unário apenas.
                    if ((index != -1) && (k == 0))
                        operadoresPresentes[index].SetTipo("OPERADOR UNARIO");
                    else
                    // regra de negócio: se  o elemento currente for um parenteses abre, então será unário apenas.
                    if (((k - 1) >= 0) && (elementos[k - 1] == "(") && (index != -1))
                        operadoresPresentes[index].SetTipo("OPERADOR UNARIO");
                    else
                    // regra de negócio: se o elemento posterior for um parenteses fecha, e o elemento currente for um operador, então será unário apenas.
                    if (((k + 1) < elementos.Count) && (index != -1) && (elementos[k + 1] == ")"))
                        operadoresPresentes[index].SetTipo("OPERADOR UNARIO");
                    else
                    if (index != -1)
                        operadoresPresentes[index].SetTipo("OPERADOR BINARIO"); // se não tiver em nenhuma das regras heurísticas, então o operador é binário.
                } // if
            } // for k
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

        /// <summary>
        /// retorna o tipo da expressão, sub-classes.
        /// </summary>
        public Expressao GetTypeExpression(Expressao exprss)
        {
            if (exprss.GetType() == typeof(ExpressaoChamadaDeFuncao))
                return (ExpressaoChamadaDeFuncao)exprss;
            if (exprss.GetType() == typeof(ExpressaoElemento))
                return (ExpressaoElemento)exprss;
            if (exprss.GetType() == typeof(ExpressaoNumero))
                return (ExpressaoNumero)exprss;
            if (exprss.GetType() == typeof(ExpressaoOperador))
                return (ExpressaoOperador)exprss;
            if (exprss.GetType() == typeof(ExpressaoVariavel))
                return (ExpressaoVariavel)exprss;
            if (exprss.GetType() == typeof(ExpressaoVariavelVetor))
                return (ExpressaoVariavelVetor)exprss;

            return (Expressao)exprss;

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
                    str += ((ExpressaoChamadaDeFuncao)Elementos[x]).ToString()+ "( )" + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoElemento))
                    str += ((ExpressaoElemento)Elementos[x]).ToString() + " ";

            } // for x
            str = str.Trim(' ');
            return str;
        } // ToString()
    } // class expressoes

  
} // namespace 
