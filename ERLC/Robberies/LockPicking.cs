using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading; // Required for Thread.Sleep
using ERLC; // Required for Screen, Roblox, Mouse classes

namespace ERLC.Robberies;
public class LockPicking // Made non-static
{
    // Constants remain the same
    private const int StartTime = 1; // Time in seconds to wait before starting the lockpicking process.
    private const string LineColorHtmlString = "#FFC903"; // HTML color code for the yellow line in the lockpicking minigame.
    private const int BarSizeOffsetBase = 83; // Base offset in pixels for calculating the position of each bar in the lockpicking minigame.
    private const int UpperColorOffset = -4; // Vertical offset (in pixels) from the detected line to check for a secondary color point (original logic).
    private const int LowerColorOffset = 10; // Vertical offset (in pixels) from the detected line to check for a primary color point (original logic, now mostly superseded by area check).
    private const int RgbBrightnessThreshold = 140; // Minimum RGB value (for R, G, and B components) to consider a pixel "bright".
    private const int LineColorTolerance = 10; // Tolerance value used when searching for the line color, allowing for slight variations.
    private const int TargetAreaVerticalRadius = 3; // Defines the vertical radius (in pixels) above and below the detected line's Y-coordinate to scan for bright pixels.
    private const int MinBrightPixelsForClick = 4;  // Minimum number of bright pixels that must be detected within the target area to trigger a click.

    private Color _lineColorInstance = ColorTranslator.FromHtml(LineColorHtmlString); // Instance field for color

    // Helper method for drawing text
    private Action<Graphics> CreateTextAction(string text, PointF position, Brush brush, Font font)
    {
        return g => g.DrawString(text, font, brush, position);
    }
    
    // Helper method for drawing rectangles
    private Action<Graphics> CreateRectangleAction(Pen pen, Rectangle rect)
    {
        return g => g.DrawRectangle(pen, rect);
    }

    // Helper method for drawing filled rectangles (for markers, etc.)
    private Action<Graphics> CreateFilledRectangleAction(Brush brush, Rectangle rect)
    {
        return g => g.FillRectangle(brush, rect);
    }


    public void StartLockpickingWithOverlay(Action<List<Action<Graphics>>> updateOverlayCallback, Action<string> updateStatusCallback)
    {
        var drawingActions = new List<Action<Graphics>>();
        var defaultFont = new Font("Arial", 12, FontStyle.Bold);
        var statusFont = new Font("Arial", 16, FontStyle.Bold);
        var goodBrush = Brushes.LightGreen;
        var badBrush = Brushes.Red;
        var infoBrush = Brushes.Cyan;

        updateStatusCallback("Initializing Lockpicking...");
        drawingActions.Add(CreateTextAction("Initializing Lockpicking...", new PointF(10, 30), infoBrush, statusFont));
        updateOverlayCallback(new List<Action<Graphics>>(drawingActions)); // Make a copy

        Roblox.FocusRoblox();
        Thread.Sleep(StartTime * 1000);

        int barSizeOffset = (int)Math.Floor(BarSizeOffsetBase * Screen.SystemScaleMultiplier);

        updateStatusCallback("Searching for lockpicking line...");
        drawingActions.Clear();
        // Define a search area for the line (example: middle third of the screen horizontally, full height)
        Rectangle lineSearchArea = new Rectangle(
            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 3, 
            0, 
            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 3, 
            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
        );
        drawingActions.Add(CreateRectangleAction(new Pen(Color.FromArgb(100, Color.Blue), 2), lineSearchArea));
        drawingActions.Add(CreateTextAction("Searching for line...", new PointF(lineSearchArea.X + 5, lineSearchArea.Y + 5), infoBrush, defaultFont));
        updateOverlayCallback(new List<Action<Graphics>>(drawingActions));

        var (linePosX, linePosY) = Screen.LocateColor(_lineColorInstance, LineColorTolerance, lineSearchArea);
        
        drawingActions.Clear(); // Clear previous search area drawing

        if (linePosX == 0 && linePosY == 0)
        {
            updateStatusCallback("Lockpicking line NOT found!");
            drawingActions.Add(CreateTextAction("Lockpicking line NOT found!", new PointF(10, 30), badBrush, statusFont));
            updateOverlayCallback(new List<Action<Graphics>>(drawingActions));
            Thread.Sleep(2000); // Show message for a bit
            return;
        }

        updateStatusCallback($"Line found at ({linePosX}, {linePosY})");
        Rectangle lineMarkerRect = new Rectangle(linePosX - 5, linePosY - 5, 10, 10);
        drawingActions.Add(CreateFilledRectangleAction(Brushes.Green, lineMarkerRect));
        drawingActions.Add(CreateTextAction($"Line at ({linePosX},{linePosY})", new PointF(linePosX + 15, linePosY - 7), goodBrush, defaultFont));
        updateOverlayCallback(new List<Action<Graphics>>(drawingActions)); // Show line marker before proceeding
        Thread.Sleep(500); // Briefly show the line marker

        for (int rectI = 1; rectI < 7; rectI++)
        {
            int currentBarX = linePosX + (barSizeOffset * rectI);
            updateStatusCallback($"Checking bar {rectI} at X: {currentBarX}");
            
            // Prepare a fresh list for each bar's check cycle
            var currentBarDrawingActions = new List<Action<Graphics>>();
            currentBarDrawingActions.Add(CreateFilledRectangleAction(Brushes.Green, lineMarkerRect)); // Keep line marker
            currentBarDrawingActions.Add(CreateTextAction($"Line at ({linePosX},{linePosY})", new PointF(linePosX + 15, linePosY - 7), goodBrush, defaultFont));
            currentBarDrawingActions.Add(CreateTextAction($"Checking Bar {rectI}", new PointF(currentBarX - 30, linePosY - 50), infoBrush, defaultFont));


            while (true) // This inner loop should ideally have a timeout or escape
            {
                var perLoopDrawingActions = new List<Action<Graphics>>(currentBarDrawingActions); // Start with base drawings for this bar
                int brightPixelCount = 0;

                for (int yOffset = -TargetAreaVerticalRadius; yOffset <= TargetAreaVerticalRadius; yOffset++)
                {
                    int checkY = linePosY + yOffset;
                    Color pixelColor = Screen.GetColorAtPixel(currentBarX, checkY);
                    Rectangle pixelCheckRect = new Rectangle(currentBarX - 2, checkY - 2, 5, 5);
                    bool isBright = pixelColor.R > RgbBrightnessThreshold &&
                                    pixelColor.G > RgbBrightnessThreshold &&
                                    pixelColor.B > RgbBrightnessThreshold;

                    if (isBright)
                    {
                        brightPixelCount++;
                        perLoopDrawingActions.Add(CreateFilledRectangleAction(Brushes.LimeGreen, pixelCheckRect));
                    }
                    else
                    {
                        perLoopDrawingActions.Add(CreateRectangleAction(Pens.DarkGray, pixelCheckRect));
                    }
                }
                
                perLoopDrawingActions.Add(CreateTextAction($"Bright: {brightPixelCount}/{MinBrightPixelsForClick}", new PointF(currentBarX + 10, linePosY + 10), brightPixelCount >= MinBrightPixelsForClick ? goodBrush : badBrush, defaultFont));
                updateOverlayCallback(new List<Action<Graphics>>(perLoopDrawingActions));


                if (brightPixelCount >= MinBrightPixelsForClick)
                {
                    Mouse.LeftClick();
                    Mouse.SetMousePos(currentBarX, linePosY); // As per original logic
                    updateStatusCallback($"Clicked bar {rectI} ({brightPixelCount} bright pixels).");
                    
                    // Add visual confirmation of click
                    var finalBarActions = new List<Action<Graphics>>(currentBarDrawingActions); // Base drawings
                    finalBarActions.Add(CreateTextAction($"Clicked Bar {rectI}!", new PointF(currentBarX - 30, linePosY - 30), goodBrush, statusFont));
                    updateOverlayCallback(new List<Action<Graphics>>(finalBarActions));
                    Thread.Sleep(110); // Keep existing delay
                    break; 
                }
                
                Thread.Sleep(10); // Prevent busy-waiting
            }
        }
        
        drawingActions.Clear();
        updateStatusCallback("Lockpicking finished!");
        drawingActions.Add(CreateTextAction("Lockpicking Finished!", new PointF(10, 30), goodBrush, statusFont));
        updateOverlayCallback(new List<Action<Graphics>>(drawingActions));
        Thread.Sleep(1500); // Show final status
        
        // Clear overlay one last time
        updateOverlayCallback(new List<Action<Graphics>>());
    }
}