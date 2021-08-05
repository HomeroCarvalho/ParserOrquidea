using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    /// <summary>
    /// interface utilizada para extender a classe [compilador] a N linguagens a 
    /// definir.
    /// </summary>
    public abstract class UmaGramaticaComputacional
    {
        public List<producao> producoes = new List<producao>();
        public List<operador> operadores = new List<operador>();
   
        public List<metodo> metodos = new List<metodo>();
        public List<classe> classes = new List<classe>();
        public string[] todosTermosChaveDaLinguagem { get; set; }
        public List<string> strOperadores = new List<string>();
        protected abstract void inicializaPropriedadesOperadoresNativosParaLinguagem();

        protected abstract void inicializaProducoesDaLinguagem();

        /// <summary>
        /// inicializa a linguagem: definições em [producoes],
        /// algum tratamento em acréscimo (meta-tag "-r"),   e
        /// eliminando espaços vazios no nome da produção
        /// </summary>
        public UmaGramaticaComputacional()
        {
            this.inicializaProducoesDaLinguagem();
            this.inicializaPropriedadesOperadoresNativosParaLinguagem();
            this.todosTermosChaveDaLinguagem = this.getTodosTermosChave();
            int producao, termoChave;

            // termina o processamento das producoes.
            for (producao = 0; producao < this.producoes.Count; producao++)
            {
                producoes[producao].nomeProducao = producoes[producao].nomeProducao.Trim();
                for (termoChave = 0; termoChave < this.producoes[producao].termosChave.Count;
                       termoChave++)
                    this.producoes[producao].termosChave[termoChave].Trim();
            } // for producao
            // obtém os operadores dentro do arquivo de BNF. Pode-se posteriormente acrescentar
            // mais operadores.
            for (producao = 0; producao < this.producoes.Count; producao++)
            {
                if (this.producoes[producao].tipo.Equals("[OPERADOR]"))
                {
                    this.strOperadores.Add(this.producoes[producao].termosChave[0]);
                }
            } // for producao
        } // UmaGramaticaComputacional()

        
        
        
        /// <summary>
        /// faz a validação de uma produção para um trecho de programa, através
        /// de uma amostra de termos-chave da produção [prod] e termos-chave
        /// presentes no trecho de programa [_programa]. A idéia principal aqui
        /// é que operadores são tokens variantes nas produções da gramática,
        /// tornando as comparações dos termos-chave uma temeridade. Então,
        /// pegamos uma amostra de termos-chave sem repetições, o que elimina
        /// o problema da variância. Outra idéia é unificar linhas do programa
        /// para tentar conseguir pegar todos termos-chave necessários.
        /// </summary>
        /// <param name="lng">gramática computacional utilizada.</param>
        /// <param name="prod">produção dentro da gramática, a ser validada.</param>
        /// <param name="_programa">lista contendo as linhas de instruções da linguagem utilizada.</param>
        /// <param name="line">índice dentro da lista do programa, para indicar a instrução currente.</param>
        /// <returns></returns>
        public bool match(UmaGramaticaComputacional lng, producao prod, ref List<string> _programa, int line)
        {

            List<string> programa = _programa.ToList<string>();
            try
            {
                string[] todosOperadores = lng.getTodosOperadores();
                // calcula os termos-chave da produção [prod], sem repetições.
                List<string> lstTokensProducao = stringUtilities.localizadorDeStrings.localizaStringsSemRepeticoes(prod.termosChave).ToList<string>();
                // calcula os termos-chave presentes no trecho de programa, sem repetições.
                List<string> lstTokensPresentes = stringUtilities.localizadorDeStrings.localizaStringsSemRepeticoes(programa[line], prod.termosChave).ToList<string>();
                // faz o cálculo de instruções dispostas em múltiplas linhas.
                while ((lstTokensPresentes.Count < lstTokensProducao.Count) && (programa.Count > 1))
                {
                    // tenta unificar o trecho do programa, para que todos termos-chave da produção [prod] estejam 
                    // presentes numa única linha de programa.
                    uneDuasLinhasDePrograma(line, ref programa);

                    // recalcula os termos-chave da produção a ser investigada.
                    lstTokensPresentes = stringUtilities.localizadorDeStrings.localizaStringsSemRepeticoes(
                                        programa[line],
                                        prod.termosChave).ToList<string>();

                    // chama recursivamente o método, pois duas linhas foram unificadas.
                    return (this.match(lng, prod, ref programa, line));

                }// while termosChavePresentes
                // todos termos-chave presentes foram localizados, tenta comparar os termos-chave 
                // da produção investigada e os termos-chave do programa
                for (int i = 0; i < lstTokensProducao.Count; i++)
                {

                    // compara termos-chave que não são repetidos. Operadores podem variar, e são termos-chave falsos. 
                    // outro problema é que itens iguais, como operadores, atrapalham a pesquisa de termos-chave.
                    if (!lstTokensPresentes[i].Equals(lstTokensProducao[i]))
                        return false;
                } // for y
                _programa = programa.ToList<string>();
                return true;
            } // try
            catch
            {
                return false;
            } // catch
        } // bool match()

        /// <summary>
        /// une duas linhas do programa, para compatibilidade entre termos-chave
        /// espalhados por diversas linhas. A lógica é que se alcançar o final do
        /// programa, retorna [true], porque atingiu o termino do trecho do
        /// programa, [false] se não atingiu o final, e uniu as linhas.
        /// </summary>
        /// <param name="line">linha a ser unida com a linha seguinte.</param>
        /// <param name="programa">trecho do programa sendo processado.</param>
        /// <returns>retorna [false] alcancou o final do programa e não uniu as linhas, 
        /// [true] se uniu as linhas e não alcancou o final do programa.</returns>
        public static bool uneDuasLinhasDePrograma(int line, ref List<string> programa)
        {
            if (line < programa.Count - 1)
            {
                programa[line] += programa[line + 1];
                programa.RemoveAt(line + 1);
                return true;
            } // if
            return false;
        } // uneDuasLinhasDePrograma()

        
        /// <summary>
        /// obtém uma produção ou produções pelo seu tipo.
        /// </summary>
        /// <param name="tipo">tipo de produção.</param>
        /// <returns>retorna um vetor contendo todas produções cujo tipo de produção
        /// é igual a [tipo].</returns>
        public producao[] localizaProducoesPeloTipo(string tipo)
        {
            List<producao> lstProducoesLocalizadas = new List<producao>();
            foreach (producao p in producoes)
                if (p.tipo.Equals(tipo))
                    lstProducoesLocalizadas.Add(p);

            return lstProducoesLocalizadas.ToArray();    
        } // localizaProducoesPeloTipo()

        /// <summary>
        /// localiza uma produção pelo nome.
        /// </summary>
        /// <param name="nome"></param>
        /// <returns></returns>
        public producao localizaProducaoPeloNome(string nome)
        {
            foreach (producao p in producoes)
                if (p.nomeProducao.Equals(nome))
                    return p;
            return null;
        } // localizaProducoesPeloTipo()

     
        /// <summary>
        /// adiciona uma producao para a sintaxe da linguagem.
        /// </summary>
        /// <param name="novaproducao">nova produção, extendendo a linguagem.</param>
        public void adicionaProducao(producao novaproducao)
        {
            this.producoes.Add(novaproducao);
        }

        /// <summary>
        /// adiciona um operador para a sintaxe da linguagem.
        /// </summary>
        /// <param name="op"></param>
        public void adicionaOperador(operador op)
        {
            this.operadores.Add(op);
            this.strOperadores.Add(op.getNomeMetodo());
        } // void adicionaaOperador()
        
        /// <summary>
        /// retorna todos os termos chave de todas produções da linguagem currente,
        /// inclusive de todos operadores registrados na linguagem currente.
        /// </summary>
        /// <returns>todos os termos chave da presente linguagem.</returns>
        public string[] getTodosTermosChave()
        {
            List<string> trmchv = new List<string>();
            int x = 0;
            for (x = 0; x < this.producoes.Count; x++)
                if (!this.producoes[x].nomeProducao.Equals("[ID]"))
                {
                    for (int k = 0; k < this.producoes[x].termosChave.Count; k++)
                        trmchv.Add(this.producoes[x].termosChave[k]);
                } // if 
             for (x = 0; x < this.operadores.Count; x++)
            {
                trmchv.Add(this.operadores[x].nomeMetodo);
            } // for x
       
            return (trmchv.ToArray());
        } // getTodosTermosChave()


        /// <summary>
        /// retorna todos operadores presentes na  linguagem. TOMAR CUIDADO NO ARQUIVO .xml DE DEFINIÇÕES
        /// INSERIR EM TODOS OPERADORES, APENAS UM OPERADOR, OU SEJA OS TERMOS-CHAVE É REFERENTE A APE-
        /// NAS UM OPERADOR.
        /// </summary>
        /// <returns></returns>
        public string[] getTodosOperadores()
        {
            List<string> lstOp= new List<string>();
            // procura operadores criados na definição BNF da linguagem.
            for (int producao = 0; producao < this.producoes.Count; producao++)
            {
                if (this.producoes[producao].tipo.Equals("[OPERADOR]"))
                {
                   lstOp.Add(this.producoes[producao].termosChave[0]);
                } // if

            } // for
            // adiciona os operadores criados dinamicamente
            for (int i = 0; i < this.operadores.Count; i++)
                lstOp.Add(this.operadores[i].getNomeMetodo());

            return lstOp.ToArray();
        }  // getTodosOperadores()


        /// <summary>
        /// verifica se uma palavra é um operador-duplo, um tipo
        /// especial de operador que aparece duas vezes para complitude.
        /// </summary>
        /// <param name="token">palavra a ser investigada</param>
        /// <returns>retorna a produção do operador-duplo.</returns>
        public producao verificaSeEOperadorDuplo(string token)
        {
            try
            {
                foreach (producao p in this.producoes)
                {
                    if (p.tipo.Equals("[OPERADOR DUPLO]"))
                    {
                        if ((p.termosChave[0].Equals(token)) || (p.termosChave[1].Equals(token)))
                            return p;
                    } // if
                } // foreach
            } // try
            catch
            {
                return null;
            } // catch
            return null;
        } // verificaSeEOperadorDuplo()

        /// <summary>
        /// verifica se uma palavra é um operador.
        /// </summary>
        /// <param name="operadores">operadores da linguagem</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool verificaSeEOperador(string[] operadores, string token)
        {
            foreach (string op in operadores)
            {
                if (token.Equals(op))
                    return true;
            } // foreach
            return false;
        } // bool isOperador()

        /// <summary>
        /// verifica se a palavra [token] é um ID, ou seja um termo utilizado
        /// para idenficar um termo fora da linguagem, ou se a palavra for um
        /// número, também um [ID]  (tipo final) . retorna uma string  "[ID]"
        /// se for ID, ou "[NUMERO]" se a palavra é um número, ou uma string
        /// vazia "" se a palavra for reservada e não é um número.
        /// ATENÇÃO: SE A PALAVRA CONTER UMA PALAVRA-RESERVADA, por exemplo 
        /// [belse] (contém [else]), o método vai falhar. CORRIGIR ISTO.
        /// </summary>
        /// <param name="token">palavra a ser investigada.</param>
        /// <returns>retorna "[ID]" se a palavra for um ID, "[NUMERO]" se a
        /// palavra for um número, ou "" se a palavra for reservada (con-
        /// tendo termos-chave, e não é um número.</returns>
        public string verificaSeEID(string token)
        {
            List<string> lstTrmChvDaLng = this.todosTermosChaveDaLinguagem.ToList<string>();
            lstTrmChvDaLng.Add(" ");
            List<string> termoschavePresentes = stringUtilities.localizadorDeStrings.localizaStrings(token, lstTrmChvDaLng);
            List<string> tokens =stringUtilities.localizadorDeStrings.localizaComplementoStrings(token, termoschavePresentes.ToList<string>());
            if (termoschavePresentes.Count > 0)
                return ("[PALAVRA RESERVADA]");
            if ((tokens.Count==0) || (tokens[0].Equals(token)))
            {
                if (this.verificaSeENumero(token).Equals("[NUMERO]"))
                    return "[NUMERO]";
                else 
                    return ("[ID]");
            }
            return ("");
        } // bool verificaSeEID(string)

        
        /// <summary>
        /// verifica se uma palavra é um número puro.
        /// </summary>
        /// <param name="token">palavra a ser investigada se é número.</param>
        /// <returns>retorna uma string "[NUMERO]" se a palavra é um número,
        /// caso contrário, retorna uma string vazia "".</returns>
        public string verificaSeENumero(string token)
        {
            List<string> lstDigitos = new List<string>();
            lstDigitos.Add("0");
            lstDigitos.Add("1");
            lstDigitos.Add("2");
            lstDigitos.Add("3");
            lstDigitos.Add("4");
            lstDigitos.Add("5");
            lstDigitos.Add("6");
            lstDigitos.Add("7");
            lstDigitos.Add("8");
            lstDigitos.Add("9");
            lstDigitos.Add(".");
            string[] partes = token.Split(lstDigitos.ToArray(),
                                          StringSplitOptions.RemoveEmptyEntries);
           
            // testa se há mais de um dot (".").
            Func<char,bool> metodoAcharDots=dotcont;
            int dotString = token.Count<char>(metodoAcharDots);
            if (dotString > 1) return "";
            // se contém apenas digitos ou somente um dot no número, retorna "[NUMERO]".
            if (partes.Length == 0)
                return "[NUMERO]";
            return "";
        }

        /// <summary>
        /// verifica se o [token] é uma das palavras chave..
        /// </summary>
        /// <param name="token">termo, [ID], que está sendo verificado.</param>
        /// <returns>[true]se a palavra é um termo-chave da linguagem currente, [false] se não é termo-chave</returns>
        public bool verificaSEeTermoChave(string token)
        {

            string[] todosTermosChave = this.getTodosTermosChave();
            for (int x = 0; x < todosTermosChave.Length; x++)
            {
                if (token.Equals(todosTermosChave[x]))
                    return true;
            } // for x
            return false;
        } // verificaSeeTermoChave()

        /// <summary>
        /// unifica numa só string os termos-chave de uma lista.
        /// </summary>
        /// <param name="trmChv">lista de termos-chave.</param>
        /// <returns>retorna uma string contendo os termos-chave.</returns>
        public string unificaTermosChave(List<string> trmChv)
        {
            string s = "";
            foreach (string trm in trmChv)
                s += trm.Trim();
            return s;
        } // unificaTermosChave()

        /// <summary>
        /// retira termos vazios ou com espaços vazios, de um vetor de string.
        /// </summary>
        /// <param name="partes">vetor de string a ser processado.</param>
        /// <returns>retorna um vetor de strings não-vazias.</returns>
        private string[] retornaVetorSemVazios(string[] partes)
        {
            List<string> lstResult = new List<string>();
            for (int x = 0; x < partes.Length; x++)
            {
                partes[x] = partes[x].Trim();
                if (!partes[x].Equals(""))
                    lstResult.Add(partes[x]);
            } // foreach
            return lstResult.ToArray();
        } // retornaVetorSemVazios()

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

        
    } // classe UmaGramaticaComputacional
} // namespace parser
