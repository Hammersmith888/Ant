using System;
using BugsFarm.Services.CommandService;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public struct UnitSpawnProtocol : IProtocol
    {
        public string Guid { get; }
        public object[] Args { get;  private set; }
        public Vector2? SpawnPoint { get;  private set; }

        public UnitSpawnProtocol(string guid)
        {
            Guid = guid;
            Args = new object[0];
            SpawnPoint = null;
        }

        public UnitSpawnProtocol(string guid, Vector2 spwanPoint)
        {
            Guid = guid;
            Args = new object[0];
            SpawnPoint = spwanPoint;
        }
        public UnitSpawnProtocol SetExtraArgs(params object[] args)
        {
            Args = args;
            return this;
        }
    }
}