#pragma once

#ifdef UTILITYNATIVE_EXPORTS
#define NATIVE_API __declspec(dllexport)
#else
#define UTILITYNATIVE_API __declspec(dllimport)
#endif

extern "C" NATIVE_API int huffman_uncompress(unsigned char* uncompr, unsigned long uncomprlen, unsigned char* compr, unsigned long comprlen);

extern "C" NATIVE_API int huffman_compress(unsigned char* compr, unsigned long comprlen, unsigned char* uncompr, unsigned long uncomprlen);