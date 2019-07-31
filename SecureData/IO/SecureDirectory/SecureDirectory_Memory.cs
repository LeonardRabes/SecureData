using SecureData.Cyphers;
using System;
using System.IO;

namespace SecureData.IO
{
    public partial class SecureDirectory
    {
        public class MemoryManager
        {
            public const int ChunkSize = ushort.MaxValue + 1;

            public int[] AllocatableChunks { get => _allocatableChunks; }
            public int[] OccupiedChunks { get => _occupiedChunks; }
            public long AllocationStartIndex { get => _allocStartIndex; }
            public int ChunkCount { get => _chunkCount; }

            private int[] _allocatableChunks;
            private int[] _occupiedChunks;

            private Stream _directoryStream;
            private long _allocStartIndex;
            private int _chunkCount;

            /// <summary>
            /// Creates an instance of MemoryManager
            /// </summary>
            /// <param name="directoryStream">Stream to be handled</param>
            /// <param name="startIndex">Start index, from where memory is handled</param>
            /// <param name="chunkCount">Currently existing chunks, of prev MemoryManager</param>
            /// <param name="allocChunks">Allocatable chunks, of prev MemoryManager</param>
            /// <param name="occChunks">Occupied chunks, of prev MemoryManager</param>
            public MemoryManager(Stream directoryStream, long startIndex, int chunkCount, int[] allocChunks, int[] occChunks)
            {
                _directoryStream = directoryStream;
                _allocStartIndex = startIndex;

                _allocatableChunks = allocChunks;
                _occupiedChunks = occChunks;
            }

            /// <summary>
            /// Allocates space in memory
            /// </summary>
            /// <param name="length">Amount of bytes to allocate</param>
            /// <returns>Array of chunk indices, which can fit byte amount inside them</returns>
            public int[] AllocateBytes(long length)
            {
                int amount = (int)Math.Ceiling(length / (float)ChunkSize);
                int[] allocated = AllocateChunks(amount);
                return allocated;
            }

            /// <summary>
            /// Allocates chunks in memory
            /// </summary>
            /// <param name="length">Amount of chunks to allocate</param>
            /// <returns>Array of chunk indices</returns>
            public int[] AllocateChunks(int length)
            {
                int[] allocated = new int[length];
                Array.Sort(_allocatableChunks); //sort to occupy low indices first

                //add new chunks if not enough free are available
                if (_allocatableChunks.Length < length)
                {
                    int diff = length - _allocatableChunks.Length;

                    int[] newAllocChunks = new int[length];
                    _allocatableChunks.CopyTo(newAllocChunks, 0); //create new larger array

                    //set new indices
                    for (int i = _allocatableChunks.Length; i < length; i++)
                    {
                        newAllocChunks[i] = _chunkCount + i;
                    }
                    _chunkCount += diff; // update total chunk count
                    _allocatableChunks = newAllocChunks;
                }

                //copy out available chunks and return them
                Array.Copy(_allocatableChunks, allocated, length);
                return allocated;
            }

            public void SecureWrite(Stream source, int[] targetChunks, ICypher cypher, byte[] key)
            {
                long totalBytes = source.Length;
                byte[] buffer = new byte[ChunkSize];
                source.Position = 0;

                for (int i = 0; totalBytes > 0; i++)
                {
                    int byteCount = (int)Math.Min(ChunkSize, totalBytes); //amount of bytes to write
                    long offset = _allocStartIndex + targetChunks[i] * ChunkSize; //offset for chunks

                    source.Read(buffer, 0, byteCount); //read entire/partial chunk

                    cypher.Encrypt(ref buffer, 0, key);

                    _directoryStream.Position = offset;
                    _directoryStream.Write(buffer, 0, buffer.Length); //write entire buffer

                    totalBytes -= byteCount;
                }

                AddChunks(ref _occupiedChunks, targetChunks);
                RemoveChunks(ref _allocatableChunks, targetChunks);
            }

            public void SecureRead(Stream output, long length, int[] targetChunks, ICypher cypher, byte[] key)
            {
                long totalBytes = length;
                byte[] buffer = new byte[ChunkSize];
                output.Position = 0;

                for (int i = 0; totalBytes > 0; i++)
                {
                    int byteCount = (int)Math.Min(ChunkSize, totalBytes); //amount of bytes to write
                    long offset = _allocStartIndex + targetChunks[i] * ChunkSize; //offset for chunks

                    _directoryStream.Position = offset;
                    _directoryStream.Read(buffer, 0, buffer.Length); //read entire chunk

                    cypher.Decrypt(ref buffer, 0, key);

                    output.Write(buffer, 0, byteCount); //write entire/partial buffer according to size

                    totalBytes -= byteCount;
                }
            }

            /// <summary>
            /// Marks chunks as allocatable.
            /// </summary>
            /// <param name="targetChunks">Chunks to be marked</param>
            /// <returns>Boolean, if deallocation was successfull</returns>
            public bool Deallocate(int[] targetChunks)
            {
                bool contains = ContainsChunks(_occupiedChunks, targetChunks); //check if target chunks were allocated
                if (contains)
                {
                    AddChunks(ref _allocatableChunks, targetChunks);
                    RemoveChunks(ref _occupiedChunks, targetChunks);
                }

                return contains;
            }

            private bool ContainsChunks(int[] array, int[] chunks)
            {
                int found = 0;
                for (int iter = 0; iter < array.Length; iter++)
                {
                    for (int i = 0; i < chunks.Length; i++)
                    {
                        if (array[iter] == chunks[i])
                        {
                            found++;
                            break;
                        }
                    }
                }

                return found == chunks.Length;
            }

            private void AddChunks(ref int[] array, int[] chunks)
            {
                int offset = array.Length;
                Array.Resize(ref array, array.Length + chunks.Length);
                chunks.CopyTo(array, offset);
                Array.Sort(array);
            }

            private void RemoveChunks(ref int[] array, int[] chunks)
            {
                int found = 0;
                for (int iter = 0; iter < array.Length; iter++)
                {
                    for (int i = 0; i < chunks.Length; i++)
                    {
                        if (array[iter] == chunks[i])
                        {
                            array[iter] = -1;
                            found++;
                            break;
                        }
                    }
                }

                int[] removed = new int[array.Length - found];

                int removedIndex = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] > 0)
                    {
                        removed[removedIndex++] = array[i];
                    }
                }

                array = removed;
            }
        }
    }
}
