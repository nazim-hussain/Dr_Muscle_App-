using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public enum WeightUnities
    {
        kg,
        lb
    }

    /// <summary>
	/// Multi unity weight. Handle a weight entered in kg or lbs and able to convert between those two.
	/// </summary>
	public class MultiUnityWeight
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dr.MaxMuscle.MultiUnityWeight"/> class. With default values (100kg).
        /// </summary>
        public MultiUnityWeight()
        {
            Entered = 100;
            Unity = WeightUnities.kg;
        }

        public MultiUnityWeight(decimal weight, string unity)
        {
            Entered = weight;
            Unity = (WeightUnities)Enum.Parse(typeof(WeightUnities), unity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dr.MaxMuscle.MultiUnityWeight"/> class. With the specified weight in the specified unity
        /// </summary>
        /// <param name="weight">Weight.</param>
        /// <param name="unity">Unity.</param>
        public MultiUnityWeight(decimal weight, WeightUnities unity)
        {
            Entered = weight;
            Unity = unity;
        }

        /// <summary>
        /// Gets or sets the unity.
        /// </summary>
        /// <value>The unity.</value>
        public WeightUnities Unity { get; set; }
        /// <summary>
        /// Gets or sets the entered weight.
        /// </summary>
        /// <value>The entered.</value>
        public decimal Entered { get; set; }
        /// <summary>
        /// Gets the weight in kg.
        /// </summary>
        /// <value>The kg.</value>
        public decimal Kg
        {
            get
            {
                if (Unity == WeightUnities.kg)
                    return Entered;
                else
                    return Entered * (decimal)0.453592;
            }
        }
        /// <summary>
        /// Gets the weight in lb.
        /// </summary>
        /// <value>The lb.</value>
        public decimal Lb
        {
            get
            {
                if (Unity == WeightUnities.lb)
                    return Entered;
                else
                    return Entered * (decimal)2.20462;
            }
        }
    }
}
