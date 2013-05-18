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

DECDLL void* BooleanToClr(int boolean);
DECDLL int ClrToBoolean(void* obj);

DECDLL void* FixnumToClr(signed long int num);
DECDLL int ClrToInt(void* obj);

DECDLL void* StringToClr(const char* str);
DECDLL void* ClrToGoshString(void* clrObj);

DECDLL void ClrPropSetClrObj(void* obj, const char* name, ObjWrapper* indexer, int numIndexer, void* clrObj);
DECDLL void ClrPropSetScmObj(void* obj, const char* name, ObjWrapper* indexer, int numIndexer, void* scmObj);
DECDLL void ClrPropSetInt(void* obj, const char* name, ObjWrapper* indexer, int numIndexer, int value);
DECDLL void ClrPropSetString(void* obj, const char* name, ObjWrapper* indexer, int numIndexer, const char* value);

DECDLL void* ClrPropGet(ObjWrapper* obj, const char* name, ObjWrapper* indexer, int numIndexer);

DECDLL void ClrFieldSetClrObj(void* obj, const char* name,  void* clrObj);
DECDLL void ClrFieldSetScmObj(void* obj, const char* name,  void* scmObj);
DECDLL void ClrFieldSetInt(void* obj, const char* name,  int value);
DECDLL void ClrFieldSetString(void* obj, const char* name,  const char* value);

DECDLL void* ClrFieldGet(void* obj, const char* name);

DECDLL void ClrEventAddGoshProc(void* obj, const char* name, void* goshProc);
DECDLL void ClrEventAddClrObj(void* obj, const char* name, void* clrObj);
DECDLL void ClrEventRemove(void* obj, const char* name, void* proc);

DECDLL int ClrReferenceAssembly(const char* assemblyName);

DECDLL void* ClrValidTypeName(const char* fullTypeName);

DECDLL void* ClrNewArray(TypeSpec* typeSpec, int size);

DECDLL void* ClrNew(
                    TypeSpec* methodSpec
                    , ObjWrapper* args, int numArg
                    );

DECDLL void* ClrCallMethod(
                         TypeSpec* methodSpec
                         , void* obj, int isStatic
                         , ObjWrapper* args, int numArg
                         );

DECDLL int ClrIs(TypeSpec* typeSpec, void* obj);

DECDLL void* GetEnumObject(TypeSpec* enumTypeSpec, const char* enumObj);

// Util

DECDLL void* ClrPrint(void* clrObj);
DECDLL void* ClrGetTypeName(void* obj);

DECDLL void* ClrMember(void* obj, int isStatic, const char* name);

#ifdef __cplusplus
}
#endif


#endif //CLRBRIDGE_H
