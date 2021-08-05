using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    public class classe
    {
        public string nome { get; set; }
        public List<metodo> metodos { get; set; }
        public classe(string name, metodo[] methods)
        {
            this.metodos = new List<metodo>();
            this.nome = name;
            for (int i = 0; i < methods.Length; i++)
                this.metodos.Add(methods[i]);
        }
    }
    
}
