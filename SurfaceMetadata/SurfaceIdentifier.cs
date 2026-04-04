using GameFramework.Dependencies;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameFramework.SurfaceMetadata
{
    public class SurfaceIdentifier : MonoBehaviour, ISurfaceModifier, IEntityComponent
    {
        public IEntity Entity { get; set; }
        
        public SurfaceMeta Meta => meta;
        
        [FormerlySerializedAs("Meta")] [SerializeField] 
        private SurfaceMeta meta;
    }
}