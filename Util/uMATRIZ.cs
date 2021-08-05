using System;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Spatial.Euclidean;
using MathNet.Numerics.LinearAlgebra;
using parser;
namespace MATRIZES
{
    // **************************************************************************
    //                              CLASSE MATRIZ
    // **************************************************************************                             
    // classe que encapsula uma matriz bidimensional- base
    // para calculos. contem um aleatorizador para
    // preencimento randomico de elementos.
    // Metodos publicos:
    // ->Matriz(lin,col) : construtor;
    // ->Matriz(lin,col,params double[] elementos): construtor com vetor de elementos iniciais.
    // ->Matriz.MatrizInversa(A): calcula a Matriz inversa da Matriz A.
    // ->Identidade(): calcula a Matriz identidade da matriz que chamou o método.
    // ->operadores +,-,/,*: operadores para operações matriciais.
    // ->ToString(): coloca no console os elementos da Matriz
    //   que chamou o metodo.
    // ->preencheMatriz(intervalo): prenche uma Matriz com 
    //   valores aleatorios variando de [0..intervalo*qtLin].
    // ->Sort(): ordena uma matriz se esta tiver lin=1 ou col=1.
    // ->Clone(): devolve uma copia da matriz que chamou o metodo;
    // ->Determinante(Matriz M): encontra o determinante da matriz M.

    /// <summary>
    /// classe que encapsula o conceito matematico de matriz.
    /// </summary>
    public class Matriz
    {
        private double[,] matriz;
        public int qtLin;
        public int qtCol;
        private static Random aleatorizador = new Random(seedRandom);
        private static int seedRandom = 10000;


        public void SetElement(int lin, int col, double el)
        {
            this.matriz[lin, col] = el;

        } // void setElemento()
        public static Matriz Identidade(int dim)
        {
            Matriz I = new Matriz(dim, dim);
            for (int l = 0; l < dim; l++)
                for (int c = 0; c < dim; c++)
                {
                    if (l == c)

                        I.matriz[l, c] = 1.0;
                    else
                        I.matriz[l, c] = 0.0;
                } // for int c
            return (I);
        } //Identidade()

        public double GetElement(int lin, int col)
        {
            return (this.matriz[lin, col]);
        } // double getElemento()

        public Matriz(int lin, int col, params double[] elementos)
        {
            this.qtLin = lin;
            this.qtCol = col;
            this.matriz = new double[this.qtLin, this.qtCol];
            if (elementos.Length == (this.qtLin * this.qtCol))
            {
                int contadorElementos = 0;
                try
                {
                    for (int l = 0; l < this.qtLin; l++)
                        for (int c = 0; c < this.qtCol; c++)
                            this.matriz[l, c] = elementos[contadorElementos++];
                }
                catch { throw new Exception("Falta ou exesso de parametros no construtor. "); }
            } // if
        } // Matriz()




        public Matriz(int lin, int col)
        {
            this.qtLin = lin;
            this.qtCol = col;
            this.matriz = new double[this.qtLin, this.qtCol];

        } // Matriz()

        /// construtor basico para classes herdeiras.
        /// aceita o numero de linhas e colunas da matriz.
        /// <param name="lin">numero de linhas da matriz.</param>
        /// <param name="col">numero de colunas da matriz.</param>
        public void InitDeviceMatriz(int lin, int col)
        {
            this.qtLin = lin;
            this.qtCol = col;
            this.matriz = new double[this.qtLin, this.qtCol];
        } // Matriz()

        /// metodo controle para construtores e clone. Copia para
        /// a matriz que chamou o metodo a matriz parametro.
        /// <param name="M">matriz parametro a ser copiada.</param>
        public void InitDeviceMatriz(Matriz M)
        {
            this.qtLin = M.qtLin;
            this.qtCol = M.qtCol;
            this.matriz = new double[this.qtLin, this.qtCol];
            int lin, col;
            for (lin = 0; lin < this.qtLin; lin++)
                for (col = 0; col < this.qtCol; col++)
                    this.matriz[lin, col] = M.matriz[lin, col];

        } // Matriz()
        /// retorna uma copia da matriz que chamou o metodo.
        public Matriz Clone()
        {
            return (Matriz)this.MemberwiseClone();
        } // Clone()

        /// multiplica duas matrizes, e retorna como produto uma matriz.
        /// <param name="A">matriz operando 1.</param>
        /// <param name="B">matriz operando 2.</param>
        private static Matriz MulMatriz2(Matriz A, Matriz B)
        {
            Matriz C = new Matriz(A.qtLin, B.qtCol);
            int lin, col, k;
            for (lin = 0; lin < C.qtLin; lin++)
            {
                for (col = 0; col < C.qtCol; col++)
                {
                    C.matriz[lin, col] = 0.0;
                    for (k = 0; k < A.qtCol; k++)
                        C.matriz[lin, col] += A.matriz[lin, k] * B.matriz[k, col];
                } // for col

            } // for lin

            return (C);
        } // MulMatriz()

        private static Matriz MulMatriz(Matriz a, Matriz b)
        {
            if (a.matriz.GetLength(1) == b.matriz.GetLength(0))
            {
                Matriz c = new Matriz(a.matriz.GetLength(0), b.matriz.GetLength(1));
                for (int i = 0; i < c.matriz.GetLength(0); i++)
                {
                    for (int j = 0; j < c.matriz.GetLength(1); j++)
                    {
                        c.matriz[i, j] = 0;
                        for (int k = 0; k < a.matriz.GetLength(1); k++) // OR k<b.GetLength(0)
                            c.matriz[i, j] = c.matriz[i, j] + a.matriz[i, k] * b.matriz[k, j];
                    } // for j
                } /// for i
                return c;
            }// if
            return null;
        }
        /// <summary>
        /// soma duas matrizes. Operador de adição.
        /// </summary>
        /// <param name="A">Matriz Parametro.</param>
        /// <param name="B">Matriz Parametro.</param>
        /// <returns>retorna uma Matriz soma das matrizes [A] e [B].</returns>
        public static Matriz operator +(Matriz A, Matriz B)
        {
            try
            {
                Matriz C = new Matriz(A.qtLin, A.qtCol);
                for (int lin = 0; lin < A.qtLin; lin++)
                    for (int col = 0; col < A.qtCol; col++)
                    {
                        C.matriz[lin, col] = A.matriz[lin, col] + B.matriz[lin, col];
                    } // for int col
                return (C);
            } // try
            catch
            {
                return (null);
            } // catch

        } // Matriz operator +()

        /// <summary>
        /// subtrai duas matrizes. Operador de subtração.
        /// </summary>
        /// <param name="A">Matriz Parametro.</param>
        /// <param name="B">Matriz Parametro.</param>
        /// <returns>retorna uma Matriz diferença entre as matrizes [A] e [B].</returns>
        public static Matriz operator -(Matriz A, Matriz B)
        {
            try
            {
                Matriz C = new Matriz(A.qtLin, A.qtCol);
                for (int lin = 0; lin < A.qtLin; lin++)
                    for (int col = 0; col < A.qtCol; col++)
                    {
                        C.matriz[lin, col] = A.matriz[lin, col] - B.matriz[lin, col];
                    } // for int col
                return (C);
            } // try
            catch
            {
                return (null);
            } // catch

        } // Matriz operator -()

        /// <summary>
        /// MULTIPLICA DUAS MATRIZES [A] E [B], DESDE QUE [A] E [B] OBEDECAM A REGRA:
        /// O NUMERO DE COLUNAS DE [A] DEVE SER IGUAL AO NUMERO DE COLUNAS DE [B].
        /// </summary>
        /// <param name="A">MATRIZ MULTIPLICANDA [A].</param>
        /// <param name="B">MATRIZ MULTIPLICANDA [B].</param>
        /// <returns>RETORNA O PRODUTO DA MULTIPLICACAO DE [A] POR [B].</returns>
        public static Matriz operator *(Matriz A, Matriz B)
        {
            if (A.qtCol == B.qtLin)
            {
                Matriz C = Matriz.MulMatriz(A, B);
                return (C);
            }
            else
                return (null);
        } // operator *

        public static Matriz operator *(double d, Matriz B)
        {
            Matriz R = B.Clone();
            for (int lin = 0; lin < R.qtLin; lin++)
                for (int col = 0; col < R.qtCol; col++)
                {
                    R.SetElement(lin, col, d * B.GetElement(lin, col));
                }// for col
            return R;
        } //operator *

        public static vetor2 operator *(Matriz M, vetor2 v)
        {
            Matriz mt2D = new Matriz(2, 1, v.X, v.Y);
            Matriz mtResult = M * mt2D;
            return new vetor2(mtResult.GetElement(0, 0), mtResult.GetElement(1, 0));
        }

    
        public static Matriz MatrizInversaNaoQuadratica(Matriz M)
        {
        
            if (M.qtLin>M.qtCol)
            {
               
                /// M[3,2]*M(-1)[2,3]
                /// M[3,2] é conhecida:
                /// Aux[2,3]*M*M(-1)=Aux
                /// (Aux*M)(-1)=M(-1)=(Aux*M)(-1)*Aux

                Matriz Aux = new Matriz(M.qtCol, M.qtLin);
                Aux.PreencheMatriz(1.0);
                Matriz MInv = MatrizInversa(Aux * M) * Aux;
                return MInv;

            }
            else
            if (M.qtLin<M.qtCol)
            {
                /// M[3,2]*M(-1)[2,3] ---> M[3,2]=M(-1)
                /// M[2,3] é conhecida.
                /// M(-1)*M[2,3]*Aux[3,2]=Aux[3,2]
                /// M(-1)=Aux*(M*Aux)(-1)

                Matriz Aux = new Matriz(M.qtCol, M.qtLin);
                Aux.PreencheMatriz(1.0);
                Matriz MInv = Aux * Matriz.MatrixInverse(M * Aux);
                return MInv;
            }
            else
            {
                Matriz MInv = Matriz.MatrixInverse(M);
                return MInv;
            }
        } // MatrizInversaNaoQuadratica()

        /// <summary>
        /// calcula a matriz inversa da matriz parametro.
        /// </summary>
        /// <param name="A">matriz a ter a inversa calculada.</param>
        /// <returns></returns>
        public static Matriz MatrizInversa(Matriz A)
        {
            if ((A.qtCol == 1) && (A.qtLin == 1))
            {
                Matriz M = new Matriz(1, 1);
                M.matriz[0, 0] = 1 / A.matriz[0, 0];
                return (M);
            } // if
            else
            {
                Matriz Adjt = A.Adjunta();
                Matriz M = new Matriz(A.qtLin, A.qtCol);
                int lin, col;
                double D = Matriz.determinante(A);
                for (lin = 0; lin < A.qtLin; lin++)
                    for (col = 0; col < A.qtCol; col++)
                        M.matriz[lin, col] = Adjt.matriz[lin, col] / D;
                return (M.Transposta());
            }// else
        } // MatrizInversa()

        /// <summary>
        /// calcula a matriz Transposta da matriz que chamou o metodo.
        /// </summary>
        /// <returns>retorna a matriz Transposta da matriz que chamou o método.</returns>
        private Matriz Adjunta()
        {
            int lin, col;
            Matriz Cofatores = new Matriz(this.qtLin, this.qtCol);
            for (lin = 0; lin < this.qtLin; lin++)
                for (col = 0; col < this.qtCol; col++)
                {
                    Cofatores.matriz[lin, col] = Matriz.determinante(this.ReduzMatriz(lin, col));
                    if (((lin + col) % 2) == 1)
                        Cofatores.matriz[lin, col] = -Cofatores.matriz[lin, col];
                } // for col
            return (Cofatores);
        } // Adjunta()

        /// <summary>
        /// calcula a matriz Transposta da matriz que chamou o metodo.
        /// </summary>
        /// <returns>retorna a matriz Transposta.</returns>
        private Matriz Transposta()
        {
            Matriz M = new Matriz(this.qtCol, this.qtLin);
            int lin, col;
            for (lin = 0; lin < this.qtLin; lin++)
                for (col = 0; col < this.qtCol; col++)
                    M.matriz[col, lin] = this.matriz[lin, col];
            return (M);
        } // Transposta()


        /// <summary>
        /// calcula o determinante  de uma matriz.
        /// </summary>
        /// <param name="M">Matriz a ter o determinante calculado.</param>
        /// <returns></returns>
        private static double determinante(Matriz M)
        {
            if (M.qtLin == 2)
            {
                double d = M.matriz[0, 0] * M.matriz[1, 1] - M.matriz[0, 1] * M.matriz[1, 0];
                return (d);
            } // if M.qtLin
            else
                if (M.qtLin == 3)
            {

                // det A = a00.a11.a22 + a01.a12.a20 + a02.a10.a21 – a02.a11.a20 – a00.a12.a22 – a01.a10.a22
                double d = +M.matriz[0, 0] * M.matriz[1, 1] * M.matriz[2, 2]
                               + M.matriz[0, 1] * M.matriz[1, 2] * M.matriz[2, 0]
                               + M.matriz[0, 2] * M.matriz[1, 0] * M.matriz[2, 1]
                               - M.matriz[0, 2] * M.matriz[1, 1] * M.matriz[2, 0]
                               - M.matriz[0, 0] * M.matriz[1, 2] * M.matriz[2, 2]
                               - M.matriz[0, 1] * M.matriz[1, 0] * M.matriz[2, 2];
                return (d);
            }
            else
                    if (M.qtLin == 1)
                return (M.matriz[0, 0]);
            else
                        if (M.qtLin > 3)
            {
                double d = 0.0;
                double sinal = 1.0;

                for (int col = 0; col < M.qtCol; col++)
                {
                    if ((col % 2) == 0)
                        sinal = 1.0;
                    else sinal = -1.0;
                    Matriz N = M.ReduzMatriz(0, col);
                    d += sinal * determinante(N);
                } // for col
                return (d);
            } // if M.qtLin>3
            return (0.0);
        } // double determinante()


        /// <summary>
        /// reduz a matriz que chamou o método,
        /// retirando a linha e coluna especificadas.
        /// </summary>
        /// <param name="linCorte">linha a ser retirada.</param>
        /// <param name="colCorte">coluna a ser retirada.</param>
        /// <returns></returns>
        private Matriz ReduzMatriz(int linCorte, int colCorte)
        {
            int lin, col;
            int linCount, colCount;
            Matriz M = new Matriz(this.qtLin - 1, this.qtCol - 1);
            linCount = 0;
            for (lin = 0; lin < this.qtLin; lin++)
            {
                colCount = 0;
                if (lin != linCorte)
                {
                    for (col = 0; col < this.qtCol; col++)
                        if (col != colCorte)
                        {
                            M.matriz[linCount, colCount] = this.matriz[lin, col];
                            colCount++;
                        } // if col!=colCorte
                    linCount++;
                } // if lin!=colCorte
            } // for lin
            return (M);
        } // ReduzMatriz()




        /// <summary>
        /// calcula o cofator da matriz A, e guarda na matriz temp.
        /// Function to get cofactor of A[p,q] in [,]temp. n is current 
        // dimension of A[,] 
        /// </summary>
        /// <param name="A">matriz A de entrada.</param>
        /// <param name="p">linha a ser excluida.</param>
        /// <param name="q">coluna a ser excluída.</param>
        /// <param name="n">dimensão de A[n,n].</param>
        private static Matriz GetCofactor(Matriz A, int p, int q)
        {
            int n = A.qtLin;
            int i = 0, j = 0;
            Matriz temp = new Matriz(n - 1, n - 1);
            // Looping for each element of the matrix 
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    // Copying into temporary matrix only those element 
                    // which are not in given row and column 
                    if (row != p && col != q)
                    {
                        temp.SetElement(i, j++, A.GetElement(row, col));

                        // Row is filled, so increase row index and 
                        // reset col index 
                        if (j == n - 1)
                        {
                            j = 0;
                            i++;
                        }
                    } // if row
                } // for col
            } // for row
            return temp;
        } // GetCofator()

        public static Matriz MatrixInverse(Matriz A)
        {
            double det = determinante(A);
            Matriz adj = Adjoint(A, A.qtLin);
            Matriz mtInverse = det * adj;
            return mtInverse;
        } // MatrixInverse()

        /// <summary>
        /// Function to get adjoint of A[N,N] in adj[N,N]. 
        /// </summary>
        /// <param name="A">matriz A de entrada.</param>
        /// <param name="N">dimensão da matriz A[n,n].</param>
        private static Matriz Adjoint(Matriz A, int N)
        {
            Matriz adj = new Matriz(N, N);
            if (N == 1)
            {
                adj.SetElement(0, 0,1);
                return adj;
            }// if N

            // temp is used to store cofactors of A [N,N].
            int sign = 1;
            Matriz temp = new Matriz(N-1, N-1);

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    // Get cofactor of A[i,j] 
                    temp = GetCofactor(A, i, j);

                    // sign of adj[j,i] positive if sum of row 
                    // and column indexes is even. 
                    sign = ((i + j) % 2 == 0) ? 1 : -1;
                    // Interchanging rows and columns to get the 
                    // transpose of the cofactor matrix 
                    adj.SetElement(j, i, (sign) * determinante(temp));
                }// fo j
            } // for i
            return adj;
        } // Adjoint()

        /// <summary>
        /// preenche a matriz com dados aleatorios. 
        /// </summary>
        /// <param name="intervalo">intervalo somada cumulativamente.</param>
        public void PreencheMatriz(double intervalo)
        {
            int lin, col;
            aleatorizador = new Random(Matriz.seedRandom);
            for (col = 0; col < this.qtCol; col++)
            {
                for (lin = 0; lin < this.qtLin; lin++)
                {
                    this.matriz[lin, col] = aleatorizador.NextDouble() * intervalo;
                } // for lin
            } // for col
        } // void preencheMatriz()

        /// <summary>
        /// transforma uma matriz para uma representação em String. Sobrescreve 
        /// o método ToString(), para que possa representar o objeto Matriz em
        /// operações de escrita em tela.
        /// </summary>
        /// <returns>retorna uma String representação da matriz.</returns>
        public override string ToString()
        {
            String s = "";
            for (int lin = 0; lin < this.qtLin; lin++)
            {
                s = s + "[";
                for (int col = 0; col < this.qtCol; col++)
                {
                    s = s + " [" + this.matriz[lin, col].ToString("N2") + "]";
                } // for int col
                s = s + "]";
            } // for int lin
            return (s);
        } // ToSTring()

        /// <summary>
        /// ordena a matriz se lin=1 ou col=1.
        /// </summary>
        public void Sort()
        {
            List<double> sortList = new List<double>();
            int lin, col;

            if (this.qtCol == 1)
            {

                for (lin = 0; lin < this.qtLin; lin++)
                {
                    sortList.Add(this.matriz[lin, 0]);
                } // for lin
                sortList.Sort();
                for (lin = 0; lin < this.qtLin; lin++)
                {
                    this.matriz[lin, 0] = sortList[lin];
                } // for lin

            } // if
            else
                if (this.qtLin == 1)
                {
                    for (col = 0; col < this.qtCol; col++)
                        sortList.Add(this.matriz[0, col]);
                    sortList.Sort();
                    for (col = 0; col < this.qtCol; col++)
                        this.matriz[0, col] = sortList[col];
                } // if 
        } // Sort().
    } // class Matriz
    //********************************************************************************************************************

    /// <summary>
    /// classe de vetores 2D.
    /// </summary>
    public class vetor2
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Color cor { get; set; }

        public static bool AssercoesVetor2D(vetor2 vAtual, vetor2 vEsperado, double delta)
        {
            if ((Math.Abs(vAtual.X - vEsperado.X) > delta) || (Math.Abs(vAtual.Y - vEsperado.Y) > delta))
               return false;
            return true;
        }


        public vetor2(double x, double y, Color _cor)
        {
            this.X = x;
            this.Y = y;
            this.cor = Color.FromArgb(_cor.A, _cor);

        }

        public vetor2(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.cor = Color.Black;
        }

        public vetor2(vetor2 vclone)
        {
            this.X = vclone.X;
            this.Y = vclone.Y;
            this.cor = Color.FromArgb(vclone.cor.A, vclone.cor);
        }
        public override string ToString()
        {
            string s = "X: " + this.X.ToString("N2") + "  Y: " + this.Y.ToString("N2");
            return s;
        }

        public static double Distancia(vetor2 v1, vetor2 v2)
        {
            double d = Math.Sqrt((v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y));
            return d;
        }// Distancia()

        public static vetor2 rotacionaVetor2(vetor2 v, double anguloRad)
        {
            double xn = v.X * Math.Cos(anguloRad) - v.Y * Math.Sin(anguloRad);
            double yn = v.X * Math.Sin(anguloRad) + v.Y * Math.Cos(anguloRad);
            Color cor = Color.FromArgb(v.cor.A, v.cor);
            return new vetor2(xn, yn, cor);
        }

        public void rotacionaVetorAnguloAbsoluto(double anguloRad)
        {
            double raio = Math.Sqrt(this.X * this.X + this.Y * this.Y);
            double X0 = raio * Math.Cos(anguloRad);
            double Y0 = raio * Math.Sin(anguloRad);
            this.X = X0;
            this.Y = Y0;
        }

        public void perspectivaIsometrica(vetor3 v)
        {
            this.X = v.X + v.Z / 2;
            this.Y = v.Y + v.Z / 2;
            this.cor = Color.FromArgb(v.cor.A, v.cor);
        }
        
        public void normaliza()
        {
            if ((this.X != 0.0) || (this.Y != 0.0))
            {
                double d = Math.Sqrt(this.X * this.X + this.Y * this.Y);
                this.X /= d;
                this.Y /= d;
            } // if
        }// normaliza()

        public static  vetor2 operator*(Matriz M, vetor2 v2)
        {
            Matriz mtv2 = new Matriz(2, 1);
            mtv2.SetElement(0, 0, v2.X);
            mtv2.SetElement(1, 0, v2.Y);

            Matriz mtResult = M * mtv2;
            vetor2 v2Saida = new vetor2(mtv2.GetElement(0, 0), mtv2.GetElement(1, 0));
            return v2Saida;
        }

        public static double operator *(vetor2 v1, vetor2 v2)
        {
            return (v1.X * v2.X+v1.Y * v2.Y);
        } // operator *()

        public static vetor2 operator +(vetor2 v1, vetor2 v2)
        {
            return new vetor2(v1.X + v2.X, v1.Y + v2.Y, Color.FromArgb(v1.cor.A, v1.cor));
        } // operator +()

        public static vetor2 operator -(vetor2 v1, vetor2 v2)
        {
            return new vetor2(v1.X - v2.X, v1.Y - v2.Y, Color.FromArgb(v1.cor.A, v1.cor));
        } // operator -()

        public static vetor2 operator *(double n, vetor2 v)
        {
            return new vetor2(n * v.X, n * v.Y, Color.FromArgb(v.cor.A, v.cor));
        } // operator *()


        public void Inteiro()
        {
            this.X = Math.Round(this.X);
            this.Y = Math.Round(this.Y);
        }// Inteiro()

    }
    //****************************************************************************************************************************
    /// <summary>
    /// classe de vetores 3D, conversíveis à matrizes [1,3].
    /// </summary>
    public class vetor3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Color cor { get; set; }


        /// <summary>
        /// inicializa um vetor 3D, preenchendo com 0.0 nas coordenadas.
        /// </summary>
        public vetor3()
        {
            this.X = 0.0;
            this.Y = 0.0;
            this.Z = 0.0;
        } // vetor3()
        
        /// <summary>
        /// inicializa um vetor 3D, com coordenadas dadas.
        /// </summary>
        /// <param name="x">parâmetro para a coordenada X.</param>
        /// <param name="y">parâmetro para a coordenada Y.</param>
        /// <param name="z">parâmetro para a coordenada Z.</param>
        public vetor3(double x, double y, double z)
        {

            this.X = x;
            this.Y = y;
            this.Z = z;
        } // vetor3()
        /// <summary>
        /// inicializa um vetor 3D, com coordenadas dadas e cor dada.
        /// </summary>
        /// <param name="x">parâmetro para a coordenada X.</param>
        /// <param name="y">parâmetro para a coordenada Y.</param>
        /// <param name="z">parâmetro para a coordenada Z.</param>
        /// <param name="c">parâmetro para a cor do vetor 3D.</param>
        public vetor3(double x, double y, double z, Color c)
        {

            this.X = x;
            this.Y = y;
            this.Z = z;
            this.cor = Color.FromArgb(c.A, c);
        } // vetor3()

        /// <summary>
        /// inicializa um vetor 3D, fazendo uma cópia do vetor 3D parâmetro.
        /// </summary>
        /// <param name="v">vetor 3D a ser copiado.</param>
        public vetor3(vetor3 v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
            this.cor = Color.FromArgb(v.cor.A, v.cor);
        } // vetor3()

        public vetor3 Clone()
        {
            vetor3 v = new vetor3(this);
            return v;
        } // Clone()


        public static vetor3 ProdutoVetorial(vetor3 v1, vetor3 v2)
        {
            double x, y, z;
            x = v1.Y * v2.Z - v2.Y * v1.Z;
            y = (v1.X * v2.Z - v2.X * v1.Z) * -1;
            z = v1.X * v2.Y - v2.X * v1.Y;

            var rtnvector = new vetor3(x, y, z);
            rtnvector.normalizacao(); //optional
            return rtnvector;
        }// CrossProduct()


        public vetor3 RotacionaVetor(double anguloXY, double anguloXZ, double anguloYZ)
        {  
            anguloXY = Angulos.toRadianos(anguloXY);
            anguloXZ = Angulos.toRadianos(anguloXZ);
            anguloYZ = Angulos.toRadianos(anguloYZ);
            Matriz Rx = new Matriz(3, 3, 1, 0, 0, 0, Math.Cos(anguloYZ), -Math.Sin(anguloYZ), 0, Math.Sin(anguloYZ), Math.Cos(anguloYZ));
            Matriz Ry = new Matriz(3, 3, Math.Cos(anguloXZ), 0, Math.Sin(anguloXZ), 0, 1, 0, -Math.Sin(anguloXZ), 0, Math.Cos(anguloXZ));
            Matriz Rz = new Matriz(3, 3, Math.Cos(anguloXY), -Math.Sin(anguloXY), 0, Math.Sin(anguloXY), Math.Cos(anguloXY), 0, 0, 0, 1);
            Matriz R = Rz * Ry * Rx;
            Matriz elemento = new Matriz(1, 3, X, Y, Z);
            Matriz Result = elemento * R;
            vetor3 vtResult = new vetor3(Result.GetElement(0, 0), Result.GetElement(0, 1), Result.GetElement(0, 2), this.cor);
            return vtResult;
        }
        /// <summary>
        /// retorna o módulo do vetor3 currente.
        /// </summary>
        /// <returns></returns>
        private static double modulo(vetor3 v)
        {
            return (Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z));
        } // modulo()

        public void normalizacao()
        {
            if (modulo(this) == 0)
                return;
            double mod = modulo(this);
            this.X /= mod;
            this.Y /= mod;
            this.Z /= mod;
        } // normalizacao()

        public vetor3 normaliza()
        {
            if (modulo(this) == 0)
                return null;
            vetor3 vtSaida = new vetor3(this);
            double mod = modulo(this);
            vtSaida.X /= mod;
            vtSaida.Y /= mod;
            vtSaida.Z /= mod;
            return vtSaida;
        }
        public double Distancia()
        {

            double d = Math.Sqrt(X * X + Y * Y + Z * Z);
            return d;
        }// Distancia()

        public double Distancia(vetor3 v)
        {
            double d = Math.Sqrt(
                (X - v.X) * (X - v.X)
              + (Y - v.Y) * (Y - v.Y)
              + (Z - v.Z) * (Z - v.Z));
            return d;

        }// Distancia()
        
        public double Modulo()
        {
            return this.Distancia();
        }

        /// <summary>
        /// operação de adição entre dois vetores 3D.
        /// </summary>
        /// <param name="v1">vetor 3D parâmetro.</param>
        /// <param name="v2">vetor 3D parâmetro.</param>
        /// <returns></returns>
        public static vetor3 operator +(vetor3 v1, vetor3 v2)
        {
            return new vetor3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, Color.FromArgb(v1.cor.A, v1.cor));
        } // operator +()
        
        /// <summary>
        /// operação de subtração entre dois vetores 3D.
        /// </summary>
        /// <param name="v1">vetor 3D parâmetro.</param>
        /// <param name="v2">vetor 3D parâmetro.</param>
        /// <returns></returns>
        public static vetor3 operator -(vetor3 v1, vetor3 v2)
        {
            return new vetor3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, Color.FromArgb(v1.cor.A, v1.cor));
        } // operator -()

        

        public static vetor3 operator *(vetor3 v, Matriz m)
        {
            Matriz Aux = new Matriz(1, 3, v.X, v.Y, v.Z);
            Matriz mAux = Aux * m;
            vetor3 vr = new vetor3(mAux.GetElement(0, 0), mAux.GetElement(0, 1), mAux.GetElement(0, 2), Color.FromArgb(v.cor.A, v.cor));
            return vr;
        } // operator * (v,m)

        /// <summary>
        /// calcula o produto escalar entre dois vetores.
        /// </summary>
        /// <param name="v1">vetor 1 a gerar o produto escalar.</param>
        /// <param name="v2">vetor 2 a gerar o produto escalar.</param>
        /// <returns>retorna o produto escalar, calculado pela
        ///  multiplicação entre as respectivas coordenadas (x,y,z).</returns>
        public static double operator *(vetor3 v1, vetor3 v2)
        {
            return (v1.X * v2.X+ v1.Y * v2.Y+ v1.Z * v2.Z);
        } // operator *()

        
        /// <summary>
        /// calcula o produto entre um número e um vetor 3D.
        /// </summary>
        /// <param name="n">número a ser multiplicado.</param>
        /// <param name="v1">vetor 3D a ser multiplicado.</param>
        /// <returns></returns>
        public static vetor3 operator *(double n, vetor3 v1)
        {
            return new vetor3(n * v1.X, n * v1.Y, n * v1.Z, Color.FromArgb(v1.cor.A, v1.cor));
        } // operator *()

        /// <summary>
        /// calcula o produto vetorial de dois vetores dados.
        /// </summary>
        /// <param name="v1">vetor 1 a gerar o produto vetorial.</param>
        /// <param name="v2">vetor 2 a gerar o produto vetorial.</param>
        /// <returns>retorna o produto vetorial, determinada pelo determinante
        /// dos dois vetores, mais as cordenadas literais (x,y,z).</returns>
        public static vetor3 operator &(vetor3 v1, vetor3 v2)
        {
            double[] c = new double[3];
            double[] a = new double[] { v1.X, v1.Y, v1.Z };
            double[] b = new double[] { v2.X, v2.Y, v2.Z };

            double i = a[1] * b[2] - a[2] * b[1];
            double j = a[2] * b[0] - a[0] * b[2];
            double k = a[0] * b[1] - a[1] * b[0];
            vetor3 vr = new vetor3(i, j, k, Color.FromArgb(v1.cor.A, v1.cor));
            return (vr);

        } // produtoVetorial()

        /// <summary>
        /// multiplica o vetor currente por uma base das pelos eixos vetores de entrada.
        /// </summary>
        /// <param name="i">eixo i.</param>
        /// <param name="j">eixo j.</param>
        /// <param name="k">eixo k.</param>
        public void multiplicaPorUmaBase(vetor3 i, vetor3 j, vetor3 k)
        {
            vetor3 v = new vetor3(this);
            this.X = v.X * i.X + v.Y * j.X + v.Z * k.X;
            this.Y = v.X * i.Y + v.Y * j.Y + v.Z * k.Y;
            this.Z = v.X * i.Z + v.Y * j.Z + v.Z * k.Z;
        } // multiplicaPorUmaBase()
        public override string ToString()
        {
            return ("(" + X.ToString("N2") + " , " + Y.ToString("N2") + " , " + Z.ToString("N2") + ")");
        } // ToString()

    } // class vetor3

    public class CoordenadasEsfericas
    {
        double omega;
        double fi;
        double radious;

        public CoordenadasEsfericas(double radious, double omega, double fi)
        {
            this.omega = omega;
            this.fi = fi;
            this.radious = radious;
        }

        public CoordenadasEsfericas()
        {

        }


        private double CalcRadious(vetor3 v3)
        {
            double radious = Math.Sqrt(v3.X * v3.X + v3.Y * v3.Y + v3.Z * v3.Z);
            return radious;
        }


        private double CalcOmega(vetor3 v3)
        {
            return Math.Acos(v3.Z /CalcRadious(v3));
        }
        private double CalcFi(vetor3 v3)
        {
            return Math.Acos(v3.Z / CalcRadious(v3));
        }

        private CoordenadasEsfericas ConvertParaEsferica(vetor3 v3)
        {
            double fi = CalcFi(v3);
            double omega = CalcOmega(v3);
            double radious = CalcRadious(v3);
            return new CoordenadasEsfericas(radious, omega, fi);
        }

        /// <summary>
        /// rotaciona em graus os ângulos do vetor, em coordenadas esféricas.
        /// </summary>
        /// <param name="v3">vetor 3D a ser rotacionado.</param>
        /// <param name="omega">ângulo de rotação do eixo Z.</param>
        /// <param name="fi">ânulo de rotação do plano XZ.</param>
        /// <returns></returns>
        public vetor3 RotateEsferica(vetor3 v3, double omega, double fi)
        {
            omega = Angulos.toRadianos(omega);
            fi = Angulos.toRadianos(fi);

            CoordenadasEsfericas c = ConvertParaEsferica(v3);
            double x = c.radious * Math.Sin(omega + c.omega) * Math.Cos(fi + c.fi);
            double y = c.radious * Math.Sin(omega + c.omega) * Math.Sin(fi + c.fi);
            double z = c.radious * Math.Cos(omega + c.omega);
            return new vetor3(x, y, z);
        } // RotacionaPorCoordenadasEsfericas()
    }
    public class Angulos
    {
        /// <summary>
        /// converte um ângulo em graus para um ângulo em radianos.
        /// </summary>
        /// <param name="anguloEmGraus">ângulo em graus a ser convertido.</param>
        /// <returns>retorna um ângulo em radianos.</returns>
        public static double toRadianos(double anguloEmGraus)
        {
            return (anguloEmGraus * Math.PI / 180.0F);
        }

        /// <summary>
        /// converte um ângulo em radianos para um ângulo em graus.
        /// </summary>
        /// <param name="anguloEmRadianos">ângulo em graus a ser convertido.</param>
        /// <returns>retorna um ângulo em graus.</returns>
        public static double toGraus(double anguloEmRadianos)
        {
            return (anguloEmRadianos * (180.0F / Math.PI));
        }

        /// <summary>
        /// rotaciona um vetor 2D em um angulo determinado de incremento.
        /// </summary>
        /// <param name="anguloEmGraus">angulo em graus de incremento, utilizado na rotacao.</param>
        /// <param name="v">vetor 2D a ser rotacionado.</param>
        /// <returns>retorna um vetor2 rotacionado em um angulo de [anguloEmGraus].</returns>
        public static vetor2 rotacionaVetor(double anguloEmGraus, vetor2 v)
        {
            vetor2 vf = new vetor2(0.0F, 0.0F);
            double angle = toRadianos(anguloEmGraus);
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);
            vf.X = v.X * cosAngle - v.Y * sinAngle;
            vf.Y = v.Y * cosAngle + v.X * sinAngle;
            return (vf);
        } // rotacionaVetor()

        /// <summary>
        /// rotaciona uma matriz [1,2] em [anguloEmGraus] graus.
        /// </summary>
        /// <param name="anguloEmGraus">ângulo em graus para a rotação.</param>
        /// <param name="mtv">matriz a ser rotacionada.</param>
        /// <returns>retorna uma matriz rotacionada pelo ângulo parâmetro.</returns>
        public static Matriz rotacionaVetor(double anguloEmGraus, Matriz mtv)
        {
            vetor2 vInicial = new vetor2(mtv.GetElement(0, 0), (double)mtv.GetElement(0, 1));
            vetor2 vFinal = rotacionaVetor(anguloEmGraus, vInicial);
            Matriz mtFinal = new Matriz(1, 2);
            mtFinal.SetElement(0, 0, vFinal.X);
            mtFinal.SetElement(0, 1, vFinal.Y);
            return (mtFinal);
        }
        /// <summary>
        /// muda a direção do vetor 2D para um determinado [anguloEmGraus].
        /// </summary>
        /// <param name="anguloEmGraus">novo ângulo-direção do vetor 2D de entrada.</param>
        /// <param name="v">estrutura representando o vetor 2D.</param>
        /// <returns></returns>
        public static vetor2 rotacionaVetorComAnguloAbsoluto(double anguloEmGraus, vetor2 v)
        {
            vetor2 vf = new vetor2(0.0, 0.0);
            double angle = toRadianos(anguloEmGraus);
            double raio = Math.Sqrt(v.X * v.X + v.Y * v.Y);
            vf.X = raio * Math.Cos(angle);
            vf.Y = raio * Math.Sin(angle);
            return vf;
        } // rotacionaVetorComAnguloAbsoluto()

    } // class Angulos

    /// <summary>
    /// classe que faz a transformação de um vetor para outro, utiizando uma matriz de rotação.
    /// </summary>
    public class UniMatrix
    {

        /// <summary>
        /// matriz da transformação de um vetor para outro.
        /// </summary>
        private Matrix<double> mtMain;

        private Vector<double> vectorFrom;
        private Vector<double> vectorTo;

        public void SetMatrix(Matrix<double> matriz)
        {
            matriz.CopyTo(this.mtMain);
        } // SetMatrix()

        public Matrix<double> GetMatriz()
        {
            return mtMain;
        } // GetMatriz()

        /// <summary>
        /// constroi a matriz de transformação de transformar um vetor para outro vetor.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="vetorBaseFrom">vetor Base From.</param>
        /// <param name="vetorBaseTo">vetor Base To.</param>
        public UniMatrix(Vector3D vetorBaseFrom, Vector3D vetorBaseTo)
        {
            this.mtMain = Matrix3D.RotationTo(vetorBaseFrom, vetorBaseTo);
            this.vectorFrom = vetorBaseFrom.ToVector();
            this.vectorTo = vetorBaseFrom.ToVector();

        } // UniMatrizTransformacao()
        
        /// <summary>
        /// Transforma um vetor para outro vetor, utilizando uma matriz de transformação.
        /// </summary>
        /// <param name="vtIn">vetor de entrada.</param>
        /// <param name="arredonda">se true, aplica Round() [arredondar] nas coordenadas do vetor de saída.</param>
        /// <returns>retorna o vetor de saída, que é o vetor de entrada multiplicado pela matriz de transformação-rotação.</returns>
        public Vector3D TransformVectorToOtherVector(Vector3D vtIn, bool arredonda)
        {
            // converte o vetor de entrada para um vetor de multiplicação de matrizes M<double>.
            Vector<double> vetorEntrada = vtIn.ToVector();
            // realiza a multiplicação do vetor de Entrada pela matriz de transformção.
            Vector<double> vtOut = vetorEntrada * this.mtMain;
            // converte o vetor de saída para um vetor3D, de utilização menos complexa.
            Vector3D vt3D = new Vector3D(vtOut[0], vtOut[1], vtOut[2]);
            if (arredonda)
            {
                double x = Math.Round(vt3D.X);
                double y = Math.Round(vt3D.Y);
                double z = Math.Round(vt3D.Z);
                return new Vector3D(x, y, z);
            } // if
            // retorna o vetor3D calculado.
            return vt3D;
        } // TransformVector()

        /// <summary>
        /// converte um Vector3D to vetor3
        /// </summary>
        /// <param name="vt">vetor a ser convertido.</param>
        /// <returns>retorna o vetor 3 associado.</returns>
        public vetor3 ToVetor3D(Vector3D vt)
        {
            return new vetor3(vt.X, vt.Y, vt.Z);
        } //ToVetor3()

        public static Vector3D Vector3D(vetor3 vt)
        {
            return new Vector3D(vt.X, vt.Y, vt.Z);
        }
    } // class
}// namespace MatrizLibrary
