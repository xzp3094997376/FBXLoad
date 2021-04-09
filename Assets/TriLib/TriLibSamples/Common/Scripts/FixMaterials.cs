using TriLibCore.Mappers;
using UnityEngine;

namespace TriLibCore.Samples
{
    /// <summary>
    /// Represents a class used to fix TriLib sample models depending on the rendering pipeline.
    /// </summary>
    public class FixMaterials : MonoBehaviour
    {
        private void Start()
        {
            MaterialMapper materialMapper = null;
            foreach (var materialMapperName in MaterialMapper.RegisteredMappers)
            {
                if (TriLibSettings.GetBool(materialMapperName))
                {
                    materialMapper = (MaterialMapper)ScriptableObject.CreateInstance(materialMapperName);
                    break;
                }
            }
            if (materialMapper == null)
            {
                return;
            }
            var meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                var materials = meshRenderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var color = meshRenderer.sharedMaterials[i].color;
                    materials[i] = Instantiate(materialMapper.MaterialPreset);
                    materials[i].color = color;
                }
                meshRenderer.materials = materials;
            }
            var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var materials = skinnedMeshRenderer.materials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var color = skinnedMeshRenderer.sharedMaterials[i].color;
                    materials[i] = Instantiate(materialMapper.MaterialPreset);
                    materials[i].color = color;
                }
                skinnedMeshRenderer.materials = materials;
            }
            Destroy(materialMapper);
        }
    }
}
