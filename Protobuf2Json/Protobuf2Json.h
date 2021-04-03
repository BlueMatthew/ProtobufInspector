#pragma once

#include "stdafx.h"

#ifdef PROTOBUF2JSON_EXPORTS
#define PROTOBUF2JSON_API __declspec(dllexport)
#else
#define PROTOBUF2JSON_API __declspec(dllimport)
#endif

#define ALWAYS_PRINT_ENUMS_AS_INTS 1
#define ALWAYS_PRINT_PRIMITIVE_FIELDS 2
#define ADD_WHITESPACES 4

typedef struct
{
	const char *protoPath;
	DWORD numberOfProtoFileNames;
	const char *protoFileNames;
	const char *messageTypeName;
	const unsigned char* messageData;
	DWORD lengthOfMessageData;
	DWORD options;
} PB2JSON_PROTOS_SRC_INFO;

typedef struct
{
	const char *descriptorSetFileName;
	const char *messageTypeName;
	const unsigned char* messageData;
	DWORD lengthOfMessageData;
	DWORD options;
} PB2JSON_DESCRIPTOR_SET_SRC_INFO;

extern "C" PROTOBUF2JSON_API BOOL ConvertRawMessageToJson(const unsigned char* messageData, DWORD lengthOfMessageData, DWORD options, char **outputString, DWORD *lengthOfOutputString);
extern "C" PROTOBUF2JSON_API BOOL ConvertMessageWithProtoFilesToJson(const PB2JSON_PROTOS_SRC_INFO *src, char **outputString, DWORD *lengthOfOutputString);
extern "C" PROTOBUF2JSON_API BOOL ConvertMessageWithDescriptorSetToJson(const PB2JSON_DESCRIPTOR_SET_SRC_INFO *src, char **outputString, DWORD *lengthOfOutputString);
extern "C" PROTOBUF2JSON_API BOOL FreeOutputString(char *outputString, DWORD lengthOfOutputString);

extern "C" PROTOBUF2JSON_API BOOL CacheDescriptorSet(const char *descriptorSetUrl, const char* descriptorSetFilePath, DWORD expireSeconds);

extern "C" PROTOBUF2JSON_API BOOL CacheHttpResponse(const char *url, const char* responseFilePath, unsigned char *headers, DWORD lengthOfHeaders, DWORD expireSeconds);
