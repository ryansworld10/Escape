﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Escape
{
    [Serializable]
    class Enemy : INamed
    {
        #region Declarations
        public string Name { get; set; }
        public string Description;

        public int Health;
        public int MaxHealth;

        public int Magic;
        public int MaxMagic;

        public int Power;
        public int Defense;
        public int ExpValue;

        public List<Attack> Attacks { get; set; }
        #endregion

        #region Constructor
        public Enemy(
            string name,
            // Some of these default values may not be sensible,
            // normally you'd always set some of them.
            string description = "",
            int health = 0,
            int maxHealth = 0,
            int magic = 0,
            int maxMagic = 0,
            int power = 0,
            int defense = 0,
            int expValue = 0,
            IEnumerable<Attack> attacks = null)
        {
            this.Name = name;

            // TODO: Handling health and mana max. without need for double lines outside.
            this.Description = description;
            this.Health = health;
            this.MaxHealth = maxHealth;
            this.Magic = magic;
            this.MaxMagic = maxMagic;
            this.Power = power;
            this.Defense = defense;
            this.ExpValue = expValue;
            this.Attacks = new List<Attack>(attacks ?? Enumerable.Empty<Attack>());
        }
        #endregion

        #region Public Methods
        // Removed attack accessors

        public void Attack()
        {
            Attacks[Program.Random.Next(this.Attacks.Count)].Use();
        }
        #endregion
    }
}
