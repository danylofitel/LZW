//-----------------------------------------------------------------------
// <copyright file="LZWCompress.cs" company="FTL">
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
    /// Class for LZW compression, implementation uses a hash table
    /// </summary>
    public class LZWCompress
    {
        /// <summary>
        /// The alphabet
        /// </summary>
        private readonly char[] alphabet;

        /// <summary>
        /// Table of block indices
        /// </summary>
        private readonly IDictionary<string, int> table;

        /// <summary>
        /// Index of the next block
        /// </summary>
        private int nextBlockIndex;

        /// <summary>
        /// Size of the largest block in the table
        /// </summary>
        private int maxBlockSize;

        /// <summary>
        /// Current size of the block code
        /// </summary>
        private int blockCodeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="LZWCompress"/> class.
        /// </summary>
        /// <param name="alphabet">The alphabet.</param>
        public LZWCompress(string alphabet)
        {
            if (alphabet.Distinct().Count() != alphabet.Count())
            {
                throw new ArgumentException("alphabet elements are not distinct.");
            }

            this.alphabet = alphabet.ToCharArray();
            Array.Sort(this.alphabet);

            this.table = new Dictionary<string, int>((alphabet.Length * 2) + 1);
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
                if (!this.table.ContainsKey(c.ToString()))
                {
                    throw new ArgumentException(string.Format("Character {0} is not in the alphabet.", c));
                }
            }

            StringBuilder result = new StringBuilder();
            string remaining = message;

            while (remaining.Length > 0)
            {
                // Find the next new block using binary search
                // in a range of block sizes between 1 and size of largest block in the table
                int l = 1;
                int r = Math.Min(this.maxBlockSize, remaining.Length);
                while (l != r)
                {
                    int middle = (l + r) / 2;
                    if (middle == l)
                    {
                        ++middle;
                    }

                    if (this.table.ContainsKey(remaining.Substring(0, middle)))
                    {
                        l = middle;
                    }
                    else
                    {
                        r = middle - 1;
                    }
                }

                int nextBlockSize = r;
                bool newBlockFound = nextBlockSize < remaining.Length;

                // Add the next encrypted block to the result
                result.Append(this.table[remaining.Substring(0, nextBlockSize)].ToString().PadLeft(this.blockCodeSize, '0'));

                // Check if new block has been found
                if (newBlockFound)
                {
                    // Add new block to the table
                    this.table.Add(remaining.Substring(0, nextBlockSize + 1), this.nextBlockIndex++);
                    if (nextBlockSize + 1 > this.maxBlockSize)
                    {
                        this.maxBlockSize = nextBlockSize + 1;
                    }

                    // Update current block length
                    if ((this.nextBlockIndex - 1).ToString().Length > this.blockCodeSize)
                    {
                        ++this.blockCodeSize;
                    }
                }

                // Update remaining part of the message
                remaining = remaining.Substring(nextBlockSize);
            }

            this.Cleanup();

            return result.ToString();
        }

        /// <summary>
        /// Initializes the table.
        /// </summary>
        private void Initialize()
        {
            this.maxBlockSize = this.alphabet.Length > 0 ? 1 : 0;
            this.nextBlockIndex = 0;

            for (int i = 0; i < this.alphabet.Length; i++)
            {
                this.table.Add(this.alphabet[i].ToString(), this.nextBlockIndex++);
            }

            this.blockCodeSize = this.table.Count > 0 ? (this.table.Count - 1).ToString().Length : 0;
        }

        /// <summary>
        /// Deletes reference to the table to allow garbage collector clean it up.
        /// </summary>
        private void Cleanup()
        {
            this.table.Clear();
        }
    }
}