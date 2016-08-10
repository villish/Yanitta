# Yanitta offset's finder
import re

def AsByteStr(s, appendZero=True):
    return (" ".join("%02X" % x for x in map(ord, s))) + (" 00" if appendZero else "")

def GetClientBuild():
    ref = FindBinary(0, SEARCH_DOWN, AsByteStr("<Version>", False));
    if ref == BADADDR:
        raise BaseException("Can't find offset for "+name);
    verStr = GetString(ref, -1, ASCSTR_C);
    result = re.findall("<Version> \d\.\d\.\d\.(\d{5})", verStr);
    #print(verStr)
    if len(result)==0:
        raise BaseException("Build not found in: " + verStr);
    return result[0];

def GetLuaFuncPtr(name):
    ref = FindBinary(0, SEARCH_DOWN, AsByteStr(name));
    if ref == BADADDR:
        raise BaseException("Can't find offset for "+name);
    ptr = Dword(DnextB(ref,0)+4);
    if ptr == 0 or ptr == BADADDR:
        raise BaseException("Can't function for "+name);
    return ptr;

def GetPlayerNameOffset():
    ptr = GetLuaFuncPtr("UnitName");
    cnt = 0
    while GetMnem(ptr) != "call" or cnt != 5:
        ptr = NextHead(ptr);
        if GetMnem(ptr) == "call":
            cnt = cnt + 1;
    offset = GetOperandValue(ptr, 0);
    offset = NextHead(offset);
    offset = GetOperandValue(offset, 1)
    print("GetPlayerNameOffset: 0x%08X" % offset);
    return offset
    
def GetPlayerClassOffset():
    ptr = GetLuaFuncPtr("UnitClass");
    while GetMnem(ptr) != "call" or GetMnem(GetOperandValue(ptr, 0)) != "mov" or GetOpnd(GetOperandValue(ptr, 0),0) != "al":
        ptr = NextHead(ptr);
    offset = GetOperandValue(GetOperandValue(ptr, 0), 1);
    print("GetPlayerClassOffset: 0x%08X" % offset);
    return offset
    
def GetIsInWorldOffset():
    ptr = GetLuaFuncPtr("IsPlayerInWorld");
    while GetMnem(ptr) != "movzx" and GetOpnd(ptr, 0) != "eax":
        ptr = NextHead(ptr);
    offset = GetOperandValue(ptr, 1);
    print("GetIsInWorldOffset: 0x%08X" % offset);
    return offset;

# RunScript -> FrameScript_Ececute -> FrameScript_EcecuteBuffer
def GetFrameScriptExecOffset():
    ref = FindBinary(0, SEARCH_DOWN, AsByteStr("compat.lua"));
    ptr = DnextB(ref, 0);
    while GetMnem(ptr) != "call":
        ptr = NextHead(ptr);
        
    ptr = GetOperandValue(ptr, 0);
    while GetMnem(ptr) != "call" or not GetOpnd(PrevHead(ptr),0).startswith("[ebp+") or GetOpnd(PrevHead(PrevHead(ptr)),0)!="eax":
        ptr = NextHead(ptr);
    offset = GetOperandValue(ptr, 0);
    print("GetFrameScriptExecOffset: 0x%08X" % offset);
    return offset;    

# Framescript_Execute (1 xref)
def GetInjOffset():
    ref = FindBinary(0, SEARCH_DOWN, AsByteStr("compat.lua"));
    ptr = DnextB(ref, 0);
    offset = FirstFuncFchunk(ptr);
    print("GetInjOffset: 0x%08X" % offset);
    return offset;

# fish bot
    
# Script_GetPlayerFacing -> ClntObjMgrObjectPtr -> s_curMgr
def GetObjectMgrOffset():
    ptr = GetLuaFuncPtr("GetPlayerFacing");
    while GetMnem(ptr) != "call" or not GetMnem(PrevHead(ptr)).startswith("movs"):# because instruction displayed as 'movsd'
        ptr = NextHead(ptr);
    ptr = GetOperandValue(ptr, 0);
    while GetMnem(ptr) != "cmp":
        ptr = NextHead(ptr);
    offset = GetOperandValue(ptr, 0);
    print("GetObjectMgrOffset: 0x%08X" % offset);
    return offset;

# Script_InteractUnit (guid pointer CGGameUI__m_currentObjectTrack)
def GetObjTrackOffset():
    ptr = GetLuaFuncPtr("InteractUnit");
    while GetMnem(ptr) != "mov" or GetOpType(ptr, 1) != 5:
        ptr = NextHead(ptr);
    offset = GetOperandValue(ptr, 1);
    print("GetObjTrackOffset: 0x%08X" % offset);
    return offset;
    
# Script_IsTestBuild lua_pushboolean(a1, 0); (push 0/1 instruction)
def GetTestClientOffset():
    ptr = GetLuaFuncPtr("IsTestBuild");
    while GetMnem(ptr) != "push" or GetOpnd(ptr, 0) != "0":
        ptr = NextHead(ptr);
    offset = ptr + 1;
    print("GetTestClientOffset: 0x%08X" % offset);
    return offset;

# Script_SetPOIIconOverlapDistance -> CGQuestLog::m_questIconOverlapDist (float variable default = 12.00)
def GetFishEnblOffset():
    ptr = GetLuaFuncPtr("SetPOIIconOverlapDistance");
    while GetMnem(ptr) != "fstp":
        ptr = NextHead(ptr);
    offset = GetOperandValue(ptr, 1);
    print("GetFishEnblOffset: 0x%08X" % offset);
    return offset;
    
UnitName=GetPlayerNameOffset();
UnitClas=GetPlayerClassOffset();
IsInGame=GetIsInWorldOffset();
ExecBuff=GetFrameScriptExecOffset();
Inj_Addr=GetInjOffset();

ObjectMr=GetObjectMgrOffset();
ObjTrack=GetObjTrackOffset();
TestClnt=GetTestClientOffset();
FishEnbl=GetFishEnblOffset();

print("["+GetClientBuild()+"]")
print("# Yanitta offset's")
print("PlayerName      = 0x%08X" % (UnitName-0x400000))
print("PlayerClass     = 0x%08X" % (UnitClas-0x400000))
print("IsInGame        = 0x%08X" % (IsInGame-0x400000))
print("ExecuteBuffer   = 0x%08X" % (ExecBuff-0x400000))
print("InjectAddress   = 0x%08X" % (Inj_Addr-0x400000))
print("")
print("# Fish bot offset's")
print("ObjectMgr       = 0x%08X" % (ObjectMr-0x400000))
print("ObjectTrack     = 0x%08X" % (ObjTrack-0x400000))
print("TestClient      = 0x%08X" % (TestClnt-0x400000))
print("FishEnable      = 0x%08X" % (FishEnbl-0x400000))
print("")
print("# Field offset's")
print("FirstObject     = 0xD8");
print("NextObject      = 0xD0");
print("ObjectType      = 0x20");
print("PlayerGuid      = 0x00");
print("VisibleGuid     = 0x00");
print("AnimationState  = 0x68");
print("CreatedBy       = 0x30");