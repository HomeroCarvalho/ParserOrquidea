using System;
using System.Collections.Generic;
using System.IO;
namespace parser.PROLOG
{
    public class ComandosProlog: Predicado
    {
        // predicados da linguagem:
        // 1- asserta(Predicado): adiciona o [Predicado] no inicio da lista de Predicados da Base de conhecimento. (feito)
        // 2- assertz(Predicado): adiciona o [Predicado] no fim da lista de Predicados da base de conhecimento.(feito)
        // 3- listing(nome): lista os predicados ou regras como nome [nome]. (feito)
        // 4- predicadoA(data(A,C)): define um objeto registro de nome [data], com campos [A] e [C].
        // 5- listas em PROLOG.
        //      5.1- [], uma lista vazia.
        //      5.2- [X|Y]: é uma lista vazia, [X|Y], com X= cabeça, e Y= calda (restante da lista após a cabeça).é uma lista com pelo menos um item.
        //      5.3- [X,Y]: lista com dois itens.
        // 6- tell(arquivo, Predicado(X)),told: grava em arquivo os Predicados entre tell e told, cujos predicados combinam com [Predicado].(feito).
        // 7- consult(arquivo): carrega em arquivo os Predicados gravados em arquivo.(feito)
        // 8- retract (predicado para consulta), que exclui predicados, da base de conhecimento.(feito)
        // 9- Is (seta uma variável com um valor).
        // 10- comparacao (compara duas variáveis).

        // funcionalidades de ComandosProlog:
        // 1- AdicionaFuncionalidade(): torna a linguagem estendível, seguindo o padrão de projeto Command.
        // 2- IsComando(): retorna [true] se o predicativo linha de texto da entrada é um comando da linguagem.
        // 3- ExecutaComando(): executa o método associado ao comando predicado.
        // 4- métodos de comandos pré-definidos em ComandosProlog.


        public Predicado PredicadoLinguagem { get; set; }
        public Comando Funcionalidade { get; set; }
        public static Dictionary<string, Comando> PredicativosComando { get; set; }

        public delegate bool Comando(Predicado predicadoEntrada);

        private static ComandosProlog comandosSingleton;

        private ComandosProlog()
        {
            if (PredicativosComando == null)
                InicializaFuncionalidades();
        } //ComandosProlog()

        public static ComandosProlog Instance()
        {
            if (comandosSingleton == null)
                comandosSingleton = new ComandosProlog();
            return comandosSingleton;
        }// ComandosProlog()


        /// <summary>
        /// torna a linguagem Prolog extendível, adicionando novos predicados comando.
        /// </summary>
        /// <param name="predicativoComando">nome do novo comando.</param>
        /// <param name="comando">função a ser chamada quando o comando é acionado.</param>
        public void AdicionaFuncionalidade(string predicativoComando, Comando comando)
        {
            try
            {
                PredicativosComando[predicativoComando] = comando;
            } // try
            catch (Exception e)
            {
                Log logPrologMessages = new Log("PrologErrors.txt");
                Log.addMessage("erro ao estender a linguagem do interpretador Prolog. Nome do comando: " + predicativoComando + " Mensagem de erro: " + e.Message + " Stack: " + e.StackTrace);
            } // catch
        } // AdicionaFuncionalidade()


        public Predicado GetPredicadoVerdadeiroFalso(bool valorPredicado)
        {
            
            if (valorPredicado)
                return Predicado.SetPredicado("Verdadeiro", "true");
            else
                return Predicado.SetPredicado("Falso", "false");
        } // GetPredicadoVerdadeiroFalso()

        private void InicializaFuncionalidades()
        {
            PredicativosComando = new Dictionary<string, Comando>();
            PredicativosComando["listing"] = Listing;
            PredicativosComando["asserta"] = Asserta;
            PredicativosComando["assertz"] = Assertz;
            PredicativosComando["consult"] = Consult;
            PredicativosComando["tell"] = Tell;
            PredicativosComando["retract"] = Rectract;
            PredicativosComando["Is"] = Is;
            PredicativosComando[">"] = Comparacao;
            PredicativosComando["<"] = Comparacao;
            PredicativosComando[">="] = Comparacao;
            PredicativosComando["<="] = Comparacao;
        } // InicializaFuncionalidades()


        public bool IsComando(Predicado pred)
        {
            foreach (KeyValuePair<string, Comando> umComando in PredicativosComando)
            {
                if (umComando.Key.Contains(pred.Nome))
                    return true;
            } // foreach
            return false;
        } // IsComando

        /// <summary>
        /// executa o comando associado ao predicado na linha de texto.
        /// </summary>
        /// <param name="parametros">parâmetros para o comando.</param>
        public bool ExecutaComando(Predicado predicado)
        {
            return PredicativosComando[predicado.Nome](predicado);
        } // ExecutaComando()

        /// <summary>
        /// devolve uma lista de texto contendo os predicados da consulta com a linha de texto de entrada.
        /// </summary>
        /// <param name="predicadosListagem">
        /// linhas de texto (texto[0] contendo o predicado a ser consultado, para executar o comando).</param>
        /// se (texto[1]<>null, escreve na tela os predicados da consulta do comando.
        /// <returns></returns>
        private bool Listing(Predicado predicadoEntrada)
        {
            Regra regra = new Regra();
            regra.PredicadoBase = predicadoEntrada;
            List<Predicado> predicados = Consultas.Instance().ConsultaBackTracking(regra);

            try
            {
                foreach (Predicado p in predicados)
                {
                    System.Console.WriteLine(p.ToString());
                } // foreach
                return true;
            }
            catch
            {
                return false; //não foi possível apresentar os predicados no console de entrada.
            }
        } // listing

        /// <summary>
        /// acrescenta os predicados de uma consulta no inicio da lista de predicados da base de conhecimento.
        /// </summary>
        /// <param name="predicadosAAcrescentar">linha de texto contendo o predicado a ser consultao, para o comando.</param>
        /// <param name="option">valor de funcionalidade opcional.</param>
        /// <returns></returns>
        private bool Asserta(Predicado predicadoEntrada)
        {
            Regra regra = new Regra();
            regra.PredicadoBase = predicadoEntrada;
            List<Predicado> predicados = Consultas.Instance().ConsultaBackTracking(regra);

            if ((predicados != null) && (predicados.Count > 0))
                foreach (Predicado p in predicados)
                    BaseDeConhecimento.Instance().Base.Insert(0, p);
            return true;
        } // asserta()

        /// <summary>
        /// acrescenta os predicados de uma consulta no final da lista de predicados da base de conhecimento.
        /// </summary>
        /// <param name="predicadosAAcrescentar">linhas de textos, texto[0]: contendo o predicado a ser consultado, para o comando.</param>
        /// <returns></returns>
        private bool Assertz(Predicado predicadoEntrada)
        {
            Regra regra = new Regra();
            regra.PredicadoBase = predicadoEntrada;
            List<Predicado> predicados = Consultas.Instance().ConsultaBackTracking(regra);
            if ((predicados != null) && (predicados.Count > 0))
                BaseDeConhecimento.Instance().Base.AddRange(predicados);
            return true;
        } // assertz()

        /// <summary>
        /// lê predicados cuja linhaDeTexto foram gravados em arquivo.
        /// </summary>
        /// <param name="parametros">parametros[0]: nome do arquivo para ler.</param>
        /// <returns></returns>
        private bool Consult(Predicado predicadoEntrada)
        {
            string nomeArquivoLeitura = predicadoEntrada.GetAtomos()[0];
            FileStream stream = new FileStream(nomeArquivoLeitura, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            List<Predicado> predicadosConsult = new List<Predicado>();
            while (!reader.EndOfStream)
            {
                string predicadoLinhaDeTexto = reader.ReadLine();
                predicadosConsult.Add(Predicado.GetPredicado(predicadoLinhaDeTexto));
            } // while
            if (predicadosConsult != null)
            {
                for (int x = 0; x < predicadosConsult.Count; x++)
                {
                    BaseDeConhecimento.Instance().Base.Remove(predicadosConsult[x]);
                } // for x
                return true;
            } // if
            return false;
        } // consult()

        private bool Tell(Predicado predicadoEntrada)
        {
            string nomeArquivo = predicadoEntrada.GetAtomos()[0];

            FileStream arquivoGravar = new FileStream(nomeArquivo, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(arquivoGravar);
            List<Predicado> predicadosAGravar = new List<Predicado>();
            for (int x = 1; x < predicadoEntrada.GetAtomos().Count; x++)
            {
                Predicado predicado = new Predicado(predicadoEntrada.GetAtomos()[x]);
                Regra regra = new Regra();
                regra.PredicadoBase = predicado;
                List<Predicado> predicados = Consultas.Instance().ConsultaBackTracking(regra);
                if ((predicados != null) && (predicados.Count > 0))
                    predicadosAGravar.AddRange(predicados);
                x++;
            } // for x
            if ((predicadosAGravar != null) && (predicadosAGravar.Count > 0))
            {
                foreach (Predicado p in predicadosAGravar)
                {
                    writer.WriteLine(p.ToString());
                } // foreach
            } // if
            else
                return false;
            writer.Close();
            arquivoGravar.Close();
            return true;
        } // Tell()


        /// <summary>
        /// deleta predicados da base de conhecimento, com um predicado selecionador na forma de linha de texto.
        /// </summary>
        /// <param name="linhasDeTextoPredicado">lista de textos com os parãmetros do comando. A primeira linha(texto[0]) é o predicado selecinador</param>
        /// <returns></returns>
        private bool Rectract(Predicado predicadoEntrada)
        {
            Regra regra = new Regra();
            regra.PredicadoBase = predicadoEntrada;
            List<Predicado> predicadosAApagar = Consultas.Instance().ConsultaBackTracking(regra);
            if ((predicadosAApagar != null) && (predicadosAApagar.Count > 0))
            {
                foreach (Predicado p in predicadosAApagar)
                    BaseDeConhecimento.Instance().Base.Remove(p);
                return true;
            } // if (predicadosAAPagar<>null)
            else
                return false;
        } // rectract()

   

        private bool Is(Predicado predicadoEntrada)
        {
            string valorTexto = predicadoEntrada.GetAtomos()[2];
            string predicadoTexto = predicadoEntrada.GetAtomos()[1] + "(" + valorTexto + ")";
            Predicado p = new Predicado(predicadoTexto);
            int x = 0;
            while ((x < BaseDeConhecimento.Instance().Base.Count) && (p.GetAtomos()[0] != BaseDeConhecimento.Instance().Base[x].GetAtomos()[0]))
                x++;
            // se não existir o predicado representando a variável [predicadoTexto], cria um predicado no começo da lista de Predicados da Base de Conhecimento.
            if (x >= BaseDeConhecimento.Instance().Base.Count)
            {
                this.Asserta(p);
                return true;
            } // if x
            else
            {
                // se existir o predicado na Base de Conhecimento, altera o átomo do predicado, tornando-se: predicadoTexto(valorTexto).
                BaseDeConhecimento.Instance().Base[x].GetAtomos().Clear();
                BaseDeConhecimento.Instance().Base[x].GetAtomos().Add(valorTexto);
                return true;
            } // else
        }//Is()

        /// <summary>
        /// comparação de ordem de grandeza de número com zero.
        /// Sintaxe: A>B  A<B  A>=B  ou  A< = B, ou: A operador B  (A e B duas variáveis inteiras).
        /// </summary>
        /// <paramref name="linhasDeTextoPredicado">linha de texto contendo o comando de comparação.</paramref>
        /// <returns></returns>
        private bool Comparacao(Predicado predicadoEntrada)
        {
            string[] tokensComando = predicadoEntrada.GetAtomos()[1].Split(new string[] { "<", ">", "<=", ">=" ,"=:=","=!="}, StringSplitOptions.RemoveEmptyEntries);
            Predicado p1 = new Predicado(tokensComando[0]);
            string funcaoComparacao = tokensComando[1];
            Predicado p2 = new Predicado(tokensComando[2]);
            try
            {
                float valor1 = float.Parse(p1.GetAtomos()[0]);
                float valor2 = float.Parse(p2.GetAtomos()[0]);
                switch (funcaoComparacao)
                {
                    case "=:=": return (valor1 == valor2);
                    case "=!=": return (valor1 != valor2);
                    case ">": return (valor1 > valor2);
                    case "<": return (valor1 < valor2);
                    case ">=": return (valor1 >= valor2);
                    case "<=": return (valor1 <= valor2);
                    default: return (false);
                } // swicth ()
            } // try
            catch (Exception e)
            {
                Log log = new Log("PrologErrors.txt");
                Log.addMessage("Erro no Processamento de números. Mensagem de Erros: " + e.Message.ToString() + " Stack: " + e.StackTrace);
                return false;
            } // catch
        } // Comparacao()
    } // class ComandosProlog
} // namespace  parser.PROLOG
