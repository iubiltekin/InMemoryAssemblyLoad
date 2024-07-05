using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SecondModule
{
    public class Recovery
    {
        public string Execute()
        {
            var outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("Second Recovery Module output");

            var systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var filePath = Path.Combine(systemFolder, "timeout.exe");
            Process.Start(filePath, "/t 10");

            return outputBuilder.Length > 0 ? outputBuilder.ToString().TrimEnd() : null;
        }
    }
}
