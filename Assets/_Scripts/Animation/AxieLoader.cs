using System;
using System.Collections.Generic;
using _Scripts.Player.Pawn;
using AxieMixer.Unity;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Game.Player.Axie
{
    /// <summary>
    /// The class was extracted from AxieMixerPlayground.cs
    /// </summary>
    public class AxieLoader : MonoBehaviour
    {
        Axie2dBuilder Builder => Mixer.Builder;

        private MapPawn _pawn ;

        [SerializeField] private PawnDescription _pawnDescription;
        [SerializeField] private Vector3 _skeletonScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        [SerializeField] private string _skeletonLayer = "Pawn";
        [SerializeField] private string _sortingLayerName = "Pawn";
        [SerializeField] private int _sortingOrder = 0;
        
        
        private void Awake()
        {
            _pawn = GetComponent<MapPawn>();
            
            
        }

        private void Start()
        {
            if(_pawnDescription == null && _pawn != null) _pawnDescription = _pawn.PawnDescription;
            if(_pawnDescription != null) ProcessMixer(_pawnDescription.PawnID.ToString() , _pawnDescription.AxieHex, false);
            else
            {
                Debug.LogWarning("PawnDescription is null in AxieLoader.cs");
            }
        }

        void ProcessMixer(string axieId, string genesStr, bool isGraphic)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }
            float scale = 0.007f;

            var meta = new Dictionary<string, string>();
            //foreach (var accessorySlot in ACCESSORY_SLOTS)
            //{
            //    meta.Add(accessorySlot, $"{accessorySlot}1{System.Char.ConvertFromUtf32((int)('a') + accessoryIdx - 1)}");
            //}
            
            if (Mixer.Builder == null)
            {
                Mixer.Init();
                Debug.Log("Mixer.Init()");
            }
            if (Mixer.Builder == null)
            {
                Debug.LogError("Mixer.Builder is still null");
            }
            
            var builderResult = Builder.BuildSpineFromGene(axieId, genesStr, meta, scale, isGraphic);

            if (builderResult == null)
            {
                Debug.LogError($"[{axieId}] builderResult is null!!!");
                return;
            }
            
            SpawnSkeletonAnimation(builderResult);
            
        }
        
        /// <summary>
        /// This is the method to spawn skeleton animation, use by GameObject
        /// </summary>
        /// <param name="builderResult"></param>
        void SpawnSkeletonAnimation(Axie2dBuilderResult builderResult)
        {
            SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation.gameObject.layer = LayerMask.GetMask(_skeletonLayer);
            runtimeSkeletonAnimation.transform.SetParent(transform, false);
            runtimeSkeletonAnimation.transform.localScale = _skeletonScale;
            

            runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

            if (builderResult.adultCombo.ContainsKey("body") &&
                builderResult.adultCombo["body"].Contains("mystic") &&
                builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                runtimeSkeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
            }
            runtimeSkeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
            
            MeshRenderer meshRenderer = runtimeSkeletonAnimation.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingLayerName = _sortingLayerName.ToString();
                meshRenderer.sortingOrder = _sortingOrder;
            }
        }

        
    }
}