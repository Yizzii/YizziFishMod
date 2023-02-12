using BepInEx;
using System;
using UnityEngine;
using UnityEngine.XR;
using Utilla;
using GorillaLocomotion;

namespace YizziFishMod
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        public float swimforce = 8f;
        public float dragforce = 1.5f;
        public float minforce = 2.5f;
        public Transform trackingReference;
        public Transform lefthand;
        public Transform righthand;
        public GorillaVelocityEstimator leftveloE;
        public GorillaVelocityEstimator rightveloE;
        public bool swimming;
        public bool locked = true;
        public bool LPButton;
        public bool keyP;
        public XRNode lHandNode = XRNode.LeftHand;
        public XRNode rHandNode = XRNode.RightHand;
        Rigidbody _rigidbody;

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            swimming = true;
        }

        void OnDisable()
        {
            swimming = false;
            _rigidbody.useGravity = true;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            _rigidbody = Player.Instance.bodyCollider.attachedRigidbody;
            trackingReference = GameObject.Find("Player VR Controller/GorillaPlayer/TurnParent").transform;
            lefthand = Player.Instance.leftHandTransform;
            righthand = Player.Instance.rightHandTransform;
            leftveloE = GameObject.Find("SnowballMakerLeftHand").GetComponent<GorillaVelocityEstimator>();
            rightveloE = GameObject.Find("SnowballMakerRightHand").GetComponent<GorillaVelocityEstimator>();
        }

        void Update()
        {
            if (inRoom)
            {
                InputDevices.GetDeviceAtXRNode(lHandNode).TryGetFeatureValue(CommonUsages.primaryButton, out LPButton);
                if (LPButton)
                {
                    if (keyP == false)
                    {
                        locked = !locked;
                    }
                    keyP = true;
                }
                else
                {
                    keyP = false;
                }
                if (swimming)
                {
                    if (!locked)
                    {
                        var lefthandv = leftveloE.linearVelocity;
                        var righthandv = rightveloE.linearVelocity;
                        Vector3 localVelo = righthandv;
                        Vector3 localLVelo = lefthandv;
                        localLVelo *= -1;
                        localVelo *= -1;

                        if (localVelo.sqrMagnitude > minforce * minforce)
                        {
                            Vector3 worldvelo = trackingReference.TransformDirection(localVelo);
                            _rigidbody.AddForce(localVelo * swimforce, ForceMode.Acceleration);
                        }

                        if (localLVelo.sqrMagnitude > minforce * minforce)
                        {
                            Vector3 worldvelo = trackingReference.TransformDirection(localLVelo);
                            _rigidbody.AddForce(localLVelo * swimforce, ForceMode.Acceleration);
                        }

                        if (_rigidbody.velocity.sqrMagnitude > 0.01f)
                        {
                            _rigidbody.AddForce(-_rigidbody.velocity * dragforce, ForceMode.Acceleration);
                        }
                        _rigidbody.useGravity = false;
                    }
                    else
                    {
                        _rigidbody.useGravity = true;
                    }
                }
                   
            }
        }


        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;
            _rigidbody.useGravity = true;
        }
    }
}
