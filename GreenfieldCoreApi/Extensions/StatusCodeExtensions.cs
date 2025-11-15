using System.Net;

namespace GreenfieldCoreApi.Extensions;

public static class StatusCodeExtensions
{
    public static int Get(this HttpStatusCode statusCode)
    {
        // StatusCodes from HttpStatusCode enum
        return (int)statusCode;
    }
}