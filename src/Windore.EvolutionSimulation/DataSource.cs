using System.Collections.Generic;
using Windore.Simulations2D.Data;

namespace Windore.EvolutionSimulation
{
    public interface IDataSource
    {
        Dictionary<string, DataCollector.Data> GetData(DataType type);
        
    }
    
    public enum DataType 
    {
        Animal,
        Plant
    }
}