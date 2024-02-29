using UnityEngine;
using System.IO;

public class ScreenshotCapture : MonoBehaviour
{
    public string screenshotPrefix = "Screenshot";  // Prefix for screenshot filenames.
    public int superSize = 1;                        // SuperSize for the screenshot (1 for normal size).

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CaptureScreenshot();
        }
    }

    void CaptureScreenshot()
    {
        // Create the directory path for the "Screenshots" folder within the project directory.
        string screenshotDirectory = "C:/Users/sabinasrokova/Desktop/fmri_viewpoint_task/Screenshots";

        // Create the directory if it doesn't exist.
        if (!Directory.Exists(screenshotDirectory))
        {
            Directory.CreateDirectory(screenshotDirectory);
        }

        // Define the file path for the screenshot.
        string screenshotPath = $"{screenshotDirectory}/{screenshotPrefix}_{Time.time}.png";

        // Capture the screenshot with the specified superSize.
        ScreenCapture.CaptureScreenshot(screenshotPath, superSize);
    }
}