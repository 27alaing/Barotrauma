﻿using System.Xml.Linq;

namespace Barotrauma.Abilities
{
    class CharacterAbilityResetPermanentStat : CharacterAbility
    {
        private readonly string statIdentifier;
        public override bool RequiresAlive => false;

        public CharacterAbilityResetPermanentStat(CharacterAbilityGroup characterAbilityGroup, XElement abilityElement) : base(characterAbilityGroup, abilityElement)
        {
            statIdentifier = abilityElement.GetAttributeString("statidentifier", "").ToLowerInvariant();
        }
        protected override void ApplyEffect(object abilityData)
        {
            ApplyEffectSpecific();
        }

        protected override void ApplyEffect()
        {
            ApplyEffectSpecific();
        }

        private void ApplyEffectSpecific()
        {
            Character?.Info.ResetSavedStatValue(statIdentifier);
        }
    }
}
