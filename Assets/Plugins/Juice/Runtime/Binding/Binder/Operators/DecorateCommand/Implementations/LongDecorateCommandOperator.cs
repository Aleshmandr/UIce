﻿using UnityEngine;

namespace Juice
{
    public class LongDecorateCommandOperator : DecorateCommandOperator<long>
    {
        protected override ConstantBindingInfo<long> DecorationBindingInfo => decorationBindingInfo;

        [SerializeField] private ConstantBindingInfo<long> decorationBindingInfo = new ConstantBindingInfo<long>();
    }
}
