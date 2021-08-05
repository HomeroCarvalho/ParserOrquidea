using System.Collections.Generic;
using System.Collections;
using System.Linq;
namespace parser.PROLOG
{

    public class Consultas
    {

        /// MÓDULO CONSULTAS
        /// FUNCIONALIDADES EXPORTADAS:
        /// 1- CONSULTA(TEXTO), ANALISA UM PREDICADO EXPRESSO EM TEXTO, COM REGISTROS DA BASE DE CONHECIMENTO.
        /// 2- CONSULTA(PREEDICADO): ANALISA UM PREDICADO, SE TEM REGISTROS QUE COMBINAM NA BASE DE CONHECIMENTO.
        /// 3- CONSULTA(REGRA): ANALISA UMA REGRA (CONJUNTO DE PREDICADOS COM VARIÁVEIS RELACIONADAS OU NÃO 
        ///    ENTRE SI), SE COMBINAM COM PREDICADOS NA BASE DE CONHECIMENTO.
        /// 4- CONSULTA LISTAS(): VERIFICA NA BASE DE CONHECIMENTO SE HÁ LISTAS QUE COMBINEM COM A LISTA DE ENTRADA.
        /// 5- PROGRAMA PARA LISTAS():  ANALISA UM PROGRAMA PROLOG, APLICADO A LISTAS.
        /// 
        /// NOTAS: OS MÉTODOS CONSULTA RETORNAM APENAS A PRIMEIRA SOLUÇÃO VIÁVEL, E NÃO TODAS SOLUÇÕES VIÁVEIS.
        /// PROPRIEDADES EXPORTADAS:
        /// 1- variaveis: CONTÉM O NOME E VALOR DE VARIÁVEIS, EXTRAÍDAS DE UMA REGRA, E UM CONJUNTO DE PREDICADOS SOLUÇÃO DE UMA CONSULTA(REGRA).
        /// 

        public List<string> variaveis { get; set; }

        private static Consultas umaConsulta = null;
        private static List<string> variaveisDoGoal = new List<string>();
        private static int[] indexVarPorMeta = null;
        public static Consultas Instance()
        {
            if (umaConsulta == null)
                umaConsulta = new Consultas();
            return umaConsulta;
        } // Instance()

        /// <sumary>
        /// construtor de consultas de predicados ou regras prolog.
        /// </summary>
        private Consultas()
        {
        }// Consultas()

        /// <summary>
        /// consulta uma lista (entrada, como [1 5 6]), procurando predicados que combinam com a lista.
        /// </summary>
        /// <param name="umaLista">lista de entrada a ser consultada.</param>
        /// <returns>retorna uma lista de predicados que combinam com a lista de entrada.</returns>
        public List<Predicado> ConsultaLista(ListaProlog umaLista)
        {

            List<Predicado> predicadosResultado = new List<Predicado>();
            foreach (Predicado umPredicadoDaBase in BaseDeConhecimento.Instance().Base)
            {
                // verfica se o predicado contém uma lista, e se o predicado da base tem o mesmo nome da predicado lista da entrada.
                if ((umPredicadoDaBase.GetType() == typeof(ListaProlog)) || (ListaProlog.HasList(umPredicadoDaBase)))
                {
                    if (umaLista.Match((ListaProlog)umPredicadoDaBase))
                    {
                        predicadosResultado.Add(umPredicadoDaBase);
                    }// if Match()
                } // if IsLista()
            } // foreach
            return predicadosResultado;
        } // ConsultaLista()

    
        public List<Predicado> ConsultaBackTracking(Regra umaRegra)
        {
            List<Predicado> predicadosEncontrado = new List<Predicado>();
            List<Predicado> predicadosRecursao = new List<Predicado>();
            Dictionary<string, string> variaveisAnteriormenteGuardadas = new Dictionary<string, string>();
            Consulta_BackTracking(umaRegra, predicadosEncontrado, predicadosRecursao);

            return predicadosEncontrado;
        }


        private bool Consulta_BackTracking(Regra regraConsulta, List<Predicado> predicatesFound, List<Predicado> predicadosRecursao)
        {


            Dictionary<string, string> variaveisAnteriormenteGuadadas = new Dictionary<string, string>();
            List<Predicado> predicatesBase = BaseDeConhecimento.Instance().Base;
            Dictionary<string, string> variaveisCopy = VarsCopy(variaveisAnteriormenteGuadadas); // faz uma copia do mapa de variaveis, pois pode-se ter que retroceder caso o match de predicados resulte em false.

            bool hasASolution = false;
            int indexPredicados = 0;
            while ((indexPredicados >= 0) && (indexPredicados < regraConsulta.PredicadosGoal.Count + 1)) 
            {

                if (indexPredicados == 0)
                {

                    for (int umPredicateBase = 0; umPredicateBase < predicatesBase.Count; umPredicateBase++)
                    {
                        Predicado predicadoBaseRaRegra = regraConsulta.PredicadoBase.Clone();
                        Predicado umPredicadoDaBaseDeConhecimento = (Predicado)predicatesBase[umPredicateBase].Clone(); /// investiga um predicado da base de conhecimento, se combina com um predicado goal da regra.


                        SubstituiVariaveis(umPredicadoDaBaseDeConhecimento, regraConsulta.PredicadoBase, variaveisAnteriormenteGuadadas);


                        if (regraConsulta.PredicadoBase.Match(umPredicadoDaBaseDeConhecimento))
                        {

                            if (predicatesFound.Find(k => k.EqualsPredicado(umPredicadoDaBaseDeConhecimento)) == null)
                                predicadosRecursao.Add(umPredicadoDaBaseDeConhecimento);

                            if (predicatesFound.Find(k => k.EqualsPredicado(umPredicadoDaBaseDeConhecimento)) == null)
                                predicatesFound.Add(umPredicadoDaBaseDeConhecimento);

                            indexPredicados++;
                            break;
                        }
                        else
                        {  // nao deu match, volta para as variaveis anterioremente guardadas.
                            variaveisAnteriormenteGuadadas = VarsCopy(variaveisCopy);
                            regraConsulta.PredicadoBase = predicadoBaseRaRegra.Clone(); // restaura o predicado base da regra, pois é preciso validar esse predicado, com as variaveis nao substituidas.
                            continue;
                        }

                       

                    } // for umPredicateBase
                } // if indexPredicados
                else
                if (indexPredicados > 0)
                {
                    hasASolution = false;
                    variaveisCopy = VarsCopy(variaveisAnteriormenteGuadadas);

                    Predicado predicadoGoalCurrentCopy = regraConsulta.PredicadosGoal[indexPredicados - 1].Clone();

                    for (int umPredicateBase = 0; umPredicateBase < predicatesBase.Count; umPredicateBase++)
                    {

                        Predicado predicadoDaBaseDeConhecimento = (Predicado)predicatesBase[umPredicateBase].Clone(); /// investiga um predicado da base de conhecimento, se combina com um predicado goal da regra.
                       

                        // substitui as variaveis do predicado goal da regra, pelos valores do predicado da base de conhecimento.
                        SubstituiVariaveis(predicatesBase[umPredicateBase], regraConsulta.PredicadosGoal[indexPredicados - 1], variaveisAnteriormenteGuadadas);

                        if (predicadoDaBaseDeConhecimento.Match(regraConsulta.PredicadosGoal[indexPredicados - 1]))
                        {

                            if (predicatesFound.Find(k => k.EqualsPredicado(predicadoDaBaseDeConhecimento)) == null)
                                predicadosRecursao.Add(predicadoDaBaseDeConhecimento);

                            if (predicatesFound.Find(k => k.EqualsPredicado(predicadoDaBaseDeConhecimento)) == null)
                                predicatesFound.Add(predicadoDaBaseDeConhecimento);

                            indexPredicados++;
                            hasASolution = true;
                            break; // pára a malha de [umPredicateBse], indo para a malha [indexPredicados]
                        }
                        else
                        {
                           
                            variaveisAnteriormenteGuadadas = VarsCopy(variaveisCopy); // restaura modificacoes nas variaveis, pois o predicado base que formaou os valores da veriavei, esse predicado ja nao eh mais valido.
                            continue; // passa para o proximo predicado da base de cohecimento, pois nao houve um match entre o predicado anterior e o predicado da regra.
                        }

                    } // for umPredicateBase
                   
                    if (!hasASolution)
                    {
                        regraConsulta.PredicadosGoal[indexPredicados - 1] = predicadoGoalCurrentCopy.Clone();
                        indexPredicados--;
                        if (indexPredicados == 0) 
                            return false;
                        continue;
                    }


                } // if indexPredicados
            }

            return true;
        }

        private Dictionary<string, string> VarsCopy(Dictionary<string, string> variaveis)
        {
            Dictionary<string, string>.KeyCollection keys= variaveis.Keys;
            Dictionary<string, string>.ValueCollection values = variaveis.Values;

            Dictionary<string, string> dic_clone = new Dictionary<string, string>();
            for (int x = 0; x < keys.Count; x++)
                dic_clone[keys.ElementAt<string>(x)] = values.ElementAt<string>(x);

            return dic_clone;
            
        }
        public static void SubstituiVariaveis(Predicado predicadoDaBaseDeConhecimento, Predicado predicadoRegra, Dictionary<string, string> variaveisAnteriormenteGuardadas)
        {

            // verifica se o predicado da base de conhecimento tem o mesmo numero de atomos e igual nome, pois há uma relação entre variaveis a serem subsituidas no predicado, em predicados compativeis.
            if ((predicadoRegra.Nome.Equals(predicadoDaBaseDeConhecimento.Nome)) && (predicadoRegra.GetAtomos().Count == predicadoDaBaseDeConhecimento.GetAtomos().Count))
            {
                List<string> variaveis, valores;

                ExtractVars(predicadoDaBaseDeConhecimento, predicadoRegra, out variaveis, out valores);

                for (int umaVar = 0; umaVar < variaveis.Count; umaVar++)
                {
                    string nomeVariavel = variaveis[umaVar];
                    string valorVariavel = valores[umaVar];
                    string valorVariavelAnterior = null;

                    /// 1- verifica se variaveis[umaVar] está presente ou nao no mapa de (nome,valor) de variaveis.
                    ///         2- se está presente, faz a substituicao no mapa colocando o valor como nome do atomo do predicado.
                    ///         3- se a variavel  não está presente: 
                    ///                     3.1- se o valores[umaVar] não está presente no mapa das variaveis faz a substituicao 
                    ///                     3.2-  continue;
                    ///                     
                    if ((variaveisAnteriormenteGuardadas.ContainsKey(nomeVariavel)) && (variaveisAnteriormenteGuardadas.ContainsValue(valorVariavel))) 
                    {
                        SubtituiPorUmValorEUmaVariavel(nomeVariavel, valorVariavel, predicadoRegra);
                        continue;
                    }

                    if ((variaveisAnteriormenteGuardadas.ContainsKey(nomeVariavel)) && (!variaveisAnteriormenteGuardadas.ContainsValue(valorVariavel))) 
                    {
                        variaveisAnteriormenteGuardadas[nomeVariavel] = valorVariavel; // caso em que o valor da variavel nao esta no mapa.

                        SubtituiPorUmValorEUmaVariavel(nomeVariavel, valorVariavel, predicadoRegra);
                        continue;

                    }

                    if ((!variaveisAnteriormenteGuardadas.ContainsKey(nomeVariavel)) && ((variaveisAnteriormenteGuardadas.ContainsValue(valorVariavel))))
                    {
                     
                        SubtituiPorUmValorEUmaVariavel(nomeVariavel, valorVariavel, predicadoRegra);
                        continue;
                    }

                    if ((!variaveisAnteriormenteGuardadas.ContainsKey(nomeVariavel)) && (variaveisAnteriormenteGuardadas.ContainsValue(valorVariavel))) 
                    {
                        SubtituiPorUmValorEUmaVariavel(nomeVariavel, valorVariavelAnterior, predicadoRegra);
                        continue;
                    }


                    if ((!variaveisAnteriormenteGuardadas.ContainsKey(nomeVariavel)) && (!variaveisAnteriormenteGuardadas.ContainsValue(valorVariavel)))
                    {
                        variaveisAnteriormenteGuardadas[nomeVariavel] = valorVariavel; // caso em que o valor da variavel nao esta no mapa.

                        SubtituiPorUmValorEUmaVariavel(nomeVariavel, valorVariavel, predicadoRegra);
                        continue;

                    }


                }

                return;
            } // if

            if (!predicadoRegra.Nome.Equals(predicadoDaBaseDeConhecimento.Nome))
            {
                for (int atomo = 0; atomo < predicadoRegra.GetAtomos().Count; atomo++)
                {
                    string nomeVariavel = predicadoRegra.GetAtomos()[atomo];

                    if (Predicado.IsVariavel(nomeVariavel))
                    {
                        string valorVariavelGuardada = null;
                        variaveisAnteriormenteGuardadas.TryGetValue(nomeVariavel, out valorVariavelGuardada);
                        if (valorVariavelGuardada != null)
                        {
                            SubtituiPorUmValorEUmaVariavel(nomeVariavel, valorVariavelGuardada, predicadoRegra);
                        } // if
                        else
                        {

                        } // else
                    }
                }
                return;
            } // if

        }

        private static void ExtractVars(Predicado predicadoDaBaseDeConhecimento, Predicado predicadoRegra, out List<string> variaveis, out List<string> valores)
        {
            variaveis = predicadoRegra.GetAtomos();
            valores = predicadoDaBaseDeConhecimento.GetAtomos();

            for (int umaVar = 0; umaVar < variaveis.Count; umaVar++)
            {
                if (!Predicado.IsVariavel(variaveis[umaVar])) // verifica se o atomo currente é uma variavel, se não for, remove os atomos variaveis e valores currentes da lista de variaveis.
                {
                    variaveis.RemoveAt(umaVar);
                    valores.RemoveAt(umaVar);
                    umaVar--;
                }
            }

        }

        private static void SubtituiPorUmValorEUmaVariavel(string nomeVariavel, string valorVariavel, Predicado predicado)
        {
            for (int umAtom = 0; umAtom < predicado.GetAtomos().Count; umAtom++)
            {
                string nome_variavel = predicado.GetAtomos().Find(k => k.Equals(nomeVariavel));
                if (nome_variavel != null)
                    if (predicado.GetAtomos()[umAtom].Equals(nomeVariavel))
                        predicado.SetVariavel(valorVariavel, umAtom);
            }
        }

 

        /// <summary>
        /// executa um algoritmo recursivo aplicado a listas.
        /// </summary>
        /// <param name="umaRegraDeListas">uma Regra contendo definições lista.</param>
        /// <param name="listaEntrada">lista de entrada a ser transformada pelo algoritmo.</param>
        /// <returns></returns>
        public ListaProlog ProgramaParaListas(Regra umaRegraDeListas, ListaProlog listaEntrada, int indexPredicado)
        {
            if (indexPredicado == umaRegraDeListas.PredicadosGoal.Count)
            {
                return ListaProlog.AplicaUmaListaFuncao((ListaProlog)umaRegraDeListas.PredicadoBase, listaEntrada);
            } // if 
            foreach (Predicado umaRegraOuLista in BaseDeConhecimento.Instance().Base)
            {

                if ((umaRegraOuLista.GetType() == typeof(Regra)) &&
                       ((Regra)umaRegraOuLista).PredicadoBase.Nome == umaRegraDeListas.Nome)
                {
                    ProgramaParaListas((Regra)umaRegraOuLista,
                        ListaProlog.AplicaUmaListaFuncao((ListaProlog)((Regra)umaRegraOuLista).PredicadosGoal[indexPredicado],
                        listaEntrada), ++indexPredicado);
                } // if
                else
                if ((umaRegraOuLista.GetType() == typeof(ListaProlog)) && (umaRegraOuLista.Nome == listaEntrada.Nome))
                {
                    return ListaProlog.AplicaUmaListaFuncao((ListaProlog)umaRegraOuLista, listaEntrada);
                } // if
            } // foreach()
            return null;
        } // ExecuteListProgram()

        // um predicado que contém uma instrução cut [!] é uma condição de parada para processamnto de listas por recursão.
     
        private bool HasCutInstruction(Predicado predicado)
        {
            for (int atomo = 0; atomo < predicado.GetAtomos().Count; atomo++)
            {
                if (predicado.GetAtomos()[atomo].Equals("!"))
                    return true;
            } // for atomo
            return false;
        } // HasCutInstruction()

        /// <summary>
        /// Valida variáveis para soluções como: homem(X):- mortal(X), mamifero(X).
        /// </summary>
        /// <param name="regraMolde">regra que contém as variáveis a serem analisadas na consulta.</param>
        /// <param name="predicadosMetaSolucao">um conjunto de predicados, que compõe uma solução.</param>
        /// <returns>[true] se todos valores para as variáveis (como X, em homem(X):- mortal(X), mamifero(X)) forem iguais.</returns>
        public static bool ValidaVariaveis(Regra regraMolde, List<Predicado> predicadosMetaSolucao)
        {
            for (int umaVariavelGoal = 0; umaVariavelGoal < variaveisDoGoal.Count; umaVariavelGoal++)
            {
                // compara os valores da variável currente, se for diferentes, retorna [false].
                for (int metaIni = 0; metaIni < predicadosMetaSolucao.Count; metaIni++)
                {
                    for (int metaFini = metaIni + 1; metaFini < predicadosMetaSolucao.Count; metaFini++)
                    {
                        if (indexVarPorMeta[umaVariavelGoal] != -1)
                        {
                            if (predicadosMetaSolucao[metaIni].GetAtomos()[indexVarPorMeta[umaVariavelGoal]] !=
                                predicadosMetaSolucao[metaFini].GetAtomos()[indexVarPorMeta[umaVariavelGoal]])
                                return false;
                        } // for metaFini
                    } // for metaIni
                } //for metaIni
            } // umaVariavelGoal
            return true;
        } //ValidaVariaveis()

        private static void PreprocessamentoVariaveis(Regra regraMolde, out List<string> variaveisDosGoal, out int[] indexVarPorMeta)
        {
            Regra regraMoldeValidacao = new Regra(regraMolde);
            regraMoldeValidacao.PredicadosGoal.Add(regraMoldeValidacao.PredicadoBase);

            // cada item em predicadosSolucao é um predicado meta encontrado...
            variaveisDosGoal = new List<string>();
            List<string> variaveisBase = new List<string>();
            //  extrai as variaveis presentes no predicado base e predicados meta da regra,
            //  guardando além do nome, também a sua posição.
            ExtraiVariaveis(regraMoldeValidacao, variaveisDosGoal, variaveisBase);
            indexVarPorMeta = new int[variaveisDosGoal.Count];
            for (int umaVariavel = 0; umaVariavel < variaveisDosGoal.Count; umaVariavel++)
            {
                // acha o índice da variável curremte, para cada predicado meta.
                for (int meta = 0; meta < variaveisDosGoal.Count; meta++)
                {
                    indexVarPorMeta[meta] = GetIndiceVariavel(regraMoldeValidacao.PredicadosGoal[meta], variaveisDosGoal[umaVariavel]);
                } // for meta
            } // for umaVariavel
        }

        private static int GetIndiceVariavel(Predicado predicado, string variavel)
        {
            for (int atomo = 0; atomo < predicado.GetAtomos().Count; atomo++)
            {
                if (predicado.GetAtomos()[atomo] == variavel)
                    return atomo;
            } //for atomo
            return -1;
        } // GetIndiceVariavel()

        private static void ExtraiVariaveis(Regra regraFormula,
                                           List<string> variaveisGoal, List<string> variaveisBase)
        {

            Predicado predicado = null;
            for (int meta = 0; meta < regraFormula.PredicadosGoal.Count; meta++)
            {
                // extrai o nome e posição dos predicados meta da regra.
                predicado = regraFormula.PredicadosGoal[meta];
                for (int atomo = 0; atomo < predicado.GetAtomos().Count; atomo++)
                {

                    string umaPossivelVariavel = predicado.GetAtomos()[atomo];
                    if ((Predicado.IsVariavel(umaPossivelVariavel)) && (variaveisGoal.IndexOf(umaPossivelVariavel)==-1))
                    {
                        variaveisGoal.Add(umaPossivelVariavel);
                    } // if

                } // for atomo
            } // for meta
            for (int atomo = 0; atomo < regraFormula.PredicadoBase.GetAtomos().Count; atomo++)
            {
                string umaPossivelVariavel = regraFormula.PredicadoBase.GetAtomos()[atomo];
                if ((Predicado.IsVariavel(umaPossivelVariavel)) && (variaveisBase.IndexOf(umaPossivelVariavel)==-1))
                {
                    variaveisBase.Add(umaPossivelVariavel);
                }// for 
            } //for atomo

        } // ExtraiVariaveis
    }// class Consultas
} // namespace
