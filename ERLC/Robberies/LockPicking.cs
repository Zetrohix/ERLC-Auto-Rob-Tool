using System.Drawing;

namespace ERLC.Robberies;
public class LockPicking
{
    private const int StartTime = 1; // Time in seconds to wait before starting the lockpicking process.
    private const string LineColorHtmlString = "#FFC903"; // HTML color code for the yellow line in the lockpicking minigame.
    private const int BarSizeOffsetBase = 83; // Base offset in pixels for calculating the position of each bar in the lockpicking minigame.
    private const int UpperColorOffset = -4; // Vertical offset (in pixels) from the detected line to check for a secondary color point (original logic).
    private const int LowerColorOffset = 10; // Vertical offset (in pixels) from the detected line to check for a primary color point (original logic, now mostly superseded by area check).
    private const int RgbBrightnessThreshold = 140; // Minimum RGB value (for R, G, and B components) to consider a pixel "bright".
    private const int LineColorTolerance = 10; // Tolerance value used when searching for the line color, allowing for slight variations.
    private const int TargetAreaVerticalRadius = 3; // Defines the vertical radius (in pixels) above and below the detected line's Y-coordinate to scan for bright pixels.
    private const int MinBrightPixelsForClick = 4;  // Minimum number of bright pixels that must be detected within the target area to trigger a click.

    private static Color LineColor = ColorTranslator.FromHtml(LineColorHtmlString); // Translates the HTML color string to a Color object.
    
    public static void StartProcess()
    {
        Console.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();

        Thread.Sleep(StartTime * 1000);

        int barSizeOffset = (int)Math.Floor(BarSizeOffsetBase * Screen.SystemScaleMultiplier);

        // Locate the yellow line, now using tolerance for more robust detection.
        var (linePosX, linePosY) = Screen.LocateColor(LineColor, LineColorTolerance);
        if (linePosX == 0 && linePosY == 0)
        {
            Console.WriteLine("! ~ LockPicking line could not be found!");
            return;
        }

        Console.WriteLine($"i ~ Found Line at {linePosY}, {linePosY}");

        for (int rectI = 1; rectI < 7; rectI++)
        {
            int x = linePosX + (barSizeOffset * rectI);
            //Mouse.SetMousePos(x, linePosY);
            while (true)
            {
                int brightPixelCount = 0;
                // Check a vertical strip of pixels around the detected line's Y-coordinate for the current bar (x).
                // This area-based approach requires a minimum number of pixels (MinBrightPixelsForClick)
                // within this strip (defined by TargetAreaVerticalRadius) to be "bright" (RgbBrightnessThreshold)
                // before triggering a click, making the detection more resilient to minor variations.
                for (int yOffset = -TargetAreaVerticalRadius; yOffset <= TargetAreaVerticalRadius; yOffset++)
                {
                    Color pixelColor = Screen.GetColorAtPixel(x, linePosY + yOffset);
                    if (pixelColor.R > RgbBrightnessThreshold && 
                        pixelColor.G > RgbBrightnessThreshold && 
                        pixelColor.B > RgbBrightnessThreshold)
                    {
                        brightPixelCount++;
                    }
                }

                if (brightPixelCount >= MinBrightPixelsForClick)
                {
                    Mouse.LeftClick();
                    // Consider if Mouse.SetMousePos(x, linePosY); is still needed here or if it should be before the check.
                    // For now, keep it consistent with the old logic's placement.
                    Mouse.SetMousePos(x, linePosY); 
                    Console.WriteLine($"Clicked for bar {rectI} with {brightPixelCount} bright pixels.");

                    Thread.Sleep(110); // Keep existing delay
                    break; 
                }
                
                // It's good practice to add a small sleep in a tight while(true) loop 
                // if the condition isn't met, to prevent high CPU usage.
                // However, the original code didn't have this, and the game's lockpicking speed might be sensitive.
                // Let's add a very small one for now.
                Thread.Sleep(10); // Added a small delay to prevent busy-waiting if no click condition met.
            }
        }

        Console.WriteLine("i ~ Robbing Finished!");
    }
}