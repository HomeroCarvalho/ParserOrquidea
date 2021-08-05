using System;

namespace parser.PROLOG
{
    public class EditorTextoSuporteAProlog
    {
       
        /// <summary>
        /// extrai um predicado a partir de um texto, no molde PredicadoX(fato1,fato2,...fatoN).
        /// O texto de entrada é modificado, retirando o texto do predicado.
        /// </summary>
        /// <param name="texto">texto a ser interpretado, retorna o texto de um predicado na saída.</param>
        /// <param name="textoPredicado">texto do predicado extraído.</param>
        public static void ExtraiCaracteresDeUmPredicado(ref string texto, ref string textoPredicado)
        {
            try
            {

                Pilha<string> pilhaParenteses = new Pilha<string>("pilha de parenteses");
                string predicadoEmTexto = "";
                int umCaracter = 0;
                while (umCaracter < texto.Length)
                {
                    if (texto[umCaracter].Equals('('))
                    {
                        predicadoEmTexto += "(";
                        pilhaParenteses.Push("");
                        umCaracter++;
                    } // if
                    else
                    if (texto[umCaracter].Equals(')'))
                    {
                        predicadoEmTexto += ")";
                        umCaracter++;
                        pilhaParenteses.Pop();
                        if (pilhaParenteses.Empty())
                        {
                            texto= texto.Replace(predicadoEmTexto,"");
                            textoPredicado = predicadoEmTexto;
                            return;
                        } //if
                    } // else
                    else
                    {
                        predicadoEmTexto += texto[umCaracter];
                        umCaracter++;
                    } // else
                } // while
                if ((umCaracter == texto.Length) && (!pilhaParenteses.Empty()))
                {
                    Log logFile = new Log("FalhasInterpretadorProlog.txt");
                    Log.addMessage("Falha na extração de um predicado a partir do editor de texto Prolog.");
                } // if
                if ((texto != null) && (texto.Length > 0))
                    texto = (string)texto.Replace(predicadoEmTexto, "").Clone();
            } // try
            catch (Exception ex)
            {
                Log logFile = new Log("FalhasInterpretadorProlog.txt");
                Log.addMessage("Falha na extração de um predicado a partir do editor de texto Prolog. mensagem de erro: " + ex.ToString() + "Stack: " + ex.StackTrace);
                texto = "";
                return;
            } // catch
        } // ExtraTextoPredicado()
    }// class
}// namespace
