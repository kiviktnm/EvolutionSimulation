using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public class AnimalProperties : OrganismProperties
    {
        /// <summary>
        /// Gets or sets the property defining the maximum units the animal can move in an update
        /// </summary>
        [DataPoint("MovementSpeed")]
        public Property MovementSpeed { get => Properties["Movement Speed"]; set => Properties["Movement Speed"] = value; }

        /// <summary>
        /// Gets or sets the property defining the probability of preferring an animal food source
        /// </summary>
        [DataPoint("CarnivorityTendency")]
        public Property CarnivorityTendency { get => Properties["Carnivority Tendency"]; set => Properties["Carnivority Tendency"] = value; }

        /// <summary>
        /// Gets or sets the property defining the vision range
        /// </summary>
        [DataPoint("Eyesight")]
        public Property Eyesight { get => Properties["Eyesight"]; set => Properties["Eyesight"] = value; }

        /// <summary>
        /// Gets or sets the property defining the capability to cause harm to other animals
        /// </summary>
        [DataPoint("OffensiveCapability")]
        public Property OffensiveCapability { get => Properties["Offensive Capability"]; set => Properties["Offensive Capability"] = value; }

        /// <summary>
        /// Gets or sets the property defining the capability to mitigate harm caused by other animals
        /// </summary>
        [DataPoint("DefensiveCapability")]
        public Property DefensiveCapability { get => Properties["Defensive Capability"]; set => Properties["Defensive Capability"] = value; }

        /// <summary>
        /// Gets or sets the property defining the amount larger an animals offensive and defensive capability has to be to be considered a threat.
        /// </summary>
        [DataPoint("ThreatConsiderationLimit")]
        public Property ThreatConsiderationLimit { get => Properties["Threat Consideration Limit"]; set => Properties["Threat Consideration Limit"] = value; }

        /// <summary>
        /// Gets or sets the property defining the animal's ability to resist plant poison
        /// </summary>
        [DataPoint("PlantToxicityResistance")]
        public Property PlantToxicityResistance { get => Properties["Plant Toxicity Resistance"]; set => Properties["Plant Toxicity Resistance"] = value; }

        /// <summary>
        /// Gets or sets the property defining the animal's ability to store food and to digest that stored food
        /// </summary>
        [DataPoint("FoodStoringAndDigestingCapability")]
        public Property FoodStoringAndDigestingCapability { get => Properties["Food Storing And Digesting Capability"]; set => Properties["Food Storing And Digesting Capability"] = value; }

        public AnimalProperties CreateMutated()
        {
            AnimalProperties newProperties = new AnimalProperties
            {
                // Copies the dictionary while mutating all properties
                Properties = Properties.ToDictionary(entry => entry.Key,
                    entry => entry.Value.CreateMutated(new Percentage(MutationStrength.Value)))
            };
            return newProperties;
        }
    }
}
