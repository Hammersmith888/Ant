using System;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete.Components
{
    public static class AiFactory
    {
        public static IAiControlable Create(StateAnt aiType, int priority)
        {
            switch (aiType)
            {
                case StateAnt.Fall:         return new SM_Fall(priority,aiType);                            
                case StateAnt.Drink:        return new SM_Drink(priority,aiType);                           
                case StateAnt.Eat:          return new SM_Eat(priority,aiType);                             
                case StateAnt.Rest:         return new SM_Rest(priority,aiType);                            
                case StateAnt.Sleep:        return new SM_Sleep(priority,aiType);                           
                case StateAnt.Dead:         return new SM_Death(priority,aiType);                           
                case StateAnt.Restock:      return new SM_RestockFood(priority,aiType);          
                case StateAnt.RestockFight: return new SM_RestockFight(priority,aiType);
                case StateAnt.RestockHerbs: return new SM_RestockHerbs(priority,aiType);
                case StateAnt.Walk:         return new SM_Walk(priority,aiType);    
                case StateAnt.Feed:         return new SM_Feed(priority,aiType);    
                case StateAnt.Goldmine:     return new SM_Goldmine(priority,aiType);
                case StateAnt.Clean:        return new SM_Clean(priority,aiType);   
                case StateAnt.Garden:       return new SM_Garden(priority,aiType);  
                case StateAnt.Build:        return new SM_Build(priority,aiType);
                case StateAnt.Dig:          return new SM_Dig(priority,aiType);      
                case StateAnt.VineBuild:    return new SM_VineBuild(priority,aiType);   
                case StateAnt.Patrol:       return new SM_Patrol(priority,aiType);       
                case StateAnt.TrainArcher:  return new SM_TrainArcher(priority,aiType);  
                case StateAnt.TrainPikeman: return new SM_TrainPikeman(priority,aiType); 
                case StateAnt.TrainWorker:  return new SM_TrainWorker(priority,aiType);  
                case StateAnt.Dealer:       return new SM_Dealer(priority,aiType);  
                default: Debug.LogException(new Exception($"Такого AI не существует : {aiType}")); 
                    return default;
            }
        }
    }
}