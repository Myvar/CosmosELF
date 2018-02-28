using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public unsafe class ElfFile
    {
        public Elf32Ehdr ElfHeader { get; set; }
        public List<Elf32Shdr> SectionHeaders { get; set; } = new List<Elf32Shdr>();
        public List<Elf32Rel> RelocationInformation { get; set; } = new List<Elf32Rel>();
        public List<Elf32Sym> Symbols { get; set; } = new List<Elf32Sym>();
        private readonly uint _stringTableOffset;

        public string ResolveName(uint offset, MemoryStream stream)
        {
            var old = stream.Posistion;
            stream.Posistion = _stringTableOffset + offset;
            var reader = new BinaryReader(stream);
            var s = reader.ReadString();
            stream.Posistion = old;
            return s;
        }

        public ElfFile(MemoryStream stream)
        {
            //load main file header
            ElfHeader = new Elf32Ehdr((Elf32_Ehdr*) stream.Pointer);

            //load section headers
            for (int i = 0; i < ElfHeader.Shnum; i++)
            {
                var x = new Elf32Shdr(
                    (Elf32_Shdr*) (stream.Pointer + ElfHeader.Shoff + i * ElfHeader.Shentsize));
                if (x.Type == SectionType.StringTable) _stringTableOffset = x.Offset;
                SectionHeaders.Add(x);
            }

            //now we can load names into symbols and process sub data
            for (var index = 0; index < SectionHeaders.Count; index++)
            {
                var sectionHeader = SectionHeaders[index];
                sectionHeader.Name = ResolveName(sectionHeader.NameOffset, stream);

                switch (sectionHeader.Type)
                {
                    case SectionType.Relocation:
                        for (int i = 0; i < sectionHeader.Size / sectionHeader.Entsize; i++)
                        {
                            RelocationInformation.Add(new Elf32Rel(
                                    (Elf32_Rel*) (stream.Pointer + sectionHeader.Offset + i * sectionHeader.Entsize))
                                {Section = index});
                        }

                        break;
                    case SectionType.SymbolTable:
                        for (int i = 0; i < sectionHeader.Size / sectionHeader.Entsize; i++)
                        {
                            var x = new Elf32Sym(
                                (Elf32_Sym*) (stream.Pointer + sectionHeader.Offset + i * sectionHeader.Entsize));
                            x.Name = ResolveName(x.NameOffset, stream);
                            Symbols.Add(x);
                        }

                        break;
                }
            }
        }
    }
}