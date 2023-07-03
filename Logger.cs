using System;

public static class Logger
{
    public enum LoggerLevel
    {
        INFO,
        WARN,
        ERROR
    }

    public static void Log(string header, string message)
    {
        string str = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{header}] {message}";
        Console.WriteLine(str);
    }

    public static void Log(LoggerLevel level, string message)
    {
        if (level == LoggerLevel.WARN)
            Console.ForegroundColor = ConsoleColor.Yellow;
        else if (level == LoggerLevel.ERROR)
            Console.ForegroundColor = ConsoleColor.Red;
            
        Log(level.ToString(), message);
        Console.ResetColor();
    }

    public static void Info(string message)
    {
        Log(LoggerLevel.INFO, message);
    }

    public static void Warning(string message)
    {
        Log(LoggerLevel.WARN, message);
    }

        public static void Error(string message)
    {
        Log(LoggerLevel.ERROR, message);
    }
}