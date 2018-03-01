namespace ForgelightUnity.Forgelight.Utils
{
    using System.IO;
    using UnityEditor;

    public class ValidationResult
    {
        public bool result;
        public string errorTitle;
        public string errorDesc;
        public string path;
    }

    public class DialogUtils
    {
        public delegate ValidationResult Validate(string path);

        public static string OpenDirectory(string title, string folder, string defaultName)
        {
            return OpenDirectory(title, folder, defaultName, null);
        }

        public static string SaveDirectory(string title, string defaultDirectory, string defaultName)
        {
            return SaveDirectory(title, defaultDirectory, defaultName, null);
        }

        public static string SaveFile(string title, string defaultDirectory, string defaultName, string extension)
        {
            return SaveFile(title, defaultDirectory, defaultName, extension, null);
        }

        public static string OpenFile(string title, string startingDirectory, string extension)
        {
            return OpenFile(title, startingDirectory, extension, null);
        }

        public static ValidationResult DirectoryIsEmpty(string path)
        {
            ValidationResult validationResult = new ValidationResult();

            string[] files = Directory.GetFiles(path);

            if (files.Length > 0)
            {
                validationResult.result = true;
                validationResult.path = path;
            }
            else
            {
                validationResult.result = false;
                validationResult.errorTitle = "Invalid Directory";
                validationResult.errorDesc = "Please select a directory that contains files.";
            }

            return validationResult;
        }

        private static ValidationResult ValidatePath(string path, Validate validationMethod)
        {
            ValidationResult valResult;

            if (path.Length > 0)
            {
                if (validationMethod != null)
                {
                    valResult = validationMethod(path);
                }

                else
                {
                    valResult = new ValidationResult { result = true, path = path};
                }
            }

            else
            {
                valResult = new ValidationResult { result = false };
            }

            return valResult;
        }

        public static string OpenDirectory(string title, string folder, string defaultName, Validate validationMethod)
        {
            var path = EditorUtility.OpenFolderPanel(title, folder, defaultName);

            ValidationResult valResult = ValidatePath(path, validationMethod);

            if (valResult.result)
            {
                return valResult.path;
            }

            string errTitle = "Invalid Directory";
            string errDesc = "Please select a valid directory";

            if (valResult.errorTitle != null && valResult.errorDesc != null)
            {
                errTitle = valResult.errorTitle;
                errDesc = valResult.errorDesc;
            }

            bool result = DisplayCancelableDialog(errTitle, errDesc);

            if (result)
            {
                return OpenDirectory(title, folder, defaultName, validationMethod);
            }

            return null;
        }

        public static string SaveDirectory(string title, string defaultDirectory, string defaultName, Validate validationMethod)
        {
            var path = EditorUtility.SaveFolderPanel(title, defaultDirectory, defaultName);

            ValidationResult valResult = ValidatePath(path, validationMethod);

            if (valResult.result)
            {
                return valResult.path;
            }

            string errTitle = "Invalid Directory";
            string errDesc = "Please select a valid save location.";

            if (valResult.errorTitle != null && valResult.errorDesc != null)
            {
                errTitle = valResult.errorTitle;
                errDesc = valResult.errorDesc;
            }

            bool result = DisplayCancelableDialog(errTitle, errDesc);

            if (result)
            {
                return SaveDirectory(title, defaultDirectory, defaultName, validationMethod);
            }

            return null;
        }

        public static string SaveFile(string title, string defaultDirectory, string defaultName, string extension, Validate validationMethod)
        {
            var path = EditorUtility.SaveFilePanel(title, defaultDirectory, defaultName, extension);

            ValidationResult valResult = ValidatePath(path, validationMethod);

            if (valResult.result)
            {
                return valResult.path;
            }

            string errTitle = "Invalid Directory";
            string errDesc = "Please select a valid save location.";

            if (valResult.errorTitle != null && valResult.errorDesc != null)
            {
                errTitle = valResult.errorTitle;
                errDesc = valResult.errorDesc;
            }

            bool result = DisplayCancelableDialog(errTitle, errDesc);

            if (result)
            {
                return SaveFile(title, defaultDirectory, defaultName, extension, validationMethod);
            }

            return null;
        }

        public static string OpenFile(string title, string startingDirectory, string extension, Validate validationMethod)
        {
            var path = EditorUtility.OpenFilePanel(title, startingDirectory, extension);

            ValidationResult valResult = ValidatePath(path, validationMethod);

            if (valResult.result)
            {
                return valResult.path;
            }

            string errTitle = "Invalid File Path";
            string errDesc = "Please select a valid file path";

            if (valResult.errorTitle != null && valResult.errorDesc != null)
            {
                errTitle = valResult.errorTitle;
                errDesc = valResult.errorDesc;
            }

            bool result = DisplayCancelableDialog(errTitle, errDesc);

            if (result)
            {
                return OpenFile(title, startingDirectory, extension, validationMethod);
            }

            return null;
        }

        public static bool DisplayCancelableDialog(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "OK", "Cancel");
        }

        public static bool DisplayDialog(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "OK");
        }
    }
}
