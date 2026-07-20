namespace App.Scripts.UI.Constants
{
    public static class UxmlNames
    {
        public static class Containers
        {
            public const string ROOT = "root";
            public const string SCREENS = "screens-container";
            public const string SCREEN_OVERLAY = "screen-overlay";
            public const string SCREENS_CONTENT = "screens-content";
            public const string POPUPS = "popups-container";
            public const string POPUPS_CONTENT = "popups-content";
            public const string POPUP_OVERLAY = "popup-overlay";
            public const string LOADER = "loader-container";
            public const string TOAST = "toast-container";
            public const string ERROR = "error-container";
        }

        public static class Screens
        {
            public const string MENU = "menu-container";
        }

        public static class Loader
        {
            public const string CONTENT = "loader__content";
            public const string SPINNER = "loader__spinner";
        }

        public static class Menu
        {
            public const string HEADER = "menu__header";
            public const string SETTINGS_BUTTON = "settings-btn";
            public const string PLAY_BUTTON = "play-btn";
            public const string BUTTONS_CONTAINER = "buttons-container";
        }

        public static class Popups
        {
            public const string SETTINGS = "settings-popup";
        }

        public static class Toast
        {
            public const string MESSAGE = "toast__message";
        }

        public static class Error
        {
            public const string OVERLAY = "error__overlay";
            public const string MESSAGE = "error__message";
            public const string RETRY_BUTTON = "error__retry-btn";
            public const string SKIP_BUTTON = "error__skip-btn";
        }

        public static class Settings
        {
            public const string HEADER = "settings__header";
            public const string CLOSE_BUTTON = "settings__close-btn";
            public const string MUSIC_TOGGLE = "settings__music-toggle";
            public const string SOUNDS_TOGGLE = "settings__sounds-toggle";
            public const string VIBRATION_TOGGLE = "settings__vibration-toggle";
            public const string PRIVACY_POLICY_LINK = "settings__privacy-link";
        }

        public static class LevelComplete
        {
            public const string TITLE = "level-complete__title";
            public const string SUBTITLE = "level-complete__subtitle";
            public const string NEXT_BUTTON = "level-complete__next-btn";
        }

        public static class LevelFailed
        {
            public const string TITLE = "level-failed__title";
            public const string SUBTITLE = "level-failed__subtitle";
            public const string RETRY_BUTTON = "level-failed__retry-btn";
        }

        public static class Game
        {
            public const string HEADER = "game__header";
            public const string LEVEL_LABEL = "game__level-label";
            public const string RESTART_BUTTON = "game__restart-btn";
        }
    }
}