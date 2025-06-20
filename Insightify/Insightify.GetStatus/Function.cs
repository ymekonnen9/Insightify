using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Net;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Insightify.GetStatus;

/// <summary>
/// Defines the clean JSON structure we will send back to the frontend.
/// This acts as a clear "API contract".
/// </summary>
public class JobStatusResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public float PositiveScore { get; set; }
    public float NegativeScore { get; set; }
    public float NeutralScore { get; set; }
    public float MixedScore { get; set; }
    public List<string> KeyEntities { get; set; } = new();
    public string? SubmittedAt { get; set; }
    public string? CompletedAt { get; set; }
}


public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public Function()
    {
        _dynamoDbClient = new AmazonDynamoDBClient();
        _tableName = Environment.GetEnvironmentVariable("ANALYSIS_TABLE_NAME")
            ?? throw new ArgumentNullException("Env var ANALYSIS_TABLE_NAME not set");
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Handle CORS preflight request
        if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string> {
                    { "Access-Control-Allow-Origin", "*" }, { "Access-Control-Allow-Methods", "POST,GET,OPTIONS" }, { "Access-Control-Allow-Headers", "Content-Type,Authorization" }
                }
            };
        }

        try
        {
            if (!request.PathParameters.TryGetValue("jobId", out var jobId) || string.IsNullOrEmpty(jobId))
            {
                return CreateResponse(HttpStatusCode.BadRequest, "{\"message\":\"JobId must be provided in the URL path.\"}");
            }

            var table = Table.LoadTable(_dynamoDbClient, _tableName);
            var item = await table.GetItemAsync(jobId);

            if (item == null)
            {
                return CreateResponse(HttpStatusCode.NotFound, $"{{\"message\":\"No job found with ID: {jobId}\"}}");
            }

            // Manually build our clean response object from the DynamoDB document
            var responsePayload = new JobStatusResponse
            {
                JobId = item.TryGetValue("JobId", out var id) ? id.AsString() : string.Empty,
                Status = item.TryGetValue("Status", out var status) ? status.AsString() : string.Empty,
                Sentiment = item.TryGetValue("Sentiment", out var sentiment) ? sentiment.AsString() : string.Empty,
                PositiveScore = item.TryGetValue("PositiveScore", out var pos) ? pos.AsSingle() : 0,
                NegativeScore = item.TryGetValue("NegativeScore", out var neg) ? neg.AsSingle() : 0,
                NeutralScore = item.TryGetValue("NeutralScore", out var neu) ? neu.AsSingle() : 0,
                MixedScore = item.TryGetValue("MixedScore", out var mix) ? mix.AsSingle() : 0,
                SubmittedAt = item.TryGetValue("SubmittedAt", out var submitted) ? submitted.AsString() : null,
                CompletedAt = item.TryGetValue("CompletedAt", out var completed) ? completed.AsString() : null,
                // Convert the DynamoDB list to a simple C# list of strings
                KeyEntities = item.TryGetValue("KeyEntities", out var entities) ? entities.AsListOfString() : new List<string>()
            };

            // Serialize our clean C# object to a JSON string for the response body
            var responseBody = JsonSerializer.Serialize(responsePayload);

            return CreateResponse(HttpStatusCode.OK, responseBody);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Internal server error: {ex.Message}");
            return CreateResponse(HttpStatusCode.InternalServerError, "{\"message\":\"An unexpected error occurred.\"}");
        }
    }

    private APIGatewayProxyResponse CreateResponse(HttpStatusCode statusCode, string body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)statusCode,
            Body = body,
            Headers = new Dictionary<string, string> {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" }
            }
        };
    }
}
