ENTRY(main)
OUTPUT_FORMAT(elf32-i386)

SECTIONS {
	. = 0x0;
	.text : { *(.text*)	}	
	.data : { *(.data*) }
	.bss : { *(.bss*) }
	/DISCARD/ : {*(.eh_frame) *(.note*) *(.comment)}
}