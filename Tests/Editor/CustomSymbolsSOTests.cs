#if UNITY_EDITOR

using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace ActionFit.CustomSymbols.Editor.Tests
{
    public class CustomSymbolsSOTests
    {
        private CustomSymbolsSO _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<CustomSymbolsSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void InitializeFromPlatformSymbolsSetsBuildAndPlatformStates()
        {
            _settings.InitializeFromPlatformSymbols(
                new[] { "COMMON", "DESKTOP_ONLY" },
                new[] { "COMMON", "ANDROID_ONLY" },
                new[] { "COMMON", "IOS_ONLY" });

            Assert.That(
                _settings.customAllSymbols.Select(entry => entry.symbolName),
                Is.EqualTo(new[] { "ANDROID_ONLY", "COMMON", "DESKTOP_ONLY", "IOS_ONLY" }));
            Assert.That(_settings.customAllSymbols.All(entry => entry.includedInBuild), Is.True);
            Assert.That(_settings.allPlatformSymbols, Is.EqualTo(new[] { "COMMON" }));
            Assert.That(_settings.windowPlatformSymbols, Is.EqualTo(new[] { "COMMON", "DESKTOP_ONLY" }));
            Assert.That(_settings.macPlatformSymbols, Is.EqualTo(new[] { "COMMON", "DESKTOP_ONLY" }));
            Assert.That(_settings.aosPlatformSymbols, Is.EqualTo(new[] { "ANDROID_ONLY", "COMMON" }));
            Assert.That(_settings.iosPlatformSymbols, Is.EqualTo(new[] { "COMMON", "IOS_ONLY" }));
        }

        [Test]
        public void InitializeFromPlatformSymbolsNormalizesDuplicatesAndBlanks()
        {
            _settings.InitializeFromPlatformSymbols(
                new[] { "  DESKTOP  ", "", "DESKTOP" },
                new[] { "ANDROID", null, "ANDROID" },
                new[] { " IOS ", "IOS" });

            Assert.That(
                _settings.customAllSymbols.Select(entry => entry.symbolName),
                Is.EqualTo(new[] { "ANDROID", "DESKTOP", "IOS" }));
            Assert.That(_settings.allPlatformSymbols, Is.Empty);
            Assert.That(_settings.windowPlatformSymbols, Is.EqualTo(new[] { "DESKTOP" }));
            Assert.That(_settings.macPlatformSymbols, Is.EqualTo(new[] { "DESKTOP" }));
            Assert.That(_settings.aosPlatformSymbols, Is.EqualTo(new[] { "ANDROID" }));
            Assert.That(_settings.iosPlatformSymbols, Is.EqualTo(new[] { "IOS" }));
        }
    }
}

#endif
