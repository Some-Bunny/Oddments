﻿using Alexandria.DungeonAPI;
using Alexandria.ItemAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GungeonAPI
{
    public class ShrineFactory
    {
        public static void Init()
        {
            bool initialized = ShrineFactory.m_initialized;
            bool flag = !initialized;
            if (flag)
            {
                DungeonHooks.OnFoyerAwake += ShrineFactory.PlaceBreachShrines;
                DungeonHooks.OnPreDungeonGeneration += delegate (LoopDungeonGenerator generator, Dungeon dungeon, DungeonFlow flow, int dungeonSeed)
                {
                    bool flag2 = flow.name != "Foyer Flow" && !GameManager.IsReturningToFoyerWithPlayer;
                    if (flag2)
                    {
                        foreach (ShrineFactory.CustomShrineController customShrineController in UnityEngine.Object.FindObjectsOfType<ShrineFactory.CustomShrineController>())
                        {
                            bool flag4 = !ShrineFakePrefab.IsFakePrefab(customShrineController);
                            if (flag4)
                            {
                                UnityEngine.Object.Destroy(customShrineController.gameObject);
                            }
                        }
                        ShrineFactory.m_builtShrines = false;
                    }
                };
                ShrineFactory.m_initialized = true;
            }
        }



        public class ShrineShadowHandler : MonoBehaviour
        {
            public ShrineShadowHandler()
            {
                this.shadowObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("DefaultShadowSprite"));
                this.Offset = new Vector2(0, 0);
            }

            public void Start()
            {
                GameObject shadowObj = (GameObject)UnityEngine.Object.Instantiate(shadowObject);
                shadowObj.transform.parent = base.gameObject.transform;
                tk2dSprite shadowSprite = shadowObj.GetComponent<tk2dSprite>();
                shadowSprite.renderer.enabled = true;
                shadowSprite.HeightOffGround = base.gameObject.GetComponent<tk2dSprite>().HeightOffGround - 0.1f;
                shadowObj.transform.position.WithZ(base.gameObject.transform.position.z + 99999f);
                shadowObj.transform.position = base.gameObject.transform.position + Offset;
                DepthLookupManager.ProcessRenderer(shadowObj.GetComponent<Renderer>(), DepthLookupManager.GungeonSortingLayer.BACKGROUND);
                shadowSprite.usesOverrideMaterial = true;
                shadowSprite.renderer.material.shader = Shader.Find("Brave/Internal/SimpleAlphaFadeUnlit");
                shadowSprite.renderer.material.SetFloat("_Fade", 0.66f);
            }


            public Vector3 Offset;
            public GameObject shadowObject;
        }

        public static Dictionary<string, GameObject> registeredShrines = new Dictionary<string, GameObject>();
        public GameObject BuildWithoutBaseGameInterference()
        {
            GameObject result;
            try
            {


                Texture2D textureFromResource = ResourceExtractor.GetTextureFromResource(this.spritePath);
                GameObject gameObject = SpriteBuilder.SpriteFromResource(this.spritePath, null);
                string text = (this.modID + ":" + this.name).ToLower().Replace(" ", "_");
                gameObject.name = text;
                tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
                component.IsPerpendicular = true;
                component.PlaceAtPositionByAnchor(this.offset, tk2dBaseSprite.Anchor.LowerCenter);
                Transform transform = new GameObject("talkpoint").transform;
                transform.position = gameObject.transform.position + this.talkPointOffset;
                transform.SetParent(gameObject.transform);
                bool flag = !this.usesCustomColliderOffsetAndSize;
                bool flag2 = flag;
                bool flag3 = flag2;
                if (flag3)
                {
                    IntVector2 intVector = new IntVector2(textureFromResource.width, textureFromResource.height);
                    this.colliderOffset = colliderOffset != null ? colliderOffset : new IntVector2(0, 0);
                    this.colliderSize = colliderSize != null ? colliderSize : new IntVector2(intVector.x, intVector.y / 2);
                }
                SpeculativeRigidbody speculativeRigidbody = component.SetUpSpeculativeRigidbody(this.colliderOffset, this.colliderSize);
                ShrineFactory.CustomShrineController customShrineController = gameObject.AddComponent<ShrineFactory.CustomShrineController>();
                customShrineController.ID = text;
                customShrineController.roomStyles = this.roomStyles;
                customShrineController.isBreachShrine = true;
                customShrineController.offset = this.offset;
                customShrineController.pixelColliders = speculativeRigidbody.specRigidbody.PixelColliders;
                customShrineController.factory = this;
                customShrineController.OnAccept = this.OnAccept;
                customShrineController.OnDecline = this.OnDecline;
                customShrineController.CanUse = this.CanUse;
                customShrineController.text = this.text;
                customShrineController.acceptText = this.acceptText;
                customShrineController.declineText = this.declineText;
                customShrineController.HasRoomIcon = HasRoomIcon;
                if (!string.IsNullOrEmpty(RoomIconSpritePath))
                {
                    GameObject OptionalMinimapIcon = SpriteBuilder.SpriteFromResource(RoomIconSpritePath);
                    UnityEngine.Object.DontDestroyOnLoad(OptionalMinimapIcon);
                    FakePrefab.MarkAsFakePrefab(OptionalMinimapIcon);
                    OptionalMinimapIcon.SetActive(false);
                    customShrineController.roomIcon = OptionalMinimapIcon;
                }
                else
                {
                    customShrineController.roomIcon = (GameObject)BraveResources.Load("Global Prefabs/Minimap_Shrine_Icon", ".prefab");

                }


                bool flag4 = this.interactableComponent == null;
                bool flag5 = flag4;
                bool flag6 = flag5;
                if (flag6)
                {
                    SimpleShrine simpleShrine = gameObject.AddComponent<SimpleShrine>();
                    simpleShrine.isToggle = this.isToggle;
                    simpleShrine.OnAccept = this.OnAccept;
                    simpleShrine.OnDecline = this.OnDecline;
                    simpleShrine.CanUse = this.CanUse;
                    simpleShrine.text = this.text;
                    simpleShrine.acceptText = this.acceptText;
                    simpleShrine.declineText = this.declineText;
                    simpleShrine.roomIcon = customShrineController.roomIcon;
                    simpleShrine.talkPoint = transform;
                    simpleShrine.HasRoomIcon = HasRoomIcon;
                }
                else
                {
                    gameObject.AddComponent(this.interactableComponent);
                }

                if (AdditionalComponent != null)
                {
                    gameObject.AddComponent(this.AdditionalComponent);
                }

                gameObject.name = text;
                if (!this.isBreachShrine)
                {
                    DungeonPlaceable table = StaticReferences.GetAsset<DungeonPlaceable>("WhichShrineWillItBe");
                    if (table != null)
                    {
                        table.variantTiers.Add(new DungeonPlaceableVariant()
                        {
                            percentChance = ShrinePercentageChance,
                            prerequisites = preRequisites ?? new DungeonPrerequisite[0],
                            nonDatabasePlaceable = gameObject,
                            unitOffset = roomOffset == null ? new Vector2(0, 0) : roomOffset
                        });
                    }
                }
                FakePrefab.MarkAsFakePrefab(gameObject);
                StaticReferences.StoredRoomObjects.Add(this.name, gameObject);
                ShrineFactory.registeredShrines.Add(text, gameObject);

                PrototypeDungeonRoom room = RoomFactory.CreateEmptyRoom();
                RegisterShrineRoom(gameObject, room, this.name, new Vector3(-1, -1, 0), -1f);
                result = gameObject;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e);
                result = null;
            }
            return result;
        }

        public Vector2 roomOffset;
        public float ShrinePercentageChance;
        public DungeonPrerequisite[] preRequisites;

        public static void RegisterShrineRoom(GameObject shrine, PrototypeDungeonRoom protoroom, string ID, Vector2 offset, float roomweight)
        {
            DungeonPrerequisite[] array = new DungeonPrerequisite[0];
            Vector2 vector = new Vector2((float)(protoroom.Width / 2) + offset.x, (float)(protoroom.Height / 2) + offset.y);
            protoroom.placedObjectPositions.Add(vector);

            DungeonPlaceable placeableContents = ScriptableObject.CreateInstance<DungeonPlaceable>();
            placeableContents.width = 2;
            placeableContents.height = 2;
            placeableContents.respectsEncounterableDifferentiator = true;
            placeableContents.variantTiers = new List<DungeonPlaceableVariant>
            {
                new DungeonPlaceableVariant
                {
                    percentChance = 1f,
                    nonDatabasePlaceable = shrine,
                    prerequisites = array,
                    materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                }
            };

            protoroom.placedObjects.Add(new PrototypePlacedObjectData
            {

                contentsBasePosition = vector,
                fieldData = new List<PrototypePlacedObjectFieldData>(),
                instancePrerequisites = array,
                linkedTriggerAreaIDs = new List<int>(),
                placeableContents = placeableContents

            });

            /*protoroom.placedObjects.Add(new PrototypePlacedObjectData
			{
				contentsBasePosition = vector,
				fieldData = new List<PrototypePlacedObjectFieldData>(),
				instancePrerequisites = array,
				linkedTriggerAreaIDs = new List<int>(),
				placeableContents = new DungeonPlaceable
				{
					width = 2,
					height = 2,
					respectsEncounterableDifferentiator = true,
					variantTiers = new List<DungeonPlaceableVariant>
					{
						new DungeonPlaceableVariant
						{
							percentChance = 1f,
							nonDatabasePlaceable = shrine,
							prerequisites = array,
							materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
						}
					}
				}
			});

			*/
            RoomFactory.RoomData roomData = new RoomFactory.RoomData
            {
                room = protoroom,
                category = protoroom.category.ToString(),
                weight = roomweight,
            };
            RoomFactory.rooms.Add(ID, roomData);
            DungeonHandler.RegisterForShrine(roomData);
        }
        public static void RegisterShrineRoomNoObject(PrototypeDungeonRoom protoroom, string ID, float roomweight)
        {

            RoomFactory.RoomData roomData = new RoomFactory.RoomData
            {
                room = protoroom,
                category = protoroom.category.ToString(),
                weight = roomweight,
            };
            RoomFactory.rooms.Add(ID, roomData);
            DungeonHandler.RegisterForShrine(roomData);
        }

        public static void PlaceBreachShrines()
        {
            bool flag = ShrineFactory.m_builtShrines;
            bool flag2 = !flag;
            if (flag2)
            {
                foreach (GameObject gameObject in ShrineFactory.registeredShrines.Values)
                {
                    try
                    {
                        ShrineFactory.CustomShrineController component = gameObject.GetComponent<ShrineFactory.CustomShrineController>();
                        bool flag3 = !component.isBreachShrine;
                        bool flag4 = !flag3;
                        if (flag4)
                        {
                            ShrineFactory.CustomShrineController component2 = UnityEngine.Object.Instantiate<GameObject>(gameObject).GetComponent<ShrineFactory.CustomShrineController>();
                            component2.Copy(component);
                            component2.gameObject.SetActive(true);
                            component2.sprite.PlaceAtPositionByAnchor(component2.offset, tk2dBaseSprite.Anchor.LowerCenter);
                            IPlayerInteractable component3 = component2.GetComponent<IPlayerInteractable>();
                            bool flag5 = component3 is SimpleInteractable;
                            bool flag6 = flag5;
                            if (flag6)
                            {
                                ((SimpleInteractable)component3).OnAccept = component2.OnAccept;
                                ((SimpleInteractable)component3).OnDecline = component2.OnDecline;
                                ((SimpleInteractable)component3).CanUse = component2.CanUse;
                            }
                            bool flag7 = !RoomHandler.unassignedInteractableObjects.Contains(component3);
                            bool flag8 = flag7;
                            if (flag8)
                            {
                                RoomHandler.unassignedInteractableObjects.Add(component3);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ETGModConsole.Log(e);
                    }
                }
                ShrineFactory.m_builtShrines = true;
            }
        }

        public bool HasRoomIcon = true;

        public float RoomWeight;

        public string RoomIconSpritePath;

        public string name;

        public string modID;

        public string spritePath;

        public string roomPath;

        public string text;

        public string acceptText;

        public string declineText;

        public Action<PlayerController, GameObject> OnAccept;

        public Action<PlayerController, GameObject> OnDecline;

        public Func<PlayerController, GameObject, bool> CanUse;

        public Vector3 talkPointOffset;

        public Vector3 offset = new Vector3(43.8f, 42.4f, 42.9f);

        public IntVector2 colliderOffset;

        public IntVector2 colliderSize;

        public bool isToggle;

        public string shadowPath;

        public float ShadowOffsetX;

        public float ShadowOffsetY;

        public bool usesCustomColliderOffsetAndSize;

        public Type interactableComponent = null;

        public Type AdditionalComponent = null;

        public bool isBreachShrine = false;

        public PrototypeDungeonRoom room;

        public GameObject roomIcon;

        public Dictionary<string, int> roomStyles;

        private static bool m_initialized;

        private static bool m_builtShrines;

        public class CustomShrineController : DungeonPlaceableBehaviour
        {
            private void Start()
            {
                string text = base.name.Replace("(Clone)", "");

                if (ShrineFactory.registeredShrines.ContainsKey(text))
                {
                    this.Copy(ShrineFactory.registeredShrines[text].GetComponent<ShrineFactory.CustomShrineController>());
                }
                else
                {
                    ETGModConsole.Log("Was this shrine registered correctly?: " + text);
                }
                SimpleInteractable component = base.GetComponent<SimpleInteractable>();
                if (component != null)
                {
                    component.OnAccept = this.OnAccept;
                    component.OnDecline = this.OnDecline;
                    component.CanUse = this.CanUse;
                    component.text = this.text;
                    component.acceptText = this.acceptText;
                    component.declineText = this.declineText;
                    component.roomIcon = this.roomIcon;
                    component.HasRoomIcon = this.HasRoomIcon;
                }
            }

            public void Copy(ShrineFactory.CustomShrineController other)
            {
                this.ID = other.ID;
                this.roomStyles = other.roomStyles;
                this.isBreachShrine = other.isBreachShrine;
                this.offset = other.offset;
                this.pixelColliders = other.pixelColliders;
                this.factory = other.factory;
                this.OnAccept = other.OnAccept;
                this.OnDecline = other.OnDecline;
                this.CanUse = other.CanUse;
                this.text = other.text;
                this.acceptText = other.acceptText;
                this.declineText = other.declineText;
                this.roomIcon = other.roomIcon;
                this.HasRoomIcon = other.HasRoomIcon;
            }

            public void ConfigureOnPlacement(RoomHandler room)
            {
                this.m_parentRoom = room;
                this.RegisterMinimapIcon();
            }

            public void RegisterMinimapIcon()
            {
                if (HasRoomIcon == true) { this.m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(this.m_parentRoom, this.roomIcon, false); }
            }

            public void GetRidOfMinimapIcon()
            {
                if (this.m_instanceMinimapIcon != null)
                {
                    Minimap.Instance.DeregisterRoomIcon(this.m_parentRoom, this.m_instanceMinimapIcon);
                    this.m_instanceMinimapIcon = null;
                }
            }

            public bool HasRoomIcon;


            public bool RoomIcon;

            public string ID;

            public bool isBreachShrine;

            public Vector3 offset;

            public List<PixelCollider> pixelColliders;

            public Dictionary<string, int> roomStyles;

            public ShrineFactory factory;

            public Action<PlayerController, GameObject> OnAccept;

            public Action<PlayerController, GameObject> OnDecline;

            public Func<PlayerController, GameObject, bool> CanUse;

            private RoomHandler m_parentRoom;

            private GameObject m_instanceMinimapIcon;

            public int numUses = 0;

            public string text;

            public string acceptText;

            public string declineText;

            public GameObject roomIcon;

        }
    }
}
