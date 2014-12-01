//-----------------------------------------------------------------------
// <copyright file="ZipEncryptPT.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace Zip
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class for Zip encryption
    /// </summary>
    public class ZipEncryptPT
    {
        /// <summary>
        /// The primary dictionary
        /// </summary>
        private readonly char[] dictionary;

        /// <summary>
        /// The node pool used to decrease memory allocations
        /// </summary>
        private List<PrefixTree> nodePool;

        /// <summary>
        /// Index of the next free node in the pool
        /// </summary>
        private int nextFreeNode;

        /// <summary>
        /// The prefix tree containing string blocks
        /// </summary>
        private PrefixTree blockTree;

        /// <summary>
        /// Index of the next block
        /// </summary>
        private int nextBlockIndex;

        /// <summary>
        /// Current size of the block code
        /// </summary>
        private int blockCodeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipEncryptPT"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public ZipEncryptPT(string dictionary)
        {
            if (dictionary.Distinct().Count() != dictionary.Count())
            {
                throw new ArgumentException("Dictionary elements are not distinct.");
            }

            this.dictionary = dictionary.ToCharArray();
            Array.Sort(this.dictionary);

            this.nodePool = new List<PrefixTree>(dictionary.Length + 1);
            for (int i = 0; i < this.nodePool.Capacity; ++i)
            {
                PrefixTree node = new PrefixTree();
                node.Nodes = new Dictionary<char, PrefixTree>();
                this.nodePool.Add(node);
            }

            this.nextFreeNode = 0;
        }

        /// <summary>
        /// Encrypts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>An encrypted string</returns>
        public string Encrypt(string message)
        {
            this.Initialize();

            foreach (var c in message)
            {
                if (!this.blockTree.Nodes.ContainsKey(c))
                {
                    throw new ArgumentException(string.Format("Character {0} is not in the dictionary.", c));
                }
            }

            string result = string.Empty;
            string remaining = message;

            while (remaining.Length > 0)
            {
                // Find the next new block in the block tree
                int currentIndex = 0;
                PrefixTree currentNode = this.blockTree;

                while (currentIndex < remaining.Length && currentNode.Nodes.ContainsKey(remaining[currentIndex]))
                {
                    currentNode = currentNode.Nodes[remaining[currentIndex]];
                    ++currentIndex;
                }

                // Check if a new block has been found
                if (currentIndex <= remaining.Length - 1)
                {
                    if (!currentNode.Nodes.ContainsKey(remaining[currentIndex]))
                    {
                        PrefixTree newNode = this.GetPrefixTreeNode();
                        newNode.Number = this.nextBlockIndex++;
                        currentNode.Nodes[remaining[currentIndex]] = newNode;
                    }
                }

                // Add the next encrypted block to the result
                result += currentNode.Number.ToString().PadLeft(this.blockCodeSize, '0');

                // Update current block length
                if ((this.nextBlockIndex - 1).ToString().Length > this.blockCodeSize)
                {
                    ++this.blockCodeSize;
                }

                // Update remaining part of the message
                remaining = remaining.Substring(currentIndex);
            }

            this.Cleanup();

            return result;
        }

        /// <summary>
        /// Initializes the prefix tree.
        /// </summary>
        private void Initialize()
        {
            this.nextBlockIndex = 0;
            this.nextFreeNode = 0;

            this.blockTree = this.GetPrefixTreeNode();
            for (int i = 0; i < this.dictionary.Length; i++)
            {
                PrefixTree node = this.GetPrefixTreeNode();
                node.Number = this.nextBlockIndex++;
                this.blockTree.Nodes.Add(this.dictionary[i], node);
            }

            this.blockCodeSize = this.dictionary.Length > 0 ? (this.dictionary.Length - 1).ToString().Length : 0;
        }

        /// <summary>
        /// Deletes reference to the tree to allow garbage collector clean it up.
        /// </summary>
        private void Cleanup()
        {
            this.blockTree = null;
            this.nextFreeNode = 0;
        }

        /// <summary>
        /// Gets the next prefix tree node from node pool.
        /// </summary>
        /// <returns>A new node.</returns>
        private PrefixTree GetPrefixTreeNode()
        {
            if (this.nextFreeNode >= this.nodePool.Capacity)
            {
                this.nodePool.Capacity *= 2;
                for (int i = this.nodePool.Count; i < this.nodePool.Capacity; ++i)
                {
                    PrefixTree node = new PrefixTree();
                    node.Nodes = new Dictionary<char, PrefixTree>();
                    this.nodePool.Add(node);
                }
            }

            this.nodePool[this.nextFreeNode].Number = 0;
            this.nodePool[this.nextFreeNode].Nodes.Clear();

            return this.nodePool[this.nextFreeNode++];
        }

        /// <summary>
        /// Node of a prefix tree
        /// </summary>
        private class PrefixTree
        {
            /// <summary>
            /// Gets or sets the number of current block.
            /// </summary>
            /// <value>
            /// The number of current block.
            /// </value>
            public int Number { get; set; }

            /// <summary>
            /// Gets or sets the child nodes.
            /// </summary>
            /// <value>
            /// The child nodes.
            /// </value>
            public IDictionary<char, PrefixTree> Nodes { get; set; }
        }
    }
}