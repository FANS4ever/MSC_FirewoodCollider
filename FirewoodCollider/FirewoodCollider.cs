using HutongGames.PlayMaker;
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

        private SettingsSliderInt sliderBreakForce;
        private SettingsCheckBox checkboxFixDefaultLogs;
        private SettingsCheckBox checkboxFixNoLogValue;


        public override void ModSetup()
        {
            SetupFunction(Setup.ModSettings, Mod_Settings);
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        private void Mod_Settings()
        {
            Settings.AddHeader("Dynamic Settings");
            sliderBreakForce = Settings.AddSlider("sliderBreakForce", "Log break force", 10, 20000, 10000);

            Settings.AddHeader("Onload Settings");
            checkboxFixDefaultLogs = Settings.AddCheckBox("checkboxFixDefaultLogs", "Fix prespawned logs breaking on start", true);
            checkboxFixNoLogValue = Settings.AddCheckBox("checkboxFixNoLogValue", "Fix wrong NoLog value on start", true);
        } 

        private void Mod_OnLoad()
        {
            var yard = GameObject.Find("YARD"); // Near players house
            var cottage = GameObject.Find("COTTAGE"); // On island
            var cabin = GameObject.Find("CABIN"); // Ritoranta's house (Ventti guy)

            var yardLoggingCollObj = yard.transform.Find("MachineHall/Logging/Pölkky/PlayerOnlyColl").gameObject;
            var cottageLoggingCollObj = cottage.transform.Find("Logging/Pölkky/PlayerOnlyColl").gameObject;
            var cabinLoggingCollObj = cabin.transform.Find("LOD/Logging/Pölkky/PlayerOnlyColl").gameObject;

            // Colliders that will be controlled by the log collider script
            var pushColliders = new Collider[] {
                yardLoggingCollObj.GetComponent<Collider>(),
                cottageLoggingCollObj.GetComponent<Collider>(),
                cabinLoggingCollObj.GetComponent<Collider>()
            };

            // Get wood log prefab used by the FSM using some sorcery
            // This is a prefab so it should affect all locations in game
            var logWallYard = yard.transform.Find("MachineHall/Logging/Logwall");
            PlayMakerFSM logWallYardFSM = logWallYard.GetPlayMaker("Use");
            logWallYardFSM.InitializeFSM();
            GameObject logPrefab = logWallYardFSM.GetState("Create log").GetAction<CreateObject>(3).gameObject.Value;
            var prefabLogHalf = logPrefab.transform.GetChild(0);

            // Make log unbreakable until collider controller takes over
            prefabLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;
            
            // Attach collider controller script to wood log half
            var prefabColliderController = prefabLogHalf.gameObject.AddComponent<LogColliderController>();

            prefabColliderController.PushColliders = pushColliders;
            prefabColliderController.BreakForceSlider = sliderBreakForce;

            if (checkboxFixDefaultLogs.GetValue()) 
            {
                var yardLogHalf = yard.transform.Find("MachineHall/Logging/log(Clone)/log(Clone)");
                var cottageLogHalf = cottage.transform.Find("Logging/log(Clone)/log(Clone)");
                var cabinLogHalf = cabin.transform.Find("LOD/Logging/log(Clone)/log(Clone)");

                yardLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;
                cottageLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;
                cabinLogHalf.GetComponent<FixedJoint>().breakForce = float.PositiveInfinity;

                var yardColliderController = yardLogHalf.gameObject.AddComponent<LogColliderController>();
                var cottageColliderController = cottageLogHalf.gameObject.AddComponent<LogColliderController>();
                var cabinColliderController = cabinLogHalf.gameObject.AddComponent<LogColliderController>();

                yardColliderController.PushColliders = pushColliders;
                cottageColliderController.PushColliders = pushColliders;
                cabinColliderController.PushColliders = pushColliders;

                yardColliderController.BreakForceSlider = sliderBreakForce;
                cottageColliderController.BreakForceSlider = sliderBreakForce;
                cabinColliderController.BreakForceSlider = sliderBreakForce;
            }

            if (checkboxFixNoLogValue.GetValue())
            {
                // Get other location FSM's
                var logWallCottage = cottage.transform.Find("Logging/Logwall");
                var logWallCabin = cabin.transform.Find("LOD/Logging/Logwall");
                PlayMakerFSM logWallCottageFSM = logWallCottage.GetPlayMaker("Use");
                PlayMakerFSM logWallCabinFSM = logWallCabin.GetPlayMaker("Use");
                logWallCottageFSM.InitializeFSM();
                logWallCabinFSM.InitializeFSM();

                var noLogYard = (FsmBool)logWallYardFSM.FsmVariables.BoolVariables.GetValue(1);
                var noLogCottage = (FsmBool)logWallCottageFSM.FsmVariables.BoolVariables.GetValue(1);
                var noLogCabin = (FsmBool)logWallCabinFSM.FsmVariables.BoolVariables.GetValue(1);

                /* ModConsole.Log("Yard");
                foreach (var item in logWallYardFSM.FsmVariables.BoolVariables)
                {
                    ModConsole.Log(item.Name + "(" + item.GetType() + ")" + ": " + item.Value);
                }
                ModConsole.Log("Cottage");
                foreach (var item in logWallCottageFSM.FsmVariables.BoolVariables)
                {
                    ModConsole.Log(item.Name + "(" + item.GetType() + ")" + ": " + item.Value);
                }
                ModConsole.Log("Cabin");
                foreach (var item in logWallCabinFSM.FsmVariables.BoolVariables)
                {
                    ModConsole.Log(item.Name + "(" + item.GetType() + ")" + ": " + item.Value);
                } */

                // This if statement is only for logging purposes.
                if (noLogYard.Value || noLogCottage.Value || noLogCabin.Value)
                {
                    ModConsole.Log(Name + ": Detected wrong NoLog value. Fixing all values.");
                    noLogYard.Value = false;
                    noLogCottage.Value = false;
                    noLogCabin.Value = false;
                }
            }

            // Activate the collider
            var layerMask = LayerMask.NameToLayer("Collider2");
            yardLoggingCollObj.layer = layerMask;
            cottageLoggingCollObj.layer = layerMask;
            cabinLoggingCollObj.layer = layerMask;

            ModConsole.Log(Name + ": Initialized!");
        }
    }
}
