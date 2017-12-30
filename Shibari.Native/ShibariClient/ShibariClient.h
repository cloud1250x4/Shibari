// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the SHIBARICLIENT_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// SHIBARICLIENT_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef SHIBARICLIENT_EXPORTS
#define SHIBARICLIENT_API __declspec(dllexport)
#else
#define SHIBARICLIENT_API __declspec(dllimport)
#endif

// This class is exported from the ShibariClient.dll
class SHIBARICLIENT_API CShibariClient {
public:
	CShibariClient(void);
	// TODO: add your methods here.
};

extern SHIBARICLIENT_API int nShibariClient;

SHIBARICLIENT_API int fnShibariClient(void);
