using System;

namespace Watermelon
{
    [Flags]
    public enum UIGamepadButtonTag
    {
        None = 0,
        Settings = 1,
        MainMenu = 2,
        Weapons = 4,
        Characters = 8,
        Game = 16,
        Pause = 32,
        Complete = 64,
        CharacterSuggestion = 128,
        GameOver = 256,
    }
}