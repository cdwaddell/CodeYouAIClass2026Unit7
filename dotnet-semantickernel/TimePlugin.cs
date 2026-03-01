using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SemanticKernelAgent
{
  /// <summary>
  /// Plugin for time-related functions
  /// </summary>
  public class TimePlugin
    {
        [KernelFunction, Description("Gets the current date and time")]
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
