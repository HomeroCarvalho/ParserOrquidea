using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using parser.ProgramacaoOrentadaAObjetos;
using System.Reflection;
namespace parser
{

    public class Funcao
    {
        public string acessor { get; set; }

        // nome da função.
        public string nome { get; set; }
        public string nomeLongo { get; set; }

        public string nomeClasse { get; set; }


        public string nomeClasseDoOperador { get; set; }

        public void SetNomeLongo()
        {
            // muda o nome do metodo para o nome longo (nomeDaClasseDoMetodo+"."+ nomeDoMetodo).
            this.nomeLongo = nomeClasseDoOperador + "." + nome;
            this.nome = this.nomeLongo;
        }
        public int prioridade { get; set; } // prioridade de execução em expressões.

        public string tipo { get; set; }

        public propriedade[] parametrosDaFuncao { get; set; } // parâmetros da função.
        public string tipoDoRetornoDaFuncao { get; set; } // tipo de retorno da função.


        public delegate object FuncaoGeral(params object[] parametros);
        public delegate void CallFunction(); // prototipo de execução da função, implementada na liguagem utilizada na construção do compilador;

        public static CallFunction funcaoVazia { get; set; }


        public MethodInfo InfoMethod { get; set; }
        public ConstructorInfo InfoConstructor { get; set; }

       
        public List<Instrucao> instrucoesFuncao { get; set; } // contém as instruções de código que compõe a função.
        public object caller = null; // guarda que está chamando a execução da função.

        public Escopo escopo;

        private List<List<Variavel>> stackVariaveisLocais { get; set; } // pilha de variáveis locais. A remoção das variáveis da stack é direto: remove da lista todas as variaveis, quando terminar o processamento currente da função/método.

        private List<Variavel> variaveisLocais { get; set; } // guarda uma copia das variaveis locais da função/método, para inicializarem o escopo.

        private int contadorVariaveis = 0;

        // Modos de execucao da função:
        //     1- via instruções da linguagem orquidea.
        //     2- via metodo via reflexao (é preciso setar o objeto que fará a chamada do método).
        public object ExecuteAFunction(List<object> parametros)
        {

            ProgramaEmVM program = new ProgramaEmVM(this.instrucoesFuncao);
            if (this.instrucoesFuncao != null) // avaliação da função via instruções da linguagem orquidea.
            {
                // atualiza as expressoes da funcao.
                for (int x = 0; x < this.instrucoesFuncao.Count; x++)
                    for (int exprss = 0; exprss < this.instrucoesFuncao[x].expressoes.Count; exprss++)
                        this.CarregaValoresParaUmaExpressaoParametro(this.instrucoesFuncao[x].expressoes[exprss], parametros, this.escopo);

                this.stackVariaveisLocais[contadorVariaveis++].AddRange(this.variaveisLocais.ToList<Variavel>());
                this.escopo.tabela.Variaveis = this.variaveisLocais.ToList<Variavel>(); // faz uma copia das variaveis para a pilha.


                program.Run(escopo);
                object result = program.resultLastInstruction; // SAIU LISO, A ULTIMA INSTRUCAO DO PROGRAMA, CALCULA O OBJETO RESULTANTE DA OPERAÇÃO DE UMA INSTRUÇÃO.

                this.stackVariaveisLocais.RemoveAt(--contadorVariaveis);
                return result;
            } // if 
            else
            if (this.InfoMethod != null) // avaliação de função via método importado com API Reflexão.
            {
                object result1= this.InfoMethod.Invoke(this.caller, parametros.ToArray());
                return result1;
            }
            else
                return null;

        } // ExecuteAFunction()

        private void CarregaValoresParaUmaExpressaoParametro(Expressao expressaoParametro, List<object> valoresEscopo, Escopo escopo)
        {
            int contadorValores = 0;

            for (int exprss = 0; exprss < expressaoParametro.Elementos.Count; exprss++)
            {
                if (expressaoParametro.Elementos[exprss].GetType() == typeof(ExpressaoOperador))
                    continue;

                for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                {
                    string nomeParametro = this.parametrosDaFuncao[x].GetNome().ToString().Trim(' ');
                    string nomeParametroExpressao = expressaoParametro.Elementos[exprss].ToString().Trim(' ');
                    if ((escopo.tabela.GetVar(nomeParametro, escopo) != null) && (nomeParametro.Equals(nomeParametroExpressao))) 
                    {
                        Variavel vv = escopo.tabela.GetVar(nomeParametro, escopo);
                        vv.SetValor(valoresEscopo[contadorValores++], escopo);
                        expressaoParametro.Elementos[exprss] = new ExpressaoVariavel(vv);
                    }
                    else
                    if ((escopo.tabela.GetVarVetor(nomeParametro, escopo) != null) && (nomeParametro.Equals(nomeParametroExpressao)))
                    {
                        VariavelVetor vvt = escopo.tabela.GetVarVetor(nomeParametro, escopo);
                        vvt.SetValor(valoresEscopo[contadorValores++], escopo);
                        expressaoParametro.Elementos[exprss] = new ExpressaoVariavelVetor(vvt);
                    }
                }
            }
        }

        ///  avalia uma função, tendo como parâmetros expressões, muito apreciada nas chamadas de função, que utiliza expressões como parâmetros.
        ///  o objeto "caller" é conseguido com a chamada do construtor, em cada variavel, variavelVetor, objeto.
        public object ExecuteAFunction(List<Expressao> parametros, object caller)
        {
         
            List<object> valoresParametro = new List<object>();

            for (int x = 0; x < parametros.Count; x++)
            {


                object valor = null;

                if (escopo.tabela.GetVar(parametros[x].ToString(), escopo) != null)
                {
                    valor = escopo.tabela.GetVar(parametros[x].ToString(), escopo).GetValor();
                    valoresParametro.Add(valor);
                }
                else
                if (escopo.tabela.GetVarVetor(parametros[x].ToString(), escopo) != null)
                {
                    valor = escopo.tabela.GetVarVetor(parametros[x].ToString(), escopo).GetValor();
                    valoresParametro.Add(valor);
                }
                else
                if (escopo.tabela.GetObjeto(parametros[x].ToString()) != null)
                {
                    valor = escopo.tabela.GetObjeto(parametros[x].ToString());
                    valoresParametro.Add(valor);
                }
                
            } // for x


            // carrega os parametros da função com os valores atuais das variaveis.
            for (int paramFnc = 0; paramFnc < this.parametrosDaFuncao.Length; paramFnc++)
                this.parametrosDaFuncao[paramFnc].valor = valoresParametro[paramFnc];


            if (this.InfoMethod == null)
                return ExecuteAFunction(valoresParametro);
            else
            if ((this.InfoMethod != null) && (caller != null))
                return this.InfoMethod.Invoke(caller, valoresParametro.ToArray());

            return null;
        } // ExecuteAFunction()
  
        public object ExecuteAConstructor(List<Expressao> parametros, Type classeDoConstrutor)
        {
            List<object> valoresParametro = new List<object>();
            for (int x = 0; x < parametros.Count; x++)
            {
                object valor = new EvalExpression().EvalPosOrdem(parametros[x], escopo);
                valoresParametro.Add(valor);
            }
            if (this.InfoConstructor != null)
            {

                int indexConstrutor = parser.ProcessadorDeID.FoundACompatibleConstructor(classeDoConstrutor.Name, parametros);
                if (indexConstrutor == -1)
                    return null;
                else
                    return classeDoConstrutor.GetConstructors()[indexConstrutor].Invoke(valoresParametro.ToArray());
            }
            return null;
        }

  
        public Funcao()
        {
            this.escopo = null;
            this.acessor = "protected"; // valor default para o acessor da função.
            this.nome = "";
            this.tipoDoRetornoDaFuncao = null;

            this.prioridade = 300;  // seta a prioridade da função em avaliação de expressões. A regra de negócio é que a função sempre tem prioridade sobre os operadores.
   
            this.instrucoesFuncao = new List<Instrucao>();
            this.stackVariaveisLocais = new List<List<Variavel>>();
            this.variaveisLocais = new List<Variavel>();
        } //Funcao()

        public Funcao(string acessor, string nome, FuncaoGeral fncImplementa, string tipoRetorno, params propriedade[] parametrosMetodo)
        {
            this.escopo = null;
            this.InfoMethod = null;
            this.InfoConstructor = null;
            this.acessor = acessor;
            if (acessor == null)
                this.acessor = "protected";
            this.nome = nome;
            this.tipoDoRetornoDaFuncao = tipoRetorno;
            if (parametrosMetodo != null)
                this.parametrosDaFuncao = parametrosMetodo.ToArray<propriedade>();

        }


        public Funcao(string classe, string acessor, string nome, propriedade[] parametrosMetodo, string tipoRetorno, List<Instrucao> instrucoesCorpo, Escopo escopoDaFuncao)
        {
            
            this.InfoMethod = null;
            this.InfoConstructor = null;
            if (acessor == null)
                acessor = "protected"; // se nao tiver acessor, é uma função estruturada, seta o acessor para protected.
            else
                this.acessor = acessor; // acessor da função.
            this.nome = nome; // nome da função.
            this.parametrosDaFuncao = new propriedade[parametrosMetodo.Length]; // inicializa a lista de parâmetros da função.

            if ((parametrosMetodo != null) && (parametrosMetodo.Length > 0)) // obtém uma lista dos parâmetros da função. 
                this.parametrosDaFuncao = parametrosMetodo.ToArray<propriedade>();


            this.instrucoesFuncao = new List<Instrucao>(); // sem instruções (sem corpo de função).
            this.tipoDoRetornoDaFuncao = tipoRetorno; // tipo do retorno da função.


            this.escopo = escopoDaFuncao.Clone();
            this.variaveisLocais = this.escopo.tabela.Variaveis.ToList<Variavel>();

            for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                escopo.tabela.AddVar(parametrosDaFuncao[x].acessor, parametrosDaFuncao[x].GetNome(), parametrosDaFuncao[x].tipo, null, escopo, false);


            if (instrucoesCorpo != null)
                this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();

            this.nomeClasse = classe;
        } // Funcao()


        ///  construtor com método importado via API Reflexao.
        public Funcao(string nomeClasse, string acessor, string nome, MethodInfo metodoImportado, string tipoRetorno, params propriedade[] parametrosMetodo)
        {
            this.escopo = null;
            this.acessor = acessor;
            this.nome = nome;
            this.tipoDoRetornoDaFuncao = tipoRetorno;
            this.parametrosDaFuncao = parametrosMetodo.ToArray<propriedade>();

            this.InfoMethod = metodoImportado;
            this.InfoConstructor = null;
            this.instrucoesFuncao = null;
            this.nomeClasse = nomeClasse;
      

            Type classeOperador = this.InfoMethod.DeclaringType;
            List<Type> tiposDosParametros = new List<Type>();
            List<object> nomesDosParametros = new List<object>();

        }

        private object GetCaller(Type tipo)
        {
            object objetoInstanciar = Activator.CreateInstance(tipo);
            return objetoInstanciar;
        }

        public Funcao(string nomeClasse, string acessor, string nome, ConstructorInfo construtorImportado, string tipoRetorno, Escopo escopoDaFuncao, params propriedade[] parametrosMetodo)
        {
            this.acessor = acessor;
            this.nome = nome;
            this.InfoMethod = null;
            this.InfoConstructor = construtorImportado;
            this.tipoDoRetornoDaFuncao = tipoRetorno;
            this.nomeClasse = nomeClasse;
            this.parametrosDaFuncao = parametrosMetodo.ToArray<propriedade>();
            if (escopoDaFuncao != null)
            {
                this.escopo = escopoDaFuncao.Clone();
                this.stackVariaveisLocais = new List<List<Variavel>>();
                this.variaveisLocais = this.escopo.tabela.Variaveis.ToList<Variavel>();
            }
            
        }

        public override string ToString()
        {
            string str = "";
            if ((this.tipoDoRetornoDaFuncao != null) && (this.tipoDoRetornoDaFuncao != ""))
                str += this.tipoDoRetornoDaFuncao.ToString() + "  ";

            if ((this.nome != null) && (this.nome != ""))
                str += this.nome + "( ";
            if ((this.parametrosDaFuncao != null) && (this.parametrosDaFuncao.Length > 0))
            {
                for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                {
                    str += this.parametrosDaFuncao[x].tipo + " " + this.parametrosDaFuncao[x].GetNome();
                    if (x < (parametrosDaFuncao.Length - 1))
                        str += ",";
                } // for x
            } // if
            str += ")";
            return str;
        } // ToString()
    } // class Funcao

    public class Operador : Funcao
    {
        public new int prioridade { get;  set; } // prioridade do operador nas expressões.
        public string tipoRetorno { get; private set; } // tipo de retorno da função

        internal int indexPosOrdem = 0; // utilizada para processamento de PosOrdem().


        //*********************************************tipos de funcao que implementam a avaliacao do  operador.
        // metodo para operador com função nativa da linguagem Base (C sharp).
        public Funcao.CallFunction metodoNativoOperador { get; set; }

        // função com instrucoes orquidea.
        public Funcao funcaoImplementadoraDoOperador { get; set; }
        //*****************************************************************************************************

        public int GetPrioridade()
        {
            return prioridade;
        }
        public string GetTipo()
        {
            return tipo;
        }

        public string GetTipoFuncao()
        {
            return tipo;
        }

        public void SetTipo(string tipoNovo)
        {
            this.tipo = tipoNovo;
        }



        private Random aleatorizador = new Random(1000);

      
        public Operador(string nomeClasse, string nomeOperador, int prioridade, propriedade[] parametros, string tipoOperador, MethodInfo metodoImpl, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            this.parametrosDaFuncao = parametros.ToArray<propriedade>(); // faz uma copia em profundidade nos parametros.
            this.tipo = tipoOperador;
            this.tipoDoRetornoDaFuncao = UtilTokens.Casting(metodoImpl.ReflectedType.Name);
            this.InfoMethod = metodoImpl;


            Type classeOperador = this.InfoMethod.DeclaringType;
            this.caller = classeOperador.GetConstructor(new Type[] { }).Invoke(null); // obtem um objeto da classe que construiu o operador.

            this.instrucoesFuncao = null;
            this.prioridade = prioridade;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }

        public Operador(string nomeClasse, string nomeOperador, int prioridade, string[] tiposParametros, string tipoOperador, Funcao funcaoDeImplementacaoDoOperador, Escopo escopoDoOperador) : base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            this.prioridade = prioridade;


            propriedade[] operandos = new propriedade[2];

            if (tiposParametros.Length > 0)
                operandos[0] = new propriedade("A", tiposParametros[0], null, false);

            if (tiposParametros.Length > 1)
                operandos[1] = new propriedade("B", tiposParametros[1], null, false);

            this.parametrosDaFuncao = operandos;

            this.tipo = tipoOperador;

            this.funcaoImplementadoraDoOperador = funcaoDeImplementacaoDoOperador;
            this.instrucoesFuncao = funcaoDeImplementacaoDoOperador.instrucoesFuncao;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        } // Operador()

        public Operador(string nomeClase, string nomeOperador, int prioridade, string tipoRetorno, List<Instrucao> instrucoesCorpo, propriedade[] parametros, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.tipoRetorno = tipoRetorno;
            this.prioridade = prioridade;
            this.nomeClasseDoOperador = nomeClase;
            this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();
            this.parametrosDaFuncao = parametros;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }




        public static Operador GetOperador(string nomeOperador, string classeOperador, string tipo, UmaGramaticaComputacional lng)
        {
            Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeOperador);
            if (classe == null)
                return null;
            int index = classe.GetOperadores().FindIndex(k => k.nome.Equals(nomeOperador));

            if (index != -1)
            {
                Operador op = classe.GetOperadores().Find(k => k.GetTipo().Contains(tipo));
                op.tipo = classe.GetNome();
                return classe.GetOperadores()[index];
            } // if
            return null;
        }

     
        public object ExecuteOperador( string nomeDoOperador,  Escopo escopo, params object[] valoresParametros)
        {
            object result = null;
            if (caller == null)
                throw new Exception("objeto que chama a execucao de funcao eh nulo.");

            if (this.instrucoesFuncao == null)
                result= InfoMethod.Invoke(caller, valoresParametros);


            if (this.instrucoesFuncao != null)
                result = this.ExecuteAFunction(valoresParametros.ToList<object>());

            if (this.tipoRetorno == "int")
                return (int)result;
            else
            if (this.tipoRetorno == "float")
                return (float)result;
            else
                return result;
   
        } // ExecuteOperador()

        public override string ToString()
        {
            string str = "";
            if (this.nome != null)
                str += "Nome: " + this.nome + "  pri: " + this.prioridade.ToString();
            return str;
        }// ToString()

 
      
    } // class
   
} //namespace
