package com.codeyou.agent;

import com.microsoft.semantickernel.semanticfunctions.annotations.DefineKernelFunction;
import com.microsoft.semantickernel.semanticfunctions.annotations.KernelFunctionParameter;

import javax.script.ScriptEngine;
import javax.script.ScriptEngineManager;

/**
 * Plugin for mathematical operations
 */
public class MathPlugin {
    
    private final ScriptEngine engine;
    
    public MathPlugin() {
        ScriptEngineManager manager = new ScriptEngineManager();
        this.engine = manager.getEngineByName("JavaScript");
    }
    
    @DefineKernelFunction(
        name = "calculate",
        description = "Evaluates a mathematical expression",
        returnDescription = "The result of the mathematical expression"
    )
    public String calculate(
            @KernelFunctionParameter(
                name = "expression",
                description = "A mathematical expression like '25 * 4 + 10'"
            ) String expression) {
        try {
            Object result = engine.eval(expression);
            return result.toString();
        } catch (Exception e) {
            return "Error: " + e.getMessage();
        }
    }
}
