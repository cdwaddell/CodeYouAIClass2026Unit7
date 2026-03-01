using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SemanticKernelAgent
{
  /// <summary>
  /// Plugin for string manipulation
  /// </summary>
  public class StringPlugin
    {
        [KernelFunction, Description("Reverses a string")]
        public string ReverseString([Description("The string to reverse")] string input)
        {
            var charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
