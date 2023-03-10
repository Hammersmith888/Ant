using System;

namespace BugsFarm.AudioSystem.Obsolete
{
    public interface ISoundOccupant
    {
        bool CanSoundPlay { get; }
        /// <summary>
        /// Sound - клип, bool - зациклить
        /// </summary>
        event Action<Sound> OnSoundChange;
        event Action<ISoundOccupant> OnSoundFree;
    }
}