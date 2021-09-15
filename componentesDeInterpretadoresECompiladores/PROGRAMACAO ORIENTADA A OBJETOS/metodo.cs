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

        public void SetNomeLongo(string nomeClasse)
        {
            // muda o nome do metodo para o nome longo (nomeDaClasseDoMetodo+"."+ nomeDoMetodo).
            this.nomeLongo = nomeClasse + "." + nome;
            this.nome = this.nomeLongo;
        }
        public int prioridade { get; set; } // prioridade de execução em expressões.

        public string tipo { get; set; }

        public Objeto[] parametrosDaFuncao { get; set; } // parâmetros da função.
        public string tipoReturn { get; set; } // tipo de retorno da função.


        public delegate object FuncaoGeral(params object[] parametros);
        public delegate void CallFunction(); // prototipo de execução da função, implementada na liguagem utilizada na construção do compilador;

        public static CallFunction funcaoVazia { get; set; }


        public MethodInfo InfoMethod { get; set; }
        public ConstructorInfo InfoConstructor { get; set; }

       
        public List<Instrucao> instrucoesFuncao { get; set; } // contém as instruções de código que compõe a função.
        public object caller = null; // guarda que está chamando a execução da função.

        public Escopo escopo;

  
    

        // Modos de execucao da função:
        //     1- via instruções da linguagem orquidea.
        //     2- via metodo via reflexao (é preciso setar o objeto que fará a chamada do método).
        protected object ExecuteAFunction(List<object> parametros, Escopo escopo)
        {
            Escopo escopoFuncao = null;
            if (escopo != null)
                escopoFuncao = escopo.Clone(); // cria uma cópia para cada chamada de função, para que possa criar variáveis locais, durante o processamento da funcao.
            else
                return null;

            for (int x = 0; x < parametros.Count; x++)
                escopoFuncao.tabela.GetObjetos().Add(new Objeto("private", this.parametrosDaFuncao[x].GetTipo(), parametrosDaFuncao[x].GetNome(), parametros[x]));
           
          

            if (this.instrucoesFuncao != null) // avaliação da função via instruções da linguagem orquidea.
            {
                ProgramaEmVM program = new ProgramaEmVM(this.instrucoesFuncao);
                program.Run(escopoFuncao);
                object result = program.resultLastInstruction;
                if (result.GetType() == typeof(Objeto))
                    result = ((Objeto)result).GetValor();
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

       


        /// <summary>
        /// executa um metodo, da classe do objeto chamador (caller).
        /// O escopo tem referencias das propriedades da classe.
        /// </summary>
        public object ExecuteAMethod(List<Expressao> parametos, Escopo escopo, Objeto objetoCaller)
        {
            Escopo escopoDoMetodo = new Escopo(escopo); // objetos sem referencia serao coletados, se nao tiver nenhuma referencia apontando para o objeto.
            Classe classeDoObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(objetoCaller.GetTipo());

            // faz copias das propriedades e operadores, sem referencia, pois os valores das propriedades podem ser modificadas.
            List<Objeto> propriedadesEstaticasDaClasse = classeDoObjeto.propriedadesEstaticas; // faz por referencia para que a modificao modifique a propriedade estatica.
            List<Objeto> propriedadesDaClasse = classeDoObjeto.GetPropriedades().ToList<Objeto>();
            List<Operador> operadoresDaClasse = classeDoObjeto.GetOperadores().ToList<Operador>();

            escopoDoMetodo.tabela.GetObjetos().AddRange(propriedadesEstaticasDaClasse);
            escopoDoMetodo.tabela.GetObjetos().AddRange(propriedadesDaClasse);
            escopo.tabela.GetOperadores().AddRange(operadoresDaClasse);

            object result= ExecuteAFunction(parametos, objetoCaller, escopoDoMetodo);
            for (int x = 0; x < propriedadesDaClasse.Count; x++)
            {
                Objeto propriedade = escopoDoMetodo.tabela.GetObjetos().Find(k => k.GetNome() == propriedadesDaClasse[x].GetNome());
                objetoCaller.GetField(propriedade.GetNome()).SetValor(propriedade.GetValor()); // atualiza os valores de propriedades para o objeto que chamou a execução do método.
            }

            return result;
        }
        /// <summary>
        ///  avalia uma função, tendo como parâmetros expressões, muito apreciada nas chamadas de função, que utiliza expressões como parâmetros.
        ///  o objeto "caller" é utilizado para funções de objetos importados.
        /// </summary>
        public object ExecuteAFunction(List<Expressao> parametros, object caller, Escopo escopo)
        {
         
            List<object> valoresParametro = new List<object>();
            EvalExpression eval = new EvalExpression();

            for (int x = 0; x < parametros.Count; x++)
            {
                object valorParametro = eval.EvalPosOrdem(parametros[x], escopo);
                valoresParametro.Add(valorParametro);
            } // for x


         
            if (this.InfoMethod == null)
                return ExecuteAFunction(valoresParametro, escopo);
            else
            if ((this.InfoMethod != null) && (caller != null))
                return this.InfoMethod.Invoke(caller, valoresParametro.ToArray());

            return null;
        } // ExecuteAFunction()
  
        public object ExecuteAConstructor(List<Expressao> parametros, string nomeClasse, Escopo escopoFuncao)
        {
            Escopo escopo = escopoFuncao.Clone();
           
            if (this.InfoConstructor != null)
            {
                int indexConstrutor = parser.ProcessadorDeID.FoundACompatibleConstructor(nomeClasse, parametros);
                if (indexConstrutor == -1)
                    return null;

                List<object> valoresParametro = new List<object>();

                for (int x = 0; x < parametros.Count; x++)
                {
                    object valor = new EvalExpression().EvalPosOrdem(parametros[x], escopo);
                    valoresParametro.Add(valor);
                }

                Classe classeDOconstrutor = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);
                if (classeDOconstrutor != null)
                    return classeDOconstrutor.construtores[indexConstrutor].InfoConstructor.Invoke(valoresParametro.ToArray()); ;
            }
            else
            {
                int indexConstrutor = parser.ProcessadorDeID.FoundACompatibleConstructor(nomeClasse, parametros);
                if (indexConstrutor == -1)
                    return null;

                Objeto objetoInstanciado = InstanciaObjeto(nomeClasse, parametros, escopo, indexConstrutor);
                return objetoInstanciado; // retorna o objeto instanciado.

            }
            return null;
        }

        private Objeto InstanciaObjeto(string nomeClasse, List<Expressao> parametros, Escopo escopo, int indexConstructor)
        {

            List<object> valoresParametro = new List<object>();
            int indexConstrutor = parser.ProcessadorDeID.FoundACompatibleConstructor(nomeClasse, parametros);
            if (indexConstrutor == -1)
                return null;

            for (int x = 0; x < parametros.Count; x++)
            {
                object valor = new EvalExpression().EvalPosOrdem(parametros[x], escopo);
                valoresParametro.Add(valor);
            }

            Classe classeDoObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);

    
            escopo.tabela.GetObjetos().AddRange(classeDoObjeto.escopoDaClasse.tabela.GetObjetos());
            escopo.tabela.GetVetores().AddRange(classeDoObjeto.escopoDaClasse.tabela.GetVetores());

            classeDoObjeto.construtores[indexConstructor].ExecuteAFunction(valoresParametro, escopo);

            
            Objeto objetoInstanciado = new Objeto("private", classeDoObjeto.GetNome(), "b", null);
            List<Objeto> propriedades = objetoInstanciado.GetFields();
    
            
            
            for (int x = 0; x < propriedades.Count; x++)
            {
                if ((propriedades != null) && (propriedades.Count > 0))
                {
                    Objeto objPropriedade = escopo.tabela.GetObjetos().Find(k => k.GetNome() == propriedades[x].GetNome());
                    if (objPropriedade != null)
                        propriedades[x].SetValor(objPropriedade.GetValor());
                }

            }


            return objetoInstanciado;
        }

        public Funcao()
        {
            this.escopo = null;
            this.acessor = "protected"; // valor default para o acessor da função.
            this.nome = "";
            this.tipoReturn = null;

            this.prioridade = 300;  // seta a prioridade da função em avaliação de expressões. A regra de negócio é que a função sempre tem prioridade sobre os operadores.
   
            this.instrucoesFuncao = new List<Instrucao>();
        } //Funcao()

        public Funcao Clone()
        {
            Funcao fncClone = new Funcao(this.nomeClasse, this.acessor, this.nome, this.InfoMethod, this.tipoReturn, this.parametrosDaFuncao);
            fncClone.escopo = this.escopo.Clone();
            if (this.instrucoesFuncao != null)
                fncClone.instrucoesFuncao = this.instrucoesFuncao.ToList<Instrucao>();
            else
                fncClone.instrucoesFuncao = new List<Instrucao>();
            if (this.InfoConstructor != null)
                fncClone.InfoConstructor = this.InfoConstructor;

            return fncClone;
        }
        public Funcao(string acessor, string nome, FuncaoGeral fncImplementa, string tipoRetorno, params Objeto[] parametrosMetodo)
        {
            this.escopo = null;
            this.InfoMethod = null;
            this.InfoConstructor = null;
            this.acessor = acessor;
            if (acessor == null)
                this.acessor = "protected";
            this.nome = nome;
            this.tipoReturn = tipoRetorno;
            if (parametrosMetodo != null)
                this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();
            this.instrucoesFuncao = new List<Instrucao>();
        }


        public Funcao(string classe, string acessor, string nome, Objeto[] parametrosMetodo, string tipoRetorno, List<Instrucao> instrucoesCorpo, Escopo escopoDaFuncao)
        {
            
            this.InfoMethod = null;
            this.InfoConstructor = null;
            if (acessor == null)
                acessor = "protected"; // se nao tiver acessor, é uma função estruturada, seta o acessor para protected.
            else
                this.acessor = acessor; // acessor da função.
            this.nome = nome; // nome da função.
            this.parametrosDaFuncao = new Objeto[parametrosMetodo.Length]; // inicializa a lista de parâmetros da função.

            if ((parametrosMetodo != null) && (parametrosMetodo.Length > 0)) // obtém uma lista dos parâmetros da função. 
                this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();


            this.instrucoesFuncao = new List<Instrucao>(); // sem instruções (sem corpo de função).
            this.tipoReturn = tipoRetorno; // tipo do retorno da função.


            this.escopo = escopoDaFuncao.Clone();
    
            for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                escopo.tabela.GetObjetos().Add(new Objeto("private", parametrosDaFuncao[x].GetTipo(), parametrosDaFuncao[x].GetNome(), null, escopo, false));


            if (instrucoesCorpo != null)
                this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();

            this.nomeClasse = classe;
        } // Funcao()


        ///  construtor com método importado via API Reflexao.
        public Funcao(string nomeClasse, string acessor, string nome, MethodInfo metodoImportado, string tipoRetorno, params Objeto[] parametrosMetodo)
        {
            this.escopo = null;
            this.acessor = acessor;
            this.nome = nome;
            this.tipoReturn = tipoRetorno;
            this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();

            this.InfoMethod = metodoImportado;
            this.InfoConstructor = null;
            this.instrucoesFuncao = new List<Instrucao>();
            this.nomeClasse = nomeClasse;
     
            List<Type> tiposDosParametros = new List<Type>();
            List<object> nomesDosParametros = new List<object>();

        }

      
        public Funcao(string nomeClasse, string acessor, string nome, ConstructorInfo construtorImportado, string tipoRetorno, Escopo escopoDaFuncao, params Objeto[] parametrosMetodo)
        {
            this.acessor = acessor;
            this.nome = nome;
            this.InfoMethod = null;
            this.InfoConstructor = construtorImportado;
            this.tipoReturn = tipoRetorno;
            this.nomeClasse = nomeClasse;
            this.instrucoesFuncao = new List<Instrucao>();
            this.parametrosDaFuncao = parametrosMetodo.ToArray<Objeto>();
            if (escopoDaFuncao != null)
                this.escopo = escopoDaFuncao.Clone();
            
        }

        public override string ToString()
        {
            string str = "";
            if ((this.tipoReturn != null) && (this.tipoReturn != ""))
                str += this.tipoReturn.ToString() + "  ";

            if ((this.nome != null) && (this.nome != ""))
                str += this.nome + "( ";
            if ((this.parametrosDaFuncao != null) && (this.parametrosDaFuncao.Length > 0))
            {
                for (int x = 0; x < this.parametrosDaFuncao.Length; x++)
                {
                    str += this.parametrosDaFuncao[x] + " ";
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
        public string tipoRetorno { get; set; } // tipo de retorno da função

        internal int indexPosOrdem = 0; // utilizada para processamento de PosOrdem().


      
    

        // função com instrucoes orquidea.
        public Funcao funcaoImplementadoraDoOperador { get; set; }
    

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

      
        public Operador(string nomeClasse, string nomeOperador, int prioridade, Objeto[] parametros, string tipoOperador, MethodInfo metodoImpl, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.nomeClasse = nomeClasse;
            if (parametros != null)
                this.parametrosDaFuncao = parametros.ToArray<Objeto>(); // faz uma copia em profundidade nos parametros.
            else
                this.parametrosDaFuncao = new Objeto[0];
            
            this.tipo = tipoOperador;
            this.tipoReturn = UtilTokens.Casting(metodoImpl.ReflectedType.Name);
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


            Objeto[] operandos = new Objeto[2];

            if (tiposParametros.Length > 0)
                operandos[0] = new Objeto("A", tiposParametros[0], null, false);

            if (tiposParametros.Length > 1)
                operandos[1] = new Objeto("B", tiposParametros[1], null, false);

            this.parametrosDaFuncao = operandos;

            this.tipo = tipoOperador;

            this.funcaoImplementadoraDoOperador = funcaoDeImplementacaoDoOperador;
            this.instrucoesFuncao = funcaoDeImplementacaoDoOperador.instrucoesFuncao;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        } // Operador()

        public Operador(string nomeClase, string nomeOperador, int prioridade, string tipoRetorno, List<Instrucao> instrucoesCorpo, Objeto[] parametros, Escopo escopoDoOperador):base()
        {
            this.nome = nomeOperador;
            this.tipoRetorno = tipoRetorno;
            this.prioridade = prioridade;
            this.nomeClasseDoOperador = nomeClase;
            this.instrucoesFuncao = instrucoesCorpo.ToList<Instrucao>();
            this.parametrosDaFuncao = parametros;
            LinguagemOrquidea.RegistraOperador(this); // adiciona o operador criado, para a lista de operadores da linguagem, atualizando a lista de operadores para processamento.

        }

        public new Operador Clone()
        {
            Operador operador = new Operador(this.nomeClasse, this.nome, this.prioridade, this.parametrosDaFuncao, this.tipo, this.InfoMethod, this.escopo);
            operador.tipoRetorno = this.tipoRetorno;
            operador.tipoReturn = this.tipoReturn;
            return operador;
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
                return classe.GetOperadores()[index];
            } // if
            return null;
        }

     
        public object ExecuteOperador( string nomeDoOperador,  Escopo escopo, params object[] valoresParametros)
        {
            object result = null;
            if (caller == null)
                throw new Exception("objeto que chama a execucao de funcao eh nulo.");


            if (this.InfoMethod != null)
                result = InfoMethod.Invoke(caller, valoresParametros);
            else
            if (this.instrucoesFuncao != null)
                result = this.ExecuteAFunction(valoresParametros.ToList<object>(), escopo);
            else
            if (this.tipoRetorno == "int")
                return (int)result;
            else
            if (this.tipoRetorno == "float")
                return (float)result;
            else
            if (this.tipoRetorno == "double")
                return (double)result;
            else
                return result;
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
