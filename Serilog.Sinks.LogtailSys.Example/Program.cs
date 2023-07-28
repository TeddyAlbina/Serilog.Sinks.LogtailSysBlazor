﻿// See https://aka.ms/new-console-template for more information


using Serilog;
using Serilog.Debugging;

var logConfig = new LoggerConfiguration()
    .WriteTo.Console();
 var log = logConfig
    .WriteTo.Logtail(
        token: "8txbp8NA9s8n8gbbv8nH4RRz",
        appName: "HI"
    )
    .Enrich.FromLogContext()
    .CreateLogger();
    
SelfLog.Enable(Console.Error);

log.Error(new Exception("exp"), "Hello, World! 22");

Console.ReadKey();