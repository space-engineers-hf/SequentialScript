using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    static class ActionProfiles
    {

        static readonly IList<IActionProfile> _profiles = new List<IActionProfile>()
        {
            // Core actions first, especific block type actions last.
            new ActionProfileFunctionalBlockEnable(), new ActionProfileFunctionalBlockDisable(),
            new ActionProfilePistonBaseExtend(), new ActionProfilePistonBaseRetract(),
            new ActionProfileMergeBlockEnable(), new ActionProfileMergeBlockDisable(),
            new ActionProfileDoorOpen(), new ActionProfileDoorClose(),
            new ActionProfileAirVentPressurize(), new ActionProfileAirVentDepressurize(),
            new ActionProfileTimerStart(), new ActionProfileTimerStop(), new ActionProfileTimerTrigger(),
            new ActionProfileSoundPlay(), new ActionProfileSoundStop(),
            new ActionProfileMotorStatorForward(), new ActionProfileMotorStatorBack(),
            new ActionProfileProgrammableBlockRun(),
            new ActionProfileBatteryAuto(), new ActionProfileBatteryRecharge(), new ActionProfileBatteryDischarge(),
            new ActionProfileConnectorLock(), new ActionProfileConnectorUnlock(),
            new ActionProfileLightSet(),
            new ActionProfileGasTankStockpile(), new ActionProfileGasTankAuto(),
            new ActionProfileLcdDisplay(),
            new ActionProfileThrusterSet()
        };

        public static IActionProfile GetActionProfile(IMyTerminalBlock block, string action)
        {
            IActionProfile actionProfile = _profiles.LastOrDefault(profile => profile.IsAssignableFrom(block) && profile.ActionNames.Contains(action, StringComparer.OrdinalIgnoreCase));

            if (actionProfile == null)
            {
                throw new NullReferenceException($"There is no implementation for type '{block.DefinitionDisplayNameText}' -> '{action}' ({block.DisplayNameText}).");
            }
            return actionProfile;
        }

    }
}
