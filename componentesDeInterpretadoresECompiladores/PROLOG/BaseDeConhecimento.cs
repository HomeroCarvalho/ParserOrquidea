using System.Collections.Generic;
using System.Collections;
using System.Linq;
namespace parser.PROLOG
{
    public class BaseDeConhecimento
    {
        /// <summary>
        /// base de conhecimento, podendo ser um fato (um único predicado) ou uma regra (vários predicados (submetas) e um predicado base.
        /// </summary>
        public List<Predicado> Base { get; set; }

        /// <summary>
        /// Instância Singleton de um objeto único da classe.
        /// </summary>
        private static BaseDeConhecimento BaseSingleton = null;

        private BaseDeConhecimento()
        {
            this.Base = new List<Predicado>();
        } // BaseDeConhecimento()

        /// <summary>
        /// obtém o objeto único da Base De Conhecimento.
        /// </summary>
        /// <returns></returns>
        public static BaseDeConhecimento Instance()
        {
            if (BaseSingleton == null)
            {
                BaseSingleton = new BaseDeConhecimento
                {
                    Base = new List<Predicado>()
                };
            } // if
            return BaseSingleton;
        } // Instance()

        public List<Predicado> ClonePredicates()
        {
            return this.Base.ToList<Predicado>();
        }

        public void AddPredicates(List<Predicado> predicados)
        {
            if ((predicados == null) || (predicados.Count > 0))
                this.Base.AddRange(predicados);
        }
        public void AddPredicates(string textWithPredicates)
        {
            List<Predicado> predicados = ParserPROLOG.GetPredicados(textWithPredicates);
            this.AddPredicates(predicados);
        }
    } // class BaseDeConhecimento
} // namespace
