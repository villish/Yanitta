#include <stdio.h>
#include <tchar.h>
#include <Windows.h>

typedef struct {
    void* vTable;
    BYTE* buffer;
    DWORD mBase;
    DWORD alloc;
    DWORD size;
    DWORD read;
} CDataStore;

static int testValue;

int __cdecl Test(char* text1, char* text2, int state);
unsigned int __cdecl Send2(CDataStore* packet);
int ExecLua(char* src, int len, char* path, int unk1, int unk2, long long unk3);

void Test2()
{
    printf("Ohhhoohh\n");
}

void Exec()
{
    testValue = ExecLua("printf(\"Hello\");", 22, "teldrassil.lua", 0, 0, 0x7fffff);
    printf("%i", testValue);
}

int _tmain(int argc, _TCHAR* argv[])
{
    testValue = 5;
    Exec();
    printf("%i", testValue);

    Send2(NULL);
    Test2();
    while (true)
    {
        Test("Test", "path", 0);
        Sleep(2000);
    }
	return 0;
}

int ExecLua(char* src, int len, char* path, int unk1, int unk2, long long unk3)
{
    printf("Src: %s, Len: %i, Path: %s, Unk1: %i, Unk2: %i, Unk3: %i\n",
        src, len, path, unk1, unk2, unk3);
    return 1;
}

int __cdecl Test(char* text1, char* text2, int state)
{
    printf("Text: %s, %s - %i\n", text1, text2, state);
    return 0;
}

unsigned int __cdecl Send2(CDataStore* packet)
{
    if (packet == NULL)
    {
        printf("Send empty packet!\n");
        return -2;
    }

    printf("Packet Size %i\n", packet->size);

    if (packet->size < 4)
    {
        printf("Detect empty packet!\n");
        return -1;
    }

    if (packet->size > 4)
    {
        printf("Data:");
        for (int i = 0; i < packet->size; i++)
        {
            printf(" %02X", *(BYTE*)(packet->buffer + i));
        }
    }
    printf("\n\n");
    return 0u;
}