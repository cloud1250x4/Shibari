// ShibariClient.cpp : Defines the exported functions for the DLL application.
//

#include "header.h"
#include "ShibariClient.h"


// This is an example of an exported variable
SHIBARICLIENT_API int nShibariClient=0;

// This is an example of an exported function.
SHIBARICLIENT_API int fnShibariClient(void)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see ShibariClient.h for the class definition
CShibariClient::CShibariClient()
{
    return;
}
