using System;
using System.Collections.Generic;
using System.Linq;

namespace parser.PROLOG
{
    public class Regra : Predicado
    {
        public List<Predicado> PredicadosGoal { get; set; }
        //private List<Lista> PredicadoListas { get; set; }
        public Predicado PredicadoBase { get; set; }
      

        public Regra(string nomeRegra, Predicado predicadoBase, List<Predicado> predicadosDaRegra) : base()
        {
            this.Nome = nomeRegra;
            this.PredicadoBase = predicadoBase;
            this.PredicadosGoal = new List<Predicado>();
            this.PredicadosGoal.AddRange(predicadosDaRegra);
        } // Regra()

        public Regra()
        {
            this.Nome = "";
            this.PredicadoBase = null;
            this.PredicadosGoal = new List<Predicado>();
        } // Regra()
        public Regra(Regra r)
        {
            this.Nome = r.Nome;
            this.PredicadoBase = r.PredicadoBase;
            this.PredicadosGoal = r.PredicadosGoal.ToList<Predicado>();
        } // Regra()


        public  new Regra Clone()
        {
            Regra regraClonada = new Regra();
            regraClonada.Nome = this.Nome;
            regraClonada.PredicadoBase = (Predicado)this.PredicadoBase.Clone();
            regraClonada.PredicadosGoal = new List<Predicado>();
            if (this.PredicadosGoal != null)
                for (int x = 0; x < this.PredicadosGoal.Count; x++)
                    regraClonada.PredicadosGoal.Add((Predicado)this.PredicadosGoal[x].Clone());
            return regraClonada;
        }
        
        public new List<string> GetAtomos()
        {
            List<string> atomos = new List<string>();
            for (int umAtomo = 0; umAtomo < this.PredicadoBase.GetAtomos().Count; umAtomo++)
                atomos.Add(this.PredicadoBase.GetAtomos()[umAtomo]);
            
            for (int id_predicadoGoal = 0; id_predicadoGoal < this.PredicadosGoal.Count; id_predicadoGoal++)
                for (int umAtomo = 0; umAtomo < this.PredicadosGoal[id_predicadoGoal].GetAtomos().Count; umAtomo++)
                    atomos.Add(this.PredicadosGoal[id_predicadoGoal].GetAtomos()[umAtomo]);

            return atomos;
        }
        public bool Match(Regra umaRegra)
        {
            for (int x = 0; x < this.PredicadosGoal.Count; x++)
                if (!this.Match(this.PredicadosGoal[x]))
                    return false;
            return true;
        }// Match()


  
        // obtém uma regra a partir de um texto de um editor de texto.
        public static Regra GetRegra(string texto)
        {
            List<Predicado> predicadosDaRegra = ParserPROLOG.GetPredicados(texto);
            if (predicadosDaRegra.Count == 0)
                return new Regra();
            if (predicadosDaRegra.Count == 1)
                return new Regra(predicadosDaRegra[0].Nome, predicadosDaRegra[0], new List<Predicado>());
            if (predicadosDaRegra.Count > 1)
                return new Regra(predicadosDaRegra[0].Nome, predicadosDaRegra[0], predicadosDaRegra.GetRange(1, predicadosDaRegra.Count - 1));

            return new Regra();
        } // GetRegra()

        /*
         *         try
             {
                 List<string> listaPredicadosBase = new List<string>();
                 // texto de um predicado.
                 string textoPredicado = "";

                 // extrai o predicado inicial da regra. Da varíavel texto é extraída o texto do predicado.
                 EditorTextoSuporteAProlog.ExtraiCaracteresDeUmPredicado(ref texto, ref textoPredicado);

                 // calcula um predicado a partir do texto conseguido na extração de caracteres.
                 Predicado predicadoBaseDaRegra = GetPredicado(textoPredicado);
                 if (ListaProlog.HasList(textoPredicado))
                     predicadoBaseDaRegra = new ListaProlog(textoPredicado);
                 else
                     predicadoBaseDaRegra = GetPredicado(textoPredicado);

                 List<Predicado> predicadosMetas = new List<Predicado>();
                 List<string> especificacoesMetasListas = new List<string>();
                 // retira do texto da especificação o operador de cláusulas.
                 texto = texto.Replace(":-", "").TrimStart(' ').TrimEnd(' ');
                 // extrai os predicados metas da regra.
                 while ((texto != null) && (texto.Length > 0) && (texto != ""))
                 {

                     // extrai o texto para um predicado, da variável texto é extraído o texto do predicado.
                     EditorTextoSuporteAProlog.ExtraiCaracteresDeUmPredicado(ref texto, ref textoPredicado);
                     if ((textoPredicado != "") && (textoPredicado.Length > 0))
                     {
                         if (ListaProlog.HasList(textoPredicado))
                             predicadosMetas.Add(new ListaProlog(textoPredicado));
                         else
                             // adiciona um Predicado para a lista de Predicados sub-metas.
                             predicadosMetas.Add(GetPredicado(textoPredicado));
                     } // if texto<>""
                      //extrai a parte restante do texto (o método de retirada de predicado, 
                     //não trabalha com a retirada da vírgula entre predicados, pois isto está definido apenas nos predicados de Regra.

                     int indiceValido = texto.IndexOf(",") + 1;
                     // primeiro caracter começando com [,], depois de extrair o texto do predicado já processado.
                     texto = texto.Substring(indiceValido).TrimEnd(' ').TrimStart(' ');
                 } // while (texto)

                 // substiitui nas listas a instância da variável nos predicados meta por instuções de lista (Head(lst),Tail(lst).
                 if (predicadoBaseDaRegra.GetType() == typeof(ListaProlog))
                     ((ListaProlog)predicadoBaseDaRegra).ReconstroiRegra((ListaProlog)predicadoBaseDaRegra, predicadosMetas);
                 // cria a regra, com especificações de lista, se tiver, e o predicado base e os predicados meta.
                 Regra regraResultado = new Regra(predicadoBaseDaRegra.Nome, predicadoBaseDaRegra, predicadosMetas);
                 return regraResultado;
             } // try
             catch (Exception ex)
             {
                 Log.addMessage(ex.ToString() + "  Falha na inserção do texto de uma regra, via editor de texto  PROLOG.");
                 return null;
             } // catch
         * 
         * 
         */
        public bool EqualsRegra(Regra r1)
        {
            if (this.Nome != r1.Nome)
                return false;
            if (this.GetAtomos().Count != r1.GetAtomos().Count)
                return false;
            for (int umPredicado = 0; umPredicado < this.PredicadosGoal.Count; umPredicado++)
                for (int umAtomo = 0; umAtomo < this.PredicadosGoal[umPredicado].GetAtomos().Count; umAtomo++) 
                if (this.PredicadosGoal[umPredicado].GetAtomos()[umAtomo] != 
                        r1.PredicadosGoal[umPredicado].GetAtomos()[umAtomo])
                    return false;
            return true;
        } // EqualsRegra()

        public static bool AdicionaUmaRegra(string texto)
        {
            try
            {
                Regra umaRegra = Regra.GetRegra(texto);
                BaseDeConhecimento.Instance().Base.Add(umaRegra);
                return true;
            }// try
            catch
            {
                return false;
            } // catch
        } // ConversaoDeTextoParaPredicado()

        private static bool IsRegra(string linhaTexto)
        {
            return (linhaTexto.IndexOf(":-") != -1);
        } // IsRegra()


        public override string ToString()
        {
            string strResult = "";
            if (PredicadoBase != null)
                if (ListaProlog.HasList(this.PredicadoBase))
                    strResult += ((ListaProlog)this.PredicadoBase).ToString().Replace(".","");
                else
                    strResult = this.PredicadoBase.ToString().Replace(".","") + ":- ";
          
            if (PredicadosGoal != null)
                for (int x = 0; x < PredicadosGoal.Count; x++)
                {
                    strResult += " " + PredicadosGoal[x].ToString().Replace(".","") + ",";
                } // if PredicadosGoal
            strResult = strResult.Remove(strResult.Length - 1);
            strResult = strResult.Replace(".", "");
            return strResult;
        } // ToString();
    } // class Regra
} // namespace
