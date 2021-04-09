#pragma warning disable 649
using System.Collections;
using TriLibCore.General;
using UnityEngine;
using UnityEngine.UI;

namespace TriLibCore.Samples
{
    /// <summary>Represents a TriLib sample which allows the user to load models from a website and switch between them to build a 3D face.</summary>

    public class FaceBuilder : AssetViewerBase
    {
        /// <summary>
        /// Parts turning interval in seconds.
        /// </summary>
        private const float TurnLength = 1f;

        /// <summary>
        /// Distance face parts should stay from the scene center.
        /// </summary>
        private const float Distance = 100f;

        /// <summary>
        /// URI from where the models will be downloaded.
        /// </summary>
        private const string BaseURI = "https://ricardoreis.net/trilib/demos/assetdownloader/";

        /// <summary>
        /// Downloaded item indicator template.
        /// </summary>
        [SerializeField]
        private GameObject _downloadTemplate;

        /// <summary>
        /// Switches to the previous part.
        /// </summary>
        /// <param name="partName">The area name to switch (hair, eyes, nose or mouth).</param>
        public void PreviousPart(string partName)
        {
            StartCoroutine(TurnWrapper(partName, -90f));
        }
        /// <summary>
        /// Switches to the next part.
        /// </summary>
        /// <param name="partName">The area name to switch (hair, eyes, nose or mouth).</param>
        public void NextPart(string partName)
        {
            StartCoroutine(TurnWrapper(partName, 90f));
        }

        /// <summary>
        /// Coroutine used to actually switch/turn the parts.
        /// </summary>
        /// <param name="partName">The area name to switch (hair, eyes, nose or mouth).</param>
        /// <param name="angle">The relative angle to rotate the part.</param>
        /// <returns>The Coroutine IEnumerator.</returns>
        private IEnumerator TurnWrapper(string partName, float angle)
        {
            var wrapper = GameObject.Find($"{partName}Wrapper");
            if (wrapper == null)
            {
                yield break;
            }
            var line = GameObject.Find($"{partName}Line");
            if (line == null)
            {
                yield break;
            }
            var buttons = line.GetComponentsInChildren<Button>();
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                button.enabled = false;
            }

            var initialYaw = wrapper.transform.rotation.eulerAngles.y;
            var finalYaw = initialYaw + angle;
            for (var i = 0f; i < TurnLength; i += Time.deltaTime)
            {
                var eulerAngles = wrapper.transform.rotation.eulerAngles;
                eulerAngles.y = Mathf.Lerp(initialYaw, finalYaw, Easing(i / TurnLength));
                wrapper.transform.rotation = Quaternion.Euler(eulerAngles);
                yield return null;
            }
            var finalEulerAngles = wrapper.transform.rotation.eulerAngles;
            finalEulerAngles.y = finalYaw;
            wrapper.transform.rotation = Quaternion.Euler(finalEulerAngles);
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                button.enabled = true;
            }
        }

        /// <summary>Easing method to smooth out a rotation.</summary>
        /// <param name="value">The value to ease.</param>
        /// <returns>The smoothed-out value.</returns>
        private static float Easing(float value)
        {
            if ((value *= 2f) < 1f) return 0.5f * value * value * value;
            return 0.5f * ((value -= 2f) * value * value + 2f);
        }

        /// <summary>
        /// Loads all parts from the given area.
        /// </summary>
        /// <param name="partName">The area name to load (hair, eyes, nose or mouth).</param>
        private void LoadParts(string partName)
        {
            for (var i = 0; i < 4; i++)
            {
                LoadPart(partName, i);
            }
        }

        /// <summary>
        /// Loads the part with the given index from the given area.
        /// </summary>
        /// <param name="partName">The area name to load (hair, eyes, nose or mouth).</param>
        /// <param name="partIndex">The area part index.</param>
        private void LoadPart(string partName, int partIndex)
        {
            var wrapper = GameObject.Find($"{partName}Wrapper");
            if (wrapper == null)
            {
                return;
            }
            var request = AssetDownloader.CreateWebRequest($"{BaseURI}{partName}{partIndex}.zip");
            AssetDownloader.LoadModelFromUri(request, OnLoad, OnMaterialsLoad, OnProgress, OnError, wrapper, AssetLoaderOptions, partIndex, null, true);
        }

        /// <summary>Checks if the Dispatcher instance exists, stores this class instance as the Singleton and load all area parts.</summary>
        protected override void Start()
        {
            base.Start();
            AssetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            LoadParts("Hair");
            LoadParts("Eyes");
            LoadParts("Nose");
            LoadParts("Mouth");
        }


        /// <summary>Event triggered when there is any Model loading error.</summary>
        /// <param name="contextualizedError">The Contextualized Error that has occurred.</param>
        protected override void OnError(IContextualizedError contextualizedError)
        {
            var context = contextualizedError.GetContext();
            if (context is AssetLoaderContext assetLoaderContext)
            {
                var zipLoadCustomContextData = (ZipLoadCustomContextData)assetLoaderContext.CustomData;
                var uriLoadCustomContextData = (UriLoadCustomContextData)zipLoadCustomContextData.CustomData;
                var downloaded = Instantiate(_downloadTemplate, _downloadTemplate.transform.parent);
                var text = downloaded.GetComponentInChildren<Text>();
                text.text = $"Error: {uriLoadCustomContextData.UnityWebRequest.uri.Segments[uriLoadCustomContextData.UnityWebRequest.uri.Segments.Length - 1]}";
                downloaded.SetActive(true);
            }
            base.OnError(contextualizedError);
        }

        /// <summary>Event triggered when the Model and all its resources loaded.</summary>
        /// <param name="assetLoaderContext">The Asset Loader Context reference. Asset Loader Context contains the Model loading data.</param>
        protected override void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            if (assetLoaderContext.RootGameObject != null)
            {
                var zipLoadCustomContextData = (ZipLoadCustomContextData)assetLoaderContext.CustomData;
                var uriLoadCustomContextData = (UriLoadCustomContextData)zipLoadCustomContextData.CustomData;
                var downloaded = Instantiate<GameObject>(_downloadTemplate, _downloadTemplate.transform.parent);
                var text = downloaded.GetComponentInChildren<Text>();
                text.text = $"Done: {uriLoadCustomContextData.UnityWebRequest.uri.Segments[uriLoadCustomContextData.UnityWebRequest.uri.Segments.Length - 1]}";
                downloaded.SetActive(true);
            }
            base.OnMaterialsLoad(assetLoaderContext);
        }

        /// <summary>Event triggered when the Model Meshes and hierarchy are loaded.</summary>
        /// <param name="assetLoaderContext">The Asset Loader Context reference. Asset Loader Context contains the Model loading data.</param>
        protected override void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            if (assetLoaderContext.RootGameObject != null)
            {
                var zipLoadCustomContextData = (ZipLoadCustomContextData)assetLoaderContext.CustomData;
                var uriLoadCustomContextData = (UriLoadCustomContextData)zipLoadCustomContextData.CustomData;
                var partIndex = (int)uriLoadCustomContextData.CustomData;
                var rotation = Quaternion.Euler(0f, partIndex * 90f, 0f);
                assetLoaderContext.RootGameObject.transform.SetPositionAndRotation(rotation * new Vector3(0f, 0f, Distance), rotation);
            }
            base.OnLoad(assetLoaderContext);
        }
    }
}