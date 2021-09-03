﻿using System.Xml.Linq;

namespace Barotrauma.Abilities
{
    class CharacterAbilityModifyValue : CharacterAbility
    {
        private float addedValue;
        private float multiplyValue;

        public CharacterAbilityModifyValue(CharacterAbilityGroup characterAbilityGroup, XElement abilityElement) : base(characterAbilityGroup, abilityElement)
        {
            addedValue = abilityElement.GetAttributeFloat("addedvalue", 0f);
            multiplyValue = abilityElement.GetAttributeFloat("multiplyvalue", 1f);
        }

        protected override void ApplyEffect(object abilityData)
        {
            if (abilityData is IAbilityValue abilityValue)
            {
                abilityValue.Value += addedValue;
                abilityValue.Value *= multiplyValue;
            }
        }
    }
}
