using FileEmulationFramework.Interfaces;
using FileEmulationFramework.Lib.Utilities;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using SPC.Stream.Emulator.Configuration;
using SPC.Stream.Emulator.Template;

namespace SPC.Stream.Emulator
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private Logger _log;
        private SpcEmulator _spcEmulator;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _logger = context.Logger;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            _modLoader.ModLoading += OnModLoading;
            _modLoader.OnModLoaderInitialized += OnModLoaderInitialized;
            _log = new Logger(_logger, _configuration.LogLevel);
            _log.Info("Starting SPC.Stream.Emulator");
            _spcEmulator = new SpcEmulator(_log, _configuration.DumpSpc);
            _log.Info("Started SPC.Stream.Emulator 2");
            _modLoader.GetController<IEmulationFramework>().TryGetTarget(out var framework);
            _log.Info("Started SPC.Stream.Emulator 3");
            framework!.Register(_spcEmulator);
            _log.Info("Started SPC.Stream.Emulator");
        }
        private void OnModLoaderInitialized()
        {
            _modLoader.ModLoading -= OnModLoading;
            _modLoader.OnModLoaderInitialized -= OnModLoaderInitialized;
        }

        private void OnModLoading(IModV1 mod, IModConfigV1 modConfig) => _spcEmulator.OnModLoading(_modLoader.GetDirectoryForModId(modConfig.ModId));



        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
            _log.LogLevel = configuration.LogLevel;
            _configuration.DumpSpc = configuration.DumpSpc;
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}