using UnityEngine;
using System.Diagnostics;

public class OpenPDF : MonoBehaviour
{
    public string pdfPath = "Assets/poly_map.pdf";

    public void OpenMyPDF()
    {
        string fullPath = System.IO.Path.Combine(Application.dataPath, "../", pdfPath);
        Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }
}
