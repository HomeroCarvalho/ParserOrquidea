using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using stringUtilities;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    /// <summary>
    /// Classe para uma definicao de linguagem a mais generica possivel, carregada via aruivo.XML.
    /// </summary>
    public abstract class UmaGramaticaComputacional
    {
        protected static List<producao> producoes = new List<producao>();
        public List<producao> GetProducoes()
        {
            return producoes;
        }
        public static List<Operador> operadores = new List<Operador>();
        public virtual List<Operador> GetOperadores()
        {
            return operadores;
        }

        private static List<string> todosTermosChave;
 
        private static List<string> strOperadores = null;
 
        /// <summary>
        /// carrega as producoes da linguagem via um arquivo .XML.
        /// </summary>
        public abstract void inicializaProducoesDaLinguagem();

        /// <summary>
        /// inicializa a linguagem: definições em [producoes], carregada por um arquivo .XML
        /// A ideia eh separar o front end do compilador do back end. Isto permite uma possibilidade de modificar
        /// a linguagem de forma mais facil, com o arquivo de definicao cobrindo tambem a maquina virtual (VM),
        /// permitindo o compilador e a execucao do programa compilado mais faceis de ser modificado.
        /// </summary>
        public UmaGramaticaComputacional()
        {


            if (producoes.Count == 0)
            {
                todosTermosChave = new List<string>();
                this.inicializaProducoesDaLinguagem();
                strOperadores = new List<string>();

                int producao;

                // termina o processamento das producoes.
                for (producao = 0; producao < producoes.Count; producao++)
                {
                    producoes[producao].nomeProducao = producoes[producao].nomeProducao.Trim(' ');
                    todosTermosChave.AddRange(producoes[producao].termos_Chave);
                } // for producao

                // adiciona os operadores previsto no arquivo .XML da linguagem.
                foreach (producao umOperador in producoes)
                    if ((umOperador.tipo.Contains("BINARIO")) || (umOperador.tipo.Contains("UNARIO")) || (umOperador.tipo.Contains("DUPLO")))
                        foreach (string nomesOperador in umOperador.termos_Chave)
                            if (strOperadores.IndexOf(nomesOperador) == -1)
                                strOperadores.Add(nomesOperador);

            } // if producoes.Count==0.

        } // UmaGramaticaComputacional()


        public static void RegistraOperador(Operador operador)
        {
            operadores.Add(operador);
        }
   

        /// <summary>
        /// obtém uma produção ou produções pelo seu tipo.
        /// </summary>
        /// <param name="tipo">tipo de produção.</param>
        /// <param name="prod">produção a ser excluída.</param>
        /// <returns>retorna um vetor contendo todas produções cujo tipo de produção
        /// é igual a [tipo].</returns>
        public producao LocalizaProducaoPeloTipo(string tipo, producao prod)
        {

            return producoes.Find(k => k.tipo == tipo && (k.nomeProducao != prod.nomeProducao));
        } // LocalizaProducoesPeloTipo()

        /// <summary>
        /// localiza as produções com primeiro termo chave igual a [primeiroTermoChave]
        /// </summary>
        /// <param name="termoChaveProcura">termo chave para a busca.</param>
        /// <returns></returns>
        public List<producao> LocalizaProducoesPorTermoChave(string termoChaveProcura)
        {
            List<producao> producoesRetorno = new List<producao>();
            for (int p = 0; p < producoes.Count; p++)
            {
                if ((producoes[p].termos_Chave != null) && (producoes[p].termos_Chave.Count > 0) && (producoes[p].termos_Chave[0] == termoChaveProcura))
                    producoesRetorno.Add(producoes[p]);
            }
            return producoesRetorno;
        }

        /// <summary>
        /// localiza uma produção pelo nome.
        /// </summary>
        /// <param name="nome"></param>
        /// <returns></returns>
        public List<producao> LocalizaProducoesPeloNome(string nome, producao prod)
        {
            if (prod == null)
                return producoes.FindAll(k => k.nomeProducao == nome);
            else
                return producoes.FindAll(k => k.nomeProducao == nome && (k.nomeProducao != prod.nomeProducao));
        } // LocalizaProducoesPeloTipo()

        /// <summary>
        /// adiciona uma producao para a sintaxe da linguagem.
        /// </summary>
        /// <param name="novaproducao">nova produção, extendendo a linguagem.</param>
        public void adicionaProducao(producao novaproducao)
        {
            producoes.Add(novaproducao);
        }

        /// <summary>
        /// retorna todos os termos chave de todas produções da linguagem currente,
        /// inclusive de todos operadores registrados na linguagem currente.
        /// </summary>
        /// <returns>todos os termos chave da presente linguagem.</returns>
        public List<string> GetTodosTermosChave()
        {
            return todosTermosChave.ToList<string>();
        
        }// GetTodosTermosChave()

       

        public List<string> GetOperadoresBloco()
        {

            return new List<string>() { "{", "}" };
        } // GetOperadoresBloco()

        /// <summary>
        /// obtém todos operadores duplos (exemplo: [(] e [)] ).São operadores que aparecem com dois tokens.
        /// </summary>
        /// <returns></returns>
        public List<string[]> GetTodosOperadoresDuplos()
        {
            List<string[]> lstOperadoresDuplo = new List<string[]>();
            foreach (producao p in producoes)
            {
                if ((p.tipo.Contains("OPERADOR BLOCO"))
                    || (p.tipo.Contains("OPERADOR PARENTESES")))
                    lstOperadoresDuplo.Add(new string[] { p.termos_Chave[0], p.termos_Chave[1] });

            } // foreach
            return lstOperadoresDuplo;
        } // GetTodosOperadoresDuplos()

       
        /// <summary>
        /// retorna todos operadores presentes na  linguagem, inclusive de classe herdeiras. 
        /// </summary>
        /// <returns>lista de operadores genéricos registrados.</returns>
        public virtual List<string> GetTodosOperadores()
        {
            return strOperadores;
        } // GetTodosOperadores()

        /// <summary>
        /// obtém o propriedade [operador] para um dado nome.
        /// </summary>
        /// <param name="nomeOperador">nome do operador a ser pesquisado.</param>
        /// o<returns>o propriedade [operador] para o nome parâmetro.</returns>
        public Operador GetOperadorPeloNome(string nomeOperador)
        {
            return operadores.Find(k => k.nome == nomeOperador);
        } // GetOperadorPeloNome()

        /// <summary>
        /// obtém a prioridade (um número inteiro para comparação com outros operadores)
        /// do operador parâmetro.
        /// </summary>
        /// <param name="nomeOperador">nome do operador a ser pesquisado.</param>
        /// <returns></returns>
        public int GetPrioridadeOperador(string nomeOperador)
        {
                return (this.GetOperadorPeloNome(nomeOperador).GetPrioridade());
        }

     
        /// <summary>
        /// verifica se uma palavra é um operador-duplo, um tipo
        /// especial de operador que aparece duas vezes para complitude.
        /// </summary>
        /// <param name="token">palavra a ser investigada</param>
        /// <returns>retorna true se o [token] é operador duplo, false se não.</returns>
        public bool VerificaSeEhOperadorDuplo(string token)
        {
            return (VerificaSeEhOperador(token, "OPERADOR DUPLO"));
        } // VerificaSeEhOperadorDuplo()

        /// <summary>
        /// verifica se uma palavra é um operador binário.
        /// </summary>
        /// <param name="token">palavra a ser investigada.</param>
        /// <returns>[true] se a palavra investigada é um operador binário, [false] se não.</returns>
        public bool VerificaSeEhOperadorBinario(string token)
        {
            return (VerificaSeEhOperador(token, "OPERADOR BINARIO"));
        }

        /// <summary>
        /// verifica se uma palavra é um operador unário.
        /// </summary>
        /// <param name="token">palavra a ser investigada.</param>
        /// <returns>[true] se [token] é um operador unário, false se não.</returns>
        public bool VerificaSeEhOperadorUnario(string token)
        {

            return (this.VerificaSeEhOperador(token, "OPERADOR UNARIO"));
        }

        /// <summary>
        /// verifica se a palavra é um operador.
        /// </summary>
        /// <param name="token">palavra a ser investigada.</param>
        /// <returns>retorna [true] se o [token] esta registrado como operador, [false] se não.</returns>
        public bool VerificaSeEhOperador(string token)
        {
            if (token == null)
                return false;
            return this.GetOperadores().Find(k => k.nome == token) != null;
        } // VerificaSeEhOperador()

        /// <summary>
        /// verifica se um [token] é um operador do tipo [tipoOperador] (como "OPERADOR BINARIO", "OPERADOR UNARIO","OPERADOR DUPLO".
        /// </summary>
        /// <param name="token">palavra a ser investigada.</param>
        /// <param name="tipoOperador">tipo do operador.</param>
        /// <returns>[true] se é um [operador=token] e [tipo=tipoOperador], [false] se não.</returns>
        private bool VerificaSeEhOperador(string token, string tipoOperador)
        {
            token = token.TrimStart(' ').TrimEnd(' ');
            for (int p=0; p<producoes.Count;p++)
            {
                if ((producoes[p].tipo.Contains(tipoOperador)) &&
                    (producoes[p].termos_Chave.Count>0) &&
                    (token == producoes[p].termos_Chave[0]))
                    return true;
            } // for p
            return false;
        } // VerificaSeEhOperador()

        /// <summary>
        /// verifica se uma palavra é um número: int, float, ou double.
        /// </summary>
        /// <param name="token">palavra a ser investigada se é número.</param>
        /// <returns>[true] se o token e numero, [false] se nao.</returns>
        public bool VerificaSeEhNumero(string token)
        {
            int iNumero = 0;
            float fNumero = 0;
            double dNumero = 0;
        
            bool isNumero = int.TryParse(token,out iNumero);
            if (isNumero)
                return true;

            isNumero = float.TryParse(token, out fNumero);
            if (isNumero)
                return true;

            isNumero = double.TryParse(token, out dNumero);
            if (isNumero)
                return true;
            return isNumero;
        } // VerificaSeEhNumero()

        public bool VerificaSeEString(string token)
        {
            char aspas = '\"';
            return (token[0].Equals(aspas) && (token[token.Length - 1].Equals(aspas)));
            
        }

        public bool VerificaSeEhInt(string token)
        {
            try
            {
                int.Parse(token);
                return true;

            }
            catch
            {
                return false;
            }
        }

        public bool VerificaSeEhFloat(string token)
        {
            try
            {
                float.Parse(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool isTermoChave(string token)
        {
            return (this.GetTodosTermosChave().Find(k => k == token.Trim(' ')) != null);
  
        } //  VerificaSeEhTermoChave()

        /// <summary>
        /// Verifica se o token é um ID ou número.
        /// </summary>
        /// <param name="token">palavra a ser investigada.</param>
        /// <returns>retorna [true] se a palavra for um ID,
        ///                 [true] se a  palavra for um número, ou
        ///                 [false] se a palavra for reservada (contendo termos-chave, e não é um número).</returns>
        public bool VerificaSeEhID(string token)
        {
            token = token.TrimStart(' ').TrimEnd(' ');
            List<string> todosTermosChaveETodosOperadores = new List<string>();
            //obtém dos termos-chave da linguagem.
            todosTermosChaveETodosOperadores.AddRange(GetTodosTermosChave().ToList<string>());
            // obtém todos operadores da linguagem.
            todosTermosChaveETodosOperadores.AddRange(this.GetTodosOperadores());

            int index = todosTermosChaveETodosOperadores.FindIndex(k => k == token);
            if (index != -1)
                return false;          
            if (this.VerificaSeEhNumero(token).Equals("[NUMERO]"))
                return false;
            return true;
        } // bool VerificaSeEhID(string)

       
        /// <summary>
        /// método utilizado para localizar chars dot ('.').
        /// </summary>
        /// <param name="s">caracter a ser investigado.</param>
        /// <returns>[true] se o caracter é um dot ('.'), [false] se contrário.</returns>
        private bool dotcont(char s)
        {
            if (s.Equals('.'))
                return true;
            return false;
        } // dotCont()
 
    } // Classe UmaGramaticaComputacional
} // namespace parser
