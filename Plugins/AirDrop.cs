using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Oxide.Plugins
{
    [Info("AirDrop", "Tiago Dias", "1.0")]
    class AirDrop : RustPlugin
    {
        private Configuration _configuration;

        #region Methods
        private void SpawnCargoPlane(Vector3? dropPosition)
        {
            var cargoPlaneEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
            var cargoPlane = cargoPlaneEntity.GetComponent<CargoPlane>();

            if (!dropPosition.HasValue)
            {
                cargoPlane.RandomDropPosition();
            }
            else
            {
                cargoPlane.InitDropPosition(dropPosition.Value);
            }
            cargoPlane.Spawn();
        }
        #endregion

        #region Commands
        [ChatCommand("AirDrop")]
        private void DropMe(BasePlayer player, string airDrop, string[] args)
        {
            if (!permission.UserHasGroup(player.UserIDString, "AirDrop.use")) return;

            if (args == null)
            {
                SpawnCargoPlane(null);
            }
            else if (args[0] == "me")
            {
                SpawnCargoPlane(Player.Position(player));
            }
            else
            {
                SpawnCargoPlane(Player.Position(BasePlayer.Find(args[0])));
            }
        }
        #endregion

        #region Hooks
        private void Loaded()
        {
            if (_configuration.AirDropScheduler.IsActive)
            {
                timer.Every(_configuration.AirDropScheduler.MinutesBetweenDrops * 60, () =>
                {
                    SpawnCargoPlane(null);
                });
            }
        }

        private void OnEntitySpawn(BaseEntity entity)
        {
            if (entity is SupplyDrop)
            {
                var supplyDrop = entity as SupplyDrop;

                supplyDrop.GetComponent<Rigidbody>().drag = (float)_configuration.SupplyDropDragPhysics.Value;
            }
        }
        #endregion

        #region Configuration
        private class Configuration
        {
            public AirDropScheduler AirDropScheduler { get; set; }
            public SupplyDropDragPhysics SupplyDropDragPhysics { get; set; }
        }

        private class AirDropScheduler
        {
            public bool IsActive { get; set; } = true;
            public int MinutesBetweenDrops { get; set; } = 60;
        }

        private class SupplyDropDragPhysics
        {
            public bool IsActive { get; set; }
            public double Value { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            try
            {
                _configuration = Config.ReadObject<Configuration>();
                if (_configuration == null) throw new Exception();
            }
            catch
            {
                PrintWarning("No configuration found, loading default configuration...");
                LoadDefaultConfig();
            }
            SaveConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_configuration);
        }

        protected override void LoadDefaultConfig()
        {
            _configuration = new Configuration
            {
                AirDropScheduler = new AirDropScheduler
                {
                    IsActive = true,
                    MinutesBetweenDrops = 60
                },
                SupplyDropDragPhysics = new SupplyDropDragPhysics
                {
                    IsActive = true,
                    Value = 2.0
                }
            };
        }
        #endregion
    }
}