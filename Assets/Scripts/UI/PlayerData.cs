using System;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]

 [Serializable]
    public class PlayerData : BindableObject
    {
        private int _level = 1;
        private float _health = 100f;
        private float _maxHealth = 100f;
        private float _mana = 50f;
        private float _maxMana = 50f;
        private int _experience = 0;
        private int _experienceToNext = 100;
        private Vector3 _position;
        private string _playerName = "Player";
        
        public int Level 
        { 
            get => _level; 
            set => SetProperty(ref _level, value, nameof(Level)); 
        }
        
        public float Health 
        { 
            get => _health; 
            set => SetProperty(ref _health, Mathf.Clamp(value, 0, _maxHealth), nameof(Health)); 
        }
        
        public float MaxHealth 
        { 
            get => _maxHealth; 
            set 
            {
                SetProperty(ref _maxHealth, value, nameof(MaxHealth));
                Health = Mathf.Min(Health, MaxHealth);
            }
        }
        
        public float Mana 
        { 
            get => _mana; 
            set => SetProperty(ref _mana, Mathf.Clamp(value, 0, _maxMana), nameof(Mana)); 
        }
        
        public float MaxMana 
        { 
            get => _maxMana; 
            set 
            {
                SetProperty(ref _maxMana, value, nameof(MaxMana));
                Mana = Mathf.Min(Mana, MaxMana);
            }
        }
        
        public int Experience 
        { 
            get => _experience; 
            set => SetProperty(ref _experience, value, nameof(Experience)); 
        }
        
        public int ExperienceToNext 
        { 
            get => _experienceToNext; 
            set => SetProperty(ref _experienceToNext, value, nameof(ExperienceToNext)); 
        }
        
        public Vector3 Position 
        { 
            get => _position; 
            set => SetProperty(ref _position, value, nameof(Position)); 
        }
        
        public string PlayerName 
        { 
            get => _playerName; 
            set => SetProperty(ref _playerName, value, nameof(PlayerName)); 
        }
        
        public float HealthPercentage => MaxHealth > 0 ? Health / MaxHealth : 0f;
        public float ManaPercentage => MaxMana > 0 ? Mana / MaxMana : 0f;
        public float ExperiencePercentage => ExperienceToNext > 0 ? (float)Experience / ExperienceToNext : 0f;
    }