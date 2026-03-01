package com.codeyou.agent;

import com.microsoft.semantickernel.semanticfunctions.annotations.DefineKernelFunction;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

/**
 * Plugin for time-related functions
 */
public class TimePlugin {
    
    @DefineKernelFunction(
        name = "getCurrentTime",
        description = "Gets the current date and time",
        returnDescription = "The current date and time as a string"
    )
    public String getCurrentTime() {
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");
        return LocalDateTime.now().format(formatter);
    }
}
