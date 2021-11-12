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


     

        public void SetNomeLongo(string classeDoMetodo)
        {
            // muda o nome do metodo para o nome longo (nomeDaClasseDoMetodo+"."+ nomeDoMetodo).
            this.nomeLongo = classeDoMetodo + "@" + nome;
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
        protected object ExecuteAFunction(List<object> valoresDosParametros, Escopo escopo)
        {
  

            if (this.instrucoesFuncao != null) // avaliação da função via instruções da linguagem orquidea.
            {
            

                ProgramaEmVM program = new ProgramaEmVM(this.instrucoesFuncao);
                program.Run(escopo);


                object result = program.resultLastInstruction;
                       
                return result;
            } // if 
            else
            if (this.InfoMethod != null) // avaliação de função via método importado com API Reflexão.
            {
                object result1= this.InfoMethod.Invoke(this.caller, valoresDosParametros.ToArray());
                return result1;
            }
            else
                return null;

        } // ExecuteAFunction()

       


        /// <summary>
        /// executa um metodo, da classe do objeto chamador (caller).
        /// O escopo tem referencias das propriedades da classe.
        /// modifica o objeto chamador (caller).
        /// </summary>
        public object ExecuteAMethod(List<Expressao> parametrosDoMetodo, Escopo escopo,  Objeto objetoCaller)
        {
           
            Classe classeDoObjeto = RepositorioDeClassesOO.Instance().GetClasse(objetoCaller.GetTipo());


            Objeto actual = new Objeto(objetoCaller); // o objeto "actual" é uma referência ao objeto que chamou o metodo. é útil para chamada à construtores de classes herdadas
            actual.SetNome("actual"); // seta o nome do objeto "actual".
            escopo.tabela.GetObjetos().Add(actual);


            Escopo escopoDoMetodo = escopo.Clone(); // copia o escopo da classe, como sendo o escopo do método.


            escopoDoMetodo.tabela.GetObjetos().AddRange(objetoCaller.GetFields());

            // copia os valores das propriedades do objeto que chama o método, para dentro do escopo do metodo.
            foreach (Objeto umaPropriedade in objetoCaller.GetFields())
                escopoDoMetodo.tabela.GetObjeto(umaPropriedade.GetNome(), escopoDoMetodo).SetField(umaPropriedade);


            // valoes dos parametros, calculado após a avaliação das expressões parâmetros.
            List<object> valoresDosParametros = new List<object>();

            if ((parametrosDoMetodo != null) && (parametrosDoMetodo.Count > 0))
            {
                // o numero de parametros da funcao e os parametros passados pelo metodo precisam ser iguais.
                if (parametrosDaFuncao.Length != parametrosDoMetodo.Count)
                    return null;
               
                // calcula os valores de cada parâmetro da chamada do método.
                for (int x = 0; x < parametrosDoMetodo.Count; x++) 
                {
                    object valorParametro = new EvalExpression().EvalPosOrdem(parametrosDoMetodo[x], escopo);
                    this.parametrosDaFuncao[x].SetValor(valorParametro);
                    valoresDosParametros.Add(valorParametro);
                }

                // adiciona os objetos parâmetros para o escopo de execução do método.
                escopoDoMetodo.tabela.GetObjetos().AddRange(parametrosDaFuncao); 

           
            }


            object objetoValor = ExecuteAFunction(valoresDosParametros, escopoDoMetodo); 
            
  
            if (classeDoObjeto.propriedadesEstaticas != null) // repassa moficações de propriedades estáticas no escopo.
            {
                List<Objeto> propEstaticas = RepositorioDeClassesOO.Instance().GetClasse(objetoCaller.GetTipo()).propriedadesEstaticas;
                for (int x = 0; x < classeDoObjeto.propriedadesEstaticas.Count; x++)
                {
                    Objeto ObjEstatico = escopoDoMetodo.tabela.GetObjeto(classeDoObjeto.propriedadesEstaticas[x].GetNome(), escopoDoMetodo);
                    if (ObjEstatico != null)
                        classeDoObjeto.propriedadesEstaticas[x].SetValor(ObjEstatico.GetValor());
                }

            }

            // repassa o valor modificados no escopo do método, para as propriedades do objeto caller.
            if (objetoCaller.GetFields() != null) 
                for (int x = 0; x < objetoCaller.GetFields().Count; x++)
                {
                    Objeto propriedadeModificada = escopoDoMetodo.tabela.GetObjetos().Find(k => k.GetNome() == objetoCaller.GetFields()[x].GetNome());
                    if (propriedadeModificada != null)
                        objetoCaller.SetField(propriedadeModificada);

                }


            escopoDoMetodo.Dispose(); // libera os recurso desta seção de execução de método.            

            return objetoValor; 
        }
        /// <summary>
        ///  avalia uma função, tendo como parâmetros expressões, muito apreciada nas chamadas de função, que utiliza expressões como parâmetros.
        ///  o objeto "caller" é utilizado para funções de objetos importados.
        /// </summary>
        public object ExecuteAFunction(List<Expressao> parametros, object caller, Escopo escopo)
        {
            Escopo escopoDaFuncao = new Escopo(escopo);

            List<object> valoresParametro = new List<object>();
            EvalExpression eval = new EvalExpression();

            for (int x = 0; x < parametros.Count; x++)
            {
                parametros[x].isModify = true;
                object umValor = eval.EvalPosOrdem(parametros[x], escopoDaFuncao);
                if ((umValor != null) && (Expressao.Instance.IsNumero(umValor.ToString())))
                    valoresParametro.Add(umValor);
                else
                if ((umValor != null) && (umValor.GetType() == typeof(Objeto))) 
                    valoresParametro.Add(((Objeto)umValor).GetValor());
                else
                    valoresParametro.Add(umValor);
            } // for x



            List<object> parametrosValoresAtuais = new List<object>();
            if (this.parametrosDaFuncao != null)
                for (int x = 0; x < parametrosDaFuncao.Length; x++)
                {
                    parametrosDaFuncao[x].SetValor(valoresParametro[x]);
                    escopoDaFuncao.tabela.GetObjetos().Add(parametrosDaFuncao[x]);

                    parametrosValoresAtuais.Add(parametrosDaFuncao[x]);
                 
                }

            object resultCalcFuncao = null;

            if (this.InfoMethod == null)
                resultCalcFuncao = ExecuteAFunction(parametrosValoresAtuais, escopoDaFuncao);
            else
            if ((this.InfoMethod != null) && (caller != null))
                resultCalcFuncao = this.InfoMethod.Invoke(caller, valoresParametro.ToArray());



            for (int x = 0; x < parametrosDaFuncao.Length; x++)
                escopoDaFuncao.tabela.GetObjetos().Remove(parametrosDaFuncao[x]);



            return resultCalcFuncao;
        }


 

        public object ExecuteAConstructor(List<Expressao> parametros, string nomeClasse, Escopo escopoFuncao, int indexConstrutor)
        {
            Escopo escopo = escopoFuncao.Clone();

            List<object> valoresParametro = new List<object>();

            for (int x = 0; x < parametros.Count; x++)
            {
                object valor = new EvalExpression().EvalPosOrdem(parametros[x], escopo);
                valoresParametro.Add(valor);
            }


            if (this.InfoConstructor != null)
            {
           
                Classe classeDOconstrutor = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
                if (classeDOconstrutor != null)
                    return classeDOconstrutor.construtores[indexConstrutor].InfoConstructor.Invoke(valoresParametro.ToArray()); 
            }
            else
            {
                if (indexConstrutor == -1)
                    return null;

                // os objetos inicializados no construtor são passados no escopo!
                return RepositorioDeClassesOO.Instance().GetClasse(nomeClasse).construtores[indexConstrutor].ExecuteAFunction(valoresParametro, escopo);
                   
            }
            return null;
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


        public static bool IguaisFuncoes(Funcao fncA, Funcao fncB)
        {
            if (fncA.nome != fncB.nome)
                return false;

            if ((fncA.parametrosDaFuncao == null) && (fncB.parametrosDaFuncao == null))
                return true;

            if ((fncA.parametrosDaFuncao == null) && (fncB.parametrosDaFuncao != null))
                return false;

            if ((fncA.parametrosDaFuncao != null) && (fncB.parametrosDaFuncao == null))
                return false;

            if (fncA.parametrosDaFuncao.Length != fncB.parametrosDaFuncao.Length)
                return false;

          
            for (int x = 0; x < fncA.parametrosDaFuncao.Length; x++)
                if (fncA.parametrosDaFuncao[x].GetTipo() != fncB.parametrosDaFuncao[x].GetTipo())
                    return false;

            if (fncA.tipoReturn != fncB.tipoReturn)
                return false;

            return true;
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
            this.nomeClasse = nomeClase;
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

        public static bool IguaisOperadores(Operador op1, Operador op2)
        {
            if ((op1.parametrosDaFuncao == null) && (op2.parametrosDaFuncao == null))
                return true;
             
            if ((op1.parametrosDaFuncao == null) && (op2.parametrosDaFuncao != null))
                return false;

            if ((op1.parametrosDaFuncao != null) && (op2.parametrosDaFuncao == null))
                return false;

            if (op1.parametrosDaFuncao.Length != op2.parametrosDaFuncao.Length)
                return false;

            for (int x = 0; x < op1.parametrosDaFuncao.Length; x++)
                if (op1.parametrosDaFuncao[x].GetTipo() != op2.parametrosDaFuncao[x].GetTipo())
                    return false;

            return true;
        }


        public static Operador GetOperador(string nomeOperador, string classeOperador, string tipo, UmaGramaticaComputacional lng)
        {
            Classe classe = RepositorioDeClassesOO.Instance().GetClasse(classeOperador);
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


        public object ExecuteOperador(string nomeDoOperador, Escopo escopo, params object[] valoresParametros)
        {
            object result = null;
            if (caller == null)
                throw new Exception("objeto que chama a execucao de funcao eh nulo.");
            for (int x = 0; x < valoresParametros.Length; x++)
            {
                if (valoresParametros[x] != null)
                {
                    if (valoresParametros[x].GetType() == typeof(Objeto))
                        valoresParametros[x] = ((Objeto)valoresParametros[x]).GetValor();


                    if ((valoresParametros[x] != null) && Expressao.Instance.IsNumero(valoresParametros[x].ToString()))
                        valoresParametros[x] = Expressao.Instance.ConverteParaNumero(valoresParametros[x].ToString(), escopo);
                }

            }

            if (this.InfoMethod != null)
                result = InfoMethod.Invoke(caller, valoresParametros);
            else
            if (this.instrucoesFuncao != null)
                result = this.ExecuteAFunction(valoresParametros.ToList<object>(), escopo);
            else
                return Expressao.Instance.ConverteParaNumero(result.ToString(), escopo);


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
