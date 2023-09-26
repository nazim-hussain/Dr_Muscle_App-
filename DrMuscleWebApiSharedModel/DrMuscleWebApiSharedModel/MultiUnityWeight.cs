using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
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
        private float targetBodyweight;
        private string v;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dr.MaxMuscle.MultiUnityWeight"/> class. With default values (100kg).
        /// </summary>
        public MultiUnityWeight()
        {
            Entered = 100;
            Unity = WeightUnities.kg;
        }

        public MultiUnityWeight(float targetBodyweight, string v)
        {
            this.targetBodyweight = targetBodyweight;
            this.v = v;
        }

        public MultiUnityWeight(decimal weight, string unity, bool isRound = true)
        {
            Entered = weight;
            Unity = (WeightUnities)Enum.Parse(typeof(WeightUnities), unity);
            IsRound = isRound;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dr.MaxMuscle.MultiUnityWeight"/> class. With the specified weight in the specified unity
        /// </summary>
        /// <param name="weight">Weight.</param>
        /// <param name="unity">Unity.</param>
        public MultiUnityWeight(decimal weight, WeightUnities unity, bool isRound = true)
        {
            Entered = weight;
            Unity = unity;
            IsRound = isRound;

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

        public bool IsRound { get; set; } = true;
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
                var lbEntered = Entered;
                if (Unity == WeightUnities.lb)
                    lbEntered = Entered;
                else
                    lbEntered = Entered * (decimal)2.2046244201837751;
                //1.00000000000000000000
                //113776
                //Calculator 2.204624420183778
                try
                {
                    if (IsRound)
                    {
                        if (lbEntered.ToString().Contains("."))
                        {
                            var subCount = lbEntered.ToString().Substring(lbEntered.ToString().IndexOf('.'));
                            if (subCount.Length > 2)
                            {
                                var val = decimal.Parse(subCount);
                                if (val > (decimal)0.40 && val < (decimal)0.45)
                                    lbEntered += (decimal)0.05;
                                if (val > (decimal)0.90)
                                    return System.Math.Round(lbEntered);
                                else if ((val > (decimal)0.40 && val < (decimal)0.50) || (val > (decimal)0.15 && val < (decimal)0.20) || (val > (decimal)0.58 && val < (decimal)0.60) || (val > (decimal)0.68 && val < (decimal)0.70))
                                    return System.Math.Round(lbEntered, 1);
                                else
                                    return lbEntered;

                            }
                            return subCount.Length > 2 ? System.Math.Round(lbEntered, 2) : lbEntered;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                return lbEntered;
            }
        }
    }
}
