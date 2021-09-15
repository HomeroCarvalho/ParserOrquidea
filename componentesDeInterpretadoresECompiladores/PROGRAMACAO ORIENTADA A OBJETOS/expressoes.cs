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

    public class ExpressaoAtribuicaoPropriedadesAninhadas: Expressao
    { 
        // é preciso uma atribuicao para validar a atribuicao de propriedades aninhadas.
        public Objeto objetoInicial;
        public Expressao expresaoAtribuicao;
        public List<Objeto> aninhamento;


        public static bool IsProopriedadesAninhadas(Expressao exprssCurrente, Escopo escopo, int indexTokenObjeto)
        {
            Objeto objInicial = escopo.tabela.GetObjeto(exprssCurrente.tokens[indexTokenObjeto], escopo);
            if (objInicial == null)
                return false;
            string nomePropriedade = exprssCurrente.tokens[indexTokenObjeto + 2].ToString();
            if (objInicial.GetField(nomePropriedade) == null)
                return false;

            if (exprssCurrente.tokens.IndexOf("=") == -1)
                return false;
            return true;
        }

        public static bool AtualizaExpressao(Expressao exprssMain, Escopo escopo)
        {
            ExpressaoAtribuicaoPropriedadesAninhadas exprssProcessada = new ExpressaoAtribuicaoPropriedadesAninhadas();
            exprssProcessada.aninhamento = new List<Objeto>();


            exprssProcessada.tokens = exprssMain.tokens.ToList<string>();
            exprssProcessada.indexToken = exprssMain.indexToken;

            Objeto objInicial = null;

            Classe classeAninhada = RepositorioDeClassesOO.Instance().ObtemUmaClasse(exprssProcessada.tokens[exprssProcessada.indexToken]);
            if (classeAninhada != null) // objeto inicial é um objeto estático.
            {
                // obtem o objeto inicial, se o objeto nao tiver registro, é um objeto estático da classe.
                string nomePropriedadeEstatica = exprssProcessada.tokens[exprssProcessada.indexToken].ToString(); // +1 do operador ".", +1 do nome daa classe contido no inicio da expressao.
                objInicial = classeAninhada.propriedadesEstaticas.Find(k => k.GetNome() == nomePropriedadeEstatica); // obtem o objeto inicial, como sendo a propriedade estática.
            }
            else
                objInicial = escopo.tabela.GetObjeto(exprssProcessada.tokens[exprssProcessada.indexToken], escopo); // obtem o objeto inicial, que faz a chamada de funcoes e propriedades aninhados.

            exprssProcessada.objetoInicial = new Objeto(objInicial); // atribui ao objeto inicial, o objeto inicial da expressao.
            exprssProcessada.indexToken += 2; // consome os tokens do objeto inicial, e do operador ".", utilizado para a referencia das propriedades/metodos aninhados.
            exprssProcessada.aninhamento.Add(objInicial);



            while ((exprssProcessada.indexToken < exprssProcessada.tokens.Count) &&
                ((objInicial.GetField(exprssProcessada.tokens[exprssProcessada.indexToken]) != null) || (exprssProcessada.tokens[exprssProcessada.indexToken] == "."))) // faz uma varredura, obtendo chamadas de funções, e objetos, aninhados, e de tokens "." dot.
            {

                if (objInicial.GetField(exprssProcessada.tokens[exprssProcessada.indexToken])!=null)
                {   // o token é um campo do objeto currente de propriedade aninhada.
                    if (objInicial == null)
                    {
                        if (ExpressaoObjeto.AtualizaExpressao(escopo, exprssProcessada.tokens[exprssProcessada.indexToken], exprssProcessada))
                            objInicial = ((ExpressaoObjeto)exprssProcessada.Elementos[exprssProcessada.Elementos.Count - 1]).objeto;
                    }
                    if (Objeto.GetCampo(objInicial.GetTipo(), exprssProcessada.tokens[exprssProcessada.indexToken]) != null) //indices: +1 do token nome do objeto, 1 do operador ".".
                    {
                        string nomeObjetoPrincipal = objInicial.GetNome();
                        objInicial = Objeto.GetCampo(objInicial.GetTipo(), exprssProcessada.tokens[exprssProcessada.indexToken]); // obtem o campo aninhado.
         
                        
                        if (objInicial == null)
                        {
                            UtilTokens.WriteAErrorMensage(escopo, "propriedade: " + exprssProcessada.tokens[exprssProcessada.indexToken] + " nao pertence a classe: " + objInicial.GetTipo() + ", erro na expressao: " + Utils.UneLinhasPrograma(exprssProcessada.tokens) + ".", exprssProcessada.tokens);
                            return false;
                        }

                        exprssProcessada.aninhamento.Add(objInicial);
                        ExpressaoObjeto.AtualizaExpressao(escopo, objInicial.GetNome(), exprssProcessada);
                        
                        
                        exprssProcessada.indexToken += 2; // +1 do nome do campo do objeto, +1 do token operador.
                    }
                }
                else
                if (exprssProcessada.tokens[exprssProcessada.indexToken] == ".")
                    exprssProcessada.indexToken += 1;  // o token eh um ".", separador de propriedades/metodos aninhados.
                else
                    UtilTokens.WriteAErrorMensage(escopo, "propriedade/metodo aninhado: " + exprssProcessada.tokens[exprssProcessada.indexToken] + ", na expressao: " + Util.UtilString.UneLinhasLista(exprssProcessada.tokens.ToList<string>()) + " nao reconhecida.", exprssProcessada.tokens.ToList<string>());
               
            } // while

            exprssProcessada.indexToken -= 2;
            if (exprssProcessada.indexToken < exprssProcessada.tokens.Count)
            {
                int indiceAtribuicao = exprssProcessada.tokens.IndexOf("=") + 1;
                List<string> tokensAtribuicao = exprssProcessada.tokens.GetRange(indiceAtribuicao, exprssProcessada.tokens.Count - indiceAtribuicao);

                if (tokensAtribuicao.Count > 0)
                {
                    Expressao exprssAtribuicao = new Expressao(tokensAtribuicao.ToArray(), escopo); // compoe a expressao de atribuicao.
                    if (exprssAtribuicao != null)
                    {
                        exprssProcessada.expresaoAtribuicao = exprssAtribuicao;
                        exprssProcessada.indexToken += tokensAtribuicao.Count + 2;  // +1 do parenteses abre, +1 do parenteses fecha.
                    } // if
                    else
                    {
                        UtilTokens.WriteAErrorMensage(escopo, "expressao de atribuicao de propriedades aninhadas inexistente.", exprssMain.tokens);
                        return false;
                    } // else
                } // if
            } // if
            exprssMain.Elementos.Add(exprssProcessada);
            exprssMain.indexToken = exprssProcessada.indexToken;

            return true;
        }

        public override string ToString()
        {
            if ((aninhamento == null) || (aninhamento.Count == 0))
                return "";
            string str = "";
            for (int x = 0; x < aninhamento.Count - 1; x++)
                str += aninhamento[x].ToString() + ".";
            str += aninhamento[aninhamento.Count - 1].ToString();
            if (expresaoAtribuicao != null)
                str += "= " + expresaoAtribuicao.ToString();
            return str;
        }
    }
    public class ExpressaoChamadaDeMetodo : Expressao
    {
        public Objeto objectCaller;
        public List<ExpressaoChamadaDeFuncao> chamadaDoMetodo;
        public ExpressaoAtribuicaoPropriedadesAninhadas proprieades;
        public static bool IsChamadaDeMetodo(Expressao exprssCurrente, Escopo escopo, int indexTokenObjeto)
        {
            

            if ((indexTokenObjeto + 2) > exprssCurrente.tokens.Count)
                return false;

            Objeto objInicial = escopo.tabela.GetObjeto(exprssCurrente.tokens[indexTokenObjeto], escopo);
            if (objInicial == null)
                return false;

            
            string nomeMetodo = exprssCurrente.tokens[indexTokenObjeto + 2];
            string nomeClasseDoMetodo = objInicial.GetTipo();
            Classe classeDoMetodo = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasseDoMetodo);
         

            if (classeDoMetodo == null)
                return false;

            if (classeDoMetodo.GetMetodos().Find(k => k.nome == objInicial.GetTipo()) == null) 
                return false;

            int firstParenteses = exprssCurrente.tokens.IndexOf("(");
            if (firstParenteses == -1)
                return false;

            List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(firstParenteses, "(", ")", exprssCurrente.tokens);
            if (tokensParametros == null)
                return false;

            tokensParametros.RemoveAt(0);
            tokensParametros.RemoveAt(tokensParametros.Count - 1);


            List<Expressao> parametros= Expressao.Instance.ExtraiExpressoes(escopo, tokensParametros);
            if (UtilTokens.ObtemFuncaoCompativelComAChamadaDeFuncao(nomeMetodo, parametros, escopo) != null)
                return true;
            return false;
           
        }
        public static bool AtualizaExpressao(Escopo escopo, Expressao exprssMain)
        {
            // template: obj . metodo(x,...);
            ExpressaoChamadaDeMetodo exprssCurrente = new ExpressaoChamadaDeMetodo();
            
            exprssCurrente.indexToken = exprssMain.indexToken;
            exprssCurrente.tokens = exprssMain.tokens.ToList<string>();



         

            // obtem o nome do objeto que chama os metodos, e guarda na proprieddade [objCaller]
            string nomeObjeto = exprssCurrente.tokens[exprssCurrente.indexToken];
            Objeto objectCaller = escopo.tabela.GetObjeto(nomeObjeto, escopo);
            if (objectCaller == null)
                return false;


            exprssCurrente.proprieades = new ExpressaoAtribuicaoPropriedadesAninhadas();
            exprssCurrente.chamadaDoMetodo = new List<ExpressaoChamadaDeFuncao>();
            exprssCurrente.objectCaller = objectCaller;


            string nomeMetodoInicial = exprssCurrente.tokens[exprssCurrente.indexToken + 2];
            string nomeClasseCurrente = objectCaller.GetTipo();



            exprssCurrente.indexToken += 2; // consome os tokens do objeto inicial, e do operador ".", utilizado para a referencia das propriedades/metodos aninhados.
            int indexTokenPropriedades = exprssCurrente.indexToken;
           
          
            while ((exprssCurrente.indexToken<exprssCurrente.tokens.Count) &&
                (
                (ExpressaoChamadaDeFuncao.IsChamadaDeFuncao(exprssCurrente.tokens[exprssCurrente.indexToken], nomeClasseCurrente, escopo)) ||
                (objectCaller.GetField(exprssCurrente.tokens[exprssCurrente.indexToken]) != null) ||
                (exprssCurrente.tokens[exprssCurrente.indexToken] == "."))) // faz uma varredura, obtendo chamadas de funções, e objetos, aninhados, e de tokens "." dot.
            {
                if (ExpressaoChamadaDeFuncao.IsChamadaDeFuncao(exprssCurrente.tokens[exprssCurrente.indexToken], nomeClasseCurrente, escopo))
                {
                    int firstParenteses = exprssCurrente.tokens.IndexOf("(");
            
                    
                    List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(firstParenteses, "(", ")", exprssCurrente.tokens);
                    tokensParametros.RemoveAt(0);
                    tokensParametros.RemoveAt(tokensParametros.Count - 1);



                    List<Expressao> parametros = Expressao.Instance.ExtraiExpressoes(escopo, tokensParametros);


                    // o token é de uma chamada de função.
                    string nomeMetodo = exprssCurrente.tokens[exprssCurrente.indexToken];
                    Funcao metodo = UtilTokens.ObtemFuncaoCompativelComAChamadaDeFuncao(nomeMetodo, parametros, escopo);
                    if (metodo == null)
                    {
                        UtilTokens.WriteAErrorMensage(escopo, "erro no processament de uma expressao que contem uma chamada de metodo. ", exprssCurrente.tokens);
                        return false;
                    }


                    ExpressaoChamadaDeFuncao chamadaFuncao = new ExpressaoChamadaDeFuncao(metodo);
                    chamadaFuncao.expressoesParametros.AddRange(parametros);
                    exprssCurrente.chamadaDoMetodo.Add(chamadaFuncao);



                    nomeClasseCurrente = metodo.tipoReturn; // seta o nome da classe para validar a próxima chamada de função.

                    exprssCurrente.indexToken += tokensParametros.Count + 2; // +1 do parenteses abre da interface da chamada de funcao, +1 do parenteses fechada.

                }
                else
                if (objectCaller.GetField(exprssCurrente.tokens[exprssCurrente.indexToken]) != null)
                {
                    string nomeCampo = exprssCurrente.tokens[exprssCurrente.indexToken];
                    exprssCurrente.proprieades.aninhamento.Add(objectCaller.GetField(nomeCampo));
                    objectCaller = objectCaller.GetField(nomeCampo);

                    nomeClasseCurrente = objectCaller.GetTipo(); // seta o nome da classe para validar a próxima chamada de função.
                    exprssCurrente.indexToken++;
                }
                else
                if (exprssCurrente.tokens[exprssCurrente.indexToken] == ".") 
                {
                    exprssCurrente.indexToken++;
                }
                else
                {
                    UtilTokens.WriteAErrorMensage(escopo, "erro em uma expressao de chamada de metodo. ", exprssCurrente.tokens);
                    return false;
                }
            }


            exprssMain.Elementos.Add(exprssCurrente);
            exprssMain.indexToken = exprssCurrente.indexToken;
            return true;
        }


        public override string ToString()
        {
            string str = this.objectCaller.GetNome()+".";


            if ((this.proprieades != null) && (this.proprieades.aninhamento != null)) 
                for (int x = 0; x < this.proprieades.aninhamento.Count; x++)
                    str += this.proprieades.aninhamento[x].GetNome() + ".";
            
            
            if (this.chamadaDoMetodo != null)
            {
                for (int x = 0; x < this.chamadaDoMetodo.Count - 1; x++)
                    str += this.chamadaDoMetodo[x].ToString() + ".";
                str += this.chamadaDoMetodo[chamadaDoMetodo.Count - 1].ToString();
            }
            return str;
        }
    }

    /// <summary>
    /// Uma expressão que encapsula uma chamada de função, tendo como parâmetros as expressões as sub-expressões.
    /// Faz os cálculos como se os parâmetros não sejam objetos, mas expressões que são avaliadas,
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


        public static bool IsChamadaDeFuncao(string nomeFuncao, string nomeClasseDaFuncao, Escopo escopo)
        {
            if ((escopo.tabela.IsFunction(nomeFuncao, escopo) == null) && (nomeClasseDaFuncao != null)) 
            {
                // trata do caso de programacao orientada a objetos, onde funcões estão dentro de metodos.
                Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasseDaFuncao);
                if (classe != null)
                    return classe.GetMetodos().Find(k => k.nome == nomeFuncao) != null;
            }
            else
               // trata do caso de programacao estruturada, onde funcoes estao fora das classes.
               return (escopo.tabela.IsFunction(nomeFuncao, escopo)) != null;

            return false;
        }

        public static bool AtualizaExpressao(Escopo escopo, Expressao expressaoCurrente)
        {
            try
            {
                string nomeMetodo = expressaoCurrente.tokens[expressaoCurrente.indexToken];
                List<Expressao> expressaoParametros = new List<Expressao>();
         

                // retira os tokens da chamada de função.
                List<string> tokensParametros = UtilTokens.GetCodigoEntreOperadores(expressaoCurrente.indexToken + 1, "(", ")", expressaoCurrente.tokens.ToList<string>());
                if (tokensParametros == null)
                {

                    UtilTokens.WriteAErrorMensage(escopo, "Erro em chamda da função: " + nomeMetodo, expressaoCurrente.tokens);
                    expressaoCurrente.indiceProcessamentoDaProximaExpressao = -1;
                    return false;
                }
                else
                // faz o processamento da interface de expressoes-parâmetros da chamada de função. exemplo: funcaoB(x+1,y*2) --> extrai duas expressões: "x+1", e "y*2.
                if ((tokensParametros != null) && (tokensParametros.Count > 0))
                {
                    tokensParametros.RemoveAt(0); // retira o primeiro parenteses.
                    tokensParametros.RemoveAt(tokensParametros.Count - 1); // retira o ultimo parenteses.

                    List<Expressao> parametros = Expressao.Instance.ExtraiExpressoes(escopo, tokensParametros); //compoe uma lista de expressoes, a partir da lista de tokens calculada acima.
                    
                    Funcao funcaoValidada = UtilTokens.ObtemFuncaoCompativelComAChamadaDeFuncao(nomeMetodo, parametros[0].Elementos, escopo);
                    if (funcaoValidada == null)
                    {
                        UtilTokens.WriteAErrorMensage(escopo, "funcao compativel com chamada de funcao: " + Utils.UneLinhasPrograma(expressaoCurrente.tokens) + " nao encontrada.", expressaoCurrente.tokens);
                        return false;
                    }
                    else
                    {
                        ExpressaoChamadaDeFuncao exprssChamada = new ExpressaoChamadaDeFuncao(funcaoValidada);
                        exprssChamada.expressoesParametros = parametros[0].Elementos;
                        
                        
                        expressaoCurrente.Elementos.Add(exprssChamada); // adiciona a expressão função para a lista de expressões da expressao currente.
                        expressaoCurrente.indexToken += 1 + 1 + tokensParametros.Count;
                        expressaoCurrente.indiceProcessamentoDaProximaExpressao = expressaoCurrente.indexToken;


                        return true;
                    }
                }//if
            } 
            catch
            {
                UtilTokens.WriteAErrorMensage(escopo, "Erro no processamento de uma chamada de função.", expressaoCurrente.tokens);
                return false;
                
            }
            return false;
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

        public static bool isOperadorBinario(string tokenOperador, Escopo escopo, Expressao expressaoCurrente)
        {
            if (Expressao.linguagem.VerificaSeEhOperadorBinario(tokenOperador))
            {


                Operador operadorBinario = Operador.GetOperador(tokenOperador, expressaoCurrente.tipo, "BINARIO", linguagem);
                if (operadorBinario != null)
                    return true;
                else
                {
                    if (tokenOperador == "=") // o operador "=" é universal, pertence a todas classes do repositorio.
                        return true;

                    UtilTokens.WriteAErrorMensage(escopo, "operador: " + tokenOperador + " para o tipo da expressão: " + expressaoCurrente.tipo + " nao encontrado.", expressaoCurrente.tokens);
                    return false;
                }

            }
            return false;

        }

        public static bool isOperadorUnario(string tokenOperador, Escopo escopo, Expressao expressaoCurrente)
        {

            if (Expressao.linguagem.VerificaSeEhOperadorUnario(tokenOperador))
            {

                Operador operadorUnario = Operador.GetOperador(tokenOperador, expressaoCurrente.tipo, "UNARIO", linguagem);
                if (operadorUnario != null)
                    return true;
                else
                {
                    UtilTokens.WriteAErrorMensage(escopo, "operador: " + tokenOperador + " para o tipo da expressão: " + expressaoCurrente.tipo + " nao encontrado.", expressaoCurrente.tokens);
                    return false;
                }
            }
            return false;
        }

        public static bool AtualizaExpressao(string nomeOperador, Expressao expressaoCurrente, Escopo escopo, string tipoOperador)
        {
            string tipoDaExpressao = GetTipoExpressao(expressaoCurrente, escopo);
            Operador operador = Operador.GetOperador(nomeOperador, tipoDaExpressao, tipoOperador, Expressao.linguagem);
            if ((operador == null) && (nomeOperador == "="))
                operador = Operador.GetOperador("=", "int", "BINARIO", linguagem);
            else
                if (operador == null)
                return false;
            expressaoCurrente.Elementos.Add(new ExpressaoOperador(operador));
            return true;
        }

        public static bool isOperador(string nomeOperador, Escopo escopo)
        {
            return escopo.tabela.GetOperadores().Find(k => k.nome.Equals(nomeOperador)) != null;
        }



        public override string ToString()
        {
            if (this.operador != null)
                return operador.nome;
            else
                return "operador";
        }
    }

    public class ExpressaoObjeto : Expressao
    {
        public Objeto objeto;
        public ExpressaoObjeto(Objeto v) : base()
        {
            this.objeto = v; // constroi com uma referencia.
            this.Elementos = new List<Expressao>();
        }

        public static bool IsExpressaoObjeto(Escopo escopo, string nomeObjeto)
        {
            // o token é nome de uma Variavel?
            return (escopo.tabela.GetObjeto(nomeObjeto, escopo) != null);
        }

        /// <summary>
        /// faz o processamento de objeto já definido no escopo.
        /// </summary>
        public static bool AtualizaExpressao(Escopo escopo, string nomeObjetoJaDefinido, Expressao expressaoCurrente)
        {
            try
            {
                // index+0--> nome do objeto,   index+1--> possivel operador de atribuicao "=".
                Objeto obj1 = escopo.tabela.GetObjeto(nomeObjetoJaDefinido, escopo);
                expressaoCurrente.Elementos.Add(new ExpressaoObjeto(obj1));
                if (((expressaoCurrente.indexToken + 3) < expressaoCurrente.tokens.Count) && (expressaoCurrente.tokens[expressaoCurrente.indexToken + 2] == "="))
                {
                    string numero = expressaoCurrente.tokens[expressaoCurrente.indexToken + 3];
                    object valorNumero = Expressao.Instance.ConverteNumeroParaObjeto(numero, escopo);
                    if (valorNumero != null)
                        obj1.SetValor(valorNumero);

                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public override object GetElemento()
        {
            return objeto;
        }

        public override string ToString()
        {
            if (objeto != null)
                return objeto.GetNome();
            else
                return "object null";
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

        public static bool isNumero(string numero)
        {
            return Expressao.linguagem.VerificaSeEhNumero(numero);
        }


        public static bool AualizaExpressao(string numero, Expressao expressaoCurrente)
        {
            try
            {
                ExpressaoNumero numeroExpressao = new ExpressaoNumero(numero);
                expressaoCurrente.Elementos.Add(numeroExpressao);
                return true;
            }
            catch
            {
                return false;
            }
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
       

        public static bool AtualizaExpressao(string nomeElemento, Expressao exprssCurrente)
        {
            try
            {
                Expressao expressaoElementoID = new ExpressaoElemento(nomeElemento);
                expressaoElementoID.tipo = "string";
                exprssCurrente.Elementos.Add(expressaoElementoID);

                exprssCurrente.indiceProcessamentoDaProximaExpressao = exprssCurrente.indexToken;

                return true;
            }
            catch
            {
                return false;
            }
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

    public class ExpressaoVetor : Expressao
    {
        public Vetor vetor;
        public ExpressaoOperadorMatricial indicesVetor;

        public ExpressaoVetor(Vetor variavel, ExpressaoOperadorMatricial matricial) : base()
        {
            this.vetor = variavel;
            this.indicesVetor = matricial;
            this.Elementos = new List<Expressao>();
        }

        public ExpressaoVetor(Vetor variavel) : base()
        {
            this.vetor = variavel;
            this.indicesVetor = new ExpressaoOperadorMatricial();
            this.Elementos = new List<Expressao>();
        }

        public override object GetElemento()
        {
            return vetor;
        }

        public static bool isExpressaoVetor(Escopo escopo, string nomeVetor)
        {
            return escopo.tabela.GetVetor(nomeVetor, escopo) != null;
        }

        public static bool AtualizaExpressao(Escopo escopo, string nomeVetor, Expressao expressaoCurrente)
        {
            try
            {


                Vetor vvt = escopo.tabela.GetVetor(nomeVetor, escopo);
                ExpressaoOperadorMatricial operadorMatricial = (ExpressaoOperadorMatricial)Expressao.Instance.ExtraiOperadorMatricial(expressaoCurrente.tokens.ToArray(), escopo, ref expressaoCurrente.indexToken);
                ExpressaoVetor expressaoVetor = new ExpressaoVetor(vvt, operadorMatricial);

                expressaoCurrente.Elementos.Add(expressaoVetor);
                if (((expressaoCurrente.indexToken + 2) < expressaoCurrente.tokens.Count) && (expressaoCurrente.tokens[expressaoCurrente.indexToken + 1] == "="))
                {

                    string numero = expressaoCurrente.tokens[expressaoCurrente.indexToken + 2];
                    object valorNumero = Expressao.Instance.ConverteNumeroParaObjeto(numero, escopo);
                    if (valorNumero != null)
                        vvt.SetValor(valorNumero);


                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return vetor.ToString();
        }
    }


    /// <summary>
    /// expressoes! Os metodos (matemáticos, lógicos, de comparação) e as propriedades e variáveis. Uma expressão é
    /// tudo do programa, menos as sintaxes dos comandos da linguagem.
    /// </summary>
    public class Expressao
    {

        public virtual object GetElemento() { return null; }
        protected static LinguagemOrquidea linguagem;
        private static Expressao InstanceSingleton;

        public Escopo escopoDaExpressao; // contexto-escopo onde a expressão está codificada.
        private List<string> codigo; // trecho de código onde está localizado a expressão.

        public object oldValue;

        public string tipo = null; // tipo de retorno da expressão.



        public bool isModify = true; // otimização de expressão.
        public bool isInPosOrdem = false;

        public List<string> MsgErros; // lista de erros encontrados no processamento da expressão.
        public List<Expressao> Elementos; // lista de expressões-elementos da expressão.


        public List<string> elementosNaoProcessados { get; set; }
        public List<string> tokens { get; set; } // tokens que compoe a expressão.


        public int indiceProcessamentoDaProximaExpressao = -1;
        internal int indexToken = -1;

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
            this.isModify = true;
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
            if ((tokens.Length>=3) && (tokens[0]=="a") && (tokens[1]=="=") && (tokens[2]=="1"))
            {
                int k = 0;
                k++;
            }
            
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

            try
            {
                escopoDaExpressao = escopo;
                for (indexToken = 0; indexToken < tokens.Length; indexToken++)
                {
                    bool resultAtualizacao = true;
                    this.tipo = GetTipoExpressao(this, escopo);

                    if (((indexToken + 1) < tokens.Length) && (tokens[indexToken + 1] == ".") && (ExpressaoChamadaDeMetodo.IsChamadaDeMetodo(this, escopo, indexToken)))
                        resultAtualizacao = ExpressaoChamadaDeMetodo.AtualizaExpressao(escopo, this);
                    else
                    if (((indexToken + 1) < tokens.Length) && (tokens[indexToken + 1] == ".") && (ExpressaoAtribuicaoPropriedadesAninhadas.IsProopriedadesAninhadas(this, escopo, indexToken)))
                        resultAtualizacao = ExpressaoAtribuicaoPropriedadesAninhadas.AtualizaExpressao(this, escopo);
                    else
                    // token de bloco, ou  token de termino de linha, termina de processar os elementos da expressao currente.
                    if ((tokens[indexToken] == "{") || (tokens[indexToken] == ";"))
                    {
                        this.indiceProcessamentoDaProximaExpressao = -1;
                        this.tipo = GetTipoExpressao(this, escopo);
                        return;
                    }
                    else
                    // o token é um nome de função? (regra de uma chamada de um função).
                    if (ExpressaoChamadaDeFuncao.IsChamadaDeFuncao(tokens[indexToken], null, escopo))
                        resultAtualizacao = ExpressaoChamadaDeFuncao.AtualizaExpressao(escopo, this);
                    else
                    // expressao formada por um token palavra-chave, ou operador, mas não é palavra-chave ou operador, e sim um nome de termo-chave ou operador, como "@+", "+" nao é um operador, mas um nome de algum texto.
                    if ((tokens[indexToken] == "@") && ((indexToken + 1) < tokens.Length))
                        resultAtualizacao = ExpressaoElemento.AtualizaExpressao(tokens[indexToken + 1], this);
                    else
                    // expressao entre parenteses, e nao eh chamada de funcao.
                    if (tokens[indexToken] == "(")
                        resultAtualizacao = ExtraiExpressaoEntreParenteses(escopo, this);
                    else
                    // o token é nome de uma Variavel?
                    if (ExpressaoObjeto.IsExpressaoObjeto(escopo, tokens[indexToken]))
                        resultAtualizacao = ExpressaoObjeto.AtualizaExpressao(escopo, tokens[indexToken], this);
                    else
                    // o token é nome de um vetor:
                    if (ExpressaoVetor.isExpressaoVetor(escopo, tokens[indexToken]))
                        resultAtualizacao = ExpressaoVetor.AtualizaExpressao(escopo, tokens[indexToken], this);
                    else
                    if (ExpressaoOperador.isOperadorBinario(tokens[indexToken], escopo, this))
                        resultAtualizacao = ExpressaoOperador.AtualizaExpressao(tokens[indexToken], this, escopo, "BINARIO");
                    else
                    if (ExpressaoOperador.isOperadorUnario(tokens[indexToken], escopo, this))
                        resultAtualizacao = ExpressaoOperador.AtualizaExpressao(tokens[indexToken], this, escopo, "UNARIO");
                    else
                    if (ExpressaoOperador.isOperador(tokens[indexToken], escopo))
                        resultAtualizacao = ExpressaoOperador.AtualizaExpressao(tokens[indexToken], this, escopo, "BINARIO");
                    else
                    // o token é um numero?
                    if (ExpressaoNumero.isNumero(tokens[indexToken]))
                        resultAtualizacao = ExpressaoNumero.AualizaExpressao(tokens[indexToken], this);
                    else
                    // o token é um nome de uma classe? (instanciação dentro da expressão).
                    if ((RepositorioDeClassesOO.Instance().ObtemUmaClasse(tokens[indexToken]) != null) && ((indexToken + 1) < tokens.Length) && (linguagem.VerificaSeEhID(tokens[indexToken + 1])))
                    {
                        if (ExpressaoObjeto.IsExpressaoObjeto(escopo, tokens[indexToken + 1]))
                            resultAtualizacao = ExpressaoObjeto.AtualizaExpressao(escopo, tokens[indexToken + 1], this);
                        else
                        {
                            Objeto obj1 = new Objeto("private", tokens[indexToken], tokens[indexToken + 1], null);
                            ExpressaoObjeto expressaoObjetoInstanciado = new ExpressaoObjeto(obj1);
                            this.Elementos.Add(new ExpressaoElemento(tokens[indexToken])); // adiciona o tipo do objeto a ser instanciado.
                            this.Elementos.Add(expressaoObjetoInstanciado);
                        }
                        indexToken += 1;
                        resultAtualizacao = true;
                    }
                    else
                    if (linguagem.VerificaSeEhID(tokens[indexToken])) // o token eh um nome de um campo de objeto, por exemplo.
                        resultAtualizacao = ExpressaoElemento.AtualizaExpressao(tokens[indexToken], this);

                    if (!resultAtualizacao)
                    {
                        this.Elementos = new List<Expressao>();
                        return;
                    }

                } // for i

                this.tipo = GetTipoExpressao(this, escopo);
                this.isModify = true;
            } 
            catch (Exception ex)
            {
                ModuloTESTES.LoggerTests.AddMessage("Erro no processamento de expressao: " + ex.StackTrace);
            }
        } // Expressao()

     
         private static bool ExtraiExpressaoEntreParenteses(Escopo escopo, Expressao exprssCurrente)
        {
            List<string> tokens = exprssCurrente.tokens;

            int indexStart = tokens.IndexOf("(");
            // quebra e adiciona duas expressões na expressão principal: a expressão entre parênteses, e a expressão após a expressão entre parenteses.
            List<string> tokensEmParenteses = UtilTokens.GetCodigoEntreOperadores(tokens.ToList<string>().IndexOf("("), "(", ")", tokens.ToList<string>());
     
            if ((tokensEmParenteses != null) && (tokensEmParenteses.Count > 0))
            {


                // a expressao tem uma subexpressao entre parenteses, mais uma subexpressao após os parenteeses, ligadas por um operador binario.
                string nextToken = tokens[tokensEmParenteses.Count];
                if (ExpressaoOperador.isOperadorBinario(nextToken, escopo, exprssCurrente))  
                {

                    // calculo da expressao entre parenteses.
                    Expressao exprssEntreParenteses = new Expressao(tokensEmParenteses.GetRange(1, tokensEmParenteses.Count - 2).ToArray(), escopo);

                    if ((exprssEntreParenteses != null) && (exprssEntreParenteses.Elementos.Count > 0))
                    {
                        // restaura os parenteses da subexpressao, pois entra no processamento de PosOrdem da expressão.
                        exprssEntreParenteses.Elementos.Insert(0, new ExpressaoElemento("(")); 
                        exprssEntreParenteses.Elementos.Add(new ExpressaoElemento(")"));
                    }

                    // calculo da expressao apos o fechamento de parenteses.
                    List<string> tokensRestantes = tokens.ToList<string>().GetRange(tokensEmParenteses.Count, tokens.Count - tokensEmParenteses.Count);
                    Expressao expressaoRestante = new Expressao(tokensRestantes.ToArray(), escopo);

                    exprssCurrente.indiceProcessamentoDaProximaExpressao = exprssCurrente.indexToken + tokensEmParenteses.Count + tokensRestantes.Count;
                    exprssCurrente.indexToken += tokensEmParenteses.Count + tokensRestantes.Count;


                    if ((expressaoRestante != null) && (expressaoRestante.Elementos.Count > 0) && ((exprssEntreParenteses != null) && (exprssEntreParenteses.Elementos.Count > 0)))
                        Expressao.UnificaExpressoes(exprssCurrente, exprssEntreParenteses, expressaoRestante); // unifica as expressoes que formam uma expressao entre parenteses, mais expressão após os parenteses.
                    return true;
                }

            }
            else
            {
                
                // a expressao soh tem uma subexpressao entre parenteses, e nada mais além.
                tokensEmParenteses.RemoveAt(0);
                tokensEmParenteses.RemoveAt(tokensEmParenteses.Count - 1);

                Expressao exprssResutante = new Expressao(tokensEmParenteses.ToArray(), escopo);
                if ((exprssResutante != null) && (exprssResutante.Elementos.Count > 0))
                {
                    exprssCurrente.indiceProcessamentoDaProximaExpressao = exprssCurrente.indexToken + 2 + tokensEmParenteses.Count;
                    exprssCurrente.Elementos.AddRange(exprssResutante.Elementos.ToList<Expressao>());

                }
                return true;
            }
            return false;
        }



        // une expressões, pois são partes de uma unica expressao.
        private static void UnificaExpressoes(Expressao exprssCurrente,params Expressao[] expressoesAUnificar)
        {
            for (int umaExpressao = 0; umaExpressao < expressoesAUnificar.Length; umaExpressao++)
                exprssCurrente.Elementos.AddRange(expressoesAUnificar[umaExpressao].Elementos);
        }
        internal Expressao ExtraiOperadorMatricial(string[] _tokens, Escopo escopo, ref int indexInicioOperador)
        {

            int indexStart = _tokens.ToList<string>().IndexOf("[");
            List<string> tokens = _tokens.ToList<string>().GetRange(indexStart, _tokens.Length - indexStart);

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
                        UtilTokens.WriteAErrorMensage(escopo, "Erro na extracao de um operador matricial: " + Utils.UneLinhasPrograma(tokensMatriciais), escopo.codigo);
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

            int indexErros = escopo.GetMsgErros().Count;
            List<Expressao> expressoesExtraidas = new List<Expressao>();
            List<string> tokensRaw = tokensExpressoes.ToList<string>();

            int indiceDeUmaExpressao = 0;
            string[] array_tokens = tokensRaw.ToArray();
            int x = 0;
            while ((x < tokensRaw.Count) && (indiceDeUmaExpressao != -1))
            {
                Expressao umaExpressao = new Expressao(tokensRaw.ToArray(), escopo);
                if (escopo.GetMsgErros().Count > indexErros)
                    return null;

                if (umaExpressao != null)
                {
                    expressoesExtraidas.Add(umaExpressao);

                    if (umaExpressao.indiceProcessamentoDaProximaExpressao != -1)
                    {
                        int startSearch = umaExpressao.indiceProcessamentoDaProximaExpressao + 1;
                        if ((startSearch == tokensRaw.Count) || (startSearch > tokensRaw.Count))
                            break;
                        else
                            tokensRaw = tokensRaw.GetRange(startSearch, tokensRaw.Count - startSearch);
                    }
                    else
                        tokensRaw.Clear();

                   
                }
                else
                    x++;
            }

            return expressoesExtraidas;

        }

      
        public object ConverteNumeroParaObjeto(string str_numero, Escopo escopo)
        {
            object obj_result = null;
            if (IsTipoInteiro(str_numero))
                obj_result = int.Parse(str_numero);
            else
            if (IsTipoFloat(str_numero))
                obj_result = float.Parse(str_numero);
            else
            if (IsTipoDouble(str_numero))
                obj_result = double.Parse(str_numero);

            return obj_result;
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
        public bool IsTipoDouble(string str_numero)
        {
            double numeroDouble = 0.0;
            return double.TryParse(str_numero.Trim(' '), out numeroDouble);
        }

        public bool IsNumero(string str_numero)
        {
            return IsTipoInteiro(str_numero) || IsTipoFloat(str_numero) || (IsTipoDouble(str_numero));
        }


        /// <summary>
        /// calcula o tipo da expressao, verificando tipos de expressao, ou pelos tokens da expressao.
        /// </summary>
        public  static string GetTipoExpressao(Expressao expressaoCurrente, Escopo escopo)
        {

            string tipoDaExpressao = null;
            List<string> tipoDosElementos = new List<string>();
            List<string> elementos = expressaoCurrente.tokens.ToList<string>();

            // verifica o tipo da expressão através da análise das sub-expressoes. É útil quando a expressao currente já está formada.
            if ((expressaoCurrente.Elementos != null) && (expressaoCurrente.Elementos.Count > 0))
            {
                for (int x = 0; x < expressaoCurrente.Elementos.Count; x++)
                {
                    if (expressaoCurrente.Elementos[x].GetType() == typeof(ExpressaoAtribuicaoPropriedadesAninhadas))
                    {
                        ExpressaoAtribuicaoPropriedadesAninhadas expressao = (ExpressaoAtribuicaoPropriedadesAninhadas)expressaoCurrente.Elementos[x];
                        if (expressao.aninhamento.Count > 0)
                        {
                            Objeto propriedadeFinal = expressao.aninhamento[expressao.aninhamento.Count - 1];
                            if ((tipoDaExpressao != null) && (tipoDaExpressao != propriedadeFinal.GetTipo()))
                                return null;
                            else
                                if (propriedadeFinal.GetTipo() != null)
                                tipoDaExpressao = propriedadeFinal.GetTipo();
                        }
                    }

                    else
                    if (expressaoCurrente.Elementos[x].GetType() == typeof(ExpressaoChamadaDeMetodo))
                    {
                        ExpressaoChamadaDeMetodo expressao = (ExpressaoChamadaDeMetodo)expressaoCurrente.Elementos[x];
                        if ((expressao.chamadaDoMetodo != null) && (expressao.chamadaDoMetodo[expressao.chamadaDoMetodo.Count - 1].funcao != null)) 
                        {
                            if ((tipoDaExpressao != null) && (tipoDaExpressao != expressao.chamadaDoMetodo[expressao.chamadaDoMetodo.Count - 1].funcao.tipoReturn))
                                return null;
                            else
                                if ((tipoDaExpressao == null) && (expressao.chamadaDoMetodo[expressao.chamadaDoMetodo.Count - 1].funcao != null))
                                tipoDaExpressao = expressao.chamadaDoMetodo[expressao.chamadaDoMetodo.Count - 1].funcao.tipoReturn;
                        }
                    }
                    else
                    if (expressaoCurrente.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                    {
                        ExpressaoObjeto expressao = (ExpressaoObjeto)expressaoCurrente.Elementos[x];
                        if ((tipoDaExpressao != null) && (tipoDaExpressao != expressao.objeto.GetTipo()))
                            return null;
                        else
                            if ((expressao.objeto != null) && (tipoDaExpressao == null))
                            tipoDaExpressao = expressao.objeto.GetTipo();
                        else
                            if (expressao.objeto == null)
                            return null;

                    }
                    else
                    if (expressaoCurrente.Elementos[x].GetType() == typeof(ExpressaoVetor))
                    {
                        ExpressaoVetor expressao = (ExpressaoVetor)expressaoCurrente.Elementos[x];
                        if ((tipoDaExpressao != null) && (expressao.vetor != null) && (tipoDaExpressao != expressao.vetor.GetTipo()))
                            return null;
                        else
                            if ((expressao.vetor != null) && (tipoDaExpressao == null))
                            tipoDaExpressao = expressao.vetor.GetTipo();
                        else
                            if (expressao.vetor == null)
                            return null;
                    }


                    if (expressaoCurrente.Elementos[x].GetType() == typeof(ExpressaoNumero))
                    {
                        ExpressaoNumero expressao = (ExpressaoNumero)expressaoCurrente.Elementos[x];
                        if ((expressao.numero != null) && (expressao.numero != tipoDaExpressao))
                            return null;
                        else
                            if ((expressao.numero != null) && (tipoDaExpressao == null))
                        {
                            string tipoNumero = Expressao.ObtemTipoDoNumero(expressao);
                            tipoDaExpressao = tipoNumero;
                        }
                        else
                            if (expressao.numero == null)
                            return null;
                    }

                    if (expressaoCurrente.Elementos[x].GetType() == typeof(ExpressaoChamadaDeFuncao))
                    {
                        ExpressaoChamadaDeFuncao expressao = (ExpressaoChamadaDeFuncao)expressaoCurrente.Elementos[x];
                        if ((tipoDaExpressao != null) && (expressao.funcao != null) && (tipoDaExpressao != expressao.funcao.tipoReturn))
                            return null;
                        else
                            if ((expressao.funcao != null) && (tipoDaExpressao == null))
                            tipoDaExpressao = expressao.funcao.tipoReturn;
                        else
                            if (expressao.funcao == null)
                            return null;

                    }

                }
                return tipoDaExpressao;
            }

            // verifica o tipo da expressao pelos seus tokens. Útil quando a expressao currente nao está formada ainda.
            for (int x = 0; x < expressaoCurrente.tokens.Count; x++)
            {

                string elemento = expressaoCurrente.tokens[x];

                if (escopo.tabela.GetObjeto(elemento, escopo) != null)
                    tipoDosElementos.Add(escopo.tabela.GetObjeto(elemento, escopo).GetTipo());
                else
                if (escopo.tabela.GetVetor(elemento, escopo) != null)
                    tipoDosElementos.Add(escopo.tabela.GetVetor(elemento, escopo).GetTiposElemento());
                else
                if (elemento == "float")
                    tipoDosElementos.Add("float");
                else
                if (elemento == "double")
                    tipoDosElementos.Add("double");
                else
                if (elemento == "int")
                    tipoDosElementos.Add("int");
                else
                if (Expressao.GetTipoNumero(elemento) == "int")
                    tipoDosElementos.Add("int");
                else
                if (Expressao.GetTipoNumero(elemento) == "float")
                    tipoDosElementos.Add("float");
                else
                if (Expressao.GetTipoNumero(elemento) == "double")
                    tipoDosElementos.Add("double");

            }

            if ((tipoDosElementos.Count > 0) && (IsSomenteNumeros(tipoDosElementos)))
            {

                if (IsTipoUnicoElementosDaExpressao(tipoDosElementos, "int"))
                    return "int";
                else 
                if (IsTipoUnicoElementosDaExpressao(tipoDosElementos, "float"))
                    return "float";
                else
                if (IsTipoUnicoElementosDaExpressao(tipoDosElementos, "double"))
                    return "double";
                else
                if ((tipoDosElementos.Contains("float")) && (!tipoDosElementos.Contains("double")))
                    return "float";
                else
                if (tipoDosElementos.Contains("double"))
                    return "double";
            } // if
            else
            if (elementos.Contains("="))
            {
                int indexSignalEquals = elementos.IndexOf("=");
                if (indexSignalEquals >= 2)
                {
                    string nomeDoObjetoELemento = elementos[indexSignalEquals - 1];
                    string classeDoObjetoElemento = elementos[indexSignalEquals - 2];
                    if ((RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeDoObjetoElemento) != null) &&  ((indexSignalEquals - 2) >= 0))
                        {
                            string classeDoNumero = elementos[indexSignalEquals - 2];
                            Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeDoNumero); // se for uma propriedade estática, retorna o tipo da classe (nome).
                            if (classe != null)
                                return classe.GetNome(); // retorna como tipo da expressão o tipo da variavel  a receber valor da atribuicao expressao.
                            else
                            {
                                Objeto objetoAtribuicao = escopo.tabela.GetObjeto(elementos[indexSignalEquals - 2], escopo); // se for um objeto que faz a referencia na atribuição,
                                                                                                                             // retorna o tipo deste objeto, pois a expressão toda deve ter este tipo do objeto da atribuição.

                                if (objetoAtribuicao != null)
                                    return objetoAtribuicao.GetTipo();
                            }
                        }
                }
            }
            else
            {

                for (int x = 0; x < tipoDosElementos.Count; x++)
                    if (GetTipoNumero(tipoDosElementos[x]) == null)
                    {
                        Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDosElementos[x]);
                        if (classe != null)
                            return classe.GetNome();
                    }

            }
            if (expressaoCurrente.Elementos.Count == 0)
            {
                if (escopo.tabela.GetObjeto(expressaoCurrente.ToString(), escopo) != null)
                    return escopo.tabela.GetObjeto(expressaoCurrente.ToString(), escopo).GetTipo();
                else
                if (escopo.tabela.GetVetor(expressaoCurrente.ToString(), escopo) != null)
                    return escopo.tabela.GetVetor(expressaoCurrente.ToString(), escopo).GetTiposElemento();
            }

            return null;
        }

        private static string ObtemTipoDoNumero(ExpressaoNumero expressao)
        {
            string tipoDaExpressao = "";
            if (Expressao.Instance.IsTipoInteiro(expressao.numero))
                tipoDaExpressao = "int";
            else
            if (Expressao.Instance.IsTipoFloat(expressao.numero))
                tipoDaExpressao = "float";
            else
            if (Expressao.Instance.IsTipoDouble(expressao.numero))
                tipoDaExpressao = "double";
            return tipoDaExpressao;
        }

        private static string GetTipoNumero(string numero)
        {
            if (Expressao.Instance.IsTipoInteiro(numero))
                return "int";

            if (Expressao.Instance.IsTipoFloat(numero))
                return "float";
            if (Expressao.Instance.IsTipoDouble(numero))
                return "double";

            return null;
        }

        private static bool IsSomenteNumeros(List<string> tipoElementos)
        {
            for (int x = 0; x < tipoElementos.Count; x++)
                if ((tipoElementos[x] != "int") && (tipoElementos[x] != "float") && (tipoElementos[x] != "double"))
                    return false;
            return true;
        }
      
        private static bool IsTipoUnicoElementosDaExpressao(List<string> elementosDaExpresao, string tipoAVerificar)
        {
            for (int x = 0; x < elementosDaExpresao.Count; x++)
                if (elementosDaExpresao[x] != tipoAVerificar)
                    return false;

            return true;

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
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoVetor))
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
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                {
                    string nomeObjeto = ((Objeto)exprss.Elementos[x].GetElemento()).GetNome();
                    if (linguagem.VerificaSeEhNumero(nomeObjeto))
                        resumida.Add("ID");
                    else
                    {

                        Objeto v = escopo.tabela.GetObjeto(nomeObjeto, escopo);
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
                if (exprss.Elementos[x].GetType() == typeof(ExpressaoVetor))
                {
                    List<string> tokensVector = ObtemExpressaoCondicionalResumida(exprss.Elementos[x], escopo);
                    if ((tokensVector == null) || (tokensVector.Count == 0))
                        return null;
                    x += tokensVector.Count;

                } // if


                if (exprss.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                {
                    string nomeObjeto = ((Objeto)exprss.Elementos[x].GetElemento()).GetNome();

                    //é operador não condicional, mas um operador?
                    if ((linguagem.VerificaSeEhOperador(nomeObjeto) && (!linguagem.IsOperadorCondicional(nomeObjeto))))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.
                    } // if

                    else
                    // é um ID não nome de função? 
                    if ((linguagem.VerificaSeEhID(nomeObjeto)) && (escopo.tabela.IsFunction(nomeObjeto, escopo) == null))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.
                    }
                    else
                    // é operador condicional?
                    if (linguagem.IsOperadorCondicional(nomeObjeto))
                    {
                        resumidoSubExpressao.Add("CONDICIONAL"); // acrescenta um resumo condicional, e começa uma nova sub-expressão.
                        resumidoExressaoPrincipal.AddRange(resumidoSubExpressao);
                        resumidoSubExpressao.Clear();
                    } // if 
                    else
                    //é  parenteses abre?
                    if (nomeObjeto.Equals("("))
                    {
                        if (resumidoSubExpressao.Find(k => k.Equals("ID")) == null) // verifica se a sub-expressão já tem um operando resumido ID.
                            resumidoSubExpressao.Add("ID"); // inicia a sub-expressão com um ID, resumindo variáveis, e operadores aritmeticos.

                        pilhaInteiroParenteses++; // incrementa a pilha de parenteses, para verificar se a expressão tem um termo final (parenteses fecha).
                    } // if
                    else
                    // é parenteses fecha?
                    if (nomeObjeto.Equals(")"))
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
                if (this.Elementos[index].GetType() == (typeof(ExpressaoObjeto)))
                    expressaoRetorno.Elementos.Add(this.Elementos[index]);
                else
                if (this.Elementos[index].GetType() == (typeof(ExpressaoVetor)))
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


     
        public  bool isExpressionAritmetico(Expressao exprss, Escopo escopo)
        {
            int hasIntVariable = 0;
            int hasOperatorUnary = 0;
            for (int x = 0; x < exprss.Elementos.Count; x++)
            {
                Objeto v = escopo.tabela.GetObjeto(exprss.Elementos[x].ToString(), escopo);
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

                if (this.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                    str += ((ExpressaoObjeto)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoVetor))
                    str += ((ExpressaoVetor)Elementos[x]).ToString() + " ";
                if (this.Elementos[x].GetType() == typeof(ExpressaoOperador))
                    str += ((ExpressaoOperador)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoChamadaDeFuncao))
                    str += ((ExpressaoChamadaDeFuncao)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoElemento))
                    str += ((ExpressaoElemento)Elementos[x]).ToString() + " ";

                if (this.Elementos[x].GetType() == typeof(ExpressaoAtribuicaoPropriedadesAninhadas))
                    str += ((ExpressaoAtribuicaoPropriedadesAninhadas)this.Elementos[x]).ToString();

                if (this.Elementos[x].GetType() == typeof(ExpressaoChamadaDeMetodo))
                    str += ((ExpressaoChamadaDeMetodo)this.Elementos[x]).ToString();



            } // for x
            str = str.Trim(' ');
            return str;
        } // ToString()
    } // class expressoes

  
} // namespace 
