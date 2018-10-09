using System;
using System.Runtime.InteropServices;

namespace Emulator
{
    class MemoryMap
    {
        public readonly ushort PROGMEM;
        
        private ushort[] memory;
        public static readonly ushort progStart = 0x8000;

        public MemoryMap(ReadOnlySpan<byte> program)
        {
            memory = new ushort[0x10000];
            program.CopyTo(MemoryMarshal.AsBytes(memory.AsSpan(progStart)));
            PROGMEM = (ushort)(Array.IndexOf(MemoryMarshal.Cast<ushort, uint>(memory.AsSpan()).ToArray(), 0xFF_FF_FF_FF) * 2 + 2);
        }

        public ref ushort this[int i]
        {
            get
            {
                return ref memory[i];
            }
        }

        public ReadOnlySpan<byte> GetInstruction(ushort address)
        {
            Span<ushort> programSpace = memory.AsSpan(progStart);

            Span<uint> instructions = MemoryMarshal.Cast<ushort, uint>(programSpace);

            Span<uint> instruction = instructions.Slice(address, 1);

            return MemoryMarshal.Cast<uint, byte>(instruction);
        }
    }
}