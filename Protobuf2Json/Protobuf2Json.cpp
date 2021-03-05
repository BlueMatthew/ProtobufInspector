// Protobuf2Json.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Protobuf2Json.h"
#include <string>
#include <vector>
#include <iostream>
#include <streambuf>

#include <google/protobuf/descriptor.h>
#include <google/protobuf/descriptor.pb.h>
#include <google/protobuf/descriptor_database.h>
#include <google/protobuf/dynamic_message.h>
#include <google/protobuf/io/coded_stream.h>
#include <google/protobuf/io/zero_copy_stream_impl.h>
#include <google/protobuf/io/tokenizer.h>
#include <google/protobuf/compiler/parser.h>
#include <google/protobuf/compiler/importer.h>
#include <google/protobuf/util/json_util.h>

#include "RawTextFormat.h"


using namespace google::protobuf;
using namespace google::protobuf::io;
using namespace google::protobuf::compiler;
using std::string;
using std::wstring;
using std::vector;
using std::cout;

BOOL PrintError(char **outputString, DWORD *lengthOfOutputString, const char *error);
BOOL FormatError(char **outputString, DWORD *lengthOfOutputString, const char *format, ...);

#define ERROR_NEW_MEMORY "Failed to allocate memory of size: %ld."
#define ERROR_NO_MESSAGE_TYPE "Failed to find message type: %s."
#define ERROR_NO_DESCRIPTOR "Failed to find descriptor of message type: %s."
#define ERROR_NEW_MESSAGE "Failed to create message object: %s."
#define ERROR_PARSING_MESSAGE "Failed to parse message data of type: %s."

inline google::protobuf::util::JsonPrintOptions MakeJsonPrintOptions(DWORD options)
{
	google::protobuf::util::JsonPrintOptions jpo;
	jpo.always_print_enums_as_ints = ((options & ALWAYS_PRINT_ENUMS_AS_INTS) == ALWAYS_PRINT_ENUMS_AS_INTS);
	jpo.always_print_primitive_fields = ((options & ALWAYS_PRINT_PRIMITIVE_FIELDS) == ALWAYS_PRINT_PRIMITIVE_FIELDS);
	jpo.add_whitespace = ((options & ADD_WHITESPACES) == ADD_WHITESPACES);

	return jpo;
}

struct InputMemoryBuffer : std::streambuf
{
	InputMemoryBuffer(const char* base, size_t size)
	{
		char* p(const_cast<char*>(base));
		this->setg(p, p, p + size);
	}
};

class Protobuf2JsonErrorCollector : public google::protobuf::compiler::MultiFileErrorCollector
{
	virtual void AddError(const std::string & filename, int line, int column, const std::string & message) {
		// define import error collector
		printf("%s, %d, %d, %s\n", filename.c_str(), line, column, message.c_str());
	}
};


struct InputMemoryStream : virtual InputMemoryBuffer, std::istream
{
	InputMemoryStream(const char* mem, size_t size) :
		InputMemoryBuffer(mem, size),
		std::istream(static_cast<std::streambuf*>(this))
	{
	}
};

BOOL ConvertRawMessageToJson(const unsigned char* messageData, DWORD lengthOfMessageData, DWORD options, char **outputString, DWORD *lengthOfOutputString)
{
	// HACK:  Define an EmptyMessage type to use for decoding.
	DescriptorPool pool;
	FileDescriptorProto file;
	file.set_name("empty_message.proto");
	file.add_message_type()->set_name("EmptyMessage");
	GOOGLE_CHECK(pool.BuildFile(file) != NULL);
	
	const Descriptor *descriptor = pool.FindMessageTypeByName("EmptyMessage");
	if (NULL == descriptor)
	{
		// FormatError(outputString, lengthOfOutputString, ERROR_NO_MESSAGE_TYPE, src->messageTypeName);
		return FALSE;
	}

	DynamicMessageFactory factory(&pool);
	const Message *protoType = factory.GetPrototype(descriptor);
	if (NULL == protoType)
	{
		return FALSE;
	}
	std::unique_ptr<Message> message(protoType->New());
	if (NULL == message.get())
	{
		return FALSE;
	}
	if (!message->ParseFromArray(reinterpret_cast<const void *>(messageData), lengthOfMessageData))
	{		
		return FALSE;
	}

	std::string jsonString;
	RawTextFormat::Printer printer;
	if (printer.PrintToString(*(message.get()), &jsonString))
	{
		if (NULL != outputString)
		{
			*outputString = (char *)::LocalAlloc(LPTR, jsonString.length() + 1);
			::CopyMemory(*outputString, jsonString.c_str(), jsonString.length());
			if (NULL != lengthOfOutputString) *lengthOfOutputString = (DWORD)jsonString.length();
		}

		return TRUE;
	}

	return FALSE;
}

BOOL ConvertMessageWithProtoFilesToJson(const PB2JSON_PROTOS_SRC_INFO *src, char **outputString, DWORD *lengthOfOutputString)
{
	if (src->numberOfProtoFileNames == 0 || src->messageTypeName == NULL || strlen(src->messageTypeName) == 0)
	{
		return ConvertRawMessageToJson(src->messageData, src->lengthOfMessageData, src->options, outputString, lengthOfOutputString);
	}

	DiskSourceTree sourceTree;
	sourceTree.MapPath("", src->protoPath);
	Protobuf2JsonErrorCollector errorCollector;

	Importer importer(&sourceTree, &errorCollector);
	const char *protoFileNames = src->protoFileNames;

	for (DWORD idx = 0; idx < src->numberOfProtoFileNames; idx++)
	{
		std::string filename = protoFileNames;

		const FileDescriptor* fileDescriptor = importer.Import(protoFileNames);
		if (NULL == fileDescriptor)
		{
		}

		protoFileNames += strlen(protoFileNames) + 1;
		if (NULL == protoFileNames)
		{
			break;
		}
	}

	const Descriptor *descriptor = importer.pool()->FindMessageTypeByName(src->messageTypeName);
	if (NULL == descriptor)
	{
		FormatError(outputString, lengthOfOutputString, ERROR_NO_MESSAGE_TYPE, src->messageTypeName);
		return FALSE;
	}

	DynamicMessageFactory factory;
	const Message *message = factory.GetPrototype(descriptor);

	Message *msg = message->New();
	if (NULL == msg)
	{
		FormatError(outputString, lengthOfOutputString, ERROR_NEW_MESSAGE, src->messageTypeName);
		return FALSE;
	}

	InputMemoryStream stm((const char *)src->messageData, (size_t)src->lengthOfMessageData);
	if (!msg->ParseFromIstream(&stm))
	{
		delete msg;
		FormatError(outputString, lengthOfOutputString, ERROR_PARSING_MESSAGE, src->messageTypeName);
		return FALSE;
	}

	string jsonString;
	google::protobuf::util::Status status = google::protobuf::util::MessageToJsonString(*msg, &jsonString, MakeJsonPrintOptions(src->options));
	delete msg;
	if (!status.ok())
	{
		PrintError(outputString, lengthOfOutputString, status.error_message().as_string().c_str());
		return FALSE;
	}
	if (NULL != outputString)
	{
		*outputString = (char *)::LocalAlloc(LPTR, jsonString.length() + 1);
		::CopyMemory(*outputString, jsonString.c_str(), jsonString.length());
		if (NULL != lengthOfOutputString) *lengthOfOutputString = (DWORD)jsonString.length();
	}

	return TRUE;
}

BOOL ConvertMessageWithDescriptorSetToJson(const PB2JSON_DESCRIPTOR_SET_SRC_INFO *src, char **outputString, DWORD *lengthOfOutputString)
{
	if (src->descriptorSetFileName == NULL || strlen(src->descriptorSetFileName) == 0 || src->messageTypeName == NULL || strlen(src->messageTypeName) == 0)
	{
		return ConvertRawMessageToJson(src->messageData, src->lengthOfMessageData, src->options, outputString, lengthOfOutputString);
	}

	HANDLE hFile = CreateFileA(src->descriptorSetFileName, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
	{
		return FALSE;
	}

	DWORD dwFileSize = GetFileSize(hFile, NULL);
	DWORD dwBytesRead = 0;

	unsigned char *descriptorData = new unsigned char[dwFileSize];
	if (NULL == descriptorData)
	{
		CloseHandle(hFile);
		FormatError(outputString, lengthOfOutputString, ERROR_NEW_MEMORY, dwFileSize);
		return FALSE;
	}
	BOOL bResult = ReadFile(hFile, descriptorData, dwFileSize, &dwBytesRead, NULL);
	CloseHandle(hFile);
	if (!bResult)
	{
		delete[] descriptorData;
		return FALSE;
	}

	google::protobuf::DescriptorPool pool;

	{
		CodedInputStream input(descriptorData, (int)dwFileSize);

		google::protobuf::FileDescriptorSet descriptors;
		descriptors.ParseFromCodedStream(&input);

		for (auto i = 0; i < descriptors.file_size(); ++i)
		{
			pool.BuildFile(descriptors.file(i));
		}
	}
	delete[] descriptorData;

	google::protobuf::DynamicMessageFactory factory(&pool);

	const Descriptor *descriptor = pool.FindMessageTypeByName(src->messageTypeName);
	if (NULL == descriptor)
	{
		FormatError(outputString, lengthOfOutputString, ERROR_NO_DESCRIPTOR, src->messageTypeName);
		return FALSE;
	}

	const Message *message = factory.GetPrototype(descriptor);

	Message *msg = message->New();
	if (NULL == msg)
	{
		FormatError(outputString, lengthOfOutputString, ERROR_NEW_MESSAGE, src->messageTypeName);
		return FALSE;
	}

	InputMemoryStream stm((const char *)src->messageData, (size_t)src->lengthOfMessageData);
	if (!msg->ParseFromIstream(&stm))
	{
		delete msg;
		FormatError(outputString, lengthOfOutputString, ERROR_PARSING_MESSAGE, src->messageTypeName);
		return FALSE;
	}

	string jsongString;
	google::protobuf::util::Status status = google::protobuf::util::MessageToJsonString(*msg, &jsongString, MakeJsonPrintOptions(src->options));
	if (!status.ok())
	{
		PrintError(outputString, lengthOfOutputString, status.error_message().as_string().c_str());
	}
	if (NULL != outputString)
	{
		*outputString = (char *)::LocalAlloc(LPTR, jsongString.length() + 1);
		if (NULL == *outputString)
		{
			FormatError(outputString, lengthOfOutputString, ERROR_NEW_MEMORY, jsongString.length() + 1);
		}
		::CopyMemory(*outputString, jsongString.c_str(), jsongString.length());
		if (NULL != lengthOfOutputString) *lengthOfOutputString = (DWORD)jsongString.length();
	}

	delete msg;

	return TRUE;
}

BOOL PrintError(char **outputString, DWORD *lengthOfOutputString, const char *error)
{
	if (NULL != outputString)
	{
		size_t errorLength = strlen(error);
		*outputString = (char *)::LocalAlloc(LPTR, errorLength + 1);
		if (NULL == *outputString)
		{
			return FALSE;
		}

		*outputString[errorLength] = 0;
		::CopyMemory(*outputString, error, errorLength);
	}

	return TRUE;
}

BOOL FormatError(char **outputString, DWORD *lengthOfOutputString, const char *format, ...)
{
	if (NULL != outputString)
	{
		size_t bufferLength = strlen(format) * 4;
		*outputString = (char *)::LocalAlloc(LPTR, bufferLength + 1);
		if (NULL == *outputString)
		{
			return FALSE;
		}
		if (NULL != lengthOfOutputString)
		{
			*lengthOfOutputString = (DWORD)bufferLength;
		}
		(*outputString)[bufferLength] = 0;

		va_list args;
		va_start(args, format);
		vsnprintf(*outputString, bufferLength, format, args);
		va_end(args);
	}

	return TRUE;
}


BOOL FreeOutputString(char *outputString, DWORD lengthOfOutputString)
{
	if (NULL != outputString)
	{
		::LocalFree((HLOCAL)outputString);
	}

	return TRUE;
}