using System;

namespace GameFramework.Effects
{
    /// <summary>
    /// Default implementation of <see cref="IBattery"/>.
    /// Designed for use in gameplay logic, simulation, or tests.
    /// </summary>
    public class Battery : IBattery
    {
        private float _maxCapacity;

        public float MaxCapacity
        {
            get => _maxCapacity;
            set
            {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxCapacity must be greater than zero.");

                _maxCapacity = value;
                CurrentCharge = Clamp(CurrentCharge, 0f, _maxCapacity);
            }
        }

        public float CurrentCharge { get; private set; }

        public float ChargeRatePerSecond { get; set; }

        public bool IsCharging => ChargeRatePerSecond > 0f;

        public float NormalizedCharge =>
            MaxCapacity <= 0f ? 0f : CurrentCharge / MaxCapacity;

        /// <summary>
        /// Creates a new battery instance.
        /// </summary>
        /// <param name="maxCapacity">Maximum battery capacity.</param>
        /// <param name="initialCharge">Initial charge value.</param>
        /// <param name="initialChargeRate">Initial charge rate in units per second.</param>
        public Battery(float maxCapacity, float initialCharge = 0f, float initialChargeRate = 0f)
        {
            if (maxCapacity <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity), "MaxCapacity must be greater than zero.");

            MaxCapacity = maxCapacity;
            CurrentCharge = Clamp(initialCharge, 0f, MaxCapacity);
            ChargeRatePerSecond = initialChargeRate;
        }

        public void SetChargeRate(float unitsPerSecond)
        {
            ChargeRatePerSecond = unitsPerSecond;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || ChargeRatePerSecond == 0f)
                return;

            float delta = ChargeRatePerSecond * deltaTime;
            CurrentCharge = Clamp(CurrentCharge + delta, 0f, MaxCapacity);
        }

        public float AddCharge(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float previous = CurrentCharge;
            CurrentCharge = Clamp(CurrentCharge + amount, 0f, MaxCapacity);
            return CurrentCharge - previous;
        }

        public float RemoveCharge(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float previous = CurrentCharge;
            CurrentCharge = Clamp(CurrentCharge - amount, 0f, MaxCapacity);
            return previous - CurrentCharge;
        }

        public void Deplete()
        {
            CurrentCharge = 0f;
        }

        public void Fill()
        {
            CurrentCharge = MaxCapacity;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}