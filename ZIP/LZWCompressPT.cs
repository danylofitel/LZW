//-----------------------------------------------------------------------
// <copyright file="LZWCompressPT.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace ZIP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for LZW compression, implementation uses a prefix tree
    /// </summary>
    public class LZWCompressPT
    {
        /// <summary>
        /// The primary alphabet
        /// </summary>
        private readonly char[] alphabet;

        /// <summary>
        /// The node pool used to avoid in-place memory allocations
        /// </summary>
        private NodePool nodePool;

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
        /// Initializes a new instance of the <see cref="LZWCompressPT"/> class.
        /// </summary>
        /// <param name="alphabet">The alphabet.</param>
        public LZWCompressPT(string alphabet)
        {
            if (alphabet.Distinct().Count() != alphabet.Count())
            {
                throw new ArgumentException("Dictionary elements are not distinct.");
            }

            this.alphabet = alphabet.ToCharArray();
            Array.Sort(this.alphabet);

            this.nodePool = new NodePool((alphabet.Length * 2) + 1);
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
                    throw new ArgumentException(string.Format("Character {0} is not in the alphabet.", c));
                }
            }

            StringBuilder result = new StringBuilder();
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

                // Add the next encrypted block to the result
                result.Append(currentNode.Number.ToString().PadLeft(this.blockCodeSize, '0'));

                // Check if new block has been found
                if (currentIndex <= remaining.Length - 1)
                {
                    if (!currentNode.Nodes.ContainsKey(remaining[currentIndex]))
                    {
                        // Add new block to the table
                        PrefixTree newNode = this.nodePool.GetNode();
                        newNode.Number = this.nextBlockIndex++;
                        currentNode.Nodes[remaining[currentIndex]] = newNode;

                        // Update current block length
                        if ((this.nextBlockIndex - 1).ToString().Length > this.blockCodeSize)
                        {
                            ++this.blockCodeSize;
                        }
                    }
                }

                // Update remaining part of the message
                remaining = remaining.Substring(currentIndex);
            }

            return result.ToString();
        }

        /// <summary>
        /// Initializes the prefix tree.
        /// </summary>
        private void Initialize()
        {
            this.nodePool.Reset();
            this.nextBlockIndex = 0;

            this.blockTree = this.nodePool.GetNode();
            for (int i = 0; i < this.alphabet.Length; i++)
            {
                PrefixTree node = this.nodePool.GetNode();
                node.Number = this.nextBlockIndex++;
                this.blockTree.Nodes.Add(this.alphabet[i], node);
            }

            this.blockCodeSize = this.alphabet.Length > 0 ? (this.alphabet.Length - 1).ToString().Length : 0;
        }

        /// <summary>
        /// A pool of prefix tree nodes used to avoid in-place memory allocations
        /// </summary>
        private class NodePool
        {
            /// <summary>
            /// The node pool used to decrease memory allocations
            /// </summary>
            private readonly List<PrefixTree> nodePool;

            /// <summary>
            /// Index of the next free node in the pool
            /// </summary>
            private int nextFreeNode;

            /// <summary>
            /// Initializes a new instance of the <see cref="NodePool"/> class.
            /// </summary>
            /// <param name="capacity">The initial capacity.</param>
            public NodePool(int capacity)
            {
                this.nodePool = new List<PrefixTree>(capacity);
                for (int i = 0; i < this.nodePool.Capacity; ++i)
                {
                    PrefixTree node = new PrefixTree();
                    node.Nodes = new Dictionary<char, PrefixTree>();
                    this.nodePool.Add(node);
                }

                this.nextFreeNode = 0;
            }

            /// <summary>
            /// Returns the next free node.
            /// </summary>
            /// <returns>A free node</returns>
            public PrefixTree GetNode()
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
            /// Resets the node count.
            /// </summary>
            public void Reset()
            {
                this.nextFreeNode = 0;
            }
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