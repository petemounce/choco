﻿namespace chocolatey.infrastructure.commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using filesystem;

    public sealed class PowershellExecutor
    {
        private static readonly IList<string> _powershellLocations = new List<string>
            {
                Environment.ExpandEnvironmentVariables("%windir%\\SysNative\\WindowsPowerShell\\v1.0\\powershell.exe"),
                Environment.ExpandEnvironmentVariables("%windir%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe"),
                "powershell.exe"
            };

        private static string _powershell = string.Empty;

        public static string wrap_command_with_module(string command, IFileSystem fileSystem)
        {
            //[System.Threading.Thread]::CurrentThread.CurrentCulture = ''; [System.Threading.Thread]::CurrentThread.CurrentUICulture = '';& location.ps1
            //todo: get args
            // $importChocolateyHelpers = "";
            //Get-ChildItem "$nugetChocolateyPath\helpers" -Filter *.psm1 | ForEach-Object { $importChocolateyHelpers = "& import-module -name  `'$($_.FullName)`';$importChocolateyHelpers" };
            return "[System.Threading.Thread]::CurrentThread.CurrentCulture = '';[System.Threading.Thread]::CurrentThread.CurrentUICulture = ''; {0}".format_with(command);
        }

        public static int execute(
            string command,
            IFileSystem fileSystem,
            Action<object, DataReceivedEventArgs> stdOutAction,
            Action<object, DataReceivedEventArgs> stdErrAction
            )
        {
            if (string.IsNullOrWhiteSpace(_powershell)) _powershell = get_powershell_location(fileSystem);
            //-NoProfile -NoLogo -ExecutionPolicy unrestricted -Command "[System.Threading.Thread]::CurrentThread.CurrentCulture = ''; [System.Threading.Thread]::CurrentThread.CurrentUICulture = '';& '%DIR%chocolatey.ps1' %PS_ARGS%"
            string arguments = "-NoProfile -NoLogo -ExecutionPolicy Bypass -Command \"{0}\"".format_with(command);

            return CommandExecutor.execute(
                _powershell,
                arguments,
                waitForExit: true,
                workingDirectory: fileSystem.get_directory_name(Assembly.GetExecutingAssembly().Location),
                stdOutAction: stdOutAction,
                stdErrAction: stdErrAction,
                updateProcessPath: true
                );
        }

        public static string get_powershell_location(IFileSystem fileSystem)
        {
            foreach (var powershellLocation in _powershellLocations)
            {
                if (fileSystem.file_exists(powershellLocation))
                {
                    return powershellLocation;
                }
            }

            throw new FileNotFoundException("Unable to find suitable location for PowerShell. Searched the following locations: '{0}'".format_with(string.Join("; ", _powershellLocations)));
        }
    }
}