using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parser
{
    /// <summary>
    /// estrutura de dados básica de Fila.
    /// </summary>
    /// <typeparam name="T">tipo de elemento a ser guardada na fila.</typeparam>
    public class Queue<T>
    {
        /// <summary>
        /// guarda os elementos da fila.
        /// </summary>
        private List<T> fila;
        private string nomeFila;
 
        /// <summary>
        /// construtor básico.
        /// </summary>
        public Queue(string _nomeFila)
        {
            this.fila = new List<T>();
            this.nomeFila = _nomeFila;
        } // Queue()

        /// <summary>
        /// retira o primeiro elemento da fila.
        /// </summary>
        /// <returns>retorna o primeiro elemento da fila.</returns>
        public T DeQueue()
        {
            if ((fila == null) || (fila.Count == 0))
                throw new Exception("tentativa de retirar elemento da Fila "+this.nomeFila+", a qual está vazia.");
            T element = this.fila.ElementAt<T>(0);
            this.fila.RemoveAt(0);
            return element;
        } // deQueue()

        /// <summary>
        /// enfilera no fim da fila o elemento parâmetro.
        /// </summary>
        /// <param name="element">elemento parâmetro a ser enfilerado.</param>
        public void EnQueue(T element)
        {
            this.fila.Add(element);
            
        } // enQueue()

        /// <summary>
        /// retorna [true] se a fila está vazia, [false] se contrário.
        /// </summary>
        /// <returns>[true] se a fila está vazia, [false] se está cheia.</returns>
        public bool Empty()
        {
            if ((fila == null) || (fila.Count == 0))
                return true;
            return false;
        } // empty()

        public void Clear()
        {
            if (this.fila != null)
                this.fila.Clear();
            if (this.fila == null)
                this.fila = new List<T>();
        } //  Cclear()
    } // class Queue
}// namespace
