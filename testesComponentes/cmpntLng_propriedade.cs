using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    public class propriedade
    {
        public static Dictionary<string, propriedade> tkpropriedade = new Dictionary<string, propriedade>();
        public static Dictionary<propriedade, string> tkrspropriedade = new Dictionary<propriedade, string>();
        public string nomepropriedade;
        public string tipodados;
        public string valorArmazenado;
        public List<string> metodos = new List<string>();

        public propriedade()
        {
           
        } // propriedade()

        public propriedade(string nome, string tipdata, string val)
        {
            this.nomepropriedade = nome;
            this.valorArmazenado = val;
            this.tipodados = tipdata;
            propriedade.tkpropriedade[this.nomepropriedade] = this;
            propriedade.tkrspropriedade[this] = this.nomepropriedade;
        }
        public static propriedade getpropriedade(string nome)
        {
            try
            {
                return (propriedade.tkpropriedade[nome]);

            }
            catch
            {
                return null;

            } // catch

        } // getpropriedade()
    } // classe propriedade

} // namespace parser
