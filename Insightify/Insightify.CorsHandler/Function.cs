using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Insightify.CorsHandler;

public class Function
{
    /// <summary>
    /// This function's ONLY purpose is to respond to browser OPTIONS
    /// preflight requests with the correct CORS headers.
    /// </summary>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("CORS Handler invoked for a preflight request.");
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Headers", "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token" },
                { "Access-Control-Allow-Methods", "POST,GET,OPTIONS" },
                { "Access-Control-Allow-Origin", "https://insightify.yaredmekonnendomain.click" }
            }
        };
    }
}
