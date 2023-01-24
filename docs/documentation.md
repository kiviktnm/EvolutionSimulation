# Documentation

## Agents

Evolution simulation is an agent based model.

Agents are divided into the following types:

- Environments
- Organisms
  - Plants
  - Animals

Each agent changes its state during an **update**.  Updates are
generally very small and not much will change in a single update.
Updating can be paused and the rate of updates can be limited.
Usually, the CPU will be the gratest limitor of updates. Reducing the
size of the simulation area can increase the update rate.

### Environments

Environments have three properties: **temperature**, **soil nutrient
content**, and **environmental toxin content**. These properties
affect the organisms within the environment. The effects are covered
in a different section.

There are three environments in the simulation area. By default,
temperature changes in the side environment 1, soil nutrient content
changes in the center environment, and environmental toxin content
changes in the side environment 2. The changes in environment’s
properties can be configured at program launch.

![The three environments of the
EvolutionSimulation](EvolutionSimulation_Environments.png)

The environments are sometimes refered by their position. For example
side environment 1 may be called the left side environment.

### Organisms

Organisms have multiple properties. These properties affect the
organism’s behaviour and usually energy consumption.

Adult Size

Defines the size of a fully grown organism. Functions like the size of
real world organisms. For example, larger animals need more energy,
but can also store more energy.

Mutation Effect Magnitude (%)

Defines how much mutations change the values of properties. Functions
as a percentage coefficient for mutation changes.

Reproduction Energy (%)

Defines the amount of energy used for reproduction. This value is
represented as a percentage of maximum stored energy.

Offspring Amount

Defines the amount of offspring an organism will produce. This number
may be a decimal number. In this case the decimal part will function
as a chance of an additional offspring. The integer part defines the
minimum number of offspring.

Backup Energy (%)

Defines the amount of extra energy that is stored before the organism
reproduces. Functions as a percentage of maximum energy.

Optimal Temperature

Defines the optimal temperature for an organism. If the temperature
differs, the organism must use extra energy. Optimal temperature may
only be 80 while the environment’s temperature can get up to 100.

Temperature Change Resistance

Defines how much temperature changes affect an organism. Reduces the
effect of a difference between the optimal temperature and the current
temperature. Overall, increases energy consumption.

Environmental Toxin Resistance

Defines how much environmental toxin affects an organism. Reduces the
effect of high toxin content, but overall increases energy
consumption.

#### Plants

Plants are stationary organisms that can produce energy. The amount of
energy produced depends on the size of the plant, the amount of nearby
plants and how close they are and the soil nutrient content.

Plants have three unique properties in addition to organism
properties.

Toxicity

Defines how much plant toxin there is in a single size unit of the
plant. Animals consuming plants will get injured from plant toxins.
Increases energy consumption.

Reproduction Area

Defines how far can plant offspring appear. Placing an offspring
further requires more energy, but the property itself doesn't increase
energy consumption.

Energy Production In Low Nutrient Soil

Increases how much energy a plant will produce in low nutrient soil but
decreases energy production in high nutrient soil. Currently, this
property isn't functioning as is wanted and is pretty useless for plants.

***

TODO: Add missing documentation
