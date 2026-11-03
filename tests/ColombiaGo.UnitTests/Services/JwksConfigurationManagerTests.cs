using ColombiaGo.Infrastructure.Auth;
using FluentAssertions;

namespace ColombiaGo.UnitTests.Services;

public class JwksConfigurationManagerTests
{
    [Fact]
    public async Task Cancelar_la_peticion_que_dispara_la_descarga_no_la_tumba()
    {
        using var http = new HttpClient(new LentoHandler("{\"keys\":[]}", TimeSpan.FromMilliseconds(300)));
        using var manager = new JwksConfigurationManager("https://example.test/jwks.json", http);
        using var cliente = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var configuracion = await manager.GetConfigurationAsync(cliente.Token);

        configuracion.SigningKeys.Should().BeEmpty();
    }

    private sealed class LentoHandler(string cuerpo, TimeSpan demora) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(demora, cancellationToken);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(cuerpo) };
        }
    }
}
