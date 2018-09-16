using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour {

    public Vector2 randomHeightRange = new Vector2(0f, 0.1f);
    public Terrain terrain;
    public TerrainData terrainData;

    public bool resetTerrain = true;

    // Load Texture
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);
    
    // Perlin Noise
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;
    
    // Multiple Perlin Noise
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

    // Voronoi
    public int peakCount = 9;
    public float fallOff = 0.2f;
    public float dropOff = 0.6f;
    public float minHeight = 0.1f;
    public float maxHeight = 0.5f;
    public enum VoroniType { Linear = 0, Power = 1, SinPow = 2, Combined = 3 }
    public VoroniType voronoiType = VoroniType.Linear;

    // Midpoint Displacement
    public float mdpMinHeight = -10.0f;
    public float mdpMaxHeight = 10.0f;
    public float roughness = 2.0f;
    public float heightDampenerPower = 2.0f;

    // Smooth
    public int smoothAmount = 1;

    // Splat Maps
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Vector2 tilOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float splatNoiseXScale = 0.01f;
        public float splatNoiseYscale = 0.01f;
        public float splatNoiseScale = 0.1f;
        public float splatNoiseOffset = 0.01f;
        public bool remove = false;
    }

    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    // Vegetation
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public float minScale = 0.5f;
        public float maxScale = 1.0f;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightColour1 = Color.white;
        public Color lightColour2 = Color.white;
        public float minRotation = 0;
        public float maxRotation = 360;
        public float density = 0.5f;
        public bool remove = false;
    }

    public List<Vegetation> vegetation = new List<Vegetation>()
    {
        new Vegetation()
    };

    public int maxTrees = 5000;
    public int treeSpacing = 5;
    public enum TagType { Tag = 0, Layer = 1}
    [SerializeField]
    int terrainLayer = -1;

    // Detail
    [System.Serializable]
    public class Detail
    {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1;
        public Color dryColour = Color.white;
        public Color healthyColour = Color.white;
        public Vector2 heightRange = new Vector2(1, 1);
        public Vector2 widthRange = new Vector2(1, 1);
        public float noiseSpread = 0.5f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public bool remove = false;
    }

    public List<Detail> details = new List<Detail>()
    {
        new Detail()
    };

    public int maxDetails = 5000;
    public int detailSpacing = 5;

    // Water
    public float waterHeight = 0.5f;
    public GameObject waterGO = null;
    public Material shoreLineMaterial = null;

    // Erosion
    public enum ErosionType { Rain = 0, Thermal = 1, Tidal = 2,
                                River = 3, Wind = 4, Canyon = 5 }
    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public float erosionAmount = 0.01f;
    public int springsPerRiver = 5;
    public float solubility = 0.01f;
    public int droplets = 10;
    public int erosionSmoothAmount = 5;

    // Clouds
    public int numClouds = 1;
    public int particlesPerCloud = 50;
    public int cloudStartSize = 5;
    public Vector3 cloudScaleMin = new Vector3(1, 1, 1);
    public Vector3 cloudScaleMax = new Vector3(1, 1, 1);
    public Material cloudMaterial = null;
    public Material cloudShadowMaterial = null;
    public Color cloudColour = Color.white;
    public Color cloudLining = Color.gray;
    public float cloudMinSpeed = 0.2f;
    public float cloudMaxSpeed = 0.5f;
    public int cloudRange = 500;

    void NormalizeVector(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                terrainData.heightmapHeight);
        } else
        {
            return new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        }
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                          (int)(y * heightMapScale.z)).grayscale
                                                            * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                //heightMap[x, y] = Mathf.PerlinNoise((x + perlinOffsetX) * perlinXScale, 
                //                                    (y + perlinOffsetY) * perlinYScale);
                heightMap[x, y] += Utils.fBM((x+perlinOffsetX) * perlinXScale, 
                                            (y+perlinOffsetY) * perlinYScale, 
                                            perlinOctaves, 
                                            perlinPersistance) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM((x + p.mPerlinOffsetX) * p.mPerlinXScale,
                                                (y + p.mPerlinOffsetY) * p.mPerlinYScale,
                                                p.mPerlinOctaves,
                                                p.mPerlinPersistance) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if (keptPerlinParameters.Count == 0)       // don't want to keep any
        {
            keptPerlinParameters.Add(new PerlinParameters());   // add at least 1
        }
        perlinParameters = keptPerlinParameters;
    }

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();

        Vector3[] peaks = new Vector3[peakCount];

        for (int i = 0; i < peakCount; i++)
        {
            peaks[i] = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                       UnityEngine.Random.Range(minHeight, maxHeight),
                                       UnityEngine.Random.Range(0, terrainData.heightmapHeight));
        }

        foreach (Vector3 peak in peaks)
        {
            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;   // Peak is not higher than existing height

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth,
                                                                               terrainData.heightmapHeight));
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                for (int y = 0; y < terrainData.heightmapHeight; y++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h;
                        if (voronoiType == VoroniType.Combined)
                        {
                            h = peak.y - distanceToPeak * fallOff -
                                Mathf.Pow(distanceToPeak, dropOff); //Combined
                        } else if (voronoiType == VoroniType.Power)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak, dropOff) * fallOff; //Power
                        }
                        else if (voronoiType == VoroniType.SinPow)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, fallOff) -
                                    Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / dropOff; //SinPow
                        }
                        else
                        {
                            h = peak.y - distanceToPeak * fallOff;  //Linear
                        }

                        if (h > heightMap[x, y])
                        {
                            heightMap[x, y] = h;
                        }
                    }
                }
            }

        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                               Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos))
                        neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }

    public void MidpointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        float heightMin = mdpMinHeight;
        float heightMax = mdpMaxHeight;
        float heightDampener = (float)Mathf.Pow(heightDampenerPower, -1 * roughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /*heightMap[0, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[0, terrainData.heightmapHeight - 2] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapWidth - 2, 0] = UnityEngine.Random.Range(0f, 0.2f);
        heightMap[terrainData.heightmapWidth - 2, terrainData.heightmapHeight - 2] =
                                                       UnityEngine.Random.Range(0f, 0.2f);*/

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[cornerX, cornerY]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));

                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0
                        || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    heightMap[midX, y] = (float)((heightMap[x, y] +
                                                     heightMap[midX, midY] +
                                                     heightMap[cornerX, y] +
                                                     heightMap[midX, pmidYD]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));

                    heightMap[x, midY] = (float)((heightMap[pmidXL, midY] +
                                                     heightMap[x, cornerY] +
                                                     heightMap[midX, midY] +
                                                     heightMap[x, y]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));

                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] +
                                                     heightMap[midX, pmidYU] +
                                                     heightMap[cornerX, cornerY] +
                                                     heightMap[midX, midY]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));

                    heightMap[cornerX, midY] = (float)((heightMap[midX, midY] +
                                                     heightMap[cornerX, cornerY] +
                                                     heightMap[pmidXR, midY] +
                                                     heightMap[cornerX, y]) / 4.0f +
                                                     UnityEngine.Random.Range(heightMin, heightMax));

                }
            }
            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                         "Progress",
                                         smoothProgress / smoothAmount);

        for (int t = 0; t < smoothAmount; t++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                for (int y = 0; y < terrainData.heightmapHeight; y++)
                {
                    float avgHeight = heightMap[x, y];

                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y),
                                                                  terrainData.heightmapWidth,
                                                                  terrainData.heightmapHeight);
                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                 "Progress",
                                 smoothProgress / smoothAmount);
        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }
        if (keptSplatHeights.Count == 0)
        {
            keptSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keptSplatHeights;
    }

    float GetSteepness(float[,] heightmap, int x, int y, int width, int height)
    {
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        // If on the upper edge of the map find gradient by going backward.
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }

    public void SplatMaps()
    {
        SplatPrototype[] newSplatPrototypes;
        newSplatPrototypes = new SplatPrototype[splatHeights.Count];
        int spindex = 0;
        foreach (SplatHeights sh in splatHeights)
        {
            newSplatPrototypes[spindex] = new SplatPrototype();
            newSplatPrototypes[spindex].texture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tilOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].texture.Apply(true);
            spindex++;
        }
        terrainData.splatPrototypes = newSplatPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                          terrainData.heightmapHeight);
        float[,,] splatmapData = new float[terrainData.alphamapWidth,
                                            terrainData.alphamapHeight,
                                            terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * splatHeights[i].splatNoiseXScale, 
                                                    y * splatHeights[i].splatNoiseYscale) 
                                                   * splatHeights[i].splatNoiseScale;
                    float offset = splatHeights[i].splatNoiseOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;

                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight,
                                            x / (float)terrainData.alphamapWidth);

                    if ((heightMap[x,y] >= thisHeightStart && heightMap[x,y] <= thisHeightStop) &&
                        (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetation = new List<Vegetation>();
        for (int i = 0; i < vegetation.Count; i++)
        {
            if (!vegetation[i].remove)
            {
                keptVegetation.Add(vegetation[i]);
            }
        }
        if (keptVegetation.Count == 0)
        {
            keptVegetation.Add(vegetation[0]);
        }
        vegetation = keptVegetation;
    }

    public void PlantVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetation.Count];
        int tindex = 0;
        foreach (Vegetation t in vegetation)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetation[tp].density) break;

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightEnd = vegetation[tp].maxHeight;

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x,
                                                               z / (float)terrainData.size.z);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) &&
                        (steepness >= vegetation[tp].minSlope && steepness <= vegetation[tp].maxSlope))
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x,
                                                        thisHeight,
                                                        (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,
                            instance.position.y * terrainData.size.y,
                            instance.position.z * terrainData.size.z)
                                                        + this.transform.position;

                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;
                        if (Physics.Raycast(treeWorldPos, -Vector3.up, out hit, 100, layerMask) ||
                            Physics.Raycast(treeWorldPos, Vector3.up, out hit, 100, layerMask))
                        {
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x,
                                                            treeHeight,
                                                            instance.position.z);

                            instance.rotation = UnityEngine.Random.Range(vegetation[tp].minRotation,
                                                                         vegetation[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetation[tp].colour1,
                                                        vegetation[tp].colour2,
                                                        UnityEngine.Random.Range(0.0f, 1.0f));

                            instance.lightmapColor = Color.Lerp(vegetation[tp].lightColour1,
                                                        vegetation[tp].lightColour2,
                                                        UnityEngine.Random.Range(0.0f, 1.0f));

                            float s = UnityEngine.Random.Range(vegetation[tp].minScale, vegetation[tp].maxScale);
                            instance.heightScale = s;
                            instance.widthScale = s;

                            allVegetation.Add(instance);
                            if (allVegetation.Count >= maxTrees) goto TREESDONE;
                        }
                    }
                }
            }
        }
        TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();
    }

    public void AddNewDetail()
    {
        details.Add(new Detail());
    }

    public void RemoveDetails()
    {
        List<Detail> keptDetails = new List<Detail>();
        for (int i = 0; i < details.Count; i++)
        {
            if (!details[i].remove)
            {
                keptDetails.Add(details[i]);
            }
        }
        if (keptDetails.Count == 0)
        {
            keptDetails.Add(details[0]);
        }
        details = keptDetails;
    }

    public void PlaceDetails()
    {
        DetailPrototype[] newDetailPrototypes;
        newDetailPrototypes = new DetailPrototype[details.Count];
        int dindex = 0;
        foreach (Detail d in details)
        {
            newDetailPrototypes[dindex] = new DetailPrototype();
            newDetailPrototypes[dindex].prototype = d.prototype;
            newDetailPrototypes[dindex].prototypeTexture = d.prototypeTexture;
            newDetailPrototypes[dindex].healthyColor = d.healthyColour;
            newDetailPrototypes[dindex].dryColor = d.dryColour;
            newDetailPrototypes[dindex].minHeight = d.heightRange.x;
            newDetailPrototypes[dindex].maxHeight = d.heightRange.y;
            newDetailPrototypes[dindex].minWidth = d.widthRange.x;
            newDetailPrototypes[dindex].maxWidth = d.widthRange.y;
            newDetailPrototypes[dindex].noiseSpread = d.noiseSpread;
            if (newDetailPrototypes[dindex].prototype)
            {
                newDetailPrototypes[dindex].usePrototypeMesh = true;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.VertexLit;
            }
            else
            {
                newDetailPrototypes[dindex].usePrototypeMesh = false;
                newDetailPrototypes[dindex].renderMode = DetailRenderMode.GrassBillboard;
            }
            dindex++;
        }
        terrainData.detailPrototypes = newDetailPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0,
                                                    terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);

        for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
        {
            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];

            for (int y = 0; y < terrainData.detailHeight; y += detailSpacing)
            {
                for (int x = 0; x < terrainData.detailWidth; x += detailSpacing)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density) continue;
                    int xHM = (int)(x / (float)terrainData.detailWidth * terrainData.heightmapWidth);
                    int yHM = (int)(y / (float)terrainData.detailHeight * terrainData.heightmapHeight);

                    float thisNoise = Utils.Map(Mathf.PerlinNoise(x * details[i].feather,
                                                                  y * details[i].feather), 0, 1, 0.5f, 1);
                    float thisHeightStart = details[i].minHeight * thisNoise -
                                            details[i].overlap * thisNoise;

                    float thisHeightEnd = details[i].maxHeight * thisNoise +
                                            details[i].overlap * thisNoise;

                    float thisHeight = heightMap[yHM, xHM];
                    float steepness = terrainData.GetSteepness(xHM / (float)terrainData.size.x,
                                                               yHM / (float)terrainData.size.z);
                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) &&
                        (steepness >= details[i].minSlope && steepness <= details[i].maxSlope))
                    {
                        detailMap[y, x] = 1;
                    }
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
    }

    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] = 0f;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddWater()
    {
        GameObject water = GameObject.Find("water");
        if (!water)
        {
            water = Instantiate(waterGO, this.transform.position, this.transform.rotation);
            water.name = "water";
        }
        water.transform.position = this.transform.position + new Vector3(terrainData.size.x / 2,
                                                                         waterHeight * terrainData.size.y,
                                                                         terrainData.size.z / 2);
        water.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
    }

    public void DrawShoreline()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        int quadCount = 0;
        // GameObject quads = new GameObject("QUADS");

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                // Find spot on shore
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation,
                                                              terrainData.heightmapWidth,
                                                              terrainData.heightmapHeight);
                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x,y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        // if (quadCount < 1000)
                        // {
                        quadCount++;
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        go.transform.localScale *= 20;

                        go.transform.position = this.transform.position +
                                        new Vector3(y / (float)terrainData.heightmapHeight
                                                      * terrainData.size.z,
                                                    waterHeight * terrainData.size.y,
                                                    x / (float)terrainData.heightmapWidth
                                                      * terrainData.size.x);

                        go.transform.LookAt(new Vector3(n.y / (float)terrainData.heightmapHeight
                                                            * terrainData.size.z,
                                                            waterHeight * terrainData.size.y,
                                                            n.x / (float)terrainData.heightmapWidth
                                                                * terrainData.size.x));

                        go.transform.Rotate(90, 0, 0);
                        go.tag = "Shore";
                        // go.transform.parent = quads.transform;

                        // }
                    }
                }
            }
        }

        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
        MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];
        for (int m = 0; m < shoreQuads.Length; m++)
        {
            meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while ( i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        GameObject currentShoreLine = GameObject.Find("ShoreLine");
        if (currentShoreLine)
        {
            DestroyImmediate(currentShoreLine);
        }
        GameObject shoreLine = new GameObject();
        shoreLine.name = "ShoreLine";
        shoreLine.AddComponent<WaveAnimation>();
        shoreLine.transform.position = this.transform.position;
        shoreLine.transform.rotation = this.transform.rotation;
        MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
        thisMF.mesh = new Mesh();
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();
        r.sharedMaterial = shoreLineMaterial;

        for (int sQ = 0; sQ < shoreQuads.Length; sQ++)
            DestroyImmediate(shoreQuads[sQ]);

    }

    public void Erode()
    {
        if (erosionType == ErosionType.Rain)
            Rain();
        else if (erosionType == ErosionType.Tidal)
            Tidal();
        else if (erosionType == ErosionType.Thermal)
            Thermal();
        else if (erosionType == ErosionType.River)
            River();
        else if (erosionType == ErosionType.Wind)
            Wind();
        else if (erosionType == ErosionType.Canyon)
            DigCanyon();

        smoothAmount = erosionSmoothAmount;
        if (smoothAmount > 0)
            Smooth();

    }

    void Rain()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);

        for (int i = 0; i < droplets; i++)
        {
            heightMap[UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                      UnityEngine.Random.Range(0, terrainData.heightmapHeight)]
                        -= erosionStrength;
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void Tidal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
                                            terrainData.heightmapWidth,
                                            terrainData.heightmapHeight);

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation,
                                                              terrainData.heightmapWidth,
                                                              terrainData.heightmapHeight);
                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        heightMap[x, y] = waterHeight;
                        heightMap[(int)n.x, (int)n.y] = waterHeight;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    void Thermal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, 
                            terrainData.heightmapWidth,
                            terrainData.heightmapHeight);

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation,
                                                              terrainData.heightmapWidth,
                                                              terrainData.heightmapHeight);
                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)
                    {
                        float currentHeight = heightMap[x, y];
                        heightMap[x, y] -= currentHeight * erosionAmount;
                        heightMap[(int)n.x, (int)n.y] += currentHeight * erosionAmount;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

void River()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
                                            terrainData.heightmapWidth,
                                            terrainData.heightmapHeight);
        float[,] erosionMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int i = 0; i < droplets; i++)
        {
            Vector2 dropletPosition = new Vector2(UnityEngine.Random.Range(0, terrainData.heightmapWidth),
                                                  UnityEngine.Random.Range(0, terrainData.heightmapHeight));
            erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] = erosionStrength;
            for (int j = 0; j < springsPerRiver; j++)
            {
                erosionMap = RunRiver(dropletPosition, heightMap, erosionMap,
                                    terrainData.heightmapWidth,
                                    terrainData.heightmapHeight);
            }
        }

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                if (erosionMap[x,y] > 0)
                {
                    heightMap[x, y] -= erosionMap[x, y];
                }
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    float[,] RunRiver(Vector3 dropletPosition, float[,] heightMap, float[,] erosionMap, int width, int height)
    {
        while (erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] > 0)
        {
            List<Vector2> neighbours = GenerateNeighbours(dropletPosition, width, height);
            neighbours.Shuffle();
            bool foundLower = false;
            foreach (Vector2 n in neighbours)
            {
                if (heightMap[(int)n.x, (int)n.y] < heightMap[(int)dropletPosition.x, (int)dropletPosition.y])
                {
                    erosionMap[(int)n.x, (int)n.y] = erosionMap[(int)dropletPosition.x, 
                                                                (int)dropletPosition.y] - solubility;
                    dropletPosition = n;
                    foundLower = true;
                    break;
                }
            }
            if (!foundLower)
            {
                erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] -= solubility;
            }
        }
        return erosionMap;
    }

    void Wind()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
                                            terrainData.heightmapWidth,
                                            terrainData.heightmapHeight);
        int width = terrainData.heightmapWidth;
        int height = terrainData.heightmapHeight;

        float WindDir = 30;
        float sinAngle = -Mathf.Sin(Mathf.Deg2Rad * WindDir);
        float cosAngle = Mathf.Cos(Mathf.Cos(Mathf.Deg2Rad * WindDir));

        for (int y = -(height - 1)*2; y <= height*2; y += 10)
        {
            for (int x = -(width - 1)*2; x <= width*2; x += 1)
            {
                float thisNoise = (float)Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 20 * erosionStrength;
                int nx = (int)x;
                int digy = (int)y + (int)thisNoise;
                int ny = (int)y + 5 + (int)thisNoise;

                Vector2 digCoords = new Vector2(x * cosAngle - digy * sinAngle, digy * cosAngle + x * sinAngle);
                Vector2 pileCoords = new Vector2(nx * cosAngle - ny * sinAngle, ny * cosAngle + nx * sinAngle);

                if (!(pileCoords.x < 0 || pileCoords.x > (width - 1) || pileCoords.y < 0 || 
                      pileCoords.y > (height - 1) ||
                     (int)digCoords.x < 0 || (int)digCoords.x > (width - 1) ||
                     (int)digCoords.y < 0 || (int)digCoords.y > (height - 1)))
                {
                    heightMap[(int)digCoords.x, (int)digCoords.y] -= 0.001f;
                    heightMap[(int)pileCoords.x, (int)pileCoords.y] += 0.001f;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    float[,] tempHeightMap;
    public void DigCanyon()
    {
        float digDepth = 0.05f;
        float bankSlope = 0.001f;
        float maxDepth = 0;
        tempHeightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                terrainData.heightmapHeight);
        int cx = 1;
        int cy = UnityEngine.Random.Range(10, terrainData.heightmapHeight - 10);
        while (cy >= 0 && cy < terrainData.heightmapHeight && cx > 0
               && cx < terrainData.heightmapWidth)
        {
            CanyonCrawler(cx, cy, tempHeightMap[cx, cy] - digDepth, bankSlope, maxDepth);
            cx = cx + UnityEngine.Random.Range(1, 3);
            cy = cy + UnityEngine.Random.Range(-2, 3);
        }
        terrainData.SetHeights(0, 0, tempHeightMap);
    }

    void CanyonCrawler(int x, int y, float height, float slope, float maxDepth)
    {
        if (x < 0 || x >= terrainData.heightmapWidth) return;   // Off x range of map
        if (y < 0 || y >= terrainData.heightmapHeight) return;  // Off y range of map
        if (height <= maxDepth) return;                         // Hit lowest level
        if (tempHeightMap[x, y] <= height) return;              // Run into lower elevation

        tempHeightMap[x, y] = height;

        CanyonCrawler(x + 1, y, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x - 1, y, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x + 1, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x - 1, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x, y - 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
        CanyonCrawler(x, y + 1, height + UnityEngine.Random.Range(slope, slope + 0.01f), slope, maxDepth);
    }

    public void GenerateClouds()
    {
        GameObject cloudManager = GameObject.Find("CloudManager");
        if (!cloudManager)
        {
            cloudManager = new GameObject();
            cloudManager.name = "CloudManager";
            cloudManager.AddComponent<CloudManager>();
            cloudManager.transform.position = this.transform.position;
        }

        GameObject[] allClouds = GameObject.FindGameObjectsWithTag("Cloud");
        for (int i = 0; i < allClouds.Length; i++)
        {
            DestroyImmediate(allClouds[i]);
        }

        for (int c = 0; c < numClouds; c++)
        {
            GameObject cloudGO = new GameObject();
            cloudGO.name = "Cloud" + c;
            cloudGO.tag = "Cloud";

            cloudGO.transform.rotation = cloudManager.transform.rotation;
            cloudGO.transform.position = cloudManager.transform.position;

            CloudController cc = cloudGO.AddComponent<CloudController>();
            cc.lining = cloudLining;
            cc.colour = cloudColour;
            cc.numberOfParticles = particlesPerCloud;
            cc.minSpeed = cloudMinSpeed;
            cc.maxSpeed = cloudMaxSpeed;
            cc.distance = cloudRange;

            ParticleSystem cloudSystem = cloudGO.AddComponent<ParticleSystem>();
            Renderer cloudRend = cloudGO.GetComponent<Renderer>();
            cloudRend.material = cloudMaterial;

            cloudGO.layer = LayerMask.NameToLayer("Sky");
            GameObject cloudProjector = new GameObject();
            cloudProjector.name = "Shadow";
            cloudProjector.transform.position = cloudGO.transform.position;
            cloudProjector.transform.forward = Vector3.down;
            cloudProjector.transform.parent = cloudGO.transform;

            if (UnityEngine.Random.Range(0, 10) < 5)
            {
                Projector cp = cloudProjector.AddComponent<Projector>();
                cp.material = cloudShadowMaterial;
                cp.farClipPlane = terrainData.size.y;
                int skyLayerMask = 1 << LayerMask.NameToLayer("Sky");
                int waterLayerMask = 1 << LayerMask.NameToLayer("Water");
                cp.ignoreLayers = skyLayerMask | waterLayerMask;
                cp.fieldOfView = 20.0f;
            }

            cloudRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            cloudRend.receiveShadows = false;
            ParticleSystem.MainModule main = cloudSystem.main;
            main.loop = false;
            main.startLifetime = Mathf.Infinity;
            main.startSpeed = 0;
            main.startSize = cloudStartSize;
            main.startColor = Color.white;

            var emission = cloudSystem.emission;
            emission.rateOverTime = 0;    // All at once
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, (short)particlesPerCloud) });

            var shape = cloudSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            Vector3 newScale = new Vector3(UnityEngine.Random.Range(cloudScaleMin.x, cloudScaleMax.x),
                                           UnityEngine.Random.Range(cloudScaleMin.y, cloudScaleMax.y),
                                           UnityEngine.Random.Range(cloudScaleMin.z, cloudScaleMax.z));
            shape.scale = newScale;

            cloudGO.transform.parent = cloudManager.transform;
            cloudGO.transform.localScale = new Vector3(1, 1, 1);

        }
    }

    private void OnEnable()
    {
        Debug.Log("Initializing Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    private void Awake()
    {
        SerializedObject tagManager = new SerializedObject(
                    AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/tagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);
        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        AddTag(layerProp, "Sky", TagType.Layer);
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        // take this object
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }

    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;
        // ensure the tag doen't already exist

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; return i; }
        }
        // add your new tag
        if (!found && tType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        // Add new layer
        else if(!found && tType == TagType.Layer)
        {
            for (int j = 8; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }
        return -1;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
