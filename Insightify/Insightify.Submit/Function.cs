using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Insightify.Submit;

public class RequestBody
{
    public string TextToAnalyze { get; set; } = string.Empty;
}

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IAmazonSQS _sqsClient;
    private readonly string _tableName;
    private readonly string _queueUrl;

    public Function()
    {
        _dynamoDbClient = new AmazonDynamoDBClient();
        _sqsClient = new AmazonSQSClient();
        _tableName = Environment.GetEnvironmentVariable("ANALYSIS_TABLE_NAME")
            ?? throw new ArgumentNullException("Env var ANALYSIS_TABLE_NAME not set");
        _queueUrl = Environment.GetEnvironmentVariable("ANALYSIS_QUEUE_URL")
            ?? throw new ArgumentNullException("Env var ANALYSIS_QUEUE_URL not set");
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Explicitly handle the browser's preflight security check
        if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            context.Logger.LogInformation("Received OPTIONS preflight request. Responding with CORS headers.");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string> {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "POST,GET,OPTIONS" },
                    { "Access-Control-Allow-Headers", "Content-Type,Authorization" }
                }
            };
        }

        try
        {
            context.Logger.LogInformation($"Received request: {request.Body}");
            if (string.IsNullOrEmpty(request.Body))
            {
                return CreateResponse(HttpStatusCode.BadRequest, "{\"message\":\"Request body cannot be empty.\"}");
            }
            var requestBody = JsonSerializer.Deserialize<RequestBody>(request.Body);
            if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.TextToAnalyze))
            {
                return CreateResponse(HttpStatusCode.BadRequest, "{\"message\":\"Invalid input. 'TextToAnalyze' field is required.\"}");
            }

            var jobId = Guid.NewGuid().ToString();

            // Correct way to get a reference to the table with the Document Model
            var table = Table.LoadTable(_dynamoDbClient, _tableName);

            var item = new Document
            {
                ["JobId"] = jobId,
                ["Status"] = "PENDING",
                ["SubmittedAt"] = DateTime.UtcNow.ToString("o"),
                ["OriginalText"] = requestBody.TextToAnalyze
            };
            await table.PutItemAsync(item);

            var sqsMessageBody = JsonSerializer.Serialize(new { JobId = jobId, TextToAnalyze = requestBody.TextToAnalyze });
            await _sqsClient.SendMessageAsync(new SendMessageRequest { QueueUrl = _queueUrl, MessageBody = sqsMessageBody });

            var responseBody = JsonSerializer.Serialize(new { JobId = jobId });
            return CreateResponse(HttpStatusCode.Accepted, responseBody);
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
