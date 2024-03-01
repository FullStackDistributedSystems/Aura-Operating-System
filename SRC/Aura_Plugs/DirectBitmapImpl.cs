﻿using Cosmos.HAL.BlockDevice;
using IL2CPU.API.Attribs;
using System.Reflection;
using System;
using XSharp.Assembler;
using static XSharp.XSRegisters;
using XSharp;
using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.System.Graphics;
using XSharp.Assembler.x86;
using System.Security.Cryptography;
using XSharp.Assembler.x86.SSE;
using static System.Net.Mime.MediaTypeNames;

namespace Cosmos.System_Plugs.System.Drawing
{
    [Plug(Target = typeof(DirectBitmap))]
    public unsafe static class DrawImageAlphaImpl
    {
        [PlugMethod(Assembler = typeof(AlphaBltSSEASM))]
        public static void AlphaBltSSE(byte* dst, byte* src, int w, int h, int wmul4) => throw new NotImplementedException();

        [PlugMethod(Assembler = typeof(BrightnessASM))]
        public static void BrightnessSSE(byte* image, int len) => throw new NotImplementedException();
    }

    public class BrightnessASM : AssemblerMethod
    {
        private const int ImgDisplacement = 12;
        private const int LenDisplacement = 8;

        //public static void BrightnessSSE(byte* image, int len)
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // Copy Src to ESI
            XS.Set(ECX, EBP, sourceIsIndirect: true, sourceDisplacement: LenDisplacement);
            // Copy Dst to EDI
            XS.Set(EDI, EBP, sourceIsIndirect: true, sourceDisplacement: ImgDisplacement);

            XS.Label("start_loop");

            XS.LiteralCode("test ecx, ecx");
            XS.LiteralCode("jz end_loop");
            XS.LiteralCode("mov al, [edi + 3]");
            XS.LiteralCode("cmp al, 0xFF");
            XS.LiteralCode("je skip_alpha_adjust");
            XS.LiteralCode("mov byte [edi + 3], 0xFF");

            XS.Label("skip_alpha_adjust");

            XS.Add(EDI, 4);
            XS.Decrement(ECX);
            XS.Jump("start_loop");

            XS.Label("end_loop");
        }
    }

    public class AlphaBltSSEASM : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.LiteralCode("        mov         edi, [ebp + 24]"); // Move the address of index 0 of dst (unsigned char*(Because this is a pointer, it's an int, being compiled as x86, so 32 bits, or 4 bytes)) into EDI destination index register, for string operations
            XS.LiteralCode("        mov         esi, [ebp + 20]"); // Move the address of index 0 of src (unsigned char*(Because this is a pointer, it's an int, being compiled as x86, so 32 bits, or 4 bytes)) into ESI source index register, for string operations
            XS.LiteralCode("        mov         edx, [ebp + 12]"); // Move the address of h (int, 32 bits(4 bytes)) into 32-bit(4 byte) EDX register. EDX now points to h.
            XS.LiteralCode("        pxor        mm6,mm6"); // Performs a logical XOR operation on mm6 and mm6, then stores the result in mm6, a 64 bit MMX register. Because XORing itself, this initializes to 0.
            XS.LiteralCode("        pxor        mm7,mm7"); // Performs a logical XOR operation on mm7 and mm7, then stores the result in mm7, a 64 bit MMX register. Because XORing itself, this initializes to 0.
            XS.LiteralCode("        xor         eax,eax"); // Performs a logical XOR operation on eax and eax, then stores the result in eax, a 32 bit x86 register. Because XORing itself, this initializes to 0.
            XS.LiteralCode("scan_loop:"); // Label this line to be jumped to.
            XS.LiteralCode("        mov ecx, [ebp + 16]"); // Move address of w into ECX 32 bit(4 byte) register. ECX is our count register, it will decrement while looping. ECX now points to w.
            XS.LiteralCode("        xor ebx, ebx"); // Performs a logical XOR operation on EBX and EBX, then stores the result in EBX, a 32 bit x86 register. Because XORing itself, this initializes to 0.
            XS.LiteralCode("pix_loop:"); // Label this line to be jumped to.
            XS.LiteralCode("        movq mm4,[esi + ebx * 8]"); // mm0 = src (RG BA RG BA) // Moves a quadword(8 bytes) of data from address of esi+ebx*8 to mm4. ESI is src's starting address+EBX which is an offset counter, of 8 bytes. EBX is a counter.
            XS.LiteralCode("        movq mm5,[edi + ebx * 8]"); // mm1 = dst (RG BA RG BA) // Moves a quadword(8 bytes) of data from address of edi+ebx*8 to mm5. ESD is dst's starting address+EBX which is an offset counter, of 8 bytes. EBX is a counter.
            // FIRST PIXEL
            XS.LiteralCode("        movq mm0, mm4"); // mm0 = 00 00 RG BA // Moves the quadword(8 bytes) contents of mm4 into mm0. This will be RGBA, with each channel being 2 bytes, with a value of 0-255.
            XS.LiteralCode("        movq mm1, mm5"); // mm1 = 00 00 RG BA // Moves the quadword(8 bytes) contents of mm5 into mm1. This will be RGBA, with each channel being 2 bytes, with a value of 0-255.
            XS.LiteralCode("        punpcklbw mm0, mm6"); // mm0 = (0R 0G 0B 0A) // Interleaves low order bytes of mm0 and mm6 together, into mm0. http://qcd.phys.cmu.edu/QCDcluster/intel/vtune/reference/vc265.htm
            XS.LiteralCode("        punpcklbw mm1, mm7"); // mm0 = (0R 0G 0B 0A) // Interleaves low order bytes of mm1 and mm7 together, into mm1. http://qcd.phys.cmu.edu/QCDcluster/intel/vtune/reference/vc265.htm
            XS.LiteralCode("        pshufw mm2, mm0,0ffh"); // mm2 = 0A 0A 0A 0A   // Shuffles the words(2 bytes) from mm0 into mm2 using the third operand to define their placement in mm2. http://qcd.phys.cmu.edu/QCDcluster/intel/vtune/reference/vc254.htm -- Uses mm1 instead of mm2 for Question 2.
            XS.LiteralCode("        movq        mm3,mm1"); // mm3 = mm1		  // Moves the quadword(8 bytes) contents of mm1 to mm3.
            XS.LiteralCode("        psubw       mm0,mm1"); // mm0 = mm0 - mm1	  // Subtract packed word integers in mm1 from packed word integers in mm0. https://www.felixcloutier.com/x86/psubb:psubw:psubd
            XS.LiteralCode("        psllw       mm3,8"); // mm3 = mm1 * 256	  // Shift bits in mm3 left 8 places (Multiply by 256). https://www.felixcloutier.com/x86/psllw:pslld:psllq
            XS.LiteralCode("        pmullw mm0, mm2"); // mm0 = (src-dst)*alpha // Multiply mm2 and mm0, and store low order bits of result in mm0 https://docs.oracle.com/cd/E19120-01/open.solaris/817-5477/eojdc/index.html
            XS.LiteralCode("        paddw mm0, mm3"); // mm0 = (src-dst)*alpha+dst*256 // Add packed word integers mm3 and mm0, into mm0. https://www.felixcloutier.com/x86/paddb:paddw:paddd:paddq
            XS.LiteralCode("        psrlw mm0,8"); // mm0 = ((src - dst) * alpha + dst * 256) / 256 // Shift words in mm0 right by 8 (Divide by 256). https://www.felixcloutier.com/x86/psrlw:psrld:psrlq
            // SECOND PIXEL
            XS.LiteralCode("        punpckhbw mm5, mm7"); // mm5 = (0R 0G 0B 0A) // Unpack and interleave high-order bytes from mm5 and mm7 into mm5. https://www.felixcloutier.com/x86/punpckhbw:punpckhwd:punpckhdq:punpckhqdq
            XS.LiteralCode("        punpckhbw mm4, mm6"); // mm4 = (0R 0G 0B 0A) // Unpack and interleave high-order bytes from mm4 and mm6 into mm4. https://www.felixcloutier.com/x86/punpckhbw:punpckhwd:punpckhdq:punpckhqdq
            XS.LiteralCode("        movq mm3, mm5"); // mm3 = mm5		  // Moves the quadword(8 bytes) contents of mm5 to mm3.
            XS.LiteralCode("        pshufw mm2, mm4,0ffh"); // mm2 = 0A 0A 0A 0A  // Shuffles the words(2 bytes) from mm4 into mm2 using the third operand to define their placement in mm2. http://qcd.phys.cmu.edu/QCDcluster/intel/vtune/reference/vc254.htm -- Uses mm5 instead of mm4 for Question 2.
            XS.LiteralCode("        psllw       mm3,8"); // mm3 = mm5 * 256		// Shift bits in mm3 left 8 places (Multiply by 256). https://www.felixcloutier.com/x86/psllw:pslld:psllq
            XS.LiteralCode("        psubw mm4, mm5"); // mm4 = mm4 - mm5		// Subtract packed word integers in mm5 from packed word integers in mm4. https://www.felixcloutier.com/x86/psubb:psubw:psubd
            XS.LiteralCode("        pmullw mm4, mm2"); // mm4 = (src-dst)*alpha // Multiply mm2 and mm4, and store low order bits of result in mm4 https://docs.oracle.com/cd/E19120-01/open.solaris/817-5477/eojdc/index.html
            XS.LiteralCode("        paddw mm4, mm3"); // mm4 = (src-dst)*alpha+dst*256 // Add packed word integers mm4 and mm3, into mm4. https://www.felixcloutier.com/x86/paddb:paddw:paddd:paddq
            XS.LiteralCode("        psrlw mm4,8"); // mm4 = ((src - dst) * alpha + dst * 256) / 256 // Shift words in mm4 right by 8 (Divide by 256). https://www.felixcloutier.com/x86/psrlw:psrld:psrlq
            XS.LiteralCode("        packuswb mm0, mm4"); // mm0 = RG BA RG BA // Converts 4 signed word integers from mm0 and 4 signed word integers from mm4 into 8 unsigned byte integers in mm0 using unsigned saturation. (Set to min of range if below, set to max of range if above)
            XS.LiteralCode("        movq[edi + ebx * 8],mm0"); // dst = mm0		// Moves the quadword(8 bytes) contents of mm0 to address indicated by edi+ebx*8. EDI is dst's starting address+EBX which is an offset counter, of 8 bytes. EBX is a counter.
            XS.LiteralCode("        inc         ebx"); // increment the value stored at the address of ebx	// Increment EBX
            XS.LiteralCode("        loop        pix_loop"); // loop back to pix_loop label. https://docs.oracle.com/cd/E19455-01/806-3773/instructionset-72/index.html Loop also decrements ECX, and continues without looping if that register is zero.
            XS.LiteralCode("        mov         ebx, [ebp + 8]"); // Move the address of wmul4 into ebx, creating a pointer to wmul4 in ebx.
            XS.LiteralCode("        add         esi, ebx"); // Add the contents of EBX into ESI
            XS.LiteralCode("        add         edi, ebx"); // Add the contents of EBX into EDI
            XS.LiteralCode("        dec         edx"); // Decrement EDX
            XS.LiteralCode("        jnz         scan_loop"); // Jump to label if zero flag is not set. Not entirely sure how and where the zero flag is being set or cleared here.
        }
    }
}