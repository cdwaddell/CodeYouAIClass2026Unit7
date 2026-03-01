package com.codeyou.agent;

import com.microsoft.semantickernel.semanticfunctions.annotations.DefineKernelFunction;
import com.microsoft.semantickernel.semanticfunctions.annotations.KernelFunctionParameter;

/**
 * Plugin for string manipulation
 */
public class StringPlugin {
    
    @DefineKernelFunction(
        name = "reverseString",
        description = "Reverses a string",
        returnDescription = "The reversed string"
    )
    public String reverseString(
            @KernelFunctionParameter(
                name = "input",
                description = "The string to reverse"
            ) String input) {
        return new StringBuilder(input).reverse().toString();
    }
}
