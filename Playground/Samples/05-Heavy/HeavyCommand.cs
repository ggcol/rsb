﻿using Rsb.Abstractions;
using Rsb.Accessories.Heavy;

namespace Playground.Samples._05_Heavy;

public class HeavyCommand : IAmACommand
{
    public Heavy<string> AHeavyProp { get; set; }
}