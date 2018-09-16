using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor {

    // properties
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapImage;
    SerializedProperty heightMapScale;
    SerializedProperty resetTerrain;

    // Perlin
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    // Voronoi
    SerializedProperty peakCount;
    SerializedProperty fallOff;
    SerializedProperty dropOff;
    SerializedProperty minHeight;
    SerializedProperty maxHeight;
    SerializedProperty voronoiType;

    // Midpoint Displacement
    SerializedProperty mdpMinHeight;
    SerializedProperty mdpMaxHeight;
    SerializedProperty roughness;
    SerializedProperty heightDampenerPower;

    // Smooth
    SerializedProperty smoothAmount;

    // Splat Maps
    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    // Vegetation
    GUITableState vegetationTable;
    SerializedProperty vegetation;
    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;

    // Detail
    GUITableState detailsTable;
    SerializedProperty details;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;

    // Water
    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    SerializedProperty shoreLineMaterial;

    // Erosion
    SerializedProperty erosionType;
    SerializedProperty erosionStrength;
    SerializedProperty erosionAmount;
    SerializedProperty springsPerRiver;
    SerializedProperty solubility;
    SerializedProperty droplets;
    SerializedProperty erosionSmoothAmount;

    // Clouds
    SerializedProperty numClouds;
    SerializedProperty particlesPerCloud;
    SerializedProperty cloudStartSize;
    SerializedProperty cloudScaleMin;
    SerializedProperty cloudScaleMax;
    SerializedProperty cloudMaterial;
    SerializedProperty cloudShadowMaterial;
    SerializedProperty cloudColour;
    SerializedProperty cloudLining;
    SerializedProperty cloudMinSpeed;
    SerializedProperty cloudMaxSpeed;
    SerializedProperty cloudRange;

    // fold outs
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMPD = false;
    bool showSmooth = false;
    bool showSplatMaps = false;
    bool showHeightMap = false;
    bool showVegetation = false;
    bool showDetail = false;
    bool showWater = false;
    bool showErosion = false;
    bool showClouds = false;

    Texture2D hmTexture;

    private void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");
        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        peakCount = serializedObject.FindProperty("peakCount");
        fallOff = serializedObject.FindProperty("fallOff");
        dropOff = serializedObject.FindProperty("dropOff");
        minHeight = serializedObject.FindProperty("minHeight");
        maxHeight = serializedObject.FindProperty("maxHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");
        mdpMinHeight = serializedObject.FindProperty("mdpMinHeight");
        mdpMaxHeight = serializedObject.FindProperty("mdpMaxHeight");
        roughness = serializedObject.FindProperty("roughness");
        heightDampenerPower = serializedObject.FindProperty("heightDampenerPower");
        smoothAmount = serializedObject.FindProperty("smoothAmount");
        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        vegetationTable = new GUITableState("vegetationTable");
        vegetation = serializedObject.FindProperty("vegetation");
        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        detailsTable = new GUITableState("detailsTable");
        details = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        shoreLineMaterial = serializedObject.FindProperty("shoreLineMaterial");
        erosionType = serializedObject.FindProperty("erosionType");
        erosionStrength = serializedObject.FindProperty("erosionStrength");
        erosionAmount = serializedObject.FindProperty("erosionAmount");
        springsPerRiver = serializedObject.FindProperty("springsPerRiver");
        solubility = serializedObject.FindProperty("solubility");
        droplets = serializedObject.FindProperty("droplets");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");
        numClouds = serializedObject.FindProperty("numClouds");
        particlesPerCloud = serializedObject.FindProperty("particlesPerCloud");
        cloudStartSize = serializedObject.FindProperty("cloudStartSize");
        cloudScaleMin = serializedObject.FindProperty("cloudScaleMin");
        cloudScaleMax = serializedObject.FindProperty("cloudScaleMax");
        cloudMaterial = serializedObject.FindProperty("cloudMaterial");
        cloudShadowMaterial = serializedObject.FindProperty("cloudShadowMaterial");
        cloudColour = serializedObject.FindProperty("cloudColour");
        cloudLining = serializedObject.FindProperty("cloudLining");
        cloudMinSpeed = serializedObject.FindProperty("cloudMinSpeed");
        cloudMaxSpeed = serializedObject.FindProperty("cloudMaxSpeed");
        cloudRange = serializedObject.FindProperty("cloudRange");

        hmTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        EditorGUILayout.PropertyField(resetTerrain);

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        showPerlin = EditorGUILayout.Foldout(showPerlin, "Single Perlin Noise");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin Noise Heights", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0f, 0.1f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0f, 0.1f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y Offset"));
            // Fractal Brownian Motion additions
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            if (GUILayout.Button("Perlin Noise"))
            {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin Noise");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, 
                                                            perlinParameters);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }

        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.IntSlider(peakCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(fallOff, 0f, 10f, new GUIContent("Falloff"));
            EditorGUILayout.Slider(dropOff, 0f, 10f, new GUIContent("Dropoff"));
            EditorGUILayout.Slider(minHeight, 0f, 1f, new GUIContent("Min Height"));
            EditorGUILayout.Slider(maxHeight, 0f, 1f, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);

            if (GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }
        }

        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(mdpMinHeight, new GUIContent("Min Height"));
            EditorGUILayout.PropertyField(mdpMaxHeight, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(roughness, new GUIContent("Roughness"));
            EditorGUILayout.PropertyField(heightDampenerPower, new GUIContent("Height Damp. Pow"));

            if (GUILayout.Button("MPD"))
            {
                terrain.MidpointDisplacement();
            }
        }

        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
            splatMapTable = GUITableLayout.DrawTable(splatMapTable,
                                        serializedObject.FindProperty("splatHeights"));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply SplatMaps"))
            {
                terrain.SplatMaps();
            }
        }

        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(maxTrees, 100, 10000, new GUIContent("Max Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 1, 100, new GUIContent("Tree Spacing"));
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);
            vegetationTable = GUITableLayout.DrawTable(vegetationTable,
                                        serializedObject.FindProperty("vegetation"));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewVegetation();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Vegetation"))
            {
                terrain.PlantVegetation();
            }
        }

        showDetail = EditorGUILayout.Foldout(showDetail, "Detail");
        if (showDetail)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(maxDetails, 100, 10000, new GUIContent("Max Distance"));
            EditorGUILayout.IntSlider(detailSpacing, 1, 100, new GUIContent("Detail Spacing"));
            GUILayout.Label("Details", EditorStyles.boldLabel);
            detailsTable = GUITableLayout.DrawTable(detailsTable,
                                        serializedObject.FindProperty("details"));

            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewDetail();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveDetails();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Details"))
            {
                terrain.PlaceDetails();
            }
        }

        showWater = EditorGUILayout.Foldout(showWater, "Water");
        if (showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.Slider(waterHeight, 0f, 1.0f, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO, new GUIContent("Water GO"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Water"))
            {
                terrain.AddWater();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(shoreLineMaterial, new GUIContent("Shore Line Material"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Shoreline"))
            {
                terrain.DrawShoreline();
            }
            EditorGUILayout.EndHorizontal();
        }

        showErosion = EditorGUILayout.Foldout(showErosion, "Erosion");
        if (showErosion)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(erosionType);
            EditorGUILayout.Slider(erosionStrength, 0f, 1.0f, new GUIContent("Erosion Strength"));
            EditorGUILayout.Slider(erosionAmount, 0f, 0.5f, new GUIContent("Erosion Amount"));
            EditorGUILayout.IntSlider(droplets, 1, 500, new GUIContent("Droplets"));
            EditorGUILayout.Slider(solubility, 0.001f, 1.0f, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Springs Per River"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));

            if (GUILayout.Button("Erode"))
            {
                terrain.Erode();
            }
        }

        showClouds = EditorGUILayout.Foldout(showClouds, "Clouds");
        if (showClouds)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(numClouds, new GUIContent("Number of Clouds"));
            EditorGUILayout.PropertyField(particlesPerCloud, new GUIContent("Particles Per Cloud"));
            EditorGUILayout.PropertyField(cloudStartSize, new GUIContent("Cloud Start Size"));
            EditorGUILayout.PropertyField(cloudScaleMin, new GUIContent("Scale Min"));
            EditorGUILayout.PropertyField(cloudScaleMax, new GUIContent("Scale Max"));
            EditorGUILayout.PropertyField(cloudMaterial, new GUIContent("Cloud Material"));
            EditorGUILayout.PropertyField(cloudShadowMaterial, new GUIContent("Cloud Shadow Material"));
            EditorGUILayout.PropertyField(cloudColour, new GUIContent("Colour"));
            EditorGUILayout.PropertyField(cloudLining, new GUIContent("Lining"));
            EditorGUILayout.PropertyField(cloudMinSpeed, new GUIContent("Min Speed"));
            EditorGUILayout.PropertyField(cloudMaxSpeed, new GUIContent("Max Speed"));
            EditorGUILayout.PropertyField(cloudRange, new GUIContent("Distance Travelled"));

            if (GUILayout.Button("Generate Clouds"))
            {
                terrain.GenerateClouds();
            }
        }

        showSmooth = EditorGUILayout.Foldout(showSmooth, "Smooth Terrain");
        if (showSmooth)
        {
            EditorGUILayout.IntSlider(peakCount, 1, 10, new GUIContent("Smooth Aamount"));
            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        showHeightMap = EditorGUILayout.Foldout(showHeightMap, "Height Map");
        if (showHeightMap)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0,
                                                                    terrain.terrainData.heightmapWidth,
                                                                    terrain.terrainData.heightmapHeight);
                for (int y = 0; y < terrain.terrainData.heightmapHeight; y++)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapWidth; x++)
                    {
                        hmTexture.SetPixel(x, y, new Color(heightMap[x, y],
                                                          heightMap[x, y],
                                                          heightMap[x, y], 1));
                    }
                }
                hmTexture.Apply();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
