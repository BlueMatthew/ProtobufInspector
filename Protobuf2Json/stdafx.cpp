#include "stdafx.h"

#ifdef _DEBUG

#ifdef _WIN64
#pragma comment ( lib, "Protobuf\\libs\\debug\\x64\\libprotobufd.lib" )
#else
#pragma comment ( lib, "Protobuf\\libs\\debug\\x86\\libprotobufd.lib" )
#endif

#else

#ifdef _WIN64
#pragma comment ( lib, "Protobuf\\libs\\release\\x64\\libprotobuf.lib" )
#else
#pragma comment ( lib, "Protobuf\\libs\\release\\x86\\libprotobuf.lib" )
#endif

#endif