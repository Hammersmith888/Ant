﻿using System;

namespace BugsFarm.Services.StatsService
{
    [Serializable]
    public class StatVitalDto : StatAttributeDto
    {
        public float CurrentValue;

        public StatVitalDto(StatModel initModel) : base(initModel)
        {
            CurrentValue = initModel.InitValue;
        }
    }
}