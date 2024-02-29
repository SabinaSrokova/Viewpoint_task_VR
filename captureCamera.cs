using UnityEngine;
using System.IO;

public class CaptureCameraView : MonoBehaviour
{
    public Camera captureCamera; // Assign your camera to capture from the inspector

    public int captureWidth = 1152; // Desired width
    public int captureHeight = 648; // Desired height

    public string captureFileName = "CapturedImage.png";

    public void CaptureAndSave()
    {
        // Create a RenderTexture with the desired resolution
        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24);
        captureCamera.targetTexture = rt;

        // Render the camera's view into the RenderTexture
        captureCamera.Render();

        // Create a Texture2D and read the RenderTexture data into it
        Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Encode the Texture2D to a PNG file
        byte[] bytes = screenShot.EncodeToPNG();

        // Save the PNG to a file
        File.WriteAllBytes(captureFileName, bytes);

        Debug.Log("Capture saved to " + captureFileName);
    }

    // You can call this function from a button click or any other trigger
    // For example, you can attach this function to a UI button's OnClick event.

  //  public void CaptureButtonClicked()
   //  {
    //         CaptureAndSave();
   //  }
}