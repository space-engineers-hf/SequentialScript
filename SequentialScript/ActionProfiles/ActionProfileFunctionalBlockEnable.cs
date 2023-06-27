﻿using Sandbox.Game.EntityComponents;
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
    sealed class ActionProfileFunctionalBlockEnable : ActionProfile<IMyFunctionalBlock>
    {

        public override string ActionName => "Enable";

        public override Action<IMyFunctionalBlock> OnActionCallback =>
            block => block.Enabled = true;

        public override Func<IMyFunctionalBlock, bool> IsCompleteCallback =>
            block => block.Enabled;
    }
}