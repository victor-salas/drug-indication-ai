using DrugIndication.Domain.Config;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace DrugIndication.Tests.Services;

public class OpenAiServiceTests
{
    [Fact]
    public async Task GetStandardizedDiagnosisAsync_ShouldReturnCodeAndDescription_WhenValidResponse()
    {
        // Arrange
        var expectedJsonResponse = """
        {
          "choices": [
            {
              "message": {
                "content": "{ \"code\": \"J45\", \"description\": \"Asthma\" }"
              }
            }
          ]
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedJsonResponse, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.openai.com/"),
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-api-key");

        var optionsMock = Options.Create(new OpenAiOptions { ApiKey = "test-api-key" });

        // Inject HttpClient
        var openAiService = new OpenAiServiceForTest(httpClient, optionsMock);

        // Act
        var (code, description) = await openAiService.GetStandardizedDiagnosisAsync("Asthma");

        // Assert
        code.Should().Be("J45");
        description.Should().Be("Asthma");
    }
}

