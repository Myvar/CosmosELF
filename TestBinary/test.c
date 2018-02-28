
char *LFB = (char *)0xB8000;
int LFB_X = 0;
int LFB_Y = 0;
int VGA_WIDTH = 80;
int VGA_HEIGHT = 25;

char BG = 0x0f;

void tty_set_color(char color, char bg)
{
    BG = (char)color | (char)bg << 8;
}

void tty_reset_color()
{
    BG = 0x0f;
}


void tty_clear()
{
    int i = 0;

    for (i = 0; i < 80 * 25 * 2; i++)
    {
        LFB[i] = 0;
    }

    LFB_X = 0;
    LFB_Y = 0;
}



void tty_scroll_up()
{
    for (int y = 1; y < 20; ++y)
    {
        for (int x = 0; x < 80; ++x)
        {
            unsigned int offset = (y * 80 + x) * 2;
            unsigned int offset_up = offset - (80 * 2);

            LFB[offset_up + 0] = LFB[offset + 0];
            LFB[offset_up + 1] = LFB[offset + 1];
            LFB[offset + 0] = 0;
            LFB[offset + 1] = 0;
        }
    }
}

void tty_putc(char c)
{
    if (c == '\n')
    {
        LFB_X = 0;
        LFB_Y++;

        if (LFB_Y >= 20)
        {
            LFB_Y--;
            tty_scroll_up();
        }
    }
    else
    {
        if (c == '\t')
        {
            LFB_X += 4;
        }
        else
        {
            if (c == '\b')
            {
                int offset = ((LFB_Y * VGA_WIDTH) + LFB_X - 1) * 2;

                LFB[offset] = ' ';
                LFB[offset + 1] = 0x0F;

                LFB_X--;
            }
            else
            {
                int offset = ((LFB_Y * VGA_WIDTH) + LFB_X) * 2;

                LFB[offset] = c;
                LFB[offset + 1] = BG;

                LFB_X++;

                if (LFB_X >= 80)
                {
                    LFB_X = 0;
                    LFB_Y++;

                    if (LFB_Y >= 20)
                    {
                        LFB_Y--;
                        tty_scroll_up();
                    }
                }
            }
        }
    }
}

void tty_puts(char *str)
{
    while (*str != 0)
    {
        tty_putc(*str++);
    }
}