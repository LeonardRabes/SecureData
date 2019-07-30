using System;

namespace DataEncrypter.Cyphers
{
    /// <summary>
    /// Provides encryption and decryption with the Advanced Encryption Standard
    /// </summary>
    public partial class AES : ICypher
    {
        /// <summary>
        /// Size of minimal Data input to the AES
        /// </summary>
        public int MinDataSize { get => 16; }

        /// <summary>
        /// String to identify AES
        /// </summary>
        public string CypherIdentifier { get => "AES"; }

        /// <summary>
        /// Encrypts data with the Advanced Encryption Standard.
        /// </summary>
        /// <param name="plaintext">Plaintext, which is encrypted in place, length must be a multible of 16</param>
        /// <param name="startIndex">Index to start the process from.</param>
        /// <param name="key">Key to encrypt</param>
        public void Encrypt(ref byte[] plaintext, int startIndex, byte[] key)
        {
            byte[][] expandedKeys = KeyExpansion(key);

            if ((plaintext.Length - startIndex) % 16 != 0)
            {
                throw new Exception("Incorrect Length: Length must be a multible of 16!");
            }

            for (int index = startIndex; index < plaintext.Length; index+=16)
            {
                //initial round
                AddRoundKey(ref plaintext, index, expandedKeys[0]);

                //encryption rounds
                for (int round = 1; round < 10; round++)
                {
                    SubByte(ref plaintext, index);
                    ShiftRows(ref plaintext, index);
                    MixColumns(ref plaintext, index);
                    AddRoundKey(ref plaintext, index, expandedKeys[round]);
                }

                //last round
                SubByte(ref plaintext, index);
                ShiftRows(ref plaintext, index);
                AddRoundKey(ref plaintext, index, expandedKeys[10]);
            }
        }

        /// <summary>
        /// Decrypts data with the Advanced Encryption Standard.
        /// </summary>
        /// <param name="plaintext">Cyphertext, which is decrypted in place, length must be a multible of 16</param>
        /// <param name="startIndex">Index to start the process from.</param>
        /// <param name="key">Key to decrypt</param>
        public void Decrypt(ref byte[] cyphertext, int startIndex, byte[] key)
        {
            byte[][] expandedKeys = KeyExpansion(key);

            if (cyphertext.Length % 16 != 0)
            {
                throw new Exception("Incorrect Length: Length must be a multible of 16!");
            }

            for (int index = 0; index < cyphertext.Length; index += 16)
            {
                //initial round
                AddRoundKey(ref cyphertext, index, expandedKeys[10]);
                InvShiftRows(ref cyphertext, index);
                InvSubByte(ref cyphertext, index);

                //decryption rounds
                for (int round = 9; round > 0; round--)
                {
                    AddRoundKey(ref cyphertext, index, expandedKeys[round]);
                    InvMixColumns(ref cyphertext, index);
                    InvShiftRows(ref cyphertext, index);
                    InvSubByte(ref cyphertext, index);
                }

                //last round
                AddRoundKey(ref cyphertext, index, expandedKeys[0]);
            }
        }

        /// <summary>
        /// Padds data to fit into the 16byte blocksize.
        /// </summary>
        /// <param name="state">Reference to the current encryption state</param>
        public void Padding(ref byte[] state)
        {
            int l = state.Length;

            if (l % 16 != 0)
            {
                Array.Resize(ref state, 16 * (int)Math.Ceiling(l / 16F));
            }
        }

        /// <summary>
        /// Creates Keys for en-/decryption rounds.
        /// </summary>
        /// <param name="initialKey">The key to create 10 additional keys.</param>
        /// <returns>Returns an array with 11 round keys</returns>
        private byte[][] KeyExpansion(byte[] initialKey)
        {
            initialKey = CastKey(initialKey, 16);

            //add init key
            byte[][] expKeys = new byte[11][];
            expKeys[0] = initialKey;

            //generate keys for each round
            for (int keyIndex = 1; keyIndex < expKeys.Length; keyIndex++)
            {
                expKeys[keyIndex] = GenerateRoundKey(expKeys[keyIndex - 1], keyIndex);
            }

            return expKeys;
        }

        /// <summary>
        /// Casts a key of any length to its desired length.
        /// </summary>
        /// <param name="initialKey">Initial Key</param>
        /// <param name="length">Desired length of the output</param>
        /// <returns>Returns the casted key</returns>
        private byte[] CastKey(byte[] initialKey, int length)
        {
            if (initialKey.Length < length) //lengthen the key to correct length by repeating the key
            {
                byte[] longerKey = new byte[length];

                int shortKeyCount = 0;
                bool substituteBytes = false;

                for (int i = 0; i < 16; i++)
                {
                    if (shortKeyCount >= initialKey.Length)
                    {
                        shortKeyCount = 0;
                        substituteBytes = true;
                    }

                    if (!substituteBytes) //copy the key to its initial length
                    {
                        longerKey[i] = initialKey[shortKeyCount++];
                    }
                    else //all bytes over init length will be substituted
                    {
                        longerKey[i] = _sbox[initialKey[shortKeyCount++]];
                    }
                }

                return longerKey;
            }
            else if (initialKey.Length > length) //shorten the key to correct length xor the longer bytes to begining bytes
            {
                byte[] shorterKey = new byte[length];

                for (int i = 0; i < initialKey.Length; i++)
                {
                    if (i < length)
                    {
                        shorterKey[i] = initialKey[i];
                    }
                    else
                    {
                        shorterKey[i - length] ^= initialKey[i];
                    }
                }

                return shorterKey;
            }
            else
            {
                return initialKey;
            }
        }

        /// <summary>
        /// Generates a new Key.
        /// </summary>
        /// <param name="prevKey">Base for a new key.</param>
        /// <param name="keyIndex">The round of the new key</param>
        /// <returns></returns>
        private byte[] GenerateRoundKey(byte[] prevKey, int keyIndex)
        {
            byte[] newKey = new byte[16];
            //get row3 of key
            byte[] row3 = { prevKey[12], prevKey[13], prevKey[14], prevKey[15] };

            //rotate bytes
            byte buffer = row3[0];
            row3[0] = row3[1];
            row3[1] = row3[2];
            row3[2] = row3[3];
            row3[3] = buffer;

            //substitute bytes
            for (int i = 0; i < 4; i++)
            {
                row3[i] = _sbox[row3[i]];
            }

            //xor rcon
            row3[0] ^= _rcon[keyIndex];

            //xor with row0
            for (int i = 0; i < 4; i++)
            {
                newKey[i] = (byte)(row3[i] ^ prevKey[i]);
            }

            //create next row(n) with row of old key(n) and and prev generated row(n-1)
            for (int row = 1; row < 4; row++)
            {
                int start = row * 4;

                for (int i = start; i < start + 4; i++)
                {
                    newKey[i] = (byte)(prevKey[i] ^ newKey[i - 4]);
                }
            }

            return newKey;
        }

        //Structure of 128Bit block:
        //
        //  col0 col1 col2 col3
        //
        //  X0   X4   X8   X12  row0
        //  X1   X5   X9   X13  row1
        //  X2   X6   X10  X14  row2
        //  X3   X7   X11  X15  row3

        /// <summary>
        /// Adds a round key to a 128 bit block.
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        /// <param name="roundKey">RoundKey</param>
        private void AddRoundKey(ref byte[] state, int index, byte[] roundKey)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] ^= roundKey[i - index]; //xor state with round key
            }
        }

        /// <summary>
        /// Substitutes all bytes of a 128 bit block by bytes from the sbox lookup table.
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        private void SubByte(ref byte[] state, int index)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] = _sbox[state[i]];
            }
        }

        /// <summary>
        /// Substitutes all bytes of a 128 bit block by bytes from the inverse sbox lookup table.
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        private void InvSubByte(ref byte[] state, int index)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] = _invsbox[state[i]];
            }
        }

        /// <summary>
        /// Shifts bytes (amount by row index) to the right of a 128 bit block.
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        private void ShiftRows(ref byte[] state, int index)
        {
            for (byte row = 0; row < 4; row++)
            {
                for (byte i = 0; i < row; i++)
                {
                    shiftOnceRight(ref state, row);
                }
            }

            void shiftOnceRight(ref byte[] st, byte row)
            {
                int start = index + row;

                byte buffer = st[start];

                st[start] = st[start + 1 * 4];
                st[start + 1 * 4] = st[start + 2 * 4];
                st[start + 2 * 4] = st[start + 3 * 4];
                st[start + 3 * 4] = buffer;
            }
        }

        /// <summary>
        /// Shifts bytes (amount by row index) to the left of a 128 bit block.
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        private void InvShiftRows(ref byte[] state, int index)
        {
            for (byte row = 0; row < 4; row++)
            {
                for (byte i = 0; i < row; i++)
                {
                    shiftOnceLeft(ref state, row);
                }
            }

            void shiftOnceLeft(ref byte[] st, byte row)
            {
                int start = index + row;

                byte buffer = st[start + 3 * 4];
                st[start + 3 * 4] = st[start + 2 * 4];
                st[start + 2 * 4] = st[start + 1 * 4];
                st[start + 1 * 4] = st[start];
                st[start] = buffer;
            }
        }

        /// <summary>
        /// Mixes columns according to the AES (https://en.wikipedia.org/wiki/Rijndael_MixColumns).
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        private void MixColumns(ref byte[] state, int index)
        {
            byte[] buffer = new byte[16];
            for (int col = 0; col < 4; col++)
            {
                int offset = index + col * 4;

                buffer[offset + 0 - index] = (byte)(_mul2[state[offset + 0]] ^ _mul3[state[offset + 1]] ^ state[offset + 2] ^ state[offset + 3]);
                buffer[offset + 1 - index] = (byte)(state[offset + 0] ^ _mul2[state[offset + 1]] ^ _mul3[state[offset + 2]] ^ state[offset + 3]);
                buffer[offset + 2 - index] = (byte)(state[offset + 0] ^ state[offset + 1] ^ _mul2[state[offset + 2]] ^ _mul3[state[offset + 3]]);
                buffer[offset + 3 - index] = (byte)(_mul3[state[offset + 0]] ^ state[offset + 1] ^ state[offset + 2] ^ _mul2[state[offset + 3]]);
            }

            for (int i = 0; i < 16; i++)
            {
                state[i + index] = buffer[i];
            }
        }

        /// <summary>
        /// Mixes columns inverse according to the AES (https://en.wikipedia.org/wiki/Rijndael_MixColumns).
        /// </summary>
        /// <param name="state">Reference to the current en-/decryption state</param>
        /// <param name="index">Index of the first byte of the block</param>
        private void InvMixColumns(ref byte[] state, int index)
        {
            byte[] buffer = new byte[16];

            for (int col = 0; col < 4; col++)
            {
                int offset = index + col * 4;

                buffer[offset + 0 - index] = (byte)(_mul14[state[offset + 0]] ^ _mul11[state[offset + 1]] ^ _mul13[state[offset + 2]] ^ _mul9[state[offset + 3]]);
                buffer[offset + 1 - index] = (byte)(_mul9[state[offset + 0]] ^ _mul14[state[offset + 1]] ^ _mul11[state[offset + 2]] ^ _mul13[state[offset + 3]]);
                buffer[offset + 2 - index] = (byte)(_mul13[state[offset + 0]] ^ _mul9[state[offset + 1]] ^ _mul14[state[offset + 2]] ^ _mul11[state[offset + 3]]);
                buffer[offset + 3 - index] = (byte)(_mul11[state[offset + 0]] ^ _mul13[state[offset + 1]] ^ _mul9[state[offset + 2]] ^ _mul14[state[offset + 3]]);
            }

            for (int i = 0; i < 16; i++)
            {
                state[i + index] = buffer[i];
            }
        }
    }
}
