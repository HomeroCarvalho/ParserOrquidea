using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    /// <summary>
    /// implementa uma linguage chamada orquidea
    /// </summary>
    public class linguagemOrquidea : UmaGramaticaComputacional
    {

        public linguagemOrquidea()
        {
           // coloque aqui qualquer dado necessário no inicio da linguagem 
           // específica da orquidea.
            this.todosTermosChaveDaLinguagem = base.getTodosTermosChave();
        } // linguagemOrquidea()

        protected override void inicializaPropriedadesOperadoresNativosParaLinguagem()
        {
            this.operadores = new List<operador>();
            List<operador> op_int=new List<operador>();
            op_int.Add(new operador("+", 1, new string[] { "int", "int" }));
            op_int.Add(new operador("-", 1, new string[] { "int", "int" }));
            op_int.Add(new operador("*", 2, new string[] { "int", "int" }));
            op_int.Add(new operador("/", 2, new string[] { "int", "int" }));
            op_int.Add(new operador("%", 2, new string[] { "int", "int" }));
            op_int.Add(new operador("^", 3, new string[] { "int", "int" }));
            op_int.Add(new operador("(", 9, new string[] { "int", "int" }));
            op_int.Add(new operador(")", 9, new string[] { "int", "int" }));
            op_int.Add(new operador(",", 8, new string[] { "int", "int" }));
            op_int.Add(new operador(">", 0, new string[] { "int", "int" }));
            op_int.Add(new operador("<", 0, new string[] { "int", "int" }));
            op_int.Add(new operador(">=", 0, new string[] { "int", "int" }));
            op_int.Add(new operador("<=", 0, new string[] { "int", "int" }));
            op_int.Add(new operador("==", 0, new string[] { "int", "int" }));
            op_int.Add(new operador("=", 0, new string[] { "int", "int" }));
            foreach (operador op in op_int)
                this.operadores.Add(op);

            List<operador> op_float = new List<operador>();
            op_float.Add(new operador("+", 1, new string[] { "float", "float" }));
            op_float.Add(new operador("-", 1, new string[] { "float", "float" }));
            op_float.Add(new operador("*", 2, new string[] { "float", "float" }));
            op_float.Add(new operador("/", 2, new string[] { "float", "float" }));
            op_float.Add(new operador("^", 3, new string[] { "float", "float" }));
            op_float.Add(new operador("(", 9, new string[] { "float", "float" }));
            op_float.Add(new operador(")", 9, new string[] { "float", "float" }));
            op_float.Add(new operador(",", 8, new string[] { "float", "float" }));
            op_float.Add(new operador(">", 0, new string[] { "float", "float" }));
            op_float.Add(new operador("<", 0, new string[] { "float", "float" }));
            op_float.Add(new operador(">=", 0, new string[] { "float", "float" }));
            op_float.Add(new operador("<=", 0, new string[] { "float", "float" }));
            op_float.Add(new operador("==", 0, new string[] { "float", "float" }));
            op_float.Add(new operador("=", 0, new string[] { "float", "float" }));

            // operadores de casting float-int, para operações aritmética,
            // assim impede-se conversões em just-in-time.
            op_float.Add(new operador("+", 1, new string[] { "float", "int" }));
            op_float.Add(new operador("-", 1, new string[] { "float", "int" }));
            op_float.Add(new operador("*", 2, new string[] { "float", "int" }));
            op_float.Add(new operador("/", 2, new string[] { "float", "int" }));
            op_float.Add(new operador("^", 3, new string[] { "float", "int" }));
            // operador de casting para atribuição float-int.
            op_float.Add(new operador("=", 0, new string[] { "float", "int" }));

            // operadores de casting int-float para operações de aritmética.
            op_float.Add(new operador("+", 1, new string[] { "int", "float" }));
            op_float.Add(new operador("-", 1, new string[] { "int", "float" }));
            op_float.Add(new operador("*", 2, new string[] { "int", "float" }));
            op_float.Add(new operador("/", 2, new string[] { "int", "float" }));
            op_float.Add(new operador("^", 3, new string[] { "int", "float" }));
            // operador de casting para atribuição int-float.
            op_float.Add(new operador("=", 0, new string[] { "int", "float" }));

            foreach(operador op in op_float)
                this.operadores.Add(op);

            List<operador> op_string = new List<operador>();
            op_string.Add(new operador("+", 0, new string[] { "string", "string" }));
            op_string.Add(new operador("==", 0, new string[] { "string", "string" }));
            op_string.Add(new operador("=", 9, new string[] { "string", "string" }));
            op_string.Add(new operador(",", 8, new string[] { "string", "string" }));
            foreach (operador op in op_string)
                this.operadores.Add(op);


            List<operador> op_bool = new List<operador>();
            op_bool.Add(new operador("==", 0, new string[] { "bool,bool" }));
            op_bool.Add(new operador("=", 9, new string[] { "bool, bool" }));
            op_bool.Add(new operador("!=", 0, new string[] { "bool", "bool" }));
            op_bool.Add(new operador(",", 8, new string[] { "bool", "bool" }));
            foreach (operador op in op_bool)
                this.operadores.Add(op);
           

            List<operador> op_char = new List<operador>();
            op_char.Add(new operador("=", 9, new string[] { "char", "char" }));
            op_char.Add(new operador("==", 0, new string[] { "char", "char" }));
            op_char.Add(new operador("!=", 0, new string[] { "char", "char" }));
            op_char.Add(new operador(",", 8, new string[] { "char", "char" }));
            foreach (operador op in op_char)
                this.operadores.Add(op);

            this.adicionaClasse(new classe("int", op_int.ToArray()));
            this.adicionaClasse(new classe("float", op_float.ToArray()));
            this.adicionaClasse(new classe("string", op_string.ToArray()));
            this.adicionaClasse(new classe("bool", op_bool.ToArray()));
            this.adicionaClasse(new classe("char", op_char.ToArray()));

        }

        public void adicionaClasse(classe nwclass)
        {
            this.classes.Add(nwclass);
        } // adicionaClasse(classe)

        protected override void inicializaProducoesDaLinguagem()
        {
            xmlREADER_LINGUAGEM xmlreader = new xmlREADER_LINGUAGEM();
            xmlreader.LE_ARQUIVO_LINGUAGEM(ref this.producoes, "ORQUIDIA");
        } // inicializaProducoesParaLinguagem()

       
        //_________________________________________________________________________
        //  MÉTODOS PARA MANUSEIO DE OPERADORES
        /// <summary>
        /// adiciona um novo operador para a List de metodos no "core"
        /// da linguagem, fazendo entao parte da linguagem.
        /// </summary>
        /// <param name="newoperador">nome do operador a ser adicionado</param>
        public new void adicionaOperador(operador newoperador)
        {
            this.operadores.Add(newoperador);
            this.strOperadores.Add(newoperador.nomeMetodo);
        } // void adicionaOperador()

        /// <summary>
        /// obtém o nome de todos operadores registrados na linguagem currente.
        /// </summary>
        /// <returns>o nome de todos operadores da linguagem.</returns>
        public string[] getNomeTodosOperadores()
        {
            return (this.strOperadores.ToArray<string>());
        } // getNomeTodosOperadores()
        
        /// <summary>
        /// obtém o propriedade [operador] para um dado nome.
        /// </summary>
        /// <param name="nomeOperador">nome do operador a ser pesquisado.</param>
        /// <returns>o propriedade [operador] para o nome parâmetro.</returns>
        public operador[] getOperadorPeloNome(string nomeOperador)
        {
            if (nomeOperador == null)
                return null;
            for (int x = 0; x < this.operadores.Count; x++)
                if (this.operadores[x].getNomeMetodo().Equals(nomeOperador))
                    return (new operador[] { this.operadores[x] });
            // os operadores podem estar juntos, grudados..
            // tenta separar os operadores.
            if (nomeOperador.Length == 2)
            {
                operador[] op = new operador[2];
                op[0] = getOperadorPeloNome(new string(nomeOperador[0], 1))[0];
                op[1] = getOperadorPeloNome(new string(nomeOperador[1], 1))[0];
                return (op);

            } //if
            if (nomeOperador.Length == 3)
            {
                operador[] op3 = new operador[2];
                string s= new string(nomeOperador[0],1)+
                          new string(nomeOperador[1],1);
                op3[0]= getOperadorPeloNome(s)[0];
                if (op3[0]== null)
                {
                    string s1= new string(nomeOperador[0],1);
                    op3[0]=getOperadorPeloNome(s1)[0];
                    s1= new string(nomeOperador[1],1)+ new string(nomeOperador[2],1);
                    op3[1]= getOperadorPeloNome(s1)[0];
                    return (op3);
                } // if op3
            } // if nomeOperador.Length
            if (nomeOperador.Length==4)
            {
                operador[] op4= new operador[2];
                string s1 = new string(nomeOperador[0], 1) +
                          new string(nomeOperador[1], 1);
                string s2 = new string(nomeOperador[2], 1) +
                          new string(nomeOperador[3], 1);
                op4[0] = getOperadorPeloNome(s1)[0];
                op4[1] = getOperadorPeloNome(s2)[0];
                return (op4);
            }
            return null;
        } // getOperadorPeloNome()

        /// <summary>
        /// obtém a prioridade (um número inteiro para comparação com outros operadores)
        /// do operador parâmetro.
        /// </summary>
        /// <param name="nomeOperador">nome do operador a ser pesquisado.</param>
        /// <returns></returns>
        public int getPrioridadeOperador(string nomeOperador)
        {
            if (nomeOperador == null)
                return -1;
            else
                return (this.getOperadorPeloNome(nomeOperador)[0].getPrioridade());
        }

        //_______________________________________________________________________

    } // class linguagemOrquidea


} // namespace parser
