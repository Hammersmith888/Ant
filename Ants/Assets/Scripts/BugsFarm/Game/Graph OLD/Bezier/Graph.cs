using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;
using Random = UnityEngine.Random;


/*
	https://stackoverflow.com/questions/5186394/using-conditional-operator-for-method-selection-in-c-sharp-3-0
	https://stackoverflow.com/a/5186442/4830242
*/


public class Graph : MB_Singleton<Graph>
{
    public static List<GVert> Roads => Instance?._roads;

    public static GVert Grass_1 => Instance?._grass_1;
    public static GVert Grass_2 => Instance?._grass_2;
    public static GVert Road_L1 => Instance?._road_L1;
    public static GVert Road_L2 => Instance?._road_L2;
    public static GVert Road_L3 => Instance?._road_L3;


    public static readonly Dictionary<int, List<GVert>> PlacesGroups = new Dictionary<int, List<GVert>>();


#pragma warning disable 0649

    [SerializeField] GVert _road_R4_2;
    [SerializeField] GVert _road_R7_1;

    [SerializeField] GVert _grass_1;
    [SerializeField] GVert _grass_2;
    [SerializeField] GVert _road_L1;
    [SerializeField] GVert _road_L2;
    [SerializeField] GVert _road_L3;

#pragma warning restore 0649

    UndirectedGraph<GVert, GEdge> _graph;
    Dictionary<GEdge, double> _edgeCosts;
    Func<GEdge, double> _edgeCost;


    readonly List<GVert> _roads = new List<GVert>();
    readonly List<GVert> _roadsOpened = new List<GVert>();

    readonly List<GVert> _beziers = new List<GVert>();
    readonly List<GVert> _joints = new List<GVert>();

    readonly BiDict<GVert, int> _gVerts = new BiDict<GVert, int>();


    void CollectAndInit_gVerts()
    {
        int key = 0;

        foreach (Transform level in transform)
            foreach (Transform child in level)
            {
                GVert gVert = child.GetComponent<GVert>();

                gVert.Init();
                _gVerts.Add(gVert, ++key);

                if (gVert.Type == GVertType.Joint)
                {
                    _joints.Add(gVert);
                }
                else
                {
                    _beziers.Add(gVert);

                    if (gVert.Type == GVertType.Road)
                    {
                        if (gVert.PlacesGroup < 1 || gVert.PlacesGroup > 8)
                            throw new Exception($"{ gVert.transform.parent.name} / {gVert.name} - unknown places group ({ gVert.PlacesGroup })");

                        _roads.Add(gVert);

                        if (!PlacesGroups.ContainsKey(gVert.PlacesGroup))
                            PlacesGroups[gVert.PlacesGroup] = new List<GVert>();

                        PlacesGroups[gVert.PlacesGroup].Add(gVert);
                    }
                }
            }
    }


    void Start()
    {
        CollectAndInit_gVerts();


        _graph = new UndirectedGraph<GVert, GEdge>(false);
        _edgeCosts = new Dictionary<GEdge, double>();
        _edgeCost = AlgorithmExtensions.GetIndexer(_edgeCosts);


        // Build graph
        foreach (GVert bezier in _beziers)
        {
            foreach (GVert joint in _joints)
            {
                if (Vector2.Distance(bezier.GetPointAt(0), joint.transform.position) < Constants.JointMinDist)
                {
                    AddEdge(joint, bezier);

                    bezier.SetJoint_t0(joint);
                }
                else if (Vector2.Distance(bezier.GetPointAt(1), joint.transform.position) < Constants.JointMinDist)
                {
                    AddEdge(joint, bezier);

                    bezier.SetJoint_t1(joint);
                }
            }

            bezier.Stretch();
        }


        // Calc walkable ranges (aside on ladders)
        foreach (GVert road in _roads)
        {
            SetWalkMinMax(road, road.Joint_t0, true);
            SetWalkMinMax(road, road.Joint_t1, false);
        }


        Init_OpenedRoads();
    }


    public void Init_OpenedRoads()
    {
        _roadsOpened.Clear();

        //foreach (GVert road in _roads)
        //    if (!RoomsBook.Instance.ClosedRoads.Contains(road))
        //        OpenRoad(road);
    }


    void SetWalkMinMax(GVert road, GVert joint, bool is0)
    {
        bool hasAdjacentLadder = false;

        if (joint)
            foreach (GEdge edge2 in AdjacentEdges(joint))
            {
                GVert adjacent = edge2.Other(joint);

                if (adjacent.IsLadder)
                {
                    hasAdjacentLadder = true;
                    break;
                }
            }


        Action<float> FSet = is0 ? (Action<float>)road.SetWalkTarget_tMin : road.SetWalkTarget_tMax;
        float t = 0;
        float ladderMinDist =
                                            road == _road_R4_2 || road == _road_R7_1 ?
                                            Constants.LadderMinDist / 2 :
                                            Constants.LadderMinDist
        ;

        if (hasAdjacentLadder)
            t += ladderMinDist / road.Length;

        if (!is0)
            t = 1 - t;

        FSet(t);
    }


    void AddEdge(GVert source, GVert target)
    {
        GEdge edge = new GEdge(source, target);

        _graph.AddVerticesAndEdge(edge);

        _edgeCosts.Add(edge, 1);

        // Debug.Log( $"edge: {source.name}, {target.name}" );
    }


    void OpenRoad(GVert road)
    {
        if (
                road.WalkTarget_tMin < 1 &&
                road.WalkTarget_tMax > 0 &&
                road.WalkTarget_tMin < road.WalkTarget_tMax
            )
            _roadsOpened.Add(road);
    }




    public IEnumerable<GEdge> AdjacentEdges(GVert gVert)
    {
        return _graph.AdjacentEdges(gVert);
    }


    public GPos PatrolPos(GPos gPos)
    {
        GVert gVert =
                            gPos.gVert == _road_L2 ?
                            _road_L1 :
                            _road_L2
        ;

        return new GPos(gVert, .5f);
    }


    public GPos RandomRoad(GVert except, bool checkOccupied, bool grassOnly = false)
    {
        const float minDist = Constants.RandomRoad_MinDist;

        const int maxAttempts = 30;
        int attempt = 0;


        GPos gPos = new GPos();
        bool success = false;

        while (
                !success &&
                attempt++ < maxAttempts
            )
        {
            gPos.gVert = grassOnly ? Tools.RandomBool() ? Grass_1 : Grass_2 :
                                     _roadsOpened[Random.Range(0, _roadsOpened.Count)]
            ;
            gPos.t = Random.Range(gPos.gVert.WalkTarget_tMin, gPos.gVert.WalkTarget_tMax);


            if (gPos.gVert == except)
                continue;


            success = true;
            if (checkOccupied)
            {
                Vector2 pos = gPos.GetPoint();

                // Not to close to the Bowl
                //if (Vector2.Distance(pos, MB_Bowl.Instance.Center_OnRoad) < Constants.RandomRoad_MinDist_Obj)
                //    success = false;

                // Not to close to the Goldmine
                /*
				if (
						success &&
						Vector2.Distance( pos, MB_Goldmine.Instance.Center_OnRoad ) < Constants.RandomRoad_MinDist_Obj
					)
					success				= false;
				*/

                // Not to close to Targets of others
                if (success)
                    foreach (var other in Occupied.Targets)
                        if (Vector2.Distance(pos, other.Value) < minDist)
                        {
                            success = false;
                            break;
                        }

                // Not to close to others currently walking
#pragma warning disable 0162
                if (false)
                    if (
                            success &&
                            Occupied.Walkers.ContainsKey(gPos.gVert)
                        )
                        foreach (var other in Occupied.Walkers[gPos.gVert])
                            if (Vector2.Distance(pos, other.Value.Position) < minDist)
                            {
                                success = false;
                                break;
                            }
#pragma warning restore 0162
            }
        }


        return gPos;
    }


    public bool FindPath( List<GStep> path, GPos source, GPos target )
    {
        bool result = FindPath(path, source.gVert, target.gVert, source.t, target.t);

        if (result)
        {
            // LogPath();
        }
        else
            Debug.Log(
                "Path not found! " +
                $"source: ({source.gVert.transform.parent.name}) {source.gVert.name} (t = {source.t:F4}), " +
                $"target: ({target.gVert.transform.parent.name}) {target.gVert.name} (t = {target.t:F4})"
            );

        return result;
    }


    void LogPath(List<GStep> path)
    {
        string str = "";
        foreach (GStep step in path)
        {
            str += step.gVert.name + $" ({ step.t0 })\n";
            str += step.gVert.name + $" ({ step.t1 })\n";
        }
        Debug.Log(str);
    }


    bool FindPath(
            List<GStep> path,
            GVert source,
            GVert target,
            float source_t,
            float target_t
        )
    {
        path.Clear();


        if (source == target)
        {
            path.Add(new GStep(source, source_t, target_t));
            return true;
        }


        var tryGetPaths = _graph.ShortestPathsDijkstra(_edgeCost, source);

        if (!tryGetPaths(target, out var pathEdges))
            return false;


        path.Add(new GStep(source, source_t));

        GVert cur = source;
        foreach (var edge in pathEdges)
        {
            GVert nxt = edge.Other(cur);

            if (nxt.Type == GVertType.Joint)
                Set_t1_OfLast(path, cur.Joint_t0 == nxt ? 0 : 1);
            else
                path.Add(new GStep(nxt, nxt.Joint_t0 == cur ? 0 : 1));

            cur = nxt;
        }
        Set_t1_OfLast(path, target_t);


        return true;
    }


    void Set_t1_OfLast(List<GStep> path, float t1)
    {
        int i = path.Count - 1;
        GStep gStep = path[i];
        path[i] = new GStep(gStep.gVert, gStep.t0, t1);
    }


    public GVert Key_2_GVert(int key) { return key == 0 ? null : _gVerts[key]; }
    public int GVert_2_Key(GVert gVert) { return gVert == null ? 0 : _gVerts[gVert]; }
}

