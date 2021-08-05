using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{

    public class metodo: propriedade
    {
        public string nomeMetodo { get; set; }
        public List<propriedade> parametrosMetodo { get; set; }
        public propriedade retornoMetodo { get; set; }
        public metodo()
        {
            this.parametrosMetodo = new List<propriedade>();

        }
        public metodo(string nome, propriedade[] parametrosMetodo, propriedade retorno)
        {
            this.nomeMetodo = nome;
            this.parametrosMetodo = new List<propriedade>();
            for (int i = 0; i < parametrosMetodo.Length; i++)
                this.parametrosMetodo.Add(parametrosMetodo[i]);
            this.retornoMetodo = retorno;
        }

        public string getNomeMetodo()
        {
            return (this.nomeMetodo);

        }

        public void setMetodo(string funcao)
        {
            this.nomeMetodo = funcao;
        }
        public string getRetornoMetodo(string nmMetodo)
        {
            if (this.nomeMetodo.Equals(nmMetodo))
                return (this.retornoMetodo.nomepropriedade);
            throw new Exception("Método não identificado: " + nmMetodo);
        }
    }




    public class operador : metodo
    {

        public int prioridade;
        private Random aleatorizador = new Random(1000);

        public operador(string nome, int prioridade, string[] tiposParametros)
        {

            this.nomeMetodo = nome;
            this.prioridade = prioridade;
            foreach(string tipoDeUmParametro in tiposParametros)
            {
                this.parametrosMetodo.Add(new propriedade(nome, tipoDeUmParametro, "null")); 
            }
        }

        public int getPrioridade()
        {
            return (this.prioridade);
        }

        public void setPrioridade(int novaprioridade)
        {
            this.prioridade = novaprioridade;
        }

        public string geraNomeAleatorio()
        {
        
            char[] vetLetters= new char[]{'A','B','C','D','E','F','G','H','I','J','L','M',
                'N','O','P','Q','R','S','T','U','V','X','Z','0',
                '1','2','3','4','5','6','7','8','9'};
            string nome="";
            for (int i = 0; i < 8; i++)
                nome += vetLetters[aleatorizador.Next(33)];
            return (nome);
        
        
        }


    }

    public class metodoOperador
    {
        public delegate T metodoOp<T>(T param1, T param2);
    }
}
