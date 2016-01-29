using System;
using System.IO;

public class AssetLoader
{
    public static string OpenAssetFolder()
    {
        string path = DialogUtils.OpenDirectory(
        "Select folder containing Forgelight pack files.",
        "",
        "", CheckGivenAssetDirectory);

        return path;
    }

    private static ValidationResult CheckGivenAssetDirectory(string path)
    {
        ValidationResult validationResult = new ValidationResult();

        String[] files = Directory.GetFiles(path);

        foreach (string fileName in files)
        {
            if (fileName.EndsWith(".pack"))
            {
                validationResult.result = true;
                return validationResult;
            }
        }

        validationResult.result = false;
        validationResult.errorTitle = "Invalid Asset Directory";
        validationResult.errorDesc = "The folder provided does not contain any valid .pack files. Please make sure you have selected the correct assets folder.";

        return validationResult;
    }
}
