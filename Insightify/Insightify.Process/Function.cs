using Amazon.Lambda.SQSEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Insightify.Process;


public class FunctionInput
{
    public string JobId { get; set; } = string.Empty;
    public string TextToAnalyze { get; set; } = string.Empty;
}

public class Function
{
    private readonly IAmazonComprehend _comprehendClient;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public Function()
    {
        _comprehendClient = new AmazonComprehendClient();
        _dynamoDbClient = new AmazonDynamoDBClient();


        var tableNameFromEnv = Environment.GetEnvironmentVariable("ANALYSIS_TABLE_NAME");
        if (string.IsNullOrEmpty(tableNameFromEnv))
        {
            throw new ArgumentNullException("Environment variable ANALYSIS_TABLE_NAME is not set.");
        }
        _tableName = tableNameFromEnv;
    }


    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Received {sqsEvent.Records.Count} SQS message(s).");

        foreach (var message in sqsEvent.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }


    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing message ID: {message.MessageId}");
            context.Logger.LogInformation($"Message Body: {message.Body}");

            var input = JsonSerializer.Deserialize<FunctionInput>(message.Body);
            if (input == null || string.IsNullOrEmpty(input.JobId) || string.IsNullOrEmpty(input.TextToAnalyze))
            {
                context.Logger.LogError("Message body is invalid or missing required fields.");
                return;
            }

            var sentimentResponse = await _comprehendClient.DetectSentimentAsync(new DetectSentimentRequest
            {
                Text = input.TextToAnalyze,
                LanguageCode = "en"
            });
            var entitiesResponse = await _comprehendClient.DetectEntitiesAsync(new DetectEntitiesRequest
            {
                Text = input.TextToAnalyze,
                LanguageCode = "en"
            });
            var entityNames = entitiesResponse.Entities.Select(e => e.Text).ToList();
            context.Logger.LogInformation($"Analysis complete for Job ID: {input.JobId}");


            // Correct way to get a reference to the table with the Document Model
            var table = Table.LoadTable(_dynamoDbClient, _tableName);

            var doc = new Document();
            doc["JobId"] = input.JobId;
            doc["Status"] = "COMPLETED";
            doc["OriginalText"] = input.TextToAnalyze;
            doc["Sentiment"] = sentimentResponse.Sentiment.Value;
            doc["PositiveScore"] = sentimentResponse.SentimentScore.Positive;
            doc["NegativeScore"] = sentimentResponse.SentimentScore.Negative;
            doc["NeutralScore"] = sentimentResponse.SentimentScore.Neutral;
            doc["MixedScore"] = sentimentResponse.SentimentScore.Mixed;
            doc["KeyEntities"] = entityNames;
            doc["CompletedAt"] = DateTime.UtcNow.ToString("o");


            await table.PutItemAsync(doc);

            context.Logger.LogInformation($"Successfully saved results for Job ID {input.JobId} to DynamoDB table {_tableName}.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing message: {ex.Message}");
            throw;
        }
    }
}
