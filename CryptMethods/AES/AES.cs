using System;

namespace DataEncrypter.CryptMethods
{
    public partial class AES
    {
        //Structure of 128Bit block:
        //
        //  col0 col1 col2 col3
        //
        //  X0   X4   X8   X12  row0
        //  X1   X5   X9   X13  row1
        //  X2   X6   X10  X14  row2
        //  X3   X7   X11  X15  row3

        public AES(byte[] key)
        {

        }

        public byte[] Encrypt(ref byte[] plaintext, byte[] cypher)
        {
            Padding(ref plaintext);
            for (int i = 0; i < plaintext.Length; i+=16)
            {
                
            }

            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] cyphertext, byte[] cypher)
        {
            throw new NotImplementedException();
        }

        private void KeySchedule()
        {
            throw new NotImplementedException();
        }

        private byte[] Padding(ref byte[] state)
        {
            int l = state.Length;

            if (l % 16 != 0)
            {
                Array.Resize(ref state, l + l % 16);
            }

            return state;
        }

        private void AddRoundKey(ref byte[] state, int index, ref byte[] roundKey)
        {
            for (int i = index; i < index + 16; i++)
            {
                state[i] ^= roundKey[i];
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

        public void ShiftRows(ref byte[] state, int index)
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

        public void InvShiftRows(ref byte[] state, int index)
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

        public void MixColumns(ref byte[] state, int index)
        {
            byte[] buffer = new byte[16];
            for (int col = 0; col < 4; col++)
            {
                int offset = col * 4;

                buffer[offset + 0] = (byte)(_mul2[state[offset + 0]] ^ _mul3[state[offset + 1]] ^ state[offset + 2] ^ state[offset + 3]);
                buffer[offset + 1] = (byte)(state[offset + 0] ^ _mul2[state[offset + 1]] ^ _mul3[state[offset + 2]] ^ state[offset + 3]);
                buffer[offset + 2] = (byte)(state[offset + 0] ^ state[offset + 1] ^ _mul2[state[offset + 2]] ^ _mul3[state[offset + 3]]);
                buffer[offset + 3] = (byte)(_mul3[state[offset + 0]] ^ state[offset + 1] ^ state[offset + 2] ^ _mul2[state[offset + 3]]);
            }

            for (int i = 0; i < 16; i++)
            {
                state[i + index] = buffer[i];
            }
        }

        public void InvMixColumns(ref byte[] state, int index)
        {
            byte[] buffer = new byte[16];

            for (int col = 0; col < 4; col++)
            {
                int offset = col * 4;

                buffer[offset + 0] = (byte)(_mul14[state[offset + 0]] ^ _mul11[state[offset + 1]] ^ _mul13[state[offset + 2]] ^ _mul9[state[offset + 3]]);
                buffer[offset + 1] = (byte)(_mul9[state[offset + 0]] ^ _mul14[state[offset + 1]] ^ _mul11[state[offset + 2]] ^ _mul13[state[offset + 3]]);
                buffer[offset + 2] = (byte)(_mul13[state[offset + 0]] ^ _mul9[state[offset + 1]] ^ _mul14[state[offset + 2]] ^ _mul11[state[offset + 3]]);
                buffer[offset + 3] = (byte)(_mul11[state[offset + 0]] ^ _mul13[state[offset + 1]] ^ _mul9[state[offset + 2]] ^ _mul14[state[offset + 3]]);
            }

            for (int i = 0; i < 16; i++)
            {
                state[i + index] = buffer[i];
            }
        }
    }
}
