using System.Diagnostics;
using System.IO;

namespace SecureData.IO
{
    public static class SecureDelete
    {
        public static bool DeleteFile(string filePath, int passes = 1)
        {
            if (!File.Exists("sdelete.exe"))
            {
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "sdelete.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-p {passes} -nobanner \"{filePath}\"";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            using (Process deleteProc = Process.Start(startInfo))
            {
                deleteProc.WaitForExit();

                string output = deleteProc.StandardOutput.ReadToEnd();
                return output.Contains("Files deleted: 1");
            }
        }

        public static bool IsPossible()
        {
            return File.Exists("sdelete.exe");
        }
    }
}
