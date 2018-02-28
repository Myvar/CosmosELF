using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosELFCore
{
    public unsafe class ArgumentWriter
    {
        private BinaryWriter _writer;

        public ArgumentWriter()
        {
            //clear call stack
            for (int k = 0; k < 1024; k++)
            {
                ((byte*) Invoker.Stack)[k] = 0;
            }

            _writer = new BinaryWriter(new MemoryStream((byte*) Invoker.Stack));
            _writer.BaseStream.Posistion = 50;
        }


        public void Push(char c) => _writer.Write(c);

        public void Push(byte c) => _writer.Write(c);

        public void Push(short c) => _writer.Write(c);

        public void Push(int c) => _writer.Write(c);
    }
}