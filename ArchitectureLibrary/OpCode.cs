
namespace ArchitectureLibrary
{
    public enum OpCode
    {
        nop = 0x00,
        ret = 0x01,

        add = 0x10,
        sub = 0x11,
        mul = 0x12,
        div = 0x13,
        mod = 0x14,
        eq = 0x15,
        lt = 0x16,
        gt = 0x17,
        lte = 0x18,
        gte = 0x19,
        nand = 0x1A,
        and = 0x1B,
        xor = 0x1C,
        or = 0x1D,

        jzi = 0x20,
        jnzi = 0x21,
        loadi = 0x22,
        stori = 0x23,
        not = 0x24,
        neg = 0x25,
        mov,

        shl = 0x30,
        shr,

        peek = 0x40,
        load = 0x41,
        stor = 0x42,
        jz = 0x43,
        jnz = 0x44,
        set = 0x45,
        inc = 0x46,
        dec = 0x47,
        loadprog = 0x48,

        call = 0x50,
        jmp,

        push = 0x60,
        pop,

        calli = 0x70,
        jmpi,
    }
}
