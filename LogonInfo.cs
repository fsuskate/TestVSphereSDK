using System;

namespace TestVSphereSDK
{
    class LogonInfo
    {
        public enum LogonInfoType
        {
            USERNAME = 0,
            PASSWORD = 1,
            URI = 2
        };

        public string URI { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public LogonInfo()
        {
            Username = Prompt(LogonInfoType.USERNAME);
            Password = Prompt(LogonInfoType.PASSWORD);
            URI = Prompt(LogonInfoType.URI);
        }

        private string PromptPassword()
        {
            string password = string.Empty;
            Console.Write("Enter Password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                if (password.Length >= 255) break;
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();

            return password.Trim();
        }

        private string PromptUsername()
        {
            Console.Write("Enter Username: ");
            return Console.ReadLine();
        }

        private string PromptURI()
        {
            Console.Write("Enter server: ");
            return Console.ReadLine();
        }

        public string Prompt(LogonInfoType type)
        {
            switch (type)
            {
                case LogonInfoType.PASSWORD:
                    return PromptPassword();
                case LogonInfoType.USERNAME:
                    return PromptUsername();
                case LogonInfoType.URI:
                    return PromptURI();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
