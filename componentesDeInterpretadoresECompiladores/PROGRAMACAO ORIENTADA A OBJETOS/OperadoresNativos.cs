using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.Reflection;
using MATRIZES;
namespace parser
{
    public class OperadoresImplementacao
    {
        public string nome;
        public List<Funcao> funcaoImplOperadores = new List<Funcao>();
        public List<MethodInfo> metodosImpl = new List<MethodInfo>();

        // alguns operadores pre-definidos. Pode ser extendido, com metodo AdicionaOperadorNativo().
        List<string> operadoresPrioridade1 = new List<string>() { "+", "-" };
        List<string> operadoresPrioridade2 = new List<string>() { "*", "/" };
        List<string> operadoresPrioridade3 = new List<string>() { "^" };
        List<string> operadoresPrioridade4 = new List<string>() { "<", ">,", ">=", "<=", "==", "!=" };
        List<string> operadoresPrioridade5 = new List<string>() { "=" };
        List<string> operadoresPrioridade6 = new List<string>() { "++", "--" };

        public List<Funcao> GetImplentacao(string classeOperador)
        {
            // obtem dados de metodos da classe herdada, para retirar os metodos da classe herdada, da lista de metodos implementadores de operadores nativos.
            List<MethodInfo> metodosClasseBase = Type.GetType(classeOperador).BaseType.GetMethods().ToList<MethodInfo>();

            // obtem dados de metodos de uma classe.
            MethodInfo[] metodosmplementadores = Type.GetType(classeOperador).GetMethods();
            this.metodosImpl = metodosmplementadores.ToList<MethodInfo>();
            
            
            int prioridadeOperador = 0;
            foreach (MethodInfo umMetodoImpl in metodosmplementadores)
            {
                if (metodosClasseBase.Find(k => k.Name.Equals(umMetodoImpl.Name)) != null)
                    continue;
                string nomeOperador = "";
                string tipoOperador = "";
                if (umMetodoImpl.GetBaseDefinition().Name.Contains("_Operador"))
                    continue;

                GetNomeOperador(umMetodoImpl, ref nomeOperador, ref tipoOperador);
                List<Objeto> parametrosDoOperador = GetTipoParametros(umMetodoImpl);


                prioridadeOperador = GetPrioridadeOperador(nomeOperador);
                Funcao fncImplementador = new Funcao(umMetodoImpl.DeclaringType.Name, "public", nomeOperador, umMetodoImpl, UtilTokens.Casting(umMetodoImpl.ReturnType.Name), parametrosDoOperador.ToArray());


                fncImplementador.tipoReturn = umMetodoImpl.ReturnType.Name;
                fncImplementador.prioridade = prioridadeOperador;
                fncImplementador.tipo = tipoOperador;

                this.funcaoImplOperadores.Add(fncImplementador);
            }
            return this.funcaoImplOperadores;
        }

        // obtem os tipos dos parametros, necessário quando executar o operador.
        private List<Objeto> GetTipoParametros(MethodInfo info)
        {
            List<Objeto> objetosParametros = new List<Objeto>();
            for (int x = 0; x < info.GetParameters().Length; x++)
            {
                ParameterInfo parametroDados = info.GetParameters()[x];
                Type umTipoParametro = parametroDados.ParameterType;
                Objeto obj_param = new Objeto("private", UtilTokens.Casting(umTipoParametro.Name), "a", null);
                objetosParametros.Add(obj_param);
            }
            return objetosParametros;

        }
        /// <summary>
        /// permite a extensão de operadores, com importação da função que executa o operador, atraves de reflexão.
        /// </summary>
        /// <param name="nomeOperador">nome do operador.</param>
        /// <param name="umMetodoImpl">metodo info reflexao que contem os dados da funcao que executa o operador.</param>
        /// <param name="prioridade">prioridade do operador, para calculos em expressoes.</param>
        public void AdicionaOperadorNativo(string nomeOperador, MethodInfo umMetodoImpl, int prioridade, string classeDoOperador)
        {
            string tipoOperador = "";
            Funcao fncImplementador = null;
            GetNomeOperador(umMetodoImpl, ref nomeOperador, ref tipoOperador);

            List<Objeto> parametrosObjetos = null;
            if ((umMetodoImpl.GetParameters() != null) && (umMetodoImpl.GetParameters().Length > 0))
                parametrosObjetos = GetTipoParametros(umMetodoImpl);
            if (parametrosObjetos == null)
                fncImplementador = new Funcao(classeDoOperador, "public", nomeOperador, umMetodoImpl, UtilTokens.Casting(umMetodoImpl.ReturnType.Name));
            else
                fncImplementador = new Funcao(classeDoOperador, "public", nomeOperador, umMetodoImpl, UtilTokens.Casting(umMetodoImpl.ReturnType.Name), parametrosObjetos.ToArray());

            fncImplementador.prioridade = prioridade;

            this.funcaoImplOperadores.Add(fncImplementador);
        }


        private int GetPrioridadeOperador(string nomeOperador)
        {
            int prioridadeOperador = 7;

           
            if (operadoresPrioridade1.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 2;
            if (operadoresPrioridade2.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 3;
            if (operadoresPrioridade3.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 4;
            if (operadoresPrioridade4.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 5;
            
            if (operadoresPrioridade5.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 0;

            if (operadoresPrioridade6.Find(k => k.Equals(nomeOperador)) != null)
                prioridadeOperador = 6;

            return prioridadeOperador;
        }

        private static void GetNomeOperador(MethodInfo umMetodoImpl, ref string nomeOperador, ref string tipoOperador)
        {
            switch (umMetodoImpl.Name)
            {
                case "Igual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "=";
                    break;
                case "Soma":
                    tipoOperador = "BINARIO";
                    nomeOperador = "+";
                    break;
                case "Sub":
                    tipoOperador = "BINARIO";
                    nomeOperador = "-";
                    break;
                case "Mult":
                    tipoOperador = "BINARIO";
                    nomeOperador = "*";
                    break;
                case "Div":
                    tipoOperador = "BINARIO";
                    nomeOperador = "/";
                    break;
                case "ComparacaoIgual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "==";
                    break;
                case "Desigual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "!=";
                    break;
                case "Maior":
                    tipoOperador = "BINARIO";
                    nomeOperador = ">";
                    break;
                case "MaiorOuIgual":
                    tipoOperador = "BINARIO";
                    nomeOperador = ">=";
                    break;
                case "Menor":
                    tipoOperador = "BINARIO";
                    nomeOperador = "<";
                    break;
                case "MenorOuIgual":
                    tipoOperador = "BINARIO";
                    nomeOperador = "<=";
                    break;
                case "IncrementoUnario":
                    tipoOperador = "UNARIO";
                    nomeOperador = "++";
                    break;
                case "DecrementoUnario":
                    tipoOperador = "UNARIO";
                    nomeOperador = "--";
                    break;
                case "Atribuicao":
                    tipoOperador = "BINARIO";
                    nomeOperador = "=";
                    break;
                case "Potenciacao":
                    tipoOperador = "BINARIO";
                    nomeOperador = "^";
                    break;
                default:
                    tipoOperador = "BINARIO";
                    nomeOperador = umMetodoImpl.Name;
                    break;
            }

        }

    }


    public class OperadoresInt: OperadoresImplementacao
    {
        public delegate int operadorBinario(int x, int y);
        public delegate int operadorUnario(int x);


        public int Soma(int x, int y)
        {
            return x + y;
        }
        public int Sub(int x, int y)
        {
            return x - y;
        }

        public int Mult(int x, int y)
        {
            return x * y;
        }

        public int Div(int x, int y)
        {
            if (y == 0)
                throw new Exception("divisao por zero!");
            return x / y;
        }
     
        public int Igual(int x, int y)
        {
            return y;
        }

        public bool ComparacaoIgual(int x, int y)
        {
            return x == y;
        }

        public bool Desigual(int x, int y)
        {
            return x != y;
        }

        public bool Maior(int x, int y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(int x, int y)
        {
            return x >= y;
        }

        public bool Menor(int x, int y)
        {
            return x < y;
        }

        public bool MenorOuIgual(int x, int y)
        {
            return x <= y;
        }

        public int Atribuicao(int x)
        {
            return x;
        }

        public int IncrementoUnario(int x)
        {
            return ++x;
        }

        public int DecrementoUnario(int x)
        {
            return --x;
        }

        public int Potenciacao(int x, int y)
        {
            return (int)Math.Pow(x, y);
        }
    }



    public class OperadoresDouble : OperadoresImplementacao
    {
        public delegate double operadorBinario(double x, double y);
        public delegate double operadorUnario(double x);


        public double Soma(double x, double y)
        {
            return x + y;
        }
        public double Sub(double x, double y)
        {
            return x - y;
        }

        public double Mult(double x, double y)
        {
            return x * y;
        }

        public double Div(double x, double y)
        {
            if (y == 0)
                throw new Exception("divisao por zero!");
            return x / y;
        }

        public double Igual(double x, double y)
        {
            return y;
        }

        public bool ComparacaoIgual(double x, double y)
        {
            return x == y;
        }

        public bool Desigual(double x, double y)
        {
            return x != y;
        }

        public bool Maior(double x, double y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(double x, double y)
        {
            return x >= y;
        }

        public bool Menor(double x, double y)
        {
            return x < y;
        }

        public bool MenorOuIgual(double x, double y)
        {
            return x <= y;
        }

        public double Atribuicao(double x)
        {
            return x;
        }

        public double IncrementoUnario(double x)
        {
            return ++x;
        }

        public double DecrementoUnario(double x)
        {
            return --x;
        }

        public double Potenciacao(double x, double y)
        {
            return (double)Math.Pow(x, y);
        }
    }


    public class OperadoresFloat : OperadoresImplementacao
    {
        public delegate float operadorBinario(float x, float y);
        public delegate float operadorUnario(float x);

      
        public float Soma(float x, float y)
        {
            return x + y;
        }
        public float Sub(float x, float y)
        {
            return x - y;
        }

        public float Mult(float x, float y)
        {
            return x * y;
        }

        public float Div(float x, float y)
        {
            if (y == 0)
                throw new Exception("divisao por zero!");
            return x / y;
        }

        public float Igual(float x, float y)
        {
            return y;
        }

        public bool ComparacaoIgual(float x, float y)
        {
            return x == y;
        }

        public bool Desigual(float x, float y)
        {
            return x != y;
        }

        public bool Maior(float x, float y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(float x, float y)
        {
            return x >= y;
        }

        public bool Menor(float x, float y)
        {
            return x < y;
        }

        public bool MenorOuIgual(float x, float y)
        {
            return x <= y;
        }

        public float Atribuicao(float x)
        {
            return x;
        }

        public float IncrementoUnario(float x)
        {
            return ++x;
        }

        public float DecrementoUnario(float x)
        {
            return --x;
        }

        public float Potenciacao(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }
    }


    public class OperadoresString : OperadoresImplementacao
    {
        public delegate string operadorBinario(string x, string y);
        public delegate string operadorUnario(string x);

        public string Soma(string x, string y)
        {
            return x + y;
        }
        public string Sub(string x, string y)
        {
            return x.Replace(y, "");
        }

        public string Igual(string x, string y)
        {
            return (string)y.Clone();
        }
        public bool ComparacaoIgual(string x, string y)
        {
            return x == y;
        }

        public bool Desigual(string x, string y)
        {
            return x != y;
        }

        public bool Maior(string x, string y)
        {
            return x.Length > y.Length;
        }

        public bool MaiorOuIgual(string x, string y)
        {
            return x.Length >= y.Length;
        }

        public bool Menor(string x, string y)
        {
            return x.Length < y.Length;
        }

        public bool MenorOuIgual(string x, string y)
        {
            return x.Length <= y.Length;
        }

        public string Atribuicao(string x)
        {
            return x;
        }
    }

    public class OperadoresChar : OperadoresImplementacao
    {
        public delegate Char operadorBinario(Char x, Char y);
        public delegate Char operadorUnario(Char x);


        public Char Igual(Char x, Char y)
        {
            return y;
        }

        public bool ComparacaoIgual(Char x, Char y)
        {
            return x == y;
        }

        public bool Desigual(Char x, Char y)
        {
            return x != y;
        }

        public bool Maior(Char x, Char y)
        {
            return x > y;
        }

        public bool MaiorOuIgual(Char x, Char y)
        {
            return x>= y;
        }

        public bool Menor(Char x, Char y)
        {
            return x < y;
        }

        public bool MenorOuIgual(Char x, Char y)
        {
            return x <= y;
        }

        public Char Atribuicao(Char x)
        {
            return x;
        }
    }


    public class OperadoresBoolean : OperadoresImplementacao
    {
        public delegate Boolean operadorBinario(Boolean x, Boolean y);
        public delegate Boolean operadorUnario(Boolean x);

        public Boolean Igual(Boolean x, Boolean y)
        {
            return y;
        }

        public bool ComparacaoIgual(Boolean x, Boolean y)
        {
            return x == y;
        }

        public bool Desigual(Boolean x, Boolean y)
        {
            return x != y;
        }


        public Boolean Atribuicao(Boolean x)
        {
            return x;
        }


    }
    public class OperadoresMatriz:OperadoresImplementacao
    {


        private static Matriz aux1;
        private static Matriz aux2;

        public Matriz mtMain;

        public OperadoresMatriz() { }


        public OperadoresMatriz(int linhas, int colunas)
        {
            if ((aux1 == null) || (aux2 == null))
            {
                aux1 = new Matriz(1, 5);
                aux2 = new Matriz(5, 1);

                aux1.PreencheMatriz(1.0);
                aux2.PreencheMatriz(1.5);
            } // if

            this.nome = "Matriz";
            this.mtMain = new Matriz(linhas, colunas);
        }

        public static Matriz GetMatriz(Matriz M)
        {
            Matriz mt = new Matriz(M.qtLin, M.qtCol);
            for (int lin = 0; lin < M.qtLin; lin++)
                for (int col = 0; col < M.qtCol; col++)
                    mt.SetElement(lin, col, M.GetElement(lin, col));
            return mt;
        }
        public float GetElement(int lin, int col)
        {
            return (float)this.mtMain.GetElement(lin, col);
        }

        public void SetElement(int lin, int col, object valor)
        {
            this.mtMain.SetElement(lin, col, (float)valor);
        }
        private object Soma(params object[] operandos)
        {
            Matriz m1, m2, mResult;
            LoadParameters(operandos, out m1, out m2, out mResult);
            mResult = m1+ m2;
            return mResult;
        } // OperadorAdicao


        private Matriz Sub(params object[] operandos)
        {
            Matriz m1, m2, mResult;
            LoadParameters(operandos, out m1, out m2, out mResult);
            mResult = m1 - m2;
            return mResult;
        } // OperadorSubtracao

        private Matriz Mult(params object[] operandos)
        {
            Matriz m1, m2, mResult;
            LoadParameters(operandos, out m1, out m2, out mResult);
            mResult = m1 * m2;
            return mResult;
        } // OperadorAdicao

        private Matriz Div(params object[] operandos)
        {
            Matriz m1, m2, mResult;

            LoadParameters(operandos, out m1, out m2, out mResult);
            mResult = Mult(m1, Matriz.MatrizInversaNaoQuadratica(m2));
            return mResult;
        } // OperadorAdicao

        private Matriz Igual(Matriz m1, Matriz m2)
        {
            return m2.Clone();
        }
        
        private object Maior(params object[] operandos)
        {
            Matriz m1 = (Matriz)operandos[0];
            Matriz m2 = (Matriz)operandos[1];
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
                return null;
            return (gp1 > gp2);
        }

        private object MaiorOuIgual(params object[] operandos)
        {
            Matriz m1 = (Matriz)operandos[0];
            Matriz m2 = (Matriz)operandos[1];
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
                return null;
            return (gp1 >= gp2);
        }

        private object Menor(params object[] operandos)
        {
            Matriz m1 = (Matriz)operandos[0];
            Matriz m2 = (Matriz)operandos[1];
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
                return null;
            return (gp1 < gp2);
        }

        private object MenorOuIgual(params object[] operandos)
        {
            Matriz m1 = (Matriz)operandos[0];
            Matriz m2 = (Matriz)operandos[1];
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
                return null;
            return (gp1 <= gp2);
        }



        private object ComparacaoIgual(params object[] operandos)
        {
            Matriz m1 = (Matriz)operandos[0];
            Matriz m2 = (Matriz)operandos[1];
            double gp1 = 0.0;
            double gp2 = 0.0;
            if (!CalcGrauPrecisao(m1, m2, ref gp1, ref gp2))
                return null;
            return (gp1 == gp2);
        }




        //__________________________________________________________________________________________________________________________________________________________________
        // CÁLCULO DO GRAU DE PRECISÃO, UM REDUTOR DE DIMENSÕES DE MATRIZ, UTILIZADA PARA COMPARAR MATRIZES.
        /// <summary>
        /// calcula o redutor de matrizes, grau de precisão.
        /// 1- se as colunas das matrizes auxiliares tiverem colunas diferentes da matriz m1, recalcula as matrizes auxiliares, com o mesmo numero de colunas da matriz m1.
        /// 2- se as matrizes m1 e m2 tiverem dimensões diferentes, retorna false, pois não há como comparar graus de precisão com matrizes de dimensões diferentes entre si.
        private static bool CalcGrauPrecisao(Matriz m1, Matriz m2, ref double grauPrecisao1, ref double grauPrecisao2)
        {

            if ((m1.qtCol != m2.qtCol) || (m1.qtCol != m2.qtCol))
                return false;
            if (m1.qtCol != aux1.qtCol)
            {
                aux1 = new Matriz(1, m1.qtCol);
                aux2 = new Matriz(m1.qtCol, 1);
                aux1.PreencheMatriz(1.5);
                aux2.PreencheMatriz(1.5);
            }

            grauPrecisao1 = GetMatriz(aux1* m1 * aux2).GetElement(0, 0);
            grauPrecisao2 = GetMatriz(aux1 * m2 * aux2).GetElement(0, 0);

            return true;
        }


        private void LoadParameters(object[] operandos, out Matriz m1, out Matriz m2, out Matriz mResult)
        {
            m1 = (Matriz)operandos[0];
            m2 = (Matriz)operandos[1];
            mResult = new Matriz(m1.qtLin, m1.qtCol);
        }


    }
 
    
} // namespace
