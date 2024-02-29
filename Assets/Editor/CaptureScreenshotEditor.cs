#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class EditorScreenshotCapture : MonoBehaviour
{
    [MenuItem("Tools/Capture Screenshot")]
    public static void CaptureScreenshot()
    {
        string screenshotPrefix = "Screenshot";  // Prefix for screenshot filenames
        int width = 1330;                       // Width of the screenshot
        int height = 630;                       // Height of the screenshot
        int dpi = 100;                          // DPI (dots per inch)

        // Create the directory path for the "Screenshots" folder within the project directory
        string screenshotDirectory = "C:/Users/sabinasrokova/VR_SpatialMem_Aging/Screenshots";

        // Create the directory if it doesn't exist
        if (!Directory.Exists(screenshotDirectory))
        {
            Directory.CreateDirectory(screenshotDirectory);
        }

        // Define the file path for the screenshot
        string screenshotPath = $"{screenshotDirectory}/{screenshotPrefix}_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png";

        // Set the screen resolution and DPI
        Screen.SetResolution(width, height, false, dpi);

        // Capture the screenshot
        ScreenCapture.CaptureScreenshot(screenshotPath);
    }
}
#endif