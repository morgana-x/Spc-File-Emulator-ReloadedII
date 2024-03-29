﻿using System.ComponentModel;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Mod.Interfaces.Structs;
using SPC.Stream.Emulator.Template.Configuration;

namespace SPC.Stream.Emulator.Configuration
{
    public class Config : Configurable<Config>
    {
            [DisplayName("Log Level")]
            [Description("Declares which elements should be logged to the console.\nMessages less important than this level will not be logged.")]
            [DefaultValue(LogSeverity.Warning)]
            public LogSeverity LogLevel { get; set; } = LogSeverity.Information;

            [DisplayName("Dump Emulated Spc Files")]
            [Description("Creates a dump of emulated WAD files as they are written.")]
            [DefaultValue(LogSeverity.Information)]
            public bool DumpSpc { get; set; } = false;
        }

        /// <summary>
        /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
        /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
        /// </summary>
        public class ConfiguratorMixin : ConfiguratorMixinBase
        {
            // 
        }
    }
