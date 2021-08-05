using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

namespace parser
{


    
    /// <summary>
    /// esta classe é para o termo cunhado na teoria dos automatos: a producao de uma linguagem
    /// que é onde se define o caminho na máquina de estados onde o automato irá percorrer, bem
    /// como define os termos da linguagem.
    /// </summary>
    public class producao
    {
        public string maquinaDeEstados { get; set; }
        public List<string> semiproducoes { get; set; }
        public List<string> termosChave { get; set; }
        public string nomeProducao { get; set; }
        public string tipo { get; set; }
        public int linhaPrograma { get; set; }
        public string trechoPrograma { get; set; }
        public string str_termoschave { get; set; }



        public producao(string nomeProd, string tipoProd, string maqDeEstados, string termoschave)
        {
            this.nomeProducao = nomeProd.ToString();
            this.tipo = tipoProd.ToString();
            this.str_termoschave = termoschave;
            //_______________________________________________________________________________________________
            // localiza os termos chave
            string[] trmschv = termoschave.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            this.termosChave = new List<string>();
            int x;
            for (x = 0; x < trmschv.Length; x++)
            {
                // o termo - chave [espaco] é essencial para identificar espaçamento entre termos que não tem termos-chave,
                // apenas dois ou mais [ID] e/ou [TYPE_ID], por exemplo.
                if (trmschv[x].Equals("[espaco]"))
                    this.termosChave.Add(" ");
                else
                    this.termosChave.Add(trmschv[x]);

            } // for x
            //____________________________________________________________________________________________________
            // constroi as semiproducoes, numa lista própria.
            string[] semiprod = maqDeEstados.Split(this.termosChave.ToArray(),
                                                 StringSplitOptions.RemoveEmptyEntries);
            semiprod = this.retiraStringsVazias(semiprod);
            this.semiproducoes = new List<string>();
            foreach (string semiproducao in semiprod)
            {
                if (semiprod.Equals("[espaco]"))
                    this.semiproducoes.Add(" ");
                else
                    if (semiproducao != null)
                        this.semiproducoes.Add(semiproducao);
            }
            //__________________________________________________________________________________________________
            // monta a maquina de estados
            this.maquinaDeEstados = (string)maqDeEstados.Clone();

        } // public producao

        private string[] retiraStringsVazias(string[] str)
        {
            List<string> s = new List<string>();
            for (int x=0; x<str.Length;x++)
            {
                str[x]=str[x].Trim();
                if (str[x].Equals("") == false)
                    s.Add(str[x]);
            } // foreach
            return (s.ToArray());
        } // retiraStringsVazias()

        public static string juntaTermosChave(string[] trmv)
        {
            string ss = "";
            foreach (string s in trmv)
                ss += s;
            return (ss);
        }// juntaTermosChave()
        
    } // class producao

 
} //namespace parser
