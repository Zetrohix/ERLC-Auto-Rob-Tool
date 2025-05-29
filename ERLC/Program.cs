using System;
using System;
using System;
using System;
using System.Threading;
using ERLC;
using ERLC.Robberies;
using Spectre.Console; // Added for Spectre.Console

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "ER:LC AutoRob Tool";
        Console.TreatControlCAsInput = false; // Good to keep

        // Display disclaimer and version info once at the start
        AnsiConsole.MarkupLine("[yellow]DISCLAIMER: Use this tool responsibly and in accordance with ER:LC's terms of service.[/]");
        AnsiConsole.MarkupLine("[yellow]We, the developers and contributors of this tool are not responsible for any consequences resulting from the misuse of the tool.[/]");
        AnsiConsole.MarkupLine("\n[bold aqua]Make sure you downloaded the program from the original github link available below.[/]");
        AnsiConsole.MarkupLine("[bold aqua]This program is free - if you bought it, you got scammed.[/]");
        AnsiConsole.MarkupLine("[underline aqua]https://github.com/IceMinisterq/ERLC-Auto-Rob-Tool[/]\n");
        
        AnsiConsole.MarkupLine($"[dim]> Last Update: 16/03/24[/]"); // Kept original date
        AnsiConsole.MarkupLine($"[dim]> Version    : 1.1.1[/]");   // Kept original version
        AnsiConsole.MarkupLine($"[dim]> By Ketami & Liker[/]");
        AnsiConsole.MarkupLine($"[dim]> Screen Scale Factor : {Screen.SystemScaleMultiplier}[/]\n");

        if (!Roblox.IsRobloxRunning())
        {
            AnsiConsole.MarkupLine("[bold red]i ~ Waiting for Roblox to open...[/]");
            while (!Roblox.IsRobloxRunning())
            {
                Thread.Sleep(500);
            }
            AnsiConsole.MarkupLine("[bold green]i ~ Roblox detected![/]\n");
        }

        LockPicking lockPicker = new LockPicking();
        // ATM, GlassCutting, Crowbar are assumed to have static StartProcess methods based on previous structure

        while (true)
        {
            AnsiConsole.Clear();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold cyan]ER:LC AutoRob Tool[/]\n[dim]Choose a robbery method:[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(new[] {
                        "[green]Auto Lockpick[/]",
                        "[blue]Auto ATM[/]",
                        "[yellow]Auto Glass Cutting[/]",
                        "[magenta]Auto Car Crowbar[/]", // Added Car Crowbar
                        "[red]Exit[/]"
                    }));

            switch (choice)
            {
                case "[green]Auto Lockpick[/]":
                    lockPicker.StartProcess();
                    break;
                case "[blue]Auto ATM[/]":
                    ATM.StartProcess(); // Assuming static method
                    break;
                case "[yellow]Auto Glass Cutting[/]":
                    GlassCutting.StartProcess(); // Assuming static method
                    break;
                case "[magenta]Auto Car Crowbar[/]":
                    Crowbar.StartProcess(); // Assuming static method
                    break;
                case "[red]Exit[/]":
                    AnsiConsole.MarkupLine("[bold red]Exiting application...[/]");
                    Thread.Sleep(500); // Brief pause before exit
                    return; // Exit the Main method, thus the application
            }

            AnsiConsole.MarkupLine("\n[yellow]Robbery module finished. Press any key to return to the menu...[/]");
            Console.ReadKey(true); // Wait for user input before looping back
        }
    }
}
