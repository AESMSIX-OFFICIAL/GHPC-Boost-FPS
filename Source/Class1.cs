using System;
using UnityEngine;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine.Rendering.PostProcessing;

[assembly: MelonInfo(typeof(LowQualityMod.LowQualityModMain), "Ultra Low Graphic", "1.1.3", "AESMSIX")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace LowQualityMod
{
    public class LowQualityModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Loading Low Quality Mod...");
            ApplyAllLowQualitySettings();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                ApplyAllLowQualitySettings();
                CreateBeaconAboveTargets();
                MelonLogger.Msg("F9 pressed! Low quality settings re-applied.");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonLogger.Msg($"Scene loaded: {sceneName} (Index: {buildIndex})");
            ApplyAllLowQualitySettings();
        }

        private void ApplyAllLowQualitySettings()
        {
            ApplyLowQualitySettings();
            ConfigureMaterialsAndCameras();
            DeleteNonEssentialObjects();
        }

        private void ApplyLowQualitySettings()
        {
            QualitySettings.globalTextureMipmapLimit = 25;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.antiAliasing = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.lodBias = 0.01f;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            QualitySettings.shadowCascades = 0;

            ScalableBufferManager.ResizeBuffers(0.01f, 0.01f);
            RenderSettings.fog = false;
            RenderSettings.skybox = null;

            Time.timeScale = 1f;
            Time.maximumDeltaTime = 0.1f;

            MelonLogger.Msg("Low graphic settings applied.");
        }

        private void ConfigureMaterialsAndCameras()
        {
            foreach (Terrain terrain in GameObject.FindObjectsOfType<Terrain>())
            {
                terrain.detailObjectDensity = 1f;
                terrain.treeBillboardDistance = 5f;
                terrain.detailObjectDistance = 15f;
                terrain.heightmapPixelError = 55;

                if (terrain.materialTemplate != null && terrain.materialTemplate.HasProperty("_MainTex"))
                {
                    terrain.materialTemplate.SetTexture("_MainTex", null);
                }
            }

            foreach (Light light in GameObject.FindObjectsOfType<Light>())
            {
                if (light.shadows != LightShadows.None)
                    light.shadows = LightShadows.None;
            }

            foreach (Material mat in Resources.FindObjectsOfTypeAll<Material>())
            {
                mat.enableInstancing = true;
            }

            foreach (Camera camera in GameObject.FindObjectsOfType<Camera>())
            {
                if (camera == null) continue;

                var postLayer = camera.GetComponent<PostProcessLayer>();
                if (postLayer != null)
                    postLayer.enabled = false;

                var postVolume = camera.GetComponent<PostProcessVolume>();
                if (postVolume != null)
                    postVolume.enabled = false;
            }


            foreach (PostProcessVolume volume in GameObject.FindObjectsOfType<PostProcessVolume>())
            {
                volume.enabled = false;
            }

            MelonLogger.Msg("Materials and camera post-processing configured.");
        }

        private void DeleteNonEssentialObjects()
        {
            string[] keywords = { "dust", "smoke", "skybox", "cloud", "fog", "shadow", "grass" };
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int deletedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj == null || string.IsNullOrEmpty(obj.name))
                    continue;

                string lowerName = obj.name.ToLower();
                foreach (string keyword in keywords)
                {
                    if (lowerName.Contains(keyword))
                    {
                        try
                        {
                            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true))
                                r.enabled = false;

                            obj.SetActive(false);
                            deletedCount++;
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Warning($"Error disabling object '{obj.name}': {ex.Message}");
                        }

                        break; // No need to check other keywords
                    }
                }
            }

            MelonLogger.Msg($"Disabled {deletedCount} non-essential objects.");
        }
        private void CreateBeaconAboveTargets()
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int beaconCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj == null || string.IsNullOrEmpty(obj.name)) continue;

                string name = obj.name.ToLower();
                if (name.Contains("body"))
                {
                    Vector3 beaconPosition = obj.transform.position + new Vector3(0f, 2f, 0f); // 2 units above
                    GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere); // You can replace this with a custom prefab
                    beacon.name = "Beacon_" + obj.name;
                    beacon.transform.position = beaconPosition;
                    beacon.transform.localScale = new Vector3(0.3f, 50f, 0.3f); // Small beacon
                    beacon.GetComponent<Renderer>().material.color = Color.white;
                    beacon.transform.SetParent(obj.transform); // Follow the object

                    beaconCount++;
                }
            }

            MelonLogger.Msg($"Created {beaconCount} beacons above 'body' or 'tank' objects.");
        }

    }
}
