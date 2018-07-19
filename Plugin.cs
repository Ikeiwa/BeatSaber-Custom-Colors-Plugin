using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Xft;
using TMPro;

namespace CustomColors
{
    public class Plugin : IPlugin
    {
        private Color _colorLeft = new Color(1.0f, 1.0f, 0.0f);
        private Color _colorRight = new Color(0.0f, 1.0f, 0.0f);

        public string Name
        {
            get { return "Custom Note Mod"; }
        }

        public string Version
        {
            get { return "1.0"; }
        }

        private bool _init;

        private bool _colorInit;

        //Color objects
        ColorManager colManager;
        SimpleColorSO[] colorSOs;
        Material[] source;
        List<Material> envLights;
        Saber[] sabers;
        XWeaponTrail[] weapontrails;
        TubeBloomPrePassLight[] prePassLights;

        public void OnApplicationStart()
        {
            if (_init) return;
            _init = true;

            _colorInit = false;

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            GetObjects();
            setupColors();
        }

        public void SetColors(Color leftColor, Color rightColor)
        {
            _colorLeft = leftColor;
            _colorRight = rightColor;
            setupColors();
        }

        public void SetLeftColor(Color leftColor)
        {
            _colorLeft = leftColor;
            setupColors();
        }

        public void SetRightColor(Color rightColor)
        {
            _colorRight = rightColor;
            setupColors();
        }

        private void GetObjects()
        {
            colManager = Resources.FindObjectsOfTypeAll<ColorManager>().FirstOrDefault();
            colorSOs = Resources.FindObjectsOfTypeAll<SimpleColorSO>();
            source = Resources.FindObjectsOfTypeAll<Material>();
            sabers = UnityEngine.Object.FindObjectsOfType<Saber>();
            weapontrails = UnityEngine.Object.FindObjectsOfType<XWeaponTrail>();
            prePassLights = UnityEngine.Object.FindObjectsOfType<TubeBloomPrePassLight>();

            envLights = new List<Material>();
            Renderer[] rends = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (Renderer rend in rends)
            {
                if (rend.materials.Length > 0)
                {
                    if (rend.material.shader.name == "Custom/ParametricBox" || rend.material.shader.name == "Custom/ParametricBoxOpaque")
                    {
                        envLights.Add(rend.material);
                        Console.WriteLine("found material");
                    }
                }
            }
        }

        private void setupColors()
        {
            _colorInit = false;
            
            if (colManager != null)
            {
                ReflectionUtil.SetPrivateField(colManager, "_colorA", _colorLeft);
                ReflectionUtil.SetPrivateField(colManager, "_colorB", _colorRight);
            }
            
            foreach(SimpleColorSO colorSO in colorSOs)
            {
                var oldCol = ReflectionUtil.GetPrivateField<Color>(colorSO, "_color");
                if (oldCol.r == 1.0f)
                {
                    ReflectionUtil.SetPrivateField(colorSO, "_color", _colorLeft);
                }
                else
                {
                    ReflectionUtil.SetPrivateField(colorSO, "_color", _colorRight);
                }
            }

            var _blueSaberMat = source.FirstOrDefault((Material x) => x.name == "BlueSaber");
            _blueSaberMat.SetColor("_Color", _colorRight);
            _blueSaberMat.SetColor("_EmissionColor", _colorRight);
            var _redSaberMat = source.FirstOrDefault((Material x) => x.name == "RedSaber");
            _redSaberMat.SetColor("_Color", _colorLeft);
            _redSaberMat.SetColor("_EmissionColor", _colorLeft);

            foreach (Saber saber in sabers)
            {
                MeshRenderer lineRenderer = saber.gameObject.transform.Find("Handle/Ornament").GetComponent<MeshRenderer>();

                if (saber.saberType == Saber.SaberType.SaberA)
                {
                    lineRenderer.sharedMaterial = _redSaberMat;
                    lineRenderer.material = _redSaberMat;
                }
                else
                {
                    lineRenderer.sharedMaterial = _blueSaberMat;
                    lineRenderer.material = _blueSaberMat;
                }
            }
        }

        public void OnUpdate()
        {
            if (_colorInit == false)
            {

                foreach (XWeaponTrail trail in weapontrails)
                {
                    var oldCol = ReflectionUtil.GetPrivateField<Color>(trail, "MyColor");
                    if (oldCol.r == 1.0f)
                    {
                        ReflectionUtil.SetPrivateField(trail, "MyColor", new Color(_colorLeft.r, _colorLeft.g, _colorLeft.b, oldCol.a));
                    }
                    else
                    {
                        ReflectionUtil.SetPrivateField(trail, "MyColor", new Color(_colorRight.r, _colorRight.g, _colorRight.b, oldCol.a));
                    }
                }
                
                if (prePassLights.Length > 0)
                {
                    foreach (TubeBloomPrePassLight prePassLight in prePassLights)
                    {
                        var oldCol = ReflectionUtil.GetPrivateField<Color>(prePassLight, "_color");
                        if (oldCol.r == 1.0f)
                        {
                            ReflectionUtil.SetPrivateField(prePassLight, "_color", _colorLeft);
                        }
                        else
                        {
                            ReflectionUtil.SetPrivateField(prePassLight, "_color", _colorRight);
                        }
                    }
                }

                foreach (Material mat in envLights)
                {
                    mat.SetColor("_Color", new Color(_colorRight.r * 0.5f, _colorRight.g * 0.5f, _colorRight.b * 0.5f, 1.0f));
                }

                if (SceneManager.GetActiveScene().name == "Menu")
                {
                    TextMeshPro[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshPro>();
                    if (texts.Length > 0)
                    {
                        foreach (TextMeshPro text in texts)
                        {
                            var oldCol = ReflectionUtil.GetPrivateField<Color>(text, "m_fontColor");
                            if (oldCol.r == 1.0f)
                            {
                                ReflectionUtil.SetPrivateField(text, "m_fontColor", _colorLeft);
                            }
                            else
                            {
                                ReflectionUtil.SetPrivateField(text, "m_fontColor", _colorRight);
                            }
                        }
                    }

                    FlickeringNeonSign E = UnityEngine.Object.FindObjectOfType<FlickeringNeonSign>();
                    if (E != null)
                    {
                        ReflectionUtil.SetPrivateField(E, "_onColor", _colorRight);
                    }
                }

                _colorInit = true;
            }
        }

        public void OnFixedUpdate()
        {

        }

        public void OnLevelWasInitialized(int level)
        {


        }

        public void OnLevelWasLoaded(int level)
        {

        }
    }
}
