// ShibariClient.cpp : Defines the exported functions for the DLL application.
//

#include "header.h"
#include "ShibariClient.h"

#ifndef ZMQ_STATIC
#define ZMQ_STATIC
#endif
#include <zmq.hpp>

#include <string>

const std::string g_connectionString("tcp://localhost:43317");

typedef struct _SHIBARI_CLIENT_T
{
    zmq::context_t *ctx;
    zmq::socket_t *sub;

} SHIBARI_CLIENT;

SHIBARI_API BOOLEAN shibari_is_available(void)
{
    auto client = static_cast<PSHIBARI_CLIENT>(malloc(sizeof(SHIBARI_CLIENT)));

    client->ctx = new zmq::context_t(1);
    client->sub = new zmq::socket_t(*client->ctx, ZMQ_SUB);

    client->sub->connect(g_connectionString);

    return FALSE;
}
