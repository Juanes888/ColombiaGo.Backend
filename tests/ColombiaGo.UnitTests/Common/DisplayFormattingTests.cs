using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Common;

public class DisplayFormattingTests
{
    [Theory]
    [InlineData(0, "0 reseñas")]
    [InlineData(1, "1 reseña")]
    [InlineData(2, "2 reseñas")]
    [InlineData(980, "980 reseñas")]
    [InlineData(999, "999 reseñas")]
    public void ToReviewsLabel_BelowOneThousand_ReturnsPlainCount(int count, string expected) =>
        DisplayFormatting.ToReviewsLabel(count).Should().Be(expected);

    [Theory]
    [InlineData(1000, "1k reseñas")]
    [InlineData(1400, "1.4k reseñas")]
    [InlineData(2000, "2k reseñas")]
    [InlineData(2400, "2.4k reseñas")]
    [InlineData(3100, "3.1k reseñas")]
    public void ToReviewsLabel_OneThousandOrMore_ReturnsAbbreviatedForm(int count, string expected) =>
        DisplayFormatting.ToReviewsLabel(count).Should().Be(expected);

    [Fact]
    public void ToPriceLabel_Gratis_IgnoresCostoEntrada() =>
        DisplayFormatting.ToPriceLabel(RangoPrecio.Gratis, 99999).Should().Be("Gratis");

    [Fact]
    public void ToPriceLabel_WithCostoEntrada_ReturnsDesdeLabel() =>
        DisplayFormatting.ToPriceLabel(RangoPrecio.Economico, 15000).Should().Be("Desde $15.000");

    [Theory]
    [InlineData(RangoPrecio.Economico, "Económico")]
    [InlineData(RangoPrecio.Moderado, "Moderado")]
    [InlineData(RangoPrecio.Costoso, "Costoso")]
    [InlineData(RangoPrecio.Lujo, "Lujo")]
    public void ToPriceLabel_WithoutCostoEntrada_FallsBackToRangoLabel(RangoPrecio rango, string expected) =>
        DisplayFormatting.ToPriceLabel(rango, null).Should().Be(expected);

    [Fact]
    public void ToHourlyPriceLabel_FormatsAsColombianPesosPerHour() =>
        DisplayFormatting.ToHourlyPriceLabel(70000).Should().Be("$70.000 por hora");
}
