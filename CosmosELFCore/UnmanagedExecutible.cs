using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core.Memory.Old;

namespace CosmosELFCore
{
    public unsafe class UnmanagedExecutible
    {
        private MemoryStream _stream;
        private ElfFile _elf;
        private byte* _finalExecutible;

        public UnmanagedExecutible(byte* elfbin)
        {
            _stream = new MemoryStream(elfbin);
        }

        public void Load()
        {
            _elf = new ElfFile(_stream);

            /*
             * 1. determin the total size of the final loaded sections
             * 2. maloc some space for them and allocate them
             * 3. update headers location information
             */

            //calcualte final size
            uint totalSize = 0;
            foreach (var header in _elf.SectionHeaders)
            {
                if ((header.Flag & SectionAttributes.Alloc) == SectionAttributes.Alloc)
                {
                    totalSize += header.Size;
                }
            }

            //alocate final size
            _finalExecutible = (byte*) Heap.MemAlloc(totalSize);

            var stream = new MemoryStream(_finalExecutible);
            var writer = new BinaryWriter(stream);

            foreach (var header in _elf.SectionHeaders)
            {
                if ((header.Flag & SectionAttributes.Alloc) != SectionAttributes.Alloc) continue;

                //read the data from the orginal file
                var reader = new BinaryReader(_stream);
                reader.BaseStream.Posistion = header.Offset;

                //update the meta data
                header.Offset = writer.BaseStream.Posistion;

                //write the data from the old file into the loaded executible
                for (int i = 0; i < header.Size; i++)
                {
                    writer.Write(reader.ReadByte());
                }
            }
        }

        public void Link()
        {
            foreach (var rel in _elf.RelocationInformation)
            {
                var addr = (uint) _finalExecutible +
                           _elf.SectionHeaders[(int) _elf.SectionHeaders[rel.Section].Info].Offset;
                var refr = (uint*) (addr + rel.Offset);
                var symval = _elf.Symbols[(int) rel.Symbol].Value;

                var memOffset = (uint) _finalExecutible +
                                _elf.SectionHeaders[_elf.Symbols[(int) rel.Symbol].Shndx].Offset;
//
//                Console.WriteLine($"addr: {addr}, refr: {*refr}, symval: {symval}, memoffset: {memOffset}");
//                Console.WriteLine("(symval + *refr) + memOffset");
//                Console.WriteLine($"{symval} + {*refr} + {memOffset} = {(symval + *refr) + memOffset}");

                switch (rel.Type)
                {
                    case RelocationType.R38632:
                        *refr = (symval + *refr) + memOffset; // Symbol + Offset
                        break;
                    case RelocationType.R386Pc32:
                        *refr = (symval + *refr - (uint) refr) + memOffset; // Symbol + Offset - Section Offset
                        break;
                    case RelocationType.R386None:
                        //nop
                        break;
                    default:
                        Console.WriteLine($"Error RelocationType({(int) rel.Type}) not implmented");
                        break;
                }
            }
        }

        public uint Invoke(string function)
        {
            for (int i = 0; i < _elf.Symbols.Count; i++)
            {
                if (function == _elf.Symbols[i].Name)
                {
                    Invoker.Offset =
                        (uint) _finalExecutible + _elf.Symbols[i].Value +
                        _elf.SectionHeaders[_elf.Symbols[i].Shndx].Offset;

                    Invoker.StoreState();
                    Invoker.CallCode();
                    Invoker.LoadState();

                    break;
                }
            }

            return Invoker.Stack[0];
        }
    }
}