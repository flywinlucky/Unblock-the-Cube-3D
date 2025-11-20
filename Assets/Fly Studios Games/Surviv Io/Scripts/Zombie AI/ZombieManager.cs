using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Needed for NavMesh.SamplePosition

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance;

    [Header("Referințe")]
    public Player player;
    public GameObject zombiePrefab;

    [Header("Setări Valuri (Waves)")]
    public int initialCount = 4;
    public float spawnDistance = 12f;
    public float timeBetweenWaves = 10f;

    [Header("Tactică de Încercuire")]
    public float surroundRadius = 3.5f; // Cât de larg e cercul în jurul playerului
    public float updateRate = 0.5f;     // Cât de des recalculăm pozițiile (optimizare)

    [Header("Dynamic Formation")]
    public float breatheAmplitude = 0.25f;      // % expansion around base (surroundRadius)
    public float breatheSpeed = 0.5f;           // Speed of sinus "lung" effect
    public float noiseScale = 0.5f;             // Perlin noise influence on radius
    public float positionalJitter = 0.75f;      // Random jitter offset per zombie

    [Header("Separation (Anti-Clump)")]
    public float separationRadius = 1.2f;       // Radius to start pushing zombies apart
    public float separationForce = 0.75f;       // Scaling factor for repulsion

    [Header("Safe Spawn")]
    public float navSampleRadius = 2f;          // Radius to search for valid navmesh
    public int navSampleMaxAttempts = 8;        // Attempts before fallback

    [Header("Lane / Corridor System")]
    public int laneSampleDirections = 12;          // Directions sampled around player
    public int maxLaneCount = 6;                   // Limit number of usable lanes
    public int minSubHordeSize = 2;                // Mini-hoard (lane capacity) min
    public int maxSubHordeSize = 10;               // Mini-hoard (lane capacity) max
    public int maxLaneCornersUsed = 3;             // Use only first N corners (variation)
    public float laneRecalcInterval = 5f;          // Rebuild lanes periodically
    public float minLaneSeparationAngle = 20f;     // Prevent very similar directions
    public float laneStartRadius = 10f;            // Where lane entry points begin
    public float laneCornerProximity = 0.6f;       // Filter very close consecutive corners

    [Header("Perception / LOD")]
    public float tacticalLODDistance = 25f;      // Zombies beyond skip ring update
    public LayerMask visionBlockMask;            // Walls etc.

    [Header("Flocking Weights")]
    public float flockAlignmentWeight = 0.4f;
    public float flockCohesionWeight = 0.35f;
    public float flockSeparationWeight = 0.8f;

    [Header("Reactive Surge")]
    public float surgeDuration = 2.5f;
    public float surgeSpeedMultiplier = 1.3f;
    public float surgeRadius = 12f;

    [Header("Spatial Hash")]
    public float cellSize = 2f;

    // Lista cu toți zombii activi din scenă
    public List<ZombieAI> activeZombies = new List<ZombieAI>();

    // Per-zombie angle noise cache to keep their slot feel persistent
    private Dictionary<ZombieAI, float> _angleNoise = new Dictionary<ZombieAI, float>();

    private float _nextWaveTime;
    private int _waveNumber = 1;

    private List<List<Vector3>> _lanePaths = new List<List<Vector3>>();
    private List<int> _laneCapacities = new List<int>();
    private Dictionary<ZombieAI,int> _laneAssignments = new Dictionary<ZombieAI,int>();
    private float _nextLaneRebuildTime;

    private Dictionary<Vector2Int, List<ZombieAI>> _spatial = new Dictionary<Vector2Int, List<ZombieAI>>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (player == null) player = FindObjectOfType<Player>();
        
        // Pornim logica de poziționare tactică
        StartCoroutine(UpdateTacticsRoutine());
        
        // Pornim primul val
        SpawnWave(initialCount);

        BuildLanes(); // initial
    }

    void Update()
    {
        // Verificăm dacă toți zombii au murit
        if (activeZombies.Count == 0 && Time.time > _nextWaveTime)
        {
            _waveNumber++;
            _nextWaveTime = Time.time + timeBetweenWaves;
            // Creștem dificultatea
            SpawnWave(initialCount + (_waveNumber * 2));
        }
        if (Time.time >= _nextLaneRebuildTime)
        {
            BuildLanes();
            _nextLaneRebuildTime = Time.time + laneRecalcInterval;
        }
    }

    void SpawnWave(int count)
    {
        Debug.Log($"[ZombieManager] Spawning Wave {_waveNumber} with {count} zombies.");
        if (_lanePaths.Count == 0) BuildLanes();
        for (int i = 0; i < count; i++)
        {
            // Random radial direction with varied distance
            Vector2 rndDir = Random.insideUnitCircle.normalized;
            float dist = spawnDistance * (0.75f + Random.value * 0.5f);
            Vector3 desired = player.transform.position + (Vector3)(rndDir * dist);

            Vector3 spawnPos = SafeSpawnPosition(desired);

            // Creăm zombiul
            GameObject newZ = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
            ZombieAI ai = newZ.GetComponent<ZombieAI>();

            if (ai != null)
            {
                activeZombies.Add(ai);
                // Cache persistent angle noise (small offset)
                _angleNoise[ai] = Random.Range(-10f, 10f) * Mathf.Deg2Rad;
                ai.InitializeVariance(); // Randomize movement characteristics
                AssignLane(ai);
            }
        }
    }

    // Safe NavMesh spawn sampling
    Vector3 SafeSpawnPosition(Vector3 desired)
    {
        NavMeshHit hit;
        // Try desired first with jitter attempts
        for (int attempt = 0; attempt < navSampleMaxAttempts; attempt++)
        {
            Vector3 probe = desired + (Vector3)Random.insideUnitCircle * navSampleRadius;
            if (NavMesh.SamplePosition(probe, out hit, navSampleRadius, NavMesh.AllAreas))
                return hit.position;
        }
        // Fallback: keep original
        if (NavMesh.SamplePosition(desired, out hit, navSampleRadius, NavMesh.AllAreas))
            return hit.position;
        return desired; // Last resort
    }

    public void NotifyGunshot(Vector3 pos)
    {
        // Speed pulse
        foreach (var z in activeZombies)
        {
            if (z == null) continue;
            if (Vector3.Distance(z.transform.position, pos) <= surgeRadius)
                z.ApplyTemporarySpeedBoost(surgeSpeedMultiplier, surgeDuration);
        }
    }

    void RebuildSpatial()
    {
        _spatial.Clear();
        for (int i = 0; i < activeZombies.Count; i++)
        {
            var z = activeZombies[i];
            if (z == null) continue;
            Vector3 p = z.transform.position;
            Vector2Int key = new Vector2Int(Mathf.FloorToInt(p.x / cellSize), Mathf.FloorToInt(p.y / cellSize));
            if (!_spatial.TryGetValue(key, out var list))
            {
                list = new List<ZombieAI>();
                _spatial[key] = list;
            }
            list.Add(z);
        }
    }

    bool HasLineOfSight(ZombieAI z)
    {
        if (player == null || z == null) return false;
        Vector3 dir = player.transform.position - z.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(z.transform.position, dir.normalized, dir.magnitude, visionBlockMask);
        return hit.collider == null;
    }

    // Corutina care rulează infinit și le spune zombilor unde să meargă
    IEnumerator UpdateTacticsRoutine()
    {
        while (true)
        {
            RebuildSpatial();
            if (player != null && activeZombies.Count > 0)
            {
                float angleStep = 360f / activeZombies.Count;
                float dynamicRadius = surroundRadius * (1f + Mathf.Sin(Time.time * breatheSpeed) * breatheAmplitude);
                dynamicRadius += (Mathf.PerlinNoise(Time.time * 0.2f, 0f) - 0.5f) * noiseScale;

                for (int i = 0; i < activeZombies.Count; i++)
                {
                    ZombieAI z = activeZombies[i];
                    if (z == null) continue;

                    // LOD skip
                    if (Vector3.Distance(z.transform.position, player.transform.position) > tacticalLODDistance) continue;

                    float angle = (i * angleStep) * Mathf.Deg2Rad;
                    if (_angleNoise.TryGetValue(z, out float noiseAng)) angle += noiseAng;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dynamicRadius;
                    offset += (Vector3)Random.insideUnitCircle * positionalJitter;
                    Vector3 targetPos = player.transform.position + offset;

                    // Local flocking (alignment/cohesion/separation)
                    Vector3 alignment = Vector3.zero;
                    Vector3 cohesion = Vector3.zero;
                    Vector3 separation = Vector3.zero;
                    int alignCount = 0, cohCount = 0;

                    Vector2Int cell = new Vector2Int(Mathf.FloorToInt(z.transform.position.x / cellSize), Mathf.FloorToInt(z.transform.position.y / cellSize));
                    for (int cx = -1; cx <= 1; cx++)
                        for (int cy = -1; cy <= 1; cy++)
                        {
                            Vector2Int k = new Vector2Int(cell.x + cx, cell.y + cy);
                            if (!_spatial.TryGetValue(k, out var list)) continue;
                            foreach (var other in list)
                            {
                                if (other == null || other == z) continue;
                                float d = Vector3.Distance(z.transform.position, other.transform.position);
                                if (d < 0.0001f) continue;
                                // Cohesion
                                cohesion += other.transform.position;
                                cohCount++;
                                // Alignment
                                alignment += other.CurrentVelocity;
                                alignCount++;
                                // Separation
                                if (d < separationRadius)
                                    separation += (z.transform.position - other.transform.position).normalized * (1f - d / separationRadius);
                            }
                        }

                    if (cohCount > 0) cohesion = (cohesion / cohCount - z.transform.position);
                    if (alignCount > 0) alignment /= alignCount;

                    Vector3 flock = alignment * flockAlignmentWeight +
                                    cohesion * flockCohesionWeight +
                                    separation * flockSeparationWeight;

                    targetPos += flock;

                    // LOS modifies aggression (closer radius if seen)
                    if (HasLineOfSight(z))
                        z.SetAggressionState(ZombieAI.Aggression.Frenzy);
                    else
                        z.SetAggressionState(ZombieAI.Aggression.Alert);

                    z.SetTacticalTarget(targetPos);
                }
            }
            yield return new WaitForSeconds(updateRate);
        }
    }

    void AssignLane(ZombieAI ai)
    {
        // Try find lane with free capacity
        List<int> candidateLanes = new List<int>();
        for (int i = 0; i < _lanePaths.Count; i++)
        {
            int used = 0;
            foreach (var kv in _laneAssignments)
                if (kv.Value == i) used++;
            if (used < _laneCapacities[i])
                candidateLanes.Add(i);
        }
        int laneIdx;
        if (candidateLanes.Count > 0)
            laneIdx = candidateLanes[Random.Range(0, candidateLanes.Count)];
        else
            laneIdx = Random.Range(0, _lanePaths.Count); // fallback

        _laneAssignments[ai] = laneIdx;
        ai.SetLanePath(_lanePaths[laneIdx]);
    }

    void BuildLanes()
    {
        _lanePaths.Clear();
        _laneCapacities.Clear();
        // Sample around player
        if (player == null) return;

        Vector3 center = player.transform.position;
        List<float> usedAngles = new List<float>();

        for (int i = 0; i < laneSampleDirections && _lanePaths.Count < maxLaneCount; i++)
        {
            float angleDeg = (360f / laneSampleDirections) * i + Random.Range(-8f, 8f);
            // Avoid similar angles
            bool tooClose = false;
            foreach (var a in usedAngles)
                if (Mathf.Abs(Mathf.DeltaAngle(a, angleDeg)) < minLaneSeparationAngle)
                { tooClose = true; break; }
            if (tooClose) continue;

            usedAngles.Add(angleDeg);
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Vector3 startPos = center + dir * laneStartRadius;

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(startPos, out hit, 3f, NavMesh.AllAreas))
                continue;

            // Build path from hit.position to player
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(hit.position, center, NavMesh.AllAreas, path))
                continue;
            if (path.corners == null || path.corners.Length < 2)
                continue;

            List<Vector3> laneNodes = new List<Vector3>();
            // Keep first few corners excluding final near player to diversify approach
            laneNodes.Add(hit.position);
            for (int c = 1; c < path.corners.Length && laneNodes.Count < maxLaneCornersUsed; c++)
            {
                if (Vector3.Distance(path.corners[c], center) < surroundRadius + 1f) break;
                // Filter proximity
                if (Vector3.Distance(laneNodes[laneNodes.Count - 1], path.corners[c]) < laneCornerProximity) continue;
                laneNodes.Add(path.corners[c]);
            }
            if (laneNodes.Count > 0)
            {
                _lanePaths.Add(laneNodes);
                _laneCapacities.Add(Random.Range(minSubHordeSize, maxSubHordeSize + 1));
            }
        }
        _nextLaneRebuildTime = Time.time + laneRecalcInterval;
    }

    // Funcție apelată de Zombie când moare
    public void UnregisterZombie(ZombieAI z)
    {
        if (activeZombies.Contains(z))
            activeZombies.Remove(z);
        if (_angleNoise.ContainsKey(z))
            _angleNoise.Remove(z);
        if (_laneAssignments.ContainsKey(z))
            _laneAssignments.Remove(z);
    }
}