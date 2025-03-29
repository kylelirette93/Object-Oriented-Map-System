using System;

namespace Object_Oriented_Map_System.Entities
{
    public class HealthComponent
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public event Action OnHealthChanged; // Event for UI updates (health bars, etc.)
        public event Action OnDeath; // Event for when entity dies

        public HealthComponent(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            CurrentHealth -= amount;
            if (CurrentHealth < 0) CurrentHealth = 0;

            OnHealthChanged?.Invoke(); // Notify UI or animations

            if (CurrentHealth == 0)
            {
                OnDeath?.Invoke(); // Notify that this entity has died
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged?.Invoke();
        }
    }
}