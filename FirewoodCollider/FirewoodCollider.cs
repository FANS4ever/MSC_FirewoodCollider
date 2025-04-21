using HutongGames.PlayMaker.Actions;
using MSCLoader;
using UnityEngine;

namespace FirewoodCollider
{
    public class FirewoodCollider : Mod
    {
        public override string ID => "FirewoodCollider";
        public override string Name => "FirewoodCollider";
        public override string Author => "epicduck410, FANS4ever";
        public override string Version => "2.0";

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        private void Mod_OnLoad()
        {
            var yard = GameObject.Find("YARD"); // Near players house
            var cottage = GameObject.Find("COTTAGE"); // On island
            var cabin = GameObject.Find("CABIN"); // Ritoranta's house (Ventti guy)

            var yardLoggingCollObj = yard.transform.Find("MachineHall/Logging/Pölkky/PlayerOnlyColl").gameObject;
            var cottageLoggingCollObj = cottage.transform.Find("Logging/Pölkky/PlayerOnlyColl").gameObject;
            var cabinLoggingCollObj = cabin.transform.Find("LOD/Logging/Pölkky/PlayerOnlyColl").gameObject;

            // We setup the collider here
            var layerMask = LayerMask.NameToLayer("Collider2");
            yardLoggingCollObj.layer = layerMask;
            cottageLoggingCollObj.layer = layerMask;
            cabinLoggingCollObj.layer = layerMask;

            // Get wood log prefab using some sorcery
            // This is a prefab so it should affect all locations in game
            var logWall = yard.transform.Find("MachineHall/Logging/Logwall");
            PlayMakerFSM loggingwall = logWall.GetPlayMaker("Use");
            loggingwall.InitializeFSM();
            GameObject logPrefab = loggingwall.GetState("Create log").GetAction<CreateObject>(3).gameObject.Value;

            // Collider is already active so to avoid wood breaking as soon as the player 
            // presses on the log pile we make the wood unbreakable. The log collider controller
            // will change this value to the default when a new log spawns.
            var logHalf = logPrefab.transform.GetChild(0);
            logHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;

            // Attach collider controller script
            var colliderController = logHalf.gameObject.AddComponent<LogColliderController>();
            colliderController.PushColliders = new Collider[] {
                yardLoggingCollObj.GetComponent<Collider>(),
                cottageLoggingCollObj.GetComponent<Collider>(),
                cabinLoggingCollObj.GetComponent<Collider>()
            };

            ModConsole.Log("FirewoodCollider: Initialized!");
        }
    }
}
