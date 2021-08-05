using System;
using System.Collections.Generic;
using System.Linq;

namespace parser.PROLOG
{
    public class Predicado
    {
        /// MÓDULO PREDICADO
        /// FUNCIONALIDADES EXPORTADOS:
        /// 1- PREDICADO(): CONSTRÓI UM PREDICADO SEM VALORES.
        /// 2- CONSTRUTOR PREDICADO(TEXTO): CONSTRÓI UM PREDICADO A PARTIR DE UM TEXTO.
        /// 3- CONSTRUTOR PREDICADO(PREDICADO): CONSTRÓI UM PREDICADO A PARTIR DE UM PREDICADO DE ENTRADA.
        /// 4- ADICIONA_PREDICADOS(TEXTO[]): RETIRA PREDICADOS DE UM VETOR DE TEXTOS, E OS ADICIONA À BASE DE CONHECIMENTO.
        /// 5- ADICIONA_UM_PREDICADO(TEXTO): CONSTRÓI UM PREDICADO E O ADICIONA À BASE DE CONHECIMENTO.
        /// 6- SET_PREDICADO(): RETORNA UM PREDICADO A PARTIR DE UM TEXTO NOME E UM VETOR DE TEXTOS ÁTOMOS.
        /// 7- GET_PREDICADO(TEXTO): RETORNA UM PREDICADO A PARTIR DE UM TEXTO DE ENTRADA.
        /// 8- GETATOMOS(): RETORNA O CONJUNTO DE ÁTOMOS DO PREDICADO.(UM ÁTOMO É A MENOR PARTE DE UMA ESTRUTURA PROLOG. PODE SER UMA VARIÁVEL OU VALOR.
        /// 9- MATCH(PREDICADO): VERIFICA SE O PREDICADO DE ENTRADA COMBINA COM O PREDICADO QUE CHAMOU O MÉTODO. (COMBINAR É VERIFICAR SE TEM O MESMO NOME, E MESMOS ÁTOMOS QUE NÃO SÃO VARIÁVEIS).
        /// 10- IS_VARIAVEL_ANONIMA(TEXTO): VERIFICA SE O TEXTO DE ENTRADA É UMA VARIÁVEL ANÔNIMA.
        /// 11- RETIRA_VARIAVEIS: RETORNA UMA LISTA DAS VARIÁVEIS CONTIDAS NO PREDICADO.
     

        public string Nome { get; set; }

        private List<string> Atomos = new List<string>();


        public Predicado(string textoPredicado)
        {
            Predicado p = Predicado.GetPredicado(textoPredicado);
            this.Nome = p.Nome;
            this.Atomos = p.Atomos.ToList<string>();
        } // Predicado()

        public static Predicado SetPredicado(string nome, params string[] atomos)
        {
            Predicado p1 = new Predicado();
            p1.Nome = nome;
            p1.Atomos = atomos.ToList<string>();
            return p1;
        } // SetPredicado()

        public Predicado()
        {
            this.Nome = "";
            this.Atomos = new List<string>();
        } //Predicado()

        public List<string> GetAtomos()
        {
            return Atomos;
        } // GetAtomos()

        public Predicado Clone()
        {
            Predicado predicadoClonado = new Predicado();
            predicadoClonado.Atomos = this.Atomos.ToList<string>();
            predicadoClonado.Nome = this.Nome;

            return predicadoClonado;
        }

        public bool Match(Predicado predicadoParaMatch)
        {
            // se os predicados não são compatíveis (mesmo nome e mesmo número de átomos), retorna fals;
            if ((predicadoParaMatch.Nome != this.Nome) || (predicadoParaMatch.GetAtomos().Count != this.GetAtomos().Count))
                return false;
            // extrai as variáveis e fatos do predicado de entrada.

            // prepara os vetores para o Match do predicado inicial e o predicado que chamou este método.
            string[] atomosConsulta = predicadoParaMatch.Atomos.ToArray();
            int[] variaveisDoPredicado = new int[predicadoParaMatch.Atomos.Count];
            string[] fatosDoPredicado = new string[predicadoParaMatch.Atomos.Count];

            // preenche os vetores de variáveis e de fatos, do predicado de entrada.
            PreparaConsulta(predicadoParaMatch.Atomos.ToArray(), out variaveisDoPredicado, out fatosDoPredicado);
            for (int x = 0; x < atomosConsulta.Length; x++)
            {
                // compara átomos que não são variáveis, do predicado de entrada, com o predicado que chamou este método.
                if ((variaveisDoPredicado[x] == -1) && 
                   (!fatosDoPredicado[x].Equals(GetAtomos()[x])))
                    return false;
            } // for x
            return true;
        }//Match()

        public static bool IsVariavelAnonima(string variavel)
        {
            if ((IsVariavel(variavel)) && (variavel == "_"))
                return true;
            return false;
        } // IsVariavelAnonimo()

        /// <summary>
        /// prepara o predicado para uma consulta, diferenciando variáveis de fatos.
        /// </summary>
        /// <param name="atomosDoPredicado">átomos do predicado a ser preparado para consulta.</param>
        /// <param name="variaveisDoPredicado">variáveis do predicado (letra inicial maiúscula).</param>
        /// <param name="fatosDoPredicado">fatos do predicado (letra inicial minúscula).</param>
        private void PreparaConsulta(string[] atomosDoPredicado, out int[] variaveisDoPredicado, out string[] fatosDoPredicado)
        {
            variaveisDoPredicado = new int[atomosDoPredicado.Length];
            fatosDoPredicado = new string[atomosDoPredicado.Length];
            for (int x = 0; x < atomosDoPredicado.Length; x++)
            {
                // regra de negócio: 
                // 1- um texto com inicial maiúscula é uma variável de consulta.
                // 2- um texto com inicial minúscula é um fato para consulta.
                // 3- um texto  nao é variavel anônima.
                string letraInicial = atomosDoPredicado[x][0].ToString();
                if (letraInicial == atomosDoPredicado[x][0].ToString().ToUpper())
                {
                    variaveisDoPredicado[x] = x;
                    fatosDoPredicado[x] = null;
                } // if
                else
                if (letraInicial == atomosDoPredicado[x][0].ToString().ToLower())
                {
                    variaveisDoPredicado[x] = -1;
                    fatosDoPredicado[x] = atomosDoPredicado[x];
                } // if
                else
                if (IsVariavelAnonima(atomosDoPredicado[x]))
                {
                    variaveisDoPredicado[x] = -1;
                    fatosDoPredicado[x] = null;
                } //id
            } // for x
        }// PreparaConsulta()

        /// <summary>
        /// obtém um Predicado a partir de uma linha de texto.O Predicado não é adicionado à base de conhecimento.
        /// </summary>
        /// <param name="texto">linha de texto descrevendo o Predicado.</param>
        /// <returns>retorna um objeto Predicado, a partir da linha de texto.</returns>
        public static Predicado GetPredicado(string texto)
        {
            List<string> tokens = ExtraiDeTextoUmPredicado(texto);
            string nomePredicado = tokens[0];
            //  exemplo de predicado complexo: "tell(teste.txt, homem(X))"  OPERADORES: ( , ( ) )
            nomePredicado = nomePredicado.TrimStart(' ').TrimEnd(' ');
            return Predicado.SetPredicado(nomePredicado, tokens.ToArray());
        } // GetPredicado()

        /// <summary>
        /// retira as variáveis do predicado.
        /// </summary>
        /// <returns></returns>
        public List<string> RetiraVariaveis()
        {
            List<string> variaveis = new List<string>();
            foreach (string umaVariavel in GetAtomos())
                if (IsVariavel(umaVariavel))
                    variaveis.Add(umaVariavel);
            return variaveis;
        } // RetiraVariaveis()

        public void SetVariavel(string nomeDaVariavel, string valorDaVariavel)
        {
            if (IsVariavel(nomeDaVariavel))
            {
                for (int x = 0; x < this.Atomos.Count; x++)
                {
                    if (this.Atomos[x].Equals(nomeDaVariavel))
                        this.Atomos[x] = valorDaVariavel;
                }
            } // if
        }

        public void SetVariavel( string valorDaVariavel, int indexVariavel)
        {
            if (indexVariavel < this.Atomos.Count)
                this.Atomos[indexVariavel] = valorDaVariavel;
        }
        /// <summary>
        /// Obtém tokens a partir de um texto, a partir de termos-chave da linguagem Prolog. Utiliza a classe ParserUniversal, através da classe Tokens.
        /// </summary>
        /// <param name="texto">texto a ter retirado os tokens.</param>
        /// <returns>retorna uma lista com tokens da linguagem Prolog.</returns>
        public static List<string> ExtraiDeTextoUmPredicado(string texto)
        {
            // termos chave da linguagem Prolog.
            List<string> termosChaveProlog = new List<string>() { ",", " ", ":-", "(", ")", "[", "]", "|", "." };

            // retira todos tokens de um texto.
            List<string> tokens = new Tokens(new LinguagemOrquidea(), new List<string>(){texto}).GetTokens();
            return tokens;
        } // ExtraiDeTextoUmPredicado()

        /// <summary>
        /// adiciona uma lista de predicados, na forma de textos, para a base de conhecimento.
        /// </summary>
        /// <param name="textosPredicados">linhas de texto descrevendo os Predicados.</param>
        public static void AdicionaPredicados(params string[] textosPredicados)
        {
            for (int umPredicado = 0; umPredicado < textosPredicados.Length; umPredicado++)
            {
                AdicionaUmPredicado(textosPredicados[umPredicado].TrimStart(' ').TrimStart(' '));
            } // for x
        } // AdicionaPredicados()

        /// <summary>
        /// adiciona um Predicado para a base de conhecimento, a partir da linha de texto contendo o Predicado.
        /// </summary>
        /// <param name="texto">linha de texto descrevendo o Predicado.</param>
        /// <returns>[true] se a adição foi realizada, [false] se ocorreu algum problema.</returns>
        public static bool AdicionaUmPredicado(string texto)
        {
            try
            {
                Predicado umPredicado = Predicado.GetPredicado(texto);
                BaseDeConhecimento.Instance().Base.Add(umPredicado);
                return true;
            }// try
            catch
            {
                return false;
            } // catch
        } // ConversaoDeTextoParaPredicado()

       
        public static bool IsVariavel(string atomo)
        {

            try
            {
                int isNumero = int.Parse(atomo);
                return false;
            }
            // se a tentativa de converter o texto [atomo] falhar, é porque não é número.
            catch
            {
                // verifica se a primeira letra do texto [atomo] é maiúscula, caso que é uma variável.
                return (atomo[0].ToString() == atomo[0].ToString().ToUpper());
            }

        } // IsVariavel()

        public bool EqualsPredicado(Predicado p1)
        {
            if (this.Nome != p1.Nome)
                return false;
            if (this.Atomos.Count != p1.Atomos.Count)
                return false;
            for (int umAtomo = 0; umAtomo < this.Atomos.Count; umAtomo++)
            {
                if (this.Atomos[umAtomo] != p1.Atomos[umAtomo])
                    return false;
            } // for umAtomo
            return true;
        } // EqualsPredicado()

        public override string ToString()
        {
            string str = "";
            int i = 0;
            str += this.Nome + "(";
            for (i = 0; i < this.Atomos.Count - 1; i++)
                if (!IsVariavelAnonima(this.Atomos[i]))
                    str += this.Atomos[i] + ",";
            if (!IsVariavelAnonima(this.Atomos[i]))
                str += this.Atomos[i] + ").";
            return str;
        } // ToString()
    } // class Predicado

} // namespace
