using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using Harmony;
using System.Collections;

namespace AudicaModding
{
    public class AudicaMod : MelonMod
    {
        public static class BuildInfo
        {
            public const string Name = "FollowCamera";
            public const string Author = "Continuum";
            public const string Company = null;
            public const string Version = "1.2.0";
            public const string DownloadLink = null;
        }


        public static float fov = 0f;
        public static float oldFov = 0f;

        public static bool camOK = false;
        public static bool spectatorCamSet = false;
        public static bool menuCreated = false;

        public static SpectatorCam spectatorCam = null;

        public static OptionsMenuButton toggleButton = null;
        public static OptionsMenuSlider positionSmoothingSlider = null;
        public static OptionsMenuSlider rotationSmoothingSlider = null;
        public static OptionsMenuSlider camHeightSlider = null;
        public static OptionsMenuSlider camDistanceSlider = null;
        public static OptionsMenuSlider camRotationSlider = null;
        public static OptionsMenuSlider camOffsetSlider = null;

        public static Config config = new Config();
        public static string path = Application.dataPath + "/../Mods/Config/FollowCamera.json";

        public static void SaveConfig()
        {
            Directory.CreateDirectory(Application.dataPath + "/../Mods/Config");
            string contents = Encoder.GetConfig(config);
            File.WriteAllText(path, contents);
        }

        public static void LoadConfig()
        {
            if (!File.Exists(path))
            {
                SaveConfig();
            }
            Encoder.SetConfig(config, File.ReadAllText(path));
        }

        public static void UpdateSlider(OptionsMenuSlider slider, string text)
        {
            if (slider == null)
            {
                return;
            }
            else
            {
                slider.label.text = text;
                SaveConfig();
            }
        }

        public static Vector3 ClampMagnitude(Vector3 input, float minMagnitude, float maxMagnitude)
        {
            float inMagnitude = input.magnitude;
            if (inMagnitude < minMagnitude)
            {
                Vector3 inNormalized = input / inMagnitude; //equivalent to in.normalized, but slightly faster in this case
                return inNormalized * minMagnitude;
            }
            else if (inMagnitude > maxMagnitude)
            {
                Vector3 inNormalized = input / inMagnitude; //equivalent to in.normalized, but slightly faster in this case
                return inNormalized * maxMagnitude;
            }

            // No need to clamp at all
            return input;
        }

        public static IEnumerator PerformChecks()
        {
            if (!spectatorCamSet) yield break;
            yield return new WaitForSeconds(.5f);
            CheckCamera();
            if (!camOK) yield break;
            //MelonLogger.Log("Checking done.");

            //MelonLogger.Log("disabling preview stuff");
                spectatorCam.previewCam.gameObject.SetActive(false);
                spectatorCam.previewCamDisplay.SetActive(false);

            //MelonLogger.Log("checking fov");
                //fov = PlayerPreferences.I.SpectatorCamFOV;
                fov = spectatorCam.mFov;
                if (fov != oldFov)
                {
                //if (oldFov != 0f)
                //{
                //MelonLogger.Log("updating fov");
                        spectatorCam.UpdateFOV();
                   // }
                    oldFov = fov;
                }
        }

        public static void SetSpectatorCam(SpectatorCam cam, bool isSet)
        {
            spectatorCam = cam;
            spectatorCamSet = isSet;
        }

        private static void CheckCamera()
        {
            //If spectator cam is on
            bool camOn = PlayerPreferences.I.SpectatorCam.Get();
            if (camOn)
            {
                //If spectator cam is set to static third person
                //float camMode = PlayerPreferences.I.SpectatorCamMode.Get();
                SpectatorCam.CamType camType = spectatorCam.mLastThirdPersonMode;
                if (camType == SpectatorCam.CamType.ThirdPerson)
                {
                    if (config.activated)
                    {
                        //If camOK is already true at this point we don't need to do anything
                        if (!camOK)
                        {
                            //If it's not, get reference for SpectatorCam class and set camOK to true
                            //spectatorCam = UnityEngine.Object.FindObjectOfType<SpectatorCam>();
                            camOK = true;
                        }
                    }
                    else { camOK = false; }
                }
                else { camOK = false; }
            }
            else { camOK = false; }
        }

        public override void OnApplicationStart()
        {
            HarmonyInstance instance = HarmonyInstance.Create("AudicaMod");
            LoadConfig();
        }

        public static void ShowPage(OptionsMenu optionsMenu, OptionsMenu.Page page)
        {
            if (page == OptionsMenu.Page.SpectatorCam && !menuCreated)
            {              
                /*string toggleText = "OFF";

                if (config.activated)
                {
                    toggleText = "ON";
                }
                
                toggleButton = optionsMenu.AddButton
                    (0,
                    toggleText, 
                    new Action(() => { 
                        if (config.activated)
                        {
                            config.activated = false;
                            toggleButton.label.text = "OFF";
                            SaveConfig();
                        }
                        else
                        {
                            config.activated = true;
                            toggleButton.label.text = "ON";
                            SaveConfig();
                        }
                    }), 
                    null, 
                    "Turns the follow camera on or off");
                */

                positionSmoothingSlider = optionsMenu.AddSlider
                    (
                    0,
                    "Position Speed",
                    "P",
                    new Action<float>((float n) => {
                        config.positionSmoothing = Mathf.Round((config.positionSmoothing + (n * 0.001f)) * 1000.0f) / 1000.0f;
                        UpdateSlider(positionSmoothingSlider, "Position Smoothing : " + config.positionSmoothing.ToString());
                    }),
                    null,
                    null,
                    "Changes how smooth position will be"
                    );
                positionSmoothingSlider.label.text = "Position Smoothing : " + config.positionSmoothing.ToString();

                rotationSmoothingSlider = optionsMenu.AddSlider
                    (
                    0,
                    "Rotation Speed",
                    "P",
                    new Action<float>((float n) => {
                        config.rotationSmoothing = Mathf.Round((config.rotationSmoothing + (n * 0.001f)) * 1000.0f) / 1000.0f;
                        UpdateSlider(rotationSmoothingSlider, "Rotation Smoothing : " + config.rotationSmoothing.ToString());
                    }),
                    null,
                    null,
                    "Changes how smooth rotation will be"
                    );
                rotationSmoothingSlider.label.text = "Rotation Smoothing : " + config.rotationSmoothing.ToString();

                camOffsetSlider = optionsMenu.AddSlider
                    (
                    0,
                    "Horizontal Offset",
                    "P",
                    new Action<float>((float n) => {
                        config.camOffset = Mathf.Round((config.camOffset + (n * 0.1f)) * 10.0f) / 10.0f;
                        UpdateSlider(camOffsetSlider, "Horizontal Offset : " + config.camOffset.ToString());
                    }),
                    null,
                    null,
                    "Changes horizontal position"
                    );
                camOffsetSlider.label.text = "Horizontal Offset : " + config.camOffset.ToString();

                camHeightSlider = optionsMenu.AddSlider
                    (
                    0,
                    "Vertical Offset",
                    "P",
                    new Action<float>((float n) => {
                        config.camHeight = Mathf.Round((config.camHeight + (n * 0.1f)) * 10.0f) / 10.0f;
                        UpdateSlider(camHeightSlider, "Vertical Offset : " + config.camHeight.ToString());
                    }),
                    null,
                    null,
                    "Changes vertical position"
                    );
                camHeightSlider.label.text = "Vertical Offset : " + config.camHeight.ToString();

                camDistanceSlider = optionsMenu.AddSlider
                    (
                    0,
                    "Distance",
                    "P",
                    new Action<float>((float n) => {
                        config.camDistance = Mathf.Round((config.camDistance + (n * 0.1f)) * 10.0f) / 10.0f;
                        UpdateSlider(camDistanceSlider, "Distance : " + config.camDistance.ToString());
                    }),
                    null,
                    null,
                    "Changes the distance"
                    );
                camDistanceSlider.label.text = "Distance : " + config.camDistance.ToString();

                camRotationSlider = optionsMenu.AddSlider
                    (
                    0,
                    "Tilt",
                    "P",
                    new Action<float>((float n) => {
                        config.camRotation = Mathf.Round((config.camRotation + (n * 0.1f)) * 10.0f) / 10.0f;
                        UpdateSlider(camRotationSlider, "Rotation : " + config.camRotation.ToString());
                    }),
                    null,
                    null,
                    "Changes the rotation"
                    );
                camRotationSlider.label.text = "Rotation : " + config.camRotation.ToString();

                optionsMenu.scrollable.AddRow(optionsMenu.AddHeader(0, "Follow Camera <size=5>Must be set to 3rd person static</size>"));

                //optionsMenu.scrollable.AddRow(toggleButton.gameObject);
                optionsMenu.scrollable.AddRow(positionSmoothingSlider.gameObject);
                optionsMenu.scrollable.AddRow(rotationSmoothingSlider.gameObject);
                optionsMenu.scrollable.AddRow(camOffsetSlider.gameObject);
                optionsMenu.scrollable.AddRow(camHeightSlider.gameObject);
                optionsMenu.scrollable.AddRow(camDistanceSlider.gameObject);
                optionsMenu.scrollable.AddRow(camRotationSlider.gameObject);

                if (config.activated)
                {
                    spectatorCam.previewCam.gameObject.SetActive(true);
                    spectatorCam.previewCamDisplay.SetActive(true);
                }

                menuCreated = true;
            }
            else
            {
                menuCreated = false;
            }
        }

        public static void SpectatorCamUpdate()
        {        
            Transform head = AvatarSelector.I.customHead.transform;

            Vector3 hmdPos = head.position;
            Vector3 hmdRot = head.rotation.eulerAngles;

            Vector3 hmdOffsetPos = new Vector3(hmdPos.x + (head.right.x * config.camOffset), hmdPos.y, hmdPos.z + (head.right.z * config.camOffset));

            Vector3 camPos = spectatorCam.cam.gameObject.transform.position;
            Quaternion camRot = spectatorCam.cam.gameObject.transform.rotation;

            Vector3 destinationPos = new Vector3(hmdOffsetPos.x - (head.forward.x * config.camDistance), config.camHeight, hmdOffsetPos.z - (head.forward.z * config.camDistance));
            Quaternion destinationRot = Quaternion.Euler(config.camRotation, hmdRot.y, 0);

            spectatorCam.cam.gameObject.transform.position = Vector3.Slerp(camPos, destinationPos, config.positionSmoothing);
            spectatorCam.cam.gameObject.transform.rotation = Quaternion.Slerp(camRot, destinationRot, config.rotationSmoothing);
        }
    }
}
