using System.ComponentModel;
using System.Data;
using Microsoft.SemanticKernel;

namespace SemanticKernelAgent
{
  /// <summary>
  /// Plugin for mathematical operations
  /// </summary>
  public class MathPlugin
    {
        [KernelFunction, Description("Evaluates a mathematical expression")]
        public string Calculate([Description("A mathematical expression like '25 * 4 + 10'")] string expression)
        {
            try
            {
                // Simple expression evaluation
                var result = new DataTable().Compute(expression, null);
                return result?.ToString() ?? "Unable to compute";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
