using System.Collections.Generic;
using System.Collections;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Security.Principal;
using System;
using System.IO;
using System.Reflection;

using parser.LISP;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.SqlServer.Server;
using Util;

namespace parser
{

    public class TablelaDeValores
    {


        /*
         * 
        // MÉTODOS DA CLASSE TabelaDeValores

        /// PARA FUNÇÕES:
        public Funcao GetFuncao(string nomeFuncao, string tipoRetornoFuncao, escopo)
        public Funcao GetFuncao(string nomeFuncao)
        public bool RegistraFuncao(Funcao funcao)
        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        public Funcao IsFunction(string nomeFuncao, Escopo escopo)
        
 
        // PARA CLASSES        
        public List<Classe> GetClasses()
        public void RegistraClasse(Classe umaClasse)
        public propriedade GetPropriedade(string nomeClasse, string nomePropriedade)
        public bool ValidaTipo(string tipo)
    
        
        // PARA OBJETOS
        public Objeto GetObjeto(string nomeObjeto)
        public void RegistraObjeto(Objeto objeto)
        public void RemoveObjeto(string nomeObjeto)
        private propriedade ObtemPropriedadeDeUmObjeto(string tipoObjeto, string propriedadeDoObjeto)
        private Funcao ObtemMetodoDeUmObjeto(string tipoObjeto, string metodoDoObjeto)
        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
       
        
        // PARA VETORES:
            public GetVetor(string nome, Escopo escopo)
            public AddVetor(string acessor, string nome,string tipo, int[]dims, Escopo escopo, bool isStatic )
        
        // PARA OPERADORES:
        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopoCurrente)
        public static int VerificaSeEhOperador(string operador, string tipoDeOperando)
       
       
        // PARA PROPRIEDADES:
        private List<propriedade> ObtemPropriedadesEncadeadas(List<string> tokens, Escopo escopo, int indiceInicio)

        // PARA EXPRESSOES:       
        private string ObtemOTipoDaExpressao(List<string> tokens, Escopo escopo)
        public static int VerificaSeEhTermoChave(string tokenChave, List<string> termosChave)
 
         
        // PROPRIEDADES PUBLICAS:
        public List<Operador> Operadores = new List<Operador>();
         
         */



        private List<Classe> Classes = new List<Classe>();
        private List<Operador> operadores = new List<Operador>();
      

        internal List<Expressao> expressoes = new List<Expressao>(); // contém as expressões validadas no escopo currente.
        private List<Funcao> Funcoes = new List<Funcao>(); // contém as funções do escopo currente.

       
        private List<Vetor> VariaveisVetor = new List<Vetor>(); // variaveis vetor.
        


        private List<Objeto> objetos = new List<Objeto>(); // contém uma lista de objetos instanciados.
        
        private List<string> codigo { get; set; }


        public TablelaDeValores Clone()
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(this.codigo);
            if (tabelaClone != null)
            {
                tabelaClone.Classes = this.Classes.ToList<Classe>();
                tabelaClone.operadores = this.operadores.ToList<Operador>();
                tabelaClone.expressoes = this.expressoes.ToList<Expressao>();
                tabelaClone.Funcoes = this.Funcoes.ToList<Funcao>();
                tabelaClone.VariaveisVetor = this.VariaveisVetor.ToList<Vetor>();
                tabelaClone.objetos = this.objetos.ToList<Objeto>();
                tabelaClone.codigo = this.codigo.ToList<string>();
            }
            return tabelaClone;
        }

        private static LinguagemOrquidea lng = new LinguagemOrquidea();
        public TablelaDeValores(List<string> _codigo)
        {
            lng = new LinguagemOrquidea();
            if ((_codigo != null) && (_codigo.Count > 0))
                this.codigo = _codigo.ToList<string>();
            else
                this.codigo = new List<string>();
        } //TabelaDeValores()

        
        public List<Expressao> GetExpressoes()
        {
            return this.expressoes;
        } // GetExpressoes()

        public List<Operador> GetOperadores()
        {
            return this.operadores;
        }
        public List<Objeto> GetObjetos()
        {
            return this.objetos;
        }

        public List<Funcao> GetFuncoes()
        {
            return Funcoes;
        } // GetFuncoes()

        /// <summary>
        /// obtém a função nesta tabela de valores, e se não encontrar, obtém a função no repositorio de classes orquidea.
        /// </summary>
        public Funcao GetFuncao(string nomeFuncao, string classeDaFuncao, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                  return FindFuncao(classeDaFuncao, nomeFuncao, escopo);

            Funcao fuctionFound = FindFuncao(classeDaFuncao, nomeFuncao, escopo);
            if (fuctionFound != null) 
                return fuctionFound;
            else
                return GetFuncao(nomeFuncao, classeDaFuncao, escopo.escopoPai);
        } // GetFuncao()



        private  Funcao FindFuncao(string classeDaFuncao, string nomeFuncao, Escopo escopo)
        {
        
            Funcao umaFuncaoDasTabelas = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (umaFuncaoDasTabelas != null)
                return escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);

            Funcao umaFuncaoDoRepositorio = RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
            if (umaFuncaoDoRepositorio != null)
                return umaFuncaoDoRepositorio;
            return null;
        }


        /// <summary>
        /// obtem funcoes com o nome parâmetro, não importando a classe em que a função foi definida.
        /// </summary>
        public List<Funcao> GetFuncao(string nomeFuncao)        {

            List<Funcao> funcoesDaTabelaComMesmoNome = this.Funcoes.FindAll(k => k.nome.Equals(nomeFuncao));

            if (funcoesDaTabelaComMesmoNome != null)
                return funcoesDaTabelaComMesmoNome;

            List<Funcao> funcoesDoRepositorioDeClassesComMesmoNome = new List<Funcao>();
            List<Classe> lstClasses = RepositorioDeClassesOO.Instance().classesRegistradas;
            foreach (Classe umaClasse in lstClasses)
            {
                List<Funcao> lstFuncoes = umaClasse.GetMetodos().FindAll(k => k.nome.Equals(nomeFuncao));
               if (lstFuncoes!=null)
                {
                    funcoesDoRepositorioDeClassesComMesmoNome.AddRange(lstFuncoes);
                } // if
            } // foreach
            return funcoesDoRepositorioDeClassesComMesmoNome;
        } // GetFuncao()

      
        public List<Vetor> GetVetores()
        {
            return VariaveisVetor;
        }
        public List<Classe> GetClasses()
        {
           return Classes;
        }

        public Classe GetClasse(string nomeDaClasse, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return escopo.tabela.Classes.Find(k => k.nome.Equals(nomeDaClasse));
            else
            {
                Classe classe = escopo.tabela.Classes.Find(k => k.GetNome().Equals(nomeDaClasse));
                if (classe != null)
                    return classe;

                if (classe == null)
                    return GetClasse(nomeDaClasse, escopo.escopoPai);
            }
            return null;
        }


        public static TablelaDeValores Clone(TablelaDeValores tabela)
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(tabela.codigo);
            if (tabela.GetClasses().Count > 0)
                tabelaClone.GetClasses().AddRange(tabela.GetClasses().ToList<Classe>());
            
            if (tabela.GetExpressoes().Count > 0)
                tabelaClone.GetExpressoes().AddRange(tabela.GetExpressoes().ToList<Expressao>());

            if (tabela.GetFuncoes().Count > 0)
                tabelaClone.GetFuncoes().AddRange(tabela.GetFuncoes().ToList<Funcao>());

            if (tabela.GetObjetos().Count > 0)
                tabelaClone.GetObjetos().AddRange(tabela.GetObjetos().ToList<Objeto>());

            if (tabela.GetOperadores().Count > 0)
                tabelaClone.GetOperadores().AddRange(tabela.GetOperadores().ToList<Operador>());

            if (tabela.GetVetores().Count > 0)
                tabelaClone.GetVetores().AddRange(tabela.GetVetores().ToList<Vetor>());


            return tabelaClone;
            
        }


        public void RegistraClasse(Classe umaClasse)
        {
            Classes.Add(umaClasse);
            this.operadores.AddRange(umaClasse.GetOperadores());
        } // RegistraClasse()
        public Objeto GetObjeto(string nomeObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objetoRetorno = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto);
            if (objetoRetorno != null)
                return objetoRetorno;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetObjeto(nomeObjeto, escopo.escopoPai);

            return null;
        } // GetObjeto().

     

        public Objeto GetPropriedade(string nomeClasse, string nomePropriedade)
        {
            // tenta localizar a classe da propriedade.
            Classe umaClasse = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);

            // se não localizar, retorna null.
            if (umaClasse == null)
                return null;
            
            // tenta localizar a propriedade da classe encontrada.
            int indexPropriedade = umaClasse.GetPropriedades().FindIndex(k => k.GetNome()== nomePropriedade.Trim(' '));
            // se não localizar, retorna null.
            if (indexPropriedade == -1)
            {
                indexPropriedade = umaClasse.GetPropriedades().FindIndex(k => k.GetNome() == "static." + nomePropriedade.Trim(' '));
                if (indexPropriedade == -1)
                    return null;
                else
                    return umaClasse.GetPropriedades().Find(k => k.GetNome() == "static." + nomePropriedade.Trim(' '));
            }
            // retorna a propriedade encontrada.
            return umaClasse.GetPropriedades()[indexPropriedade];

        } // GetPropriedade()
        public void RegistraObjeto(Objeto objeto)
        {
            this.objetos.Add(objeto);
        } // RegistraObjeto().


        // remove a ultima instancia do objeto com o nome de entrada.
        public void RemoveObjeto(string nomeObjeto)
        {

            int index = this.objetos.FindLastIndex(k => k.GetNome() == nomeObjeto);
            if (index != -1)
                this.objetos.RemoveAt(index);

        } // RemoveObjeto()
        public bool RegistraFuncao(Funcao funcao)
        {
            this.Funcoes.Add(funcao);
            return true;
        }
     

        public bool ValidaTipo(string tipo)
        {
            for (int x = 0; x < this.Classes.Count; x++)
                if (this.Classes[x].GetNome() == tipo)
                    return true;
            return false;
        } // ValidaTipo()


        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopo)
        {

            Classe classeDoOperador = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoOPerador);
            if (classeDoOperador != null)
            {
                if (classeDoOperador.GetOperadores().Find(k => k.nome.Equals(nomeOperador)) != null)
                    return true;
            }
            if (escopo.tabela.GetOperadores().Find(k => k.nome.Equals(nomeOperador) && (k.tipoRetorno.Equals(tipoDoOPerador))) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaOperador(nomeOperador, tipoDoOPerador, escopo.escopoPai);
            return false;
        } // ValidaOperador()
    
        
        /// <summary>
        ///  obtém uma propriedade, que é de uma sequencia de propriedades aninhadas.
        /// </summary>
        internal Objeto ValidaPropriedadesAninhadas(string classeDaPropriedade, string nomeDaPropriedade)
        {
            Classe clsObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeDaPropriedade);
            if (clsObjeto == null)
                return null;

            // "class A { int varA;} class B {A varB;}" };
            // propriedade prop = escopo.tabela.ObtemPropriedadeAninhada("B", "varB.varA");

            List<Objeto> props = new List<Objeto>();

            string[] proppriedadesAninhadas = nomeDaPropriedade.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < proppriedadesAninhadas.Length; x++)
            {
                string nomeLongo = clsObjeto.GetNome() + "." + proppriedadesAninhadas[x];

                Objeto proprSendoVerificada = clsObjeto.GetPropriedades().Find(k => k.GetNome().Equals(nomeLongo));
                if (proprSendoVerificada == null)
                    return null;
                props.Add(proprSendoVerificada);

                clsObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(props[props.Count - 1].GetTipo());
            } // for x
            if (props.Count < 0)
                return null;
            return props[props.Count - 1];
        } //GetPropriedadeEncadeadaDeUmaExpressao()


        public bool IsClasse(string nomeClasse, Escopo escopo)
        {
            if (escopo == null)
                return false;
            int indexClasse = escopo.tabela.GetClasses().FindIndex(k => k.GetNome() == nomeClasse);
            if (indexClasse != -1)
                return true;
            if (escopo.ID == Escopo.tipoEscopo.escopoNormal)
                return IsClasse(nomeClasse, escopo.escopoPai);
            return false;

        }  // IsClasse()

    
 

    

        public void AddObjetoVetor(string acessor, string nome, string tipo, int[] dims, Escopo escopo, bool isStatic)
        {
            Vetor v = new Vetor(acessor, nome, tipo, escopo, dims);
            v.SetAcessor(acessor);
            v.isStatic = isStatic;
            escopo.tabela.VariaveisVetor.Add(v);
        } // RegistraVetor()

        public void AddObjeto(string acessor,string nome, string tipo, object valor, Escopo escopo)
        {
            Objeto objeto = new Objeto(acessor, tipo, nome, valor);
            this.objetos.Add(objeto);
        }

   
        public Vetor GetObjetoVetor(string nomeVariavel, Escopo escopo, params int[] indices)
        {
            if (escopo == null)
                return null;
            Vetor v = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
            {
                return GetElementoVetor(escopo, indices, ref v);
            } // if
            Vetor vEstatica = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == "static." + nomeVariavel);
            if (vEstatica != null)
            {
                return GetElementoVetor(escopo, indices, ref v);
            }

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetVetor(nomeVariavel, escopo.escopoPai);
            return null;
        } // GetObjetoVetor()

        public Vetor GetVetor(string nomeVariavel, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Vetor v = this.GetVetores().Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
                return v;
            else
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return null;
            else
                return GetVetor(nomeVariavel, escopo.escopoPai);
        } // GetObjetoVetor()


        private static Vetor GetElementoVetor(Escopo escopo, int[] indices, ref Vetor v)
        {
            Vetor vt_result = new Vetor(v.GetAcessor(), v.GetNome(), v.GetTiposElemento(), escopo, v.dimensoes);
            for (int index = 0; index < indices.Length; index++)
                vt_result = vt_result.tailVetor[indices[index]];
            return vt_result;
        }

        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Funcao funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao.tipoReturn;

            Funcao umaFuncaoDoRepositorioDeClasses = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasseDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
            if (umaFuncaoDoRepositorioDeClasses != null)
                return umaFuncaoDoRepositorioDeClasses.tipoReturn;

            if (escopo.escopoPai.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetTypeFunction(nomeClasseDaFuncao, nomeFuncao, escopo.escopoPai);
            return null;
        }

      

        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objeto = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto);
            if (objeto != null)
                return objeto;
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsObjetoRegistrado(nomeObjeto, escopo.escopoPai);
            return null;
        } // IsObjectRegistrade()



        public bool IsFunction(Escopo escopo, string nameFuncion)
        {
            return (IsFunction(nameFuncion, escopo) != null);
        }


        /// <summary>
        /// verifica se um token é nome de uma função.
        /// </summary>
        public Funcao IsFunction(string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Funcao funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsFunction(nomeFuncao, escopo.escopoPai);
            return null;
        } // IsFunction()

       

        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
        {

            if (escopo == null)
                return false;
            if (escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaObjeto(nomeObjeto, escopo.escopoPai);
            return false;
        } // ValidaVariavel()

        public bool ValidaTipoObjeto( string tipoVariavel, Escopo escopo)
        {
            if (escopo == null)
                return false;
            foreach (Classe umaClasse in escopo.tabela.Classes)
            {
                if (umaClasse.GetNome() == tipoVariavel)
                    return true;
            }// foreach

            
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return (ValidaTipoObjeto(tipoVariavel, escopo.escopoPai));
            return false;
        } // ValidaTipoVariavel()

        

    } // class TabelaDeValores

    public class Vetor: Objeto
    {
        // o valor de um elemento do vetor é o valor do Objeto associado.
        public string nome;
        private string tipo;

        public List<Vetor> tailVetor { get; set; }

        public int[] dimensoes;


        public string GetTiposElemento()
        {
            return tipo;
        }


        public Vetor()
        {
            this.tailVetor = new List<Vetor>();
            this.nome = "";
            this.tipo = "";
           
            this.dimensoes = new int[1]; 
        }


        public Vetor(string acessor, string nome, string tipoElementoVetor, Escopo escopo, params int[] dims) : base(acessor, "Vetor", nome, null)
        {
            Init(nome, tipoElementoVetor, dims);

            for (int x = 0; x < dims.Length; x++) // inicializa as variaveis vetor de elementos, para evitar recursão inifinita.
            {
                this.tailVetor.Add(new Vetor());
                this.tailVetor[tailVetor.Count - 1].Init(nome, tipoElementoVetor, dims);
            }

        }

        private void Init(string nomeVariavel, string tipoElemento, int[] dims)
        {
            this.tipo = tipoElemento;
            this.nome = nomeVariavel;
            this.dimensoes = dims;

            this.tailVetor = new List<Vetor>();

            if (this.dimensoes == null)
                this.dimensoes = new int[1];
        }



        /// <summary>
        /// seta o elemento com os indices matriciais de entrada. 
        /// Util para modificar um elemento com dimensões bem definidas: M[1,5,8] um vetor, e queremos acessar
        /// a variavel: m[0,0,3].
        /// </summary>
        public void SetElementoPorOffset(List<Expressao> exprssIndices, object newValue, Escopo escopo)
        {
            EvalExpression eval = new EvalExpression();
            List<int> indices = new List<int>();
            for (int x = 0; x < exprssIndices.Count; x++)
                indices.Add(int.Parse(eval.EvalPosOrdem(exprssIndices[x], escopo).ToString()));

            int indexOffet = this.BuildIndex(indices.ToArray());
            this.tailVetor[indexOffet].SetValor(newValue);
            
        }

        /// <summary>
        /// seta elemento com elementos vetor dentro de vetores, como: [[1,5],2,6,8,[1,3,5]].
        /// </summary>
        public void SetElementoAninhado(object newValue, Escopo escopo, params Expressao[] exprssoesIndices)
        {
            List<int> indices = new List<int>();
            EvalExpression eval = new EvalExpression();
            for (int k = 0; k< exprssoesIndices.Length; k++)
                indices.Add(int.Parse(eval.EvalPosOrdem(exprssoesIndices[k], escopo).ToString()));
               
            Vetor v = this;
            for (int x = 0; x < indices.Count - 1; x++)
                if (v.tailVetor[indices[x]].GetType() == typeof(Vetor))
                    v = v.tailVetor[indices[x]]; // o elemento do vetor eh outro vetor.
                else
                {
                    v.tailVetor[indices[x]].SetValor(newValue); // o elemento eh um objeto, não um Vetor.
                    break;
                }

            v = v.tailVetor[indices[indices.Count - 1]];
            v.SetValor(newValue);

        }


        /// <summary>
        /// constroi um indice de acessso de vetores com varias dimensoes. Eh um offset de
        /// endereço onde esta localizado a variavel que queremos acessar, dentro da variavel vetor.
        /// Util quando temos um vetor como: vetor[4,5,7], e queremos o elemento vetor[1,2,5].
        /// </summary>
        public int BuildIndex(int[] indices)
        {
            if (indices.Length != this.dimensoes.Length)
                return -1;

            int indiceTotal = 0;
            int dimsAtual = 1;
            for (int x = 0; x < this.dimensoes.Length; x++)
            {
                dimsAtual = dimsAtual * this.dimensoes[x];
                indiceTotal += dimsAtual * indices[x];
            }
            return indiceTotal;
        }


        public object GetElemento(Escopo escopo, params int[] indices)
        {
            Vetor vt = new Vetor(this.GetAcessor(), this.GetNome(), this.GetTiposElemento(), escopo, this.dimensoes);
            for (int x = 0; x < indices.Length; x++)
                vt = vt.tailVetor[indices[x]];
            return vt.GetValor();
        }


        public override string ToString()
        {
            string str = this.GetTiposElemento() + " " + this.nome + " [ ";

            int x = 0;
            for (x = 0; x < this.tailVetor.Count - 1; x++)
                str += this.dimensoes[x] + ",";

            str += this.dimensoes[x] + " ]";


            return str;
        }
    }
} // namespace
