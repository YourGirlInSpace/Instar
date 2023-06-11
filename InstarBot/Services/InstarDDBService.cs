using System.Diagnostics.CodeAnalysis;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace PaxAndromeda.Instar.Services;

[ExcludeFromCodeCoverage]
public class InstarDDBService : IInstarDDBService
{
    private const string TableName = "UserData";
    private const string PrimaryKey = "UserID";
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssK"; // ISO-8601

    private readonly AmazonDynamoDBClient _client;

    public InstarDDBService(IConfiguration config)
    {
        var region = config.GetSection("AWS").GetValue<string>("Region");

        _client = new AmazonDynamoDBClient(new AWSIAMCredential(config), RegionEndpoint.GetBySystemName(region));
    }

    public async Task<bool> UpdateUserBirthday(Snowflake snowflake, DateTime birthday)
    {
        return await UpdateUserData(snowflake,
            DataType.Birthday,
            (DynamoDBEntry)birthday.ToString(DateTimeFormat));
    }

    public async Task<bool> UpdateUserJoinDate(Snowflake snowflake, DateTime joinDate)
    {
        return await UpdateUserData(snowflake,
            DataType.JoinDate,
            (DynamoDBEntry)joinDate.ToString(DateTimeFormat));
    }

    public async Task<DateTime?> GetUserBirthday(Snowflake snowflake)
    {
        var entry = await GetUserData(snowflake, DataType.Birthday);
        return entry?.AsDateTime();
    }

    public async Task<DateTime?> GetUserJoinDate(Snowflake snowflake)
    {
        var entry = await GetUserData(snowflake, DataType.JoinDate);
        return entry?.AsDateTime();
    }

    private async Task<bool> UpdateUserData<T>(Snowflake snowflake, DataType dataType, T data)
        where T : DynamoDBEntry
    {
        var table = Table.LoadTable(_client, TableName);

        var updateData = new Document(new Dictionary<string, DynamoDBEntry>
        {
            { PrimaryKey, snowflake.ID.ToString() },
            { nameof(DataType), dataType.ToString() },
            { dataType.ToString(), data }
        });

        var result = await table.UpdateItemAsync(updateData, new UpdateItemOperationConfig
        {
            ReturnValues = ReturnValues.AllNewAttributes
        });

        return result[PrimaryKey].AsULong() == snowflake.ID &&
               result[nameof(DataType)].AsString().Equals(dataType.ToString(), StringComparison.Ordinal) &&
               result[dataType.ToString()].AsString().Equals(data.AsString());
    }

    private async Task<DynamoDBEntry?> GetUserData(Snowflake snowflake, DataType dataType)
    {
        var table = Table.LoadTable(_client, TableName);
        var scan = table.Query(new Primitive(snowflake.ID.ToString()),
            new QueryFilter(nameof(DataType), QueryOperator.Equal, dataType.ToString()));

        var results = await scan.GetRemainingAsync();

        switch (results.Count)
        {
            case > 1:
                Log.Warning("Found duplicate data type {DataType} for user ID {UserID}", dataType, snowflake.ID);
                break;
            case 0:
                return null;
        }

        if (results.First().TryGetValue(dataType.ToString(), out var entry))
            return entry;

        Log.Warning("Failed to query data type {DataType} for user ID {UserID}", dataType, snowflake.ID);
        return null;
    }

    private enum DataType
    {
        Birthday,
        JoinDate
    }
}