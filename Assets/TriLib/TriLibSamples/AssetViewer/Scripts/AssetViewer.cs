#pragma warning disable 649
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TriLibCore.SFB;
using TriLibCore.General;
using TriLibCore.Extensions;
using TriLibCore.Fbx.Reader;
using TriLibCore.Mappers;
using TriLibCore.Pooling;
using TriLibCore.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace TriLibCore.Samples
{
    /// <summary>Represents a TriLib sample which allows the user to load models and HDR skyboxes from the local file-system.</summary>
    public class AssetViewer : AssetViewerBase
    {
        /// <summary>
        /// Maximum camera distance ratio based on model bounds.
        /// </summary>
        private const float MaxCameraDistanceRatio = 3f;

        /// <summary>
        /// Camera distance ratio based on model bounds.
        /// </summary>
        protected const float CameraDistanceRatio = 2f;

        /// <summary>
        /// minimum camera distance.
        /// </summary>
        protected const float MinCameraDistance = 0.01f;

        /// <summary>
        /// Skybox scale based on model bounds.
        /// </summary>
        protected const float SkyboxScale = 100f;

        /// <summary>
        /// Skybox game object.
        /// </summary>
        [SerializeField]
        protected GameObject Skybox;

        /// <summary>
        /// Skybox game object renderer.
        /// </summary>
        [SerializeField]
        private Renderer _skyboxRenderer;

        /// <summary>
        /// Directional light.
        /// </summary>
        [SerializeField]
        private Light _light;

        /// <summary>
        /// Skybox material preset to create the final skybox material.
        /// </summary>
        [SerializeField]
        private Material _skyboxMaterialPreset;

        /// <summary>
        /// Main reflection probe.
        /// </summary>
        [SerializeField]
        private ReflectionProbe _reflectionProbe;

        /// <summary>
        /// Skybox exposure slider.
        /// </summary>
        [SerializeField]
        private Slider _skyboxExposureSlider;

        /// <summary>
        /// Current camera distance.
        /// </summary>
        protected float CameraDistance = 1f;

        /// <summary>
        /// Current camera pivot position.
        /// </summary>
        protected Vector3 CameraPivot;

        /// <summary>
        /// Current directional light angle.
        /// </summary>
        private Vector2 _lightAngle = new Vector2(0f, -45f);

        /// <summary>
        /// Input multiplier based on loaded model bounds.
        /// </summary>
        protected float InputMultiplier = 1f;

        /// <summary>
        /// Skybox instantiated material.
        /// </summary>
        private Material _skyboxMaterial;

        /// <summary>
        /// Texture loaded for skybox.
        /// </summary>
        private Texture2D _skyboxTexture;

        /// <summary>
        /// List of loaded animations.
        /// </summary>
        private List<AnimationClip> _animations;

        /// <summary>
        /// Created animation component for the loaded model.
        /// </summary>
        private Animation _animation;

        private Stopwatch _stopwatch;

        /// <summary>Gets the playing Animation State.</summary>
        private AnimationState CurrentAnimationState
        {
            get
            {
                if (_animation != null)
                {
                    return _animation[PlaybackAnimation.options[PlaybackAnimation.value].text];
                }
                return null;
            }
        }
        /// <summary>Is there any animation playing?</summary>
        private bool AnimationIsPlaying => _animation != null && _animation.isPlaying;

        /// <summary>Shows the file picker for loading a model from the local file-system.</summary>
        public void LoadModelFromFile()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            base.LoadModelFromFile();
        }

        /// <summary>Shows the file picker for loading a skybox from the local file-system.</summary>
        public void LoadSkyboxFromFile()
        {
            SetLoading(false);
            var title = "Select a skybox image";
            var extensions = new ExtensionFilter[]
            {
                new ExtensionFilter("Radiance HDR Image (hdr)", "hdr")
            };
            StandaloneFileBrowser.OpenFilePanelAsync(title, null, extensions, true, OnSkyboxStreamSelected);
        }

        /// <summary>
        /// Removes the skybox texture.
        /// </summary>
        public void ClearSkybox()
        {
            if (_skyboxMaterial == null)
            {
                _skyboxMaterial = Instantiate(_skyboxMaterialPreset);
            }
            _skyboxMaterial.mainTexture = null;
            _skyboxExposureSlider.value = 1f;
            OnSkyboxExposureChanged(1f);
        }

        public void ResetModelScale()
        {
            if (RootGameObject != null)
            {
                RootGameObject.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Plays the selected animation.
        /// </summary>
        public override void PlayAnimation()
        {
            if (_animation == null)
            {
                return;
            }
            _animation.Play(PlaybackAnimation.options[PlaybackAnimation.value].text);
        }

        /// <summary>
        /// Stop playing the selected animation.
        /// </summary>
        public override void StopAnimation()
        {
            if (_animation == null)
            {
                return;
            }
            PlaybackSlider.value = 0f;
            _animation.Stop();
            SampleAnimationAt(0f);
        }

        /// <summary>Switches to the animation selected on the Dropdown.</summary>
        /// <param name="index">The selected Animation index.</param>
        public override void PlaybackAnimationChanged(int index)
        {
            StopAnimation();
        }

        /// <summary>Event triggered when the Animation slider value has been changed by the user.</summary>
        /// <param name="value">The Animation playback normalized position.</param>
        public override void PlaybackSliderChanged(float value)
        {
            if (!AnimationIsPlaying)
            {
                var animationState = CurrentAnimationState;
                if (animationState != null)
                {
                    SampleAnimationAt(value);
                }
            }
        }

        /// <summary>Samples the Animation at the given normalized time.</summary>
        /// <param name="value">The Animation normalized time.</param>
        private void SampleAnimationAt(float value)
        {
            if (_animation == null || RootGameObject == null)
            {
                return;
            }
            var animationClip = _animation.GetClip(PlaybackAnimation.options[PlaybackAnimation.value].text);
            animationClip.SampleAnimation(RootGameObject, animationClip.length * value);
        }

        /// <summary>
        /// Event triggered when the user selects the skybox on the selection dialog.
        /// </summary>
        /// <param name="files">Selected files.</param>
        private void OnSkyboxStreamSelected(IList<ItemWithStream> files)
        {			
            if (files != null && files.Count > 0 && files[0].HasData)
            {
#if (UNITY_WSA || UNITY_ANDROID) && !UNITY_EDITOR
                Dispatcher.InvokeAsync(new ContextualizedAction<Stream>(LoadSkybox, files[0].OpenStream()));
#else
                LoadSkybox(files[0].OpenStream());
#endif
            } else
            {

#if (UNITY_WSA || UNITY_ANDROID) && !UNITY_EDITOR
                Dispatcher.InvokeAsync(new ContextualizedAction(ClearSkybox));
#else
                ClearSkybox();
#endif
            }
        }

        /// <summary>Loads the skybox from the given Stream.</summary>
        /// <param name="stream">The Stream containing the HDR Image data.</param>
        /// <returns>Coroutine IEnumerator.</returns>
        private IEnumerator DoLoadSkybox(Stream stream)
        {
            //Double frame waiting hack
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (_skyboxTexture != null)
            {
                Destroy(_skyboxTexture);
            }
            ClearSkybox();
            _skyboxTexture = HDRLoader.HDRLoader.Load(stream, out var gamma, out var exposure);
            _skyboxMaterial.mainTexture = _skyboxTexture;
            _skyboxExposureSlider.value = 1f;
            OnSkyboxExposureChanged(exposure);
            stream.Close();
            SetLoading(false);
        }

        /// <summary>Starts the Coroutine to load the skybox from the given Sstream.</summary>
        /// <param name="stream">The Stream containing the HDR Image data.</param>
        private void LoadSkybox(Stream stream)
        {
            SetLoading(true);
            StartCoroutine(DoLoadSkybox(stream));
        }

        /// <summary>Event triggered when the skybox exposure Slider has changed.</summary>
        /// <param name="exposure">The new exposure value.</param>
        public void OnSkyboxExposureChanged(float exposure)
        {
            _skyboxMaterial.SetFloat("_Exposure", exposure);
            _skyboxRenderer.material = _skyboxMaterial;
            RenderSettings.skybox = _skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            _reflectionProbe.RenderProbe();
        }

        /// <summary>Initializes the base-class and clears the skybox Texture.</summary>
        protected override void Start()
        {
            base.Start();
            AssetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            AssetLoaderOptions.Timeout = 180;
            ClearSkybox();
        }

        /// <summary>Handles the input.</summary>
        private void Update()
        {
            ProcessInput();
            UpdateHUD();
        }

        /// <summary>Handles the input and moves the Camera accordingly.</summary>
        protected virtual void ProcessInput()
        {
            ProcessInputInternal(Camera.main.transform);
        }

        /// <summary>
        /// Handles the input using the given Camera.
        /// </summary>
        /// <param name="cameraTransform">The Camera to process input movements.</param>
        private void ProcessInputInternal(Transform cameraTransform)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButton(0))
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    {
                        _lightAngle.x = Mathf.Repeat(_lightAngle.x + Input.GetAxis("Mouse X"), 360f);
                        _lightAngle.y = Mathf.Clamp(_lightAngle.y + Input.GetAxis("Mouse Y"), -MaxPitch, MaxPitch);
                    }
                    else
                    {
                        UpdateCamera();
                    }
                }
                if (Input.GetMouseButton(2))
                {
                    CameraPivot -= cameraTransform.up * Input.GetAxis("Mouse Y") * InputMultiplier + cameraTransform.right * Input.GetAxis("Mouse X") * InputMultiplier;
                }
                CameraDistance = Mathf.Min(CameraDistance - Input.mouseScrollDelta.y * InputMultiplier, InputMultiplier * (1f / InputMultiplierRatio) * MaxCameraDistanceRatio);
                if (CameraDistance < 0f)
                {
                    CameraPivot += cameraTransform.forward * -CameraDistance;
                    CameraDistance = 0f;
                }
                Skybox.transform.position = CameraPivot;
                cameraTransform.position = CameraPivot + Quaternion.AngleAxis(CameraAngle.x, Vector3.up) * Quaternion.AngleAxis(CameraAngle.y, Vector3.right) * new Vector3(0f, 0f, Mathf.Max(MinCameraDistance, CameraDistance));
                cameraTransform.LookAt(CameraPivot);
                _light.transform.position = CameraPivot + Quaternion.AngleAxis(_lightAngle.x, Vector3.up) * Quaternion.AngleAxis(_lightAngle.y, Vector3.right) * Vector3.forward;
                _light.transform.LookAt(CameraPivot);
            }
        }

        /// <summary>Updates the HUD information.</summary>
        private void UpdateHUD()
        {
            var animationState = CurrentAnimationState;
            var time = animationState == null ? 0f : PlaybackSlider.value * animationState.length % animationState.length;
            var seconds = time % 60f;
            var milliseconds = time * 100f % 100f;
            PlaybackTime.text = $"{seconds:00}:{milliseconds:00}";
            var normalizedTime = animationState == null ? 0f : animationState.normalizedTime % 1f;
            if (AnimationIsPlaying)
            {
                PlaybackSlider.value = float.IsNaN(normalizedTime) ? 0f : normalizedTime;
            }
            var animationIsPlaying = AnimationIsPlaying;
            if (_animation != null)
            {
                Play.gameObject.SetActive(!animationIsPlaying);
                Stop.gameObject.SetActive(animationIsPlaying);
            }
        }

        /// <summary>Event triggered when the user selects a file or cancels the Model selection dialog.</summary>
        /// <param name="hasFiles">If any file has been selected, this value is <c>true</c>, otherwise it is <c>false</c>.</param>
        protected override void OnBeginLoadModel(bool hasFiles)
        {
            base.OnBeginLoadModel(hasFiles);
            if (hasFiles)
            {
                _animations = null;
            }
        }

        /// <summary>Event triggered when the Model Meshes and hierarchy are loaded.</summary>
        /// <param name="assetLoaderContext">The Asset Loader Context reference. Asset Loader Context contains the Model loading data.</param>
        protected override void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            base.OnLoad(assetLoaderContext);
            ResetModelScale();
            if (assetLoaderContext.RootGameObject != null)
            {
                PlaybackAnimation.options.Clear();
                if (assetLoaderContext.Options.AnimationType == AnimationType.Legacy)
                {
                    _animation = assetLoaderContext.RootGameObject.GetComponent<Animation>();
                    if (_animation != null)
                    {
                        _animations = _animation.GetAllAnimationClips();
                        if (_animations.Count > 0)
                        {
                            for (var i = 0; i < _animations.Count; i++)
                            {
                                var animationClip = _animations[i];
                                PlaybackAnimation.options.Add(new Dropdown.OptionData(animationClip.name));
                            }

                            PlaybackAnimation.captionText.text = _animations[0].name;
                        }
                        else
                        {
                            _animation = null;
                        }
                    }
                    if (_animation == null)
                    {
                        PlaybackAnimation.captionText.text = null;
                    }
                }
                PlaybackAnimation.value = 0;
                StopAnimation();
                RootGameObject = assetLoaderContext.RootGameObject;
            }
            ModelTransformChanged();
        }

        /// <summary>
        /// Changes the camera placement when the Model has changed.
        /// </summary>
        protected virtual void ModelTransformChanged()
        {
            if (RootGameObject != null)
            {
                var bounds = RootGameObject.CalculateBounds();
                Camera.main.FitToBounds(bounds, CameraDistanceRatio);
                CameraDistance = Camera.main.transform.position.magnitude;
                CameraPivot = bounds.center;
                Skybox.transform.localScale = bounds.size.magnitude * SkyboxScale * Vector3.one;
                InputMultiplier = bounds.size.magnitude * InputMultiplierRatio;
                CameraAngle = Vector2.zero;
            }
        }

        /// <summary>
        /// Event is triggered when any error occurs.
        /// </summary>
        /// <param name="contextualizedError">The Contextualized Error that has occurred.</param>
        protected override void OnError(IContextualizedError contextualizedError)
        {
            base.OnError(contextualizedError);
            StopAnimation();
            _stopwatch?.Stop();
        }

        protected override void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            base.OnMaterialsLoad(assetLoaderContext);
            _stopwatch.Stop();
            Debug.Log("Loaded in:"  +_stopwatch.Elapsed);
        }
    }
}