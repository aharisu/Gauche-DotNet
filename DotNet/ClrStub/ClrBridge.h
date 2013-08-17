/*
 * ClrBridge.h
 *
 * MIT License
 * Copyright 2013 aharisu
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 *
 * aharisu
 * foo.yobina@gmail.com
 */

#ifndef CLRBRIDGE_H
#define CLRBRIDGE_H

#include "ClrMethodCallStruct.h"

#ifdef DLL_EXPORT
#define DECDLL __declspec(dllexport)
#else
#define DECDLL __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

DECDLL void ReleaseClrObject(void* obj);
DECDLL void* ToClrObj(void* scmObj);

DECDLL int ClrEqualP(void* x, void* y);
DECDLL int ClrCompare(void* x, void* y);

DECDLL int ClrGetHash(void* obj);

DECDLL void* ClrGetEnumerator(void* obj);
DECDLL int ClrIsIterEnd(void* iter);
DECDLL void* ClrIterNext(void* iter);
DECDLL void ClrIterDispose(void* iter);

DECDLL void* ClrToGoshObj(void* obj);

DECDLL void* BooleanToClr(int boolean);
DECDLL int ClrToBoolean(void* obj);

DECDLL void* FixnumToClr(signed long int num);
#ifdef GAUCHE_H
DECDLL void* Int64ToClr(ScmInt64 num);
#else
DECDLL void* Int64ToClr(System::Int64 num);
#endif
DECDLL void* DoubleToClr(double num);
DECDLL void* ClrToNumber(void* obj);

DECDLL void* StringToClr(const char* str);
DECDLL void* ClrToGoshString(void* clrObj);

typedef enum {
    KIND_FIELD = 1,
    KIND_PROP = 1 << 1,
    KIND_FIELD_PROP = KIND_FIELD | KIND_PROP
}FieldPropKind;

DECDLL void ClrFieldPropSetClrObj(FieldPropKind kind, void* obj, const char* name, ObjWrapper* indexer, int numIndexer, void* clrObj);
DECDLL void ClrFieldPropSetScmObj(FieldPropKind kind, void* obj, const char* name, ObjWrapper* indexer, int numIndexer, void* scmObj);
DECDLL void ClrFieldPropSetInt(FieldPropKind kind, void* obj, const char* name, ObjWrapper* indexer, int numIndexer, int value);
DECDLL void ClrFieldPropSetString(FieldPropKind kind, void* obj, const char* name, ObjWrapper* indexer, int numIndexer, const char* value);

DECDLL void* ClrFieldPropGet(FieldPropKind kind, ObjWrapper* obj, const char* name, ObjWrapper* indexer, int numIndexer);

DECDLL void ClrEventAddGoshProc(void* obj, const char* name, void* goshProc);
DECDLL void ClrEventAddClrObj(void* obj, const char* name, void* clrObj);
DECDLL void ClrEventRemove(void* obj, const char* name, void* proc);

DECDLL int ClrReferenceAssembly(const char* assemblyName);
DECDLL void ClrUsingNamespace(const char* ns, void* module);

DECDLL void* ClrValidTypeName(const char* fullTypeName);

DECDLL void* ClrNewArray(TypeSpec* typeSpec, ObjWrapper* sizes, int numSizes);

DECDLL void* ClrNew(
                    TypeSpec* methodSpec
                    , ObjWrapper* args, int numArg
                    );

DECDLL void* ClrCallMethod(void* module
                         , TypeSpec* methodSpec
                         , void* obj, int isStatic
                         , ObjWrapper* args, int numArg
                         );

DECDLL int ClrIs(TypeSpec* typeSpec, void* obj);

DECDLL void* GetEnumObject(TypeSpec* enumTypeSpec, const char* enumObj);

// Util

DECDLL void* ClrPrint(void* clrObj);
DECDLL void* ClrGetTypeName(void* obj);
DECDLL void* ClrTypeSpecToTypeHandle(TypeSpec* spec);
DECDLL void ClrFreeTypeHandle(void* type);

DECDLL void* ClrTypeHandleToString(void* type);

DECDLL void* ClrMember(void* obj, int isStatic, const char* name);

#ifdef __cplusplus
}
#endif


#endif //CLRBRIDGE_H
