using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    public class propriedade
    {
        /// <summary>
        /// acessor desta variavel (public, private, protected).
        /// </summary>
        public string acessor { get; set; }
        /// <summary>
        /// nome desta propriedade.
        /// </summary>
        private string nome { get; set; }

        /// <summary>
        /// nome da classe a qual pertence esta propriedade.
        /// </summary>
        public string nomeDaClasse { get; set; }
        public string nomeLongo
        {
            get
            {
                if (nomeDaClasse == null)
                    return nome;
                else
                    return nomeDaClasse + "." + nome;
            }
        }

        public void SetNomeLongo()
        {
            this.nome = this.nomeDaClasse + "." + this.nome;
        }

        public string GetNome()
        {
            return this.nome;
        }

        public string tipo { get; set; }
        public object valor { get; set; }

        public bool isStatic = false;
      
    
        public propriedade(string nome, string tipdata, object val, bool isEstatica)
        {
            this.nome = nome;
            this.valor = val;
            this.tipo = tipdata;
            this.isStatic = isEstatica;
        }

        public propriedade(string acessor, string nome, string tipdata, object val, bool isEstatica)
        {
            this.nome = nome;
            this.valor = val;
            this.tipo = tipdata;
            this.acessor = acessor;
            this.isStatic = isEstatica;
        }


        public override string ToString()
        {
            string str = "";
            if (this.acessor != null)
                str += this.acessor + " ";

            if (this.tipo != null)
                str += this.tipo + " ";

            if (this.nome != null)
                str += this.nome;
            return str;
        } // ToString()

    } // class propriedade
} // namespace parser
