using Avalonia.Metadata;
using System.Runtime.CompilerServices;

// Allow the unit test project to access internal members (e.g. CrtDisplay.PositiveMod).
[assembly: InternalsVisibleTo("Pipboy.Avalonia.Tests")]
[assembly: XmlnsPrefix("https://github.com/NeverMorewd/Pipboy.Avalonia", "pb")]
[assembly: XmlnsDefinition("https://github.com/NeverMorewd/Pipboy.Avalonia", "Pipboy.Avalonia")]
[assembly: XmlnsDefinition("https://github.com/NeverMorewd/Pipboy.Avalonia", "Pipboy.Avalonia.Controls")]
[assembly: XmlnsDefinition("https://github.com/NeverMorewd/Pipboy.Avalonia", "Pipboy.Avalonia.Styles")]