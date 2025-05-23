﻿// Copyright © 2017 - 2025 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using chocolatey.infrastructure.adapters;
using chocolatey.infrastructure.guards;
using chocolatey.infrastructure.logging;
using Console = chocolatey.infrastructure.adapters.Console;

namespace chocolatey.infrastructure.commandline
{
    public class InteractivePrompt
    {
        private static Lazy<IConsole> _console = new Lazy<IConsole>(() => new Console());
        private const int TimeoutInSeconds = 30;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void InitializeWith(Lazy<IConsole> console)
        {
            _console = console;
        }

        private static IConsole Console
        {
            get { return _console.Value; }
        }

        public static string PromptForConfirmation(string prompt, IEnumerable<string> choices, string defaultChoice, bool requireAnswer, bool allowShortAnswer = true, bool shortPrompt = false, int repeat = 10, int timeoutInSeconds = 0)
        {
            if (repeat < 0)
            {
                throw new ApplicationException("Too many bad attempts. Stopping before application crash.");
            }

            Ensure.That(() => prompt).NotNull();
            Ensure.That(() => choices).NotNull();
            Ensure
                .That(() => choices)
                .Meets(
                    c => c.Count() > 0,
                    (name, value) => { throw new ApplicationException("No choices passed in. Please ensure you pass choices"); });

            if (!string.IsNullOrWhiteSpace(defaultChoice))
            {
                Ensure
                    .That(() => choices)
                    .Meets(
                        c => c.Contains(defaultChoice),
                        (name, value) => { throw new ApplicationException("Default choice value must be one of the given choices."); });
            }

            if (allowShortAnswer)
            {
                Ensure
                    .That(() => choices)
                    .Meets(
                        c => !c.Any(string.IsNullOrWhiteSpace),
                        (name, value) => { throw new ApplicationException("Some choices are empty. Please ensure you provide no empty choices."); });

                Ensure
                 .That(() => choices)
                 .Meets(
                     c => c.Select(entry => entry.FirstOrDefault()).Distinct().Count() == c.Count(),
                     (name, value) => { throw new ApplicationException("Multiple choices have the same first letter. Please ensure you pass choices with different first letters."); });
            }

            if (timeoutInSeconds > 0)
            {
                "chocolatey".Log().Info(ChocolateyLoggers.Important, "For the question below, you have {0} seconds to make a selection.".FormatWith(timeoutInSeconds));
            }

            if (shortPrompt)
            {
                Console.Write(prompt + "(");

            }

            "chocolatey".Log().Info(shortPrompt ? ChocolateyLoggers.LogFileOnly : ChocolateyLoggers.Important, prompt);

            var counter = 1;
            IDictionary<int, string> choiceDictionary = new Dictionary<int, string>();
            foreach (var choice in choices.OrEmpty())
            {
                choiceDictionary.Add(counter, choice);
                "chocolatey".Log().Info(shortPrompt ? ChocolateyLoggers.LogFileOnly : ChocolateyLoggers.Normal, " {0}) {1}{2}".FormatWith(counter, choice.ToStringSafe(), choice.IsEqualTo(defaultChoice) ? " [Default - Press Enter]" : ""));
                if (shortPrompt)
                {
                    var choicePrompt = choice.IsEqualTo(defaultChoice) ?
                            shortPrompt ?
                                "[[{0}]{1}]".FormatWith(choice.Substring(0, 1).ToUpperInvariant(), choice.Substring(1, choice.Length - 1)) :
                                "[{0}]".FormatWith(choice.ToUpperInvariant())
                        :
                            shortPrompt ?
                                "[{0}]{1}".FormatWith(choice.Substring(0, 1).ToUpperInvariant(), choice.Substring(1, choice.Length - 1)) :
                                choice;

                    if (counter != 1)
                    {
                        Console.Write("/");
                    }

                    Console.Write(choicePrompt);
                }

                counter++;
            }

            Console.Write(shortPrompt ? "): " : "> ");

            var selection = timeoutInSeconds == 0 ? Console.ReadLine() : Console.ReadLine(timeoutInSeconds * 1000);
            if (shortPrompt)
            {
                Console.WriteLine();
            }

            if (string.IsNullOrWhiteSpace(selection) && !string.IsNullOrWhiteSpace(defaultChoice))
            {
                "chocolatey".Log().Info(ChocolateyLoggers.LogFileOnly, "Choosing default choice of '{0}'".FormatWith(defaultChoice.EscapeCurlyBraces()));
                return defaultChoice;
            }

            var selected = -1;
            if (!int.TryParse(selection, out selected) || selected <= 0 || selected > (counter - 1))
            {
                // check to see if value was passed
                var selectionFound = false;
                foreach (var pair in choiceDictionary)
                {
                    var value = pair.Value.ToStringSafe();
                    if (value.IsEqualTo(selection) ||
                        (allowShortAnswer && value.Substring(0, 1).IsEqualTo(selection)) ||
                        (value.ContainsSafe("-") && value.Substring(0, value.IndexOf("-")).TrimSafe().IsEqualTo(selection))
                        )
                    {
                        selected = pair.Key;
                        selectionFound = true;
                        "chocolatey".Log().Info(ChocolateyLoggers.LogFileOnly, "Choice selected: '{0}'".FormatWith(pair.Value.EscapeCurlyBraces()));
                        break;
                    }
                }

                if (!selectionFound)
                {
                    "chocolatey".Log().Error(ChocolateyLoggers.Important, "Timeout or your choice of '{0}' is not a valid selection.".FormatWith(selection.EscapeCurlyBraces()));
                    if (requireAnswer)
                    {
                        "chocolatey".Log().Warn(ChocolateyLoggers.Important, "You must select an answer");
                        return PromptForConfirmation(prompt, choices, defaultChoice, requireAnswer, allowShortAnswer, shortPrompt, repeat - 1);
                    }
                    return null;
                }
            }

            return choiceDictionary[selected];
        }

        public static string GetPassword(bool interactive)
        {
            var password = string.Empty;
            var possibleNonInteractive = !interactive;
            ConsoleKeyInfo info = possibleNonInteractive ? Console.ReadKey(TimeoutInSeconds * 1000) : Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                    info = possibleNonInteractive ? Console.ReadKey(TimeoutInSeconds * 1000) : Console.ReadKey(true);
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        var pos = System.Console.CursorLeft;
                        // move the cursor to the left by one character
                        System.Console.SetCursorPosition(pos - 1, System.Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        System.Console.SetCursorPosition(pos - 1, System.Console.CursorTop);
                    }
                    info = possibleNonInteractive ? Console.ReadKey(TimeoutInSeconds * 1000) : Console.ReadKey(true);
                }
            }
            for (var i = 0; i < password.Length; i++)
            {
                Console.Write("*");
            }

            System.Console.WriteLine("");

            return password;
        }


#pragma warning disable IDE0022, IDE1006
        [Obsolete("This overload is deprecated and will be removed in v3.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void initialize_with(Lazy<IConsole> console)
            => InitializeWith(console);

        [Obsolete("This overload is deprecated and will be removed in v3.")]
        public static string prompt_for_confirmation(string prompt, IEnumerable<string> choices, string defaultChoice, bool requireAnswer, bool allowShortAnswer = true, bool shortPrompt = false, int repeat = 10, int timeoutInSeconds = 0)
            => PromptForConfirmation(prompt, choices, defaultChoice, requireAnswer, allowShortAnswer, shortPrompt, repeat, timeoutInSeconds);

        [Obsolete("This overload is deprecated and will be removed in v3.")]
        public static string get_password(bool interactive)
            => GetPassword(interactive);
#pragma warning restore IDE0022, IDE1006
    }
}
