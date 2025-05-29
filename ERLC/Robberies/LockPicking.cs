using System;
using System.Drawing; 
using System.Threading; 
using ERLC; 
using Spectre.Console; // Added for Spectre.Console

namespace ERLC.Robberies;
public class LockPicking // Remains non-static
{
    // Constants remain the same
    private const int StartTime = 1; 
    private const string LineColorHtmlString = "#FFC903"; 
    private const int BarSizeOffsetBase = 83; 
    private const int UpperColorOffset = -4; 
    private const int LowerColorOffset = 10; 
    private const int RgbBrightnessThreshold = 140; 
    private const int LineColorTolerance = 10; 
    private const int TargetAreaVerticalRadius = 3; 
    private const int MinBrightPixelsForClick = 4;  

    private Color _lineColorInstance = ColorTranslator.FromHtml(LineColorHtmlString); 

    public void StartProcess() 
    {
        AnsiConsole.Write(new Rule($"[bold yellow]Lockpicking Sequence Started[/]").LeftJustified());
        AnsiConsole.MarkupLine("\n[cyan]i Initializing Lockpicking...[/]");

        AnsiConsole.MarkupLine("[yellow]i Focusing Roblox window...[/]");
        Roblox.FocusRoblox();
        Thread.Sleep(StartTime * 1000); // Wait for focus

        int barSizeOffset = (int)Math.Floor(BarSizeOffsetBase * Screen.SystemScaleMultiplier);
        AnsiConsole.MarkupLine($"[dim]  Bar offset calculated with scale multiplier ({Screen.SystemScaleMultiplier}): {barSizeOffset} pixels[/]");

        AnsiConsole.MarkupLine("\n[cyan]i Searching for lockpicking line...[/]");
        var (linePosX, linePosY) = Screen.LocateColor(_lineColorInstance, LineColorTolerance);
        
        if (linePosX == 0 && linePosY == 0)
        {
            AnsiConsole.MarkupLine("[bold red]! Lockpicking line NOT found![/]");
            AnsiConsole.MarkupLine("[dim]  Ensure the lockpicking minigame is active and visible.[/]");
            AnsiConsole.MarkupLine("[dim]  Try adjusting screen/game brightness or tool's RGB threshold if issues persist.[/]");
            Thread.Sleep(2500); 
            AnsiConsole.Write(new Rule($"[bold red]Lockpicking Failed[/]").LeftJustified());
            return;
        }

        AnsiConsole.MarkupLine($"[green]i Line found at (X: {linePosX}, Y: {linePosY})[/]");
        Thread.Sleep(500); // User can see the confirmation

        AnsiConsole.Write(new Rule($"[bold blue]Processing Bars[/]").Centered());
        for (int rectI = 1; rectI < 7; rectI++)
        {
            int currentBarX = linePosX + (barSizeOffset * rectI);
            AnsiConsole.MarkupLine($"\n[bold blue]== Processing Bar {rectI}/6 ==[/]");
            AnsiConsole.MarkupLine($"[dim]  Targeting X-coordinate: {currentBarX}[/]");
            
            // Using Spectre.Console's Status for the pixel checking loop
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("blue"))
                .Start($"[yellow]Scanning pixels for bar {rectI}...[/]", ctx => 
                {
                    while (true) 
                    {
                        int brightPixelCount = 0;
                        for (int yOffset = -TargetAreaVerticalRadius; yOffset <= TargetAreaVerticalRadius; yOffset++)
                        {
                            int checkY = linePosY + yOffset;
                            // For performance, avoid logging every pixel check. 
                            // AnsiConsole.MarkupLine($"  [grey]Checking pixel at ({currentBarX}, {checkY})[/]");
                            Color pixelColor = Screen.GetColorAtPixel(currentBarX, checkY);
                            bool isBright = pixelColor.R > RgbBrightnessThreshold &&
                                            pixelColor.G > RgbBrightnessThreshold &&
                                            pixelColor.B > RgbBrightnessThreshold;

                            if (isBright)
                            {
                                brightPixelCount++;
                            }
                        }
                        ctx.Status($"[yellow]Scanning pixels for bar {rectI}... Found {brightPixelCount}/{MinBrightPixelsForClick} bright pixels.[/]");
                        
                        if (brightPixelCount >= MinBrightPixelsForClick)
                        {
                            Mouse.LeftClick();
                            Mouse.SetMousePos(currentBarX, linePosY); 
                            AnsiConsole.MarkupLine($"[bold lime]>>> Clicked for bar {rectI}! (Bright pixels: {brightPixelCount}) <<<[/]");
                            Thread.Sleep(110); // Keep existing delay post-click
                            break; // Exit while loop for this bar
                        }
                        
                        Thread.Sleep(10); // Short delay between checks to avoid busy-looping and reduce CPU
                    }
                });
        }
        
        AnsiConsole.MarkupLine("\n[bold green]i Lockpicking finished successfully![/]");
        AnsiConsole.Write(new Rule($"[bold green]Lockpicking Sequence Complete[/]").LeftJustified());
        Thread.Sleep(1500); 
    }
}