using System;

namespace DataEncrypter.CryptMethods
{
    public partial class AES
    {
        public byte[][] ExpandedKeys {get; internal set;}

        /// <summary>
        /// Creates a object for encryption and decryption.
        /// </summary>
        /// <param name="key">Cypher to en-/decrypt</param>
        public AES(byte[] key)
        {
            ExpandedKeys = KeyExpansion(key);
        }

        public void Encrypt(ref byte[] plaintext)
        {
            for (int index = 0; index < plaintext.Length; index+=16)
            {
                //initial round
                AddRoundKey(ref plaintext, index, ExpandedKeys[0]);

                //encryption rounds
                for (int round = 1; round < 10; round++)
                {
                    SubByte(ref plaintext, index);
                    ShiftRows(ref plaintext, index);
                    MixColumns(ref plaintext, index);
                    AddRoundKey(ref plaintext, index, ExpandedKeys[round]);
                }

                //last round
                SubByte(ref plaintext, index);
                ShiftRows(ref plaintext, index);
                AddRoundKey(ref plaintext, index, ExpandedKeys[10]);
            }
        }

        public void Decrypt(ref byte[] cyphertext)
        {
            for (int index = 0; index < cyphertext.Length; index += 16)
            {
                //initial round
                AddRoundKey(ref cyphertext, index, ExpandedKeys[10]);
                InvShiftRows(ref cyphertext, index);
                InvSubByte(ref cyphertext, index);

                //decryption rounds
                for (int round = 9; round > 0; round--)
                {
                    AddRoundKey(ref cyphertext, index, ExpandedKeys[round]);
                    InvMixColumns(ref cyphertext, index);
                    InvShiftRows(ref cyphertext, index);
                    InvSubByte(ref cyphertext, index);
                }

                //last round
                AddRoundKey(ref cyphertext, index, ExpandedKeys[0]);
            }
        }

        private byte[][] KeyExpansion(byte[] initialKey)
        {
            //add init key
            byte[][] expKeys = new byte[11][];
            expKeys[0] = initialKey;

            //generate keys for each round
            for (int keyIndex = 1; keyIndex < expKeys.Length; keyIndex++)
            {
                expKeys[keyIndex] = GenerateKey(expKeys[keyIndex - 1], keyIndex);
            }

            return expKeys;
        }

        private byte[] GenerateKey(byte[] prevKey, int keyIndex)
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

        
        private void AddRoundKey(ref byte[] state, int index, byte[] roundKey)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] ^= roundKey[i - index]; //xor state with round key
            }
        }

        private void SubByte(ref byte[] state, int index)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] = _sbox[state[i]];
            }
        }

        private void InvSubByte(ref byte[] state, int index)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] = _invsbox[state[i]];
            }
        }

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
