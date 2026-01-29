using System.Globalization;
using FluentAssertions;
using SwiftApp.ERP.SharedKernel.Services;
using Xunit;

namespace SwiftApp.ERP.SharedKernel.Tests;

public class TranslationResolverTests
{
    private record FakeTranslation(string Locale, string Value);

    [Fact]
    public void Resolve_ShouldReturnDefault_WhenTranslationsNull()
    {
        var result = TranslationResolver.Resolve<FakeTranslation>(
            null, t => t.Locale, t => t.Value, "default");

        result.Should().Be("default");
    }

    [Fact]
    public void Resolve_ShouldReturnDefault_WhenTranslationsEmpty()
    {
        var result = TranslationResolver.Resolve(
            Array.Empty<FakeTranslation>(), t => t.Locale, t => t.Value, "default");

        result.Should().Be("default");
    }

    [Fact]
    public void Resolve_ShouldMatchCurrentCulture()
    {
        var prev = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("fr");
            var translations = new FakeTranslation[]
            {
                new("de", "Deutsch"),
                new("fr", "Français"),
                new("en", "English"),
            };

            var result = TranslationResolver.Resolve(
                translations, t => t.Locale, t => t.Value, "fallback");

            result.Should().Be("Français");
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Resolve_ShouldFallbackToGerman_WhenCurrentCultureNotFound()
    {
        var prev = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("ja");
            var translations = new FakeTranslation[]
            {
                new("de", "Deutsch"),
                new("en", "English"),
            };

            var result = TranslationResolver.Resolve(
                translations, t => t.Locale, t => t.Value, "fallback");

            result.Should().Be("Deutsch");
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void Resolve_ShouldReturnAny_WhenNeitherCurrentNorGermanFound()
    {
        var prev = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("ja");
            var translations = new FakeTranslation[]
            {
                new("it", "Italiano"),
            };

            var result = TranslationResolver.Resolve(
                translations, t => t.Locale, t => t.Value, "fallback");

            result.Should().Be("Italiano");
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void ToMap_ShouldReturnEmpty_WhenTranslationsNull()
    {
        var result = TranslationResolver.ToMap<FakeTranslation>(
            null, t => t.Locale, t => t.Value);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ToMap_ShouldReturnDictionary()
    {
        var translations = new FakeTranslation[]
        {
            new("de", "Deutsch"),
            new("fr", "Français"),
        };

        var result = TranslationResolver.ToMap(
            translations, t => t.Locale, t => t.Value);

        result.Should().HaveCount(2);
        result["de"].Should().Be("Deutsch");
        result["fr"].Should().Be("Français");
    }

    [Fact]
    public void SupportedLocales_ShouldContainSwissLocales()
    {
        TranslationResolver.SupportedLocales.Should().Contain("de");
        TranslationResolver.SupportedLocales.Should().Contain("fr");
        TranslationResolver.SupportedLocales.Should().Contain("it");
        TranslationResolver.SupportedLocales.Should().Contain("en");
    }
}
