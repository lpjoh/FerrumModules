using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FerrumModules.Engine
{
    public static class FE_Input
    {
        private struct PlayerButton
        {
            public PlayerIndex playerIndex;
            public Buttons button;
        }

        private static readonly Dictionary<string, Keys> keyboardDict = new Dictionary<string, Keys>();
        private static readonly Dictionary<string, PlayerButton> buttonDict = new Dictionary<string, PlayerButton>();

        private static KeyboardState previousKeyboardState;
        private static readonly Dictionary<PlayerIndex, GamePadState> previousButtonStates = new Dictionary<PlayerIndex, GamePadState>();

        public static void UpdateActionStates()
        {
            previousKeyboardState = Keyboard.GetState();

            foreach (var playerButton in buttonDict.Values)
                previousButtonStates[playerButton.playerIndex] = GamePad.GetState(playerButton.playerIndex);
        }

        private static bool IsActionPreviouslyPressed(string actionName)
        {
            bool actionPreviouslyPressed = false;

            if (keyboardDict.ContainsKey(actionName))
                actionPreviouslyPressed |= previousKeyboardState.IsKeyDown(keyboardDict[actionName]);

            if (buttonDict.ContainsKey(actionName))
                actionPreviouslyPressed |= previousButtonStates[buttonDict[actionName].playerIndex].IsButtonDown(buttonDict[actionName].button);
            return actionPreviouslyPressed;
        }

        public static bool IsActionJustPressed(string actionName)
        {
            return IsActionPressed(actionName) && !IsActionPreviouslyPressed(actionName);
        }

        public static bool IsActionJustReleased(string actionName)
        {
            return !IsActionPressed(actionName) && IsActionPreviouslyPressed(actionName);
        }

        public static bool IsActionPressed(string actionName)
        {
            bool actionPressed = false;

            if (keyboardDict.ContainsKey(actionName))
                actionPressed |= Keyboard.GetState().IsKeyDown(keyboardDict[actionName]);

            if (buttonDict.ContainsKey(actionName))
                actionPressed |= GamePad.GetState(buttonDict[actionName].playerIndex).IsButtonDown(buttonDict[actionName].button);

            return actionPressed;
        }

        public static void AddAction(string actionName, Keys key)
        {
            keyboardDict[actionName] = key;
        }

        public static void AddAction(string actionName, Buttons button, PlayerIndex playerIndex = PlayerIndex.One)
        {
            buttonDict[actionName] = new PlayerButton{ playerIndex = playerIndex, button = button };
        }

        public static void AddAction(string actionName, Keys key, Buttons button, PlayerIndex playerIndex = PlayerIndex.One)
        {
            AddAction(actionName, key);
            AddAction(actionName, button, playerIndex);
        }

        public static void RemoveAction(string actionName)
        {
            buttonDict.Remove(actionName);
            keyboardDict.Remove(actionName);
        }
    }
}
