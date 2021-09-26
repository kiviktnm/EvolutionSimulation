using System.Collections.Generic;
using Windore.Simulations2D.Data;

namespace Windore.EvolutionSimulation.Objects
{
    public abstract class OrganismProperties
    {
        public Dictionary<string, Property> Properties { get; protected set; } = new Dictionary<string, Property>();

        /// <summary>
        /// Gets or sets the property defining the fully grown size of the organism
        /// </summary>
        [DataPoint("AdultSize")]
        public Property AdultSize { get => Properties["Adult Size"]; set => Properties["Adult Size"] = value; }

        /// <summary>
        /// Gets or sets the property defining the strength of any mutation affecting any property
        /// </summary>
        [DataPoint("MutationStrength")]
        public Property MutationStrength { get => Properties["Mutation Strength"]; set => Properties["Mutation Strength"] = value; }

        /// <summary>
        /// Gets or sets the property defining the energy the organism uses for reproduction
        /// </summary>
        [DataPoint("ReproductionEnergy")]
        public Property ReproductionEnergy { get => Properties["Reproduction Energy"]; set => Properties["Reproduction Energy"] = value; }

        /// <summary>
        /// Gets or sets the property defining the average offspring amount
        /// </summary>
        [DataPoint("OffspringAmount")]
        public Property OffspringAmount { get => Properties["Offspring Amount"]; set => Properties["Offspring Amount"] = value; }

        /// <summary>
        /// Gets or sets the property defining the percentage of energy to be spared before reproducing
        /// </summary>
        [DataPoint("BackupEnergy")]
        public Property BackupEnergy { get => Properties["Backup Energy"]; set => Properties["Backup Energy"] = value; }

        /// <summary>
        /// Gets or sets the property defining the optimal temperature
        /// </summary>
        [DataPoint("OptimalTemperature")]
        public Property OptimalTemperature { get => Properties["Optimal Temperature"]; set => Properties["Optimal Temperature"] = value; }

        /// <summary>
        /// Gets or sets the property defining the resistance to changes in temperature
        /// </summary>
        [DataPoint("TemperatureChangeResistance")]
        public Property TemperatureChangeResistance { get => Properties["Temperature Change Resistance"]; set => Properties["Temperature Change Resistance"] = value; }

        /// <summary>
        /// Gets or sets the property defining the resistance to changes in temperature
        /// </summary>
        [DataPoint("EnvironmentToxicityResistance")]
        public Property EnvironmentToxicityResistance { get => Properties["Environment Toxicity Resistance"]; set => Properties["Environment Toxicity Resistance"] = value; }
    }
}