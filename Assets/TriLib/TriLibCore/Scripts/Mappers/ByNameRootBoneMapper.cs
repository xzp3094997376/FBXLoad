using TriLibCore.General;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that searches for a root bone on the Models by the bone names.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/Root Bone/By Name Root Bone Mapper", fileName = "ByNameRootBoneMapper")]
    public class ByNameRootBoneMapper : RootBoneMapper
    {
        /// <summary>
        /// String comparison mode to use on the mapping.
        /// </summary>
        [Header("Left = Loaded GameObjects Names, Right = Names in RootBoneNames")]
        public StringComparisonMode StringComparisonMode;

        /// <summary>
        /// Is the string comparison case insensitive?
        /// </summary>
        public bool CaseInsensitive = true;

        /// <summary>
        /// Root bone names to be searched.
        /// </summary>
        public string[] RootBoneNames = { "Hips", "Bip01", "Pelvis" };

        /// <inheritdoc />        
        public override Transform Map(AssetLoaderContext assetLoaderContext)
        {
            if (RootBoneNames != null)
            {
                for (var i = 0; i < RootBoneNames.Length; i++)
                {
                    var rootBoneName = RootBoneNames[i];
                    var found = FindDeepChild(assetLoaderContext.RootGameObject.transform, rootBoneName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return base.Map(assetLoaderContext);
        }

        private Transform FindDeepChild(Transform transform, string right)
        {
            if (StringComparer.Matches(StringComparisonMode, CaseInsensitive, transform.name, right))
            {
                return transform;
            }

            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var found = FindDeepChild(child, right);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
    }
}