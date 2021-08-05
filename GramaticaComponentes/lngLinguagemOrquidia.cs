﻿using System.Collections.Generic;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// implementa uma linguage chamada orquidea
    /// </summary>
    public class LinguagemOrquidea : UmaGramaticaComputacional
    {

        /// <summary>
        /// inicializa dados da linguagem orquídea.
        /// </summary>
        public LinguagemOrquidea() : base()
        {
              this.inicializaOperadores();// inicializa todos operadores das classes nativas.
        } // LinguagemOrquidea()

        public static List<Operador> operadoresCondicionais { get; set; }
        public static List<Operador> operadoresBinarios { get; set; }
        public static List<Operador> operadoresUnarios { get; set; }

        public void inicializaOperadores()
        {

            if (LinguagemOrquidea.Classes != null)
                return;

            List<Operador> op_int = new List<Operador>();
            List<Operador> op_float = new List<Operador>();
            List<Operador> op_string = new List<Operador>();
            List<Operador> op_bool = new List<Operador>();
            List<Operador> op_char = new List<Operador>();
            List<Operador> op_matriz = new List<Operador>();



            List<Funcao> operadores_int = new _Operador().GetImplentacao("parser.OperadoresInt");
            List<Funcao> operadores_float = new _Operador().GetImplentacao("parser.OperadoresFloat");
            List<Funcao> operadores_string = new _Operador().GetImplentacao("parser.OperadoresString");
            List<Funcao> operadrores_char = new _Operador().GetImplentacao("parser.OperadoresChar");
            List<Funcao> operadores_boolean = new _Operador().GetImplentacao("parser.OperadoresBoolean");
            List<Funcao> operadores_matriz = new _Operador().GetImplentacao("parser.OperadoresMatriz");


            CriaOperadores(op_int, operadores_int);
            CriaOperadores(op_float, operadores_float);
            CriaOperadores(op_string, operadores_string);
            CriaOperadores(op_char, operadrores_char);
            CriaOperadores(op_bool, operadores_boolean);
            CriaOperadores(op_matriz, operadores_matriz);



            LinguagemOrquidea.Classes = new List<Classe>();

            adicionaClasse(new Classe("public", "int", null, op_int, null));
            adicionaClasse(new Classe("public", "float", null, op_float, null));
            adicionaClasse(new Classe("public", "string", null, op_string, null));
            adicionaClasse(new Classe("public", "bool", null, op_bool, null));
            adicionaClasse(new Classe("public", "char", null, op_char, null));

          
            GetOperadores().AddRange(op_int);
            GetOperadores().AddRange(op_float);
            GetOperadores().AddRange(op_string);
            GetOperadores().AddRange(op_bool);
            GetOperadores().AddRange(op_char);
            GetOperadores().AddRange(op_matriz);

            operadoresCondicionais = this.GetOperadoresCondicionais();
            operadoresBinarios = this.GetOperadoresBinarios();
            operadoresUnarios = this.GetOperadoresUnarios();

        } // inicializaOperadores()

        private static void CriaOperadores(List<Operador> operadores, List<Funcao> funcoesImpl)
        {
            for (int x = 0; x < funcoesImpl.Count; x++)
            {
                string tipoDeOperador = "";
                tipoDeOperador = funcoesImpl[x].tipo;
                operadores.Add(new Operador(funcoesImpl[x].nomeClasse, funcoesImpl[x].nome, funcoesImpl[x].prioridade, funcoesImpl[x].parametrosDaFuncao, tipoDeOperador, funcoesImpl[x].InfoMethod, null));
            }
        }

        /// adiciona o operador para os operadores binários da linguagem.
        public bool AddOperator(Operador operador)
        {
            List<Operador> todosOperadoresDaLinguagem = this.GetOperadores();
            for (int op = 0; op < todosOperadoresDaLinguagem.Count; op++)
            {
                if ((todosOperadoresDaLinguagem[op].nome == operador.nome) &&
                    (todosOperadoresDaLinguagem[op].parametrosDaFuncao[0].tipo == operador.parametrosDaFuncao[0].tipo) &&
                    (todosOperadoresDaLinguagem[op].parametrosDaFuncao.Length > 1) &&
                    (todosOperadoresDaLinguagem[op].parametrosDaFuncao[1].tipo == operador.parametrosDaFuncao[1].tipo)) 
                    return false;
            } // for op
            this.GetOperadores().Add(operador);
            return true;
        } // AddOperatorInLanguem()

        public static List<Classe> Classes { get; set; }
        
        public List<Classe> GetClasses()
        {
            return Classes;
        } // GetClasses()


        private void adicionaClasse(Classe c)
        {
            Classes.Add(c);
            RepositorioDeClassesOO.Instance().RegistraUmaClasse(c);
        } // adicionaClasse()

        /// <summary>
        /// inicializa as produções a partir de um arquivo XML.
        /// </summary>
        public override void inicializaProducoesDaLinguagem()
        {
            if (UmaGramaticaComputacional.producoes.Count == 0)
            {
                xmlREADER_LINGUAGEM xmlreader = new xmlREADER_LINGUAGEM();
                xmlreader.LE_ARQUIVO_LINGUAGEM(producoes, "ORQUIDIA");
            } // if

        } // inicializaProducoesParaLinguagem()


        public  Operador GetOperador(string nome, string nomeClasse)
        {
            Classe classe = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);
            if (classe != null)
            {
                List<Operador> operadores = classe.GetOperadores().FindAll(k => k.nome == nome);
                if ((operadores == null) || (operadores.Count == 0))
                    return null;

                return operadores[0];
            }
            return null;
        }


        public override List<string> GetTodosOperadores()
        {

            List<string> nomeOperadores = base.GetTodosOperadores();

            List<Classe> classesNativas = RepositorioDeClassesOO.Instance().classesRegistradas;
            if (classesNativas != null)
                for (int x = 0; x < classesNativas.Count; x++)
                {
                    for (int y = 0; y < classesNativas[x].GetOperadores().Count; y++)
                    {
                        string nome = classesNativas[x].GetOperadores()[y].nome;
                        if (nome != "")
                        {
                            int indexOperador = nomeOperadores.FindIndex(k => k.Equals(nome));
                            if (indexOperador == -1)
                                nomeOperadores.Add(nome);
                        }
                    }
                } // for x
            return nomeOperadores;
        }

        public bool isOperador(string tokenOperador)
        {
            return GetOperadores().Find(k => k.nome.Equals(tokenOperador)) != null;
        }


        public override List<Operador> GetOperadores()
        {
            List<Operador> operadores = base.GetOperadores().ToList<Operador>();

            List<Classe> classes = RepositorioDeClassesOO.Instance().classesRegistradas;
            if ((classes != null) && (classes.Count > 0))
                for (int x = 0; x < classes.Count; x++)
                {
                    List<Operador> operadoresDaClasse = classes[x].GetOperadores();
                    if ((operadoresDaClasse != null) && (operadoresDaClasse.Count > 0))
                        operadores.AddRange(operadoresDaClasse);
                } // for x

            return operadores;
        }

        public List<Operador> GetOperadoresCondicionais() 
        {
            List<Operador> condicionais = this.GetOperadores().FindAll(k => k.GetTipoFuncao().Contains("CONDICIONAL"));
            return condicionais;
        } // GetOperadoresCondicionais() 

        public List<Operador> GetOperadoresBinarios()
        {
            List<Operador> binarios=this.GetOperadores().FindAll(k => k.GetTipo().Contains("BINARIO"));
            return binarios;

        } // GetOperadoresBinarios()

        public List<Operador> GetOperadoresUnarios()
        {
            List<Operador> unarios = this.GetOperadores().FindAll(k => k.GetTipo().Contains("UNARIO"));
            return unarios;
        } // GetOperadoresUnarios()

        public bool IsOperadorBinario(string nomeOperador)
        {
            return operadoresBinarios.FindIndex(k => k.nome == nomeOperador) != -1;
        } // IsOperadorBinario()

        public bool IsOperadorUnario(string nomeOperador)
        {
            return operadoresUnarios.FindIndex(k => k.nome == nomeOperador) != -1;
        } // IsOperadorUnario()


        public bool IsOperadorCondicional(string nomeOperador)
        {
            return operadoresCondicionais.FindIndex(k => k.nome == nomeOperador && k.GetTipoFuncao() == "CONDICIONAL") != -1;
        } // IsOperadorCondicional()


        public bool IsOperadorAritmetico(string nomeOperador)
        {
            int indexBinariosAritmeticos = operadoresBinarios.FindIndex(k => k.nome == nomeOperador && (k.GetTipoFuncao().Contains("ARITMETICO")));
            if (indexBinariosAritmeticos != -1)
                return true;
            int indexUnariosAritmeticos = this.GetOperadoresUnarios().FindIndex(k => k.nome == nomeOperador && (k.GetTipoFuncao().Contains("ARITMETICO")));
            if (indexUnariosAritmeticos != -1)
                return true;
            return false;

        } // IsOperadorAritmetico()
    } // class LinguagemOrquidea


    // classe para conversores de tipos.
    public class ConversoresDeTipos
    {

        public List<ConverteParaOutroTipo> ConversoresDinamico = new List<ConverteParaOutroTipo>();

        public delegate object ConverteParaOutroTipo(object typeAtConvert);

        public static int ConverFloatToInt(float x)
        {
            return (int)x;
        }
        public static float ConvertIntToFloat(int x)
        {
            return (float)x;
        }

        public static int ConvertByteToInt(byte b)
        {
            return (int)b;
        }

        public static byte ConvertIntToByte(int x)
        {
            return (byte)x;
        }
    }// class

} // namespace parser
