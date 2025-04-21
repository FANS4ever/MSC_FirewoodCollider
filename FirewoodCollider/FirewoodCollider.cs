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
        public override string Version => "2.1";

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

            // Get wood log prefab used by the FSM using some sorcery
            // This is a prefab so it should affect all locations in game
            var logWall = yard.transform.Find("MachineHall/Logging/Logwall");
            PlayMakerFSM loggingwall = logWall.GetPlayMaker("Use");
            loggingwall.InitializeFSM();
            GameObject logPrefab = loggingwall.GetState("Create log").GetAction<CreateObject>(3).gameObject.Value;

            // Get the prefab log half and the prespawned logs which dont get affected by prefab changes
            var prefabLogHalf = logPrefab.transform.GetChild(0);
            var yardLogHalf = yard.transform.Find("MachineHall/Logging/log(Clone)/log(Clone)");
            var cottageLogHalf = cottage.transform.Find("Logging/log(Clone)/log(Clone)");
            var cabinLogHalf = cabin.transform.Find("LOD/Logging/log(Clone)/log(Clone)");

            // Collider is already active so to avoid wood breaking as soon as the player 
            // presses on the log pile we make the wood unbreakable. The log collider controller
            // will change this value to the default when a new log spawns.
            prefabLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;
            yardLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;
            cottageLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;
            cabinLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;

            // We setup the collider here
            // Do it after setting breakforce otherwise there will be one split log on the floor at each location
            var layerMask = LayerMask.NameToLayer("Collider2");
            yardLoggingCollObj.layer = layerMask;
            cottageLoggingCollObj.layer = layerMask;
            cabinLoggingCollObj.layer = layerMask;

            // Attach collider controller script to wood log half
            var prefabColliderController = prefabLogHalf.gameObject.AddComponent<LogColliderController>();
            var yardColliderController = yardLogHalf.gameObject.AddComponent<LogColliderController>();
            var cottageColliderController = cottageLogHalf.gameObject.AddComponent<LogColliderController>();
            var cabinColliderController = cabinLogHalf.gameObject.AddComponent<LogColliderController>();

            // Set the colliders the script will control
            var pushColliders = new Collider[] {
                yardLoggingCollObj.GetComponent<Collider>(),
                cottageLoggingCollObj.GetComponent<Collider>(),
                cabinLoggingCollObj.GetComponent<Collider>()
            };

            prefabColliderController.PushColliders = pushColliders;
            yardColliderController.PushColliders = pushColliders;
            cottageColliderController.PushColliders = pushColliders;
            cabinColliderController.PushColliders = pushColliders;

            ModConsole.Log("FirewoodCollider: Initialized!");
        }
    }
}
