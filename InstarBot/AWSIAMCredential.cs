using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace PaxAndromeda.Instar;

[ExcludeFromCodeCoverage]
public record AWSIAMCredential
{
    private string AccessKey { get; }
    private string SecretKey { get; }

    public AWSIAMCredential(IConfiguration config)
    {
        var awsSection = config.GetSection("AWS");

        AccessKey = awsSection.GetValue<string>("AccessKey")!;
        SecretKey = awsSection.GetValue<string>("SecretAccessKey")!;

        if (AccessKey is null || SecretKey is null)
            throw new ConfigurationException("AWS credentials were not set.");
    }

    public static implicit operator BasicAWSCredentials(AWSIAMCredential credential)
    {
        return new BasicAWSCredentials(credential.AccessKey, credential.SecretKey);
    }
}