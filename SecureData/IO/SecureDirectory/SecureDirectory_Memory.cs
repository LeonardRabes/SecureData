using SecureData.Cyphers;
using System;
using System.IO;

namespace SecureData.IO
{
    public partial class SecureDirectory
    {
        public class MemoryManager
        {
            public const int SectorSize = 4096;

            public int[] AllocatableSectors { get => _allocatableSectors; }
            public int[] OccupiedSectors { get => _occupiedSectors; }
            public long AllocationStartIndex { get => _allocStartIndex; }
            public int SectorCount { get => _sectorCount; }

            private int[] _allocatableSectors;
            private int[] _occupiedSectors;

            private Stream _directoryStream;
            private long _allocStartIndex;
            private int _sectorCount;

            /// <summary>
            /// Creates an instance of MemoryManager
            /// </summary>
            /// <param name="directoryStream">Stream to be handled</param>
            /// <param name="startIndex">Start index, from where memory is handled</param>
            /// <param name="sectorCount">Currently existing sectors, of prev MemoryManager</param>
            /// <param name="allocSectors">Allocatable sectors, of prev MemoryManager</param>
            /// <param name="occSectors">Occupied sectors, of prev MemoryManager</param>
            public MemoryManager(Stream directoryStream, long startIndex, int sectorCount, int[] allocSectors, int[] occSectors)
            {
                _directoryStream = directoryStream;
                _allocStartIndex = startIndex;

                _sectorCount = sectorCount;
                _allocatableSectors = allocSectors;
                _occupiedSectors = occSectors;
            }

            /// <summary>
            /// Allocates space in memory
            /// </summary>
            /// <param name="length">Amount of bytes to allocate</param>
            /// <returns>Array of sector indices, which can fit byte amount inside them</returns>
            public int[] AllocateBytes(long length)
            {
                int amount = (int)Math.Ceiling(length / (float)SectorSize);
                int[] allocated = AllocateSectors(amount);
                return allocated;
            }

            /// <summary>
            /// Allocates sectors in memory
            /// </summary>
            /// <param name="length">Amount of sectors to allocate</param>
            /// <returns>Array of sector indices</returns>
            public int[] AllocateSectors(int length)
            {
                int[] allocated = new int[length];
                Array.Sort(_allocatableSectors); //sort to occupy low indices first

                //add new sectors if not enough free are available
                if (_allocatableSectors.Length < length)
                {
                    int diff = length - _allocatableSectors.Length;

                    int[] newAllocSectors = new int[length];
                    _allocatableSectors.CopyTo(newAllocSectors, 0); //create new larger array

                    //set new indices
                    for (int i = _allocatableSectors.Length; i < length; i++)
                    {
                        newAllocSectors[i] = _sectorCount + i;
                    }
                    _sectorCount += diff; // update total sector count
                    _allocatableSectors = newAllocSectors;
                }

                //copy out available sectors and return them
                Array.Copy(_allocatableSectors, allocated, length);
                return allocated;
            }

            /// <summary>
            /// Writes data into referenced sectors
            /// </summary>
            /// <param name="source">Source stream of data</param>
            /// <param name="targetSectors">Sector references to write</param>
            /// <param name="cypher">Cypher for encryption</param>
            /// <param name="key">Key for encryption</param>
            public void SecureWrite(Stream source, int[] targetSectors, ICypher cypher, byte[] key)
            {
                long totalBytes = source.Length;
                byte[] buffer = new byte[SectorSize];
                source.Position = 0;

                for (int i = 0; totalBytes > 0; i++)
                {
                    int byteCount = (int)Math.Min(SectorSize, totalBytes); //amount of bytes to write
                    long offset = _allocStartIndex + targetSectors[i] * SectorSize; //offset for sectors

                    source.Read(buffer, 0, byteCount); //read entire/partial sector

                    cypher.Encrypt(ref buffer, 0, key);

                    _directoryStream.Position = offset;
                    _directoryStream.Write(buffer, 0, buffer.Length); //write entire buffer

                    totalBytes -= byteCount;
                }

                AddSectors(ref _occupiedSectors, targetSectors);
                RemoveSectors(ref _allocatableSectors, targetSectors);
            }

            public void SecureRead(Stream output, long length, int[] targetSectors, ICypher cypher, byte[] key)
            {
                long totalBytes = length;
                byte[] buffer = new byte[SectorSize];
                output.Position = 0;

                for (int i = 0; totalBytes > 0; i++)
                {
                    int byteCount = (int)Math.Min(SectorSize, totalBytes); //amount of bytes to write
                    long offset = _allocStartIndex + targetSectors[i] * SectorSize; //offset for sectors

                    _directoryStream.Position = offset;
                    _directoryStream.Read(buffer, 0, buffer.Length); //read entire sector

                    cypher.Decrypt(ref buffer, 0, key);

                    output.Write(buffer, 0, byteCount); //write entire/partial buffer according to size

                    totalBytes -= byteCount;
                }
            }

            /// <summary>
            /// Marks sectors as allocatable.
            /// </summary>
            /// <param name="targetSectors">Sectors to be marked</param>
            /// <returns>Boolean, if deallocation was successfull</returns>
            public bool Deallocate(int[] targetSectors)
            {
                bool contains = ContainsSectors(_occupiedSectors, targetSectors); //check if target sectors were allocated
                if (contains)
                {
                    AddSectors(ref _allocatableSectors, targetSectors);
                    RemoveSectors(ref _occupiedSectors, targetSectors);
                }

                return contains;
            }

            private bool ContainsSectors(int[] array, int[] sectors)
            {
                int found = 0;
                for (int iter = 0; iter < array.Length; iter++)
                {
                    for (int i = 0; i < sectors.Length; i++)
                    {
                        if (array[iter] == sectors[i])
                        {
                            found++;
                            break;
                        }
                    }
                }

                return found == sectors.Length;
            }

            private void AddSectors(ref int[] array, int[] sectors)
            {
                int offset = array.Length;
                Array.Resize(ref array, array.Length + sectors.Length);
                sectors.CopyTo(array, offset);
                Array.Sort(array);
            }

            private void RemoveSectors(ref int[] array, int[] sectors)
            {
                int found = 0;
                for (int iter = 0; iter < array.Length; iter++)
                {
                    for (int i = 0; i < sectors.Length; i++)
                    {
                        if (array[iter] == sectors[i])
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
