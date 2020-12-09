﻿using System;

using BreadTh.StronglyApied.Attributes.Core;

namespace BreadTh.StronglyApied.Direct.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StronglyApiedAttributeAttribute : StronglyApiedRelationBaseAttribute
    {
        public StronglyApiedAttributeAttribute(string name = null) : base(name) { }
    }
}