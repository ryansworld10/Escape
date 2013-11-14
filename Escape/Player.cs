﻿using System;
using System.Collections.Generic;

namespace Escape
{
	static class Player
	{
		#region Declarations
		//Defines all the base stats for the player. These don't change as the they are modified based on the player's level at runtime
		private const int baseHealth = 100;
		private const int baseMagic = 100;

		private const int power = 15;
		private const int defense = 10;

		//Defines the player's name and what their current location is
		private static string name;
		private static int location = 0;

		//Defines the player's current level and exp point total
		private static int level = 1;
		private static int exp = 0;
		
		//Set's the player's starting health and magic to their max.
		private static int health = MaxHealth;
		private static int magic = MaxMagic;	
		
		//Creates and empty list for the player's inventory
		public static List<int> Inventory = new List<int>();

		//Creates a list for the player's attacks and gives them the default "scratch"
		public static List<int> Attacks = new List<int>() { 1 };
		#endregion

		#region Properties
		//All properties expose the player's private variables to the other classes. This allows for additional logic and bug checking
		//Name allows the player's name to be write-once then read-only
		public static string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (name != "")
				{
					name = value;
				}
				else
				{
					//Error: PL44
					Program.SetError("Go tell the developer he dun goofed. Error: PL44");
				}
			}
		}

		//Location checks that the value assigned is actually a location. This should normally never cause error PL64 though
		public static int Location
		{
			get
			{
				return location;
			}
			set
			{
				if (World.IsLocation(value))
				{
					location = value;
				}
				else
				{
					//Error: PL64
					Program.SetError("Go tell the developer he dun goofed. Error: PL64");
				}
			}
		}

		//Health checks that the value assigned is never more than the max. It also ends the game if the value is below 0
		public static int Health
		{
			get
			{
				return health;
			}
			set
			{
				health = Math.Min(value, MaxHealth);

				if (health <= 0)
				{
					Program.GameState = Program.GameStates.GameOver;
				}
			}
		}

		//MaxHealth returns baseHealth modified by the player's level
		public static int MaxHealth
		{
			get
			{
				return BattleCore.CalculateHealthStat(baseHealth, level);
			}
		}

		//Magic checks that the value assigned is never more than the max
		public static int Magic
		{
			get
			{
				return magic;
			}
			set
			{
				magic = Math.Max(Math.Min(value, MaxMagic), 0);
			}
		}
		
		//MaxMagic returns baseMagic modified by the player's level
		public static int MaxMagic
		{
			get
			{
				return BattleCore.CalculateHealthStat(baseMagic, level);
			}
		}

		//Level doesn't currently add any logic
		public static int Level
		{
			get
			{
				return level;
			}
			set
			{
				level = value;
			}
		}

		//Exp levels the player up when the new exp value is greater than the amount required to level up
		public static int Exp
		{
			get
			{
				return exp;
			}
			set
			{
				exp = value;

				while (exp >= GetNextLevel())
				{
					LevelUp();
				}
			}
		}

		//Power returns power modified by the player's level
		public static int Power
		{
			get
			{
				return BattleCore.CalculateStat(power, level);
			}
		}

		//Defense returns defense modified by the player's level
		public static int Defense
		{
			get
			{
				return BattleCore.CalculateStat(defense, level);
			}
		}
		#endregion

		#region Public Methods
		//This interprets the player's input
		public static void Do(string aString)
		{		
			string verb = "";
			string noun = "";
			
			//This splits the player's input into a verb and a noun
			if (aString.IndexOf(" ") > 0)
			{
				string[] temp = aString.Split(new char[] {' '}, 2);
				verb = temp[0].ToLower();
				noun = temp[1].ToLower();
			}
			//If the player didn't give a noun, just set a verb
			else
			{
				verb = aString.ToLower();
			}
			
			switch(Program.GameState)
			{
				case Program.GameStates.Playing:
					switch(verb)
					{
						case "help":
						case "?":
							WriteCommands();
							break;
						case "exit":
						case "quit":
							Program.GameState = Program.GameStates.Quit;
							break;
						case "move":
						case "go":
							MoveTo(noun);
							break;
						case "examine":
							Examine(noun);
							break;
						case "take":
						case "pickup":
							Pickup(noun);
							break;
						case "drop":
						case "place":
							Place(noun);
							break;
						case "use":
						case "item":
							Use(noun);
							break;
						case "items":
						case "inventory":
						case "inv":
							DisplayInventory();
							break;
						case "attack":
							Attack(noun);
							break;
						case "hurt":
							Player.Health -= Convert.ToInt32(noun);
							break;
						case "exp":
							GiveExp(Convert.ToInt32(noun));
							break;
						case "save":
							Program.Save();
							break;
						case "load":
							Program.Load();
							break;
						default:
							InputNotValid();
							break;							
					}
					break;
					
				case Program.GameStates.Battle:
					switch(verb)
					{
						case "help":
						case "?":
							WriteBattleCommands();
							break;
						case "attack":
							AttackInBattle(noun);
							break;
						case "flee":
						case "escape":
						case "run":
							//flee command
							break;
						case "use":
						case "item":
							UseInBattle(noun);
							break;
						case "items":
						case "inventory":
						case "inv":
							DisplayBattleInventory();
							BattleCore.CurrentTurn = "enemy";
							break;
						case "exit":
						case "quit":
							Program.GameState = Program.GameStates.Quit;
							break;
						default:
							{
								if (World.IsAttack(verb))
								{
									AttackInBattle(verb);
								}
								else
								{
									InputNotValid();
								}

								BattleCore.CurrentTurn = "enemy";
								break;
							}
					}
					break;
			}
		}

		public static void GiveAttack(string attackName)
		{
			if (World.IsAttack(attackName))
			{
				int attackId = World.GetAttackIDByName(attackName);

				Attacks.Add(attackId);
				Program.SetNotification("You learned the attack " + World.Attacks[attackId].Name + "!");
			}
			else
			{
				//Error: PL149
				Program.SetError("Go tell the developer he dun goofed. Error: PL149");
			}
		}

		public static void GiveItem(string itemName)
		{
			if (World.IsItem(itemName))
			{
				int itemId = World.GetItemIDByName(itemName);
				Inventory.Add(itemId);

				Program.SetNotification("You were given " + Text.AorAn(World.Items[itemId].Name));
			}
			else
			{
				//Error: PL177
				Program.SetError("Go tell the developer he dun goofed. Error: PL177");
			}
		}
		
		public static void RemoveItemFromInventory(int itemId)
		{
			if (ItemIsInInventory(itemId))
			{
				Inventory.Remove(itemId);
			}
		}

		public static List<int> ItemsUsableInBattle()
		{
			List<int> result = new List<int>();

			foreach (int item in Inventory)
			{
				if (World.Items[item].CanUseInBattle)
				{
					result.Add(item);
				}
			}

			return result;
		}

		public static void GiveExp(int amount)
		{
			Exp += amount;
		}

		public static int GetNextLevel()
		{
			int result = (int)Math.Pow(Level, 3) + 10;

			if (Level < 5)
			{
				result += 10;
			}

			return result;
		}
		#endregion
		
		#region Command Methods
		private static void WriteCommands()
		{
			Text.WriteColor("`g`Available Commands:`w`");
			Text.WriteColor("help/? - Display this list.");
			Text.WriteColor("exit/quit - Exit the game.");
			Text.WriteColor("move/go <`c`location`w`> - Move to the specified location.");
			Text.WriteColor("examine <`c`item`w`> - Show info about the specified item.");
			Text.WriteColor("take/pickup <`c`item`w`> - Put the specified item in your inventory.");
			Text.WriteColor("drop/place <`c`item`w`> - Drop the specified item from your inventory and place it in the world.");
			Text.WriteColor("items/inventory/inv - Display your current inventory.");
			Text.WriteColor("use/item <`c`item`w`> - Use the specified item.");
			Text.WriteColor("attack <`c`enemy`w`> - Attack the specified enemy.");
			Text.WriteColor("save/load - saves/loads the game respectively.");
			Text.BlankLines();
		}
		
		private static void MoveTo(string locationName)
		{
			if (World.IsLocation(locationName))
			{
				int locationId = World.GetLocationIDByName(locationName);
				
				if (World.Map[Location].ContainsExit(locationId))
				{
					Location = locationId;
					
					World.Map[Location].CalculateRandomBattle();
				}
				else if (Player.Location == locationId)
				{
					Program.SetError("You are already there!");
				}
				else
				{
					Program.SetError("You can't get there from here!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid location!");
			}
		}
		
		private static void Examine(string itemName)
		{
			int itemId = World.GetItemIDByName(itemName);
			
			if (World.IsItem(itemName))
			{				
				if (World.Map[Location].ContainsItem(itemId) || ItemIsInInventory(itemId))
				{
					World.ItemDescription(itemId);
				}
				else
				{
					Program.SetError("That item isn't here!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid item!");
			}
		}
		
		private static void Pickup(string itemName)
		{
			if (World.IsItem(itemName))
			{
				int itemId = World.GetItemIDByName(itemName);
				
				if (World.Map[Location].ContainsItem(itemId))
				{
					World.Map[Location].RemoveItem(itemId);
					Inventory.Add(itemId);
					Program.SetNotification("You put the " + World.Items[itemId].Name + " in your bag!");
				}
				else
				{
					Program.SetError("That item isn't here!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid item!");
			}
		}
		
		private static void Place(string itemName)
		{
			if (World.IsItem(itemName))
			{
				int itemId = World.GetItemIDByName(itemName);
				
				if (ItemIsInInventory(itemId))
				{
					Inventory.Remove(itemId);
					World.Map[Location].AddItem(itemId);
					Program.SetNotification("You placed the " + World.Items[itemId].Name + " in the room!");
				}
				else
				{
					Program.SetError("You aren't holding that item!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid item!");
			}
		}
		
		private static void Use(string itemName)
		{
			if (World.IsItem(itemName))
			{
				int itemId = World.GetItemIDByName(itemName);
				
				if (ItemIsInInventory(itemId))
				{
					World.Items[itemId].Use();
				}
				else
				{
					Program.SetError("You aren't holding that item!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid item!");
			}
		}

		private static void Attack(string enemyName)
		{
			if (World.IsEnemy(enemyName))
			{
				int enemyId = World.GetEnemyIDByName(enemyName);

				if (World.Map[Location].ContainsEnemy(enemyId))
				{
					BattleCore.StartBattle(enemyId);
					Program.SetNotification("You attacked the " + World.Enemies[enemyId].Name + ". Prepare for battle!");
				}
				else
				{
					Program.SetError("That enemy isn't able to take your call at the moment, please leave a message!..... **BEEP**");
				}
			}
			else
			{
				Program.SetError("That isn't a valid enemy!");
			}
		}
		
		private static void DisplayInventory()
		{
			if (Inventory.Count <= 0)
			{
				Program.SetNotification("You aren't carrying anything!");
				return;
			}
				
			Text.WriteColor("`m`/-----------------\\");
			Text.WriteColor("|`w`    Inventory    `m`|");
			Text.WriteLine(">-----------------<");
			
			for (int i = 0; i < Inventory.Count; i++)
			{
				string name = World.Items[Inventory[i]].Name;
				Text.WriteColor("|`w` " + name + Text.BlankSpaces(16 - name.Length, true) + "`m`|");
			}
			
			Text.WriteColor("\\-----------------/`w`");
			Text.BlankLines();
		}
		#endregion

		#region Battle Command Methods
		private static void WriteBattleCommands()
		{
			Text.WriteColor("`g`Available Battle Commands:`w`");
			Text.WriteColor("help/? - Display this list.");
			Text.BlankLines();
		}

		private static void AttackInBattle(string attackName)
		{
			if (World.IsAttack(attackName))
			{
				int attackID = World.GetAttackIDByName(attackName);

				if (AttackIsInInventory(attackID))
				{
					World.Attacks[attackID].Use();
				}
				else
				{
					Program.SetError("You don't know that attack!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid attack!");
			}
		}

		private static void UseInBattle(string itemName)
		{
			if (World.IsItem(itemName))
			{
				int itemId = World.GetItemIDByName(itemName);

				if (ItemIsInInventory(itemId))
				{
					if (World.Items[itemId].CanUseInBattle)
					{
						World.Items[itemId].UseInBattle();
						return;
					}
					else
					{
						Program.SetError("You can't use that item in battle!");
					}
				}
				else
				{
					Program.SetError("You aren't holding that item!");
				}
			}
			else
			{
				Program.SetError("That isn't a valid item!");
			}

			BattleCore.CurrentTurn = "enemy";
		}

		private static void DisplayBattleInventory()
		{
			if (Inventory.Count <= 0)
			{
				Program.SetNotification("You aren't carrying anything!");
				return;
			}

			Text.WriteColor("`m`/-----------------\\");
			Text.WriteColor("|`w`    Inventory    `m`|");
			Text.WriteColor("|`w`   Battle Only   `m`|");
			Text.WriteLine(">-----------------<");

			foreach (int item in Player.ItemsUsableInBattle())
			{
				string name = World.Items[item].Name;
				Text.WriteColor("|`w` " + name + Text.BlankSpaces(16 - name.Length, true) + "`m`|");
			}

			Text.WriteColor("\\-----------------/`w`");
			Text.BlankLines();
		}
		#endregion

		#region Helper Methods
		private static void InputNotValid()
		{
			Program.SetError("That isn't a valid command!");
		}
		
		private static bool ItemIsInInventory(int itemId)
		{
			if (Inventory.Contains(itemId))
				return true;
			else
				return false;
		}

		private static bool AttackIsInInventory(int attackId)
		{
			if (Attacks.Contains(attackId))
				return true;
			else
				return false;
		}

		private static void LevelUp()
		{
			exp -= GetNextLevel();
			Level++;
		}
		#endregion
	}
}
