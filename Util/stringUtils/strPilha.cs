using System;
using System.Collections.Generic;
using System.Linq;
namespace parser
{   /// <summary>
    /// Classe de estrutura de dados pilha genérica.
    /// </summary>
    /// <typeparam name="T">tipo genérico</typeparam>
    public class Pilha<T>
    {

        public int lenghtPilha
        {
            get
            {
                return this.stack.Count;
            }
        }
        /// <summary>
        /// guarda os elementos da pilha.
        /// </summary>
        private List<T> stack = new List<T>();

        /// <summary>
        /// nome da pilha, útil para identificação do objeto em caso de erros.
        /// </summary>
        private string nameStack;

        public List<T> GetElementos()
        {
            return stack.ToList<T>();
        }
        public void Esvazia()
        {
            this.stack = new List<T>();
        }
        /// <summary>
        /// construtor.
        /// </summary>
        /// <param name="nomePilha">nome da pilha.</param>
        public Pilha(string nomePilha)
        {
            this.stack = new List<T>();
            this.nameStack = nomePilha;
        }
        /// <summary>
        /// empilha o elemento [op] no topo da pilha.
        /// </summary>
        /// <param name="op">elemento a ser empilhado.</param>
        public void Push(T op)
        {
            this.stack.Add(op);
        }

        public void Pop(int contadorDesempilha)
        {
            if (contadorDesempilha < this.stack.Count)
                this.stack.RemoveRange(this.stack.Count - contadorDesempilha, contadorDesempilha);
        } // RetireParameters()

        /// <summary>
        /// retira e retorna o elemento no topo da pilha.
        /// </summary>
        /// <returns>elemento empilhado no topo da pilha.</returns>
        public T Pop()
        {
            T element = default(T);
            try
            {
               element= this.stack.ElementAt<T>(this.stack.Count - 1);
                this.stack.RemoveAt(this.stack.Count - 1);
                return element;
            } // try
            catch
            {
                throw new Exception("Erro de pilha");
            } // catch

        } // pop()

        /// <summary>
        /// retorna o elemento no topo da pilha, sem retirá-lo.
        /// </summary>
        /// <returns>retorna o elemento no topo da pilha.</returns>
        public T Peek()
        {
            try
            {
                return (stack[stack.Count-1]);
            } // try
            catch
            {
                throw new Exception("Tentativa de retirar elemento de pilha vazia");
            } // catch
        } // Peek()

        /// <summary>
        /// retorna [true] se a pilha está vazia, [false] se tem elementos na pilha.
        /// </summary>
        /// <returns>[true] se a pilha está vazia, [false] se contrário.</returns>
        public bool Empty()
        {
            if ((this.stack != null) && (stack.Count > 0))
                return (false);
            return (true);
        }
    } // Classe pilha
} // namespace
