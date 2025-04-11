using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[assembly: MelonInfo(typeof(LowQualityMod.LowQualityModMain), "Ultra Low Graphic", "1.1.3", "AESMSIX")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace LowQualityMod
{
    public class LowQualityModMain : MelonMod
    {
        public override void OnInitializeMelon()
        {
            ApplyLowQualitySettings();
            ConfigureMaterials();
            MelonLogger.Msg("Low Quality Mod loaded successfully. Setting graphics settings and removing non-essential objects...");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                ApplyLowQualitySettings();
                ConfigureMaterials();
                DeleteNonEssentialObjectsDuringGameplay();
                MelonLogger.Msg("F9 pressed! Re-running graphics settings and removing environmental objects...");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            ApplyLowQualitySettings();
            MelonLogger.Msg($"New scene loaded: {sceneName} (Index: {buildIndex}). Applying low quality settings.");
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

            MelonLogger.Msg("Graphics quality settings have been applied.");
        }

        private void ConfigureMaterials()
        {
            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
            foreach (Terrain terrain in terrains)
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

            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light != null && light.shadows != LightShadows.None)
                    light.shadows = LightShadows.None;
            }

            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    mat.enableInstancing = true;
                }
            }

            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in cameras)
            {
                if (camera == null) continue;

                var postProcessingLayer = camera.GetComponent<PostProcessLayer>();
                if (postProcessingLayer != null)
                {
                    postProcessingLayer.enabled = false;
                }

                var postProcessingVolume = camera.GetComponent<PostProcessVolume>();
                if (postProcessingVolume != null)
                {
                    postProcessingVolume.enabled = false;
                }
            }

            PostProcessVolume[] volumes = GameObject.FindObjectsOfType<PostProcessVolume>();
            foreach (var volume in volumes)
            {
                if (volume != null)
                {
                    volume.enabled = false;
                }
            }
        }

        private void DeleteNonEssentialObjectsDuringGameplay()
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int deletedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj == null)
                    continue;

                string objName = obj.name.ToLower();

                if (
                    objName.Contains("dust") || objName.Contains("smoke") ||
                    objName.Contains("skybox") || objName.Contains("cloud") ||
                    objName.Contains("fog") || objName.Contains("shadow") ||
                    objName.Contains("grass"))
                {
                    try
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                            renderer.enabled = false;

                        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
                        foreach (Renderer childRenderer in childRenderers)
                        {
                            childRenderer.enabled = false;
                        }

                        obj.SetActive(false);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Msg($"Failed to delete object {obj.name}: {ex.Message}");
                    }
                }
            }

            MelonLogger.Msg($"Number of objects deleted and forced not to render: {deletedCount}");
        }
    }
}
