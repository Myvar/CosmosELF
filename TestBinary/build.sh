#!/bin/bash
gcc -c test.c -o ./test.o -masm=intel -m32 -std=gnu99 -fno-builtin -ffreestanding -w -O2 -Wall -Wextra -nostartfiles -nostdlib -fno-stack-protector 
ld -m elf_i386 -r -T linker.ld -o ./test.so ./test.o 