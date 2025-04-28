using NexusMods.Abstractions.GameLocators;
using NexusMods.Abstractions.Games;
using NexusMods.ProxyConsole.Abstractions.VerbDefinitions;

namespace NexusMods.CLI.OptionParsers;

/// <summary>
/// Parses a string into an <see cref="IGame"/>
/// </summary>
internal class GameParser(IGameRegistry gameRegistry) : IOptionParser<IGame>
{
    public bool TryParse(string toParse, out IGame value, out string error)
    {
        var install = gameRegistry.SupportedGames.FirstOrDefault(g => g.GameId.ToString() == toParse) ??
                    gameRegistry.SupportedGames.FirstOrDefault(g => g.Name.Equals(toParse, StringComparison.CurrentCultureIgnoreCase))!;
        if (install == null!)
        {
            value = null!;
            error = $"No game installation found matching '{toParse}'.\n";
            return false;
        }
        value = (IGame)install;
        error = string.Empty;
        return true;
    }
}
