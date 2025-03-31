using DrugIndication.Domain.Config;
using DrugIndication.Parsing.Services;
using Microsoft.Extensions.Options;

namespace DrugIndication.Tests.Services;

public class OpenAiServiceForTest : OpenAiService
{
    public OpenAiServiceForTest(HttpClient httpClient, IOptions<OpenAiOptions> options)
        : base(options)
    {
        // Overwrite HttpClient
        typeof(OpenAiService)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(this, httpClient);
    }
}

