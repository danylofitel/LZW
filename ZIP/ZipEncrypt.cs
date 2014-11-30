//-----------------------------------------------------------------------
// <copyright file="ZipEncrypt.cs" company="FTL">
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
    public class ZipEncrypt
    {
        /// <summary>
        /// The primary dictionary
        /// </summary>
        private readonly char[] dictionary;

        /// <summary>
        /// Table of block indices
        /// </summary>
        private IDictionary<string, int> table;

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
        /// Initializes a new instance of the <see cref="ZipEncrypt"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public ZipEncrypt(string dictionary)
        {
            if (dictionary.Distinct().Count() != dictionary.Count())
            {
                throw new ArgumentException("Dictionary elements are not distinct.");
            }

            this.dictionary = dictionary.ToCharArray();
            Array.Sort(this.dictionary);
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
                    throw new ArgumentException(string.Format("Character {0} is not in the dictionary.", c));
                }
            }

            string result = string.Empty;
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
                string newBlockIndex = this.table[remaining.Substring(0, nextBlockSize)].ToString().PadLeft(this.blockCodeSize, '0');
                result += newBlockIndex;

                // Add new block to the table
                if (newBlockFound)
                {
                    // Update current block length
                    if (this.nextBlockIndex.ToString().Length > this.blockCodeSize)
                    {
                        ++this.blockCodeSize;
                    }

                    // Add new block to the table
                    this.table.Add(remaining.Substring(0, nextBlockSize + 1), this.nextBlockIndex++);
                    if (nextBlockSize + 1 > this.maxBlockSize)
                    {
                        this.maxBlockSize = nextBlockSize + 1;
                    }
                }

                // Update remaining part of the message
                remaining = remaining.Substring(nextBlockSize);
            }

            return result;
        }

        /// <summary>
        /// Initializes the table.
        /// </summary>
        private void Initialize()
        {
            this.maxBlockSize = this.dictionary.Length > 0 ? 1 : 0;
            this.nextBlockIndex = 0;

            this.table = new Dictionary<string, int>();
            for (int i = 0; i < this.dictionary.Length; i++)
            {
                this.table.Add(this.dictionary[i].ToString(), this.nextBlockIndex++);
            }

            this.blockCodeSize = this.table.Count > 0 ? (this.table.Count - 1).ToString().Length : 0;
        }
    }
}